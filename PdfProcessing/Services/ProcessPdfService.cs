using IronOcr;
using IronPdf;
using MassTransit;
using Microsoft.Extensions.Options;
using PdfProcessing.Application.Entities;
using PdfProcessing.Entities;
using PdfProcessing.Entities.Document;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PdfProcessing.Services
{
    public class ProcessPdfService : IProcessPdfService, IConsumer<CustomFileInfo>
    {
        public BlockingCollection<Tuple<MemoryStream, int>> inputQueue;
        static int order = 0;

        private AppSetting _appSetting;
        private readonly EndpointUrls _endpointUrls;
        private IHttpClientFactory _httpClientFactory;
        private IronTesseract _ironOcr;

        public ProcessPdfService(IOptionsSnapshot<AppSetting> appSetting, IOptionsSnapshot<EndpointUrls> endpointUrls, IHttpClientFactory httpClientFactory)
        {
            _appSetting = appSetting.Value;
            _endpointUrls = endpointUrls.Value;
            _httpClientFactory = httpClientFactory;
        }

        public Task Consume(ConsumeContext<ServiceMessageIn> context)
        {
            var message = context.Message;
            var fileInfos = ProcessFile(message);
            var statusList = fileInfos.Select(t =>
            {
                var status = t != null ? Status.Success : Status.Error;
                return Tuple.Create(t, new StatusDetail { Status = status });
            }).ToList();

            _documentStatusService.SendDocumentStatus(statusList);

            SendDataToImproveApi(fileInfos);

            return Task.CompletedTask;
        }

        public List<ServiceMessageOut> ProcessFile(ServiceMessageIn message)
        {
            var serviceMessageOuts = new List<ServiceMessageOut>();
            var documents = GetDocumentsResultV1(message);

            if (documents.Count == 0)
            {
                return serviceMessageOuts;
            }

            bool isMultipleFile = documents.Count > 1;

            for (int i = 0; i < documents.Count; i++)
            {
                var documentId = isMultipleFile ? Guid.NewGuid() : documents[i].DocumentId;
                var ocrDataFile = Path.Combine(_config.WorkingDirectory, AppDefiner.PDFPROC_ROOTNAME, $"OCR_{documentId}_{documents[i].IndexDoc}{AppDefiner.JSON_EXTENSION}");
                File.WriteAllText(ocrDataFile, Util.SerializeObject(documents[i], JsonFormatting.Indented));

                var documentInfo = new ServiceMessageExt();
                documentInfo.InputFileFullPath = ocrDataFile;
                documentInfo.DocumentName = isMultipleFile ? $"{inputFile.Name.Replace(inputFile.Extension, AppDefiner.APP_CHARACTER_EMPTY).ToValidFileName()}_{documents[i].IndexDoc}{inputFile.Extension}" : inputFile.Name;
                documentInfo.DocumentId = documentId;
                documentInfo.ParentDocumentId = isMultipleFile ? documents[i].DocumentId : Guid.Empty;
                documentInfo.Size = docSize;

                serviceMessageOuts.Add(documentInfo);
            }

            return rs;
        }

        public List<Document> GetDocumentsResultV1(ServiceMessageIn serviceMessageIn)
        {
            var inputFile = new FileInfo(serviceMessageIn.InputFileAbsolutePath);
            var pdf = new PdfDocument(inputFile.FullName);
            var queueOcrResults = GetOcrResultV1(inputFile.FullName);

            var listOcrResult = queueOcrResults.ToList().OrderBy(t => t.PageIndex).ToList();
            var lstGroups = _processDocument.GroupDocuments(listOcrResult);
            var documents = new List<Document>();
            var fileName = val.FileName;

            Parallel.ForEach(lstGroups, g =>
            {
                var doc = _processDocument.ConvertListPageInfoToDocument(g.Item1);
                doc.DocumentId = val.DocumentId;
                doc.IndexDoc = g.Item2;
                doc.DocumentName = fileName;
                doc.FileFullPath = val.FileFullPath;

                var pages = doc.Pages.Where(t => t.Words.Count > 0);

                foreach (var item in pages)
                {
                    if (!string.IsNullOrEmpty(item.Text))
                    {
                        item.LanguageCode = _processDocument.IdentifyLanguageFromText(item.Text);
                    }
                }
                doc.LanguageCode = "eng";
                var dicDoc = pages.Where(t => t.LanguageCode != null).GroupBy(t => t.LanguageCode).ToDictionary(t => t.Key, t => t.Count());

                if (dicDoc.Count() > 0)
                {
                    doc.LanguageCode = dicDoc.First().Key;
                }

                documents.Add(doc);
            });

            return documents;
        }

        public async Task<OcrResult> GetOcrResultV1(string fileName)
        {
            Log.Information("Get OCR result from file {fileName}", fileName);
            var ocr = new IronTesseract();
            ocr.Language = OcrLanguage.English;
            ocr.AddSecondaryLanguage(OcrLanguage.German);
            ocr.AddSecondaryLanguage(OcrLanguage.French);
            ocr.AddSecondaryLanguage(OcrLanguage.Italian);
            ocr.Configuration.ReadBarCodes = true;
            ocr.MultiThreaded = true;

            var watch0 = new Stopwatch();
            watch0.Start();
            OcrResult ocrResult = null;

            try
            {
                using (var input = new OcrInput(fileName))
                {
                    ocrResult = await ocr.ReadAsync(input);
                    ocrResult.SaveAsSearchablePdf(Path.GetFileNameWithoutExtension(fileName) + Guid.NewGuid() + ".pdf");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, Resources.Resources.READING_PDF);
            }
            watch0.Stop();
            Log.Information(string.Format(Resources.Resources.END_READING_IN, watch0.ElapsedMilliseconds));
            return ocrResult;
        }
    }
}
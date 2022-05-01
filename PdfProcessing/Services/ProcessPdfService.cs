using IronOcr;
using IronPdf;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NTextCat;
using PdfProcessing.Entities;
using PdfProcessing.Entities.Document;
using PdfProcessing.Enums;
using PdfProcessing.Sercurity;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PdfProcessing.Services
{
    public class ProcessPdfService : IProcessPdfService, IConsumer<CustomFileInfo>
    {
        public BlockingCollection<Tuple<MemoryStream, int>> inputQueue;
        static PriorityLock priorityLock = new PriorityLock();
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

        public Task Consume(ConsumeContext<CustomFileInfo> context)
        {
            var c = context.Message;
            c.PriorityOrder = order--;
            priorityLock.Lock(c.PriorityOrder, c.IsHighPriority);
            ProcessFile(c);
            priorityLock.Unlock();
            return null;
        }

        public void ProcessFile(CustomFileInfo c)
        {
            var fileToSend = new FileProtocol();
            fileToSend.Content = File.ReadAllBytes(c.InitialPath);
            fileToSend.ContentType = c.DocumentType;
            var urlSendFile = $"{_endpointUrls.SendFileUrl}/{_appSetting.TenantName}";
            
            var inputFile = new FileInfo(c.FileFullPath);
            string processDetail = string.Empty;
            bool isCreateEditor =
                    !string.IsNullOrEmpty(_appSetting.MonitorEditorDir) && inputFile.FullName.ToLower().Contains(_appSetting.MonitorEditorDir.ToLower())
                    ? true : false;
            bool isAddTextLayer = GetConfigAddTextLayer() == Constants.PROCESSING_ADD_TEXT_LAYER ? true : false;
            bool errorWhileProcessing;
            string attachmentFilePath = string.Empty;
            var fileName = Path.GetFileName(inputFile.FullName);
            fileToSend.FileName = fileName;

            try
            {
                var pdf = new PdfDocument(inputFile.FullName);
                processDetail = Resources.Resources.FILE_NAME + fileName + Environment.NewLine;

                DocumentStatus dataStatus;
                NewDocumentStatus newDataStatus;
                CustomFileInfo documentCustomInfo = c;
                documentCustomInfo.DisplayFileName = inputFile.FullName;

                FileInfo documentInfo = new FileInfo(documentCustomInfo.FileFullPath);
                string size = Helper.MeasureSizeOfFile(documentInfo.Length);
                string name = inputFile.Name;
                string parrentScanDocumentId = Guid.NewGuid().ToString();
                Guid parrentId = Guid.Empty;
                Guid tenantId = new Guid(_appSetting.TenantID);
                if (!isCreateEditor)
                {
                    newDataStatus = new NewDocumentStatus
                    {
                        Id = documentCustomInfo.NewDocStatusId,
                        TenantId = tenantId,
                        Name = name,
                        Size = size,
                        PathFilePriorityDocument = c.FileFullPath,
                        InitialPath = c.InitialPath,
                        ScanDocumentID = parrentScanDocumentId,
                        IsAddTextLayer = isAddTextLayer,
                    };

                    parrentId = NewInsertStatus(newDataStatus);
                }

                var documents = GetDocumentsResult(new GetDocumentsResultParams
                {
                    FileName = inputFile.Name,
                    PdfData = pdf,
                    IsCreateEditor = isCreateEditor,
                    IsAddTextLayer = isAddTextLayer
                }, c);

                if (!isCreateEditor)
                {
                    var command = (CommandType)c.CommandType;
                    if (command == CommandType.DocStatusInsert)
                    {
                        bool multipleFile = documents.Count > 1;
                        Guid documentID = c.DocumentId;

                        if (multipleFile)
                        {
                            statisticData.ProcessingTo = DateTime.Now;
                            statisticData.ProcessingStatus = AppDefiner.PROCESS_STATUS_SUCCESS;
                            SendDataToStatisticalApi(statisticData);
                            CustomLog.LogInfor(fileName, Helper.GetSuccessFormat(Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.SENT_DATA_TO_STATISTICAL)));
                        }

                        for (int i = 0; i < documents.Count; i++)
                        {
                            errorWhileProcessing = false;
                            var doc = documents[i];
                            attachmentFilePath = doc.FileFullPath;


                            documentCustomInfo = Helper.DeepCopyAnyObject(c);
                            documentCustomInfo.FileFullPath = doc.FileFullPath;
                            documentInfo = new FileInfo(doc.FileFullPath);

                            if (multipleFile)
                            {
                                documentCustomInfo.DisplayFileName = $"{inputFile.FullName.Replace(inputFile.Extension, AppDefiner.APP_CHARACTER_EMPTY).ToValidFileName() }_{doc.IndexDoc}{inputFile.Extension}";
                                name = $"{inputFile.Name.Replace(inputFile.Extension, AppDefiner.APP_CHARACTER_EMPTY).ToValidFileName() }_{doc.IndexDoc}{inputFile.Extension}";
                                size = Helper.MeasureSizeOfFile(documentInfo.Length);
                                fileToSend.FileName = name;
                                fileToSend.Content = File.ReadAllBytes(doc.FileFullPath);
                                documentID = Guid.NewGuid();
                                statisticData.DocumentName = name;
                                statisticData.DocumentId = documentID;
                            }

                            int totalBarCodeCount = 0;
                            doc.Pages.ForEach(y => totalBarCodeCount += y.Barcodes.Count);

                            processDetail = Resources.Resources.FILE_NAME + name + Environment.NewLine;
                            processDetail += Resources.Resources.NUMBER_PAGE + doc.Pages.Count + Environment.NewLine;
                            processDetail += Resources.Resources.NUMBER_QR + totalBarCodeCount + Environment.NewLine;

                            dataStatus = new DocumentStatus
                            {
                                Id = documentCustomInfo.DocStatusId,
                                Name = name,
                                Size = size,
                                IProcess = new DocumentStatusDetail { Value = (int)DocumentStatusValue.NotSet, DataState = Helper.SerializeObject(documentCustomInfo), IsOnlyImage = doc.IsOnlyImage },
                                PathFilePriorityDocument = c.FileFullPath,
                                InitialPath = c.InitialPath,
                                ScanDocumentID = _config.SendScanDocumentID ? doc.ScanDocumentID : string.Empty,
                                ParentDocumentID = parrentScanDocumentId,
                                IsAddTextLayer = isAddTextLayer
                            };
                            newDataStatus = new NewDocumentStatus
                            {
                                Name = name,
                                Size = size,
                                PathFilePriorityDocument = c.FileFullPath,
                                InitialPath = c.InitialPath,
                                ScanDocumentID = _config.SendScanDocumentID ? doc.ScanDocumentID : string.Empty,
                                ParentDocumentID = parrentScanDocumentId,
                                IsAddTextLayer = isAddTextLayer,
                                Id = documentCustomInfo.NewDocStatusId,
                                TenantId = tenantId,
                                IsOnlyImage = doc.IsOnlyImage
                            };

                            try
                            {
                                newDataStatus.Id = multipleFile ? NewInsertStatus(newDataStatus) : parrentId;
                                documentCustomInfo.NewDocStatusId = newDataStatus.Id;
                                dataStatus.IProcess.Value = (int)DocumentStatusValue.Success;
                                AddStatusdetail(dataStatus.IProcess, documentCustomInfo.NewDocStatusId);
                                var ocrDir = Helper.AppGetFolderPath(_config.PdfProcessingOcrDir, true);
                                var ocrDataFile = Helper.AppGetFilePath($"{ocrDir}\\{inputFile.Name.Replace(inputFile.Extension, AppDefiner.APP_CHARACTER_EMPTY).ToValidFileName()}_{doc.IndexDoc}{AppDefiner.JSON_EXTENSION}", true);
                                File.WriteAllText(ocrDataFile, Helper.SerializeObject(doc, JsonFormatting.Indented));

                                documentCustomInfo.FileResultPath = ocrDataFile;
                                documentCustomInfo.DocStatusName = dataStatus.Name;
                                documentCustomInfo.DocumentId = documentID;
                                documentCustomInfo.RequestTimeout = _config.RequestTimeout;
                                if (_config.RequestTimeout <= 0)
                                {
                                    errorWhileProcessing = true;
                                    throw new Exception(Helper.GetErrorFormat(String.Format(Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.SENT_DATA_TO_IMPROVEMENT_RESPONSE_CODE), (int)HttpStatusCode.RequestTimeout)));
                                }

                                var policy = CustomPolicy.GetTimeoutRetryPolicy(_config.RequestTimeout, _config.RetryNumber);

                                var response = policy.Execute(() => SendDataToImproveApi(documentCustomInfo));

                                if (response == HttpStatusCode.RequestTimeout)
                                {
                                    errorWhileProcessing = true;
                                    message.DocumentName = name;
                                    throw new Exception(Helper.GetErrorFormat(String.Format(Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.SENT_DATA_TO_IMPROVEMENT_RESPONSE_CODE), (int)response)));
                                }

                                statisticData.ProcessingTo = DateTime.Now;
                                TimeSpan diffTime = statisticData.ProcessingTo - statisticData.ProcessingFrom;
                                processDetail += Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.PROCESS_TIME) + diffTime.TotalMilliseconds + Environment.NewLine;
                                processDetail += Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.OUTPUT_TIME) + Path.GetFileName(doc.FileFullPath);
                                statisticData.Detail = processDetail;
                                statisticData.ProcessingStatus = AppDefiner.PROCESS_STATUS_SUCCESS;

                                SendDataToStatisticalApi(statisticData);
                                CustomLog.LogInfor(fileName, Helper.GetSuccessFormat(Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.SENT_DATA_TO_STATISTICAL)));
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());                                
                            }
                        }
                    }
                }
                else return;
            }
            catch (Exception ex)
            {
                Log.Error(Resources.Resources.FILE_PATH + inputFile.FullName + ". Exception: " + ex.ToString());
            }
        }

        private HttpStatusCode SendDataToImproveApi(CustomFileInfo c)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var fileName = c.DocStatusName;
            var response = httpClient.CreatePostRequestAsync(_endpointUrls.ImprovementApiUrl, c).Result;
            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
            {
                Log.Information(string.Format(Resources.Resources.SENT_DATA_TO_IMPROVEMENT_RESPONSE_CODE, (int)response.StatusCode));
            }
            return response.StatusCode;
      
        }

        private string GetConfigAddTextLayer()
        {
            var configFile = AppGetFilePath(Constants.PDFPROC_FILE_CONFIGURATION);
            string dataJson = File.ReadAllText(configFile);
            var dataModels = JsonConvert.DeserializeObject<List<BaseModel>>(dataJson);
            var result = dataModels.FirstOrDefault(x => x.Id == 1);
            return result?.Value ?? string.Empty;
        }

        public DocumentForTextLayerModel GetDataDocument(string fileName)
        {
            var ocrFolderPath = AppGetFolderPath(_appSetting.PdfProcessingOcrDir);
            var dataDocumentTextLayer = new DocumentForTextLayerModel();

            var pathFiles = Directory.EnumerateFiles(ocrFolderPath, AppDefiner.RESULT_FILE).ToList();
            foreach (var file in pathFiles)
            {
                string JSONtxt = File.ReadAllText(file);
                var data = Helper.DeserializeObject<DocumentForTextLayerModel>(JSONtxt);
                if (data.BaseFileNamePdf == fileName)
                {
                    dataDocumentTextLayer = data;
                }
            };
            return dataDocumentTextLayer;
        }

        public BlockingCollection<PageInfo> GetOcrResult(string fileName, PdfDocument pdf)
        {
            Log.Information(Resources.Resources.START_READING_FILE);
            var queueOcrResults = new BlockingCollection<PageInfo>();

            int pageCount = pdf.PageCount;
            int pageProcessed = 0;
            int pageSize = _config.ProcessPageSize;
            if (pageSize >= pageCount) pageSize = pageCount;
            string shortFileName = fileName.Length > 100 ? (fileName.Substring(0, 98) + Constants.APP_CHARACTER_TWO_DOT) : fileName;

            var watch0 = new Stopwatch();
            watch0.Start();
            while (true)
            {
                if (pageProcessed >= pageCount) break;
                if (watch0.ElapsedMilliseconds > _config.ProcessTimeout)
                {
                    Log.Information(string.Format(Resources.Resources.TIMEOUT_FILE_TOO_LARGE, pageProcessed));
                    break;
                }
                if (pageSize >= pageCount - pageProcessed) pageSize = pageCount - pageProcessed;
                int nextLoop = pageProcessed + pageSize;

                PdfDocument temp = null;
                for (int i = pageProcessed; i < nextLoop; i++)
                {
                    if (temp == null) temp = pdf.CopyPage(i);
                    else
                    {
                        temp = PdfDocument.Merge(temp, pdf.CopyPage(i));
                    }
                }

                Log.Information(string.Format(Resources.Resources.OCR_FILENAME_PAGES, shortFileName, pageProcessed, pageSize));
                int errors = 0;
                try
                {
                    using (var input = new OcrInput(temp.Stream))
                    {
                        var result = ocr.Read(input);
                        for (int index = 0; index < result.Pages.Count(); index++)
                        {
                            var barcode = result.Pages[index].Barcodes.FirstOrDefault();
                            var qrType = barcode == null ? TypesQRCode.Null
                            : (barcode.Value.Equals(_config.ContentQRKeeping, StringComparison.CurrentCultureIgnoreCase)
                            ? TypesQRCode.Keeping : (barcode.Value.Equals(_config.ContentQRRemoving, StringComparison.CurrentCultureIgnoreCase)
                            ? TypesQRCode.Removing : TypesQRCode.Null));

                            int pageNumber = index + pageProcessed + 1;
                            queueOcrResults.Add(new PageInfo
                            {
                                PageIndex = pageNumber,
                                PageIndexExact = pageNumber,
                                Results = result.Pages[index].ToLocalPage(pageNumber),
                                QRCodeType = qrType
                            });
                        };
                        GC.SuppressFinalize(result);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(Resources.Resources.READING_PDF + " Exception: " + ex.ToString());
                    errors++;
                }
                GC.SuppressFinalize(temp);

                if (errors == 0)
                {
                    pageProcessed += pageSize;
                    Log.Information(string.Format(Resources.Resources.OCR_FILENAME_PAGESOF, shortFileName, pageProcessed, pageCount));
                }
            }
            queueOcrResults.CompleteAdding();
            watch0.Stop();
            Log.Information(string.Format(Resources.Resources.END_READING_IN, watch0.ElapsedMilliseconds));

            return queueOcrResults;
        }

        public List<Document> GetDocumentsResult(GetDocumentsResultParams val, CustomFileInfo customFileInfo)
        {
            var processDir = _appSetting.ProcessDir;

            var queueOcrResults = GetOcrResult(val.FileName, val.PdfData);
            if (queueOcrResults.Count <= 0) return new List<Document>();
            var listOcrResult = queueOcrResults.ToList().OrderBy(t => t.PageIndex).ToList();
            var lstGroups = GroupDocuments(listOcrResult);
            var documents = new List<Document>();
            var fileName = val.FileName;
            lstGroups.ForEach(g =>
            {
                var doc = ConvertListPageInfoToDocument(g.Item1);
                doc.IndexDoc = g.Item2;
                doc.BaseFileNamePdf = fileName;

                var pages = doc.Pages.Where(t => t.Words.Count > 0);
                foreach (var item in pages)
                {
                    if (!string.IsNullOrEmpty(item.Text))
                    {
                        item.LanguageCode = IdentifyLanguageFromText(item.Text);
                    }
                }
                var scanDocumentID = Guid.NewGuid().ToString();
                var dataPath = $"{processDir}\\{scanDocumentID}_{fileName.Split('.')[0]}{Constants.PDF_EXTENSION_LOWERCASE}";
                doc.LanguageCode = "eng";
                doc.ScanDocumentID = scanDocumentID;
                doc.FileFullPath = dataPath;
                var dicDoc = pages.Where(t => t.LanguageCode != null).GroupBy(t => t.LanguageCode).ToDictionary(t => t.Key, t => t.Count());
                if (dicDoc.Count() > 0)
                {
                    doc.LanguageCode = dicDoc.First().Key;
                }
                documents.Add(doc);

                //process: combine pages -> pdf files for delivery
                var dataPages = doc.Pages.Select(t => t.PageNumber - 1).ToList();
                var dataPdf = val.PdfData.CopyPages(dataPages);
                if (dataPdf.TrySaveAs(dataPath))
                {
                    doc.FilePDFToDelivery = CryptoProvider.Encrypt(dataPath);
                    doc.IsOnlyImage = CheckFilePdfIsOnlyImage(dataPdf);
                }
            });

            //Process save result
            if (val.IsCreateEditor)
            {
                try
                {
                    var docData = processDir.LoadModelsFromFolderX<DocumentProcessedFile>(AppDefiner.JSON_EXTENSION);
                    var docOcrData = processDir.LoadModelsFromFolderX<DocumentProcessedFileData>(".result");
                    int count = 1;
                    bool isMultipleDoc = documents.Count > 1;
                    documents.ForEach(t =>
                    {
                        var itemName = Guid.NewGuid().ToString();
                        var docRs = t.Pages.Select(x =>
                        {
                            var imagePath = $"{processDir}\\{itemName}_{x.PageNumber}.png";
                            var bmp = val.PdfData.PageToBitmap(x.PageNumber, 105);
                            var processedBmp = ResizeImage(bmp, bmp.Width, bmp.Height);
                            processedBmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
                            return new DocumentEdit()
                            {
                                FileName = fileName,
                                PageIndex = x.PageNumber,
                                DocumentWidth = x.Width,
                                DocumentHeight = x.Height,
                                Result = x.Words,
                                DocumentImage = $"{itemName}_{x.PageNumber}.png"
                            };
                        }).ToList();

                        //int newId = docData.GetNewIntIdX();

                        Guid _newId = Guid.NewGuid();

                        var docPath = $"{processDir}\\{itemName}_{fileName.Split('.')[0]}{AppDefiner.JSON_EXTENSION}";

                        var doc = new DocumentProcessedFile()
                        {
                            Id = _newId,
                            //Id = newId,
                            FileName = isMultipleDoc ? $"{fileName.Split('.')[0]}({count})" : fileName,
                            Status = (int)DocumentStatusCode.Processed,
                            PhysicalName = $"{itemName}_{fileName.Split('.')[0]}{AppDefiner.JSON_EXTENSION}",
                            DocumentPhysicalName = fileName,
                            DocumentResults = docRs
                        };
                        File.WriteAllText(docPath, Helper.SerializeObject(doc, JsonFormatting.Indented));
                        docData.Add(doc);

                        var resultPath = $"{processDir}\\{itemName}_{fileName.Split('.')[0]}.result";
                        int newOcrId = docOcrData.GetNewIntIdX();
                        var docOcr = new DocumentProcessedFileData()
                        {
                            Id = newOcrId,
                            ParentId = _newId,
                            //ParentId = Guid.NewGuid().ToString(),
                            OcrDoc = t
                        };
                        File.WriteAllText(resultPath, Helper.SerializeObject(docOcr, JsonFormatting.Indented));
                        docOcrData.Add(docOcr);

                        count++;
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(Resources.Resources.SAVE_OCR_RESULT + ". Exception: " + ex.ToString());
                }
            }
            
            return documents;
        }

        public string IdentifyLanguageFromText(string textPage)
        {
            var rankedLanguageIdentifierFactory = new RankedLanguageIdentifierFactory();
            var profilePath = AppGetFilePath(Constants.PDFPROC_FILE_IDENLANGUAGE);
            var _identifier = rankedLanguageIdentifierFactory.Load(profilePath);
            var languages = _identifier.Identify(textPage);
            var mostCertainLanguage = languages.FirstOrDefault()?.Item1.Iso639_3;
            return mostCertainLanguage;
        }

        public string AppGetFilePath(string subFilePath)
        {
            if (string.IsNullOrEmpty(subFilePath))
                throw new ArgumentNullException(nameof(subFilePath));

            //remove invalid charaters from subFilePath
            subFilePath = subFilePath.Trim(Path.GetInvalidPathChars()).Trim(Path.GetInvalidFileNameChars());

            if (File.Exists(subFilePath))
                return subFilePath;

            string filePath = Path.Combine(_appSetting.BaseDirectory, subFilePath);

            return filePath;
        }

        public static Document ConvertListPageInfoToDocument(List<PageInfo> lstPages)
        {
            var doc = new Document();
            var sb = new StringBuilder();
            var lines = new List<LineOfText>();
            string textLine;
            lstPages.ForEach(p =>
            {
                var page = p.Results;
                var rs = new List<Word>();
                if (page.Barcodes.Count == 0)
                {
                    lines = page.Lines.OrderBy(t => t.Location.Y).ToList();
                }
                else
                {
                    foreach (var item in page.Barcodes)
                    {
                        var x1BarCode = item.X;
                        var y1BarCode = item.Y;
                        var x2BarCode = item.X + item.Width;
                        var y2BarCode = item.Y + item.Height;
                        var lineContainsBarCode = page.Lines.FirstOrDefault(r => r.Location.Y == y1BarCode);
                        if (lineContainsBarCode != null)
                        {
                            lineContainsBarCode.Words.Add(new Word
                            {
                                Width = item.Width,
                                Height = item.Height,
                                X = item.X,
                                Y = item.Y,
                                Text = item.Value,
                                Location = item.Location,
                                WordNumber = -1
                            });
                        }
                        else
                        {
                            page.Lines.Add(new LineOfText
                            {
                                Location = item.Location,
                                Text = item.Value,
                                Words = new List<Word> {
                                    new Word
                                    {
                                        X = item.X,
                                        Y = item.Y,
                                        Width = item.Width,
                                        Height = item.Height,
                                        Text = item.Value,
                                        Location = item.Location,
                                        WordNumber = -1
                                    }
                                },
                                LineNumber = -1,
                                PageNumber = page.PageNumber,
                                WordCount = 1
                            });
                        }
                        page.Lines = page.Lines.FindAll(l => l.X < x1BarCode || l.X > x2BarCode || l.Y < y1BarCode || l.Y > y2BarCode);
                    }
                    lines = page.Lines.OrderBy(t => t.Location.Y).ToList();
                }

                lines.ForEach(line =>
                {
                    List<Word> overlap = new List<Word>();
                    var words = line.Words.FindAll(r => r.WordNumber != -1);
                    Barcode barcode;
                    Word word;
                    for (var i = 0; i < page.Barcodes.Count; i++)
                    {
                        barcode = page.Barcodes[i];
                        for (var j = 0; j < words.Count; j++)
                        {
                            word = words[j];
                            if (barcode.X < (word.X + word.Width) && (barcode.X + barcode.Width) > word.X &&
                                barcode.Y < (word.Y + word.Height) && (barcode.Y + barcode.Height) > word.Y)
                            {
                                overlap.Add(word);
                            }
                        }
                    }
                    line.Words = line.Words.FindAll(w => !overlap.Contains(w));
                    line.WordCount = line.Words.Count;
                    var wordsOcr = line.Words.OrderBy(t => t.Location.X).ToList();
                    var wordsClone = wordsOcr.Select(t => t).ToList();
                    rs.AddRange(wordsClone);
                    textLine = "";
                    line.Words.ForEach(element =>
                    {
                        if (element.WordNumber != -1) textLine += textLine.Length > 0 ? " " + element.Text : element.Text;
                    });
                    if (textLine.Length > 0) sb.AppendLine(textLine);
                });
                lines = lines.FindAll(l => l.LineNumber != -1 && l.Words.Count > 0);
                Dictionary<int, List<Word>> mapLines = lines.ToDictionary(t => t.LineNumber, t => t.Words);
                List<Line> lineToRemove = new List<Line>();
                page.Paragraphs.ForEach(paragraph =>
                {
                    paragraph.Lines.ForEach(line =>
                    {
                        if (mapLines.ContainsKey(line.LineNumber))
                        {
                            line.Words = mapLines[line.LineNumber];
                        }
                        else
                        {
                            lineToRemove.Add(line);
                        }
                    });
                    paragraph.Lines.RemoveAll(l => lineToRemove.Contains(l));
                });
                page.Words = rs;
                page.Text = sb.ToString();
                sb.Clear();
                page.Lines = lines;
                doc.Pages.Add(page);
            });
            return doc;
        }

        public static List<Tuple<List<PageInfo>, int>> GroupDocuments(List<PageInfo> lstPage)
        {
            var groups = new List<Tuple<List<PageInfo>, int>>();
            // Default the first page contain QR1 if none QR Code is foundon this page
            var firstPage = lstPage.First();
            if (!firstPage.QRCodeType.HasValue || firstPage.QRCodeType == TypesQRCode.Null)
            {
                firstPage.QRCodeType = TypesQRCode.Keeping;
            }
            // Count number of QR Code on all pages(1 page only count 1 QR Code)
            int numberGroups = 0;
            var pages = new List<PageInfo>();
            lstPage.ForEach(p =>
            {
                if (p.QRCodeType.HasValue && p.QRCodeType != TypesQRCode.Null)
                {
                    if (numberGroups >= 1)
                    {
                        if (pages.Count == 0)
                        {
                            numberGroups--;
                        }
                        else
                        {
                            groups.Add(Tuple.Create(pages, numberGroups));
                        }
                    }
                    pages = new List<PageInfo>();
                    numberGroups++;
                }
                if (!p.QRCodeType.Equals(TypesQRCode.Removing))
                {
                    pages.Add(p);
                }
                if (p.PageIndex == lstPage.Count)
                {
                    if (pages.Count == 0)
                    {
                        numberGroups--;
                    }
                    else
                    {
                        groups.Add(Tuple.Create(pages, numberGroups));
                    }
                }
            });
            return groups;
        }

        public static Image ResizeImage(Image img, int w, int h)
        {
            var bmp = new Bitmap(img, w, h);
            var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            return bmp;
        }        

        public bool CheckFilePdfIsOnlyImage(PdfDocument pdf)
        {
            return string.IsNullOrWhiteSpace(pdf.ExtractAllText());            
        }
    }
}
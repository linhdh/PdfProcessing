using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfProcessing.Entities;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace PdfProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        IBus _bus;
        //IProcessPdfService _processPdfService;
        AppSetting _config;

        public PdfController(IBus bus, /*IProcessPdfService processPdfService,*/ AppSetting config)
        {
            _bus = bus;
            //_processPdfService = processPdfService;
            _config = config;
        }

        [HttpPost]
        [Route("queuefile")]
        public HttpResponseMessage QueueFile([FromBody] CustomFileInfo c)
        {
            /*HttpResponseMessage badRequestResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var modelNotNull = Guard.NotNull(c, nameof(c));
            if (!modelNotNull) return badRequestResponse;

            var fileFullPathNotNull = Guard.StringNotNullOrEmpty(c.FileFullPath, nameof(c.FileFullPath));
            if (!fileFullPathNotNull) return badRequestResponse;

            var fileFullPathExist = Guard.Exist(c.FileFullPath, nameof(c.FileFullPath));
            if (!fileFullPathExist) return badRequestResponse;*/

            return ProcessFile(c);
        }

        private HttpResponseMessage ProcessFile(CustomFileInfo c)
        {
            var inputFile = new FileInfo(c.FileFullPath);
            var fileName = Path.GetFileName(inputFile.FullName);
            
            Uri uri;
            ISendEndpoint endpoint;
            HttpStatusCode httpStatus = HttpStatusCode.Accepted;

            if (string.IsNullOrEmpty(c.FileFullPath))
            {
                //return new HttpResponseMessage(
                //         httpStatus =   HttpStatusCode.BadRequest, Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.FILE_PATH_NULL_EMPTY));

                httpStatus = HttpStatusCode.BadRequest;
                return new HttpResponseMessage(httpStatus);
            }

            if (!System.IO.File.Exists(c.FileFullPath))
            {
                //throw new HttpResponseException(Request.CreateErrorResponse
                //            (HttpStatusCode.NotFound, Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.FILE_NOT_EXISTS)));

                httpStatus = HttpStatusCode.NotFound;
                return new HttpResponseMessage(httpStatus);
            }

            if (c.IsHighPriority)
            {
                uri = new Uri("queue:kloon_pdf_processing_high_queue");
            }
            else
            {
                uri = new Uri("queue:kloon_pdf_processing_low_queue");
            }

            endpoint = _bus.GetSendEndpoint(uri).Result;
            endpoint.Send<CustomFileInfo>(new { c.FileFullPath, c.IsHighPriority, CommandType = (int)CommandType.DocStatusInsert, c.InitialPath, c.DocumentId });

            return new HttpResponseMessage(httpStatus);
        }

        //[HttpPost]
        //[Route("rerun")]
        //public int ReRunProcess([FromBody] DocumentStatus sender)
        //{
        //    string fileName = String.Empty;
        //    try
        //    {
        //        if (sender.Id <= 0) throw new Exception(Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.INVALID_DS_ID));

        //        var c = Helper.DeserializeObject<CustomFileInfo>(sender.IProcess.DataState);
        //        if (c == null) c = new CustomFileInfo();
        //        // reassign the FileFullPath equal PathFilePriorityDocument (is original path file) to rerun origin file
        //        c.FileFullPath = sender.PathFilePriorityDocument;
        //        fileName = c.DisplayFileName;
        //        if (!System.IO.File.Exists(c.FileFullPath)) return (int)FileRerunMessage.FileNotExist;
        //        c.InitialPath = sender.InitialPath;
        //        c.CommandType = (int)CommandType.DocStatusUpdate;
        //        c.DocumentId = Guid.NewGuid();
        //        var response = ProcessFile(c);
        //        return (int)FileRerunMessage.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        CustomLog.LogErrorException(fileName, ex, Helper.GetErrorFormat(Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.RE_RUN_PROCESS)));
        //        return (int)FileRerunMessage.UndefinedError;
        //    }
        //}

        //[HttpGet]
        //[Route("data-ocr")]
        //public DocumentForTextLayerModel GetDataDocument(string fileName)
        //{
        //    var fileNameNotNull = Guard.StringNotNullOrEmpty(fileName, nameof(fileName));
        //    if (!fileNameNotNull)
        //    {
        //        return null;
        //    }
        //    var result = _processPdfService.GetDataDocument(fileName);
        //    return result;
        //}

        //[HttpGet]
        //[Route("image-pdf")]
        //public byte[] GetImage(string fileName)
        //{
        //    var fileNameNotNull = Guard.StringNotNullOrEmpty(fileName, nameof(fileName));
        //    if (!fileNameNotNull) return null;

        //    var imagepath = Helper.AppGetFilePath($"{_config.PdfProcessingOcrDir}\\{fileName}");

        //    var imagePathNotNull = Guard.StringNotNullOrEmpty(imagepath, nameof(imagepath));
        //    if (!imagePathNotNull) return null;

        //    var imagePathExist = Guard.Exist(imagepath, nameof(imagepath));
        //    if (!imagePathExist) return null;

        //    var imgData = System.IO.File.ReadAllBytes(imagepath);
        //    return imgData;
        //}

        //[HttpPost]
        //[Route("pdf-text-layer/download")]
        //public FileProtocol DownloadPdfTextLayer([FromBody] PdfTextLayerModel model)
        //{
        //    var modelNotNull = Guard.NotNull(model, nameof(model));
        //    if (!modelNotNull) return null;

        //    var fileNameNotNull = Guard.StringNotNullOrEmpty(model.FileName, nameof(model.FileName));
        //    if (!fileNameNotNull) return null;

        //    var dFile = Helper.AppGetFilePath($"{AppDefiner.PDFPROC_FOLDER_PDFSEARCHABLE}\\{model.FileName}");

        //    var dFileNotNull = Guard.StringNotNullOrEmpty(dFile, nameof(dFile));
        //    if (!dFileNotNull) return null;

        //    var dFileExist = Guard.Exist(dFile, nameof(dFile));
        //    if (!dFileExist) return null;

        //    var dObj = new FileProtocol();
        //    dObj.Content = System.IO.File.ReadAllBytes(dFile);
        //    dObj.ContentType = AppDefiner.PDF_CONTENT_TYPE;
        //    return dObj;
        //}
    }
}

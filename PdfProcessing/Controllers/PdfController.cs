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
    }
}

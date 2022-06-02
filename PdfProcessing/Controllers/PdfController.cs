using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfProcessing.Entities;
using PdfProcessing.Services;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PdfProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        IBus _bus;
        IProcessPdfService _processPdfService;

        public PdfController(IBus bus, IProcessPdfService processPdfService)
        {
            _bus = bus;
            _processPdfService = processPdfService;
        }

        [HttpPost]
        [Route("queuefile")]
        public async Task<IActionResult> QueueFile([FromBody] ServiceMessageIn serviceMessageIn)
        {
            Uri uri;
            Log.Information("Queue a file: {serviceMessageIn.InputFileAbsolutePath}, IsHighPriority: {IsHighPriority}", serviceMessageIn.InputFileAbsolutePath, serviceMessageIn.IsHighPriority);

            if (serviceMessageIn.IsHighPriority)
            {
                uri = new Uri(Constants.HIGHQUEUE_URI_SEND);
            }
            else
            {
                uri = new Uri(Constants.LOWQUEUE_URI_SEND);
            }

            var endpoint = _bus.GetSendEndpoint(uri).Result;
            await endpoint.Send(serviceMessageIn);
            return Accepted();
        }
    }
}

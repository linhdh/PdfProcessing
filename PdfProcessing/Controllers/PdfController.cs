using MassTransit;
using Microsoft.AspNetCore.Mvc;
using PdfProcessing.Entities;
using Serilog;
using System;
using System.Threading.Tasks;

namespace PdfProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        IBus _bus;
        
        public PdfController(IBus bus)
        {
            _bus = bus;
        }

        [HttpPost]
        [Route("queuefile")]
        public async Task<IActionResult> QueueFile([FromBody] ServiceMessageIn serviceMessageIn)
        {
            Log.Information("Queue a file: {serviceMessageIn.InputFileAbsolutePath}, IsHighPriority: {serviceMessageIn.IsHighPriority}", serviceMessageIn.InputFileAbsolutePath, serviceMessageIn.IsHighPriority);
            Uri uri = serviceMessageIn.IsHighPriority ? uri = new Uri(Constants.HIGHQUEUE_URI_SEND) : uri = new Uri(Constants.LOWQUEUE_URI_SEND);
            var endpoint = _bus.GetSendEndpoint(uri).Result;
            await endpoint.Send(serviceMessageIn);
            return Accepted();
        }
    }
}

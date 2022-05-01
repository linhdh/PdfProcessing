using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfProcessing.Entities;
using PdfProcessing.Services;
using System;
using System.Collections.Generic;

namespace PdfProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly AppSetting _setting;
        private IConfigurationService _configurationService;

        public ConfigurationController(IConfigurationService configurationService, AppSetting setting)
        {
            _setting = setting;
            _configurationService = configurationService;
        }

        [HttpGet]
        public List<BaseModel> GetAllConfiguration()
        {
            var result = _configurationService.GetConfigurationAll();
            return result;
        }

        [HttpGet]
        [Route("{id}")]
        public BaseModel GetConfigById(int id)
        {
            var result = _configurationService.GetConfigById(id);
            return result;
        }

        [HttpPost]
        [Route("update")]
        public ApiResponse<int> UpdateConfiguration([FromBody] BaseModel configurationModels)
        {

            /*var modelNotNull = Guard.NotNull(configurationModels, nameof(configurationModels));
            if (!modelNotNull || !ModelState.IsValid)
            {
                return new ApiResponse<int>
                {
                    StatusCode = Enums.StatusCode.MODEL_INVALID,
                    Message = LangKeys.MODEL_INVALID
                };
            }*/

            var result = _configurationService.UpdateConfiguration(configurationModels);
            return result;
        }

        [HttpPost]
        public ApiResponse<int> CreateConfig([FromBody] BaseModel model)
        {

            /*var modelNotNull = Guard.NotNull(model, nameof(model));
            if (!modelNotNull || !ModelState.IsValid)
            {
                return new ApiResponse<int>
                {
                    StatusCode = Enums.StatusCode.MODEL_INVALID,
                    Message = LangKeys.MODEL_INVALID
                };
            }*/

            var result = _configurationService.CreateConfig(model);
            return result;
        }

        [HttpGet]
        [Route("qr-code/{tenantId}")]

        public QrCodeConfigModel GetQrCode(Guid tenantId)
        {
            var result = new QrCodeConfigModel();
            if (_setting.Tenants.ContainsKey(tenantId))
                return result = _configurationService.GetQrCode(_setting.Tenants[tenantId]);
            return result;
        }

        [HttpPost]
        [Route("qr-code/{tenantId}")]

        public bool SaveQrCode(Guid tenantId, [FromBody] QrCodeConfigModel model)
        {

            /*var modelNotNull = Guard.NotNull(model, nameof(model));
            if (!modelNotNull)
            {
                return false;
            }*/

            var result = _configurationService.SaveQrCode(tenantId, model);
            return result;
        }

        [HttpGet]
        [Route("delete")]
        // [ApiExplorerSettings(IgnoreApi = true)]
        public bool DeleteConfig(int id)
        {
            var result = _configurationService.DeleteConfig(id);
            return result;
        }
    }
}

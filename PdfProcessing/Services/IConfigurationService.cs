using PdfProcessing.Entities;
using System;
using System.Collections.Generic;

namespace PdfProcessing.Services
{
    public interface IConfigurationService
    {
        List<BaseModel> GetConfigurationAll();
        BaseModel GetConfigById(int id);
        ApiResponse<int> UpdateConfiguration(BaseModel dataModel);
        ApiResponse<int> CreateConfig(BaseModel dataModel);
        QrCodeConfigModel GetQrCode(String tenant);
        bool SaveQrCode(Guid tenantId, QrCodeConfigModel model);
        bool DeleteConfig(int id);
    }
}

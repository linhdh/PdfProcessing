using PdfProcessing.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfProcessing.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _pathFileConfiguration;

        private readonly AppSetting _config;
        public ConfigurationService(AppSetting config)
        {
            _config = config;
            _pathFileConfiguration = Helper.AppGetFilePath(AppDefiner.PDFPROC_FILE_CONFIGURATION, true);
        }

        public List<BaseModel> GetConfigurationAll()
        {
            var dataModel = new List<BaseModel>();
            if (!File.Exists(_pathFileConfiguration))
                return dataModel;
            string dataJson = File.ReadAllText(_pathFileConfiguration);
            if (string.IsNullOrWhiteSpace(dataJson))
                return dataModel;
            dataModel = Helper.DeserializeObject<List<BaseModel>>(dataJson);
            return dataModel;
        }

        public BaseModel GetConfigById(int id)
        {
            var dataModels = GetConfigurationAll();
            try
            {
                return dataModels.FirstOrDefault(t => t.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error(Resources.Resources.ERROR_WHILE_GET_CONFIG + ". Exception: " + ex.ToString());

            }
            return null;
        }

        public ApiResponse<int> UpdateConfiguration(BaseModel dataModel)
        {
            try
            {
                var listDataModel = _pathFileConfiguration.LoadModelsFromFilePath<BaseModel>();
                bool isDuplicate = listDataModel.Where(x => x.Id != dataModel.Id).Any(x => x.Key == dataModel.Key);
                if (isDuplicate)
                {
                    return new ApiResponse<int>
                    {
                        StatusCode = StatusCode.DUPLICATE,
                        Message = LangKeys.DUPLCATE_KEY
                    };
                }
                var indexOldRegex = listDataModel.FindIndex(x => x.Id == dataModel.Id);
                listDataModel[indexOldRegex] = dataModel;

                var newDataJson = Helper.SerializeObject(listDataModel, JsonFormatting.Indented);
                File.WriteAllText(_pathFileConfiguration, newDataJson);
                return new ApiResponse<int>
                {
                    Data = dataModel.Id
                };
            }
            catch (Exception ex)
            {
                CustomLog.LogErrorExceptionNoDocument(ex, Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.ERROR_WHILE_UPDATE_CONFIGURATION));
                return new ApiResponse<int>
                {
                    StatusCode = StatusCode.INTERNAL_SERVER_ERROR
                };
            }
        }

        public ApiResponse<int> CreateConfig(BaseModel dataModel)
        {
            try
            {
                var listDataModel = _pathFileConfiguration.LoadModelsFromFilePath<BaseModel>();
                bool isDuplicate = listDataModel.Any(x => x.Key == dataModel.Key);
                if (isDuplicate)
                {
                    return new ApiResponse<int>
                    {
                        StatusCode = StatusCode.DUPLICATE,
                        Message = LangKeys.DUPLCATE_KEY
                    };
                }
                int nextId = listDataModel.GetIdMaxWithStringProperty(AppDefiner.ID);
                dataModel.Id = nextId;
                listDataModel.Add(dataModel);
                var newDataJson = Helper.SerializeObject(listDataModel, JsonFormatting.Indented);
                File.WriteAllText(_pathFileConfiguration, newDataJson);
                return new ApiResponse<int>
                {
                    Data = nextId
                };
            }
            catch (Exception ex)
            {
                CustomLog.LogErrorExceptionNoDocument(ex, Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.ERROR_WHILE_CREATE_CONFIGURATION));
                return new ApiResponse<int>
                {
                    StatusCode = StatusCode.INTERNAL_SERVER_ERROR
                };
            }
        }

        public QrCodeConfigModel GetQrCode(string tenant)
        {
            var path = $"{Helper.BaseDirectory}\\{AppDefiner.QRCODECONFIG}\\{tenant.Trim()}\\{AppDefiner.PDFPROC_FILE_CONFIGQRCODE}";
            if (!File.Exists(path))
                return null;
            string dataJson = File.ReadAllText(path);
            var dataModel = Helper.DeserializeObject<QrCodeConfigModel>(dataJson);
            return dataModel;
        }

        public bool SaveQrCode(Guid tenantId, QrCodeConfigModel model)
        {
            if (_config.Tenants.ContainsKey(tenantId))
            {
                try
                {
                    var path = $"{Helper.BaseDirectory}\\{AppDefiner.QRCODECONFIG}\\{_config.Tenants[tenantId]}\\{AppDefiner.PDFPROC_FILE_CONFIGQRCODE}";
                    var newDataJson = Helper.SerializeObject(model, JsonFormatting.Indented);
                    File.WriteAllText(path, newDataJson);
                }
                catch (Exception ex)
                {
                    CustomLog.LogErrorExceptionNoDocument(ex, Helper.GetErrorFormat(Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.WHILE_SAVE_QR)));
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool DeleteConfig(int id)
        {
            try
            {
                var oldRegularExpressions = _pathFileConfiguration.LoadModelsFromFilePath<BaseModel3>();
                var itemRegex = oldRegularExpressions.FirstOrDefault(x => x.Id == id);
                if (itemRegex != null)
                {
                    oldRegularExpressions.Remove(itemRegex);
                    var newDataJson = Helper.SerializeObject(oldRegularExpressions, JsonFormatting.Indented);
                    File.WriteAllText(_pathFileConfiguration, newDataJson);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                CustomLog.LogErrorExceptionNoDocument(ex, Helper.GetLanguageString(LanguageResource.LanguageResources, LangKeys.ERROR_WHILE_DELETE_CONFIGURATION));
                return false;
            }
        }
    }
}
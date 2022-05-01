using System;
using System.Collections.Generic;

namespace PdfProcessing.Entities
{
    public class AppSetting
    {
        public string ImprovementApiUrl { get; set; }
        public string StatisticalApiUrl { get; set; }
        public string NotificationApiUrl { get; set; }
        public string LogSystemApiUrl { get; set; }
        public string BackendClientApiBase { get; set; }
        public string ContentQRKeeping { get; set; }
        public string ContentQRRemoving { get; set; }
        public int ProcessPageSize { get; set; } = 10;
        public int ProcessColorDepth { get; set; } = 4;
        public long ProcessTimeout { get; set; } = 1000 * 60 * 60 * 4; //4hours.
        public string ProcessDir { get; set; } = "C:\\Temp";
        public string PdfProcessingOcrDir { get; set; }
        public string IronOcrDir { get; set; }
        public string IronPdfDir { get; set; }
        public string MonitorEditorDir { get; set; }
        public string IronOcrLic { get; set; }
        public string IronPdfLic { get; set; }
        public string BaseDirectory { get; set; }
        public bool SendScanDocumentID { get; set; }
        public int RequestTimeout { get; set; }
        public int RetryNumber { get; set; }
        public string UserName { get; set; }
        public string Passwords { get; set; }
        public string Domain { get; set; }
        public string TenantID { get; set; }
        public string TenantName { get; set; }
        public string SendEmailUrl { get; set; }
        public string WebBackendUrl { get; set; }
        public string StatisticalBaseUrl { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string SearchableUrl { get; set; }
        public Dictionary<Guid, string> Tenants { get; set; }
        public string SendFileUrl { get; set; }

        public string QRCodeKeeping { get; set; }
        public string QRCodeRemoving { get; set; }

    }
}
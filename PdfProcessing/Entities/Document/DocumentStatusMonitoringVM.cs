using System.Collections.Generic;

namespace PdfProcessing.Entities.Document
{
    public class DocumentStatusMonitoringVM : DocumentStatusMonitoring
    {
        public List<DocumentStatusMonitoring> Childs { get; set; }
    }
}

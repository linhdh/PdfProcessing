using System;
using System.Collections.Generic;

namespace PdfProcessing.Entities.Document
{
    public class NewDocumentStatus : NewDocStatusBase
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string PhysicalPath { get; set; }
        public string PathFilePriorityDocument { get; set; }
        public List<DocumentStatusDetail> DocumentStatusDetails { get; set; }
        public bool typeProcess { get; set; }
        public string ParentDocumentID { get; set; } = string.Empty;
        public string InitialPath { get; set; }
        public bool IsAddTextLayer { get; set; }
    }
}

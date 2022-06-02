using System;

namespace PdfProcessing.Entities
{
    public class ServiceMessageOut
    {
        public Guid ParentDocumentId { get; set; }
        public Guid DocumentId { get; set; }
        public string InputFileAbsolutePath { get; set; }
        public string DocumentName { get; set; }
        public bool IsHighPriority { get; set; }
        public int Size { get; set; }
    }
}

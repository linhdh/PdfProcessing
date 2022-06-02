using System;

namespace PdfProcessing.Entities
{
    public class ServiceMessageIn
    {
        public Guid DocumentId { get; set; }
        public string InputFileAbsolutePath { get; set; }
        public string DocumentName { get; set; }
        public bool IsHighPriority { get; set; }
    }
}

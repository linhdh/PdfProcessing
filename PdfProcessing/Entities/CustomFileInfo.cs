using System;

namespace PdfProcessing.Entities
{
    public class CustomFileInfo
    {
        public int PriorityOrder { get; set; }
        public bool IsHighPriority { get; set; }
        public string FileFullPath { get; set; }
        public string InitialPath { get; set; }
        public string FileResultPath { get; set; }
        public int CommandType { get; set; }
        public int DocStatusId { get; set; } = 0;
        public Guid NewDocStatusId { get; set; }
        public string DocumentType { get; set; }
        public string DocStatusName { get; set; }
        public string DocumentLanguage { get; set; }
        public string TemplateClassificationName { get; set; }
        public string TemplateExtractionName { get; set; }
        public string PathExtractionTemplate { get; set; }
        public string PathExtractionResult { get; set; }
        public string IdSearchedMatch { get; set; }
        public string CleanUpFilePath { get; set; }
        public string SearchFileResult { get; set; }
        public string FilePDFToDelivery { get; set; }
        public string PatternNaming { get; set; }
        public string DisplayFileName { get; set; }
        public Guid DocumentId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string ScanCategory { get; set; } = string.Empty;
        public int RequestTimeout { get; set; }
    }
}

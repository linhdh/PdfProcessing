using System;
using System.Collections.Generic;

namespace PdfProcessing.Entities.Document
{
    public class Document
    {
        public List<Page> Pages { get; set; } = new List<Page>();
        public int IndexDoc { get; set; }
        public string PatternNaming { get; set; }
        public string BaseFileNamePdf { get; set; }
        public string LanguageCode { get; set; }
        public string TemplateClassificationName { get; set; }
        public Guid TemplateExtractionId { get; set; }
        public string FilePDFToDelivery { get; set; }
        public string ScanDocumentID { get; set; } = string.Empty;
        public string FileFullPath { get; set; }
        public bool IsOnlyImage { get; set; }
    }
}

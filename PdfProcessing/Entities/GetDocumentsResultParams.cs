using IronPdf;

namespace PdfProcessing.Entities
{
    public class GetDocumentsResultParams
    {
        public string FileName { get; set; }
        public PdfDocument PdfData { get; set; }
        public bool IsCreateEditor { get; set; }
        public bool IsCreateSearchAblePdf { get; set; }
        public bool IsAddTextLayer { get; set; }
    }
}
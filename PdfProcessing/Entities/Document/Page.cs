using System.Collections.Generic;

namespace PdfProcessing.Entities.Document
{
    public class Page
    {
        public List<Barcode> Barcodes { get; set; } = new List<Barcode>();
        public List<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();
        public List<Word> Words { get; set; } = new List<Word>();
        public List<LineOfText> Lines { get; set; } = new List<LineOfText>();
        public int PageNumber { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Text { get; set; }
        public string LanguageCode { get; set; }
        public string PagePdfFullPath { get; set; }
        public string PageImageFullPath { get; set; }
    }
}

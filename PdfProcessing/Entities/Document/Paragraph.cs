using System.Collections.Generic;

namespace PdfProcessing.Entities.Document
{
    public class Paragraph
    {
        public List<Line> Lines { get; set; } = new List<Line>();
        public int ParagraphNumber { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
    }
}

using System.Collections.Generic;

namespace PdfProcessing.Entities.Document
{
    public class Line
    {
        public List<Word> Words { get; set; } = new List<Word>();
        public int LineNumber { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
    }
}

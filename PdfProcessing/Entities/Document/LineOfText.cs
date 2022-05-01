using System.Collections.Generic;
using System.Drawing;

namespace PdfProcessing.Entities.Document
{
    public class LineOfText
    {
        public override string ToString()
        {
            return Text;
        }

        public Rectangle Location { get; set; }
        public List<Word> Words { get; set; }
        public int LineNumber { get; set; }
        public int PageNumber { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Text { get; set; }

        public int WordCount { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}

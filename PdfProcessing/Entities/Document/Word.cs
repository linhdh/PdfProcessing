using System.Collections.Generic;
using System.Drawing;

namespace PdfProcessing.Entities.Document
{
    public class Word
    {
        public override string ToString()
        {
            return Text;
        }

        public OcrFontInfo FontInfos { get; set; }
        public Rectangle Location { get; set; }
        public List<SuggestWord> SuggestWords { get; set; } = new List<SuggestWord>();
        public int WordNumber { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public string KeySounding { get; set; }
    }
}

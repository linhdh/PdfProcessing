using System.Drawing;

namespace PdfProcessing.Entities.Document
{
    public class Barcode
    {
        public Rectangle Location { get; set; }
        public int BarcodeNumber { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Value { get; set; }
        public string BarcodeImageFullPath { get; set; }
    }
}

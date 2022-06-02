using PdfProcessing.Entities.Document;
using PdfProcessing.Enums;

namespace PdfProcessing.Application.Entities
{
    public class CapturedOcr
    {        
        public int PageIndex { get; set; }
        public Page Results { get; set; }
        public TypesQRCode QRCodeType { get; set; }
    }
}
using PdfProcessing.Entities.Document;
using PdfProcessing.Enums;

namespace PdfProcessing.Entities
{
    public class PageInfo
    {        
        public int PageIndex { get; set; }
        public int PageIndexExact { get; set; }
        public int GroupNumber { get; set; }
        public Page Results { get; set; }
        public TypesQRCode? QRCodeType { get; set; }
    }
}
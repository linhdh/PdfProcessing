using System;

namespace PdfProcessing.Entities.Document
{
    public class DocStatusBase
    {
        public int Id { get; set; }
        public Guid NewId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Size { get; set; }
        public bool IsAllOkay { get; set; }
        public bool IsProcessing { get; set; }
        public string ScanDocumentID { get; set; } = string.Empty;
        public string FileNameStatus { get; set; }
        public bool IsOnlyImage { get; set; }
    }
}

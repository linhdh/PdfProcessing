namespace PdfProcessing.Entities.Document
{
    public class DocumentStatusToExport
    {
        public string ScanDocumentID { get; set; } = string.Empty;
        public string Name { get; set; }
        public string CreatedDateStr { get; set; }
        public string Size { get; set; }
        public string ModifiedDateStr { get; set; }
        public int IProcess { get; set; }
        public int IImprove { get; set; }
        public int IClassify { get; set; }
        public int IExtract { get; set; }
        public int IDelivery { get; set; }
    }
}

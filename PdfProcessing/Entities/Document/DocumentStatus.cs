namespace PdfProcessing.Entities.Document
{
    public class DocumentStatus : DocStatusBase
    {
        public string PhysicalPath { get; set; }
        public string PathFilePriorityDocument { get; set; }
        public DocumentStatusDetail IProcess { get; set; }
        public DocumentStatusDetail IImprove { get; set; }
        public DocumentStatusDetail IClassify { get; set; }
        public DocumentStatusDetail IExtract { get; set; }
        public DocumentStatusDetail IDelivery { get; set; }
        public bool typeProcess { get; set; }
        public string ParentDocumentID { get; set; } = string.Empty;
        public string InitialPath { get; set; }
        public bool IsAddTextLayer { get; set; }
    }    
}



namespace PdfProcessing.Entities.Document
{
    public class DocumentStatusView : DocStatusBase
    {
        public DocStatusDetailBase IProcess { get; set; }
        public DocStatusDetailBase IImprove { get; set; }
        public DocStatusDetailBase IClassify { get; set; }
        public DocStatusDetailBase IExtract { get; set; }
        public DocStatusDetailBase IDelivery { get; set; }
        public string CreatedDateStr => CreatedDate?.ToString("dd.MM.yyyy HH:mm:ss") ?? "";
        public string ModifiedDateStr => ModifiedDate?.ToString("dd.MM.yyyy HH:mm:ss") ?? "";
        public string InitialPath { get; set; }
        public bool IsAddTextLayer { get; set; }
    }
}

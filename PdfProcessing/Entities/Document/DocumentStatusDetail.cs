using OcrImprovement.Enums;

namespace PdfProcessing.Entities.Document
{
    public class DocumentStatusDetail : DocStatusDetailBase
    {
        public Process Process { get; set; }
        public string DataState { get; set; } = "";
        public bool IsOnlyImage { get; set; }
    }
}

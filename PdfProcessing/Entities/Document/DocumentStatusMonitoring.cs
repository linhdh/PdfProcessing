using OcrImprovement.Entities.GridPage;

namespace PdfProcessing.Entities.Document
{
    public class DocumentStatusMonitoring : DocumentStatusView, IRowIndex
    {
        public string ParentDocumentID { get; set; } = string.Empty;
        public int RowIndex { get; set; }
    }
}

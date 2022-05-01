namespace PdfProcessing.Entities.GridPage
{
    public interface IGridOrderBy
    {
        string fieldName { get; set; }
        
        bool isDesc { get; set; }

    }
}

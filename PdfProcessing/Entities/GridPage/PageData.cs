using System.Collections.Generic;

namespace PdfProcessing.Entities.GridPage
{
    public class PageData<T> where T : IRowIndex
    {
        public List<T> DataSource { get; set; }

        public int TotalRows { get; set; } = 0;
    }
}

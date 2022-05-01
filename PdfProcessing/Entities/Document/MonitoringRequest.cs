using OcrImprovement.Entities.GridPage;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PdfProcessing.Entities.Document
{
    public class MonitoringRequest : IGridPage
    {
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;

        [Required]
        public List<FilterCriteria> filters { get; set; }
        public List<SortItem> sorts { get; set; }
    }
}

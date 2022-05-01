using PdfProcessing.Entities.Document;
using System.Collections.Generic;

namespace PdfProcessing.Entities
{
    public class DocumentForTextLayerModel
    {
        public string BaseFileNamePdf { get; set; }
        public string LanguageCode { get; set; }
        public bool IsOnlyImage { get; set; }
        public List<Page> Pages { get; set; }
    }
}
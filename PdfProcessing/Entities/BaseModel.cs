using System.ComponentModel.DataAnnotations;

namespace PdfProcessing.Entities
{
    public class BaseModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Key { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string Value { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace PdfProcessing.Entities
{
    public class FileProtocol
    {
        [Required(AllowEmptyStrings = false)]
        public string FileName { get; set; }

        public string ContentType { get; set; }
        
        [Required]
        [MinLength(1)]
        public byte[] Content { get; set; }
    }
}

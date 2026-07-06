using TMG.Models.CompanyModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.MiscModels
{
    public class MisStamp : BaseEntity
    {
        public int MisStampId { get; set; }
        public string FilePath { get; set; } 
        public string SignatureFilePath { get; set; }
        public string AccountantSignatureFilePath { get; set; }
        public int CompanyId { get; set; }
        public Company company { get; set; }
        [NotMapped, Required] public IFormFile File { get; set; }
        [NotMapped, Required] public IFormFile FileSignature { get; set; }
        [NotMapped, Required] public IFormFile AccountantSignatureFile { get; set; }
    }
}

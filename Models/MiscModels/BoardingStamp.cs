using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.MiscModels
{
    public class BoardingStamp : BaseEntity
    {
        public int BoardingStampId {  get; set; }
        public string FilePath { get; set; }
        [NotMapped, Required] public IFormFile File { get; set; }
    }
}

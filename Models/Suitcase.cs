using TMG.Models.PackageModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models
{
    public class Suitcase : BaseEntity
    {
        public int SuitcaseId { get; set; }
        public string SuitcaseCode { get; set; }
        public string Color { get; set; }
        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }
    }
}

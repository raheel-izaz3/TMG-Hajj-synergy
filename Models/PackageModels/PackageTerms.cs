using System.ComponentModel.DataAnnotations;
using TMG.Models.MiscModels;

namespace TMG.Models.PackageModels
{
    public class PackageTerms: BaseEntity
    {
        public int PackageTermsId { get; set; }
        [Required]
        public string PackageTerm {  get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

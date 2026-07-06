using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;

namespace TMG.Models.CompanyModels
{
    public class Munazzim: BaseEntity
    {
        public int MunazzimId { get; set; }

        [Required, Display(Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "Cluster #", Prompt = "Enter Cluster #...")]
        public int ClusterNumber { get; set; }

        public string StampPath { get; set; }
        public string SignaturePath { get; set; }

        [Display(Name = "Stamp File")]
        [NotMapped] public IFormFile StampFile { get; set; }
        
        [Display(Name = "Signature File")]
        [NotMapped] public IFormFile SignatureFile { get; set; }
        
        [NotMapped] public List<int> CompaniesId { get; set; }
        [NotMapped] public List<bool> SelectedCompanies { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }


    }
}
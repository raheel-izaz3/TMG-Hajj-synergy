using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.MiscModels
{
    public class HajjAgreement: BaseEntity
    {
        public int HajjAgreementId { get; set; }
        // public int PackageId { get; set; }
        // public Package Package{ get; set; }

        [Required, Display(Name = "Title", Prompt = "Enter Title...")]
        public string Title { get; set; }
        
        [Required, Display(Name = "For the Year", Prompt = "Enter Year...")]
        public string ForTheYear { get; set; }

        [Required, Display(Name = "Shared Agreement")]
        [Column(TypeName = "text")]
        public string SharedAgreement { get; set; }

        [Display(Name = "Package Specific Agreement")]
        [Column(TypeName = "text")]
        public string SpecificAgreement { get; set; }

        
        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }

    }
}

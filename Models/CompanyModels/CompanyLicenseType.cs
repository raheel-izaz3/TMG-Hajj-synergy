using System.ComponentModel.DataAnnotations;

namespace TMG.Models.CompanyModels
{
    public class CompanyLicenseType: BaseEntity
    {
        public int CompanyLicenseTypeId { get; set; }

        [Required]
        [Display(Name = "License", Prompt = "Enter License Name...")]
        public string LicenseName { get; set; }
    }
}

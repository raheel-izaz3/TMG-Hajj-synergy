using System.ComponentModel.DataAnnotations;

namespace TMG.Models.CompanyModels
{
    public class CompanyEmailType: BaseEntity
    {
        public int CompanyEmailTypeId { get; set; }

        [Required]
        [Display(Name = "Email Type", Prompt = "Enter Email Type...")]
        public string EmailType { get; set; }
    }
}

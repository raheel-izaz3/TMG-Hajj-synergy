using System.ComponentModel.DataAnnotations;

namespace TMG.Models.CompanyModels
{
    public class CompanyContactType: BaseEntity
    {
        public int CompanyContactTypeId { get; set; }

        [Required]
        [Display(Name = "Contact Type", Prompt = "Enter Contact Type...")]
        public string ContactType { get; set; }
    }
}

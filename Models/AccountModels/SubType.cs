using TMG.Models.CompanyModels;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccountModels
{
    public class SubType: BaseEntity
    {
        public int SubTypeId { get; set; }

        [Required, Display(Name = "Account Type")]
        public int TypeId { get; set; }
        public Type Type { get; set; }

        [Required, Display(Name = "Account Sub Type", Prompt = "Enter Account Sub Type...")]
        public string Name { get; set; }

        [Required, Display(Name = "Company ")]

        public int CompanyId { get; set; }

        public Company Company { get; set; }    
    }
}

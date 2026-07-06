using System.ComponentModel.DataAnnotations;
using TMG.Models.CompanyModels;

namespace TMG.Models.Updated_Accounts
{
    public class AccountGroups : BaseEntity
    {
        [Key]
        public int AccountGroupId { get; set; }

        [Required, Display(Name = "Account Sub Type", Prompt = "Enter Account Sub Type...")]
        public string Name { get; set; }

        // FK

        [Required, Display(Name = "Account Type")]
        public int AccountTypesId { get; set; }
        public AccountTypes AccountTypes { get; set; }
    }
}

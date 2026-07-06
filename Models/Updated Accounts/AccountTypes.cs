using System.ComponentModel.DataAnnotations;
using TMG.Models.AccountModels;

namespace TMG.Models.Updated_Accounts
{
    public class AccountTypes : BaseEntity
    {
        [Key]
        public int AccountTypeId { get; set; }

        [Required, Display(Name = "Account Type", Prompt = "Enter Account Type...")]
        public string Name { get; set; }

        public AccountNature AccountNature { get; set; }

    }
}

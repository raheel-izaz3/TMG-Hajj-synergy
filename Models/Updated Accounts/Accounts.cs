using System.ComponentModel.DataAnnotations;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;
using TMG.Models.MiscModels;

namespace TMG.Models.Updated_Accounts
{
    public class Accounts : BaseEntity
    {
        [Key]
        public int AccountsId { get; set; }

        [Required, Display(Name = "Account Name", Prompt = "Enter Account Name...")]
        public string Name { get; set; }
        public bool IsHajjiAccount { get; set; } = false;
        public bool IsBank { get; set; } = false;
        public bool IsTax { get; set; } = false;
        public bool IsVendorAccount { get; set; } = false;
        public bool IsExpenseAccount { get; set; } = false;
        
        [Display(Name = "Opening Balance")]
        public float? OpeningBalance { get; set; }
        public Currencies? Currencies { get; set; }

        // FK

        [Required, Display(Name = "Category")]
        public int AccountGroupsId { get; set; }
        public AccountGroups AccountGroups { get; set; }


        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        [Required, Display(Name = "Company")]
        public int? CompanyId { get; set; }
        //public Company Company { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

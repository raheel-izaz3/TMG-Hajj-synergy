using System.ComponentModel.DataAnnotations;
using TMG.Models.CompanyModels;
using TMG.Models.Updated_Accounts;
using TMG.Models.MiscModels;

namespace TMG.Models.AccountModels
{
    public class ExpenseSubType : BaseEntity
    {
        public int ExpenseSubTypeId { get; set; }
        public string ExpenseSubTypeName { get; set; }

        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        [Display(Name = "Currency")]
        public Currencies Currency { get; set; }

        [Display(Name = "Account Title")]
        public string? AccountTitle {  get; set; }
        
        [Display(Name = "IBAN")]
        public string? IBAN { get; set; }

        [Display(Name = "Swift Code")] 
        public string? SwiftCode { get; set; }

        [Display(Name = "NTN")]
        public string? NTN { get; set; }

        [Display(Name = "Beneficiary Address")]
        public string? BeneficiaryAddress { get; set; }

        [Display(Name = "Bank Address")]
        public string? BankAddress { get; set; }

        // Expense Type
        public int ExpenseTypeId { get; set; }
        public ExpenseType ExpenseType { get; set; }

        // Account
        public int? AccountsId { get; set; }
        public Accounts Accounts { get; set; }
        
        // Company
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        
        // HajjSeasonID
        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

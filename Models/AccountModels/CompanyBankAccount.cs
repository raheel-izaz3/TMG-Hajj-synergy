using TMG.Models.CompanyModels;
using System.ComponentModel.DataAnnotations;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Models.AccountModels
{
    public class CompanyBankAccount : BaseEntity
    {
        public int CompanyBankAccountId { get; set; }

        [Required, Display(Name = "Company", Prompt = "")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Display(Name = "Account Currency")]
        public Currencies AccountCurrency { get; set; }

        public int? BankId { get; set; }
        public Bank Bank { get; set; }


        [Required, Display(Name = "Branch Code", Prompt = "Enter Branch Code")]
        public string BranchCode { get; set; }

        [Required, Display(Name = "Account Title", Prompt = "Enter Account Title...")]
        public string Title { get; set; }

        [Required, Display(Name = "Account Number", Prompt = "Enter Account Number...")]
        public string Number { get; set; }

        [Required, Display(Name = "Account IBAN", Prompt = "Enter Account IBAN...")]
        public string IBAN { get; set; }

        [Required, Display(Name = "Swift", Prompt = "Enter Swift...")]
        public string Swift { get; set; }

        [Required, Display(Name = "Beneficiary Address", Prompt = "Enter Beneficiary Address ...")]
        public string BeneficiaryAddress { get; set; }

        [Required, Display(Name = "Bank Address", Prompt = "Enter Bank Address...")]
        public string BankAddress { get; set; }

        [Required, Display(Name = "NTN", Prompt = "Enter Credit Limit...")]
        public string NTN { get; set; }

        [Required, Display(Name = "Account's Balance", Prompt = "Enter Account's Balance...")]
        public float AccountBalance { get; set; }


        public int? AccountId { get; set; }
        public Account Account { get; set; }

        // Account
        public int? BankAssetAccountId { get; set; }
        public int? BankExpenseAccountId { get; set; }
        public Accounts Accounts { get; set; }

    }
}

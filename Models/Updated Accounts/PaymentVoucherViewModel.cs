using System.ComponentModel.DataAnnotations;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;

namespace TMG.Models.Updated_Accounts
{
    public class PaymentVoucherViewModel
    {
        public int NewPaymentVoucherId { get; set; }
        public int JVId { get; set; }
        public string PaymentVoucherNo { get; set; }
        public string Date { get; set; }
        public DateTime DateEdit { get; set; }

        [Display(Name = "Credit Remarks")]
        public string? CreditRemarks { get; set; }
        [Display(Name = "Debit Remarks")]
        public string? DebitRemarks { get; set; }
        public float? TotalDebit { get; set; }
        public float? TotalCredit { get; set; }
        public float? ROE { get; set; }
        public float? Amount { get; set; }
        public string? InstrumentNumber { get; set; }
        public string? CreatorSignature { get; set; }
        public IFormFile? CreatorSignatureFile { get; set; }
        public string? FinanceOfficerSignature { get; set; }
        public IFormFile? FinanceOfficerSignatureFile { get; set; }
        public string? AuthorizingAgentSignature { get; set; }
        public IFormFile? AuthorizingAgentSignatureFile { get; set; }
        public string? CEOSignature { get; set; }
        public string File { get; set; }
        public IFormFile? CEOSignatureFile { get; set; }
        public List<NewPayment> Payments { get; set; }
        public List<NewTax> Taxes { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

        // Others

        public int CreditId { get; set; }
        public string CreditName { get; set; }
        public int DebitId { get; set; }
        public string DebitName { get; set; }

        public string PrintDate { get; set; }
        public string PrintedBy { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyContact { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanySite { get; set; }

        // Taxes
        public float? DebitBankCharges { get; set; }
        public float? DebitOPAPTax { get; set; }
        public float? DebitRemittanceTax { get; set; }
        public float? DebitThirdPartyTax { get; set; }
        public float? DebitOtherTaxes { get; set; }
        public float? DebitBouncePanelty { get; set; }
        public float? CreditBankCharges { get; set; }
        public float? CreditOPAPTax { get; set; }
        public float? CreditRemittanceTax { get; set; }
        public float? CreditThirdPartyTax { get; set; }
        public float? CreditOtherTaxes { get; set; }
        public float? CreditBouncePanelty { get; set; }
        public bool IsCreditBank { get; set; }
        public bool IsDebitBank { get; set; }
        public float? WTH { get; set;}
        public string IsIBFT { get; set; }
        public string PORefNo { get; set; }
        public float? SalesTax { get; set; }
        public float? AdvanceTax { get; set; }
        public int? EmployeeId { get; set; }
        public float? BasicSalary { get; set; }
        public float? Others { get; set; }
        public float? Fuel { get; set; }
        public float? Medical { get; set; }
        public float? Leaves { get; set; }
        public string EmployeeName { get; set; }
        public string DropDown { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using TMG.Models.InvoiceModels;
using TMG.Controllers.AccountControllers.Updated_Accounts;

namespace TMG.Models.Updated_Accounts
{
    public class NewPaymentVoucher : BaseEntity
    {
        public int NewPaymentVoucherId { get; set; }
        
        [Required]
        public string PaymentVoucherNo { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Display(Name = "Debit Remarks")]
        public string? DebitRemarks { get; set; }

        [Display(Name = "Credit Remarks")]
        public string? CreditRemarks { get; set; }
        public float? TotalDebit { get; set; }
        public float? TotalCredit { get; set; }
        public float? Amount { get; set; }
        
        [Display(Name = "ROE")]
        public float? ROE { get; set; }
        
        [Display(Name = "Instrument Number")]
        public string? InstrumentNumber { get; set; }

        [Display(Name = "Creator Signature")]
        public string? CreatorSignature { get; set; }

        [NotMapped]
        public IFormFile? CreatorSignatureFile { get; set; }
        
        [Display(Name = "Finance Officer Signature")] 
        public string? FinanceOfficerSignature { get; set; }
        
        [NotMapped]
        public IFormFile? FinanceOfficerSignatureFile { get; set; }
        
        [Display(Name = "Authorizing Agent Signature")]
        public string? AuthorizingAgentSignature { get; set; }
        
        [NotMapped]
        public IFormFile? AuthorizingAgentSignatureFile { get; set; }
        
        [Display(Name = "CEO Signature")]
        public string? CEOSignature { get; set; }
        
        [NotMapped]
        public IFormFile? CEOSignatureFile { get; set; }
        public ICollection<NewPayment> Payments { get; set; } = new List<NewPayment>();
        public ICollection<NewTax> Taxes { get; set; } = new List<NewTax>();
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

        // Debit Taxes
        public float? DebitBankCharges { get; set; }
        public float? DebitOPAPTax { get; set; }
        public float? DebitRemittanceTax { get; set; }
        public float? DebitThirdPartyTax { get; set; }
        public float? DebitOtherTaxes { get; set; }
        public float? DebitBouncePanelty { get; set; }
        public float? SalesTax { get; set; }
        public float? AdvanceTax { get; set; }


        // Credit Ta?xes
        public float? CreditBankCharges { get; set; }
        public float? CreditOPAPTax { get; set; }
        public float? CreditRemittanceTax { get; set; }
        public float? CreditThirdPartyTax { get; set; }
        public float? CreditOtherTaxes { get; set; }
		public float? CreditBouncePanelty { get; set; }

        public ReceiptStatus? Status { get; set; }
        public float? WTH { get; set; }
        public string PaymentMode { get; set; }
        public string PORefNo { get; set;}
        public PaymentType PaymentType { get; set; }
        public string File { get; set; }
        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public float? BasicSalary { get; set; }
        public float? Fuel { get; set; }
        public float? Medical { get; set; }
        public float? Others { get; set; }
        public float? Leaves { get; set; }
        public string TypeDropDown { get; set; }
    }

    public class NewPayment : BaseEntity 
    {
        public int NewPaymentId { get; set; }

        [Display(Name = "Type")]
        public AccountNature AccountNature { get; set; }

        [NotMapped]
        public string Currency { get; set; }

        // FK
        public int NewPaymentVoucherId { get; set; }

        [Required, Display(Name = "Accounts")]
        public int AccountsId { get; set; }
        public Accounts Accounts { get; set; }
    }

    public class NewTax : BaseEntity
    {
        public int NewTaxId { get; set; }
        public float TaxPercentage { get; set; }
        public float TaxAmount { get; set; }
        public AccountNature AccountNature { get; set; }

        // FK
        public int? NewPaymentVoucherId { get; set; }
        public int? POVisaId { get; set; }
        public int TaxId { get; set; }
        public Tax Taxes { get; set; }
    }

    public class Tax : BaseEntity
    {
        public int TaxId { get; set; }
        public string TaxName { get; set; }

        [Display(Name = "Claimable")]
        public bool IsClaimable { get; set; } = false;

        [Display(Name = "Tax Type")]
        public TaxType TaxTypes { get; set; }
        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }

    public enum TaxType
    {
        Additive,
        Subtractive
    }

}

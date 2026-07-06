using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using TMG.Models.AccountModels;

namespace TMG.Models.InvoiceModels
{
    public class NewInvoice : BaseEntity
    {
        public NewInvoice()
        {
            ReceiptStatus = ReceiptStatus.Holding;
        }
        public string ClearanceFileBase64 { get; set; }
        public string ClearanceFilePath { get; set; }
        [NotMapped]
        public IFormFile ClearanceFile { get; set; }
        public int NewInvoiceId { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public Currencies Currencies { get; set; }
        public float AmountRecieved { get; set; }
        public float ROE { get; set; }
        public DateTime Date { get; set; }
        public int? CompanyBankAccountId { get; set; }
        public CompanyBankAccount CompanyBankAccount { get; set; }
        public string Remarks { get; set; }
       
        public ReceiptStatus ReceiptStatus { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

        [Display(Name = "Clearance Date")]
        [Column(TypeName = "DATE"), DataType(DataType.Date)]
        public DateTime ClearanceDate { get; set; }

        [Display(Name = "Cheque Date")]
        [Column(TypeName = "DATE"), DataType(DataType.Date)] 
        public DateTime ChequeDate { get; set; }

        [Display(Name = "Cheque Number")]
        public string ChequeNumber { get; set; }

        public int? BankId { get; set; }
        public Bank Bank { get; set; }

        [Column(TypeName = "DATE"), DataType(DataType.Date)]
        [Display(Name = "Deposit Date")]
        public DateTime DepositDate { get; set; }
        public string ImageBase64 { get; set; }
        public string ImagePath { get; set; }
        public string AuthorizorSignatureBase64 { get; set; }
        public string AuthorizorSignaturePath { get; set; }
        [NotMapped] 
        public IFormFile AuthorizorSignatureFile { get; set; }

        public string CreatorSignatureBase64 { get; set; } 
        public string CreatorSignaturePath { get; set; }
        [NotMapped]
        public IFormFile CreatorSignatureFile { get; set; }

        public string FinanceSignatureBase64 { get; set; }
        public string FinanceSignaturePath { get; set; }
        [NotMapped]
        public IFormFile FinanceSignatureFile { get; set; }
        [NotMapped] public IFormFile File { get; set; }

        public string InstrumentNo { get; set; }
        public int ProformaNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public ModeOfPayment ModeOfPayment { get; set; }
        public float BouncePenalty { get; set; }

        [Display(Name = "TP - HB No")]
        public string TPHBNo { get; set; }
        
        [Display(Name = "TP - Recipt No")]
        public string TPHBReciptNo { get; set; }
    }

    public enum ReceiptStatus
    {
        Holding = 0,
        Returned = 1,
        Cleared = 2,
        Bounced = 3
    }
}

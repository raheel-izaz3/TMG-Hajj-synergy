using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.InvoiceModels
{
    public enum ModeOfPayment
    {
        Cheque, Payorder, Online, Cash
    }
    public enum InvoiceType
    {
        Null, Package, Airline, AdditionalFacility, ExtraADFS
    }

    public enum InvoiceStatus
    {

        Unsupervised, Supervised, Pending, Bounce, ClearingInTransit, Cleared
    }



    public class Invoice : BaseEntity
    {

        public Invoice()
        {
            InvoiceStatus = InvoiceStatus.Unsupervised;
        }
        public int ProformaNumber { get; set; }
        public int InvoiceId { get; set; }

        [Display(Name = "Family Head")]
        public int? FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public Currencies Currency { get; set; }

        [Display(Name = "Amount Received")]
        public float AmountRecived { get; set; }


        [Display(Name = "PKR ROE")]
        public float PKRROE { get; set; }

        [Display(Name = "SAR ROE")]
        public float SARROE { get; set; }

        [Display(Name = "USD ROE")]
        public float USDROE { get; set; }
        
        //[Display(Name = "AED ROE")]
        //public float AEDROE { get; set; }

        //[Display(Name = "GBP ROE")]
        //public float GBPROE { get; set; }
        
        //[Display(Name = "EURO ROE")]
        //public float EUROROE { get; set; }

        [Display(Name = "Mode of Payment")]
        public ModeOfPayment ModeOfPayment { get; set; }

        [Display(Name = "Invoice Type")]
        public InvoiceType? InvoiceType { get; set; }


        [Display(Name = "Clearance Date")]
        [Column(TypeName = "DATE"), DataType(DataType.Date)]
        public DateTime ClearanceDate { get; set; }

        [Column(TypeName = "DATE"), DataType(DataType.Date)]
        [Display(Name = "Deposit Date")]
        public DateTime DepositDate { get; set; }
        public string Instrument { get; set; }

        [Column(TypeName = "DATE"), DataType(DataType.Date)]
        [Display(Name = "Instrument Date")]
        public DateTime InstrumentDate { get; set; }
        [Column(TypeName = "DATE"), DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Bank")]
        public int? BankId { get; set; }
        public Bank Bank { get; set; }

        [Display(Name = "Invoice Status")]
        public InvoiceStatus InvoiceStatus { get; set; }
        public string Remarks { get; set; }
        [Column(TypeName = "TEXT")]
        public string ImageBase64 { get; set; }
        public string ImagePath { get; set; }

        [Column(TypeName = "DATE"), DataType(DataType.Date)]
        [Display(Name = "Receipt Date")]
        public DateTime ReceiptDate { get; set; }

        [NotMapped] public IFormFile File { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

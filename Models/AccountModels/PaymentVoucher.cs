using TMG.Models.CompanyModels;
using TMG.Models.InvoiceModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.AccountModels
{
    public enum ShowAccountDebitOrCreditSide
    {
        Debit,
        Credit
    }
    public class PaymentVoucher : BaseEntity
    {
        public int PaymentVoucherId { get; set; }

        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public int VoucherTypeId { get; set; }
        public VoucherType VoucherType { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "DATE")]
        public DateTime Date { get; set; }
        public int? AccountId { get; set; }
        public Account Account { get; set; }
        public string ReceivingParty { get; set; }
        public string InstrumentNumber { get; set; }
        public string Description { get; set; }
        public float Amount { get; set; }
        public Currencies Currency { get; set; }
        public float PKRROE { get; set; }
        public float SARROE { get; set; }
        public float USDROE { get; set; }

        // public int? InvoiceId { get; set; }
        // public Invoice Invoice { get; set; }

    }
}

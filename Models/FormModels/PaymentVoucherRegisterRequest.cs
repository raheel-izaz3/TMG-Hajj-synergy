using TMG.Models.AccountModels;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels
{
    public class PaymentVoucherRegisterRequest
    {
        [Required, Display(Name = "Company")]
        public int CompanyId { get; set; }
        public int? AccountId { get; set; }

        [Required, Display(Name = "Voucher")]
        public int VoucherTypeId { get; set; }

        [Required, Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }



        [Display(Name = "Receipt Party", Prompt = "Enter Party...")]
        public string ReceivingParty { get; set; }

        [Display(Name = "Instrument Number", Prompt = "Enter Instrument Number...")]
        public string InstrumentNumber { get; set; }

        [Required, Display(Name = "Description", Prompt = "Enter Description...")]
        public string Description { get; set; }

        public List<int> AccountIds { get; set; }
        public List<float> DebitAmounts { get; set; }
        public List<float> CreditAmounts { get; set; }

        [Required, Display(Name = "Rate of Exchange", Prompt = "Enter R.O.E.")]
        public float PKRROE { get; set; }
        public float SARROE { get; set; }
        public float USDROE { get; set; }
        public Currencies Currency { get; set; }
    }
}

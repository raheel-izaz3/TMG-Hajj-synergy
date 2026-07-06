using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class QuotationPaymentPolicy : BaseEntity
    {
        public int QuotationPaymentPolicyId { get; set; }

        [Required, Display(Name = "Payment Policy", Prompt = "Enter Payment Policy...")]
        public string PaymentPolicy { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class QuotationTC : BaseEntity
    {
        public int QuotationTCId { get; set; }

        [Required, Display(Name = "Terms & Conditions", Prompt = "Enter T&C...")]
        public string TermsCondtion { get; set; }
    }
}

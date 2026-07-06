using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class QurbaniSlipTC : BaseEntity
    {
        public int QurbaniSlipTCId { get; set; }

        [Required, Display(Name = "Terms & Conditions", Prompt = "Enter T & C...")]
        public string SlipTC { get; set; }
    }
}

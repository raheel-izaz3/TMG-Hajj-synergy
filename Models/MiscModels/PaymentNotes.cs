using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class PaymentNotes : BaseEntity
    {
        public int PaymentNotesId { get; set; }

        [Required, Display(Name = "Payment Note", Prompt = "Enter Payement Note...")]
        public string Note { get; set; }
    }
}

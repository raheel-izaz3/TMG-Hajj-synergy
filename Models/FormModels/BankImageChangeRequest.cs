using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels
{
    public class BankImageChangeRequest
    {
        public int BankId { get; set; }

        [Required, Display(Name = "Image File")]
        public IFormFile File { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels
{
    public class BankRegisterRequest
    {
        [Required, Display(Name = "Bank Name", Prompt = "Enter Bank Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "Bank Logo")]
        public IFormFile File { get; set; }
    }
}

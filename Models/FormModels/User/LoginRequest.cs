using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.User
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email{ get; set; }

        [Required, MinLength(8, ErrorMessage = "Minimum 8 characters required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public int? ErrorStatusCode { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.User
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(8, ErrorMessage = "Minimum 8 characters required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, MinLength(8, ErrorMessage = "Minimum 8 characters required")]
        [DataType(DataType.Password)]
        [DisplayName("Confirm Password")]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [Required]
        [Range(1000, 9999, ErrorMessage = "Pin Code must be exactly 4 digits.")]
        public int? PinCode { get; set; }

        public int RoleId { get; set; }
        public IFormFile? UserSignatureFile { get; set; }
        public List<int> CompanyId { get; set; }

        //public IFormFile Image { get; set; }
    }
} 

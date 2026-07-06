using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.User
{
    public class UserEditRequest
    {
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public int? RoleId { get; set; }
        public IFormFile? UserSignatureFile { get; set; }
        public string? UserSignature { get; set; }

        public int? PinCode { get; set; }

        public List<int> CompanyId { get; set; }
    }
}

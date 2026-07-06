using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.User
{
    public class PasswordChangeRequest
    {
        public int UserId { get; set; }

        [MinLength(4), MaxLength(4)]
        public int? PinCode { get; set; }

        [Required, MinLength(8, ErrorMessage = "asd")]
        [DataType(DataType.Password)]
        [DisplayName("New Password")]
        public string NewPassword { get; set; }

        [Required, MinLength(8, ErrorMessage = "asd")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        [DisplayName("Confirm New Password")]
        public string ConfirmNewPassword { get; set; }
    }
}

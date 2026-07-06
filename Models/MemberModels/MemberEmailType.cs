using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MemberModels
{
    public class MemberEmailType: BaseEntity
    {
        public int MemberEmailTypeId { get; set; }
        
        [Required]
        [Display(Name = "Email Type", Prompt = "Enter Email Type...")]
        public string EmailType { get; set; }
    }
}

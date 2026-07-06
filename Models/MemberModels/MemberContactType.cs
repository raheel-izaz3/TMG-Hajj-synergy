using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MemberModels
{
    public class MemberContactType: BaseEntity
    {
        public int MemberContactTypeId { get; set; }

        [Required]
        [Display(Name = "Contact Type", Prompt = "Enter Contact Type...")]
        public string ContactType { get; set;}
    }
}

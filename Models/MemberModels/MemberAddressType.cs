using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MemberModels
{
    public class MemberAddressType: BaseEntity
    {
        public int MemberAddressTypeId { get; set; }

        [Required]
        [Display(Name = "Address Type", Prompt = "Enter Address Type...")]
        public string AddressType { get; set; }

    }
}

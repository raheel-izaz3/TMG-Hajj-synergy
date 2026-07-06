using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.CompanyModels
{
    public class CompanyAddressType: BaseEntity
    {
        public int CompanyAddressTypeId { get; set; }

        [Required]
        [Display(Name = "Address Type", Prompt = "Enter Address Type...")]
        public string AddressType { get; set; }
    }
}

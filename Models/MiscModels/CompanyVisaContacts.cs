using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class CompanyVisaContacts
    {
        public int CompanyVisaContactsId { get; set; }
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
        public int CompanyVisasId { get; set; }
        public CompanyVisas CompanyVisas { get; set; }

    }
}

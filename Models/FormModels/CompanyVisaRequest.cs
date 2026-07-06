using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels
{
    public class CompanyVisaRequest
    {
        [Display(Name = "Maktab Number")]
        public string MaktabNumber { get; set; }
        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Office Address")]
        public string OfficeAddress { get; set; }

        [Display(Name = "Contact Person Name")]
        public string ContactPersonName { get; set; }

        public string MunazimName { get; set; }
        public string SaudiMunazimNo { get; set; }
        public string HGO { get; set; }
        public string MoraNo { get; set; }

        public int? VendorId { get; set; }
        [Display(Name = "Contact Number")]
        public List<string> ContactNumber { get; set; } = new List<string>();
    }
}

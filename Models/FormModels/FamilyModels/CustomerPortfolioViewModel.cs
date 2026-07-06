using TMG.Models.FamilyModels;

namespace TMG.Models.FormModels.FamilyModels
{
    public class CustomerPortfolioViewModel
    {
        public int SerialNumber { get; set; }
        public int CustomerPortfolioId { get; set; }
        public string FullName { get; set; }
        public string SPOName { get; set; }
        public string CustomerRemarks { get; set; }
        public string CustomerStatus { get; set; }
        public string ReferenceName { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string HajjDepartmentName { get; set; }
        public string Feedby { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string HajjDepartment { get; set; }
        public DateTime DateOfQuery { get; set; }
        public string DateOfInquiry { get; set; }
        public DateTime? DateOfEmail { get; set; }
        public ApplicantType ApplicantType { get; set; }
        public string Package { get; set; }
        public string Department { get; set; }
    }
}

namespace TMG.Models.CompanyModels
{
    public class CompanyLicenses
    {
        public int CompanyLicensesId { get; set; }
        public string CompanyLicense { get; set; }
        public string IssuePlace { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ValidUpto { get; set; }
        public bool Preferred { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public int CompanyLicenseTypeId { get; set; }
        public virtual CompanyLicenseType CompanyLicenseType { get; set; }
        
    }
}

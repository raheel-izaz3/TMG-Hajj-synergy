namespace TMG.Models.CompanyModels
{
    public class CompanyEmails
    {
        public int CompanyEmailsId { get; set; }
        public string CompanyEmail { get; set; }
        public bool Preferred { get; set; }

        public int CompanyEmailTypeId { get; set; }
        public virtual CompanyEmailType CompanyEmailType { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}

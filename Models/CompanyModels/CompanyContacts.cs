namespace TMG.Models.CompanyModels
{
    public class CompanyContacts
    {
        public int CompanyContactsId { get; set; }
        public string CompanyContact { get; set; }
        public bool Preferred { get; set; }
        
        public int CompanyContactTypeId { get; set; }
        public virtual CompanyContactType CompanyContactType { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}

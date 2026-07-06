namespace TMG.Models.CompanyModels{
    public class MunazzimCompanies{
        public int MunazzimCompaniesId { get; set; }
        public int MunazzimId { get; set; }
        public Munazzim Munazzim { get; set; }
        public int CompanyId {get;set;}
        public Company Company {get;set;}
        
    }
}
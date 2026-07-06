namespace TMG.Models.CompanyModels
{
    public class UserCompanies
    {
        public int UserCompaniesId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }

        public User User { get; set; }
        public Company Company { get; set; }

    }
}

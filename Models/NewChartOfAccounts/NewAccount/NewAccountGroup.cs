using TMG.Models.CompanyModels;

namespace TMG.Models.NewChartOfAccounts.NewAccount
{
    public class NewAccountGroup :  BaseEntity
    {
        public int NewAccountGroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }
        public string Nature { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}

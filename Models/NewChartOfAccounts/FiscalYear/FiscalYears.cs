using TMG.Models.CompanyModels;

namespace TMG.Models.NewChartOfAccounts.FiscalYear
{
    public class FiscalYears : BaseEntity
    {
        public int FiscalYearsId { get; set; }
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
        public DateTime YearStart { get; set; }
        public DateTime YearEnd { get; set; }
        public bool IsClosed { get; set; }

    }
}

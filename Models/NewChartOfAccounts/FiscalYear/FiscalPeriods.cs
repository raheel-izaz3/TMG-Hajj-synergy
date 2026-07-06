using TMG.Models.CompanyModels;

namespace TMG.Models.NewChartOfAccounts.FiscalYear
{
    public class FiscalPeriods : BaseEntity
    {
        public int FiscalPeriodsId { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTime YearStart { get; set; }
        public DateTime YearEnd { get; set; }
        public bool IsClosed { get; set; }
        public int FiscalYearsId { get; set; }
        public FiscalYears FiscalYears { get; set; }

    }
}

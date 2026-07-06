namespace TMG.Models.NewChartOfAccounts.Currencies
{
    public class CurrencyType : BaseEntity
    {
        public int CurrencyTypeId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string Symbol { get; set; }
    }
}

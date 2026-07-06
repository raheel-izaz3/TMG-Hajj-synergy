namespace TMG.Models
{
    public class ReceiptReportModel
    {
        public DateTime Date { get; set; }
        public string CompanyName { get; set; }
        public string BankName { get; set; }
        public string HajjiName { get; set; }
        public string HBNumber { get; set; }
        public string CollectionCurrency { get; set; }
        public string HajjiCurrency { get; set; }
        public string Remarks { get; set; }
        public string Voucher { get; set; }
        public float? Debit { get; set; } 
        public float Credit { get; set; }
        public float Balance { get; set; } 
    }

}

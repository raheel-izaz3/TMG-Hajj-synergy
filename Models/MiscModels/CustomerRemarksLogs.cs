namespace TMG.Models.MiscModels
{
    public class CustomerRemarksLogs
    {
        public int CustomerRemarksLogsId { get; set; }
        public int CustomerPortfolioId { get; set; }
        public string Remarks { get; set; }
        public int? CustomerRemarksId { get; set; }
        public CustomerRemarks CustomerRemarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

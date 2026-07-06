namespace TMG.Models.ReportModels
{
    public record CompanyWiseRailwayTravelReportModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int EconomySeats { get; set; }
        public int BusinessSeats { get; set; }
        public int SelfSeats { get; set; }
    }

}

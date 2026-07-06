namespace TMG.Models.AirlineModels
{
    public record FlightSummaryDailyReportModel
    {
        public List<FlightSummaryDailyReportMembers> FlightDailyMembers { get; set; } = new();
    }
}
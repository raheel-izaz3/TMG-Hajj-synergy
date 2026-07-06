namespace TMG.Models.AirlineModels
{
    public record FlightSummaryDailyReportMembers
    {
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string PlaceName { get; set; }
        public string PlaceId { get; set; }
        public string PAXCount { get; set; }
    }
}
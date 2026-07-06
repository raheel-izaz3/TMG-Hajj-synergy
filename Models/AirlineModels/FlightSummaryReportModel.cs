namespace TMG.Models.AirlineModels
{
    public record FlightSummaryReportModel
    {
        public List<FlightSummaryMembers> FlightMembers { get; set; } = new List<FlightSummaryMembers>();
        public List<string> departureDateHeader { get; set; } = new();
        public List<string> arrivalDateHeader { get; set; } = new();
    }
}
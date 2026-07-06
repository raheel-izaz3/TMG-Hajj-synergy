namespace TMG.Models.ReportModels
{
    public class CombinedFlightsResult
    {
        public string Location { get; set; }
        public DateTime DepartureDate { get; set; }
        public int TotalEconomySeats { get; set; }
        public int TotalBusinessSeats { get; set; }
        public int TotalSelfTickets { get; set; }
    }
}

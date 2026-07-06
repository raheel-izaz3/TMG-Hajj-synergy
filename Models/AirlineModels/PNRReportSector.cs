namespace TMG.Models.AirlineModels
{
    public record PNRReportSector
    {
        public int SectorNumber { get; set; }
        public string FlightNumber { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
    }
}
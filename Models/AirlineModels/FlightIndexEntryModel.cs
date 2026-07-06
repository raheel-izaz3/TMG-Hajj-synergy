namespace TMG.Models.AirlineModels
{
    public record FlightIndexEntryModel
    {
        public int FlightId { get; set; }
        public string AirlineName { get; set; }
        public string FlightNumber { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public int Capacity { get; set; }
        public int Economy { get; set; }
        public int Business { get; set; }
        public int EconomyBookings { get; set; }
        public int BusinessBookings { get; set; }
        public int EconomyBalance { get; set; }
        public int BusinessBalance { get; set; }
        public List<string> Packages { get; set; } = new List<string>();

    }
}
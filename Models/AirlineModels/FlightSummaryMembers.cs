namespace TMG.Models.AirlineModels
{
    public record FlightSummaryMembers
    {
        public string PackageName { get; set; }
        public string HotelName { get; set; }
        public string HGO { get; set; }
        public string GroupName { get; set; }
        public string PAXName { get; set; }
        public string AirlineName { get; set; }
        public string DepartureDateTime { get; set; }
        public string DepartureDate { get; set; }
        public string PAXNameCount { get; set; }
        public string PNR { get; set; }
        public string OutBound { get; set; }
        public string OutBoundFlightNo { get; set; }
        public string OutboundKSADate { get; set; }
        public string OutBoundKSA { get; set; }
        public string OutBoundPAK { get; set; }
        public string InBound { get; set; }
        public string InBoundDate { get; set; }
        public string InBoundFlightNo { get; set; }
        public string InBoundPAK { get; set; }
        public string InBoundKSA { get; set; }
        public string InboundKSADate { get; set; }
    }
}
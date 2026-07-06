using TMG.Models.AirlineModels;
using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public enum TicketStatus
    {
        Active, InActive
    }
    public enum FlightClass
    {
        Economy, Business, Self, FirstClass
    }
    public class FamilyHeadFlight
    {
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int FamilyHeadFlightId { get; set; }
        public FlightType? FlightType { get; set; }
        public int? AirlineId { get; set; }
        public Airline Airline { get; set; }
        public string? FlightNo { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime? DepartureDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? PNRNo { get; set; }
        public string? PNRGDSNo { get; set; }
        public string? TicketNo { get; set; }
        public string? TicketPDF { get; set; }
        public int? CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public float? FareAmount { get; set; }
        public float? Taxes { get; set; }
        public float? Commission { get; set; }
        public float? Total {  get; set; }
        public TicketStatus? TicketStatus { get; set; }
        public FlightClass? FlightClass { get; set; }
        public int? IssuingAgentId { get; set; }
        public IssuingAgent IssuingAgent { get; set; }
        public string FFNumber { get; set; }
    }
}

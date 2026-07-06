using TMG.Models.AirlineModels;
using TMG.Models.MiscModels;
using TMG.Models.RailwayModels;

namespace TMG.Models.FamilyModels
{
    public enum TrainClass
    {
        Economy, Business
    }
    public class FamilyHeadTrain
    {
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int FamilyHeadTrainId { get; set; }
        public int? RailwayId { get; set; }
        public Railway Railway { get; set; }
        public string? TrainNo { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime? DepartureDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? PNRNo { get; set; }
        public string? TicketNo { get; set; }
        public string? TicketPDF { get; set; }
        public int? CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public float? FareAmount { get; set; }
        public float? Taxes { get; set; }
        public float? Commission { get; set; }
        public float? Total { get; set; }
        public TicketStatus? TicketStatus { get; set; }
        public TrainClass? TrainClass { get; set; }
        public int? IssuingAgentId { get; set; }
        public IssuingAgent IssuingAgent { get; set; }
    }
}

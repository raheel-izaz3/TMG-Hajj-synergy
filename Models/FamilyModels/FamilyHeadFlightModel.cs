using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;

namespace TMG.Controllers.FamilyControllers;

public class FamilyHeadFlightModel
{
    public bool FlightAsPerPackage { get; set; }
    public List<FlightType?> FlightTypes { get; set; }
    public List<int?> FamilyHeadFlightIds { get; set; }
    public List<int?> AirlineIds { get; set; }
    public List<string?> FlightNos { get; set; }
    public List<FlightClass?> FlightClass { get; set; }
    public List<string?> Origins { get; set; }
    public List<string?> Destinations { get; set; }
    public List<DateTime?> DepartureDateTimes { get; set; }
    public List<DateTime?> ArrivalDateTimes { get; set; }
    public List<DateTime?> DepartureTimes { get; set; }
    public List<DateTime?> ArrivalTimes { get; set; }
    public List<string?> PNRNos { get; set; }
    public List<string?> TicketNos { get; set; }
    //public List<IFormFile?> TicketPDFs { get; set; }
    public List<int?> CurrencyIds { get; set; }
    public List<float?> FareAmounts { get; set; }
    public List<float?> Taxes { get; set; }
    public List<float?> Commissions { get; set; }
    public List<float?> Totals { get; set; }
    public List<TicketStatus?> TicketStatuses { get; set; }
    public List<int?> IssuingAgentIds { get; set; }
    public List<bool> Modified { get; set; }
    public List<string> FFNumber { get; set; }
}
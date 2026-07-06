using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;

namespace TMG.Controllers.FamilyControllers;

public class FamilyHeadTrainModel
{
    public bool TrainAsPerPackage { get; set; }
    public List<int?> FamilyHeadTrainIds { get; set; }
    public List<int?> RailwayIds { get; set; }
    public List<string?> TrainNos { get; set; }
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
    public List<TrainClass?> TrainClass { get; set; }
    public List<int?> IssuingAgentIds { get; set; }
    public List<bool> Modified { get; set; }
}
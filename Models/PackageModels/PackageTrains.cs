using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.RailwayModels;

namespace TMG.Models.PackageModels;

public class PackageTrains
{
    public int PackageTrainsId { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; }
    public int? RailwayId { get; set; }
    public Railway Railway { get; set; }
    public string TrainNumber { get; set; }
    public TrainClass? TrainClass { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string PNRNumber { get; set; }
    public float? TicketAmount { get; set; } = 0;
}

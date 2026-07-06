using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;

namespace TMG.Models.PackageModels;

public class PackageFlights
{
    public int PackageFlightsId { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; }
    public FlightType? FlightType { get; set; }
    public int? AirlineId { get; set; }
    public Airline Airline { get; set; }
    public string? FlightNumber { get; set; }
    public FlightClass? FlightClass { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTime? DepartureDate { get; set; } 
    public DateTime? ArrivalDate { get; set; }    
    public DateTime? DepartureTime { get; set; }  
    public DateTime? ArrivalTime { get; set; }
    public string? PNRNumber { get; set; }
    public string? PNRGDSNumber { get; set; }
    public float? TicketAmount { get; set; } = 0;
    public bool IsPreffered { get; set; }
}

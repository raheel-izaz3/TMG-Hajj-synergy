using TMG.Models.LogisticModels;

namespace TMG.Models.FamilyModels;

public enum VehicleType
{
    GroupSharing, Group, Private, Economy, NA
}

public class FamilyHeadItineraryVehicles
{
    public int FamilyHeadItineraryVehiclesId { get; set; }
    public int FamilyHeadId { get; set; }
    public FamilyHead FamilyHead { get; set; }

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }

    public int Quantity { get; set; }
    public VehicleType VehicleType { get; set; }

    public int TravelRouteId {get;set;}
    public TravelRoute TravelRoute {get;set;}
    public int RouteId {get;set;}
    public LogisticModels.Route Route {get;set;}
}
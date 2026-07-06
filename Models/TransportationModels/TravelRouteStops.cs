using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;

namespace TMG.Models.LogisticModels
{
    public class TravelRouteStops
    {
        public int TravelRouteStopsId { get; set; }

        public int TravelRouteId { get; set; }
        public TravelRoute TravelRoute { get; set; }

        public int RouteId { get; set; }
        public Route Route { get; set; }

        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public FamilyTranasport TransportType { get; set; }
        //public float? Price { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime CheckOut { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

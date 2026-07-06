namespace TMG.Models.LogisticModels
{
    public class RouteVehiclePrice
    {

        public int RouteId { get; set; }
        public Route Route { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public float Price { get; set; }
    }
}

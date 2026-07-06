using TMG.Models.AirlineModels;
using TMG.Models.PackageModels;

namespace TMG.Models.RailwayModels
{
    public class RailwayTravelPackages
    {
        public int RailwayTravelPackagesId { get; set; }
        public int RailwayTravelId { get; set; }
        public RailwayTravel RailwayTravel { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

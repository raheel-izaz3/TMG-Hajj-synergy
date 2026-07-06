using TMG.Models.PackageModels;

namespace TMG.Models.AirlineModels
{
    public class FlightPackages
    {
        public int FlightPackagesId { get; set; }
        public int FlightId { get; set; }
        public Flight Flight { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

using TMG.Models.PackageModels;

namespace TMG.Models.AirlineModels
{
    public class PNRPackage
    {
        public int PNRPackageId { get; set; }

        public int PNRId { get; set; }
        public PNR PNR { get; set; }

        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

using TMG.Models.PackageModels;

namespace TMG.Models.AirlineModels
{
    public class PNRTrainPackage
    {
        public int PNRTrainPackageId { get; set; }

        public int PNRTrainId { get; set; }
        public PNRTrain PNRTrain { get; set; }

        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

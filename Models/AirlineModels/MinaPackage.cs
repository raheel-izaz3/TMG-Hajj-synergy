using TMG.Models.PackageModels;

namespace TMG.Models.AirlineModels
{
    public class MinaPackage
    {
        public int MinaPackageId { get; set; }

        public int MinaId { get; set; }
        public Mina Mina { get; set; }

        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

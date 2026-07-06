using TMG.Models.PackageModels;

namespace TMG.Models
{
    public class SuitcasePackages
    {
        public int SuitcasePackagesId { get; set; }
        public int SuitcaseId { get; set; }
        public Suitcase Suitcase { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

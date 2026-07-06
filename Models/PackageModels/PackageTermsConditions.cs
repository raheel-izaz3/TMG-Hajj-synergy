using TMG.Models.MiscModels;

namespace TMG.Models.PackageModels
{
    public class PackageTermsConditions
    {
        public int PackageTermsConditionsId { get; set; }
        public int PackageId { get; set; }
        public Package Package  { get; set; }
        public string Term { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

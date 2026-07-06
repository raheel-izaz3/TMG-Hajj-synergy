using TMG.Models.MiscModels;
using TMG.Models.PackageModels;

namespace TMGHajj.Models.PackageModels
{
    public class PackageCategoryFacilities
    {
        public int PackageCategoryFacilitiesId { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
        public int? IndividualFacilityDescriptionId { get; set; }
        public IndividualFacilityDescription IndividualFacilityDescription { get; set; }
    }
}

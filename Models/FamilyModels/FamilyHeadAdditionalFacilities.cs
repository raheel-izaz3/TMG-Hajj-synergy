using TMG.Models.PackageModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadAdditionalFacilities
    {
        public int FamilyHeadAdditionalFacilitiesId { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }

        public int AdditionalFacilitiesId { get; set; }
        public AdditionalFacilities AdditionalFacilities { get; set; }
    }
}

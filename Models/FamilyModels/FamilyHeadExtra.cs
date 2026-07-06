using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadExtra
    {
        public int FamilyHeadExtraId { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }

        //public string Code { get; set; }
        public string Description { get; set; }
        public int? ExtraFacilityDescriptionId { get; set; }
        public ExtraFacilityDescription ExtraFacilityDescription { get; set; }
        public float? PricePkr { get; set; }
        public float? PriceSar { get; set; }
        public float? PriceUsd { get; set; }
        public float? Price { get; set; }
        public float? Discount { get; set; }
    }
}

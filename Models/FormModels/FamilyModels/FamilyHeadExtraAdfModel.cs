namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadExtraAdfModel
{
    public List<int?> ExtraFacilityIds { get; set; }
    public List<float?> FacilityPricePkr { get; set; }
    public List<float?> FacilityPriceSar { get; set; }
    public List<float?> FacilityPriceUsd { get; set; }
    public List<float?> Price { get; set; }
    public List<float?> Discount { get; set; }
    public List<bool?> ExtraFacilitiesSelected { get; set; }
}
namespace TMG.Models.FormModels.FamilyModels;

public sealed record FamilyHeadDiscountModel
{
    public float? GeneralDiscount { get; set; } = 0;
    public float PkrDiscountAmount { get; set; } = 0;
    public float SarDiscountAmount { get; set; } = 0;
    public float UsdDiscountAmount { get; set; } = 0;
}
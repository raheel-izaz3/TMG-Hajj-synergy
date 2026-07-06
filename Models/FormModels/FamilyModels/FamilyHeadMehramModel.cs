namespace TMG.Models.FormModels.FamilyModels;

public sealed record FamilyHeadMehramModel
{
    public string MehramCNIC { get; set; }
    public int? RelationWithMehramId { get; set; }
}
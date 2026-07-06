namespace TMG.Models.FormModels.FamilyModels;

public sealed record FamilyHeadRemarksModel
{
    public List<string> Remarks { get; set; }
    public string VisaRemarks { get; set; }
}


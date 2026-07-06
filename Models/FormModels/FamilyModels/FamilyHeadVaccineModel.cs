namespace TMG.Models.FormModels.FamilyModels;

public sealed record FamilyHeadVaccineModel
{
    public int? VaccineId1 { get; set; }
    public DateTime? DoseDate1 { get; set; }

    public int? VaccineId2 { get; set; }
    public DateTime? DoseDate2 { get; set; }

    public int? VaccineId3 { get; set; }
    public DateTime? DoseDate3 { get; set; }

    public int? VaccineId4 { get; set; }
    public DateTime? DoseDate4 { get; set; }

    public int? VaccineId5 { get; set; }
    public DateTime? DoseDate5 { get; set; }
}
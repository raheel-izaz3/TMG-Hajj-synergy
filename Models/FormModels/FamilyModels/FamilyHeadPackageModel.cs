using TMG.Models.FamilyModels;

namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadPackageModel
{
    public int PackageId { get; set; }
    public Currencies Currencies { get; set; }
    public float BasePackageAmount { get; set; }
    public string POD { get; set; }
    public FlightTicketType FlightTicketType { get; set; }
    public string FlightClass { get; set; }
    public float? FlightTicketDifferencesPkr { get; set; }
    public float? FlightTicketDifferencesSar { get; set; }
    public float? FlightTicketDifferencesUsd { get; set; }
    public string TrainNumberForPackage { get; set; }
    public string SeatNo { get; set; }
    public string CoachNo { get; set; }

}
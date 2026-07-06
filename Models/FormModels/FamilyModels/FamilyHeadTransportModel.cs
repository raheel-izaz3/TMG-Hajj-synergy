using TMG.Models.FamilyModels;

namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadTransportModel
{
    public bool RouteAsPerPackage { get; set; }
    public List<int> RouteIds { get; set; } = new List<int>();

    public List<DateTime?> CheckIns { get; set; } = new List<DateTime?>();

    public List<DateTime?> CheckOuts { get; set; } = new List<DateTime?>();

    public List<string> TimeIns { get; set; } = new List<string>();

    public List<string> TimeOuts { get; set; } = new List<string>();

    public List<int?> VehicleIds { get; set; } = new List<int?>();

    public List<FamilyTranasport> TransportTypes { get; set; }

    public List<float?> Prices { get; set; } = new List<float?>();

}
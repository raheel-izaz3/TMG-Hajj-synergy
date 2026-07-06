namespace TMG.Models.ReportModels;

public record RiyazUlJannahCheckInModel
{
    public DateTime Arrival { get; set; }
    public List<RiyazUlJannahCheckInGroupModel> MunazzimGroups { get; set; } = new();
}

public record RiyazUlJannahCheckInGroupModel
{
    public string MunazzimName { get; set; }
    public List<RiyazUlJannahCheckInGroupRowModel> Rows { get; set; } = new();
    public int TotalMaleCount { get; set; }
    public int TotalFemaleCount { get; set; }
    public int TotalInfantCount { get; set; }
}

public record RiyazUlJannahCheckInGroupRowModel
{
    public string MunazzimName { get; set; }
    public string HGOName { get; set; }
    public string PackageName { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int InfantCount { get; set; }
    public string ArrivalTime { get; set; }
    public string MedinaCheckOut { get; set; }
    public string Days { get; set; }
}

namespace TMG.Models.DemandReportModels
{
    public sealed record RoomDemandReportModel
    {
        public string RoomWiseHeaders { get; set; }
        public List<RoomDemandReportRowModel> Rows { get; set; } = new();

    }

    public sealed record RoomDemandReportRowModel
    {
        public int SNo { get; set; }
        public string Company { get; set; }
        public string Package { get; set; }
        public string Place { get; set; }
        public string Hotel { get; set; }
        public string CheckIn { get; set; }
        public string PackageCheckOut { get; set; }
        public List<float> RoomCounts { get; set; } = new();
    }
}



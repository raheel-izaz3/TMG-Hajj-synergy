namespace TMG.Models.AirlineModels
{
    public record PNRReportEntryModel
    {
        public string PNR { get; set; }
        public List<PNRMembersReportModel> PNRMembers { get; set; } = new List<PNRMembersReportModel>();
    }
}
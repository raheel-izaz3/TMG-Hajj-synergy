namespace TMG.Models.AirlineModels
{
    public record PNRReportModel
    {
        public List<PNRReportSector> Sectors { get; set; } = new List<PNRReportSector>();
        public List<PNRReportEntryModel> Entries { get; set; } = new List<PNRReportEntryModel>();
    }
}
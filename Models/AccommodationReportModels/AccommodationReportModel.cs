namespace TMG.Models.AccommodationReportModels;
public sealed record AccommodationReportModel
{
    public int SNo { get; set; }
    public string CompanyCode { get; set; }
    public string PackageName { get; set; }
    public string GroupCode { get; set; }
    public string HbNumber { get; set; }
    public string HajiName { get; set; }

    public AccommodationReportHotelModel MakkahHotel { get; set; }
    public AccommodationReportHotelModel MedinaHotel { get; set; }
    public AccommodationReportHotelModel AziziaHotel { get; set; }
    public AccommodationReportHotelModel MinaCamp { get; set; }
}

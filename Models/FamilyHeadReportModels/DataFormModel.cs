namespace TMG.Models.FamilyHeadReportModels
{
    public record DataFormModel
    {
        public string BaseCurrency { get; set; }
        public string MunazzimName { get; set; }
        public float BasePackagePrice { get; set; } = 0f;
        public float FlightPrice { get; set; } = 0f;
        public float TrainPrice { get; set; } = 0f;
        public float RoomPrice { get; set; } = 0f;
        public float AddonsPrice { get; set; } = 0f;
        public float AddonsDiscount { get; set; } = 0f;
        public float TotalDiscount { get; set; } = 0f;
        public float TotalPackagePrice { get; set; } = 0f;


        public List<DataFormModelEntry> Entries { get; set; } = new List<DataFormModelEntry>();
    }
}

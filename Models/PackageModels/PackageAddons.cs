namespace TMG.Models.PackageModels
{
    public class PackageAddons
    {
        public int PackageAddonsId { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }

        public string Code { get; set; }
        public string Description { get; set; }
        public float PkrPrice { get; set; }
        public float SarPrice { get; set; }
        public float UsdPrice { get; set; }
    }
}

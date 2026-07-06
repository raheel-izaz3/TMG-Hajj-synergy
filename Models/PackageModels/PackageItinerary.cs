namespace TMG.Models.PackageModels
{
    public class PackageItinerary
    {
        public int PackageItineraryId { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
        public string Description { get; set; }
       
    }
}

//using System.IO.Packaging;

using TMG.Models.PackageModels;

namespace TMG.Models.POModels
{
    public class ItineraryBlockGroup
    {
        public int ItineraryBlockGroupId { get; set; }
        public int ItineraryBlockId { get; set; }
        public ItineraryBlock ItineraryBlock { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

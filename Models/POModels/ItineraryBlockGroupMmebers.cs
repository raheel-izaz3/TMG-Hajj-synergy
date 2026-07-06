using TMG.Models.FamilyModels;

namespace TMG.Models.POModels
{
    public class ItineraryBlockGroupMmebers
    {
        public int ItineraryBlockId { get; set; }
        public ItineraryBlock ItineraryBlock { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

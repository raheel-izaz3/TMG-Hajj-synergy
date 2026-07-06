using TMG.Models.FamilyModels;

namespace TMG.Models.AirlineModels
{
    public class PNRMembers
    {
        public int PNRMembersId { get; set; }
        public int PNRId { get; set; }
        public PNR PNR { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

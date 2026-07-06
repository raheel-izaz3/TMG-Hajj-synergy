using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;

namespace TMG.Models.RailwayModels
{
    public class RailwayPNRMember
    {
        public int RailwayPNRMemberId { get; set; }
        public int RailwayPNRId { get; set; }
        public RailwayPNR RailwayPNR { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

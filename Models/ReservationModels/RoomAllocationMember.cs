using TMG.Models.FamilyModels;

namespace TMG.Models.ReservationModels
{
    public class RoomAllocationMember
    {
        public int RoomAllocationMemberId { get; set; }

        public int RoomAllocationId { get; set; }
        public RoomAllocation RoomAllocation { get; set; }

        public int? FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

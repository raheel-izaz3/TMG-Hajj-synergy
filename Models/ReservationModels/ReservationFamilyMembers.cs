using TMG.Models.FamilyModels;

namespace TMG.Models.ReservationModels
{
    public class ReservationFamilyMembers
    {
        public int ReservationFamilyMembersId { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }

        public int? ReservationRoomsId { get; set; }
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public int? FloorId { get; set; }
        public Floor Floor { get; set; }
        public int? FloorRoomId { get; set; }
        public FloorRoom FloorRoom { get; set; }


    }
}

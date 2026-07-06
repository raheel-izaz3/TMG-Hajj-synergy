using TMG.Models.FamilyModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.POModels
{
    public class ItineraryBlockReservationForAzizia : BaseEntity
    {
        public int ItineraryBlockReservationForAziziaId { get; set; }
        public string StayWith { get; set; }
        public string ConnectedFloorNumber { get; set; }
        public string ConnectedRoomNumber { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int? FloorRoomId { get; set; }
        public FloorRoom FloorRoom { get; set; }
        public int ItineraryBlockId { get; set; }
        public ItineraryBlock ItineraryBlock { get; set; }

    }
}

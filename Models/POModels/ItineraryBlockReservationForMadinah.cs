using TMG.Models.FamilyModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.POModels
{
    public class ItineraryBlockReservationForMadinah
    {
        public int ItineraryBlockReservationForMadinahId { get; set; }
        public string StayWith { get; set; }
        public string ConnectedFloorNumber { get; set; }
        public string ConnectedRoomNumber { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int? RoomDetailsId { get; set; }
        public RoomDetails RoomDetails { get; set; }
        public int ItineraryBlockId { get; set; }
        public ItineraryBlock ItineraryBlock { get; set; }
    }
}

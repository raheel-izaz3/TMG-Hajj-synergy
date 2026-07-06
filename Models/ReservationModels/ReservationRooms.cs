using TMG.Models.AccomodationModels;

namespace TMG.Models.ReservationModels
{
    public class ReservationRooms
    {
        public int ReservationRoomsId { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }

        public int RoomId {  get; set; }
        public Room Room { get; set; }

        public int? FloorRoomId { get; set; }
        public FloorRoom FloorRoom { get; set; }

        public int? ConnectedWithRoomId { get; set; }
        public ReservationRooms ConnectedWithRoom { get; set; }

        //public int Quantity { get; set; }
    }
}

namespace TMG.Models.ReservationModels
{
    public class RoomAllocation: BaseEntity
    {
        public int RoomAllocationId { get; set; }

        public int FloorRoomId { get; set; }
        public FloorRoom FloorRoom { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public bool Connected { get; set; }


        public int? ConnectedWithFloorRoomId { get; set; }
        public FloorRoom ConnectedWithFloorRoom { get; set; }
    }
}

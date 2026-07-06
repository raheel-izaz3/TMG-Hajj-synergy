using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.POModels
{
    

    public class POHotelRooms : BaseEntity
    {
        public int POHotelRoomsId { get; set; }
        public int? Quantity { get; set; }
        public float? Amount { get; set; }
        public DateTime? Arrival { get; set; }
        public DateTime? Departure { get; set; }
        public float? Total { get; set; }
        public BathroomType? BathroomType { get; set; }
        public int? RoomId { get; set; }
        public Room Room { get; set; }
        public int? RoomViewId { get; set; }
        public RoomView RoomViews { get; set; }
        public int? POHotelsId { get; set; }
        public POHotels POHotels { get; set; }
        public ICollection<RoomDetails> RoomDetails {  get; set; }
        public ICollection<FloorRoom> FloorRooms {  get; set; }
    }
}

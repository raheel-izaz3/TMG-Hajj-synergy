using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using TMG.Models.POModels;

namespace TMG.Models.ReservationModels
{
    public class RoomDetails 
    {
        public int RoomDetailsId { get; set; }
        public string RoomNumber { get; set; }
        public BathroomType? BathroomType { get; set; }
        public DateTime? Arrival { get; set; }
        public DateTime? Departure { get; set; }
        public int? RoomBedTypeId { get; set; }
        public RoomBedType RoomBedType { get; set; }
        public int? RoomId { get; set; }
        public Room Room { get; set; }
        public int? PAX { get; set; }
        public int? RoomViewId { get; set; }
        public RoomView RoomViews { get; set; }
        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public int PlaceId { get; set; }
        public Place Place { get; set; }
        public int POHotelsId { get; set; }
        public POHotels POHotels { get; set; }
        public int? POHotelRoomsId { get; set; }
        public POHotelRooms POHotelRooms { get; set; }
    }
}

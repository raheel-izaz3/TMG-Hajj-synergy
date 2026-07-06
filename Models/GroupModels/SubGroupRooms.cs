using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using TMG.Models.POModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.GroupModels;

public class SubGroupRooms
{
    public int SubGroupRoomsId { get; set; }
    public string RoomNumber { get; set; }
    public int SubGroupId { get; set; }
    public SubGroup SubGroup { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; }
    public BathroomType? BathroomType { get; set; }
    public int? RoomViewId { get; set; }
    public RoomView RoomViews { get; set; }
    public int Quantity { get; set; }

    public int? POHotelsId { get; set; }
    public POHotels POHotels { get; set; }
    public int? POHotelRoomsId { get; set; }
    public POHotelRooms POHotelRooms { get; set; }
    public int? PlaceId { get; set; }
    public Place Place { get; set; }
    public int? RoomDetailsId { get; set; }
    public RoomDetails RoomDetails { get; set; }
}
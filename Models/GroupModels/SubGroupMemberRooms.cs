using TMG.Models.AccomodationModels;
using TMG.Models.FamilyModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.GroupModels;

public class SubGroupMemberRooms
{
    public int SubGroupMemberRoomsId { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; }

    public int? SubGroupId { get; set; }
    public SubGroup? SubGroup { get; set; }

    public int FamilyHeadId { get; set; }
    public FamilyHead FamilyHead { get; set; }

    public int? RoomId { get; set; }
    public Room Room { get; set; }

    public string FloorNumber { get; set; }
    public string RoomNumber { get; set; }
/*    public int? HotelId { get; set; }
    public Hotel Hotels { get; set; }*/

    public string ConnectedFloorNumber { get; set; }
    public string ConnectedRoomNumber { get; set; }
    public int PlaceId { get; set; }
    

}
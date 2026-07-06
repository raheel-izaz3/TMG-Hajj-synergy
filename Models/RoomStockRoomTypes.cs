using TMG.Models.AccomodationModels;

namespace TMG.Models;

public class RoomStockRoomTypes {
    public int RoomStockRoomTypesId { get; set; }
    public int RoomStockId { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; }
    public int Quantity { get; set; }
}

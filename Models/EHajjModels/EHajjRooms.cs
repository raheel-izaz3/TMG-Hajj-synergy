using TMG.Models.AccomodationModels;

namespace TMG.Models.EHajjModels
{
    public class EHajjRooms
    {
        public int EHajjRoomsId { get; set; }
        public int EhajjContractId { get; set; }
        public EHajjContract EhajjContract { get; set; }
        public int RoomId { get; set; }
        public int Rooms { get; set; }
        public int Paxs { get; set; }
        public Room Room { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.ReservationModels
{
    public class RoomAllotment: BaseEntity
    {
        public int RoomAllotmentId { get; set; }

        public int FloorRoomId { get; set; }
        public FloorRoom FloorRoom { get; set; }

        [Required, Display(Name = "Valid From"), DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; }

        [Required, Display(Name = "Valid Till"), DataType(DataType.Date)]
        public DateTime ValidTill { get; set; }
    }
}

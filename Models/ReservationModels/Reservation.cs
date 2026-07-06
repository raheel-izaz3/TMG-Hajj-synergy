using TMG.Models.GroupModels;
using TMG.Models.PackageModels;

namespace TMG.Models.ReservationModels
{
    public class Reservation: BaseEntity
    {
        public int ReservationId { get; set; }

        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public int? SubGroupId { get; set; }
        public SubGroup SubGroup { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }
       
    }
}

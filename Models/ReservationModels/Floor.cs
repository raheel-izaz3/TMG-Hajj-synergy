using System.ComponentModel.DataAnnotations;
using TMG.Models.POModels;

namespace TMG.Models.ReservationModels
{
    public class Floor: BaseEntity
    {
        public int FloorId { get; set; }

        [Required, Display(Name = "Floor Number", Prompt = "Enter Floor Number...")]
        public int FloorNumber { get; set; }

        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public int? POHotelsId { get; set; }
        public POHotels POHotels { get; set; }

    }
}

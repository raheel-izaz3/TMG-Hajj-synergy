using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.ReservationModels
{
    public class HotelEditRequest
    {
        public string Location { get; set; }

        public int HotelId { get; set; }

        [Required, Display(Name = "Hotel Number", Prompt = "Enter Hotel Number...")]
        public int HotelNumber { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string HotelName { get; set; }

        [Required, Display(Name = "Code", Prompt = "Enter Code...")]
        public string HotelCode { get; set; }

        [Required, Display(Name = "Address", Prompt = "Enter Address...")]
        public string HotelAddress { get; set; }

        [Required, Display(Name = "Contact", Prompt = "Enter Contact...")]
        public string HotelContact { get; set; }

        [Required, Display(Name = "Email", Prompt = "Enter Email...")]
        public string HotelEmail { get; set; }

        [Required, Display(Name = "Place")]
        public int PlaceId { get; set; }

        [Required, Display(Name = "Accomodation Type")]
        public int AccomodationId { get; set; }

        [Required, Display(Name = "Accomodation Category")]
        public int AccomodationCategoryId { get; set; }
    }
}

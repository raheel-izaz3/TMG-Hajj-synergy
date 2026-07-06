using TMG.Models.AccomodationModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;

namespace TMG.Models.ReservationModels
{
    public class Hotel : BaseEntity
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

        public string HotelLogo { get; set; }

        [Required, Display(Name = "Place")]
        public int PlaceId { get; set; }
        public Place Place { get; set; }

        [Required, Display(Name = "Accomodation Type")]
        public int AccomodationId { get; set; }
        public Accomodation Accomodation { get; set; }

        [Required, Display(Name = "Accomodation Category")]
        public int AccomodationCategoryId { get; set; }
        public AccomodationCategory AccomodationCategory { get; set; }

        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }

        public ICollection<RoomDetails> RoomDetails { get; set; }
    }

}

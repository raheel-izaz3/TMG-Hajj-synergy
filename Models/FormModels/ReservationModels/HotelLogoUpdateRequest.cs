using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.ReservationModels
{
    public class HotelLogoUpdateRequest
    {
        public int HotelId { get; set; }

        [Required, Display(Name = "New Hotel Logo")]
        public IFormFile NewLogo { get; set; }
    }
}

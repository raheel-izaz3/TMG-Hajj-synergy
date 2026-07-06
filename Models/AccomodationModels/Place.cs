using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccomodationModels
{
    public class Place: BaseEntity
    {
        public int PlaceId { get; set; }

        [Required, Display(Name = "Place Name", Prompt = "Enter Place Name...")]
        public string PlaceName { get; set; }

        public const int MAKKAH_ID = 1;
        public const int MEDINA_ID = 2;
        public const int MINA_ID = 3;
        public const int AZIZIA_ID = 4;
    }
}

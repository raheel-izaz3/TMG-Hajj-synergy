using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.AirlineModels
{
    public class Airline: BaseEntity
    {
        public int AirlineId { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "Code", Prompt = "Enter Code...")]
        public string Code { get; set; }

        [Required, Display(Name = "IATA Code", Prompt = "Enter IATA Code...")]
        public string IATACode { get; set; }
        
        [Required, Display(Name = "ICAO Code", Prompt = "Enter ICAO Code...")]
        public string ICAOCode { get; set; }

        [Required, Display(Name = "Country", Prompt = "Enter Country...")]
        public string Country { get; set; }

        [Required, Display(Name = "Call Sign", Prompt = "Enter Call Sign...")]
        public string CallSign { get; set; }

        [Display(Name = "Airline Logo", Prompt = "Enter Airline Logo...")]
        public string AirlineLogo { get; set; }

        public string FFNumber { get; set; }

        [Display(Name = "Airline Logo Pic")]
        [NotMapped] public IFormFile AirlineLogoPic { get; set; }
    }
}

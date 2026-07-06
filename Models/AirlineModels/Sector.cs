using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AirlineModels
{
    public enum FlightType
    {
        Inbound, Outbound
    }

    public class Sector
    {
        public int SectorId { get; set; }

        public int FlightId { get; set; }
        public Flight Flight { get; set; }

        [Display(Name = "Destination", Prompt = "Enter Destination...")]
        public string Destination { get; set; }

        [Display(Name = "Departure", Prompt = "Enter Departure...")]
        public DateTime? DepartureDateTime { get; set; }

        [Display(Name = "Arrival", Prompt = "Enter Arrival...")]
        public DateTime? ArrivalDateTime { get; set; }

        public FlightType FlightType { get; set; }
        public string FlightNo { get; set; }
    }
}

using TMG.Models.AirlineModels;
using TMG.Models.MiscModels;

namespace TMG.Models
{
    public class POTicketFlightData : BaseEntity
    {
        public int POTicketFlightDataId { get; set; } 

        public string FlightNumber { get; set; }  

        public int DepartureAirportId { get; set; }

        public Airport Departure { get; set; }

        public int ArrivalAirportId { get; set; }

        public Airport Arrival { get; set; }

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }  

        public int? HajjSeasonsId { get; set; }

        public HajjSeasons HajjSeasons { get; set; }
        public int? AirlineId { get; set; }
        public Airline Airline { get; set; }
    }
}
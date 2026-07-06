using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;

namespace TMG.Models.AirlineModels
{
    public enum PNRType
    {
        Economy, Business, First, Self
    }

    public enum Type
    {
        Individual, Group, Self
    }
    public class PNR: BaseEntity
    {
        public int PNRId { get; set; }

        public string Name { get; set; }

        [Required, Display(Name="Flight")]
        public int? FlightId { get; set; }
        public Flight Flight { get; set; }

        public int? POTicketFlightDataId { get; set; }
        public POTicketFlightData POTicketFlightData { get; set; }

        public string PNRNo { get; set; }

        public int? AirlineId { get; set; }
        public Airline Airline { get; set; }

        public int Capacity { get; set; }

        public PNRType Type { get; set; }

        public Type PNRType { get; set; }
        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

    }
}

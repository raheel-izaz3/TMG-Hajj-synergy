using TMG.Models.AirlineModels;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.RailwayModels
{
    public class RailwayPNR: BaseEntity
    {
        public int RailwayPNRId { get; set; }

        public string Name { get; set; }

        public int RailwayTravelId { get; set; }
        public RailwayTravel RailwayTravel { get; set; }

        public int Capacity { get; set; }

        public PNRType Type { get; set; }

    }
}

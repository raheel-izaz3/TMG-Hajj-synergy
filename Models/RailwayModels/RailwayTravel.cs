using TMG.Models.AirlineModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.RailwayModels
{
    public class RailwayTravel: BaseEntity
    {
        public int RailwayTravelId { get; set; }

        [Required, Display(Name = "Train")]
        public int RailwayId { get; set; }
        public Railway Railway { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "Outbound Train #", Prompt = "Enter Outbound Train #...")]
        public string OutboundRailwayNumber { get; set; }

        [Required, Display(Name = "Outbound Departure")]
        public DateTime OutboundDepartureDateTime { get; set; }

        [Required, Display(Name = "Outbound Arrival")]
        public DateTime OutboundArrivalDateTime { get; set; }


        [Display(Name = "Capacity", Prompt = "Capacity")]
        public int Capacity { get; set; }


        [Required, Display(Name = "Economy Seats", Prompt = "Enter Economy Seats")]
        public int EconomySeats { get; set; }

        [Required, Display(Name = "Business Seats", Prompt = "Enter Business Seats")]
        public int BusinessSeats { get; set; }


        [Display(Name = "SAR ROE", Prompt = "SAR ROE")]
        public float SARROE { get; set; } = 0;

        [Display(Name = "USD ROE", Prompt = "USD ROE")]
        public float USDROE { get; set; } = 0;

        [Display(Name = "Adult PKR", Prompt = "Enter Adult PKR...")]
        public float EconomyAdultPkr { get; set; } = 0;

        [Display(Name = "Adult SAR", Prompt = "Enter Adult SAR...")]
        public float EconomyAdultSar { get; set; } = 0;

        [Display(Name = "Adult USD", Prompt = "Enter Adult USD...")]
        public float EconomyAdultUsd { get; set; } = 0;

        [Display(Name = "Child PKR", Prompt = "Enter Child PKR...")]
        public float EconomyChildPkr { get; set; } = 0;

        [Display(Name = "Child SAR", Prompt = "Enter Child SAR...")]
        public float EconomyChildSar { get; set; } = 0;

        [Display(Name = "Child USD", Prompt = "Enter Child USD...")]
        public float EconomyChildUsd { get; set; } = 0;

        [Display(Name = "Infant PKR", Prompt = "Enter Infant PKR...")]
        public float EconomyInfantPkr { get; set; } = 0;

        [Display(Name = "Infant SAR", Prompt = "Enter Infant SAR...")]
        public float EconomyInfantSar { get; set; } = 0;

        [Display(Name = "Infant USD", Prompt = "Enter Infant USD...")]
        public float EconomyInfantUsd { get; set; } = 0;


        [Display(Name = "Adult PKR", Prompt = "Enter Adult PKR...")]
        public float BusinessAdultPkr { get; set; } = 0;

        [Display(Name = "Adult SAR", Prompt = "Enter Adult SAR...")]
        public float BusinessAdultSar { get; set; } = 0;

        [Display(Name = "Adult USD", Prompt = "Enter Adult USD...")]
        public float BusinessAdultUsd { get; set; } = 0;

        [Display(Name = "Child PKR", Prompt = "Enter Child PKR...")]
        public float BusinessChildPkr { get; set; } = 0;

        [Display(Name = "Child SAR", Prompt = "Enter Child SAR...")]
        public float BusinessChildSar { get; set; } = 0;

        [Display(Name = "Child USD", Prompt = "Enter Child USD...")]
        public float BusinessChildUsd { get; set; } = 0;

        [Display(Name = "Infant PKR", Prompt = "Enter Infant PKR...")]
        public float BusinessInfantPkr { get; set; } = 0;

        [Display(Name = "Infant SAR", Prompt = "Enter Infant SAR...")]
        public float BusinessInfantSar { get; set; } = 0;

        [Display(Name = "Infant USD", Prompt = "Enter Infant USD...")]
        public float BusinessInfantUsd { get; set; } = 0;

        [NotMapped] public List<int> PackageIds { get; set; }

        [Display(Name = "Destination", Prompt = "Enter Destination...")]
        public string Destination { get; set; }

        [Display(Name = "Departure", Prompt = "Enter Departure...")]
        public DateTime? SectorDeparture { get; set; }

        [Display(Name = "Arrival", Prompt = "Enter Arrival...")]
        public DateTime? SectorArrival { get; set; }

        [NotMapped] public List<PNRType> PNRTypes { get; set; }
        [NotMapped] public List<string> PNRNames { get; set; }
        [NotMapped] public List<int> Capacities { get; set; }

        [NotMapped] public List<bool> PackagesSelected { get; set; }

    }
}

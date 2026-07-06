using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;
using TMG.Models.RailwayModels;

namespace TMG.Models.AirlineModels
{
    public class PNRTrain: BaseEntity
    {
        public int PNRTrainId { get; set; }

        public string Name { get; set; }

        [Required, Display(Name="Flight")]
        public int? FlightId { get; set; }
        public Flight Flight { get; set; }

        public string PNRNo { get; set; }

        public int? RailwayId { get; set; }
        public Railway Railway { get; set; }

        public int? CreditCardId { get; set; }
        public CreditCard CreditCard { get; set; }

        public int Capacity { get; set; }

        public PNRType Type { get; set; }

        public Type PNRType { get; set; }

        public string TrainNumber { get; set; }

        public string File { get; set; }

        public DateTime? Departure { get; set; }
        public DateTime? Arrival { get; set; }

        public float? Price { get; set; }


        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

    }
}

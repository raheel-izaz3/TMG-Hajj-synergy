using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using TMG.Models.RailwayModels;

namespace TMG.Models.AirlineModels
{
    public class Mina: BaseEntity
    {
        public int MinaId { get; set; }

        public string Name { get; set; }

        [Required, Display(Name="Flight")]
        public int? FlightId { get; set; }
        public Flight Flight { get; set; }

        public int? CompanyVisasId { get; set; }
        public CompanyVisas CompanyVisas { get; set; }

        public string Code { get; set; }

        public int Capacity { get; set; }

        public string CampNumber { get; set; }

        public Gender Gender { get; set; }

        public string File { get; set; }

        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        public float? Price { get; set; }

        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

    }
}

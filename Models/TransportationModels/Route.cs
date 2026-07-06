using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;

namespace TMG.Models.LogisticModels
{
    public class Route: BaseEntity
    {
        public int RouteId { get; set; }
        public string StartPlace { get; set; }
        public string Destination { get; set; }

        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }
        [NotMapped] public string FullRoute => $"{StartPlace} to {Destination}";
    }
}

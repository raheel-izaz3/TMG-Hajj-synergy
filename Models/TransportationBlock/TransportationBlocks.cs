using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.AccomodationModels;
using TMG.Models.LogisticModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;


namespace TMG.Models.TransportationBlock
{
    public class TransportationBlocks : BaseEntity
    {
        public int TransportationBlocksId { get; set; }
        public int PAX { get; set; }
        public int PackageId { get; set; } = 2; // TODO remove this
        public Package Package { get; set; }
        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }
        public int Selected { get; set; } = 0;
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
        public int RouteId { get; set; }
        public LogisticModels.Route Route { get; set; }
        public DateTime? Date { get; set; }
        public int? PlaceId { get; set; }
        public Place Place { get; set; }

        public ICollection<TransportationBlockMembers> TransportationBlockMembers { get; set; }
    }
}

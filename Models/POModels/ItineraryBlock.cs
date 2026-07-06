using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.FamilyModels;

//using System.IO.Packaging;
using TMG.Models.GroupModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;

namespace TMG.Models.POModels
{

    public class ItineraryBlock : BaseEntity
    {
        public int ItineraryBlockId { get; set; }
        public int PAX { get; set; }
        public int POHotelsId { get; set; }
        public POHotels POHotels { get; set; }
        public Gender? Gender { get; set; }
        public int PackageId { get; set; } = 2; // TODO remove this
        public Package Package { get; set; }
        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }
        public int Selected { get; set; } = 0;
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
        public ICollection<ItineraryBlockGroup> ItineraryBlockGroups { get; set; }

    }
}

using TMG.Models.AccomodationModels;

namespace TMG.Models.MiscModels
{
    public class IndividualFacilityDescription : BaseEntity
    {
        public int IndividualFacilityDescriptionId { get; set; }
        public string Description { get; set; }
        
        public int? RoomId { get; set; }
        public Room Room { get; set; }
        
        public int? PlaceId { get; set; }
        public Place Place { get; set; }

        public int? AccomodationId { get; set; }
        public Accomodation Accomodation { get; set; }

        public float Price { get; set; }
        public bool? IsSharing { get; set; }

        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }
    }
}

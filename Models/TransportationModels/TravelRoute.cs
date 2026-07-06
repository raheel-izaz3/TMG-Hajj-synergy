using TMG.Models.MiscModels;

namespace TMG.Models.LogisticModels
{
    public class TravelRoute: BaseEntity
    {
        public int TravelRouteId { get; set; }
        public string RouteName { get; set; }
        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }
    }
}

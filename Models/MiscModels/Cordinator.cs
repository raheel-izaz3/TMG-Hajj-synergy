using TMG.Models.AccomodationModels;

namespace TMG.Models.MiscModels
{
    public class Cordinator : BaseEntity
    {
        public int CordinatorId { get; set; }
        public int PlaceId { get; set; }
        public Place Place { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
    }
}

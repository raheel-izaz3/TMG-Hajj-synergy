using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using System.Drawing.Printing;

namespace TMG.Models.PackageModels
{
    public class AdditionalFacilities
    {
        public int AdditionalFacilitiesId { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }

        public int? Code { get; set; }
        public int? RoomId { get; set; }
        public Room Room { get; set; }

        public int? IndividiualFacilityDescriptionId { get; set; }
        public IndividualFacilityDescription IndividiualFacilityDescription { get; set; }

        public int? PlaceId { get; set; }
        public Place Place { get; set; }

        public int? AccomodationId { get; set; }
        public Accomodation Accomodation { get; set; }


        public float? USDROE { get; set; }
        public float? SARROE { get; set; }

        public float? AdultPkr { get; set; }
        public float? AdultSar { get; set; }
        public float? AdultUsd { get; set; }
        public float? ChildPkr { get; set; }
        public float? ChildSar { get; set; }
        public float? ChildUsd { get; set; }
        public float? InfantPkr { get; set; }
        public float? InfantSar { get; set; }
        public float? InfantUsd { get; set; }

        public string Note { get; set; }


    }
}

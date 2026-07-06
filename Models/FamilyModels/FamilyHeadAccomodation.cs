using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadAccomodation
    {
        public int FamilyHeadAccomodationId { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public int IndividualFacilityDescriptionId { get; set; }
        public IndividualFacilityDescription IndividualFacilityDescription { get; set; }

        public int? FoodPackageId { get; set; }
        public FoodPackage FoodPackage { get; set; }

        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        public string Day { get; set; }
        public string Night { get; set; }

        public string FloorNumber { get; set; }
        public string RoomNumber { get; set; }
        //public bool GenderSegregated { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

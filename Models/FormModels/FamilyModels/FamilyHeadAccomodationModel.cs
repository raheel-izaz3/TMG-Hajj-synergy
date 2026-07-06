namespace TMG.Models.FormModels.FamilyModels
{
    public class FamilyHeadAccomodationModel
    {
        public bool AccomodationAsPerPackage { get; set; }
        public List<int> HotelIds { get; set; }
        public List<int> IndividualFacilityDescriptionIds { get; set; }
        public List<DateTime?> CheckIns { get; set; }
        public List<DateTime?> CheckOuts { get; set; }
        public List<string> FloorNumbers { get; set; }
        public List<string> RoomNumbers { get; set; }
        public List<string> Day { get; set; }
        public List<string> Night { get; set; }
        public List<int> MealPlan { get; set; }
        //public List<bool> GenderSegregateds { get; set; }
    }
}

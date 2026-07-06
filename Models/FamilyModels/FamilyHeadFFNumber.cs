using TMG.Models.AirlineModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadFFNumber : BaseEntity
    {
        public int FamilyHeadFFNumberId { get; set; }
        public string FFNumber { get; set; }
        public int? AirlineId { get; set; }
        public Airline Airline { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

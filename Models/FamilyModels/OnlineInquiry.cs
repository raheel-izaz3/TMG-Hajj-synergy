using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public class OnlineInquiry : BaseEntity
    {
        public int OnlineInquiryId { get; set; }
        public int Adult { get; set; }
        public int Child { get; set; }
        public int Infant { get; set; }
        public string SurName { get; set; }
        public string GivenName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public int? NoOfDaysHajjId { get; set; }
        public InquiryEmail NoOfDaysHajj { get; set; }
        public string TravelingMonth { get; set; }
        public int? NoOfDaysUmrahId { get; set; }
        public InquiryEmail NoOfDaysUmrah { get; set; }
        public int? NoOfDaysTourId { get; set; }
        public InquiryEmail NoOfDaysTour { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }

        public int? AccomodationCategoryId { get; set; }
        public AccomodationCategory AccomodationCategory { get; set; }

        public int? DestinationId { get; set; }
        public Destination Destination { get; set; }

        public int? CountryId { get; set; }
        public Country Country { get; set; }

        public int? CityId { get; set; }
        public City City { get; set; }
    }
}

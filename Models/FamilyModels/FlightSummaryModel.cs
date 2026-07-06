using TMG.Models.GroupModels;

namespace TMG.Controllers.FamilyControllers
{
    public class FlightSummaryModel
    {
        public int FamilyHeadId { get; set; }
        public string FamilyHeadGivenName { get; set; }
        public string FamilyHeadSurName { get; set; }
        public string HBNo { get; set; }
        public string Airline { get; set; }
        public string PackageName { get; set; }
        public string FlightType { get; set; }
        public string Class { get; set; }
        public string FlightNo { get; set; }
        public string TicketNo { get; set; }
        public string PNRNo { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string GroupName { get; set; }
        public string VehicleToHotel { get; set; }
        public string PassportNumber { get; set; }
        public string PassportExpiry { get; set; }
        public string DateOfBirth { get; set; }
        public float Total { get; set; }
    }
}

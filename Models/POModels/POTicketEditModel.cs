using TMG.Controllers.POControllers;
using TMG.Models.FamilyModels;

namespace TMG.Models.POModels
{
    public class POTicketEditModel
    {

        public int POTicketId { get; set; }
        public int POTicketHeadId { get; set; }
        public string PONumber { get; set; }
        public string XONumber { get; set; }
        public string PassportNumber { get; set; }
        public DateTime DateofBirth { get; set; }
        public DateTime PassportExpiry { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public int? familyHeadId { get; set; }
        public string TPRFNo { get; set; }
        public TicketType TicketType { get; set; }
        public FareType FareType { get; set; }
        public int? FamilyMembersId { get; set; }
        public PostingType PostingType { get; set; }
        public IssuedType? IssuedType { get; set; }

        public DateTime? IssueDate { get; set; }
        public int? CompanyId { get; set; }
        public int? VendorTypeId { get; set; }
        public int? VendorId { get; set; }
        public int? BookingAgentId { get; set; }
        public int? TicketingAgentId { get; set; }
        public int? RecoveryOfficerId { get; set; }
        public int? PostingAgentId { get; set; }
        public float? BasicFare { get; set; }
        public float? ntax { get; set; }
        public float? rg { get; set; }
        public float? sf { get; set; }
        public float? sp { get; set; }
        public float? ydAmount { get; set; }
        public float? gtaxAmount { get; set; }
        public float? pzAmount { get; set; }
        public float? qaAmount { get; set; }
        public float? rtaxAmount { get; set; }
        public float? etaxAmount { get; set; }
        public float? ioAmount { get; set; }
        public float? yr { get; set; }
        public float? ttax { get; set; }
        public float? Ftax { get; set; }
        public float? Zrtax { get; set; }
        public float? Aetax { get; set; }
        public float? Tptax { get; set; }
        public float? Catax { get; set; }
        public float? Sqtax { get; set; }
        public float? Rctax { get; set; }
        public float? Xttax { get; set; }
        public float? Aytax { get; set; }
        public float? Ustax { get; set; }
        public float? Xatax { get; set; }
        public float? Xftax { get; set; }
        public float? Xytax { get; set; }
        public float? Yctax { get; set; }
        public float? Ubtax { get; set; }
        public float? Gbtax { get; set; }
        public float? svc { get; set; }
        public float? srb { get; set; }
        public float? pk { get; set; }
        public float? ptt { get; set; }
        public float? pb { get; set; }
        public float? yq { get; set; }
        public float? KPR { get; set; }
        public float? KBP { get; set; }
        public float? Discount { get; set; }
        public float? CommissionPercentage { get; set; }
        public float? Commission { get; set; }
        public float? WithHoldingPercentage { get; set; }
        public float? withholding { get; set; }
        public float? SPTaxPercentage { get; set; }
        public float? SPTax { get; set; }
        public float? finalkbAmount { get; set; }
        public float? GrossClient { get; set; }
        public float? GrossAirline { get; set; }
        public float? NetClient { get; set; }
        public float? NetAirline { get; set; }
        public float? NetTotal { get; set; }
        public float? OtherTaxes { get; set; }
        public float? AgentPersentage { get; set; }
        public float? AgentAmount { get; set; }
        public float? AgentFixedAmount { get; set; }
        public float? ReIssuePenulty { get; set; }
        public float? RefundPenulty { get; set; }
        public float? SeatMappingPenulty { get; set; }
        public float? NoShowPenulty { get; set; }
        public float? ExtraBaggage { get; set; }
        public float? DifferenceOfFare { get; set; }
        public float NetTax { get; set; }
        public string PassengerCategory { get; set; }
        public string TicketImages { get; set; }
        public string FrequentFlyer { get; set; }
        public float? ClientFareAmounFromInvoice { get; set; }
        public float? NetClientFromInvoice { get; set; }
        public float? SVCNet { get; set; }
        public float? ConersionRate { get; set; }
        public float? NetTaxesForRefund { get; set; }
        public string ReIssuedTicketNumber { get; set; }
        public int? POFlightDetailIds { get; set; } // ✅ Nullable int, since it may be null

        public List<int> POFlightDetailId { get; set; } = new List<int>();
        public List<TicketCategory?> TicketCategory { get; set; } = new List<TicketCategory?>();
        public List<string> SeatNumber { get; set; } = new List<string>();
        public List<string> TicketNumber { get; set; } = new List<string>();
        public List<int?> POTicketFlightDataId { get; set; } = new List<int?>();
        public List<int?> DepartureAirportId { get; set; } = new List<int?>();
        public List<int?> ArrivalAirportId { get; set; } = new List<int?>();
        public List<DateTime?> FlightDepartureTime { get; set; } = new List<DateTime?>();
        public List<DateTime?> FlightArrivalTime { get; set; } = new List<DateTime?>();
        public List<string> PNRNo { get; set; } = new List<string>();
        public List<IFormFile> Url { get; set; }
        public List<string> ExistingUrls { get; set; }

        public List<string> PNRGDSNo { get; set; } = new List<string>();
        public List<string> FlightOrigion { get; set; } = new List<string>();
        public List<string> FlightDestination { get; set; } = new List<string>();
        public List<string> PreferedMeals { get; set; } = new List<string>();
        public List<FlightClass> FlightClass { get; set; } = new List<FlightClass>();
        public List<AirportsType> AirportsType { get; set; } = new List<AirportsType>();
        public List<int?> Airline { get; set; } = new List<int?>();
        public List<DateTime?> ArrivalDate { get; set; } = new List<DateTime?>();
        public List<DateTime?> DepartureDate { get; set; } = new List<DateTime?>();
        public List<int?> CityTax { get; set; } = new List<int?>();
        public List<float?> Amount { get; set; } = new List<float?>();
        public List<string> TransectTime { get; set; } = new List<string>();
        public List<int?> SubClass { get; set; } = new List<int?>();

    }
}

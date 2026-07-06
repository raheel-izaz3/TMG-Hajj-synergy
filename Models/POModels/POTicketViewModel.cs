using TMG.Controllers.POControllers;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Models.POModels
{
    public class POTicketViewModel
    {
        public List<POFlightDetailVM> POFlightDetails { get; set; } = new List<POFlightDetailVM>();
        public List<NewTax> Taxes { get; set; } = new();
        public List<PaymentViewModel> Payments { get; set; }
        public List<POTicketInvoice> PoInvoice  { get; set; }

    }

    public class POTicketInvoice
    {
        public int POTicketId { get; set; }
        public int POTicketHeadId { get; set; }
        public int POVisaId { get; set; }
        public string PONumber { get; set; }
        public string XONumber { get; set; }
        public string PassportNumber { get; set; }
        public string FrequentFlyer { get; set; }
        public DateTime DateofBirth { get; set; }
        public DateTime PassportExpiry { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public string TPRFNo { get; set; }
        public TicketType TicketType { get; set; }
        public FareType FareType { get; set; }
        public int familyHeadId { get; set; }
        public int FamilyMembersId { get; set; }
        public PostingType PostingType { get; set; }
        public IssuedType? IssuedType { get; set; }
        public DateTime? IssueDate { get; set; }
        public int CompanyId { get; set; }
        public int VendorTypeId { get; set; }
        public int? VendorId { get; set; }
        public Vendor Vendor { get; set; }
        public int BookingAgentId { get; set; }
        public int TicketingAgentId { get; set; }
        public int RecoveryOfficerId { get; set; }
        public int PostingAgentId { get; set; }
        public float basicFare { get; set; }
        public float CommissionPersentage { get; set; }
        public float Commission { get; set; }
        public float? Discount { get; set; }
        public float? NetTotal { get; set; }
        public float NetTax { get; set; }
        public float ntax { get; set; }
        public float rg { get; set; }
        public float sf { get; set; }
        public float sp { get; set; }
        public float ydAmount { get; set; }
        public float gtaxAmount { get; set; }
        public float pzAmount { get; set; }
        public float qaAmount { get; set; }
        public float rtaxAmount { get; set; }
        public float etaxAmount { get; set; }
        public float ioAmount { get; set; }
        public float yr { get; set; }
        public float ttax { get; set; }
        public float Ftax { get; set; }
        public float Zrtax { get; set; }
        public float Aetax { get; set; }
        public float Tptax { get; set; }
        public float Catax { get; set; }
        public float Sqtax { get; set; }
        public float Rctax { get; set; }
        public float Xttax { get; set; }
        public float Aytax { get; set; }
        public float Ustax { get; set; }
        public float Xatax { get; set; }
        public float Xftax { get; set; }
        public float Xytax { get; set; }
        public float Yctax { get; set; }
        public float Ubtax { get; set; }
        public float Gbtax { get; set; }
        public float svc { get; set; }
        public float srb { get; set; }
        public float pk { get; set; }
        public float ptt { get; set; }
        public float pb { get; set; }
        public float yq { get; set; }
        public float? OtherTaxes { get; set; }
        public float? AgentPersentage { get; set; }
        public float? AgentAmount { get; set; }
        public float? AgentFixedAmount { get; set; }
        public float WTHPersentage { get; set; }
        public float WTHAmount { get; set; }
        public float kpr { get; set; }
        public float kbp { get; set; }
        public float spTaxPercentage { get; set; }
        public float spTax { get; set; }
        public float? finalkbAmount { get; set; }
        public float grossClient { get; set; }
        public float netClient { get; set; }
        public float grossAirline { get; set; }
        public float netAirline { get; set; }
        public string PassengerCategory { get; set; }
        public string TicketNumber { get; set; }
        public float? GrossClientFromInvoice { get; set; }
        public float? NetClientFromInvoice { get; set; }
        public float? SVCNet { get; set; }
        public float? ConersionRate { get; set; }
        public string ReIssuedTicketNumber { get; set; }
        public List<POFlightDetailVM> FlightDetails { get; set; }
    }

    public class POFlightDetailVM
    {
        public int? familyHeadId { get; set; }
        public int? FamilyMembersId { get; set; }
        public int POFlightDetailId { get; set; }
        public int POTicketId { get; set; }
        public IFormFile TicketImages { get; set; }
        public TicketCategory? TicketCategory { get; set; }
        public string SeatNumber { get; set; }
        public string TicketNumber { get; set; }
        public int? POTicketFlightDataId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public DateTime? FlightDepartureTime { get; set; }
        public DateTime? FlightArrivalTime { get; set; }
        public string PNRNo { get; set; }
        public string PNRGDSNo { get; set; }
        public string FlightOrigion { get; set; }
        public string FlightDestination { get; set; }
        public string PreferedMeals { get; set; }
        public FlightClass FlightClass { get; set; }
        public AirportsType? AirportsType { get; set; }

        public int? SubClass { get; set; }
        public string SubClassName {  get; set; }
        public string TransectTime { get; set; }
        public int? Airline { get; set; }
        public string AirlineName { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public int CityTax { get; set; }
        public float Amount { get; set; }
        public string DepartureAirportName { get; set; }
        public string ArrivalAirportName { get; set; }
        public string ClientName { get; set; }
        public string FlightNumber { get; set; }
        public float? WTH { get; set; }
        public float? Comission { get; set; }
        public float? SPTaxAm { get; set; }
        public float? SVC { get; set; }
        public float? NetAirline { get; set; }
        public DateTime? DepDate { get; set; }
        public DateTime? ArrDate { get; set; }
        public string? IssueDate { get; set; }
        public string HbNo { get; set; }
        public float? BasicFare { get; set; }
        public float? SP { get; set; }
        public float? RG { get; set; }
        public float? SF { get; set; }
        public float? SRB { get; set; }
        public float? PK { get; set; }
        public float? PTT { get; set; }
        public float? PB { get; set; }
        public float? YQ { get; set; }
        public float? YR { get; set; }
        public float? KPR { get; set; }
        public float? KBP { get; set; }
        public float? GrossAirline { get; set; }
        public float? GrossClient { get; set; }
        public float? NetClient { get; set; }
    }

    public class PaymentViewModel
    {
        public DateTime DueDate { get; set; }
        public float? Persentage { get; set; }
        public float? Amount { get; set; }
    }
}

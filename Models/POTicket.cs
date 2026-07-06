using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Controllers.POControllers;
using TMG.Models.AccountModels;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;

namespace TMG.Models
{
    public enum AirportsType
    {
        SelectType,
        Arrival,
        Departure
    }

    public enum IssuedType
    {
        Issued,
        ReIssued,
        Refund,
        Self,
        Void
    }


    public class POTicket : BaseEntity
    {
        public int POTicketId { get; set; }
        public string PONumber { get; set; }
        public string XONumber { get; set; }
        public string PassportNumber { get; set; }
        public string FrequentFlyer { get; set; }
        public DateTime DateofBirth { get; set; }
        public DateTime PassportExpiry { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public string TPRFNo { get; set; }
        public int? FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int? FamilyMembersId { get; set; }
        public FamilyMembers FamilyMembers { get; set; }
        public int? CompanyId { get; set; }
        public int? VendorTypeId { get; set; }
        public int? VendorId { get; set; }
        public Vendor Vendor {  get; set; }
        public int? BookingAgentId { get; set; }
        //public SPO BookingAgent { get; set; }
        public int? TicketingAgentId { get; set; }
        //public SPO TicketingAgent { get; set; }
        public int? RecoveryOfficerId { get; set; }
        //public SPO RecoveryOfficer { get; set; }
        public int? PostingAgentId { get; set; }
        //public SPO PostingAgent { get; set; }
        public TicketType TicketType { get; set; }
        public FareType FareType { get; set; }
        public PostingType PostingType { get; set; }
        public IssuedType? IssuedType { get; set; }

        public DateTime? IssueDate { get; set; }
        public List<POFlightDetail> FlightDetails { get; set; } = new List<POFlightDetail>();
        public List<POPayment> POPayments { get; set; } = new List<POPayment>();
        public float? BasicFare { get; set; }

        public float? ntax { get; set; }
        public float? RG { get; set; }
        public float? SF { get; set; }
        public float? SP { get; set; }
        public float? ydAmount { get; set; }
        public float? gtaxAmount { get; set; }
        public float? pzAmount { get; set; }
        public float? qaAmount { get; set; }
        public float? rtaxAmount { get; set; }
        public float? etaxAmount { get; set; }
        public float? ioAmount { get; set; }
        public float? YR { get; set; }
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
        public float? SVC { get; set; }
        public float? SRB { get; set; }
        public float? PK { get; set; }
        public float? PTT { get; set; }
        public float? PB { get; set; }
        public float? YQ { get; set; }
        public float? KPR { get; set; }
        public float? KBP { get; set; }
        public float? OtherTaxes { get; set; }
        public float? CommissionPersentage { get; set; }
        public float? CommissionAmount { get; set; }
        public float? WithHoldingPers { get; set; }
        public float? WithHoldingAmount { get; set; }
        public float? SPTaxPers { get; set; }
        public float? SPTaxAmount { get; set; }
        public float? finalkbAmount { get; set; }
        public float? GrossClient { get; set; }
        public float? GrossAirline { get; set; }
        public float? NetClient { get; set; }
        public float? NetAirline { get; set; }
        public float? NetTotal { get; set; }
        public float? Discount { get; set; }
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
        public float? GrossClientFromInvoice { get; set; }
        public float? NetClientFromInvoice { get; set; }
        public float? SVCNet { get; set; }
        public float? NetTaxesForRefund { get; set; }
        public float? ConersionRate { get; set; }
        public string PassengerCategory { get; set; }
        public string ReIssuedTicketNumber { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }

    public class POFlightDetail : BaseEntity
    {
        public int POFlightDetailId { get; set; }
        public TicketCategory? TicketCategory { get; set; }
        public string TicketImages { get; set; }
        public string SeatNumber { get; set; }
        public string TicketNumber { get; set; }
        public int? POTicketFlightDataId { get; set; }
        public POTicketFlightData POTicketFlightData {  get; set; }
        public int? DepartureAirportId { get; set; }
        public Airport Departure { get; set; }
        public int? ArrivalAirportId { get; set; }
        public Airport Arrival { get; set; }
        public DateTime? FlightDepartureTime { get; set; }
        public DateTime? FlightArrivalTime { get; set; }
        public string PNRNo { get; set; }
        public string PNRGDSNo { get; set; }
        public string FlightOrigion { get; set; }
        public string FlightDestination { get; set; }
        public FlightClass FlightClass { get; set; }
        public AirportsType? AirportsType { get; set; }
        public string TransectTime { get; set; }
        public int? SubClassesId { get; set; }
        public SubClasses SubClasses { get; set;}
        public int? AirlineId { get; set; }
        public Airline Airline { get; set; }
        public string PreferedMeals { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public int? CityTaxId { get; set; }
        public float? Amount { get; set; }
        public int POTicketId { get; set; }
        public POTicket POTicket { get; set; }
        //public int? FamilyHeadId { get; set; }
        //public FamilyHead FamilyHead { get; set; }
        //public int? FamilyMembersId { get; set; }
        //public FamilyMembers FamilyMembers { get; set; }

    }

    public class POTicketInvoices 
    {
        public int POTicketInvoicesId { get; set; }

        public int? POTicketId { get; set; }

        public int? POTicketHeadId { get; set; }
    }

    public class POPayment
    {
        public int PoPaymentId { get; set; }
        public DateTime DueDate { get; set; }
        public float? Persentage { get; set; }
        public float? Amount { get; set; }
        public int POTicketId { get; set; }
        public POTicket POTicket { get; set; }

    }
} 
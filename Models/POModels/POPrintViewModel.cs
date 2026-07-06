using System.ComponentModel.DataAnnotations;
using TMG.Controllers.POControllers;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Models.POModels
{
    public class POPrintViewModel
    {
        public string PONumber { get; set; }
        public string XONumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public int? FamilyHeadId { get; set; }
        public string TPRFNo { get; set; }
        public int? FamilyMemberId { get; set; }
        public int CompanyId { get; set; }
        public string HajjiName { get; set; }
        public string VendorName { get; set; }
        public string VendorAccount { get; set; }
        public string VendorVATNo { get; set; }
        public string BookingAgentName { get; set; }
        public string TicketingAgentName { get; set; }
        public string RecoveryOfficerName { get; set; }
        public string PostingAgentName { get; set; }
        public string TicketType { get; set; }
        public string FareType { get; set; }
        public string PostingType { get; set; }
        public string IssueDate { get; set; }
        public float? BasicFare { get; set; }
        public float? SP { get; set; }
        public float? RG { get; set; }
        public float? SF { get; set; }
        public float? SVC { get; set; }
        public float? SRB { get; set; }
        public float? PK { get; set; }
        public float? PTT { get; set; }
        public float? PB { get; set; }
        public float? YQ { get; set; }
        public float? YR { get; set; }
        public float? KPR { get; set; }
        public float? KBP { get; set; }
        public float? CommissionPersentage { get; set; }
        public float? CommissionAmount { get; set; }
        public float? WithHoldingPers { get; set; }
        public float? WithHoldingAmount { get; set; }
        public float? SPTaxPers { get; set; }
        public float? SPTaxAmount { get; set; }
        public float? GrossClient { get; set; }
        public float? GrossAirline { get; set; }
        public float? NetClient { get; set; }
        public float? NetAirline { get; set; }
        //public Dictionary<int, List<POFlightDetail>> FlightDetails { get; set; }
        public List<POFlightDetail> FlightDetails { get; set; } // Ensure this property exists

        public List<POPayment> POPayments { get; set; } = new List<POPayment>();
        public List<POTicket> POTickets { get; set; } = new List<POTicket>();
        public List<POTicketInvoices> POTicketInvoices { get; set; }

        // Others
        public string PrintDate { get; set; }
        public string PrintedBy { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyContact { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanySite { get; set; }
    }
}

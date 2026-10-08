using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.ExpenseModels
{
    public class CompanyLedgerFilterViewModel
    {
        [Required(ErrorMessage = "Please select a company")]
        [Display(Name = "Select Company *")]
        public int CompanyId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }
    }

    public class CompanyLedgerStatementViewModel
    {
        // Header & Meta
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int TotalBookingsCount { get; set; }
        public int TotalPaxCount { get; set; }
        public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;

        // Date Filter echo
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Top KPI Bar
        public decimal TotalBookingAmount { get; set; }
        public decimal TotalReceivedAtBooking { get; set; }
        public decimal TotalReceivedViaTransactions { get; set; }
        public decimal GrandTotalReceived => TotalReceivedAtBooking + TotalReceivedViaTransactions;
        public decimal Balance => TotalBookingAmount - GrandTotalReceived;

        // Table 1: Bookings List
        public List<CompanyBookingRowDto> Bookings { get; set; } = new();

        // Table 2: Financial Transactions List
        public List<CompanyTransactionRowDto> Transactions { get; set; } = new();
    }

    public class CompanyBookingRowDto
    {
        public int SerialNo { get; set; }
        public DateTime BookingDate { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public int Pax { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Received { get; set; }
        public decimal Balance => TotalAmount - Received;
        public int FamilyHeadId { get; set; }
    }

    public class CompanyTransactionRowDto
    {
        public int SerialNo { get; set; }
        public DateTime Date { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
}

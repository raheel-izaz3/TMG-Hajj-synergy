using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.ExpenseModels
{
    public class ClientLedgerFilterViewModel
    {
        [Required(ErrorMessage = "Please select a client")]
        [Display(Name = "Select Client *")]
        public int ClientId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }
    }

    public class ClientLedgerStatementViewModel
    {
        // Client Header
        public int ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = "Active";

        // Financial KPI Summary Cards
        public decimal TotalBookingAmount { get; set; }
        public decimal ReceivedAtBooking { get; set; }
        public decimal TotalPaidViaTransactions { get; set; }
        public decimal BalanceDue => TotalBookingAmount - (ReceivedAtBooking + TotalPaidViaTransactions);

        // Date Filter echo
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Transaction History Table
        public List<ClientLedgerTransactionDto> Transactions { get; set; } = new();
    }

    public class ClientLedgerTransactionDto
    {
        public int SerialNo { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
        public string? ProofUrl { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TMG.Models.ClientModels
{
    public class ClientCompanyTransactionFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Booking For selection is required.")]
        [Display(Name = "Booking For *")]
        public string BookingFor { get; set; } = "Client"; // "Client" or "Company"

        [Display(Name = "Client *")]
        public int? FamilyHeadId { get; set; }

        [Display(Name = "Company *")]
        public int? CompanyId { get; set; }

        [Display(Name = "Select Booking *")]
        public int? SelectedBookingId { get; set; }

        [Required(ErrorMessage = "Payment Type is required.")]
        [Display(Name = "Payment Type *")]
        public string PaymentType { get; set; } = "Cash";

        [Required(ErrorMessage = "Amount in PKR is required.")]
        [Range(0.01, 999999999999.99, ErrorMessage = "Amount must be greater than zero.")]
        [Display(Name = "Amount (PKR) *")]
        public decimal AmountPkr { get; set; }

        [Required(ErrorMessage = "Payment Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date *")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Display(Name = "Reference / Cheque No")]
        [StringLength(100)]
        public string? ReferenceChequeNo { get; set; }

        [Display(Name = "Proof of Payment (JPG, PNG, PDF — Max 2MB)")]
        public IFormFile? ProofFile { get; set; }

        public string? ExistingProofFilePath { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Status *")]
        public string Status { get; set; } = "Confirmed";

        [Display(Name = "Notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class ClientCompanyTransactionIndexViewModel
    {
        public long Id { get; set; }
        public int RowIndex { get; set; }
        public string BookingFor { get; set; } = "Client";
        public int? FamilyHeadId { get; set; }
        public int? CompanyId { get; set; }
        public int? SelectedBookingId { get; set; }
        public string PartnerName { get; set; } = "N/A";
        public string BookingCode { get; set; } = "-";
        public string PaymentType { get; set; } = string.Empty;
        public decimal AmountPkr { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? ReferenceChequeNo { get; set; }
        public string? ProofFilePath { get; set; }
        public string Status { get; set; } = "Confirmed";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookingLookupDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
    }
}

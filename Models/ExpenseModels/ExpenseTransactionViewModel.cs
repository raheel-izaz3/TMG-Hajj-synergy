using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TMG.Models.ExpenseModels
{
    public class ExpenseTransactionFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Expense Head is required.")]
        [Display(Name = "Expense Head *")]
        public int ExpenseHeadId { get; set; }

        [Required(ErrorMessage = "Transaction Type is required.")]
        [Display(Name = "Type *")]
        public string Type { get; set; } = "Operational";

        [Required(ErrorMessage = "Year is required.")]
        [Display(Name = "Year *")]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 999999999999.99, ErrorMessage = "Amount must be a positive number.")]
        [Display(Name = "Amount *")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment Type is required.")]
        [Display(Name = "Payment Type *")]
        public string PaymentType { get; set; } = "Cash";

        [Required(ErrorMessage = "Currency is required.")]
        [Display(Name = "Currency *")]
        public string Currency { get; set; } = "PKR";

        [Required(ErrorMessage = "Exchange Rate is required.")]
        [Range(0.0001, 99999.9999, ErrorMessage = "Exchange Rate must be greater than zero.")]
        [Display(Name = "Exchange Rate *")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date *")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Reference No")]
        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        [Display(Name = "Proof (Slip / Screenshot)")]
        public IFormFile? ProofFile { get; set; }

        public string? ExistingProofDocumentPath { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000)]
        public string? Description { get; set; }
    }

    public class ExpenseTransactionIndexViewModel
    {
        public long Id { get; set; }
        public int RowIndex { get; set; }
        public int ExpenseHeadId { get; set; }
        public string ExpenseHeadName { get; set; } = "N/A";
        public string Type { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public string Currency { get; set; } = "PKR";
        public decimal ExchangeRate { get; set; }
        public DateTime Date { get; set; }
        public string? ReferenceNo { get; set; }
        public string? ProofDocumentPath { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

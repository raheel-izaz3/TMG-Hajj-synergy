using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.ExpenseModels
{
    [Table("ExpenseTransactions")]
    public class ExpenseTransaction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int HajjSeasonsId { get; set; }

        [Required]
        public int ExpenseHeadId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "Operational";

        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; } = "Cash";

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "PKR";

        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        [StringLength(500)]
        public string? ProofDocumentPath { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool Trashed { get; set; } = false;
    }
}

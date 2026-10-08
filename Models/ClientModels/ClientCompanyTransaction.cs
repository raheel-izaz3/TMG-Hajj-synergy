using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.ClientModels
{
    [Table("ClientCompanyTransactions")]
    public class ClientCompanyTransaction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int HajjSeasonsId { get; set; }

        [Required]
        [StringLength(20)]
        public string BookingFor { get; set; } = "Client"; // "Client" or "Company"

        public int? FamilyHeadId { get; set; }

        public int? CompanyId { get; set; }

        public int? SelectedBookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; } = "Cash";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPkr { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? ReferenceChequeNo { get; set; }

        [StringLength(500)]
        public string? ProofFilePath { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Confirmed"; // "Confirmed", "Pending", "Bounced", "Cancelled"

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool Trashed { get; set; } = false;
    }
}

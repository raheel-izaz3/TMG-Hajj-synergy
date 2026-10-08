using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.ExpenseModels
{
    [Table("ExpenseHeads")]
    public class ExpenseHead
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int HajjSeasonsId { get; set; }

        [Required(ErrorMessage = "Expense name is required.")]
        [StringLength(150, ErrorMessage = "Expense name cannot exceed 150 characters.")]
        public string ExpenseName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool Trashed { get; set; } = false;
    }
}

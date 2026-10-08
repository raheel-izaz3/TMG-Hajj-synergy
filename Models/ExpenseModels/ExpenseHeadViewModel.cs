using System;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.ExpenseModels
{
    public class ExpenseHeadFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Expense name is required.")]
        [StringLength(150, ErrorMessage = "Expense name cannot exceed 150 characters.")]
        [Display(Name = "Expense Name *")]
        public string ExpenseName { get; set; } = string.Empty;
    }

    public class ExpenseHeadIndexViewModel
    {
        public int Id { get; set; }
        public int RowIndex { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Trashed { get; set; }
    }
}

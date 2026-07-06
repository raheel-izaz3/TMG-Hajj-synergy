using Microsoft.CodeAnalysis.Operations;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccountModels
{
    public class Account: BaseEntity
    {
        public int AccountId { get; set; }

        [Required, Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required, Display(Name = "Account Name", Prompt = "Enter Account Name...")]
        public string Name { get; set; }

        public bool IsReceivable { get; set; } = false;
        public bool IsBank { get; set; } = false;
        public bool CompanySaleAccount { get; set; } = false;

    }
}

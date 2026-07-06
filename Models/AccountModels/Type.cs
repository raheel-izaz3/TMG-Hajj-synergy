using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccountModels
{
    public enum AccountNature
    {
        Debit,
        Credit
    }
    public class Type
    {
        public int TypeId { get; set; }

        [Required, Display(Name = "Account Type", Prompt = "Enter Account Type...")]
        public string Name { get; set; }

        public AccountNature AccountNature { get; set; }
    }
}

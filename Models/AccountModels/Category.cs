using TMG.Models.CompanyModels;
using System.ComponentModel.DataAnnotations;
using TMG.Models.Updated_Accounts;

namespace TMG.Models.AccountModels
{
    public class Category : BaseEntity
    {
        public int CategoryId { get; set; }

        [Required, Display(Name = "Account Sub Type")]
        public int SubTypeId { get; set; }
        public SubType SubType { get; set; }

        [Required, Display(Name = "Category", Prompt = "Enter Category Name...")]
        public string Name { get; set; }

        public bool CompanyReceivable { get; set; } = false;
        public bool CompanyBank { get; set; } = false;
        public int? AccountGroupId { get; set; }
        public AccountGroups AccountGroups { get; set; }
    }
}

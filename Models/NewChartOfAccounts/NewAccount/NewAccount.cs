using TMG.Models.CompanyModels;
using System.Collections.Generic;

namespace TMG.Models.NewChartOfAccounts.NewAccount
{
    public enum OpeningType
    {
        Debit = 1,
        Credit = 2
    }

    public class NewAccount : BaseEntity
    {
        public int NewAccountId { get; set; }

        public string AccountName { get; set; }
        public string AccountCode { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public bool IsActive { get; set; }
        public OpeningType? OpeningType { get; set; }
        public bool AllowPosting { get; set; }  
        public float OpeningDebit { get; set; } = 0;
        public float OpeningCredit { get; set; } = 0;
        public int NewAccountGroupId { get; set; }
        public NewAccountGroup NewAccountGroup { get; set; }
        public int? ParentAccountId { get; set; }   
        public NewAccount ParentAccount { get; set; }
        public ICollection<NewAccount> ChildAccounts { get; set; }
    }
}

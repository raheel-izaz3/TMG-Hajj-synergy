using TMG.Models.AccountModels;

namespace TMG.Models.Updated_Accounts;

public class GeneralLedger : BaseEntity
{
    public int GeneralLedgerId { get; set; }
    public DateTime Date { get; set; }
    public ICollection<GeneralLedgerEntry> LedgerEntries { get; set; } = new List<GeneralLedgerEntry>();
    public int? NewPaymentVoucherId { get; set; }        
    public NewPaymentVoucher NewPaymentVoucher { get; set; }

}

public class GeneralLedgerEntry
{
    public DateTime Date { get; set; } = DateTime.Now;
    public int GeneralLedgerEntryId { get; set; }
    public AccountNature AccountNature { get; set; }
    public float Amount { get; set; }
    public string Description { get; set; }
    public int GeneralLedgerId { get; set; }
    public int AccountsId { get; set; }
    public Accounts Accounts { get; set; }
}



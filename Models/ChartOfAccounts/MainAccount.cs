using TMG.Models.MiscModels;

namespace TMG.Models.ChartOfAccounts;

public class MainAccount : BaseEntity
{
    public int MainAccountId { get; set; }
    public string AccountName { get; set; }
    public float OpeningBalance { get; set; }

    public int HajjSeasonsId {  get; set; }
    public HajjSeasons HajjSeasons { get; set; }

}

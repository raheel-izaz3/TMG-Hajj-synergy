namespace TMG.Models.ChartOfAccounts;

public class AccountSubHead: BaseEntity
{
    public int AccountSubHeadId { get;set; }

    public string SubHeadName {  get;set; }
    public int TypeId { get;set; }
    public Type Type { get;set; }
}

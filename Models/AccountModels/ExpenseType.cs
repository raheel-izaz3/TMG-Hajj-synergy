using TMG.Models.Updated_Accounts;

namespace TMG.Models.AccountModels;

public class ExpenseType: BaseEntity
{
    public int ExpenseTypeId { get; set; }
    public string TypeName { get; set; }

    // Account Group
    public int? AccountGroupId { get; set; }
    public AccountGroups AccountGroup { get; set; }
}

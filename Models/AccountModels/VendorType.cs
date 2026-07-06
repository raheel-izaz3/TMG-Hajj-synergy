using TMG.Models.Updated_Accounts;

namespace TMG.Models.AccountModels;

public class VendorType : BaseEntity
{
    public int VendorTypeId { get; set; }
    public string TypeName { get; set; }

    // Account Group
    public int AccountGroupId { get; set; }
    public AccountGroups AccountGroups { get; set; }
}

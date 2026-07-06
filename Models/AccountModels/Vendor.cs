using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using TMG.Models.CompanyModels;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Models.AccountModels;

public class Vendor : BaseEntity
{ 
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public int VendorTypeId { get; set; }
    public Currencies Currencies { get; set; }
    public VendorType VendorType { get; set; }

    [Display(Name = "Account Title")]
    public string? AccountTitle {  get; set; }

    [Display(Name = "Account Number")]
    public string? AccountNumber {  get; set; }
    
    [Display(Name = "IBAN")]
    public string? IBAN { get; set; }

    [Display(Name = "Cluster Name")]
    public string? ClusterName { get; set; }

    [Display(Name = "Swift Code")] 
    public string? SwiftCode { get; set; }

    [Display(Name = "NTN")]
    public string? NTN { get; set; }
    
    [Display(Name = "VAT Number")]
    public string? VATNo { get; set; }

    [Display(Name = "Beneficiary Address")]
    public string? BeneficiaryAddress { get; set; }

    [Display(Name = "Bank Address")]
    public string? BankAddress { get; set; }
    public string Logo { get; set; }
    // Accounts
    public int? AccountsId { get; set; }
    public Accounts Accounts { get; set; }

    // Haji Season Id
    public int? HajjSeasonsId { get; set; }
    public HajjSeasons HajjSeasons { get; set; }
}

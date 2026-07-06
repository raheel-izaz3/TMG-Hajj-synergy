using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;
using TMG.Models.MiscModels;

namespace TMG.Models.POModels;

public class POVisa : BaseEntity
{
    public int POVisaId { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }

    public DateTime Date { get; set; }

    public string PONumber { get; set; }
    public float VatPercentage { get; set; }
    public float MunicipilityPercentage { get; set; }
    public float OtherTaxAmount { get; set; }

    public string Description { get; set; }
    public float Amount { get; set; }

    public float Subtotal { get; set; }
    public float VAT { get; set; }
    public float MunicipilityTax { get; set; }
    public float OtherTaxes { get; set; }
    public float Total { get; set; }

    public int VendorId { get; set; }
    public Vendor Vendor { get; set; }
    public int VendorTypeId { get; set; }
    public VendorType VendorType { get; set; }
    public string Terms { get; set; }

    public int HajjSeasonsId { get; set; }
    public HajjSeasons HajjSeasons { get; set; }

    public ICollection<POVisaInstallment> POVisaInstallments { get; set; }

}
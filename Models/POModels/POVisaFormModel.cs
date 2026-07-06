using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Models.POModels;

public class POVisaFormModel : BaseEntity
{
    public int POVisaId { get; set; }
    public int CompanyId { get; set; }

    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }

    public string PONumber { get; set; }

    public string Terms { get; set; }

    public int VendorTypeId { get; set; }
    public int VendorId { get; set; }
    public string Description { get; set; }
    public float Amount { get; set; }
    public float Subtotal { get; set; }
    public float OtherTaxes { get; set; }
    public float Total { get; set; }
    public float VatPercentage { get; set; }
    public float MunicipilityPercentage { get; set; }
    public float OtherTaxAmount { get; set; }
    public float VAT { get; set; }
    public float MunicipilityTax { get; set; }
    public List<NewTax> Taxes { get; set; } = new();
    public List<DateTime> DueDates { get; set; } = new();
    public List<float> Percentages { get; set; } = new();
    public List<float> Amounts { get; set; } = new();

}

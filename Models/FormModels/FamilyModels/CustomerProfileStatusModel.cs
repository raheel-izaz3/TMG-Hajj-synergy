using TMG.Models.FamilyModels;

namespace TMG.Models.FormModels.FamilyModels;

public class CustomerProfileStatusModel
{
    public List<int> CustomerPortfolioIds { get; set; }
    public int? Remarks { get; set; }
    public int? Status { get; set; }
}
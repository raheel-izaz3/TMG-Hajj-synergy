using TMG.Models.FamilyModels;

namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadMembersModel
{
    public List<int> CustomerPortfolioIds { get; set; }
    public List<int> ClientDataIds { get; set; }
    public List<int> FamilyHeadIds { get; set; }
    public List<int> HonorificIds { get; set; }
    public List<string> GivenNames { get; set; }
    public List<string> SurNames { get; set; }
    public List<Gender> Genders { get; set; }
    public List<DateTime> DateOfBirths { get; set; }
    public List<int> FiqahIds { get; set; }
    public List<string> PassportNumbers { get; set; }
    public List<DateTime> PassportsDateOfIssue { get; set; }
    public List<DateTime> PassportsDateOfExpiry { get; set; }

}
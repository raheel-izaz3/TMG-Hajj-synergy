namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadContactModel
{
    // Address
    public List<int?> MemberAddressTypeIds { get; set; }
    public List<string> MemberAddresses { get; set; }
    public List<string> MemberAddressCities { get; set; }
    public List<string> MemberAddressCountries { get; set; }
    public List<string> MemberAddressZipCodes { get; set; }
    public List<string> MemberAddressPoBoxes { get; set; }
    public List<int?> MemberAddressTehsilCodes { get; set; }
    public List<bool> MemberAddressPreferred { get; set; }
    public List<int?> CountryId { get; set; }
    public List<int?> CityId { get; set; }

    // Contact 
    public List<int?> MemberContactTypeIds { get; set; }
    public List<string> MemberContacts { get; set; }
    public List<bool> MemberContactPreferred { get; set; }

    // Email
    public List<int?> MemberEmailTypeIds { get; set; }
    public List<string> MemberEmails { get; set; }
    public List<bool> MemberEmailPreferred { get; set; }
}
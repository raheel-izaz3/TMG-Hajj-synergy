namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadNomineeModel
{
    public List<string> NomineeNames { get; set; }
    public List<int?> NomineeRelations { get; set; }
    public List<string> NomineeCNICs { get; set; }
    public List<string> NomineeAddresses { get; set; }
    public List<string> NomineeCountries { get; set; }
    public List<int?> NomineeCountriesId { get; set; }
    public List<string> NomineeCities { get; set; }
    public List<int?> NomineeCitiesId { get; set; }
    public List<string> NomineeZipCodes { get; set; }
    public List<string> NomineePoBoxes { get; set; }
    public List<string> NomineeContacts { get; set; }
    public List<string> NomineeEmails { get; set; }
    public List<bool> NomineePreferred { get; set; }
    public IFormFile NomineeCnic { get; set; }
}
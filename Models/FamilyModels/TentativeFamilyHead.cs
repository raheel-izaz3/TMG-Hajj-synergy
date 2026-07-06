namespace TMG.Models.FamilyModels
{
    public record TentativeFamilyHead(
        string SurName, string GivenName, string CNIC, string Phone, int PackageId, 
        int ContactTypeId, int SPOID, int adult, int child, int infant, int HonorificsId,
        bool RouteAsPerPackage, bool TrainAsPerPackage, bool FlightAsPerPackage, string Email, DateTime DateOfQuery, int? Remarks, int? Status, int? Refferel,
        ApplicantType ApplicantType, int? CityId, int? CountryId, int? HajjDepartmentId, string Address, int? ApplicantTypesId, int? TehsilId, string PoBox,
        string ZipCode, string FatherHusbandName, string Password, string GroupNo
        );

}

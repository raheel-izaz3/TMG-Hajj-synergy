using TMG.Models.FamilyModels;

namespace TMG.Controllers.FamilyControllers;
public partial class FamilyHeadFormController
{
    public sealed record FamilyHeadPassportModel
    {
        public int? CityId { get; set; }
        public int? HajjDepartmentId { get; set; }
        public int? SPOId { get; set; }

        public string FeedBy { get; set; }

        public ApplicantType ApplicantType { get; set; }

        public int? HonorificsId { get; set; }
        
        public string SurName { get; set; }

        public string GivenName { get; set; }
        public DateTime? DateOfEmail { get; set; }

        public MehramRelationEnum MehramRelationEnum { get; set; } = MehramRelationEnum.Neither;

        public string FatherHusbandName { get; set; }

        public string CNIC { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string PlaceOfBirth { get; set; }

        public string Nationality { get; set; }

        public Gender Gender { get; set; }

        public string BloodGroup { get; set; }

        public int? FiqahId { get; set; }

        public bool IsMarried { get; set; }

        public string Residency { get; set; }

        public string PassportType { get; set; }

        public string CountryCode { get; set; }

        public string PassportNumber { get; set; }

        public DateTime? PassportDateOfIssue { get; set; }

        public DateTime? PassportDateOfExpiry { get; set; }

        public string EmployedAt { get; set; }
        public int? Designation { get; set; }

        public string IssuingAuthority { get; set; }

        public string TrackingNumber { get; set; }

        public string BookletNumber { get; set; }

        public int? PassportTypeId { get; set; }
        public int? CountryId { get; set; }
        public int? NationalityId { get; set; }
        public int? CountryCodeId { get; set; }

        public bool BiometricVerified { get; set; } = false;
        public bool ContactPerson { get; set; }
        public string BiometricCode { get; set; }

        public string TokenRemarks { get; set; }
        public string FFNumber { get; set; }
        public string FileNumber { get; set; }


    }
}

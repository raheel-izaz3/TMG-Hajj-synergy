using TMG.ManagerClasses;
using TMG.Models.AccountModels;
using TMG.Models.AirlineModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using TMG.Models.ReservationModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.FamilyModels
{
    public class CustomerPortfolio : BaseEntity
    {
        public ApplicantType ApplicantType { get; set; } = ApplicantType.Regular;
        public int CustomerPortfolioId { get; set; }

        public bool IsHead { get; set; }

        public int? SPOId { get; set; }
        public SPO SPO { get; set; }

        public int? HajjDepartmentId { get; set; }
        public HajjDepartment HajjDepartment { get; set; }

        public DateTime DateOfQuery { get; set; }

        public string FeedBy { get; set; }

        public int? HonorificsId { get; set; }
        public Honorifics Honorifics { get; set; }
        public string SurName { get; set; }

        public string GivenName { get; set; }
        public string FatherHusbandName { get; set; }

        public DateTime? DateOfEmail { get; set; }

        public string CNIC { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string PlaceOfBirth { get; set; }

        public string Nationality { get; set; }

        public Gender Gender { get; set; }

        public string BloodGroup { get; set; }

        public int? FiqahId { get; set; }
        public Fiqah Fiqah { get; set; }

        public bool IsMarried { get; set; }

        public bool IsEmployed { get; set; }

        public string EmployedAt { get; set; }

        public string Designation { get; set; }

        public string Residency { get; set; }

        public string PassportType { get; set; }

        public string CountryCode { get; set; }

        public int? PassportTypeId { get; set; }
        public PassportType PassportTypes { get; set; }

        public int? CountryId { get; set; }
        public Country Countries { get; set; }

        public int? NationalityId { get; set; }
        public Nationality Nationalities { get; set; }

        public int? CountryCodeId { get; set; }
        public CountryCode CountryCodes { get; set; }

        public string PassportNumber { get; set; }

        public DateTime? PassportDateOfIssue { get; set; }

        public DateTime? PassportDateOfExpiry { get; set; }
        public bool ContactPerson { get; set; }

        public string MemberImage { get; set; }

        public string PassportImage { get; set; }
        public string PassportTokenImage { get; set; }
        public string TokenRemarks { get; set; }

        public string VisaImage { get; set; }

        public int? FamilyHeadRelationId { get; set; }
        public FamilyHeadRelation FamilyHeadRelation { get; set; }

        public bool Tentative { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }

        public int? CustomerRemarksId { get; set; }
        public CustomerRemarks CustomerRemarks { get; set; }

        public int? CustomerStatusId { get; set; }
        public CustomerStatus CustomerStatus { get; set; }

        public int? ReferenceId { get; set; }
        public Reference Reference { get; set; }

        public int? CityId { get; set; }
        public City City { get; set; }

        public bool? IsQuery { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

        public ICollection<CustomerPortfolioEmail> CustomerPortfolioEmails { get; set; } = new List<CustomerPortfolioEmail>();

        [NotMapped] public int Age => DateTimeManager.CalculateAge(DateOfBirth);
        [NotMapped] public bool IsAdult => Age >= 12;
        [NotMapped] public bool IsChild => Age >= 2 && Age < 12;
        [NotMapped] public bool IsInfant => Age < 2;
        [NotMapped] public string FullName => $"{GivenName} {SurName}";
    }
}
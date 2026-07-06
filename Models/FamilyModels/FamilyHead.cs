using TMG.ManagerClasses;
using TMG.Models.AccountModels;
using TMG.Models.AirlineModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using TMG.Models.ReservationModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.Updated_Accounts;
using TMG.Models.GroupModels;
using System.Text.RegularExpressions;

namespace TMG.Models.FamilyModels
{
    public enum HajjiBookingStatus
    {
        Tentative, Booked, Cancelled
    }
    public enum MehramRelationEnum
    {
        Neither, Father, Husband
    }
    public enum DiscountType
    {
        None, Percentage, FixedAmount
    }
    public enum Gender
    {
        Male, Female
    }

    public enum ApplicantType
    {
        Regular, Overseas, Intercity
    }

    public enum FlightTicketType
    {
        Included, Excluded, IncludedWithDifferences
    }

    public enum QurbaniTypes
    {
        Excluded, Paid, Complimentry
    }

    public enum TktStatus
    {
        NotIssued, Self, Issued, Tentative 
    }

    public enum VisaStatus
    {
        Issued, NonIssued
    }

    public class FamilyHead : BaseEntity
    {
        public bool AccomodationAsPerPackage { get; set; }
        public bool FlightAsPerPackage { get; set; }
        public bool TrainAsPerPackage { get; set; }
        public bool RouteAsPerPackage { get; set; }
        public string NusukId { get; set; }
        public int? QurbaniToken { get; set; }
        public int? AssignedVehicle { get; set; }
        public bool QurbaniDone { get; set; }
        public float QurbaniPkr { get; set; }
        public float QurbaniSar { get; set; }
        public float QurbaniUsd { get; set; }
        public QurbaniTypes QurbaniType { get; set; }
        public TktStatus? TicketStatus { get; set; }
        public VisaStatus? VisaStatus { get; set; }
        public int? CompanyVisasId { get; set; }
        public CompanyVisas CompanyVisas { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string RecieverName { get; set; }

        public DateTime? DDDate { get; set; }
        public string DDReceiver { get; set; }

        /*public string Maktab { get; set; }
        public string MaktabNumber { get; set; }*/

        public bool EHajjVerified { get; set; }
        public string KSAMobileNumber { get; set; }
        public bool ENumberVerified { get; set; }
        public string POD { get; set; }
        public string FcId { get; set; }
        public string IndId { get; set; }
        public bool Verified { get; set; }
        public float SARROE { get; set; }
        public float USDROE { get; set; }

        public FlightTicketType FlightTicketType { get; set; }
        public float? FlightTicketDifferencesPkr { get; set; } = 0f;
        public float? FlightTicketDifferencesSar { get; set; } = 0f;
        public float? FlightTicketDifferencesUsd { get; set; } = 0f;
        public string FlightClass { get; set; }

        public ApplicantType ApplicantType { get; set; } = ApplicantType.Regular;
        public int FamilyHeadId { get; set; }

        public bool IsHead { get; set; }

        public int? MehramFamilyHeadId { get; set; }

        public int? RelationWithMehramId { get; set; }
        public MehramRelation RelationWithMehram { get; set; }

        public DateTime? ProformaDate { get; set; }

        #region Passport
        public int? SPOId { get; set; }
        public SPO SPO { get; set; }

        public string FeedBy { get; set; }

        public int? HonorificsId { get; set; }
        public Honorifics Honorifics { get; set; }
        public string SurName { get; set; }

        public string GivenName { get; set; }

        public MehramRelationEnum MehramRelationEnum { get; set; } = MehramRelationEnum.Neither;
        public string FatherHusbandName { get; set; }

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

        public int? DesignationId { get; set; }
        public Designation Designations { get; set; }

        [ForeignKey("NomineeCountry")]
        public int? NomineeCountryId { get; set; }
        public Country NomineeCountry { get; set; } 

        [ForeignKey("NomineeCity")]
        public int? NomineeCityId { get; set; }
        public City NomineeCity { get; set; }

        public string Residency { get; set; }

        public string PassportType { get; set; }

        public int? PassportTypeId { get; set; }
        public PassportType PassportTypes { get; set; }

        public int? CountryId { get; set; }
        public Country Countries { get; set; }

        public int? NationalityId { get; set; }
        public Nationality Nationalities { get; set; }

        public int? CountryCodeId { get; set; }
        public CountryCode CountryCodes { get; set; }

        public string CountryCode { get; set; }

        public string PassportNumber { get; set; }

        public DateTime? PassportDateOfIssue { get; set; }

        public DateTime? PassportDateOfExpiry { get; set; }

        public string IssuingAuthority { get; set; }

        public string TrackingNumber { get; set; }

        public string BookletNumber { get; set; }

        public string PassportScanType { get; set; }

        public string PassportMRZScan { get; set; }

        public string VisaScanType { get; set; }

        public string VisaMRZScan { get; set; }

        public bool BiometricVerified { get; set; } = false;
        public bool ContactPerson { get; set; }
        public string BiometricCode { get; set; }

        public string MemberImage { get; set; }
        public string MISPdfPath { get; set; }
        public string SignedMISPdfPath { get; set; }

        public string PassportImage { get; set; }
        public string PassportTokenImage { get; set; }
        public string Ehajj { get; set; }
        public string TokenRemarks { get; set; }

        public string VisaImage { get; set; }

        public string MemberVaccinationImage { get; set; }
        public string MemberTrainImage { get; set; }
        public string BiometricImage { get; set; }

        public string InsuranceImage { get; set; }
        public string AirlineTicketImage { get; set; }
        public string BoardedAirlineTicketImage { get; set; }
        public string TrainTicketImage { get; set; }

        public string FileNumber { get; set; }

        public string VisaRemarks { get; set; }
        public string VisaTMGSign { get; set; }

        #endregion

        #region Reference
        public int? ReferenceId { get; set; }
        public Reference Reference { get; set; }

        //public string FriendCNIC { get; set; }
        public int? FriendId { get; set; }
        public FamilyHead Friend { get; set; }

        public int? FamilyHeadRelationId { get; set; }
        public FamilyHeadRelation FamilyHeadRelation { get; set; }
        #endregion

        #region Hajj App Form
        public string FormNumber { get; set; }

        public string HajjAppSerialNumber { get; set; }

        public string HajjApplicationNumber { get; set; }

        public string GroupCoordAppNumber { get; set; }

        public string SubstitutedAgainstAppNumber { get; set; }

        public string ENumber { get; set; }

        public string EHajjId { get; set; }

        public string EHajjSerial { get; set; }

        public string MofaNumber { get; set; }

        public string TicketNumber { get; set; }

        public string VisaNumber { get; set; }

        public bool PerformedHajjInLastFiveYears { get; set; }

        public bool WantToPerformHajjEBadal { get; set; }

        public bool WantToPerformAsGroupWorker { get; set; }

        public bool Eligible { get; set; }
        public string YearsPerformedHajj { get; set; }
        public HajjiBookingStatus HajjiBookingStatus { get; set; }

        #endregion

        #region Package
        public int PackageId { get; set; } = 0;
        public Package Package { get; set; }

        public bool Tentative { get; set; }

        //public int? AirlineFareId { get; set; }
        //public AirlineFare AirlineFare { get; set; }

        public int? FlightId { get; set; }
        public Flight Flight { get; set; }

        public bool? BothChildBedMedinah { get; set; }

        public bool? BothChildBedMakkah { get; set; }

        public string MaleChildSharingMedinah { get; set; }
        public string MaleChildSharingMakkah { get; set; }
        public string MaleChildSharingAzizia { get; set; }
        public string MaleChildSharingMinaArafat { get; set; }
        public Currencies? Currencies { get; set; }

        public string TrainNumber { get; set; }
        public string SeatNo { get; set; }
        public string CoachNo { get; set; }
        //public Currency Currency { get; set; }

        //public float BaseAmount { get; set; }

        #endregion

        #region Discount
        //public DiscountType DiscountType { get; set; } = DiscountType.None;

        public float? GeneralDiscount { get; set; } = 0;

        public float PkrDiscountAmount { get; set; } = 0;
        public float SarDiscountAmount { get; set; } = 0;
        public float UsdDiscountAmount { get; set; } = 0;

        //public DiscountType CurrencyAdjustment { get; set; } = DiscountType.None;

        //public float CurrencyAmountOrPercentage { get; set; } = 0;

        //public DiscountType PackageRebateType { get; set; } = DiscountType.None;

        //public float RebateAmountOrPercentage { get; set; } = 0;

        #endregion

        #region Vaccine
        public int? VaccineId1 { get; set; }
        public DateTime? DoseDate1 { get; set; }

        public int? VaccineId2 { get; set; }
        public DateTime? DoseDate2 { get; set; }

        public int? VaccineId3 { get; set; }
        public DateTime? DoseDate3 { get; set; }

        public int? VaccineId4 { get; set; }
        public DateTime? DoseDate4 { get; set; }

        public int? VaccineId5 { get; set; }
        public DateTime? DoseDate5 { get; set; }
        #endregion

        public int? AccountId { get; set; }
        public Account Account { get; set; }

        public int? RevenueAccountsId { get; set; }
        public int? ReceivableAccountsId { get; set; }
        public Accounts Accounts { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

        public string SignatureBase64 { get; set; }
        public string AgreementBase64 { get; set; }
        public DateTime? SignatureDate { get; set; }

        public int? StayWithId { get; set; }
        // public FamilyHead StayWith { get; set; }

        public int? ConnectedWithHotelId { get; set; }
        public int? ConnectedWithFloorNumber { get; set; }
        public int? ConnectedWithRoomNumber { get; set; }

        public string BloodProof { get; set; }
        public string CnicUpload { get; set; }
        public string PolioUpload { get; set; }
        public string CovidUpload { get; set; }
        public string MangatitiseUpload { get; set; }
        public string InfluenzaUpload { get; set; }

        [NotMapped] public int Age => DateTimeManager.CalculateAge(DateOfBirth);
        [NotMapped] public bool IsAdult => Age >= 12;
        [NotMapped] public bool IsChild => Age >= 2 && Age < 12;
        [NotMapped] public bool IsInfant => Age < 2;
        
        [NotMapped] public string ExtractedFamilyNumber => FcId?.Remove(0, 3);
        [NotMapped] public string FullName => $"{GivenName} {SurName}";

        [NotMapped] public bool IsTicketIncluded => FlightTicketType == FlightTicketType.Included;
        [NotMapped] public bool IsTicketExcluded => FlightTicketType == FlightTicketType.Excluded;
        [NotMapped] public bool IsTicketIncludedWithDifference => FlightTicketType == FlightTicketType.IncludedWithDifferences;

        public bool GiftPack { get; set; }
        public bool SuitCase { get; set; }

        public ICollection<FamilyHeadFFNumber> FFNumbers { get; set; } = new List<FamilyHeadFFNumber>();
        public ICollection<FamilyHeadFlight> FamilyHeadFlights { get; set; } = new List<FamilyHeadFlight>();
        public ICollection<FamilyHeadAddress> FamilyHeadAddresses { get; set; } = new List<FamilyHeadAddress>();
        public ICollection<FamilyHeadEmail> FamilyHeadEmails { get; set; } = new List<FamilyHeadEmail>();
        public ICollection<FamilyHeadContact> FamilyHeadContacts { get; set; } = new List<FamilyHeadContact>();
        public ICollection<POTicket> POTickets { get; set; } = new List<POTicket>();

    }
}

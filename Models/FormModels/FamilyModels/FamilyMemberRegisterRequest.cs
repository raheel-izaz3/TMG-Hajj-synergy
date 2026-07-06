using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.FamilyModels
{
   

    public class FamilyMemberRegisterRequest
    {
        public string POD {  get; set; }
        public int ClientDataId { get; set; }
        public bool Tentative { get; set; }
        
        [Display(Name = "Years Performed Hajj", Prompt = "Enter Years...")]
        public string YearsPerformedHajj { get; set; }

        [Display(Name = "Applicant Type")]
        public ApplicantType ApplicantType { get; set; }
        public int FamilyHeadId { get; set; }
        public int CustomerPortfolioId { get; set; }

        [Display(Name = "Father / Husband")]
        public MehramRelationEnum MehramRelation { get; set; }

        public string MehramCNIC { get; set; }

        [Display(Name = "Relation w/ Mehram")]
        public int? RelationWithMehramId { get; set; }

        public int? PassportTypeId { get; set; }
        public int? CountryId { get; set; }
        public int? NationalityId { get; set; }
        public int? CountryCodeId { get; set; }
        public int? DesignationId { get; set; }

        public string Number { get; set; }
        public string Email { get; set; }
        public int SId { get; set; }
        public DateTime RegistrationDate { get; set; }


        #region Passport

        [Display(Name = "Booked By", Prompt = "Enter Name...")]
        public int? SPOId { get; set; }

        [Display(Name = "Feed By", Prompt = "Enter Name...")]
        public string FeedBy { get; set; }

        [Display(Name = "Honorifics")]
        public int? HonorificsId { get; set; }

        [Display(Name = "Sur Name", Prompt = "Enter Sur Name...")]
        public string SurName { get; set; }

        [Display(Name = "Given Name", Prompt = "Enter Given Name...")]
        public string GivenName { get; set; }

        [Display(Name = "Father / Husband")]
        public MehramRelationEnum MehramRelationEnum { get; set; }

        [Display(Name = "Father / Husband Name", Prompt = "Enter Father / Husband Name...")]
        public string FatherHusbandName { get; set; }

        [Display(Name = "CNIC", Prompt = "Enter CNIC...")]
        public string CNIC { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Place of Birth", Prompt = "Enter Place of Birth...")]
        public string PlaceOfBirth { get; set; }

        [Display(Name = "Nationality", Prompt = "Enter Nationality...")]
        public string Nationality { get; set; }

        [Display(Name = "Gender", Prompt = "Enter Gender..."), ReadOnly(true), DefaultValue("Male")]
        public Gender Gender { get; set; }

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        [Display(Name = "Fiqah")]
        public int? FiqahId { get; set; }

        [Display(Name = "Is Married?")]
        public bool IsMarried { get; set; }

        //[Display(Name = "Is Employed?")]
        //public bool IsEmployed { get; set; }

        [Display(Name = "Employed At", Prompt = "Enter Company Name...")]
        public string EmployedAt { get; set; }

        //[Display(Name = "Designation", Prompt = "Enter Designation...")]
        //public string Designation { get; set; }

        [Display(Name = "Residency", Prompt = "Enter Residency...")]
        public string Residency { get; set; }

        [Display(Name = "Passport Type", Prompt = "Enter Passport Type...")]
        public string PassportType { get; set; }
        
        [Display(Name = "Country Code", Prompt = "Enter Country Code...")]
        public string CountryCode { get; set; }

        [Display(Name = "Passport Number", Prompt = "Enter Passport Number...")]
        public string PassportNumber { get; set; }

        [Display(Name = "Passport Date of Issue")]
        [DataType(DataType.Date)]
        public DateTime? PassportDateOfIssue { get; set; }

        [Display(Name = "Passport Date of Expiry")]
        [DataType(DataType.Date)]
        public DateTime? PassportDateOfExpiry { get; set; }

        //[Display(Name = "Biometric Verified")]
        //public bool BiometricVerified { get; set; }

        //[Display(Name = "Biometric Code", Prompt = "Enter Code...")]
        //public string BiometricCode { get; set; }

        [Display(Name = "Issuing Authority", Prompt = "Enter Issuing Authority...")]
        public string IssuingAuthority { get; set; }

        //[Display(Name = "Tracking Number", Prompt = "Enter Tracking Number...")]
        //public string TrackingNumber { get; set; }

        //[Display(Name = "Booklet Number", Prompt = "Enter Booklet Number...")]
        //public string BookletNumber { get; set; }

        //[Display(Name = "Passport Scan Type", Prompt = "Enter Passport Scan Type...")]
        //public string PassportScanType { get; set; }

        //[Display(Name = "Passport MRZ Scan", Prompt = "Enter Passport MRZ Scan...")]
        //public string PassportMRZScan { get; set; }

        //[Display(Name = "Visa MRZ Type", Prompt = "Enter Visa MRZ Type...")]
        //public string VisaScanType { get; set; }

        //[Display(Name = "Visa MRZ Scan", Prompt = "Enter Visa MRZ Scan...")]
        //public string VisaMRZScan { get; set; }

        [Display(Name = "Member Image")]
        public IFormFile MemberImage { get; set; }

        [Display(Name = "Passport Image")]
        public IFormFile PassportImage { get; set; }

        [Display(Name = "Visa Image")]
        public IFormFile VisaImage { get; set; }

        [Display(Name = "Insurance Image")]
        public IFormFile InsuranceImage { get; set; }

        [Display(Name = "Train Ticket")]
        public IFormFile TrainTicketImage { get; set; }

        [Display(Name = "Airline Ticket")]
        public IFormFile AirlineTicketImage { get; set; }

        [Display(Name = "Passport Token Image")]
        public IFormFile PassportTokenImage { get; set; }
        
        [Display(Name = "Member Vaccination Image")]
        public IFormFile MemberVaccinationImage { get; set; }
        public string TokenRemarks { get; set; }

        [Display(Name = "E - Hajj Upload")]
        public IFormFile Ehajj { get; set; }
        public int ImageExtensionError { get; set; }

        //[Display(Name = "Contact Person")]
        //public bool ContactPerson { get; set; }

        [Display(Name = "File Number", Prompt = "Enter File Number...")]
        public string FileNumber { get; set; }
        #endregion

        #region Contact
        public List<int?> MemberAddressTypeIds { get; set; }
        public List<string> MemberAddresses { get; set; }
        public List<string> MemberAddressCities { get; set; }
        public List<string> MemberAddressCountries { get; set; }
        public List<string> MemberAddressZipCodes { get; set; }
        public List<string> MemberAddressPoBoxes { get; set; }
        public List<int?> MemberAddressTehsilCodes { get; set; }
        public List<bool> MemberAddressPreferred { get; set; }

        public List<int?> MemberContactTypeIds { get; set; }
        public List<string> MemberContacts { get; set; }
        public List<bool> MemberContactPreferred { get; set; }

        public List<int?> MemberEmailTypeIds { get; set; }
        public List<string> MemberEmails { get; set; }
        public List<bool> MemberEmailPreferred { get; set; }

        #endregion

        #region Reference
        [Display(Name = "Reference")]
        public int? ReferenceId { get; set; }

        [Display(Name = "RELATION WITH FAMILY HEAD")]
        public int? FamilyHeadRelationId { get; set; }
        #endregion
        
        #region Hajj App Form
        [Display(Name = "Form #", Prompt = "Enter Form #...")]
        public string FormNumber { get; set; }

        [Display(Name = "Hajj App Serial #", Prompt = "Enter Application Serial #...")]
        public string HajjAppSerialNumber { get; set; }

        [Display(Name = "Hajj Application #", Prompt = "Enter Application #...")]
        public string HajjApplicationNumber { get; set; }

        [Display(Name = "GROUP COORDINATOR'S APP #", Prompt = "Enter Application #...")]
        public string GroupCoordAppNumber { get; set; }

        [Display(Name = "SUBSTITUTED AGAINST APP #", Prompt = "Enter Application #...")]
        public string SubstitutedAgainstAppNumber { get; set; }

        [Display(Name = "E #", Prompt = "Enter E #...")]
        public string ENumber { get; set; }

        [Display(Name = "E Hajj Id", Prompt = "Enter E Hajj Id...")]
        public string EHajjId { get; set; }

        [Display(Name = "E Hajj Serial #", Prompt = "Enter E Hajj Serial #...")]
        public string EHajjSerial { get; set; }

        [Display(Name = "MOFA #", Prompt = "Enter MOFA #...")]
        public string MofaNumber { get; set; }

        [Display(Name = "Ticket #", Prompt = "Enter Ticket #...")]
        public string TicketNumber { get; set; }

        [Display(Name = "Visa #", Prompt = "Enter Visa #...")]
        public string VisaNumber { get; set; }

        [Display(Name = "Performed Hajj in Last 5 Years?")]
        public bool PerformedHajjInLastFiveYears { get; set; }

        [Display(Name = "want to perform hajj-e-badal?")]
        public bool WantToPerformHajjEBadal { get; set; }

        [Display(Name = "Want to perform hajj as group worker?")]
        public bool WantToPerformAsGroupWorker { get; set; }

        [Display(Name = "Hajj Form Status")]
        public bool Eligible { get; set; }

        public List<int> RequiredDocumentIds {  get; set; }

        public List<bool> Recieved {  get; set; }

        [Display(Name = "Remarks", Prompt = "Enter Remarks...")]
        public List<string> DocumentRemarks {  get; set; }

        #endregion

        #region Package
        [Display(Name = "Package")]
        public int PackageId { get; set; }

        // Package Additional Facility 
        public List<int> PackageAdditionalFacilitiesIds { get; set; }
        public List<bool> PackageAdditionalFacilitiesChecked { get; set; }

        [Display(Name = "Flight")]
        public int? FlightId { get; set; }

        [Display(Name = "Flight Ticket")]
        public FlightTicketType FlightTicketType { get; set; }
        
        [Display(Name = "Flight Class", Prompt = "Enter Class...")]
        public string FlightClass { get; set; }


        [Display(Name = "Flight Ticket Differences (PKR)")]
        public float? FlightTicketDifferencesPkr { get; set; } = 0f;

        [Display(Name = "Flight Ticket Differences (SAR)")]
        public float? FlightTicketDifferencesSar { get; set; } = 0f;

        [Display(Name = "Flight Ticket Differences (USD)")]
        public float? FlightTicketDifferencesUsd { get; set; } = 0f;

        [Display(Name = "BED - MEDINAH")]
        public bool BothChildBedMedinah { get; set; }

        [Display(Name = "BED - Makkah")]
        public bool BothChildBedMakkah { get; set; }

        [Display(Name = "Male Child Sharing Medinah")]
        public string MaleChildSharingMedinah { get; set; }

        [Display(Name = "Male Child Sharing Makkah")]
        public string MaleChildSharingMakkah { get; set; }

        [Display(Name = "Male Child Sharing Azizia")]
        public string MaleChildSharingAzizia { get; set; }

        [Display(Name = "Male Child Sharing Arafat")]
        public string MaleChildSharingMinaArafat { get; set; }

        #endregion

        #region Nominee

        public List<string> NomineeNames { get; set; }
        public List<int?> NomineeRelations { get; set; }
        public List<string> NomineeCNICs { get; set; }
        public List<string> NomineeAddresses { get; set; }
        public List<string> NomineeCountries { get; set; }
        public List<string> NomineeCities { get; set; }
        public List<string> NomineeZipCodes { get; set; }
        public List<string> NomineePoBoxes { get; set; }
        public List<string> NomineeContacts { get; set; }
        public List<string> NomineeEmails { get; set; }
        public List<bool> NomineePreferred { get; set; }

        #endregion

        #region Extra Facility
        public List<string> FacilityCode { get; set; }
        public List<string> FacilityDesc { get; set; }
        public List<int?> ExtraFacilityIds { get; set; }
        public List<float?> FacilityPrice { get; set; }
        public List<float?> FacilityPriceSar { get; set; }
        public List<float?> FacilityPriceUsd { get; set; }

        #endregion

        #region Discount
        //[Display(Name = "Discount Type")]
        //public DiscountType DiscountType { get; set; }

        //[Display(Name = "Amount / Percentage", Prompt = "Enter Amount / Percentage...")]
        //public float? DiscountAmountOrPercentage { get; set; } = 0;

        [Display(Name = "PKR Discount Amount", Prompt = "Enter PKR Discount Amount...")]
        public float PkrDiscountAmount { get; set; } = 0;

        [Display(Name = "SAR Discount Amount", Prompt = "Enter SAR Discount Amount...")]
        public float SarDiscountAmount { get; set; } = 0;

        [Display(Name = "USD Discount Amount", Prompt = "Enter USD Discount Amount...")]
        public float UsdDiscountAmount { get; set; } = 0;


        //[Display(Name = "Currency Adjustment", Prompt = "Enter Currency Adjustment...")]
        //public DiscountType CurrencyAdjustment { get; set; }

        //[Display(Name = "Amount / Percentage", Prompt = "Enter Amount / Percentage...")]
        //public float? CurrencyAmountOrPercentage { get; set; } = 0;

        //[Display(Name = "Package Rebate Type")]
        //public DiscountType PackageRebateType { get; set; }

        //[Display(Name = "Amount / Percentage", Prompt = "Enter Amount / Percentage...")]
        //public float? RebateAmountOrPercentage { get; set; } = 0;

        #endregion

        #region Remarks
        public List<string> Remarks { get; set; }
        #endregion

        #region Transport
        public List<int> RouteIds { get; set; } = new List<int>();

        [Display(Name = "Check In")]
        public List<DateTime?> CheckIns { get; set; } = new List<DateTime?>();

        [Display(Name = "Check Out")]
        public List<DateTime?> CheckOuts { get; set; } = new List<DateTime?>();

        /*[Display(Name = "Time In", Prompt = "Time In")]
        public List<string> TimeIns { get; set; } = new List<string>();

        [Display(Name = "Time Out", Prompt = "Time Out")]
        public List<string> TimeOuts { get; set; } = new List<string>();*/

        [Display(Name = "Vehicle")]
        public List<int?> VehicleIds { get; set; } = new List<int?>();
        public List<FamilyTranasport> TransportTypes { get; set; }

        #endregion

        #region Vaccine
        [Display(Name = "Vaccine Name")]
        public int? VaccineId1 { get; set; }

        [Display(Name = "Dose Date")]
        [DataType(DataType.Date)]
        public DateTime? DoseDate1 { get; set; }

        [Display(Name = "Vaccine Name")]
        [DataType(DataType.Date)]
        public int? VaccineId2 { get; set; }

        [Display(Name = "Dose Date")]
        [DataType(DataType.Date)]
        public DateTime? DoseDate2 { get; set; }

        [Display(Name = "Vaccine Name")]
        public int? VaccineId3 { get; set; }

        [Display(Name = "Dose Date")]
        [DataType(DataType.Date)]
        public DateTime? DoseDate3 { get; set; }

        [Display(Name = "Vaccine Name")]
        public int? VaccineId4 { get; set; }

        [Display(Name = "Dose Date")]
        [DataType(DataType.Date)]
        public DateTime? DoseDate4 { get; set; }

        [Display(Name = "Vaccine Name")]
        public int? VaccineId5 { get; set; }

        [Display(Name = "Dose Date")]
        [DataType(DataType.Date)]
        public DateTime? DoseDate5 { get; set; }
        #endregion

    }
}

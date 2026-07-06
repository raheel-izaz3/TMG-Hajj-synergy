using TMG.Models.FamilyModels;

namespace TMG.Models.FamilyHeadReportModels
{
    public record DataFormModelEntry
    {
        public List<FamilyHeadDocument> Documents { get; set; }

        public int Id { get; set; }
        public string SignedDate { get; set; }
        public bool ShowSignature { get; set; }
        public string CID { get; set; }
        public string SignatureBase64 { get; set; }
        public string MehramSignatureBase64 { get; set; }
        public string ApplicantType { get; set; }
        public string FamilyHeadImage { get; set; }
        public string HajjAppNumber { get; set; }
        public string EnrollmentNumber { get; set; }
        public string PackageNameDuration { get; set; }
        public int? MoraPackageNumber { get; set; }
        public string HGOName { get; set; }
        public string StayDuration { get; set; }
        public string SubAgaintAppNumber { get; set; }
        public string SurName { get; set; }
        public string GivenName { get; set; }
        public string FatherHusbandName { get; set; }
        public string Email { get; set; }
        public string PassportNumber { get; set; }
        public string PassportDateOfExpiry { get; set; }
        public string DateOfBirth { get; set; } // dd/mm/yyyy
        public string CNIC { get; set; }
        public string BloodGroup { get; set; }
        public Gender? Gender { get; set; }
        public string Fiqah { get; set; }
        public string IsMarried { get; set; }
        public string PresentPostalAddress { get; set; }
        public string TehsilCode { get; set; }
        public string MobileNumber { get; set; }
        public string TelephoneNumber { get; set; }

        public string EmployerBusinessName { get; set; }
        public string Designation { get; set; }
        public string OfficialAddress { get; set; }
        public string OfficialContact { get; set; }
        public string OfficialEmail { get; set; }

        public string PerformedHajjInLast5Years { get; set; }
        public string YearsPerformedHajj { get; set; }
        public string WantToHajjEBadal { get; set; }
        public string WantToPerformHajjAsGroupWorker { get; set; }
        public string AreYouMehramOfALady { get; set; }
        public string NomineeName { get; set; }
        public string NomineeRelation { get; set; }
        public string NomineeCNIC { get; set; }
        public string NomineeTelephone { get; set; }
        public string NomineeContact { get; set; }

        public string EmergencyName { get; set; }
        public string EmergencyRelation { get; set; }
        public string EmergencyCellNumber { get; set; }
        public string MehramAppNumber { get; set; }
        public string MehramCode { get; set; }
        public string MehramRelationship { get; set; }
        public string MehramName { get; set; }

        public string HajjPackageDuesPkr { get; set; }
        public string HajjPackageDuesSar { get; set; }
        public string HajjPackageDuesUsd { get; set; }
        public string IndividualAdditionalFacilitiesDuesPkr { get; set; }
        public string IndividualAdditionalFacilitiesDuesSar { get; set; }
        public string IndividualAdditionalFacilitiesDuesUsd { get; set; }

        public string ExtraAdfDuesPkr { get; set; }
        public string ExtraAdfDuesSar { get; set; }
        public string ExtraAdfDuesUsd { get; set; }
        public string DiscountPkr { get; set; }
        public string DiscountSar { get; set; }
        public string DiscountUsd { get; set; }
        public string TotalPkr { get; set; }
        public string TotalSar { get; set; }
        public string TotalUsd { get; set; }
        public string MedinahArrival { get; set; }
        public string FlightType { get; set; }
        public string FlightClass { get; set; }
        public string TicketIncluded { get; set; }
        public string TicketPeriod { get; set; }
        public float? PackageTicketPricePkr { get; set; }
        public float? PackageTicketPriceSar { get; set; }
        public float? PackageTicketPriceUsd { get; set; }

        public float? FlightTicketDifferencesPkr { get; set; }
        public float? FlightTicketDifferencesSar { get; set; }
        public float? FlightTicketDifferencesUsd { get; set; }
        public string POD { get; set; }

        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Airline { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public DateTime? DepartureDateTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }

        public string TrainOrigin { get; set; }
        public string TrainDestination { get; set; }
        public string TrainClass { get; set; }
        public DateTime? TrainArrivalDateTime { get; set; }
        public DateTime? TrainDepartureDateTime { get; set; }

        public DateTime? TrainArrivalTime { get; set; }
        public DateTime? TrainDepartureTime { get; set; }

        public string TravelRoutes { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string VehicleName { get; set; }

        public List<DataFormTrainingSession> TrainingSessions { get; set; } = new List<DataFormTrainingSession>();

        public string IntercityTransport { get; set; }
        public string WithinCity { get; set; }
        public string MashairTrasport { get; set; }
        public string AirportTransport { get; set; }

        public List<DataFormIndividualFacility> IndividualFacilities { get; set; } = new List<DataFormIndividualFacility>();
        public List<DataFormGift> Gifts { get; set; } = new List<DataFormGift>();
        public List<DataFormExtraFacility> ExtraFacilities { get; set; } = new List<DataFormExtraFacility>();

        public DataFormAzizia AziziaAccomodation { get; set; }
        public DataFormAccomodation MakkahAccomodation { get; set; }
        public DataFormAccomodation SecondMakkahAccomodation { get; set; }
        public DataFormAccomodation MedinaAccomodation { get; set; }
        public DataFormMina MinaAccomodation { get; set; }

        public List<string> Remarks { get; set; }
    }
}

using TMG.Controllers;
using TMG.Models;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.AirlineModels;
using TMG.Models.CompanyModels;
using TMG.Models.EHajjModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.InvoiceModels;
using TMG.Models.LogisticModels;
using TMG.Models.MemberModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using TMG.Models.QurbaniModels;
using TMG.Models.RailwayModels;
using TMG.Models.ReservationModels;
using TMG.Models.SettingsModels;
using Microsoft.EntityFrameworkCore;
using TMG.Models.UserModels;
using TMGHajj.Models.PackageModels;
using TMG.Models.ReportModels;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TMG.Models.POModels;
using TMG.Models.Updated_Accounts;
using TMG.Models.ClientModels;
using TMG.Models.TransportationBlock;
using TMG.Models.TransportationModels;
using TMG.Models.NewChartOfAccounts.Currencies;
using TMG.Models.NewChartOfAccounts.FiscalYear;
using TMG.Models.NewChartOfAccounts.NewAccount;
using TMG.Models.NewChartOfAccounts.NewVoucher;

namespace TMG.Data
{


    public class TMGDbContext : DbContext
    {
        private readonly IHttpContextAccessor _http;

        public TMGDbContext(DbContextOptions<TMGDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _http = httpContextAccessor;
        }

        public DbSet<UserHajjAgreement> UserHajjAgreements { get; set; }
        public DbSet<CombinedFlightsResult> CombinedFlightsResult { get; set; }
        public DbSet<CombinedTrainsResult> CombinedTrainsResult { get; set; }
        public DbSet<CombinedFlightSummaryDetail> CombinedFlightSummaryDetail { get; set; }
        public DbSet<LedgerReportDto> LedgerReportDto { get; set; }
        public DbSet<BalanceCFDto> BalanceCFDto { get; set; }
        public DbSet<GiveAway> GiveAways{ get; set; }
        public DbSet <CreditCard> CreditCards { get; set; }


        #region Updated Accounts

        public DbSet<JV> JVs { get; set; }
        public DbSet<JVAccounts> JVAccounts { get; set; }
        public DbSet<JVNewPayment> JVNewPayments { get; set; }
        public DbSet<AccountTypes> AccountTypes { get; set; }
        public DbSet<AccountGroups> AccountGroups { get; set; }
        public DbSet<Accounts> Account { get; set; }
        public DbSet<GeneralLedger> GeneralLedgers { get; set; }
        public DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        public DbSet<NewPaymentVoucher> NewPaymentVouchers { get; set; }
        public DbSet<NewPayment> NewPayments { get; set; }
        public DbSet<NewTax> NewTaxes { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<CityTax> CityTaxes { get; set; }
        public DbSet<POTicket> POTickets { get; set; }
        public DbSet<POFlightDetail> POFlightDetails { get; set; }
        public DbSet<POTicketInvoices> POTicketInvoices { get; set; }
        public DbSet<POPayment> POPayments { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<POTicketFlightData> POTicketFlightDatas  { get; set; }
        public DbSet<POHotels> POHotels  { get; set; }
        public DbSet<POHotelRooms> POHotelRooms  { get; set; }
        public DbSet<RoomDetails> RoomDetails  { get; set; }
        public DbSet<ItineraryBlock> ItineraryBlocks { get; set; }
        public DbSet<ItineraryBlockGroup> ItineraryBlockGroups { get; set; }
        public DbSet<ItineraryBlockGroupMmebers> ItineraryBlockGroupMmebers { get; set; }
        public DbSet<ItineraryBlockReservationForAzizia> ItineraryBlockReservationForAzizias { get; set; }
        public DbSet<ItineraryBlockReservationForMadinah> ItineraryBlockReservationForMadinah { get; set; }
        public DbSet<ItineraryBlockReservationForMakkah> ItineraryBlockReservationForMakkah { get; set; }
        public DbSet<ItineraryBlockReservationForMinah> ItineraryBlockReservationForMinah { get; set; }


        #endregion

        #region ResultGroup
        public DbSet<QueryResultGroup> QueryResultGroups { get; set; }
        #endregion

        #region Settings Entities
        public DbSet<ApplicationSettings> ApplicationSettings { get; set; }
        #endregion

        #region User Module Entites
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePrivileges> RolePrivileges { get; set; }
        #endregion

        #region Company 
        public DbSet<Company> Companies { get; set; }
        public DbSet<QuotaCompany> QuotaCompanies { get; set; }
        public DbSet<HajjSeasons> HajjSeasons { get; set; }
        public DbSet<CompanyAddressType> CompanyAddressTypes { get; set; }
        public DbSet<CompanyContactType> CompanyContactTypes { get; set; }
        public DbSet<CompanyEmailType> CompanyEmailTypes { get; set; }
        public DbSet<CompanyLicenseType> CompanyLicenseTypes { get; set; }
        public DbSet<CompanyAddresses> CompanyAddresses { get; set; }
        public DbSet<CompanyContacts> CompanyContacts { get; set; }
        public DbSet<CompanyEmails> CompanyEmails { get; set; }
        public DbSet<CompanyLicenses> CompanyLicenses { get; set; }
        public DbSet<UserCompanies> UserCompanies { get; set; }
        public DbSet<Munazzim> Munazzims { get; set; }
        public DbSet<MunazzimCompanies> MunazzimCompanies { get; set; }
        public DbSet<CongratulationLetter> CongratulationLetters { get; set; }
        public DbSet<CongratulationLetterPackages> CongratulationLetterPackages { get; set; }
        public DbSet<CompanyDepartment> CompanyDepartments { get; set; }
        public DbSet<DepartmentFiling> DepartmentFilings { get; set; }
        #endregion

        #region Logistics Module Entites
        public DbSet<Models.LogisticModels.Route> Routes { get; set; }
        //public DbSet<RouteVehiclePrice> RouteVehiclePrices { get; set; }
        public DbSet<TravelRoute> TravelRoutes { get; set; }
        public DbSet<TravelRouteStops> TravelRouteStops { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<FamilyHeadItineraryVehicles> FamilyHeadItineraryVehicles { get; set; }

        #endregion

        #region Accomodation Module Entities
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Accomodation> Accomodations { get; set; }
        public DbSet<AccomodationCategory> AccomodationCategories { get; set; }
        public DbSet<FoodPackage> FoodPackages { get; set; }

        #endregion

        #region Demand Module Entities
        public DbSet<RoomStock> RoomStocks { get; set; }
        public DbSet<RoomStockRoomTypes> RoomStockRoomTypes { get; set; }
        public DbSet<RoomStockPackages> RoomStockPackages { get; set; }

        #endregion

        #region Client Module Entites
        public DbSet<ApplicantTypes> ApplicantTypes { get; set; }
        public DbSet<ClientData> ClientDatas { get; set; }
        public DbSet<ClientDataEmail> ClientDataEmails { get; set; }
        public DbSet<ClientDataAddress> ClientDataAddresses { get; set; }
        public DbSet<ClientDataContact> ClientDataContacts { get; set; }
        public DbSet<ClientDataMembers> ClientDataMembers { get; set; }
        public DbSet<ClientDataDocuments> ClientDataDocuments { get; set; }
        #endregion

        #region Misc Module Entites
        public DbSet<InquiryEmail> InquiryEmails { get; set; }
        public DbSet<EmailVendor> EmailVendors { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<IDCardList> IDCardLists { get; set; }
        public DbSet<ListFamily> ListFamilies { get; set; }
        public DbSet<EIslamic> EIslamics { get; set; }
        public DbSet<EMarketing> EMarketings { get; set; } 
        public DbSet<SubClasses> SubClasses { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LetterHead> LetterHeads { get; set; }
        public DbSet<SPOTaskManagement> SPOTaskManagements { get; set; }
        public DbSet<HajjDepartment> HajjDepartments { get; set; }
        public DbSet<Cordinator> Cordinators { get; set; }
        public DbSet<MediaDocument> MediaDocuments { get; set; }
        public DbSet<EVideoFiling> EVideoFilings { get; set; }
        public DbSet<CustomerRemarksLogs> CustomerRemarksLogs { get; set; }
        public DbSet<CustomerRemarks> CustomerRemarks { get; set; }
        public DbSet<CustomerStatus> CustomerStatuses { get; set; }
        public DbSet<EmailData> EmailDatas { get; set; }
        public DbSet<CountryCode> CountryCodes { get; set; }
        public DbSet<PassportType> PassportTypes { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<BoardingStamp> BoardingStamps { get; set; }
        public DbSet<IssuingAgent> IssuingAgents { get; set; }
        public DbSet<MisStamp> MisStamps { get; set; }
        public DbSet<RoomView> RoomViews { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<IndividualFacilityDescription> IndividualFacilityDescriptions { get; set; }
        public DbSet<ExtraFacilityDescription> ExtraFacilityDescriptions { get; set; }
        public DbSet<HajjAgreement> HajjAgreements { get; set; }
        public DbSet<HajjAgreementPackages> HajjAgreementPackages { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<SPO> SPOs { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<Tehsil> Tehsils { get; set; }
        public DbSet<Honorifics> Honorifics { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Fiqah> Fiqahs { get; set; }
        public DbSet<NomineeRelation> NomineeRelations { get; set; }
        public DbSet<MehramRelation> MehramRelations { get; set; }
        public DbSet<PaymentNotes> PaymentNotes { get; set; }
        public DbSet<Reference> References { get; set; }
        public DbSet<FamilyHeadRelation> FamilyHeadRelations { get; set; }
        public DbSet<RequiredDocument> RequiredDocuments { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<QurbaniSlipTC> QurbaniSlipTCs { get; set; }
        public DbSet<QurbaniSlipNotes> QurbaniSlipNotes { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<BatchList> BatchLists { get; set; }
        public DbSet<RoomBedType> RoomBedTypes { get; set; }

        #endregion

        #region Reservation Entites
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<FloorRoom> FloorRoom { get; set; }
        public DbSet<RoomAllocation> RoomAllocations { get; set; }
        public DbSet<RoomAllocationMember> RoomAllocationMembers { get; set; }
        public DbSet<RoomAllotment> RoomAllotments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationRooms> ReservationRooms { get; set; }
        public DbSet<ReservationFamilyMembers> ReservationFamilyMembers { get; set; }

        #endregion

        #region Transportation Entites
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightPackages> FlightPackages { get; set; }
        public DbSet<PNR> PNRs { get; set; }
        public DbSet<PNRMembers> PNRMembers { get; set; }
        public DbSet<PNRPackage> PNRPackages { get; set; }
        public DbSet<PNRTrain> PNRTrains { get; set; }
        public DbSet<PNRTrainMembers> PNRTrainMembers { get; set; }
        public DbSet<PNRTrainPackage> PNRTrainPackages { get; set; }
        public DbSet<Mina> Minas { get; set; }
        public DbSet<MinaMember> MinaMembers { get; set; }
        public DbSet<MinaPackage> MinaPackages { get; set; }
        public DbSet<Sector> Sectors { get; set; }

        public DbSet<Railway> Railways { get; set; }
        public DbSet<RailwayPNR> RailwayPNRs { get; set; }
        public DbSet<RailwayPNRMember> RailwayPNRMembers { get; set; }
        public DbSet<RailwayTravelPackages> RailwayTravelPackages { get; set; }
        public DbSet<RailwayTravel> RailwayTravels { get; set; }
        #endregion

        #region Package Entities
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageAddons> PackageAddons { get; set; }
        public DbSet<PackageAccomodations> PackageAccomodtions { get; set; }
        public DbSet<PackageGifts> PackageGifts { get; set; }
        public DbSet<PackagePaymentNotes> PackagePaymentNotes { get; set; }
        public DbSet<PackageTrainings> PackageTrainings { get; set; }
        public DbSet<AdditionalFacilities> AdditionalFacilities { get; set; }
        public DbSet<PackageCategoryFacilities> PackageCategoryFacilities { get; set; }
        public DbSet<PackageTerms> PackageTerms { get; set; }
        public DbSet<PackageTermsConditions> PackageTermsConditions { get; set; }
        public DbSet<PackageItinerary> PackageItinerary { get; set; }
        public DbSet<PackageFlights> PackageFlights { get; set; }
        public DbSet<PackageTrains> PackageTrains { get; set; }
        #endregion

        #region Member Entites
        public DbSet<MemberEmailType> MemberEmailTypes { get; set; }
        public DbSet<MemberContactType> MemberContactTypes { get; set; }
        public DbSet<MemberAddressType> MemberAddressTypes { get; set; }

        #endregion

        #region Family Entities
        public DbSet<OnlineInquiry> OnlineInquiries { get; set; }
        public DbSet<DataForm> DataForms { get; set; }
        public DbSet<DataFormMember> DataFormMembers { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<FamilyHeadGiveAways> FamilyHeadGiveAways { get; set; }
        public DbSet<FamilyHeadAccomodation> FamilyHeadAccomodations { get; set; }
        public DbSet<FamilyHead> FamilyHeads { get; set; }
        public DbSet<FamilyHeadFFNumber> FamilyHeadFFNumbers { get; set; }
        public DbSet<CustomerPortfolio> CustomerPortfolios { get; set; }
        public DbSet<CustomerPortfolioMembers> CustomerPortfoliosMembers { get; set; }
        public DbSet<CustomerPortfolioAddress> CustomerPortfolioAddress { get; set; }
        public DbSet<CustomerPortfolioContact> CustomerPortfolioContacts { get; set; }
        public DbSet<CustomerPortfolioEmail> CustomerPortfolioEmails { get; set; }
        public DbSet<CustomerPortfolioDocuments> CustomerPortfolioDocuments { get; set; }
        public DbSet<FamilyHeadFlight> FamilyHeadFlight { get; set; }
        public DbSet<FamilyHeadTrain> FamilyHeadTrain { get; set; }
        public DbSet<FamilyHeadAddress> FamilyHeadAddresses { get; set; }
        public DbSet<FamilyHeadContact> FamilyHeadContacts { get; set; }
        public DbSet<FamilyHeadEmail> FamilyHeadEmails { get; set; }
        public DbSet<FamilyHeadExtra> FamilyHeadExtras { get; set; }
        public DbSet<FamilyHeadRemark> FamilyHeadRemarks { get; set; }
        public DbSet<FamilyHeadDocument> FamilyHeadDocuments { get; set; }
        public DbSet<FamilyHeadNominee> FamilyHeadNominees { get; set; }
        public DbSet<FamilyMembers> FamilyMembers { get; set; }
        public DbSet<FamilyHeadAdditionalFacilities> FamilyHeadAdditionalFacilities { get; set; }
        public DbSet<FamilyHeadTransport> FamilyHeadTransports { get; set; }
        #endregion

        #region Group Entities
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupPackages> GroupPackages { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<SubGroup> SubGroups { get; set; }
        public DbSet<SubGroupMember> SubGroupMembers { get; set; }
        public DbSet<SubGroupRooms> SubGroupRooms { get; set; }
        public DbSet<SubGroupMemberRooms> SubGroupMemberRooms { get; set; }

        public DbSet<CoordinatorRole> CoordinatorRoles { get; set; }
        public DbSet<Coordinator> Coordinators { get; set; }

        #endregion

        #region Account Entities

        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<ExpenseSubType> ExpenseSubTypes { get; set; }
        public DbSet<ExpenseContactPerson> ExpenseContactPersons { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<VendorType> VendorTypes { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorContactPerson> VendorContactPersons { get; set; }
        public DbSet<BankAccountContactPerson> BankAccountContactPersons { get; set; }
        public DbSet<POVisa> POVisas { get; set; }
        public DbSet<POVisaInstallment> POVisaInstallments { get; set; }

        public DbSet<Models.AccountModels.Type> Types { get; set; }
        public DbSet<SubType> SubTypes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Currency> Currencies { get; set; }

        public DbSet<CompanyBankAccount> CompanyBankAccounts { get; set; }

        public DbSet<VoucherType> VoucherTypes { get; set; }
        public DbSet<PaymentVoucher> PaymentVouchers { get; set; }
        public DbSet<VoucherEntry> VoucherEntries { get; set; }

        public DbSet<RateOfExchange> RateOfExchanges { get; set; }
        public DbSet<RoeRequest> RoeRequests { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<AuthorizationAgent> AuthorizationAgents { get; set; }
        public DbSet<NewInvoice> NewInvoice { get; set; }



        #endregion

        #region Qurbani Entities

        public DbSet<Qurbani> Qurbani { get; set; }
        public DbSet<QurbaniQueue> QurbaniQueue { get; set; }
        public DbSet<QurbaniBatch> QurbaniBatches { get; set; }

        #endregion
        public DbSet<Suitcase> Suitcases { get; set; }
        public DbSet<SuitcasePackages> SuitcasePackages { get; set; }
        public DbSet<Models.Module> Modules { get; set; }
        public DbSet<SubModule> SubModules { get; set; }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<CompanyVisas> CompanyVisas { get; set; }
        public DbSet<CompanyVisaContacts> CompanyVisaContacts { get; set; }
        public DbSet<TransportationBlocks> TransportationBlocks { get; set; }
        public DbSet<TransportationBlockMembers> TransportationBlockMembers { get; set; }
        public DbSet<TranspotationBlockGroups> TranspotationBlockGroups { get; set; }
        public DbSet<TranspportationBlockReservation> TranspportationBlockReservations { get; set; }

        public DbSet<CurrencyType> CurrencyTypes { get; set; } 
        public DbSet<FiscalYears> FiscalYears { get; set; } 
        public DbSet<FiscalPeriods> FiscalPeriods { get; set; } 
        public DbSet<NewAccountGroup> NewAccountGroups { get; set; } 
        public DbSet<NewAccount> NewAccounts { get; set; } 
        public DbSet<NewVoucherTypes> NewVoucherTypes { get; set; } 

        #region Without Trash
        public IQueryable<BoardingStamp> BoardingStampsWithoutTrash => BoardingStamps.Where(e => !e.Trashed);
        public IQueryable<EMarketing> EMarketingsWithoutTrash => EMarketings.Where(e => !e.Trashed);
        public IQueryable<PackageTerms> PackageTermsWithoutTrash => PackageTerms.Where(e => !e.Trashed);
        public IQueryable<MisStamp> MisStampsWithoutTrash => MisStamps.Where(e => !e.Trashed);
        public IQueryable<RoomView> RoomViewsWithoutTrash => RoomViews.Where(e => !e.Trashed);
        public IQueryable<ExtraFacilityDescription> ExtraFacilityDescriptionsWithoutTrash => ExtraFacilityDescriptions.Where(e => !e.Trashed);
        public IQueryable<RoomStock> RoomStocksWithoutTrash => RoomStocks.Where(e => !e.Trashed);
        public IQueryable<RailwayPNR> RailwayPNRsWithoutTrash => RailwayPNRs.Where(e => !e.Trashed);
        public IQueryable<Railway> RailwaysWithoutTrash => Railways.Where(e => !e.Trashed);
        public IQueryable<RailwayTravel> RailwayTravelsWithoutTrash => RailwayTravels.Where(e => !e.Trashed);
        public IQueryable<Flight> FlightsWithoutTrash => Flights.Where(e => !e.Trashed);
        public IQueryable<PNR> PNRsWithoutTrash => PNRs.Where(e => !e.Trashed);
        public IQueryable<PNRTrain> PNRTrainsWithoutTrash => PNRTrains.Where(e => !e.Trashed);
        public IQueryable<Mina> MinasWithoutTrash => Minas.Where(e => !e.Trashed);
        public IQueryable<Doctor> DoctorsWithoutTrash => Doctors.Where(e => !e.Trashed);
        public IQueryable<IndividualFacilityDescription> IndividualFacilityDescriptionsWithoutTrash => IndividualFacilityDescriptions.Where(e => !e.Trashed);
        public IQueryable<HajjAgreement> HajjAgreementsWithoutTrash => HajjAgreements.Where(e => !e.Trashed);
        public IQueryable<CongratulationLetter> CongratulationLettersWithoutTrash => CongratulationLetters.Where(e => !e.Trashed);
        public IQueryable<RoomAllotment> RoomAllotmentsWithoutTrash => RoomAllotments.Where(e => !e.Trashed);
        public IQueryable<RoomBedType> RoomBedTypesWithoutTrash => RoomBedTypes.Where(e => !e.Trashed);
        public IQueryable<SPO> SPOsWithoutTrash => SPOs.Where(e => !e.Trashed);
        public IQueryable<User> UsersWithoutTrash => Users.Where(e => !e.Trashed);
        public IQueryable<Package> PackagesWithoutTrash => Packages.Where(e => !e.Trashed);
        public IQueryable<Role> RolesWithoutTrash => Roles.Where(e => !e.Trashed);
        public IQueryable<Company> CompaniesWithoutTrash => Companies.Where(e => !e.Trashed);
        public IQueryable<Vehicle> VehiclesWithoutTrash => Vehicles.Where(e => !e.Trashed);
        public IQueryable<Room> RoomsWithoutTrash => Rooms.Where(e => !e.Trashed);
        public IQueryable<Place> PlacesWithoutTrash => Places.Where(e => !e.Trashed);
        public IQueryable<Accomodation> AccomodationsWithoutTrash => Accomodations.Where(e => !e.Trashed);
        public IQueryable<AccomodationCategory> AccomodationCategoriesWithoutTrash => AccomodationCategories.Where(e => !e.Trashed);
        public IQueryable<FoodPackage> FoodPackagesWithoutTrash => FoodPackages.Where(e => !e.Trashed);
        public IQueryable<Gift> GiftsWithoutTrash => Gifts.Where(e => !e.Trashed);
        public IQueryable<Training> TrainingsWithoutTrash => Trainings.Where(e => !e.Trashed);
        public IQueryable<Models.LogisticModels.Route> RoutesWithoutTrash => Routes.Where(e => !e.Trashed);
        public IQueryable<TravelRoute> TravelRoutesWithoutTrash => TravelRoutes.Where(e => !e.Trashed);
        public IQueryable<Tehsil> TehsilsWithoutTrash => Tehsils.Where(e => !e.Trashed);
        public IQueryable<Fiqah> FiqahsWithoutTrash => Fiqahs.Where(e => !e.Trashed);
        public IQueryable<Honorifics> HonorificsWithoutTrash => Honorifics.Where(e => !e.Trashed);
        public IQueryable<Bank> BanksWithoutTrash => Banks.Where(e => !e.Trashed);
        public IQueryable<NomineeRelation> NomineeRelationsWithoutTrash => NomineeRelations.Where(e => !e.Trashed);
        public IQueryable<MehramRelation> MehramRelationsWithoutTrash => MehramRelations.Where(e => !e.Trashed);
        public IQueryable<PaymentNotes> PaymentNotesWithoutTrash => PaymentNotes.Where(e => !e.Trashed);
        public IQueryable<Hotel> HotelsWithoutTrash => Hotels.Where(e => !e.Trashed);
        public IQueryable<Reference> ReferencesWithoutTrash => References.Where(e => !e.Trashed);
        public IQueryable<FamilyHeadRelation> FamilyHeadRelationsWithoutTrash => FamilyHeadRelations.Where(e => !e.Trashed);
        public IQueryable<RequiredDocument> RequiredDocumentsWithoutTrash => RequiredDocuments.Where(e => !e.Trashed);
        public IQueryable<Vaccine> VaccinesWithoutTrash => Vaccines.Where(e => !e.Trashed);
        public IQueryable<Airline> AirlinesWithoutTrash => Airlines.Where(e => !e.Trashed);

        public IQueryable<MemberAddressType> MemberAddressTypesWithoutTrash => MemberAddressTypes.Where(e => !e.Trashed);
        public IQueryable<MemberContactType> MemberContactTypesWithoutTrash => MemberContactTypes.Where(e => !e.Trashed);
        public IQueryable<MemberEmailType> MemberEmailTypesWithoutTrash => MemberEmailTypes.Where(e => !e.Trashed);
        public IQueryable<FamilyHead> FamilyHeadsWithoutTrash => FamilyHeads.Where(e => !e.Trashed);

        public IQueryable<Group> GroupsWithoutTrash => Groups.Where(e => !e.Trashed);
        public IQueryable<SubGroup> SubGroupsWithoutTrash => SubGroups.Where(e => !e.Trashed);
        public IQueryable<CoordinatorRole> CoordinatorRolesWithoutTrash => CoordinatorRoles.Where(e => !e.Trashed);
        public IQueryable<Coordinator> CoordinatorsWithoutTrash => Coordinators.Where(e => !e.Trashed);

        public IQueryable<CompanyBankAccount> CompanyBankAccountsWithoutTrash => CompanyBankAccounts.Where(e => !e.Trashed);

        public IQueryable<SubType> SubTypesWithoutTrash => SubTypes.Where(e => !e.Trashed);
        public IQueryable<Category> CategoriesWithoutTrash => Categories.Where(e => !e.Trashed);
        public IQueryable<Account> AccountsWithoutTrash => Accounts.Where(e => !e.Trashed);
        public IQueryable<PaymentVoucher> PaymentVouchersWithoutTrash => PaymentVouchers.Where(e => !e.Trashed);

        public IQueryable<RateOfExchange> RateOfExchangesWithoutTrash => RateOfExchanges.Where(e => !e.Trashed);
        public IQueryable<RoeRequest> RoeRequestsWithoutTrash => RoeRequests.Where(e => !e.Trashed);
        public IQueryable<Invoice> InvoicesWithoutTrash => Invoices.Where(e => !e.Trashed);
        public IQueryable<NewInvoice> NewInvoicesWithoutTrash => NewInvoice.Where(e => !e.Trashed);


        public IQueryable<QurbaniSlipNotes> QurbaniSlipNotesWithoutTrash => QurbaniSlipNotes.Where(e => !e.Trashed);
        public IQueryable<QurbaniSlipTC> QurbaniSlipTCsWithoutTrash => QurbaniSlipTCs.Where(e => !e.Trashed);

        public IQueryable<RoomAllocation> RoomAllocationsWithoutTrash => RoomAllocations.Where(e => !e.Trashed);
        public IQueryable<POHotels> POHotelsWithoutTrash => POHotels.Where(e => !e.Trashed);
        public IQueryable<POHotelRooms> POHotelRoomsWithoutTrash => POHotelRooms.Where(e => !e.Trashed);

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            #region Updated Accounts

            // Account Types

            modelBuilder.Entity<AccountTypes>()
                .HasData(
                    new AccountTypes { AccountTypeId = 1, Name = "Asset", CreatedAt = DateTime.Now, Trashed = false, AccountNature = AccountNature.Debit },
                    new AccountTypes { AccountTypeId = 2, Name = "Expense", CreatedAt = DateTime.Now, Trashed = false, AccountNature = AccountNature.Debit },
                    new AccountTypes { AccountTypeId = 3, Name = "Revenue", CreatedAt = DateTime.Now, Trashed = false, AccountNature = AccountNature.Credit },
                    new AccountTypes { AccountTypeId = 4, Name = "Liability", CreatedAt = DateTime.Now, Trashed = false, AccountNature = AccountNature.Credit },
                    new AccountTypes { AccountTypeId = 5, Name = "Capital", CreatedAt = DateTime.Now, Trashed = false, AccountNature = AccountNature.Credit }
                );

            // Account Groups

            modelBuilder.Entity<AccountGroups>()
                .HasData(
                    new AccountGroups { AccountGroupId = 1, Name = "Current Assets", AccountTypesId = 1, CreatedAt = DateTime.Now, Trashed = false },
                    new AccountGroups { AccountGroupId = 2, Name = "Non Current Assets", AccountTypesId = 1, CreatedAt = DateTime.Now, Trashed = false },
                    new AccountGroups { AccountGroupId = 3, Name = "Bank Expense", AccountTypesId = 2, CreatedAt = DateTime.Now, Trashed = false },
                    new AccountGroups { AccountGroupId = 4, Name = "Service Revenue", AccountTypesId = 3, CreatedAt = DateTime.Now, Trashed = false }
                    //new AccountGroups { AccountGroupId = 5, Name = "Taxes", AccountTypesId = 2, CreatedAt = DateTime.Now, Trashed = false },
                    //new AccountGroups { AccountGroupId = 6, Name = "Taxes", AccountTypesId = 4, CreatedAt = DateTime.Now, Trashed = false }
                );

            //modelBuilder.Entity<Accounts>()
            //    .HasData(
            //        new Accounts { AccountsId = 1, Name = "Sales Tax Payable", AccountGroupsId = 5, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 2, Name = "Sales Tax Expense", AccountGroupsId = 6, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 3, Name = "VAT Tax Payable", AccountGroupsId = 5, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 4, Name = "VAT Tax Expense", AccountGroupsId = 6, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 5, Name = "SRB Tax Payable", AccountGroupsId = 5, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 6, Name = "SRB Tax Expense", AccountGroupsId = 6, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 7, Name = "Income Tax Payable", AccountGroupsId = 5, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 8, Name = "Income Tax Expense", AccountGroupsId = 6, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        new Accounts { AccountsId = 9, Name = "SVC Tax", AccountGroupsId = 4, IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false }
            //        //new Accounts { AccountsId = 10, Name = "WTH Tax", AccountGroupsId = , IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false },
            //        //new Accounts { AccountsId = 11, Name = "WTH Tax", AccountGroupsId = , IsTax = true, Currencies = Models.Currencies.PKR, CreatedAt = DateTime.Now, Trashed = false }
            //    );

            #endregion

            modelBuilder.Entity<POVisa>()
                .HasQueryFilter(e => !e.Trashed);

            modelBuilder.Entity<ExpenseType>()
                .HasQueryFilter(e => !e.Trashed);

            modelBuilder.Entity<Expense>()
                .HasQueryFilter(e => !e.Trashed)
                .HasQueryFilter(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));


            modelBuilder.Entity<CombinedFlightsResult>()
                .HasNoKey();

            modelBuilder.Entity<CombinedTrainsResult>()
                .HasNoKey();

            modelBuilder.Entity<CombinedFlightSummaryDetail>()
                .HasNoKey();

            modelBuilder.Entity<LedgerReportDto>()
               .HasNoKey();

            modelBuilder.Entity<BalanceCFDto>()
               .HasNoKey();


            modelBuilder.Entity<ApplicationSettings>()
                .HasData(new ApplicationSettings
                {
                    ApplicationSettingsId = 1,
                    SARROE = 0,
                    USDROE = 0,
                    VaccineCount = 5,
                });

            modelBuilder.Entity<PackageAccomodations>()
                .Property(p => p.AccomodationCategoryId)
                .IsRequired(false);

            var dateConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.Date : null,
                v => v);

            var timeConverter = new ValueConverter<DateTime?, TimeSpan?>(
                v => v.HasValue ? v.Value.TimeOfDay : null,
                v => v.HasValue ? DateTime.Today.Add(v.Value) : null);

            modelBuilder.Entity<PackageFlights>(entity =>
            {
                entity.Property(e => e.DepartureDate).HasConversion(dateConverter);
                entity.Property(e => e.ArrivalDate).HasConversion(dateConverter);

                entity.Property(e => e.DepartureTime).HasConversion(timeConverter);
                entity.Property(e => e.ArrivalTime).HasConversion(timeConverter);
            });

            var trainDateConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.Date : null,
                v => v);

            var trainTimeConverter = new ValueConverter<DateTime?, TimeSpan?>(
                v => v.HasValue ? v.Value.TimeOfDay : null,
                v => v.HasValue ? DateTime.Today.Add(v.Value) : null);

            modelBuilder.Entity<PackageTrains>(entity =>
            {
                entity.Property(e => e.DepartureDate).HasConversion(trainDateConverter);
                entity.Property(e => e.ArrivalDate).HasConversion(trainDateConverter);

                entity.Property(e => e.DepartureTime).HasConversion(trainTimeConverter);
                entity.Property(e => e.ArrivalTime).HasConversion(trainTimeConverter);
            });


            var familyFlightDateConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.Date : null,
                v => v);

            var familyFlightTimeConverter = new ValueConverter<DateTime?, TimeSpan?>(
                v => v.HasValue ? v.Value.TimeOfDay : null,
                v => v.HasValue ? DateTime.Today.Add(v.Value) : null);

            modelBuilder.Entity<FamilyHeadFlight>(entity =>
            {
                entity.Property(e => e.DepartureDateTime).HasConversion(familyFlightDateConverter);
                entity.Property(e => e.ArrivalDateTime).HasConversion(familyFlightDateConverter);

                entity.Property(e => e.DepartureTime).HasConversion(familyFlightTimeConverter);
                entity.Property(e => e.ArrivalTime).HasConversion(familyFlightTimeConverter);
            });


            var familyTrainDateConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.Date : null,
                v => v);

            var familyTrainTimeConverter = new ValueConverter<DateTime?, TimeSpan?>(
                v => v.HasValue ? v.Value.TimeOfDay : null,
                v => v.HasValue ? DateTime.Today.Add(v.Value) : null);

            modelBuilder.Entity<FamilyHeadTrain>(entity =>
            {
                entity.Property(e => e.DepartureDateTime).HasConversion(familyTrainDateConverter);
                entity.Property(e => e.ArrivalDateTime).HasConversion(familyTrainDateConverter);

                entity.Property(e => e.DepartureTime).HasConversion(familyFlightTimeConverter);
                entity.Property(e => e.ArrivalTime).HasConversion(familyTrainTimeConverter);
            });


            modelBuilder.Entity<GroupMember>()
                    .HasKey(e => new { e.FamilyHeadId, e.GroupId });

            modelBuilder.Entity<ItineraryBlockGroupMmebers>()
                    .HasKey(e => new { e.FamilyHeadId, e.ItineraryBlockId });

            modelBuilder.Entity<TransportationBlockMembers>()
                    .HasKey(e => new { e.FamilyHeadId, e.TransportationBlocksId });

            modelBuilder.Entity<SubGroupMember>()
                .HasKey(e => new { e.FamilyHeadId, e.SubGroupId });

            modelBuilder.Entity<PackageGifts>()
                .HasKey(e => new { e.PackageId, e.GiftId });

            modelBuilder.Entity<PackagePaymentNotes>()
                .HasKey(e => new { e.PackageId, e.PaymentNotesId });

            modelBuilder.Entity<RoleModules>()
                .HasKey(ur => new { ur.RoleId, ur.ModuleId });

            modelBuilder.Entity<RolePermissions>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });


            modelBuilder.Entity<Models.AccountModels.Type>()
                .HasData(
                    new Models.AccountModels.Type { TypeId = 1, Name = "Asset", AccountNature = AccountNature.Debit },
                    new Models.AccountModels.Type { TypeId = 2, Name = "Expense", AccountNature = AccountNature.Debit },
                    new Models.AccountModels.Type { TypeId = 3, Name = "Revenue", AccountNature = AccountNature.Credit },
                    new Models.AccountModels.Type { TypeId = 4, Name = "Liability", AccountNature = AccountNature.Credit },
                    new Models.AccountModels.Type { TypeId = 5, Name = "Capital", AccountNature = AccountNature.Credit }
                );

            modelBuilder.Entity<VoucherType>()
                .HasData(
                    new VoucherType { VoucherTypeId = 1, Type = "Bank Payment Voucher", Code = "BPV" },
                    new VoucherType { VoucherTypeId = 2, Type = "Bank Receive Voucher", Code = "BRV" },
                    new VoucherType { VoucherTypeId = 3, Type = "Cash Payment Voucher", Code = "CPV" },
                    new VoucherType { VoucherTypeId = 4, Type = "Cash Receive Voucher", Code = "CRV" },
                    new VoucherType { VoucherTypeId = 5, Type = "Journal Voucher", Code = "JV" }
                );

            // Seeding
            User.GenerateHash("12345678", out byte[] hash, out byte[] salt);

            modelBuilder.Entity<User>()
                .HasData(new User
                {
                    UserId = 1,
                    Username = "Admin",
                    Email = "admin@tmg.com",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    RoleId = 1
                });

            modelBuilder.Entity<Role>()
                .HasData(new Role
                {
                    RoleId = 1,
                    RoleName = "Admin",
                    UserModuleAllowed = true,
                    CompanyModuleAllowed = true,
                    AccommodationModuleAllowed = true,
                    PackageModuleAllowed = true,
                    MiscModuleAllowed = true,
                    AccountsModuleAllowed = true,
                    TransportationModuleAllowed = true,
                    FamilyModuleAllowed = true,
                    GroupsModuleAllowed = true,
                    ReceiptModuleAllowed = true,
                    QurbaniModuleAllowed = true,
                    ReservationModuleAllowed = true,
                    DemandModuleAllowed = true,
                    PurchaseOrderModuleAllowed = true,
                });


            modelBuilder.Entity<Models.Module>()
                .HasData(
                    new Models.Module { ModuleId = 1, ModuleName = "User Module" },
                    new Models.Module { ModuleId = 2, ModuleName = "Company Module" },
                    new Models.Module { ModuleId = 3, ModuleName = "Accounts Module" },
                    new Models.Module { ModuleId = 4, ModuleName = "Package Module" },
                    new Models.Module { ModuleId = 5, ModuleName = "Airline Module" },
                    new Models.Module { ModuleId = 6, ModuleName = "Misc Modules" },
                    new Models.Module { ModuleId = 7, ModuleName = "Registration Modules" },
                    new Models.Module { ModuleId = 8, ModuleName = "Member Module" },
                    new Models.Module { ModuleId = 9, ModuleName = "Family Module" },
                    new Models.Module { ModuleId = 10, ModuleName = "Reports Module" },
                    new Models.Module { ModuleId = 11, ModuleName = "Qurbani Module" },
                    new Models.Module { ModuleId = 12, ModuleName = "Groups Module" },
                    new Models.Module { ModuleId = 13, ModuleName = "Reservation Module" },
                    new Models.Module { ModuleId = 14, ModuleName = "Receipt Module" },
                    new Models.Module { ModuleId = 15, ModuleName = "Logistics Module" },
                    new Models.Module { ModuleId = 16, ModuleName = "Accomodation Module" },
                    new Models.Module { ModuleId = 17, ModuleName = "Purchase Order Module" }
                );


            modelBuilder.Entity<SubModule>()
                .HasData(
                    new SubModule { SubModuleId = 1, SubModuleName = "User", ModuleId = 1 },
                    new SubModule { SubModuleId = 2, SubModuleName = "Roles", ModuleId = 1 },

                    new SubModule { SubModuleId = 3, SubModuleName = "Company", ModuleId = 2 },
                    new SubModule { SubModuleId = 4, SubModuleName = "Company - Company Address", ModuleId = 2 },
                    new SubModule { SubModuleId = 5, SubModuleName = "Company - Company Contact Type", ModuleId = 2 },
                    new SubModule { SubModuleId = 6, SubModuleName = "Company - Company Email Type", ModuleId = 2 },
                    new SubModule { SubModuleId = 7, SubModuleName = "Company - Company License Type", ModuleId = 2 },
                    new SubModule { SubModuleId = 8, SubModuleName = "Company - Staff Members", ModuleId = 2 },
                    new SubModule { SubModuleId = 9, SubModuleName = "Company - Designation", ModuleId = 2 },

                    new SubModule { SubModuleId = 10, SubModuleName = "Vehicle", ModuleId = 15 },
                    new SubModule { SubModuleId = 11, SubModuleName = "Route", ModuleId = 15 },
                    new SubModule { SubModuleId = 12, SubModuleName = "Travel Routes", ModuleId = 15 },
                    new SubModule { SubModuleId = 13, SubModuleName = "Reports", ModuleId = 15 },

                    new SubModule { SubModuleId = 14, SubModuleName = "Rooms", ModuleId = 16 },
                    new SubModule { SubModuleId = 15, SubModuleName = "Places", ModuleId = 16 },
                    new SubModule { SubModuleId = 16, SubModuleName = "Accomodation", ModuleId = 16 },
                    new SubModule { SubModuleId = 17, SubModuleName = "Accomodation Category", ModuleId = 16 },
                    new SubModule { SubModuleId = 18, SubModuleName = "Food Package", ModuleId = 16 },

                    new SubModule { SubModuleId = 19, SubModuleName = "Gift", ModuleId = 6 },
                    new SubModule { SubModuleId = 20, SubModuleName = "Training Session", ModuleId = 6 },
                    new SubModule { SubModuleId = 21, SubModuleName = "Bank", ModuleId = 6 },
                    new SubModule { SubModuleId = 22, SubModuleName = "Honorifics", ModuleId = 6 },
                    new SubModule { SubModuleId = 23, SubModuleName = "Fiqahs", ModuleId = 6 },
                    new SubModule { SubModuleId = 24, SubModuleName = "Tehsils", ModuleId = 6 },
                    new SubModule { SubModuleId = 25, SubModuleName = "References", ModuleId = 6 },
                    new SubModule { SubModuleId = 26, SubModuleName = "Family Heads", ModuleId = 6 },
                    new SubModule { SubModuleId = 27, SubModuleName = "Mehram Relation", ModuleId = 6 },
                    new SubModule { SubModuleId = 28, SubModuleName = "Nominee Relation", ModuleId = 6 },
                    new SubModule { SubModuleId = 29, SubModuleName = "Required Documents", ModuleId = 6 },
                    new SubModule { SubModuleId = 30, SubModuleName = "Payment Notes", ModuleId = 6 },
                    new SubModule { SubModuleId = 31, SubModuleName = "Vaccine", ModuleId = 6 },
                    new SubModule { SubModuleId = 32, SubModuleName = "Quotation Payment Policy", ModuleId = 6 },
                    new SubModule { SubModuleId = 33, SubModuleName = "Quotation TC", ModuleId = 6 },

                    new SubModule { SubModuleId = 34, SubModuleName = "Types", ModuleId = 8 },
                    new SubModule { SubModuleId = 35, SubModuleName = "Address", ModuleId = 8 },
                    new SubModule { SubModuleId = 36, SubModuleName = "Contact", ModuleId = 8 },
                    new SubModule { SubModuleId = 37, SubModuleName = "Email", ModuleId = 8 },

                    new SubModule { SubModuleId = 38, SubModuleName = "Families", ModuleId = 8 },

                    new SubModule { SubModuleId = 39, SubModuleName = "Coord Roles", ModuleId = 12 },
                    new SubModule { SubModuleId = 40, SubModuleName = "Coord", ModuleId = 12 },
                    new SubModule { SubModuleId = 41, SubModuleName = "Groups", ModuleId = 12 },
                    new SubModule { SubModuleId = 42, SubModuleName = "Sub Group", ModuleId = 12 },
                    new SubModule { SubModuleId = 43, SubModuleName = "Airline Sub Group", ModuleId = 12 },

                    new SubModule { SubModuleId = 44, SubModuleName = "Packages", ModuleId = 4 },
                    new SubModule { SubModuleId = 45, SubModuleName = "Quotation Packages", ModuleId = 4 },

                    new SubModule { SubModuleId = 46, SubModuleName = "Member Types", ModuleId = 8 },
                    new SubModule { SubModuleId = 47, SubModuleName = "Member Address", ModuleId = 8 },
                    new SubModule { SubModuleId = 48, SubModuleName = "Member Contact", ModuleId = 8 },
                    new SubModule { SubModuleId = 49, SubModuleName = "Member Email", ModuleId = 8 },


                    new SubModule { SubModuleId = 50, SubModuleName = "Airlines", ModuleId = 5 },
                    new SubModule { SubModuleId = 51, SubModuleName = "Airline Class", ModuleId = 5 },
                    new SubModule { SubModuleId = 52, SubModuleName = "Airline Fare", ModuleId = 5 },

                    new SubModule { SubModuleId = 53, SubModuleName = "Families", ModuleId = 9 },

                    new SubModule { SubModuleId = 54, SubModuleName = "Qurbani Types", ModuleId = 11 },

                    new SubModule { SubModuleId = 55, SubModuleName = "Records Per Page", ModuleId = 6 },

                    new SubModule { SubModuleId = 56, SubModuleName = "Receipts List", ModuleId = 14 },
                    new SubModule { SubModuleId = 57, SubModuleName = "Client Statement", ModuleId = 14 },
                    new SubModule { SubModuleId = 58, SubModuleName = "Mini Statement", ModuleId = 14 },

                    new SubModule { SubModuleId = 59, SubModuleName = "Hotels", ModuleId = 13 },
                    new SubModule { SubModuleId = 60, SubModuleName = "Room Allotments", ModuleId = 13 },
                    new SubModule { SubModuleId = 61, SubModuleName = "Hotel Room Summary", ModuleId = 13 },
                    new SubModule { SubModuleId = 62, SubModuleName = "Daily Hotel Room Summary", ModuleId = 13 },
                    new SubModule { SubModuleId = 63, SubModuleName = "E Hajj Contracts", ModuleId = 13 },
                    new SubModule { SubModuleId = 64, SubModuleName = "POTicket", ModuleId = 17 }


                );

            modelBuilder.Entity<Permission>()
                .HasData(
                // Users
                new Permission { PermissionId = 1, PermissionName = "List", SubModuleId = 1, ModuleId = 1 },
                new Permission { PermissionId = 2, PermissionName = "Create", SubModuleId = 1, ModuleId = 1 },
                new Permission { PermissionId = 3, PermissionName = "Edit", SubModuleId = 1, ModuleId = 1 },
                new Permission { PermissionId = 4, PermissionName = "Trash", SubModuleId = 1, ModuleId = 1 },
                new Permission { PermissionId = 5, PermissionName = "Restore", SubModuleId = 1, ModuleId = 1 },

                // Roles
                new Permission { PermissionId = 6, PermissionName = "List", SubModuleId = 2, ModuleId = 1 },
                new Permission { PermissionId = 7, PermissionName = "Create", SubModuleId = 2, ModuleId = 1 },
                new Permission { PermissionId = 8, PermissionName = "Edit", SubModuleId = 2, ModuleId = 1 },
                new Permission { PermissionId = 9, PermissionName = "Trash", SubModuleId = 2, ModuleId = 1 },
                new Permission { PermissionId = 10, PermissionName = "Restore", SubModuleId = 2, ModuleId = 1 },
                new Permission { PermissionId = 11, PermissionName = "Set Privileges", SubModuleId = 2, ModuleId = 1 },

                // Company
                new Permission { PermissionId = 12, PermissionName = "List", SubModuleId = 3, ModuleId = 2 },
                new Permission { PermissionId = 13, PermissionName = "Create", SubModuleId = 3, ModuleId = 2 },
                new Permission { PermissionId = 14, PermissionName = "Edit", SubModuleId = 3, ModuleId = 2 },
                new Permission { PermissionId = 15, PermissionName = "Trash", SubModuleId = 3, ModuleId = 2 },
                new Permission { PermissionId = 16, PermissionName = "Restore", SubModuleId = 3, ModuleId = 2 },
                new Permission { PermissionId = 17, PermissionName = "Individual Reports", SubModuleId = 3, ModuleId = 2 },
                new Permission { PermissionId = 18, PermissionName = "Combine Reports", SubModuleId = 3, ModuleId = 2 },

                // Address
                new Permission { PermissionId = 19, PermissionName = "List", SubModuleId = 4, ModuleId = 2 },
                new Permission { PermissionId = 20, PermissionName = "Create", SubModuleId = 4, ModuleId = 2 },
                new Permission { PermissionId = 21, PermissionName = "Edit", SubModuleId = 4, ModuleId = 2 },
                new Permission { PermissionId = 22, PermissionName = "Trash", SubModuleId = 4, ModuleId = 2 },
                new Permission { PermissionId = 23, PermissionName = "Restore", SubModuleId = 4, ModuleId = 2 },

                // Contact
                new Permission { PermissionId = 24, PermissionName = "List", SubModuleId = 5, ModuleId = 2 },
                new Permission { PermissionId = 25, PermissionName = "Create", SubModuleId = 5, ModuleId = 2 },
                new Permission { PermissionId = 26, PermissionName = "Edit", SubModuleId = 5, ModuleId = 2 },
                new Permission { PermissionId = 27, PermissionName = "Trash", SubModuleId = 5, ModuleId = 2 },
                new Permission { PermissionId = 28, PermissionName = "Restore", SubModuleId = 5, ModuleId = 2 },

                // Email
                new Permission { PermissionId = 29, PermissionName = "List", SubModuleId = 6, ModuleId = 2 },
                new Permission { PermissionId = 30, PermissionName = "Create", SubModuleId = 6, ModuleId = 2 },
                new Permission { PermissionId = 31, PermissionName = "Edit", SubModuleId = 6, ModuleId = 2 },
                new Permission { PermissionId = 32, PermissionName = "Trash", SubModuleId = 6, ModuleId = 2 },
                new Permission { PermissionId = 33, PermissionName = "Restore", SubModuleId = 6, ModuleId = 2 },

                // Lisence
                new Permission { PermissionId = 34, PermissionName = "List", SubModuleId = 7, ModuleId = 2 },
                new Permission { PermissionId = 35, PermissionName = "Create", SubModuleId = 7, ModuleId = 2 },
                new Permission { PermissionId = 36, PermissionName = "Edit", SubModuleId = 7, ModuleId = 2 },
                new Permission { PermissionId = 37, PermissionName = "Trash", SubModuleId = 7, ModuleId = 2 },
                new Permission { PermissionId = 38, PermissionName = "Restore", SubModuleId = 7, ModuleId = 2 },

                // Staff
                new Permission { PermissionId = 39, PermissionName = "List", SubModuleId = 8, ModuleId = 2 },
                new Permission { PermissionId = 40, PermissionName = "Create", SubModuleId = 8, ModuleId = 2 },
                new Permission { PermissionId = 41, PermissionName = "Edit", SubModuleId = 8, ModuleId = 2 },
                new Permission { PermissionId = 42, PermissionName = "Trash", SubModuleId = 8, ModuleId = 2 },
                new Permission { PermissionId = 43, PermissionName = "Restore", SubModuleId = 8, ModuleId = 2 },

                // Designation
                new Permission { PermissionId = 44, PermissionName = "List", SubModuleId = 9, ModuleId = 2 },
                new Permission { PermissionId = 45, PermissionName = "Create", SubModuleId = 9, ModuleId = 2 },
                new Permission { PermissionId = 46, PermissionName = "Edit", SubModuleId = 9, ModuleId = 2 },
                new Permission { PermissionId = 47, PermissionName = "Trash", SubModuleId = 9, ModuleId = 2 },
                new Permission { PermissionId = 48, PermissionName = "Restore", SubModuleId = 9, ModuleId = 2 },

                // Vehicle
                new Permission { PermissionId = 49, PermissionName = "List", SubModuleId = 10, ModuleId = 15 },
                new Permission { PermissionId = 50, PermissionName = "Create", SubModuleId = 10, ModuleId = 15 },
                new Permission { PermissionId = 51, PermissionName = "Edit", SubModuleId = 10, ModuleId = 15 },
                new Permission { PermissionId = 52, PermissionName = "Trash", SubModuleId = 10, ModuleId = 15 },
                new Permission { PermissionId = 53, PermissionName = "Restore", SubModuleId = 10, ModuleId = 15 },

                // Routes
                new Permission { PermissionId = 54, PermissionName = "List", SubModuleId = 11, ModuleId = 15 },
                new Permission { PermissionId = 55, PermissionName = "Create", SubModuleId = 11, ModuleId = 15 },
                new Permission { PermissionId = 56, PermissionName = "Edit", SubModuleId = 11, ModuleId = 15 },
                new Permission { PermissionId = 57, PermissionName = "Trash", SubModuleId = 11, ModuleId = 15 },
                new Permission { PermissionId = 58, PermissionName = "Restore", SubModuleId = 11, ModuleId = 15 },

                // Travel Routes
                new Permission { PermissionId = 59, PermissionName = "List", SubModuleId = 12, ModuleId = 15 },
                new Permission { PermissionId = 60, PermissionName = "Create", SubModuleId = 12, ModuleId = 15 },
                new Permission { PermissionId = 61, PermissionName = "Edit", SubModuleId = 12, ModuleId = 15 },
                new Permission { PermissionId = 62, PermissionName = "Trash", SubModuleId = 12, ModuleId = 15 },
                new Permission { PermissionId = 63, PermissionName = "Restore", SubModuleId = 12, ModuleId = 15 },

                // Reports
                new Permission { PermissionId = 64, PermissionName = "List", SubModuleId = 13, ModuleId = 15 },

                // Rooms
                new Permission { PermissionId = 65, PermissionName = "List", SubModuleId = 14, ModuleId = 16 },
                new Permission { PermissionId = 66, PermissionName = "Create", SubModuleId = 14, ModuleId = 16 },
                new Permission { PermissionId = 67, PermissionName = "Edit", SubModuleId = 14, ModuleId = 16 },
                new Permission { PermissionId = 68, PermissionName = "Trash", SubModuleId = 14, ModuleId = 16 },
                new Permission { PermissionId = 69, PermissionName = "Restore", SubModuleId = 14, ModuleId = 16 },

                // Places
                new Permission { PermissionId = 70, PermissionName = "List", SubModuleId = 15, ModuleId = 16 },
                new Permission { PermissionId = 71, PermissionName = "Create", SubModuleId = 15, ModuleId = 16 },
                new Permission { PermissionId = 72, PermissionName = "Edit", SubModuleId = 15, ModuleId = 16 },
                new Permission { PermissionId = 73, PermissionName = "Trash", SubModuleId = 15, ModuleId = 16 },
                new Permission { PermissionId = 74, PermissionName = "Restore", SubModuleId = 15, ModuleId = 16 },

                // Accommodation
                new Permission { PermissionId = 75, PermissionName = "List", SubModuleId = 16, ModuleId = 16 },
                new Permission { PermissionId = 76, PermissionName = "Create", SubModuleId = 16, ModuleId = 16 },
                new Permission { PermissionId = 77, PermissionName = "Edit", SubModuleId = 16, ModuleId = 16 },
                new Permission { PermissionId = 78, PermissionName = "Trash", SubModuleId = 16, ModuleId = 16 },
                new Permission { PermissionId = 79, PermissionName = "Restore", SubModuleId = 16, ModuleId = 16 },

                // Accommodation Category (Saudi Five Star Rating)
                new Permission { PermissionId = 80, PermissionName = "List", SubModuleId = 17, ModuleId = 16 },
                new Permission { PermissionId = 81, PermissionName = "Create", SubModuleId = 17, ModuleId = 16 },
                new Permission { PermissionId = 82, PermissionName = "Edit", SubModuleId = 17, ModuleId = 16 },
                new Permission { PermissionId = 83, PermissionName = "Trash", SubModuleId = 17, ModuleId = 16 },
                new Permission { PermissionId = 84, PermissionName = "Restore", SubModuleId = 17, ModuleId = 16 },

                // Food Package
                new Permission { PermissionId = 85, PermissionName = "List", SubModuleId = 18, ModuleId = 16 },
                new Permission { PermissionId = 86, PermissionName = "Create", SubModuleId = 18, ModuleId = 16 },
                new Permission { PermissionId = 87, PermissionName = "Edit", SubModuleId = 18, ModuleId = 16 },
                new Permission { PermissionId = 88, PermissionName = "Trash", SubModuleId = 18, ModuleId = 16 },
                new Permission { PermissionId = 89, PermissionName = "Restore", SubModuleId = 18, ModuleId = 16 },

                // Gifts
                new Permission { PermissionId = 90, PermissionName = "List", SubModuleId = 19, ModuleId = 6 },
                new Permission { PermissionId = 91, PermissionName = "Create", SubModuleId = 19, ModuleId = 6 },
                new Permission { PermissionId = 92, PermissionName = "Edit", SubModuleId = 19, ModuleId = 6 },
                new Permission { PermissionId = 93, PermissionName = "Trash", SubModuleId = 19, ModuleId = 6 },
                new Permission { PermissionId = 94, PermissionName = "Restore", SubModuleId = 19, ModuleId = 6 },

                // Training Session
                new Permission { PermissionId = 95, PermissionName = "List", SubModuleId = 20, ModuleId = 6 },
                new Permission { PermissionId = 96, PermissionName = "Create", SubModuleId = 20, ModuleId = 6 },
                new Permission { PermissionId = 97, PermissionName = "Edit", SubModuleId = 20, ModuleId = 6 },
                new Permission { PermissionId = 98, PermissionName = "Trash", SubModuleId = 20, ModuleId = 6 },
                new Permission { PermissionId = 99, PermissionName = "Restore", SubModuleId = 20, ModuleId = 6 },

                // Banks
                new Permission { PermissionId = 100, PermissionName = "List", SubModuleId = 21, ModuleId = 6 },
                new Permission { PermissionId = 101, PermissionName = "Create", SubModuleId = 21, ModuleId = 6 },
                new Permission { PermissionId = 102, PermissionName = "Edit", SubModuleId = 21, ModuleId = 6 },
                new Permission { PermissionId = 103, PermissionName = "Trash", SubModuleId = 21, ModuleId = 6 },
                new Permission { PermissionId = 104, PermissionName = "Restore", SubModuleId = 21, ModuleId = 6 },

                // Honorifics
                new Permission { PermissionId = 105, PermissionName = "List", SubModuleId = 22, ModuleId = 6 },
                new Permission { PermissionId = 106, PermissionName = "Create", SubModuleId = 22, ModuleId = 6 },
                new Permission { PermissionId = 107, PermissionName = "Edit", SubModuleId = 22, ModuleId = 6 },
                new Permission { PermissionId = 108, PermissionName = "Trash", SubModuleId = 22, ModuleId = 6 },
                new Permission { PermissionId = 109, PermissionName = "Restore", SubModuleId = 22, ModuleId = 6 },

                // Fiqahs
                new Permission { PermissionId = 110, PermissionName = "List", SubModuleId = 23, ModuleId = 6 },
                new Permission { PermissionId = 111, PermissionName = "Create", SubModuleId = 23, ModuleId = 6 },
                new Permission { PermissionId = 112, PermissionName = "Edit", SubModuleId = 23, ModuleId = 6 },
                new Permission { PermissionId = 113, PermissionName = "Trash", SubModuleId = 23, ModuleId = 6 },
                new Permission { PermissionId = 114, PermissionName = "Restore", SubModuleId = 23, ModuleId = 6 },

                // Tehsils
                new Permission { PermissionId = 115, PermissionName = "List", SubModuleId = 24, ModuleId = 6 },
                new Permission { PermissionId = 116, PermissionName = "Create", SubModuleId = 24, ModuleId = 6 },
                new Permission { PermissionId = 117, PermissionName = "Edit", SubModuleId = 24, ModuleId = 6 },
                new Permission { PermissionId = 118, PermissionName = "Trash", SubModuleId = 24, ModuleId = 6 },
                new Permission { PermissionId = 119, PermissionName = "Restore", SubModuleId = 24, ModuleId = 6 },

                // References
                new Permission { PermissionId = 120, PermissionName = "List", SubModuleId = 25, ModuleId = 6 },
                new Permission { PermissionId = 121, PermissionName = "Create", SubModuleId = 25, ModuleId = 6 },
                new Permission { PermissionId = 122, PermissionName = "Edit", SubModuleId = 25, ModuleId = 6 },
                new Permission { PermissionId = 123, PermissionName = "Trash", SubModuleId = 25, ModuleId = 6 },
                new Permission { PermissionId = 124, PermissionName = "Restore", SubModuleId = 25, ModuleId = 6 },

                // Family Heads Relations
                new Permission { PermissionId = 125, PermissionName = "List", SubModuleId = 26, ModuleId = 6 },
                new Permission { PermissionId = 126, PermissionName = "Create", SubModuleId = 26, ModuleId = 6 },
                new Permission { PermissionId = 127, PermissionName = "Edit", SubModuleId = 26, ModuleId = 6 },
                new Permission { PermissionId = 128, PermissionName = "Trash", SubModuleId = 26, ModuleId = 6 },
                new Permission { PermissionId = 129, PermissionName = "Restore", SubModuleId = 26, ModuleId = 6 },

                // Mehram Relations
                new Permission { PermissionId = 130, PermissionName = "List", SubModuleId = 27, ModuleId = 6 },
                new Permission { PermissionId = 131, PermissionName = "Create", SubModuleId = 27, ModuleId = 6 },
                new Permission { PermissionId = 132, PermissionName = "Edit", SubModuleId = 27, ModuleId = 6 },
                new Permission { PermissionId = 133, PermissionName = "Trash", SubModuleId = 27, ModuleId = 6 },
                new Permission { PermissionId = 134, PermissionName = "Restore", SubModuleId = 27, ModuleId = 6 },

                // Nominee Relations
                new Permission { PermissionId = 135, PermissionName = "List", SubModuleId = 28, ModuleId = 6 },
                new Permission { PermissionId = 136, PermissionName = "Create", SubModuleId = 28, ModuleId = 6 },
                new Permission { PermissionId = 137, PermissionName = "Edit", SubModuleId = 28, ModuleId = 6 },
                new Permission { PermissionId = 138, PermissionName = "Trash", SubModuleId = 28, ModuleId = 6 },
                new Permission { PermissionId = 139, PermissionName = "Restore", SubModuleId = 28, ModuleId = 6 },

                // Required Documents
                new Permission { PermissionId = 140, PermissionName = "List", SubModuleId = 29, ModuleId = 6 },
                new Permission { PermissionId = 141, PermissionName = "Create", SubModuleId = 29, ModuleId = 6 },
                new Permission { PermissionId = 142, PermissionName = "Edit", SubModuleId = 29, ModuleId = 6 },
                new Permission { PermissionId = 143, PermissionName = "Trash", SubModuleId = 29, ModuleId = 6 },
                new Permission { PermissionId = 144, PermissionName = "Restore", SubModuleId = 29, ModuleId = 6 },

                // Payment Notes
                new Permission { PermissionId = 145, PermissionName = "List", SubModuleId = 30, ModuleId = 6 },
                new Permission { PermissionId = 146, PermissionName = "Create", SubModuleId = 30, ModuleId = 6 },
                new Permission { PermissionId = 147, PermissionName = "Edit", SubModuleId = 30, ModuleId = 6 },
                new Permission { PermissionId = 148, PermissionName = "Trash", SubModuleId = 30, ModuleId = 6 },
                new Permission { PermissionId = 149, PermissionName = "Restore", SubModuleId = 30, ModuleId = 6 },

                // Vaccines
                new Permission { PermissionId = 150, PermissionName = "List", SubModuleId = 31, ModuleId = 6 },
                new Permission { PermissionId = 151, PermissionName = "Create", SubModuleId = 31, ModuleId = 6 },
                new Permission { PermissionId = 152, PermissionName = "Edit", SubModuleId = 31, ModuleId = 6 },
                new Permission { PermissionId = 153, PermissionName = "Trash", SubModuleId = 31, ModuleId = 6 },
                new Permission { PermissionId = 154, PermissionName = "Restore", SubModuleId = 31, ModuleId = 6 },

                // Quotation Payment Policies
                new Permission { PermissionId = 155, PermissionName = "List", SubModuleId = 32, ModuleId = 6 },
                new Permission { PermissionId = 156, PermissionName = "Create", SubModuleId = 32, ModuleId = 6 },
                new Permission { PermissionId = 157, PermissionName = "Edit", SubModuleId = 32, ModuleId = 6 },
                new Permission { PermissionId = 158, PermissionName = "Trash", SubModuleId = 32, ModuleId = 6 },
                new Permission { PermissionId = 159, PermissionName = "Restore", SubModuleId = 32, ModuleId = 6 },

                // Quotation T&Cs
                new Permission { PermissionId = 160, PermissionName = "List", SubModuleId = 33, ModuleId = 6 },
                new Permission { PermissionId = 161, PermissionName = "Create", SubModuleId = 33, ModuleId = 6 },
                new Permission { PermissionId = 162, PermissionName = "Edit", SubModuleId = 33, ModuleId = 6 },
                new Permission { PermissionId = 163, PermissionName = "Trash", SubModuleId = 33, ModuleId = 6 },
                new Permission { PermissionId = 164, PermissionName = "Restore", SubModuleId = 33, ModuleId = 6 },

                // Packages
                new Permission { PermissionId = 165, PermissionName = "List", SubModuleId = 44, ModuleId = 4 },
                new Permission { PermissionId = 166, PermissionName = "Create", SubModuleId = 44, ModuleId = 4 },
                new Permission { PermissionId = 167, PermissionName = "Edit", SubModuleId = 44, ModuleId = 4 },
                new Permission { PermissionId = 168, PermissionName = "Trash", SubModuleId = 44, ModuleId = 4 },
                new Permission { PermissionId = 169, PermissionName = "Restore", SubModuleId = 44, ModuleId = 4 },
                new Permission { PermissionId = 170, PermissionName = "Report", SubModuleId = 44, ModuleId = 4 },
                new Permission { PermissionId = 171, PermissionName = "Copy", SubModuleId = 44, ModuleId = 4 },
                new Permission { PermissionId = 172, PermissionName = "Details", SubModuleId = 44, ModuleId = 4 },

                // Quotation Packages
                new Permission { PermissionId = 173, PermissionName = "List", SubModuleId = 45, ModuleId = 4 },
                new Permission { PermissionId = 174, PermissionName = "Customize Package", SubModuleId = 45, ModuleId = 4 },
                new Permission { PermissionId = 175, PermissionName = "Edit", SubModuleId = 45, ModuleId = 4 },
                new Permission { PermissionId = 176, PermissionName = "Trash", SubModuleId = 45, ModuleId = 4 },
                new Permission { PermissionId = 177, PermissionName = "Restore", SubModuleId = 45, ModuleId = 4 },
                new Permission { PermissionId = 178, PermissionName = "Quotation", SubModuleId = 45, ModuleId = 4 },
                new Permission { PermissionId = 179, PermissionName = "Details", SubModuleId = 45, ModuleId = 4 },
                new Permission { PermissionId = 180, PermissionName = "Book", SubModuleId = 45, ModuleId = 4 },

                // Member Types
                new Permission { PermissionId = 181, PermissionName = "List", SubModuleId = 46, ModuleId = 8 },
                new Permission { PermissionId = 182, PermissionName = "Create", SubModuleId = 46, ModuleId = 8 },
                new Permission { PermissionId = 183, PermissionName = "Edit", SubModuleId = 46, ModuleId = 8 },
                new Permission { PermissionId = 184, PermissionName = "Trash", SubModuleId = 46, ModuleId = 8 },
                new Permission { PermissionId = 185, PermissionName = "Restore", SubModuleId = 46, ModuleId = 8 },

                // Member Address
                new Permission { PermissionId = 186, PermissionName = "List", SubModuleId = 47, ModuleId = 8 },
                new Permission { PermissionId = 187, PermissionName = "Create", SubModuleId = 47, ModuleId = 8 },
                new Permission { PermissionId = 188, PermissionName = "Edit", SubModuleId = 47, ModuleId = 8 },
                new Permission { PermissionId = 189, PermissionName = "Trash", SubModuleId = 47, ModuleId = 8 },
                new Permission { PermissionId = 190, PermissionName = "Restore", SubModuleId = 47, ModuleId = 8 },

                // Member Contact
                new Permission { PermissionId = 191, PermissionName = "List", SubModuleId = 48, ModuleId = 8 },
                new Permission { PermissionId = 192, PermissionName = "Create", SubModuleId = 48, ModuleId = 8 },
                new Permission { PermissionId = 193, PermissionName = "Edit", SubModuleId = 48, ModuleId = 8 },
                new Permission { PermissionId = 194, PermissionName = "Trash", SubModuleId = 48, ModuleId = 8 },
                new Permission { PermissionId = 195, PermissionName = "Restore", SubModuleId = 48, ModuleId = 8 },

                // Member Email
                new Permission { PermissionId = 196, PermissionName = "List", SubModuleId = 49, ModuleId = 8 },
                new Permission { PermissionId = 197, PermissionName = "Create", SubModuleId = 49, ModuleId = 8 },
                new Permission { PermissionId = 198, PermissionName = "Edit", SubModuleId = 49, ModuleId = 8 },
                new Permission { PermissionId = 199, PermissionName = "Trash", SubModuleId = 49, ModuleId = 8 },
                new Permission { PermissionId = 200, PermissionName = "Restore", SubModuleId = 49, ModuleId = 8 },

                // Airlines
                new Permission { PermissionId = 201, PermissionName = "List", SubModuleId = 50, ModuleId = 5 },
                new Permission { PermissionId = 202, PermissionName = "Create", SubModuleId = 50, ModuleId = 5 },
                new Permission { PermissionId = 203, PermissionName = "Edit", SubModuleId = 50, ModuleId = 5 },
                new Permission { PermissionId = 204, PermissionName = "Trash", SubModuleId = 50, ModuleId = 5 },
                new Permission { PermissionId = 205, PermissionName = "Restore", SubModuleId = 50, ModuleId = 5 },

                // Airline Class
                new Permission { PermissionId = 206, PermissionName = "List", SubModuleId = 51, ModuleId = 5 },
                new Permission { PermissionId = 207, PermissionName = "Create", SubModuleId = 51, ModuleId = 5 },
                new Permission { PermissionId = 208, PermissionName = "Edit", SubModuleId = 51, ModuleId = 5 },
                new Permission { PermissionId = 209, PermissionName = "Trash", SubModuleId = 51, ModuleId = 5 },
                new Permission { PermissionId = 210, PermissionName = "Restore", SubModuleId = 51, ModuleId = 5 },

                // Airline Fare
                new Permission { PermissionId = 211, PermissionName = "List", SubModuleId = 52, ModuleId = 5 },
                new Permission { PermissionId = 212, PermissionName = "Create", SubModuleId = 52, ModuleId = 5 },
                new Permission { PermissionId = 213, PermissionName = "Edit", SubModuleId = 52, ModuleId = 5 },
                new Permission { PermissionId = 214, PermissionName = "Trash", SubModuleId = 52, ModuleId = 5 },
                new Permission { PermissionId = 215, PermissionName = "Restore", SubModuleId = 52, ModuleId = 5 },

                // Families
                new Permission { PermissionId = 216, PermissionName = "List", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 217, PermissionName = "Create Family Head", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 218, PermissionName = "Edit", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 219, PermissionName = "Trash", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 220, PermissionName = "Restore", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 221, PermissionName = "View Family", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 222, PermissionName = "Letter Head", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 223, PermissionName = "Invoice", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 224, PermissionName = "Add Member", SubModuleId = 53, ModuleId = 9 },
                new Permission { PermissionId = 225, PermissionName = "Verify", SubModuleId = 53, ModuleId = 9 },

                // Receipts
                new Permission { PermissionId = 226, PermissionName = "List", SubModuleId = 56, ModuleId = 14 },
                new Permission { PermissionId = 227, PermissionName = "Create", SubModuleId = 56, ModuleId = 14 },
                new Permission { PermissionId = 228, PermissionName = "Edit", SubModuleId = 56, ModuleId = 14 },
                new Permission { PermissionId = 229, PermissionName = "Trash", SubModuleId = 56, ModuleId = 14 },
                new Permission { PermissionId = 230, PermissionName = "Restore", SubModuleId = 56, ModuleId = 14 },
                new Permission { PermissionId = 231, PermissionName = "Change Status", SubModuleId = 56, ModuleId = 14 },
                new Permission { PermissionId = 232, PermissionName = "Slip", SubModuleId = 56, ModuleId = 14 },

                new Permission { PermissionId = 233, PermissionName = "Client Statement List", SubModuleId = 57, ModuleId = 14 },
                new Permission { PermissionId = 234, PermissionName = "Mini Statement List", SubModuleId = 58, ModuleId = 14 },

                new Permission { PermissionId = 235, PermissionName = "List", SubModuleId = 59, ModuleId = 13 },
                new Permission { PermissionId = 236, PermissionName = "Create", SubModuleId = 59, ModuleId = 13 },
                new Permission { PermissionId = 237, PermissionName = "Edit", SubModuleId = 59, ModuleId = 13 },
                new Permission { PermissionId = 238, PermissionName = "Trash", SubModuleId = 59, ModuleId = 13 },
                new Permission { PermissionId = 239, PermissionName = "Restore", SubModuleId = 59, ModuleId = 13 },

                new Permission { PermissionId = 240, PermissionName = "List", SubModuleId = 60, ModuleId = 13 },
                new Permission { PermissionId = 241, PermissionName = "Create", SubModuleId = 60, ModuleId = 13 },
                new Permission { PermissionId = 242, PermissionName = "Edit", SubModuleId = 60, ModuleId = 13 },
                new Permission { PermissionId = 243, PermissionName = "Trash", SubModuleId = 60, ModuleId = 13 },
                new Permission { PermissionId = 244, PermissionName = "Restore", SubModuleId = 60, ModuleId = 13 },

                new Permission { PermissionId = 245, PermissionName = "View Room Summary", SubModuleId = 61, ModuleId = 13 },
                new Permission { PermissionId = 246, PermissionName = "View Daily Room Summary", SubModuleId = 62, ModuleId = 13 },

                new Permission { PermissionId = 247, PermissionName = "List", SubModuleId = 64, ModuleId = 17 },
                new Permission { PermissionId = 248, PermissionName = "Create", SubModuleId = 64, ModuleId = 17 },
                new Permission { PermissionId = 249, PermissionName = "Edit", SubModuleId = 64, ModuleId = 17 },
                new Permission { PermissionId = 250, PermissionName = "Trash", SubModuleId = 64, ModuleId = 17 },
                new Permission { PermissionId = 251, PermissionName = "Restore", SubModuleId = 64, ModuleId = 17 }

                );

            const int permCount = 246;
            RolePermissions[] rolePermissions = new RolePermissions[permCount];

            for (int i = 0; i < permCount; i++)
            {
                rolePermissions[i] = new RolePermissions { RoleId = 1, PermissionId = i + 1, Allowed = true };
            }

            modelBuilder.Entity<RolePermissions>()
                .HasData(rolePermissions);

            // Fixed
            modelBuilder.Entity<Place>()
                .HasData(
                    new Place { PlaceId = 1, PlaceName = "Makkah" },
                    new Place { PlaceId = 2, PlaceName = "Medinah" },
                    new Place { PlaceId = 3, PlaceName = "Mina / Arafat" },
                    new Place { PlaceId = 4, PlaceName = "Azizia" }
                );

            modelBuilder.Entity<Gift>()
                .HasData(
                    new Gift { GiftId = 1, GiftCode = "GW-01", Giveaway = "Suit Case" },
                    new Gift { GiftId = 2, GiftCode = "GW-02", Giveaway = "Bottle" }
                );

            modelBuilder.Entity<Accomodation>()
                .HasData(
                    new Accomodation { AccomodationId = 1, AccomodationName = "Hotel" },
                    new Accomodation { AccomodationId = 2, AccomodationName = "Maktab" },
                    new Accomodation { AccomodationId = 3, AccomodationName = "Building / Flat" }
                );

            modelBuilder.Entity<AccomodationCategory>()
                .HasData(
                    new AccomodationCategory { AccomodationCategoryId = 1, Category = "5 Star" },
                    new AccomodationCategory { AccomodationCategoryId = 2, Category = "4 Star" },
                    new AccomodationCategory { AccomodationCategoryId = 3, Category = "3 Star" }
                );


            modelBuilder.Entity<Reference>()
                .HasData(
                    new Reference { ReferenceId = 1, ReferenceCode = "1", ReferenceName = "Office" }
                );


            modelBuilder.Entity<Vehicle>()
                .HasData(
                    new Vehicle { VehicleId = 1, VehicleType = "Bus" },
                    new Vehicle { VehicleId = 2, VehicleType = "Car" }
                );


            modelBuilder.Entity<RequiredDocument>()
                .HasData(
                    new RequiredDocument { RequiredDocumentId = 1, DocumentsCode = "1", Documents = "02 CNIC Copies" },
                    new RequiredDocument { RequiredDocumentId = 2, DocumentsCode = "2", Documents = "02 Passport Copies" },
                    new RequiredDocument { RequiredDocumentId = 3, DocumentsCode = "3", Documents = "08 Pics (3x4 cm Blue BG)" }
                );



            base.OnModelCreating(modelBuilder);
        }


        #region Ehajj Modules
        public DbSet<EHajjContract> EHajjContracts { get; set; }
        public DbSet<EHajjContractMember> EHajjContractMembers { get; set; }
        public DbSet<EHajjRooms> EHajjRooms { get; set; }

        public IQueryable<EHajjContract> EHajjContractsWithoutTrash => EHajjContracts.Where(e => !e.Trashed);
        #endregion

    }
}

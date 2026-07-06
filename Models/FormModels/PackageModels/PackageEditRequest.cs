using TMG.Models.AccomodationModels;
using System.ComponentModel.DataAnnotations;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;

namespace TMG.Models.FormModels.PackageModels
{
    public class PackageEditRequest
    {
        [Display(Name = "Customer Name", Prompt = "Enter Customer Name...")]
        public string? CustomerName { get; set; }

        [Display(Name = "Customer Phone", Prompt = "Enter Customer Phone...")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Customer Email", Prompt = "Enter Customer Email...")]
        public string? CustomerEmail { get; set; }

        [Display(Name = "Customer Address", Prompt = "Enter Customer Name...")]
        public string? CustomerAddress { get; set; }

        [Display(Name = "Number of Adults", Prompt = "Enter Number of Adults...")]
        public int? NumberOfAdults { get; set; }
        [Display(Name = "Number of Childs", Prompt = "Enter Number of Childs...")]
        public int? NumberOfChilds { get; set; }
        [Display(Name = "Number of Infants", Prompt = "Enter Number of Infants...")]
        public int? NumberOfInfants { get; set; }

        [Display(Name = "Valid Till")]
        public DateTime? ValidTill { get; set; }



        // Package 
        public int? PackageId { get; set; }

        [Required, Display(Name = "Company")]
        public int CompanyId { get; set; }

        // [Display(Name = "Mora Package #", Prompt = "Enter Package Number...")]
        // public int PackageNumber { get; set; }

        [Required, Display(Name = "Package Name", Prompt = "Enter Package Name...")]
        public string? PackageName { get; set; }
        [Display(Name = "Package #", Prompt = "Enter Package Number...")]
        public int? PackageNumber { get; set; }

        [Required, Display(Name = "Package Code", Prompt = "Enter Package Code...")]
        public string? PackageCode { get; set; }

        [Required, Display(Name = "Package Quota", Prompt = "Enter Package Quota...")]
        public int PackageQouta { get; set; }

        [Required, Display(Name = "Days", Prompt = "Enter Days...")]
        public string? Days { get; set; }
        public string? TermsnCondition { get; set; }
        public string Color { get; set; }

        public string Maktab { get; set; }
        public string MaktabNumber { get; set; }

        public string MaktabAddress { get; set; }

        public string OfficeAddress { get; set; }

        // [Required, Display(Name = "Compulsory Hajj Dues", Prompt = "Enter Compulsory Hajj Dues...")]
        // public float CompulsoryHajjDues { get; set; }

        [Display(Name = "Travel Routes")]
        public int? TravelRouteId { get; set; }

        [Required, Display(Name = "Medina Arrival")]
        public string? MedinaArrival { get; set; }

        [Required, Display(Name = "Hajj Duration")]
        public string? HajjDuration { get; set; }

        [Required, Display(Name = "Is Food Included?")]
        public bool PackageFoodIncluded { get; set; }

        [Required, Display(Name = "Is Qurbani Included?")]
        public bool PackageQurbaniIncluded { get; set; }

        [Required, Display(Name = "USD ROE")]
        public float USDROE { get; set; } = 0;

        [Required, Display(Name = "SAR ROE")]
        public float SARROE { get; set; } = 0;
        public float PKRROE { get; set; } = 0;
        public float? GBPROE { get; set; } = 0f;
        public float? EUROROE { get; set; } = 0f;
        public float? AEDROE { get; set; } = 0f;

        [Required, Display(Name = "Adult (PKR)")]
        public float AdultPkr { get; set; } = 0;

        [Required, Display(Name = "Adult (SAR)")]
        public float AdultSar { get; set; } = 0;

        [Required, Display(Name = "Adult (USD)")]
        public float AdultUsd { get; set; } = 0;

        [Required, Display(Name = "Child (PKR)")]
        public float ChildPkr { get; set; } = 0;

        [Required, Display(Name = "Child (SAR)")]
        public float ChildSar { get; set; } = 0;

        [Required, Display(Name = "Child (USD)")]
        public float ChildUsd { get; set; } = 0;
        public float? ChildGbp { get; set; } = 0f;
        public float? ChildEuro { get; set; } = 0f;
        public float? ChildAed { get; set; } = 0f;
        public float? AdultGbp { get; set; } = 0f;
        public float? AdultEuro { get; set; } = 0f;
        public float? AdultAed { get; set; } = 0f;

        [Required, Display(Name = "Infant (PKR)")]
        public float InfantPkr { get; set; } = 0;

        [Required, Display(Name = "Infant (SAR)")]
        public float InfantSar { get; set; } = 0;

        [Required, Display(Name = "Infant (USD)")]
        public float InfantUsd { get; set; } = 0;
        public float? InfantGbp { get; set; } = 0f;
        public float? InfantEuro { get; set; } = 0f;
        public float? InfantAed { get; set; } = 0f;
        public int? RoomIds { get; set; }
        public int? RoomTypesId { get; set; }
        public int? Camps { get; set; }
        public string NearBy { get; set; }
        public List<int?> PackageFacilitiesIds { get; set; }
        public List<bool> SelectedFacilities { get; set; }


        // public int AddOnError { get; set; }
        // public List<int> PackageAddons { get; set; }
        // public List<string> AddOnCode { get; set; }
        // public List<string> AddOnDescription { get; set; }
        // public List<float> AddOnPkr { get; set; }
        // public List<float> AddOnSar { get; set; }
        // public List<float> AddOnUsd { get; set; }


        // Accomodation
        public int? AccommodationError { get; set; }

        [Required, Display(Name = "Place")]
        public List<int?> AccomodationPlaceId { get; set; }

        [Required, Display(Name = "Accomodation")]
        public List<int?> AccomodationId { get; set; }

        [Required, Display(Name = "Saudi Star Rating")]
        public List<int?> AccomodationCategoryId { get; set; }

        [Display(Name = "Accomodation", Prompt = "Enter Accomodation...")]
        public List<string?> Accomodation { get; set; }

        [Display(Name = "Distance (meters)", Prompt = "Enter Distance...")]
        public List<string?> AccomodationDistance { get; set; }

        [Display(Name = "Days", Prompt = "Enter Days...")]
        public List<string?> AccomodationDays { get; set; }

        [Display(Name = "Nights", Prompt = "Enter Nights...")]
        public List<string?> AccomodationNights { get; set; }

        [Display(Name = "Check In", Prompt = "Enter Check In...")]
        public List<string?> AccomodationCheckIn { get; set; }

        [Display(Name = "Check Out", Prompt = "Enter Check Out...")]
        public List<string?> AccomodationCheckOut { get; set; }

        [Required, Display(Name = "Is Food Included?")]
        public List<bool> AccomodationFoodIncluded { get; set; }

        [Required, Display(Name = "Food Package")]
        public List<int?> AccomodationFoodPackageId { get; set; }

        [Required, Display(Name = "Is Residence Included?")]
        public List<bool> AccomodationResidenceIncluded { get; set; }

        [Required, Display(Name = "Food Package")]
        public List<string?> AccomodationActualAccomodation { get; set; }
        public List<int?> HotelIds { get; set; }

        // [Required, Display(Name = "Actual Days")]
        // public List<int?> AccomodationActualDays { get; set; }

        // [Required, Display(Name = "Actual Nights")]
        // public List<int?> AccomodationActualNights { get; set; }

        // [Required, Display(Name = "Actual Check In")]
        // public List<string> AccomodationActualCheckIn { get; set; }

        // [Required, Display(Name = "Actual Check Out")]
        // public List<string> AccomodationActualCheckOut { get; set; }

        [Required, Display(Name = "Actual Check In Time")]
        public List<DateTime?> AccomodationActualCheckInTime { get; set; }

        [Required, Display(Name = "Actual Check Out Time")]
        public List<DateTime?> AccomodationActualCheckOutTime { get; set; }

        /*[Required, Display(Name = "Load / Room (Beds)", Prompt = "Enter Load / Room...")]
        public List<int?> AccomodationLoadPerRoomBeds { get; set; }

        [Required, Display(Name = "Load / Washroom (Persons)", Prompt = "Enter Load / Washroom...")]
        public List<int?> AccomodationLoadPerWashroomPersons { get; set; }*/

        [Required, Display(Name = "Actual Load / Room", Prompt = "Enter Actual Load / Room...")]
        public List<int?> AccomodationActualLoadPerRoom { get; set; }

        [Required, Display(Name = "Group Ziarat", Prompt = "Enter Group Ziarat...")]
        public List<string?> AccomodationGroupZiarat { get; set; }

        [Required, Display(Name = "Religious Lecture", Prompt = "Enter Religious Lecture...")]
        public List<string?> AccomodationReligiousLecture { get; set; }

        [Required, Display(Name = "Distribution Arrangements", Prompt = "Enter Distribution Arrangements...")]
        public List<string?> AccomodationDistribution { get; set; }

        [Required, Display(Name = "Camp", Prompt = "Enter Camp...")]
        public List<string?> AccomodationCamp { get; set; }

        [Required, Display(Name = "Arafat", Prompt = "Enter Arafat...")]
        public List<string?> AccomodationArafat { get; set; }

        [Required, Display(Name = "Shuttle", Prompt = "Enter Shuttle...")]
        public List<string?> AccomodationShuttle { get; set; }

        [Required, Display(Name = "Bedding (Sofa Mattress)", Prompt = "Enter Bedding...")]
        public List<string?> AccomodationBedding { get; set; }

        [Required, Display(Name = "Sharing (Room / Tent / Camp)", Prompt = "Enter Sharing...")]
        public List<string?> AccomodationSharing { get; set; }

        [Required, Display(Name = "Sharing Type (Camp)", Prompt = "Enter Sharing Type (Camp)...")]
        public List<string?> AccomodationSharingType { get; set; }

        [Required, Display(Name = "Note", Prompt = "Enter Note...")]
        public List<string?> AccomodationNote { get; set; }

        // Carrier
        [Required, Display(Name = "Flight Type")]
        public string? FlightType { get; set; }

        [Required, Display(Name = "Flight Class", Prompt = "Enter Class...")]
        public string? FlightClass { get; set; }

        [Required, Display(Name = "Transport Intercity", Prompt = "Enter Transport Intercity...")]
        public string? TransportIntercity { get; set; }

        [Required, Display(Name = "Transport Within City", Prompt = "Enter Transport Within City...")]
        public string? WithInCity { get; set; }

        [Required, Display(Name = "POD", Prompt = "Enter POD...")]
        public string? POD { get; set; }

        [Required, Display(Name = "Airport Transfer", Prompt = "Enter Airport Transport...")]
        public string? AirportTransfer { get; set; }

        [Required, Display(Name = "Transport Within Mashair", Prompt = "Enter Trasnport...")]
        public string? TransportWithMashair { get; set; }

        [Display(Name = "Flight Ticket Price (PKR)", Prompt = "Enter Price...")]
        public float TicketPricePkr { get; set; } = 0;

        [Display(Name = "Flight Ticket Price (SAR)", Prompt = "Enter Price...")]
        public float TicketPriceSar { get; set; } = 0;

        [Display(Name = "Flight Ticket Price (USD)", Prompt = "Enter Price...")]
        public float TicketPriceUsd { get; set; } = 0;
        [Display(Name = "Ticket Period", Prompt = "Enter Ticket Period...")]
        public string? TicketPeriod { get; set; }


        // Additional Facilities
        public int AdditionalFacilityError { get; set; }
        public List<int?> AdditionalFacilityIds { get; set; }
        public List<int?> AdditionalCode { get; set; }
        public List<int?> AdditionalRoomId { get; set; }
        public List<int> AdditionalFacilityDescriptionId { get; set; }
        public List<int?> AdditionalPlaceId { get; set; }
        public List<int?> AdditionalAccomodationId { get; set; }

        //public List<float?> AdditionalUSDROE { get; set; }
        //public List<float?> AdditionalSARROE { get; set; }

        public List<float?> AdditionalAdultPkr { get; set; }
        public List<float?> AdditionalAdultSar { get; set; }
        public List<float?> AdditionalAdultUsd { get; set; }
        public List<float?> AdditionalChildPkr { get; set; }
        public List<float?> AdditionalChildSar { get; set; }
        public List<float?> AdditionalChildUsd { get; set; }
        public List<float?> AdditionalInfantPkr { get; set; }
        public List<float?> AdditionalInfantSar { get; set; }
        public List<float?> AdditionalInfantUsd { get; set; }
        public List<string> AdditionalNotes { get; set; }


        public List<FlightType?> FlightTypes { get; set; }
        public List<int?> AirlineIds { get; set; }
        public List<int?> RailwayIds { get; set; }
        public List<string?> FlightNos { get; set; }
        public List<string?> TrainNos { get; set; }
        public List<FlightClass?> FlightClasses { get; set; }
        public List<TrainClass?> TrainClasses { get; set; }
        public List<string?> Origins { get; set; }
        public List<string?> Destinations { get; set; }
        public List<string?> TrainOrigins { get; set; }
        public List<string?> TrainDestinations { get; set; }
        public List<DateTime?> DepartureDate { get; set; }
        public List<DateTime?> ArrivalDate { get; set; }
        public List<DateTime?> DepartureTime { get; set; }
        public List<DateTime?> ArrivalTime { get; set; }
        public List<DateTime?> TrainDepartureTime { get; set; }
        public List<DateTime?> TrainDepartureDate { get; set; }
        public List<DateTime?> TrainArrivalDate { get; set; }
        public List<DateTime?> TrainArrivalTime { get; set; }
        public List<string?> PNRNos { get; set; }
        public List<string?> TrainPNRNos { get; set; }
        public List<float?> TicketAmount { get; set; }
        public List<float?> TrainTicketAmount { get; set; }
        public List<bool> IsPreffereds { get; set; }

        // Training & Gifts
        public List<int?> TrainingIds { get; set; }
        public List<bool> TrainingSelected { get; set; }

        public int? GiftError { get; set; }
        public List<int?> GiftId { get; set; }
        public List<bool> GiftSelected { get; set; }



        // Payment Notes
        // public List<int> NoteId { get; set; }
        // public List<bool> NoteSelected { get; set; }

        [Required, Display(Name = "Category Zone", Prompt = "Enter Category Zone...")]
        public string? PackageCategoryZone { get; set; }
        public List<string?> Terms { get; set; }
        public string Description { get; set; }


        public IFormFile? MINA { get; set; }
        public IFormFile? ARAFAT { get; set; }
        public IFormFile? MUZDALIFAH { get; set; }
        public IFormFile? RamiDayOne { get; set; }
        public IFormFile? RamiDayTwo { get; set; }
        public IFormFile? RamiDayThree { get; set; }
    }
}

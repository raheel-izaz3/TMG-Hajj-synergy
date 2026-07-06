using TMG.Models.AccomodationModels;
using TMG.Models.CompanyModels;
using TMG.Models.LogisticModels;
using TMG.Models.MiscModels;

namespace TMG.Models.PackageModels
{
    public enum PackageStatus
    {
        Quotation,
        Booked
    }
    public enum PackageState
    {
        Orignal,
        Customized
    }

    public class Package : BaseEntity
    {
        public int PackageId { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public string Content { get; set; }
        public int? PackageNumber { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int Quota { get; set; }

        public string Maktab { get; set; }
        public string MaktabNumber { get; set; }
        public string MaktabAddress { get; set; }
        public string OfficeAddress { get; set; }



        public float HajjDues { get; set; } = 0f;
        public string? HajjDuration { get; set; }
        public string? MedinahArrival { get; set; }
        public string? Days { get; set; }
        public bool FoodIncluded { get; set; }
        //public bool QurbaniIncluded { get; set; }
        public string? TermsnCondition { get; set; }

        public int? TravelRouteId { get; set; }
        public TravelRoute TravelRoute { get; set; }

        public float USDROE { get; set; } = 0f;
        public float SARROE { get; set; } = 0f;
        public float PKRROE { get; set; } = 0f;
        public float? GBPROE { get; set; } = 0f;
        public float? EUROROE { get; set; } = 0f;
        public float? AEDROE { get; set; } = 0f;

        public float AdultPkr { get; set; } = 0f;
        public float AdultSar { get; set; } = 0f;
        public float AdultUsd { get; set; } = 0f;
        public float? AdultGbp { get; set; } = 0f;
        public float? AdultEuro{ get; set; } = 0f;
        public float? AdultAed{ get; set; } = 0f;
        public float ChildPkr { get; set; } = 0f;
        public float ChildSar { get; set; } = 0f;
        public float ChildUsd { get; set; } = 0f;
        public float? ChildGbp{ get; set; } = 0f;
        public float? ChildEuro{ get; set; } = 0f;
        public float? ChildAed{ get; set; } = 0f;
        public float InfantPkr { get; set; } = 0f;
        public float InfantSar { get; set; } = 0f;
        public float InfantUsd { get; set; } = 0f;
        public float? InfantGbp{ get; set; } = 0f;
        public float? InfantEuro{ get; set; } = 0f;
        public float? InfantAed { get; set; } = 0f;

        public int? RoomId { get; set; }
        public Room Room { get; set; }

        public int? RoomTypesId { get; set; }
        public Room RoomTypes { get; set; }

        public string NearBy { get; set; }
        public string Color { get; set; }

        public string? Image { get; set; }

        public string? FlightType { get; set; }
        public string? FlightClass { get; set; }
        public string? POD { get; set; }
        public string? TransportIntercity { get; set; }
        public string? AirportTransport { get; set; }
        public string? TransportWithMashair { get; set; }
        public string? WithInCity { get; set; }

        public PackageStatus? PackageStatus { get; set; }

        public PackageState? PackageState { get; set; }

        public string? CategoryZone { get; set; }

        public float FlightTicketPricePkr { get; set; } = 0f;
        public float FlightTicketPriceSar { get; set; } = 0f;
        public float FlightTicketPriceUsd { get; set; } = 0f;

        public string FlightTicketPeriod { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons hajjSeasons { get; set; }
        public string? MINA { get; set; }
        public string? ARAFAT { get; set; }
        public string? MUZDALIFAH { get; set; }
        public string? RamiDayOne { get; set; }
        public string? RamiDayTwo { get; set; }
        public string? RamiDayThree { get; set; }
        public bool? Status { get; set; }

        public ICollection<PackageAccomodations> PackageAccomodations { get; set; }
    }
}

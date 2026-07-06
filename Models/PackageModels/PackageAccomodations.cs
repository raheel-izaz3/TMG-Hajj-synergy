using TMG.Models.AccomodationModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.PackageModels
{
    public class PackageAccomodations
    {
        public int? PackageAccomodationsId { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }

        public int? PlaceId { get; set; }
        public Place Place { get; set; }

        public int? AccomodationId { get; set; }
        public Accomodation Accomodation { get; set; }

        public int? AccomodationCategoryId { get; set; }
        public AccomodationCategory AccomodationCategory { get; set; }

        public string? AccomodationName { get; set; }

        public string? DistanceInMeters { get; set; }

        public string? Days { get; set; }
        public string? Nights { get; set; }

        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }

        public bool FoodIncluded { get; set; }
        public bool ResidenceIncluded { get; set; }

        public int? FoodPackageId { get; set; }
        public FoodPackage FoodPackage { get; set; }

        public string? ActualAccommodation { get; set; }
        public int? ActualDays { get; set; }
        public int? ActualNights { get; set; }
        public string? ActualCheckIn { get; set; }
        public string? ActualCheckOut { get; set; }
        public  DateTime? ActualCheckInTime { get; set; }
        public  DateTime? ActualCheckOutTime { get; set; }

       /* public int LoadPerRoom { get; set; }
        public int LoadPerWashroom { get; set; }*/

/*        public int? RoomId { get; set; }
        public Room Room { get; set; }*/

        public string? GroupZiarat { get; set; }
        public string? ReligiousLectures { get; set; }

        public string? DistributionArrangements { get; set; }
        public string? Camp { get; set; }
        public string? Arafat { get; set; }
        public string? Shuttle { get; set; }
        public string? Bedding { get; set; }
        public string? Sharing { get; set; }
        public string? SharingType { get; set; }


        public string? Notes { get; set; }
        public int? HotelId { get; set; }
        public Hotel? Hotel { get; set; }

    }
}

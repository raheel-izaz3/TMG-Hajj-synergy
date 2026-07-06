using TMG.Models.ReservationModels;

namespace TMG.Models.POModels
{
    public class POHotelEditViewModel
    {
        public int POHotelsId { get; set; }
        public int? NumberOfRooms { get; set; }
        public float? MunicipilityAmount { get; set; }
        public float? MunicipilityPercentage { get; set; }
        public float? MunicipilityFeeAmount { get; set; }
        public float? VatAmmount { get; set; }
        public float? VatPercentage { get; set; }
        public float? VatFeeAmount { get; set; }
        public float? HandlingCharges { get; set; }
        public float? HandlingPercentage { get; set; }
        public float? HandlingFeeAmount { get; set; }
        public float? Total { get; set; }
        public float? SubTotal { get; set; }
        public int PlaceId { get; set; }
        public string Code { get; set; }
        public PaymentMode? PaymentMode { get; set; }
        public Taxes? TaxesId { get; set; }
        public int? VendorId { get; set; }
        public int? HotelId { get; set; }
        public bool ForceDelete { get; set; } = false;
        public List<string> RoomNumber { get; set; } = new List<string>();

        public List<int> POHotelRoomDatasId { get; set; } = new List<int>();
        public List<int?> Quantity { get; set; } = new List<int?>();
        public List<float?> Amount { get; set; } = new List<float?>();
        public List<DateTime?> Arrival { get; set; } = new List<DateTime?>();
        public List<DateTime?> Departure { get; set; } = new List<DateTime?>();
        public List<float?> Totals { get; set; } = new List<float?>();
        public List<BathroomType?> BathroomType { get; set; } = new List<BathroomType?>();
        public List<int?> RoomId { get; set; } = new List<int?>();
        public List<int?> RoomViewId { get; set; } = new List<int?>();
        public List<POHotelRoomData> POHotelRoomDatas { get; set; }
    }
    public class POHotelRoomData
    {
        public int POHotelRoomDatasId {get; set;}
        public int? Quantity { get; set; }
        public float? Amount { get; set; }
        public DateTime? Arrival { get; set; }
        public DateTime? Departure { get; set; }
        public float? Total { get; set; }
        public BathroomType? BathroomType { get; set; }
        public int? RoomId { get; set; }
        public int? RoomViewId { get; set; }
    }
}

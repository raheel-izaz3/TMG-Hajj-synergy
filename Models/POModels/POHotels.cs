using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.POModels
{
    public enum Taxes
    {
        IncludingTax,
        ExcludingTax
    }
    public enum PaymentMode
    {
        Vendor,
        CreditCard,
        EHAJJ,
        Contract
    }

    public class POHotels : BaseEntity
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
        public string Code { get; set; }
        public PaymentMode? PaymentMode { get; set; }
        public Taxes? TaxesId { get; set; }
        public int? VendorId { get; set; }
        public Vendor? Vendor { get; set; }
        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public int PlaceId { get; set; }
        public Place Place { get; set; }
        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
        public ICollection<POHotelRooms> POHotelRooms { get; set; }
    }
}

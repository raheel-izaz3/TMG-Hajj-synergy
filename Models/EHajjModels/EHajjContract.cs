using TMG.Models.AccomodationModels;
using TMG.Models.CompanyModels;
using TMG.Models.PackageModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.EHajjModels
{
    public class EHajjContract : BaseEntity
    {
        public int EhajjContractId { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
        public Hotel Hotel { get; set; }
        public int HotelId { get; set; }
        public int FoodPackageId { get; set; }
        public FoodPackage FoodPackage { get; set; }
        public int Capacity { get; set; }
        public int Used { get; set; }
        public int Unused { get; set; }
        public string EhajjContractNumber { get; set; }
        public string VoucherNumber { get; set; }
        public float Amount { get; set; }
        public string Contractpath { get; set; }
        public string Voucherpath { get; set; }
        public string QrPath { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string HotelReservationNumber { get; set; }


    }
}

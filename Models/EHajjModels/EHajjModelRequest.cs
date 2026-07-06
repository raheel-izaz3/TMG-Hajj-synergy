namespace TMG.Models.EHajjModels
{
    public class EHajjModelRequest
    {
        public int EhajjContractId { get; set; }
        public int CompanyId { get; set; }
        public int PackageId { get; set; }
        public int HotelId { get; set; }
        public int FoodPackageId { get; set; }
        public int Capacity { get; set; }
        public string EhajjContractNumber { get; set; }
        public string VoucherNumber { get; set; }
        public float Amount { get; set; }
        public IFormFile? Contractpath { get; set; }
        public string? Contractpathstr { get; set; }
        public string? Voucherpathstr { get; set; }
        public IFormFile? Voucherpath { get; set; }
        public IFormFile? QrPath { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public List<int> RoomIds { get; set; }
        public List<int> Paxs { get; set; }
        public int RoomsQuantity { get; set; }
        public int PaxsQuantity { get; set; }
        public List<int> IndividualRoomsQuantity { get; set; }
        public List<int> IndividualPaxQuantity { get; set; }
    }
}

using TMG.Models.MiscModels;

namespace TMG.Models.PackageModels
{
    public class PackageGifts
    {
        public int PackageGiftsId { get; set; }

        public int PackageId { get; set; }
        public virtual Package Package { get; set; }

        public int? GiftId { get; set; }
        public virtual Gift Gift { get; set; }
    }
}

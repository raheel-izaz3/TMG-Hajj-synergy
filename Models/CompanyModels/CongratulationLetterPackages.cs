using TMG.Models.MiscModels;
using TMG.Models.PackageModels;

namespace TMG.Models.CompanyModels
{
    public class CongratulationLetterPackages
    {
        public int CongratulationLetterPackagesId { get; set; }
        public int CongratulationLetterId { get; set; }
        public CongratulationLetter CongratulationLetter { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

using iText.Kernel.Pdf;
using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadGiveAways
    {
        public int FamilyHeadGiveAwaysId { get; set; }
        
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHeads { get; set; }

        public int GiveAwayId { get; set; }
        public GiveAway GiveAways { get; set; } 
    }
}

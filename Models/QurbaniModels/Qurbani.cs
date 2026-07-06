using TMG.Models.FamilyModels;

namespace TMG.Models.QurbaniModels
{
    public class Qurbani
    {
        public int QurbaniId { get; set; }
        
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }


        public bool Active { get; set; }
    }
}

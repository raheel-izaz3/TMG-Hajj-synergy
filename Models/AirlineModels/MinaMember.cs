using TMG.Models.FamilyModels;

namespace TMG.Models.AirlineModels
{
    public class MinaMember
    {
        public int MinaMemberId { get; set; }
        public int MinaId { get; set; }
        public Mina Mina { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

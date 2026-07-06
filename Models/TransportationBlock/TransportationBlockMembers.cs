using TMG.Models.FamilyModels;

namespace TMG.Models.TransportationBlock
{
    public class TransportationBlockMembers
    {
        public int TransportationBlocksId { get; set; }
        public TransportationBlocks TransportationBlock{ get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

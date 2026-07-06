using TMG.Models.PackageModels;

namespace TMG.Models.TransportationBlock
{
    public class TranspotationBlockGroups
    {
        public int TranspotationBlockGroupsId { get; set; }
        public int TransportationBlocksId { get; set; }
        public TransportationBlocks TransportationBlock { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
}

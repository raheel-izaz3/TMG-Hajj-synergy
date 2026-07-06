using TMG.Models.FamilyModels;

namespace TMG.Models.EHajjModels
{
    public class EHajjContractMember
    {
        public int EHajjContractMemberId { get; set; }
        public int EhajjContractId { get; set; }
        public EHajjContract EhajjContract { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

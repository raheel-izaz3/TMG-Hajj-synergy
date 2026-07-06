using TMG.Models.FamilyModels;

namespace TMG.Models.GroupModels
{
    public class SubGroupMember
    {
        public int SubGroupId { get; set; }
        public SubGroup SubGroup { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }

        public int FamilyMembersId { get; set; }
        public FamilyMembers FamilyMembers { get; set; }
    }
}

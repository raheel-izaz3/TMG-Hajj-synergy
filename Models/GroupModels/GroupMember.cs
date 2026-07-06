using TMG.Models.FamilyModels;

namespace TMG.Models.GroupModels
{
    public class GroupMember
    {
        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

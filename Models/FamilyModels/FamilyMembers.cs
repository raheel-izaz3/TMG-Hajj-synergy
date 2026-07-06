using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.FamilyModels
{
    public class FamilyMembers
    {
        public int FamilyMembersId { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }

        [ForeignKey("FamilyHeadId")]
        public int HeadId { get; set; }
        public FamilyHead Head { get; set; }

        public bool IsHead { get; set; }
    }
}

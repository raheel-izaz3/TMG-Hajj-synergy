using TMG.Models.MemberModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadEmail
    {
        public int FamilyHeadEmailId { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int? MemberEmailTypeId { get; set; }
        public MemberEmailType MemberEmailType { get; set; }

        public string Email { get; set; }
        public bool Preferred { get; set; }

    }
}

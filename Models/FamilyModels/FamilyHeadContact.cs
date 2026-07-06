using TMG.Models.MemberModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadContact
    {
        public int FamilyHeadContactId { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int? MemberContactTypeId { get; set; }
        public MemberContactType MemberContactType { get; set; }

        public string Contact { get; set; }
        public bool Preferred { get; set; }

    }
}

using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.FamilyModels;

namespace TMG.Models.MiscModels
{
    public class IDCardList : BaseEntity
    {
        public int IDCardListId { get; set; }
        public string Name { get; set; }

        [NotMapped] public List<int> FamilyHeadIds { get; set; }
        [NotMapped] public List<bool> FamilyHeadSelected { get; set; }
    }

    public class ListFamily
    {
        public int ListFamilyId { get; set; }
        public int IDCardListId { get; set; }
        public IDCardList IDCardList { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

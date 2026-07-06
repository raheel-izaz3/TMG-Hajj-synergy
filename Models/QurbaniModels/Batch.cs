using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;

namespace TMG.Models.QurbaniModels
{
    public class Batch : BaseEntity
    {
        public int BatchId { get; set; }
        public string BatchName { get; set; }

        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

        [NotMapped] public List<int> FamilyHeadIds { get; set; }
        [NotMapped] public List<bool> FamilyHeadSelected { get; set; }
    }

    public class BatchList
    {
        public int BatchListId { get; set; }

        public int BatchId { get; set; }
        public Batch Batch { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class NomineeRelation: BaseEntity
    {
        public int NomineeRelationId { get; set; }

        [Required]
        [Display(Prompt = "Enter Nominee Relation...")]
        public string Relation { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class FamilyHeadRelation: BaseEntity
    {
        public int FamilyHeadRelationId { get; set; }

        [Required, Display(Name = "Relation", Prompt = "Enter Relation...")]
        public string Relation { get; set; }

    }
}

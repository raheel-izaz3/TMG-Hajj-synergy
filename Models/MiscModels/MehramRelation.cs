using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class MehramRelation: BaseEntity
    {
        public int MehramRelationId { get; set; }

        [Display(Name = "Code", Prompt = "Enter Code here...")]
        public string? Code { get; set; }

        [Required, Display(Name = "Relation", Prompt = "Enter Relation here...")]
        public string Relation { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.GroupModels
{
    public class CoordinatorRole: BaseEntity
    {
        public int CoordinatorRoleId { get; set; }

        [Required, Display(Name = "Cooridinator Role", Prompt = "Enter Coordinator's Role...")]
        public string Role { get; set; }
    }
}

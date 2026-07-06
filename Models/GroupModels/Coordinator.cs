using System.ComponentModel.DataAnnotations;

namespace TMG.Models.GroupModels
{
    public class Coordinator: BaseEntity
    {
        public int CoordinatorId { get; set; }

        [Required, Display(Name = "Group", Prompt = "Enter Group...")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required, Display(Name = "Role", Prompt = "Enter Role...")]
        public int CoordinatorRoleId { get; set; }
        public CoordinatorRole CoordinatorRole { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "Local Number", Prompt = "Enter Local Number...")]
        public string LocalNumber { get; set; }

        [Required, Display(Name = "International Number", Prompt = "Enter International Number...")]
        public string InternationalNumber { get; set; }

    }
}

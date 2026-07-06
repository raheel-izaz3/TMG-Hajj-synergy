using System.ComponentModel.DataAnnotations;

namespace TMG.Models.RailwayModels
{
    public class Railway : BaseEntity
    {
        public int RailwayId { get; set; }

        [Required, Display(Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Prompt = "Enter Code...")]
        public string Code { get; set; }
    }
}

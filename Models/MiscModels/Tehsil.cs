using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class Tehsil: BaseEntity
    {
        public int TehsilId { get; set; }

        [Required, Display(Name = "Tehsil Name", Prompt = "Enter Tehsil Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "Tehsil Code", Prompt = "Enter Tehsil Code...")]
        public string Code { get; set; }
    }
}

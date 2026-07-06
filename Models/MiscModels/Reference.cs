using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class Reference: BaseEntity
    {
        public int ReferenceId { get; set; }

        [Required, Display(Name = "Code", Prompt = "Enter Code...")]
        public string ReferenceCode { get; set; }
        
        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string ReferenceName { get; set; }
    }
}

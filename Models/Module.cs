using System.ComponentModel.DataAnnotations;

namespace TMG.Models
{
    public class Module: BaseEntity
    {
        public int ModuleId { get; set; }
        
        [Required]
        public string ModuleName { get; set; }
    }
}

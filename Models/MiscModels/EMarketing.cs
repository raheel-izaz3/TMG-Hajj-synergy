using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class EMarketing : BaseEntity
    {
        public int EMarketingId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string BackgroundImage { get; set; }
        [Required]
        public string Content { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class IssuingAgent : BaseEntity
    {
        public int IssuingAgentId { get; set; }
        [Required]
        public string AgentName { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

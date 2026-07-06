using TMG.Models;

namespace TMG.Models
{
    public class HajjSeason: BaseEntity
    {
        public int HajjSeasonId { get; set; }
        public string Year { get; set; }
        public bool Active { get; set; }

        public bool IsActive => Active;
        public bool IsInActive => !Active;
    }
}

namespace TMG.Models.MiscModels
{
    public class HajjSeasons : BaseEntity
    {
        public int HajjSeasonsId { get; set; }
        public string Season { get; set; }
        public string Hijri { get; set; }
    }
}

namespace TMG.Models.MiscModels
{
    public class Fiqah: BaseEntity
    {
        public int FiqahId { get; set; }
        public string Name { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

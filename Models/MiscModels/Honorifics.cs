using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class Honorifics: BaseEntity
    {
        public int HonorificsId { get; set; }

        [Required, Display(Name = "Title", Prompt = "Enter Title...")]
        public string Title { get; set; }

        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

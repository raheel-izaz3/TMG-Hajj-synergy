using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class Gift: BaseEntity
    {
        public int GiftId { get; set; }

        [Required, Display(Name = "Gift Code", Prompt = "Enter Gift Code...")]
        public string GiftCode { get; set; }

        [Required, Display(Name = "Gift", Prompt = "Enter Gift Name...")]
        public string Giveaway { get; set; }

        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }

    }
}

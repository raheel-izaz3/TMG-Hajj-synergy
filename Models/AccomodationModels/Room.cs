using System.ComponentModel.DataAnnotations;
using TMG.Models.MiscModels;

namespace TMG.Models.AccomodationModels
{
    public class Room: BaseEntity
    {
        public int RoomId { get; set; }

        [Required, Display(Name = "Room Name", Prompt = "Enter Room Name...")]
        public string RoomName { get; set; }

        [Required, Display(Name = "Room Code", Prompt = "Enter Room Code...")]
        public string RoomCode { get; set; }

        [Required, Display(Name = "PAX", Prompt = "Enter PAX...")]
        public int PAX { get; set; }
        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }
    }
}

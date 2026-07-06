using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class RoomBedType: BaseEntity
    {
        public int RoomBedTypeId { get; set; }

        [Required, Display(Name = "Room Bed Types", Prompt = "Enter Bed Types...")]
        public string BedType { get; set; }
    }
}

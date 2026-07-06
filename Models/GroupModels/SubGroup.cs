using TMG.Models.AccomodationModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;
using TMG.Models.ReservationModels;

namespace TMG.Models.GroupModels
{
    public class SubGroup : BaseEntity
    {
        public int SubGroupId { get; set; }

        [Required, Display(Name = "Place")]
        public int PlaceId { get; set; }
        public Place Place { get; set; }

        [Required, Display(Name = "Group")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Display(Name = "Merge w/ other subgroup")]
        public int? SubGroupMergeId { get; set; }
        public SubGroup SubGroupMerge { get; set; }

        [Required, Display(Name = "Contract", Prompt = "Enter Contract Number...")]
        public string Contract { get; set; }

        [Required, Display(Name = "Code", Prompt = "Enter Code...")]
        public string Code { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "PAX", Prompt = "Enter PAX...")]
        public int PAX { get; set; }

        public int Selected { get; set; } = 0;

        [NotMapped] public List<int> RoomIds { get; set; }
        [NotMapped] public List<int> Quantities { get; set; }
        [NotMapped] public List<BathroomType?> BathroomType { get; set; }
        [NotMapped] public List<int> RoomViewId { get; set; }
        [NotMapped] public List<int> POHotelsId { get; set; }
        [NotMapped] public List<int> PLaceIds { get; set; }
        [NotMapped] public List<int> POHotelRoomsId { get; set; }
        [NotMapped] public List<string> RoomNumber { get; set; }
        [NotMapped] public List<int> RoomDetailsId { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

    }

}

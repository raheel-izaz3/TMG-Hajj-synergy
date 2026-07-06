using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.AccomodationModels;
using TMG.Models.GroupModels;
using TMG.Models.MiscModels;
using TMG.Models.POModels;
using TMG.Models.ReservationModels;


namespace TMG.Models.FormModels
{
    public class SubGroupRoomsEditModel
    {
        public int SubGroupId { get; set; }

        [Required, Display(Name = "Contract", Prompt = "Enter Contract Number...")]
        public string Contract { get; set; }

        [Required, Display(Name = "Code", Prompt = "Enter Code...")]
        public string Code { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "PAX", Prompt = "Enter PAX...")]
        public int PAX { get; set; }
        [Required, Display(Name = "Group")]
        public int GroupId { get; set; }

        [Display(Name = "Merge w/ other subgroup")]
        public int? SubGroupMergeId { get; set; }
        public int PlaceIds { get; set; }

        public List<string> RoomNumber { get; set; } = new List<string>();
        public List<int> RoomId { get; set; } = new List<int>();
        public List<BathroomType?> BathroomType { get; set; } = new List<BathroomType?>();
        public List<int?> RoomViewId { get; set; } = new List<int?>();
        public List<int?> POHotelsId { get; set; } = new List<int?>();
        public List<int?> POHotelRoomsId { get; set; } = new List<int?>();
        public int? PlaceId { get; set; }
        public List<int?> RoomDetailsId { get; set; } = new List<int?>();
        public List<int> SelectedRoomDetailsId { get; set; } = new List<int>();
        public List<SubGroupRoomsModel> SubGroupRooms { get; set; }

    }

    public class SubGroupRoomsModel
    {
        public string RoomNumber { get; set; } 

        public int RoomId { get; set; } 
        public BathroomType? BathroomType { get; set; } 
        public int? RoomViewId { get; set; }
        public int Quantity { get; set; } 

        public int? POHotelsId { get; set; } 
        public int? POHotelRoomsId { get; set; } 
        public int? PlaceId { get; set; } 
        public int? RoomDetailsId { get; set; } 
    }
}

using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.POModels;

namespace TMG.Models.ReservationModels
{
    public enum BathroomType
    {
        Indian, English, Both
    }
    public class FloorRoom: BaseEntity
    {
        public int FloorRoomId { get; set; }

        [Display(Name = "Room Number", Prompt = "Enter Room Number")]
        public string RoomNumber { get; set; }

        public int FloorId { get; set; }
        public Floor Floor { get; set; }

        [Required]
        public BathroomType? BathroomType { get; set; }

        [Required, Display(Name = "PAX", Prompt = "Enter PAX...")]
        public int PAX { get; set; }

        [Display(Name = "Bed Type")]
        public int? RoomBedTypeId { get; set; }
        public RoomBedType RoomBedType { get; set; }

        [Display(Name = "Room")]
        public int? RoomId { get; set; }
        
        public int? RoomViewId { get; set; }
        public Room Room { get; set; }

        [NotMapped]
        public List<string> Rooms { get; set; }

        [NotMapped]
        public List<BathroomType> BathRoom { get; set; }
        [NotMapped]
        public List<int> BedType { get; set; }
        [NotMapped]
        public List<int> RoomType { get; set; }
        [NotMapped]
        public List<int> RoomView { get; set; }

        public int? POHotelRoomsId { get; set; }
        public POHotelRooms POHotelRooms { get; set; }

    }
}

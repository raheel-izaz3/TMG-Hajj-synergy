using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.OperationalGroupModels
{
    public enum GroupCategory
    {
        Flight = 1,
        Bus = 2,
        Hotel = 3,
        Route = 4,
        General = 5
    }

    public class OperationalGroup
    {
        [Key]
        public int Id { get; set; }

        public int HajjSeasonsId { get; set; }

        public GroupCategory Category { get; set; } = GroupCategory.Flight;

        // --- Section 1: Basic Group Details ---
        [Required]
        [StringLength(150)]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [StringLength(150)]
        [Display(Name = "Group Leader Name")]
        public string GroupLeaderName { get; set; }

        [StringLength(50)]
        [Display(Name = "Leader Mobile Number")]
        public string LeaderMobileNumber { get; set; }

        // --- Section 2: Resource Allocation ---
        // Flight Resources
        public int? AirlineId { get; set; }
        public int? FlightId { get; set; }

        [StringLength(50)]
        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "PNR Reference")]
        public string PNRReference { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Flight Date")]
        public DateTime? FlightDate { get; set; }

        // Bus Resources
        public int? VehicleId { get; set; }

        [StringLength(100)]
        [Display(Name = "Bus Identifier / Plate No")]
        public string BusIdentifierPlateNo { get; set; }

        [Display(Name = "Bus Capacity")]
        public int BusCapacity { get; set; } = 45;

        // Hotel Resources
        public int? HotelId { get; set; }
        public int? RoomId { get; set; }

        // Route Resources
        public int? RouteId { get; set; }
        public int? ItineraryScheduleId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Trashed { get; set; } = false;

        public ICollection<OperationalGroupMember> Members { get; set; } = new List<OperationalGroupMember>();
    }
}

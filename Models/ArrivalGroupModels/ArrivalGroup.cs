using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.ArrivalGroupModels
{
    public class ArrivalGroup
    {
        [Key]
        public int Id { get; set; }

        public int HajjSeasonsId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        public int? AirlineId { get; set; }
        public int? FlightId { get; set; }

        [StringLength(50)]
        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; }

        [Display(Name = "Flight Arrival Date")]
        [DataType(DataType.Date)]
        public DateTime? FlightArrivalDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Flight Arrival Time")]
        public string FlightArrivalTime { get; set; }

        [StringLength(50)]
        [Display(Name = "PNR Number")]
        public string PNRNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Departure City")]
        public string DepartureCity { get; set; }

        [StringLength(100)]
        [Display(Name = "Arrival City")]
        public string ArrivalCity { get; set; }

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Trashed { get; set; } = false;

        public ICollection<ArrivalGroupMember> Members { get; set; } = new List<ArrivalGroupMember>();
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.ArrivalGroupModels
{
    public class ArrivalGroupMember
    {
        [Key]
        public int Id { get; set; }

        public int ArrivalGroupId { get; set; }

        public int? FamilyHeadId { get; set; }
        public int? FamilyMemberId { get; set; }

        public bool IsHead { get; set; }

        [StringLength(200)]
        public string FullName { get; set; }

        [StringLength(50)]
        public string PassportNumber { get; set; }

        [StringLength(50)]
        public string CNIC { get; set; }

        [StringLength(50)]
        public string BookingCode { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool Trashed { get; set; } = false;

        [ForeignKey("ArrivalGroupId")]
        public ArrivalGroup ArrivalGroup { get; set; }
    }
}

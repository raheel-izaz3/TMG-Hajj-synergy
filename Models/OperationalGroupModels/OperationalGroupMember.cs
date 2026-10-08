using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.OperationalGroupModels
{
    public class OperationalGroupMember
    {
        [Key]
        public int Id { get; set; }

        public int OperationalGroupId { get; set; }

        public int? FamilyHeadId { get; set; }
        public int? FamilyMemberId { get; set; }

        public bool IsHead { get; set; }

        [StringLength(200)]
        public string FullName { get; set; }

        [StringLength(50)]
        public string CNIC { get; set; }

        [StringLength(50)]
        public string PassportNumber { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        [StringLength(50)]
        public string BookingCode { get; set; }

        [StringLength(150)]
        public string ClientCompanyName { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool Trashed { get; set; } = false;

        [ForeignKey("OperationalGroupId")]
        public OperationalGroup OperationalGroup { get; set; }
    }
}

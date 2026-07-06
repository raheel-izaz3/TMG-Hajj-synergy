using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models
{
    public class Role : BaseEntity
    {
        [Comment("Primary Key of Roles")]
        public int RoleId { get; set; }

        [Required]
        [DisplayName("Role Name")]
        public string RoleName { get; set; }

        [DisplayName("User Module")]
        public bool UserModuleAllowed { get; set; } = false;

        [DisplayName("Company Module")]
        public bool CompanyModuleAllowed { get; set; } = false;

        [DisplayName("Accommodation Module")]
        public bool AccommodationModuleAllowed { get; set; } = false;

        [DisplayName("Misc Module")]
        public bool MiscModuleAllowed { get; set; } = false;

        [DisplayName("Package Module")]
        public bool PackageModuleAllowed { get; set; } = false;

        [DisplayName("Accounts Module")]
        public bool AccountsModuleAllowed { get; set; } = false;

        [DisplayName("Transportation Module")]
        public bool TransportationModuleAllowed { get; set; } = false;

        [DisplayName("Family Module")]
        public bool FamilyModuleAllowed { get; set; } = false;

        [DisplayName("Qurbani Module")]
        public bool QurbaniModuleAllowed { get; set; } = false;

        [DisplayName("Groups Module")]
        public bool GroupsModuleAllowed { get; set; } = false;

        [DisplayName("Reservation Module")]
        public bool ReservationModuleAllowed { get; set; } = false;

        [DisplayName("Receipt Module")]
        public bool ReceiptModuleAllowed { get; set; } = false;

        [DisplayName("Demand Module")]
        public bool DemandModuleAllowed { get; set; } = false;

        [DisplayName("Purchase Order")]
        public bool PurchaseOrderModuleAllowed { get; set; } = false;

        //[DisplayName("Reports Module")]
        //public bool ReportsModuleAllowed { get; set; } = false;

        //[DisplayName("Logistics Module")]
        //public bool LogisticsModuleAllowed { get; set; } = false;

        //[DisplayName("Registration Module")]
        //public bool RegistrationModuleAllowed { get; set; } = false;

        //[DisplayName("Member Module")]
        //public bool MemberModuleAllowed { get; set; } = false;

        public List<RolePermissions> Permissions { get; set; } = new();

    }
}

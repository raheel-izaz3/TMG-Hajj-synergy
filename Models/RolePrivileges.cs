
using TMG.ManagerClasses;

namespace TMG.Models.UserModels
{
    public class RolePrivileges
    {
        public int RolePrivilegesId { get; set; }
        public int RoleId { get; set; }
        public string Privilege { get; set; }
    }
}

namespace TMG.Models
{
    public class RolePermissions
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public bool Allowed { get; set; }

        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }
    }
}

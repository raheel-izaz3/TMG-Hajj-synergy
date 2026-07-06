namespace TMG.Models
{
    public class RoleModules
    {
        public int RoleId { get; set; }
        public int ModuleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual Module Module { get; set; }
    }
}

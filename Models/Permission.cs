namespace TMG.Models
{
    public class Permission: BaseEntity
    {
        public int PermissionId { get; set; }
        public int SubModuleId { get; set; }
        public int ModuleId { get; set; }
        public string PermissionName { get; set; }

        public virtual SubModule SubModule { get; set; }
        public virtual Module Module { get; set; }
    }
}

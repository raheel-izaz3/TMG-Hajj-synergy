namespace TMG.Models
{
    public class SubModule: BaseEntity
    {
        public int SubModuleId { get; set; }
        public string SubModuleName { get; set;}

        public int ModuleId { get; set; }
        public virtual Module Module { get; set; }
    }
}

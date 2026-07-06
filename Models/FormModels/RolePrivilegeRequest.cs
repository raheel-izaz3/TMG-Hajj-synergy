namespace TMG.Models.FormModels
{
    public class RolePrivilegeRequest
    {
        public int RoleId { get; set; }

        public List<bool> Permissions { get; set; } = new List<bool>();
        
    }
}

using TMG.Models.MiscModels;

namespace TMG.Models.CompanyModels
{
    public class CompanyDepartment : BaseEntity
    {
        public int CompanyDepartmentId { get; set; }
        public string CompanyDepartmentName { get; set; }
        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

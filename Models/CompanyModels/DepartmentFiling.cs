using TMG.Models.MiscModels;

namespace TMG.Models.CompanyModels
{
    public class DepartmentFiling : BaseEntity
    {
        public int DepartmentFilingId { get; set; }
        public DateTime Date { get; set; }
        public string Document { get; set; }
        public string Note { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int CompanyDepartmentId { get; set; }
        public CompanyDepartment CompanyDepartment { get; set; }
        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeaons { get; set; }
    }
}

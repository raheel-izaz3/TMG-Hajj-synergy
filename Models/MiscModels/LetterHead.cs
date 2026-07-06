using TMG.Models.CompanyModels;

namespace TMG.Models.MiscModels
{
    public class LetterHead:BaseEntity
    {
        public int LetterHeadId { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int? CompanyDepartmentId { get; set; }
        public CompanyDepartment Department { get; set; }
        public string Data { get; set; }
        public string Image { get; set; }
        public string FootImage { get; set; }
    }
}

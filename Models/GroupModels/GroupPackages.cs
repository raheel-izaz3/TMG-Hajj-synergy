using TMG.Models.AirlineModels;
using TMG.Models.PackageModels;

namespace TMG.Models.GroupModels
{
    public class GroupPackages
    {
        public int GroupPackagesId { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
    }
   
}

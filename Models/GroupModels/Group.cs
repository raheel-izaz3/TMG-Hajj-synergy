using TMG.Models.PackageModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;

namespace TMG.Models.GroupModels
{
    public class  Group : BaseEntity
    {
        public int GroupId { get; set; }

        [Required, Display(Name = "Code", Prompt = "Enter Code...")]
        public string Code { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "PAX", Prompt = "Enter PAX...")]
        public int PAX { get; set; }

        [Required, Display(Name = "Package")]
        public int PackageId { get; set; } = 2; // TODO remove this
        public Package Package { get; set; }

        public int Selected { get; set; } = 0;

        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }
        [Required]
        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

        public ICollection<GroupPackages> GroupPackages { get; set; }
    }
}

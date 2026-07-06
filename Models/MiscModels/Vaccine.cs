using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class Vaccine: BaseEntity
    {
        public int VaccineId { get; set; }

        [Required, Display(Name = "Name", Prompt = "Enter Name...")]
        public string VaccineName { get; set; }

        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using TMG.Models.MiscModels;

namespace TMG.Models.LogisticModels
{
    public class Vehicle : BaseEntity
    {
        public int VehicleId { get; set; }

        [Required, Display(Name = "Vehicle Type", Prompt = "Enter Vehicle Type...")]
        public string VehicleType { get; set; }

        public int PAX { get; set; }
        public float Price { get; set; }

        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }
    }
}

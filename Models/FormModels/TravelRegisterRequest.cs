using System.ComponentModel.DataAnnotations;
using TMG.Models.FamilyModels;

namespace TMG.Models.FormModels
{
    public class TravelRegisterRequest
    {
        public int TravelRouteId { get; set; }
        [Required, Display(Name = "Route #", Prompt = "Enter Route Number...")]
        public int RouteNumber { get; set; }


        [Required, Display(Name = "Route Name", Prompt = "Enter Route Name...")]
        public string RouteName { get; set; }
        
        // [Required, Display(Name = "Standard Price", Prompt = "Enter Standard Price...")]
        // public float StandardPrice { get; set; }
        
        // [Required, Display(Name = "R.O.E", Prompt = "Enter R.O.E...")]
        // public float ROE { get; set; }

        public List<int> RouteIds { get; set; } = new List<int>();
        public List<bool> RoutesSelected { get; set; } = new List<bool>();
        public List<int> VehicleIds { get; set; } = new List<int>();
        public List<float?> Prices { get; set; } = new List<float?>();
        public List<DateTime?> CheckIns { get; set; } = new List<DateTime?>();
        public List<DateTime?> CheckInsTime { get; set; }
        public List<FamilyTranasport> TransportTypes { get; set; }
        // public List<DateTime?> CheckOuts { get; set; } = new List<DateTime?>();
        // public List<float?> SARPrices { get; set; } = new List<float?>();

    }
}

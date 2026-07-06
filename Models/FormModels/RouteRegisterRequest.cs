using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels
{
    public class RouteRegisterRequest
    {

        [Required, Display(Name = "Start Place", Prompt = "Enter Start Place...")]
        public string StartPlace { get; set; }

        [Required, Display(Name = "End Place", Prompt = "Enter End Place...")]
        public string Destination { get; set; }

        public List<int> VehicleIds { get; set; }
        public List<float> Prices { get; set; }
    }
}

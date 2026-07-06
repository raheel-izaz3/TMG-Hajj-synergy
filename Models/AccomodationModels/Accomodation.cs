using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccomodationModels
{
    public class Accomodation: BaseEntity
    {
        public int AccomodationId { get; set; }

        [Required, Display(Name = "Accomodation Type", Prompt = "Enter Accomodation Type...")]
        public string AccomodationName { get; set; }
    }
}

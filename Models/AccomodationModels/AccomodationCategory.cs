using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccomodationModels
{
    public class AccomodationCategory: BaseEntity
    {
        public int AccomodationCategoryId { get; set; }

        [Required, Display(Name = "Saudi Star Rating", Prompt = "Enter Rating...")]
        public string Category { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccomodationModels
{
    public class FoodPackage: BaseEntity
    {
        public int FoodPackageId { get; set; }

        [Required, Display(Name = "Food Package Name", Prompt = "Enter Package Name...")]
        public string Package { get; set; }
    }
}

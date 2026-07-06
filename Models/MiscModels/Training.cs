using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class Training: BaseEntity
    {
        public int TrainingId { get; set; }

        [Required, Display(Name = "Training Code", Prompt = "Enter Training Code...")]
        public string TrainingCode { get; set; }

        [Required, Display(Name = "Training Session", Prompt = "Enter Training Session...")]
        public string TrainingSession { get; set; }

        [Required, Display(Name = "Expected Training Time", Prompt = "Enter Expected Training Time...")]
        public string ExpectedTrainingTime { get; set; }

        [Required, Display(Name = "Expected Training Venue", Prompt = "Enter Expected Training Venue...")]
        public string ExpectedTrainingVenue { get; set; }

        //public int? HajjSeasonsId { get; set; }
        //public HajjSeasons HajjSeasons { get; set; }


    }
}

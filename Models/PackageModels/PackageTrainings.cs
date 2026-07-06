using TMG.Models.MiscModels;

namespace TMG.Models.PackageModels
{
    public class PackageTrainings
    {
        public int PackageTrainingsId { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }

        public int? TrainingId { get; set; }
        public Training Training { get; set; }
    }
}

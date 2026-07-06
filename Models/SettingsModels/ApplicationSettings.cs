using System.ComponentModel.DataAnnotations;
using TMG.Models.MiscModels;

namespace TMG.Models.SettingsModels
{
    public class ApplicationSettings
    {
        public int ApplicationSettingsId { get; set; }

        [Display(Name = "SAR ROE", Prompt = "Enter ROE...")]
        public float SARROE { get; set; }

        [Display(Name = "USD ROE", Prompt = "Enter ROE...")]
        public float USDROE { get; set; }

        [Display(Name = "Vaccine Count", Prompt = "Enter Vaccine Count...")]
        public byte VaccineCount { get; set; }
        public int FamilyCount { get; set; }
        public int IndividualCount { get; set; }

        public bool EnableQurbani { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }

    }
}

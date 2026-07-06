using TMG.Models.MiscModels;

namespace TMG.Models.CompanyModels
{
    public class QuotaCompany
    {
        public int QuotaCompanyId { get; set; }
        public int? Quota { get; set; }
        
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

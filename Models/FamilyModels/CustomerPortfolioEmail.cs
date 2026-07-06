using TMG.Models.MemberModels;

namespace TMG.Models.FamilyModels
{
    public class CustomerPortfolioEmail
    {
        public int CustomerPortfolioEmailId { get; set; }
        public int CustomerPortfolioId { get; set; }
        public CustomerPortfolio CustomerPortfolio { get; set; }
        public int? MemberEmailTypeId { get; set; }
        public MemberEmailType MemberEmailType { get; set; }

        public string Email { get; set; }
        public bool Preferred { get; set; }

    }
}

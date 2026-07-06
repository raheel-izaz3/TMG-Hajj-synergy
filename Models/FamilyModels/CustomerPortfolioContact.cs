using TMG.Models.MemberModels;

namespace TMG.Models.FamilyModels
{
    public class CustomerPortfolioContact
    {
        public int CustomerPortfolioContactId { get; set; }
        public int CustomerPortfolioId { get; set; }
        public CustomerPortfolio CustomerPortfolio { get; set; }
        public int? MemberContactTypeId { get; set; }
        public MemberContactType MemberContactType { get; set; }

        public string Contact { get; set; }
        public bool Preferred { get; set; }

    }
}

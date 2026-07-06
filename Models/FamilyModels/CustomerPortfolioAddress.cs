using TMG.Models.MemberModels;
using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public class CustomerPortfolioAddress
    {
        public int CustomerPortfolioAddressId { get; set; }
        public int CustomerPortfolioId { get; set; }
        public CustomerPortfolio CustomerPortfolio { get; set; }
        public int? MemberAddressTypeId { get; set; }
        public MemberAddressType MemberAddressType { get; set; }

        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string PoBox { get; set; }
        public int? TehsilId { get; set; }
        public Tehsil Tehsil { get; set; }
        public bool Preferred { get; set; }

    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.FamilyModels
{
    public class CustomerPortfolioMembers
    {
        public int CustomerPortfolioMembersId { get; set; }

        public int CustomerPortfolioId { get; set; }
        public CustomerPortfolio CustomerPortfolio { get; set; }

        [ForeignKey("CustomerPortfolioId")]
        public int HeadId { get; set; }
        public CustomerPortfolio Head { get; set; }

        public bool IsHead { get; set; }
    }
}

using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public class CustomerPortfolioDocuments
    {
        public int CustomerPortfolioDocumentsId { get; set; }

        public int CustomerPortfolioId { get; set; }
        public CustomerPortfolio CustomerPortfolio { get; set; }

        public int RequiredDocumentId { get; set; }
        public RequiredDocument RequiredDocument { get; set; }

        public bool Recieved { get; set; }

        public string Remarks { get; set; }
    }
}

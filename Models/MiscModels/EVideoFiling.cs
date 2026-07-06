namespace TMG.Models.MiscModels
{
    public class EVideoFiling : BaseEntity
    {
        public int EVideoFilingId { get; set; }
        public string Name { get; set; }
        public string Video { get; set; }
        public string DocumentType { get; set; }
        public int? MediaDocumentId { get; set; }
        public MediaDocument MediaDocument { get; set; }
    }
}

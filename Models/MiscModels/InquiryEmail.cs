namespace TMG.Models.MiscModels
{
    public class InquiryEmail:BaseEntity
    {
        public int InquiryEmailId { get; set; }
        public string NoOfDays { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
    }
}

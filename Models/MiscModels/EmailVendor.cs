namespace TMG.Models.MiscModels
{
    public class EmailVendor : BaseEntity
    {
        public int EmailVendorId { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Attachment { get; set; }
        public string Message { get; set; }
    }
}

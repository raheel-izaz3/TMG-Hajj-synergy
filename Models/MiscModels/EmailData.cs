namespace TMG.Models.MiscModels
{
    public class EmailData : BaseEntity
    {
        public int EmailDataId { get; set; }
        public int? Day { get; set; }
        public string Subject { get; set; }
        public DateTime? SpecificDate { get; set; }
        public string Email { get; set; }
    }
}

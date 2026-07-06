namespace TMG.Models.MiscModels
{
    public class Country : BaseEntity
    {
        public int CountryId { get; set; }
        public string Name { get; set; }

        public int? DestinationId { get; set; }
        public Destination Destination { get; set; }
    }
}

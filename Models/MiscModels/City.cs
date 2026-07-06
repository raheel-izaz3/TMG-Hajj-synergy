namespace TMG.Models.MiscModels
{
    public class City : BaseEntity
    {
        public int CityId { get; set; }
        public string Name { get; set; }

        public int? CountryId { get; set; }
        public Country Country { get; set; }
    }
}

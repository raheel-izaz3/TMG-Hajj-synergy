namespace TMG.Models.AirlineModels
{
    public record FlightIndexModel
    {
        public List<FlightIndexEntryModel> Entries { get; set; } = new List<FlightIndexEntryModel>();
    }
}
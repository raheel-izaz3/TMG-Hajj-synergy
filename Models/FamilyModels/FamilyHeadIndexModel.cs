namespace TMG.Models.FamilyModels
{
    public record FamilyHeadIndexModel
    {
        public int FamilyCount { get; set; }
        public int FamilyMemberCount { get; set; }
        public int TentativeCount { get; set; }
        public int BookedCount { get; set; }
        public int CancelledCount { get; set; }
        public int BioImageCount { get; set; }
        public int ENumberVerifiedCount { get; set; }
        public int EHajjVerifiedCount { get; set; }
        public int FormVerifiedCount { get; set; }
        public bool IsAdmin { get; set; }
        public List<FamilyHeadIndexEntryModel> Entries { get; set; } = new();
        public List<DataFormIndexEntryModel> DataEntries { get; set; } = new();
    }

}

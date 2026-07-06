namespace TMG.Models.FamilyModels
{
    public class BioSaveModel
    {
        public int FamilyHeadId { get; set; }
        public string ENo { get; set; }
        public IFormFile File { get; set; }
        public int Error { get; set; }
    }

}

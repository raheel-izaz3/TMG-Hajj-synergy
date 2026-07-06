namespace TMG.Models.FamilyModels
{
    public record FamilyBiometricUploadModel
    {
        public bool Saved { get; set; }
        public int FamilyHeadId { get; set; }
        public int SNo { get; set; }
        public string FullName { get; set; }
        public string CID { get; set; }
        public string Eno { get; set; }
        public IFormFile File { get; set; }
    }

}

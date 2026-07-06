namespace TMG.Models.FamilyModels
{
    public record VerifyBioViewModel
    {
        public int FamilyHeadId { get; set; }
        public int SNo { get; set; }
        public string FullName { get; set; }
        public string CID { get; set; }
        public string Eno { get; set; }
        public string Image { get; set; }
        public bool Saved { get; set; }

    }

}

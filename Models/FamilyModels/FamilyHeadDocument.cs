using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadDocument
    {
        public int FamilyHeadDocumentId { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }

        public int RequiredDocumentId { get; set; }
        public RequiredDocument RequiredDocument { get; set; }

        public bool Recieved { get; set; }

        //public string? CnicImage { get; set; }
        //public string? BloodProof { get; set; }
        //public string? NomineeCnic { get; set; }

        public string Remarks { get; set; }
    }
}

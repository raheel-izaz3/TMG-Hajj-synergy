namespace TMG.Models.FamilyModels
{
    public class FamilyHeadRemark
    {
        public int FamilyHeadRemarkId { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public string Remarks { get; set; }
    }
}

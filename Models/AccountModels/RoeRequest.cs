using TMG.Models.FamilyModels;

namespace TMG.Models.AccountModels
{
   
    public enum Status
    {
        Pending,Approved,Reject
    }

    public class RoeRequest : BaseEntity
    {
        public RoeRequest()
        {
            Status = Status.Pending;
        }

        public int RoeRequestId { get; set; }
        public int? CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public Currencies Currency { get; set; }
        public float PKRROE { get; set; }
        public float SARROE { get; set; }
        public float USDROE { get; set; }
        public Status Status { get; set; }
        public string Remarks { get; set; }
        public int? FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

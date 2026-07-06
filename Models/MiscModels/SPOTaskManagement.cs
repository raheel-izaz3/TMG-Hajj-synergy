namespace TMG.Models.MiscModels
{
    public class SPOTaskManagement : BaseEntity
    {
        public int SPOTaskManagementId { get; set; }
        public int? AssignedSPOId { get; set; }
        public SPO AssignedSPO { get; set; }

        public int? AssignedById { get; set; }
        public SPO AssignedBy { get; set; }

        public DateTime? DateOfAssigned { get; set; }
        public string TaskFile { get; set; }
        public string Task { get; set; }
    }
}

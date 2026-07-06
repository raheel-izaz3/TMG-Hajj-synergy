using TMG.Models.FamilyModels;

namespace TMG.Models.AirlineModels
{
    public class PNRTrainMembers
    {
        public int PNRTrainMembersId { get; set; }
        public int PNRTrainId { get; set; }
        public PNRTrain PNRTrain { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
    }
}

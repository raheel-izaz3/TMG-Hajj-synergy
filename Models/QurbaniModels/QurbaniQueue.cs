
using TMG.Models.FamilyModels;

namespace TMG.Models.QurbaniModels;
public class QurbaniQueue
{
    public int QurbaniQueueId { get; set; }

    public int FamilyHeadId { get; set; }
    public FamilyHead FamilyHead { get; set; }

    public DateTime StartTime { get; set; }


    public int? QurbaniBatchId { get; set; }
    public QurbaniBatch QurbaniBatch { get; set; }

}

using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadHajjAppFormModel
{
    public string FormNumber { get; set; }

    public string HajjAppSerialNumber { get; set; }

    public string HajjApplicationNumber { get; set; }

    public string GroupCoordAppNumber { get; set; }

    public string SubstitutedAgainstAppNumber { get; set; }

    public string ENumber { get; set; }

    public string EHajjId { get; set; }

    public string EHajjSerial { get; set; }

    public string MofaNumber { get; set; }

    public string TicketNumber { get; set; }

    public string VisaNumber { get; set; }

    public bool PerformedHajjInLastFiveYears { get; set; }

    public bool WantToPerformHajjEBadal { get; set; }

    public bool WantToPerformAsGroupWorker { get; set; }

    public bool Eligible { get; set; }

    public List<int> RequiredDocumentIds { get; set; }

    public List<bool> Recieved { get; set; }

    //public IFormFile NomineeCnic { get; set; }

    public List<string> DocumentRemarks { get; set; }

}
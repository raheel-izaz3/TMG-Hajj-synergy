using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.QurbaniModels;

public class QurbaniBatch : BaseEntity
{
    public int QurbaniBatchId { get; set; }

    [Required, Display(Name = "List Name", Prompt = "Enter List Name...")]
    public string Name { get; set; }

    public string WhatsappQrCodePath { get; set; }

    [NotMapped, Display(Name = "Whatsapp QR Code")]
    public IFormFile WhatsappQrCodeFile { get; set; }

    [DataType(DataType.Url)]
    [Display(Name = "Whatsapp Group Link (https://url)", Prompt = "Enter Group Link e.g.https://url...")]
    public string WhatsappGroupLink { get; set; }

    public int Capacity { get; set; }
    public bool Stopped { get; set; }

    public int DurationInMinutes { get; set; } = 360;

    public bool Completed { get; set; }

    [NotMapped] public List<int> FamilyHeadIds { get; set; }
    [NotMapped] public List<bool> Selected { get; set; }

}

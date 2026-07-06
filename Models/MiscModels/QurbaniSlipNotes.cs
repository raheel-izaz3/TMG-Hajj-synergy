using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class QurbaniSlipNotes : BaseEntity
    {
        public int QurbaniSlipNotesId { get; set; }

        [Required, Display(Name = "Note", Prompt = "Enter Note...")]
        public string Note { get; set; }
    }
}

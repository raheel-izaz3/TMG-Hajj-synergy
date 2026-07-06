using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class RecordsPerPage: BaseEntity
    {
        public int RecordsPerPageId { get; set; }

        [Required, Display(Name = "Records Per Page", Prompt = "Enter # of Records...")]
        public int Amount { get; set; }
    }
}

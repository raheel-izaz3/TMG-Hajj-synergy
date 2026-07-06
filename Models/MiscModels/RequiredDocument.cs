using System.ComponentModel.DataAnnotations;

namespace TMG.Models.MiscModels
{
    public class RequiredDocument: BaseEntity
    {
        public int RequiredDocumentId { get; set; }
        
        [Required, Display(Name = "Documents Code", Prompt = "Enter Document Code...")]
        public string DocumentsCode { get; set; }

        [Required, Display(Name = "Documents Required", Prompt = "Enter Required Documents...")]
        public string Documents { get; set; }
    }
}

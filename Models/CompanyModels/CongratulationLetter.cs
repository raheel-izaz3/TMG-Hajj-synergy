using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.CompanyModels
{
    public class CongratulationLetter : BaseEntity
    {
        public int CongratulationLetterId { get; set; }
     
        [Required, Display(Name = "Letter Content")]
        [Column(TypeName = "text")]
        public string LetterContent { get; set; }

        [NotMapped] public List<int> PackageIds { get; set; }
        [NotMapped] public List<bool> PackagesSelected { get; set; }
    }
    public record CongratulationLetterIndexModel
    {
        public int CongratulationLetterId { get; set; }
        public int SNo { get; set; }
        public List<string> Packages { get; set; } = new();
    }

}

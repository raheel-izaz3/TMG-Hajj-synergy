using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.MiscModels
{
    public class Doctor: BaseEntity
    {
        public int DoctorId { get; set; }

        [Required, Display(Prompt = "Enter Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "Hospital Name", Prompt = "Enter Hospital Name...")]
        public string HospitalName { get; set; }

        [Display(Name = "Tehsil")]
        public int? TehsilId { get; set; }
        public Tehsil Tehsil { get; set; }

        [Display(Name = "District", Prompt = "Enter District...")]
        public string District { get; set; }

        [Display(Name = "Landline", Prompt = "Enter Landline...")]
        public string Landline { get; set; }

        [Display(Name = "Cell #", Prompt = "Enter Cell #...")]
        public string CellNumber { get; set; }

        [Required, Display(Name = "Registration #", Prompt = "Enter Registration #...")]
        public string RegistrationNumber { get; set; }
        public string SealSignaturePath { get; set; }

        [NotMapped]
        [Required, Display(Name = "Seal & Signature")]
        public IFormFile SealSignatureFile { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
    }
}

using TMG.Models.MiscModels;

namespace TMG.Models.FamilyHeadReportModels
{
    // Medical Affidavit
    public record MedicalAffidavitModel
    {
        public string PdfPath { get; set; }
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public List<MedicalAffidavitModelEntry> Entries { get; set; } = new List<MedicalAffidavitModelEntry>();
    }
}

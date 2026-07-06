using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels.FamilyModels;

public class FamilyHeadDocumentsModel
{
    public IFormFile MemberImage { get; set; }

    public IFormFile PassportImage { get; set; }

    public IFormFile MemberVaccinationImage { get; set; }
    public IFormFile MemberTrainImage { get; set; }

    public IFormFile VisaImage { get; set; }

    public IFormFile InsuranceImage { get; set; }

    public IFormFile MISPdfPath { get; set; }

    public IFormFile BloodProof { get; set; }
    
    public IFormFile CnicUpload { get; set; }

    public IFormFile PolioUpload { get; set; }

    public IFormFile CovidUpload { get; set; }

    public IFormFile MangatitiseUpload { get; set; }

    public IFormFile InfluenzaUpload { get; set; }

}
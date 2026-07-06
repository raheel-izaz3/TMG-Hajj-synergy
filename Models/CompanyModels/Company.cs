using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;
using TMG.Models.NewChartOfAccounts.Currencies;

namespace TMG.Models.CompanyModels
{
    public class Company : BaseEntity
    {
        public int CompanyId { get; set; }

        [DisplayName("Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [DisplayName("Company Code")]
        public string CompanyCode { get; set; } = string.Empty;
        public string Heading { get; set; } = string.Empty;

        [DisplayName("Sub Heading")]
        public string SubHeading { get; set; } = string.Empty;

        [DisplayName("Established On")]
        public DateTime EstablishmentDate { get; set; }

        [DisplayName("POD")]
        public string POD { get; set; } = string.Empty;

        //[DataType(DataType.Url)]
        public string Website { get; set; }

        [DisplayName("Company Status On Establishment")]
        public string CompanyStatus { get; set; }

        [DisplayName("Current Company Status")]
        public string CurrentStatus { get; set; }

        public string Logo { get; set; }

        public string Stamp { get; set; }
        public string Signature { get; set; }
        public string LetterHead { get; set; }
        public string LetterHeadFooter { get; set; }
        public string LetterHeadCompany { get; set; }
        public string LetterHeadFooterCompany { get; set; }

        public int Quota { get; set; }

        public int? CurrencyTypeId { get; set; }
        public CurrencyType CurrencyType { get; set; }

        // ========================= NEW FIELDS =========================
        [DisplayName("Mohra Enrollment #")]
        public string MohraEnrollmentNo { get; set; }

        [DisplayName("Munazzam #")]
        public string MunazzamNo { get; set; }

        [DisplayName("Cluster Enrollment #")]
        public string ClusterEnrollmentNo { get; set; }

        [DisplayName("DTS #")]
        public string DTSNo { get; set; }

        [DisplayName("DTS Expiry")]
        [DataType(DataType.Date)]
        public DateTime? DTSExpiry { get; set; }

        [DisplayName("IATA #")]
        public string IATANo { get; set; }

        [DisplayName("IATA Expiry")]
        [DataType(DataType.Date)]
        public DateTime? IATAExpiry { get; set; }

        [DisplayName("NTN #")]
        public string NTN { get; set; }

        [DisplayName("Director Name")]
        public string DirectorName { get; set; }

        [DisplayName("Director Photo")]
        public string DirectorPhoto { get; set; }

        [DisplayName("Director CNIC #")]
        public string DirectorCNIC { get; set; }

        [DisplayName("Director CNIC Expiry")]
        [DataType(DataType.Date)]
        public DateTime? DirectorCNICExpiry { get; set; }

        [DisplayName("Director Detail")]
        public string DirectorDetail { get; set; }

        [DisplayName("Bank Name")]
        public string BankName { get; set; }

        [DisplayName("Account Title")]
        public string AccountTitle { get; set; }

        [DisplayName("Branch")]
        public string Branch { get; set; }

        [DisplayName("Account #")]
        public string AccountNumber { get; set; }

        // ========================= FILE UPLOADS =========================


        [NotMapped][DisplayName("New Company Logo")] public IFormFile LogoFile { get; set; }
        [NotMapped][DisplayName("New Company Stamp")] public IFormFile StampFile { get; set; }
        [NotMapped][DisplayName("New Company LetterHead")] public IFormFile LetterHeadFile { get; set; }
        [NotMapped][DisplayName("New Company LetterHead Footer")] public IFormFile LetterHeadFooterFile { get; set; }
        [NotMapped][DisplayName("New Company LetterHead")] public IFormFile LetterHeadFileCompany { get; set; }
        [NotMapped][DisplayName("New Company LetterHead Footer")] public IFormFile LetterHeadFooterFileCompany { get; set; }

        //[NotMapped][DisplayName("Director Photo Upload")]public IFormFile DirectorPhotoFile { get; set; }


    }
}

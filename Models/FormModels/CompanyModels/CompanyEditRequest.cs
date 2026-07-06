using TMG.Models.CompanyModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace TMG.Models.FormModels.CompanyModels
{
    public class CompanyEditRequest
    {
        public int CompanyId { get; set; }

        [Required]
        [DisplayName("Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [DisplayName("Company Code")]
        public string CompanyCode { get; set; } = string.Empty;
        // public string Heading { get; set; } = string.Empty;

        // [DisplayName("Sub Heading")]
        // public string SubHeading { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Established On")]
        public DateTime EstablishmentDate { get; set; }

        // [DisplayName("POD")]
        // public string POD { get; set; } = string.Empty;

        //[DataType(DataType.Url)]
        public string Website { get; set; }

        [DisplayName("Company Status On Establishment")]
        public string CompanyStatus { get; set; }

        [DisplayName("Current Company Status")]
        public string CurrentStatus { get; set; }

        public IFormFile Header { get; set; }
        public IFormFile Footer { get; set; }
        public IFormFile Signatures { get; set; }

        //public IFormFile? DirectorPhoto { get; set; }
        //public string? DirectorPhotoPath { get; set; }
        public string LetterHead { get; set; }
        public string LetterHeadFooter { get; set; }

        public int? Quota { get; set; }
        public int? CurrencyTypeId { get; set; }

        // ===== NEW FIELDS FROM CREATE =====
        [DisplayName("Mohra Enrollment No")]
        public string MohraEnrollmentNo { get; set; }

        [DisplayName("Munazzam No")]
        public string MunazzamNo { get; set; }

        [DisplayName("Cluster Enrollment No")]
        public string ClusterEnrollmentNo { get; set; }

        [DisplayName("DTS No")]
        public string DTSNo { get; set; }

        [DisplayName("DTS Expiry")]
        [DataType(DataType.Date)]
        public DateTime? DTSExpiry { get; set; }

        [DisplayName("IATA No")]
        public string IATANo { get; set; }

        [DisplayName("IATA Expiry")]
        [DataType(DataType.Date)]
        public DateTime? IATAExpiry { get; set; }

        [DisplayName("NTN")]
        public string NTN { get; set; }

        [DisplayName("Director Name")]
        public string DirectorName { get; set; }

        [DisplayName("Director CNIC")]
        public string DirectorCNIC { get; set; }

        [DisplayName("Director CNIC Expiry")]
        [DataType(DataType.Date)]
        public DateTime? DirectorCNICExpiry { get; set; }

        [DisplayName("Director Detail")]
        public string DirectorDetail { get; set; }

        [DisplayName("Director Photo")]
        public IFormFile? DirectorPhoto { get; set; }
        public string? DirectorPhotoPath { get; set; }

        [DisplayName("Bank Name")]
        public string BankName { get; set; }

        [DisplayName("Account Title")]
        public string AccountTitle { get; set; }

        [DisplayName("Branch")]
        public string Branch { get; set; }

        [DisplayName("Account Number")]
        public string AccountNumber { get; set; }

        // ====================== MULTI VALUE FIELDS ======================


        public List<int> AddressTypeId { get; set; }
        public List<string> Address { get; set; }
        public List<string> Country { get; set; }
        public List<string> City { get; set; }
        public List<string> PoBox { get; set; }
        public List<string> Zip { get; set; }
        public List<bool> AddressPreferred { get; set; }


        public List<int> ContactTypeId { get; set; }
        public List<string> Contact { get; set; }
        public List<bool> ContactPreferred { get; set; }


        public List<int> EmailTypeId { get; set; }
        public List<string> Email { get; set; }
        public List<bool> EmailPreferred { get; set; }


        public List<int> LicenseTypeId { get; set; }
        public List<string> LicenseNo { get; set; }
        public List<string> IssuePlace { get; set; }
        public List<string> IssueDate { get; set; }
        public List<string> ValidDate { get; set; }
        public List<bool> LicensePreferred { get; set; }



    }
}

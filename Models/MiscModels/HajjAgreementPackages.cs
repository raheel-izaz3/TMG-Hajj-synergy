using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.MiscModels
{
    public class HajjAgreementPackages
    {
        public int HajjAgreementPackagesId { get; set; }
        public int HajjAgreementId { get; set; }
        public HajjAgreement HajjAgreement { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }

    }
}

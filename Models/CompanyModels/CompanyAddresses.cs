using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.Identity.Client;

namespace TMG.Models.CompanyModels
{
    public class CompanyAddresses: BaseEntity
    {
        public int CompanyAddressesId { get; set; }
        public int CompanyId { get; set; }
        public int CompanyAddressId { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string PoBox { get; set; }
        public bool Preferred { get; set; }


        public virtual Company Company { get; set; }
        public virtual CompanyAddressType CompanyAddress { get; set; }
    }
}

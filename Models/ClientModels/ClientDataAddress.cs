using TMG.Models.MemberModels;
using TMG.Models.MiscModels;

namespace TMG.Models.ClientModels
{
    public class ClientDataAddress
    {
        public int ClientDataAddressId { get; set; }
        public int ClientDataId { get; set; }
        public ClientData ClientData { get; set; }
        public int? MemberAddressTypeId { get; set; }
        public MemberAddressType MemberAddressType { get; set; }

        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int? CityId { get; set; }
        public City Cities { get; set; }
        public int? CountryId { get; set; }
        public Country Countries { get; set; }
        public string ZipCode { get; set; }
        public string PoBox { get; set; }
        public int? TehsilId { get; set; }
        public Tehsil Tehsil { get; set; }
        public bool Preferred { get; set; }
    }
} 
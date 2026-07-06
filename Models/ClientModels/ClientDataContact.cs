using TMG.Models.MemberModels;

namespace TMG.Models.ClientModels
{
    public class ClientDataContact
    {
        public int ClientDataContactId { get; set; }
        public int ClientDataId { get; set; }
        public ClientData ClientData { get; set; }
        public int? MemberContactTypeId { get; set; }
        public MemberContactType MemberContactType { get; set; }

        public string Contact { get; set; }
        public bool Preferred { get; set; }
    }
} 
using TMG.Models.MemberModels;

namespace TMG.Models.ClientModels
{
    public class ClientDataEmail
    {
        public int ClientDataEmailId { get; set; }
        public int ClientDataId { get; set; }
        public ClientData ClientData { get; set; }
        public int? MemberEmailTypeId { get; set; }
        public MemberEmailType MemberEmailType { get; set; }

        public string Email { get; set; }
        public bool Preferred { get; set; }
    }
} 
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models.ClientModels
{
    public class ClientDataMembers
    {
        public int ClientDataMembersId { get; set; }

        public int ClientDataId { get; set; }
        public ClientData ClientData { get; set; }

        [ForeignKey("ClientDataId")]
        public int HeadId { get; set; }
        public ClientData Head { get; set; }

        public bool IsHead { get; set; }
    }
} 
using TMG.Models.MiscModels;

namespace TMG.Models.ClientModels
{
    public class ClientDataDocuments
    {
        public int ClientDataDocumentsId { get; set; }

        public int ClientDataId { get; set; }
        public ClientData ClientData { get; set; }

        public int RequiredDocumentId { get; set; }
        public RequiredDocument RequiredDocument { get; set; }

        public bool Recieved { get; set; }

        public string Remarks { get; set; }
    }
} 
using TMG.Models.LogisticModels;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadTransport
    {
        public int FamilyHeadTransportId { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead{ get; set; }

        public int RouteId { get; set; } 
        public LogisticModels.Route Route { get; set; }
        public DateTime? CheckIn { get; set; }
        //public DateTime? CheckOut { get; set; }
        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public FamilyTranasport TransportType { get; set; }
        public float? Price { get; set; }
    }
}

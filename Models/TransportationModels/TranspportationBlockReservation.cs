using TMG.Models.FamilyModels;
using TMG.Models.LogisticModels;
using TMG.Models.TransportationBlock;

namespace TMG.Models.TransportationModels
{
    public class TranspportationBlockReservation
    {
        public int TranspportationBlockReservationId { get; set; }
        public DateTime? FlightArrival { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? FlightDeparture { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? TrainArrival { get; set; }
        public DateTime? TrainDeparture { get; set; }
        public DateTime? ActualCheckIn { get; set; }
        public DateTime? ActualCheckOut { get; set; }
        public int? StayWith { get; set; }
        public string ConnectedNumber { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public string ConnectedVehicleNumber { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }
        public int TransportationBlocksId { get; set; }
        public TransportationBlocks TransportationBlocks { get; set; }

    }
}

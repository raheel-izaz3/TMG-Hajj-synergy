namespace TMG.Models.FamilyHeadReportModels
{
    public sealed record NewItineraryReportModel 
    {
        public List<NewItineraryReportMemberModel> Members { get; set; } = new();
        public List<NewItineraryReportFlightModel> Flights { get; set; } = new();
        public List<NewItineraryReportFlightWOPCKGModel> WOPKGFlights { get; set; } = new();
        public List<NewItineraryReportTrainModel> Trains { get; set; } = new();
        public List<NewItineraryReportTrainWOPKGModel> WOPKGTrains { get; set; } = new();
        public List<NewItineraryReportRouteModel> Routes { get; set; } = new();
        public List<NewItineraryReportAccommodationsModel> Accommodations { get; set; } = new();
        public NewItineraryReportAccommodationsModel MakkahAccommodation { get; set; } = new();
        public NewItineraryReportAccommodationsModel MedinaAccommodation { get; set; } = new();
        public NewItineraryReportAccommodationsModel AziziaAccommodation { get; set; } = new();
        public NewItineraryReportAccommodationsModel MinaAccommodation { get; set; } = new();
    }

    public sealed record NewItineraryReportMemberModel 
    {
        public string Name { get; set; }
        public string HbNo { get; set; }
        public string Gender { get; set; }
        public string Passport { get; set; }
        public string Visa { get; set; }
        public string TicketNumber { get; set; }
    }

    public sealed record NewItineraryReportFlightModel 
    {
        public string Package { get; set; }
        public string Type { get; set; }    
        public string Flight { get; set; }
        public string Sector { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Luggage { get; set; }
        public string Class { get; set; }

        public List<FlightDetailModel> FlightDetails = new List<FlightDetailModel>();
    }

    public sealed record FlightDetailModel
    {
        public string TicketNumber { get; set; }
        public string Type { get; set; }
        public string Flight { get; set; }
        public string Sector { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Luggage { get; set; }
        public string Class { get; set; }
    }

    public sealed record NewItineraryReportFlightWOPCKGModel
    {
        public string HajjiName { get; set; }
        public string HBNo { get; set; }
        public string Type { get; set; }
        public string Flight { get; set; }
        public string Sector { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Luggage { get; set; }
        public string Class { get; set; }
    }

    public sealed record NewItineraryReportTrainModel 
    {
        public string Package { get; set; }
        public string Train { get; set; }
        public string Sector { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Luggage { get; set; }
        public string Class { get; set; }
    }

    public sealed record NewItineraryReportTrainWOPKGModel
    {
        public string HajjiName { get; set; }
        public string HBNo { get; set; }
        public string Train { get; set; }
        public string Sector { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Luggage { get; set; }
        public string Class { get; set; }
    }

    public sealed record NewItineraryReportRouteModel 
    {
        public string Route { get; set; }
        public string Vehicle { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }

    }

    public sealed record NewItineraryReportAccommodationsModel 
    {
        public string Hotel { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string Nights { get; set; }
        public int PAX { get; set; }
        public string RoomType { get; set; }
        public string Qty { get; set; }
        public string MealPlan { get; set; }
        public int Count { get; set; } // <-- New property
        public DateTime CheckInDateTime { get; set; }   
        public int? PlaceId { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TMG.Models.OperationalGroupModels
{
    public class OperationalPassengerSelectionItemViewModel
    {
        public int IndividualId { get; set; }
        public int? FamilyHeadId { get; set; }
        public int? FamilyMemberId { get; set; }
        public bool IsHead { get; set; }
        public string FullName { get; set; }
        public string CNIC { get; set; }
        public string PassportNumber { get; set; }
        public string Gender { get; set; }
        public string BookingCode { get; set; }
        public string ClientCompanyName { get; set; }
        public string HeadName { get; set; }
        public int HeadPaxCount { get; set; }
        public bool IsSelected { get; set; }
    }

    public class OperationalBookingGroupViewModel
    {
        public int HeadId { get; set; }
        public string BookingCode { get; set; }
        public string HeadName { get; set; }
        public int PaxCount { get; set; }
        public string ClientCompanyName { get; set; }
        public List<OperationalPassengerSelectionItemViewModel> Passengers { get; set; } = new List<OperationalPassengerSelectionItemViewModel>();
    }

    public class OperationalGroupCreateViewModel
    {
        public int Id { get; set; }

        public int HajjSeasonsId { get; set; }

        public GroupCategory Category { get; set; } = GroupCategory.Flight;

        // --- Section 1: Basic Group Details ---
        [Required(ErrorMessage = "Group Name is required.")]
        [StringLength(150, ErrorMessage = "Group Name cannot exceed 150 characters.")]
        public string GroupName { get; set; }

        [StringLength(150)]
        public string GroupLeaderName { get; set; }

        [StringLength(50)]
        public string LeaderMobileNumber { get; set; }

        // --- Section 2: Resource Allocation ---
        // Flight
        public int? AirlineId { get; set; }
        public int? FlightId { get; set; }

        [StringLength(50)]
        public string FlightNumber { get; set; }

        [StringLength(50)]
        public string PNRReference { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FlightDate { get; set; }

        // Bus
        public int? VehicleId { get; set; }

        [StringLength(100)]
        public string BusIdentifierPlateNo { get; set; }

        public int BusCapacity { get; set; } = 45;

        // Hotel
        public int? HotelId { get; set; }
        public int? RoomId { get; set; }

        // Route
        public int? RouteId { get; set; }
        public int? ItineraryScheduleId { get; set; }

        public List<int> SelectedPassengerIds { get; set; } = new List<int>();

        // Master Dropdowns
        public List<SelectListItem> Airlines { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Flights { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Vehicles { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Hotels { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Rooms { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Routes { get; set; } = new List<SelectListItem>();

        public List<OperationalBookingGroupViewModel> Bookings { get; set; } = new List<OperationalBookingGroupViewModel>();
    }

    public class OperationalGroupIndexItemViewModel
    {
        public int Id { get; set; }
        public GroupCategory Category { get; set; }
        public string GroupName { get; set; }
        public string GroupLeaderName { get; set; }
        public string LeaderMobileNumber { get; set; }
        public string ResourceSummary { get; set; }
        public int PassengerCount { get; set; }
        public int BusCapacity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

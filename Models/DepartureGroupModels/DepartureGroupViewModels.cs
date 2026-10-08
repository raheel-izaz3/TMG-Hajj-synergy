using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TMG.Models.DepartureGroupModels
{
    public class DeparturePassengerSelectionItemViewModel
    {
        public int IndividualId { get; set; }
        public int? FamilyHeadId { get; set; }
        public int? FamilyMemberId { get; set; }
        public bool IsHead { get; set; }
        public string FullName { get; set; }
        public string PassportNumber { get; set; }
        public string CNIC { get; set; }
        public string BookingCode { get; set; }
        public string HeadName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DepartureBookingCardViewModel
    {
        public int HeadId { get; set; }
        public string BookingCode { get; set; }
        public string HeadName { get; set; }
        public List<DeparturePassengerSelectionItemViewModel> Passengers { get; set; } = new List<DeparturePassengerSelectionItemViewModel>();
    }

    public class DepartureGroupCreateViewModel
    {
        public int Id { get; set; }

        public int HajjSeasonsId { get; set; }

        [Required(ErrorMessage = "Group Name is required.")]
        [StringLength(150, ErrorMessage = "Group Name cannot exceed 150 characters.")]
        public string GroupName { get; set; }

        public int? AirlineId { get; set; }
        public int? FlightId { get; set; }

        [StringLength(50)]
        public string FlightNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FlightDepartureDate { get; set; }

        [StringLength(20)]
        public string FlightDepartureTime { get; set; }

        [StringLength(50)]
        public string PNRNumber { get; set; }

        [StringLength(100)]
        public string DepartureCity { get; set; }

        [StringLength(100)]
        public string ArrivalCity { get; set; }

        [StringLength(1000)]
        public string Remarks { get; set; }

        public List<int> SelectedPassengerIds { get; set; } = new List<int>();

        public List<SelectListItem> Airlines { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Flights { get; set; } = new List<SelectListItem>();
        public List<DepartureBookingCardViewModel> Bookings { get; set; } = new List<DepartureBookingCardViewModel>();
    }

    public class DepartureGroupIndexItemViewModel
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string AirlineName { get; set; }
        public string FlightNumber { get; set; }
        public DateTime? FlightDepartureDate { get; set; }
        public string FlightDepartureTime { get; set; }
        public string PNRNumber { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public int PassengerCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

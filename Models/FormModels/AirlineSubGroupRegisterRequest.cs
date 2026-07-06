using TMG.Models.AirlineModels;
using TMG.Models.GroupModels;
using System.ComponentModel.DataAnnotations;

namespace TMG.Models.FormModels
{
    public class AirlineSubGroupRegisterRequest
    {
        [Required, Display(Name = "Group")]
        public int GroupId { get; set; }

        [Required, Display(Name = "Sub Group Code", Prompt = "Enter Sub Group Code...")]
        public string Code { get; set; }

        [Required, Display(Name = "Sub Group Name", Prompt = "Enter Sub Group Name...")]
        public string Name { get; set; }

        [Required, Display(Name = "PNR Code", Prompt = "Enter PNR Code...")]
        public string PnrCode { get; set; }

        [Required, Display(Name = "PNR PAX", Prompt = "Enter PNR PAX...")]
        public int PnrPAX { get; set; }

        [Required, Display(Name = "PNR Create Date", Prompt = "Enter PNR Create Date...")]
        public DateTime PnrCreateDate { get; set; }

        [Required, Display(Name = "PNR Deadline Date", Prompt = "Enter PNR Deadline Date...")]
        public DateTime PnrDeadlineDate { get; set; }

        [Required, Display(Name = "PNR Deadline Time", Prompt = "Enter PNR Deadline Time...")]
        public string PnrDeadlineTime { get; set; }

        [Required, Display(Name = "Booking Reference", Prompt = "Enter Booking Reference...")]
        public string BookingReference { get; set; }

        [Required, Display(Name = "Issuing Agent", Prompt = "Enter Issuing Agent...")]
        public string IssuingAgent { get; set; }

        [Required, Display(Name = "Baggage Allowance", Prompt = "Enter Baggage Allowance...")]
        public float BaggageAllowance { get; set; }

        [Required, Display(Name = "Weight Allowance", Prompt = "Enter Weight Allowance...")]
        public float WeightAllowance { get; set; }

        [Required, Display(Name = "Note", Prompt = "Enter Note...")]
        public string Note { get; set; }

        [Display(Name = "Airline Logo Image")]
        public IFormFile AirlineLogo { get; set; }

        public List<int> AirlineIds { get; set; }

        public List<int> AirlineClassIds { get; set; }

        public List<string> SectorNumbers { get; set; }
        public List<string> SectorCodes { get; set; }
        public List<string> SectorNames { get; set; }
        public List<DateTime> FlightDates { get; set; }
        public List<string> FlightNumbers { get; set; }
        public List<string> StatusCodes { get; set; }
        public List<DateTime> DepartureDates { get; set; }
        public List<string> DepartureTimes { get; set; }
        public List<DateTime> ArrivalDates { get; set; }
        public List<string> ArrivalTimes { get; set; }
        public List<string> SectorNotes { get; set; }
    }
}

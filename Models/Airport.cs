// Models/POTicketFlight.cs

using System.ComponentModel.DataAnnotations;
using TMG.Models.MiscModels;

namespace TMG.Models
{
    public class Airport : BaseEntity
    {
        public int AirportId { get; set; }

        [Required]
        [StringLength(100)]
        public string AirportName { get; set; }

        public string Code { get; set; }

        public int? HajjSeasonsId { get; set; }

        public HajjSeasons HajjSeasons { get; set; }

    }
}
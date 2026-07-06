using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models
{
    public class BaseEntity
    {
        [Comment("Entity's created date time")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Comment("Entity's last update date time")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool Trashed { get; set; }

    }
}

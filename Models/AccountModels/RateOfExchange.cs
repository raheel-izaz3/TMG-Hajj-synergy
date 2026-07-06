using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMG.Models
{

    public enum Currencies
    {
        PKR, SAR, USD,
        AED, GBP, EURO
    }
    public class RateOfExchange : BaseEntity
    {
        public int RateOfExchangeID { get; set; }
        public Currencies Currency { get; set; }
        public float PKRROE { get; set; }
        public float SARROE { get; set; }
        public float USDROE { get; set; }
        public float AEDROE { get; set; }
        public float GBPROE { get; set; }
        public float EUROROE { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime Date { get; set; }

    }
}

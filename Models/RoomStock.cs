using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.AccomodationModels;

namespace TMG.Models;

public class RoomStock : BaseEntity
{
    public int RoomStockId { get; set; }

    public int PlaceId { get; set; }
    public Place Place { get; set; }

    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }

    [NotMapped] public List<int> PackageIds { get; set; }
    [NotMapped] public List<bool> PackagesSelected { get; set; }

    [NotMapped] public List<int> RoomIds { get; set; }
    [NotMapped] public List<int> Quantities { get; set; }
    [NotMapped] public List<string> RoomNames { get; set; } = new();
    [NotMapped] public List<string> PackageNames { get; set; } = new();


}

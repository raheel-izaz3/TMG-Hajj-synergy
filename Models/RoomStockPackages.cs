using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.AccomodationModels;
using TMG.Models.PackageModels;

namespace TMG.Models;

public class RoomStockPackages
{
    public int RoomStockPackagesId { get; set; }

    public int RoomStockId { get; set; }
    public RoomStock RoomStock { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; }
}

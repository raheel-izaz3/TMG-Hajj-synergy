using TMG.Data;
using TMG.Models;
using TMG.Models.DemandModels;
using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.DemandControllers
{
    public class RoomStockController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public RoomStockController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RoomStocks.Where(ca => ca.Trashed).Count();

            // ViewBag.TrashAllowed = PrivilegeManager.CanTrashAirlines(_dbContext, _http);
            // ViewBag.CreateAllowed = PrivilegeManager.CanCreateAirlines(_dbContext, _http);
            // ViewBag.EditAllowed = PrivilegeManager.CanEditAirlines(_dbContext, _http);
            // ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreAirlines(_dbContext, _http);

            var stocks = _dbContext.RoomStocksWithoutTrash
            .Include(e => e.Place).ToList();

            var packages = _dbContext.RoomStockPackages
            .Include(e => e.Package)
            .Select(e => new
            {
                e.Package.Name,
                e.RoomStockId
            });

            var types = _dbContext.RoomStockRoomTypes.Include(e => e.Room);

            for (int i = 0; i < stocks.Count; i++)
            {
                var stock = stocks.ElementAt(i);
                var currTypes = types.Where(e => e.RoomStockId == stock.RoomStockId).ToList();
                var currPackages = packages.Where(e => e.RoomStockId == stock.RoomStockId).ToList();


                for (int j = 0; j < currTypes.Count; j++)
                {
                    stock.RoomNames.Add($"{currTypes.ElementAt(j).Room.RoomName} ({currTypes.ElementAt(j).Quantity})");
                }

                for (int j = 0; j < currPackages.Count; j++)
                    stock.PackageNames.Add($"{currPackages.ElementAt(j).Name}");


            }

            // return Ok(stocks);
            return View(stocks);
        }

        public IActionResult Create()
        {
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.Packages = _dbContext.PackagesWithoutTrash.Select(e => new
            {
                e.PackageId,
                e.Name
            });


            return View();
        }
        [HttpPost]
        public IActionResult Create(RoomStock request)
        {
            if (!ModelState.IsValid) return View();

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.RoomStocks.Add(request);
                _dbContext.SaveChanges();

                for (int i = 0; i < (request.PackageIds?.Count ?? 0); i++)
                {
                    if (request.PackagesSelected[i] == false) continue;

                    _dbContext.RoomStockPackages.Add(new()
                    {
                        PackageId = request.PackageIds[i],
                        RoomStockId = request.RoomStockId
                    });
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < request.RoomIds.Count; i++)
                {
                    _dbContext.RoomStockRoomTypes.Add(new()
                    {
                        RoomId = request.RoomIds[i],
                        Quantity = request.Quantities[i],
                        RoomStockId = request.RoomStockId
                    });
                }

                _dbContext.SaveChanges();

                transaction.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return View(request);
            }

        }
        public IActionResult Edit(int id)
        {
            RoomStock _roomStocks = _dbContext.RoomStocks.Find(id);
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Packages = _dbContext.RoomStockPackages
                .Where(a => !a.Package.Trashed)
                .Select(a => a.Package.Name);
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            return View(_roomStocks);
        }

        public IActionResult Trash(int id)
        {
            RoomStock room = _dbContext.RoomStocks.Find(id);
            if (room == null) return RedirectToAction(nameof(Index));

            room.Trashed = true;
            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            RoomStock room = _dbContext.RoomStocks.Find(id);
            room.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            var stocks = _dbContext.RoomStocks.Where(s => s.Trashed)
            .Include(e => e.Place).ToList();


            var types = _dbContext.RoomStockRoomTypes.Include(e => e.Room);

            for (int i = 0; i < stocks.Count; i++)
            {
                var stock = stocks.ElementAt(i);
                var currTypes = types.Where(e => e.RoomStockId == stock.RoomStockId).ToList();

                for (int j = 0; j < currTypes.Count; j++)
                {
                    stock.RoomNames.Add($"{currTypes.ElementAt(j).Room.RoomName} ({currTypes.ElementAt(j).Quantity})");
                }
            }
            return View(stocks);
        }
    }
}
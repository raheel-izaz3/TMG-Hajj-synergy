using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using TMG.Models.LogisticModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.LogisticControllers
{
    public class VehicleController: Controller
    {
        private readonly TMGDbContext _dbContext;

        private readonly IHttpContextAccessor _http;

        public VehicleController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Vehicles.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashVehicle(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateVehicle(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditVehicle(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreVehicle(_dbContext, _http);

            return View(_dbContext.VehiclesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Vehicle vehicle)
        {
            if (!ModelState.IsValid) return View();

            //vehicle.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Vehicles.Add(vehicle);
            _dbContext.SaveChanges();

            IEnumerable<Models.LogisticModels.Route> routes = _dbContext.Routes;

            /*for(int i = 0; i < routes.Count(); i++)
            {
                _dbContext.RouteVehiclePrices.Add(
                    new RouteVehiclePrice { RouteId = routes.ElementAt(i).RouteId, VehicleId = vehicle.VehicleId, Price = 0 }
                    );
            }
*/
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Vehicle vehicle = _dbContext.Vehicles.Find(id);

            return View(vehicle);
        }


        [HttpPost]
        public IActionResult Edit(Vehicle vehicle)
        {
            if (!ModelState.IsValid) return View();

            //vehicle.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(vehicle).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Vehicles.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Vehicle vehicle = _dbContext.Vehicles.Find(id);
            if (vehicle == null) return RedirectToAction(nameof(Index));

            vehicle.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Vehicle vehicle = _dbContext.Vehicles.Find(id);
            vehicle.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

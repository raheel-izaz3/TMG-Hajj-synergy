using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using TMG.Models.LogisticModels;
using TMG.Models.FormModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.LogisticControllers;
public class RouteController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;

    public RouteController(TMGDbContext dbContext, IHttpContextAccessor http)
    {
        _dbContext = dbContext;
        _http = http;
    }

    public IActionResult Index()
    {
        ViewBag.TrashCount = _dbContext.Routes.Where(e => e.Trashed).Count();

        ViewBag.TrashAllowed = PrivilegeManager.CanTrashRoute(_dbContext, _http);
        ViewBag.CreateAllowed = PrivilegeManager.CanCreateRoute(_dbContext, _http);
        ViewBag.EditAllowed = PrivilegeManager.CanEditRoute(_dbContext, _http);
        ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreRoute(_dbContext, _http);

        return View(_dbContext.RoutesWithoutTrash);
    }

    public IActionResult Create()
    {
        ViewBag.VehiclesCount = _dbContext.VehiclesWithoutTrash.Count();
        ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;

        return View();
    }


    [HttpPost]
    public IActionResult Create(RouteRegisterRequest request)
    {
        ViewBag.VehiclesCount = _dbContext.VehiclesWithoutTrash.Count();
        ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;

        /*if (request.VehicleIds.Count != request.Prices.Count)
        {
            ModelState.AddModelError("Prices", "Kindly provide prices for all vehicles");
        }*/

        if (!ModelState.IsValid) return View();

        Models.LogisticModels.Route route = new();
        route.StartPlace = request.StartPlace;
        route.Destination = request.Destination;
        //route.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

        _dbContext.Routes.Add(route);
        _dbContext.SaveChanges();

        /*for (int i = 0; i < request.VehicleIds.Count; i++)
        {
            RouteVehiclePrice routeVehiclePrice = new();

            routeVehiclePrice.RouteId = route.RouteId;
            routeVehiclePrice.VehicleId = request.VehicleIds[i];
            routeVehiclePrice.Price = request.Prices[i];

            _dbContext.RouteVehiclePrices.Add(routeVehiclePrice);
        }*/

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        ViewBag.VehiclesCount = _dbContext.VehiclesWithoutTrash.Count();
        ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;

        Models.LogisticModels.Route route = _dbContext.Routes.Find(id);

        //IEnumerable<RouteVehiclePrice> Prices = _dbContext.RouteVehiclePrices.Where(e => e.RouteId == id);

        //return Ok(Prices);

        List<int> VehicleIds = new List<int>();
        List<float> VehiclePrices = new List<float>();

        RouteEditRequest editRequest = new RouteEditRequest
        {
            RouteId = id,
            StartPlace = route.StartPlace,
            Destination = route.Destination,
        };

        /*for (int i = 0; i < Prices.Count(); i++)
        {
            VehicleIds.Add(Prices.ElementAt(i).VehicleId);
            VehiclePrices.Add(Prices.ElementAt(i).Price);
        }*/

        //editRequest.VehicleIds = VehicleIds;
        editRequest.Prices = VehiclePrices;

        return View(editRequest);
    }


    [HttpPost]
    public IActionResult Edit(RouteEditRequest request)
    {
        ViewBag.VehiclesCount = _dbContext.VehiclesWithoutTrash.Count();
        ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;

        //if (request.VehicleIds.Count != request.Prices.Count)
        //{
        //    ModelState.AddModelError("Prices", "Kindly provide prices for all vehicles");
        //}


        if (!ModelState.IsValid) return View();

        //return Ok(request);

        Models.LogisticModels.Route route = _dbContext.Routes.Find(request.RouteId);

        route.StartPlace = request.StartPlace;
        route.Destination = request.Destination;
        route.UpdatedAt = DateTime.Now;
        //route.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

        //_dbContext.RouteVehiclePrices.Where(e => e.RouteId == request.RouteId).ExecuteDelete();

        /*for (int i = 0; i < request.VehicleIds.Count; i++)
        {
            _dbContext.RouteVehiclePrices.Add(
                new RouteVehiclePrice { RouteId = request.RouteId, VehicleId = request.VehicleIds[i], Price = request.Prices[i] }
                );
        }*/

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult ViewTrash()
    {
        return View(_dbContext.Routes);
    }

    public IActionResult Trash(int id)
    {
        Models.LogisticModels.Route Route = _dbContext.Routes.Find(id);
        if (Route == null) return RedirectToAction(nameof(Index));

        Route.Trashed = true;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Restore(int id)
    {
        Models.LogisticModels.Route Route = _dbContext.Routes.Find(id);
        Route.Trashed = false;
        _dbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

}

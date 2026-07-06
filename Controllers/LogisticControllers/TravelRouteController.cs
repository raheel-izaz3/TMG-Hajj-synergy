using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using TMG.Models.LogisticModels;
using TMG.Models.FormModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.FamilyModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace TMG.Controllers.LogisticControllers
{
    public class TravelRouteController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public TravelRouteController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.TravelRoutes.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashTravelRoute(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateTravelRoute(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditTravelRoute(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreTravelRoute(_dbContext, _http);

            return View(_dbContext.TravelRoutesWithoutTrash);
        }

        public IActionResult Create()
        {

            ViewBag.Routes = _dbContext.RoutesWithoutTrash;
            ViewBag.RoutesCount = _dbContext.RoutesWithoutTrash.Count();
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash; ;
            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(TravelRegisterRequest request)
        {

            //return Ok(request);
            ViewBag.Routes = _dbContext.RoutesWithoutTrash;
            ViewBag.RoutesCount = _dbContext.RoutesWithoutTrash.Count();
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash; ;
            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            if (_dbContext.RoutesWithoutTrash.Count() == 0)
            {
                ModelState.AddModelError("RoutesSelected", "Please Create Routes First.");
            }
            else
            {
                bool success = false;
                for (int i = 0; i < request.RouteIds.Count; i++)
                {
                    if (request.RoutesSelected[i] == true)
                    {
                        success = true;
                        break;
                    }
                }

                if (!success)
                {
                    ModelState.AddModelError("RoutesSelected", "Kindly Select Atleast 1 Route");
                }
            }

            

            if (!ModelState.IsValid) return View();

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                TravelRoute travelRoute = new TravelRoute
                {
                    //RouteNumber = request.RouteNumber,
                    RouteName = request.RouteName,
                    // StandardPrice = request.StandardPrice,
                    // ROE = request.ROE,
                    //HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                };

                _dbContext.TravelRoutes.Add(travelRoute);
                _dbContext.SaveChanges();

                for (int i = 0; i < request.RouteIds.Count; i++)
                {
                    if (request.RoutesSelected[i] == false) continue;


                    _dbContext.TravelRouteStops.Add(
                        new TravelRouteStops
                        {
                            TravelRouteId = travelRoute.TravelRouteId,
                            RouteId = request.RouteIds[i],
                            CheckIn = (DateTime)request.CheckIns[i],
                            CheckOut = (DateTime)request.CheckIns[i],
                            CheckInTime = request.CheckInsTime[i],
                            VehicleId = request.VehicleIds[i],
                            TransportType = request.TransportTypes[i],
                            //Price = request.Prices[i],
                            // SARPrice = request.SARPrices[i] != null ? (float)request.SARPrices[i] : 0
                            HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        }
                        );
                }

                _dbContext.SaveChanges();

                transaction.Commit();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(request);  
            }

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.TravelRoutes.Where(s => s.Trashed));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Routes = _dbContext.RoutesWithoutTrash;

            ViewBag.RoutesCount = _dbContext.RoutesWithoutTrash.Count();
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;
            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            TravelRoute travelRoute = _dbContext.TravelRoutes.Find(id);

            TravelEditRequest request = new TravelEditRequest
            {
                TravelRouteId = id,
                //RouteNumber = travelRoute.RouteNumber,
                RouteName = travelRoute.RouteName,
                // StandardPrice = travelRoute.StandardPrice,
                // ROE = travelRoute.ROE
            };

            ViewBag.AssignedTravelRouteStops = _dbContext.TravelRouteStops.Where(e => e.TravelRouteId == id);

            IEnumerable<Models.LogisticModels.Route> routes = _dbContext.RoutesWithoutTrash;

            return View(request);

        }

        [HttpPost]
        public IActionResult Edit(TravelEditRequest request)
        {
            ViewBag.Routes = _dbContext.RoutesWithoutTrash;
            ViewBag.RoutesCount = _dbContext.RoutesWithoutTrash.Count();
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;
            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
            .ToList();

            ViewBag.AssignedTravelRouteStops = _dbContext.TravelRouteStops.Where(e => e.TravelRouteId == request.TravelRouteId);

            //return Ok(request);

            TravelRoute travelRoute = _dbContext.TravelRoutes.Find(request.TravelRouteId);

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                travelRoute.RouteName = request.RouteName;
                //travelRoute.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                _dbContext.TravelRouteStops.Where(e => e.TravelRouteId == request.TravelRouteId).ExecuteDelete();

                for (int i = 0; i < request.RouteIds.Count; i++)
                {
                    if (request.RoutesSelected[i] == false) continue;

                    _dbContext.TravelRouteStops.Add(
                       new TravelRouteStops
                       {
                           TravelRouteId = travelRoute.TravelRouteId,
                           RouteId = request.RouteIds[i],
                           CheckIn = (DateTime)request.CheckIns[i],
                           CheckOut = (DateTime)request.CheckIns[i],
                           CheckInTime = request.CheckInsTime[i],
                           VehicleId = request.VehicleIds[i],
                           TransportType = request.TransportTypes[i],
                           //Price = request.Prices[i],
                           HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                       }
                       );
                }

                _dbContext.SaveChanges();

                transaction.Commit();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
                return View(request);
            }

        }

        public IActionResult Copy(int id)
        {
            ViewBag.Routes = _dbContext.RoutesWithoutTrash;
            ViewBag.RoutesCount = _dbContext.RoutesWithoutTrash.Count();
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;
            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            TravelRoute travelRoute = _dbContext.TravelRoutes.Find(id);

            TravelRegisterRequest request = new TravelRegisterRequest
            {
                TravelRouteId = id,
                //RouteNumber = travelRoute.RouteNumber,
                RouteName = travelRoute.RouteName,
                // StandardPrice = travelRoute.StandardPrice,
                // ROE = travelRoute.ROE
            };

            ViewBag.AssignedTravelRouteStops = _dbContext.TravelRouteStops.Where(e => e.TravelRouteId == id);

            IEnumerable<Models.LogisticModels.Route> routes = _dbContext.RoutesWithoutTrash;

            return View(request);
        }

        [HttpPost]
        public IActionResult Copy(TravelRegisterRequest request)
        {
            //return Ok(request);
            ViewBag.Routes = _dbContext.RoutesWithoutTrash;
            ViewBag.RoutesCount = _dbContext.RoutesWithoutTrash.Count();
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash; 
            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            if (_dbContext.RoutesWithoutTrash.Count() == 0)
            {
                ModelState.AddModelError("RoutesSelected", "Please Create Routes First.");
            }
            else
            {
                bool success = false;
                for (int i = 0; i < request.RouteIds.Count; i++)
                {
                    if (request.RoutesSelected[i] == true)
                    {
                        success = true;
                        break;
                    }
                }

                if (!success)
                {
                    ModelState.AddModelError("RoutesSelected", "Kindly Select Atleast 1 Route");
                }
            }

            if (!ModelState.IsValid) return View();

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                TravelRoute travelRoute = new TravelRoute
                {
                    //RouteNumber = request.RouteNumber,
                    RouteName = request.RouteName,
                    // StandardPrice = request.StandardPrice,
                    // ROE = request.ROE,
                    //HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                };

                _dbContext.TravelRoutes.Add(travelRoute);
                _dbContext.SaveChanges();

                for (int i = 0; i < request.RouteIds.Count; i++)
                {
                    if (request.RoutesSelected[i] == false) continue;


                    _dbContext.TravelRouteStops.Add(
                        new TravelRouteStops
                        {
                            TravelRouteId = travelRoute.TravelRouteId,
                            RouteId = request.RouteIds[i],
                            CheckIn = (DateTime)request.CheckIns[i],
                            CheckOut = (DateTime)request.CheckIns[i],
                            CheckInTime = request.CheckInsTime[i],
                            VehicleId = request.VehicleIds[i],
                            TransportType = request.TransportTypes[i],
                            //Price = request.Prices[i],
                            // SARPrice = request.SARPrices[i] != null ? (float)request.SARPrices[i] : 0
                            HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        }
                        );
                }

                _dbContext.SaveChanges();

                transaction.Commit();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(request);
            }

        }

        public IActionResult Trash(int id)
        {
            TravelRoute travelRoute = _dbContext.TravelRoutes.Find(id);
            if (travelRoute == null) return RedirectToAction(nameof(Index));

            travelRoute.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            TravelRoute travelRoute = _dbContext.TravelRoutes.Find(id);
            travelRoute.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

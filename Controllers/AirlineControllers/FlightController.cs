using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace TMG.Controllers.AirlineControllers
{
    [Authorize]
    public class FlightController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public FlightController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _http = http;
            _host = host;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Flights.Where(ca => ca.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashAirlines(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateAirlines(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditAirlines(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreAirlines(_dbContext);

            var flights = _dbContext.Flights
                .Include(e => e.Airline)
                .Where(ca => !ca.Trashed)
                .ToList();

            var flightPackages = _dbContext.FlightPackages
                .Include(e => e.Package)
                .Select(e => new
                {
                    e.PackageId,
                    e.Package.Name,
                    e.Package.Trashed,
                    e.FlightId
                })
                .Where(ca => !ca.Trashed)
                .ToList();

            var pnrMembers = _dbContext.PNRMembers
                .Include(e => e.PNR)
                .Select(e => new
                {
                    e.PNRId,
                    e.FamilyHeadId,
                    e.PNR.FlightId,
                    e.PNR.Type
                })
                .ToList();

            FlightIndexModel model = new FlightIndexModel();

            for (int i = 0; i < flights.Count; i++)
            {
                Flight currFlight = flights.ElementAt(i);

                FlightIndexEntryModel entry = new FlightIndexEntryModel
                {
                    FlightId = currFlight.FlightId,
                    AirlineName = currFlight.Airline.Name,
                    FlightNumber = currFlight.OutboundFlightNumber,
                    DepartureDateTime = currFlight.OutboundDepartureDateTime,
                    ArrivalDateTime = currFlight.OutboundArrivalDateTime,
                    Capacity = currFlight.Capacity,
                    Economy = currFlight.EconomySeats,
                    Business = currFlight.BusinessSeats,
                };

                int economyBooked = pnrMembers
                    .Where(e => e.FlightId == currFlight.FlightId && e.Type == PNRType.Economy)
                    .Count();

                int businessBooked = pnrMembers
                    .Where(e => e.FlightId == currFlight.FlightId && e.Type == PNRType.Business)
                    .Count();

                int economyBalance = currFlight.EconomySeats - economyBooked;
                int businessBalance = currFlight.BusinessSeats - businessBooked;

                entry.EconomyBookings = economyBooked;
                entry.BusinessBookings = businessBooked;

                entry.EconomyBalance = economyBalance;
                entry.BusinessBalance = businessBalance;

                var packages = flightPackages
                    .Where(e => e.FlightId == currFlight.FlightId)
                    .ToList();

                for (int j = 0; j < packages.Count; j++)
                {
                    entry.Packages.Add(packages.ElementAt(j).Name);
                }

                model.Entries.Add(
                    entry
                    );
            }

            return View(model);
        }

        public IActionResult Create()
        {

            ViewBag.ROE = _dbContext
                .ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(e => new { e.SARROE, e.USDROE })
                .FirstOrDefault();
            ViewBag.Airlines = _dbContext.AirlinesWithoutTrash;
            ViewBag.Packages = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            
            return View();
        }

        [HttpPost]
        public IActionResult Create(Flight flight)
        {
            //return Ok(flight);

            ViewBag.Airlines = _dbContext.AirlinesWithoutTrash;
            ViewBag.Packages = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            ViewBag.ROE = _dbContext
                .ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(e => new { e.SARROE, e.USDROE })
                .FirstOrDefault();

            if (!ModelState.IsValid) return View(flight);

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                flight.Capacity = flight.BusinessSeats + flight.EconomySeats;

                _dbContext.Flights.Add(flight);
                _dbContext.SaveChanges();

                for (int i = 0; i < (flight.PackageIds?.Count ?? 0); i++)
                {
                    if (flight.PackagesSelected[i] == false) continue;

                    _dbContext.FlightPackages.Add(new FlightPackages
                    {
                        PackageId = flight.PackageIds[i],
                        FlightId = flight.FlightId
                    });
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < flight.Destinations.Count; i++)
                {
                    _dbContext.Sectors.Add(new Sector
                    {
                        FlightId = flight.FlightId,
                        Destination = flight.Destinations[i],
                        FlightNo = flight.FlightNo[i],
                        ArrivalDateTime = flight.SectorArrivals[i],
                        DepartureDateTime = flight.SectorDepartures[i],
                        FlightType = flight.FlightTypes[i]
                    });
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < flight.PNRNames.Count; i++)
                {
                    _dbContext.PNRs.Add(new PNR
                    {
                        FlightId = flight.FlightId,
                        Name = flight.PNRNames[i],
                        Type = flight.PNRTypes[i],
                        Capacity = flight.Capacities[i],
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(flight);
            }



        }

        public IActionResult Edit(int id)
        {
            ViewBag.Airlines = _dbContext.AirlinesWithoutTrash;
            ViewBag.Packages = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );


            Flight flight = _dbContext.Flights.Find(id);
            ViewBag.FlightPackages = _dbContext.FlightPackages.Where(e => e.FlightId == flight.FlightId);
            ViewBag.Sectors = _dbContext.Sectors.Where(e => e.FlightId == flight.FlightId);

            return View(flight);
        }

        [HttpPost]
        public IActionResult Edit(Flight flight)
        {

            ViewBag.Airlines = _dbContext.AirlinesWithoutTrash;
            ViewBag.Packages = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.FlightPackages = _dbContext.FlightPackages.Where(e => e.FlightId == flight.FlightId);
            ViewBag.Sectors = _dbContext.Sectors.Where(e => e.FlightId == flight.FlightId);

            if (!ModelState.IsValid) return View(flight);

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.Entry(flight).State = EntityState.Modified;

                _dbContext.FlightPackages.Where(e => e.FlightId == flight.FlightId).ExecuteDelete();

                for (int i = 0; i < (flight.PackageIds?.Count ?? 0); i++)
                {
                    if (flight.PackagesSelected[i] == false) continue;

                    _dbContext.FlightPackages.Add(new FlightPackages
                    {
                        PackageId = flight.PackageIds[i],
                        FlightId = flight.FlightId
                    });
                }
                _dbContext.SaveChanges();


                _dbContext.Sectors.Where(e => e.FlightId == flight.FlightId).ExecuteDelete();
                for (int i = 0; i < flight.Destinations.Count; i++)
                {
                    _dbContext.Sectors.Add(new Sector
                    {
                        FlightId = flight.FlightId,
                        Destination = flight.Destinations[i],
                        FlightNo = flight.FlightNo[i],
                        ArrivalDateTime = flight.SectorArrivals[i],
                        DepartureDateTime = flight.SectorDepartures[i],
                        FlightType = flight.FlightTypes[i]
                    });
                }
                _dbContext.SaveChanges();


                transaction.Commit();


                TempData["updated"] = true;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return View(flight);
            }

        }

        public IActionResult Trash(int id)
        {
            Flight flight = _dbContext.Flights.Find(id);
            if (flight == null) return RedirectToAction(nameof(Index));

            flight.Trashed = true;

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            var flights = _dbContext.Flights
                .Include(e => e.Airline)
                .Where(ca => ca.Trashed)
                .ToList();

            var flightPackages = _dbContext.FlightPackages
                .Include(e => e.Package)
                .Select(e => new
                {
                    e.PackageId,
                    e.Package.Name,
                    e.Package.Trashed,
                    e.FlightId
                })
                .Where(ca => !ca.Trashed)
                .ToList();

            FlightIndexModel model = new FlightIndexModel();

            for (int i = 0; i < flights.Count; i++)
            {
                Flight currFlight = flights.ElementAt(i);

                FlightIndexEntryModel entry = new FlightIndexEntryModel
                {
                    FlightId = currFlight.FlightId,
                    AirlineName = currFlight.Airline.Name,
                    //ArrivalDateTime = currFlight.ArrivalDateTime,
                    //DepartureDateTime = currFlight.DepartureDateTime,
                    Capacity = currFlight.Capacity,
                };

                var packages = flightPackages
                    .Where(e => e.FlightId == currFlight.FlightId)
                    .ToList();

                for (int j = 0; j < packages.Count; j++)
                {
                    entry.Packages.Add(packages.ElementAt(j).Name);
                }

                model.Entries.Add(
                    entry
                    );
            }


            return View(model);
        }

        public IActionResult Restore(int id)
        {
            Flight flight = _dbContext.Flights.Find(id);
            flight.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetPackageFlights(int id)
        {
            var flights = _dbContext
                .FlightPackages
                .Include(e => e.Flight)
                .Select(e => new
                {
                    //e.Flight.Name,
                    e.FlightId,
                    e.PackageId,
                })
                .Where(e => e.PackageId == id);

            return Ok(flights);
        }

        public IActionResult PNRList(int id)
        {
            ViewBag.TrashCount = _dbContext.PNRs.Where(ca => ca.Trashed).Count();

            var pnrs = _dbContext
                .PNRsWithoutTrash
                .Include(e => e.Flight)
                .Where(e => e.FlightId == id)
                .ToList();

            var pnrMembers = _dbContext
                .PNRMembers
                .Include(e => e.PNR)
                .Where(e => !e.PNR.Trashed)
                .ToList();

            PNRIndexModel model = new PNRIndexModel();

            for (int i = 0; i < pnrs.Count; i++)
            {
                PNR currPnr = pnrs.ElementAt(i);
                int booked = pnrMembers.Where(e => e.PNRId == currPnr.PNRId).Count();
                int balance = currPnr.Capacity - booked;

                model.Entries.Add(
                    new PNRIndexEntryModel(
                        currPnr.PNRId, currPnr.Name, currPnr.PNRNo, currPnr.PNRType.ToString(),
                        currPnr.Flight.Name,
                        currPnr.Type.ToString(), currPnr.Capacity, booked, balance
                        )
                    );
            }

            return View(model);
        }

        public IActionResult btnReport_Click(int id, DateTime Date)
        {

            string? strErrorMessage = string.Empty;
            string? strModal = string.Empty;
            string? redirectUrl = string.Empty;

            if (id != null && Date != null)
            {
                redirectUrl = Url.Action("FlightSummaryDailyReportGenerate", "Flight",
                    new
                    {
                        id = id,
                        Date = Date
                    });
            }

            return Json(new { redirectTo = redirectUrl });
        }

        public IActionResult ViewTrashPNR()
        {
            var trashedPNRs = _dbContext.PNRs.Where(s => s.Trashed).ToList();


            ViewBag.trashedPNR = trashedPNRs;
            return View();
        }
        
        public JsonResult btnSave_Click(List<VehicleData> vehicleData)
        {
            //return Json(vehicleData);
            foreach (var data in vehicleData)
            {
                var familyHead = _dbContext.FamilyHeads.FirstOrDefault(fh => fh.FamilyHeadId == data._FamilyMembersId);
                if (familyHead != null)
                {
                    familyHead.AssignedVehicle = data.VehicleId;
                }
            }

            _dbContext.SaveChanges();
            return Json(new { redirectTo = Url.Action("FlightMembersDetailsArrivalFamilywise", "Flight") });
        }

        public IActionResult btnReport_Click2(int id, DateTime Date)
        {

            string? strErrorMessage = string.Empty;
            string? strModal = string.Empty;
            string? redirectUrl = string.Empty;

            if (id != null && Date != null)
            {
                redirectUrl = Url.Action("FlightSummaryDailyReportGenerateDeparture", "Flight",
                    new
                    {
                        id = id,
                        Date = Date
                    });
            }

            return Json(new { redirectTo = redirectUrl });
        }


    }
}

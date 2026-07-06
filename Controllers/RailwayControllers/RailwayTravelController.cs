using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using TMG.Models.PackageModels;
using TMG.Models.RailwayModels;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Controllers.FamilyControllers;
using TMG.Models.FamilyModels;

namespace TMG.Controllers.RailwayControllers
{
    public class RailwayTravelController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public RailwayTravelController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RailwayTravels.Where(ca => ca.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashAirlines(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateAirlines(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditAirlines(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreAirlines(_dbContext);

            var railwayTravels = _dbContext.RailwayTravels
                .Include(e => e.Railway)
                .Where(ca => !ca.Trashed)
                .ToList();

            var railwayPackages = _dbContext.RailwayTravelPackages
                .Include(e => e.Package)
                .Select(e => new
                {
                    e.PackageId,
                    e.Package.Name,
                    e.Package.Trashed,
                    e.RailwayTravelId
                })
                .Where(ca => !ca.Trashed)
                .ToList();

            var pnrMembers = _dbContext.RailwayPNRMembers
                .Include(e => e.RailwayPNR)
                .Select(e => new
                {
                    e.RailwayPNRId,
                    e.FamilyHeadId,
                    e.RailwayPNR.RailwayTravelId,
                    e.RailwayPNR.Type
                })
                .ToList();

            RailwayIndexModel model = new RailwayIndexModel();

            for (int i = 0; i < railwayTravels.Count; i++)
            {
                RailwayTravel currRailwayTravel = railwayTravels.ElementAt(i);

                RailwayIndexEntryModel entry = new RailwayIndexEntryModel
                {
                    RailwayTravelId = currRailwayTravel.RailwayTravelId,
                    RailwayName = currRailwayTravel.Railway.Name,
                    TrainNumber = currRailwayTravel.OutboundRailwayNumber,
                    DepartureDateTime = currRailwayTravel.OutboundDepartureDateTime,
                    ArrivalDateTime = currRailwayTravel.OutboundArrivalDateTime,
                    Capacity = currRailwayTravel.Capacity,
                    Economy = currRailwayTravel.EconomySeats,
                    Business = currRailwayTravel.BusinessSeats,
                };

                int economyBooked = pnrMembers
                    .Where(e => e.RailwayTravelId == currRailwayTravel.RailwayTravelId && e.Type == PNRType.Economy)
                    .Count();

                int businessBooked = pnrMembers
                    .Where(e => e.RailwayTravelId == currRailwayTravel.RailwayTravelId && e.Type == PNRType.Business)
                    .Count();

                int economyBalance = currRailwayTravel.EconomySeats - economyBooked;
                int businessBalance = currRailwayTravel.BusinessSeats - businessBooked;

                entry.EconomyBookings = economyBooked;
                entry.BusinessBookings = businessBooked;

                entry.EconomyBalance = economyBalance;
                entry.BusinessBalance = businessBalance;

                var packages = railwayPackages
                    .Where(e => e.RailwayTravelId == currRailwayTravel.RailwayTravelId)
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
                .Select(e => new { e.SARROE, e.USDROE })
                .FirstOrDefault();

            ViewBag.Railways = _dbContext.RailwaysWithoutTrash;
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
        public IActionResult Create(RailwayTravel railwayTravel)
        {
            //return Ok(railwayTravel);

            ViewBag.ROE = _dbContext
                .ApplicationSettings
                .Select(e => new { e.SARROE, e.USDROE })
                .FirstOrDefault();

            ViewBag.Railways = _dbContext.RailwaysWithoutTrash;
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

            if (!ModelState.IsValid) return View(railwayTravel);

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                railwayTravel.Capacity = railwayTravel.BusinessSeats + railwayTravel.EconomySeats;

                _dbContext.RailwayTravels.Add(railwayTravel);
                _dbContext.SaveChanges();

                for (int i = 0; i < (railwayTravel.PackageIds?.Count ?? 0); i++)
                {
                    if (railwayTravel.PackagesSelected[i] == false) continue;

                    _dbContext.RailwayTravelPackages.Add(new RailwayTravelPackages
                    {
                        PackageId = railwayTravel.PackageIds[i],
                        RailwayTravelId = railwayTravel.RailwayTravelId
                    });
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < (railwayTravel.PNRNames?.Count ?? 0); i++)
                {
                    _dbContext.RailwayPNRs.Add(new RailwayPNR
                    {
                        RailwayTravelId = railwayTravel.RailwayTravelId,
                        Name = railwayTravel.PNRNames[i],
                        Type = railwayTravel.PNRTypes[i],
                        Capacity = railwayTravel.Capacities[i],
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(railwayTravel);
            }



        }

        public IActionResult Trash(int id)
        {
            RailwayTravel railwayTravel = _dbContext.RailwayTravels.Find(id);
            if (railwayTravel == null) return RedirectToAction(nameof(Index));

            railwayTravel.Trashed = true;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            var railwayTravels = _dbContext.RailwayTravels
                .Include(e => e.Railway)
                .Where(ca => ca.Trashed)
                .ToList();

            var railwayPackages = _dbContext.RailwayTravelPackages
                .Include(e => e.Package)
                .Select(e => new
                {
                    e.PackageId,
                    e.Package.Name,
                    e.Package.Trashed,
                    e.RailwayTravelId
                })
                .Where(ca => !ca.Trashed)
                .ToList();

            var pnrMembers = _dbContext.RailwayPNRMembers
                .Include(e => e.RailwayPNR)
                .Select(e => new
                {
                    e.RailwayPNRId,
                    e.FamilyHeadId,
                    e.RailwayPNR.RailwayTravelId,
                    e.RailwayPNR.Type
                })
                .ToList();

            RailwayIndexModel model = new RailwayIndexModel();

            for (int i = 0; i < railwayTravels.Count; i++)
            {
                RailwayTravel currRailwayTravel = railwayTravels.ElementAt(i);

                RailwayIndexEntryModel entry = new RailwayIndexEntryModel
                {
                    RailwayTravelId = currRailwayTravel.RailwayId,
                    RailwayName = currRailwayTravel.Railway.Name,
                    DepartureDateTime = currRailwayTravel.OutboundDepartureDateTime,
                    ArrivalDateTime = currRailwayTravel.OutboundArrivalDateTime,
                    Capacity = currRailwayTravel.Capacity,
                    Economy = currRailwayTravel.EconomySeats,
                    Business = currRailwayTravel.BusinessSeats,
                };

                int economyBooked = pnrMembers
                    .Where(e => e.RailwayTravelId == currRailwayTravel.RailwayId && e.Type == PNRType.Economy)
                    .Count();

                int businessBooked = pnrMembers
                    .Where(e => e.RailwayTravelId == currRailwayTravel.RailwayId && e.Type == PNRType.Business)
                    .Count();

                int economyBalance = currRailwayTravel.EconomySeats - economyBooked;
                int businessBalance = currRailwayTravel.BusinessSeats - businessBooked;

                entry.EconomyBookings = economyBooked;
                entry.BusinessBookings = businessBooked;

                entry.EconomyBalance = economyBalance;
                entry.BusinessBalance = businessBalance;

                var packages = railwayPackages
                    .Where(e => e.RailwayTravelId == currRailwayTravel.RailwayTravelId)
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
            try
            {
                RailwayTravel railwayTravel = _dbContext.RailwayTravels.FirstOrDefault(a => a.RailwayId == id);

                if (railwayTravel != null)
                {
                    railwayTravel.Trashed = false;
                    _dbContext.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"RailwayTravel record with ID {id} was not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while restoring RailwayTravel with ID {id}: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.ROE = _dbContext
                .ApplicationSettings
                .Select(e => new { e.SARROE, e.USDROE })
                .FirstOrDefault();

            ViewBag.Railways = _dbContext.RailwaysWithoutTrash;
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

            RailwayTravel railwayTravel = _dbContext.RailwayTravels.Find(id);
            ViewBag.RailwayTravelPackages = _dbContext
                .RailwayTravelPackages
                .Where(e => e.RailwayTravelId == railwayTravel.RailwayTravelId);

            return View(railwayTravel);
        }

        [HttpPost]
        public IActionResult Edit(RailwayTravel railwayTravel)
        {

            ViewBag.ROE = _dbContext
                 .ApplicationSettings
                 .Select(e => new { e.SARROE, e.USDROE })
                 .FirstOrDefault();

            ViewBag.Railways = _dbContext.RailwaysWithoutTrash;
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

            ViewBag.RailwayTravelPackages = _dbContext
                .RailwayTravelPackages
                .Where(e => e.RailwayTravelId == railwayTravel.RailwayTravelId);

            if (!ModelState.IsValid) return View(railwayTravel);

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.Entry(railwayTravel).State = EntityState.Modified;

                _dbContext.RailwayTravelPackages
                    .Where(e => e.RailwayTravelId == railwayTravel.RailwayTravelId)
                    .ExecuteDelete();

                for (int i = 0; i < (railwayTravel.PackageIds?.Count ?? 0); i++)
                {
                    if (railwayTravel.PackagesSelected[i] == false) continue;

                    _dbContext.RailwayTravelPackages.Add(new RailwayTravelPackages
                    {
                        PackageId = railwayTravel.PackageIds[i],
                        RailwayTravelId = railwayTravel.RailwayTravelId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return View(railwayTravel);
            }

        }

        public IActionResult PNRList(int id)
        {
            ViewBag.TrashCount = _dbContext.RailwayPNRs.Where(ca => ca.Trashed).Count();

            var pnrs = _dbContext
                .RailwayPNRsWithoutTrash
                .Include(e => e.RailwayTravel)
                .Where(e => e.RailwayTravelId == id)
                .ToList();

            var pnrMembers = _dbContext
                .RailwayPNRMembers
                .Include(e => e.RailwayPNR)
                .Where(e => !e.RailwayPNR.Trashed)
                .ToList();

            RailwayPNRIndexModel model = new RailwayPNRIndexModel();

            for (int i = 0; i < pnrs.Count; i++)
            {
                RailwayPNR currPnr = pnrs.ElementAt(i);
                int booked = pnrMembers.Where(e => e.RailwayPNRId == currPnr.RailwayPNRId).Count();
                int balance = currPnr.Capacity - booked;

                model.Entries.Add(
                    new RailwayPNRIndexEntryModel(
                        currPnr.RailwayPNRId, currPnr.Name,
                        currPnr.RailwayTravel.Name,
                        currPnr.Type.ToString(), currPnr.Capacity, booked, balance
                        )
                    );
            }

            return View(model);
        }

        public IActionResult PNRReport(int id)
        {
            RailwayTravel travel = _dbContext.RailwayTravels.Find(id);

            var pnrs = _dbContext.RailwayPNRs.Where(e => e.RailwayTravelId == id).ToList();
            var pnrMembers = _dbContext.RailwayPNRMembers
                .Include(e => e.RailwayPNR)
                .Where(e => e.RailwayPNR.RailwayTravelId == id)
                .Select(e => new
                {
                    e.FamilyHead.FamilyHeadId,
                    e.FamilyHead.GivenName,
                    e.FamilyHead.SurName,
                    e.FamilyHead.Gender,
                    e.FamilyHead.PassportNumber,
                    e.FamilyHead.PassportDateOfExpiry,
                    e.FamilyHead.DateOfBirth,
                    e.RailwayPNRId,
                    SPOName = e.FamilyHead.SPO.Name,
                    PackageName = e.FamilyHead.Package.Name
                })
                .OrderBy(e => e.FamilyHeadId)
                .ToList();

            RailwayPNRReportModel model = new();
            model.Destination = travel.Destination;
            model.RailwayNumber = travel.Name;
            model.DepartureDate = DateTimeManager.GetDate(travel.OutboundDepartureDateTime);
            model.DepartureTime = travel.OutboundDepartureDateTime.ToString("HH:mm");
            model.ArrivalDate = DateTimeManager.GetDate(travel.OutboundArrivalDateTime);
            model.ArrivalTime = travel.OutboundArrivalDateTime.ToString("HH:mm");


            for (int i = 0; i < pnrs.Count; i++)
            {
                RailwayPNR currPnr = pnrs.ElementAt(i);
                var currPnrMembers = pnrMembers.Where(e => e.RailwayPNRId == currPnr.RailwayPNRId).ToList();

                RailwayPNRReportEntryModel entry = new();
                entry.PNR = currPnr.Name;

                for (int j = 0; j < currPnrMembers.Count; j++)
                {
                    var currMember = currPnrMembers.ElementAt(j);
                    entry.PNRMembers.Add(new RailwayPNRMembersReportModel
                    {
                        SNo = j + 1,
                        GivenName = currMember.GivenName,
                        SPOName = currMember.SPOName,
                        PackageName = currMember.PackageName,
                        SurName = currMember.SurName,
                        Gender = currMember.Gender.ToString(),
                        PassportNumber = currMember.PassportNumber,
                        PassportDOE = currMember.PassportDateOfExpiry?.ToString("dd/MMM/yyyy"),
                        DateOfBirth = currMember.DateOfBirth.ToString("dd/MMM/yyyy"),
                    });
                }

                model.Entries.Add(entry);
            }


            return View(model);
        }

        public IActionResult RailwayTravelSummary(string dateType, DateTime? filterDate)
        {

            List<TrainReportSummaryModel> trainSummaries = new List<TrainReportSummaryModel>();

            var familyHeads = _dbContext.FamilyHeadsWithoutTrash
                               .Where(e => e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                               .OrderBy(e => e.FamilyHeadId)
                               .ToList();

            var familyHeadTrains = _dbContext.FamilyHeadTrain
                .Select(e => new
                {
                    e.FamilyHead.Package.HajjSeasonsId,
                    e.FamilyHead.Package.Trashed,
                    Railway = e.Railway.Name,
                    e.TrainClass,
                    e.ArrivalDateTime,
                    e.DepartureDateTime,
                    e.TrainNo,
                    e.Origin,
                    e.Destination,
                    e.FamilyHead.PackageId,
                    e.FamilyHeadId,
                    PackageName = e.FamilyHead.Package.Name,
                    e.Total,
                    e.PNRNo,
                })
                .Where(e => e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => !e.Trashed)
                .ToList();


            var packageTrains = _dbContext.PackageTrains
                                    .Select(e => new
                                    {
                                        e.Package.HajjSeasonsId,
                                        e.Package.Trashed,
                                        Railway = e.Railway.Name,
                                        e.TrainClass,
                                        //e.ArrivalDateTime,
                                        //e.DepartureDateTime,
                                        e.TrainNumber,
                                        e.Origin,
                                        e.Destination,
                                        e.PackageId,
                                        PackageName = e.Package.Name,
                                        e.TicketAmount,
                                        e.PNRNumber,
                                    })
                                    .Where(e => e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                                    .Where(e => !e.Trashed)
                                    .ToList();


            foreach (FamilyHead member in familyHeads)
            {
                if (member.TrainAsPerPackage)
                {
                    var packageFlightInfo = packageTrains.Where(e => e.PackageId == member.PackageId);

                    foreach (var item in packageFlightInfo)
                    {
                        TrainReportSummaryModel _ = new()
                        {
                            TrainClass = item.TrainClass?.ToString(),
                            TrainNo = item.TrainNumber,
                            TrainName = item.Railway,
                            //ArrivalDate = item.ArrivalDateTime?.ToString("dd/MMM/yyyy"),
                            //ArrivalTime = item.ArrivalDateTime?.ToString("HH:mm"),
                            DateOfBirth = member.DateOfBirth.ToString("dd/MMM/yyyy"),
                            //DepartureDate = item.DepartureDateTime?.ToString("dd/MMM/yyyy"),
                            //DepartureTime = item.DepartureDateTime?.ToString("HH:mm"),
                            Destination = item.Destination,
                            FamilyHeadGivenName = member.GivenName,
                            FamilyHeadSurName = member.SurName,
                            GroupName = "",
                            HBNo = member.FcId,
                            Origin = item.Origin,
                            PackageName = item.PackageName,
                            PassportExpiry = member.PassportDateOfExpiry?.ToString("dd/MMM/yyyy"),
                            PassportNumber = member.PassportNumber,
                            PNRNo = item.PNRNumber,
                            TicketNo = "",
                            Total = item.TicketAmount ?? 0f,
                            VehicleToHotel = "",
                        };

                        trainSummaries.Add(_);
                    }
                }
                else
                {
                    var familyHeadTrainsInfo = familyHeadTrains.Where(e => e.FamilyHeadId == member.FamilyHeadId);

                    foreach (var item in familyHeadTrainsInfo)
                    {
                        TrainReportSummaryModel _ = new()
                        {
                            TrainClass = item.TrainClass?.ToString(),
                            TrainNo = item.TrainNo,
                            TrainName = item.Railway,
                            ArrivalDate = item.ArrivalDateTime?.ToString("dd/MMM/yyyy"),
                            ArrivalTime = item.ArrivalDateTime?.ToString("HH:mm"),
                            DateOfBirth = member.DateOfBirth.ToString("dd/MMM/yyyy"),
                            DepartureDate = item.DepartureDateTime?.ToString("dd/MMM/yyyy"),
                            DepartureTime = item.DepartureDateTime?.ToString("HH:mm"),
                            Destination = item.Destination,
                            FamilyHeadGivenName = member.GivenName,
                            FamilyHeadSurName = member.SurName,
                            GroupName = "",
                            HBNo = member.FcId,
                            Origin = item.Origin,
                            PackageName = item.PackageName,
                            PassportExpiry = member.PassportDateOfExpiry?.ToString("dd/MMM/yyyy"),
                            PassportNumber = member.PassportNumber,
                            PNRNo = item.PNRNo,
                            TicketNo = "",
                            Total = item.Total ?? 0f,
                            VehicleToHotel = "",
                        };

                        trainSummaries.Add(_);
                    }
                }
            }

            return View(trainSummaries);
        }

        public IActionResult RailwaySummaryDailyReport()
        {

            var train = _dbContext.RailwayTravels
                .Include(a => a.Railway)
                .ToList();

            var places = _dbContext.Places
            .ToList();

            foreach (var place in places)
            {
                if (place.PlaceId == 2)
                {
                    place.PlaceName = "Medinah";
                }
                else
                {
                    place.PlaceName = "Makkah";
                }
            }

            var uniquePlaces = places.GroupBy(p => p.PlaceName)
                                     .Select(g => g.First())
                                     .ToList();




            ViewBag.Train = train.Where(a => !a.Trashed);
            ViewBag.Place = uniquePlaces;


            ViewBag.TrashCount = _dbContext.Places.Where(e => e.Trashed).Count();
            ViewBag.Places = _dbContext.Places.Where(a => !a.Trashed).ToList();
            return View();

        }

        public IActionResult btnReport_Click(int id, DateTime Date)
        {

            string? strErrorMessage = string.Empty;
            string? strModal = string.Empty;
            string? redirectUrl = string.Empty;

            if (id != null && Date != null)
            {
                redirectUrl = Url.Action("RailwaySummaryDailyReportGenerate", "RailwayTravel",
                    new
                    {
                        id = id,
                        Date = Date
                    });
            }

            return Json(new { redirectTo = redirectUrl });
        }

        public IActionResult RailwaySummaryDailyReportGenerate(int id, DateTime Date)
        {
            string PlaceValue = (id == 1 ? "MAK" : "MED");

            var trainsByDate = _dbContext.RailwayTravels
     .Where(a => a.OutboundArrivalDateTime.Date == Date.Date)
                .Select(a => a.OutboundArrivalDateTime.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var time = (from r in _dbContext.RailwayTravels
                        where !r.Trashed && r.OutboundArrivalDateTime.Date == Date.Date && r.Destination.EndsWith(PlaceValue)
                        group r by r.OutboundRailwayNumber into g
                        select new
                        {
                            OutboundRailwayNumber = g.Key,
                            OutboundArrivalDateTime = g.Max(s => s.OutboundArrivalDateTime)
                        })
                           .OrderBy(a => a.OutboundArrivalDateTime)
              .ToList();

            var trains = (from r in _dbContext.RailwayTravels
                          join al in _dbContext.Railways on r.RailwayId equals al.RailwayId
                          where r.OutboundArrivalDateTime.Date == Date.Date && !r.Trashed && r.Destination.EndsWith(PlaceValue)
                          group r by new { r.OutboundRailwayNumber, r.OutboundArrivalDateTime.Date } into g
                          select new
                          {
                              RNo = g.Key.OutboundRailwayNumber,
                              OutboundArrivalDateTime = g.Max(a => a.OutboundArrivalDateTime)
                          })
                           .OrderBy(a => a.OutboundArrivalDateTime)
                           .ToList();

            var place = _dbContext.Places
                    .Where(a => a.PlaceId == id && !a.Trashed)
                    .ToList();

            var companies = (from rp in _dbContext.RailwayTravelPackages
                             join r in _dbContext.RailwayTravels on rp.RailwayTravelId equals r.RailwayTravelId
                             join p in _dbContext.Packages on rp.PackageId equals p.PackageId
                             join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                             where r.OutboundArrivalDateTime.Date == Date.Date
                                   && r.Destination.EndsWith(PlaceValue)
                                   && r.Trashed == false
                             select c).Distinct();

            var dateFilter = Date.Date;


            var results = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join rp in _dbContext.RailwayTravelPackages on p.PackageId equals rp.PackageId
                           join r in _dbContext.RailwayTravels on rp.RailwayTravelId equals r.RailwayTravelId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join pnr in _dbContext.RailwayPNRs on r.RailwayTravelId equals pnr.RailwayTravelId
                           join pnrm in _dbContext.RailwayPNRMembers on pnr.RailwayPNRId equals pnrm.RailwayPNRId
                           where r.OutboundArrivalDateTime.Date == Date.Date
                                 && pnrm.FamilyHeadId == fm.FamilyHeadId
                                 && r.Destination.EndsWith(PlaceValue)
                                 && r.Trashed == false
                                 && fh.Trashed == false
                           group new { fh, fm, p, rp, r, c, pnr, pnrm } by new { c.CompanyCode, r.OutboundRailwayNumber } into g
                           select new
                           {
                               g.Key.CompanyCode,
                               OutboundArrivalDateTime = g.Max(x => x.r.OutboundArrivalDateTime),
                               g.Key.OutboundRailwayNumber,
                               FamilyHeadCount = g.Select(x => x.fh.FamilyHeadId).Distinct().Count(),
                               TrainId = g.Min(x => x.r.RailwayTravelId)
                           })
                           .OrderBy(a => a.OutboundArrivalDateTime.TimeOfDay)
            .ToList();


            Dictionary<int, int> trainIdsDict = new Dictionary<int, int>();
            int snotrain = 0;

            foreach (var item in trains)
            {
                var trainIds = _dbContext.RailwayTravels
    .Where(f => f.OutboundArrivalDateTime.Date == Date.Date &&
                f.OutboundRailwayNumber == item.RNo &&
                !f.Trashed)
    .OrderByDescending(f => f.RailwayTravelId)
    .Select(f => new { f.RailwayTravelId, f.OutboundRailwayNumber, f.OutboundArrivalDateTime })
    .FirstOrDefault();
                if (trainIds != null)
                {
                    trainIdsDict.Add(snotrain + 1, Convert.ToInt32(trainIds.RailwayTravelId));
                    snotrain++;
                }

            }


            ViewBag.Place = place; // done
            ViewBag.train = trainsByDate; // 
            ViewBag.time = time;
            ViewBag.Trains = trains;
            ViewBag.Companies = companies;
            ViewBag.Results = results;
            if (trainIdsDict.Count > 0)
            {
                ViewBag.TrainId = trainIdsDict;
            }
            else
            {
                ViewBag.TrainId = 0;
            }


            return View();
        }

        public IActionResult TrainMembersDetailsArrival(int id)
        {

            var outbondarrival = _dbContext.RailwayTravels
                                .Where(s => s.RailwayTravelId == id)
                                .Select(s => new { s.OutboundArrivalDateTime, s.OutboundRailwayNumber })
                                .ToList();


            var date = outbondarrival[0].OutboundArrivalDateTime.Date;
            var trainnumber = outbondarrival[0].OutboundRailwayNumber;
            var query = from fh in _dbContext.FamilyHeads
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                        from gm in gmGroup.DefaultIfEmpty()
                        join g in _dbContext.Groups.Where(g => !g.Trashed) on gm.GroupId equals g.GroupId into gGroup
                        from g in gGroup.DefaultIfEmpty()
                        join cm in _dbContext.EHajjContractMembers on fh.FamilyHeadId equals cm.FamilyHeadId into cmGroup
                        from cm in cmGroup.DefaultIfEmpty()
                        join ehc in _dbContext.EHajjContracts on cm.EhajjContractId equals ehc.EhajjContractId into ehcGroup
                        from ehc in ehcGroup.DefaultIfEmpty()
                        join h in _dbContext.Hotels on ehc.HotelId equals h.HotelId into hGroup
                        from h in hGroup.DefaultIfEmpty()
                        join pnrm in _dbContext.RailwayPNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
                        join pnr in _dbContext.RailwayPNRs on pnrm.RailwayPNRId equals pnr.RailwayPNRId
                        join f in _dbContext.RailwayTravels on pnr.RailwayTravelId equals f.RailwayTravelId
                        where f.OutboundArrivalDateTime.Date == date && f.OutboundRailwayNumber == trainnumber && !fh.Trashed
                        orderby fh.FamilyHeadId
                        select new
                        {
                            FmId = fh.FamilyHeadId,
                            IsHead = fh.IsHead,
                            FullName = fh.GivenName + " " + fh.SurName,
                            MemberImage = fh.MemberImage ?? "",

                            //              CID = (c.CompanyCode + "." + fh.IndId + "." + fh.FcId.Substring(3) + ".(" +
                            //              _dbContext.FamilyMembers.Count(fm => fm.HeadId == fh.FamilyHeadId) + ")" +
                            //              p.Name + (gm != null ? g.Name + g.GroupId.ToString() : "")) ?? "",


                            CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                            $".{fm.Head.ExtractedFamilyNumber}.({_dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count()})" +
                            $".{fm.FamilyHead.Package.Name}",

                            PackageName = p.Name ?? "",
                            PassportNumber = fh.PassportNumber ?? "",
                            Gender = fh.Gender,
                            EhajjContractNumber = ehc.EhajjContractNumber ?? "",
                            HotelName = h.HotelName ?? "",
                            CheckIn = ehc.CheckIn != null ? ehc.CheckIn : DateTime.MinValue,
                            CheckOut = ehc.CheckOut == null ? DateTime.MinValue : ehc.CheckOut,
                            HotelReservationNumber = ehc.HotelReservationNumber ?? "",
                            FcId = fh.FcId ?? "",
                            PNR = pnr.Name ?? "",
                            TrainNo = f.OutboundRailwayNumber ?? "",
                        };

            var result = query.ToList();


            var pnrs = (from fh in _dbContext.FamilyHeads
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                        from gm in gmGroup.DefaultIfEmpty()
                        join g in _dbContext.Groups.Where(g => !g.Trashed) on gm.GroupId equals g.GroupId into gGroup
                        from g in gGroup.DefaultIfEmpty()
                        join cm in _dbContext.EHajjContractMembers on fh.FamilyHeadId equals cm.FamilyHeadId into cmGroup
                        from cm in cmGroup.DefaultIfEmpty()
                        join ehc in _dbContext.EHajjContracts on cm.EhajjContractId equals ehc.EhajjContractId into ehcGroup
                        from ehc in ehcGroup.DefaultIfEmpty()
                        join h in _dbContext.Hotels on ehc.HotelId equals h.HotelId into hGroup
                        from h in hGroup.DefaultIfEmpty()
                        join pnrm in _dbContext.RailwayPNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
                        join pnr in _dbContext.RailwayPNRs on pnrm.RailwayPNRId equals pnr.RailwayPNRId
                        join f in _dbContext.RailwayTravels on pnr.RailwayTravelId equals f.RailwayTravelId
                        where f.OutboundArrivalDateTime.Date == date && f.OutboundRailwayNumber == trainnumber && !fh.Trashed
                        select new
                        {
                            PNR = pnr.Name
                        }).Distinct()
                        .ToList();


            ViewBag.details = result;
            ViewBag.PNR = pnrs;

            return View();

        }

        public IActionResult TrainMembersDetailsArrivalFamilywise(string id)
        {


            var query = from fh in _dbContext.FamilyHeads
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join fhc in _dbContext.FamilyHeadContacts on new { fh.FamilyHeadId, Preferred = true } equals new { fhc.FamilyHeadId, fhc.Preferred } into fhcGroup
                        from fhc in fhcGroup.DefaultIfEmpty()
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                        from gm in gmGroup.DefaultIfEmpty()
                        join g in _dbContext.Groups on gm.GroupId equals g.GroupId into gGroup
                        from g in gGroup.DefaultIfEmpty()
                        join cm in _dbContext.EHajjContractMembers on fh.FamilyHeadId equals cm.FamilyHeadId into cmGroup
                        from cm in cmGroup.DefaultIfEmpty()
                        join ehc in _dbContext.EHajjContracts on cm.EhajjContractId equals ehc.EhajjContractId into ehcGroup
                        from ehc in ehcGroup.DefaultIfEmpty()
                        join h in _dbContext.Hotels on ehc.HotelId equals h.HotelId into hGroup
                        from h in hGroup.DefaultIfEmpty()
                        join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId into mcGroup
                        from mc in mcGroup.DefaultIfEmpty()
                        join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId into mGroup
                        from m in mGroup.DefaultIfEmpty()
                        join pnrm in _dbContext.RailwayPNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
                        join pnr in _dbContext.RailwayPNRs on pnrm.RailwayPNRId equals pnr.RailwayPNRId
                        join f in _dbContext.RailwayTravels on pnr.RailwayPNRId equals f.RailwayTravelId
                        where fh.FcId == id && !fh.Trashed
                        select new
                        {

                            FullName = fh.GivenName + " " + fh.SurName,
                            MemberImage = fh.MemberImage ?? "",
                            CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                            $".{fm.Head.ExtractedFamilyNumber}.({_dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count()})" +
                            $".{fm.FamilyHead.Package.Name}",
                            PackageName = p.Name ?? "",
                            PassportNumber = fh.PassportNumber ?? "",
                            fh.Gender,
                            EhajjContractNumber = ehc.EhajjContractNumber ?? "",
                            Contact = fhc.Contact ?? "",
                            HotelName = h.HotelName ?? "",
                            Location = h.Location ?? "",
                            CheckIn = ehc.CheckIn != null ? ehc.CheckIn : DateTime.MinValue,
                            CheckOut = ehc.CheckOut == null ? DateTime.MinValue : ehc.CheckOut,
                            HotelReservationNumber = ehc.HotelReservationNumber ?? "",
                            QR = ehc.QrPath ?? "",
                            ArrivalDate = pnr.RailwayTravel.OutboundArrivalDateTime != null ? pnr.RailwayTravel.OutboundArrivalDateTime : DateTime.MinValue,
                            TrainNo = pnr.RailwayTravel.OutboundRailwayNumber ?? "",
                            Nights = ehc.CheckOut != null && ehc.CheckIn != null
                    ? (ehc.CheckOut.Day - ehc.CheckIn.Day)
                    : 0,
                            CompanyName = c.CompanyName ?? "",
                            MunazimName = m.Name ?? "",
                            PNR = pnrm.RailwayPNR.Name ?? "",
                        };

            var result = query.ToList();

            int maleCount = result.Count(a => a.Gender == 0);
            int femaleCount = result.Count(a => (int)a.Gender == 1);
            int total = maleCount + femaleCount;

            ViewBag.Male = maleCount;
            ViewBag.Female = femaleCount;
            ViewBag.PAX = total;

            ViewBag.details = result;
            return View();

        }

        public IActionResult PNRReportV2(int id)
        {
            RailwayTravel travel = _dbContext.RailwayTravels.Find(id);

            var pnrs = _dbContext.RailwayPNRs.Where(e => e.RailwayTravelId == id).ToList();

            //var pnrMembers = _dbContext.RailwayPNRMembers
            //    .Include(e => e.RailwayPNR)
            //    .Where(e => e.RailwayPNR.RailwayTravelId == id)
            //    .Select(e => new {
            //        e.FamilyHead.FamilyHeadId,
            //        e.FamilyHead.GivenName,
            //        e.FamilyHead.SurName,
            //        e.FamilyHead.Gender,
            //        e.FamilyHead.PassportNumber,
            //        e.FamilyHead.PassportDateOfExpiry,
            //        e.FamilyHead.DateOfBirth,
            //        e.RailwayPNRId,
            //        SPOName = e.FamilyHead.SPO.Name,
            //        PackageName = e.FamilyHead.Package.Name
            //    })
            //    .OrderBy(e => e.FamilyHeadId)
            //    .ToList();

            var pnrMembers = (from rpm in _dbContext.RailwayPNRMembers
                              join rp in _dbContext.RailwayPNRs on rpm.RailwayPNRId equals rp.RailwayPNRId
                              join fh in _dbContext.FamilyHeads on rpm.FamilyHeadId equals fh.FamilyHeadId
                              join spo in _dbContext.SPOs on fh.SPOId equals spo.SPOId into spoJoin
                              from spo in spoJoin.DefaultIfEmpty()
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId into pJoin
                              from p in pJoin.DefaultIfEmpty()
                              join c in _dbContext.Companies on p.CompanyId equals c.CompanyId into cJoin
                              from c in cJoin.DefaultIfEmpty()
                              join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId into mcJoin
                              from mc in mcJoin.DefaultIfEmpty()
                              join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId into mJoin
                              from m in mJoin.DefaultIfEmpty()
                              where rp.RailwayTravelId == id
                              orderby fh.FamilyHeadId
                              select new
                              {
                                  Munazzim = m.Name,
                                  HGO = c.CompanyName,
                                  fh.FamilyHeadId,
                                  fh.GivenName,
                                  fh.SurName,
                                  Gender = fh.Gender,
                                  fh.PassportNumber,
                                  fh.PassportDateOfExpiry,
                                  fh.DateOfBirth,
                                  rpm.RailwayPNRId,
                                  SPOName = spo.Name,
                                  PackageName = p.Name
                              }).ToList();

            RailwayPNRReportModel model = new();
            model.Destination = travel.Destination;
            model.RailwayNumber = travel.Name;
            model.DepartureDate = DateTimeManager.GetDate(travel.OutboundDepartureDateTime);
            model.DepartureTime = travel.OutboundDepartureDateTime.ToString("HH:mm");
            model.ArrivalDate = DateTimeManager.GetDate(travel.OutboundArrivalDateTime);
            model.ArrivalTime = travel.OutboundArrivalDateTime.ToString("HH:mm");


            var pnrMembersFamilyHeadIds = pnrMembers.Select(e => e.FamilyHeadId).ToList();

            var maktab = from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                         where pa.PlaceId == 3
                         select new
                         {
                             fh.FamilyHeadId,
                             pa.AccomodationName
                         };




            for (int i = 0; i < pnrs.Count; i++)
            {
                RailwayPNR currPnr = pnrs.ElementAt(i);
                var currPnrMembers = pnrMembers.Where(e => e.RailwayPNRId == currPnr.RailwayPNRId).ToList();

                RailwayPNRReportEntryModel entry = new();
                entry.PNR = currPnr.Name;

                for (int j = 0; j < currPnrMembers.Count; j++)
                {
                    var currMember = currPnrMembers.ElementAt(j);
                    entry.PNRMembers.Add(new RailwayPNRMembersReportModel
                    {
                        SNo = j + 1,
                        Munazzim = currMember.Munazzim,
                        HGO = currMember.HGO,
                        GivenName = currMember.GivenName,
                        SPOName = currMember.SPOName,
                        PackageName = currMember.PackageName,
                        SurName = currMember.SurName,
                        Gender = currMember.Gender.ToString(),
                        PassportNumber = currMember.PassportNumber,
                        PassportDOE = currMember.PassportDateOfExpiry?.ToString("dd/MMM/yyyy"),
                        DateOfBirth = currMember.DateOfBirth.ToString("dd/MMM/yyyy"),
                        Maktab = maktab.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId).AccomodationName.Contains("-A") ? "1" : "3"
                    });
                }

                model.Entries.Add(entry);
            }


            return View(model);
        }

    }
}

namespace TMG.Models.RailwayModels
{

    public record DailyTrainSummaryReportModel
    {
        public string Place { get; set; }
        public string Date { get; set; }
        public List<string> ArrivalHeaders { get; set; }
        public List<string> TrainNumbers { get; set; }
        public List<int> TrainIds { get; set; }
        public List<DailyTrainSummaryReportRowModel> Rows { get; set; } = new();
        public List<int?> Totals { get; set; } = new();
    }

    public record DailyTrainSummaryReportRowModel
    {
        public int SrNo { get; set; }
        public string HGO { get; set; }
        public List<DailyTrainSummaryReportRowPaxModel> PAXES { get; set; } = new();
        public List<int> TrainIds { get; set; } = new();
    }
    public record DailyTrainSummaryReportRowPaxModel
    {
        public int? PAX { get; set; }
        public int? TrainId { get; set; }
    }

    public record TrainSummaryReportModel
    {
        public List<TrainSummaryMembers> TrainMembers { get; set; } = new List<TrainSummaryMembers>();
        public List<string> departureDateHeader { get; set; } = new();
        public List<string> arrivalDateHeader { get; set; } = new();
    }
    public record TrainSummaryDailyReportModel
    {
        public List<TrainSummaryDailyReportMembers> TrainDailyMembers { get; set; } = new();
    }
    public record TrainSummaryDailyReportMembers
    {
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string PlaceName { get; set; }
        public string PlaceId { get; set; }
        public string PAXCount { get; set; }
    }
    public record TrainSummaryMembers
    {
        public string PackageName { get; set; }
        public string HotelName { get; set; }
        public string HGO { get; set; }
        public string GroupName { get; set; }
        public string PAXName { get; set; }
        public string RailwayName { get; set; }
        public string DepartureDateTime { get; set; }
        public string DepartureDate { get; set; }
        public string PAXNameCount { get; set; }
        public string PNR { get; set; }
        public string OutBound { get; set; }
        public string OutBoundTrainNo { get; set; }
        public string OutboundDate { get; set; }
        public string OutboundArrivalDateTime { get; set; }
    }

    public record RailwayIndexModel
    {
        public List<RailwayIndexEntryModel> Entries { get; set; } = new List<RailwayIndexEntryModel>();
    }

    public record RailwayIndexEntryModel
    {
        public int RailwayTravelId { get; set; }
        public string RailwayName { get; set; }
        public string TrainNumber { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public int Capacity { get; set; }
        public int Economy { get; set; }
        public int Business { get; set; }
        public int EconomyBookings { get; set; }
        public int BusinessBookings { get; set; }
        public int EconomyBalance { get; set; }
        public int BusinessBalance { get; set; }
        public List<string> Packages { get; set; } = new List<string>();

    }

    public record RailwayPNRIndexModel
    {
        public List<RailwayPNRIndexEntryModel> Entries { get; set; } = new();
    }
    public record RailwayPNRIndexEntryModel(int PNRId,
        string Name,
        string RailName, string PNRType, int Capacity, int Booked, int Balance);

    public record RailwayPNRReportModel
    {
        public string RailwayNumber { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public List<RailwayPNRReportEntryModel> Entries { get; set; } = new List<RailwayPNRReportEntryModel>();
    }


    public record RailwayPNRReportEntryModel
    {
        public string PNR { get; set; }
        public List<RailwayPNRMembersReportModel> PNRMembers { get; set; } = new();
    }

    public record RailwayPNRMembersReportModel
    {
        public int SNo { get; set; }
        public string Munazzim { get; set; }
        public string HGO { get; set; }
        public string SPOName { get; set; }
        public string PackageName { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
        public string PassportDOE { get; set; }
        public string DateOfBirth { get; set; }
        public string Maktab { get; set; }
    }
}

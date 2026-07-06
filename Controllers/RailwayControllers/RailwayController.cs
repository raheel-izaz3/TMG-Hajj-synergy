using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using TMG.Models.RailwayModels;
using TMG.Models.RailwayModels;
using TMG.Models.RailwayModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.RailwayControllers
{
    public class RailwayController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public RailwayController(TMGDbContext humsDbContext)
        {
            _dbContext = humsDbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Railways.Where(r => r.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashRole(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateRole(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditRole(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreRole(_dbContext);

            return View(_dbContext.RailwaysWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Railway request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.Railways.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Railway railway = _dbContext.Railways.Find(id);
            if (railway == null) return RedirectToAction(nameof(Index));

            return View(railway);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Railway request)
        {
            if (!ModelState.IsValid) return View(request);

            Railway railway = _dbContext.Railways.Find(request.RailwayId);

            railway.Name = request.Name;
            railway.Code = request.Code;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            Railway railway = _dbContext.Railways.Find(id);
            if (railway == null) return RedirectToAction(nameof(Index));

            railway.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Railways.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            Railway railway = _dbContext.Railways.Find(id);
            railway.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Summary(int id)
        {
            Railway railway = _dbContext.Railways.Find(id);

            var pnrs = _dbContext.RailwayPNRs
                .Include(e => e.RailwayTravel)
                .Where(e => e.RailwayTravel.RailwayTravelId == id)
                .ToList();

            var pnrMembers = _dbContext.RailwayPNRMembers
                .Include(e => e.RailwayPNR)
                .ThenInclude(e => e.RailwayTravel)
                .ThenInclude(e => e.Railway)
                .Where(e => e.RailwayPNR.RailwayTravel.RailwayId == id)
                .Select(e => new
                {
                    e.RailwayPNR.RailwayTravel.RailwayId,
                    e.FamilyHeadId,
                    e.RailwayPNRId
                });

            var packages = _dbContext.PackagesWithoutTrash
                .Include(e => e.Company)
                .Select(e => new
                {
                    e.Name,
                    e.Company.CompanyName,
                    e.Company.CompanyCode
                })
                .ToList();

            var railwayPackages = _dbContext.RailwayTravelPackages
                .Include(e => e.RailwayTravel)
                .Include(e => e.Package)
                .ThenInclude(e => e.Company)
                .Select(e => new
                {
                    e.Package.Name,
                    e.Package.Company.CompanyCode,
                    e.Package.Company.CompanyName,
                    e.RailwayTravelId
                })
                .ToList();

            var travels = _dbContext.RailwayTravels
                .Where(e => e.RailwayId == id)
                .ToList();


            RailwaySummaryModel model = new();


            string tableHeader = "";

            tableHeader += $"<th>#</th>" +
                $"<th>PNR #</th>" +
                $"<th>PNR Name</th>" +
                $"<th>Class</th>" +
                $"<th>Company</th>" +
                $"<th>Package</th>" +
                $"<th>Airline</th>" +
                $"<th>Sector</th>" +
                $"<th>Train #</th>" +
                $"<th>Departure Date</th>" +
                $"<th>Departure Time</th>" +
                $"<th>Arrival Date</th>" +
                $"<th>Arrival Time</th>" +
                $"<th>PNR Capacity</th>" +
                $"<th>PNR Booked</th>" +
                $"<th>PNR Balance</th>";


            model.TableHeader = tableHeader;
            model.Railway = railway.Name;

            for (int i = 0; i < pnrs.Count; i++)
            {
                RailwaySummaryEntryModel entry = new();

                RailwayPNR currPnr = pnrs.ElementAt(i);

                RailwayTravel currTravel = travels
                    .FirstOrDefault(e => e.RailwayTravelId == currPnr.RailwayTravelId);

                List<string> pnrPackages = railwayPackages
                    .Where(e => e.RailwayTravelId == currPnr.RailwayTravelId)
                    .Select(e => e.Name)
                    .Distinct()
                    .ToList();

                List<string> pnrPackageCompanyCodes = railwayPackages
                    .Where(e => e.RailwayTravelId == currPnr.RailwayTravelId)
                    .Select(e => e.CompanyCode)
                    .Distinct()
                    .ToList();

                string packageString = string.Join(", ", pnrPackages);
                string companyString = string.Join(", ", pnrPackageCompanyCodes);


                int booked = pnrMembers.Where(e => e.RailwayPNRId == currPnr.RailwayPNRId).Count();
                int balance = currPnr.Capacity - booked;


                entry.RailwayTravelId = currPnr.RailwayTravelId;
                entry.SNo = i + 1;
                entry.PNRNumber = currPnr.Name;
                entry.PNRName = "";
                entry.Class = currPnr.Type.ToString().Substring(0, 3).ToUpper();
                entry.Company = companyString;
                entry.Package = packageString;
                entry.Railway = railway.Code;

                entry.FirstSectorTrainNumber = currTravel.OutboundRailwayNumber;

                entry.FirstSectorDestination = currTravel.Destination;
                entry.FirstSectorDepartureDate = DateTimeManager.GetDate(currTravel.OutboundDepartureDateTime);
                entry.FirstSectorDepartureTime = currTravel.OutboundDepartureDateTime.ToString("HH:mm");

                entry.FirstSectorArrivalDate = DateTimeManager.GetDate(currTravel.OutboundArrivalDateTime);
                entry.FirstSectorArrivalTime = currTravel.OutboundArrivalDateTime.ToString("HH:mm");

                entry.Capacity = currPnr.Capacity;
                entry.Booked = booked;
                entry.Balance = balance;


                model.Entries.Add(entry);
            }

            //return Ok(model);
            return View(model);
        }

    }
}

namespace TMG.Models.AirlineModels
{
    public record RailwaySummaryModel
    {
        public string Railway { get; set; }
        public string TableHeader { get; set; }
        public List<RailwaySummaryEntryModel> Entries { get; set; } = new();

    }

    public record RailwaySummaryEntryModel
    {
        public int RailwayTravelId { get; set; }
        public int SNo { get; set; }
        public string PNRNumber { get; set; }
        public string PNRName { get; set; }
        public string Class { get; set; }
        public string Company { get; set; }
        public string Package { get; set; }
        public string Railway { get; set; }
        public string FirstSectorTrainNumber { get; set; }
        public string FirstSectorDestination { get; set; }
        public string FirstSectorDepartureDate { get; set; }
        public string FirstSectorDepartureTime { get; set; }
        public string FirstSectorArrivalDate { get; set; }
        public string FirstSectorArrivalTime { get; set; }
        public int Capacity { get; set; }
        public int Booked { get; set; }
        public int Balance { get; set; }
    }

}

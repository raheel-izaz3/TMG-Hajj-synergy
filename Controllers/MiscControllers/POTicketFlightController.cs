using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models;

namespace TMG.Controllers.MiscControllers
{
    public class POTicketFlightController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public POTicketFlightController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.POTicketFlightDatas.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.POTicketFlightDatas
                .Include(a => a.Departure)
                .Include(a => a.Arrival)
                .Where(e => !e.Trashed)
                .ToList());

        }

        public IActionResult Create()
        {
            var airport = _dbContext.Airports.Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
            ViewBag.Airport = airport;
            ViewBag.Airline = _dbContext.AirlinesWithoutTrash;
            return View();
        }


        [HttpPost]
        public IActionResult Create(POTicketFlightData flight)
        {
            if (!ModelState.IsValid) return View();

            int hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            flight.HajjSeasonsId = hajjSeasonId;

            _dbContext.POTicketFlightDatas.Add(flight);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var airport = _dbContext.Airports.Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
            ViewBag.Airport = airport;
            ViewBag.Airline = _dbContext.AirlinesWithoutTrash;
            POTicketFlightData flight = _dbContext.POTicketFlightDatas.Find(id);

            return View(flight);
        }


        [HttpPost]
        public IActionResult Edit(POTicketFlightData flight)
        {
            if (!ModelState.IsValid) return View();

            flight.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(flight).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.POTicketFlightDatas
                .Include(a => a.Departure)
                .Include(a => a.Arrival)
                .Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            POTicketFlightData flight = _dbContext.POTicketFlightDatas.Find(id);
            if (flight == null) return RedirectToAction(nameof(Index));

            flight.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            POTicketFlightData flight = _dbContext.POTicketFlightDatas.Find(id);
            flight.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

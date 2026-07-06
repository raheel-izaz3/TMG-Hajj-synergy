using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models;

namespace TMG.Controllers.MiscControllers
{
    public class AirportController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public AirportController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Airports.Where(e => e.Trashed ).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.Airports
                .Where(e => !e.Trashed)
                .ToList());

        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Airport Airport)
        {
            if (!ModelState.IsValid) return View();

            int hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            Airport.HajjSeasonsId = hajjSeasonId;

            _dbContext.Airports.Add(Airport);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Airport Airports = _dbContext.Airports.Find(id);

            return View(Airports);
        }


        [HttpPost]
        public IActionResult Edit(Airport Airport)
        {
            if (!ModelState.IsValid) return View();

            Airport.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(Airport).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Airports.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Airport Airports = _dbContext.Airports.Find(id);
            if (Airports == null) return RedirectToAction(nameof(Index));

            Airports.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Airport Airports = _dbContext.Airports.Find(id);
            Airports.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

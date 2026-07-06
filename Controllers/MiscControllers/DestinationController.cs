using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class DestinationController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public DestinationController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Destinations.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.Destinations.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Destination fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Destinations.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Destination Fiqah = _dbContext.Destinations.Find(id);

            return View(Fiqah);
        }


        [HttpPost]
        public IActionResult Edit(Destination fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Destinations.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Destination fiqah = _dbContext.Destinations.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Destination fiqah = _dbContext.Destinations.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

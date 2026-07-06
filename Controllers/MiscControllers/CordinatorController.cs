using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class CordinatorController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CordinatorController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Cordinators.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.Cordinators.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Place = _dbContext.PlacesWithoutTrash
                .Where(e => e.PlaceId == 1 || e.PlaceId == 2);

            return View();
        }


        [HttpPost]
        public IActionResult Create(Cordinator fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Cordinators.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Place = _dbContext.PlacesWithoutTrash
                .Where(e => e.PlaceId == 1 || e.PlaceId == 2);

            Cordinator Fiqah = _dbContext.Cordinators.Find(id);

            return View(Fiqah);
        }


        [HttpPost]
        public IActionResult Edit(Cordinator fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Cordinators.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Cordinator fiqah = _dbContext.Cordinators.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Cordinator fiqah = _dbContext.Cordinators.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

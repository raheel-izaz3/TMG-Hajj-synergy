using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.LogisticModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccomodationControllers
{
    public class PlaceController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public PlaceController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Places.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashPlace(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreatePlace(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditPlace(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestorePlace(_dbContext, _http);

            return View(_dbContext.PlacesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Place place)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Places.Add(place);
            _dbContext.SaveChanges();

            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Place place = _dbContext.Places.Find(id);

            return View(place);
        }


        [HttpPost]
        public IActionResult Edit(Place place)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(place).State = EntityState.Modified;

            _dbContext.SaveChanges();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Places.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Place place = _dbContext.Places.Find(id);
            if (place == null) return RedirectToAction(nameof(Index));

            place.Trashed = true;
            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Place place = _dbContext.Places.Find(id);
            place.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

    }
}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccomodationsControllers
{
    public class AccomodationController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public AccomodationController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Accomodations.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashAccomodation(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateAccomodation(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditAccomodation(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreAccomodation(_dbContext, _http);

            return View(_dbContext.AccomodationsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Accomodation place)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Accomodations.Add(place);
            _dbContext.SaveChanges();


            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Accomodation place = _dbContext.Accomodations.Find(id);

            return View(place);
        }


        [HttpPost]
        public IActionResult Edit(Accomodation place)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(place).State = EntityState.Modified;

            _dbContext.SaveChanges();


            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Accomodations.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Accomodation place = _dbContext.Accomodations.Find(id);
            if (place == null) return RedirectToAction(nameof(Index));

            place.Trashed = true;
            _dbContext.SaveChanges();


            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Accomodation place = _dbContext.Accomodations.Find(id);
            place.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

    }
}

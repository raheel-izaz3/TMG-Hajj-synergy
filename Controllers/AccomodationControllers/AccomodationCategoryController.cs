using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccomodationCategoryCategoriesControllers
{
    public class AccomodationCategoryController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public AccomodationCategoryController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.AccomodationCategories.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashAccomodationCategory(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateAccomodationCategory(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditAccomodationCategory(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreAccomodationCategory(_dbContext, _http);

            return View(_dbContext.AccomodationCategoriesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(AccomodationCategory ac)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.AccomodationCategories.Add(ac);
            _dbContext.SaveChanges();


            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            AccomodationCategory ac = _dbContext.AccomodationCategories.Find(id);

            return View(ac);
        }


        [HttpPost]
        public IActionResult Edit(AccomodationCategory ac)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(ac).State = EntityState.Modified;

            _dbContext.SaveChanges();


            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.AccomodationCategories.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            AccomodationCategory ac = _dbContext.AccomodationCategories.Find(id);
            if (ac == null) return RedirectToAction(nameof(Index));

            ac.Trashed = true;
            _dbContext.SaveChanges();


            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            AccomodationCategory ac = _dbContext.AccomodationCategories.Find(id);
            ac.Trashed = false;
            _dbContext.SaveChanges();


            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

    }
}

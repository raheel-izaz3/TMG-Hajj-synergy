using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.LogisticModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccomodationControllers
{
    public class FoodPackageController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public FoodPackageController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.FoodPackages.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFoodPackage(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFoodPackage(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFoodPackage(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFoodPackage(_dbContext, _http);

            return View(_dbContext.FoodPackagesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(FoodPackage foodPackage)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.FoodPackages.Add(foodPackage);
            _dbContext.SaveChanges();

            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            FoodPackage FoodPackage = _dbContext.FoodPackages.Find(id);

            return View(FoodPackage);
        }


        [HttpPost]
        public IActionResult Edit(FoodPackage foodPackage)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(foodPackage).State = EntityState.Modified;

            _dbContext.SaveChanges();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.FoodPackages.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            FoodPackage foodPackage = _dbContext.FoodPackages.Find(id);
            if (foodPackage == null) return RedirectToAction(nameof(Index));

            foodPackage.Trashed = true;
            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            FoodPackage foodPackage = _dbContext.FoodPackages.Find(id);
            foodPackage.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

    }
}

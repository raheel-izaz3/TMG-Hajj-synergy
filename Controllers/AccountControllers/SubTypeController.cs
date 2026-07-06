using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubType = TMG.Models.AccountModels.SubType;

namespace TMG.Controllers.AccountControllers
{
    public class SubTypeController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public SubTypeController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.SubTypes.Where(e => e.Trashed).Count();

            var subTypes = _dbContext.SubTypesWithoutTrash.Include(e => e.Type).Include(e => e.Company);

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashSubType(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateSubType(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditSubType(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreSubType(_dbContext);

            return View(subTypes);
        }

        public IActionResult Create()
        {
            ViewBag.Types = _dbContext.Types;

            ViewBag.Company = _dbContext.CompaniesWithoutTrash;

            return View();
        }


        [HttpPost]
        public IActionResult Create(SubType type)
        {
            ViewBag.Types = _dbContext.Types;

            ViewBag.Company = _dbContext.CompaniesWithoutTrash;    

            if (!ModelState.IsValid) return View();

            _dbContext.SubTypes.Add(type);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Types = _dbContext.Types;

            ViewBag.Company = _dbContext.CompaniesWithoutTrash;

            SubType type = _dbContext.SubTypes.Find(id);

            return View(type);
        }


        [HttpPost]
        public IActionResult Edit(SubType type)
        {
            ViewBag.Types = _dbContext.Types;

            ViewBag.Company = _dbContext.CompaniesWithoutTrash;

            if (!ModelState.IsValid) return View();

            _dbContext.Entry(type).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.SubTypes.Include(e => e.Type).Include(e => e.Company).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            SubType type = _dbContext.SubTypes.Find(id);
            if (type == null) return RedirectToAction(nameof(Index));

            type.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            SubType type = _dbContext.SubTypes.Find(id);
            type.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetCategories(int id)
        {
            return Json(
                _dbContext.Categories.Where(e => e.SubTypeId == id)
                );
        }
    }
}

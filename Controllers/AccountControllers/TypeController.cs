using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Type = TMG.Models.AccountModels.Type;

namespace TMG.Controllers.AccountControllers
{
    public class TypeController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public TypeController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Types.Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashType(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateType(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditType(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreType(_dbContext);

            return View(_dbContext.Types);
        }

        //public IActionResult Create()
        //{
        //    ViewBag.Types = _dbContext.Types;

        //    return View();
        //}


        //[HttpPost]
        //public IActionResult Create(Type type)
        //{
        //    ViewBag.Types = _dbContext.Types;

        //    if (!ModelState.IsValid) return View();

        //    _dbContext.Types.Add(type);
        //    _dbContext.SaveChanges();

        //    return RedirectToAction(nameof(Index));
        //}

        //public IActionResult Edit(int id)
        //{
        //    ViewBag.Types = _dbContext.Types;

        //    Type type = _dbContext.Types.Find(id);

        //    return View(type);
        //}


        //[HttpPost]
        //public IActionResult Edit(Type type)
        //{
        //    ViewBag.Types = _dbContext.Types;

        //    if (!ModelState.IsValid) return View();

        //    _dbContext.Entry(type).State = EntityState.Modified;

        //    _dbContext.SaveChanges();

        //    return RedirectToAction(nameof(Index));
        //}

        //public IActionResult ViewTrash()
        //{
        //    return View(_dbContext.Types.Where(s => s.Trashed));
        //}

        //public IActionResult Trash(int id)
        //{
        //    Type type = _dbContext.Types.Find(id);
        //    if (type == null) return RedirectToAction(nameof(Index));

        //    type.Trashed = true;
        //    _dbContext.SaveChanges();

        //    return RedirectToAction(nameof(Index));
        //}

        //public IActionResult Restore(int id)
        //{
        //    Type type = _dbContext.Types.Find(id);
        //    type.Trashed = false;
        //    _dbContext.SaveChanges();
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpGet]
        public IActionResult GetSubTypes(int companyId, int typeId)
        {
            var subTypes = _dbContext.SubTypes
                .Where(e => e.CompanyId == companyId && e.TypeId == typeId)
                .Select(e => new { e.SubTypeId, e.Name })
                .ToList();

            return Json(subTypes);
        }


        [HttpGet]
        public IActionResult GetCategory(int subTypeId)
        {
            var category = _dbContext.CategoriesWithoutTrash
                .Where(e => e.SubTypeId == subTypeId)
                .Select(e => new { e.CategoryId, e.Name })
                .ToList();

            return Json(category);
        }

        [HttpGet]
        public IActionResult GetAccount(int categoryId)
        {
            var account = _dbContext.AccountsWithoutTrash
                .Where(e => e.CategoryId == categoryId)
                .Select(e => new { e.AccountId, e.Name })
                .ToList();

            return Json(account);
        }


    }
}

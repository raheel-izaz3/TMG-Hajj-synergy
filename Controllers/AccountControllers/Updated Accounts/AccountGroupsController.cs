using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public class AccountGroupsController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public AccountGroupsController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TrashCount = _dbContext.AccountGroups.Where(e => e.Trashed).Count();
            var AccGroups = await _dbContext.AccountGroups
                .Include(a => a.AccountTypes)
                .Where(a => a.Trashed == false)
                .ToListAsync();

            return View(AccGroups);
        }

        public async Task<IActionResult> Create()
        {
            var AccTypes = await _dbContext.AccountTypes.ToListAsync();
            ViewBag.Types = new SelectList(AccTypes, "AccountTypeId", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountGroups AccGroup)
        {
            var AccTypes = await _dbContext.AccountTypes.ToListAsync();
            ViewBag.Types = new SelectList(AccTypes, "AccountTypeId", "Name");

            if (!ModelState.IsValid) return View();

            _dbContext.AccountGroups.Add(AccGroup);
            await _dbContext.SaveChangesAsync();

            TempData["created"] = true;

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var AccGroups = await _dbContext.AccountGroups.FindAsync(id);

            var AccTypes = await _dbContext.AccountTypes.ToListAsync();
            ViewBag.Types = new SelectList(AccTypes, "AccountTypeId", "Name", AccGroups.AccountTypesId);

            return View(AccGroups);
        }


        //[HttpPost]
        //public IActionResult Edit(SubType type)
        //{
        //    ViewBag.Types = _dbContext.Types;

        //    if (!ModelState.IsValid) return View();

        //    _dbContext.Entry(type).State = EntityState.Modified;

        //    _dbContext.SaveChanges();

        //    return RedirectToAction(nameof(Index));
        //}

        //public IActionResult ViewTrash()
        //{
        //    return View(_dbContext.SubTypes.Include(e => e.Type).Include(e => e.Company).Where(s => s.Trashed));
        //}

        //public IActionResult Trash(int id)
        //{
        //    SubType type = _dbContext.SubTypes.Find(id);
        //    if (type == null) return RedirectToAction(nameof(Index));

        //    type.Trashed = true;
        //    _dbContext.SaveChanges();

        //    return RedirectToAction(nameof(Index));
        //}

        //public IActionResult Restore(int id)
        //{
        //    SubType type = _dbContext.SubTypes.Find(id);
        //    type.Trashed = false;
        //    _dbContext.SaveChanges();
        //    return RedirectToAction(nameof(Index));
        //}

        //public JsonResult GetCategories(int id)
        //{
        //    return Json(
        //        _dbContext.Categories.Where(e => e.SubTypeId == id)
        //        );
        //}
    }
}

using TMG.Data;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccountControllers
{
    public class AccountController: Controller
    {
        private readonly TMGDbContext _dbContext;

        public AccountController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Accounts.Where(e => e.Trashed).Count();
            
            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashAccount(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateAccount(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditAccount(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreAccount(_dbContext);

            return View(_dbContext.Accounts
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .ThenInclude(e => e.Company)
                
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .ThenInclude(e => e.Type)
                .Where(e=>!e.Trashed)
                );
        }

        public IActionResult Create()
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;

            return View();
        }

        [HttpPost]
        public IActionResult Create(Account account)
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

            if (!ModelState.IsValid) return View();

            _dbContext.Accounts.Add(account);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

            Account account = _dbContext.Accounts
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .FirstOrDefault(e => e.AccountId == id);

            ViewBag.SubTypeId = account.Category.SubTypeId;
            ViewBag.TypeId = account.Category.SubType.TypeId;
            ViewBag.CompanyId = account.Category.SubType.CompanyId;

            return View(account);
        }


        [HttpPost]
        public IActionResult Edit(Account account)
        {
            Account _account = _dbContext.Accounts.Include(e => e.Category).ThenInclude(e => e.SubType).FirstOrDefault(e => e.AccountId == account.AccountId);

            ViewBag.Types = _dbContext.Types;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

            ViewBag.SubTypeId = _account.Category.SubTypeId;
            ViewBag.TypeId = _account.Category.SubType.TypeId;
            //ViewBag.CompanyId = account.Category.SubType.CompanyId;

            if (!ModelState.IsValid) return View();

            _account.Name = account.Name;
            _account.CategoryId = account.CategoryId;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Accounts.Include(e => e.Category).ThenInclude(e => e.SubType).ThenInclude(e => e.Type).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Account account = _dbContext.Accounts.Find(id);
            if (account == null) return RedirectToAction(nameof(Index));

            account.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Account account = _dbContext.Accounts.Find(id);
            account.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCategories(int id)
        {
            var categories = _dbContext.CategoriesWithoutTrash
                .Where(c => c.SubTypeId == id)
                .Select(c => new { c.CategoryId, c.Name })
                .ToList();

            return Json(categories);
        }
    }
}

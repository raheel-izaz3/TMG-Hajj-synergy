using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.NewChartOfAccounts.FiscalYear;
using TMG.Models.NewChartOfAccounts.NewAccount;

namespace TMG.Controllers.NewChartOfAccounts.NewAccounts
{
    public class NewAccountGroupsController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public NewAccountGroupsController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.Trashed = _dbContext.NewAccountGroups.Where(e => e.Trashed).Count();

            return View(_dbContext.NewAccountGroups.Include(c => c.Company).Where(e => !e.Trashed));
        }

        public IActionResult Create()
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash.ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(NewAccountGroup newGroup)
        {
            if (!ModelState.IsValid) return View(newGroup);

            // Default hamesha open rahe

            _dbContext.NewAccountGroups.Add(newGroup);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash.ToList();

            NewAccountGroup newGroup = _dbContext.NewAccountGroups.Include(c => c.Company).FirstOrDefault(f => f.NewAccountGroupId == id);

            return View(newGroup);
        }


        [HttpPost]
        public IActionResult Edit(NewAccountGroup newGroup)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(newGroup).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.NewAccountGroups.Include(c => c.Company).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            NewAccountGroup newGroup = _dbContext.NewAccountGroups.Find(id);
            if (newGroup == null) return RedirectToAction(nameof(Index));

            newGroup.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            NewAccountGroup newGroup = _dbContext.NewAccountGroups.Find(id);
            newGroup.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

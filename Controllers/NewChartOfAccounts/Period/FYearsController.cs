using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.MiscModels;
using TMG.Models.NewChartOfAccounts.Currencies;
using TMG.Models.NewChartOfAccounts.FiscalYear;

namespace TMG.Controllers.NewChartOfAccounts.Period
{
    public class FYearsController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
         
        public FYearsController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.FiscalYears.Where(e => e.Trashed).Count();

            return View(_dbContext.FiscalYears.Include(c => c.Company).Where(e => !e.Trashed));
        }

        public IActionResult Create()
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash.ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(FiscalYears fyears)
        {
            if (!ModelState.IsValid) return View(fyears);

            // Default hamesha open rahe
            fyears.IsClosed = false;

            _dbContext.FiscalYears.Add(fyears);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash.ToList();

            FiscalYears fyears = _dbContext.FiscalYears.Include(c => c.Company).FirstOrDefault(f => f.FiscalYearsId == id);

            return View(fyears);
        }


        [HttpPost]
        public IActionResult Edit(FiscalYears fYeays)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fYeays).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.FiscalYears.Include(c => c.Company).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            FiscalYears fyears = _dbContext.FiscalYears.Find(id);
            if (fyears == null) return RedirectToAction(nameof(Index));

            fyears.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            FiscalYears fyears = _dbContext.FiscalYears.Find(id);
            fyears.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

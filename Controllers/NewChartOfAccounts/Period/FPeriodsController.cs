using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.MiscModels;
using TMG.Models.NewChartOfAccounts.Currencies;
using TMG.Models.NewChartOfAccounts.FiscalYear;

namespace TMG.Controllers.NewChartOfAccounts.Period
{
    public class FPeriodsController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
         
        public FPeriodsController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.FiscalPeriods.Where(e => e.Trashed).Count();

            return View(_dbContext.FiscalPeriods.Include(c => c.FiscalYears).Where(e => !e.Trashed));
        }

        public IActionResult Create()
        {
            ViewBag.FiscalYear = _dbContext.FiscalYears.Include(ct => ct.Company).ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(FiscalPeriods fperiod)
        {
            if (!ModelState.IsValid) return View(fperiod);

            // Default hamesha open rahe
            fperiod.IsClosed = false;

            _dbContext.FiscalPeriods.Add(fperiod);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            ViewBag.FiscalYear = _dbContext.FiscalYears.Include(ct => ct.Company).ToList();

            FiscalPeriods fPeriods = _dbContext.FiscalPeriods.Include(c => c.FiscalYears).FirstOrDefault(f => f.FiscalPeriodsId == id);

            return View(fPeriods);
        }


        [HttpPost]
        public IActionResult Edit(FiscalPeriods fPeriods)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fPeriods).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.FiscalPeriods.Include(c => c.FiscalYears).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            FiscalPeriods fyears = _dbContext.FiscalPeriods.Find(id);
            if (fyears == null) return RedirectToAction(nameof(Index));

            fyears.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            FiscalPeriods fyears = _dbContext.FiscalPeriods.Find(id);
            fyears.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

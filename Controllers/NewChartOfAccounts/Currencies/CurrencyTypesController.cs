using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.MiscModels;
using TMG.Models.NewChartOfAccounts.Currencies;

namespace TMG.Controllers.NewChartOfAccounts.Currencies
{
    public class CurrencyTypesController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
         
        public CurrencyTypesController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CurrencyTypes.Where(e => e.Trashed).Count();

            return View(_dbContext.CurrencyTypes.Where(e => !e.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(CurrencyType currency)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.CurrencyTypes.Add(currency);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CurrencyType currency = _dbContext.CurrencyTypes.Find(id);

            return View(currency);
        }


        [HttpPost]
        public IActionResult Edit(CurrencyType currency)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(currency).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CurrencyTypes.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            CurrencyType currency = _dbContext.CurrencyTypes.Find(id);
            if (currency == null) return RedirectToAction(nameof(Index));

            currency.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            CurrencyType currency = _dbContext.CurrencyTypes.Find(id);
            currency.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

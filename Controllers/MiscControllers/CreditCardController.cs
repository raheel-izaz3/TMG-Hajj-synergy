using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
//using TMG.Migrations;
using TMG.Models.MiscModels;

namespace TMG.Controllers.MiscControllers
{
    public class CreditCardController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;
        private readonly IHttpContextAccessor _http;

        public CreditCardController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CreditCards.Where(e => e.Trashed).Count();
            return View(_dbContext.CreditCards.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreditCard fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.CreditCards.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CreditCard Fiqah = _dbContext.CreditCards.Find(id);
            return View(Fiqah);
        }
        [HttpPost]
        public IActionResult Edit(CreditCard fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult ViewTrash()
        {
            return View(_dbContext.CreditCards.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            CreditCard fiqah = _dbContext.CreditCards.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            CreditCard fiqah = _dbContext.CreditCards.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.MiscModels;

namespace TMG.Controllers.MiscControllers
{
    public class GiveAwayController : Controller
    {

        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;
        private readonly IHttpContextAccessor _http;

        public GiveAwayController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.GiveAways.Where(e => e.Trashed).Count();
            return View(_dbContext.GiveAways.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(GiveAway fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.GiveAways.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            GiveAway Fiqah = _dbContext.GiveAways.Find(id);
            return View(Fiqah);
        }

        [HttpPost]
        public IActionResult Edit(GiveAway fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult ViewTrash()
        {
            return View(_dbContext.GiveAways.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            GiveAway fiqah = _dbContext.GiveAways.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            GiveAway fiqah = _dbContext.GiveAways.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public class CityTaxController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CityTaxController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TrashCount = await _dbContext.CityTaxes.Where(e => e.Trashed).CountAsync();

            var CT = await _dbContext.CityTaxes.Where(c => !c.Trashed).ToListAsync();

            return View(CT);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CityTax CT)
        {
            if (!ModelState.IsValid)
            {
                return View(CT);
            }

            _dbContext.CityTaxes.Add(CT);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {

            var CT = await _dbContext.CityTaxes.FindAsync(id);

            if (CT == null)
            {
                return NotFound();
            }

            return View(CT);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CityTax CT)
        {
            if (!ModelState.IsValid)
            {
                return View(CT);
            }
            
            _dbContext.Entry(CT).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewTrash()
        {
            var CT = await _dbContext.CityTaxes.Where(e => e.Trashed).ToListAsync();

            return View(CT);
        }

        public IActionResult Trash(int id)
        {
            var CT = _dbContext.CityTaxes.Find(id);
            if (CT == null) return RedirectToAction(nameof(Index));

            CT.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            var CT = _dbContext.CityTaxes.FirstOrDefault(e => e.CityTaxId == id);
            CT.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

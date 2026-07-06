using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public class TaxController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public TaxController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TrashCount = await _dbContext.Taxes.Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).CountAsync();

            var Taxes = await _dbContext.Taxes
                .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            return View(Taxes);
        }

        public async Task<IActionResult> Create()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tax tax)
        {
            if (!ModelState.IsValid)
            {

                return View(tax);
            }

            tax.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.Taxes.Add(tax);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {

            var Taxes = await _dbContext.Taxes.FindAsync(id);

            if (Taxes == null)
            {
                return NotFound();
            }

            return View(Taxes);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Tax taxes)
        {
            if (!ModelState.IsValid)
            {
                return View(taxes);
            }

            var Tax = await _dbContext.Taxes.FindAsync(taxes.TaxId);
            
            if (Tax == null)
            {
                return NotFound();
            }

            Tax.TaxName = taxes.TaxName;
            Tax.IsClaimable = taxes.IsClaimable;
            Tax.TaxTypes = taxes.TaxTypes;
            Tax.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(Tax).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewTrash()
        {
            var Taxes = await _dbContext.Taxes
                .Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            return View(Taxes);
        }

        public IActionResult Trash(int id)
        {
            var tax = _dbContext.Taxes.Find(id);
            if (tax == null) return RedirectToAction(nameof(Index));

            tax.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            var tax = _dbContext.Taxes.FirstOrDefault(e => e.TaxId == id);
            tax.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

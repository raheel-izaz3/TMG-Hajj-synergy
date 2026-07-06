using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public class AccountsController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public AccountsController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TrashCount = _dbContext.Account.Where(e => e.Trashed).Count();

            var Acc = await _dbContext.Account
                .Include(a => a.AccountGroups)
                .Where(a => a.Trashed == false)
                .ToListAsync();

            return View(Acc);
        }

        public async Task<IActionResult> Create()
        {
            var AccGroups = await _dbContext.AccountGroups
                .Where(g => g.Trashed == false)
                .ToListAsync();

            ViewBag.Acc = new SelectList(AccGroups, "AccountGroupId", "Name");

            var Companies = await _dbContext.Companies
                .Where(g => g.Trashed == false)
                .ToListAsync();
            
            ViewBag.Company = new SelectList(Companies, "CompanyId", "CompanyName");

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Accounts Acc)
        {
            var AccGroups = await _dbContext.AccountGroups.Where(g => g.Trashed == false).ToListAsync();
            ViewBag.Acc = new SelectList(AccGroups, "AccountGroupId", "Name");

            var Companies = await _dbContext.Companies.Where(g => g.Trashed == false).ToListAsync();
            ViewBag.Company = new SelectList(Companies, "CompanyId", "CompanyName");

            if (!ModelState.IsValid) return View();

            _dbContext.Account.Add(Acc);
            await _dbContext.SaveChangesAsync();

            TempData["created"] = true;

            return RedirectToAction(nameof(Index));
        }
    }
}

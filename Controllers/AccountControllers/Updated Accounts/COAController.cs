using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public class COAController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public COAController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Company = await _dbContext.CompaniesWithoutTrash
                .Where(e => e.Trashed == false)
                .ToListAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCOA(int companyId)
        {
            var accTypes = await _dbContext.AccountTypes.ToListAsync();

            var accGroups = await _dbContext.AccountGroups
                .Where(g => g.Trashed == false)
                .Include(g => g.AccountTypes)
                .ToListAsync();

            var accSubGroups = await _dbContext.CategoriesWithoutTrash
                .Include(e => e.AccountGroups)
                .ToListAsync();

            var accounts = await _dbContext.Account
                .Include(a => a.AccountGroups)
                .Where(c =>
                    !c.Trashed
                    && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    && (c.IsVendorAccount == true ? true : c.CompanyId == companyId)
                )
                .ToListAsync();

            return Ok(new
            {
                AccTypes = accTypes,
                AccGroups = accGroups,
                AccSubGroups = accSubGroups,
                Accounts = accounts
            });
        }

    }
}

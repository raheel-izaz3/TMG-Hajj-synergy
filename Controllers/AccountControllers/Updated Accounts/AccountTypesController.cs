using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public class AccountTypesController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public AccountTypesController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var AccTypes = await _dbContext.AccountTypes.ToListAsync();

            return View(AccTypes);
        }
    }
}

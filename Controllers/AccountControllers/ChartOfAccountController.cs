using TMG.Data;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccountControllers
{
    public class ChartOfAccountController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public ChartOfAccountController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.Company = _dbContext.CompaniesWithoutTrash;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Categories = _dbContext.CategoriesWithoutTrash;
            ViewBag.Accounts = _dbContext.AccountsWithoutTrash;

            return View();
        }

        [HttpGet]
        public IActionResult GetCOA(int companyId)
        {
            //var compnay = _dbContext.Companies.Find(companyId);
            //var coa = _dbContext.Accounts.Include(e => e.Category).ThenInclude(e => e.SubType).Where(e => e.Category.SubType.CompanyId == companyId);
            var types = _dbContext.Types;
            var subTypes = _dbContext.SubTypesWithoutTrash.Where(e => e.CompanyId == companyId);
            var categories = _dbContext.CategoriesWithoutTrash
                .Include(e => e.SubType)
                .Where(e => e.SubType.CompanyId == companyId);
            var accounts = _dbContext.AccountsWithoutTrash
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .Where(e => e.Category.SubType.CompanyId == companyId);

            return Ok(new
            {
                types, subTypes, categories, accounts
            });          
        }

    }
}

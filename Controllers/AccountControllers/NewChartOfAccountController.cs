using Microsoft.AspNetCore.Mvc;
using TMG.Data;

namespace TMG.Controllers.AccountControllers;
public class NewChartOfAccountController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;

    public NewChartOfAccountController(TMGDbContext dbContext, IHttpContextAccessor http)
    {
        _dbContext = dbContext;
        _http = http;
    }


    public IActionResult Index()
    {
        //ViewBag.SubHeads = _dbContext.AccountSubHeads;
        //ViewBag.Accounts = _dbContext.MainAccounts;
        ViewBag.Types = _dbContext.Types;

        return View();
    }
}

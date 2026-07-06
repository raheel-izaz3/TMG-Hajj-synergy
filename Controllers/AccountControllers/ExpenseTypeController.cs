using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers;
public class ExpenseTypeController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;

    public ExpenseTypeController(TMGDbContext dbContext, IHttpContextAccessor http)
    {
        _dbContext = dbContext;
        _http = http;
    }

    public IActionResult Index()
    {
        ViewBag.TrashCount = _dbContext.ExpenseTypes
            .IgnoreQueryFilters()
            .Where(e => e.Trashed)
            .Count();

        //ViewBag.TrashAllowed = PrivilegeManager.CanTrashType(_dbContext);
        //ViewBag.CreateAllowed = PrivilegeManager.CanCreateType(_dbContext);
        //ViewBag.EditAllowed = PrivilegeManager.CanEditType(_dbContext);
        //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreType(_dbContext);

        return View(_dbContext.ExpenseTypes.Where(e => !e.Trashed));
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(ExpenseType type)
    {
        if (!ModelState.IsValid) return View();

        _dbContext.ExpenseTypes.Add(type);

        var AccGroupExp = _dbContext.AccountTypes.Where(g => g.Name == "Expense").FirstOrDefault();

        if (AccGroupExp != null)
        {
            var AccountExp = new AccountGroups()
            {
                AccountTypesId = AccGroupExp.AccountTypeId,
                Name = $"{type.TypeName}"
            };

            _dbContext.AccountGroups.Add(AccountExp);

            _dbContext.SaveChanges();

            type.AccountGroupId = AccountExp.AccountGroupId;
        }

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        ViewBag.Types = _dbContext.ExpenseTypes;

        ExpenseType type = _dbContext.ExpenseTypes.Find(id);

        return View(type);
    }

    [HttpPost]
    public IActionResult Edit(ExpenseType type)
    {
        ViewBag.Types = _dbContext.ExpenseTypes;

        if (!ModelState.IsValid) return View();

        if (type.AccountGroupId.HasValue)
        {
            var AccGroup = _dbContext.AccountGroups.Where(g => g.AccountGroupId == type.AccountGroupId).FirstOrDefault();
            if (AccGroup != null)
            {
                AccGroup.Name = type.TypeName;

                _dbContext.Entry(AccGroup).State = EntityState.Modified;
            }
        }

        _dbContext.Entry(type).State = EntityState.Modified;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult ViewTrash()
    {
        return View(_dbContext.ExpenseTypes.IgnoreQueryFilters().Where(s => s.Trashed));
    }

    public IActionResult Trash(int id)
    {
        ExpenseType type = _dbContext.ExpenseTypes.Find(id);
        if (type == null) return RedirectToAction(nameof(Index));

        type.Trashed = true;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Restore(int id)
    {
        ExpenseType type = _dbContext.ExpenseTypes.IgnoreQueryFilters().FirstOrDefault(e => e.ExpenseTypeId == id);
        type.Trashed = false;
        _dbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

   
}

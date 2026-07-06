using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers;
public class VendorTypeController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;

    public VendorTypeController(TMGDbContext dbContext, IHttpContextAccessor http)
    {
        _dbContext = dbContext;
        _http = http;
    }

    public IActionResult Index()
    {
        ViewBag.TrashCount = _dbContext.VendorTypes.Where(e => e.Trashed).Count();

        //ViewBag.TrashAllowed = PrivilegeManager.CanTrashType(_dbContext);
        //ViewBag.CreateAllowed = PrivilegeManager.CanCreateType(_dbContext);
        //ViewBag.EditAllowed = PrivilegeManager.CanEditType(_dbContext);
        //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreType(_dbContext);

        return View(_dbContext.VendorTypes.Where(e => !e.Trashed));
    }

    public IActionResult Create()
    {
        ViewBag.Types = _dbContext.VendorTypes;

        return View();
    }


    [HttpPost]
    public IActionResult Create(VendorType type)
    {
        
        if (!ModelState.IsValid)
        {
            ViewBag.Types = _dbContext.VendorTypes;

            return View();
        }

        _dbContext.VendorTypes.Add(type);

        var AccGroupVend = _dbContext.AccountTypes.Where(g => g.Name == "Liability").FirstOrDefault();

        if (AccGroupVend != null)
        {
            var AccountVend = new AccountGroups()
            {
                AccountTypesId = AccGroupVend.AccountTypeId,
                Name = $"{type.TypeName} - Payable"
            };

            _dbContext.AccountGroups.Add(AccountVend);
            _dbContext.SaveChanges();

            type.AccountGroupId = AccountVend.AccountGroupId;
        }

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        ViewBag.Types = _dbContext.VendorTypes;

        var type = _dbContext.VendorTypes.Find(id);

        return View(type);
    }


    [HttpPost]
    public IActionResult Edit(VendorType type)
    {
        ViewBag.Types = _dbContext.VendorTypes;

        if (!ModelState.IsValid) return View();

        _dbContext.Entry(type).State = EntityState.Modified;

        var AccGroup = _dbContext.AccountGroups.Where(g => g.AccountGroupId == type.AccountGroupId).FirstOrDefault();

        if (AccGroup != null)
        {
            AccGroup.Name = $"{type.TypeName} - Payable";
            _dbContext.Entry(AccGroup).State = EntityState.Modified;
        }

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }


    public IActionResult ViewTrash()
    {
        return View(_dbContext.VendorTypes.Where(s => s.Trashed));
    }

    public IActionResult Trash(int id)
    {
        VendorType type = _dbContext.VendorTypes.Find(id);
        if (type == null) return RedirectToAction(nameof(Index));

        type.Trashed = true;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Restore(int id)
    {
        VendorType type = _dbContext.VendorTypes.Find(id);
        type.Trashed = false;
        _dbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public JsonResult GetVendors(int id)
    {
        return Json(_dbContext.Vendors
            .Where(e => !e.Trashed)
            .Where(e => e.VendorTypeId == id));
    }
}

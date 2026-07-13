using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;

namespace TMG.Controllers.AccountControllers;
public class ExpenseController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;
    private readonly IWebHostEnvironment _host;

    public ExpenseController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment host)
    {
        _dbContext = dbContext;
        _http = http;
        _host = host;
    }

    public IActionResult Index()
    {
        ViewBag.TrashCount = _dbContext.Expenses
            .IgnoreQueryFilters()
            .Where(e => e.Trashed)
            .Count();

        //ViewBag.TrashAllowed = PrivilegeManager.CanTrashType(_dbContext);
        //ViewBag.CreateAllowed = PrivilegeManager.CanCreateType(_dbContext);
        //ViewBag.EditAllowed = PrivilegeManager.CanEditType(_dbContext);
        //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreType(_dbContext);

        ViewBag.TotalExpense = ExpenseManager.GetTotalExpense(_dbContext);

        return View(_dbContext.Expenses
            .Include(e => e.ExpenseType)
            .Include(e => e.CompanyBankAccount)
            .ThenInclude(e => e.Company));
    }

    public IActionResult Create()
    {
        ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

        ViewBag.ExpenseTypes = _dbContext.ExpenseTypes;


        return View();
    }


    [HttpPost]
    public IActionResult Create(Expense expense)
    {
        ViewBag.Companies = _dbContext.CompaniesWithoutTrash
            .Select(e => new
            {
                e.CompanyId, e.CompanyName
            });

        ViewBag.ExpenseTypes = _dbContext.ExpenseTypes;


        if (!ModelState.IsValid) return View();


        Expense exp = new Expense() { 
            Date = expense.Date,
            ExpenseTypeId = expense.ExpenseTypeId,
            HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
            Description = expense.Description,
            Amount = expense.Amount,
            CompanyBankAccountId = expense.CompanyBankAccountId,
            AdvanceTax = expense.AdvanceTax,
            MiscTax = expense.MiscTax,
            SalesTax = expense.SalesTax,
            WithholdingTax = expense.WithholdingTax,    
            
        };

        if(expense.ImageFile is not null)
        {
            string path = Path.Combine(_host.WebRootPath, "Images", "ExpenseImages", expense.ImageFile.FileName);
            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                expense.ImageFile.CopyTo(fs);
            }
            exp.Image = "Images/ExpenseImages/" + expense.ImageFile.FileName;
        }

        _dbContext.Expenses.Add(exp);
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        ViewBag.Types = _dbContext.Expenses;

        Expense expense = _dbContext.Expenses.Find(id);

        return View(expense);
    }


    [HttpPost]
    public IActionResult Edit(Expense expense)
    {
        ViewBag.Types = _dbContext.Expenses;

        if (!ModelState.IsValid) return View();

        _dbContext.Entry(expense).State = EntityState.Modified;

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult ViewTrash()
    {
        return View(_dbContext.Expenses.IgnoreQueryFilters().Where(s => s.Trashed));
    }

    public IActionResult Trash(int id)
    {
        Expense expense = _dbContext.Expenses.Find(id);
        if (expense == null) return RedirectToAction(nameof(Index));

        expense.Trashed = true;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Restore(int id)
    {
        Expense expense = _dbContext.Expenses.IgnoreQueryFilters().FirstOrDefault(e => e.ExpenseId == id);
        expense.Trashed = false;
        _dbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Print(int id)
    {

        Expense exp = _dbContext.Expenses
                        .Include(e => e.ExpenseType)
                        .Include(e => e.CompanyBankAccount)
                        .ThenInclude(e => e.Company)
                        .FirstOrDefault(e => e.ExpenseId == id);

        CompanyAddresses ca = _dbContext.CompanyAddresses
            .FirstOrDefault(e => e.CompanyId == exp.CompanyBankAccount.CompanyId && e.Preferred);

        CompanyEmails ce = _dbContext.CompanyEmails
            .FirstOrDefault(e => e.CompanyId == exp.CompanyBankAccount.CompanyId && e.Preferred);

        CompanyContacts cc = _dbContext.CompanyContacts
            .FirstOrDefault(e => e.CompanyId == exp.CompanyBankAccount.CompanyId && e.Preferred);

        ViewBag.CompanyAddress = ca.Address;
        ViewBag.City = $"{ca.City}, {ca.Country}";
        ViewBag.Email = ce.CompanyEmail;
        ViewBag.Contact = cc.CompanyContact;

        return View(exp);
    }

   
}

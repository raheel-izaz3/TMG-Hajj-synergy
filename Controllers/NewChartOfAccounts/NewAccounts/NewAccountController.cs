using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Controllers.POControllers;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.NewChartOfAccounts.NewAccount;

namespace TMG.Controllers.NewChartOfAccounts.NewAccounts
{
    public class NewAccountController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public NewAccountController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.NewAccounts.Where(e => e.Trashed && e.ParentAccountId == null).Count();

            var accounts = _dbContext.NewAccounts
                 .Include(c => c.Company)
                 .Include(c => c.NewAccountGroup)
                 .Include(c => c.ParentAccount)
                 .Where(e => !e.Trashed && e.ParentAccountId == null);

            return View(accounts);
        }

        public IActionResult ViewSubAccount(int id)
        {
            ViewBag.TrashCount = _dbContext.NewAccounts.Where(e => e.Trashed && e.ParentAccountId == id).Count();

            var accounts = _dbContext.NewAccounts
                 .Include(c => c.Company)
                 .Include(c => c.NewAccountGroup)
                 .Include(c => c.ParentAccount)
                 .Where(e => !e.Trashed && e.ParentAccountId == id);

            ViewBag.FirstParentAccountId = id;

            return View(accounts);
        }

        public IActionResult ViewSubChildAccounts(int id)
        {
            ViewBag.TrashCount = _dbContext.NewAccounts.Where(e => e.Trashed && e.ParentAccountId == id).Count();

            var accounts = _dbContext.NewAccounts
                 .Include(c => c.Company)
                 .Include(c => c.NewAccountGroup)
                 .Include(c => c.ParentAccount)
                 .Where(e => !e.Trashed && e.ParentAccountId == id);

            ViewBag.FirstParentAccountId = id;

            return View(accounts);
        }

        public IActionResult CreateSubAccount(int id)
        {
            ViewBag.OpeningType = Enum.GetValues(typeof(OpeningType))
                          .Cast<OpeningType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash.ToList();
            ViewBag.NewAccounts = _dbContext.NewAccounts.Where(na => !na.Trashed && na.ParentAccountId == id).ToList();
            ViewBag.NewAccountGroups = _dbContext.NewAccountGroups.Where(na => !na.Trashed).ToList();

            return View();
        }

        public IActionResult Create()
        {
            ViewBag.OpeningType = Enum.GetValues(typeof(OpeningType))
                          .Cast<OpeningType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash.ToList();
            ViewBag.NewAccounts = _dbContext.NewAccounts.Where(na => !na.Trashed && na.ParentAccountId == null).ToList();
            ViewBag.NewAccountGroups = _dbContext.NewAccountGroups.Where(na => !na.Trashed).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(NewAccount model, float OpeningAmount)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.OpeningType == OpeningType.Debit)
            {
                model.OpeningDebit = OpeningAmount;
                model.OpeningCredit = 0;
            }
            else
            {
                model.OpeningDebit = 0;
                model.OpeningCredit = OpeningAmount;
            }

            _dbContext.NewAccounts.Add(model);
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            ViewBag.OpeningType = Enum.GetValues(typeof(OpeningType))
                          .Cast<OpeningType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash.ToList();
            ViewBag.NewAccounts = _dbContext.NewAccounts.Where(na => !na.Trashed).ToList();
            ViewBag.NewAccountGroups = _dbContext.NewAccountGroups.Where(na => !na.Trashed).ToList();

            NewAccount newAccount = _dbContext.NewAccounts
                .Include(c => c.Company)
                .Include(c => c.NewAccountGroup)
                .FirstOrDefault(f => f.NewAccountId == id);

            // Opening amount decide
            float openingAmount = newAccount.OpeningType == OpeningType.Debit
            ? newAccount.OpeningDebit
                : newAccount.OpeningCredit;

            ViewBag.OpeningAmount = openingAmount;

            return View(newAccount);
        }

        [HttpPost]
        public IActionResult Edit(NewAccount newAccount, float OpeningAmount)
        {
            //if (!ModelState.IsValid) return View();

            if (newAccount.OpeningType == OpeningType.Debit)
            {
                newAccount.OpeningDebit = OpeningAmount;
                newAccount.OpeningCredit = 0;
            }
            else
            {
                newAccount.OpeningDebit = 0;
                newAccount.OpeningCredit = OpeningAmount;
            }

            _dbContext.Entry(newAccount).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            var accounts = _dbContext.NewAccounts
                 .Include(c => c.Company)
                 .Include(c => c.NewAccountGroup)
                 .Where(e => e.Trashed && e.ParentAccountId == null);

            return View(accounts);
        } 
        
        public IActionResult ViewSubAccountTrash(int id)
        {
            var accounts = _dbContext.NewAccounts
                 .Include(c => c.Company)
                 .Include(c => c.NewAccountGroup)
                 .Where(e => e.Trashed && e.ParentAccountId == id);

            return View(accounts);
        }
        
        public IActionResult ViewSubChildAccountsTrash(int id)
        {
            var accounts = _dbContext.NewAccounts
                 .Include(c => c.Company)
                 .Include(c => c.NewAccountGroup)
                 .Where(e => e.Trashed && e.ParentAccountId == id);

            return View(accounts);
        }

        public IActionResult SingleTrash(int id)
        {
            NewAccount newAccount = _dbContext.NewAccounts.Find(id);
            if (newAccount == null) return RedirectToAction(nameof(Index));

            newAccount.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(ViewSubChildAccounts));
        }

        public IActionResult SingleRestore(int id)
        {
            NewAccount newAccount = _dbContext.NewAccounts.Find(id);
            newAccount.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(ViewSubChildAccounts));
        }

        public IActionResult Trash(int id)
        {
            var account = _dbContext.NewAccounts.Find(id);
            if (account == null)
                return RedirectToAction(nameof(Index));

            // Recursive method call
            TrashAccountWithChildren(account);

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void TrashAccountWithChildren(NewAccount account)
        {
            // Mark this account as trashed
            account.Trashed = true;

            // Find all children of this account
            var children = _dbContext.NewAccounts
                .Where(a => a.ParentAccountId == account.NewAccountId && !a.Trashed)
                .ToList();

            // Recursive call for each child
            foreach (var child in children)
            {
                TrashAccountWithChildren(child);
            }
        }
        
        public IActionResult Restore(int id)
        {
            var account = _dbContext.NewAccounts.Find(id);
            if (account == null)
                return RedirectToAction(nameof(Index));

            // Recursive method call
            RestoreAccountWithChildren(account);

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void RestoreAccountWithChildren(NewAccount account)
        {
            // Mark this account as restored
            account.Trashed = false;

            // Find all children of this account
            var children = _dbContext.NewAccounts
                .Where(a => a.ParentAccountId == account.NewAccountId && a.Trashed)
                .ToList();

            // Recursive call for each child
            foreach (var child in children)
            {
                RestoreAccountWithChildren(child);
            }
        }

    }
}

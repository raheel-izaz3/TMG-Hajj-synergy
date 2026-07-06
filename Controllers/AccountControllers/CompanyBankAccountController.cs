using TMG.Data;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using TMG.Models.Updated_Accounts;
using TMG.ManagerClasses;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;

namespace TMG.Controllers.AccountControllers
{
    public class CompanyBankAccountController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyBankAccountController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CompanyBankAccounts.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashBankBranch(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateBankBranch(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditBankBranch(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreBankBranch(_dbContext);

            return View(_dbContext.CompanyBankAccountsWithoutTrash
                .Include(e => e.Bank)
            .Include(e => e.Company)
                );
        }

        public IActionResult Create()
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Banks = _dbContext.BanksWithoutTrash;

            return View();
        }


        [HttpPost]
        public IActionResult Create(CompanyBankAccount accountBankAccount)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Banks = _dbContext.BanksWithoutTrash;

            if (!ModelState.IsValid) return View();

            var category = _dbContext.Categories
                .Include(e => e.SubType)
                .Select(e => new {
                    e.SubType.CompanyId, e.CompanyBank, e.CategoryId
                })
                .FirstOrDefault(e => e.CompanyBank && e.CompanyId == accountBankAccount.CompanyId);


            int? AccAssetId = null;
            int? AccExpId = null;

            var AccGroupRev = _dbContext.AccountGroups.Where(g => g.Name == COAManager.CurrentAssetsString).FirstOrDefault();
            var AccGroupBankExp = _dbContext.AccountGroups.Where(g => g.Name == COAManager.BankExpenseString).FirstOrDefault();
            var BankName = _dbContext.BanksWithoutTrash.Where(e => e.BankId == accountBankAccount.BankId).FirstOrDefault();

            if (AccGroupRev != null)
            {
                var AccountRev = new Accounts()
                {
                    AccountGroupsId = AccGroupRev.AccountGroupId,
                    Name = $" {BankName.Name} - {accountBankAccount.Title} ({accountBankAccount.AccountCurrency})",
                    CompanyId = accountBankAccount.CompanyId,
                    IsBank = true,
                    OpeningBalance = accountBankAccount.AccountBalance,
                    HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
                    Currencies = accountBankAccount.AccountCurrency
                };

                _dbContext.Account.Add(AccountRev);
                _dbContext.SaveChanges();

                AccAssetId = AccountRev.AccountsId;
            }

            if (AccGroupBankExp != null)
            {
                var AccountExp = new Accounts()
                {
                    AccountGroupsId = AccGroupBankExp.AccountGroupId,
                    Name = $" {BankName.Name} - {accountBankAccount.Title} - Expense ({accountBankAccount.AccountCurrency})",
                    CompanyId = accountBankAccount.CompanyId,
                    IsBank = true,
                    OpeningBalance = accountBankAccount.AccountBalance,
                    HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
                    Currencies = accountBankAccount.AccountCurrency
                };

                _dbContext.Account.Add(AccountExp);
                _dbContext.SaveChanges();

                AccExpId = AccountExp.AccountsId;
            }

            CompanyBankAccount BankAccount = new CompanyBankAccount
            {
                CompanyId = accountBankAccount.CompanyId,
                AccountCurrency = accountBankAccount.AccountCurrency,
                BankId = accountBankAccount.BankId,
                BranchCode = accountBankAccount.BranchCode,
                Title = accountBankAccount.Title,
                Number = accountBankAccount.Number,
                IBAN = accountBankAccount.IBAN,
                Swift = accountBankAccount.Swift,
                BeneficiaryAddress = accountBankAccount.BeneficiaryAddress,
                BankAddress = accountBankAccount.BankAddress,
                NTN = accountBankAccount.NTN,
                //AccountId = AccountId,
                AccountBalance = accountBankAccount.AccountBalance,
                BankAssetAccountId = AccAssetId,
                BankExpenseAccountId = AccExpId,
                //HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
            };

            _dbContext.CompanyBankAccounts.Add(BankAccount);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Banks = _dbContext.BanksWithoutTrash;


            CompanyBankAccount accountBankAccount = _dbContext.CompanyBankAccounts.Find(id);

            return View(accountBankAccount);
        }


        [HttpPost]
        public IActionResult Edit(CompanyBankAccount bankBranch)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Banks = _dbContext.BanksWithoutTrash;

            if (!ModelState.IsValid) return View();

            //bankBranch.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(bankBranch).State = EntityState.Modified;

            var BankAssetAccount = _dbContext.Account.Where(a => a.AccountsId == bankBranch.BankAssetAccountId).FirstOrDefault();
            var BankExpenseAccount = _dbContext.Account.Where(a => a.AccountsId == bankBranch.BankExpenseAccountId).FirstOrDefault();

            var BankName = _dbContext.BanksWithoutTrash.Where(e => e.BankId == bankBranch.BankId).FirstOrDefault();

            if (BankAssetAccount != null)
            {
                BankAssetAccount.Name = $" {BankName.Name} - {bankBranch.Title} ({bankBranch.AccountCurrency})";
                BankAssetAccount.CompanyId = bankBranch.CompanyId;
                BankAssetAccount.OpeningBalance = bankBranch.AccountBalance;
                BankAssetAccount.IsBank = true;
                BankAssetAccount.Currencies = bankBranch.AccountCurrency;
                BankAssetAccount.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                _dbContext.Entry(BankAssetAccount).State = EntityState.Modified;
            }
            
            if (BankExpenseAccount != null)
            {
                BankExpenseAccount.Name = $" {BankName.Name} - {bankBranch.Title} ({bankBranch.AccountCurrency})";
                BankExpenseAccount.CompanyId = bankBranch.CompanyId;
                BankExpenseAccount.OpeningBalance = bankBranch.AccountBalance;
                BankExpenseAccount.IsBank = true;
                BankExpenseAccount.Currencies = bankBranch.AccountCurrency;
                BankExpenseAccount.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                _dbContext.Entry(BankExpenseAccount).State = EntityState.Modified;
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CompanyBankAccounts.Include(e => e.Company).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            CompanyBankAccount accountBankAccount = _dbContext.CompanyBankAccounts?.Find(id);
            Account account = _dbContext.Accounts.Find(accountBankAccount?.AccountId);

            if (accountBankAccount == null) return RedirectToAction(nameof(Index));

            account.Trashed = true;
            accountBankAccount.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            CompanyBankAccount accountBankAccount = _dbContext.CompanyBankAccounts.Find(id);
            Account account = _dbContext.Accounts.Find(accountBankAccount.AccountId);

            account.Trashed = false; 
            accountBankAccount.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetAccounts(int id)
        {
            return Json(
                _dbContext.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyId == id)
                .Select(e => new
                {
                    e.CompanyBankAccountId,
                    title = $"{e.Title} ({e.AccountCurrency.ToString()})",
                }));
        }

        public JsonResult GetPKRAccounts(int id)
        {
            return Json(
                _dbContext.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyId == id && e.AccountCurrency == Models.Currencies.PKR)
                .Select(e => new
                {
                    e.CompanyBankAccountId,
                    title = $"{e.Title} ({e.AccountCurrency.ToString()})",
                }));
        }

        public async Task<IActionResult> ViewActiveMembers(int Id)
        {
            var CBAName = await _dbContext.CompanyBankAccounts.Where(v => v.CompanyBankAccountId == Id && !v.Trashed).SingleOrDefaultAsync();
            ViewBag.CBAName = CBAName.Title;

            var Actives = await _dbContext.BankAccountContactPersons
                .Include(v => v.Designation)
                .Where(v => v.CompanyBankAccountId == Id && v.ActiveStatus == ActiveStatus.Active && !v.Trashed && v.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            if (Actives == null)
            {
                return NotFound();
            }

            return View(Actives);
        }

    }
}

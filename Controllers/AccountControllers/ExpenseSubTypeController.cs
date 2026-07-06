using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers
{
    public class ExpenseSubTypeController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public ExpenseSubTypeController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TrashCount = await _dbContext.ExpenseSubTypes.Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).CountAsync();
            
            var ExpSubType = await _dbContext.ExpenseSubTypes
                .Include(e => e.Accounts)
                .Include(e => e.ExpenseType)
                .Include(e => e.Company)
                .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            return View(ExpSubType);
        }

        public async Task<IActionResult> Create()
        {
            var ExpTypes = await _dbContext.ExpenseTypes.Where(e => !e.Trashed).ToListAsync();
            ViewBag.ExpTypes = new SelectList(ExpTypes, "ExpenseTypeId", "TypeName");
            
            var Comp = await _dbContext.Companies.Where(e => !e.Trashed).ToListAsync();
            ViewBag.Company = new SelectList(Comp, "CompanyId", "CompanyName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExpenseSubType Subtype)
        {
            if (!ModelState.IsValid)
            {
                var ExpTypes = await _dbContext.ExpenseTypes.Where(e => !e.Trashed).ToListAsync();
                var Comp = await _dbContext.Companies.Where(e => !e.Trashed).ToListAsync();

                ViewBag.ExpTypes = new SelectList(ExpTypes, "ExpenseTypeId", "TypeName");
                ViewBag.Company = new SelectList(Comp, "CompanyId", "CompanyName");

                return View(Subtype);
            }

            Subtype.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.ExpenseSubTypes.Add(Subtype);

            var ExpType = await _dbContext.ExpenseTypes.Where(e => e.ExpenseTypeId == Subtype.ExpenseTypeId).FirstOrDefaultAsync();
            var AccGroupExp = await _dbContext.AccountGroups.Where(a => a.AccountGroupId == ExpType.AccountGroupId).FirstOrDefaultAsync();

            if (AccGroupExp != null)
            {
                var AccountExp = new Accounts()
                {
                    AccountGroupsId = AccGroupExp.AccountGroupId,
                    Name = $"{Subtype.ExpenseSubTypeName}",
                    CompanyId = Subtype.CompanyId,
                    IsExpenseAccount = true,
                    HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
                    Currencies = Subtype.Currency
                };

                _dbContext.Account.Add(AccountExp);
                await _dbContext.SaveChangesAsync();

                Subtype.AccountsId = AccountExp.AccountsId;
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ExpTypes = await _dbContext.ExpenseTypes.Where(e => !e.Trashed).ToListAsync();
            var Comp = await _dbContext.Companies.Where(e => !e.Trashed).ToListAsync();

            var ExpSubType = await _dbContext.ExpenseSubTypes.FindAsync(id);

            if (ExpSubType == null)
            {
                return NotFound();
            }

            ViewBag.ExpTypes = new SelectList(ExpTypes, "ExpenseTypeId", "TypeName", ExpSubType.ExpenseTypeId);
            ViewBag.Company = new SelectList(Comp, "CompanyId", "CompanyName", ExpSubType.CompanyId);

            return View(ExpSubType);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ExpenseSubType Subtype)
        {
            if (!ModelState.IsValid)
            {
                var ExpTypes = await _dbContext.ExpenseTypes.Where(e => !e.Trashed).ToListAsync();
                var Comp = await _dbContext.Companies.Where(e => !e.Trashed).ToListAsync();

                ViewBag.ExpTypes = new SelectList(ExpTypes, "ExpenseTypeId", "TypeName", Subtype.ExpenseTypeId);
                ViewBag.Company = new SelectList(Comp, "CompanyId", "CompanyName", Subtype.CompanyId);

                return View(Subtype);
            }

            var expenseSubType = await _dbContext.ExpenseSubTypes.FindAsync(Subtype.ExpenseSubTypeId);
            if (expenseSubType == null)
            {
                return NotFound();
            }

            expenseSubType.ExpenseSubTypeName = Subtype.ExpenseSubTypeName;
            expenseSubType.ExpenseTypeId = Subtype.ExpenseTypeId;
            expenseSubType.CompanyId = Subtype.CompanyId;
            expenseSubType.Currency = Subtype.Currency;
            expenseSubType.AccountNumber = Subtype.AccountNumber;
            expenseSubType.IBAN = Subtype.IBAN;
            expenseSubType.SwiftCode = Subtype.SwiftCode;
            expenseSubType.NTN = Subtype.NTN;
            expenseSubType.BeneficiaryAddress = Subtype.BeneficiaryAddress;
            expenseSubType.BankAddress = Subtype.BankAddress;
            expenseSubType.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            if (expenseSubType.AccountsId.HasValue)
            {
                var account = await _dbContext.Account.FirstOrDefaultAsync(a => a.AccountsId == expenseSubType.AccountsId);
                if (account != null)
                {
                    var expenseType = await _dbContext.ExpenseTypes.FirstOrDefaultAsync(e => e.ExpenseTypeId == Subtype.ExpenseTypeId);
                    var accountGroup = await _dbContext.AccountGroups.FirstOrDefaultAsync(a => a.AccountGroupId == expenseType.AccountGroupId);

                    account.Name = Subtype.ExpenseSubTypeName;
                    account.AccountGroupsId = accountGroup.AccountGroupId;
                    account.CompanyId = Subtype.CompanyId;
                    account.Currencies = Subtype.Currency;
                    account.IsExpenseAccount = true;
                    account.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                    _dbContext.Entry(account).State = EntityState.Modified;
                }
            }

            _dbContext.Entry(expenseSubType).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewTrash()
        {
            var ExpSubType = await _dbContext.ExpenseSubTypes
                .Include(e => e.Accounts)
                .Include(e => e.ExpenseType)
                .Include(e => e.Company)
                .Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            return View(ExpSubType);
        }

        public IActionResult Trash(int id)
        {
            var type = _dbContext.ExpenseSubTypes.Find(id);
            if (type == null) return RedirectToAction(nameof(Index));

            type.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            var type = _dbContext.ExpenseSubTypes.IgnoreQueryFilters().FirstOrDefault(e => e.ExpenseSubTypeId == id);
            type.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewActiveMembers(int Id)
        {
            var ExpSubTypeName = await _dbContext.ExpenseSubTypes.Where(v => v.ExpenseSubTypeId == Id && !v.Trashed).SingleOrDefaultAsync();
            ViewBag.ExpSubTypeName = ExpSubTypeName.ExpenseSubTypeName;

            var Actives = await _dbContext.ExpenseContactPersons
                .Include(v => v.Designation)
                .Where(v => v.ExpenseSubTypeId == Id 
                        && v.ActiveStatus == ActiveStatus.Active 
                        && !v.Trashed 
                        && v.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            if (Actives == null)
            {
                return NotFound();
            }

            return View(Actives);
        }

        public IActionResult Print(int id)
        {
            ViewBag.Name = _dbContext.ExpenseSubTypes
                .Where(e => e.ExpenseSubTypeId == id)
                .Select(e => e.ExpenseSubTypeName)
                .FirstOrDefault();

            ViewBag.AcTitle = _dbContext.ExpenseSubTypes
                .Where(e => e.ExpenseSubTypeId == id)
                .Select(e => e.AccountTitle)
                .FirstOrDefault();

            ViewBag.AcNo = _dbContext.ExpenseSubTypes
                .Where(e => e.ExpenseSubTypeId == id)
                .Select(e => e.AccountNumber)
                .FirstOrDefault();

            ViewBag.IBAN = _dbContext.ExpenseSubTypes
                .Where(e => e.ExpenseSubTypeId == id)
                .Select(e => e.IBAN)
                .FirstOrDefault();

            int? accountsId = _dbContext.ExpenseSubTypes
                .Where(e => e.ExpenseSubTypeId == id)
                .Select(e => e.AccountsId)
                .FirstOrDefault();

            var query = (from npv in _dbContext.NewPaymentVouchers
                        join np in _dbContext.NewPayments
                        on npv.NewPaymentVoucherId equals np.NewPaymentVoucherId
                        where !npv.Trashed && np.AccountsId == accountsId
                        select new { npv, np });

            var result = query.Select(e => new ExpenseReport
            {
                PVNumber = e.npv.PaymentVoucherNo,
                Date = e.npv.Date.ToString("dd/MMM/yyyy"),
                InstrumentNo = e.npv.InstrumentNumber,
                Remarks = e.npv.DebitRemarks,
                TotalAmount = e.npv.TotalDebit,
                PaymentMode = e.npv.PaymentMode.ToString()
            }).ToList();

            return View(result);
        }

    }

    public record ExpenseReport
    {
        public string PVNumber { get; set; }
        public string Date { get; set; }
        public string InstrumentNo { get; set; }
        public string Remarks { get; set; }
        public float? TotalAmount { get; set; }
        public string PaymentMode { get; set; }
    }

}


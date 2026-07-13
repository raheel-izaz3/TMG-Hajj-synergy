using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TMG.Data;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers;



public class VendorController : Controller
{
    public record VendorReport
    {
        public string Date { get; set; }
        public string PV { get; set; }
        public string InstrumentNo { get; set; }
        public Currencies Currency { get; set; }
        public float? ReceivedAmount { get; set; }
        public float? ROE { get; set; }
        public float? Amount { get; set; }
        public string Logo { get; set; }

    }

    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;
    private readonly IWebHostEnvironment _environment;

    public VendorController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _http = http;
        _environment = environment;
    }

    public IActionResult Index()
    {
        ViewBag.TrashCount = _dbContext.Vendors.Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();

        //ViewBag.TrashAllowed = PrivilegeManager.CanTrashType(_dbContext);
        //ViewBag.CreateAllowed = PrivilegeManager.CanCreateType(_dbContext);
        //ViewBag.EditAllowed = PrivilegeManager.CanEditType(_dbContext);
        //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreType(_dbContext);

        return View(_dbContext.Vendors.Include(e => e.VendorType).Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
    }

    public IActionResult Create()
    {
        ViewBag.Types = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Country = _dbContext.Countries.Where(e => !e.Trashed);
        ViewBag.City = _dbContext.Cities.Where(e => !e.Trashed);

        return View();
    }


    [HttpPost]
    public IActionResult Create(Vendor type, IFormFile image)
    {

        if (!ModelState.IsValid)
        {
            ViewBag.Types = _dbContext.VendorTypes.Where(e => !e.Trashed);

            return View();
        }

        if (image != null && image.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "VendorLogo");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Path.GetFileName(image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            type.Logo = "/VendorLogo/" + fileName;
        }

        type.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
        _dbContext.Vendors.Add(type);

        var VendorTyp = _dbContext.VendorTypes.Where(v => v.VendorTypeId == type.VendorTypeId).FirstOrDefault();

        if (VendorTyp != null)
        {
            Accounts Acc = new Accounts()
            {
                Name = $"{type.VendorName} ({type.Currencies.ToString()})",
                AccountGroupsId = VendorTyp.AccountGroupId,
                IsVendorAccount = true,
                CompanyId = 1,
                HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
                Currencies = type.Currencies
            };

            _dbContext.Account.Add(Acc);
            _dbContext.SaveChanges();

            type.AccountsId = Acc.AccountsId;
        }

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        ViewBag.Types = _dbContext.VendorTypes.Where(e => !e.Trashed);
        Vendor type = _dbContext.Vendors.Include(v => v.Accounts).Where(v => v.VendorId == id).FirstOrDefault();
        ViewBag.Country = _dbContext.Countries.Where(e => !e.Trashed);
        ViewBag.City = _dbContext.Cities.Where(e => !e.Trashed);

        return View(type);
    }


    [HttpPost]
    public async Task<IActionResult> Edit(Vendor type, IFormFile image)
    {

        if (!ModelState.IsValid)
        {
            ViewBag.Types = _dbContext.VendorTypes.Where(e => !e.Trashed);

            return View();
        }

        var account = await _dbContext.Account.FirstOrDefaultAsync(a => a.AccountsId == type.AccountsId);
        if (account != null)
        {
            var vendType = await _dbContext.VendorTypes.FirstOrDefaultAsync(e => e.VendorTypeId == type.VendorTypeId);
            var accountGroup = await _dbContext.AccountGroups.FirstOrDefaultAsync(a => a.AccountGroupId == vendType.AccountGroupId);

            account.Name = $"{type.VendorName} ({type.Currencies.ToString()})";
            account.AccountGroupsId = accountGroup.AccountGroupId;
            account.Currencies = type.Currencies;
            account.IsVendorAccount = true;
            account.CompanyId = 1;
            account.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(account).State = EntityState.Modified;
        }

        if (image != null && image.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "VendorLogo");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Path.GetFileName(image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            type.Logo = "/VendorLogo/" + fileName;
        }

        type.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
        _dbContext.Entry(type).State = EntityState.Modified;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult ViewTrash()
    {
        return View(_dbContext.Vendors.Include(e => e.VendorType).Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
    }

    public IActionResult Trash(int id)
    {
        Vendor type = _dbContext.Vendors.Find(id);
        if (type == null) return RedirectToAction(nameof(Index));

        type.Trashed = true;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Restore(int id)
    {
        Vendor type = _dbContext.Vendors.Find(id);
        type.Trashed = false;
        _dbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ViewActiveMembers(int Id)
    {
        var VendName = await _dbContext.Vendors.Where(v => v.VendorId == Id && !v.Trashed).SingleOrDefaultAsync();
        ViewBag.VendName = VendName.VendorName;

        var Actives = await _dbContext.VendorContactPersons
            .Include(v => v.Designation)
            .Where(v => v.VendorId == Id 
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

    public IActionResult Report(int id)
    {
        ViewBag.VendorName = _dbContext.Vendors
            .Where(e => e.VendorId == id)
            .Select(e => e.VendorName)
            .FirstOrDefault();

        ViewBag.AccountNumber = _dbContext.Vendors
            .Where(e => e.VendorId == id)
            .Select(e => e.AccountNumber)
            .FirstOrDefault();

        ViewBag.Iban = _dbContext.Vendors
            .Where(e => e.VendorId == id)
            .Select(e => e.IBAN)
            .FirstOrDefault();

        ViewBag.Logo = _dbContext.Vendors
            .Where(e => e.VendorId == id)
            .Select(e => e.Logo)
            .FirstOrDefault();

        var query = _dbContext.Vendors
            .Join(_dbContext.NewPayments,
                v => v.AccountsId,
                np => np.AccountsId,
                (v, np) => new { v, np })
            .Join(_dbContext.NewPaymentVouchers,
                npv => npv.np.NewPaymentVoucherId,
                pv => pv.NewPaymentVoucherId,
                (npv, pv) => new { npv.v, npv.np, pv })
            .Where(e => e.v.VendorId == id)
            .Select(r => new
            {
                Vendor = r.v,
                NewPayment = r.np,
                NewPaymentVoucher = r.pv
            });

        var result = query.Select(e => new VendorReport
        {
            Date = e.NewPaymentVoucher.Date.ToString("dd/MMM/yyyy"),
            PV = e.NewPaymentVoucher.PaymentVoucherNo,
            InstrumentNo = e.NewPaymentVoucher.InstrumentNumber,
            Currency = e.Vendor.Currencies,
            ReceivedAmount = e.NewPaymentVoucher.TotalCredit,
            ROE = e.NewPaymentVoucher.ROE,
            Amount = e.NewPaymentVoucher.TotalDebit,
            //Logo = e.Vendor.Logo
        }).ToList();

        ViewBag.Model = result;

        return View(result);
    }
}

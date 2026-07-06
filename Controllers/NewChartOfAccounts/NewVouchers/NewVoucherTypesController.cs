using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.NewChartOfAccounts.FiscalYear;
using TMG.Models.NewChartOfAccounts.NewVoucher;

namespace TMG.Controllers.NewChartOfAccounts.NewVouchers
{
    public class NewVoucherTypesController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public NewVoucherTypesController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.FiscalPeriods.Where(e => e.Trashed).Count();

            return View(_dbContext.NewVoucherTypes.Where(e => !e.Trashed));
        }

        public IActionResult Create()
        {

            return View();
        }


        [HttpPost]
        public IActionResult Create(NewVoucherTypes voucher)
        {
            if (!ModelState.IsValid) return View(voucher);

            _dbContext.NewVoucherTypes.Add(voucher);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {

            NewVoucherTypes voucher = _dbContext.NewVoucherTypes.FirstOrDefault(f => f.NewVoucherTypesId == id);

            return View(voucher);
        }


        [HttpPost]
        public IActionResult Edit(NewVoucherTypes vouchers)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(vouchers).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.NewVoucherTypes.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            NewVoucherTypes vouchers = _dbContext.NewVoucherTypes.Find(id);
            if (vouchers == null) return RedirectToAction(nameof(Index));

            vouchers.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            NewVoucherTypes vouchers = _dbContext.NewVoucherTypes.Find(id);
            vouchers.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

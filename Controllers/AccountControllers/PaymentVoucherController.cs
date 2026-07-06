using TMG.Data;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;
using TMG.Models.FormModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static System.Net.WebRequestMethods;

namespace TMG.Models.AccountModels
{
    public record PaymentVoucherInvoiceModel
    {
        public PaymentVoucher PaymentVoucher { get; set; }
        public List<VoucherEntry> VoucherEntries { get; set; }
        public float Amount { get; set; }
    }
}

namespace TMG.Controllers.AccountControllers
{
    public class PaymentVoucherController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public PaymentVoucherController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext
                .PaymentVouchers
                .Where(e => e.Trashed)
                .Count();

            ViewBag.VoucherTypes = _dbContext.VoucherTypes;

            return View(_dbContext
                .PaymentVouchersWithoutTrash
                .Include(e => e.VoucherType)
                .Include(e => e.Account)
                .Include(e => e.Company)
            );
        }

        [HttpGet]
        public IActionResult GetRoe(Currencies currency, string date)
        {

            DateTime Today = DateTime.Parse(date);
            var RateOfExchange = _dbContext.RateOfExchangesWithoutTrash.Where(e => e.Date == Today && e.Currency == currency);
            return Ok(RateOfExchange);
        }

        [HttpGet]

        public IActionResult GetVoucher(int id, string startDate, string endDate)
        {


            //DateTime parsedStartDate;
            //DateTime parsedEndDate;

            DateTime StartDateTime = DateTime.Parse(startDate);
            DateTime EndDateTime = DateTime.Parse(endDate);

            //if (DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartDate)
            //    &&
            //    DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndDate))
            //{

            var vouchers = _dbContext
             .PaymentVouchersWithoutTrash
             .Where(e => e.VoucherTypeId == id)
             .Where(e => e.Date >= StartDateTime && e.Date <= EndDateTime)
             .Include(e => e.Company)
             .Include(e => e.Account)
             .ToList();

            return Ok(vouchers);

            //}
            //else{
            //    return Ok("ABC");
            //}


        }

        public IActionResult Create()
        {
            ViewBag.VoucherTypes = _dbContext.VoucherTypes;
            ViewBag.Companies = _dbContext.UserCompanies
            .Include(e => e.Company)
            .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);

            ViewBag.Accounts = _dbContext.AccountsWithoutTrash
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .ThenInclude(e => e.Type);

            return View();
        }

        [HttpPost]
        public IActionResult Create(PaymentVoucherRegisterRequest request)
        {

            // return Ok(request);
            if (ModelState.IsValid)
            {
                var transaction = _dbContext.Database.BeginTransaction();
                try
                {
                    ViewBag.VoucherTypes = _dbContext.VoucherTypes;
                    ViewBag.Companies = _dbContext.UserCompanies.Include(e => e.Company)
                            .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);

                    ViewBag.Accounts = _dbContext.AccountsWithoutTrash
                    .Include(e => e.Category)
                    .ThenInclude(e => e.SubType)
                    .ThenInclude(e => e.Type);
                    PaymentVoucher voucher = new PaymentVoucher
                    {
                        CompanyId = request.CompanyId,
                        Date = request.Date,
                        AccountId = request.AccountId,
                        InstrumentNumber = request.InstrumentNumber,
                        VoucherTypeId = request.VoucherTypeId,
                        Description = request.Description,
                        Amount = request.DebitAmounts[0],
                        PKRROE = request.PKRROE,
                        SARROE = request.SARROE,
                        USDROE = request.USDROE,
                        Currency = request.Currency,
                        ReceivingParty = request.ReceivingParty,

                    };

                    _dbContext.PaymentVouchers.Add(voucher);
                    _dbContext.SaveChanges();

                    _dbContext.VoucherEntries.Add(
                        new VoucherEntry
                        {
                            PaymentVoucherId = voucher.PaymentVoucherId,
                            AccountId = request.AccountIds[0],
                            Debit = request.DebitAmounts[0],
                            Credit = request.CreditAmounts[0],
                            DebitOrCreditSide = ShowAccountDebitOrCreditSide.Debit
                        }
                        );

                    _dbContext.VoucherEntries.Add(
                        new VoucherEntry
                        {
                            PaymentVoucherId = voucher.PaymentVoucherId,
                            AccountId = request.AccountIds[1],
                            Debit = request.DebitAmounts[1],
                            Credit = request.CreditAmounts[1],
                            DebitOrCreditSide = ShowAccountDebitOrCreditSide.Credit
                        }
                        );

                    _dbContext.SaveChanges();

                    transaction.Commit();
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return View(request);
                }

            }
            else { return View(request); }

        }

        public IActionResult Edit(int id)
        {
            ViewBag.VoucherTypes = _dbContext.VoucherTypes;
            ViewBag.Companies = _dbContext.UserCompanies.Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed)
                        .ToList();

            ViewBag.Accounts = _dbContext.AccountsWithoutTrash
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .ThenInclude(e => e.Type);

            PaymentVoucher voucher = _dbContext.PaymentVouchers.Find(id);

            var entries = _dbContext.VoucherEntries.Where(e => e.PaymentVoucherId == id).ToList();

            List<float> Debits = new List<float>();
            List<float> Credits = new List<float>();
            List<int> AccountIds = new List<int>();

            for (int i = 0; i < entries.Count(); i++)
            {
                AccountIds.Add(entries.ElementAt(i).AccountId);
                Debits.Add(entries.ElementAt(i).Debit);
                Credits.Add(entries.ElementAt(i).Credit);
            }

            PaymentVoucherEditRequest request = new PaymentVoucherEditRequest
            {
                PaymentVoucherId = voucher.PaymentVoucherId,
                Date = voucher.Date,
                InstrumentNumber = voucher.InstrumentNumber,
                AccountId = voucher.AccountId,
                PKRROE = voucher.PKRROE,
                SARROE = voucher.SARROE,
                USDROE = voucher.USDROE,
                VoucherTypeId = voucher.VoucherTypeId,
                DebitAmounts = Debits,
                CreditAmounts = Credits,
                AccountIds = AccountIds,
                Description = voucher.Description,
                ReceivingParty = voucher.ReceivingParty,
                CompanyId = (int)voucher.CompanyId
            };

            return View(request);
        }

        [HttpPost]
        public IActionResult Edit(PaymentVoucherEditRequest request)
        {
            ViewBag.VoucherTypes = _dbContext.VoucherTypes;
            ViewBag.Companies = _dbContext.UserCompanies.Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed)
                        .ToList();

            ViewBag.Accounts = _dbContext.AccountsWithoutTrash
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .ThenInclude(e => e.Type);

            PaymentVoucher _voucher = _dbContext.PaymentVouchers.Find(request.PaymentVoucherId);

            if (!ModelState.IsValid) return View(request);

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                _voucher.CompanyId = request.CompanyId;
                _voucher.PKRROE = request.PKRROE;
                _voucher.AccountId = request.AccountId;
                _voucher.ReceivingParty = request.ReceivingParty;
                _voucher.InstrumentNumber = request.InstrumentNumber;
                _voucher.Date = request.Date;
                _voucher.Description = request.Description;
                _voucher.VoucherTypeId = request.VoucherTypeId;
                _voucher.Amount = request.DebitAmounts[0];

                _dbContext.VoucherEntries.Where(e => e.PaymentVoucherId == request.PaymentVoucherId).ExecuteDelete();

                _dbContext.VoucherEntries.Add(
                       new VoucherEntry
                       {
                           PaymentVoucherId = request.PaymentVoucherId,
                           AccountId = request.AccountIds[0],
                           Debit = request.DebitAmounts[0],
                           Credit = request.CreditAmounts[0],
                           DebitOrCreditSide = ShowAccountDebitOrCreditSide.Debit
                       }
                       );

                _dbContext.VoucherEntries.Add(
                    new VoucherEntry
                    {
                        PaymentVoucherId = request.PaymentVoucherId,
                        AccountId = request.AccountIds[1],
                        Debit = request.DebitAmounts[1],
                        Credit = request.CreditAmounts[1],
                        DebitOrCreditSide = ShowAccountDebitOrCreditSide.Credit
                    }
                    );

                _dbContext.SaveChanges();

                transaction.Commit();

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                transaction.Rollback();

                return View(request);
            }

        }

        public IActionResult ViewTrash()
        {
            return View(
                _dbContext
                .PaymentVouchers
                .Include(e => e.VoucherType)
                .Include(e => e.Company)
                .Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            PaymentVoucher ac = _dbContext.PaymentVouchers.Find(id);
            if (ac == null) return RedirectToAction(nameof(Index));

            ac.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            PaymentVoucher ac = _dbContext.PaymentVouchers.Find(id);
            ac.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Invoice(int id)
        {

            PaymentVoucher voucher = _dbContext.PaymentVouchers
                 .Include(e => e.VoucherType)
                 .Include(e => e.Company)
                 .FirstOrDefault(e => e.PaymentVoucherId == id);

            var entries = _dbContext.VoucherEntries
                .Include(e => e.Account)
                .ThenInclude(e => e.Category)
                .ThenInclude(e => e.SubType)
                .Where(e => e.PaymentVoucherId == id).ToList();

            PaymentVoucherInvoiceModel model = new PaymentVoucherInvoiceModel
            {
                PaymentVoucher = voucher,
                VoucherEntries = entries,
                Amount = entries.ElementAt(0).Debit
            };

            return View(model);


        }




    }
}

using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web.WebPages;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers
{
    public class BankBalanceData
    {
        public int BankId { get; set; }
        public string Bank { get; set; }
        public string BankName { get; set; }
        public string Currency { get; set; }
        public float TotalDebit { get; set; }
        public float TotalCredit { get; set; }
        public float Balance { get; set; }
    }

    public class ADashboardController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _http;

        public ADashboardController(TMGDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public IActionResult Index()
        {
            var banks = GetBankWiseBalance();

            ViewBag.Banks = banks;

            return View();
        }

        public List<BankBalanceData> GetBankWiseBalance()
        {
            var hajjSeasonId = Convert.ToInt32(HttpContext.Session.GetString("ActiveSeasonId"));

            var banks = _context.CompanyBankAccountsWithoutTrash.Include(e => e.Bank).ToList();

            List<BankBalanceData> model = new();

            foreach (var bank in banks)
            {
                var totalCredit = _context.NewInvoicesWithoutTrash
                    .Where(i => i.CompanyBankAccountId == bank.CompanyBankAccountId && i.ReceiptStatus == ReceiptStatus.Cleared)
                    .Select(i => (float?)i.AmountRecieved)
                    .Sum() ?? 0;

                //var totalDebit = (from pv in _context.NewPaymentVouchers
                //                  from newPayment in pv.Payments
                //                  from cba in _context.CompanyBankAccountsWithoutTrash.Where(e => e.BankAssetAccountId == newPayment.AccountsId)
                //                  where cba.CompanyBankAccountId == bank.CompanyBankAccountId
                //                    && newPayment.AccountNature == AccountNature.Debit
                //                  select (float?)pv.TotalDebit).Sum() ?? 0;

                //var totalDebit = (from pv in _context.NewPaymentVouchers
                //                   from newPayment in pv.Payments
                //                   from cba in _context.CompanyBankAccountsWithoutTrash
                //                        .Where(e => e.BankAssetAccountId == newPayment.AccountsId)
                //                   where cba.CompanyBankAccountId == bank.CompanyBankAccountId
                //                   select (float?)(
                //                       newPayment.AccountNature == AccountNature.Credit
                //                           ? pv.TotalDebit
                //                           : pv.TotalCredit
                //                   )).Sum() ?? 0;


                var totalDebit = (from pv in _context.NewPaymentVouchers
                                  from newPayment in pv.Payments
                                  from cba in _context.CompanyBankAccountsWithoutTrash
                                       .Where(e => e.BankAssetAccountId == newPayment.AccountsId)
                                  where cba.CompanyBankAccountId == bank.CompanyBankAccountId
                                  select (float?)(newPayment.AccountNature == AccountNature.Debit
                                       ? pv.TotalDebit
                                       : 0)).Sum() ?? 0;

                var totalCredit2 = (from pv in _context.NewPaymentVouchers
                                   from newPayment in pv.Payments
                                   from cba in _context.CompanyBankAccountsWithoutTrash
                                        .Where(e => e.BankAssetAccountId == newPayment.AccountsId)
                                   where cba.CompanyBankAccountId == bank.CompanyBankAccountId
                                   select (float?)(newPayment.AccountNature == AccountNature.Credit
                                        ? pv.TotalCredit
                                        : 0)).Sum() ?? 0;


                var balance =  (totalCredit + totalDebit) - totalCredit2;

                BankBalanceData view = new()
                {
                    BankId = bank.CompanyBankAccountId,
                    Bank = bank.Bank.Name,
                    BankName = bank.Title,
                    Currency = bank.AccountCurrency.ToString(),
                    Balance = balance
                };

                model.Add(view);
            }

            return model;
        }

        public IActionResult BanksReport(int id)
        {
            ViewBag.CompanyBankName = _context.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyBankAccountId == id)
                .Select(e => e.Title)
                .FirstOrDefault();

            ViewBag.AccountNumber = _context.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyBankAccountId == id)
                .Select(e => e.IBAN)
                .FirstOrDefault();

            ViewBag.Iban = _context.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyBankAccountId == id)
                .Select(e => e.IBAN)
                .FirstOrDefault();

            var invoiceQuery = _context.NewInvoicesWithoutTrash
                .Where(invoice => invoice.CompanyBankAccountId == id && invoice.ReceiptStatus == ReceiptStatus.Cleared)
                .Select(invoice => new BankStatementData
                {
                    Date = invoice.ClearanceDate,
                    Remarks = invoice.Remarks ?? "",
                    DebitAmount = 0f,
                    CreditAmount = invoice.AmountRecieved,
                    Voucher = invoice.ReceiptNumber,
                    Name = invoice.FamilyHead.FullName,
                    HBNo = invoice.FamilyHead.FcId,
                    Currency = invoice.Currencies.ToString(),
                    Proof = invoice.ImagePath
                }).ToList();

            var paymentQuery = from payment in _context.NewPaymentVouchers
                               from newPayment in payment.Payments
                               from cba in _context.CompanyBankAccountsWithoutTrash.Where(e => e.BankAssetAccountId == newPayment.AccountsId)
                               where cba.CompanyBankAccountId == id
                               select new BankStatementData
                               {
                                   Date = payment.Date,
                                   Remarks = payment.CreditRemarks ?? "",
                                   DebitAmount = newPayment.AccountNature == AccountNature.Credit ? payment.TotalCredit ?? 0f : 0f,
                                   CreditAmount = newPayment.AccountNature == AccountNature.Debit ? payment.TotalDebit ?? 0f : 0f,
                                   Voucher = payment.PaymentVoucherNo,
                                   Name = newPayment.Accounts.Name,
                                   HBNo = "",
                                   Currency = newPayment.Accounts.Currencies.ToString(),
                                   Proof = payment.File
                               };

            var allData = invoiceQuery
                    .Union(paymentQuery)
                    .OrderBy(x => x.Date)
                    .ToList();

            float runningBalance = 0;

            var finalReport = allData.Select(item =>
            {
                runningBalance += (item.CreditAmount - item.DebitAmount);
                return new BankStatment
                {
                    Voucher = item.Voucher,
                    Name = item.Name,
                    HBNo = item.HBNo,
                    Date = item.Date,
                    Remarks = item.Remarks,
                    Debit = item.DebitAmount,
                    Credit = item.CreditAmount,
                    Balance = runningBalance,
                    Currency = item.Currency,
                    Proof = item.Proof
                };
            }).ToList();

            return View(finalReport);
        }

    }
}

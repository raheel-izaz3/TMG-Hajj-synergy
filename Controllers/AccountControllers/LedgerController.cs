using Azure.Core;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.InvoiceModels;
using TMG.Models.SettingsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Collections;

namespace TMG.Models.AccountModels
{
    public record LedgerReportModel
    {
        public Account Account { get; set; }
        //public List<PaymentVoucher> PaymentVouchers { get; set; }
        //public List<List<VoucherEntry>> VoucherEntries { get; set; } = new List<List<VoucherEntry>>();
        public List<DateTime> DateTimes { get; set; } = new List<DateTime>();
        public List<int> VoucherIds { get; set; } = new List<int>();
        public List<string> CreditDebit { get; set; } = new List<string>();
        public List<string> Details { get; set; } = new List<string>();
        public List<float> Debits { get; set; } = new List<float>();
        public List<float> Credits { get; set; } = new List<float>();
        public List<float> Balances { get; set; } = new List<float>();
        public List<float> PkrRoe { get; set; } = new List<float>();
        public List<float> SarRoe { get; set; } = new List<float>();
        public List<float> UsdRoe { get; set; } = new List<float>();

        public List<string> Currency { get; set; } = new List<string>();


    }
    
}




namespace TMG.Controllers.AccountControllers
{
    public class LedgerController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public LedgerController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {

            ViewBag.Types = _dbContext.Types;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;

            ViewBag.Company = _dbContext.CompaniesWithoutTrash;
            ViewBag.Category = _dbContext.CategoriesWithoutTrash;
            ViewBag.Account = _dbContext.AccountsWithoutTrash;

            return View();
        }

        [HttpGet]
        public IActionResult Print(int id,string startDate ,string endDate)
        {
            ViewBag.Account = new {
                id,
                startDate,
                endDate
            };

            return View();
        }
        [HttpGet]
        public IActionResult GetLedger(int compnay_id, int category_id, int account_id, string startDate, string endDate, int currency)
        {
            // DateTime StartDateTime = DateTime.Parse(startDate);
            // DateTime EndDateTime = DateTime.Parse(endDate);

            DateTime StartDateTime = DateTime.Parse(startDate);
            DateTime EndDateTime = DateTime.Parse(endDate);
            var accounts = _dbContext.AccountsWithoutTrash
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .ThenInclude(e => e.Type)
                .Where(e => e.AccountId == account_id) as IEnumerable<Account>;

            AccountNature accountNature = _dbContext.AccountsWithoutTrash
                            .Include(e => e.Category)
                            .ThenInclude(e => e.SubType)
                            .ThenInclude(e => e.Type)
                            .Where(e => e.AccountId == account_id)
                            .Select(e => e.Category.SubType.Type.AccountNature)
                            .FirstOrDefault();

            int invoiceId = _dbContext.FamilyHeadsWithoutTrash
                            .Where(e => e.AccountId == account_id)  
                            .Select(e => e.FamilyHeadId)
                            .FirstOrDefault();


            var vouchers = _dbContext.PaymentVouchersWithoutTrash
                .Include(e => e.VoucherType)
                // .Include(e => e.Invoice)
                .Where(e => e.Date >= StartDateTime && e.Date <= EndDateTime);
                // .Where(e => e.Invoice.InvoiceStatus == InvoiceStatus.Cleared);

            var voucherEntries = _dbContext.VoucherEntries
                .Include(e => e.Account)
                .Include(e => e.PaymentVoucher);

            List<LedgerReportModel> models = new List<LedgerReportModel>();

            
            List<LedgerReportModel> models_open = new List<LedgerReportModel>();


            Account currentAccount = accounts.ElementAt(0);
            LedgerReportModel model = new LedgerReportModel { Account = currentAccount };
            LedgerReportModel model_open = new LedgerReportModel { Account = currentAccount };



            var currentAccountEntries_only = voucherEntries
                                .Where(e => e.AccountId == currentAccount.AccountId)
                                as IEnumerable<VoucherEntry>;

            var currentAccountEntries_opening = voucherEntries
                                .Where(e => e.AccountId == currentAccount.AccountId)
                                .Where(e => e.PaymentVoucher.Date < StartDateTime)
                                as IEnumerable<VoucherEntry>;

        

            var currentAccountEntries = voucherEntries
                                .Include(e => e.PaymentVoucher)
                                .Where(e => e.AccountId == currentAccount.AccountId)
                                .Where(e => e.PaymentVoucher.Date >= StartDateTime && e.PaymentVoucher.Date <= EndDateTime) as IEnumerable<VoucherEntry>;


            float startingBalance = 0;
            float balanceCarriedForward = 0;


            for (int j = 0; j < currentAccountEntries_opening.Count(); j++)
            {
                var entry = currentAccountEntries_opening.ElementAt(j);
                model_open.DateTimes.Add(entry.PaymentVoucher.Date);
                model_open.Currency.Add(entry.PaymentVoucher.Currency.ToString());
                model_open.PkrRoe.Add(entry.PaymentVoucher.PKRROE);
                model_open.SarRoe.Add(entry.PaymentVoucher.SARROE);
                model_open.UsdRoe.Add(entry.PaymentVoucher.USDROE);
                model_open.Details.Add(entry.PaymentVoucher.Description);
                model_open.VoucherIds.Add(entry.PaymentVoucherId);
                model_open.Debits.Add(entry.Debit);
                model_open.Credits.Add(entry.Credit);
                model_open.CreditDebit.Add(entry.DebitOrCreditSide.ToString());

                if (j == 0)
                {
                    if (currentAccount.Category.SubType.Type.AccountNature == AccountNature.Debit)
                    {
                        startingBalance = entry.Debit - entry.Credit;
                    }
                    else
                    {
                        startingBalance = entry.Credit - entry.Debit;
                    }
                    model_open.Balances.Add(startingBalance);
                    balanceCarriedForward = startingBalance;

                }
                else
                {
                    if (currentAccount.Category.SubType.Type.AccountNature == AccountNature.Debit)
                    {
                        balanceCarriedForward = (balanceCarriedForward + entry.Debit) - entry.Credit;
                        model_open.Balances.Add(balanceCarriedForward);
                    }
                    else
                    {
                        balanceCarriedForward = (balanceCarriedForward + entry.Credit) - entry.Debit;
                        model_open.Balances.Add(balanceCarriedForward);
                    }
                }



                models_open.Add(model_open);
            }

            for (int j = 0; j < currentAccountEntries.Count(); j++)
            {
                var entry = currentAccountEntries.ElementAt(j);
                model.DateTimes.Add(entry.PaymentVoucher.Date);
                model.Currency.Add(entry.PaymentVoucher.Currency.ToString());
                model.PkrRoe.Add(entry.PaymentVoucher.PKRROE);
                model.SarRoe.Add(entry.PaymentVoucher.SARROE);
                model.UsdRoe.Add(entry.PaymentVoucher.USDROE);
                model.Details.Add(entry.PaymentVoucher.Description);
                model.VoucherIds.Add(entry.PaymentVoucherId);
                model.Debits.Add(entry.Debit);
                model.Credits.Add(entry.Credit);
                model.CreditDebit.Add(entry.DebitOrCreditSide.ToString());


                if (j == 0)
                {
                    if (currentAccount.Category.SubType.Type.AccountNature == AccountNature.Debit)
                    {
                        startingBalance = entry.Debit - entry.Credit;
                    }
                    else
                    {
                        startingBalance = entry.Credit - entry.Debit;
                    }
                    model.Balances.Add(startingBalance);
                    balanceCarriedForward = startingBalance;

                }
                else
                {
                    if (currentAccount.Category.SubType.Type.AccountNature == AccountNature.Debit)
                    {
                        balanceCarriedForward = (balanceCarriedForward + entry.Debit) - entry.Credit;
                        model.Balances.Add(balanceCarriedForward);
                    }
                    else
                    {
                        balanceCarriedForward = (balanceCarriedForward + entry.Credit) - entry.Debit;
                        model.Balances.Add(balanceCarriedForward);
                    }
                }



                models.Add(model);
            }

            return Ok(new {
                invoiceId,
                accountNature,
                model,
                model_open
            });

        }
    }
}

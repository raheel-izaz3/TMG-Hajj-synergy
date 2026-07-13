using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NuGet.Protocol.Resources;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.FormModels;
using TMG.Models.InvoiceModels;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;
using static iTextSharp.text.pdf.AcroFields;
using Currencies = TMG.Models.Currencies;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public class JVController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public JVController(TMGDbContext dbContext, IHttpContextAccessor contextAccessor, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _http = contextAccessor;
            _host = host;
        }

        public IActionResult Index()
        {
            //ViewBag.TrashCount = _dbContext.NewPaymentVouchers.Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();
            ViewBag.TrashCount = _dbContext.JVs
            .Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();

            var PV = _dbContext.JVs
                .Include(p => p.Payments)
                .ThenInclude(p => p.Accounts)
                .IgnoreQueryFilters()
                .Where(p => !p.Trashed && p.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View(PV);
        }

        #region Create With Taxes
        //public async Task<IActionResult> Create()
        //{
        //    var Taxes = await _dbContext.Taxes.Where(t => !t.Trashed && t.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToListAsync();

        //    var Accounts = await _dbContext.Account
        //        .Where(
        //            e => !e.Trashed &&
        //            e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
        //            )
        //        .Select(a => new
        //        {
        //            a.AccountsId,
        //            a.Name,
        //            Currency = a.Currencies.ToString(),
        //            Bank = a.IsBank,
        //            Vendor = a.IsVendorAccount,
        //            Hajji = a.IsHajjiAccount,
        //            Expense = a.IsExpenseAccount
        //        })
        //        .ToListAsync();

        //    var accountNatures = Enum.GetValues(typeof(AccountNature))
        //                             .Cast<AccountNature>()
        //                             .Select(x => new { Value = x, DisplayName = x.ToString() })
        //                             .ToList();

        //    ViewBag.Taxes = Taxes;
        //    ViewBag.Accounts = Accounts;
        //    ViewBag.AccountNatures = accountNatures;

        //    return View();
        //}

        #endregion

        public async Task<IActionResult> Create()
        {            
            var Accounts = await _dbContext.Account
                .Where(
                    e => !e.Trashed && 
                    e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                .Select(a => new 
                    { 
                        a.AccountsId, 
                        a.Name, 
                        Currency = a.Currencies.ToString(),
                        Bank = a.IsBank,
                        Vendor = a.IsVendorAccount,
                        Hajji = a.IsHajjiAccount,
                        Expense = a.IsExpenseAccount
                    })
                .ToListAsync();

            var accountNatures = Enum.GetValues(typeof(AccountNature))
                                     .Cast<AccountNature>()
                                     .Select(x => new { Value = x, DisplayName = x.ToString() })
                                     .ToList();

            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewPaymentVoucher voucher, IFormFile image)
        {
            using var trans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                int DebitPaymentAccounId = 0;
                int CreditPaymentAccounId = 0;

                #region PaymentVoucherNumber
                
                // Generate the next Payment Voucher Number
                var lastReceipt = _dbContext.NewPaymentVouchers
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                    .OrderByDescending(x => x.NewPaymentVoucherId)
                    .Select(x => x.PaymentVoucherNo)
                    .FirstOrDefault();

                int nextPVNumber = 1;

                if (!string.IsNullOrEmpty(lastReceipt) && lastReceipt.StartsWith("PV-"))
                {
                    nextPVNumber = int.Parse(lastReceipt.Substring(3)) + 1;
                } 

                #endregion

                NewPaymentVoucher paymentVoucher = new NewPaymentVoucher()
                {
                    Date = voucher.Date,
                    CreditRemarks = voucher.CreditRemarks,
                    DebitRemarks = voucher.DebitRemarks,
                    ROE = voucher.ROE,
                    Amount = voucher.Amount,
                    InstrumentNumber = voucher.InstrumentNumber,
                    HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
                    Trashed = voucher.Trashed,
                    PaymentVoucherNo = $"PV-{nextPVNumber:D2}",
                    TotalCredit = voucher.TotalCredit,
                    TotalDebit = voucher.TotalDebit,

                    // Debit Taxes
                    DebitBankCharges = voucher.DebitBankCharges,
                    DebitOPAPTax = voucher.DebitOPAPTax,
                    DebitRemittanceTax = voucher.DebitRemittanceTax,
                    DebitThirdPartyTax = voucher.DebitThirdPartyTax,
                    DebitOtherTaxes = voucher.DebitOtherTaxes,
                    DebitBouncePanelty = voucher.DebitBouncePanelty,

                    // Credit Taxes
                    CreditBankCharges = voucher.CreditBankCharges,
                    CreditOPAPTax = voucher.CreditOPAPTax,
                    CreditRemittanceTax = voucher.CreditRemittanceTax,
                    CreditThirdPartyTax = voucher.CreditThirdPartyTax,
                    CreditOtherTaxes = voucher.CreditOtherTaxes,
                    CreditBouncePanelty = voucher.CreditBouncePanelty,
                    Status = ReceiptStatus.Holding,
                    WTH = voucher.WTH,
                    PaymentMode = voucher.PaymentMode,
                    PaymentType = PaymentType.Bank
                };

                if (image != null && image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "PaymentVoucher");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Path.GetFileName(image.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }

                    paymentVoucher.File = "/PaymentVoucher/" + fileName;
                }

                _dbContext.NewPaymentVouchers.Add(paymentVoucher);

                #region Signature

                var User = _dbContext.Users.Where(u => u.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))).FirstOrDefault();

                if (User is not null)
                {
                    paymentVoucher.CreatorSignature = User.UserSignature;
                }

                #endregion

                await _dbContext.SaveChangesAsync();

                foreach (var item in voucher.Payments)
                {
                    if (item.AccountNature == AccountNature.Credit)
                    {
                        NewPayment CreditPayment = new NewPayment()
                        {
                            AccountNature = item.AccountNature,
                            AccountsId = item.AccountsId,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId,
                        };
                        _dbContext.NewPayments.Add(CreditPayment);
                        CreditPaymentAccounId = CreditPayment.AccountsId;

                        #region Ledger Entry

                        GeneralLedger generalLedger = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                        };
                        _dbContext.GeneralLedgers.Add(generalLedger);
                        await _dbContext.SaveChangesAsync();

                        GeneralLedgerEntry CreditPaymentEntry = new GeneralLedgerEntry()
                        {
                            Description = voucher.CreditRemarks,
                            AccountNature = CreditPayment.AccountNature,
                            GeneralLedgerId = generalLedger.GeneralLedgerId,
                            Amount = voucher.TotalCredit ?? 0,
                            AccountsId = CreditPayment.AccountsId
                        };
                        _dbContext.GeneralLedgerEntries.Add(CreditPaymentEntry);
                        
                        if(voucher.CreditBankCharges > 0)
                        {
                            GeneralLedgerEntry CreditBankChargesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bank Charges",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditBankCharges ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditBankChargesEntry);
                        }
                        
                        if(voucher.CreditOPAPTax > 0)
                        {
                            GeneralLedgerEntry CreditOPAPTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "OPAP Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditOPAPTax ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditOPAPTaxEntry);
                        }
                        
                        if(voucher.CreditRemittanceTax > 0)
                        {
                            GeneralLedgerEntry CreditRemittanceTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Remittance Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditRemittanceTax ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditRemittanceTaxEntry);
                        }
                        
                        if(voucher.CreditThirdPartyTax > 0)
                        {
                            GeneralLedgerEntry CreditThirdPartyTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Third Party Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditThirdPartyTax ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditThirdPartyTaxEntry);
                        }
                        
                        if(voucher.CreditOtherTaxes > 0)
                        {
                            GeneralLedgerEntry CreditOtherTaxesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Other Taxes",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditOtherTaxes ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditOtherTaxesEntry);
                        }
                        
                        if(voucher.CreditBouncePanelty > 0)
                        {
                            GeneralLedgerEntry CreditBouncePaneltyEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bounce Panelty",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditBouncePanelty ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditBouncePaneltyEntry);
                        }

                        await _dbContext.SaveChangesAsync();

                        #endregion

                    }
                    else if (item.AccountNature == AccountNature.Debit)
                    {
                        NewPayment DebitPayment = new NewPayment()
                        {
                            AccountNature = item.AccountNature,
                            AccountsId = item.AccountsId,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                        };
                        _dbContext.NewPayments.Add(DebitPayment);
                        DebitPaymentAccounId = DebitPayment.AccountsId;

                        #region Ledger Entry

                        GeneralLedger generalLedger = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                        };
                        _dbContext.GeneralLedgers.Add(generalLedger);
                        await _dbContext.SaveChangesAsync();

                        GeneralLedgerEntry DebitPaymentEntry = new GeneralLedgerEntry()
                        {
                            Description = voucher.DebitRemarks,
                            AccountNature = DebitPayment.AccountNature,
                            GeneralLedgerId = generalLedger.GeneralLedgerId,
                            Amount = voucher.TotalDebit ?? 0,
                            AccountsId = DebitPayment.AccountsId
                        };
                        _dbContext.GeneralLedgerEntries.Add(DebitPaymentEntry);

                        if (voucher.DebitBankCharges > 0)
                        {
                            GeneralLedgerEntry DebitBankChargesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bank Charges",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitBankCharges ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitBankChargesEntry);
                        }

                        if (voucher.DebitOPAPTax > 0)
                        {
                            GeneralLedgerEntry DebitOPAPTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "OPAP Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitOPAPTax ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitOPAPTaxEntry);
                        }

                        if (voucher.DebitRemittanceTax > 0)
                        {
                            GeneralLedgerEntry DebitRemittanceTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Remittance Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitRemittanceTax ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitRemittanceTaxEntry);
                        }

                        if (voucher.DebitThirdPartyTax > 0)
                        {
                            GeneralLedgerEntry DebitThirdPartyTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Third Party Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitThirdPartyTax ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitThirdPartyTaxEntry);
                        }

                        if (voucher.DebitOtherTaxes > 0)
                        {
                            GeneralLedgerEntry DebitOtherTaxesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Other Taxes",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitOtherTaxes ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitOtherTaxesEntry);
                        }
                        
                        if (voucher.DebitBouncePanelty > 0)
                        {
                            GeneralLedgerEntry DebitBouncePaneltyEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bounce Panelty",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitBouncePanelty ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitBouncePaneltyEntry);
                        }

                        await _dbContext.SaveChangesAsync();

                        #endregion
                    }
                }

                await _dbContext.SaveChangesAsync();
                await trans.CommitAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return View(voucher);
            }
        }

        public IActionResult CreatePayroll()
        {
            var Accounts = _dbContext.Account
               .Where(
                   e => !e.Trashed &&
                   e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                   )
               .Select(a => new
               {
                   a.AccountsId,
                   a.Name,
                   Currency = a.Currencies.ToString(),
                   Bank = a.IsBank,
                   Vendor = a.IsVendorAccount,
                   Hajji = a.IsHajjiAccount,
                   Expense = a.IsExpenseAccount
               })
               .ToList();

            var accountNatures = Enum.GetValues(typeof(AccountNature))
                                     .Cast<AccountNature>()
                                     .Select(x => new { Value = x, DisplayName = x.ToString() })
                                     .ToList();

            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;

            ViewBag.Employee = _dbContext.Employees
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.Hajji = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.IsHead)
                .ToList();

            ViewBag.Vendors = _dbContext.Vendors
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.Expenses = _dbContext.ExpenseSubTypes
                .Where(e => !e.Trashed)
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayroll(JV voucher, IFormFile image, List<int?> debitId, List<int?> creditId, List<Currencies?> currencyD, List<Currencies?> currencyC)
        {

            ViewBag.Employee = _dbContext.Employees
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.Hajji = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.IsHead)
                .ToList();

            ViewBag.Vendors = _dbContext.Vendors
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.Expenses = _dbContext.ExpenseSubTypes
                .Where(e => !e.Trashed)
                .ToList();

            using var trans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                int DebitPaymentAccounId = 0;
                int CreditPaymentAccounId = 0;

                #region PaymentVoucherNumber

                // Generate the next Payment Voucher Number
                var lastReceipt = _dbContext.JVs
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                    .OrderByDescending(x => x.JVId)
                    .Select(x => x.PaymentVoucherNo)
                    .FirstOrDefault();

                int nextPVNumber = 1;

                if (!string.IsNullOrEmpty(lastReceipt) && lastReceipt.StartsWith("PV-"))
                {
                    nextPVNumber = int.Parse(lastReceipt.Substring(3)) + 1;
                }

                #endregion

                JV paymentVoucher = new JV()
                {
                    Date = voucher.Date,
                    CreditRemarks = voucher.CreditRemarks,
                    DebitRemarks = voucher.DebitRemarks,
                    ROE = voucher.ROE,
                    Amount = voucher.Amount,
                    InstrumentNumber = voucher.InstrumentNumber,
                    HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
                    Trashed = voucher.Trashed,
                    PaymentVoucherNo = $"JV-{nextPVNumber}",
                    TotalCredit = voucher.TotalCredit,
                    TotalDebit = voucher.TotalDebit,

                    // Debit Taxes
                    DebitBankCharges = voucher.DebitBankCharges,
                    DebitOtherTaxes = voucher.DebitOtherTaxes,

                    // Credit Taxes
                    Status = ReceiptStatus.Holding,
                    WTH = voucher.WTH,
                    PaymentMode = voucher.PaymentMode,
                    PaymentType = PaymentType.Payroll,
                    EmployeeId = voucher.EmployeeId,
                    CreditId = 1,
                    DebitId = 1,
                    BasicSalary = voucher.BasicSalary,
                    Fuel = voucher.Fuel,
                    Medical = voucher.Medical,
                    Leaves = voucher.Leaves,
                    Others = voucher.Others,
                    TypeDropDown = voucher.TypeDropDown,
                };

                if (image != null && image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "PaymentVoucher");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Path.GetFileName(image.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }

                    paymentVoucher.File = "/PaymentVoucher/" + fileName;
                }

                _dbContext.JVs.Add(paymentVoucher);

                #region Signature

                var User = _dbContext.Users.Where(u => u.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))).FirstOrDefault();

                if (User is not null)
                {
                    paymentVoucher.CreatorSignature = User.UserSignature;
                }

                #endregion

                await _dbContext.SaveChangesAsync();

                for (int i = 0; i < debitId.Count; i++)
                {
                    _dbContext.JVAccounts.Add(new JVAccounts
                    {
                        JVId = paymentVoucher.JVId,
                        CreditId = creditId[i],
                        DebitId = debitId[i],
                        CurrencyD = currencyD[i],
                        CurrencyC = currencyC[i]
                    });
                }

                await _dbContext.SaveChangesAsync();

                await trans.CommitAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return View(voucher);
            }
        }

        public IActionResult CreateRefund()
        {
            var Accounts = _dbContext.Account
               .Where(
                   e => !e.Trashed &&
                   e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                   )
               .Select(a => new
               {
                   a.AccountsId,
                   a.Name,
                   Currency = a.Currencies.ToString(),
                   Bank = a.IsBank,
                   Vendor = a.IsVendorAccount,
                   Hajji = a.IsHajjiAccount,
                   Expense = a.IsExpenseAccount
               })
               .ToList();

            var accountNatures = Enum.GetValues(typeof(AccountNature))
                                     .Cast<AccountNature>()
                                     .Select(x => new { Value = x, DisplayName = x.ToString() })
                                     .ToList();

            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRefund(NewPaymentVoucher voucher, IFormFile image)
        {
            using var trans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                int DebitPaymentAccounId = 0;
                int CreditPaymentAccounId = 0;

                #region PaymentVoucherNumber

                // Generate the next Payment Voucher Number
                var lastReceipt = _dbContext.NewPaymentVouchers
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                    .OrderByDescending(x => x.NewPaymentVoucherId)
                    .Select(x => x.PaymentVoucherNo)
                    .FirstOrDefault();

                int nextPVNumber = 1;

                if (!string.IsNullOrEmpty(lastReceipt) && lastReceipt.StartsWith("PV-"))
                {
                    nextPVNumber = int.Parse(lastReceipt.Substring(3)) + 1;
                }

                #endregion

                NewPaymentVoucher paymentVoucher = new NewPaymentVoucher()
                {
                    Date = voucher.Date,
                    CreditRemarks = voucher.CreditRemarks,
                    DebitRemarks = voucher.DebitRemarks,
                    ROE = voucher.ROE,
                    Amount = voucher.Amount,
                    InstrumentNumber = voucher.InstrumentNumber,
                    HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
                    Trashed = voucher.Trashed,
                    PaymentVoucherNo = $"PV-{nextPVNumber:D2}",
                    TotalCredit = voucher.TotalCredit,
                    TotalDebit = voucher.TotalDebit,

                    // Debit Taxes
                    DebitBankCharges = voucher.DebitBankCharges,
                    DebitOtherTaxes = voucher.DebitOtherTaxes,
                    SalesTax = voucher.SalesTax,
                    AdvanceTax = voucher.AdvanceTax,

                    // Credit Taxes
                    CreditBankCharges = voucher.CreditBankCharges,
                    CreditOPAPTax = voucher.CreditOPAPTax,
                    CreditRemittanceTax = voucher.CreditRemittanceTax,
                    CreditThirdPartyTax = voucher.CreditThirdPartyTax,
                    CreditOtherTaxes = voucher.CreditOtherTaxes,
                    CreditBouncePanelty = voucher.CreditBouncePanelty,
                    Status = ReceiptStatus.Holding,
                    WTH = voucher.WTH,
                    PaymentMode = voucher.PaymentMode,
                    PaymentType = PaymentType.Refund,
                };

                if (image != null && image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "PaymentVoucher");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Path.GetFileName(image.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }

                    paymentVoucher.File = "/PaymentVoucher/" + fileName;
                }

                _dbContext.NewPaymentVouchers.Add(paymentVoucher);

                #region Signature

                var User = _dbContext.Users.Where(u => u.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))).FirstOrDefault();

                if (User is not null)
                {
                    paymentVoucher.CreatorSignature = User.UserSignature;
                }

                #endregion

                await _dbContext.SaveChangesAsync();

                foreach (var item in voucher.Payments)
                {
                    if (item.AccountNature == AccountNature.Credit)
                    {
                        NewPayment CreditPayment = new NewPayment()
                        {
                            AccountNature = item.AccountNature,
                            AccountsId = item.AccountsId,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId,
                        };
                        _dbContext.NewPayments.Add(CreditPayment);
                        CreditPaymentAccounId = CreditPayment.AccountsId;

                        #region Ledger Entry

                        GeneralLedger generalLedger = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                        };
                        _dbContext.GeneralLedgers.Add(generalLedger);
                        await _dbContext.SaveChangesAsync();

                        GeneralLedgerEntry CreditPaymentEntry = new GeneralLedgerEntry()
                        {
                            Description = voucher.CreditRemarks,
                            AccountNature = CreditPayment.AccountNature,
                            GeneralLedgerId = generalLedger.GeneralLedgerId,
                            Amount = voucher.TotalCredit ?? 0,
                            AccountsId = CreditPayment.AccountsId
                        };
                        _dbContext.GeneralLedgerEntries.Add(CreditPaymentEntry);

                        if (voucher.CreditBankCharges > 0)
                        {
                            GeneralLedgerEntry CreditBankChargesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bank Charges",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditBankCharges ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditBankChargesEntry);
                        }

                        if (voucher.CreditOPAPTax > 0)
                        {
                            GeneralLedgerEntry CreditOPAPTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "OPAP Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditOPAPTax ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditOPAPTaxEntry);
                        }

                        if (voucher.CreditRemittanceTax > 0)
                        {
                            GeneralLedgerEntry CreditRemittanceTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Remittance Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditRemittanceTax ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditRemittanceTaxEntry);
                        }

                        if (voucher.CreditThirdPartyTax > 0)
                        {
                            GeneralLedgerEntry CreditThirdPartyTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Third Party Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditThirdPartyTax ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditThirdPartyTaxEntry);
                        }

                        if (voucher.CreditOtherTaxes > 0)
                        {
                            GeneralLedgerEntry CreditOtherTaxesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Other Taxes",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditOtherTaxes ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditOtherTaxesEntry);
                        }

                        if (voucher.CreditBouncePanelty > 0)
                        {
                            GeneralLedgerEntry CreditBouncePaneltyEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bounce Panelty",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.CreditBouncePanelty ?? 0,
                                AccountsId = CreditPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(CreditBouncePaneltyEntry);
                        }

                        await _dbContext.SaveChangesAsync();

                        #endregion

                    }
                    else if (item.AccountNature == AccountNature.Debit)
                    {
                        NewPayment DebitPayment = new NewPayment()
                        {
                            AccountNature = item.AccountNature,
                            AccountsId = item.AccountsId,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                        };
                        _dbContext.NewPayments.Add(DebitPayment);
                        DebitPaymentAccounId = DebitPayment.AccountsId;

                        #region Ledger Entry

                        GeneralLedger generalLedger = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                            NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                        };
                        _dbContext.GeneralLedgers.Add(generalLedger);
                        await _dbContext.SaveChangesAsync();

                        GeneralLedgerEntry DebitPaymentEntry = new GeneralLedgerEntry()
                        {
                            Description = voucher.DebitRemarks,
                            AccountNature = DebitPayment.AccountNature,
                            GeneralLedgerId = generalLedger.GeneralLedgerId,
                            Amount = voucher.TotalDebit ?? 0,
                            AccountsId = DebitPayment.AccountsId
                        };
                        _dbContext.GeneralLedgerEntries.Add(DebitPaymentEntry);

                        if (voucher.DebitBankCharges > 0)
                        {
                            GeneralLedgerEntry DebitBankChargesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bank Charges",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitBankCharges ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitBankChargesEntry);
                        }

                        if (voucher.DebitOPAPTax > 0)
                        {
                            GeneralLedgerEntry DebitOPAPTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "OPAP Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitOPAPTax ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitOPAPTaxEntry);
                        }

                        if (voucher.DebitRemittanceTax > 0)
                        {
                            GeneralLedgerEntry DebitRemittanceTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Remittance Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitRemittanceTax ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitRemittanceTaxEntry);
                        }

                        if (voucher.DebitThirdPartyTax > 0)
                        {
                            GeneralLedgerEntry DebitThirdPartyTaxEntry = new GeneralLedgerEntry()
                            {
                                Description = "Third Party Tax",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitThirdPartyTax ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitThirdPartyTaxEntry);
                        }

                        if (voucher.DebitOtherTaxes > 0)
                        {
                            GeneralLedgerEntry DebitOtherTaxesEntry = new GeneralLedgerEntry()
                            {
                                Description = "Other Taxes",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitOtherTaxes ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitOtherTaxesEntry);
                        }

                        if (voucher.DebitBouncePanelty > 0)
                        {
                            GeneralLedgerEntry DebitBouncePaneltyEntry = new GeneralLedgerEntry()
                            {
                                Description = "Bounce Panelty",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = voucher.DebitBouncePanelty ?? 0,
                                AccountsId = DebitPayment.AccountsId
                            };
                            _dbContext.GeneralLedgerEntries.Add(DebitBouncePaneltyEntry);
                        }

                        await _dbContext.SaveChangesAsync();

                        #endregion
                    }
                }

                await _dbContext.SaveChangesAsync();
                await trans.CommitAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return View(voucher);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPayroll(int id)
        {
            ViewBag.Employee = _dbContext.Employees
            .Where(e => !e.Trashed)
            .ToList();

            ViewBag.Hajji = _dbContext.FamilyHeadsWithoutTrash
           .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                   && e.IsHead)
           .ToList();

            var paymentVoucher = await _dbContext.JVs
                .FirstOrDefaultAsync(p => p.JVId == id);

            if (paymentVoucher == null)
            {
                return NotFound();
            }

            var editViewModel = new PaymentVoucherViewModel 
            {
                JVId = paymentVoucher.JVId,
                DateEdit = paymentVoucher.Date,
                CreditRemarks = paymentVoucher.CreditRemarks,
                DebitRemarks = paymentVoucher.DebitRemarks,
                ROE = paymentVoucher.ROE,
                Amount = paymentVoucher.Amount,
                InstrumentNumber = paymentVoucher.InstrumentNumber,
                CreatorSignature = paymentVoucher.CreatorSignature,
                FinanceOfficerSignature = paymentVoucher.FinanceOfficerSignature,
                AuthorizingAgentSignature = paymentVoucher.AuthorizingAgentSignature,
                CEOSignature = paymentVoucher.CEOSignature,
                DebitBankCharges = paymentVoucher.DebitBankCharges,
                IsIBFT = paymentVoucher.PaymentMode,
                //CreditId = paymentVoucher.CreditId,
                //DebitId = paymentVoucher.DebitId
            };

            return View(editViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditPayroll(PaymentVoucherViewModel voucher, IFormFile image)
        {
            ViewBag.AccountsJV = _dbContext.JVAccounts
                .Where(e => e.JVId == voucher.JVId)
                .ToList();

            using var trans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                #region Deleting & Updating Data

                var existingVoucher = await _dbContext.JVs
                .FirstOrDefaultAsync(p => p.JVId == voucher.JVId);

                if (existingVoucher == null)
                {
                    return NotFound();
                }

                // Update main voucher details
                existingVoucher.Date = voucher.DateEdit;
                existingVoucher.CreditRemarks = voucher.CreditRemarks;
                existingVoucher.DebitRemarks = voucher.DebitRemarks;
                existingVoucher.ROE = voucher.ROE;
                existingVoucher.Amount = voucher.Amount;
                existingVoucher.InstrumentNumber = voucher.InstrumentNumber;
                existingVoucher.TotalCredit = voucher.TotalCredit;
                existingVoucher.TotalDebit = voucher.TotalDebit;
                existingVoucher.DebitBankCharges = voucher.DebitBankCharges;
                existingVoucher.DebitOPAPTax = voucher.DebitOPAPTax;
                existingVoucher.DebitRemittanceTax = voucher.DebitRemittanceTax;
                existingVoucher.DebitThirdPartyTax = voucher.DebitThirdPartyTax;
                existingVoucher.DebitOtherTaxes = voucher.DebitOtherTaxes;
                existingVoucher.CreditBankCharges = voucher.CreditBankCharges;
                existingVoucher.CreditOPAPTax = voucher.CreditOPAPTax;
                existingVoucher.CreditRemittanceTax = voucher.CreditRemittanceTax;
                existingVoucher.CreditThirdPartyTax = voucher.CreditThirdPartyTax;
                existingVoucher.CreditOtherTaxes = voucher.CreditOtherTaxes;
                existingVoucher.PaymentMode = voucher.IsIBFT;
                existingVoucher.PORefNo = voucher.PORefNo;
                existingVoucher.BasicSalary = voucher.BasicSalary;
                existingVoucher.Fuel = voucher.Fuel;
                existingVoucher.Medical = voucher.Medical;
                existingVoucher.Others = voucher.Others;
                existingVoucher.Leaves = voucher.Leaves;
                existingVoucher.TypeDropDown = voucher.DropDown;
                //existingVoucher.CreditId = voucher.CreditId;
                //existingVoucher.DebitId = voucher.DebitId;

                if (image != null && image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "PaymentVoucher");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Path.GetFileName(image.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }

                    existingVoucher.File = "/PaymentVoucher/" + fileName;
                }

                #endregion

                int DebitPaymentAccountId = 0;
                int CreditPaymentAccountId = 0;

                await _dbContext.SaveChangesAsync();
                await trans.CommitAsync();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                return View(voucher);
            }
        }

        public async Task<IActionResult> Trash(int id)
        {
            try
            {
                var paymentVoucher = await _dbContext.JVs
                    .FirstOrDefaultAsync(p => p.JVId == id);

                if (paymentVoucher == null)
                {
                    return Json(new { success = false, message = "Payment voucher not found" });
                }

                paymentVoucher.Trashed = true;

                //var relatedLedgers = await _dbContext.GeneralLedgers
                //    .Where(g => g.NewPaymentVoucherId == id)
                //    .ToListAsync();

                //// Update trashed status for related ledgers
                //foreach (var ledger in relatedLedgers)
                //{
                //    ledger.Trashed = true;
                //}

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ViewTrashed()
        {
            var PV = await _dbContext.JVs
                .Where(p => p.Trashed && p.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            return View(PV);
        }

        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var paymentVoucher = await _dbContext.JVs
                    .FirstOrDefaultAsync(p => p.JVId == id);

                if (paymentVoucher == null)
                {
                    return Json(new { success = false, message = "Payment voucher not found" });
                }

                paymentVoucher.Trashed = false;

                //// Get related general ledgers
                //var relatedLedgers = await _dbContext.GeneralLedgers
                //    .Where(g => g.NewPaymentVoucherId == id)
                //    .ToListAsync();

                //// Update trashed status for related ledgers
                //foreach (var ledger in relatedLedgers)
                //{
                //    ledger.Trashed = false;
                //}

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Print(int id)
        {
            int? CompanyId = 0;

            var paymentVoucher = await _dbContext.NewPaymentVouchers.Where(v => v.NewPaymentVoucherId == id).FirstOrDefaultAsync();
            var payments = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.NewPaymentVoucherId == id).ToListAsync();

            var DebitAccount = payments.Where(p => p.AccountNature == AccountNature.Debit).FirstOrDefault();
            var CreditAccount = payments.Where(p => p.AccountNature == AccountNature.Credit).FirstOrDefault();

            var CreditPayment = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.AccountNature == AccountNature.Credit).FirstOrDefaultAsync();
            var DebitPayment = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.AccountNature == AccountNature.Debit).FirstOrDefaultAsync();

            if (CreditPayment != null)
            {
                CompanyId = CreditPayment.Accounts.CompanyId;
            }
            var companies = await _dbContext.Companies.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
            var companyAdresses = await _dbContext.CompanyAddresses.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
            var companyEmails = await _dbContext.CompanyEmails.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
            var companyContacts = await _dbContext.CompanyContacts.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();

            var viewModel = new PaymentVoucherViewModel()
            {
                PaymentVoucherNo = paymentVoucher.PaymentVoucherNo ?? "",
                CreditRemarks = paymentVoucher.CreditRemarks ?? "",
                DebitRemarks = paymentVoucher.DebitRemarks ?? "",
                PrintDate = DateTime.Now.ToString("dd/MMM/yyyy") ?? "",
                PrintedBy = _http.HttpContext.Session.GetString("Username") ?? "",
                ROE = paymentVoucher.ROE ?? 0,
                InstrumentNumber = paymentVoucher.InstrumentNumber ?? "",
                CreatorSignature = paymentVoucher.CreatorSignature ?? "",
                FinanceOfficerSignature = paymentVoucher.FinanceOfficerSignature ?? "",
                AuthorizingAgentSignature = paymentVoucher.AuthorizingAgentSignature ?? "",
                CEOSignature = paymentVoucher.CEOSignature ?? "",
                Date = paymentVoucher.Date.ToString("dd/MMM/yyyy"),
                Payments = payments,
                CompanyAddress = companyAdresses.Address ?? "",
                CompanyEmail = companyEmails.CompanyEmail ?? "",
                CompanyContact = companyContacts.CompanyContact ?? "",
                CompanyLogo = companies.Logo ?? "",
                CompanyName = companies.CompanyName ?? "",
                CompanySite = companies.Website ?? "",
                TotalCredit = paymentVoucher.TotalCredit ?? 0,
                TotalDebit = paymentVoucher.TotalDebit ?? 0,
                DebitBankCharges = paymentVoucher.DebitBankCharges,
                DebitOPAPTax = paymentVoucher.DebitOPAPTax,
                DebitRemittanceTax = paymentVoucher.DebitRemittanceTax,
                DebitThirdPartyTax = paymentVoucher.DebitThirdPartyTax,
                DebitOtherTaxes = paymentVoucher.DebitOtherTaxes,
                DebitBouncePanelty = paymentVoucher.DebitBouncePanelty,
                CreditBankCharges = paymentVoucher.CreditBankCharges,
                CreditOPAPTax = paymentVoucher.CreditOPAPTax,
                CreditRemittanceTax = paymentVoucher.CreditRemittanceTax,
                CreditThirdPartyTax = paymentVoucher.CreditThirdPartyTax,
                CreditOtherTaxes = paymentVoucher.CreditOtherTaxes,
                CreditBouncePanelty = paymentVoucher.CreditBouncePanelty,
                IsDebitBank = DebitAccount.Accounts.IsBank == true ? true : false,
                IsCreditBank = CreditAccount.Accounts.IsBank == true ? true : false,
                WTH = paymentVoucher.WTH,
                IsIBFT = paymentVoucher.PaymentMode
            };

            return View(viewModel);
        }

        public async Task<IActionResult> PrintPayroll(int id)
        {
            int? CompanyId = 0;

            var paymentVoucher = await _dbContext.JVs
                .Include(e => e.Credit)
                .Include(e => e.Debit)
                .Where(v => v.JVId == id).FirstOrDefaultAsync();

            var payments = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.NewPaymentVoucherId == id).ToListAsync();

            var DebitAccount = payments.Where(p => p.AccountNature == AccountNature.Debit).FirstOrDefault();
            var CreditAccount = payments.Where(p => p.AccountNature == AccountNature.Credit).FirstOrDefault();

            var CreditPayment = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.AccountNature == AccountNature.Credit).FirstOrDefaultAsync();
            var DebitPayment = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.AccountNature == AccountNature.Debit).FirstOrDefaultAsync();

            if (CreditPayment != null)
            {
                CompanyId = CreditPayment.Accounts.CompanyId;
            }
            var companies = await _dbContext.Companies.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
            var companyAdresses = await _dbContext.CompanyAddresses.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
            var companyEmails = await _dbContext.CompanyEmails.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
            var companyContacts = await _dbContext.CompanyContacts.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();

            var viewModel = new PaymentVoucherViewModel()
            {
                PaymentVoucherNo = paymentVoucher.PaymentVoucherNo ?? "",
                CreditRemarks = paymentVoucher.CreditRemarks ?? "",
                DebitRemarks = paymentVoucher.DebitRemarks ?? "",
                PrintDate = DateTime.Now.ToString("dd/MMM/yyyy") ?? "",
                PrintedBy = _http.HttpContext.Session.GetString("Username") ?? "",
                ROE = paymentVoucher.ROE ?? 0,
                InstrumentNumber = paymentVoucher.InstrumentNumber ?? "",
                Date = paymentVoucher.Date.ToString("dd/MMM/yyyy"),
                TotalCredit = paymentVoucher.TotalCredit ?? 0,
                TotalDebit = paymentVoucher.TotalDebit ?? 0,
                DebitBankCharges = paymentVoucher.DebitBankCharges,
                //IsDebitBank = DebitAccount.Accounts.IsBank == true ? true : false,
                //IsCreditBank = CreditAccount.Accounts.IsBank == true ? true : false,
                WTH = paymentVoucher.WTH,
                IsIBFT = paymentVoucher.PaymentMode,
                PORefNo = paymentVoucher.PORefNo,
                //CreditId = paymentVoucher.CreditId,
                CreditName = paymentVoucher.Credit.FullName,
                //DebitId = paymentVoucher.DebitId,
                DebitName = paymentVoucher.Debit.FullName
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddSignature(int id, string signatureType)
        {
            try
            {
                var PV = _dbContext.NewPaymentVouchers.Find(id);
                if (PV == null)
                {
                    return Json(new { success = false, msg = "Payment voucher not found" });
                }

                var userId = Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"));
                var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);

                if (user == null)
                {
                    return Json(new { success = false, msg = "User not found" });
                }

                switch (signatureType)
                {
                    case "Finance":
                        PV.FinanceOfficerSignature = user.UserSignature;
                        break;
                    case "CEO":
                        PV.CEOSignature = user.UserSignature;
                        break;
                    case "Authorizor":
                        PV.AuthorizingAgentSignature = user.UserSignature;
                        break;
                    default:
                        return Json(new { success = false, msg = "Invalid signature type" });
                }

                _dbContext.SaveChanges();
                return Json(new { success = true, msg = "Signature added successfully" });
            }
            catch (Exception)
            {
                return Json(new { success = false, msg = "An error occurred" });
            }
        }

        public IActionResult ChangeStatus(int id)
        {
            return View(_dbContext.NewPaymentVouchers.Find(id));
        }

        [HttpPost]
        public IActionResult ChangeStatus(NewPaymentVoucher invoice)
        {
            NewPaymentVoucher editInvoice = _dbContext.NewPaymentVouchers
                .Find(invoice.NewPaymentVoucherId);

            editInvoice.Status = invoice.Status;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public JsonResult BasicSalary(int id)
        {
            var basicSalary = _dbContext.Employees
                .Where(e => e.EmployeeId == id)
                .Select(e => e.BasicSalary)
                .FirstOrDefault();

            return Json(basicSalary);
        }

        public IActionResult GetCurrency(int id, string type)
        {
            string currency = "PKR"; 

            switch (type)
            {
                case "Hajji":
                    var hajji = _dbContext.FamilyHeads.FirstOrDefault(h => h.FamilyHeadId == id);
                    currency = hajji?.Currencies.ToString() ?? "PKR";
                    break;

                case "Vendor":
                    var vendor = _dbContext.Vendors.FirstOrDefault(v => v.VendorId == id);
                    currency = vendor?.Currencies.ToString() ?? "PKR";
                    break;

                case "Expense":
                    var expense = _dbContext.ExpenseSubTypes.FirstOrDefault(e => e.ExpenseSubTypeId == id);
                    currency = expense?.Currency.ToString() ?? "PKR";
                    break;
            }

            return Json(currency);
        }

    }
}
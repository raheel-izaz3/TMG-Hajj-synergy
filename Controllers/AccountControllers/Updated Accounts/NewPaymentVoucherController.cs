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
using TMG.Models.InvoiceModels;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;
using static iTextSharp.text.pdf.AcroFields;
using Currencies = TMG.Models.Currencies;

namespace TMG.Controllers.AccountControllers.Updated_Accounts
{
    public enum PaymentType
    {
        Bank, Vendor, Expense, Payroll, Refund
    }
    public class NewPaymentVoucherController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public NewPaymentVoucherController(TMGDbContext dbContext, IHttpContextAccessor contextAccessor, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _http = contextAccessor;
            _host = host;
        }

        public IActionResult Index()
        {
            //ViewBag.TrashCount = _dbContext.NewPaymentVouchers.Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();
            ViewBag.TrashCount = _dbContext.NewPaymentVouchers
            .Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();

            var PV = _dbContext.NewPaymentVouchers
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
                    e => !e.Trashed
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

        public IActionResult CreateVendor()
        {
            var Accounts = _dbContext.Account
               .Where(
                   e => !e.Trashed
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
        public async Task<IActionResult> CreateVendor(NewPaymentVoucher voucher, IFormFile image)
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
                    PaymentType = PaymentType.Vendor,
                    PORefNo = voucher.PORefNo
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

        public IActionResult CreateExpense()
        {
            var Accounts = _dbContext.Account
               .Where(
                   e => !e.Trashed
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
        public async Task<IActionResult> CreateExpense(NewPaymentVoucher voucher, IFormFile image)
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
                    PaymentType = PaymentType.Expense,
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

        public IActionResult CreatePayroll()
        {
            var Accounts = _dbContext.Account
               .Where(
                   e => !e.Trashed 
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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayroll(NewPaymentVoucher voucher, IFormFile image)
        {

            ViewBag.Employee = _dbContext.Employees
                .Where(e => !e.Trashed)
                .ToList();

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
                    PaymentVoucherNo = $"PV-{nextPVNumber}",
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
                    //else if (item.AccountNature == AccountNature.Debit)
                    //{
                    //    NewPayment DebitPayment = new NewPayment()
                    //    {
                    //        AccountNature = item.AccountNature,
                    //        AccountsId = item.AccountsId,
                    //        NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                    //    };
                    //    _dbContext.NewPayments.Add(DebitPayment);
                    //    DebitPaymentAccounId = DebitPayment.AccountsId;

                    //    #region Ledger Entry

                    //    GeneralLedger generalLedger = new()
                    //    {
                    //        Date = DateTime.Now,
                    //        Trashed = false,
                    //        CreatedAt = DateTime.Now,
                    //        NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId
                    //    };
                    //    _dbContext.GeneralLedgers.Add(generalLedger);
                    //    await _dbContext.SaveChangesAsync();

                    //    GeneralLedgerEntry DebitPaymentEntry = new GeneralLedgerEntry()
                    //    {
                    //        Description = voucher.DebitRemarks,
                    //        AccountNature = DebitPayment.AccountNature,
                    //        GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //        Amount = voucher.TotalDebit ?? 0,
                    //        AccountsId = DebitPayment.AccountsId
                    //    };
                    //    _dbContext.GeneralLedgerEntries.Add(DebitPaymentEntry);

                    //    if (voucher.DebitBankCharges > 0)
                    //    {
                    //        GeneralLedgerEntry DebitBankChargesEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Bank Charges",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitBankCharges ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitBankChargesEntry);
                    //    }

                    //    if (voucher.DebitOPAPTax > 0)
                    //    {
                    //        GeneralLedgerEntry DebitOPAPTaxEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "OPAP Tax",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitOPAPTax ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitOPAPTaxEntry);
                    //    }

                    //    if (voucher.DebitRemittanceTax > 0)
                    //    {
                    //        GeneralLedgerEntry DebitRemittanceTaxEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Remittance Tax",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitRemittanceTax ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitRemittanceTaxEntry);
                    //    }

                    //    if (voucher.DebitThirdPartyTax > 0)
                    //    {
                    //        GeneralLedgerEntry DebitThirdPartyTaxEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Third Party Tax",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitThirdPartyTax ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitThirdPartyTaxEntry);
                    //    }

                    //    if (voucher.DebitOtherTaxes > 0)
                    //    {
                    //        GeneralLedgerEntry DebitOtherTaxesEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Other Taxes",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitOtherTaxes ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitOtherTaxesEntry);
                    //    }

                    //    if (voucher.DebitBouncePanelty > 0)
                    //    {
                    //        GeneralLedgerEntry DebitBouncePaneltyEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Bounce Panelty",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitBouncePanelty ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitBouncePaneltyEntry);
                    //    }

                    //    await _dbContext.SaveChangesAsync();

                    //    #endregion
                    //}
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
        public async Task<IActionResult> Edit(int id)
        {
            var Taxes = await _dbContext.Taxes.Where(t => !t.Trashed && t.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToListAsync();
            
            var Accounts = await _dbContext.Account
                .Where(
                    e => !e.Trashed
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

            ViewBag.Taxes = Taxes;
            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;


            var paymentVoucher = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .Include(t => t.Taxes)
                .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == id);

            var payments = await _dbContext.NewPayments
                .Include(p => p.Accounts)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();
            
            var taxes = await _dbContext.NewTaxes
                .Include(p => p.Taxes)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            if (paymentVoucher == null)
            {
                return NotFound();
            }

            var editViewModel = new PaymentVoucherViewModel
            {
                NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId,
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
                Payments = payments,
                Taxes = taxes,
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
                IsIBFT = paymentVoucher.PaymentMode
            };

            return View(editViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditVendor(int id)
        {
            var Taxes = await _dbContext.Taxes.Where(t => !t.Trashed && t.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToListAsync();

            var Accounts = await _dbContext.Account
                .Where(
                    e => !e.Trashed
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

            ViewBag.Taxes = Taxes;
            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;


            var paymentVoucher = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .Include(t => t.Taxes)
                .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == id);

            var payments = await _dbContext.NewPayments
                .Include(p => p.Accounts)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            var taxes = await _dbContext.NewTaxes
                .Include(p => p.Taxes)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            if (paymentVoucher == null)
            {
                return NotFound();
            }

            var editViewModel = new PaymentVoucherViewModel
            {
                NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId,
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
                Payments = payments,
                Taxes = taxes,
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
                IsIBFT = paymentVoucher.PaymentMode,
                PORefNo = paymentVoucher.PORefNo
            };

            return View(editViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditExpense(int id)
        {
            var Taxes = await _dbContext.Taxes.Where(t => !t.Trashed && t.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToListAsync();

            var Accounts = await _dbContext.Account
                .Where(
                    e => !e.Trashed
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

            ViewBag.Taxes = Taxes;
            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;


            var paymentVoucher = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .Include(t => t.Taxes)
                .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == id);

            var payments = await _dbContext.NewPayments
                .Include(p => p.Accounts)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            var taxes = await _dbContext.NewTaxes
                .Include(p => p.Taxes)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            if (paymentVoucher == null)
            {
                return NotFound();
            }

            var editViewModel = new PaymentVoucherViewModel
            {
                NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId,
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
                Payments = payments,
                Taxes = taxes,
                DebitBankCharges = paymentVoucher.DebitBankCharges,
                DebitOtherTaxes = paymentVoucher.DebitOtherTaxes,
                SalesTax = paymentVoucher.SalesTax,
                AdvanceTax = paymentVoucher.AdvanceTax,
                DebitBouncePanelty = paymentVoucher.DebitBouncePanelty,
                CreditBankCharges = paymentVoucher.CreditBankCharges,
                CreditOPAPTax = paymentVoucher.CreditOPAPTax,
                CreditRemittanceTax = paymentVoucher.CreditRemittanceTax,
                CreditThirdPartyTax = paymentVoucher.CreditThirdPartyTax,
                CreditOtherTaxes = paymentVoucher.CreditOtherTaxes,
                CreditBouncePanelty = paymentVoucher.CreditBouncePanelty,
                IsIBFT = paymentVoucher.PaymentMode,
            };

            return View(editViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditPayroll(int id)
        {
            var Taxes = await _dbContext.Taxes.Where(t => !t.Trashed && t.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToListAsync();

            var Accounts = await _dbContext.Account
                .Where(
                    e => !e.Trashed 
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

            ViewBag.Taxes = Taxes;
            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;

            ViewBag.Employee = _dbContext.Employees
            .Where(e => !e.Trashed)
            .ToList();


            var paymentVoucher = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .Include(t => t.Taxes)
                .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == id);

            var payments = await _dbContext.NewPayments
                .Include(p => p.Accounts)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            var taxes = await _dbContext.NewTaxes
                .Include(p => p.Taxes)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            if (paymentVoucher == null)
            {
                return NotFound();
            }

            var editViewModel = new PaymentVoucherViewModel
            {
                NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId,
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
                Payments = payments,
                Taxes = taxes,
                DebitBankCharges = paymentVoucher.DebitBankCharges,
                DebitOtherTaxes = paymentVoucher.DebitOtherTaxes,
                SalesTax = paymentVoucher.SalesTax,
                AdvanceTax = paymentVoucher.AdvanceTax,
                DebitBouncePanelty = paymentVoucher.DebitBouncePanelty,
                CreditBankCharges = paymentVoucher.CreditBankCharges,
                CreditOPAPTax = paymentVoucher.CreditOPAPTax,
                CreditRemittanceTax = paymentVoucher.CreditRemittanceTax,
                CreditThirdPartyTax = paymentVoucher.CreditThirdPartyTax,
                CreditOtherTaxes = paymentVoucher.CreditOtherTaxes,
                CreditBouncePanelty = paymentVoucher.CreditBouncePanelty,
                IsIBFT = paymentVoucher.PaymentMode,
                EmployeeId = paymentVoucher.EmployeeId,
                BasicSalary = paymentVoucher.BasicSalary,
                Fuel = paymentVoucher.Fuel,
                Medical = paymentVoucher.Medical,
                Others = paymentVoucher.Others,
                Leaves = paymentVoucher.Leaves
            };

            return View(editViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditRefund(int id)
        {
            var Taxes = await _dbContext.Taxes.Where(t => !t.Trashed && t.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToListAsync();

            var Accounts = await _dbContext.Account
                .Where(
                    e => !e.Trashed 
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

            ViewBag.Taxes = Taxes;
            ViewBag.Accounts = Accounts;
            ViewBag.AccountNatures = accountNatures;


            var paymentVoucher = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .Include(t => t.Taxes)
                .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == id);

            var payments = await _dbContext.NewPayments
                .Include(p => p.Accounts)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            var taxes = await _dbContext.NewTaxes
                .Include(p => p.Taxes)
                .Where(p => p.NewPaymentVoucherId == id)
                .ToListAsync();

            if (paymentVoucher == null)
            {
                return NotFound();
            }

            var editViewModel = new PaymentVoucherViewModel
            {
                NewPaymentVoucherId = paymentVoucher.NewPaymentVoucherId,
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
                Payments = payments,
                Taxes = taxes,
                DebitBankCharges = paymentVoucher.DebitBankCharges,
                DebitOtherTaxes = paymentVoucher.DebitOtherTaxes,
                SalesTax = paymentVoucher.SalesTax,
                AdvanceTax = paymentVoucher.AdvanceTax,
                DebitBouncePanelty = paymentVoucher.DebitBouncePanelty,
                CreditBankCharges = paymentVoucher.CreditBankCharges,
                CreditOPAPTax = paymentVoucher.CreditOPAPTax,
                CreditRemittanceTax = paymentVoucher.CreditRemittanceTax,
                CreditThirdPartyTax = paymentVoucher.CreditThirdPartyTax,
                CreditOtherTaxes = paymentVoucher.CreditOtherTaxes,
                CreditBouncePanelty = paymentVoucher.CreditBouncePanelty,
                IsIBFT = paymentVoucher.PaymentMode,
            };

            return View(editViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PaymentVoucherViewModel voucher, IFormFile image)
        {
            using var trans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                #region Deleting & Updating Data
                
                var existingVoucher = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .Include(t => t.Taxes)
                .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == voucher.NewPaymentVoucherId);

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
                existingVoucher.EmployeeId = voucher.EmployeeId;
                existingVoucher.BasicSalary = voucher.BasicSalary;
                existingVoucher.Fuel = voucher.Fuel;
                existingVoucher.Medical = voucher.Medical;
                existingVoucher.Others = voucher.Others;
                existingVoucher.Leaves = voucher.Leaves;

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

                // First fetch General Ledgers
                var existingLedgers = await _dbContext.GeneralLedgers
                    .Where(g => g.NewPaymentVoucherId == voucher.NewPaymentVoucherId)
                    .ToListAsync();

                // Get all GeneralLedgerIds
                var ledgerIds = existingLedgers.Select(l => l.GeneralLedgerId).ToList();

                // Fetch General Ledger Entries using the ledger IDs
                var existingLedgerEntries = await _dbContext.GeneralLedgerEntries
                    .Where(e => ledgerIds.Contains(e.GeneralLedgerId))
                    .ToListAsync();

                // First remove ledger entries
                _dbContext.GeneralLedgerEntries.RemoveRange(existingLedgerEntries);
                await _dbContext.SaveChangesAsync();

                // Then remove ledgers
                _dbContext.GeneralLedgers.RemoveRange(existingLedgers);
                await _dbContext.SaveChangesAsync();

                // Then remove the Payments
                _dbContext.NewPayments.RemoveRange(existingVoucher.Payments);
                await _dbContext.SaveChangesAsync();

                #endregion

                int DebitPaymentAccountId = 0;
                int CreditPaymentAccountId = 0;

                // Add updated payments
                foreach (var item in voucher.Payments)
                {
                    if (item.AccountNature == AccountNature.Credit)
                    {
                        NewPayment CreditPayment = new NewPayment()
                        {
                            AccountNature = item.AccountNature,
                            AccountsId = item.AccountsId,
                            NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
                        };
                        _dbContext.NewPayments.Add(CreditPayment);
                        CreditPaymentAccountId = CreditPayment.AccountsId;

                        #region Ledger Entry
                        
                        // Add Credit Ledger Entry
                        GeneralLedger generalLedger = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                            NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
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

						#endregion

					}
                    else if (item.AccountNature == AccountNature.Debit)
                    {
                        NewPayment DebitPayment = new NewPayment()
                        {
                            AccountNature = item.AccountNature,
                            AccountsId = item.AccountsId,
                            NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
                        };
                        _dbContext.NewPayments.Add(DebitPayment);
                        DebitPaymentAccountId = DebitPayment.AccountsId;

                        #region Ledger Entry
                        
                        // Add Debit Ledger Entry
                        GeneralLedger generalLedger = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                            NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
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

						#endregion

					}
                }

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

        [HttpPost]
        public async Task<IActionResult> EditPayroll(PaymentVoucherViewModel voucher, IFormFile image)
        {
            using var trans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                #region Deleting & Updating Data

                var existingVoucher = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .Include(t => t.Taxes)
                .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == voucher.NewPaymentVoucherId);

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
                existingVoucher.EmployeeId = voucher.EmployeeId;
                existingVoucher.BasicSalary = voucher.BasicSalary;
                existingVoucher.Fuel = voucher.Fuel;
                existingVoucher.Medical = voucher.Medical;
                existingVoucher.Others = voucher.Others;
                existingVoucher.Leaves = voucher.Leaves;
                existingVoucher.TypeDropDown = voucher.DropDown;

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

                // First fetch General Ledgers
                var existingLedgers = await _dbContext.GeneralLedgers
                    .Where(g => g.NewPaymentVoucherId == voucher.NewPaymentVoucherId)
                    .ToListAsync();

                // Get all GeneralLedgerIds
                var ledgerIds = existingLedgers.Select(l => l.GeneralLedgerId).ToList();

                // Fetch General Ledger Entries using the ledger IDs
                var existingLedgerEntries = await _dbContext.GeneralLedgerEntries
                    .Where(e => ledgerIds.Contains(e.GeneralLedgerId))
                    .ToListAsync();

                // First remove ledger entries
                _dbContext.GeneralLedgerEntries.RemoveRange(existingLedgerEntries);
                await _dbContext.SaveChangesAsync();

                // Then remove ledgers
                _dbContext.GeneralLedgers.RemoveRange(existingLedgers);
                await _dbContext.SaveChangesAsync();

                // Then remove the Payments
                _dbContext.NewPayments.RemoveRange(existingVoucher.Payments);
                await _dbContext.SaveChangesAsync();

                #endregion

                int DebitPaymentAccountId = 0;
                int CreditPaymentAccountId = 0;

                // Add updated payments
                foreach (var item in voucher.Payments)
                {
                    if (item.AccountNature == AccountNature.Credit)
                    {
                        NewPayment CreditPayment = new NewPayment()
                        {
                            AccountNature = item.AccountNature,
                            AccountsId = item.AccountsId,
                            NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
                        };
                        _dbContext.NewPayments.Add(CreditPayment);
                        CreditPaymentAccountId = CreditPayment.AccountsId;

                        #region Ledger Entry

                        // Add Credit Ledger Entry
                        GeneralLedger generalLedger = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                            NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
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

                        #endregion

                    }
                    //else if (item.AccountNature == AccountNature.Debit)
                    //{
                    //    NewPayment DebitPayment = new NewPayment()
                    //    {
                    //        AccountNature = item.AccountNature,
                    //        AccountsId = item.AccountsId,
                    //        NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
                    //    };
                    //    _dbContext.NewPayments.Add(DebitPayment);
                    //    DebitPaymentAccountId = DebitPayment.AccountsId;

                    //    #region Ledger Entry

                    //    // Add Debit Ledger Entry
                    //    GeneralLedger generalLedger = new()
                    //    {
                    //        Date = DateTime.Now,
                    //        Trashed = false,
                    //        CreatedAt = DateTime.Now,
                    //        NewPaymentVoucherId = existingVoucher.NewPaymentVoucherId
                    //    };
                    //    _dbContext.GeneralLedgers.Add(generalLedger);
                    //    await _dbContext.SaveChangesAsync();

                    //    GeneralLedgerEntry DebitPaymentEntry = new GeneralLedgerEntry()
                    //    {
                    //        Description = voucher.DebitRemarks,
                    //        AccountNature = DebitPayment.AccountNature,
                    //        GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //        Amount = voucher.TotalDebit ?? 0,
                    //        AccountsId = DebitPayment.AccountsId
                    //    };
                    //    _dbContext.GeneralLedgerEntries.Add(DebitPaymentEntry);

                    //    if (voucher.DebitBankCharges > 0)
                    //    {
                    //        GeneralLedgerEntry DebitBankChargesEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Bank Charges",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitBankCharges ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitBankChargesEntry);
                    //    }

                    //    if (voucher.DebitOPAPTax > 0)
                    //    {
                    //        GeneralLedgerEntry DebitOPAPTaxEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "OPAP Tax",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitOPAPTax ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitOPAPTaxEntry);
                    //    }

                    //    if (voucher.DebitRemittanceTax > 0)
                    //    {
                    //        GeneralLedgerEntry DebitRemittanceTaxEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Remittance Tax",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitRemittanceTax ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitRemittanceTaxEntry);
                    //    }

                    //    if (voucher.DebitThirdPartyTax > 0)
                    //    {
                    //        GeneralLedgerEntry DebitThirdPartyTaxEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Third Party Tax",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitThirdPartyTax ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitThirdPartyTaxEntry);
                    //    }

                    //    if (voucher.DebitOtherTaxes > 0)
                    //    {
                    //        GeneralLedgerEntry DebitOtherTaxesEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Other Taxes",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitOtherTaxes ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitOtherTaxesEntry);
                    //    }

                    //    if (voucher.DebitBouncePanelty > 0)
                    //    {
                    //        GeneralLedgerEntry DebitBouncePaneltyEntry = new GeneralLedgerEntry()
                    //        {
                    //            Description = "Bounce Panelty",
                    //            AccountNature = AccountNature.Credit,
                    //            GeneralLedgerId = generalLedger.GeneralLedgerId,
                    //            Amount = voucher.DebitBouncePanelty ?? 0,
                    //            AccountsId = DebitPayment.AccountsId
                    //        };
                    //        _dbContext.GeneralLedgerEntries.Add(DebitBouncePaneltyEntry);
                    //    }

                    //    #endregion

                    //}
                }

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
                var paymentVoucher = await _dbContext.NewPaymentVouchers
                    .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == id);

                if (paymentVoucher == null)
                {
                    return Json(new { success = false, message = "Payment voucher not found" });
                }

                paymentVoucher.Trashed = true;

                // Get related general ledgers
                var relatedLedgers = await _dbContext.GeneralLedgers
                    .Where(g => g.NewPaymentVoucherId == id)
                    .ToListAsync();

                // Update trashed status for related ledgers
                foreach (var ledger in relatedLedgers)
                {
                    ledger.Trashed = true;
                }

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
            var PV = await _dbContext.NewPaymentVouchers
                .Include(p => p.Payments)
                .ThenInclude(p => p.Accounts)
                .Where(p => p.Trashed && p.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToListAsync();

            return View(PV);
        }

        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var paymentVoucher = await _dbContext.NewPaymentVouchers
                    .FirstOrDefaultAsync(p => p.NewPaymentVoucherId == id);

                if (paymentVoucher == null)
                {
                    return Json(new { success = false, message = "Payment voucher not found" });
                }

                paymentVoucher.Trashed = false;

                // Get related general ledgers
                var relatedLedgers = await _dbContext.GeneralLedgers
                    .Where(g => g.NewPaymentVoucherId == id)
                    .ToListAsync();

                // Update trashed status for related ledgers
                foreach (var ledger in relatedLedgers)
                {
                    ledger.Trashed = false;
                }

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        #region PrintWithTaxes
        
        //public async Task<IActionResult> Print(int id)
        //{
        //    int? CompanyId = 0;
        //    var paymentVoucher = await _dbContext.NewPaymentVouchers.Where(v => v.NewPaymentVoucherId == id).FirstOrDefaultAsync();
        //    var payments = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.NewPaymentVoucherId == id).ToListAsync();

        //    var CreditPayment = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.AccountNature == AccountNature.Credit).FirstOrDefaultAsync();
        //    var DebitPayment = await _dbContext.NewPayments.Include(p => p.Accounts).Where(p => p.AccountNature == AccountNature.Debit).FirstOrDefaultAsync();

        //    if (CreditPayment != null)
        //    {
        //        CompanyId = CreditPayment.Accounts.CompanyId;
        //    }
        //    var taxes = await _dbContext.NewTaxes.Include(p => p.Taxes).Where(p => p.NewPaymentVoucherId == id).ToListAsync();
        //    var companies = await _dbContext.Companies.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
        //    var companyAdresses = await _dbContext.CompanyAddresses.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
        //    var companyEmails = await _dbContext.CompanyEmails.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();
        //    var companyContacts = await _dbContext.CompanyContacts.Where(a => a.CompanyId == CompanyId).FirstOrDefaultAsync();

        //    var viewModel = new PaymentVoucherViewModel()
        //    {
        //        PaymentVoucherNo = paymentVoucher.PaymentVoucherNo ?? "",
        //        CreditRemarks = paymentVoucher.CreditRemarks ?? "",
        //        DebitRemarks = paymentVoucher.DebitRemarks ?? "",
        //        PrintDate = DateTime.Now.ToString("dd/MMM/yyyy") ?? "",
        //        PrintedBy = _http.HttpContext.Session.GetString("Username") ?? "",
        //        ROE = paymentVoucher.ROE ?? 0,
        //        InstrumentNumber = paymentVoucher.InstrumentNumber ?? "",
        //        CreatorSignature = paymentVoucher.CreatorSignature ?? "",
        //        FinanceOfficerSignature = paymentVoucher.FinanceOfficerSignature ?? "",
        //        AuthorizingAgentSignature = paymentVoucher.AuthorizingAgentSignature ?? "",
        //        CEOSignature = paymentVoucher.CEOSignature ?? "",
        //        Date = paymentVoucher.Date.ToString("dd/MMM/yyyy"),
        //        Payments = payments,
        //        Taxes = taxes,
        //        CompanyAddress = companyAdresses.Address ?? "", 
        //        CompanyEmail = companyEmails.CompanyEmail ?? "",
        //        CompanyContact = companyContacts.CompanyContact ?? "",
        //        CompanyLogo = companies.Logo ?? "",
        //        CompanyName = companies.CompanyName ?? "",
        //        CompanySite = companies.Website ?? "",
        //        TotalCredit = paymentVoucher.TotalCredit ?? 0,
        //        TotalDebit = paymentVoucher.TotalDebit ?? 0,
        //    };

        //    return View(viewModel);
        //} 
        
        #endregion

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

        public async Task<IActionResult> PrintVendor(int id)
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
                IsIBFT = paymentVoucher.PaymentMode,
                PORefNo = paymentVoucher.PORefNo
            };

            return View(viewModel);
        }

        public async Task<IActionResult> PrintExpense(int id)
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
                SalesTax = paymentVoucher.SalesTax,
                AdvanceTax = paymentVoucher.AdvanceTax,
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
                IsIBFT = paymentVoucher.PaymentMode,
                PORefNo = paymentVoucher.PORefNo
            };

            return View(viewModel);
        }

        public async Task<IActionResult> PrintPayroll(int id)
        {
            int? CompanyId = 0;

            var paymentVoucher = await _dbContext.NewPaymentVouchers
                .Include(e => e.Employee)
                .Where(v => v.NewPaymentVoucherId == id).FirstOrDefaultAsync();
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
                SalesTax = paymentVoucher.SalesTax,
                AdvanceTax = paymentVoucher.AdvanceTax,
                DebitOtherTaxes = paymentVoucher.DebitOtherTaxes,
                DebitBouncePanelty = paymentVoucher.DebitBouncePanelty,
                CreditBankCharges = paymentVoucher.CreditBankCharges,
                CreditOPAPTax = paymentVoucher.CreditOPAPTax,
                CreditRemittanceTax = paymentVoucher.CreditRemittanceTax,
                CreditThirdPartyTax = paymentVoucher.CreditThirdPartyTax,
                CreditOtherTaxes = paymentVoucher.CreditOtherTaxes,
                CreditBouncePanelty = paymentVoucher.CreditBouncePanelty,
                //IsDebitBank = DebitAccount.Accounts.IsBank == true ? true : false,
                IsCreditBank = CreditAccount.Accounts.IsBank == true ? true : false,
                WTH = paymentVoucher.WTH,
                IsIBFT = paymentVoucher.PaymentMode,
                PORefNo = paymentVoucher.PORefNo,
                EmployeeId = paymentVoucher.EmployeeId,
                EmployeeName = paymentVoucher.Employee.Name,
                BasicSalary = paymentVoucher.BasicSalary,
                Fuel = paymentVoucher.Fuel,
                Medical = paymentVoucher.Medical,
                Others = paymentVoucher.Others,
                Leaves = paymentVoucher.Leaves
            };

            return View(viewModel);
        }

        public async Task<IActionResult> PrintRefund(int id)
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
                SalesTax = paymentVoucher.SalesTax,
                AdvanceTax = paymentVoucher.AdvanceTax,
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
                IsIBFT = paymentVoucher.PaymentMode,
                PORefNo = paymentVoucher.PORefNo
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

    }
}
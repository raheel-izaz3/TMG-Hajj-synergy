using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.PackageModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.InvoiceControllers
{
    public class NewInvoiceController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;
        private readonly IHttpContextAccessor _http;

        public NewInvoiceController(
            TMGDbContext dbContext,
            IWebHostEnvironment webEnviroment,
            IHttpContextAccessor http
        )
        {
            _dbContext = dbContext;
            _webEnviroment = webEnviroment;
            _http = http;
        }

        public ActionResult Index()
        {
            var User = _dbContext.Users
                .Where(u => 
                    u.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))
                    && (u.RoleId == 1 || u.RoleId == 3)
                )
                .FirstOrDefault();

            if (User is not null)
            {
                ViewBag.FinanceSignatureAllowed = true;
            }
            else
            {
                ViewBag.FinanceSignatureAllowed = false;
            }

            ViewBag.TrashCount = _dbContext
                .NewInvoice.Where(e =>
                    e.HajjSeasonsId
                    == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .AsNoTracking()
                .Where(e => e.Trashed)
                .Count();

            ViewBag.CreateAllowed = PrivilegeManager.CanCreateReceipt(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditReceipt(_dbContext, _http);
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashReceipt(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreReceipt(_dbContext, _http);
            ViewBag.SlipAllowed = PrivilegeManager.CanViewSlip(_dbContext, _http);
            ViewBag.ChangeStatusAllowed = PrivilegeManager.CanChangeReceiptStatus(
                _dbContext,
                _http
            );

            var invoices = _dbContext
                .NewInvoice.Where(e => !e.Trashed)
                .AsNoTracking()
                .Select(e => new
                {
                    ReceiptStatusEnum = e.ReceiptStatus,
                    ReceiptStatus = e.ReceiptStatus.ToString(),
                    e.NewInvoiceId,
                    e.FamilyHead.FullName,
                    e.FamilyHead.FcId,
                    e.FamilyHead.SPO.Name,
                    PackageName = e.FamilyHead.Package.Name,
                    PackageCode = e.FamilyHead.Package.Code,
                    e.FamilyHead.Package.Company.CompanyCode,
                    e.DepositDate,
                    e.ROE,
                    e.ClearanceDate,
                    Currencies = e.Currencies.ToString(),
                    e.AmountRecieved,
                    e.HajjSeasonsId,
                    Date = e.Date.ToString("dd/MMM/yyyy"),
                    BaseCurrency = e.FamilyHead.Currencies.ToString(),
                    e.ReceiptNumber,
                    ChequeDate = e.ChequeDate.ToString("dd/MMM/yyyy"),
                    ChequeNumber = e.ChequeNumber,
                    BankName = e.Bank.Name,
                    FinanceSignature = e.FinanceSignaturePath
                })
                .Where(e =>
                    e.HajjSeasonsId
                    == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .ToList();

            ViewBag.HoldingCount = invoices
                .Where(e => e.ReceiptStatusEnum == ReceiptStatus.Holding)
                .Count();
            ViewBag.ReturnedCount = invoices
                .Where(e => e.ReceiptStatusEnum == ReceiptStatus.Returned)
                .Count();
            ViewBag.BouncedCount = invoices
                .Where(e => e.ReceiptStatusEnum == ReceiptStatus.Bounced)
                .Count();
            ViewBag.ClearedCount = invoices
                .Where(e => e.ReceiptStatusEnum == ReceiptStatus.Cleared)
                .Count();
            try
            {
                return View(invoices);
            }
            catch (Exception e)
            {
                return Ok(invoices);
            }
        }

        public ActionResult Create()
        {
            ViewBag.FamilyHead = _dbContext.FamilyHeadsWithoutTrash.Where(e =>
                e.IsHead
                && e.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
            );
            ViewBag.Bank = _dbContext.CompanyBankAccountsWithoutTrash;
            ViewBag.Currency = _dbContext.Currencies.Where(e => !e.Trashed);
            ViewBag.Banks = _dbContext.BanksWithoutTrash;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NewInvoice newInvoice)
        {
            if (ModelState.IsValid)
            {
                if (newInvoice.File is not null)
                {
                    string path = Path.Combine(
                        _webEnviroment.WebRootPath,
                        "Images",
                        "Invoices",
                        newInvoice.File.FileName
                    );

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        newInvoice.File.CopyTo(fs);
                    }

                    newInvoice.ImagePath = @"Images/Invoices/" + newInvoice.File.FileName;
                }
                else
                {
                    newInvoice.ImageBase64 = newInvoice.ImageBase64;
                }

                var User = _dbContext.Users.Where(u => u.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))).FirstOrDefault();

                if (User is not null)
                {
                    newInvoice.CreatorSignaturePath = User.UserSignature;
                }

                newInvoice.HajjSeasonsId = int.Parse(
                    _http.HttpContext.Session.GetString("ActiveSeasonId")
                );
                newInvoice.ProformaNumber = _dbContext.NewInvoice.Count() + 1;
                newInvoice.ReceiptStatus = ReceiptStatus.Holding;

                // Fetch the last receipt number
                var lastReceipt = _dbContext
                    .NewInvoice
                    .Where(e =>
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                    .OrderByDescending(x => x.NewInvoiceId)
                    .Select(x => x.ReceiptNumber)
                    .FirstOrDefault();

                int nextReceiptNumber = 1;

                if (!string.IsNullOrEmpty(lastReceipt) && lastReceipt.StartsWith("RV-"))
                {
                    if (int.TryParse(lastReceipt.Substring(3), out int parsedNumber))
                    {
                        nextReceiptNumber = parsedNumber + 1;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid ReceiptNumber format in the database.");
                    }
                }

                newInvoice.ReceiptNumber = $"RV-{nextReceiptNumber}";


                _dbContext.NewInvoice.Add(newInvoice);
                _dbContext.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.FamilyHead = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.IsHead);
            ViewBag.Bank = _dbContext.BanksWithoutTrash;
            ViewBag.Currency = _dbContext.Currencies;

            return View(newInvoice);
        }

        [HttpGet]
        public IActionResult GetRoe(Currencies currency, string date, int FamilyId)
        {
            DateTime Today = DateTime.Parse(date);
            var RateOfExchange = _dbContext
                .RateOfExchangesWithoutTrash.Where(e => e.Currency == currency)
                .OrderByDescending(e => e.Date);

            var RoeRequestCount = _dbContext.RoeRequestsWithoutTrash.Count(e =>
                e.FamilyHeadId == FamilyId && e.Status == Status.Approved && e.Currency == currency
            );
            var RoeRequest = _dbContext.RoeRequestsWithoutTrash.Where(e =>
                e.FamilyHeadId == FamilyId && e.Status == Status.Approved && e.Currency == currency
            );

            return Ok(
                new
                {
                    currency,
                    RateOfExchange,
                    RoeRequestCount,
                    RoeRequest,
                }
            );
        }

        public IActionResult GetCurrencyForFamilyHead(int familyId)
        {
            var familyHead = _dbContext
                .FamilyHeads.Where(f => f.FamilyHeadId == familyId)
                //.Select(f => new { CurrencyName = f.Currency.CurrencyName })
                .FirstOrDefault();

            if (familyHead != null)
            {
                //return Json(new { currencyName = familyHead.CurrencyName });
            }

            return Json(null);
        }

        public IActionResult Slip(int id)
        {
            ViewBag.Invoice = _dbContext
                .NewInvoice.Join(
                    _dbContext.FamilyHeads,
                    invoice => invoice.FamilyHeadId,
                    family => family.FamilyHeadId,
                    (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                )
                .FirstOrDefault(e => e.Invoice.NewInvoiceId == id);

            return View();
        }

        public IActionResult Print(int id)
        {
            ViewBag.Invoice = _dbContext
                .NewInvoice
                //.Include(e => e.Currency)
                .Join(
                    _dbContext.FamilyHeads,
                    invoice => invoice.FamilyHeadId,
                    family => family.FamilyHeadId,
                    (invoice, family) =>
                        new
                        {
                            Invoice = invoice,
                            FamilyHead = family,
                            Currency = invoice.Currencies.ToString(),
                        }
                )
                .FirstOrDefault(e => e.Invoice.NewInvoiceId == id);

            var familyHeadId = _dbContext
                .NewInvoice.Where(e => e.NewInvoiceId == id)
                .Select(e => e.FamilyHeadId)
                .FirstOrDefault();

            var familyHead = _dbContext.FamilyHeads.FirstOrDefault(e =>
                e.FamilyHeadId == familyHeadId
            );

            ViewBag.Package = _dbContext
                .Packages.Select(e => new { e.Name, e.PackageId })
                .FirstOrDefault(e => e.PackageId == familyHead.PackageId);

            ViewBag.HeadNameWithId = $"{familyHead.FullName} ({familyHead.FcId})";

            var packageId = _dbContext
                .FamilyHeadsWithoutTrash.Where(e => e.FamilyHeadId == familyHeadId)
                .Select(e => e.PackageId)
                .FirstOrDefault();

            var companyId = _dbContext
                .PackagesWithoutTrash.Where(e => e.PackageId == packageId)
                .Select(e => e.CompanyId)
                .FirstOrDefault();

            ViewBag.Company = _dbContext
                .CompaniesWithoutTrash.Where(e => e.CompanyId == companyId)
                .FirstOrDefault();

            ViewBag.CompanyAddress = _dbContext
                .CompanyAddresses.Where(e => e.CompanyId == companyId)
                .FirstOrDefault();

            ViewBag.CompanyEmail = _dbContext
                .CompanyEmails.Where(e => e.CompanyId == companyId)
                .FirstOrDefault();

            ViewBag.CompanyContact = _dbContext
                .CompanyContacts.Where(e => e.CompanyId == companyId)
                .FirstOrDefault();

            ViewBag.spoName = _dbContext
                .FamilyHeadsWithoutTrash.Include(e => e.SPO)
                .Where(e => e.FamilyHeadId == familyHeadId)
                .Select(e => e.SPO.Name)
                .FirstOrDefault();

            return View();
        }

        public IActionResult Edit(int id)
        {
            NewInvoice invoice = _dbContext
                .NewInvoice.Include(e => e.FamilyHead)
                .FirstOrDefault(e => e.NewInvoiceId == id);

            ViewBag.FamilyHeads = _dbContext
                .FamilyHeadsWithoutTrash.Where(e =>
                    e.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.Trashed,
                    e.FamilyHeadId,
                    e.IsHead,
                })
                .Where(e => e.IsHead);

            ViewBag.CompanyBank = _dbContext.CompanyBankAccountsWithoutTrash;
            ViewBag.Banks = _dbContext.BanksWithoutTrash;
            ViewBag.Currency = _dbContext.Currencies.Where(e => !e.Trashed);

            return View(invoice);
        }

        public IActionResult UploadProof(int id)
        {
            NewInvoice invoice = _dbContext.NewInvoice.Find(id);
            return View(invoice);
        }

        [HttpPost]
        public IActionResult UploadProof(NewInvoice request)
        {
            NewInvoice invoice = _dbContext.NewInvoice.Find(request.NewInvoiceId);

            string path = Path.Combine(
                _webEnviroment.WebRootPath,
                "Images",
                "Invoices",
                request.File.FileName
            );

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                request.File.CopyTo(fs);
            }

            invoice.ImagePath = @"Images/Invoices/" + request.File.FileName;
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(NewInvoice request)
        {
            NewInvoice _invoice = _dbContext
                .NewInvoice.Include(e => e.FamilyHead)
                .FirstOrDefault(e => e.NewInvoiceId == request.NewInvoiceId);

            ViewBag.FamilyHeads = _dbContext
                .FamilyHeadsWithoutTrash.Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.Trashed,
                    e.FamilyHeadId,
                    e.IsHead,
                })
                .Where(e => e.IsHead);

            ViewBag.Bank = _dbContext.BanksWithoutTrash;
            ViewBag.Currency = _dbContext.Currencies;

            if (!ModelState.IsValid)
                return View();


            _invoice.FamilyHeadId = request.FamilyHeadId;
            _invoice.Currencies = request.Currencies;
            _invoice.AmountRecieved = request.AmountRecieved;
            _invoice.ROE = request.ROE;
            _invoice.Date = request.Date;
            _invoice.DepositDate = request.DepositDate;
            _invoice.ClearanceDate = request.ClearanceDate;
            _invoice.ModeOfPayment = request.ModeOfPayment;
            _invoice.InstrumentNo = request.InstrumentNo;
            _invoice.Remarks = request.Remarks;
            _invoice.ChequeNumber = request.ChequeNumber;
            _invoice.ChequeDate = request.ChequeDate;
            _invoice.BankId = request.BankId;
            _invoice.TPHBNo = request.TPHBNo;
            _invoice.TPHBReciptNo = request.TPHBReciptNo;
            _invoice.CompanyBankAccountId = request.CompanyBankAccountId;

            _invoice.HajjSeasonsId = Convert.ToInt32(
                _http.HttpContext.Session.GetString("ActiveSeasonId")
            );

            if (request.ImageBase64 is not null)
            {
                _invoice.ImageBase64 = request.ImageBase64;
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            ViewBag.Bank = _dbContext.BanksWithoutTrash;
            ViewBag.TrashCount = _dbContext
                .NewInvoice.Where(e =>
                    e.HajjSeasonsId
                    == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Where(e => e.Trashed)
                .Count();
            ViewBag.Invoice = _dbContext
                .NewInvoice
                //.Include(e => e.Currency)
                .Where(e =>
                    e.HajjSeasonsId
                    == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Where(e => e.Trashed)
                .Join(
                    _dbContext.FamilyHeads,
                    invoice => invoice.FamilyHeadId,
                    family => family.FamilyHeadId,
                    (invoice, family) =>
                        new
                        {
                            Invoice = invoice,
                            FamilyHead = family,
                            Currency = invoice.Currencies.ToString(),
                        }
                );

            return View();
        }

        public IActionResult Trash(int id)
        {
            NewInvoice invoice = _dbContext.NewInvoice.Find(id);
            if (invoice == null)
                return RedirectToAction(nameof(Index));

            invoice.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            NewInvoice invoice = _dbContext.NewInvoice.Find(id);
            invoice.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangeStatus(int id) => View(_dbContext.NewInvoice.Find(id));

        [HttpPost]
        public IActionResult ChangeStatus(NewInvoice invoice)
        {
            if (!ModelState.IsValid)
                return View(invoice);

            NewInvoice invoiceToEdit = _dbContext.NewInvoice.Find(invoice.NewInvoiceId);
            invoiceToEdit.ReceiptStatus = invoice.ReceiptStatus;
            invoiceToEdit.ClearanceDate = invoice.ClearanceDate;
            invoiceToEdit.DepositDate = invoice.DepositDate;

            if (invoice.ClearanceFile is not null)
            {
                string path = Path.Combine(
                    _webEnviroment.WebRootPath,
                    "Images",
                    "Invoices",
                    invoice.ClearanceFile.FileName
                );

                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    invoice.ClearanceFile.CopyTo(fs);
                }

                invoiceToEdit.ClearanceFilePath = @"Images/Invoices/" + invoice.ClearanceFile.FileName;
            }
            else
            {
                invoiceToEdit.ClearanceFileBase64 = invoice.ClearanceFileBase64;
            }

            if (invoiceToEdit.ReceiptStatus == ReceiptStatus.Cleared)
            {
                GeneralLedger generalLedger = new() 
                { 
                    Date = DateTime.Now,
                    Trashed = false,
                    CreatedAt = DateTime.Now,
                };
                _dbContext.GeneralLedgers.Add(generalLedger);
                _dbContext.SaveChanges();

                var bankAccount = _dbContext.CompanyBankAccounts.FirstOrDefault(e =>
                    e.CompanyBankAccountId == invoiceToEdit.CompanyBankAccountId
                );

                GeneralLedgerEntry debitEntry = new GeneralLedgerEntry()
                {
                    Description = invoiceToEdit.Remarks,
                    AccountNature = AccountNature.Debit,
                    GeneralLedgerId = generalLedger.GeneralLedgerId,
                    Amount = invoiceToEdit.AmountRecieved,
                    AccountsId = bankAccount.BankAssetAccountId ?? 0,
                };

                var fh = _dbContext
                    .FamilyHeads.Select(e => new
                    {
                        e.FamilyHeadId,
                        e.ReceivableAccountsId,
                        e.Currencies,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == invoiceToEdit.FamilyHeadId);

                var receivableAccount = _dbContext.Account.FirstOrDefault(e =>
                    e.AccountsId == fh.ReceivableAccountsId
                );
                float convertedAmount = AmountConverter.GetConvertedAmount(
                    invoiceToEdit.AmountRecieved,
                    invoiceToEdit.Currencies,
                    fh.Currencies ?? Currencies.SAR,
                    invoiceToEdit.ROE
                );

                GeneralLedgerEntry creditEntry = new GeneralLedgerEntry()
                {
                    Description = invoiceToEdit.Remarks,
                    AccountNature = AccountNature.Credit,
                    GeneralLedgerId = generalLedger.GeneralLedgerId,
                    Amount = convertedAmount,
                    AccountsId = receivableAccount.AccountsId,
                };

                _dbContext.GeneralLedgerEntries.Add(debitEntry);
                _dbContext.GeneralLedgerEntries.Add(creditEntry);
                _dbContext.SaveChanges();
            }

            if (invoiceToEdit.ReceiptStatus == ReceiptStatus.Bounced)
            {
                GeneralLedger generalLedger = new() 
                { 
                    Date = DateTime.Now,
                    Trashed = false,
                    CreatedAt = DateTime.Now,
                };
                _dbContext.GeneralLedgers.Add(generalLedger);
                _dbContext.SaveChanges();

                var bankAccount = _dbContext.CompanyBankAccounts.FirstOrDefault(e =>
                    e.CompanyBankAccountId == invoiceToEdit.CompanyBankAccountId
                );

                GeneralLedgerEntry BounceEntry = new GeneralLedgerEntry()
                {
                    Description =
                        $"Payment bounce charges against receipt number {invoiceToEdit.ReceiptNumber}",
                    AccountNature = AccountNature.Debit,
                    GeneralLedgerId = generalLedger.GeneralLedgerId,
                    Amount = invoice.BouncePenalty,
                    AccountsId = bankAccount.BankExpenseAccountId ?? 0,
                };

                var CompanyBankAccount = _dbContext
                    .CompanyBankAccounts.Select(e => new
                    {
                        e.CompanyBankAccountId,
                        e.BankAssetAccountId,
                    })
                    .FirstOrDefault(e =>
                        e.CompanyBankAccountId == invoiceToEdit.CompanyBankAccountId
                    );

                var receivableAccount = _dbContext.Account.FirstOrDefault(e =>
                    e.AccountsId == CompanyBankAccount.BankAssetAccountId
                );

                GeneralLedgerEntry creditEntry = new GeneralLedgerEntry()
                {
                    Description =
                        $"Payment bounce charges against receipt number {invoiceToEdit.ReceiptNumber}",
                    AccountNature = AccountNature.Credit,
                    GeneralLedgerId = generalLedger.GeneralLedgerId,
                    Amount = invoice.BouncePenalty,
                    AccountsId = receivableAccount.AccountsId,
                };

                _dbContext.GeneralLedgerEntries.Add(creditEntry);
                _dbContext.GeneralLedgerEntries.Add(BounceEntry);
                _dbContext.SaveChanges();
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult HajjPrint(int id)
        {
            ViewBag.ID = id;
            //var query = (
            //    from fh in _dbContext.FamilyHeads
            //    join inv in _dbContext.NewInvoice on fh.FamilyHeadId equals inv.FamilyHeadId
            //    join hs in _dbContext.HajjSeasons on fh.HajjSeasonsId equals hs.HajjSeasonsId
            //    join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
            //    join p in _dbContext.Packages on fh.PackageId equals p.PackageId
            //    join cb in _dbContext.CompanyBankAccounts on inv.CompanyBankAccountId equals cb.CompanyBankAccountId
            //    join c in _dbContext.Companies on cb.CompanyId equals c.CompanyId
            //    join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
            //    join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
            //    join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
            //    join ca in _dbContext.CompanyAddresses on c.CompanyId equals ca.CompanyId
            //    join cc in _dbContext.CompanyContacts on c.CompanyId equals cc.CompanyId
            //    join ce in _dbContext.CompanyEmails on c.CompanyId equals ce.CompanyId
            //    where inv.NewInvoiceId == id && !fh.Trashed && !c.Trashed && !inv.Trashed
            //    select new
            //    {
            //        EnrollmentNO = cl.CompanyLicense,
            //        PackageId = p.PackageId,
            //        fh.FamilyHeadId,
            //        inv.AmountRecieved,
            //        ReferenceNo = inv.InstrumentNo,
            //        CompanyId = c.CompanyId,

            //        // New

            //        HajjApplicationNumber = fh.HajjApplicationNumber,
            //        ApplicantName = fh.GivenName + " " + fh.SurName + " (" + fh.FcId + ")",
            //        CNIC = fh.CNIC,
            //        scheme = fh.ApplicantType,
            //        Passport = fh.PassportNumber,
            //        Season = hs.Season,
            //        Hijri = hs.Hijri,
            //        BaseCurrency = fh.Currencies,
            //        CompanyLogo = c.Logo,
            //        ReceiptDate = inv.Date,
            //        ClearnceDate = inv.ClearanceDate,
            //        ROE = inv.ROE,
            //        PackageName = p.Name,
            //        PaidCurrency = inv.Currencies,
            //        PaymentMode = inv.ModeOfPayment,
            //        MemberImage = fh.MemberImage,
            //        CompanyName = c.CompanyName,
            //        CompanyStamp = c.Stamp,
            //        CompanyAddress = ca.Address,
            //        CompanyContact = cc.CompanyContact,
            //        MunazimName = m.ClusterNumber.ToString() + "-" + m.Name,
            //        CompanyEmial = ce.CompanyEmail,
            //        CompanySite = c.Website,
            //        Remarks = inv.Remarks,
            //        ReceiptNumber = inv.ReceiptNumber,
            //        ChequeNumber = inv.ChequeNumber,
            //        ChequeDate = inv.ChequeDate.ToString("dd/MMM/yyyy"),
            //        BankName = inv.Bank.Name,
            //        UserSignature = inv.CreatorSignaturePath,
            //        FinanceSignature = inv.FinanceSignaturePath,
            //        File = inv.ImagePath,
            //    }
            //).ToList();

            var query = (
                from fh in _dbContext.FamilyHeads
                join inv in _dbContext.NewInvoice on fh.FamilyHeadId equals inv.FamilyHeadId
                join hs in _dbContext.HajjSeasons on fh.HajjSeasonsId equals hs.HajjSeasonsId
                join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                join cb in _dbContext.CompanyBankAccounts on inv.CompanyBankAccountId equals cb.CompanyBankAccountId
                join c in _dbContext.Companies on cb.CompanyId equals c.CompanyId into cGroup
                from c in cGroup.DefaultIfEmpty() // Left Join only here
                join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId into mcGroup
                from mc in mcGroup.DefaultIfEmpty()
                join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId into mGroup
                from m in mGroup.DefaultIfEmpty()
                join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId into clGroup
                from cl in clGroup.DefaultIfEmpty()
                join ca in _dbContext.CompanyAddresses on c.CompanyId equals ca.CompanyId into caGroup
                from ca in caGroup.DefaultIfEmpty()
                join cc in _dbContext.CompanyContacts on c.CompanyId equals cc.CompanyId into ccGroup
                from cc in ccGroup.DefaultIfEmpty()
                join ce in _dbContext.CompanyEmails on c.CompanyId equals ce.CompanyId into ceGroup
                from ce in ceGroup.DefaultIfEmpty()
                where inv.NewInvoiceId == id && !fh.Trashed && (c == null || !c.Trashed) && !inv.Trashed
                    && m.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                select new
                {
                    EnrollmentNO = cl.CompanyLicense,
                    PackageId = p.PackageId,
                    fh.FamilyHeadId,
                    inv.AmountRecieved,
                    ReferenceNo = inv.InstrumentNo,
                    CompanyId = c.CompanyId,

                    // New

                    HajjApplicationNumber = fh.HajjApplicationNumber,
                    ApplicantName = fh.GivenName + " " + fh.SurName + " (" + fh.FcId + ")",
                    CNIC = fh.CNIC,
                    scheme = fh.ApplicantType,
                    Passport = fh.PassportNumber,
                    Season = hs.Season,
                    Hijri = hs.Hijri,
                    BaseCurrency = fh.Currencies,
                    CompanyLogo = c.Logo,
                    ReceiptDate = inv.Date,
                    ClearnceDate = inv.ClearanceDate,
                    ROE = inv.ROE,
                    PackageName = p.Name,
                    PaidCurrency = inv.Currencies,
                    PaymentMode = inv.ModeOfPayment,
                    MemberImage = fh.MemberImage,
                    CompanyName = c.CompanyName,
                    CompanyStamp = c.Stamp,
                    CompanyAddress = ca.Address,
                    CompanyContact = cc.CompanyContact,
                    MunazimName = m.ClusterNumber.ToString() + "-" + m.Name,
                    CompanyEmial = ce.CompanyEmail,
                    CompanySite = c.Website,
                    Remarks = inv.Remarks,
                    ReceiptNumber = inv.ReceiptNumber,
                    ChequeNumber = inv.ChequeNumber,
                    ChequeDate = inv.ChequeDate,
                    BankName = inv.Bank.Name,
                    UserSignature = inv.CreatorSignaturePath,
                    FinanceSignature = inv.FinanceSignaturePath,
                    File = inv.ImagePath,
                }
            ).AsEnumerable() // Materialize the query here
            .Select(x => new
            {
                x.EnrollmentNO,
                x.PackageId,
                x.FamilyHeadId,
                x.AmountRecieved,
                x.ReferenceNo,
                CompanyId = x.CompanyId, 
                HajjApplicationNumber = x.HajjApplicationNumber,
                ApplicantName = x.ApplicantName,
                CNIC = x.CNIC,
                scheme = x.scheme,
                Passport = x.Passport,
                Season = x.Season,
                Hijri = x.Hijri,
                BaseCurrency = x.BaseCurrency,
                CompanyLogo = x.CompanyLogo ?? "default-logo.png", // Handle null safety here
                ReceiptDate = x.ReceiptDate,
                ClearnceDate = x.ClearnceDate,
                ROE = x.ROE,
                PackageName = x.PackageName,
                PaidCurrency = x.PaidCurrency,
                PaymentMode = x.PaymentMode,
                MemberImage = x.MemberImage,
                CompanyName = x.CompanyName ?? "No Company", // Handle null safety here
                CompanyStamp = x.CompanyStamp ?? "default-stamp.png", // Handle null safety here
                CompanyAddress = x.CompanyAddress,
                CompanyContact = x.CompanyContact,
                MunazimName = x.MunazimName,
                CompanyEmial = x.CompanyEmial,
                CompanySite = x.CompanySite ?? "N/A", // Handle null safety here
                Remarks = x.Remarks,
                ReceiptNumber = x.ReceiptNumber,
                ChequeNumber = x.ChequeNumber,
                ChequeDate = x.ChequeDate.ToString("dd/MMM/yyyy"),
                BankName = x.BankName,
                UserSignature = x.UserSignature,
                FinanceSignature = x.FinanceSignature,
                File = x.File,
            })
            .ToList();



            int FamilyHeadId = 0;
            int PackageId = 0;
            foreach (var item in query)
            {
                FamilyHeadId = item.FamilyHeadId;
                PackageId = item.PackageId;
                break;
            }
            var packageaccomodation = _dbContext.PackageAccomodtions.Where(e =>
                e.PackageId == PackageId && e.PlaceId == 1
            );
            var headid = _dbContext.FamilyMembers.FirstOrDefault(e =>
                e.FamilyHeadId == FamilyHeadId
            );

            var familyhead = _dbContext
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e =>
                    e.HeadId == (headid == null ? 0 : headid.HeadId) && !e.FamilyHead.Trashed
                )
                .ToList();

            var PAX = _dbContext
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e =>
                    e.HeadId == (headid == null ? 0 : headid.HeadId) && !e.FamilyHead.Trashed
                )
                .Count();

            Package _p = _dbContext
                .Packages.Include(e => e.Company)
                .FirstOrDefault(e => e.PackageId == PackageId);

            float PackageAmount = PackagePriceManager.GetOverallTotalPrice(
                _dbContext,
                FamilyHeadId
            );

            float ticketTotal = 0;
            float ticketDiffTotal = 0;
            float discount = 0;
            float packagePriceTotal = 0;

            float infantPackageUsd = 0,
                childPackageUsd = 0,
                adultPackageUsd = 0;

            foreach (var f in familyhead)
            {
                if (f.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                {
                    ticketTotal += _p.FlightTicketPriceUsd;
                }
                if (f.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                {
                    ticketTotal += f.FamilyHead.FlightTicketDifferencesUsd ?? 0;
                }

                discount += f.FamilyHead.UsdDiscountAmount;

                if (f.FamilyHead.Age < 2)
                {
                    packagePriceTotal += _p.InfantUsd;
                    infantPackageUsd = _p.InfantUsd;
                }
                else if (f.FamilyHead.Age < 12)
                {
                    packagePriceTotal += _p.ChildUsd;
                    childPackageUsd = _p.ChildUsd;
                }
                else
                {
                    packagePriceTotal += _p.AdultUsd;
                    adultPackageUsd = _p.AdultUsd;
                }
            }

            packagePriceTotal += ticketTotal;
            packagePriceTotal -= discount;

            float Remaning = PackageAmount - query[0].AmountRecieved;

            float ConvertedAmount = AmountConverter.GetConvertedAmount(query[0].AmountRecieved, (Currencies)query[0].BaseCurrency, query[0].PaidCurrency, query[0].ROE);

            HajjReceiptModel model = new HajjReceiptModel();
            if (query.Count > 0)
            {
                model.EnrollmentNo = query[0].EnrollmentNO ?? "";
                model.MakkahAccommodation = "NO";
                model.AdditionFacilities = "NO";
                model.AdditionalAmount = "0";
                model.TotalAmount = "As Per Package";
                model.AmountReceived =
                    query[0].AmountRecieved == null ? "0" : query[0].AmountRecieved.ToString();
                model.ReferenceNo =
                    query[0].ReferenceNo == null ? "" : query[0].ReferenceNo.ToString();
                model.MakkahAccommodation = packageaccomodation.Count() > 0 ? "YES" : "NO";


                // NEW

                model.HajjApplicationNumber = query[0].HajjApplicationNumber ?? "";
                model.ApplicantName = query[0].ApplicantName ?? "";
                model.Scheme = query[0].scheme == null ? "" : query[0].scheme.ToString();
                model.PerformaNumber = query[0].ReceiptNumber == null ? "" : query[0].ReceiptNumber;
                model.AccountantSign = _dbContext
                    .MisStamps.FirstOrDefault(e => e.CompanyId == query[0].CompanyId)
                    ?.AccountantSignatureFilePath;
                model.Season = query[0].Season == null ? "" : query[0].Season.ToString();
                model.Hijri = query[0].Hijri == null ? "" : query[0].Hijri.ToString();
                model.Passport = query[0].Passport == null ? "" : query[0].Passport.ToString();
                model.BaseCurrency =
                    query[0].BaseCurrency == null ? "" : query[0].BaseCurrency.ToString();
                model.ReceiptDate =
                    query[0].ReceiptDate == null
                        ? ""
                        : query[0].ReceiptDate.ToString("dd/MMM/yyyy");
                model.ClearnceDate =
                    query[0].ClearnceDate == null
                        ? ""
                        : query[0].ClearnceDate.ToString("dd/MMM/yyyy");
                model.HajjPackage = query[0].PackageName ?? "";
                model.PaidCurrency = query[0].PaidCurrency.ToString() ?? "";
                model.PaymentMode =
                    query[0].PaymentMode == null ? "" : query[0].PaymentMode.ToString();
                model.PAX = (PAX == null ? 0 : PAX);
                model.MunazamCompany = query[0].MunazimName ?? "";
                model.CompanyAddress =
                    query[0].CompanyAddress == null ? "" : query[0].CompanyAddress.ToString();
                model.CompanyContact =
                    query[0].CompanyContact == null ? "" : query[0].CompanyContact.ToString();
                model.CompanyLogo =
                    query[0].CompanyLogo == null ? "" : query[0].CompanyLogo.ToString();
                model.CompanyName =
                    query[0].CompanyName == null ? "" : query[0].CompanyName.ToString();
                model.MemberImage =
                    query[0].MemberImage == null ? "" : query[0].MemberImage.ToString();
                model.CompanyEmail =
                    query[0].CompanyEmial == null ? "" : query[0].CompanyEmial.ToString();
                model.CompanySite =
                    query[0].CompanySite == null ? "" : query[0].CompanySite.ToString();
                model.PackageAmount = PackageAmount.ToString("#,0.00");
                model.Remaining = Remaning.ToString("#,0.00");
                model.Remarks = query[0].Remarks == null ? "" : query[0].Remarks.ToString();
                model.Amount =
                    query[0].AmountRecieved == null
                        ? "0"
                        : query[0].AmountRecieved.ToString("#,0.00");
                model.CNIC = query[0].CNIC == null ? "" : query[0].CNIC.ToString();
                model.ROE = query[0].ROE == null ? "" : query[0].ROE.ToString();
                model.PrintDate = DateTime.Now.ToString("dd/MMM/yyyy");
                model.PrintedBy = _http.HttpContext.Session.GetString("Username");
                model.ROE = query[0].ROE == null ? "" : query[0].ROE.ToString();
                model.ChequeNumber = query[0].ChequeNumber == null ? "" : query[0].ChequeNumber.ToString();
                model.ChequeDate = query[0].ChequeDate == null ? "" : query[0].ChequeDate;
                model.BankName = query[0].BankName == null ? "" : query[0].BankName.ToString();
                model.CompanyStamp = query[0].CompanyStamp == null ? "" : query[0].CompanyStamp;
                model.FinanceSignature = query[0].FinanceSignature == null ? "" : query[0].FinanceSignature;
                model.UserSignature = query[0].UserSignature == null ? "" : query[0].UserSignature;
                model.File = query[0].File == null ? "" : query[0].File;
                model.ConvertedAmmount = ConvertedAmount == null ? "" : ConvertedAmount.ToString("N2");
            }

            //Proof Of Payment
            ViewBag.Invoice = _dbContext
                .NewInvoice.Join(
                    _dbContext.FamilyHeads,
                    invoice => invoice.FamilyHeadId,
                    family => family.FamilyHeadId,
                    (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                )
                .FirstOrDefault(e => e.Invoice.NewInvoiceId == id);

            return View(model);
        }

        [HttpPost]
        public IActionResult AddSignature(int id, string signatureType)
        {
            try
            {
                var invoice = _dbContext.NewInvoice.Find(id);
                if (invoice == null)
                {
                    return Json(new { success = false });
                }

                var User = _dbContext.Users.Where(u => u.UserId == Convert.ToInt32( _http.HttpContext.Session.GetString("UserId"))).FirstOrDefault();

                if (signatureType == "Finance")
                {
                    invoice.FinanceSignaturePath = User.UserSignature;
                }
                else
                {
                    return Json(new { success = false });
                }

                _dbContext.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
    }
}

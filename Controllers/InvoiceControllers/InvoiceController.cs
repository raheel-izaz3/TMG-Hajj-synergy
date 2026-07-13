using TMG.Data;
using TMG.Models;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Text;
using TMG.Models.CompanyModels;
using static System.Net.WebRequestMethods;
using TMG.ManagerClasses;
using System.Runtime.Intrinsics.X86;
using TMG.Models.PackageModels;

namespace TMG.Controllers.InvoiceControllers
{
    public class InvoiceController : Controller
    {

        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;
        private readonly IHttpContextAccessor _http;

        public InvoiceController(TMGDbContext dbContext, IWebHostEnvironment webEnviroment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webEnviroment = webEnviroment;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Invoices
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .AsNoTracking()
                .Where(e => e.Trashed)
                .Count();

            ViewBag.CreateAllowed = PrivilegeManager.CanCreateReceipt(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditReceipt(_dbContext, _http);
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashReceipt(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreReceipt(_dbContext, _http);
            ViewBag.SlipAllowed = PrivilegeManager.CanViewSlip(_dbContext, _http);
            ViewBag.ChangeStatusAllowed = PrivilegeManager.CanChangeReceiptStatus(_dbContext, _http);

            var invoices = _dbContext.InvoicesWithoutTrash
                        .AsNoTracking()
                        .Select(e => new
                        {
                            e.InvoiceStatus,
                            e.InvoiceId,
                            e.FamilyHead.FullName,
                            e.FamilyHead.FcId,
                            e.FamilyHead.SPO.Name,
                            PackageName = e.FamilyHead.Package.Name,
                            PackageCode = e.FamilyHead.Package.Code,
                            e.FamilyHead.Package.Company.CompanyCode,
                            e.DepositDate,
                            e.ClearanceDate,
                            e.Currency,
                            e.USDROE,
                            e.AmountRecived,
                            e.HajjSeasonsId,
                        })
                        .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                        .ToList();
            try
            {

                ViewBag.UnsupervisedCount = invoices.Where(e => e.InvoiceStatus == InvoiceStatus.Unsupervised).Count();
                ViewBag.SupervisedCount = invoices.Where(e => e.InvoiceStatus == InvoiceStatus.Supervised).Count();
                ViewBag.PendingCount = invoices.Where(e => e.InvoiceStatus == InvoiceStatus.Pending).Count();
                ViewBag.BouncedCount = invoices.Where(e => e.InvoiceStatus == InvoiceStatus.Bounce).Count();
                ViewBag.ClearingCount = invoices.Where(e => e.InvoiceStatus == InvoiceStatus.ClearingInTransit).Count();
                ViewBag.ClearedCount = invoices.Where(e => e.InvoiceStatus == InvoiceStatus.Cleared).Count();

                return View(invoices);
            }
            catch (Exception e)
            {
                return Ok(invoices);
            }
        }

        public IActionResult Create()
        {
            ViewBag.FamilyHead = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.IsHead);
            ViewBag.Bank = _dbContext.BanksWithoutTrash;

            ViewBag.VoucherTypes = _dbContext.VoucherTypes;
            ViewBag.Companies = _dbContext.UserCompanies.Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed)
                        .ToList();
            ViewBag.Accounts = _dbContext.AccountsWithoutTrash
                        .Include(e => e.Category)
                        .ThenInclude(e => e.SubType)
                        .ThenInclude(e => e.Type);
            return View();
        }
        
        public IActionResult Slip(int id)
        {
            ViewBag.Invoice = _dbContext.Invoices
                         .Join(
                             _dbContext.FamilyHeads,
                             invoice => invoice.FamilyHeadId,
                             family => family.FamilyHeadId,
                             (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                         )
                         .FirstOrDefault(e => e.Invoice.InvoiceId == id);


            return View();
        }
        
        public IActionResult Print(int id)
        {
            ViewBag.Invoice = _dbContext.Invoices
                         .Join(
                             _dbContext.FamilyHeads,
                             invoice => invoice.FamilyHeadId,
                             family => family.FamilyHeadId,
                             (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                         )
                         .FirstOrDefault(e => e.Invoice.InvoiceId == id);

            var familyHeadId = _dbContext.InvoicesWithoutTrash
                            .Where(e => e.InvoiceId == id)
                            .Select(e => e.FamilyHeadId)
                            .FirstOrDefault();

            var familyHead = _dbContext.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

            ViewBag.Package = _dbContext.Packages.Select(e => new
            {
                e.Name,
                e.PackageId
            }).FirstOrDefault(e => e.PackageId == familyHead.PackageId);

            ViewBag.HeadNameWithId = $"{familyHead.FullName} ({familyHead.FcId})";

            var packageId = _dbContext.FamilyHeadsWithoutTrash
                            .Where(e => e.FamilyHeadId == familyHeadId)
                            .Select(e => e.PackageId)
                            .FirstOrDefault();

            var companyId = _dbContext.PackagesWithoutTrash
                            .Where(e => e.PackageId == packageId)
                            .Select(e => e.CompanyId)
                            .FirstOrDefault();

            ViewBag.Company = _dbContext.CompaniesWithoutTrash
                                .Where(e => e.CompanyId == companyId)
                                .FirstOrDefault();

            ViewBag.CompanyAddress = _dbContext.CompanyAddresses
                                 .Where(e => e.CompanyId == companyId)
                                 .FirstOrDefault();

            ViewBag.CompanyEmail = _dbContext.CompanyEmails
                             .Where(e => e.CompanyId == companyId)
                             .FirstOrDefault();

            ViewBag.CompanyContact = _dbContext.CompanyContacts
                                .Where(e => e.CompanyId == companyId)
                                .FirstOrDefault();


            ViewBag.spoName = _dbContext.FamilyHeadsWithoutTrash
                             .Include(e => e.SPO)
                             .Where(e => e.FamilyHeadId == familyHeadId)
                             .Select(e => e.SPO.Name)
                             .FirstOrDefault();


            return View();
        }
        
        public IActionResult HajjPrint(int id)
        {

            var query = (from fh in _dbContext.FamilyHeads
                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                         join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                         join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                         join inv in _dbContext.Invoices on fh.FamilyHeadId equals inv.FamilyHeadId
                         join ca in _dbContext.CompanyAddresses on c.CompanyId equals ca.CompanyId
                         join cc in _dbContext.CompanyContacts on c.CompanyId equals cc.CompanyId
                         where inv.InvoiceId == id && !fh.Trashed && !c.Trashed && !inv.Trashed
                         select new
                         {
                             EnrollmentNO = cl.CompanyLicense,
                             MunazimName = m.ClusterNumber.ToString() + "-" + m.Name,
                             fh.HajjApplicationNumber,
                             ApplicantName = fh.GivenName + " " + fh.SurName,
                             scheme = fh.ApplicantType,
                             PackageName = p.Name,
                             PackageId = p.PackageId,
                             fh.FamilyHeadId,
                             inv.AmountRecived,
                             inv.ModeOfPayment,
                             ReferenceNo = inv.Instrument,
                             inv.Date,
                             inv.Remarks,
                             MemberImage = fh.MemberImage,
                             CompanyName = c.CompanyName,
                             CompanyAddress = ca.CompanyAddress,
                             CompanyContact = cc.CompanyContact,
                             CompanyId = c.CompanyId,
                             inv.ProformaNumber

                         }).ToList();

            int FamilyHeadId = 0;
            int PackageId = 0;
            foreach (var item in query)
            {
                FamilyHeadId = item.FamilyHeadId;
                PackageId = item.PackageId;
                break;
            }
            var packageaccomodation = _dbContext.PackageAccomodtions.Where(e => e.PackageId == PackageId && e.PlaceId == 1);
            var headid = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == FamilyHeadId);
            var familyhead = _dbContext.FamilyMembers
                        .Include(e => e.FamilyHead)
                        .Where(e => e.HeadId == (headid == null ? 0 : headid.HeadId) && !e.FamilyHead.Trashed)
                        .ToList();
            Package _p = _dbContext.Packages
                            .Include(e => e.Company)
                            .FirstOrDefault(e => e.PackageId == PackageId);


            float ticketTotal = 0;
            float ticketDiffTotal = 0;
            float discount = 0;
            float packagePriceTotal = 0;

            float infantPackageUsd = 0, childPackageUsd = 0, adultPackageUsd = 0;

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

            HajjReceiptModel model = new HajjReceiptModel();
            if (query.Count > 0)
            {
                model.EnrollmentNo = query[0].EnrollmentNO ?? "";
                model.MunazamCompany = query[0].MunazimName ?? "";
                model.HajjApplicationNumber = query[0].HajjApplicationNumber ?? "";
                model.ApplicantName = query[0].ApplicantName ?? "";
                model.Scheme = query[0].scheme == null ? "" : query[0].scheme.ToString();
                model.HajjPackage = query[0].PackageName ?? "";
                model.MakkahAccommodation = "NO";
                model.PackageAmount = "As Per Package";
                model.AdditionFacilities = "NO";
                model.AdditionalAmount = "0";
                model.TotalAmount = "As Per Package";
                model.AmountReceived = query[0].AmountRecived == null ? "0" : query[0].AmountRecived.ToString();
                model.PaymentMode = query[0].ModeOfPayment == null ? "" : query[0].ModeOfPayment.ToString();
                model.ReferenceNo = query[0].ReferenceNo == null ? "" : query[0].ReferenceNo.ToString();
                model.Amount = query[0].AmountRecived == null ? "0" : query[0].AmountRecived.ToString();
                model.Date = query[0].Date == null ? "" : query[0].Date.ToString("dd/MM/yyyy");
                model.Remarks = query[0].Remarks == null ? "" : query[0].Remarks.ToString();
                model.MemberImage = query[0].MemberImage == null ? "" : query[0].MemberImage.ToString();
                model.CompanyName = query[0].CompanyName == null ? "" : query[0].CompanyName.ToString();
                model.CompanyContact = query[0].CompanyContact == null ? "" : query[0].CompanyContact.ToString();
                model.CompanyAddress = query[0].CompanyAddress == null ? "" : query[0].CompanyAddress.ToString();
                model.MakkahAccommodation = packageaccomodation.Count() > 0 ? "YES" : "NO";
                model.CompanyStamp = _dbContext.MisStamps.FirstOrDefault(e => e.CompanyId == query[0].CompanyId)?.FilePath;
                model.ReceiptNumber = query[0].ProformaNumber;
                model.AccountantSign = _dbContext.MisStamps.FirstOrDefault(e => e.CompanyId == query[0].CompanyId)?.AccountantSignatureFilePath;
            }

            //Proof Of Payment
            ViewBag.Invoice = _dbContext.Invoices
                         .Join(
                             _dbContext.FamilyHeads,
                             invoice => invoice.FamilyHeadId,
                             family => family.FamilyHeadId,
                             (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                         )
                         .FirstOrDefault(e => e.Invoice.InvoiceId == id);

            return View(model);
        }
        
        public IActionResult AllHajjPrint(int id)
        {
            List<HajjReceiptModel> model = new();
            var query = (from fh in _dbContext.FamilyHeads
                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                         join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                         join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                         join inv in _dbContext.Invoices on fh.FamilyHeadId equals inv.FamilyHeadId
                         join ca in _dbContext.CompanyAddresses on c.CompanyId equals ca.CompanyId
                         join cc in _dbContext.CompanyContacts on c.CompanyId equals cc.CompanyId
                         where fh.FamilyHeadId == id && !fh.Trashed && !c.Trashed && !inv.Trashed
                         && inv.InvoiceStatus != InvoiceStatus.Bounce
                         select new
                         {
                             EnrollmentNO = cl.CompanyLicense,
                             MunazimName = m.ClusterNumber.ToString() + "-" + m.Name,
                             fh.HajjApplicationNumber,
                             ApplicantName = fh.GivenName + " " + fh.SurName,
                             scheme = fh.ApplicantType,
                             PackageName = p.Name,
                             PackageId = p.PackageId,
                             fh.FamilyHeadId,
                             inv.AmountRecived,
                             inv.ModeOfPayment,
                             ReferenceNo = inv.Instrument,
                             inv.Date,
                             inv.Remarks,
                             MemberImage = fh.MemberImage,
                             CompanyName = c.CompanyName,
                             CompanyAddress = ca.Address,
                             CompanyContact = cc.CompanyContact,
                             inv.ImageBase64,
                             inv.ImagePath,
                             inv.ProformaNumber

                         }).ToList();

            int FamilyHeadId = 0;
            int PackageId = 0;
            foreach (var item in query)
            {
                FamilyHeadId = item.FamilyHeadId;
                PackageId = item.PackageId;
                break;
            }
            var packageaccomodation = _dbContext.PackageAccomodtions.Where(e => e.PackageId == PackageId && e.PlaceId == 1);
            var headid = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == FamilyHeadId);
            var familyhead = _dbContext.FamilyMembers
                        .Include(e => e.FamilyHead)
                        .Where(e => e.HeadId == (headid == null ? 0 : headid.HeadId) && !e.FamilyHead.Trashed)
                        .ToList();
            Package _p = _dbContext.Packages
                            .Include(e => e.Company)
                            .FirstOrDefault(e => e.PackageId == PackageId);

            float ticketTotal = 0;
            float ticketDiffTotal = 0;
            float discount = 0;
            float packagePriceTotal = 0;

            float infantPackageUsd = 0, childPackageUsd = 0, adultPackageUsd = 0;


            foreach (var f in familyhead)
            {
                if (f.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                {
                    ticketTotal += _p.FlightTicketPriceUsd;
                    if (f.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                    {
                        ticketTotal += f.FamilyHead.FlightTicketDifferencesUsd ?? 0;
                    }
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

            foreach (var q in query)
            {
                HajjReceiptModel r = new HajjReceiptModel
                {
                    EnrollmentNo = q.EnrollmentNO ?? "",
                    MunazamCompany = q.MunazimName ?? "",
                    HajjApplicationNumber = q.HajjApplicationNumber,
                    ApplicantName = q.ApplicantName ?? "",
                    Scheme = q.scheme == null ? "" : q.scheme.ToString(),
                    HajjPackage = q.PackageName ?? "",
                    PackageAmount = "As Per Package",
                    AdditionFacilities = "NO",
                    AdditionalAmount = "0",
                    TotalAmount = "As Per Package",
                    AmountReceived = q.AmountRecived == null ? "0" : q.AmountRecived.ToString(),
                    PaymentMode = q.ModeOfPayment == null ? "" : q.ModeOfPayment.ToString(),
                    ReferenceNo = q.ReferenceNo == null ? "" : q.ReferenceNo.ToString(),
                    Amount = q.AmountRecived == null ? "0" : q.AmountRecived.ToString(),
                    Date = q.Date == null ? "" : q.Date.ToString("dd/MM/yyyy"),
                    Remarks = q.Remarks == null ? "" : q.Remarks.ToString(),
                    MemberImage = q.MemberImage == null ? "" : q.MemberImage.ToString(),
                    CompanyName = q.CompanyName == null ? "" : q.CompanyName.ToString(),
                    CompanyContact = q.CompanyContact == null ? "" : q.CompanyContact.ToString(),
                    CompanyAddress = q.CompanyAddress == null ? "" : q.CompanyAddress.ToString(),
                    MakkahAccommodation = packageaccomodation.Count() > 0 ? "YES" : "NO",
                    ImageBase64 = q.ImageBase64,
                    ImagePath = q.ImagePath,
                    ReceiptNumber = q.ProformaNumber

                };

                model.Add(r);

            }

            return View(model);
        }

        [HttpGet]
        public IActionResult GetRoe(Currencies currency, string date, int FamilyId)
        {

            DateTime Today = DateTime.Parse(date);
            var RateOfExchange = _dbContext.RateOfExchangesWithoutTrash
            .Where(e => e.Currency == currency)
            .OrderByDescending(e => e.Date);

            var RoeRequestCount = _dbContext.RoeRequestsWithoutTrash.Count(e => e.FamilyHeadId == FamilyId && e.Status == Status.Approved && e.Currency == currency);
            var RoeRequest = _dbContext.RoeRequestsWithoutTrash.Where(e => e.FamilyHeadId == FamilyId && e.Status == Status.Approved && e.Currency == currency);

            return Ok(new
            {
                currency,
                RateOfExchange,
                RoeRequestCount,
                RoeRequest
            });


        }

        [HttpPost]
        public IActionResult Create(Invoice request)
        {

            ViewBag.FamilyHead = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.IsHead);
            ViewBag.Bank = _dbContext.BanksWithoutTrash;

            ViewBag.VoucherTypes = _dbContext.VoucherTypes;
            ViewBag.Companies = _dbContext.UserCompanies.Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed)
                        .ToList();

            ViewBag.Accounts = _dbContext.AccountsWithoutTrash
            .Include(e => e.Category)
            .ThenInclude(e => e.SubType)
            .ThenInclude(e => e.Type);


            if (!ModelState.IsValid) return View();

            int CompanyId = _dbContext.FamilyHeadsWithoutTrash
                            .Include(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.PackageId == e.Package.PackageId)
                            .Select(e => e.Package.Company.CompanyId)
                            .FirstOrDefault();

            int AccountId = _dbContext.FamilyHeadsWithoutTrash
                                  .Include(e => e.Account)
                                  .Where(e => e.FamilyHeadId == request.FamilyHeadId)
                                  .Select(e => e.Account.AccountId)
                                  .FirstOrDefault();

            DateTime currentDate = DateTime.Today;
            DateTime dateTime = DateTime.Parse(currentDate.ToString());
            string formattedDate = dateTime.ToString("yyyy-MM-dd");
            DateOnly dateOnly = DateOnly.Parse(formattedDate);

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                // Generate Invoice
                float convertedAmount = 0f;
                if (request.Currency == Currencies.USD)
                {
                    convertedAmount = request.AmountRecived;
                }
                else if (request.Currency == Currencies.SAR)
                {
                    convertedAmount = request.AmountRecived / request.USDROE;
                }
                else if (request.Currency == Currencies.PKR)
                {
                    convertedAmount = request.AmountRecived / request.USDROE;
                }
                //else if (request.Currency == Currencies.AED)
                //{
                //    convertedAmount = request.AmountRecived / request.USDROE;
                //}
                //else if (request.Currency == Currencies.GBP)
                //{
                //    convertedAmount = request.AmountRecived * request.USDROE;
                //}
                //else if (request.Currency == Currencies.EURO)
                //{
                //    convertedAmount = request.AmountRecived * request.USDROE;
                //}

                string remarks = "";

                if (request.Currency == Currencies.USD)
                {
                    remarks = $"Received {request.AmountRecived.ToString("N0")} {request.Currency}";
                }
                else
                {
                    remarks = $"Received {request.AmountRecived.ToString("N0")} {request.Currency} @ USD ROE {request.USDROE} = {Math.Round(convertedAmount, 2).ToString("N0")}";
                }

                Invoice invoice = new Invoice
                {
                    FamilyHeadId = request.FamilyHeadId,
                    Currency = request.Currency,
                    AmountRecived = request.AmountRecived,
                    PKRROE = request.PKRROE,
                    SARROE = request.SARROE,
                    USDROE = request.USDROE,
                    Date = dateTime,
                    ModeOfPayment = request.ModeOfPayment,
                    InvoiceType = request.InvoiceType,
                    ClearanceDate = request.ClearanceDate,
                    DepositDate = request.DepositDate,
                    Instrument = request.Instrument,
                    InstrumentDate = request.InstrumentDate,
                    BankId = request.BankId,
                    Remarks = remarks,
                    ReceiptDate = request.ReceiptDate,
                    ProformaNumber = _dbContext.Invoices.Count() + 1,
                    HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                };

                if (request.File is not null)
                {
                    string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "Invoices", request.File.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.File.CopyTo(fs);
                    }

                    invoice.ImagePath = @"Images/Invoices/" + request.File.FileName;
                }
                else
                {
                    invoice.ImageBase64 = request.ImageBase64;
                }

                _dbContext.Invoices.Add(invoice);
                _dbContext.SaveChanges();

                // voucher.InvoiceId = invoice.InvoiceId;
                _dbContext.SaveChanges();

                transaction.Commit();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception e)
            {
                transaction.Rollback();
                return Ok(new
                {
                    e.Message
                });
                return View(request);
            }

        }

        public IActionResult Edit(int id)
        {
            Invoice invoice = _dbContext.Invoices
                                .Include(e => e.FamilyHead)
                                .FirstOrDefault(e => e.InvoiceId == id);

            ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                                    .Select(e => new
                                    {
                                        e.GivenName,
                                        e.SurName,
                                        e.Trashed,
                                        e.FamilyHeadId,
                                        e.IsHead,
                                    })
                                    .Where(e => e.IsHead);

            ViewBag.Bank = _dbContext.BanksWithoutTrash;


            return View(invoice);
        }

        [HttpPost]
        public IActionResult Edit(Invoice request)
        {
            Invoice _invoice = _dbContext.Invoices
                                        .Include(e => e.FamilyHead)
                                        .FirstOrDefault(e => e.InvoiceId == request.InvoiceId);

            ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                                    .Select(e => new
                                    {
                                        e.GivenName,
                                        e.SurName,
                                        e.Trashed,
                                        e.FamilyHeadId,
                                        e.IsHead,
                                    })
                                    .Where(e => e.IsHead);

            ViewBag.Bank = _dbContext.BanksWithoutTrash;

            if (!ModelState.IsValid) return View();

            _invoice.FamilyHeadId = request.FamilyHeadId;
            _invoice.ReceiptDate = request.ReceiptDate;
            _invoice.Currency = request.Currency;
            _invoice.AmountRecived = request.AmountRecived;
            _invoice.PKRROE = request.PKRROE;
            _invoice.SARROE = request.SARROE;
            _invoice.USDROE = request.USDROE;
            _invoice.ModeOfPayment = request.ModeOfPayment;
            _invoice.InvoiceType = request.InvoiceType;
            _invoice.InstrumentDate = request.InstrumentDate;
            _invoice.DepositDate = request.DepositDate;
            _invoice.ClearanceDate = request.ClearanceDate;
            _invoice.Instrument = request.Instrument;
            _invoice.BankId = request.BankId;
            _invoice.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            if (request.ImageBase64 is not null)
            {
                _invoice.ImageBase64 = request.ImageBase64;
            }

            float convertedAmount = 0f;
            if (request.Currency == Currencies.USD)
            {
                convertedAmount = request.AmountRecived;
            }
            else if (request.Currency == Currencies.SAR)
            {
                convertedAmount = request.AmountRecived / request.USDROE;
            }
            else if (request.Currency == Currencies.PKR)
            {
                convertedAmount = request.AmountRecived / request.USDROE;
            }
            //else if (request.Currency == Currencies.AED)
            //{
            //    convertedAmount = request.AmountRecived / request.USDROE;
            //}
            //else if (request.Currency == Currencies.GBP)
            //{
            //    convertedAmount = request.AmountRecived * request.USDROE;
            //}
            //else if (request.Currency == Currencies.EURO)
            //{
            //    convertedAmount = request.AmountRecived * request.USDROE;
            //}

            string remarks = "";

            if (request.Currency == Currencies.USD)
            {
                remarks = $"Received {request.AmountRecived.ToString("N0")} {request.Currency}";
            }
            else
            {
                remarks = $"Received {request.AmountRecived.ToString("N0")} {request.Currency} @ USD ROE {request.USDROE} = {Math.Round(convertedAmount, 2).ToString("N0")}";
            }

            _invoice.Remarks = remarks;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult UploadProof(int id)
        {
            Invoice invoice = _dbContext.Invoices.Find(id);
            return View(invoice);
        }

        [HttpPost]
        public IActionResult UploadProof(Invoice request)
        {
            Invoice invoice = _dbContext.Invoices.Find(request.InvoiceId);

            string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "Invoices", request.File.FileName);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                request.File.CopyTo(fs);
            }

            invoice.ImagePath = @"Images/Invoices/" + request.File.FileName;
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ViewTrash()
        {
            ViewBag.Bank = _dbContext.BanksWithoutTrash;
            ViewBag.TrashCount = _dbContext.Invoices
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.Trashed).Count();
            ViewBag.Invoice = _dbContext.Invoices
                        .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                        .Where(e => e.Trashed)
                        .Join(
                            _dbContext.FamilyHeads,
                            invoice => invoice.FamilyHeadId,
                            family => family.FamilyHeadId,
                            (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                        );

            return View();
        }

        public IActionResult Trash(int id)
        {
            Invoice invoice = _dbContext.Invoices.Find(id);
            if (invoice == null) return RedirectToAction(nameof(Index));

            invoice.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Invoice invoice = _dbContext.Invoices.Find(id);
            invoice.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangeStatus(int id) => View(_dbContext.Invoices.Find(id));

        [HttpPost]
        public IActionResult ChangeStatus(Invoice invoice)
        {
            if (!ModelState.IsValid) return View(invoice);

            Invoice invoiceToEdit = _dbContext.Invoices.Find(invoice.InvoiceId);
            invoiceToEdit.InvoiceStatus = invoice.InvoiceStatus;
            invoiceToEdit.ClearanceDate = invoice.ClearanceDate;
            invoiceToEdit.DepositDate = invoice.DepositDate;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }


}
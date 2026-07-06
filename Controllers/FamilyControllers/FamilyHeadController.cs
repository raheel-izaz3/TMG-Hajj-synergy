using System;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using CoreHtmlToImage;
using iText.StyledXmlParser.Node;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyHeadReportModels;
using TMG.Models.FamilyModels;
using TMG.Models.FormModels.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.LogisticModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using TMG.Models.QurbaniModels;
using TMG.Models.Updated_Accounts;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

//using AspNetCore;


namespace TMG.Controllers.FamilyControllers
{
    public class FamilyHeadController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public FamilyHeadController(
            TMGDbContext context,
            IWebHostEnvironment host,
            IHttpContextAccessor http
        )
        {
            _context = context;
            _host = host;
            _http = http;
        }

        public record MemberCountModel(int FamilyHeadId, int Total);

        public IActionResult TrashFamily(int id)
        {
            var members = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id)
                .ToList();

            foreach (var m in members)
            {
                m.FamilyHead.Trashed = true;
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFamilyHead(_context, _http);
            ViewBag.ViewFamilyAllowed = PrivilegeManager.CanViewFamilies(_context, _http);
            ViewBag.ViewFamilyLetterHeadAllowed = PrivilegeManager.CanViewFamilyLetterHead(
                _context,
                _http
            );
            ViewBag.ViewFamilyInvoiceAllowed = PrivilegeManager.CanViewFamilyInvoice(
                _context,
                _http
            );
            ViewBag.CreateFamilyMemberAllowed = PrivilegeManager.CanCreateFamilyMember(
                _context,
                _http
            );
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFamily(_context, _http);

            FamilyHeadIndexModel model = new();

            const int ADMIN_ROLE_ID = 1;

            model.IsAdmin =
                Convert.ToInt32(_http.HttpContext.Session.GetString("RoleId")) == ADMIN_ROLE_ID;

            var userCompanies = _context
                .UserCompanies.Include(e => e.Company)
                .Select(e => new
                {
                    e.UserId,
                    e.CompanyId,
                    e.Company.Trashed,
                })
                .Where(e =>
                    e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))
                    && !e.Trashed
                )
                .AsNoTracking()
                .ToList();

            int[] companyIds = new int[userCompanies.Count];

            for (int i = 0; i < userCompanies.Count; i++)
            {
                companyIds[i] = userCompanies.ElementAt(i).CompanyId;
            }

            var query = (
                from familyHead in _context.FamilyHeads
                where
                    !familyHead.Trashed
                    && familyHead.IsHead
                    && companyIds.Contains(familyHead.Package.CompanyId)
                    && familyHead.PackageId != 0
                select familyHead
            )
                .Include(e => e.SPO)
                .Include(e => e.Honorifics)
                .Include(e => e.Package)
                .OrderBy(e => e.FamilyHeadId)
                .Select(e => new
                {
                    e.FamilyHeadId,
                    e.GivenName,
                    e.SurName,
                    e.Honorifics.Title,
                    e.FeedBy,
                    BookedBy = e.SPO.Name,
                    e.PassportNumber,
                    e.Tentative,
                    e.IsHead,
                    PackageCode = e.Package.Code,
                    PackageName = e.Package.Name,
                    CompanyName = e.Package.Company.CompanyCode,
                    e.FcId,
                    e.BiometricVerified,
                    e.EHajjVerified,
                    e.HajjSeasonsId,
                    e.HajjiBookingStatus,
                    e.Currencies
                })
                .Where(e =>
                    e.HajjSeasonsId
                    == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .AsNoTracking()
                .ToList();

            var memberQuery = (
                from familyHead in _context.FamilyMembers
                where
                    !familyHead.FamilyHead.Trashed
                    && companyIds.Contains(familyHead.FamilyHead.Package.CompanyId)
                select familyHead
            )
                .Select(e => new
                {
                    e.FamilyHead,
                    e.FamilyHead.Tentative,
                    e.FamilyHead.BiometricImage,
                    e.FamilyHead.ENumberVerified,
                    e.FamilyHead.Verified,
                    e.HeadId,
                    e.FamilyHeadId,
                    e.FamilyHead.EHajjVerified,
                    e.FamilyHead.HajjiBookingStatus,
                })
                .AsNoTracking()
                .ToList();

            model.FamilyCount = query.Count;
            model.FamilyMemberCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count();
            model.TentativeCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Tentative);
            model.BookedCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked);
            model.CancelledCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Cancelled);
            model.BioImageCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count(e => !string.IsNullOrEmpty(e.BiometricImage));
            model.ENumberVerifiedCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count(e => e.ENumberVerified == true);
            model.FormVerifiedCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count(e => e.Verified == true);
            model.EHajjVerifiedCount = memberQuery
                .Where(e =>
                    e.FamilyHead.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count(e => e.EHajjVerified);

            for (int i = 0; i < query.Count; i++)
            {
                var currMember = query.ElementAt(i);

                int members = memberQuery.Count(e => e.HeadId == currMember.FamilyHeadId);
                int bioCount = memberQuery.Count(e =>
                    e.HeadId == currMember.FamilyHeadId && e.BiometricImage != null
                );
                int verifiedCount = memberQuery.Count(e =>
                    e.HeadId == currMember.FamilyHeadId && e.Verified
                );
                int bioVerifiedCount = memberQuery.Count(e =>
                    e.HeadId == currMember.FamilyHeadId && e.ENumberVerified
                );
                int bookings = memberQuery.Count(e =>
                    e.HeadId == currMember.FamilyHeadId
                    && e.HajjiBookingStatus == HajjiBookingStatus.Booked
                );
                int eHajjVerified = memberQuery.Count(e =>
                    e.HeadId == currMember.FamilyHeadId && e.EHajjVerified
                );

                int serialNumber = i + 1;

                var receipts = _context.NewInvoicesWithoutTrash
                    .Include(e => e.CompanyBankAccount)
                    .Where(e => e.ReceiptStatus == ReceiptStatus.Cleared && e.FamilyHeadId == currMember.FamilyHeadId)
                    .ToList();

                var receivedAmount = receipts
                    .Select(e => AmountConverter.GetConvertedAmount(e.AmountRecieved, currMember.Currencies ?? Currencies.SAR, e.Currencies, e.ROE))
                    .Sum();

                var totalPackageAmount = PackagePriceManager.GetOverallTotalPriceForAll(_context, new List<int> { currMember.FamilyHeadId });

                var refundAmount = (from npv in _context.NewPaymentVouchers
                                    join np in _context.NewPayments on npv.NewPaymentVoucherId equals np.NewPaymentVoucherId
                                    join a in _context.Account on np.AccountsId equals a.AccountsId
                                    join fh in _context.FamilyHeads on a.AccountsId equals fh.ReceivableAccountsId
                                    where fh.FamilyHeadId == currMember.FamilyHeadId && !npv.Trashed
                                    select npv).ToList();


                var refundAmountTotal = refundAmount
                        .Select(e => e.TotalDebit)
                        .Sum();

                var jvAmountList = _context.JVs
                    .Include(e => e.Debit)
                    .Include(e => e.Credit)
                    .Where(e => (e.DebitId == currMember.FamilyHeadId || e.CreditId == currMember.FamilyHeadId) && !e.Trashed)
                    .ToList();

                float? jvAdjustment = 0;
                foreach (var jv in jvAmountList)
                {
                    if (jv.CreditId == currMember.FamilyHeadId)
                    {
                        jvAdjustment -= jv.TotalCredit;
                    }
                    else if (jv.DebitId == currMember.FamilyHeadId)
                    {
                        jvAdjustment += jv.TotalDebit;
                    }
                }

                var remainingAmount = (totalPackageAmount - receivedAmount) + refundAmountTotal + jvAdjustment;

                var entry = new FamilyHeadIndexEntryModel(
                    serialNumber,
                    currMember.FamilyHeadId,
                    currMember.BookedBy,
                    currMember.FeedBy,
                    currMember.CompanyName,
                    currMember.PackageName,
                    bookings,
                    $"{currMember.Title} {currMember.GivenName} {currMember.SurName}",
                    members,
                    "CODEGREEN",
                    ApplicationManager.Encrypt(currMember.PassportNumber),
                    currMember.Tentative,
                    currMember.IsHead,
                    bioCount,
                    verifiedCount,
                    currMember.FcId,
                    bioVerifiedCount,
                    eHajjVerified,
                    currMember.HajjiBookingStatus.ToString(),
                    remainingAmount,
                    currMember.Currencies.ToString()
                );

                model.Entries.Add(entry);
            }
            ViewBag.TrashCount = _context
                .FamilyHeads.Where(e => e.Trashed && e.IsHead)
                .Where(s =>
                    s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    && s.PackageId != 0
                )
                .AsNoTracking()
                .Select(e => e.FamilyHeadId)
                .Count();
            return View(model);
        }

        public IActionResult IndexCustomerProfile()
        {
            var data = _context
                .CustomerPortfolios.Include(e => e.SPO)
                .Include(e => e.CustomerRemarks)
                .Include(e => e.CustomerStatus)
                .Include(e => e.Reference)
                .Include(e => e.City)
                .Include(e => e.Countries)
                .Include(e => e.HajjDepartment)
                .Where(e =>
                    e.HajjSeasonsId
                        == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    && e.IsHead
                    && !e.Trashed
                    && e.IsQuery == false
                )
                .OrderByDescending(e => e.CustomerPortfolioId)
                .ToList();

            var dataWithSerialNumbers = data.Select(
                    (item, index) =>
                        new CustomerPortfolioViewModel
                        {
                            SerialNumber = index + 1,
                            CustomerPortfolioId = item.CustomerPortfolioId,
                            FullName = item.FullName,
                            SPOName = item.SPO?.Name ?? "-",
                            CustomerRemarks = item.Remarks ?? "-",
                            CustomerStatus = item.CustomerStatus?.Status ?? "-",
                            ReferenceName = item.Reference?.ReferenceName ?? "-",
                            CityName = item.City?.Name ?? "-",
                            CountryName = item.Countries?.Name ?? "-",
                            HajjDepartmentName = item.HajjDepartment?.Name ?? "-",
                            Feedby = item.FeedBy,
                            DateOfQuery = item.DateOfQuery,
                            ApplicantType = item.ApplicantType,
                            DateOfEmail = item.DateOfEmail,
                        }
                )
                .ToList();

            var membersCount = _context
                .CustomerPortfoliosMembers.Where(m =>
                    m.CustomerPortfolio.HajjSeasonsId
                        == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    && !m.CustomerPortfolio.Trashed
                )
                .GroupBy(m => m.HeadId)
                .ToDictionary(g => g.Key, g => g.Count());

            var numbers = _context
                .CustomerPortfolioContacts.Where(e =>
                    e.CustomerPortfolio.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .GroupBy(e => e.CustomerPortfolioId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault()?.Contact);

            var emails = _context
                .CustomerPortfolioEmails.Where(e =>
                    e.CustomerPortfolio.HajjSeasonsId
                    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .GroupBy(e => e.CustomerPortfolioId)
                .ToDictionary(e => e.Key, e => e.FirstOrDefault()?.Email);

            var applicantTypeNames = Enum.GetValues(typeof(ApplicantType))
                .Cast<ApplicantType>()
                .ToDictionary(e => (int)e, e => e.ToString());

            ViewBag.ApplicantTypeNames = applicantTypeNames;

            ViewBag.MembersCount = membersCount;

            ViewBag.Numbers = numbers;

            ViewBag.Emails = emails;

            ViewBag.TrashCount = _context
                .CustomerPortfolios.Where(e =>
                    e.Trashed
                    && e.HajjSeasonsId
                        == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Count();

            return View(dataWithSerialNumbers);
        }

        public JsonResult StartEmail(int id)
        {
            try
            {
                var cp = _context.CustomerPortfolios.Find(id);

                if (cp == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                cp.DateOfEmail = DateTime.Now;
                _context.SaveChanges();

                return Json(new { success = true, message = "Email date updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating email date" });
            }
        }

        public JsonResult StopEmail(int id)
        {
            var cp = _context.CustomerPortfolios.Find(id);

            cp.DateOfEmail = null;
            _context.SaveChanges();

            return Json(new { success = true, message = "Email Stoped" });
        }

        public IActionResult ChangeRemarks(int id)
        {
            ViewBag.Remarks = _context.CustomerRemarks.Where(e => !e.Trashed).ToList();

            ViewBag.SelectedRemarkId = _context
                .CustomerPortfolios.Where(cp => cp.CustomerPortfolioId == id)
                .Select(cp => cp.CustomerRemarksId)
                .FirstOrDefault();

            ViewBag.Id = id;

            return View();
        }

        public IActionResult ChangeStatus(int id)
        {
            ViewBag.Status = _context.CustomerStatuses.Where(e => !e.Trashed).ToList();

            ViewBag.SelectedStatusId = _context
                .CustomerPortfolios.Where(cp => cp.CustomerPortfolioId == id)
                .Select(cp => cp.CustomerStatusId)
                .FirstOrDefault();

            ViewBag.Id = id;

            return View();
        }

        public IActionResult RemarksLogs(int id)
        {
            var data = _context
                .CustomerRemarksLogs.Include(e => e.CustomerRemarks)
                .Where(e => e.CustomerPortfolioId == id)
                .ToList();

            ViewBag.Name = _context
                .CustomerPortfolios.Where(e => e.CustomerPortfolioId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            return View(data);
        }

        public IActionResult PinCode(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        public IActionResult PinCode(int id, int pinCode)
        {
            bool pinCodeExists = _context.UsersWithoutTrash.Any(e => e.PinCode == pinCode);

            if (pinCodeExists)
            {
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Pin Code!";
                return RedirectToAction(nameof(PinCode), new { id = id });
            }
        }


        public IActionResult ViewFamily(int id)
        {
            ViewBag.EditAllowed = PrivilegeManager.CanEditFamily(_context, _http);
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFamily(_context, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFamily(_context, _http);
            ViewBag.VerifyAllowed = PrivilegeManager.CanVerifyFamilyMember(_context, _http);
            ViewBag.FamilyHeadId = id;

            var familyHead = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .AsNoTracking()
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed);

            ViewBag.TrashCount = _context
                .FamilyMembers.Where(e => e.FamilyHead.Trashed && e.HeadId == id)
                .AsNoTracking()
                .Count();

            ViewBag.HajjiBookingStatus = Enum.GetValues(typeof(HajjiBookingStatus))
                .Cast<HajjiBookingStatus>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.TicketStatus = Enum.GetValues(typeof(TktStatus))
                .Cast<TktStatus>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.VisaStatus = Enum.GetValues(typeof(VisaStatus))
                .Cast<VisaStatus>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.CompanyVisa = _context.CompanyVisas.Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            return View(familyHead);
        }

        public IActionResult ViewCustomerMember(int id)
        {
            ViewBag.EditAllowed = PrivilegeManager.CanEditFamily(_context, _http);
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFamily(_context, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFamily(_context, _http);
            ViewBag.VerifyAllowed = PrivilegeManager.CanVerifyFamilyMember(_context, _http);
            ViewBag.CustomerPortfolioId = id;

            var customerPortfolio = _context
                .CustomerPortfoliosMembers.Include(e => e.CustomerPortfolio)
                .AsNoTracking()
                .Where(e => e.HeadId == id && !e.CustomerPortfolio.Trashed);

            ViewBag.TrashCount = _context
                .CustomerPortfoliosMembers.Where(e => e.CustomerPortfolio.Trashed && e.HeadId == id)
                .AsNoTracking()
                .Count();

            ViewBag.HajjiBookingStatus = Enum.GetValues(typeof(HajjiBookingStatus))
                .Cast<HajjiBookingStatus>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            return View(customerPortfolio);
        }

        #region Default Overall
        //[HttpPost]
        //public IActionResult UpdateStatus(int familyHeadId, string status)
        //{
        //    int? ReceId = null;
        //    int? RevenId = null;

        //    var familyMember = _context.FamilyHeads.Include(f => f.Package).SingleOrDefault(f => f.FamilyHeadId == familyHeadId);
        //    var familyHead = _context.FamilyHeads.FirstOrDefault(f => f.FcId == familyMember.FcId && f.IsHead == true && !f.Trashed && f.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

        //    if (familyHead.HajjiBookingStatus != HajjiBookingStatus.Booked && !familyMember.IsHead)
        //    {
        //        return Json(new { success = false, message = "Book the head First" });
        //    }

        //    if (familyMember != null)
        //    {
        //        familyMember.HajjiBookingStatus = Enum.Parse<HajjiBookingStatus>(status);

        //        if (Enum.Parse<HajjiBookingStatus>(status) == HajjiBookingStatus.Booked)
        //        {

        //            if (familyMember.IsHead)
        //            {
        //                #region Add Accounts

        //                var AccGroupRevenue = _context.AccountGroups.Where(g => g.Name == COAManager.ServiceRevenueString).FirstOrDefault();

        //                var AccountRevenue = new Accounts()
        //                {
        //                    AccountGroupsId = AccGroupRevenue.AccountGroupId,
        //                    Name = $"{familyMember.GivenName} {familyMember.SurName} ({familyMember.FcId}) - {AccGroupRevenue.Name} ({familyMember.Currencies.ToString()})",
        //                    CompanyId = familyMember.Package.CompanyId,
        //                };

        //                _context.Account.Add(AccountRevenue);
        //                _context.SaveChanges();

        //                RevenId = AccountRevenue.AccountsId;


        //                var AccGroupReceivable = _context.AccountGroups.Where(g => g.Name == COAManager.CurrentAssetsString).FirstOrDefault();

        //                var AccountRec = new Accounts()
        //                {
        //                    AccountGroupsId = AccGroupReceivable.AccountGroupId,
        //                    Name = $"{familyMember.GivenName} {familyMember.SurName} ({familyMember.FcId}) - Receivable ({familyMember.Currencies.ToString()})",
        //                    CompanyId = familyMember.Package.CompanyId
        //                };

        //                _context.Account.Add(AccountRec);
        //                _context.SaveChanges();

        //                ReceId = AccountRec.AccountsId;

        //                #endregion

        //                #region Ledger Entry

        //                GeneralLedger generalLedger = new()
        //                {
        //                    Date = DateTime.Now,
        //                };
        //                _context.GeneralLedgers.Add(generalLedger);
        //                _context.SaveChanges();

        //                float packageAmount = PackagePriceManager.GetOverallTotalPrice(_context, familyHeadId);

        //                GeneralLedgerEntry debitEntry = new GeneralLedgerEntry()
        //                {
        //                    Description = AccountRec.Name,
        //                    AccountNature = AccountNature.Debit,
        //                    GeneralLedgerId = generalLedger.GeneralLedgerId,
        //                    Amount = packageAmount,
        //                    AccountsId = AccountRec.AccountsId
        //                };

        //                GeneralLedgerEntry creditEntry = new GeneralLedgerEntry()
        //                {
        //                    Description = AccountRevenue.Name,
        //                    AccountNature = AccountNature.Credit,
        //                    GeneralLedgerId = generalLedger.GeneralLedgerId,
        //                    Amount = packageAmount,
        //                    AccountsId = AccountRevenue.AccountsId
        //                };

        //                _context.GeneralLedgerEntries.Add(debitEntry);
        //                _context.GeneralLedgerEntries.Add(creditEntry);
        //                _context.SaveChanges();

        //                #endregion
        //            }
        //        }

        //        familyMember.ReceivableAccountsId = ReceId;
        //        familyMember.RevenueAccountsId = RevenId;

        //        _context.SaveChangesAsync();

        //        return Json(new { success = true, message = "Booking Status updated successfully!" });
        //    }
        //    return Json(new { success = false, message = "Booking Status failed to updated" });
        //}
        #endregion

        [HttpPost]
        public IActionResult UpdateStatus(int familyHeadId, string status, DateTime date)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                int? ReceId = null;
                int? RevenId = null;

                var familyMember = _context
                    .FamilyHeads.Include(f => f.Package)
                    .SingleOrDefault(f => f.FamilyHeadId == familyHeadId);

                var familyHead = _context
                    .FamilyHeads.Include(f => f.Accounts)
                    .FirstOrDefault(f =>
                        f.FcId == familyMember.FcId
                        && f.IsHead == true
                        && !f.Trashed
                        && f.HajjSeasonsId
                            == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    );

                if (
                    familyHead.HajjiBookingStatus != HajjiBookingStatus.Booked
                    && !familyMember.IsHead
                )
                {
                    return Json(new { success = false, message = "Book the head First" });
                }

                if (familyMember != null)
                {
                    familyMember.HajjiBookingStatus = Enum.Parse<HajjiBookingStatus>(status);
                    familyMember.ProformaDate = date;

                    if (Enum.Parse<HajjiBookingStatus>(status) == HajjiBookingStatus.Booked)
                    {
                        if (familyMember.IsHead)
                        {
                            #region Add Accounts

                            var AccGroupRevenue = _context
                                .AccountGroups.Where(g => g.Name == COAManager.ServiceRevenueString)
                                .FirstOrDefault();

                            var AccountRevenue = new Accounts()
                            {
                                AccountGroupsId = AccGroupRevenue.AccountGroupId,
                                Name =
                                    $"{familyMember.GivenName} {familyMember.SurName} ({familyMember.FcId}) - {AccGroupRevenue.Name} ({familyMember.Currencies.ToString()})",
                                CompanyId = familyMember.Package.CompanyId,
                                IsHajjiAccount = true,
                                HajjSeasonsId = int.Parse(
                                    _http.HttpContext.Session.GetString("ActiveSeasonId")
                                ),
                                Currencies = familyMember.Currencies,
                            };

                            _context.Account.Add(AccountRevenue);
                            _context.SaveChanges();

                            RevenId = AccountRevenue.AccountsId;

                            var AccGroupReceivable = _context
                                .AccountGroups.Where(g => g.Name == COAManager.CurrentAssetsString)
                                .FirstOrDefault();

                            var AccountRec = new Accounts()
                            {
                                AccountGroupsId = AccGroupReceivable.AccountGroupId,
                                Name =
                                    $"{familyMember.GivenName} {familyMember.SurName} ({familyMember.FcId}) - Receivable ({familyMember.Currencies.ToString()})",
                                CompanyId = familyMember.Package.CompanyId,
                                IsHajjiAccount = true,
                                HajjSeasonsId = int.Parse(
                                    _http.HttpContext.Session.GetString("ActiveSeasonId")
                                ),
                                Currencies = familyMember.Currencies,
                            };

                            _context.Account.Add(AccountRec);
                            _context.SaveChanges();

                            ReceId = AccountRec.AccountsId;

                            #endregion

                            #region Ledger Entry

                            GeneralLedger generalLedger = new()
                            {
                                Date = DateTime.Now,
                                Trashed = false,
                                CreatedAt = DateTime.Now,
                            };
                            _context.GeneralLedgers.Add(generalLedger);
                            _context.SaveChanges();

                            float packageAmount = PackagePriceManager.GetIndividualTotalPrice(
                                _context,
                                familyHeadId
                            );

                            GeneralLedgerEntry debitEntry = new GeneralLedgerEntry()
                            {
                                Description = AccountRec.Name,
                                AccountNature = AccountNature.Debit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = packageAmount,
                                AccountsId = AccountRec.AccountsId,
                            };

                            GeneralLedgerEntry creditEntry = new GeneralLedgerEntry()
                            {
                                Description = AccountRevenue.Name,
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = packageAmount,
                                AccountsId = AccountRevenue.AccountsId,
                            };

                            _context.GeneralLedgerEntries.Add(debitEntry);
                            _context.GeneralLedgerEntries.Add(creditEntry);
                            _context.SaveChanges();

                            #endregion

                            familyMember.ReceivableAccountsId = ReceId;
                            familyMember.RevenueAccountsId = RevenId;
                        }
                        else
                        {
                            #region Ledger Entry

                            int RecevableId = Convert.ToInt32(familyHead.ReceivableAccountsId);
                            int RevenueId = Convert.ToInt32(familyHead.RevenueAccountsId);

                            GeneralLedger generalLedger = new()
                            {
                                Date = DateTime.Now,
                                Trashed = false,
                                CreatedAt = DateTime.Now,
                            };
                            _context.GeneralLedgers.Add(generalLedger);
                            _context.SaveChanges();

                            float packageAmount = PackagePriceManager.GetIndividualTotalPrice(
                                _context,
                                familyHeadId
                            );

                            GeneralLedgerEntry debitEntry = new GeneralLedgerEntry()
                            {
                                Description =
                                    $"{familyMember.GivenName} {familyMember.SurName} ({familyMember.FcId}) - Receivable ({familyMember.Currencies.ToString()})",
                                AccountNature = AccountNature.Debit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = packageAmount,
                                AccountsId = RecevableId,
                            };

                            GeneralLedgerEntry creditEntry = new GeneralLedgerEntry()
                            {
                                Description =
                                    $"{familyMember.GivenName} {familyMember.SurName} ({familyMember.FcId}) - {COAManager.ServiceRevenueString} ({familyMember.Currencies.ToString()})",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedger.GeneralLedgerId,
                                Amount = packageAmount,
                                AccountsId = RevenueId,
                            };

                            _context.GeneralLedgerEntries.Add(debitEntry);
                            _context.GeneralLedgerEntries.Add(creditEntry);
                            _context.SaveChanges();

                            #endregion

                            familyMember.ReceivableAccountsId = RecevableId;
                            familyMember.RevenueAccountsId = RevenueId;
                        }
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    return Json(
                        new { success = true, message = "Booking Status updated successfully!" }
                    );
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = "Booking Status failed to update" });
            }

            return Json(new { success = false, message = "Invalid operation" });
        }

        [HttpPost]
        public IActionResult UpdateTicket(int familyHeadId, string status)
        {
            var familyHead = _context.FamilyHeads.FirstOrDefault(f => f.FamilyHeadId == familyHeadId);

            if (familyHead == null)
            {
                return Json(new { success = false, message = "Family Head not found!" });
            }

            familyHead.TicketStatus = Enum.Parse<TktStatus>(status);
            _context.SaveChanges();

            return Json(new { success = true, message = "Ticket Status updated successfully!" });
        }

        [HttpPost]
        public IActionResult UpdateVisaStatus(int familyHeadId, int newStatus)
        {
            var familyHead = _context.FamilyHeads.FirstOrDefault(f => f.FamilyHeadId == familyHeadId);
            if (familyHead == null)
            {
                return NotFound();
            }

            familyHead.VisaStatus = (VisaStatus?)newStatus;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdatesCompanyStatus(int familyHeadId, int? companyId)
        {
            var familyHead = _context.FamilyHeads.FirstOrDefault(f => f.FamilyHeadId == familyHeadId);
            if (familyHead != null)
            {
                // Update the CompanyVisasId for the FamilyHead
                familyHead.CompanyVisasId = companyId;

                // Save changes to the database
                _context.SaveChanges();

                return Ok();  // Send success response
            }

            return NotFound();  // If FamilyHead is not found, send error response
        }

        public IActionResult ConvertToJpeg(string form)
        {
            var converter = new HtmlConverter();
            //form = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head>\r\n\r\n    <link rel=\"icon\" type=\"image/x-icon\" href=\"/favicon.ico\">\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.0.1/css/bootstrap.min.css\" integrity=\"sha512-Ez0cGzNzHR1tYAv56860NLspgUGuQw16GiOOp/I2LuTmpSK9xDXlgJz3XN4cnpXWDmkNBKXR/VDMTCnAaEooxA==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\">\r\n\r\n    <title>TMG Report</title>\r\n    <style>\r\n        @media print {\r\n            .hideInPrint {\r\n                display: none;\r\n            }\r\n        }\r\n\r\n        .page-break {\r\n            page-break-after: always;\r\n        }\r\n\r\n       \r\n    </style>\r\n\r\n</head>\r\n\r\n<body>\r\n    \r\n\r\n\r\n<style>\r\n    .user-img img,\r\n    .img-stamp {\r\n        height: 100%;\r\n        width: 100%;\r\n        object-fit: contain;\r\n    }\r\n\r\n    .user-img {\r\n        height: 150px;\r\n    }\r\n\r\n    .doc-approval table {\r\n        height: 100px;\r\n        text-transform: capitalize;\r\n        text-align: center;\r\n    }\r\n\r\n    .doc-approval table td:not(td[class='seal-sig-doc']) {\r\n        vertical-align: middle;\r\n    }\r\n\r\n    .urdu {\r\n        src: url('https://www.fontsaddict.com/fontface/jameel-noori-nastaleeq.ttf');\r\n        font-family: 'Jameel Noori Nastaleeq';\r\n    }\r\n\r\n    @media print {\r\n        .hide-in-print {\r\n            display: none;\r\n        }\r\n\r\n        .img-stamp {\r\n            object-fit: contain;\r\n        }\r\n\r\n        title {\r\n            display: none\r\n        }\r\n\r\n        @page {\r\nmargin: 0;\r\n            margin-top: 20px;\r\n            margin-inline: 30px;\r\n\r\n        }\r\n\r\n@top {\r\n            content: none;\r\n        }\r\n\r\n        @bottom {\r\n            content: none;\r\n        }\r\n\r\n        .seal-sig-doc p {\r\n            text-wrap: nowrap;\r\n            font-size: 6pt !important;\r\n            font-weight: 100 !important;\r\n        }\r\n\r\n        .seal-sig-doc {\r\n            overflow-y: hidden !important;\r\n        }\r\n\r\n        .stamp-img {\r\n            border-top: .5px solid !important;\r\n        }\r\n    }\r\n\r\n    .mr-2 {\r\n        margin-right: 1%;\r\n    }\r\n\r\n    * {\r\n        font-size: 8pt;\r\n        line-height: 12px;\r\n    }\r\n\r\n    .fillable {\r\n        font-family: monospace;\r\n        text-transform: uppercase;\r\n    }\r\n\r\n    table td {\r\n        border: 0.5px solid black !important;\r\n    }\r\n\r\n    .seal-sig-doc {\r\n        vertical-align: bottom !important;\r\n        margin: 0 !important;\r\n        padding: 0 !important;\r\n    }\r\n\r\n    .seal-sig-doc p {\r\n        text-wrap: nowrap;\r\n        font-size: xx-small;\r\n        font-weight: 100 !important;\r\n    }\r\n\r\n    .halaf-nama {\r\n        direction: rtl !important;\r\n    }\r\n\r\n    .page-break {\r\n        page-break-after: always;\r\n        /* or 'auto' or 'avoid' */\r\n    }\r\n</style>\r\n<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js\"></script>\r\n\r\n\r\n\r\n<div class=\"row hide-in-print p-3\">\r\n    <div>\r\n        <input type=\"checkbox\" name=\"coronaCheckBox\" onchange=\"hidediv(this)\" value=\"Corona\">\r\n        <label>Corona</label>\r\n    </div>\r\n    <div>\r\n        <input type=\"checkbox\" name=\"doctorCheckBox\" onchange=\"onDoctorCheck(this)\" value=\"none\">\r\n        <label>None</label>\r\n    </div>\r\n        <div>\r\n            <input type=\"checkbox\" name=\"doctorCheckBox\" onchange=\"onDoctorCheck(this)\" value=\"3\">\r\n            <label>Dr. Abdul Razzaq Siddiqui</label>\r\n        </div>\r\n        <div>\r\n            <input type=\"checkbox\" name=\"doctorCheckBox\" onchange=\"onDoctorCheck(this)\" value=\"4\">\r\n            <label>Dr. Abdul Farooq Ahmed</label>\r\n        </div>\r\n</div>\r\n\r\n<button onclick=\"btncapture()\">Capture</button>\r\n<div id=\"capture\">\r\n\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n</div>\r\n<script>\r\n\r\nfunction btncapture() {\r\n    var form = document.documentElement.outerHTML;\r\n\r\n     $.ajax({\r\n         type: 'POST',\r\n         url: '/FamilyHeadReport/ConvertToJpeg',\r\n         data: {form : form},\r\n         success: function (data) {\r\n           /*  var link = document.createElement('a');\r\n            link.href = URL.createObjectURL(new Blob([data], { type: 'image/jpeg' }));\r\n            link.download = 'image.jpeg';\r\n            link.click();  */\r\n             console.log(data);\r\n         },\r\n         error: function (error) {\r\n              console.error('Error:', error);\r\n         }\r\n     });\r\n }\r\n /* function btncapture() {\r\n    $.ajax({\r\n        type: 'POST',\r\n        url: '/FamilyHeadReport/ConvertToJpeg',\r\n        contentType: 'application/json',\r\n        data: JSON.stringify({ form: document.documentElement.outerHTML }), // or the appropriate HTML content\r\n        \r\n        success: function (data) {\r\n            console.log(data);\r\n        },\r\n        error: function (error) {\r\n            console.error('Error:', error);\r\n        }\r\n    });\r\n} */\r\n    function onDoctorCheck(elem) {\r\n        const checkBoxes = document.getElementsByName(\"doctorCheckBox\")\r\n        const id = elem.value\r\n\r\n        const name = document.querySelectorAll(\"#doctor-name\")\r\n        const hospital = document.querySelectorAll(\"#doctor-hospital\")\r\n        const tehsil = document.querySelectorAll(\"#doctor-tehsil\")\r\n        const district = document.querySelectorAll(\"#doctor-district\")\r\n        const landline = document.querySelectorAll(\"#doctor-landline\")\r\n        const cell = document.querySelectorAll(\"#doctor-cell\")\r\n        const pmdc = document.querySelectorAll(\"#doctor-pmdc\")\r\n        const seal1 = document.querySelectorAll(\"#doctor-seal-1\")\r\n        const seal2 = document.querySelectorAll(\"#doctor-seal-2\")\r\n\r\n        if (elem.checked) {\r\n            for (let c of checkBoxes) {\r\n                if (c.value == id) {\r\n                    c.checked = true;\r\n                }\r\n                else {\r\n                    c.checked = false;\r\n                }\r\n            }\r\n\r\n            if (id != \"none\") {\r\n                fetch(\"/Doctor/Get/\" + id)\r\n                    .then(res => res.json())\r\n                    .then(res => {\r\n                        for (let i = 0; i < name.length; i++) {\r\n                            name[i].innerHTML = res.name;\r\n                            hospital[i].innerHTML = res.hospitalName;\r\n                            tehsil[i].innerHTML = res.tehsil.name;\r\n                            district[i].innerHTML = res.district;\r\n                            landline[i].innerHTML = res.landline;\r\n                            cell[i].innerHTML = res.cellNumber;\r\n                            seal1[i].src = \"/\" + res.sealSignaturePath;\r\n\r\n                            const stampPlaceholders = document.querySelectorAll(\"#stampPlaceholder\")\r\n                            for (let p of stampPlaceholders) {\r\n                                p.style.display = \"none\"\r\n                            }\r\n\r\n                            pmdc[i].innerHTML = `\r\n                                        <td class=\"fillable text-uppercase\">pmdc</td>\r\n                                    `\r\n\r\n                            for (let x = 0; x < res.registrationNumber.length; x++) {\r\n                                pmdc[i].innerHTML += ` <td class=\"p-2 fillable\">\r\n                                ${res.registrationNumber[x]}</td>`\r\n                            }\r\n                        }\r\n                    })\r\n            }\r\n            else {\r\n                for (let i = 0; i < name.length; i++) {\r\n                    name[i].innerHTML = \"\";\r\n                    hospital[i].innerHTML = \"\";\r\n                    tehsil[i].innerHTML = \"\";\r\n                    district[i].innerHTML = \"\";\r\n                    landline[i].innerHTML = \"\";\r\n                    cell[i].innerHTML = \"\";\r\n\r\n                    seal1[i].src = \"\";\r\n\r\n                    const stampPlaceholders = document.querySelectorAll(\"#stampPlaceholder\")\r\n                    for (let p of stampPlaceholders) {\r\n                        p.style.display = \"block\"\r\n                    }\r\n\r\n                    pmdc[i].innerHTML = `\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        `\r\n                }\r\n            }\r\n        }\r\n\r\n    }\r\n    function hidediv(elem) {\r\n            \r\n            if (elem.checked) { \r\n                const stampPlaceholders = document.querySelectorAll(\"#coronadiv\")\r\n                    for (let p of stampPlaceholders) {\r\n                        p.style.display = \"block\"\r\n                    }\r\n               \r\n            } else {\r\n                 const stampPlaceholders = document.querySelectorAll(\"#coronadiv\")\r\n                    for (let p of stampPlaceholders) {\r\n                        p.style.display = \"none\"\r\n                    }\r\n              \r\n            }\r\n        }\r\n\r\n\r\n</script>\r\n\r\n\r\n<!-- Visual Studio Browser Link -->\r\n<script type=\"text/javascript\" src=\"/_vs/browserLink\" async=\"async\" id=\"__browserLink_initializationData\" data-requestid=\"46edba0f6fc54f3db012739511255130\" data-requestmappingfromserver=\"false\" data-connecturl=\"http://localhost:60153/cb95ed154a4f4f7cbaaec3c6c3519bf0/browserLink\"></script>\r\n<!-- End Browser Link -->\r\n<script src=\"/_framework/aspnetcore-browser-refresh.js\"></script>\r\n\r\n\r\n</body></html>";
            var html = form;
            var bytes = converter.FromHtmlString(html);
            System.IO.File.WriteAllBytes("image.jpeg", bytes);

            // Return a FileResult to download the image
            return File(bytes, "image/jpeg", "image.jpeg");
        }

        public IActionResult MedicalAffidavit(int id)
        {
            var membersOfFamily = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id)
                .ToList();

            var vaccines = _context.VaccinesWithoutTrash.Select(e => new
            {
                e.VaccineName,
                e.VaccineId,
            });

            MedicalAffidavitModel model = new();

            model.Doctors = _context.DoctorsWithoutTrash.ToList();

            for (int i = 0; i < membersOfFamily.Count; i++)
            {
                FamilyMembers currMember = membersOfFamily.ElementAt(i);
                MedicalAffidavitModelEntry entry = new();

                UserHajjAgreement userHajjAgreement = _context.UserHajjAgreements.FirstOrDefault(
                    e => e.FamilyHeadId == currMember.FamilyHeadId
                );

                entry.ShowSignature = userHajjAgreement?.SignMedicalAffidavit ?? false;
                entry.SignatureBase64 = currMember.FamilyHead.SignatureBase64;
                entry.Name = currMember.FamilyHead.GivenName;
                entry.FatherHusbandName = currMember.FamilyHead.FatherHusbandName;
                entry.Age = DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth);
                entry.PassportNumber = currMember.FamilyHead.PassportNumber;
                entry.BloodGroup = currMember.FamilyHead.BloodGroup;
                entry.ImagePath = currMember.FamilyHead.MemberImage;
                entry.CNIC = currMember.FamilyHead.CNIC;

                entry.Vaccine1 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId1)
                    ?.VaccineName;
                entry.Vaccine2 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId2)
                    ?.VaccineName;
                entry.Vaccine3 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId3)
                    ?.VaccineName;
                entry.Vaccine4 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId4)
                    ?.VaccineName;
                entry.Vaccine5 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId5)
                    ?.VaccineName;

                entry.DoseDate1 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate1);
                entry.DoseDate2 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate2);
                entry.DoseDate3 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate3);
                entry.DoseDate4 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate4);
                entry.DoseDate5 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate5);

                model.Entries.Add(entry);
            }

            return View(model);
        }

        public IActionResult ViewTrash(int id)
        {
            var memberIds = _context
                .FamilyMembers.Where(e => e.HeadId == id)
                .Select(e => e.FamilyHeadId)
                .ToList();
            return View(
                _context.FamilyHeads.Where(s =>
                    memberIds.Contains(s.FamilyHeadId)
                    && s.Trashed
                    && s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
            );
        }

        public IActionResult ViewMemberTrash(int id)
        {
            var memberIds = _context
                .CustomerPortfoliosMembers.Where(e => e.HeadId == id)
                .Select(e => e.CustomerPortfolioId)
                .ToList();
            return View(
                _context.CustomerPortfolios.Where(s =>
                    memberIds.Contains(s.CustomerPortfolioId)
                    && s.Trashed
                    && s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
            );
        }

        public IActionResult ViewTrashMainPage()
        {
            return View(
                _context.FamilyHeads.Where(s =>
                    s.Trashed
                    && s.IsHead
                    && s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
            );
        }

        public IActionResult ViewCustomerTrash()
        {
            return View(
                _context.CustomerPortfolios.Where(s =>
                    s.Trashed
                    && s.IsHead
                    && s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
            );
        }

        public IActionResult Trash(int id)
        {
            FamilyHead familyHead = _context.FamilyHeads.Find(id);
            if (familyHead == null)
                return RedirectToAction(nameof(Index));

            familyHead.Trashed = true;

            if (familyHead.AccountId.HasValue)
            {
                Account account = _context.Accounts.Find(familyHead.AccountId.Value);
                if (account != null)
                {
                    account.Trashed = true;
                }
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult TrashCustomer(int id)
        {
            CustomerPortfolio familyHead = _context.CustomerPortfolios.Find(id);
            if (familyHead == null)
                return RedirectToAction(nameof(IndexCustomerProfile));

            familyHead.Trashed = true;

            _context.SaveChanges();

            return RedirectToAction(nameof(IndexCustomerProfile));
        }

        public IActionResult Restore(int id)
        {
            FamilyHead familyMember = _context.FamilyHeads.Find(id);
            Account account = _context.Accounts.Find(familyMember.AccountId);
            FamilyMembers fm = _context.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id);

            familyMember.Trashed = false;
            if (account is not null)
            {
                account.Trashed = false;
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(ViewFamily), new { id = fm.HeadId });
        }

        public IActionResult RestoreMember(int id)
        {
            CustomerPortfolio familyMember = _context.CustomerPortfolios.Find(id);
            CustomerPortfolioMembers fm = _context.CustomerPortfoliosMembers.FirstOrDefault(e =>
                e.CustomerPortfolioId == id
            );

            familyMember.Trashed = false;

            _context.SaveChanges();
            return RedirectToAction(nameof(ViewCustomerMember), new { id = fm.HeadId });
        }

        public IActionResult Restorehead(int id)
        {
            var family = _context.FamilyMembers.Where(e => e.HeadId == id);
            foreach (var item in family)
            {
                FamilyHead familyMember = _context.FamilyHeads.Find(item.FamilyHeadId);
                Account account = _context.Accounts.Find(familyMember.AccountId);

                FamilyMembers fm = _context.FamilyMembers.FirstOrDefault(e =>
                    e.HeadId == item.FamilyHeadId
                );

                familyMember.Trashed = false;
                if (account != null)
                {
                    account.Trashed = false;
                }
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index)); //, new { id = fm.HeadId }
        }

        public IActionResult RestoreCustomerHead(int id)
        {
            var family = _context.CustomerPortfoliosMembers.Where(e => e.HeadId == id);
            foreach (var item in family)
            {
                CustomerPortfolio familyMember = _context.CustomerPortfolios.Find(
                    item.CustomerPortfolioId
                );

                CustomerPortfolioMembers fm = _context.CustomerPortfoliosMembers.FirstOrDefault(e =>
                    e.HeadId == item.CustomerPortfolioId
                );

                familyMember.Trashed = false;
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(IndexCustomerProfile));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.ViewFamilyId = _context
                .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id)
                .HeadId;

            ViewBag.FoodPackage = _context.FoodPackages.Where(e => !e.Trashed).ToList();

            ViewBag.MealPlanMakkah = _context.FamilyHeadAccomodations
                .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.MAKKAH_ID)?.FoodPackageId;

            ViewBag.MealPlanMedinah = _context.FamilyHeadAccomodations
                .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.MEDINA_ID)?.FoodPackageId;

            ViewBag.MealPlanAzizia = _context.FamilyHeadAccomodations
                .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.AZIZIA_ID)?.FoodPackageId;

            ViewBag.MealPlanMina = _context.FamilyHeadAccomodations
                .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.MINA_ID)?.FoodPackageId;


            ViewBag.FFNumber = _context.FamilyHeadFFNumbers.Where(e => !e.Trashed && e.FamilyHeadId == id).ToList();

            ViewBag.Designation = _context.Designations.Where(e => !e.Trashed).ToList();

            ViewBag.PassportType = _context.PassportTypes.Where(e => !e.Trashed).ToList();

            ViewBag.Nationality = _context.Nationalities.Where(e => !e.Trashed).ToList();

            ViewBag.Country = _context.Countries.Where(e => !e.Trashed).ToList();

            ViewBag.City = _context.Cities.Where(e => !e.Trashed).ToList();

            ViewBag.CountryCode = _context.CountryCodes.Where(e => !e.Trashed).ToList();

            ViewBag.TransportPackage = _context.TravelRoutes.ToList();
            ViewBag.AssignedExtraFacilities = _context.FamilyHeadExtras.Where(e =>
                e.FamilyHeadId == id
            );
            ViewBag.ExtraFacilityDesc = _context.ExtraFacilityDescriptionsWithoutTrash;
            ViewBag.Airline = _context.Airlines.Where(e => !e.Trashed).ToList();
            ViewBag.Vehicles = _context.VehiclesWithoutTrash
                            //.Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .ToList(); ;

            if (_context.FamilyHeadTransports.Count(e => e.FamilyHeadId == id) == 0)
            {
                int headId = _context
                    .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id)
                    .HeadId;
                //FamilyHead familyHeadId = _context.FamilyHeads.Include(e => e.Package).FirstOrDefault(e => e.FamilyHeadId == id);
                //var travelRouteId = familyHeadId.Package.TravelRouteId;
                /*
                                var result = (from tr in _context.TravelRoutes
                                                   join trs in _context.TravelRouteStops on tr.TravelRouteId equals trs.TravelRouteId into trsJoin
                                                   from trs in trsJoin.DefaultIfEmpty()
                                                   join fht in _context.FamilyHeadTransports on trs.TravelRouteId equals fht.RouteId into fhtJoin
                                                   from fht in fhtJoin.DefaultIfEmpty()
                                                   join r in _context.Routes on trs.RouteId equals r.RouteId into rJoin
                                                   from r in rJoin.DefaultIfEmpty()
                                                   where tr.TravelRouteId == travelRouteId && (fht.FamilyHeadId == id || fht.FamilyHeadId == null)
                                                   select new
                                                   {
                                                       TravelRoute = tr,
                                                       TravelRouteStop = trs,
                                                       FamilyHeadTransport = fht,
                                                       StartPlace = r.StartPlace,
                                                       Destination = r.Destination,
                                                       RouteId = r.RouteId,
                                                       Route = trs.Route,
                                                       CheckIn = fht.CheckIn,
                                                       TransportType = fht.TransportType,
                                                       VehicleId = fht.VehicleId,
                                                       Vehicle = fht.Vehicle,
                                                   }).ToList();

                                //return Ok(result);
                                ViewBag.FamilyHeadTransports = result;*/

                ViewBag.FamilyHeadTransports = _context
                    .FamilyHeadTransports.Include(e => e.Route)
                    .Where(e => e.FamilyHeadId == headId)
                    .ToList();
            }
            else
            {
                FamilyHead familyHeadId = _context
                    .FamilyHeads.Include(e => e.Package)
                    .FirstOrDefault(e => e.FamilyHeadId == id);
                ViewBag.FamilyHeadTransports = _context
                    .FamilyHeadTransports.Include(e => e.Route)
                    .Where(e => e.FamilyHeadId == id)
                    .ToList();
                //return Ok(result);
            }

            if (_context.FamilyHeadFlight.Count(e => e.FamilyHeadId == id) == 0)
            {
                int headId = _context
                    .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id)
                    .HeadId;

                ViewBag.FlightDetails = _context
                    .FamilyHeadFlight.Where(e =>
                        e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == headId
                    )
                    .ToList();

                ViewBag.FlightExists =
                    _context
                        .FamilyHeadFlight.Where(e =>
                            e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == headId
                        )
                        .Count() == 0;
            }
            else
            {
                ViewBag.FlightDetails = _context
                    .FamilyHeadFlight.Where(e =>
                        e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == id
                    )
                    .ToList();

                ViewBag.FlightExists =
                    _context
                        .FamilyHeadFlight.Where(e =>
                            e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == id
                        )
                        .Count() != 0;
            }

            if (_context.FamilyHeadTrain.Count(e => e.FamilyHeadId == id) == 0)
            {
                int headId = _context
                    .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id)
                    .HeadId;

                ViewBag.TrainDetails = _context
                    .FamilyHeadTrain.Where(e =>
                        e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == headId
                    )
                    .ToList();

                ViewBag.TrainExists =
                    _context
                        .FamilyHeadTrain.Where(e =>
                            e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == headId
                        )
                        .Count() == 0;
            }
            else
            {
                ViewBag.TrainDetails = _context
                    .FamilyHeadTrain.Where(e =>
                        e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == id
                    )
                    .ToList();

                ViewBag.TrainExists =
                    _context
                        .FamilyHeadTrain.Where(e =>
                            e.TicketStatus == TicketStatus.Active && e.FamilyHeadId == id
                        )
                        .Count() != 0;
            }
            //return Ok(ViewBag.FamilyHeadTransports.Count);

            ViewBag.Railway = _context.RailwaysWithoutTrash;

            ViewBag.MehramRelations = _context.MehramRelationsWithoutTrash;

            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.NomineeRels = _context.NomineeRelationsWithoutTrash;
            ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                .Cast<FlightClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.IssuingAgent = _context.IssuingAgents.Where(e => !e.Trashed).ToList();

            ViewBag.TrainClass = Enum.GetValues(typeof(TrainClass))
                .Cast<TrainClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.FlightType = Enum.GetValues(typeof(FlightType))
                .Cast<FlightType>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            FamilyHead familyHead = _context
                .FamilyHeadsWithoutTrash.Include(e => e.Friend)
                .FirstOrDefault(e => e.FamilyHeadId == id);

            ViewBag.PackageId = familyHead.PackageId;
            var TravelRouteId = _context.Packages.Find(familyHead.PackageId).TravelRouteId;

            ViewBag.PackageFlights = _context
                .PackageFlights.Where(e => e.PackageId == familyHead.PackageId)
                .ToList();

            ViewBag.PackageTrain = _context
                .PackageTrains.Where(e => e.PackageId == familyHead.PackageId)
                .ToList();

            var TravelRouteStop = _context
                .TravelRouteStops.Include(e => e.Route)
                .Where(e => e.TravelRouteId == TravelRouteId)
                .OrderBy(e => e.CheckIn);

            ViewBag.Transportation = TravelRouteStop;

            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            var packageROE = _context
                .Packages.Select(e => new
                {
                    e.SARROE,
                    e.USDROE,
                    e.PackageId,
                })
                .FirstOrDefault(e => e.PackageId == familyHead.PackageId);

            ViewBag.USDROE = packageROE.USDROE;
            ViewBag.SARROE = packageROE.SARROE;

            ViewBag.FeedBy = familyHead.FeedBy;

            ViewBag.RoomTypes = _context
                .IndividualFacilityDescriptionsWithoutTrash
                .Include(e => e.Place)
                .Include(e => e.Accomodation)
                .Include(e => e.Room);

            //return Ok(ViewBag.RoomTypes);



            var dataExists = _context
                .FamilyHeadAccomodations.Where(e => e.FamilyHeadId == id)
                .Count();

            if (dataExists == 0)
            {
                ViewBag.MakkahPackageRoom = _context
                    .PackageCategoryFacilities.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId
                        && e.IndividualFacilityDescription.PlaceId == Place.MAKKAH_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.MedinaPackageRoom = _context
                    .PackageCategoryFacilities.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId
                        && e.IndividualFacilityDescription.PlaceId == Place.MEDINA_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.AziziaPackageRoom = _context
                    .PackageCategoryFacilities.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId
                        && e.IndividualFacilityDescription.PlaceId == Place.AZIZIA_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.MinaPackageRoom = _context
                    .PackageCategoryFacilities.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId
                        && e.IndividualFacilityDescription.PlaceId == Place.MINA_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.MakkahPackageHotel = _context
                    .PackageAccomodtions.Include(e => e.Hotel)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MAKKAH_ID
                    )
                    ?.HotelId;

                ViewBag.MedinaPackageHotel = _context
                    .PackageAccomodtions.Include(e => e.Hotel)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MEDINA_ID
                    )
                    ?.HotelId;

                ViewBag.AziziaPackageHotel = _context
                    .PackageAccomodtions.Include(e => e.Hotel)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.AZIZIA_ID
                    )
                    ?.HotelId;

                ViewBag.MinaPackageHotel = _context
                    .PackageAccomodtions.Include(e => e.Hotel)
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MINA_ID
                    )
                    ?.HotelId;

                ViewBag.MakkahPackageCheckIn = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckInTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MAKKAH_ID
                    )
                    ?.ActualCheckInTime;

                ViewBag.MakkahPackageCheckOut = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckOutTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MAKKAH_ID
                    )
                    ?.ActualCheckOutTime;

                ViewBag.MakkahPackageDay = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Days,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MAKKAH_ID
                    )
                    ?.Days;

                ViewBag.MakkahPackageNight = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Nights,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MAKKAH_ID
                    )
                    ?.Nights;

                ViewBag.MedinaPackageCheckIn = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckInTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MEDINA_ID
                    )
                    ?.ActualCheckInTime;

                ViewBag.MedinaPackageCheckOut = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckOutTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MEDINA_ID
                    )
                    ?.ActualCheckOutTime;

                ViewBag.MedinaPackageDay = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Days,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MEDINA_ID
                    )
                    ?.Days;

                ViewBag.MedinaPackageNight = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Nights,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MEDINA_ID
                    )
                    ?.Nights;

                ViewBag.AziziaPackageCheckIn = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckInTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.AZIZIA_ID
                    )
                    ?.ActualCheckInTime;

                ViewBag.AziziaPackageCheckOut = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckOutTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.AZIZIA_ID
                    )
                    ?.ActualCheckOutTime;

                ViewBag.AziziaPackageDay = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Days,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.AZIZIA_ID
                    )
                    ?.Days;

                ViewBag.AziziaPackageNight = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Nights,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.AZIZIA_ID
                    )
                    ?.Nights;

                ViewBag.MinaPackageCheckIn = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckInTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MINA_ID
                    )
                    ?.ActualCheckInTime;

                ViewBag.MinaPackageCheckOut = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.ActualCheckOutTime,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MINA_ID
                    )
                    ?.ActualCheckOutTime;

                ViewBag.MinaPackageDay = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Days,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MINA_ID
                    )
                    ?.Days;

                ViewBag.MinaPackageNight = _context
                    .PackageAccomodtions.Select(e => new
                    {
                        e.PackageId,
                        e.PlaceId,
                        e.Nights,
                    })
                    .FirstOrDefault(e =>
                        e.PackageId == familyHead.PackageId && e.PlaceId == Place.MINA_ID
                    )
                    ?.Nights;
            }
            else
            {
                ViewBag.MakkahPackageRoom = _context
                    .FamilyHeadAccomodations.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.FamilyHeadId == id
                        && e.IndividualFacilityDescription.PlaceId == Place.MAKKAH_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.MedinaPackageRoom = _context
                    .FamilyHeadAccomodations.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.FamilyHeadId == id
                        && e.IndividualFacilityDescription.PlaceId == Place.MEDINA_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.AziziaPackageRoom = _context
                    .FamilyHeadAccomodations.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.FamilyHeadId == id
                        && e.IndividualFacilityDescription.PlaceId == Place.AZIZIA_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.MinaPackageRoom = _context
                    .FamilyHeadAccomodations.Include(e => e.IndividualFacilityDescription)
                    //.Where(e =>
                    //    e.IndividualFacilityDescription.HajjSeasonsId
                    //    == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    //)
                    .FirstOrDefault(e =>
                        e.FamilyHeadId == id
                        && e.IndividualFacilityDescription.PlaceId == Place.MINA_ID
                    )
                    ?.IndividualFacilityDescriptionId;

                ViewBag.MakkahPackageHotel = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.MAKKAH_ID)
                    ?.HotelId;

                ViewBag.MedinaPackageHotel = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.MEDINA_ID)
                    ?.HotelId;

                ViewBag.AziziaPackageHotel = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.AZIZIA_ID)
                    ?.HotelId;

                ViewBag.MinaPackageHotel = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.Hotel.PlaceId == Place.MINA_ID)
                    ?.HotelId;

                ViewBag.MakkahPackageCheckIn = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckIn,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MAKKAH_ID)
                    ?.CheckIn;

                ViewBag.MakkahPackageCheckOut = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckOut,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MAKKAH_ID)
                    ?.CheckOut;

                ViewBag.MakkahPackageDay = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.Day,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MAKKAH_ID)
                    ?.Day;

                ViewBag.MakkahPackageNight = _context
                   .FamilyHeadAccomodations.Include(e => e.Hotel)
                   .Select(e => new
                   {
                       e.Hotel.PlaceId,
                       e.FamilyHeadId,
                       e.Night,
                   })
                   .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MAKKAH_ID)
                   ?.Night;

                ViewBag.MedinaPackageCheckIn = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckIn,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MEDINA_ID)
                    ?.CheckIn;

                ViewBag.MedinaPackageCheckOut = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckOut,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MEDINA_ID)
                    ?.CheckOut;

                ViewBag.MedinaPackageDay = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.Day,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MEDINA_ID)
                    ?.Day;

                ViewBag.MedinaPackageNight = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.Night,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MEDINA_ID)
                    ?.Night;

                ViewBag.AziziaPackageCheckIn = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckIn,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.AZIZIA_ID)
                    ?.CheckIn;

                ViewBag.AziziaPackageCheckOut = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckOut,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.AZIZIA_ID)
                    ?.CheckOut;

                ViewBag.AziziaPackageDay = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.Day,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.AZIZIA_ID)
                    ?.Day;

                ViewBag.AziziaPackageNight = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.Night,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.AZIZIA_ID)
                    ?.Night;

                ViewBag.MinaPackageCheckIn = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckIn,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MINA_ID)
                    ?.CheckIn;

                ViewBag.MinaPackageCheckOut = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.CheckOut,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MINA_ID)
                    ?.CheckOut;

                ViewBag.MinaPackageDay = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.Day,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MINA_ID)
                    ?.Day;

                ViewBag.MinaPackageNight = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.Hotel.PlaceId,
                        e.FamilyHeadId,
                        e.Day,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MINA_ID)
                    ?.Day;

                ViewBag.MakkahFloorNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.FloorNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MAKKAH_ID)
                    ?.FloorNumber;

                ViewBag.MakkahRoomNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.RoomNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MAKKAH_ID)
                    ?.RoomNumber;

                ViewBag.MedinahFloorNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.FloorNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MEDINA_ID)
                    ?.FloorNumber;

                ViewBag.MedinahRoomNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.RoomNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MEDINA_ID)
                    ?.RoomNumber;

                ViewBag.AziziaFloorNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.FloorNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.AZIZIA_ID)
                    ?.FloorNumber;

                ViewBag.AziziaRoomNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.RoomNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.AZIZIA_ID)
                    ?.RoomNumber;

                ViewBag.MinaFloorNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.FloorNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MINA_ID)
                    ?.FloorNumber;

                ViewBag.MinaRoomNumber = _context
                    .FamilyHeadAccomodations.Include(e => e.Hotel)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Hotel.PlaceId,
                        e.RoomNumber,
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == id && e.PlaceId == Place.MINA_ID)
                    ?.RoomNumber;
            }

            ViewBag.Hotels = _context.HotelsWithoutTrash.ToList();

            var headid = _context
                .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == familyHead.FamilyHeadId)
                .HeadId;
            var headdata = _context.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == headid);

            var nominee = _context.FamilyHeadNominees.Where(e => e.FamilyHeadId == id);
            var nomineehead = _context.FamilyHeadNominees.Where(e => e.FamilyHeadId == headid);
            if (nominee.Count() == 0)
            {
                ViewBag.FamilyHeadNominees = nomineehead;
            }
            else
            {
                ViewBag.FamilyHeadNominees = nominee;
            }

            ViewBag.FamilyHeadAddresses = _context.FamilyHeadAddresses.Where(e =>
                e.FamilyHeadId == id
            );
            ViewBag.FamilyHeadContacts = _context.FamilyHeadContacts.Where(e =>
                e.FamilyHeadId == id
            );
            ViewBag.FamilyHeadEmails = _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == id);
            ViewBag.FamilyHeadExtras = _context.FamilyHeadExtras.Where(e => e.FamilyHeadId == id);
            ViewBag.FamilyHeadRemarks = _context.FamilyHeadRemarks.Where(e => e.FamilyHeadId == id);
            //ViewBag.FamilyHeadNominees = _context.FamilyHeadNominees.Where(e => e.FamilyHeadId == headid);
            ViewBag.FamilyHeadDocuments = _context
                .FamilyHeadDocuments.Include(e => e.RequiredDocument)
                .Where(e => e.FamilyHeadId == id);

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.References = _context.ReferencesWithoutTrash;
            ViewBag.FamilyMembers = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed && !e.IsHead)
                .ToList();

            ViewBag.FamilyMembersWithHead = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .ToList();

            ViewBag.Currencies = Enum.GetValues(typeof(Currencies))
                .Cast<Currencies>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.Currency = _context.Currencies.Where(e => !e.Trashed).ToList();

            ViewBag.Packages = _context
                .UserCompanies.Where(uc =>
                    uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))
                )
                .Join(
                    _context.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId }
                )
                .Join(
                    _context.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) =>
                        new
                        {
                            uc.UserId,
                            uc.CompanyId,
                            p.Name,
                            p.PackageId,
                            p.Code,
                            p.HajjSeasonsId,
                        }
                )
                .Where(e =>
                    e.HajjSeasonsId
                    == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                );

            ViewBag.AssignedPackageFacilities = _context.FamilyHeadAdditionalFacilities.Where(e =>
                e.FamilyHeadId == id
            );

            /*ViewBag.PackageFacilities = _context.AdditionalFacilities
                .Include(e => e.IndividiualFacilityDescription)
                .Where(e => e.PackageId == familyHead.PackageId);*/

            ViewBag.PackageFacilities = _context
                .PackageCategoryFacilities.Include(p => p.IndividualFacilityDescription)
                .Where(e => e.PackageId == familyHead.PackageId)
                .ToList();

            ViewBag.Vaccines = _context.VaccinesWithoutTrash;

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            var AssignAddresshead = _context.FamilyHeadAddresses.Where(e =>
                e.FamilyHeadId == headid
            );
            var AssignAddress = _context.FamilyHeadAddresses.Where(e => e.FamilyHeadId == id);
            if (AssignAddress.Count() == 0)
            {
                ViewBag.AssignedAddresses = AssignAddresshead;
            }
            else
            {
                ViewBag.AssignedAddresses = AssignAddress;
            }

            var AssignContactshead = _context.FamilyHeadContacts.Where(e =>
                e.FamilyHeadId == headid
            );
            var AssignContacts = _context.FamilyHeadContacts.Where(e => e.FamilyHeadId == id);
            if (AssignContacts.Count() == 0)
            {
                ViewBag.AssignedContacts = AssignContactshead;
            }
            else
            {
                ViewBag.AssignedContacts = AssignContacts;
            }

            var AssignedEmailshead = _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == headid);
            var AssignedEmails = _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == id);
            if (AssignedEmails.Count() == 0)
            {
                ViewBag.AssignedEmails = AssignedEmailshead;
            }
            else
            {
                ViewBag.AssignedEmails = AssignedEmails;
            }

            //ViewBag.AssignedAddresses = _context.FamilyHeadAddresses.Where(e => e.FamilyHeadId == headid);
            //ViewBag.AssignedContacts = _context.FamilyHeadContacts.Where(e => e.FamilyHeadId == headid);
            //ViewBag.AssignedEmails = _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == headid);

            FamilyHead mehram = null;
            if (familyHead.MehramFamilyHeadId is not null)
            {
                mehram = _context.FamilyHeads.FirstOrDefault(e =>
                    e.FamilyHeadId == familyHead.MehramFamilyHeadId
                );
            }

            FamilyHeadEditRequest request = new FamilyHeadEditRequest
            {
                AccomodationAsPerPackage = familyHead.AccomodationAsPerPackage,
                TrainAsPerPackage = familyHead.TrainAsPerPackage,
                FlightAsPerPackage = familyHead.FlightAsPerPackage,
                RouteAsPerPackage = familyHead.RouteAsPerPackage,
                GeneralDiscount = familyHead.GeneralDiscount,
                Currencies = familyHead.Currencies,
                POD = familyHead.POD,
                FamilyHeadRelationId = familyHead.FamilyHeadRelationId,
                YearsPerformedHajj = familyHead.YearsPerformedHajj,
                FlightTicketType = familyHead.FlightTicketType,
                ApplicantType = familyHead.ApplicantType,
                RelationWithMehramId = familyHead.RelationWithMehramId,
                MehramCNIC = mehram?.CNIC,
                FamilyHeadId = id,
                SPOId = familyHead.SPOId,
                FeedBy = familyHead.FeedBy,
                HonorificsId = familyHead.HonorificsId,
                SurName = familyHead.SurName,
                GivenName = familyHead.GivenName,
                FatherName = familyHead.FatherHusbandName,
                CNIC = familyHead.CNIC,
                DateOfBirth = familyHead.DateOfBirth,
                PlaceOfBirth = familyHead.PlaceOfBirth,
                Nationality = familyHead.Nationality,
                Gender = familyHead.Gender,
                BloodGroup = familyHead.BloodGroup,
                FiqahId = familyHead.FiqahId,
                IsEmployed = familyHead.IsEmployed,
                IsMarried = familyHead.IsMarried,
                EmployedAt = familyHead.EmployedAt,
                Designation = familyHead.Designation,
                DesignationId = familyHead.DesignationId,
                Residency = familyHead.Residency,
                PassportType = familyHead.PassportType,
                NationalityId = familyHead.NationalityId,
                PassportTypeId = familyHead.PassportTypeId,
                CountryCodeId = familyHead.CountryCodeId,
                CountryId = familyHead.CountryId,
                CountryCode = familyHead.CountryCode,
                PassportNumber = familyHead.PassportNumber,
                PassportDateOfIssue = familyHead?.PassportDateOfIssue,
                PassportDateOfExpiry = familyHead?.PassportDateOfExpiry,
                IssuingAuthority = familyHead.IssuingAuthority,
                TrackingNumber = familyHead.TrackingNumber,
                BookletNumber = familyHead.BookletNumber,
                //PassportScanType = familyHead.PassportScanType,
                //PassportMRZScan = familyHead.PassportMRZScan,
                //VisaScanType = familyHead.VisaScanType,
                //VisaMRZScan = familyHead.VisaMRZScan,
                FileNumber = familyHead.FileNumber,
                TokenRemarks = familyHead.TokenRemarks,
                VisaRemarks = familyHead.VisaRemarks,
                //FFNumber = familyHead.FFNumber,

                ReferenceId = familyHead.ReferenceId,
                //FriendCNIC = familyHead.Friend?.CNIC,

                FormNumber = familyHead.FormNumber,
                HajjAppSerialNumber = familyHead.HajjAppSerialNumber,
                HajjApplicationNumber = familyHead.HajjApplicationNumber,
                GroupCoordAppNumber = familyHead.GroupCoordAppNumber,
                SubstitutedAgainstAppNumber = familyHead.SubstitutedAgainstAppNumber,
                ENumber = familyHead.ENumber,
                EHajjId = familyHead.EHajjId,
                EHajjSerial = familyHead.EHajjSerial,
                MofaNumber = familyHead.MofaNumber,
                TicketNumber = familyHead.TicketNumber,
                VisaNumber = familyHead.VisaNumber,
                PerformedHajjInLastFiveYears = familyHead.PerformedHajjInLastFiveYears,
                WantToPerformHajjEBadal = familyHead.WantToPerformHajjEBadal,
                WantToPerformAsGroupWorker = familyHead.WantToPerformAsGroupWorker,
                Eligible = familyHead.Eligible,

                PackageId = familyHead.PackageId,

                PkrDiscountAmount = familyHead.PkrDiscountAmount,
                SarDiscountAmount = familyHead.SarDiscountAmount,
                UsdDiscountAmount = familyHead.UsdDiscountAmount,

                VaccineId1 = familyHead.VaccineId1,
                DoseDate1 = familyHead.DoseDate1,

                VaccineId2 = familyHead.VaccineId2,
                DoseDate2 = familyHead.DoseDate2,

                VaccineId3 = familyHead.VaccineId3,
                DoseDate3 = familyHead.DoseDate3,

                VaccineId4 = familyHead.VaccineId4,
                DoseDate4 = familyHead.DoseDate4,

                VaccineId5 = familyHead.VaccineId5,
                DoseDate5 = familyHead.DoseDate5,

                FlightTicketDifferencesPkr = familyHead.FlightTicketDifferencesPkr,
                FlightTicketDifferencesSar = familyHead.FlightTicketDifferencesSar,
                FlightTicketDifferencesUsd = familyHead.FlightTicketDifferencesUsd,
                TrainNumberForPackage = familyHead.TrainNumber,
                SeatNo = familyHead.SeatNo,
                CoachNo = familyHead.CoachNo,

                FlightClass = familyHead.FlightClass,

                IsHead = familyHead.IsHead,

                Tentative = familyHead.Tentative,
                //CurrencyId = familyHead.CurrencyId,
                //BasePackageAmount = familyHead.BaseAmount,
            };

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FamilyHeadEditRequest request)
        {
            ViewBag.FoodPackage = _context.FoodPackagesWithoutTrash.ToList();

            ViewBag.ExtraFacilityDesc = _context.ExtraFacilityDescriptionsWithoutTrash;
            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            var transaction = _context.Database.BeginTransaction();

            FamilyHead familyHead = _context.FamilyHeads.Find(request.FamilyHeadId);

            try
            {
                // Get Old Amount Start
                var oldfamilyHeadPackage = _context
                    .Packages.Select(e => new
                    {
                        e.PackageId,
                        e.CompanyId,
                        e.SARROE,
                        e.USDROE,
                        e.AdultPkr,
                        e.AdultSar,
                        e.AdultUsd,
                        e.ChildPkr,
                        e.ChildSar,
                        e.ChildUsd,
                        e.InfantPkr,
                        e.InfantSar,
                        e.InfantUsd,
                        e.HajjDues,
                        e.FlightTicketPricePkr,
                        e.FlightTicketPriceSar,
                        e.FlightTicketPriceUsd,
                    })
                    .FirstOrDefault(e => e.PackageId == request.PackageId);

                var oldpackageAddonPrices = _context
                    .PackageAddons.Where(e => e.PackageId == oldfamilyHeadPackage.PackageId)
                    .Select(e => e.PkrPrice)
                    .ToList();

                var oldpackageAdditionaFacPrices = _context
                    .FamilyHeadAdditionalFacilities.Include(e => e.AdditionalFacilities)
                    .Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ToList();

                var oldextraFacPrices = _context
                    .FamilyHeadExtras.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .Select(e => e.PricePkr)
                    .ToList();

                float oldpackageTotalPkr = 0;
                oldpackageTotalPkr +=
                    oldfamilyHeadPackage.FlightTicketPricePkr
                        + familyHead.FlightTicketDifferencesPkr
                    ?? 0f;

                for (int i = 0; i < oldpackageAddonPrices.Count; i++)
                {
                    var _curr = oldpackageAddonPrices.ElementAt(i);
                    oldpackageTotalPkr += _curr;
                }

                for (int i = 0; i < oldextraFacPrices.Count; i++)
                {
                    var _curr = oldextraFacPrices.ElementAt(i);
                    oldpackageTotalPkr += _curr ?? 0f;
                }

                // ----------------------- //

                if (DateTimeManager.CalculateAge(request.DateOfBirth) < 2)
                {
                    oldpackageTotalPkr += oldfamilyHeadPackage?.InfantPkr ?? 0;

                    for (int i = 0; i < (oldpackageAdditionaFacPrices?.Count ?? 0); i++)
                    {
                        var _curr = oldpackageAdditionaFacPrices.ElementAt(i);
                        oldpackageTotalPkr += _curr.AdditionalFacilities.InfantPkr ?? 0f;
                    }
                }
                else if (DateTimeManager.CalculateAge(request.DateOfBirth) < 12)
                {
                    oldpackageTotalPkr += (oldfamilyHeadPackage?.ChildPkr ?? 0);
                    for (int i = 0; i < (oldpackageAdditionaFacPrices?.Count ?? 0); i++)
                    {
                        var _curr = oldpackageAdditionaFacPrices.ElementAt(i);
                        oldpackageTotalPkr += _curr.AdditionalFacilities.ChildPkr ?? 0f;
                    }
                }
                else if (DateTimeManager.CalculateAge(request.DateOfBirth) >= 12)
                {
                    oldpackageTotalPkr += (oldfamilyHeadPackage?.AdultPkr ?? 0);
                    for (int i = 0; i < (oldpackageAdditionaFacPrices?.Count ?? 0); i++)
                    {
                        var _curr = oldpackageAdditionaFacPrices.ElementAt(i);
                        oldpackageTotalPkr += _curr.AdditionalFacilities.AdultPkr ?? 0f;
                    }
                }

                // Get Old Amount End

                ViewBag.PackageId = familyHead.PackageId;
                ViewBag.Vehicles = _context.VehiclesWithoutTrash;
                /*ViewBag.FamilyHeadTransports = _context.FamilyHeadTransports
                    .Include(e => e.Route)
                    .Where(e => e.FamilyHeadId == request.FamilyHeadId);*/

                /*var travelRouteId = familyHead.Package.TravelRouteId;

                var result = _context.TravelRoutes
                    .Where(tr => tr.TravelRouteId == travelRouteId)
                    .Select(tr => new
                    {
                        TravelRoute = tr,
                        TravelRouteStops = _context.TravelRouteStops
                            .Where(trs => trs.TravelRouteId == tr.TravelRouteId)
                            .ToList(),
                        FamilyHeadTransports = _context.FamilyHeadTransports
                            .Where(fht => fht.RouteId == tr.TravelRouteId && (fht.FamilyHeadId == request.FamilyHeadId))
                            .ToList(),
                        Routes = _context.Routes
                            .Where(r => _context.TravelRouteStops
                                .Any(trs => trs.TravelRouteId == tr.TravelRouteId && trs.RouteId == r.RouteId))
                            .ToList()
                    })
                    .FirstOrDefault();

                ViewBag.FamilyHeadTransports = result;*/



                ViewBag.MehramRelations = _context.MehramRelationsWithoutTrash;
                ViewBag.NomineeRels = _context.NomineeRelationsWithoutTrash;
                ViewBag.SPOs = _context.SPOsWithoutTrash;
                ViewBag.FamilyHeadAddresses = _context.FamilyHeadAddresses.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.FamilyHeadContacts = _context.FamilyHeadContacts.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.FamilyHeadEmails = _context.FamilyHeadEmails.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.FamilyHeadExtras = _context.FamilyHeadExtras.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.FamilyHeadRemarks = _context.FamilyHeadRemarks.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.FamilyHeadNominees = _context.FamilyHeadNominees.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.FamilyHeadDocuments = _context
                    .FamilyHeadDocuments.Include(e => e.RequiredDocument)
                    .Where(e => e.FamilyHeadId == request.FamilyHeadId);

                ViewBag.Honorifics = _context.HonorificsWithoutTrash;
                ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
                ViewBag.Tehsils = _context.TehsilsWithoutTrash;

                ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
                ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
                ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

                ViewBag.References = _context.ReferencesWithoutTrash;

                ViewBag.Packages = _context
                    .UserCompanies.Where(uc =>
                        uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))
                    )
                    .Join(
                        _context.CompaniesWithoutTrash,
                        uc => uc.CompanyId,
                        c => c.CompanyId,
                        (uc, c) => new { uc.UserId, c.CompanyId }
                    )
                    .Join(
                        _context.PackagesWithoutTrash,
                        uc => uc.CompanyId,
                        p => p.CompanyId,
                        (uc, p) =>
                            new
                            {
                                uc.UserId,
                                uc.CompanyId,
                                p.Name,
                                p.PackageId,
                                p.Code,
                            }
                    );

                ViewBag.AssignedPackageFacilities = _context.FamilyHeadAdditionalFacilities.Where(
                    e => e.FamilyHeadId == request.FamilyHeadId
                );

                /*ViewBag.PackageFacilities = _context.AdditionalFacilities
                .Include(e => e.IndividiualFacilityDescription)
                    .Where(e => e.PackageId == familyHead.PackageId);*/

                ViewBag.PackageFacilities = _context
                    .PackageCategoryFacilities.Where(e => e.PackageId == request.PackageId)
                    .ToList();

                ViewBag.Vaccines = _context.VaccinesWithoutTrash;

                ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

                ViewBag.AssignedAddresses = _context.FamilyHeadAddresses.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.AssignedContacts = _context.FamilyHeadContacts.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );
                ViewBag.AssignedEmails = _context.FamilyHeadEmails.Where(e =>
                    e.FamilyHeadId == request.FamilyHeadId
                );

                //if (!ModelState.IsValid) return View(request);

                //FamilyHead friend = _context.FamilyHeads.FirstOrDefault(e => e.CNIC == request.FriendCNIC);

                if (request.MehramCNIC is not null && !request.MehramCNIC.Equals(""))
                {
                    FamilyHead mehram = _context.FamilyHeads.FirstOrDefault(e =>
                        e.CNIC == request.MehramCNIC && !e.Trashed
                    );
                    familyHead.MehramFamilyHeadId = mehram?.FamilyHeadId;
                }
                else
                {
                    familyHead.MehramFamilyHeadId = null;
                }

                familyHead.POD = request.POD;
                familyHead.Tentative = request.Tentative;
                familyHead.YearsPerformedHajj = request.YearsPerformedHajj;
                familyHead.ApplicantType = request.ApplicantType;
                familyHead.RelationWithMehramId = request.RelationWithMehramId;
                familyHead.SPOId = request.SPOId;
                familyHead.FeedBy = request.FeedBy?.ToUpper();
                familyHead.HonorificsId = request.HonorificsId;
                familyHead.SurName = request.SurName?.ToUpper();
                familyHead.GivenName = request.GivenName?.ToUpper();
                familyHead.FatherHusbandName = request.FatherName?.ToUpper();
                familyHead.CNIC = request.CNIC;
                familyHead.DateOfBirth = request.DateOfBirth;
                familyHead.PlaceOfBirth = request.PlaceOfBirth?.ToUpper();
                familyHead.Nationality = request.Nationality?.ToUpper();
                familyHead.Gender = request.Gender;
                familyHead.BloodGroup = request.BloodGroup;
                familyHead.FiqahId = request.FiqahId;
                familyHead.IsMarried = request.IsMarried;
                familyHead.IsEmployed = request.IsEmployed;
                familyHead.EmployedAt = request.EmployedAt?.ToUpper();
                familyHead.Designation = request.Designation?.ToUpper();
                familyHead.DesignationId = request.DesignationId;
                familyHead.Residency = request.Residency?.ToUpper();
                familyHead.PassportType = request.PassportType;
                familyHead.CountryCode = request.CountryCode?.ToUpper();
                familyHead.PassportNumber = request.PassportNumber?.ToUpper();
                familyHead.PassportDateOfIssue = request.PassportDateOfIssue;
                familyHead.PassportDateOfExpiry = request.PassportDateOfExpiry;
                familyHead.IssuingAuthority = request.IssuingAuthority?.ToUpper();
                familyHead.TrackingNumber = request.TrackingNumber?.ToUpper();
                familyHead.BookletNumber = request.BookletNumber?.ToUpper();
                //familyHead.PassportScanType = request.PassportScanType;
                //familyHead.PassportMRZScan = request.PassportMRZScan;
                familyHead.VisaNumber = request.VisaNumber;
                //familyHead.VisaMRZScan = request.VisaMRZScan;
                familyHead.FileNumber = request.FileNumber;
                familyHead.TokenRemarks = request.TokenRemarks;

                familyHead.ReferenceId = request.ReferenceId;
                //familyHead.FriendId = friend?.FamilyHeadId;

                familyHead.FormNumber = request.FormNumber;
                familyHead.HajjAppSerialNumber = request.HajjAppSerialNumber;
                familyHead.HajjApplicationNumber = request.HajjApplicationNumber;
                familyHead.GroupCoordAppNumber = request.GroupCoordAppNumber;
                familyHead.SubstitutedAgainstAppNumber = request.SubstitutedAgainstAppNumber;
                familyHead.ENumber = request.ENumber;
                familyHead.EHajjId = request.EHajjId;
                familyHead.EHajjSerial = request.EHajjSerial;
                familyHead.MofaNumber = request.MofaNumber;
                familyHead.TicketNumber = request.TicketNumber;
                familyHead.VisaNumber = request.VisaNumber;

                familyHead.PerformedHajjInLastFiveYears = request.PerformedHajjInLastFiveYears;
                familyHead.WantToPerformHajjEBadal = request.WantToPerformHajjEBadal;
                familyHead.WantToPerformAsGroupWorker = request.WantToPerformAsGroupWorker;
                familyHead.Eligible = request.Eligible;

                familyHead.PackageId = request.PackageId;
                // familyHead.FlightId = request.FlightId;

                familyHead.PkrDiscountAmount = request.PkrDiscountAmount;
                familyHead.SarDiscountAmount = request.SarDiscountAmount;
                familyHead.UsdDiscountAmount = request.UsdDiscountAmount;

                familyHead.VaccineId1 = request.VaccineId1;
                familyHead.VaccineId2 = request.VaccineId2;
                familyHead.VaccineId3 = request.VaccineId3;
                familyHead.VaccineId4 = request.VaccineId4;
                familyHead.VaccineId5 = request.VaccineId5;

                familyHead.DoseDate1 = request.DoseDate1;
                familyHead.DoseDate2 = request.DoseDate2;
                familyHead.DoseDate3 = request.DoseDate3;
                familyHead.DoseDate4 = request.DoseDate4;
                familyHead.DoseDate5 = request.DoseDate5;

                familyHead.FlightTicketType = request.FlightTicketType;
                familyHead.FlightTicketDifferencesPkr = request.FlightTicketDifferencesPkr;
                familyHead.FlightTicketDifferencesSar = request.FlightTicketDifferencesSar;
                familyHead.FlightTicketDifferencesUsd = request.FlightTicketDifferencesUsd;
                familyHead.FlightClass = request.FlightClass;
                familyHead.FamilyHeadRelationId = request.FamilyHeadRelationId;
                familyHead.TrainNumber = request.TrainNumberForPackage;
                familyHead.SeatNo = request.SeatNo;
                familyHead.CoachNo = request.CoachNo;

                _context.SaveChanges();

                _context
                    .FamilyHeadAddresses.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.MemberAddressTypeIds?.Count ?? 0); i++)
                {
                    _context.FamilyHeadAddresses.Add(
                        new FamilyHeadAddress
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            MemberAddressTypeId = request.MemberAddressTypeIds[i],
                            Address = request.MemberAddresses[i],
                            Country = request.MemberAddressCountries[i],
                            City = request.MemberAddressCities[i],
                            ZipCode = request.MemberAddressZipCodes[i],
                            PoBox = request.MemberAddressPoBoxes[i],
                            Preferred = request.MemberAddressPreferred[i],
                            TehsilId = request.MemberAddressTehsilCodes[i],
                        }
                    );
                }

                _context.SaveChanges();

                _context
                    .FamilyHeadContacts.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.MemberContactTypeIds?.Count ?? 0); i++)
                {
                    _context.FamilyHeadContacts.Add(
                        new FamilyHeadContact
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            MemberContactTypeId = request.MemberContactTypeIds[i],
                            Contact = request.MemberContacts[i],
                            Preferred = request.MemberContactPreferred[i],
                        }
                    );
                }

                _context.SaveChanges();

                _context
                    .FamilyHeadEmails.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.MemberEmailTypeIds?.Count ?? 0); i++)
                {
                    _context.FamilyHeadEmails.Add(
                        new FamilyHeadEmail
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            MemberEmailTypeId = request.MemberEmailTypeIds[i],
                            Email = request.MemberEmails[i],
                            Preferred = request.MemberEmailPreferred[i],
                        }
                    );
                }

                _context.SaveChanges();

                _context
                    .FamilyHeadExtras.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.FacilityCode?.Count ?? 0); i++)
                {
                    _context.FamilyHeadExtras.Add(
                        new FamilyHeadExtra
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            //Code = request.FacilityCode.Count > 0 ? request.FacilityCode[i] : "",
                            // Description = request.FacilityDesc[i],
                            ExtraFacilityDescriptionId =
                                request.ExtraFacilityIds != null ? request.ExtraFacilityIds[i] : 0,
                            PricePkr = request.FacilityPrice != null ? request.FacilityPrice[i] : 0,
                            PriceSar =
                                request.FacilityPriceSar != null ? request.FacilityPriceSar[i] : 0,
                            PriceUsd =
                                request.FacilityPriceUsd != null ? request.FacilityPriceUsd[i] : 0,
                        }
                    );
                }

                _context.SaveChanges();

                _context
                    .FamilyHeadRemarks.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.Remarks?.Count ?? 0); i++)
                {
                    _context.FamilyHeadRemarks.Add(
                        new FamilyHeadRemark
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            Remarks = request.Remarks[i],
                        }
                    );
                }

                _context.SaveChanges();

                _context
                    .FamilyHeadNominees.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.NomineeNames?.Count ?? 0); i++)
                {
                    _context.FamilyHeadNominees.Add(
                        new FamilyHeadNominee
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            Name = request.NomineeNames[i],
                            NomineeRelationId = request.NomineeRelations[i],
                            CNIC = request.NomineeCNICs[i],
                            Address = request.NomineeAddresses[i],
                            Country = request.NomineeCountries[i],
                            City = request.NomineeCities[i],
                            ZipCode = request.NomineeZipCodes[i],
                            PoBox = request.NomineePoBoxes[i],
                            Contact = request.NomineeContacts[i],
                            Email = request.NomineeEmails[i],
                            Preferred = request.NomineePreferred[i],
                        }
                    );
                }

                _context.SaveChanges();

                _context
                    .FamilyHeadDocuments.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.RequiredDocumentIds?.Count ?? 0); i++)
                {
                    _context.FamilyHeadDocuments.Add(
                        new FamilyHeadDocument
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            RequiredDocumentId = request.RequiredDocumentIds[i],
                            Recieved = request.Recieved[i],
                            Remarks = request.DocumentRemarks[i],
                        }
                    );
                }

                _context.SaveChanges();

                _context
                    .FamilyHeadAdditionalFacilities.Where(e =>
                        e.FamilyHeadId == request.FamilyHeadId
                    )
                    .ExecuteDelete();

                for (int i = 0; i < (request.PackageAdditionalFacilitiesIds?.Count ?? 0); i++)
                {
                    if (request.PackageAdditionalFacilitiesChecked[i] == false)
                        continue;

                    _context.FamilyHeadAdditionalFacilities.Add(
                        new FamilyHeadAdditionalFacilities
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            AdditionalFacilitiesId = request.PackageAdditionalFacilitiesIds[i],
                        }
                    );
                }

                _context
                    .FamilyHeadTransports.Where(e => e.FamilyHeadId == request.FamilyHeadId)
                    .ExecuteDelete();

                for (int i = 0; i < (request.RouteIds?.Count ?? 0); i++)
                {
                    _context.FamilyHeadTransports.Add(
                        new FamilyHeadTransport
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            RouteId = request.RouteIds[i],
                            CheckIn = request.CheckIns[i],
                            //CheckOut = request.CheckOuts[i],
                            /*TimeIn = request.TimeIns[i],
                            TimeOut = request.TimeOuts[i],*/
                            TransportType = request.TransportTypes[i],
                            VehicleId = request.VehicleIds[i],
                        }
                    );
                }

                _context.SaveChanges();

                var familyHeadPackage = _context
                    .Packages.Select(e => new
                    {
                        e.PackageId,
                        e.CompanyId,
                        e.SARROE,
                        e.USDROE,
                        e.AdultPkr,
                        e.AdultSar,
                        e.AdultUsd,
                        e.ChildPkr,
                        e.ChildSar,
                        e.ChildUsd,
                        e.InfantPkr,
                        e.InfantSar,
                        e.InfantUsd,
                        e.HajjDues,
                        e.FlightTicketPricePkr,
                        e.FlightTicketPriceSar,
                        e.FlightTicketPriceUsd,
                    })
                    .FirstOrDefault(e => e.PackageId == request.PackageId);

                var packageAddonPrices = _context
                    .PackageAddons.Where(e => e.PackageId == familyHeadPackage.PackageId)
                    .Select(e => e.PkrPrice)
                    .ToList();

                var packageAdditionaFacPrices = _context
                    .FamilyHeadAdditionalFacilities.Include(e => e.AdditionalFacilities)
                    .Where(e => e.FamilyHeadId == familyHead.FamilyHeadId)
                    .ToList();

                float packageTotalPkr = 0;

                for (int i = 0; i < (packageAddonPrices?.Count ?? 0); i++)
                {
                    var _curr = packageAddonPrices.ElementAt(i);
                    packageTotalPkr += _curr;
                }

                for (int i = 0; i < (request.FacilityCode?.Count ?? 0); i++)
                {
                    packageTotalPkr += request.FacilityPrice[i] ?? 0f;
                }

                // ----------------------- //

                if (DateTimeManager.CalculateAge(request.DateOfBirth) < 2)
                {
                    packageTotalPkr +=
                        (familyHeadPackage?.InfantPkr ?? 0f) - request.PkrDiscountAmount;

                    for (int i = 0; i < (packageAdditionaFacPrices?.Count ?? 0); i++)
                    {
                        var _curr = packageAdditionaFacPrices.ElementAt(i);
                        packageTotalPkr += _curr.AdditionalFacilities.InfantPkr ?? 0f;
                    }
                }
                else if (DateTimeManager.CalculateAge(request.DateOfBirth) < 12)
                {
                    packageTotalPkr +=
                        (familyHeadPackage?.ChildPkr ?? 0f) - request.PkrDiscountAmount;
                    for (int i = 0; i < (packageAdditionaFacPrices?.Count ?? 0); i++)
                    {
                        var _curr = packageAdditionaFacPrices.ElementAt(i);
                        packageTotalPkr += _curr.AdditionalFacilities.ChildPkr ?? 0f;
                    }
                }
                else if (DateTimeManager.CalculateAge(request.DateOfBirth) >= 12)
                {
                    packageTotalPkr +=
                        (familyHeadPackage?.AdultPkr ?? 0f) - request.PkrDiscountAmount;
                    for (int i = 0; i < (packageAdditionaFacPrices?.Count ?? 0); i++)
                    {
                        var _curr = packageAdditionaFacPrices.ElementAt(i);
                        packageTotalPkr += _curr.AdditionalFacilities.AdultPkr ?? 0f;
                    }
                }

                DateTime currentDate = DateTime.Today;
                DateTime dateTime = DateTime.Parse(currentDate.ToString());

                int headId = _context
                    .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadId)
                    .HeadId;

                int? accountId = _context
                    .FamilyHeadsWithoutTrash.Where(e => e.FamilyHeadId == headId)
                    .Select(e => e.AccountId)
                    .FirstOrDefault();

                int paymentVoucherId = _context
                    .PaymentVouchersWithoutTrash.Where(e => e.AccountId == accountId)
                    .OrderBy(e => e.PaymentVoucherId)
                    .Select(e => e.PaymentVoucherId)
                    .FirstOrDefault();

                float paymentVoucherAmount = _context
                    .PaymentVouchersWithoutTrash.Where(e => e.PaymentVoucherId == paymentVoucherId)
                    .Select(e => e.Amount)
                    .FirstOrDefault();

                packageTotalPkr += paymentVoucherAmount;

                if (request.FlightTicketType == FlightTicketType.Included)
                {
                    packageTotalPkr += familyHeadPackage.FlightTicketPricePkr;
                }
                else if (request.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                {
                    packageTotalPkr +=
                        (familyHeadPackage.FlightTicketPricePkr)
                        + (request.FlightTicketDifferencesPkr ?? 0f);
                }

                float updatedAmount = packageTotalPkr - oldpackageTotalPkr;
                PaymentVoucher paymentVoucher = _context.PaymentVouchers.Find(paymentVoucherId);
                paymentVoucher.Amount = updatedAmount;
                _context.SaveChanges();

                int voucherEntryIdDebit = _context
                    .VoucherEntries.Where(e => e.PaymentVoucherId == paymentVoucherId)
                    .OrderBy(e => e.VoucherEntryId)
                    .Select(e => e.VoucherEntryId)
                    .FirstOrDefault();

                int voucherEntryIdCredit = _context
                    .VoucherEntries.Where(e => e.PaymentVoucherId == paymentVoucherId)
                    .OrderByDescending(e => e.VoucherEntryId)
                    .Select(e => e.VoucherEntryId)
                    .FirstOrDefault();

                VoucherEntry voucherEntryDebit = _context.VoucherEntries.Find(voucherEntryIdDebit);
                voucherEntryDebit.Debit = updatedAmount;
                _context.SaveChanges();

                VoucherEntry voucherEntryCredit = _context.VoucherEntries.Find(
                    voucherEntryIdCredit
                );
                voucherEntryCredit.Credit = updatedAmount;
                _context.SaveChanges();

                transaction.Commit();

                return RedirectToAction(nameof(ViewFamily), new { id = headId });
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return Ok(
                    new
                    {
                        e.Source,
                        e.Data,
                        e.Message,
                        ModelState.IsValid,
                        ModelState,
                        request,
                        familyHead,
                    }
                );
                return View(request);
            }
        }

        public IActionResult EditCustomerProfile(int id)
        {
            ViewBag.ViewFamilyId = _context
                .CustomerPortfoliosMembers.FirstOrDefault(e => e.CustomerPortfolioId == id)
                .HeadId;

            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.NomineeRels = _context.NomineeRelationsWithoutTrash;

            ViewBag.PassportType = _context.PassportTypes.Where(e => !e.Trashed).ToList();

            ViewBag.Nationality = _context.Nationalities.Where(e => !e.Trashed).ToList();

            ViewBag.Country = _context.Countries.Where(e => !e.Trashed).ToList();

            ViewBag.City = _context.Cities.Where(e => !e.Trashed).ToList();

            ViewBag.CountryCode = _context.CountryCodes.Where(e => !e.Trashed).ToList();

            ViewBag.HajjDepartment = _context.HajjDepartments.Where(e => !e.Trashed).ToList();

            CustomerPortfolio customerPortfolio = _context
                .CustomerPortfolios.Where(e => !e.Trashed)
                .FirstOrDefault(e => e.CustomerPortfolioId == id);

            ViewBag.FeedBy = customerPortfolio.FeedBy;

            var headid = _context
                .CustomerPortfoliosMembers.FirstOrDefault(e =>
                    e.CustomerPortfolioId == customerPortfolio.CustomerPortfolioId
                )
                .HeadId;
            var headdata = _context.CustomerPortfolios.FirstOrDefault(e =>
                e.CustomerPortfolioId == headid
            );

            ViewBag.FamilyHeadAddresses = _context.CustomerPortfolioAddress.Where(e =>
                e.CustomerPortfolioId == id
            );
            ViewBag.FamilyHeadContacts = _context.CustomerPortfolioContacts.Where(e =>
                e.CustomerPortfolioId == id
            );
            ViewBag.FamilyHeadEmails = _context.CustomerPortfolioEmails.Where(e =>
                e.CustomerPortfolioId == id
            );
            ViewBag.FamilyHeadDocuments = _context
                .CustomerPortfolioDocuments.Include(e => e.RequiredDocument)
                .Where(e => e.CustomerPortfolioId == id);

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.References = _context.ReferencesWithoutTrash;
            ViewBag.FamilyMembers = _context
                .CustomerPortfoliosMembers.Include(e => e.CustomerPortfolio)
                .Where(e => e.HeadId == id && !e.CustomerPortfolio.Trashed && !e.IsHead)
                .ToList();

            ViewBag.FamilyMembersWithHead = _context
                .CustomerPortfoliosMembers.Include(e => e.CustomerPortfolio)
                .Where(e => e.HeadId == id && !e.CustomerPortfolio.Trashed)
                .ToList();

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            var AssignAddresshead = _context.CustomerPortfolioAddress.Where(e =>
                e.CustomerPortfolioId == headid
            );
            var AssignAddress = _context.CustomerPortfolioAddress.Where(e =>
                e.CustomerPortfolioId == id
            );
            if (AssignAddress.Count() == 0)
            {
                ViewBag.AssignedAddresses = AssignAddresshead;
            }
            else
            {
                ViewBag.AssignedAddresses = AssignAddress;
            }

            var AssignContactshead = _context.CustomerPortfolioContacts.Where(e =>
                e.CustomerPortfolioId == headid
            );
            var AssignContacts = _context.CustomerPortfolioContacts.Where(e =>
                e.CustomerPortfolioId == id
            );
            if (AssignContacts.Count() == 0)
            {
                ViewBag.AssignedContacts = AssignContactshead;
            }
            else
            {
                ViewBag.AssignedContacts = AssignContacts;
            }

            var AssignedEmailshead = _context.CustomerPortfolioEmails.Where(e =>
                e.CustomerPortfolioId == headid
            );
            var AssignedEmails = _context.CustomerPortfolioEmails.Where(e =>
                e.CustomerPortfolioId == id
            );
            if (AssignedEmails.Count() == 0)
            {
                ViewBag.AssignedEmails = AssignedEmailshead;
            }
            else
            {
                ViewBag.AssignedEmails = AssignedEmails;
            }

            CustomerPortfolio mehram = null;

            FamilyHeadEditRequest request = new FamilyHeadEditRequest
            {
                CustomerPortfolioId = id,
                FamilyHeadRelationId = customerPortfolio.FamilyHeadRelationId,
                MehramCNIC = mehram?.CNIC,
                FamilyHeadId = id,
                SPOId = customerPortfolio.SPOId,
                FeedBy = customerPortfolio.FeedBy,
                HonorificsId = customerPortfolio.HonorificsId,
                SurName = customerPortfolio.SurName,
                GivenName = customerPortfolio.GivenName,
                FatherName = customerPortfolio.FatherHusbandName,
                CNIC = customerPortfolio.CNIC,
                DateOfBirth = customerPortfolio.DateOfBirth,
                PlaceOfBirth = customerPortfolio.PlaceOfBirth,
                Nationality = customerPortfolio.Nationality,
                NationalityId = customerPortfolio.NationalityId,
                Gender = customerPortfolio.Gender,
                BloodGroup = customerPortfolio.BloodGroup,
                FiqahId = customerPortfolio.FiqahId,
                IsEmployed = customerPortfolio.IsEmployed,
                IsMarried = customerPortfolio.IsMarried,
                EmployedAt = customerPortfolio.EmployedAt,
                Designation = customerPortfolio.Designation,
                Residency = customerPortfolio.Residency,
                CountryId = customerPortfolio.CountryId,
                PassportType = customerPortfolio.PassportType,
                PassportTypeId = customerPortfolio.PassportTypeId,
                CountryCode = customerPortfolio.CountryCode,
                CountryCodeId = customerPortfolio.CountryCodeId,
                CityId = customerPortfolio.CityId,
                HajjDepartmentId = customerPortfolio.HajjDepartmentId,
                PassportNumber = customerPortfolio.PassportNumber,
                PassportDateOfIssue = customerPortfolio?.PassportDateOfIssue,
                PassportDateOfExpiry = customerPortfolio?.PassportDateOfExpiry,
                IsHead = customerPortfolio.IsHead,
                Tentative = customerPortfolio.Tentative,
                CustomerRemarks = customerPortfolio.Remarks,
                Status = customerPortfolio.Status,
                ApplicantType = customerPortfolio.ApplicantType,
                ReferenceId = customerPortfolio.ReferenceId,
                DateOfEmail = customerPortfolio.DateOfEmail,
            };

            return View(request);
        }

        public IActionResult AddMember(int familyHeadId)
        {
            FamilyHead familyHead = _context.FamilyHeads.FirstOrDefault(e =>
                e.IsHead && e.FamilyHeadId == familyHeadId
            );

            ViewBag.ExtraFacilityDesc = _context.ExtraFacilityDescriptionsWithoutTrash;
            ViewBag.PackageId = familyHead.PackageId;
            ViewBag.MehramRelations = _context.MehramRelationsWithoutTrash;
            ViewBag.FeedBy = familyHead.FeedBy;
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.NomineeRels = _context.NomineeRelationsWithoutTrash;
            ViewBag.FamilyHeadAddresses = _context.FamilyHeadAddresses.Where(e =>
                e.FamilyHeadId == familyHeadId
            );

            ViewBag.Designation = _context.Designations.Where(e => !e.Trashed).ToList();

            ViewBag.PassportType = _context.PassportTypes.Where(e => !e.Trashed).ToList();

            ViewBag.Nationality = _context.Nationalities.Where(e => !e.Trashed).ToList();
            ViewBag.Country = _context.Countries.Where(e => !e.Trashed).ToList();

            ViewBag.City = _context.Cities.Where(e => !e.Trashed).ToList();

            ViewBag.CountryCode = _context.CountryCodes.Where(e => !e.Trashed).ToList();

            ViewBag.FamilyHeadContacts = _context.FamilyHeadContacts.Where(e =>
                e.FamilyHeadId == familyHeadId
            );
            ViewBag.FamilyHeadEmails = _context.FamilyHeadEmails.Where(e =>
                e.FamilyHeadId == familyHeadId
            );
            ViewBag.FamilyHeadExtras = _context.FamilyHeadExtras.Where(e =>
                e.FamilyHeadId == familyHeadId
            );
            ViewBag.FamilyHeadRemarks = _context.FamilyHeadRemarks.Where(e =>
                e.FamilyHeadId == familyHeadId
            );
            ViewBag.FamilyHeadNominees = _context.FamilyHeadNominees.Where(e =>
                e.FamilyHeadId == familyHeadId
            );

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.References = _context.ReferencesWithoutTrash;
            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.FamilyHeadPackage = _context.FamilyHeads.Find(familyHeadId).PackageId;

            /* ViewBag.PackageFacilities = _context.PackageCategoryFacilities
             .Include(e => e.IndividualFacilityDescriptionId)
                     .Where(e => e.PackageId == familyHead.PackageId);*/

            ViewBag.PackageFacilities = _context
                .PackageCategoryFacilities.Where(e => e.PackageId == familyHead.PackageId)
                .ToList();

            ViewBag.AssignedPackageFacilities = _context.FamilyHeadAdditionalFacilities.Where(e =>
                e.FamilyHeadId == familyHead.FamilyHeadId
            );

            ViewBag.Packages = _context
                .UserCompanies.Where(uc =>
                    uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))
                )
                .Join(
                    _context.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId }
                )
                .Join(
                    _context.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) =>
                        new
                        {
                            uc.UserId,
                            uc.CompanyId,
                            p.Name,
                            p.PackageId,
                            p.Code,
                        }
                );

            ViewBag.Vaccines = _context.VaccinesWithoutTrash;

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            ViewBag.Vehicles = _context.VehiclesWithoutTrash;
            ViewBag.Routes = _context.RoutesWithoutTrash;

            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            ViewBag.AssignedReference = _context.ReferencesWithoutTrash.FirstOrDefault(e =>
                e.ReferenceId == familyHead.ReferenceId
            );

            ViewBag.familyHeadId = familyHeadId;

            ViewBag.SPOId = familyHead.SPOId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMember(FamilyMemberRegisterRequest request)
        {
            FamilyHead _familyHead = _context.FamilyHeads.FirstOrDefault(e =>
                e.IsHead && e.FamilyHeadId == request.FamilyHeadId
            );
            var reqDocs = _context.RequiredDocumentsWithoutTrash.ToList();
            ViewBag.ExtraFacilityDesc = _context.ExtraFacilityDescriptionsWithoutTrash;
            ViewBag.familyHeadId = request.FamilyHeadId;
            ViewBag.PackageId = _familyHead.PackageId;
            ViewBag.MehramRelations = _context.MehramRelationsWithoutTrash;
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.NomineeRels = _context.NomineeRelationsWithoutTrash;
            ViewBag.FamilyHeadAddresses = _context.FamilyHeadAddresses.Where(e =>
                e.FamilyHeadId == request.FamilyHeadId
            );
            ViewBag.FamilyHeadContacts = _context.FamilyHeadContacts.Where(e =>
                e.FamilyHeadId == request.FamilyHeadId
            );
            ViewBag.FamilyHeadEmails = _context.FamilyHeadEmails.Where(e =>
                e.FamilyHeadId == request.FamilyHeadId
            );
            ViewBag.FamilyHeadExtras = _context.FamilyHeadExtras.Where(e =>
                e.FamilyHeadId == request.FamilyHeadId
            );
            ViewBag.FamilyHeadRemarks = _context.FamilyHeadRemarks.Where(e =>
                e.FamilyHeadId == request.FamilyHeadId
            );
            ViewBag.FamilyHeadNominees = _context.FamilyHeadNominees.Where(e =>
                e.FamilyHeadId == request.FamilyHeadId
            );

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.References = _context.ReferencesWithoutTrash;

            ViewBag.PackageFacilities = _context
                .AdditionalFacilities.Include(e => e.IndividiualFacilityDescription)
                .Where(e => e.PackageId == request.PackageId);

            ViewBag.AssignedPackageFacilities = _context.FamilyHeadAdditionalFacilities.Where(e =>
                e.FamilyHeadId == _familyHead.FamilyHeadId
            );

            ViewBag.Packages = _context
                .UserCompanies.Where(uc =>
                    uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"))
                )
                .Join(
                    _context.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId }
                )
                .Join(
                    _context.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) =>
                        new
                        {
                            uc.UserId,
                            uc.CompanyId,
                            p.Name,
                            p.PackageId,
                            p.Code,
                        }
                );

            ViewBag.Vaccines = _context.VaccinesWithoutTrash;

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            ViewBag.Vehicles = _context.VehiclesWithoutTrash;
            ViewBag.Routes = _context.RoutesWithoutTrash;

            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            ViewBag.AssignedReference = _context.ReferencesWithoutTrash.FirstOrDefault(e =>
                e.ReferenceId == _familyHead.ReferenceId
            );

            var transaction = _context.Database.BeginTransaction();

            try
            {
                string memberImagePath,
                    passportImagePath,
                    visaImagePath,
                    memberVaccImagePath,
                    passportTokenImagePath,
                    EhajjPath,
                    insuranceImagePath;

                if (request.MemberImage is not null)
                {
                    memberImagePath = Path.Combine(
                        _host.WebRootPath,
                        "Images",
                        "MemberImages",
                        request.MemberImage.FileName
                    );
                    using (FileStream fs = new FileStream(memberImagePath, FileMode.Create))
                    {
                        request.MemberImage.CopyTo(fs);
                    }
                }

                if (request.PassportImage is not null)
                {
                    passportImagePath = Path.Combine(
                        _host.WebRootPath,
                        "Images",
                        "PassportImages",
                        request.PassportImage.FileName
                    );
                    using (FileStream fs = new FileStream(passportImagePath, FileMode.Create))
                    {
                        request.PassportImage.CopyTo(fs);
                    }
                }

                if (request.PassportTokenImage is not null)
                {
                    passportTokenImagePath = Path.Combine(
                        _host.WebRootPath,
                        "Images",
                        "PassportTokenImages",
                        request.PassportTokenImage.FileName
                    );
                    using (FileStream fs = new FileStream(passportTokenImagePath, FileMode.Create))
                    {
                        request.PassportTokenImage.CopyTo(fs);
                    }
                }
                if (request.Ehajj is not null)
                {
                    EhajjPath = Path.Combine(
                        _host.WebRootPath,
                        "Images",
                        "Ehajj",
                        request.Ehajj.FileName
                    );
                    using (FileStream fs = new FileStream(EhajjPath, FileMode.Create))
                    {
                        request.Ehajj.CopyTo(fs);
                    }
                }

                if (request.VisaImage is not null)
                {
                    visaImagePath = Path.Combine(
                        _host.WebRootPath,
                        "Images",
                        "VisaImages",
                        request.VisaImage.FileName
                    );
                    using (FileStream fs = new FileStream(visaImagePath, FileMode.Create))
                    {
                        request.VisaImage.CopyTo(fs);
                    }
                }

                if (request.InsuranceImage is not null)
                {
                    insuranceImagePath = Path.Combine(
                        _host.WebRootPath,
                        "Images",
                        "InsuranceImages",
                        request.InsuranceImage.FileName
                    );
                    using (FileStream fs = new FileStream(insuranceImagePath, FileMode.Create))
                    {
                        request.InsuranceImage.CopyTo(fs);
                    }
                }

                if (request.MemberVaccinationImage is not null)
                {
                    memberVaccImagePath = Path.Combine(
                        _host.WebRootPath,
                        "Images",
                        "MemberVaccImages",
                        request.MemberVaccinationImage.FileName
                    );
                    using (FileStream fs = new FileStream(memberVaccImagePath, FileMode.Create))
                    {
                        request.MemberVaccinationImage.CopyTo(fs);
                    }
                }

                var settings = _context
                    .ApplicationSettings.Where(s =>
                        s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                    .FirstOrDefault();
                settings.IndividualCount++;
                _context.SaveChanges();

                FamilyHead familyHead = new()
                {
                    POD = request.POD,
                    Tentative = request.Tentative,
                    IndId = (settings.IndividualCount + 1).ToString(),
                    FcId = _familyHead.FcId,
                    YearsPerformedHajj = request.YearsPerformedHajj,
                    ApplicantType = request.ApplicantType,
                    RelationWithMehramId = request.RelationWithMehramId,
                    IsHead = false,
                    SPOId = request.SPOId,
                    FeedBy = request.FeedBy?.ToUpper(),
                    HonorificsId = request.HonorificsId,
                    SurName = request.SurName?.ToUpper(),
                    GivenName = request.GivenName?.ToUpper(),
                    MehramRelationEnum = request.MehramRelationEnum,
                    FatherHusbandName = request.FatherHusbandName?.ToUpper(),
                    CNIC = request.CNIC,
                    DateOfBirth = request.DateOfBirth,
                    PlaceOfBirth = request.PlaceOfBirth,
                    Nationality = request.Nationality?.ToUpper(),
                    Gender = request.Gender,
                    BloodGroup = request.BloodGroup,
                    FiqahId = request.FiqahId,
                    IsMarried = request.IsMarried,
                    EmployedAt = request.EmployedAt?.ToUpper(),
                    DesignationId = request.DesignationId,
                    CountryId = request.CountryId,
                    NationalityId = request.NationalityId,
                    CountryCodeId = request.CountryCodeId,
                    CountryCode = request.CountryCode,
                    PassportTypeId = request.PassportTypeId,
                    PassportNumber = request.PassportNumber?.ToUpper(),
                    PassportDateOfIssue = request.PassportDateOfIssue,
                    PassportDateOfExpiry = request.PassportDateOfExpiry,
                    IssuingAuthority = request.IssuingAuthority?.ToUpper(),
                    //TrackingNumber = request.TrackingNumber?.ToUpper(),
                    //BookletNumber = request.BookletNumber?.ToUpper(),
                    //PassportScanType = request.PassportScanType,
                    //PassportMRZScan = request.PassportMRZScan,
                    //VisaScanType = request.VisaScanType,
                    //VisaMRZScan = request.VisaMRZScan,
                    FileNumber = request.FileNumber,
                    TokenRemarks = request.TokenRemarks,
                    //BiometricCode = request.BiometricCode,
                    //BiometricVerified = request.BiometricVerified,
                    PackageId = _familyHead.PackageId,
                    AccountId = null,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    RouteAsPerPackage = true,
                    FlightAsPerPackage = true,
                    TrainAsPerPackage = true,
                    AccomodationAsPerPackage = true,
                };

                if (request.MehramCNIC is not null && !request.MehramCNIC.Equals(""))
                {
                    FamilyHead mehram = _context.FamilyHeads.FirstOrDefault(e =>
                        e.FamilyHeadId == familyHead.MehramFamilyHeadId
                    );
                    familyHead.MehramFamilyHeadId = mehram?.FamilyHeadId;
                }
                else
                {
                    familyHead.MehramFamilyHeadId = null;
                }

                if (request.MemberImage is not null)
                {
                    familyHead.VisaImage = @"Images/MemberImages/" + request.MemberImage.FileName;
                }

                if (request.PassportImage is not null)
                {
                    familyHead.PassportImage =
                        @"Images/PassportImages/" + request.PassportImage.FileName;
                }
                if (request.PassportTokenImage is not null)
                {
                    familyHead.PassportTokenImage =
                        @"Images/PassportTokenImages/" + request.PassportTokenImage.FileName;
                }
                if (request.Ehajj is not null)
                {
                    familyHead.Ehajj = @"Images/Ehajj/" + request.Ehajj.FileName;
                }
                if (request.VisaImage is not null)
                {
                    familyHead.VisaImage = @"Images/VisaImages/" + request.VisaImage.FileName;
                }

                if (request.InsuranceImage is not null)
                {
                    familyHead.InsuranceImage =
                        @"Images/InsuranceImages/" + request.InsuranceImage.FileName;
                }

                if (request.MemberVaccinationImage is not null)
                {
                    familyHead.MemberVaccinationImage =
                        @"Images/MemberVaccinationImages/"
                        + request.MemberVaccinationImage.FileName;
                }

                _context.FamilyHeads.Add(familyHead);
                _context.SaveChanges();

                _context.FamilyMembers.Add(
                    new FamilyMembers
                    {
                        FamilyHeadId = familyHead.FamilyHeadId,
                        HeadId = _familyHead.FamilyHeadId,
                        IsHead = false,
                    }
                );
                _context.SaveChanges();

                for (int i = 0; i < reqDocs.Count; i++)
                {
                    _context.FamilyHeadDocuments.Add(
                        new FamilyHeadDocument
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            RequiredDocumentId = reqDocs.ElementAt(i).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                _context.SaveChanges();

                transaction.Commit();

                return RedirectToAction(nameof(ViewFamily), new { id = request.FamilyHeadId });
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return Ok(new { ex.InnerException.Message });
            }
        }

        public IActionResult AddCustomerMember(int customerPortfolioId)
        {
            CustomerPortfolio familyHead = _context.CustomerPortfolios.FirstOrDefault(e =>
                e.IsHead && e.CustomerPortfolioId == customerPortfolioId
            );

            ViewBag.FeedBy = familyHead.FeedBy;
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.FamilyHeadAddresses = _context.CustomerPortfolioAddress.Where(e =>
                e.CustomerPortfolioId == customerPortfolioId
            );
            ViewBag.FamilyHeadContacts = _context.CustomerPortfolioContacts.Where(e =>
                e.CustomerPortfolioId == customerPortfolioId
            );
            ViewBag.FamilyHeadEmails = _context.CustomerPortfolioEmails.Where(e =>
                e.CustomerPortfolioId == customerPortfolioId
            );

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            ViewBag.customerPortfolioId = customerPortfolioId;

            ViewBag.SPOId = familyHead.SPOId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCustomerMember(FamilyMemberRegisterRequest request)
        {
            CustomerPortfolio _familyHead = _context.CustomerPortfolios.FirstOrDefault(e =>
                e.IsHead && e.CustomerPortfolioId == request.CustomerPortfolioId
            );

            var reqDocs = _context.RequiredDocumentsWithoutTrash.ToList();
            ViewBag.CustomerPortfolioId = request.CustomerPortfolioId;
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.FamilyHeadAddresses = _context.CustomerPortfolioAddress.Where(e =>
                e.CustomerPortfolioId == request.CustomerPortfolioId
            );
            ViewBag.FamilyHeadContacts = _context.CustomerPortfolioContacts.Where(e =>
                e.CustomerPortfolioId == request.CustomerPortfolioId
            );
            ViewBag.FamilyHeadEmails = _context.CustomerPortfolioEmails.Where(e =>
                e.CustomerPortfolioId == request.CustomerPortfolioId
            );

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            var transaction = _context.Database.BeginTransaction();

            try
            {
                var settings = _context
                    .ApplicationSettings.Where(s =>
                        s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                    .FirstOrDefault();
                settings.IndividualCount++;
                _context.SaveChanges();

                CustomerPortfolio familyHead = new()
                {
                    Tentative = request.Tentative,
                    ApplicantType = request.ApplicantType,
                    IsHead = false,
                    SPOId = request.SPOId,
                    FeedBy = request.FeedBy?.ToUpper(),
                    HonorificsId = request.HonorificsId,
                    SurName = request.SurName?.ToUpper(),
                    GivenName = request.GivenName?.ToUpper(),
                    FatherHusbandName = request.FatherHusbandName?.ToUpper(),
                    CNIC = request.CNIC,
                    DateOfBirth = request.DateOfBirth,
                    PlaceOfBirth = request.PlaceOfBirth,
                    Nationality = request.Nationality?.ToUpper(),
                    Gender = request.Gender,
                    BloodGroup = request.BloodGroup,
                    FiqahId = request.FiqahId,
                    IsMarried = request.IsMarried,
                    Residency = request.Residency?.ToUpper(),
                    PassportType = request.PassportType,
                    CountryCode = request.CountryCode?.ToUpper(),
                    PassportNumber = request.PassportNumber?.ToUpper(),
                    PassportDateOfIssue = request.PassportDateOfIssue,
                    PassportDateOfExpiry = request.PassportDateOfExpiry,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                };

                _context.CustomerPortfolios.Add(familyHead);
                _context.SaveChanges();

                _context.CustomerPortfoliosMembers.Add(
                    new CustomerPortfolioMembers
                    {
                        CustomerPortfolioId = familyHead.CustomerPortfolioId,
                        HeadId = _familyHead.CustomerPortfolioId,
                        IsHead = false,
                    }
                );
                _context.SaveChanges();

                for (int i = 0; i < reqDocs.Count; i++)
                {
                    _context.CustomerPortfolioDocuments.Add(
                        new CustomerPortfolioDocuments
                        {
                            CustomerPortfolioId = familyHead.CustomerPortfolioId,
                            RequiredDocumentId = reqDocs.ElementAt(i).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                _context.SaveChanges();

                transaction.Commit();

                return RedirectToAction(
                    nameof(ViewCustomerMember),
                    new { id = request.CustomerPortfolioId }
                );
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return Ok(new { ex.InnerException.Message });
            }
        }

        public IActionResult CreateTentative()
        {
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Packages = _context
                .PackagesWithoutTrash.Where(e =>
                    e.HajjSeasonsId
                    == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .Select(e => new { e.PackageId, e.Name });

            ViewBag.ContactTypes = _context.MemberContactTypes;

            return View();
        }

        [HttpPost]
        public IActionResult CreateTentative(TentativeFamilyHead request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                ViewBag.SPOs = _context.SPOsWithoutTrash;
                ViewBag.Honorifics = _context.HonorificsWithoutTrash;
                ViewBag.Packages = _context.PackagesWithoutTrash.Select(e => new
                {
                    e.PackageId,
                    e.Name,
                });

                ViewBag.ContactTypes = _context.MemberContactTypes;

                //string cnic = request.CNIC;
                //var cnicFh = _context
                //    .FamilyHeadsWithoutTrash.Select(e => new { e.CNIC, e.HajjSeasonsId })
                //    .FirstOrDefault(e =>
                //        e.CNIC == cnic
                //        && e.HajjSeasonsId
                //            == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                //    );

                //if (cnicFh is not null)
                //{
                //    ModelState.AddModelError("CNIC", "CNIC already exists");
                //}

                if (!ModelState.IsValid)
                    return View(request);

                int Pax = request.adult + request.child + request.infant;

                Fiqah fiqah = _context.FiqahsWithoutTrash.FirstOrDefault();
                Honorifics honorifics = _context.HonorificsWithoutTrash.FirstOrDefault();

                var familyHeadPackage = _context
                    .Packages.Select(e => new
                    {
                        e.PackageId,
                        e.CompanyId,
                        e.SARROE,
                        e.USDROE,
                        e.AdultPkr,
                        e.AdultSar,
                        e.AdultUsd,
                        e.ChildPkr,
                        e.ChildSar,
                        e.ChildUsd,
                        e.InfantPkr,
                        e.InfantSar,
                        e.InfantUsd,
                        e.HajjDues,
                        e.FlightTicketPricePkr,
                        e.FlightTicketPriceSar,
                        e.FlightTicketPriceUsd,
                        e.POD,
                    })
                    .FirstOrDefault(e => e.PackageId == request.PackageId);

                var packageAddonPrices = _context
                    .PackageAddons.Where(e => e.PackageId == familyHeadPackage.PackageId)
                    .Select(e => e.PkrPrice)
                    .ToList();

                var companyReceivable = _context
                    .Categories.Include(e => e.SubType)
                    .Select(e => new
                    {
                        e.SubType.CompanyId,
                        e.CategoryId,
                        e.CompanyReceivable,
                    })
                    .FirstOrDefault(e =>
                        e.CompanyReceivable && e.CompanyId == familyHeadPackage.CompanyId
                    );

                int AccountId = 1;

                float packageTotalPkr = 0;

                DateTime currentDate = DateTime.Today;
                DateTime dateTime = DateTime.Parse(currentDate.ToString());

                DateTime infantAge = DateTime.Today;
                DateTime childAge = DateTime.Today.AddYears(-3);
                DateTime adultAge = DateTime.Today.AddYears(-13);

                var settings = _context
                    .ApplicationSettings.Where(s =>
                        s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                    .FirstOrDefault();

                int lastFcId = settings.FamilyCount;
                int lastIndId = settings.IndividualCount;

                //string newFcId = "HB-" + (lastFcId + 1);
                int newIndId = lastIndId + 1;

                /*var lastReceipt = _context
                    .FamilyHeads.Where(e =>
                        e.HajjSeasonsId
                        == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                    .OrderByDescending(x => x.FcId)
                    .Select(x => x.FcId)
                    .FirstOrDefault();

                int nextReceiptNumber = 1;

                if (!string.IsNullOrEmpty(lastReceipt) && lastReceipt.StartsWith("HB-"))
                {
                    nextReceiptNumber = int.Parse(lastReceipt.Substring(3)) + 1;
                }*/

                // ✅ Fixed logic for FcId generation
                var lastFcIdNumber = _context.FamilyHeads
                     .Where(e =>
                         e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")) &&
                         e.FcId.StartsWith("HB-"))
                     .Select(e => e.FcId.Substring(3)) // Extract the numeric part after "HB-"
                     .AsEnumerable()  // Switch to LINQ to Objects (process in-memory)
                     .Select(fcId => {
                         int parsedNumber = 0;
                         int.TryParse(fcId, out parsedNumber); // Parse it locally without the out parameter in the query
                         return parsedNumber;
                     })
                     .OrderByDescending(x => x)
                     .FirstOrDefault();

                int nextReceiptNumber = lastFcIdNumber + 1;
                string newFcId = "HB-" + nextReceiptNumber;


                FamilyHead fh = new FamilyHead
                {
                    IndId = newIndId.ToString(),
                    //FcId = $"HB-{nextReceiptNumber}",
                    FcId = newFcId,
                    GivenName = request.GivenName,
                    SurName = request.SurName,
                    CNIC = request.CNIC,
                    IsHead = true,
                    PackageId = request.PackageId,
                    Tentative = true,
                    DateOfBirth = adultAge,
                    FiqahId = fiqah.FiqahId,
                    HonorificsId = honorifics.HonorificsId,
                    Gender = Gender.Male,
                    MehramRelationEnum = MehramRelationEnum.Neither,
                    PassportDateOfIssue = DateTime.Now,
                    PassportDateOfExpiry = DateTime.Now,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    POD = familyHeadPackage.POD,
                    RouteAsPerPackage = true,
                    FlightAsPerPackage = true,
                    TrainAsPerPackage = true,
                    AccomodationAsPerPackage = true,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    Currencies = Currencies.SAR,
                    TicketStatus = TktStatus.NotIssued
                };

                _context.FamilyHeads.Add(fh);
                _context.SaveChanges();

                settings.FamilyCount++;
                settings.IndividualCount++;
                _context.SaveChanges();

                FamilyHeadContact contact = new FamilyHeadContact
                {
                    Contact = request.Phone,
                    MemberContactTypeId = request.ContactTypeId,
                    FamilyHeadId = fh.FamilyHeadId,
                    Preferred = true,
                };

                _context.FamilyHeadContacts.Add(contact);
                _context.SaveChanges();

                FamilyHeadEmail email = new FamilyHeadEmail
                {
                    Email = request.Email,
                    FamilyHeadId = fh.FamilyHeadId,
                    Preferred = true,
                };

                _context.FamilyHeadEmails.Add(email);
                _context.SaveChanges();

                var reqDocs = _context.RequiredDocumentsWithoutTrash.ToList();

                for (int i = 0; i < reqDocs.Count; i++)
                {
                    _context.FamilyHeadDocuments.Add(
                        new FamilyHeadDocument
                        {
                            FamilyHeadId = fh.FamilyHeadId,
                            RequiredDocumentId = reqDocs.ElementAt(i).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                _context.SaveChanges();

                _context.FamilyMembers.Add(
                    new FamilyMembers
                    {
                        FamilyHeadId = fh.FamilyHeadId,
                        HeadId = fh.FamilyHeadId,
                        IsHead = true,
                    }
                );

                _context.SaveChanges();

                var packageAdditionaFacPrices = _context
                    .FamilyHeadAdditionalFacilities.Include(e => e.AdditionalFacilities)
                    .Where(e => e.FamilyHeadId == fh.FamilyHeadId)
                    .Select(e => e.AdditionalFacilities.AdultPkr)
                    .ToList();

                for (int i = 0; i < (packageAddonPrices?.Count ?? 0); i++)
                {
                    var _curr = packageAddonPrices.ElementAt(i);
                    packageTotalPkr += _curr;
                }

                for (int i = 0; i < (packageAdditionaFacPrices?.Count ?? 0); i++)
                {
                    var _curr = packageAdditionaFacPrices.ElementAt(i);
                    packageTotalPkr += _curr ?? 0f;
                }

                packageTotalPkr += familyHeadPackage.AdultPkr;
                packageTotalPkr += familyHeadPackage.FlightTicketPricePkr;

                for (int i = 1; i < request.adult; i++)
                {
                    FamilyHead fhm = new FamilyHead
                    {
                        FcId = newFcId,
                        IndId = (settings.IndividualCount + 1).ToString(),
                        GivenName = request.GivenName + "'s Member",
                        SurName = request.SurName + "'s Member",
                        CNIC = request.CNIC,
                        IsHead = false,
                        PackageId = request.PackageId,
                        Tentative = true,
                        DateOfBirth = adultAge,
                        FiqahId = fiqah.FiqahId,
                        HonorificsId = honorifics.HonorificsId,
                        Gender = Gender.Male,
                        MehramRelationEnum = MehramRelationEnum.Neither,
                        PassportDateOfIssue = DateTime.Now,
                        PassportDateOfExpiry = DateTime.Now,
                        SPOId = request.SPOID,
                        FeedBy = _http.HttpContext.Session.GetString("Username"),
                        RouteAsPerPackage = true,
                        FlightAsPerPackage = true,
                        TrainAsPerPackage = true,
                        AccomodationAsPerPackage = true,
                        HajjSeasonsId = Convert.ToInt32(
                            _http.HttpContext.Session.GetString("ActiveSeasonId")
                        ),
                        Currencies = Currencies.SAR,
                        TicketStatus = TktStatus.NotIssued
                    };

                    _context.FamilyHeads.Add(fhm);
                    _context.SaveChanges();

                    settings.IndividualCount++;
                    _context.SaveChanges();

                    for (int j = 0; j < reqDocs.Count; j++)
                    {
                        _context.FamilyHeadDocuments.Add(
                            new FamilyHeadDocument
                            {
                                FamilyHeadId = fhm.FamilyHeadId,
                                RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                                Recieved = false,
                                Remarks = "",
                            }
                        );
                    }

                    _context.SaveChanges();

                    FamilyHeadContact contactm = new FamilyHeadContact
                    {
                        Contact = request.Phone,
                        MemberContactTypeId = request.ContactTypeId,
                        FamilyHeadId = fhm.FamilyHeadId,
                        Preferred = true,
                    };

                    _context.FamilyHeadContacts.Add(contactm);
                    _context.SaveChanges();

                    FamilyHeadEmail emailm = new FamilyHeadEmail
                    {
                        Email = request.Email,
                        FamilyHeadId = fhm.FamilyHeadId,
                        Preferred = true,
                    };

                    _context.FamilyHeadEmails.Add(emailm);
                    _context.SaveChanges();

                    _context.FamilyMembers.Add(
                        new FamilyMembers
                        {
                            FamilyHeadId = fhm.FamilyHeadId,
                            HeadId = fh.FamilyHeadId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();

                    for (int j = 0; j < (packageAddonPrices?.Count ?? 0); j++)
                    {
                        var _curr = packageAddonPrices.ElementAt(j);
                        packageTotalPkr += _curr;
                    }

                    for (int j = 0; j < (packageAdditionaFacPrices?.Count ?? 0); j++)
                    {
                        var _curr = packageAdditionaFacPrices.ElementAt(j);
                        packageTotalPkr += _curr ?? 0f;
                    }

                    packageTotalPkr += familyHeadPackage.AdultPkr;
                    packageTotalPkr += familyHeadPackage.FlightTicketPricePkr;
                }

                for (int i = 0; i < request.child; i++)
                {
                    FamilyHead fhm = new FamilyHead
                    {
                        IndId = (settings.IndividualCount + 1).ToString(),
                        FcId = newFcId,
                        GivenName = request.GivenName + "'s Member",
                        SurName = request.SurName + "'s Member",
                        CNIC = request.CNIC,
                        IsHead = false,
                        PackageId = request.PackageId,
                        Tentative = true,
                        DateOfBirth = childAge,
                        FiqahId = fiqah.FiqahId,
                        HonorificsId = honorifics.HonorificsId,
                        Gender = Gender.Male,
                        MehramRelationEnum = MehramRelationEnum.Neither,
                        PassportDateOfIssue = DateTime.Now,
                        PassportDateOfExpiry = DateTime.Now,
                        SPOId = request.SPOID,
                        FeedBy = _http.HttpContext.Session.GetString("Username"),
                        HajjSeasonsId = Convert.ToInt32(
                            _http.HttpContext.Session.GetString("ActiveSeasonId")
                        ),
                        RouteAsPerPackage = true,
                        FlightAsPerPackage = true,
                        TrainAsPerPackage = true,
                        AccomodationAsPerPackage = true,
                        Currencies = Currencies.SAR,
                        TicketStatus = TktStatus.NotIssued
                    };

                    _context.FamilyHeads.Add(fhm);
                    _context.SaveChanges();

                    settings.IndividualCount++;
                    _context.SaveChanges();

                    for (int j = 0; j < reqDocs.Count; j++)
                    {
                        _context.FamilyHeadDocuments.Add(
                            new FamilyHeadDocument
                            {
                                FamilyHeadId = fhm.FamilyHeadId,
                                RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                                Recieved = false,
                                Remarks = "",
                            }
                        );
                    }

                    FamilyHeadContact contactm = new FamilyHeadContact
                    {
                        Contact = request.Phone,
                        MemberContactTypeId = request.ContactTypeId,
                        FamilyHeadId = fhm.FamilyHeadId,
                        Preferred = true,
                    };

                    _context.FamilyHeadContacts.Add(contactm);
                    _context.SaveChanges();

                    FamilyHeadEmail emailm = new FamilyHeadEmail
                    {
                        Email = request.Email,
                        FamilyHeadId = fhm.FamilyHeadId,
                        Preferred = true,
                    };

                    _context.FamilyHeadEmails.Add(emailm);
                    _context.SaveChanges();

                    _context.FamilyMembers.Add(
                        new FamilyMembers
                        {
                            FamilyHeadId = fhm.FamilyHeadId,
                            HeadId = fh.FamilyHeadId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();

                    for (int j = 0; j < (packageAddonPrices?.Count ?? 0); j++)
                    {
                        var _curr = packageAddonPrices.ElementAt(j);
                        packageTotalPkr += _curr;
                    }

                    for (int j = 0; j < (packageAdditionaFacPrices?.Count ?? 0); j++)
                    {
                        var _curr = packageAdditionaFacPrices.ElementAt(j);
                        packageTotalPkr += _curr ?? 0f;
                    }

                    packageTotalPkr += familyHeadPackage.ChildPkr;
                    packageTotalPkr += familyHeadPackage.FlightTicketPricePkr;
                }

                for (int i = 0; i < request.infant; i++)
                {
                    FamilyHead fhm = new FamilyHead
                    {
                        IndId = (settings.IndividualCount + 1).ToString(),
                        FcId = newFcId,
                        GivenName = request.GivenName + "'s Member",
                        SurName = request.SurName + "'s Member",
                        CNIC = request.CNIC,
                        IsHead = false,
                        PackageId = request.PackageId,
                        Tentative = true,
                        DateOfBirth = infantAge,
                        FiqahId = fiqah.FiqahId,
                        HonorificsId = honorifics.HonorificsId,
                        Gender = Gender.Male,
                        MehramRelationEnum = MehramRelationEnum.Neither,
                        PassportDateOfIssue = DateTime.Now,
                        PassportDateOfExpiry = DateTime.Now,
                        SPOId = request.SPOID,
                        FeedBy = _http.HttpContext.Session.GetString("Username"),
                        HajjSeasonsId = Convert.ToInt32(
                            _http.HttpContext.Session.GetString("ActiveSeasonId")
                        ),
                        Currencies = Currencies.SAR,
                        RouteAsPerPackage = true,
                        FlightAsPerPackage = true,
                        TrainAsPerPackage = true,
                        AccomodationAsPerPackage = true,
                        TicketStatus = TktStatus.NotIssued
                    };

                    _context.FamilyHeads.Add(fhm);
                    _context.SaveChanges();

                    settings.IndividualCount++;
                    _context.SaveChanges();

                    for (int j = 0; j < reqDocs.Count; j++)
                    {
                        _context.FamilyHeadDocuments.Add(
                            new FamilyHeadDocument
                            {
                                FamilyHeadId = fhm.FamilyHeadId,
                                RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                                Recieved = false,
                                Remarks = "",
                            }
                        );
                    }

                    FamilyHeadContact contactm = new FamilyHeadContact
                    {
                        Contact = request.Phone,
                        MemberContactTypeId = request.ContactTypeId,
                        FamilyHeadId = fhm.FamilyHeadId,
                        Preferred = true,
                    };

                    _context.FamilyHeadContacts.Add(contactm);
                    _context.SaveChanges();

                    FamilyHeadEmail emailm = new FamilyHeadEmail
                    {
                        Email = request.Email,
                        FamilyHeadId = fhm.FamilyHeadId,
                        Preferred = true,
                    };

                    _context.FamilyHeadEmails.Add(emailm);
                    _context.SaveChanges();

                    _context.FamilyMembers.Add(
                        new FamilyMembers
                        {
                            FamilyHeadId = fhm.FamilyHeadId,
                            HeadId = fh.FamilyHeadId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();

                    for (int j = 0; j < (packageAddonPrices?.Count ?? 0); j++)
                    {
                        var _curr = packageAddonPrices.ElementAt(j);
                        packageTotalPkr += _curr;
                    }

                    for (int j = 0; j < (packageAdditionaFacPrices?.Count ?? 0); j++)
                    {
                        var _curr = packageAdditionaFacPrices.ElementAt(j);
                        packageTotalPkr += _curr ?? 0f;
                    }

                    packageTotalPkr += familyHeadPackage.InfantPkr;
                    packageTotalPkr += familyHeadPackage.FlightTicketPricePkr;
                }

                var rate = _context
                    .RateOfExchangesWithoutTrash.OrderByDescending(e => e.Date)
                    .FirstOrDefault(e => e.Currency == Currencies.PKR);

                PaymentVoucher voucher = new PaymentVoucher
                {
                    CompanyId = familyHeadPackage.CompanyId,
                    Date = dateTime,
                    AccountId = AccountId,
                    InstrumentNumber = "",
                    VoucherTypeId = 5,
                    Description = $"Receivable of {fh.GivenName} {fh.SurName} ({fh.FcId})",
                    Amount = packageTotalPkr,
                    PKRROE = 1,
                    SARROE = familyHeadPackage.SARROE,
                    USDROE = familyHeadPackage.USDROE,
                    Currency = Currencies.PKR,
                    ReceivingParty = "",
                };

                _context.PaymentVouchers.Add(voucher);
                _context.SaveChanges();

                _context.VoucherEntries.Add(
                    new VoucherEntry
                    {
                        PaymentVoucherId = voucher.PaymentVoucherId,
                        AccountId = AccountId,
                        Debit = packageTotalPkr,
                        Credit = 0,
                        DebitOrCreditSide = ShowAccountDebitOrCreditSide.Debit,
                    }
                );

                var companySaleAccount = _context
                    .Accounts.Include(e => e.Category)
                    .ThenInclude(e => e.SubType)
                    .Select(e => new { e.AccountId, e.Category.SubType.CompanyId })
                    .FirstOrDefault(a => a.CompanyId == familyHeadPackage.CompanyId);

                _context.VoucherEntries.Add(
                    new VoucherEntry
                    {
                        PaymentVoucherId = voucher.PaymentVoucherId,
                        AccountId = companySaleAccount.AccountId,
                        Debit = 0,
                        Credit = packageTotalPkr,
                        DebitOrCreditSide = ShowAccountDebitOrCreditSide.Credit,
                    }
                );

                _context.SaveChanges();

                transaction.Commit();
                return RedirectToAction("Index", "FamilyHead");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Ok(new { Message = ex.Message });
            }
        }

        public IActionResult CreateCustomerProfile()
        {
            var applicantTypeNames = Enum.GetValues(typeof(ApplicantType))
                .Cast<ApplicantType>()
                .ToDictionary(e => (int)e, e => e.ToString());

            ViewBag.ApplicantTypeNames = applicantTypeNames;

            ViewBag.City = _context.Cities.Where(e => !e.Trashed).ToList();

            ViewBag.Country = _context.Countries.Where(e => !e.Trashed).ToList();

            ViewBag.HajjDepartment = _context.HajjDepartments.Where(e => !e.Trashed).ToList();

            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypes;
            ViewBag.Remarks = _context.CustomerRemarks.Where(e => !e.Trashed).ToList();
            ViewBag.Status = _context.CustomerStatuses.Where(e => !e.Trashed).ToList();
            ViewBag.Referral = _context.ReferencesWithoutTrash;

            return View();
        }

        [HttpPost]
        public IActionResult CreateCustomerProfile(TentativeFamilyHead request)
        {
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Remarks = _context.CustomerRemarks.Where(e => !e.Trashed).ToList();
            ViewBag.Status = _context.CustomerStatuses.Where(e => !e.Trashed).ToList();
            ViewBag.Referral = _context.ReferencesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypes;

            string cnic = request.CNIC;
            var cnicFh = _context
                .FamilyHeadsWithoutTrash.Select(e => e.CNIC)
                .FirstOrDefault(e => e == cnic);

            if (cnicFh is not null)
            {
                ModelState.AddModelError("CNIC", "CNIC already exists");
            }

            if (!ModelState.IsValid)
                return View(request);

            int Pax = request.adult + request.child + request.infant;

            var transaction = _context.Database.BeginTransaction();

            Fiqah fiqah = _context.FiqahsWithoutTrash.FirstOrDefault();
            Honorifics honorifics = _context.HonorificsWithoutTrash.FirstOrDefault();

            DateTime currentDate = DateTime.Today;
            DateTime dateTime = DateTime.Parse(currentDate.ToString());

            DateTime infantAge = DateTime.Today;
            DateTime childAge = DateTime.Today.AddYears(-3);
            DateTime adultAge = DateTime.Today.AddYears(-13);
            // try{

            var settings = _context
                .ApplicationSettings.Where(s =>
                    s.HajjSeasonsId
                    == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .FirstOrDefault();

            //int lastFcId = settings.FamilyCount;
            //int lastIndId = settings.IndividualCount;

            //string newFcId = "HB-" + (lastFcId + 1);
            //int newIndId = lastIndId + 1;

            CustomerPortfolio fh = new CustomerPortfolio
            {
                GivenName = request.GivenName,
                SurName = request.SurName,
                CNIC = request.CNIC,
                IsHead = true,
                Tentative = true,
                DateOfBirth = adultAge,
                FiqahId = fiqah.FiqahId,
                HonorificsId = honorifics.HonorificsId,
                Gender = Gender.Male,
                PassportDateOfIssue = DateTime.Now,
                PassportDateOfExpiry = DateTime.Now,
                SPOId = request.SPOID,
                FeedBy = _http.HttpContext.Session.GetString("Username"),
                DateOfQuery = request.DateOfQuery,
                CustomerRemarksId = request.Remarks,
                CustomerStatusId = request.Status,
                ReferenceId = request.Refferel,
                ApplicantType = request.ApplicantType,
                CityId = request.CityId,
                CountryId = request.CountryId,
                HajjDepartmentId = request.HajjDepartmentId,
                HajjSeasonsId = Convert.ToInt32(
                    _http.HttpContext.Session.GetString("ActiveSeasonId")
                ),
                IsQuery = false,
            };

            _context.CustomerPortfolios.Add(fh);
            _context.SaveChanges();

            //settings.FamilyCount++;
            //settings.IndividualCount++;
            _context.SaveChanges();

            CustomerPortfolioContact contact = new CustomerPortfolioContact
            {
                Contact = request.Phone,
                MemberContactTypeId = request.ContactTypeId,
                CustomerPortfolioId = fh.CustomerPortfolioId,
                Preferred = true,
            };

            _context.CustomerPortfolioContacts.Add(contact);
            _context.SaveChanges();

            CustomerPortfolioEmail email = new CustomerPortfolioEmail
            {
                Email = request.Email,
                CustomerPortfolioId = fh.CustomerPortfolioId,
                Preferred = true,
            };

            _context.CustomerPortfolioEmails.Add(email);
            _context.SaveChanges();

            var reqDocs = _context.RequiredDocumentsWithoutTrash.ToList();

            for (int i = 0; i < reqDocs.Count; i++)
            {
                _context.CustomerPortfolioDocuments.Add(
                    new CustomerPortfolioDocuments
                    {
                        CustomerPortfolioId = fh.CustomerPortfolioId,
                        RequiredDocumentId = reqDocs.ElementAt(i).RequiredDocumentId,
                        Recieved = false,
                        Remarks = "",
                    }
                );
            }

            _context.SaveChanges();

            _context.CustomerPortfoliosMembers.Add(
                new CustomerPortfolioMembers
                {
                    CustomerPortfolioId = fh.CustomerPortfolioId,
                    HeadId = fh.CustomerPortfolioId,
                    IsHead = true,
                }
            );

            _context.SaveChanges();

            for (int i = 1; i < request.adult; i++)
            {
                CustomerPortfolio fhm = new CustomerPortfolio
                {
                    GivenName = request.GivenName + "'s Member",
                    SurName = request.SurName + "'s Member",
                    CNIC = request.CNIC,
                    IsHead = false,
                    Tentative = true,
                    DateOfBirth = adultAge,
                    FiqahId = fiqah.FiqahId,
                    HonorificsId = honorifics.HonorificsId,
                    Gender = Gender.Male,
                    PassportDateOfIssue = DateTime.Now,
                    PassportDateOfExpiry = DateTime.Now,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    DateOfQuery = request.DateOfQuery,
                    ApplicantType = request.ApplicantType,
                    CityId = request.CityId,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    IsQuery = false,
                };

                _context.CustomerPortfolios.Add(fhm);
                _context.SaveChanges();

                settings.IndividualCount++;
                _context.SaveChanges();

                for (int j = 0; j < reqDocs.Count; j++)
                {
                    _context.CustomerPortfolioDocuments.Add(
                        new CustomerPortfolioDocuments
                        {
                            CustomerPortfolioId = fhm.CustomerPortfolioId,
                            RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                _context.SaveChanges();

                CustomerPortfolioContact contactm = new CustomerPortfolioContact
                {
                    Contact = request.Phone,
                    MemberContactTypeId = request.ContactTypeId,
                    CustomerPortfolioId = fhm.CustomerPortfolioId,
                    Preferred = true,
                };

                _context.CustomerPortfolioContacts.Add(contactm);
                _context.SaveChanges();

                CustomerPortfolioEmail emailm = new CustomerPortfolioEmail
                {
                    Email = request.Email,
                    CustomerPortfolioId = fhm.CustomerPortfolioId,
                    Preferred = true,
                };

                _context.CustomerPortfolioEmails.Add(emailm);
                _context.SaveChanges();

                _context.CustomerPortfoliosMembers.Add(
                    new CustomerPortfolioMembers
                    {
                        CustomerPortfolioId = fhm.CustomerPortfolioId,
                        HeadId = fh.CustomerPortfolioId,
                        IsHead = false,
                    }
                );

                _context.SaveChanges();
            }

            for (int i = 0; i < request.child; i++)
            {
                CustomerPortfolio fhm = new CustomerPortfolio
                {
                    GivenName = request.GivenName + "'s Member",
                    SurName = request.SurName + "'s Member",
                    CNIC = request.CNIC,
                    IsHead = false,
                    Tentative = true,
                    DateOfBirth = childAge,
                    FiqahId = fiqah.FiqahId,
                    HonorificsId = honorifics.HonorificsId,
                    Gender = Gender.Male,
                    PassportDateOfIssue = DateTime.Now,
                    PassportDateOfExpiry = DateTime.Now,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    DateOfQuery = request.DateOfQuery,
                    ApplicantType = request.ApplicantType,
                    CityId = request.CityId,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    IsQuery = false,
                };

                _context.CustomerPortfolios.Add(fhm);
                _context.SaveChanges();

                settings.IndividualCount++;
                _context.SaveChanges();

                for (int j = 0; j < reqDocs.Count; j++)
                {
                    _context.CustomerPortfolioDocuments.Add(
                        new CustomerPortfolioDocuments
                        {
                            CustomerPortfolioId = fhm.CustomerPortfolioId,
                            RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                CustomerPortfolioContact contactm = new CustomerPortfolioContact
                {
                    Contact = request.Phone,
                    MemberContactTypeId = request.ContactTypeId,
                    CustomerPortfolioId = fhm.CustomerPortfolioId,
                    Preferred = true,
                };

                _context.CustomerPortfolioContacts.Add(contactm);
                _context.SaveChanges();

                CustomerPortfolioEmail emailm = new CustomerPortfolioEmail
                {
                    Email = request.Email,
                    CustomerPortfolioId = fhm.CustomerPortfolioId,
                    Preferred = true,
                };

                _context.CustomerPortfolioEmails.Add(emailm);
                _context.SaveChanges();

                _context.CustomerPortfoliosMembers.Add(
                    new CustomerPortfolioMembers
                    {
                        CustomerPortfolioId = fhm.CustomerPortfolioId,
                        HeadId = fh.CustomerPortfolioId,
                        IsHead = false,
                    }
                );

                _context.SaveChanges();
            }

            for (int i = 0; i < request.infant; i++)
            {
                CustomerPortfolio fhm = new CustomerPortfolio
                {
                    GivenName = request.GivenName + "'s Member",
                    SurName = request.SurName + "'s Member",
                    CNIC = request.CNIC,
                    IsHead = false,
                    Tentative = true,
                    DateOfBirth = infantAge,
                    FiqahId = fiqah.FiqahId,
                    HonorificsId = honorifics.HonorificsId,
                    Gender = Gender.Male,
                    PassportDateOfIssue = DateTime.Now,
                    PassportDateOfExpiry = DateTime.Now,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    DateOfQuery = request.DateOfQuery,
                    ApplicantType = request.ApplicantType,
                    CityId = request.CityId,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    IsQuery = false,
                };

                _context.CustomerPortfolios.Add(fhm);
                _context.SaveChanges();

                settings.IndividualCount++;
                _context.SaveChanges();

                for (int j = 0; j < reqDocs.Count; j++)
                {
                    _context.CustomerPortfolioDocuments.Add(
                        new CustomerPortfolioDocuments
                        {
                            CustomerPortfolioId = fhm.CustomerPortfolioId,
                            RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                CustomerPortfolioContact contactm = new CustomerPortfolioContact
                {
                    Contact = request.Phone,
                    MemberContactTypeId = request.ContactTypeId,
                    CustomerPortfolioId = fhm.CustomerPortfolioId,
                    Preferred = true,
                };

                _context.CustomerPortfolioContacts.Add(contactm);
                _context.SaveChanges();

                CustomerPortfolioEmail emailm = new CustomerPortfolioEmail
                {
                    Email = request.Email,
                    CustomerPortfolioId = fhm.CustomerPortfolioId,
                    Preferred = true,
                };

                _context.CustomerPortfolioEmails.Add(emailm);
                _context.SaveChanges();

                _context.CustomerPortfoliosMembers.Add(
                    new CustomerPortfolioMembers
                    {
                        CustomerPortfolioId = fhm.CustomerPortfolioId,
                        HeadId = fh.CustomerPortfolioId,
                        IsHead = false,
                    }
                );

                _context.SaveChanges();
            }

            _context.SaveChanges();

            transaction.Commit();
            return RedirectToAction("IndexCustomerProfile", "FamilyHead");
            // }
            // catch(Exception e){
            //     transaction.Rollback();

            //     return Ok(new {
            //         e.Message
            //     });
            //     return View(request);
            // }
        }

        public IActionResult BookCustomer(int id)
        {
            ViewBag.Package = _context
                .Packages.Where(e =>
                    e.HajjSeasonsId
                        == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    && !e.Trashed
                )
                .ToList();

            ViewBag.Id = id;

            return View();
        }

        [HttpPost]
        public IActionResult BookCustomer(int PackageId, int CustomerPortfolioId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var documents = _context.RequiredDocumentsWithoutTrash.ToList();

                    CustomerPortfolio customerPortfolio =
                        _context.CustomerPortfolios.FirstOrDefault(e =>
                            e.CustomerPortfolioId == CustomerPortfolioId && e.IsHead
                        );

                    var contacts = _context
                        .CustomerPortfolioContacts.Where(e =>
                            e.CustomerPortfolioId == CustomerPortfolioId
                        )
                        .ToList();

                    var emails = _context
                        .CustomerPortfolioEmails.Where(e =>
                            e.CustomerPortfolioId == CustomerPortfolioId
                        )
                        .ToList();

                    var addresses = _context
                        .CustomerPortfolioAddress.Where(e =>
                            e.CustomerPortfolioId == CustomerPortfolioId
                        )
                        .ToList();

                    if (customerPortfolio == null)
                    {
                        return NotFound("Customer not found.");
                    }

                    var settings = _context
                        .ApplicationSettings.Where(s =>
                            s.HajjSeasonsId
                            == Convert.ToUInt32(
                                _http.HttpContext.Session.GetString("ActiveSeasonId")
                            )
                        )
                        .FirstOrDefault();

                    //int lastFcId = settings.FamilyCount;
                    int lastIndId = settings.IndividualCount;

                    //string newFcId = "HB-" + (lastFcId + 1);
                    int newIndId = lastIndId + 1;

                    var lastReceipt = _context
                    .FamilyHeads.Where(e =>
                        e.HajjSeasonsId
                        == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                    .OrderByDescending(x => x.FamilyHeadId)
                    .Select(x => x.FcId)
                    .FirstOrDefault();

                    int nextReceiptNumber = 1;

                    if (!string.IsNullOrEmpty(lastReceipt) && lastReceipt.StartsWith("HB-"))
                    {
                        nextReceiptNumber = int.Parse(lastReceipt.Substring(3)) + 1;
                    }

                    #region Head

                    customerPortfolio.IsQuery = true;
                    _context.CustomerPortfolios.Update(customerPortfolio);
                    _context.SaveChanges();

                    FamilyHead familyHead = new FamilyHead
                    {
                        FcId = $"HB-{nextReceiptNumber:D2}",
                        SPOId = customerPortfolio.SPOId,
                        FeedBy = customerPortfolio.FeedBy,
                        IndId = newIndId.ToString(),
                        GivenName = customerPortfolio.GivenName,
                        SurName = customerPortfolio.SurName,
                        PassportDateOfIssue = customerPortfolio.PassportDateOfIssue,
                        PassportDateOfExpiry = customerPortfolio.PassportDateOfExpiry,
                        FatherHusbandName = customerPortfolio.FatherHusbandName,
                        CNIC = customerPortfolio.CNIC,
                        DateOfBirth = customerPortfolio.DateOfBirth,
                        PlaceOfBirth = customerPortfolio.PlaceOfBirth,
                        Gender = customerPortfolio.Gender,
                        BloodGroup = customerPortfolio.BloodGroup,
                        FiqahId = customerPortfolio.FiqahId,
                        IsMarried = customerPortfolio.IsMarried,
                        IsEmployed = customerPortfolio.IsEmployed,
                        PassportNumber = customerPortfolio.PassportNumber,
                        ContactPerson = customerPortfolio.ContactPerson,
                        PassportImage = customerPortfolio.PassportImage,
                        MemberImage = customerPortfolio.MemberImage,
                        PassportTokenImage = customerPortfolio.PassportTokenImage,
                        TokenRemarks = customerPortfolio.TokenRemarks,
                        VisaImage = customerPortfolio.VisaImage,
                        FamilyHeadRelationId = customerPortfolio.FamilyHeadRelationId,
                        Tentative = customerPortfolio.Tentative,
                        HajjSeasonsId = customerPortfolio.HajjSeasonsId,
                        IsHead = true,
                        PackageId = PackageId,
                        CountryId = customerPortfolio.CountryId,
                        CountryCodeId = customerPortfolio.CountryCodeId,
                        NationalityId = customerPortfolio.NationalityId,
                        PassportTypeId = customerPortfolio.PassportTypeId,
                        RouteAsPerPackage = true,
                        TrainAsPerPackage = true,
                        FlightAsPerPackage = true,
                        AccomodationAsPerPackage = true
                    };

                    _context.FamilyHeads.Add(familyHead);
                    _context.SaveChanges();

                    for (int i = 0; i < contacts.Count; i++)
                    {
                        var currentContact = contacts.ElementAt(i);
                        _context.FamilyHeadContacts.Add(
                            new FamilyHeadContact
                            {
                                FamilyHeadId = familyHead.FamilyHeadId,
                                Preferred = currentContact.Preferred,
                                Contact = currentContact.Contact,
                                MemberContactTypeId = currentContact.MemberContactTypeId,
                            }
                        );
                    }
                    _context.SaveChanges();

                    for (int i = 0; i < emails.Count; i++)
                    {
                        var currentEmail = emails.ElementAt(i);
                        _context.FamilyHeadEmails.Add(
                            new FamilyHeadEmail
                            {
                                FamilyHeadId = familyHead.FamilyHeadId,
                                Preferred = currentEmail.Preferred,
                                Email = currentEmail.Email,
                                MemberEmailTypeId = currentEmail.MemberEmailTypeId,
                            }
                        );
                    }
                    _context.SaveChanges();

                    for (int i = 0; i < addresses.Count; i++)
                    {
                        var currentAddress = addresses.ElementAt(i);
                        _context.FamilyHeadAddresses.Add(
                            new FamilyHeadAddress
                            {
                                FamilyHeadId = familyHead.FamilyHeadId,
                                Preferred = currentAddress.Preferred,
                                Address = currentAddress.Address,
                                City = currentAddress.City,
                                Country = currentAddress.Country,
                                PoBox = currentAddress.PoBox,
                                TehsilId = currentAddress.TehsilId,
                                ZipCode = currentAddress.ZipCode,
                                MemberAddressTypeId = currentAddress.MemberAddressTypeId,
                            }
                        );
                    }

                    _context.SaveChanges();

                    for (int i = 0; i < documents.Count; i++)
                    {
                        _context.FamilyHeadDocuments.Add(
                            new FamilyHeadDocument
                            {
                                RequiredDocumentId = documents.ElementAt(i).RequiredDocumentId,
                                FamilyHeadId = familyHead.FamilyHeadId,
                                Recieved = false,
                                Remarks = "",
                            }
                        );
                    }
                    _context.SaveChanges();

                    _context.FamilyMembers.Add(
                        new FamilyMembers
                        {
                            FamilyHeadId = familyHead.FamilyHeadId,
                            HeadId = familyHead.FamilyHeadId,
                            IsHead = true,
                        }
                    );

                    #endregion

                    #region Members

                    var members = _context
                        .CustomerPortfoliosMembers.Include(e => e.CustomerPortfolio)
                        .Where(e =>
                            e.HeadId == CustomerPortfolioId
                            && !e.IsHead
                            && !e.CustomerPortfolio.Trashed
                        )
                        .ToList();

                    for (int i = 0; i < members.Count; i++)
                    {
                        var currentMember = members.ElementAt(i);

                        currentMember.CustomerPortfolio.IsQuery = true;

                        _context.CustomerPortfolios.Update(currentMember.CustomerPortfolio);
                        _context.SaveChanges();

                        FamilyHead familyHeadMember = new FamilyHead
                        {
                            FcId = $"HB-{nextReceiptNumber:D2}",
                            SPOId = currentMember.CustomerPortfolio.SPOId,
                            FeedBy = currentMember.CustomerPortfolio.FeedBy,
                            IndId = (settings.IndividualCount + 1).ToString(),
                            GivenName = currentMember.CustomerPortfolio.GivenName,
                            SurName = currentMember.CustomerPortfolio.SurName,
                            PassportDateOfIssue = currentMember
                                .CustomerPortfolio
                                .PassportDateOfIssue,
                            PassportDateOfExpiry = currentMember
                                .CustomerPortfolio
                                .PassportDateOfExpiry,
                            FatherHusbandName = currentMember.CustomerPortfolio.FatherHusbandName,
                            CNIC = currentMember.CustomerPortfolio.CNIC,
                            DateOfBirth = currentMember.CustomerPortfolio.DateOfBirth,
                            PlaceOfBirth = currentMember.CustomerPortfolio.PlaceOfBirth,
                            Nationality = currentMember.CustomerPortfolio.Nationality,
                            Gender = currentMember.CustomerPortfolio.Gender,
                            BloodGroup = currentMember.CustomerPortfolio.BloodGroup,
                            FiqahId = currentMember.CustomerPortfolio.FiqahId,
                            IsMarried = currentMember.CustomerPortfolio.IsMarried,
                            IsEmployed = currentMember.CustomerPortfolio.IsEmployed,
                            PassportNumber = currentMember.CustomerPortfolio.PassportNumber,
                            ContactPerson = currentMember.CustomerPortfolio.ContactPerson,
                            PassportImage = currentMember.CustomerPortfolio.PassportImage,
                            MemberImage = currentMember.CustomerPortfolio.MemberImage,
                            PassportTokenImage = currentMember.CustomerPortfolio.PassportTokenImage,
                            TokenRemarks = currentMember.CustomerPortfolio.TokenRemarks,
                            VisaImage = currentMember.CustomerPortfolio.VisaImage,
                            FamilyHeadRelationId = currentMember
                                .CustomerPortfolio
                                .FamilyHeadRelationId,
                            Tentative = currentMember.CustomerPortfolio.Tentative,
                            HajjSeasonsId = currentMember.CustomerPortfolio.HajjSeasonsId,
                            IsHead = false,
                            PackageId = PackageId,
                            CountryId = currentMember.CustomerPortfolio.CountryId,
                            CountryCodeId = currentMember.CustomerPortfolio.CountryCodeId,
                            NationalityId = currentMember.CustomerPortfolio.NationalityId,
                            PassportTypeId = currentMember.CustomerPortfolio.PassportTypeId,
                            RouteAsPerPackage = true,
                            TrainAsPerPackage = true,
                            FlightAsPerPackage = true,
                            AccomodationAsPerPackage = true
                        };

                        _context.FamilyHeads.Add(familyHeadMember);
                        _context.SaveChanges();

                        for (int j = 0; j < contacts.Count; j++)
                        {
                            var currentContact = contacts.ElementAt(i);
                            _context.FamilyHeadContacts.Add(
                                new FamilyHeadContact
                                {
                                    FamilyHeadId = familyHeadMember.FamilyHeadId,
                                    Preferred = currentContact.Preferred,
                                    Contact = currentContact.Contact,
                                    MemberContactTypeId = currentContact.MemberContactTypeId,
                                }
                            );
                        }
                        _context.SaveChanges();

                        for (int j = 0; j < emails.Count; j++)
                        {
                            var currentEmail = emails.ElementAt(i);
                            _context.FamilyHeadEmails.Add(
                                new FamilyHeadEmail
                                {
                                    FamilyHeadId = familyHeadMember.FamilyHeadId,
                                    Preferred = currentEmail.Preferred,
                                    Email = currentEmail.Email,
                                    MemberEmailTypeId = currentEmail.MemberEmailTypeId,
                                }
                            );
                        }
                        _context.SaveChanges();

                        for (int j = 0; j < addresses.Count; j++)
                        {
                            var currentAddress = addresses.ElementAt(i);
                            _context.FamilyHeadAddresses.Add(
                                new FamilyHeadAddress
                                {
                                    FamilyHeadId = familyHeadMember.FamilyHeadId,
                                    Preferred = currentAddress.Preferred,
                                    Address = currentAddress.Address,
                                    City = currentAddress.City,
                                    Country = currentAddress.Country,
                                    PoBox = currentAddress.PoBox,
                                    TehsilId = currentAddress.TehsilId,
                                    ZipCode = currentAddress.ZipCode,
                                    MemberAddressTypeId = currentAddress.MemberAddressTypeId,
                                }
                            );
                        }

                        _context.SaveChanges();

                        for (int j = 0; j < documents.Count; j++)
                        {
                            _context.FamilyHeadDocuments.Add(
                                new FamilyHeadDocument
                                {
                                    RequiredDocumentId = documents.ElementAt(i).RequiredDocumentId,
                                    FamilyHeadId = familyHeadMember.FamilyHeadId,
                                    Recieved = false,
                                    Remarks = "",
                                }
                            );
                        }
                        _context.SaveChanges();

                        _context.FamilyMembers.Add(
                            new FamilyMembers
                            {
                                FamilyHeadId = familyHeadMember.FamilyHeadId,
                                HeadId = familyHead.FamilyHeadId,
                                IsHead = false,
                            }
                        );
                        _context.SaveChanges();
                    }

                    #endregion

                    transaction.Commit();

                    return RedirectToAction("Index", "FamilyHead");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                    return BadRequest(ex);
                }
            }
        }

        public IActionResult AgreementRedirect(int id)
        {
            string a = _context.FamilyHeads.Find(id).AgreementBase64;
            ViewBag.AgreementBase64 = a;
            return View();
        }

        public IActionResult StayWith(int id)
        {
            FamilyHead fh = _context.FamilyHeads.Find(id);
            int headId = _context
                .FamilyMembers.Select(e => new { e.HeadId, e.FamilyHeadId })
                .FirstOrDefault(e => e.FamilyHeadId == id)
                .HeadId;

            string headFcId = _context
                .FamilyHeads.Select(e => new { e.FcId, e.FamilyHeadId })
                .FirstOrDefault(e => e.FamilyHeadId == headId)
                .FcId;

            fh.FcId = headFcId;

            if (fh.StayWithId is not null)
            {
                ViewBag.StayWithHead = _context
                    .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id)
                    .HeadId;
            }
            return View(fh);
        }

        [HttpPost]
        public IActionResult StayWith(FamilyHead request)
        {
            var _ = _context.FamilyHeads.Find(request.FamilyHeadId);

            int _headId = _context
                .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadId)
                .HeadId;

            _.StayWithId = request.StayWithId;
            _context.SaveChanges();

            return RedirectToAction(nameof(ViewFamily), new { id = _headId });
        }

        public JsonResult GetFamily(int id) =>
            Json(
                _context
                    .FamilyMembers.Include(e => e.FamilyHead)
                    .Where(e => e.Head.FcId.Equals($"HB-{id}"))
                    .Select(e => new
                    {
                        e.FamilyHead.GivenName,
                        e.FamilyHead.SurName,
                        e.FamilyHead.Honorifics.Title,
                        e.FamilyHeadId,
                        e.Head.FcId,
                    })
                    .OrderBy(e => e.FamilyHeadId)
            );

        public IActionResult GetFriend(string cnic)
        {
            FamilyHead friend = _context.FamilyHeads.FirstOrDefault(e =>
                e.CNIC == cnic && !e.Trashed
            );

            if (friend == null)
                return Ok(
                    new
                    {
                        status = 201,
                        msg = "CNIC Not Found!",
                        data = new { cnic },
                    }
                );

            return Ok(
                new
                {
                    status = 200,
                    msg = "CNIC Found",
                    data = new
                    {
                        surName = friend.SurName,
                        givenName = friend.GivenName,
                        gender = friend.Gender.ToString(),
                        passport = friend.PassportNumber,
                    },
                }
            );
        }

        public IActionResult GetFamilyHeadQurbani(int id)
        {
            FamilyMembers fMember = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .FirstOrDefault(e => e.FamilyHeadId == id);

            if (fMember == null)
            {
                return Ok(new { status = 201, msg = "HB # Not Found" });
            }
            List<FamilyMembers> allMembers = null;
            if (fMember.IsHead)
            {
                allMembers = _context
                    .FamilyMembers.Include(e => e.FamilyHead)
                    .Where(e => e.HeadId == fMember.FamilyHeadId)
                    .ToList();
            }
            else
            {
                int headId = _context
                    .FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == fMember.HeadId)
                    .FamilyHeadId;

                allMembers = _context
                    .FamilyMembers.Include(e => e.FamilyHead)
                    .Where(e => e.HeadId == headId)
                    .ToList();
            }

            List<Qurbani> qurbanis = new List<Qurbani>();

            for (int i = 0; i < allMembers.Count; i++)
            {
                var curAllMember = allMembers.ElementAt(i);

                Qurbani q = _context.Qurbani.FirstOrDefault(e =>
                    e.FamilyHeadId == curAllMember.FamilyHeadId && e.Active
                );

                qurbanis.Add(q);
            }

            //return Ok(qurbanis);

            return Ok(
                new
                {
                    status = 200,
                    msg = "HB # Found",
                    data = new
                    {
                        requestedMember = fMember,
                        fMember.FamilyHead.GivenName,
                        qurbanis,
                        allMembers,
                    },
                }
            );
        }

        public IActionResult GetFamilyHeadForReservation(int id)
        {
            FamilyHead fh = _context.FamilyHeads.Find(id);

            if (fh == null)
            {
                return Ok(new { status = 201, msg = "Family Head Not Found!" });
            }

            return Ok(
                new
                {
                    status = 200,
                    msg = "Family Head Found!",
                    data = fh,
                }
            );
        }

        public JsonResult GetFlightClass(int id) => Json(_context.Packages.Find(id).FlightClass);

        public JsonResult GetBaseAmount(int id) => Json(_context.Packages.Find(id).AdultSar);

        public JsonResult GetPackageROE(int id) =>
            Json(
                _context
                    .Packages.Select(e => new
                    {
                        e.PackageId,
                        e.USDROE,
                        e.SARROE,
                    })
                    .FirstOrDefault(e => e.PackageId == id)
            );

        public JsonResult GetPOD(int id)
        {
            return Json(new { pod = _context.Packages.Find(id).POD });
        }

        public IActionResult VerifyMember(int id, bool value)
        {
            var member = _context.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == id);

            member.Verified = value;

            _context.SaveChanges();

            return Ok(new { status = 200, msg = "Updated" });
        }

        public IActionResult BookMember(int id, bool value)
        {
            var member = _context.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == id);

            member.Tentative = value;

            _context.SaveChanges();

            return Ok(new { status = 200, msg = "Updated" });
        }

        public IActionResult FlightHistory(int id)
        {
            FamilyHead familyHead = _context.FamilyHeads.Find(id);

            var flight = _context
                .FamilyHeadFlight.Include(e => e.Currency)
                .Include(e => e.IssuingAgent)
                .Include(e => e.Airline)
                .Where(e => e.TicketStatus == TicketStatus.InActive && e.FamilyHeadId == id)
                .ToList();

            ViewBag.Name = _context
                .FamilyHeads.Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            ViewBag.HBNo = _context
                .FamilyHeads.Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            return View(flight);
        }

        public IActionResult TrainHistory(int id)
        {
            FamilyHead familyHead = _context.FamilyHeads.Find(id);

            var train = _context
                .FamilyHeadTrain.Include(e => e.FamilyHead)
                .Include(e => e.Currency)
                .Include(e => e.IssuingAgent)
                .Include(e => e.Railway)
                .Where(e => e.TicketStatus == TicketStatus.InActive && e.FamilyHeadId == id)
                .ToList();

            ViewBag.Name = _context
                .FamilyHeads.Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            ViewBag.HBNo = _context
                .FamilyHeads.Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            return View(train);
        }

        public IActionResult ClientEmail()
        {
            return View();
        }

        //public IActionResult Visa(int id)
        //{
        //    var familyHead = _context.FamilyHeadsWithoutTrash
        //        .Where(e => e.FamilyHeadId == id)
        //        .Select(e => new Visas()
        //        {
        //            VisaImage = e.VisaImage.Replace("\\", "/"),
        //            FileExtension = Path.GetExtension(e.VisaImage).ToLower()
        //        })
        //        .FirstOrDefault();

        //    if (familyHead == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(familyHead);
        //}

        public IActionResult Visa(int id)
        {
            var familyHead = _context.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => new Visas()
                {
                    VisaImage = string.IsNullOrEmpty(e.VisaImage) ? "N/A" : e.VisaImage.Replace("\\", "/"),
                    FileExtension = string.IsNullOrEmpty(e.VisaImage) ? "N/A" : Path.GetExtension(e.VisaImage).ToLower()
                })
                .FirstOrDefault();

            if (familyHead == null)
            {
                return NotFound();
            }

            return View(familyHead);
        }

        public IActionResult GiveAway(int id)
        {
            ViewBag.GiveAways = _context.GiveAways
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.AssignedGiveAways = _context.FamilyHeadGiveAways
                .Where(e => e.FamilyHeadId == id)
                .ToList();

            ViewBag.FamilyHeadId = id;

            return View();
        }

        [HttpPost]
        public IActionResult GiveAway(FamilyHeadEditRequest request)
        {
            FamilyHead familyHead = _context.FamilyHeads.Find(request.FamilyHeadId);

            for (int i = 0; i < request.GiveAwayIds.Count(); i++)
            {
                _context.FamilyHeadGiveAways.Add(new FamilyHeadGiveAways
                {
                    GiveAwayId = request.GiveAwayIds[i],
                    FamilyHeadId = familyHead.FamilyHeadId
                });
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GiveAwayPrint()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDate([FromBody] DateUpdateRequest request)
        {
            try
            {
                // Fetch all FamilyHeadIds from FamilyMember table where HeadId matches the provided FamilyHeadId
                var familyHeadIds = await _context.FamilyMembers
                    .Where(fm => fm.HeadId == request.FamilyHeadId)
                    .Select(fm => fm.FamilyHeadId)
                    .Distinct()
                    .ToListAsync();

                // Fetch all corresponding FamilyHead records
                var familyHeads = await _context.FamilyHeads
                    .Where(fh => familyHeadIds.Contains(fh.FamilyHeadId))
                    .ToListAsync();

                // Update the date for each FamilyHead record
                foreach (var fh in familyHeads)
                {
                    fh.DeliveryDate = DateTime.Parse(request.Date); // Replace 'DateField' with your actual date column name
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRecieverName([FromBody] ReceiverUpdateRequest request)
        {
            try
            {
                var familyHeadIds = await _context.FamilyMembers
                    .Where(fm => fm.HeadId == request.FamilyHeadId)
                    .Select(fm => fm.FamilyHeadId)
                    .Distinct()
                    .ToListAsync();

                var familyHeads = await _context.FamilyHeads
                    .Where(fh => familyHeadIds.Contains(fh.FamilyHeadId))
                    .ToListAsync();

                foreach (var fh in familyHeads)
                {
                    fh.RecieverName = request.ReceiverName; // match the model property
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDDDate([FromBody] DateUpdateRequest request)
        {
            try
            {
                // Fetch all FamilyHeadIds from FamilyMember table where HeadId matches the provided FamilyHeadId
                var familyHeadIds = await _context.FamilyMembers
                    .Where(fm => fm.HeadId == request.FamilyHeadId)
                    .Select(fm => fm.FamilyHeadId)
                    .Distinct()
                    .ToListAsync();

                // Fetch all corresponding FamilyHead records
                var familyHeads = await _context.FamilyHeads
                    .Where(fh => familyHeadIds.Contains(fh.FamilyHeadId))
                    .ToListAsync();

                // Update the date for each FamilyHead record
                foreach (var fh in familyHeads)
                {
                    fh.DDDate = DateTime.Parse(request.Date); // Replace 'DateField' with your actual date column name
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDDRecieverName([FromBody] ReceiverUpdateRequest request)
        {
            try
            {
                var familyHeadIds = await _context.FamilyMembers
                    .Where(fm => fm.HeadId == request.FamilyHeadId)
                    .Select(fm => fm.FamilyHeadId)
                    .Distinct()
                    .ToListAsync();

                var familyHeads = await _context.FamilyHeads
                    .Where(fh => familyHeadIds.Contains(fh.FamilyHeadId))
                    .ToListAsync();

                foreach (var fh in familyHeads)
                {
                    fh.DDReceiver = request.ReceiverName; // match the model property
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class ReceiverUpdateRequest
    {
        public int FamilyHeadId { get; set; }
        public string ReceiverName { get; set; }
    }


    public class DateUpdateRequest
    {
        public int FamilyHeadId { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
    }
    public record Visas
    {
        public string VisaImage { get; set; }

        public string FileExtension { get; set; } // For checking file type
    }
}

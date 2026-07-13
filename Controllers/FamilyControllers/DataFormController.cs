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
using static TMG.Controllers.FamilyControllers.FamilyHeadFormController;

namespace TMG.Controllers.FamilyControllers
{
    public record HajjRegistration
    {
        public List<string> SurName { get; set; }
        public List<string> GivenName { get; set; }
        public List<string> FatherHusbandName { get; set; }
        public List<string> Number { get; set; }
        public List<string> Email { get; set; }
        public List<string> CNIC { get; set; }
        public List<DateTime> DateOfBirth { get; set; }
        public List<DateTime> DateOfR { get; set; }
        public List<IFormFile> PassportAttachment { get; set; }
        public List<IFormFile> CNICAttachment { get; set; }
        public List<IFormFile> CNICBackAttachment { get; set; }
        public List<IFormFile> MemberImage { get; set; }
        public List<string> Address { get; set; }
    }

    public class DataFormController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public DataFormController(
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
                .DataFormMembers.Include(e => e.DataForm)
                .Where(e => e.HeadId == id)
                .ToList();

            foreach (var m in members)
            {
                m.DataForm.Trashed = true;
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

            var query = (
                    from familyHead in _context.DataForms
                    where !familyHead.Trashed && familyHead.IsHead
                    select new
                    {
                        familyHead.DataFormId,
                        familyHead.GivenName,
                        familyHead.SurName,
                        familyHead.FeedBy,
                        BookedBy = familyHead.SPO.Name,
                        familyHead.IsHead,
                        familyHead.GroupNo,
                        familyHead.Status,
                        familyHead.CNICNo,
                        MemberCount = _context.DataFormMembers.Count(m => m.HeadId == familyHead.DataFormId)
                    })
                    .AsNoTracking()
                    .ToList();

            model.FamilyCount = query.Count;

            for (int i = 0; i < query.Count; i++)
            {
                int serialNumber = i + 1;

                var currMember = query.ElementAt(i);

                var entry = new DataFormIndexEntryModel(
                    serialNumber,
                    currMember.DataFormId,
                    currMember.BookedBy,
                    currMember.FeedBy,
                    $"{currMember.GivenName} {currMember.SurName}",
                    currMember.IsHead,
                    currMember.GroupNo ?? "-",
                    currMember.Status.ToString(),
                    currMember.CNICNo,
                    currMember.MemberCount
                );

                model.DataEntries.Add(entry);
            }

            ViewBag.TrashCount = _context
                .DataForms.Where(e => e.Trashed && e.IsHead)
                .AsNoTracking()
                .Select(e => e.DataFormId)
                .Count();

            return View(model);
        }

        public IActionResult ViewFamily(int id)
        {
            ViewBag.EditAllowed = PrivilegeManager.CanEditFamily(_context, _http);
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFamily(_context, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFamily(_context, _http);
            ViewBag.VerifyAllowed = PrivilegeManager.CanVerifyFamilyMember(_context, _http);
            ViewBag.FamilyHeadId = id;

            var familyHead = _context
                .DataFormMembers.Include(e => e.DataForm)
                .AsNoTracking()
                .Where(e => e.HeadId == id && !e.DataForm.Trashed);

            ViewBag.TrashCount = _context
                .DataFormMembers.Where(e => e.DataForm.Trashed && e.HeadId == id)
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

        public IActionResult View(int id)
        {
            ViewBag.FamilyHeadId = id;

            var familyHead = _context
                .DataFormMembers.Include(e => e.DataForm)
                .AsNoTracking()
                .Where(e => e.HeadId == id && !e.DataForm.Trashed);

            ViewBag.TrashCount = _context
                .DataFormMembers.Where(e => e.DataForm.Trashed && e.HeadId == id)
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

            return View(familyHead);
        }

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

        public IActionResult ViewTrash(int id)
        {
            var memberIds = _context
                .DataFormMembers.Where(e => e.HeadId == id)
                .Select(e => e.DataFormId)
                .ToList();
            return View(
                _context.DataForms.Where(s =>
                    memberIds.Contains(s.DataFormId)
                    && s.Trashed
                )
            );
        }

        public IActionResult ViewTrashMainPage()
        {
            return View(
                _context.DataForms.Where(s =>
                    s.Trashed
                    && s.IsHead
                )
            );
        }

        public IActionResult Trash(int id)
        {
            DataForm familyHead = _context.DataForms.Find(id);
            if (familyHead == null)
                return RedirectToAction(nameof(Index));

            familyHead.Trashed = true;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            DataForm familyMember = _context.DataForms.Find(id);
            DataFormMember fm = _context.DataFormMembers.FirstOrDefault(e => e.DataFormId == id);

            familyMember.Trashed = false;

            _context.SaveChanges();
            return RedirectToAction(nameof(ViewFamily), new { id = fm.HeadId });
        }

        public IActionResult Restorehead(int id)
        {
            var family = _context.DataFormMembers.Where(e => e.HeadId == id);
            foreach (var item in family)
            {
                DataForm familyMember = _context.DataForms.Find(item.DataFormId);

                DataFormMember fm = _context.DataFormMembers.FirstOrDefault(e =>
                    e.HeadId == item.DataFormId
                );

                familyMember.Trashed = false;
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index)); //, new { id = fm.HeadId }
        }

        public IActionResult Edit(int id)
        {
            ViewBag.ViewFamilyId = _context
                .DataFormMembers.FirstOrDefault(e => e.DataFormId == id)
                .HeadId;

            ViewBag.SPOs = _context.SPOsWithoutTrash;

            DataForm familyHead = _context
                .DataForms
                .FirstOrDefault(e => e.DataFormId == id && !e.Trashed);

            ViewBag.FeedBy = familyHead.FeedBy;

            var headid = _context
                .DataFormMembers.FirstOrDefault(e => e.DataFormId == familyHead.DataFormId)
                .HeadId;

            var headdata = _context.DataForms.FirstOrDefault(e => e.DataFormId == headid);

            ViewBag.FamilyMembers = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed && !e.IsHead)
                .ToList();

            ViewBag.FamilyMembersWithHead = _context
                .DataFormMembers.Include(e => e.DataForm)
                .Where(e => e.HeadId == id && !e.DataForm.Trashed)
                .ToList();

            FamilyHeadEditRequest request = new FamilyHeadEditRequest
            {
                FamilyHeadId = id,
                SPOId = familyHead.SPOId,
                FeedBy = familyHead.FeedBy,
                SurName = familyHead.SurName,
                GivenName = familyHead.GivenName,
                FatherName = familyHead.FatherHusbandName,
                CNIC = familyHead.CNICNo,
                DateOfBirth = familyHead.DOB,
                IsHead = familyHead.IsHead,
                Email = familyHead.Email,
                Number = familyHead.MobileNo,
                DateOfR = familyHead.RegistrationDate,
                GroupNo = familyHead.GroupNo,
                Address = familyHead.Address,
                Password = familyHead.Password,
                DataStatus = familyHead.Status,
                MImage = familyHead.MemberImage,
                PAttachment = familyHead.PassportAttachment,
                CAttachment = familyHead.CNICAttachment,
                FAttachment = familyHead.FormAttachment,
                CBackAttachment = familyHead.CNICBackAttachment
            };

            return View(request);
        }

        public async Task<JsonResult> Save(FamilyHeadEditRequest request, int id)
        {

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                DataForm familyHead = await _context.DataForms.FindAsync(id);

                if (familyHead == null)
                {
                    return Json(new
                    {
                        status = 404,
                        msg = "Family head not found."
                    });
                }

                familyHead.SPOId = request.SPId;
                familyHead.FeedBy = request.FeedBy;
                familyHead.SurName = request.SurName;
                familyHead.GivenName = request.GivenName;
                familyHead.FatherHusbandName = request.FatherName;
                familyHead.CNICNo = request.CNIC;
                familyHead.DOB = request.DateOfBirth;
                familyHead.RegistrationDate = request.DateOfR;
                familyHead.Email = request.Email;
                familyHead.MobileNo = request.Number;
                familyHead.GroupNo = request.GroupNo;
                familyHead.Address = request.Address;
                familyHead.Password = request.Password;

                if (request.MemberImage is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "MemberImages", request.MemberImage.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.MemberImage.CopyToAsync(fs);
                    }
                    familyHead.MemberImage = @"Images\MemberImages\" + request.MemberImage.FileName;
                }

                if (request.PassportAttachment is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PassportImages", request.PassportAttachment.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.PassportAttachment.CopyToAsync(fs);
                    }
                    familyHead.PassportAttachment = @"Images\PassportImages\" + request.PassportAttachment.FileName;
                }

                if (request.CNICAttachment is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.CNICAttachment.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.CNICAttachment.CopyToAsync(fs);
                    }
                    familyHead.CNICAttachment = @"Images\CnicUpload\" + request.CNICAttachment.FileName;
                }

                if (request.CNICBackAttachment is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.CNICBackAttachment.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.CNICBackAttachment.CopyToAsync(fs);
                    }
                    familyHead.CNICBackAttachment = @"Images\CnicUpload\" + request.CNICBackAttachment.FileName;
                }

                if (request.FormAttachment is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.FormAttachment.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.FormAttachment.CopyToAsync(fs);
                    }
                    familyHead.FormAttachment = @"Images\CnicUpload\" + request.FormAttachment.FileName;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    status = 200,
                    msg = "Passport details saved!",
                    headId = id,
                    familyHead
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return Json(new
                {
                    status = 500,
                    msg = "An error occurred while saving passport details.",
                    details = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        public IActionResult Update(int id)
        {
            ViewBag.ViewFamilyId = _context
                .DataFormMembers.FirstOrDefault(e => e.DataFormId == id)
                .HeadId;

            ViewBag.SPOs = _context.SPOsWithoutTrash;

            DataForm familyHead = _context
                .DataForms
                .FirstOrDefault(e => e.DataFormId == id && !e.Trashed);

            ViewBag.FeedBy = familyHead.FeedBy;

            var headid = _context
                .FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == familyHead.DataFormId)
                .HeadId;
            var headdata = _context.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == headid);

            ViewBag.FamilyMembers = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed && !e.IsHead)
                .ToList();

            ViewBag.FamilyMembersWithHead = _context
                .FamilyMembers.Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .ToList();

            FamilyHeadEditRequest request = new FamilyHeadEditRequest
            {
                FamilyHeadId = id,
                SPOId = familyHead.SPOId,
                FeedBy = familyHead.FeedBy,
                SurName = familyHead.SurName,
                GivenName = familyHead.GivenName,
                FatherName = familyHead.FatherHusbandName,
                CNIC = familyHead.CNICNo,
                DateOfBirth = familyHead.DOB,
                IsHead = familyHead.IsHead,
                Email = familyHead.Email,
                Number = familyHead.MobileNo,
                DateOfR = familyHead.RegistrationDate,
                GroupNo = familyHead.GroupNo,
                Address = familyHead.Address,
                Password = familyHead.Password,
                DataStatus = familyHead.Status,
                MImage = familyHead.MemberImage,
                PAttachment = familyHead.PassportAttachment,
                CAttachment = familyHead.CNICAttachment
            };

            return View(request);
        }

        public async Task<JsonResult> Store(FamilyHeadEditRequest request, int id)
        {

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                DataForm familyHead = await _context.DataForms.FindAsync(id);

                if (familyHead == null)
                {
                    return Json(new
                    {
                        status = 404,
                        msg = "Family head not found."
                    });
                }

                familyHead.SPOId = 23;
                familyHead.FeedBy = "USER";
                familyHead.SurName = request.SurName;
                familyHead.GivenName = request.GivenName;
                familyHead.FatherHusbandName = request.FatherName;
                familyHead.CNICNo = request.CNIC;
                familyHead.DOB = request.DateOfBirth;
                familyHead.RegistrationDate = request.DateOfR;
                familyHead.Email = request.Email;
                familyHead.MobileNo = request.Number;
                familyHead.Address = request.Address;

                if (request.MemberImage is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "MemberImages", request.MemberImage.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.MemberImage.CopyToAsync(fs);
                    }
                    familyHead.MemberImage = @"Images\MemberImages\" + request.MemberImage.FileName;
                }

                if (request.PassportAttachment is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PassportImages", request.PassportAttachment.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.PassportAttachment.CopyToAsync(fs);
                    }
                    familyHead.PassportAttachment = @"Images\PassportImages\" + request.PassportAttachment.FileName;
                }

                if (request.CNICAttachment is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.CNICAttachment.FileName);
                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    await using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.CNICAttachment.CopyToAsync(fs);
                    }
                    familyHead.CNICAttachment = @"Images\CnicUpload\" + request.CNICAttachment.FileName;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    status = 200,
                    msg = "Passport details saved!",
                    headId = id,
                    familyHead
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return Json(new
                {
                    status = 500,
                    msg = "An error occurred while saving passport details.",
                    details = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        public IActionResult AddMember(int familyHeadId)
        {
            FamilyHead familyHead = _context.FamilyHeads.FirstOrDefault(e =>
                e.IsHead && e.FamilyHeadId == familyHeadId
            );

            ViewBag.FeedBy = familyHead.FeedBy;
            ViewBag.SPOs = _context.SPOsWithoutTrash;

            ViewBag.familyHeadId = familyHeadId;

            ViewBag.SPOId = familyHead.SPOId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMember(FamilyMemberRegisterRequest request)
        {
            DataForm _familyHead = _context.DataForms.FirstOrDefault(e =>
                e.IsHead && e.DataFormId == request.FamilyHeadId
            );

            ViewBag.familyHeadId = request.FamilyHeadId;
            ViewBag.SPOs = _context.SPOsWithoutTrash;
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

                DataForm familyHead = new DataForm
                {
                    GroupNo = _familyHead.GroupNo,
                    GivenName = request.GivenName,
                    SurName = request.SurName,
                    CNICNo = request.CNIC,
                    IsHead = true,
                    DOB = request.DateOfBirth,
                    SPOId = request.SId,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    MobileNo = request.Number,
                    Email = request.Email,
                    FatherHusbandName = request.FatherHusbandName,
                    RegistrationDate = request.RegistrationDate,
                    Status = DataStatus.Unverified
                };

                _context.DataForms.Add(familyHead);
                _context.SaveChanges();

                _context.DataFormMembers.Add(
                    new DataFormMember
                    {
                        DataFormId = familyHead.DataFormId,
                        HeadId = _familyHead.DataFormId,
                        IsHead = false,
                    }
                );

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

        public IActionResult CreateTentative()
        {
            ViewBag.SPOs = _context.SPOsWithoutTrash;

            return View();
        }

        [HttpPost]
        public IActionResult CreateTentative(TentativeFamilyHead request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                ViewBag.SPOs = _context.SPOsWithoutTrash;

                if (!ModelState.IsValid)
                    return View(request);

                int Pax = request.adult + request.child + request.infant;

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

                int newIndId = lastIndId + 1;

                var lastFcIdNumber = _context.DataForms
                     .Where(e => e.GroupNo.StartsWith("ONLINE00009727"))
                     .Select(e => e.GroupNo.Substring(3))
                     .AsEnumerable()
                     .Select(fcId =>
                     {
                         int parsedNumber = 0;
                         int.TryParse(fcId, out parsedNumber);
                         return parsedNumber;
                     })
                     .OrderByDescending(x => x)
                     .FirstOrDefault();

                int nextReceiptNumber = lastFcIdNumber + 1;
                string newFcId = "ONLINE00009727" + nextReceiptNumber;


                DataForm fh = new DataForm
                {
                    GroupNo = request.GroupNo,
                    GivenName = request.GivenName,
                    SurName = request.SurName,
                    CNICNo = request.CNIC,
                    IsHead = true,
                    DOB = adultAge,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    MobileNo = request.Phone,
                    Email = request.Email,
                    FatherHusbandName = request.FatherHusbandName,
                    RegistrationDate = request.DateOfQuery,
                    Password = request.Password,
                    Status = DataStatus.Unverified
                };

                _context.DataForms.Add(fh);
                _context.SaveChanges();

                settings.FamilyCount++;
                settings.IndividualCount++;
                _context.SaveChanges();

                _context.DataFormMembers.Add(
                    new DataFormMember
                    {
                        DataFormId = fh.DataFormId,
                        HeadId = fh.DataFormId,
                        IsHead = true,
                    }
                );

                _context.SaveChanges();

                for (int i = 1; i < request.adult; i++)
                {
                    DataForm fhm = new DataForm
                    {
                        GroupNo = request.GroupNo,
                        GivenName = request.GivenName,
                        SurName = request.SurName,
                        CNICNo = request.CNIC,
                        IsHead = false,
                        DOB = adultAge,
                        SPOId = request.SPOID,
                        FeedBy = _http.HttpContext.Session.GetString("Username"),
                        MobileNo = request.Phone,
                        Email = request.Email,
                        FatherHusbandName = request.FatherHusbandName,
                        RegistrationDate = request.DateOfQuery,
                        Status = DataStatus.Unverified
                    };

                    _context.DataForms.Add(fhm);
                    _context.SaveChanges();

                    settings.IndividualCount++;
                    _context.SaveChanges();


                    _context.DataFormMembers.Add(
                        new DataFormMember
                        {
                            DataFormId = fhm.DataFormId,
                            HeadId = fh.DataFormId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();
                }

                for (int i = 0; i < request.child; i++)
                {
                    DataForm fhm = new DataForm
                    {
                        GroupNo = request.GroupNo,
                        GivenName = request.GivenName,
                        SurName = request.SurName,
                        CNICNo = request.CNIC,
                        IsHead = false,
                        DOB = adultAge,
                        SPOId = request.SPOID,
                        FeedBy = _http.HttpContext.Session.GetString("Username"),
                        MobileNo = request.Phone,
                        Email = request.Email,
                        FatherHusbandName = request.FatherHusbandName,
                        RegistrationDate = request.DateOfQuery,
                        Status = DataStatus.Unverified
                    };

                    _context.DataForms.Add(fhm);
                    _context.SaveChanges();

                    settings.IndividualCount++;
                    _context.SaveChanges();

                    _context.DataFormMembers.Add(
                        new DataFormMember
                        {
                            DataFormId = fhm.DataFormId,
                            HeadId = fh.DataFormId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();
                }

                for (int i = 0; i < request.infant; i++)
                {
                    DataForm fhm = new DataForm
                    {
                        GroupNo = request.GroupNo,
                        GivenName = request.GivenName,
                        SurName = request.SurName,
                        CNICNo = request.CNIC,
                        IsHead = false,
                        DOB = adultAge,
                        SPOId = request.SPOID,
                        FeedBy = _http.HttpContext.Session.GetString("Username"),
                        MobileNo = request.Phone,
                        Email = request.Email,
                        FatherHusbandName = request.FatherHusbandName,
                        RegistrationDate = request.DateOfQuery,
                        Status = DataStatus.Unverified
                    };

                    _context.DataForms.Add(fhm);
                    _context.SaveChanges();

                    settings.IndividualCount++;
                    _context.SaveChanges();

                    _context.DataFormMembers.Add(
                        new DataFormMember
                        {
                            DataFormId = fhm.DataFormId,
                            HeadId = fh.DataFormId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();
                }

                _context.SaveChanges();

                transaction.Commit();
                return RedirectToAction("Index", "DataForm");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Ok(new { Message = ex.Message });
            }
        }

        public IActionResult Hajj()
        {
            ViewBag.SPOs = _context.SPOsWithoutTrash;

            return View();
        }

        [HttpPost]
        public IActionResult Hajj(TentativeFamilyHead request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                ViewBag.SPOs = _context.SPOsWithoutTrash;

                if (!ModelState.IsValid)
                    return View(request);

                int Pax = request.adult + request.child + request.infant;

                float packageTotalPkr = 0;

                DateTime currentDate = DateTime.Today;
                DateTime dateTime = DateTime.Parse(currentDate.ToString());

                DateTime infantAge = DateTime.Today;
                DateTime childAge = DateTime.Today.AddYears(-3);
                DateTime adultAge = DateTime.Today.AddYears(-13);

                DataForm fh = new DataForm
                {
                    GroupNo = request.GroupNo,
                    GivenName = request.GivenName,
                    SurName = request.SurName,
                    CNICNo = request.CNIC,
                    IsHead = true,
                    DOB = adultAge,
                    SPOId = 23,
                    FeedBy = "USER",
                    MobileNo = request.Phone,
                    Email = request.Email,
                    FatherHusbandName = request.FatherHusbandName,
                    RegistrationDate = request.DateOfQuery,
                    Password = request.Password,
                    Status = DataStatus.Unverified
                };

                _context.DataForms.Add(fh);
                _context.SaveChanges();

                _context.DataFormMembers.Add(
                    new DataFormMember
                    {
                        DataFormId = fh.DataFormId,
                        HeadId = fh.DataFormId,
                        IsHead = true,
                    }
                );

                _context.SaveChanges();

                for (int i = 1; i < request.adult; i++)
                {
                    DataForm fhm = new DataForm
                    {
                        GroupNo = request.GroupNo,
                        GivenName = request.GivenName,
                        SurName = request.SurName,
                        CNICNo = request.CNIC,
                        IsHead = false,
                        DOB = adultAge,
                        SPOId = 23,
                        FeedBy = "USER",
                        MobileNo = request.Phone,
                        Email = request.Email,
                        FatherHusbandName = request.FatherHusbandName,
                        RegistrationDate = request.DateOfQuery,
                        Status = DataStatus.Unverified
                    };

                    _context.DataForms.Add(fhm);
                    _context.SaveChanges();

                    _context.DataFormMembers.Add(
                        new DataFormMember
                        {
                            DataFormId = fhm.DataFormId,
                            HeadId = fh.DataFormId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();
                }

                for (int i = 0; i < request.child; i++)
                {
                    DataForm fhm = new DataForm
                    {
                        GroupNo = request.GroupNo,
                        GivenName = request.GivenName,
                        SurName = request.SurName,
                        CNICNo = request.CNIC,
                        IsHead = false,
                        DOB = adultAge,
                        SPOId = 23,
                        FeedBy = "USER",
                        MobileNo = request.Phone,
                        Email = request.Email,
                        FatherHusbandName = request.FatherHusbandName,
                        RegistrationDate = request.DateOfQuery,
                        Status = DataStatus.Unverified
                    };

                    _context.DataForms.Add(fhm);
                    _context.SaveChanges();

                    _context.DataFormMembers.Add(
                        new DataFormMember
                        {
                            DataFormId = fhm.DataFormId,
                            HeadId = fh.DataFormId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();
                }

                for (int i = 0; i < request.infant; i++)
                {
                    DataForm fhm = new DataForm
                    {
                        GroupNo = request.GroupNo,
                        GivenName = request.GivenName,
                        SurName = request.SurName,
                        CNICNo = request.CNIC,
                        IsHead = false,
                        DOB = adultAge,
                        SPOId = 23,
                        FeedBy = "USER",
                        MobileNo = request.Phone,
                        Email = request.Email,
                        FatherHusbandName = request.FatherHusbandName,
                        RegistrationDate = request.DateOfQuery,
                        Status = DataStatus.Unverified
                    };

                    _context.DataForms.Add(fhm);
                    _context.SaveChanges();

                    _context.DataFormMembers.Add(
                        new DataFormMember
                        {
                            DataFormId = fhm.DataFormId,
                            HeadId = fh.DataFormId,
                            IsHead = false,
                        }
                    );

                    _context.SaveChanges();
                }

                _context.SaveChanges();

                transaction.Commit();
                return RedirectToAction("View", "DataForm", new { id = fh.DataFormId });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Ok(new { Message = ex.Message });
            }
        }

        public IActionResult HajjRegistration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult HajjRegistration(HajjRegistration request)
        {
            List<DataForm> dataForms = new List<DataForm>();

            for (int i = 0; i < request.GivenName.Count; i++)
            {
                DataForm fh = new DataForm
                {
                    GivenName = i < request.GivenName.Count ? request.GivenName[i] : null,
                    SurName = i < request.SurName.Count ? request.SurName[i] : null,
                    CNICNo = i < request.CNIC.Count ? request.CNIC[i] : null,
                    IsHead = (i == 0),
                    DOB = i < request.DateOfBirth.Count && request.DateOfBirth[i] != null ? request.DateOfBirth[i] : DateTime.MinValue,
                    SPOId = 23,
                    FeedBy = "USER",
                    MobileNo = i < request.Number.Count ? request.Number[i] : null,
                    Email = i < request.Email.Count ? request.Email[i] : null,
                    FatherHusbandName = i < request.FatherHusbandName.Count ? request.FatherHusbandName[i] : null,
                    Status = DataStatus.Unverified,
                    Address = i < request.Address.Count ? request.Address[i] : null
                };

                if (request.MemberImage != null && request.MemberImage.Count > i && request.MemberImage[i] != null)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "Images", "MemberImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, request.MemberImage[i].FileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        request.MemberImage[i].CopyTo(fs);
                    }

                    fh.MemberImage = Path.Combine("Images", "MemberImages", request.MemberImage[i].FileName);
                }

                if (request.PassportAttachment != null && request.PassportAttachment.Count > i && request.PassportAttachment[i] != null)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "Images", "PassportImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, request.PassportAttachment[i].FileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        request.PassportAttachment[i].CopyTo(fs);
                    }

                    fh.PassportAttachment = Path.Combine("Images", "PassportImages", request.PassportAttachment[i].FileName);
                }

                if (request.CNICAttachment != null && request.CNICAttachment.Count > i && request.CNICAttachment[i] != null)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "Images", "CnicUpload");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, request.CNICAttachment[i].FileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        request.CNICAttachment[i].CopyTo(fs);
                    }

                    fh.CNICAttachment = Path.Combine("Images", "CnicUpload", request.CNICAttachment[i].FileName);
                }

                if (request.CNICBackAttachment != null && request.CNICBackAttachment.Count > i && request.CNICBackAttachment[i] != null)
                {
                    string uploadsFolder = Path.Combine(_host.WebRootPath, "Images", "CnicUpload");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, request.CNICBackAttachment[i].FileName);

                    if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        request.CNICBackAttachment[i].CopyTo(fs);
                    }

                    fh.CNICBackAttachment = Path.Combine("Images", "CnicUpload", request.CNICBackAttachment[i].FileName);
                }

                dataForms.Add(fh);
                _context.DataForms.Add(fh);
            }

            _context.SaveChanges();

            var head = dataForms.FirstOrDefault(x => x.IsHead);
            int headId = head?.DataFormId ?? 0;

            foreach (var form in dataForms)
            {
                _context.DataFormMembers.Add(new DataFormMember
                {
                    DataFormId = form.DataFormId,
                    HeadId = headId,
                    IsHead = form.IsHead
                });
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(ThanksPage));
        }

        public IActionResult ThanksPage()
        {
            return View();
        }
    }
}


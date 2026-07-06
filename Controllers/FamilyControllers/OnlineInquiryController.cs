using System;
using System.IO.Compression;
using System.Net.Mail;
using System.Net;
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
using Microsoft.IdentityModel.Tokens;

//using AspNetCore;


namespace TMG.Controllers.FamilyControllers
{
    public class OnlineInquiryController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public OnlineInquiryController(
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
        public IActionResult OnlineIndex()
        {
            ViewBag.HajjSeasonName = _context.HajjSeasons
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(e => e.Season)
                .FirstOrDefault();

            ViewBag.TrashCount = _context.OnlineInquiries.Where(e => e.Trashed).Count();

            var data = _context
                .OnlineInquiries
                .Include(e => e.NoOfDaysHajj)
                .Include(e => e.NoOfDaysTour)
                .Include(e => e.NoOfDaysUmrah)
                .Where(e => !e.Trashed)
                .OrderByDescending(e => e.OnlineInquiryId)
                .ToList();

            var dataWithSerialNumbers = data.Select(
                    (item, index) =>
                        new CustomerPortfolioViewModel
                        {
                            SerialNumber = index + 1,
                            CustomerPortfolioId = item.OnlineInquiryId,
                            FullName = item.GivenName + " " + item.SurName,
                            Contact = item.MobileNo,
                            Email = item.Email,
                            DateOfInquiry = item.CreatedAt.ToString("dd/MMM/yyyy"),
                            Package = item.NoOfDaysHajjId.HasValue
                                ? item.NoOfDaysHajj.NoOfDays
                                    : item.NoOfDaysUmrahId.HasValue
                                        ? item.NoOfDaysUmrah.NoOfDays
                                        : item.NoOfDaysTourId.HasValue
                                            ? item.NoOfDaysTour.NoOfDays
                                            : "N/A",
                            Department = item.NoOfDaysHajjId.HasValue
                                ? item.NoOfDaysHajj.Type
                                    : item.NoOfDaysUmrahId.HasValue
                                        ? item.NoOfDaysUmrah.Type
                                        : item.NoOfDaysTourId.HasValue
                                            ? item.NoOfDaysTour.Type
                                            : "N/A",
                        }
                )
                .ToList();

            var membersCount = _context
                .CustomerPortfoliosMembers.Where(m =>
                    m.CustomerPortfolio.HajjSeasonsId == null
                    && !m.CustomerPortfolio.Trashed
                )
                .GroupBy(m => m.HeadId)
                .ToDictionary(g => g.Key, g => g.Count());

            var numbers = _context
                .CustomerPortfolioContacts
                .GroupBy(e => e.CustomerPortfolioId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault()?.Contact);

            var emails = _context
                .CustomerPortfolioEmails
                .GroupBy(e => e.CustomerPortfolioId)
                .ToDictionary(e => e.Key, e => e.FirstOrDefault()?.Email);

            var applicantTypeNames = Enum.GetValues(typeof(ApplicantType))
                .Cast<ApplicantType>()
                .ToDictionary(e => (int)e, e => e.ToString());

            ViewBag.ApplicantTypeNames = applicantTypeNames;

            ViewBag.MembersCount = membersCount;

            ViewBag.Numbers = numbers;

            ViewBag.Emails = emails;

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

        public IActionResult ViewTrash(int id)
        {
            return View(_context.OnlineInquiries.Where(s => s.Trashed).ToList());
        }

        public IActionResult Trash(int id)
        {
            OnlineInquiry familyHead = _context.OnlineInquiries.Find(id);
            if (familyHead == null)
                return RedirectToAction(nameof(OnlineIndex));

            familyHead.Trashed = true;

            _context.SaveChanges();

            return RedirectToAction(nameof(OnlineIndex));
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

        public IActionResult Restore(int id)
        {
            OnlineInquiry fiqah = _context.OnlineInquiries.Find(id);
            fiqah.Trashed = false;
            _context.SaveChanges();
            return RedirectToAction(nameof(OnlineIndex));
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

        public IActionResult Inquiry()
        {
            ViewBag.Destination = _context.Destinations.Where(e => !e.Trashed).ToList();

            ViewBag.Categories = _context.AccomodationCategories.Where(e => !e.Trashed).ToList();

            ViewBag.NoOfDaysHajj = _context.InquiryEmails
                .Where(e => e.Type == "Hajj")
                .ToList();

            ViewBag.NoOfDaysUmrah = _context.InquiryEmails
                .Where(e => e.Type == "Umrah")
                .ToList();

            ViewBag.NoOfDaysUmrahRamzan = _context.InquiryEmails
                .Where(e => e.Type == "UmrahRamadan")
                .ToList();

            ViewBag.NoOfDaysTour = _context.InquiryEmails
                .Where(e => e.Type == "Tour")
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Inquiry(OnlineInquiry inquiry)
        {
            _context.OnlineInquiries.Add(inquiry);
            _context.SaveChanges();

            var (subject, body, pdfFileName) = GetEmailTemplate(inquiry);

            if (!string.IsNullOrEmpty(inquiry.Email) && !string.IsNullOrEmpty(pdfFileName))
            {
                SendEmailWithAttachment(inquiry.Email, subject, body, pdfFileName);
            }

            return RedirectToAction(nameof(ThanksPage));
        }

        private (string subject, string body, string pdfFileName) GetEmailTemplate(OnlineInquiry inquiry)
        {
            int? noOfDays = null;
            string type = null;

            if (inquiry.NoOfDaysHajjId.HasValue)
            {
                noOfDays = inquiry.NoOfDaysHajjId;
                type = "Hajj";
            }
            else if (inquiry.NoOfDaysUmrahId.HasValue)
            {
                noOfDays = inquiry.NoOfDaysUmrahId;
                type = "Umrah";
            }
            else if (inquiry.NoOfDaysTourId.HasValue)
            {
                noOfDays = inquiry.NoOfDaysTourId;
                type = "Tour";
            }

            if (noOfDays.HasValue)
            {
                var emailTemplate = _context.InquiryEmails
                    .FirstOrDefault(e => e.InquiryEmailId == noOfDays.Value && e.Type == type);

                if (emailTemplate != null)
                {
                    return (emailTemplate.Subject, emailTemplate.Message, emailTemplate.Attachment);
                }
            }

            return ("Your Inquiry", "Thanks for your interest.", null);
        }

        private void SendEmailWithAttachment(string toEmail, string subject, string body, string pdfRelativePath)
        {
            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG");
            var toAddress = new MailAddress(toEmail);
            const string fromPassword = "U7H#sT8sLXA^2!8k";

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pdfRelativePath.TrimStart('/'));

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                if (!string.IsNullOrEmpty(pdfRelativePath) && System.IO.File.Exists(filePath))
                {
                    message.Attachments.Add(new Attachment(filePath));
                }

                smtp.Send(message);
            }
        }

        [HttpGet]
        public JsonResult GetCountriesByDestination(int destinationId)
        {
            var countries = _context.Countries
                .Where(c => c.DestinationId == destinationId)
                .Select(c => new { c.CountryId, c.Name })
                .ToList();

            return Json(countries);
        }

        [HttpGet]
        public JsonResult GetCitiesByCountry(int countryId)
        {
            var cities = _context.Cities
                .Where(c => c.CountryId == countryId)
                .Select(c => new { c.CityId, c.Name })
                .ToList();

            return Json(cities);
        }

        public IActionResult ThanksPage()
        {
            return View();
        }

    }
}

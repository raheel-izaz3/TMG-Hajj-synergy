using TMG.Data;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using TMG.ManagerClasses;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Cryptography.Pkcs;
using TMG.Models.Updated_Accounts;


namespace TMG.Models.AccountModels
{
    public record LedgerReportModelMiniStatement
    {
        public Account Account { get; set; }
        public List<DateTime> DateTimes { get; set; } = new List<DateTime>();
        public List<int> VoucherIds { get; set; } = new List<int>();
        public List<string> Details { get; set; } = new List<string>();
        public List<float> Debits { get; set; } = new List<float>();
        public List<float> Credits { get; set; } = new List<float>();
        public List<float> Balances { get; set; } = new List<float>();
        public List<float> PkrRoe { get; set; } = new List<float>();
        public List<float> SarRoe { get; set; } = new List<float>();
        public List<float> UsdRoe { get; set; } = new List<float>();
        public List<string> Currency { get; set; } = new List<string>();
        public List<float> Tickets { get; set; } = new();

    }

    public record InvoiceModel(string Logo, string Stamp, bool IsTentative);
}

namespace TMG.Controllers.AccountControllers
{

    public record FamilyPackageModel
    {
        public List<string> PackageId { get; set; } = new List<string>();

    }
    public class MiniStatementController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public MiniStatementController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.FamilyHead = _dbContext.FamilyHeadsWithoutTrash
                                    .Where(e => e.IsHead);
            return View();
        }

        public IActionResult Client()
        {

            if (TempData.ContainsKey("ErrorMessage"))
            {
                // Retrieve the error message from TempData
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            }

            // Retrieve family head data
            ViewBag.FamilyHead = _dbContext.FamilyHeadsWithoutTrash
                                      .Where(e => e.IsHead && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            // Return the Client view
            return View();
        }

        public IActionResult Print(int id)
        {
            ViewBag.familyId = id;
            return View();
        }

        public IActionResult Invoice(int id)
        {
            ViewBag.familyId = id;
            var fam = _dbContext.FamilyHeads.Include(e => e.Package)
            .ThenInclude(e => e.Company).Select(e => new
            {
                e.FamilyHeadId,
                e.PackageId,
                e.Package.CompanyId,
                e.FlightTicketType,
                e.Tentative
            }).FirstOrDefault(e => e.FamilyHeadId == id);

            var familyROE = _dbContext.RoeRequests.FirstOrDefault(e => e.FamilyHeadId == id);
            RateOfExchange ROE = null;
            if (familyROE is null)
            {
                ROE = _dbContext.RateOfExchanges.OrderByDescending(e => e.RateOfExchangeID).FirstOrDefault();
            }

            ViewBag.FamilyROE = familyROE;
            ViewBag.ROE = ROE;

            ViewBag.FlightTicketType = fam.FlightTicketType.ToString();

            Company company = _dbContext.Companies.Find(fam.CompanyId);

            return View(new InvoiceModel(company.Logo, company.Stamp, fam.Tentative));
        }

        [HttpGet]
        public IActionResult GetFamilyDetail(int id)
        {
            FamilyHead _ = _dbContext.FamilyHeads.Find(id);
            int familyPackageId = _.PackageId;
            bool isTentative = _.Tentative;

            var fam = _dbContext.FamilyMembers
                    .Include(e => e.FamilyHead)
                    .Where(e => e.HeadId == id)
                    .ToList();

            int infantCount = 0, childCount = 0, adultCount = 0;

            infantCount = fam.Where(e => e.FamilyHead.Age < 2).Count();
            childCount = fam.Where(e => e.FamilyHead.Age >= 2 && e.FamilyHead.Age < 12).Count();
            adultCount = fam.Where(e => e.FamilyHead.Age >= 12).Count();

            float adultBasicPackagePkr = 0f, childBasicPackagePkr = 0f, infantBasicPackagePkr = 0f;
            float adultBasicPackageSar = 0f, childBasicPackageSar = 0f, infantBasicPackageSar = 0f;
            float adultBasicPackageUsd = 0f, childBasicPackageUsd = 0f, infantBasicPackageUsd = 0f;

            Package _p = _dbContext.Packages.Find(_.PackageId);

            adultBasicPackagePkr = _p.AdultPkr - _.PkrDiscountAmount;
            childBasicPackagePkr = _p.ChildPkr - _.PkrDiscountAmount;
            infantBasicPackagePkr = _p.InfantPkr - _.PkrDiscountAmount;

            adultBasicPackageSar = _p.AdultSar - _.SarDiscountAmount;
            childBasicPackageSar = _p.ChildSar - _.SarDiscountAmount;
            infantBasicPackageSar = _p.InfantSar - _.SarDiscountAmount;

            adultBasicPackageUsd = _p.AdultUsd - _.UsdDiscountAmount;
            childBasicPackageUsd = _p.ChildUsd - _.UsdDiscountAmount;
            infantBasicPackageUsd = _p.InfantUsd - _.UsdDiscountAmount;

            var _addons = _dbContext.PackageAddons.Where(e => e.PackageId == _p.PackageId).ToList();

            // adultBasicPackagePkr += _addons.Select(e => e.PkrPrice).Sum();
            // childBasicPackagePkr += _addons.Select(e => e.PkrPrice).Sum();
            // infantBasicPackagePkr += _addons.Select(e => e.PkrPrice).Sum();

            // adultBasicPackageSar += _addons.Select(e => e.SarPrice).Sum();
            // childBasicPackageSar += _addons.Select(e => e.SarPrice).Sum();
            // infantBasicPackageSar += _addons.Select(e => e.SarPrice).Sum();

            // adultBasicPackageUsd += _addons.Select(e => e.UsdPrice).Sum();
            // childBasicPackageUsd += _addons.Select(e => e.UsdPrice).Sum();
            // infantBasicPackageUsd += _addons.Select(e => e.UsdPrice).Sum();


            if (infantCount == 0)
            {
                infantBasicPackagePkr = 0;
                infantBasicPackageSar = 0;
                infantBasicPackageUsd = 0;
            }

            if (childCount == 0)
            {
                childBasicPackagePkr = 0;
                childBasicPackageSar = 0;
                childBasicPackageUsd = 0;
            }

            if (adultCount == 0)
            {
                adultBasicPackagePkr = 0;
                adultBasicPackageSar = 0;
                adultBasicPackageUsd = 0;
            }


            int CompanyId = _dbContext.FamilyHeadsWithoutTrash
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Where(e => e.PackageId == familyPackageId)
                                .Where(e => e.FamilyHeadId == id)
                                .Select(e => e.Package.Company.CompanyId)
                                .FirstOrDefault();

            int AccountId = _dbContext.FamilyHeadsWithoutTrash
                                .Include(e => e.Account)
                                .Where(e => e.FamilyHeadId == id)
                                .Select(e => e.Account.AccountId)
                                .FirstOrDefault();

            var FamilyHead = _dbContext.FamilyHeadsWithoutTrash
                                .Where(e => e.FamilyHeadId == id);

            var SPO = _dbContext.FamilyHeadsWithoutTrash
                                .Include(e => e.SPO)
                                .Where(e => e.FamilyHeadId == id);

            string FamilyHeadAddress = _dbContext.FamilyHeadAddresses
                                        .Where(e => e.FamilyHeadId == id)
                                        .Select(e => e.Address)
                                        .FirstOrDefault();

            string FamilyHeadEmail = _dbContext.FamilyHeadEmails
                                        .Where(e => e.FamilyHeadId == id)
                                        .Select(e => e.Email)
                                        .FirstOrDefault();

            string FamilyHeadContact = _dbContext.FamilyHeadContacts
                                        .Where(e => e.FamilyHeadId == id)
                                        .Select(e => e.Contact)
                                        .FirstOrDefault();

            var FamilyMember = _dbContext
                    .FamilyMembers
                    .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                    .Join(
                        _dbContext.FamilyHeads,
                        member => member.FamilyHeadId,
                        head => head.FamilyHeadId,
                        (member, head) => new { FamilyMember = member, FamilyHead = head }
                    )
                    .Join(
                        _dbContext.Packages,
                        result => result.FamilyHead.PackageId,
                        package => package.PackageId,
                        (result, package) => new
                        {
                            result.FamilyMember,
                            result.FamilyHead,
                            Package = package
                        }
                    )
                    .ToList();

            var Invoice = _dbContext
                    .InvoicesWithoutTrash
                    .Join(
                        _dbContext.FamilyHeads,
                        invoice => invoice.FamilyHeadId,
                        head => head.FamilyHeadId,
                        (invoice, head) => new { Invoice = invoice, FamilyHead = head }
                    )
                    .Where(e => e.FamilyHead.FamilyHeadId == id)
                    .ToList();

            var FamilyPackage = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id);

            var PackageId = _dbContext.FamilyHeadsWithoutTrash
                            .Include(e => e.Package)
                            .Where(e => e.PackageId == e.Package.PackageId)
                            .Select(e => e.Package.PackageId)
                            .FirstOrDefault();

            var CompanyBank = _dbContext.CompanyBankAccountsWithoutTrash
                                .Where(e => e.CompanyId == CompanyId);

            var Company = _dbContext.CompaniesWithoutTrash
                                .Where(e => e.CompanyId == CompanyId);

            var CompanyAddress = _dbContext.CompanyAddresses
                                .Where(e => e.CompanyId == CompanyId);

            var CompanyEmail = _dbContext.CompanyEmails
                                .Where(e => e.CompanyId == CompanyId);

            var CompanyContact = _dbContext.CompanyContacts
                                .Where(e => e.CompanyId == CompanyId);

            var PackageAddons = _dbContext.PackageAddons
                                .Where(e => e.PackageId == PackageId);


            var AdditionalFacilityMemberId = _dbContext
                    .FamilyMembers
                    .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                    .Select(e => e.FamilyHeadId)
                    .ToList();

            var AdditionalFacility = _dbContext.FamilyHeadAdditionalFacilities
                .Include(e => e.AdditionalFacilities)
                .Where(e => AdditionalFacilityMemberId.Contains(e.FamilyHeadId));


            var MiniStatement = _dbContext.VoucherEntries.Where(e => e.AccountId == AccountId);

            var accounts = _dbContext.AccountsWithoutTrash
                .Include(e => e.Category)
                .ThenInclude(e => e.SubType)
                .ThenInclude(e => e.Type)
                .Where(e => e.AccountId == AccountId) as IEnumerable<Account>;

            var vouchers = _dbContext.PaymentVouchersWithoutTrash
                .Include(e => e.VoucherType);

            var voucherEntries = _dbContext.VoucherEntries
                .Include(e => e.Account)
                .Include(e => e.PaymentVoucher);


            List<int> memberIds = _dbContext.FamilyMembers.Where(e => e.HeadId == id).Select(e => e.FamilyHeadId).ToList();

            var extrasAdfs = _dbContext.FamilyHeadExtras
                .Include(e => e.ExtraFacilityDescription)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => memberIds.Contains(e.FamilyHeadId)).ToList();

            List<LedgerReportModelMiniStatement> models = new List<LedgerReportModelMiniStatement>();

            Account currentAccount = accounts.ElementAt(0);
            LedgerReportModelMiniStatement model = new LedgerReportModelMiniStatement { Account = currentAccount };

            var currentAccountEntries = voucherEntries.Where(e => e.AccountId == currentAccount.AccountId) as IEnumerable<VoucherEntry>;

            float startingBalance = 0;
            float balanceCarriedForward = 0;
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

            var RateOfExchange = _dbContext.RateOfExchangesWithoutTrash
            .Where(e => e.Currency == Currencies.USD)
            .OrderByDescending(e => e.Date);


            return Ok(new
            {
                FamilyHead,
                FamilyMember,
                FamilyPackage,
                MiniStatement,
                FamilyHeadAddress,
                FamilyHeadContact,
                FamilyHeadEmail,
                PackageAddons,
                /*AdditionalFacility,*/
                model,
                Invoice,
                SPO,
                Company,
                CompanyBank,
                CompanyAddress,
                CompanyEmail,
                CompanyContact,
                extrasAdfs,
                RateOfExchange,
                isTentative,
                adultCount,
                childCount,
                infantCount,
                adultBasicPackagePkr,
                adultBasicPackageSar,
                adultBasicPackageUsd,

                childBasicPackagePkr,
                childBasicPackageSar,
                childBasicPackageUsd,

                infantBasicPackagePkr,
                infantBasicPackageSar,
                infantBasicPackageUsd,
                headNameWithId = $"{_?.FullName} ({_?.FcId})"

            });
        }

        public IActionResult NewInvoice(int id)
        {
            if (id > 0)
            {

                FamilyHead _ = _dbContext.FamilyHeads
                .Include(e => e.SPO)
                .Include(e => e.Reference)
                .FirstOrDefault(e => e.FamilyHeadId == id);

                int familyPackageId = _.PackageId;
                bool isTentative = _.Tentative;

                List<int> memberIds = _dbContext.FamilyMembers
                                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                                .Select(e => e.FamilyHeadId)
                                .ToList();

                var remarks = _dbContext.FamilyHeadRemarks
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => memberIds.Contains(e.FamilyHeadId))
                            .Select(e => new
                            {
                                e.Remarks,
                                e.FamilyHeadId,
                                e.FamilyHead.IndId,
                                e.FamilyHead.Package.Company.CompanyCode
                            })
                            .ToList();

                var fam = _dbContext.FamilyMembers
                        .Include(e => e.FamilyHead)
                        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                        .ToList();

                Package _p = _dbContext.Packages
                            .Include(e => e.Company)
                            .FirstOrDefault(e => e.PackageId == _.PackageId);

                string FamilyHeadAddress = _dbContext.FamilyHeadAddresses
                                .Where(e => e.FamilyHeadId == id)
                                .Select(e => e.Address)
                                .FirstOrDefault();

                string FamilyHeadContact = _dbContext.FamilyHeadContacts
                                            .Where(e => e.FamilyHeadId == id)
                                            .Select(e => e.Contact)
                                            .FirstOrDefault();

                string cId =
                                $"{_.Package.Company.CompanyCode}.{_.IndId}" +
                                $".{_.ExtractedFamilyNumber}.({fam.Count()})" +
                                $".{_.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                        .Include(e => e.Group)
                        .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && !e.Group.Trashed);

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }

                Invoice invoice = _dbContext.InvoicesWithoutTrash
                                        .OrderByDescending(e => e.ProformaNumber)
                                        .FirstOrDefault(e => e.FamilyHeadId == id);

                float ticketTotal = 0;
                float ticketDiffTotal = 0;
                float discount = 0;
                float packagePriceTotal = 0;

                float infantPackageUsd = 0, childPackageUsd = 0, adultPackageUsd = 0;

                foreach (var f in fam)
                {
                    if (f.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                    {
                        ticketTotal += _p.FlightTicketPriceUsd;
                    }
                    if (f.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                    {
                        ticketDiffTotal += f.FamilyHead.FlightTicketDifferencesUsd ?? 0;
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

                var extras = _dbContext.FamilyHeadExtras
                        .Include(e => e.ExtraFacilityDescription)
                        .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Package)
                        .ThenInclude(e => e.Company)
                        .Where(e => memberIds.Contains(e.FamilyHeadId)
                        && e.ExtraFacilityDescriptionId != null);

                var distinctExtras = extras.Select(e => new
                {
                    e.FamilyHead.Package.Company.CompanyCode,
                    e.FamilyHead.IndId,
                }).Distinct().ToList();

                float extrasTotal = extras
                        .Sum(e => e.PriceUsd) ?? 0;

                string extraMemberString = "";
                foreach (var e in distinctExtras)
                {
                    extraMemberString += extraMemberString.Equals("") ? $"{e.CompanyCode}-{e.IndId}"
                    : $", {e.CompanyCode}-{e.IndId}";
                }

                ProformaViewModel model = new ProformaViewModel()
                {
                    FlightTicketType = _.FlightTicketType,
                    PackageDetails = $"{_p.Name}-{_p.Days} DAYS-MEDINAH {_p.MedinahArrival} HAJJ",
                    MegaReference = _.Reference?.ReferenceName,
                    FCID = _.FcId,
                    CompanyLogo = _p?.Company?.Logo,
                    CompanyName = _p?.Company?.CompanyName,
                    GuestName = _?.FullName,
                    InvoiceToCustomer = _?.FullName,
                    InvoiceToCompany = _p?.Company?.CompanyName,
                    Category = _?.ApplicantType.ToString(),
                    CNIC = _?.CNIC,
                    Contact = FamilyHeadContact,
                    Address = FamilyHeadAddress,
                    Date = DateTime.Now.ToString("dd/MM/yyyy"),
                    InvoiceNumber = "INV-" + invoice?.ProformaNumber,
                    Passport = _?.PassportNumber,
                    SPO = _?.SPO.Name,
                    Pax = fam.Count(),
                    Purpose = "Travel Arrangements",
                    CID = cId,
                    AirTicketUsd = "$" + ticketTotal.ToString("N0"),
                    AirTicketDifferenceUsd = "$" + ticketDiffTotal.ToString("N0"),
                    EIADFPriceUsd = "$" + extrasTotal.ToString("N0"),
                    EIADFMembers = extraMemberString,
                };

                var iadfRooms = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .ThenInclude(e => e.Room)
                            .Include(e => e.AdditionalFacilities)
                            .ThenInclude(e => e.IndividiualFacilityDescription)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => memberIds.Contains(e.FamilyHeadId))
                            .GroupBy(e => e.AdditionalFacilities.RoomId)
                            .ToList();

                List<IADFRow> rows = new();

                //float iadfPriceTotal = 0;
                float basicPackagePriceWithIadfOverall = 0f;

                if (iadfRooms.Count == 0)
                {
                    IADFRow row = new();
                    row.RoomType = "SHARING";
                    row.PriceAmount = packagePriceTotal;
                    row.PriceString = (packagePriceTotal).ToString("N0");
                    row.PAX = fam.Count;

                    float perPerson = packagePriceTotal / fam.Count;

                    string perPersonCalc = "";

                    int adultCount = fam.Count(e => e.FamilyHead.Age >= 12);
                    int childCount = fam.Count(e => e.FamilyHead.Age < 12 && e.FamilyHead.Age >= 2);
                    int infantCount = fam.Count(e => e.FamilyHead.Age < 2);

                    if (adultCount != 0 || childCount != 0 || infantCount != 0)
                    {
                        perPersonCalc += "(";

                        if (adultCount != 0)
                        {
                            perPersonCalc += $"ADULT: ${adultPackageUsd.ToString("N0")} x {adultCount}";
                        }
                        if (childCount != 0)
                        {
                            perPersonCalc += perPersonCalc.Equals("") ? $"CHILD: ${childPackageUsd.ToString("N0")} x " + childCount : $", CHILD: ${childPackageUsd.ToString("N0")} x " + childCount;
                        }
                        if (infantCount != 0)
                        {
                            perPersonCalc += perPersonCalc.Equals("") ? $"INFANT: ${infantPackageUsd.ToString("N0")} x " + infantCount : $", INFANT: ${infantPackageUsd.ToString("N0")} x " + infantCount;
                        }

                        perPersonCalc += ")";
                    }

                    row.PerPersonCalc = perPersonCalc;

                    model.IADFRows.Add(row);
                }
                else
                {
                    foreach (var r in iadfRooms)
                    {
                        IADFRow row = new();
                        row.RoomType = r.ElementAtOrDefault(0)?.AdditionalFacilities?.Room?.RoomName?.ToUpper();

                        var distinctR = r.Select(e => new
                        {
                            e.FamilyHeadId,
                            e.FamilyHead.Package.Company.CompanyCode,
                            e.FamilyHead.IndId,
                            e.FamilyHead.Age,
                        }).Distinct().ToList();

                        string roomString = "";
                        //float iadfPrice = 0;
                        float adultIadfPrice = 0, childIadfPrice = 0, infantIadfPrice = 0;
                        float adultPackagePrice = 0, childPackagePrice = 0, infantPackagePrice = 0;

                        foreach (var obj in distinctR)
                        {
                            roomString += roomString.Equals("") ? $"{obj.CompanyCode}-{obj.IndId}" : $", {obj.CompanyCode}-{obj.IndId}";
                            if (obj.Age < 2)
                            {
                                //iadfPrice += _p.InfantUsd;
                                infantPackagePrice += _p.InfantUsd;
                            }
                            else if (obj.Age < 12)
                            {
                                //iadfPrice += _p.ChildUsd;
                                childPackagePrice += _p.ChildUsd;
                            }
                            else
                            {
                                //iadfPrice += _p.AdultUsd;
                                adultPackagePrice += _p.AdultUsd;
                            }
                        }

                        row.RoomMembers = roomString;

                        foreach (var obj in r)
                        {
                            if (obj.FamilyHead.Age < 2)
                            {
                                infantIadfPrice += obj.AdditionalFacilities.InfantUsd ?? 0;
                            }
                            else if (obj.FamilyHead.Age < 12)
                            {
                                childIadfPrice += obj.AdditionalFacilities.ChildUsd ?? 0;
                            }
                            else
                            {
                                adultIadfPrice += obj.AdditionalFacilities.AdultUsd ?? 0;
                            }
                        }

                        float basicPackageWithIadf = (adultPackagePrice + childPackagePrice + infantPackagePrice)
                                + (adultIadfPrice + childIadfPrice + infantIadfPrice);
                        basicPackagePriceWithIadfOverall += basicPackageWithIadf;

                        //iadfPriceTotal += iadfPrice;

                        row.PriceString = "$" + basicPackageWithIadf.ToString("N0");
                        row.PriceAmount = basicPackageWithIadf;
                        row.PAX = r.ElementAtOrDefault(0)?.AdditionalFacilities?.Room?.PAX ?? 1;
                        //float perPerson = basicPackageWithIadf / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;

                        string perPersonCalc = "";

                        int adultCount = distinctR.Count(e => e.Age >= 12);
                        int childCount = distinctR.Count(e => e.Age < 12 && e.Age >= 2);
                        int infantCount = distinctR.Count(e => e.Age < 2);
                        //float adultPerPerson = ((adultPackagePrice + adultIadfPrice)) / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;
                        //float childPerPerson = (childPackagePrice + childIadfPrice) / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;
                        //float infantPerPerson = (infantPackagePrice + infantIadfPrice) / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;

                        float adultPerPerson = ((adultPackagePrice + adultIadfPrice)) / adultCount;
                        float childPerPerson = (childPackagePrice + childIadfPrice) / childCount;
                        float infantPerPerson = (infantPackagePrice + infantIadfPrice) / infantCount;


                        if (adultCount != 0 || childCount != 0 || infantCount != 0)
                        {
                            perPersonCalc += "(";

                            if (adultCount != 0)
                            {
                                perPersonCalc += $"ADULT: ${adultPerPerson.ToString("N0")} x {adultCount}";
                            }
                            if (childCount != 0)
                            {
                                perPersonCalc += perPersonCalc.Equals("") ? $"CHILD: ${childPerPerson.ToString("N0")} x "
                                    + childCount : $", CHILD: ${childPerPerson.ToString("N0")} x " + childCount;
                            }
                            if (infantCount != 0)
                            {
                                perPersonCalc += perPersonCalc.Equals("") ? $"INFANT: ${infantPerPerson.ToString("N0")} x "
                                    + infantCount : $", INFANT: ${infantPerPerson.ToString("N0")} x " + infantCount;
                            }

                            perPersonCalc += ")";
                        }

                        row.PerPersonCalc = perPersonCalc;

                        rows.Add(row);
                    }

                    model.IADFRows = rows;

                }


                int i = 1;
                foreach (var m in fam)
                {
                    model.Members.Add($"{i}. {m.FamilyHead.Package.Company.CompanyCode}-{m.FamilyHead.IndId} {m.FamilyHead.FullName}");
                    i++;
                }

                var packageNotes = _dbContext.PackagePaymentNotes
                                    .Include(e => e.Package)
                                    .Include(e => e.PaymentNotes)
                                    .Where(e => e.PackageId == _p.PackageId);

                model.Notes = packageNotes.Select(e => e.PaymentNotes.Note).ToList();

                var extrasGrouped = extras
                        .GroupBy(e => e.ExtraFacilityDescriptionId);

                var indFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                    .Include(e => e.AdditionalFacilities)
                                    .ThenInclude(e => e.IndividiualFacilityDescription)
                                    .Include(e => e.FamilyHead)
                                    .ThenInclude(e => e.Package)
                                    .ThenInclude(e => e.Company)
                                    .Where(e => memberIds.Contains(e.FamilyHeadId))
                                    .GroupBy(e => e.AdditionalFacilities.IndividiualFacilityDescriptionId);

                List<ADFDetails> eIADFDetails = new();
                List<ADFDetails> iADFDetails = new();

                int s = 1;
                foreach (var e in extrasGrouped)
                {
                    string _extraFacMembers = "";
                    ADFDetails _d = new();
                    foreach (var obj in e)
                    {
                        _d.Sr = s;
                        _d.DescriptionId = (int)obj.ExtraFacilityDescriptionId;
                        _d.Description = obj.ExtraFacilityDescription?.Description;
                        _extraFacMembers += _extraFacMembers.Equals("") ? $"{obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}"
                            : $", {obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}";
                        _d.Ids = _extraFacMembers;
                    }
                    s++;
                    eIADFDetails.Add(_d);
                }

                int t = 1;
                foreach (var e in indFacilities)
                {
                    string _indFacMembers = "";
                    ADFDetails _d = new();
                    foreach (var obj in e)
                    {
                        _d.Sr = t;
                        _d.DescriptionId = obj.AdditionalFacilitiesId;
                        _d.Description = obj.AdditionalFacilities.IndividiualFacilityDescription.Description;
                        _indFacMembers += _indFacMembers.Equals("") ? $"{obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}"
                            : $", {obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}";
                        _d.Ids = _indFacMembers;
                    }
                    t++;
                    iADFDetails.Add(_d);
                }

                model.EIADFDetails = eIADFDetails;
                model.IADFDetails = iADFDetails;
                model.Discount = "$" + discount.ToString("N0");
                float total = 0;
                if (iadfRooms.Count == 0)
                {
                    total = (basicPackagePriceWithIadfOverall) + extrasTotal + ticketTotal + ticketDiffTotal;
                }
                else
                {
                    total = basicPackagePriceWithIadfOverall + extrasTotal + ticketTotal + ticketDiffTotal;
                }
                model.Total = "$" + total.ToString("N0");

                float netReceivable = total - discount;
                model.NetReceivable = "$" + netReceivable.ToString("N0");

                var invoicesCleared = _dbContext.InvoicesWithoutTrash
                            .Where(e => e.FamilyHeadId == id && e.InvoiceStatus == InvoiceStatus.Cleared);

                var invoicesInTransit = _dbContext.InvoicesWithoutTrash
                                        .Where(e => e.FamilyHeadId == id && e.InvoiceStatus == InvoiceStatus.ClearingInTransit);

                model.ShowTransitRows = invoicesInTransit.Count() != 0;

                float paymentReceived = 0, paymentInTransit = 0;

                foreach (var inv in invoicesCleared)
                {
                    float converted = 0;
                    if (inv.Currency == Currencies.PKR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.SAR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.USD)
                    {
                        converted = inv.AmountRecived;
                    }
                    //else if (inv.Currency == Currencies.AED)
                    //{
                    //    converted = inv.AmountRecived / inv.USDROE;
                    //}
                    paymentReceived += converted;
                }
                foreach (var inv in invoicesInTransit)
                {
                    float converted = 0;
                    if (inv.Currency == Currencies.PKR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.SAR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.USD)
                    {
                        converted = inv.AmountRecived;
                    }
                    //else if (inv.Currency == Currencies.AED)
                    //{
                    //    converted = inv.AmountRecived / inv.USDROE;
                    //}
                    paymentInTransit += converted;
                }

                float pkrReceived = _dbContext.InvoicesWithoutTrash
                                    .Where(e => e.FamilyHeadId == id && e.Currency == Currencies.PKR && e.InvoiceStatus == InvoiceStatus.Cleared)
                                    .Sum(e => e.AmountRecived);

                float pkrInTrasit = _dbContext.InvoicesWithoutTrash
                                    .Where(e => e.FamilyHeadId == id && e.Currency == Currencies.PKR && e.InvoiceStatus == InvoiceStatus.ClearingInTransit)
                                    .Sum(e => e.AmountRecived);

                model.AirTicketPaxString = $"(${_p.FlightTicketPriceUsd.ToString("N0")} x {fam.Count()})";

                model.PaymentReceivedUSD = "$" + paymentReceived.ToString("N0");
                model.PaymentReceivedPKR = "PKR: " + pkrReceived.ToString("N0");
                model.TotalInTransitPKR = "PKR: " + pkrInTrasit.ToString("N0");
                model.BalanceReceivable = "$" + (netReceivable - paymentReceived).ToString("N0");
                model.TotalInTransit = "$" + paymentInTransit.ToString("N0");
                model.FinalBalance = "$" + (netReceivable - paymentReceived - paymentInTransit).ToString("N0");

                var bankAccounts = _dbContext.CompanyBankAccountsWithoutTrash
                            .Include(e => e.Company)
                            .Where(e => e.CompanyId == _.Package.CompanyId)
                            .ToList();

                model.BankDetails = bankAccounts;

                Company company = _.Package.Company;

                var companyAddress = _dbContext.CompanyAddresses
                                           .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
                var companyEmail = _dbContext.CompanyEmails
                                           .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
                var companyContact = _dbContext.CompanyContacts
                                            .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);

                string addressString = "";

                if (companyAddress is not null)
                {
                    addressString = $"{companyAddress.Address}, {companyAddress.ZipCode}, {companyAddress.City}, {companyAddress.Country}";
                }

                model.CompanyAddress = addressString;
                model.CompanyEmail = companyEmail?.CompanyEmail;
                model.CompanyContact = companyContact?.CompanyContact;
                model.CompanySite = company.Website;

                int rIter = 1;
                foreach (var r in remarks)
                {
                    model.Remarks.Add(new MemberRemarks
                    {
                        Sr = rIter,
                        Remarks = r.Remarks,
                        IndivdualId = $"{r.CompanyCode}-{r.IndId}"
                    });
                }
                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "No family head selected.";
                return RedirectToAction("Client", "MiniStatement");
            }
        }

        public IActionResult NewInvoiceV2(int id)
        {
            ViewBag.Id = id;

            if (id > 0)
            {

                FamilyHead _ = _dbContext.FamilyHeads
                .Include(e => e.SPO)
                .Include(e => e.Reference)
                .FirstOrDefault(e => e.FamilyHeadId == id);



                int familyPackageId = _.PackageId;
                bool isTentative = _.Tentative;

                List<int> memberIds = _dbContext.FamilyMembers
                                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                                .Select(e => e.FamilyHeadId)
                                .ToList();

                var remarks = _dbContext.FamilyHeadRemarks
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => memberIds.Contains(e.FamilyHeadId))
                            .Select(e => new
                            {
                                e.Remarks,
                                e.FamilyHeadId,
                                e.FamilyHead.IndId,
                                e.FamilyHead.Package.Company.CompanyCode
                            })
                            .ToList();

                var fam = _dbContext.FamilyMembers
                        .Include(e => e.FamilyHead)
                        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed 
                                && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
                        .ToList();

                Package _p = _dbContext.Packages
                            .Include(e => e.Company)
                            .FirstOrDefault(e => e.PackageId == _.PackageId);

                CompanyBankAccount bankAccount = _dbContext.CompanyBankAccountsWithoutTrash
                      .FirstOrDefault(e => e.AccountCurrency == _.Currencies && e.CompanyId == _p.CompanyId);


                string FamilyHeadAddress = _dbContext.FamilyHeadAddresses
                                .Where(e => e.FamilyHeadId == id)
                                .Select(e => e.Address)
                                .FirstOrDefault();

                string FamilyHeadContact = _dbContext.FamilyHeadContacts
                                            .Where(e => e.FamilyHeadId == id)
                                            .Select(e => e.Contact)
                                            .FirstOrDefault();

                string cId =
                                $"{_.Package.Company.CompanyCode}.{_.IndId}" +
                                $".{_.ExtractedFamilyNumber}.({fam.Count()})" +
                                $".{_.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                        .Include(e => e.Group)
                        .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && !e.Group.Trashed);

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }

                NewInvoice invoice = _dbContext.NewInvoicesWithoutTrash
                                        .OrderByDescending(e => e.ProformaNumber)
                                        .FirstOrDefault(e => e.FamilyHeadId == id);

                float ticketTotal = 0;
                float ticketDiffTotal = 0;
                float discount = 0;
                float packagePriceTotal = 0;

                float infantPackageUsd = 0, childPackageUsd = 0, adultPackageUsd = 0;

                foreach (var f in fam)
                {
                    if (f.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                    {
                        ticketTotal += _p.FlightTicketPriceUsd;
                    }
                    if (f.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                    {
                        ticketDiffTotal += f.FamilyHead.FlightTicketDifferencesUsd ?? 0;
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

                var extras = _dbContext.FamilyHeadExtras
                        .Include(e => e.ExtraFacilityDescription)
                        .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Package)
                        .ThenInclude(e => e.Company)
                        .Where(e => memberIds.Contains(e.FamilyHeadId)
                        && e.ExtraFacilityDescriptionId != null);

                var distinctExtras = extras.Select(e => new
                {
                    e.FamilyHead.Package.Company.CompanyCode,
                    e.FamilyHead.IndId,
                }).Distinct().ToList();

                float extrasTotal = extras
                        .Sum(e => e.PriceUsd) ?? 0;

                string extraMemberString = "";
                foreach (var e in distinctExtras)
                {
                    extraMemberString += extraMemberString.Equals("") ? $"{e.CompanyCode}-{e.IndId}"
                    : $", {e.CompanyCode}-{e.IndId}";
                }

                ProformaViewModel model = new ProformaViewModel()
                {
                    Year = _dbContext.HajjSeasons.Find(int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Season,
                    BaseCurrency = _.Currencies.ToString(),
                    BankAccount = bankAccount,
                    PackageAmount = $"{_.Currencies.ToString()} {(PackagePriceManager.GetTotalBasePrice(_dbContext, id)
                                                                + PackagePriceManager.GetTotalTrainPrice(_dbContext, id)
                                                                + PackagePriceManager.GetTotalRoomPrice(_dbContext, id)).ToString("N2")}",
                    FlightTicketType = _.FlightTicketType,
                    PackageDetails = $"{_p.Name}-{_p.Days} DAYS-MEDINAH {_p.MedinahArrival} HAJJ",
                    MegaReference = _.Reference?.ReferenceName,
                    FCID = _.FcId,
                    CompanyLogo = _p?.Company?.Logo,
                    CompanyName = _p?.Company?.CompanyName,
                    GuestName = _?.FullName,
                    InvoiceToCustomer = _?.FullName,
                    InvoiceToCompany = _p?.Company?.CompanyName,
                    Category = _?.ApplicantType.ToString(),
                    CNIC = _?.CNIC,
                    Contact = FamilyHeadContact,
                    Address = FamilyHeadAddress,
                    Date = _.CreatedAt.ToString("dd/MMM/yyyy"),
                    //Date = (_.ProformaDate ?? DateTime.Now).ToString("dd/MMM/yyyy"),
                    InvoiceNumber = "INV-" + invoice?.ProformaNumber,
                    Passport = _?.PassportNumber,
                    SPO = _?.SPO.Name,
                    Pax = fam.Count(),
                    Purpose = "Travel Arrangements",
                    CID = cId,
                    AirTicketUsd = $"{_.Currencies.ToString()} {PackagePriceManager.GetTotalFlightPrice(_dbContext, id).ToString("N2")}",
                    AirTicketDifferenceUsd = "$" + ticketDiffTotal.ToString("N0"),
                    EIADFPriceUsd = $"{_.Currencies.ToString()} {PackagePriceManager.GetTotalAddonsPrice(_dbContext, id).ToString("N2")}",
                    EIADFDiscount = $"{_.Currencies.ToString()} {PackagePriceManager.GetTotalAddonsDiscount(_dbContext, id).ToString("N2")}",
                    EIADFMembers = extraMemberString,
                };

                var iadfRooms = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .ThenInclude(e => e.Room)
                            .Include(e => e.AdditionalFacilities)
                            .ThenInclude(e => e.IndividiualFacilityDescription)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => memberIds.Contains(e.FamilyHeadId))
                            .GroupBy(e => e.AdditionalFacilities.RoomId)
                            .ToList();

                List<IADFRow> rows = new();

                //float iadfPriceTotal = 0;
                float basicPackagePriceWithIadfOverall = 0f;

                if (iadfRooms.Count == 0)
                {
                    IADFRow row = new();
                    row.RoomType = "SHARING";
                    row.PriceAmount = packagePriceTotal;
                    row.PriceString = (packagePriceTotal).ToString("N0");
                    row.PAX = fam.Count;

                    float perPerson = packagePriceTotal / fam.Count;

                    string perPersonCalc = "";

                    int adultCount = fam.Count(e => e.FamilyHead.Age >= 12);
                    int childCount = fam.Count(e => e.FamilyHead.Age < 12 && e.FamilyHead.Age >= 2);
                    int infantCount = fam.Count(e => e.FamilyHead.Age < 2);

                    if (adultCount != 0 || childCount != 0 || infantCount != 0)
                    {
                        perPersonCalc += "(";

                        if (adultCount != 0)
                        {
                            perPersonCalc += $"ADULT: ${adultPackageUsd.ToString("N0")} x {adultCount}";
                        }
                        if (childCount != 0)
                        {
                            perPersonCalc += perPersonCalc.Equals("") ? $"CHILD: ${childPackageUsd.ToString("N0")} x " + childCount : $", CHILD: ${childPackageUsd.ToString("N0")} x " + childCount;
                        }
                        if (infantCount != 0)
                        {
                            perPersonCalc += perPersonCalc.Equals("") ? $"INFANT: ${infantPackageUsd.ToString("N0")} x " + infantCount : $", INFANT: ${infantPackageUsd.ToString("N0")} x " + infantCount;
                        }

                        perPersonCalc += ")";
                    }

                    row.PerPersonCalc = perPersonCalc;

                    model.IADFRows.Add(row);
                }
                else
                {
                    foreach (var r in iadfRooms)
                    {
                        IADFRow row = new();
                        row.RoomType = r.ElementAtOrDefault(0)?.AdditionalFacilities?.Room?.RoomName?.ToUpper();

                        var distinctR = r.Select(e => new
                        {
                            e.FamilyHeadId,
                            e.FamilyHead.Package.Company.CompanyCode,
                            e.FamilyHead.IndId,
                            e.FamilyHead.Age,
                        }).Distinct().ToList();

                        string roomString = "";
                        //float iadfPrice = 0;
                        float adultIadfPrice = 0, childIadfPrice = 0, infantIadfPrice = 0;
                        float adultPackagePrice = 0, childPackagePrice = 0, infantPackagePrice = 0;

                        foreach (var obj in distinctR)
                        {
                            roomString += roomString.Equals("") ? $"{obj.CompanyCode}-{obj.IndId}" : $", {obj.CompanyCode}-{obj.IndId}";
                            if (obj.Age < 2)
                            {
                                //iadfPrice += _p.InfantUsd;
                                infantPackagePrice += _p.InfantUsd;
                            }
                            else if (obj.Age < 12)
                            {
                                //iadfPrice += _p.ChildUsd;
                                childPackagePrice += _p.ChildUsd;
                            }
                            else
                            {
                                //iadfPrice += _p.AdultUsd;
                                adultPackagePrice += _p.AdultUsd;
                            }
                        }

                        row.RoomMembers = roomString;

                        foreach (var obj in r)
                        {
                            if (obj.FamilyHead.Age < 2)
                            {
                                infantIadfPrice += obj.AdditionalFacilities.InfantUsd ?? 0;
                            }
                            else if (obj.FamilyHead.Age < 12)
                            {
                                childIadfPrice += obj.AdditionalFacilities.ChildUsd ?? 0;
                            }
                            else
                            {
                                adultIadfPrice += obj.AdditionalFacilities.AdultUsd ?? 0;
                            }
                        }

                        float basicPackageWithIadf = adultIadfPrice + childIadfPrice + infantIadfPrice;

                        basicPackagePriceWithIadfOverall += basicPackageWithIadf;

                        //iadfPriceTotal += iadfPrice;

                        row.PriceString = "$" + basicPackageWithIadf.ToString("N0");
                        row.PriceAmount = basicPackageWithIadf;
                        row.PAX = r.ElementAtOrDefault(0)?.AdditionalFacilities?.Room?.PAX ?? 1;
                        //float perPerson = basicPackageWithIadf / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;

                        string perPersonCalc = "";

                        int adultCount = distinctR.Count(e => e.Age >= 12);
                        int childCount = distinctR.Count(e => e.Age < 12 && e.Age >= 2);
                        int infantCount = distinctR.Count(e => e.Age < 2);
                        //float adultPerPerson = ((adultPackagePrice + adultIadfPrice)) / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;
                        //float childPerPerson = (childPackagePrice + childIadfPrice) / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;
                        //float infantPerPerson = (infantPackagePrice + infantIadfPrice) / r.ElementAtOrDefault(0).AdditionalFacilities.Room.PAX;

                        float adultPerPerson = adultIadfPrice / adultCount;
                        float childPerPerson = childIadfPrice / childCount;
                        float infantPerPerson = infantIadfPrice / infantCount;


                        if (adultCount != 0 || childCount != 0 || infantCount != 0)
                        {
                            perPersonCalc += "(";

                            if (adultCount != 0)
                            {
                                perPersonCalc += $"ADULT: ${adultPerPerson.ToString("N0")} x {adultCount}";
                            }
                            if (childCount != 0)
                            {
                                perPersonCalc += perPersonCalc.Equals("") ? $"CHILD: ${childPerPerson.ToString("N0")} x "
                                    + childCount : $", CHILD: ${childPerPerson.ToString("N0")} x " + childCount;
                            }
                            if (infantCount != 0)
                            {
                                perPersonCalc += perPersonCalc.Equals("") ? $"INFANT: ${infantPerPerson.ToString("N0")} x "
                                    + infantCount : $", INFANT: ${infantPerPerson.ToString("N0")} x " + infantCount;
                            }

                            perPersonCalc += ")";
                        }

                        row.PerPersonCalc = perPersonCalc;

                        rows.Add(row);
                    }

                    model.IADFRows = rows;

                }


                int i = 1;
                foreach (var m in fam)
                {
                    model.Members.Add($"{i}. {m.FamilyHead.Package.Company.CompanyCode}-{m.FamilyHead.IndId} {m.FamilyHead.FullName}");
                    i++;
                }

                var packageNotes = _dbContext.PackagePaymentNotes
                                    .Include(e => e.Package)
                                    .Include(e => e.PaymentNotes)
                                    .Where(e => e.PackageId == _p.PackageId);

                model.Notes = packageNotes.Select(e => e.PaymentNotes.Note).ToList();

                var extrasGrouped = extras
                        .GroupBy(e => e.ExtraFacilityDescriptionId);

                var indFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                    .Include(e => e.AdditionalFacilities)
                                    .ThenInclude(e => e.IndividiualFacilityDescription)
                                    .Include(e => e.FamilyHead)
                                    .ThenInclude(e => e.Package)
                                    .ThenInclude(e => e.Company)
                                    .Where(e => memberIds.Contains(e.FamilyHeadId))
                                    .GroupBy(e => e.AdditionalFacilities.IndividiualFacilityDescriptionId);

                List<ADFDetails> eIADFDetails = new();
                List<ADFDetails> iADFDetails = new();

                int s = 1;
                foreach (var e in extrasGrouped)
                {
                    string _extraFacMembers = "";
                    ADFDetails _d = new();
                    foreach (var obj in e)
                    {
                        _d.Sr = s;
                        _d.DescriptionId = (int)obj.ExtraFacilityDescriptionId;
                        _d.Description = obj.ExtraFacilityDescription?.Description;
                        _extraFacMembers += _extraFacMembers.Equals("") ? $"{obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}"
                            : $", {obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}";
                        _d.Ids = _extraFacMembers;
                    }
                    s++;
                    eIADFDetails.Add(_d);
                }

                int t = 1;
                foreach (var e in indFacilities)
                {
                    string _indFacMembers = "";
                    ADFDetails _d = new();
                    foreach (var obj in e)
                    {
                        _d.Sr = t;
                        _d.DescriptionId = obj.AdditionalFacilitiesId;
                        _d.Description = obj.AdditionalFacilities.IndividiualFacilityDescription.Description;
                        _indFacMembers += _indFacMembers.Equals("") ? $"{obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}"
                            : $", {obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}";
                        _d.Ids = _indFacMembers;
                    }
                    t++;
                    iADFDetails.Add(_d);
                }

                model.EIADFDetails = eIADFDetails;
                model.IADFDetails = iADFDetails;
                model.Discount = $"{_.Currencies.ToString()} {PackagePriceManager.GetTotalDiscount(_dbContext, id).ToString("N2")}";
                float total = 0;
                if (iadfRooms.Count == 0)
                {
                    total = packagePriceTotal + basicPackagePriceWithIadfOverall + extrasTotal + ticketTotal + ticketDiffTotal;
                }
                else
                {
                    total = packagePriceTotal + basicPackagePriceWithIadfOverall + extrasTotal + ticketTotal + ticketDiffTotal;
                }
                model.Total = $"{_.Currencies.ToString()} {(PackagePriceManager.GetOverallTotalPrice(_dbContext, id) + PackagePriceManager.GetTotalDiscount(_dbContext, id)).ToString("N2")}";

                float netReceivable = total - discount;
                model.NetReceivable = $"{_.Currencies.ToString()} {PackagePriceManager.GetOverallTotalPrice(_dbContext, id).ToString("N2")}";

                var invoicesCleared = _dbContext.InvoicesWithoutTrash
                            .Where(e => e.FamilyHeadId == id && e.InvoiceStatus == InvoiceStatus.Cleared);

                var invoicesInTransit = _dbContext.InvoicesWithoutTrash
                                        .Where(e => e.FamilyHeadId == id && e.InvoiceStatus == InvoiceStatus.ClearingInTransit);

                model.ShowTransitRows = invoicesInTransit.Count() != 0;

                float paymentReceived = 0, paymentInTransit = 0;

                foreach (var inv in invoicesCleared)
                {
                    float converted = 0;
                    if (inv.Currency == Currencies.PKR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.SAR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.USD)
                    {
                        converted = inv.AmountRecived;
                    }
                    //else if (inv.Currency == Currencies.AED)
                    //{
                    //    converted = inv.AmountRecived / inv.USDROE;
                    //}
                    paymentReceived += converted;
                }
                foreach (var inv in invoicesInTransit)
                {
                    float converted = 0;
                    if (inv.Currency == Currencies.PKR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.SAR)
                    {
                        converted = inv.AmountRecived / inv.USDROE;
                    }
                    else if (inv.Currency == Currencies.USD)
                    {
                        converted = inv.AmountRecived;
                    }
                    //else if (inv.Currency == Currencies.AED)
                    //{
                    //    converted = inv.AmountRecived / inv.USDROE;
                    //}
                    paymentInTransit += converted;
                }

                float pkrReceived = _dbContext.InvoicesWithoutTrash
                                    .Where(e => e.FamilyHeadId == id && e.Currency == Currencies.PKR && e.InvoiceStatus == InvoiceStatus.Cleared)
                                    .Sum(e => e.AmountRecived);

                float pkrInTrasit = _dbContext.InvoicesWithoutTrash
                                    .Where(e => e.FamilyHeadId == id && e.Currency == Currencies.PKR && e.InvoiceStatus == InvoiceStatus.ClearingInTransit)
                                    .Sum(e => e.AmountRecived);

                model.AirTicketPaxString = $"(${_p.FlightTicketPriceUsd.ToString("N0")} x {fam.Count()})";

                model.PaymentReceivedUSD = "$" + paymentReceived.ToString("N0");
                model.PaymentReceivedPKR = "PKR: " + pkrReceived.ToString("N0");
                model.TotalInTransitPKR = "PKR: " + pkrInTrasit.ToString("N0");
                model.BalanceReceivable = "$" + (netReceivable - paymentReceived).ToString("N0");
                model.TotalInTransit = "$" + paymentInTransit.ToString("N0");
                model.FinalBalance = "$" + (netReceivable - paymentReceived - paymentInTransit).ToString("N0");

                var bankAccounts = _dbContext.CompanyBankAccountsWithoutTrash
                            .Include(e => e.Company)
                            .Include(e => e.Bank)
                            .Where(e => e.CompanyId == _.Package.CompanyId
                                    && e.Bank.BankId == e.BankId)
                            .ToList();

                model.BankDetails = bankAccounts;

                Company company = _.Package.Company;

                var companyAddress = _dbContext.CompanyAddresses
                                           .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
                var companyEmail = _dbContext.CompanyEmails
                                           .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
                var companyContact = _dbContext.CompanyContacts
                                            .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);

                string addressString = "";

                if (companyAddress is not null)
                {
                    addressString = $"{companyAddress.Address}, {companyAddress.ZipCode}, {companyAddress.City}, {companyAddress.Country}";
                }

                model.CompanyAddress = addressString;
                model.CompanyEmail = companyEmail?.CompanyEmail;
                model.CompanyContact = companyContact?.CompanyContact;
                model.CompanySite = company.Website;

                int rIter = 1;
                foreach (var r in remarks)
                {
                    model.Remarks.Add(new MemberRemarks
                    {
                        Sr = rIter,
                        Remarks = r.Remarks,
                        IndivdualId = $"{r.CompanyCode}-{r.IndId}"
                    });
                }

                if(familyPackageId > 0)
                {
                    Package package = _dbContext.Packages
                    .Include(e => e.TravelRoute)
                    .Include(e => e.Company)
                    .Include(e => e.Room)
                    .Include(e => e.hajjSeasons)
                    .FirstOrDefault(e => e.PackageId == familyPackageId);

                    ViewBag.Routes = _dbContext.TravelRouteStops
                                    .Include(e => e.Vehicle)
                                    .Include(e => e.Route)
                                    .Where(e => e.TravelRouteId == package.TravelRouteId)
                                    .OrderBy(e => e.CheckIn);

                    var categoryFacilities = _dbContext.PackageCategoryFacilities
                        .Include(e => e.IndividualFacilityDescription)
                        .ThenInclude(e => e.Place)
                        .Where(e => e.PackageId == familyPackageId);

                    string facilityDescriptions = string.Join("<br/>", categoryFacilities
                        .Select(e => $"{e.IndividualFacilityDescription.Description} - {e.IndividualFacilityDescription.Place.PlaceName}"));

                    ViewBag.Rooms = facilityDescriptions;

                    ViewBag.AssignedAccommodations = _dbContext.PackageAccomodtions
                        .Include(e => e.Place)
                        .Include(e => e.Accomodation)
                        .Include(e => e.AccomodationCategory)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.Hotel)
                        .Where(e => e.PackageId == familyPackageId);

                    var flights = _dbContext.PackageFlights
                                        .Include(e => e.Airline)
                                        .OrderBy(e => e.DepartureDate)
                                        .Where(e => e.PackageId == familyPackageId).ToList();

                    ViewBag.Flights = flights;

                    var trains = _dbContext.PackageTrains
                                        .Include(e => e.Railway)
                                        .OrderBy(e => e.DepartureDate)
                                        .Where(e => e.PackageId == familyPackageId).ToList();

                    ViewBag.Trains = trains;

                    ViewBag.Terms = _dbContext.PackageTermsConditions
                                        .Where(e => e.PackageId == familyPackageId);


                    ViewBag.ItineraryInfo = _dbContext.PackageItinerary
                                        .Where(e => e.PackageId == familyPackageId);

                    ViewBag.AssignedGifts = _dbContext.PackageGifts.Include(e => e.Gift).Where(e => e.PackageId == familyPackageId);

                    ViewBag.Training = _dbContext.PackageTrainings
                        .Include(e => e.Training)
                        .Where(e => e.PackageId == familyPackageId);

                    float amount = 0;
                    amount += package.AdultSar;
                    amount += trains.Sum(e => e.TicketAmount) ?? 0;
                    amount += flights.Sum(e => e.TicketAmount) ?? 0;
                    //amount += package.IndividualFacilityDescription?.Price ?? 0f;
                    amount += categoryFacilities.Sum(e => e.IndividualFacilityDescription.Price);

                    ViewBag.PackageAmount = amount.ToString("N0");

                    model.Packages = package;
                }

                


                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "No family head selected.";
                return RedirectToAction("Client", "MiniStatement");
            }
        }

        public IActionResult NewMiniStatement(int id)
        {
            ViewBag.Id = id;

            FamilyHead _ = _dbContext.FamilyHeads
              .Include(e => e.SPO)
              .Include(e => e.Reference)
              .FirstOrDefault(e => e.FamilyHeadId == id);

            var fam = _dbContext.FamilyMembers
                       .Include(e => e.FamilyHead)
                       .Where(e => e.HeadId == id && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
                       .Select(e => e.FamilyHead.FullName)
                       .ToList();


            Package _p = _dbContext.Packages
                        .Include(e => e.Company)
                        .FirstOrDefault(e => e.PackageId == _.PackageId);

            Company company = _.Package.Company;



            var companyAddress = _dbContext.CompanyAddresses
                                       .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
            var companyEmail = _dbContext.CompanyEmails
                                       .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
            var companyContact = _dbContext.CompanyContacts
                                        .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);


            string FamilyHeadAddress = _dbContext.FamilyHeadAddresses
                            .Where(e => e.FamilyHeadId == id)
                            .Select(e => e.Address)
                            .FirstOrDefault();

            string FamilyHeadContact = _dbContext.FamilyHeadContacts
                                        .Where(e => e.FamilyHeadId == id)
                                        .Select(e => e.Contact)
                                        .FirstOrDefault();

            string addressString = "";

            if (companyAddress is not null)
            {
                addressString = $"{companyAddress.Address}, {companyAddress.ZipCode}, {companyAddress.City}, {companyAddress.Country}";
            }

            var receipts = _dbContext.NewInvoicesWithoutTrash
                .Include(e => e.CompanyBankAccount)
                .Where(e => e.ReceiptStatus == ReceiptStatus.Cleared && e.FamilyHeadId == id)
                .ToList();

            var receivedAmount = receipts
                    .Select(e => AmountConverter.GetConvertedAmount(e.AmountRecieved, _.Currencies ?? Currencies.SAR, e.Currencies, e.ROE))
                    .Sum();

            var totalPackageAmount = PackagePriceManager.GetOverallTotalPrice(_dbContext, id);

            var refundAmount = (from npv in _dbContext.NewPaymentVouchers
                                join np in _dbContext.NewPayments on npv.NewPaymentVoucherId equals np.NewPaymentVoucherId
                                join a in _dbContext.Account on np.AccountsId equals a.AccountsId
                                join fh in _dbContext.FamilyHeads on a.AccountsId equals fh.ReceivableAccountsId
                                where fh.FamilyHeadId == id && !npv.Trashed
                                select npv).ToList(); 


            var refundAmountTotal = refundAmount
                    .Select(e => e.TotalDebit) 
                    .Sum();

            var jvAmountList = _dbContext.JVs
                .Include(e => e.Debit)
                .Include(e => e.Credit)
                .Where(e => (e.DebitId == id || e.CreditId == id) && !e.Trashed)
                .ToList();

            float? jvAdjustment = 0;
            foreach (var jv in jvAmountList)
            {
                if (jv.CreditId == id)
                {
                    jvAdjustment -= jv.TotalCredit;
                }
                else if (jv.DebitId == id)
                {
                    jvAdjustment += jv.TotalDebit;
                }
            }

            var remainingAmount = (totalPackageAmount - receivedAmount) + refundAmountTotal + jvAdjustment;
            //var remainingAmount = totalPackageAmount - receivedAmount;

            ViewBag.CompanyAddress = addressString;
            ViewBag.CompanyEmail = companyEmail?.CompanyEmail;
            ViewBag.CompanyContact = companyContact?.CompanyContact;
            ViewBag.CompanySite = company.Website;

            return View(new NewMiniStatementViewModel()
            {
                FamilyHeadId = id,
                Address = FamilyHeadAddress,
                Contact = FamilyHeadContact,
                BaseCurrency = _.Currencies.ToString(),
                Currencies = _.Currencies ?? Currencies.SAR,
                Category = _.ApplicantType.ToString(),
                CNIC = _.CNIC,
                CompanyLogo = company.Logo,
                CompanyName = company.CompanyName,
                Date = DateTime.Now.ToString("dd/MMM/yyyy"),
                GuestName = _.FullName,
                MegaReference = _.Reference?.ReferenceName,
                Members = fam,
                Passport = _.PassportNumber,
                PAX = fam.Count.ToString(),
                Purpose = "Hajj - " + _dbContext.HajjSeasons.FirstOrDefault(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Season,
                SPO = _.SPO?.Name,
                Receipts = receipts,
                TotalPackage = totalPackageAmount.ToString("N2"),
                ReceivedAmount = receivedAmount.ToString("N2"),
                RefundAmount = refundAmountTotal?.ToString("N2"),
                Payments = refundAmount,
                JV = jvAmountList,
                RemainingAmount = remainingAmount?.ToString("N2"),
                PrintedBy = _http.HttpContext.Session.GetString("Username"),
                HBNo = _.FcId,
                PackageDetails = _.Package.Name
            });
        }

    }
}

namespace TMG.Models.AccountModels
{
    public record NewMiniStatementViewModel
    {
        public int FamilyHeadId { get; set; }
        public string PrintedBy { get; set; }
        public string TotalPackage { get; set; }
        public string RemainingAmount { get; set; }
        public string RefundAmount { get; set; }
        public string ReceivedAmount { get; set; }
        public Currencies Currencies { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string GuestName { get; set; }
        public string HBNo { get; set; }
        public string PackageDetails { get; set; }
        public string Contact { get; set; }
        public string CNIC { get; set; }
        public string Passport { get; set; }
        public string Address { get; set; }
        public string Date { get; set; }
        public string SPO { get; set; }
        public string Purpose { get; set; }
        public string Category { get; set; }
        public string MegaReference { get; set; }
        public string PAX { get; set; }
        public string BaseCurrency { get; set; }
        public List<string> Members { get; set; } = new();
        public List<NewInvoice> Receipts { get; set; } = new();
        public List<NewPaymentVoucher> Payments { get; set; } = new();
        public List<JV> JV { get; set; } = new();
    }

    public record NewMiniStatementViewEntryModel 
    {

    }

    public record ProformaViewModel
    {
        public string Year { get; set; }
        public string BaseCurrency { get; set; }
        public CompanyBankAccount BankAccount { get; set; }
        public FlightTicketType FlightTicketType { get; set; }
        public string MegaReference { get; set; }
        public string FCID { get; set; }
        public string CID { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string InvoiceToCustomer { get; set; }
        public string InvoiceToCompany { get; set; }
        public string GuestName { get; set; }
        public string Contact { get; set; }
        public string CNIC { get; set; }
        public string Passport { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string InvoiceNumber { get; set; }
        public string Date { get; set; }
        public string SPO { get; set; }
        public string Purpose { get; set; }
        public string Category { get; set; }
        public int Pax { get; set; }
        public string PackageAmount { get; set; }
        public string EIADFPriceUsd { get; set; }
        public string EIADFDiscount { get; set; }
        public string EIADFPriceString { get; set; }
        public string EIADFMembers { get; set; }
        public List<string> Members { get; set; } = new();
        public List<string> Notes { get; set; } = new();
        public List<ADFDetails> EIADFDetails { get; set; } = new();
        public List<ADFDetails> IADFDetails { get; set; } = new();
        public List<IADFRow> IADFRows { get; set; } = new();
        public string AirTicketUsd { get; set; }
        public string AirTicketPaxString { get; set; }
        public string AirTicketDifferenceUsd { get; set; }
        public string Discount { get; set; }
        public string Total { get; set; }
        public string NetReceivable { get; set; }
        public string PaymentReceivedUSD { get; set; }
        public string PaymentReceivedPKR { get; set; }
        public string TotalInTransitPKR { get; set; }
        public string BalanceReceivable { get; set; }
        public string TotalInTransit { get; set; }
        public string FinalBalance { get; set; }
        public string PackageDetails { get; set; }
        public bool ShowTransitRows { get; set; }
        public List<CompanyBankAccount> BankDetails { get; set; } = new();
        public string CompanyAddress { get; set; }
        public string CompanyContact { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanySite { get; set; }
        public List<MemberRemarks> Remarks { get; set; } = new();
        public Package Packages { get; set; }

    }

    public record MemberRemarks
    {
        public int Sr { get; set; }
        public string Remarks { get; set; }
        public string IndivdualId { get; set; }
    }

    public record ADFDetails
    {
        public int Sr { get; set; }
        public string Description { get; set; }
        public int DescriptionId { get; set; }
        public string Ids { get; set; }
    }

    public record IADFRow
    {
        public string RoomType { get; set; }
        public string RoomMembers { get; set; }
        public string Price { get; set; }
        public string PriceString { get; set; }
        public float PriceAmount { get; set; }
        public int PAX { get; set; }
        public string PerPersonCalc { get; set; }
    }

    public record BankDetails
    {
        public string Currency { get; set; }
        public string AccountTitle { get; set; }
        public string AccountNumber { get; set; }
        public string IBAN { get; set; }
        public string BranchCode { get; set; }
        public string Swift { get; set; }
        public string NTN { get; set; }
        public string BankAddress { get; set; }
    }
}

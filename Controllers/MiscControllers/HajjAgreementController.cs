using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AirlineModels;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using TMG.Models.FamilyHeadReportModels;


namespace TMG.Models.MiscModels
{
    public record HajjAgreementIndexModel
    {
        public int HajjAgreementId { get; set; }
        public string Title { get; set; }
        public int SNo { get; set; }
        public string Year { get; set; }
        public List<string> Packages { get; set; } = new();
    }
}

namespace TMG.Controllers.MiscControllers
{
    public class HajjAgreementController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public HajjAgreementController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _http = http;
            _host = host;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.HajjAgreements.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashGift(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateGift(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditGift(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreGift(_dbContext);

            List<HajjAgreementIndexModel> model = new();
            var hajjAgreements = _dbContext.HajjAgreementsWithoutTrash.ToList();
            var hajjAgreementPackages = _dbContext.HajjAgreementPackages.Select(e => new
            {
                e.Package.Name,
                e.HajjAgreementId
            }).ToList();

            for (int i = 0; i < hajjAgreements.Count; i++)
            {
                HajjAgreement _ = hajjAgreements.ElementAt(i);
                HajjAgreementIndexModel subModel = new()
                {
                    SNo = i + 1,
                    Title = _.Title,
                    Year = _.ForTheYear,
                    HajjAgreementId = _.HajjAgreementId
                };

                var currPackages = hajjAgreementPackages.Where(e => e.HajjAgreementId == _.HajjAgreementId).ToList();
                for (int j = 0; j < currPackages.Count; j++)
                {
                    subModel.Packages.Add(currPackages.ElementAt(j).Name);
                }

                model.Add(subModel);
            }

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Packages = _dbContext.UserCompanies
                   .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                   .Join(
                       _dbContext.CompaniesWithoutTrash,
                       uc => uc.CompanyId,
                       c => c.CompanyId,
                       (uc, c) => new { uc.UserId, c.CompanyId }
                   )
                   .Join(
                       _dbContext.PackagesWithoutTrash,
                       uc => uc.CompanyId,
                       p => p.CompanyId,
                       (uc, p) => new
                       {
                           uc.UserId,
                           uc.CompanyId,
                           p.Name,
                           p.PackageId,
                           p.Code,
                           p.Trashed
                       }
                   ).Where(e => !e.Trashed);

            if (_dbContext.HajjAgreements.Count() > 0)
            {
                HajjAgreement agreement = new HajjAgreement();
                agreement.SharedAgreement = _dbContext.HajjAgreements.FirstOrDefault().SharedAgreement;
                agreement.SpecificAgreement = _dbContext.HajjAgreements.FirstOrDefault().SpecificAgreement;

                return View(agreement);
            }


            return View();
        }

        [HttpPost]
        public IActionResult Create(HajjAgreement request)
        {
            // return Ok(request);

            ViewBag.Packages = _dbContext.UserCompanies
                   .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                   .Join(
                       _dbContext.CompaniesWithoutTrash,
                       uc => uc.CompanyId,
                       c => c.CompanyId,
                       (uc, c) => new { uc.UserId, c.CompanyId }
                   )
                   .Join(
                       _dbContext.PackagesWithoutTrash,
                       uc => uc.CompanyId,
                       p => p.CompanyId,
                       (uc, p) => new
                       {
                           uc.UserId,
                           uc.CompanyId,
                           p.Name,
                           p.PackageId,
                           p.Code,
                           p.Trashed
                       }
                   ).Where(e => !e.Trashed);

            if (!ModelState.IsValid) return View(request);

            request.SpecificAgreement = request.SpecificAgreement ?? "";
            _dbContext.HajjAgreements.Add(request);
            _dbContext.SaveChanges();

            for (int i = 0; i < request.PackageIds.Count; i++)
            {
                if (request.PackagesSelected[i] == false) continue;
                _dbContext.HajjAgreementPackages.Add(new HajjAgreementPackages
                {
                    PackageId = request.PackageIds[i],
                    HajjAgreementId = request.HajjAgreementId
                });
            }
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Packages = _dbContext.UserCompanies
                   .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                   .Join(
                       _dbContext.CompaniesWithoutTrash,
                       uc => uc.CompanyId,
                       c => c.CompanyId,
                       (uc, c) => new { uc.UserId, c.CompanyId }
                   )
                   .Join(
                       _dbContext.PackagesWithoutTrash,
                       uc => uc.CompanyId,
                       p => p.CompanyId,
                       (uc, p) => new
                       {
                           uc.UserId,
                           uc.CompanyId,
                           p.Name,
                           p.PackageId,
                           p.Code,
                           p.Trashed
                       }
                   ).Where(e => !e.Trashed);

            ViewBag.AgreementPackages = _dbContext.HajjAgreementPackages
                .Where(a => a.HajjAgreementId == id)
                .ToList();


            HajjAgreement agreement = _dbContext.HajjAgreements.Find(id);

            return View(agreement);
        }

        [HttpPost]
        public IActionResult Edit(HajjAgreement request)
        {
            ViewBag.Packages = _dbContext.UserCompanies
                   .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                   .Join(
                       _dbContext.CompaniesWithoutTrash,
                       uc => uc.CompanyId,
                       c => c.CompanyId,
                       (uc, c) => new { uc.UserId, c.CompanyId }
                   )
                   .Join(
                       _dbContext.PackagesWithoutTrash,
                       uc => uc.CompanyId,
                       p => p.CompanyId,
                       (uc, p) => new
                       {
                           uc.UserId,
                           uc.CompanyId,
                           p.Name,
                           p.PackageId,
                           p.Code,
                           p.Trashed
                       }
                   ).Where(e => !e.Trashed);

            ViewBag.AgreementPackages = _dbContext.HajjAgreementPackages;

            if (!ModelState.IsValid) return View(request);

            var agreements = _dbContext.HajjAgreements.ToList();

            HajjAgreement agreement = _dbContext.HajjAgreements.Find(request.HajjAgreementId);


            agreement.Title= request.Title;
            agreement.UpdatedAt = DateTime.Now;
            agreement.ForTheYear = request.ForTheYear;
            agreement.SpecificAgreement = request.SpecificAgreement ?? "";

            _dbContext.SaveChanges();

            _dbContext.HajjAgreementPackages.Where(e => e.HajjAgreementId == request.HajjAgreementId).ExecuteDelete();
            for (int i = 0; i < request.PackageIds.Count; i++)
            {
                if (request.PackagesSelected[i] == false) continue;

                _dbContext.HajjAgreementPackages.Add(new HajjAgreementPackages
                {
                    PackageId = request.PackageIds[i],
                    HajjAgreementId = request.HajjAgreementId
                });
            }
            _dbContext.SaveChanges();

            var agreementsAfterUpdate = _dbContext.HajjAgreementPackages.Where(e => e.HajjAgreementId == request.HajjAgreementId).ToList();
            for (int i = 0; i < agreementsAfterUpdate.Count; i++)
            {
                var currAgreement = agreementsAfterUpdate.ElementAt(i);
                HajjAgreement _ = _dbContext.HajjAgreements.FirstOrDefault(e => e.HajjAgreementId == currAgreement.HajjAgreementId);
                _.SharedAgreement = request.SharedAgreement;
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ViewTrash()
        {
            var trashItems = _dbContext.HajjAgreements
                                         .Where(e => e.Trashed)
                                         .Select(cl => new HajjAgreementIndexModel
                                         {
                                             HajjAgreementId = cl.HajjAgreementId,
                                             Title = cl.Title,
                                             Year = cl.ForTheYear,
                                             Packages = _dbContext.HajjAgreementPackages
                                                            .Where(clp => clp.HajjAgreementId == cl.HajjAgreementId)
                                                            .Select(clp => clp.Package.Name)
                                                            .ToList()
                                         })
                                         .ToList();

            return View(trashItems);
        }

        public IActionResult Trash(int id)
        {
            HajjAgreement agreement = _dbContext.HajjAgreements.Find(id)!;
            agreement.Trashed = true;

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            HajjAgreement agreement = _dbContext.HajjAgreements.Find(id)!;
            agreement.Trashed = false;

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        //public IActionResult Display(int id)
        //{
        //    // string decryptedPassport = ApplicationManager.Decrypt(p);

        //    FamilyHead fh = _dbContext.FamilyHeads
        //        .Include(e => e.Honorifics)
        //        .FirstOrDefault(e => e.FamilyHeadId == id);

        //    int headOfTheFamilyId = _dbContext.FamilyMembers
        //        .FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId).HeadId;

        //    HajjAgreementPackages agreementPackages = _dbContext.HajjAgreementPackages
        //    .FirstOrDefault(e => e.PackageId == fh.PackageId);

        //    HajjAgreement agreement = _dbContext.HajjAgreements
        //    .FirstOrDefault(e => e.HajjAgreementId == agreementPackages.HajjAgreementId);

        //    Package package = _dbContext.Packages.Find(fh.PackageId);

        //    Company packageCompany = _dbContext.Companies.Find(package.CompanyId);

        //    string headPassport = _dbContext.FamilyHeads
        //                        .FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId)?.PassportNumber;

        //    string headEncryptedPassport = ApplicationManager.Encrypt(headPassport);

        //    FamilyHeadAddress fha = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);
        //    FamilyHeadContact fhc = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);
        //    FamilyHeadEmail fhe = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);

        //    var fhf = _dbContext.FamilyHeadAdditionalFacilities
        //                .Include(e => e.AdditionalFacilities)
        //                .ThenInclude(e => e.IndividiualFacilityDescription)
        //                .Where(e => e.FamilyHeadId == fh.FamilyHeadId)
        //                .ToList();

        //    var memberInGroup = _dbContext.GroupMembers
        //            .Include(e => e.Group)
        //            .FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);

        //    int familyPax = _dbContext.FamilyMembers.Where(e => e.HeadId == headOfTheFamilyId).Count();

        //    string cId =
        //                $"{packageCompany.CompanyCode}" +
        //                $".{fh.IndId}" +
        //                $".{headOfTheFamilyId}({familyPax})" +
        //                $".{package.Name}";

        //    string groupNumber = "";
        //    if (memberInGroup != null)
        //    {
        //        cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
        //        groupNumber = memberInGroup?.Group?.Code ?? "-";
        //    }

        //    float facPricePkr = 0f, facPriceSar = 0f, facPriceUsd = 0f;
        //    float packagePricePersonPkr = 0f, packagePricePersonSar = 0f, packagePricePersonUsd = 0f;
        //    float totalPricePersonPkr = 0f, totalPricePersonSar = 0f, totalPricePersonUsd = 0f;

        //    if (DateTimeManager.CalculateAge(fh.DateOfBirth) < 2)
        //    {
        //        packagePricePersonPkr = package.InfantPkr;
        //        packagePricePersonSar = package.InfantSar;
        //        packagePricePersonUsd = package.InfantUsd;

        //        for (int i = 0; i < fhf.Count; i++)
        //        {
        //            var currFac = fhf.ElementAt(i);
        //            facPricePkr += currFac.AdditionalFacilities.InfantPkr ?? 0f;
        //            facPriceSar += currFac.AdditionalFacilities.InfantSar ?? 0f;
        //            facPriceUsd += currFac.AdditionalFacilities.InfantUsd ?? 0f;
        //        }
        //    }
        //    else if (DateTimeManager.CalculateAge(fh.DateOfBirth) < 12)
        //    {
        //        packagePricePersonPkr = package.ChildPkr;
        //        packagePricePersonSar = package.ChildSar;
        //        packagePricePersonUsd = package.ChildUsd;

        //        for (int i = 0; i < fhf.Count; i++)
        //        {
        //            var currFac = fhf.ElementAt(i);
        //            facPricePkr += currFac.AdditionalFacilities.ChildPkr ?? 0f;
        //            facPriceSar += currFac.AdditionalFacilities.ChildSar ?? 0f;
        //            facPriceUsd += currFac.AdditionalFacilities.ChildUsd ?? 0f;
        //        }
        //    }
        //    else if (DateTimeManager.CalculateAge(fh.DateOfBirth) >= 12)
        //    {
        //        packagePricePersonPkr = package.AdultPkr;
        //        packagePricePersonSar = package.AdultSar;
        //        packagePricePersonUsd = package.AdultUsd;

        //        for (int i = 0; i < fhf.Count; i++)
        //        {
        //            var currFac = fhf.ElementAt(i);
        //            facPricePkr += currFac.AdditionalFacilities.AdultPkr ?? 0f;
        //            facPriceSar += currFac.AdditionalFacilities.AdultSar ?? 0f;
        //            facPriceUsd += currFac.AdditionalFacilities.AdultUsd ?? 0f;
        //        }
        //    }

        //    string makkahFac = "", medinaFac = "", aziziaFac = "", mashairFac = "";

        //    var result = _dbContext.FamilyHeadAdditionalFacilities
        //        .Where(faf => faf.FamilyHeadId == fh.FamilyHeadId)
        //        .Join(
        //            _dbContext.AdditionalFacilities,
        //            faf => faf.AdditionalFacilitiesId,
        //            af => af.AdditionalFacilitiesId,
        //            (faf, af) => new { faf, af }
        //        )
        //        .Join(
        //            _dbContext.IndividualFacilityDescriptions,
        //            af => af.af.IndividiualFacilityDescriptionId,
        //            ifd => ifd.IndividualFacilityDescriptionId,
        //            (af, ifd) => new
        //            {
        //                af.af.PlaceId,
        //                af.af.Accomodation.AccomodationName,
        //                ifd.Description,
        //                DistanceInMeters = _dbContext.PackageAccomodtions
        //                    .Where(pa => pa.PackageId == package.PackageId && pa.PlaceId == af.af.PlaceId)
        //                    .Select(pa => pa.DistanceInMeters)
        //                    .FirstOrDefault(),
        //                Category = _dbContext.PackageAccomodtions
        //                    .Join(
        //                        _dbContext.AccomodationCategories,
        //                        pa => pa.AccomodationCategoryId,
        //                        ac => ac.AccomodationCategoryId,
        //                        (pa, ac) => new { pa, ac }
        //                    )
        //                    .Where(joinResult => joinResult.pa.PackageId == af.af.PackageId && joinResult.pa.PlaceId == af.af.PlaceId)
        //                    .Select(joinResult => joinResult.ac.Category)
        //                    .FirstOrDefault()
        //            }
        //        )
        //        .ToList();


        //    for (int i = 0; i < result.Count; i++)
        //    {
        //        var currFac = result.ElementAt(i);

        //        if (currFac.PlaceId == 1)
        //        {
        //            makkahFac += makkahFac.Equals("") ?
        //            $"{currFac.Description} in {currFac.AccomodationName}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB"
        //            : $", {currFac.Description} in {currFac.AccomodationName}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB";
        //        }
        //        else if (currFac.PlaceId == 2)
        //        {
        //            medinaFac += medinaFac.Equals("") ?
        //            $"{currFac.Description} in {currFac.AccomodationName}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB"
        //            : $", {currFac.Description} in {currFac.AccomodationName}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB";
        //        }
        //        else if (currFac.PlaceId == 4)
        //        {
        //            aziziaFac += aziziaFac.Equals("") ?
        //            $"{currFac.Description} in {currFac.AccomodationName}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB"
        //            : $", {currFac.Description} in {currFac.AccomodationName}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB";
        //        }
        //    }

        //    totalPricePersonPkr = facPricePkr + packagePricePersonPkr - fh.PkrDiscountAmount;
        //    totalPricePersonSar = facPriceSar + packagePricePersonSar - fh.SarDiscountAmount;
        //    totalPricePersonUsd = facPriceUsd + packagePricePersonUsd - fh.UsdDiscountAmount;

        //    float EIADFPkr = 0f, EIADFSar = 0f, EIADFUsd = 0f;

        //    var extras = _dbContext.FamilyHeadExtras
        //                    .Where(e => e.FamilyHeadId == fh.FamilyHeadId)
        //                    .ToList();

        //    string flightString = "";
        //    float flightTicketPricePkr = 0f, flightTicketPriceSar = 0f, flightTicketPriceUsd = 0f;
        //    if (fh.FlightTicketType == FlightTicketType.Included)
        //    {
        //        flightString = "Total Per Person (With Air Ticket)";
        //        flightTicketPricePkr = package.FlightTicketPricePkr ?? 0f;
        //        flightTicketPriceSar = package.FlightTicketPriceSar ?? 0f;
        //        flightTicketPriceUsd = package.FlightTicketPriceUsd ?? 0f;
        //    }
        //    else if (fh.FlightTicketType == FlightTicketType.Excluded)
        //    {
        //        flightString = "Total Per Person (Without Air Ticket)";
        //    }
        //    else if (fh.FlightTicketType == FlightTicketType.IncludedWithDifferences)
        //    {
        //        flightString = "Per Person Total (Incl. Airfare w/ Difference)";

        //        flightTicketPricePkr = (package.FlightTicketPricePkr ?? 0f) + fh.FlightTicketDifferencesPkr ?? 0f;
        //        flightTicketPriceSar = (package.FlightTicketPriceSar ?? 0f) + fh.FlightTicketDifferencesSar ?? 0f;
        //        flightTicketPriceUsd = (package.FlightTicketPriceUsd ?? 0f) + fh.FlightTicketDifferencesUsd ?? 0f;
        //    }

        //    totalPricePersonPkr += flightTicketPricePkr;
        //    totalPricePersonSar += flightTicketPriceSar;
        //    totalPricePersonUsd += flightTicketPriceUsd;

        //    EIADFPkr = extras.Sum(e => e.Price) ?? 0f;
        //    EIADFSar = extras.Sum(e => e.PriceSar) ?? 0f;
        //    EIADFUsd = extras.Sum(e => e.PriceUsd) ?? 0f;


        //    PackageAccomodations mashairAcc = _dbContext.PackageAccomodtions
        //                                        .FirstOrDefault(e => e.PackageId == package.PackageId && e.PlaceId == 3);

        //    MunazzimCompanies munazzim = _dbContext.MunazzimCompanies
        //                                    .Include(e => e.Munazzim)
        //                                    .FirstOrDefault(e => e.CompanyId == packageCompany.CompanyId);

        //    int headId = _dbContext.FamilyMembers
        //                        .First(e => e.FamilyHeadId == fh.FamilyHeadId).HeadId;

        //    int accountId = (int)_dbContext.FamilyHeads.Find(headId).AccountId;

        //    var voucher = _dbContext.PaymentVouchers.FirstOrDefault(e => e.AccountId == accountId);

        //    float receivablePkr = voucher.Amount;
        //    float receivableSar = voucher.Amount / voucher.SARROE;
        //    float receivableUsd = voucher.Amount / voucher.USDROE;

        //    UserHajjAgreement model = new()
        //    {
        //        CompanyName = packageCompany.CompanyName,
        //        ClusterName = munazzim?.Munazzim.Name,
        //        FamilyHeadId = fh.FamilyHeadId,
        //        SharedAgreement = agreement.SharedAgreement,
        //        SpecificAgreement = agreement.SpecificAgreement,
        //        CompanySeal = packageCompany.Stamp,
        //        Title = fh.Honorifics.Title,
        //        Name = $"{fh.GivenName} {fh.SurName}",
        //        CID = cId,
        //        PassportNumber = fh.PassportNumber,
        //        Address = fha?.Address ?? "N/A",
        //        Contact = fhc?.Contact ?? "N/A",
        //        Email = fhe?.Email ?? "N/A",
        //        ApplicantType = fh.ApplicantType.ToString(),
        //        HajjAppNumber = fh.HajjApplicationNumber,
        //        HajjiImage = fh.MemberImage,

        //        MashairFacilities = "-",

        //        PricingPerPersonPkr = packagePricePersonPkr.ToString("N0"),
        //        PricingPerPersonSar = packagePricePersonSar.ToString("N0"),
        //        PricingPerPersonUsd = "$" + packagePricePersonUsd.ToString("N0"),

        //        DiscountPkr = fh.PkrDiscountAmount.ToString("N0"),
        //        DiscountSar = fh.SarDiscountAmount.ToString("N0"),
        //        DiscountUsd = "$" + fh.UsdDiscountAmount.ToString("N0"),

        //        IADFPricePkr = facPricePkr.ToString("N0"),
        //        IADFPriceSar = facPriceSar.ToString("N0"),
        //        IADFPriceUsd = "$" + facPriceUsd.ToString("N0"),

        //        EIADFPricePkr = EIADFPkr.ToString("N0"),
        //        EIADFPriceSar = EIADFSar.ToString("N0"),
        //        EIADFPriceUsd = "$" + EIADFUsd.ToString("N0"),

        //        TicketString = flightString,
        //        FlightTicketPricePkr = flightTicketPricePkr.ToString("N0"),
        //        FlightTicketPriceSar = flightTicketPriceSar.ToString("N0"),
        //        FlightTicketPriceUsd = "$" + flightTicketPriceUsd.ToString("N0"),

        //        TotalPerPersonPkr = totalPricePersonPkr.ToString("N0"),
        //        TotalPerPersonSar = totalPricePersonSar.ToString("N0"),
        //        TotalPerPersonUsd = "$" + totalPricePersonUsd.ToString("N0"),

        //        Package = $"{package.Name} {package.Days} Days Shifting",

        //        MakkahFacilities = makkahFac.Equals("") ? "N/A" : makkahFac,
        //        MedinaFacilities = medinaFac.Equals("") ? "N/A" : medinaFac,
        //        AziziaFacilities = aziziaFac.Equals("") ? "N/A" : aziziaFac,

        //        Airline = $"EK ({fh.FlightClass}-Transit 6 hrs (+/-), POD: {package.POD})",
        //        MedinahArrival = package.MedinahArrival + " HAJJ",
        //        AmountReceivedPkr = "0",
        //        AmountReceivedSar = "0",
        //        AmountReceivedUsd = "$" + "0",

        //        TotalWithFamilyPkr = receivablePkr.ToString("N0"),
        //        TotalWithFamilySar = receivableSar.ToString("N0"),
        //        TotalWithFamilyUsd = "$" + receivableUsd.ToString("N0"),

        //        BalanceDuePkr = totalPricePersonPkr.ToString("N0"),
        //        BalanceDueSar = totalPricePersonSar.ToString("N0"),
        //        BalanceDueUsd = "$" + totalPricePersonUsd.ToString("N0"),

        //    };

        //    if (mashairAcc is not null)
        //    {
        //        model.MashairFacilities = $"Maktab: {package.CategoryZone} in Gender Shared Tents with FB meals, {mashairAcc.Camp}";
        //    }

        //    return View(model);
        //}

        public IActionResult NewDisplay(int id)
        {
            ViewBag.Members = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id)
                .ToList();

            // string decryptedPassport = ApplicationManager.Decrypt(p);

            FamilyHead fh = _dbContext.FamilyHeads
            .Include(e => e.Honorifics)
                                .FirstOrDefault(e => e.FamilyHeadId == id);

            int headOfTheFamilyId = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id).HeadId;
            FamilyHead head = _dbContext.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == headOfTheFamilyId);

            HajjAgreementPackages agreementPackages = _dbContext.HajjAgreementPackages.FirstOrDefault(e => e.PackageId == fh.PackageId);
            HajjAgreement agreement = _dbContext.HajjAgreements.FirstOrDefault(e => e.HajjAgreementId == (agreementPackages != null ?  agreementPackages.HajjAgreementId : 0));

            Package package = _dbContext.Packages.Find(fh.PackageId);

            Company packageCompany = _dbContext.Companies.Find(package.CompanyId);

            string headPassport = _dbContext.FamilyHeads
                                .FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId)?.PassportNumber;

            string headEncryptedPassport = ApplicationManager.Encrypt(headPassport);

            FamilyHeadAddress fha = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);
            FamilyHeadContact fhc = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);
            FamilyHeadEmail fhe = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);

            var fhf = _dbContext.FamilyHeadAdditionalFacilities
                        .Include(e => e.AdditionalFacilities)
                        .ThenInclude(e => e.IndividiualFacilityDescription)
                        .Where(e => e.FamilyHeadId == fh.FamilyHeadId)
                        .ToList();

            var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == fh.FamilyHeadId);

            int familyPax = _dbContext.FamilyMembers.Where(e => e.HeadId == headOfTheFamilyId).Count();

            string cId =
                        $"{packageCompany.CompanyCode}" +
                        $".{fh.IndId}" +
                        $".{head.ExtractedFamilyNumber}({familyPax})" +
                        $".{package.Name}";

            if (fh.IsHead)
            {
                cId =
                    $"{packageCompany.CompanyCode}" +
                        $".{fh.IndId}" +
                        $".{head.ExtractedFamilyNumber}({familyPax})" +
                        $".{package.Name}";
            }
            else
            {
                cId =
                    $"{packageCompany.CompanyCode}" +
                        $".{fh.IndId}" +
                        $".{head.ExtractedFamilyNumber}({familyPax})" +
                        $".{package.Name}";
            }

            string groupNumber = "";
            if (memberInGroup != null)
            {
                cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                groupNumber = memberInGroup?.Group?.Code ?? "-";
            }

            float facPricePkr = 0f, facPriceSar = 0f, facPriceUsd = 0f;
            float packagePricePersonPkr = 0f, packagePricePersonSar = 0f, packagePricePersonUsd = 0f;
            float totalPricePersonPkr = 0f, totalPricePersonSar = 0f, totalPricePersonUsd = 0f;


            var iadf = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .ThenInclude(e => e.Room)
                            .Where(e => e.FamilyHeadId == fh.FamilyHeadId)
                            .ToList();

            string iadfString = "";
            if (iadf is not null && iadf.Count > 0)
            {
                iadfString = iadf.ElementAt(0).AdditionalFacilities.Room.RoomName;
            }

            float iadfPriceUsd = 0;
            if (DateTimeManager.CalculateAge(fh.DateOfBirth) < 2)
            {
                iadfPriceUsd = iadf.Sum(e => e.AdditionalFacilities.InfantUsd) ?? 0;

                packagePricePersonPkr = package.InfantPkr;
                packagePricePersonSar = package.InfantSar;
                packagePricePersonUsd = package.InfantUsd;

                for (int i = 0; i < fhf.Count; i++)
                {
                    var currFac = fhf.ElementAt(i);
                    facPricePkr += currFac.AdditionalFacilities.InfantPkr ?? 0f;
                    facPriceSar += currFac.AdditionalFacilities.InfantSar ?? 0f;
                    facPriceUsd += currFac.AdditionalFacilities.InfantUsd ?? 0f;
                }
            }
            else if (DateTimeManager.CalculateAge(fh.DateOfBirth) < 12)
            {
                iadfPriceUsd = iadf.Sum(e => e.AdditionalFacilities.ChildUsd) ?? 0;

                packagePricePersonPkr = package.ChildPkr;
                packagePricePersonSar = package.ChildSar;
                packagePricePersonUsd = package.ChildUsd;

                for (int i = 0; i < fhf.Count; i++)
                {
                    var currFac = fhf.ElementAt(i);
                    facPricePkr += currFac.AdditionalFacilities.ChildPkr ?? 0f;
                    facPriceSar += currFac.AdditionalFacilities.ChildSar ?? 0f;
                    facPriceUsd += currFac.AdditionalFacilities.ChildUsd ?? 0f;
                }
            }
            else if (DateTimeManager.CalculateAge(fh.DateOfBirth) >= 12)
            {
                iadfPriceUsd = iadf.Sum(e => e.AdditionalFacilities.AdultUsd) ?? 0;

                packagePricePersonPkr = package.AdultPkr;
                packagePricePersonSar = package.AdultSar;
                packagePricePersonUsd = package.AdultUsd;

                for (int i = 0; i < fhf.Count; i++)
                {
                    var currFac = fhf.ElementAt(i);
                    facPricePkr += currFac.AdditionalFacilities.AdultPkr ?? 0f;
                    facPriceSar += currFac.AdditionalFacilities.AdultSar ?? 0f;
                    facPriceUsd += currFac.AdditionalFacilities.AdultUsd ?? 0f;
                }
            }

            string makkahFac = "", medinaFac = "", aziziaFac = "", mashairFac = "";

            var result = _dbContext.FamilyHeadAdditionalFacilities
                .Where(faf => faf.FamilyHeadId == fh.FamilyHeadId)
                .Join(
                    _dbContext.AdditionalFacilities,
                    faf => faf.AdditionalFacilitiesId,
                    af => af.AdditionalFacilitiesId,
                    (faf, af) => new { faf, af }
                )
                .Join(
                    _dbContext.IndividualFacilityDescriptions,
                    af => af.af.IndividiualFacilityDescriptionId,
                    ifd => ifd.IndividualFacilityDescriptionId,
                    (af, ifd) => new
                    {
                        af.af.PlaceId,
                        af.af.Accomodation.AccomodationName,
                        ifd.Description,
                        DistanceInMeters = _dbContext.PackageAccomodtions
                            .Where(pa => pa.PackageId == package.PackageId && pa.PlaceId == af.af.PlaceId)
                            .Select(pa => pa.DistanceInMeters)
                            .FirstOrDefault(),
                        Category = _dbContext.PackageAccomodtions
                            .Join(
                                _dbContext.AccomodationCategories,
                                pa => pa.AccomodationCategoryId,
                                ac => ac.AccomodationCategoryId,
                                (pa, ac) => new { pa, ac }
                            )
                            .Where(joinResult => joinResult.pa.PackageId == af.af.PackageId && joinResult.pa.PlaceId == af.af.PlaceId)
                            .Select(joinResult => joinResult.ac.Category)
                            .FirstOrDefault()
                    }
                )
                .ToList();


            for (int i = 0; i < result.Count; i++)
            {
                var currFac = result.ElementAt(i);

                if (currFac.PlaceId == 1)
                {
                    makkahFac += makkahFac.Equals("") ?
                    $"{currFac.Description}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB"
                    : $", {currFac.Description}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB";
                }
                else if (currFac.PlaceId == 2)
                {
                    medinaFac += medinaFac.Equals("") ?
                    $"{currFac.Description}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB"
                    : $", {currFac.Description}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with HB";
                }
                else if (currFac.PlaceId == 4)
                {
                    aziziaFac += aziziaFac.Equals("") ?
                    $"{currFac.Description}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with FB"
                    : $", {currFac.Description}, {currFac.Category}, {currFac.DistanceInMeters ?? "0"}m with FB";
                }
            }

            totalPricePersonPkr = facPricePkr + packagePricePersonPkr;
            totalPricePersonSar = facPriceSar + packagePricePersonSar;
            totalPricePersonUsd = facPriceUsd + packagePricePersonUsd;

            float EIADFPkr = 0f, EIADFSar = 0f, EIADFUsd = 0f;

            var extras = _dbContext.FamilyHeadExtras
                            .Where(e => e.FamilyHeadId == fh.FamilyHeadId)
                            .ToList();

            string flightString = "";
            float flightTicketPricePkr = 0f, flightTicketPriceSar = 0f, flightTicketPriceUsd = 0f;
            if (fh.FlightTicketType == FlightTicketType.Included)
            {
                flightString = "Total Per Person (With Air Ticket)";
                flightTicketPricePkr = package.FlightTicketPricePkr;
                flightTicketPriceSar = package.FlightTicketPriceSar;
                flightTicketPriceUsd = package.FlightTicketPriceUsd;
            }
            else if (fh.FlightTicketType == FlightTicketType.Excluded)
            {
                flightString = "Total Per Person (Without Air Ticket)";
            }
            else if (fh.FlightTicketType == FlightTicketType.IncludedWithDifferences)
            {
                flightString = "Per Person Total (Incl. Airfare w/ Difference)";

                flightTicketPricePkr = (package.FlightTicketPricePkr) + fh.FlightTicketDifferencesPkr ?? 0f;
                flightTicketPriceSar = (package.FlightTicketPriceSar) + fh.FlightTicketDifferencesSar ?? 0f;
                flightTicketPriceUsd = (package.FlightTicketPriceUsd) + fh.FlightTicketDifferencesUsd ?? 0f;
            }

            totalPricePersonPkr += flightTicketPricePkr;
            totalPricePersonSar += flightTicketPriceSar;
            totalPricePersonUsd += flightTicketPriceUsd;

            EIADFPkr = extras.Sum(e => e.PricePkr) ?? 0f;
            EIADFSar = extras.Sum(e => e.PriceSar) ?? 0f;
            EIADFUsd = extras.Sum(e => e.PriceUsd) ?? 0f;


            PackageAccomodations mashairAcc = _dbContext.PackageAccomodtions
                                                .FirstOrDefault(e => e.PackageId == package.PackageId && e.PlaceId == 3);

            MunazzimCompanies munazzim = _dbContext.MunazzimCompanies
                                            .Include(e => e.Munazzim)
                                            .FirstOrDefault(e => e.CompanyId == packageCompany.CompanyId);

            int headId = _dbContext.FamilyMembers
                                .First(e => e.FamilyHeadId == fh.FamilyHeadId).HeadId;

            //int accountId = (int)_dbContext.FamilyHeads.Find(headId).AccountId;

            //var voucher = _dbContext.PaymentVouchers.FirstOrDefault(e => e.AccountId == accountId);

            //float receivablePkr = voucher.Amount;
            //float receivableSar = voucher.Amount / voucher.SARROE;
            //float receivableUsd = voucher.Amount / voucher.USDROE;

            var invoicesCleared = _dbContext.Invoices
                                        .Where(e => e.FamilyHeadId == id && e.InvoiceStatus == InvoiceStatus.Cleared)
                                        .ToList();

            float paymentReceivedUsd = 0;
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
                paymentReceivedUsd += converted;
            }

            string extraFacString = "";
            var extraFacilities = _dbContext.FamilyHeadExtras
                                        .Include(e => e.ExtraFacilityDescription)
                                        .Where(e => e.FamilyHeadId == fh.FamilyHeadId)
                                        .ToList();

            float eiadf = extraFacilities.Sum(e => e.PriceUsd) ?? 0;
            totalPricePersonUsd += eiadf;
            foreach (var ef in extraFacilities)
            {
                extraFacString += extraFacString.Equals("") ?
                    ef.ExtraFacilityDescription?.Description
                    : $", {ef.ExtraFacilityDescription?.Description}";
            }

            var moraNumber = _dbContext.CompanyLicenses
                                .Include(e => e.CompanyLicenseType)
                                .FirstOrDefault(e => e.CompanyId == packageCompany.CompanyId
                                                && e.CompanyLicenseTypeId == 2);

            var clusterMoraNumber = _dbContext.MunazzimCompanies
                                    .Include(e => e.Munazzim)
                                    .FirstOrDefault(e => e.CompanyId == packageCompany.CompanyId);

            UserHajjAgreement model = new()
            {
                ExtraFacilitiesPriceUsd = "$" + eiadf.ToString("N0"),
                IADFString = iadf is null ? "Package" : iadfString,
                IADFPriceWithPackage = "$" + (packagePricePersonUsd + iadfPriceUsd).ToString("N0"),
                ExtraFacilities = extraFacString,
                Category = fh.ApplicantType.ToString(),
                CompanyName = packageCompany.CompanyName + $" ({moraNumber?.CompanyLicenseType?.LicenseName}: {moraNumber?.CompanyLicense})",
                ClusterName = munazzim?.Munazzim.Name + $" (MORA HP# K-{clusterMoraNumber?.Munazzim?.ClusterNumber})",
                FamilyHeadId = fh.FamilyHeadId,
                SharedAgreement = agreement?.SharedAgreement,
                SpecificAgreement = agreement?.SpecificAgreement,
                CompanySeal = packageCompany.Stamp,
                Title = fh.Honorifics.Title,
                Name = $"{fh.GivenName} {fh.SurName}",
                CID = cId,
                PassportNumber = fh.PassportNumber,
                Address = fha?.Address ?? "N/A",
                Contact = fhc?.Contact ?? "N/A",
                Email = fhe?.Email ?? "N/A",
                ApplicantType = fh.ApplicantType.ToString(),
                HajjAppNumber = fh.HajjApplicationNumber,
                HajjiImage = fh.MemberImage,


                MashairFacilities = "-",

                PricingPerPersonPkr = packagePricePersonPkr.ToString("N0"),
                PricingPerPersonSar = packagePricePersonSar.ToString("N0"),
                PricingPerPersonUsd = "$" + packagePricePersonUsd.ToString("N0"),

                DiscountPkr = fh.PkrDiscountAmount.ToString("N0"),
                DiscountSar = fh.SarDiscountAmount.ToString("N0"),
                DiscountUsd = "$" + fh.UsdDiscountAmount.ToString("N0"),

                IADFPricePkr = facPricePkr.ToString("N0"),
                IADFPriceSar = facPriceSar.ToString("N0"),
                IADFPriceUsd = "$" + facPriceUsd.ToString("N0"),

                EIADFPricePkr = EIADFPkr.ToString("N0"),
                EIADFPriceSar = EIADFSar.ToString("N0"),
                EIADFPriceUsd = "$" + EIADFUsd.ToString("N0"),

                TicketString = flightString,
                FlightTicketPricePkr = flightTicketPricePkr.ToString("N0"),
                FlightTicketPriceSar = flightTicketPriceSar.ToString("N0"),
                FlightTicketPriceUsd = "$" + flightTicketPriceUsd.ToString("N0"),

                TotalPerPersonPkr = totalPricePersonPkr.ToString("N0"),
                TotalPerPersonSar = totalPricePersonSar.ToString("N0"),
                TotalPerPersonUsd = "$" + totalPricePersonUsd.ToString("N0"),

                Package = $"{package.Name} {package.Days} Days",

                MakkahFacilities = makkahFac.Equals("") ? "N/A" : makkahFac,
                MedinaFacilities = medinaFac.Equals("") ? "N/A" : medinaFac,
                AziziaFacilities = aziziaFac.Equals("") ? "N/A" : aziziaFac,

                // Airline = $"({fh.FlightClass}-Transit 6 hrs (+/-), POD: {package.POD})",
                MedinahArrival = package.MedinahArrival + " HAJJ",
                AmountReceivedPkr = "0",
                AmountReceivedSar = "0",
                AmountReceivedUsd = "$" + paymentReceivedUsd.ToString("N0"),

                //TotalWithFamilyPkr = receivablePkr.ToString("N0"),
                //TotalWithFamilySar = receivableSar.ToString("N0"),
                //TotalWithFamilyUsd = "$" + receivableUsd.ToString("N0"),
                NetReceivable = "$" + (totalPricePersonUsd - fh.UsdDiscountAmount).ToString("N0"),

                BalanceDuePkr = totalPricePersonPkr.ToString("N0"),
                BalanceDueSar = totalPricePersonSar.ToString("N0"),
                BalanceDueUsd = "$" + totalPricePersonUsd.ToString("N0"),

            };

            if(fh.FlightTicketType != FlightTicketType.Excluded){
                model.Airline = $"({fh.FlightClass}-Transit 6 hrs (+/-), POD: {fh?.POD})";
            }
            else{
                model.Airline = "N/A";
            }

            if (mashairAcc is not null)
            {
                model.MashairFacilities = $"Maktab: {package.CategoryZone} in Gender Shared Tents with FB meals, {mashairAcc.Camp}";
            }

            return View(model);
        }

        public IActionResult AlreadyExists() => View();

        public IActionResult DisplayDataForm(int id)
        {
            ViewBag.Email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Email)
                .FirstOrDefault();

            try
            {

                ViewBag.HeadId = id;
                //ViewBag.HajjSeason = _dbContext.HajjSeasons
                //                    .FirstOrDefault(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

                ViewBag.Documents = _dbContext.RequiredDocumentsWithoutTrash;

                var membersOfFamily = _dbContext.FamilyMembers
                                        .Include(e => e.Head)
                                        .Include(e => e.FamilyHead)
                                        .ThenInclude(e => e.RelationWithMehram)
                                        .Include(e => e.FamilyHead)
                                        .ThenInclude(e => e.Fiqah)
                                        .Include(e => e.FamilyHead)
                                        .ThenInclude(e => e.Package)
                                        .ThenInclude(e => e.Company)
                                        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                                        .ToList();

                ViewBag.TermsAndCondition = _dbContext.Packages
                    .FirstOrDefault(e => e.PackageId == membersOfFamily.ElementAtOrDefault(0).FamilyHead.PackageId)
                    .TermsnCondition;

                DataFormModel superModel = new DataFormModel();
                superModel.BaseCurrency = membersOfFamily.ElementAtOrDefault(0).FamilyHead.Currencies.ToString();
                superModel.MunazzimName = _dbContext.MunazzimCompanies
                                            .Select(e => new {
                                                e.CompanyId,
                                                e.Munazzim.Name
                                            })
                                            .FirstOrDefault(e => e.CompanyId == membersOfFamily.ElementAt(0).FamilyHead.Package.CompanyId)?
                                            .Name;

                superModel.BasePackagePrice = PackagePriceManager.GetTotalBasePrice(_dbContext, id);
                superModel.FlightPrice = PackagePriceManager.GetTotalFlightPrice(_dbContext, id);
                superModel.TrainPrice = PackagePriceManager.GetTotalTrainPrice(_dbContext, id);
                superModel.RoomPrice = PackagePriceManager.GetTotalRoomPrice(_dbContext, id);
                superModel.AddonsPrice = PackagePriceManager.GetTotalAddonsPrice(_dbContext, id);
                superModel.AddonsDiscount = PackagePriceManager.GetTotalAddonsDiscount(_dbContext, id);
                superModel.TotalDiscount = PackagePriceManager.GetTotalDiscount(_dbContext, id);
                superModel.TotalPackagePrice = PackagePriceManager.GetOverallTotalPrice(_dbContext, id);

                for (int i = 0; i < membersOfFamily.Count; i++)
                {
                    FamilyMembers currMember = membersOfFamily.ElementAt(i);

                    var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                    var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

                    string cId =
                            $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
                            $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                            $".{currMember.FamilyHead.Package.Name}";

                    List<FamilyHeadDocument> familyHeadDocuments = _dbContext.FamilyHeadDocuments
                                                            .Include(e => e.RequiredDocument)
                                                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.RequiredDocument.Trashed)
                                                            .ToList();

                    FamilyHeadAddress headAddress = _dbContext
                                        .FamilyHeadAddresses
                                        .Include(e => e.Tehsil)
                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                    FamilyHeadContact preferredContact = _dbContext
                        .FamilyHeadContacts
                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                    FamilyHeadContact otherContact = _dbContext
                        .FamilyHeadContacts
                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

                    FamilyHeadNominee headNominee = _dbContext
                        .FamilyHeadNominees
                        .Include(e => e.NomineeRelation)
                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                    var trainings = _dbContext
                        .PackageTrainings
                        .Include(e => e.Training)
                        .Where(e => e.PackageId == currMember.FamilyHead.PackageId).ToList();

                    Flight flight = _dbContext.Flights
                        .FirstOrDefault(e => e.FlightId == currMember.FamilyHead.FlightId);

                    var gifts = _dbContext
                        .PackageGifts
                        .Include(e => e.Gift)
                        .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
                        .ToList();

                    var extraFacilities = _dbContext
                        .FamilyHeadExtras
                        .Include(e => e.ExtraFacilityDescription)
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .ToList();

                    string presentPostalAddress = $"{headAddress?.Address}, {headAddress?.City}, {headAddress?.ZipCode}, {headAddress?.Country}";
                    string packageNameDuration = $"{currMember?.FamilyHead.Package.Name} " +
                        $"({currMember?.FamilyHead.Package.Days})";

                    var officialAddress = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberAddressTypeId == 2);
                    var officialContact = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberContactTypeId == 2);
                    var officialEmail = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberEmailTypeId == 2);

                    string officialAddressString = officialAddress is not null ?
                                            $"{officialAddress?.Address}, {officialAddress?.City}, {officialAddress?.ZipCode}, {officialAddress?.Country}" : "";

                    string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred)?.Email;

                    UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                    DataFormModelEntry model = new DataFormModelEntry
                    {
                        Documents = familyHeadDocuments,
                        SignedDate = DateTimeManager.GetDate(userHajjAgreement?.SignedOn),
                        CID = cId,
                        ShowSignature = userHajjAgreement?.SignDataForm ?? false,
                        SignatureBase64 = currMember?.FamilyHead.SignatureBase64,
                        ApplicantType = currMember?.FamilyHead.ApplicantType.ToString(),
                        FamilyHeadImage = currMember?.FamilyHead.MemberImage ?? "",
                        HajjAppNumber = currMember?.FamilyHead.HajjApplicationNumber ?? "-",
                        EnrollmentNumber = currMember?.FamilyHead.ENumber ?? "",
                        MoraPackageNumber = currMember?.FamilyHead.Package.PackageNumber,
                        PackageNameDuration = packageNameDuration,
                        HGOName = currMember?.FamilyHead.Package.Company.CompanyName,
                        SurName = currMember?.FamilyHead.SurName,
                        GivenName = currMember?.FamilyHead.GivenName,
                        FatherHusbandName = currMember?.FamilyHead.FatherHusbandName,
                        PassportNumber = currMember?.FamilyHead.PassportNumber,
                        PassportDateOfExpiry = currMember?.FamilyHead.PassportDateOfExpiry?.ToString("dd-MMM-yyyy"),
                        DateOfBirth = currMember?.FamilyHead.DateOfBirth.ToString("dd-MMM-yyyy"),
                        CNIC = currMember?.FamilyHead.CNIC,
                        BloodGroup = currMember?.FamilyHead.BloodGroup,
                        Gender = currMember?.FamilyHead.Gender,
                        Fiqah = currMember?.FamilyHead.Fiqah.Name.ToString(),
                        IsMarried = (bool)currMember?.FamilyHead.IsMarried ? "YES" : "NO",
                        YearsPerformedHajj = currMember?.FamilyHead.YearsPerformedHajj,
                        PresentPostalAddress = presentPostalAddress,
                        TehsilCode = headAddress?.Tehsil?.Name,
                        MobileNumber = preferredContact?.Contact,
                        TelephoneNumber = otherContact?.Contact,

                        Email = email,
                        OfficialAddress = officialAddressString,
                        OfficialContact = officialContact?.Contact,
                        EmployerBusinessName = currMember.FamilyHead.EmployedAt,
                        Designation = currMember.FamilyHead.Designation,
                        OfficialEmail = officialEmail?.Email,

                        PerformedHajjInLast5Years = (bool)currMember?.FamilyHead.PerformedHajjInLastFiveYears ? "YES" : "NO",
                        WantToHajjEBadal = (bool)currMember?.FamilyHead.WantToPerformHajjEBadal ? "YES" : "NO",
                        WantToPerformHajjAsGroupWorker = (bool)currMember?.FamilyHead.WantToPerformAsGroupWorker ? "YES" : "NO",
                        NomineeName = headNominee?.Name,
                        NomineeRelation = headNominee?.NomineeRelation?.Relation,
                        NomineeCNIC = headNominee?.CNIC,
                        NomineeContact = headNominee?.Contact,
                        NomineeTelephone = "",

                        StayDuration = currMember?.FamilyHead.Package.HajjDuration,
                        SubAgaintAppNumber = currMember?.FamilyHead.SubstitutedAgainstAppNumber,

                        MedinahArrival = currMember?.FamilyHead.Package.MedinahArrival,
                        FlightType = currMember?.FamilyHead.Package.FlightType,
                        FlightClass = currMember?.FamilyHead.FlightClass,
                        TicketPeriod = currMember?.FamilyHead.Package.FlightTicketPeriod,
                        POD = currMember?.FamilyHead.POD,
                        // TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString(),

                        PackageTicketPricePkr = currMember?.FamilyHead.Package.FlightTicketPricePkr,
                        PackageTicketPriceSar = currMember?.FamilyHead.Package.FlightTicketPriceSar,
                        PackageTicketPriceUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd,

                        FlightTicketDifferencesPkr = currMember?.FamilyHead.FlightTicketDifferencesPkr,
                        FlightTicketDifferencesSar = currMember?.FamilyHead.FlightTicketDifferencesSar,
                        FlightTicketDifferencesUsd = currMember?.FamilyHead.FlightTicketDifferencesUsd,

                        IntercityTransport = currMember?.FamilyHead.Package.TransportIntercity,
                        AirportTransport = currMember?.FamilyHead.Package.AirportTransport,
                        MashairTrasport = currMember?.FamilyHead.Package.TransportWithMashair,
                        WithinCity = currMember?.FamilyHead.Package.WithInCity
                    };

                    var routes = new List<DataFormModelEntry>();

                    if (fm.FamilyHead.RouteAsPerPackage == true)
                    {
                        routes = _dbContext.TravelRouteStops
                            .Include(e => e.Route)
                            .Include(e => e.Vehicle)
                            .Where(e => e.TravelRouteId == currMember.FamilyHead.Package.TravelRouteId)
                            .OrderBy(e => e.CheckIn)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.TravelRouteStopsId,
                                TravelRoutes = e.Route.StartPlace + " to " + e.Route.Destination,
                                CheckIn = e.CheckIn,
                                CheckOut = e.CheckOut,
                                VehicleName = e.Vehicle.VehicleType
                            })
                            .ToList();
                    }
                    else
                    {
                        routes = _dbContext.FamilyHeadTransports
                            .Include(e => e.Route)
                            .Include(e => e.Vehicle)
                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                            .OrderBy(e => e.CheckIn)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.FamilyHeadTransportId,
                                TravelRoutes = e.Route.StartPlace + " to " + e.Route.Destination,
                                CheckIn = e.CheckIn,
                                //CheckOut = e.CheckOut,
                                VehicleName = e.Vehicle.VehicleType
                            })
                            .ToList();
                    }

                    if (fm.IsHead)
                    {
                        ViewBag.Routes = routes;
                    }

                    var flights = new List<DataFormModelEntry>();

                    if (fm.FamilyHead.FlightAsPerPackage == true)
                    {
                        flights = _dbContext.PackageFlights
                            .Include(e => e.Airline)
                            .Where(e => e.PackageId == currMember.FamilyHead.PackageId && e.IsPreffered == true)
                            .OrderBy(e => e.DepartureDate)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.PackageFlightsId,
                                Origin = e.Origin,
                                Destination = e.Destination,
                                Airline = e.Airline.Name,
                                ArrivalDateTime = e.ArrivalDate,
                                DepartureDateTime = e.DepartureDate,
                                DepartureTime = e.DepartureTime,
                                ArrivalTime = e.ArrivalTime,
                            })
                            .ToList();
                    }
                    else
                    {
                        flights = _dbContext.FamilyHeadFlight
                            .Include(e => e.Airline)
                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                            .OrderBy(e => e.DepartureDateTime)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.FamilyHeadFlightId,
                                Origin = e.Origin,
                                Destination = e.Destination,
                                Airline = e.Airline.Name,
                                ArrivalDateTime = e.ArrivalDateTime,
                                DepartureDateTime = e.DepartureDateTime,
                                ArrivalTime = e.ArrivalTime,
                                DepartureTime = e.DepartureTime
                            })
                            .ToList();
                    }

                    if (fm.IsHead)
                    {
                        ViewBag.Flights = flights;
                    }

                    var trains = new List<DataFormModelEntry>();

                    if (fm.FamilyHead.TrainAsPerPackage == true)
                    {
                        trains = _dbContext.PackageTrains
                            .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
                            .OrderBy(e => e.DepartureDate)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.PackageTrainsId,
                                TrainOrigin = e.Origin,
                                TrainDestination = e.Destination,
                                TrainClass = e.TrainClass.ToString(),
                                TrainArrivalDateTime = e.ArrivalDate,
                                TrainDepartureDateTime = e.DepartureDate,
                                TrainArrivalTime = e.ArrivalTime,
                                TrainDepartureTime = e.DepartureTime
                            })
                            .ToList();
                    }
                    else
                    {
                        trains = _dbContext.FamilyHeadTrain
                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                            .OrderBy(e => e.DepartureDateTime)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.FamilyHeadTrainId,
                                TrainOrigin = e.Origin,
                                TrainDestination = e.Destination,
                                TrainClass = e.TrainClass.ToString(),
                                TrainArrivalDateTime = e.ArrivalDateTime,
                                TrainDepartureDateTime = e.DepartureDateTime,
                                TrainArrivalTime = e.ArrivalTime,
                                TrainDepartureTime = e.DepartureTime,
                            })
                            .ToList();
                    }

                    if (fm.IsHead)
                    {
                        ViewBag.Trains = trains;
                    }

                    model.Remarks = _dbContext.FamilyHeadRemarks.Where(e => e.FamilyHeadId == currMember.FamilyHeadId).Select(e => e.Remarks).ToList();


                    for (int k = 0; k < extraFacilities.Count; k++)
                    {
                        FamilyHeadExtra currExtra = extraFacilities.ElementAt(k);

                        model.ExtraFacilities.Add(
                            new DataFormExtraFacility(
                                currExtra.ExtraFacilityDescription?.Description,
                                0f,
                                0f,
                                0f)
                            );
                    }


                    for (int k = 0; k < gifts.Count; k++)
                    {
                        PackageGifts currGift = gifts.ElementAt(k);
                        model.Gifts.Add(new DataFormGift(currGift.Gift.Giveaway));
                    }

                    if (currMember.FamilyHead.Gender == Gender.Male)
                    {
                        FamilyHead mehram = _dbContext.FamilyHeads.FirstOrDefault(e => e.MehramFamilyHeadId == currMember.FamilyHeadId && !e.Trashed);
                        model.AreYouMehramOfALady = mehram is not null ? "YES" : "NO";
                    }
                    else
                    {
                        FamilyHead mehram = _dbContext.FamilyHeads
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHead.MehramFamilyHeadId && !e.Trashed);
                        model.AreYouMehramOfALady = "-";
                        model.MehramAppNumber = mehram?.HajjApplicationNumber;
                        model.MehramRelationship = currMember?.FamilyHead.RelationWithMehram?.Relation;
                        model.MehramSignatureBase64 = mehram?.SignatureBase64;
                        model.MehramName = mehram?.FullName;
                    }

                    for (int j = 0; j < trainings.Count; j++)
                    {
                        PackageTrainings _t = trainings.ElementAt(j);
                        model.TrainingSessions.Add(new DataFormTrainingSession(
                            _t.Training.TrainingSession, _t.Training.ExpectedTrainingVenue,
                            _t.Training.ExpectedTrainingTime
                            ));
                    }


                    PackageAccomodations aziziaAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .Include(e => e.Hotel)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 4);

                    var makkahAccomadation = _dbContext.PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .Include(e => e.Hotel)

                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);

                    PackageAccomodations secondMakkahAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .Include(e => e.Hotel)
                        .OrderByDescending(e => e.PackageAccomodationsId)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);


                    string makkahRoomTypeDescription = null;
                    string medinahRoomTypeDescription = null;
                    string aziziaRoomTypeDescription = null;
                    string minaRoomTypeDescription = null;
                    string secondMakkahRoomTypeDescription = null;

                    if (makkahAccomadation != null)
                    {
                        if (makkahAccomadation.PackageAccomodationsId == secondMakkahAccomadation.PackageAccomodationsId)
                        {
                            secondMakkahAccomadation = null;
                        }
                    }

                    PackageAccomodations medinaAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 2);

                    PackageAccomodations minaAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 3);

                    if (aziziaAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                                    .Include(e => e.AdditionalFacilities)
                                                                    .ThenInclude(e => e.Room)
                                                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                                    && e.AdditionalFacilities.PlaceId == 4);

                        model.AziziaAccomodation = new DataFormAzizia(
                            aziziaAccomadation.Accomodation.AccomodationName,
                            aziziaAccomadation.Hotel?.HotelName ?? aziziaAccomadation.AccomodationName,
                            aziziaAccomadation.AccomodationCategory.Category,
                            aziziaAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? aziziaAccomadation.CheckIn,
                            aziziaAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? aziziaAccomadation.CheckOut,
                            aziziaAccomadation.Days,
                            aziziaAccomadation.DistanceInMeters?.ToString(),
                            aziziaAccomadation.FoodPackage?.Package,
                            //_indId is null ? aziziaAccomadation.LoadPerRoom : _indId.AdditionalFacilities.Room.PAX,
                            //_indId is null ? aziziaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                            aziziaAccomadation.Shuttle,
                            aziziaAccomadation.ReligiousLectures,
                            _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : ""

                        );

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    && e.IndividualFacilityDescription.PlaceId == aziziaAccomadation.PlaceId)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        aziziaRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.AziziaRoom = aziziaRoomTypeDescription;
                    }

                    if (makkahAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                .Include(e => e.AdditionalFacilities)
                                                .ThenInclude(e => e.Room)
                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 1);

                        model.MakkahAccomodation = new DataFormAccomodation(
                        makkahAccomadation.GroupZiarat,
                        makkahAccomadation.Days,
                        makkahAccomadation.DistanceInMeters?.ToString(),
                        makkahAccomadation.FoodPackage?.Package,
                        //_indId is null ? makkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                        makkahAccomadation.Accomodation.AccomodationName,
                        makkahAccomadation.Hotel?.HotelName ?? makkahAccomadation.AccomodationName,
                        makkahAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? makkahAccomadation.CheckIn,
                        makkahAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? makkahAccomadation.CheckOut,
                        //_indId is null ? makkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        makkahAccomadation.Sharing,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                        makkahAccomadation.AccomodationCategory.Category);

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    && e.IndividualFacilityDescription.PlaceId == makkahAccomadation.PlaceId)
                            .Include(e => e.IndividualFacilityDescription)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        makkahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.MakkahRoom = makkahRoomTypeDescription;
                    }

                    if (secondMakkahAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                .Include(e => e.AdditionalFacilities)
                                                .ThenInclude(e => e.Room)
                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 1);

                        model.SecondMakkahAccomodation = new DataFormAccomodation(
                        secondMakkahAccomadation.GroupZiarat,
                        secondMakkahAccomadation.Days,
                        secondMakkahAccomadation.DistanceInMeters?.ToString(),
                        secondMakkahAccomadation.FoodPackage?.Package,
                        //_indId is null ? secondMakkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                        secondMakkahAccomadation.Accomodation.AccomodationName,
                        secondMakkahAccomadation.Hotel?.HotelName ?? secondMakkahAccomadation.AccomodationName,
                        secondMakkahAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? secondMakkahAccomadation.CheckIn,
                        secondMakkahAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? secondMakkahAccomadation.CheckOut,
                        //_indId is null ? secondMakkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        secondMakkahAccomadation.Sharing,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                        secondMakkahAccomadation.AccomodationCategory.Category

                    );
                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    && e.IndividualFacilityDescription.PlaceId == secondMakkahAccomadation.PlaceId)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        secondMakkahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.SecondMakkah = secondMakkahRoomTypeDescription;

                    }

                    if (medinaAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                .Include(e => e.AdditionalFacilities)
                                                .ThenInclude(e => e.Room)
                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 2);

                        model.MedinaAccomodation = new DataFormAccomodation(
                        medinaAccomadation.GroupZiarat,
                        medinaAccomadation.Days,
                        medinaAccomadation.DistanceInMeters?.ToString(),
                        medinaAccomadation.FoodPackage?.Package,
                        //_indId is null ? medinaAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                        medinaAccomadation.Accomodation.AccomodationName,
                        medinaAccomadation.Hotel?.HotelName ?? medinaAccomadation.AccomodationName,
                        medinaAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? medinaAccomadation.CheckIn,
                        medinaAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? medinaAccomadation.CheckOut,
                        //_indId is null ? medinaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        medinaAccomadation.Sharing,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                        medinaAccomadation.AccomodationCategory.Category
                    );

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    && e.IndividualFacilityDescription.PlaceId == medinaAccomadation.PlaceId)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        medinahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.MedinahRoom = medinahRoomTypeDescription;

                        //  return Ok(new {
                        //         model.MedinaAccomodation
                        //     });
                    }

                    if (minaAccomadation is not null)
                    {
                        model.MinaAccomodation = new DataFormMina(
                        minaAccomadation.Package.CategoryZone,
                        minaAccomadation.FoodPackage?.Package,
                        minaAccomadation.Bedding,
                        minaAccomadation.Camp,
                        minaAccomadation.Arafat,
                        minaAccomadation.ReligiousLectures
                       );

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    && e.IndividualFacilityDescription.PlaceId == 3)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        minaRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.MinaRoom = minaRoomTypeDescription;
                    }

                    superModel.Entries.Add(model);

                }

                return View(superModel);
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    e.Message,
                });
            }

        }
        public IActionResult MedicalAffidavitDisplay(int id)
        {

            var membersOfFamily = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHeadId == id)
                .ToList();

            var vaccines = _dbContext.VaccinesWithoutTrash
                .Select(e => new
                {
                    e.VaccineName,
                    e.VaccineId,
                });

            MedicalAffidavitModel model = new();
            model.PdfPath = membersOfFamily.ElementAtOrDefault(0)?.FamilyHead?.MISPdfPath;

            model.Doctors = _dbContext.DoctorsWithoutTrash.ToList();

            for (int i = 0; i < membersOfFamily.Count; i++)
            {

                FamilyMembers currMember = membersOfFamily.ElementAt(i);
                MedicalAffidavitModelEntry entry = new();

                UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                entry.FamilyHeadId = id;
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
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId1)?.VaccineName;
                entry.Vaccine2 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId2)?.VaccineName;
                entry.Vaccine3 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId3)?.VaccineName;
                entry.Vaccine4 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId4)?.VaccineName;
                entry.Vaccine5 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId5)?.VaccineName;

                entry.DoseDate1 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate1);
                entry.DoseDate2 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate2);
                entry.DoseDate3 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate3);
                entry.DoseDate4 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate4);
                entry.DoseDate5 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate5);

                model.Entries.Add(entry);
            }
            ViewBag.HeadId = id;
            return View(model);
        }

        [HttpPost]
        public IActionResult Save(UserHajjAgreement request)
        {
            var signature = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASwAAACWCAYAAABkW7XSAAAAAXNSR0IArs4c6QAABGJJREFUeF7t1AEJAAAMAsHZv/RyPNwSyDncOQIECEQEFskpJgECBM5geQICBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAAYPlBwgQyAgYrExVghIgYLD8AAECGQGDlalKUAIEDJYfIEAgI2CwMlUJSoCAwfIDBAhkBAxWpipBCRAwWH6AAIGMgMHKVCUoAQIGyw8QIJARMFiZqgQlQMBg+QECBDICBitTlaAECBgsP0CAQEbAYGWqEpQAgQdWMQCX4yW9owAAAABJRU5ErkJggg==";
            if (request.SignatureBase64.Equals(signature))
            {
                return (RedirectToAction("NewDisplay", new { id = request.FamilyHeadId }));
            }
            FamilyHead familyHead = _dbContext.FamilyHeads
                .FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadId);

            FamilyHeadEmail email = _dbContext.FamilyHeadEmails
                                        .FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadId && e.Preferred);
            if(email != null)
            {
                email.Email = request.Email;
            }
            else
            {
                _dbContext.FamilyHeadEmails.Add(new FamilyHeadEmail
                {
                    Email = request.Email,
                    FamilyHeadId = request.FamilyHeadId,
                    MemberEmailTypeId = 1,
                    Preferred = true
                });
            }
            familyHead.SignatureBase64 = request.SignatureBase64;

            _dbContext.SaveChanges();

            HajjAgreementPackages hajjAgreement = _dbContext.HajjAgreementPackages
            .Include(e => e.HajjAgreement)
            .FirstOrDefault(e => e.PackageId == familyHead.PackageId);

            //request.SpecificAgreement = hajjAgreement.HajjAgreement.SpecificAgreement;
            //request.SharedAgreement = hajjAgreement.HajjAgreement.SharedAgreement;

            _dbContext.UserHajjAgreements.Add(request);
            _dbContext.SaveChanges();

            if (request.SignMISDataForm)
            {
                //string file = Path.Combine(_host.WebRootPath, familyHead.MISPdfPath);

                //string outputFile = Path.Combine(_host.WebRootPath, "Pdfs", "SignedMIS", familyHead.PassportNumber + "SignedMISForm.pdf");

                //familyHead.SignedMISPdfPath = @"Pdfs\SignedMIS\" + familyHead.PassportNumber + "SignedMISForm.pdf";
                //_dbContext.SaveChanges();

                //string base64Path = request.SignatureBase64;
                //float x = 125, y = 425;

                //AddImageToPdf(file, outputFile, base64Path, x, y);
            }

            //return RedirectToAction("SendHajjAgreementPdfEmail", "Email", new { id = request.FamilyHeadId });
            return RedirectToAction("AlreadyExists", "HajjAgreement");

        }

        public IActionResult ViewAgreement(int id)
        {
            var agreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == id);
            return View(agreement);
        }


        public void AddImageToPdf(string pdfFilePath, string outputFilePath, string base64Image, float x, float y)
        {
            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            using (PdfWriter pdfWriter = new PdfWriter(outputFilePath))
            using (PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter))
            {
                Document document = new Document(pdfDoc);

                PdfPage secondPage = pdfDoc.GetPage(2);
                PdfCanvas canvas = new PdfCanvas(secondPage);

                base64Image = base64Image.Replace("data:image/png;base64,", "");

                byte[] imageBytes = Convert.FromBase64String(base64Image);
                ImageData imageData = ImageDataFactory.Create(imageBytes);
                Image image = new Image(imageData);

                float scaleFactor = 0.55f;
                canvas.SaveState()
                      .ConcatMatrix(scaleFactor, 0, 0, scaleFactor, x, y)
                      .AddImageAt(imageData, 0, 0, false);

            }
        }


    }
}

namespace TMG.Models.PackageModels
{
    public record AgreementDisplayModel
    {
        public int FamilyHeadId { get; set; }
        public DateTime SignedOn { get; set; }
        public string SignatureBase64 { get; set; }


        public string SharedAgreement { get; set; }
        public string SpecificAgreement { get; set; }
        public string EncryptedPassport { get; set; }
        public string CompanySeal { get; set; }

        public string Title { get; set; }
        public string Name { get; set; }
        public string CID { get; set; }
        public string PassportNumber { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string ApplicantType { get; set; }
        public string HajjAppNumber { get; set; }
        public string HajjiImage { get; set; }

        public string PricingPerPersonPkr { get; set; }
        public string PricingPerPersonSar { get; set; }
        public string PricingPerPersonUsd { get; set; }

        public string ADFPricePkr { get; set; }
        public string ADFPriceSar { get; set; }
        public string ADFPriceUsd { get; set; }

        public string DiscountPkr { get; set; }
        public string DiscountSar { get; set; }
        public string DiscountUsd { get; set; }

        public string TotalPerPersonPkr { get; set; }
        public string TotalPerPersonSar { get; set; }
        public string TotalPerPersonUsd { get; set; }



        public string AmountReceived { get; set; }
        public string BalanceDue { get; set; }

        public string Package { get; set; }
        public string MakkahFacilities { get; set; }
        public string MedinaFacilities { get; set; }
        public string AziziaFacilities { get; set; }
        public string MashairFacilities { get; set; }
        public string Airline { get; set; }
        public string MedinahArrival { get; set; }

    }

    public record DisplayDataFormModelEntry
    {
        public int FamilyHeadId { get; set; }
        public string SignedDate { get; set; }
        public bool ShowSignature { get; set; }
        public string CID { get; set; }
        public string SignatureBase64 { get; set; }
        public string MehramSignatureBase64 { get; set; }
        public string ApplicantType { get; set; }
        public string FamilyHeadImage { get; set; }
        public string HajjAppNumber { get; set; }
        public string EnrollmentNumber { get; set; }
        public string PackageNameDuration { get; set; }
        public int? MoraPackageNumber { get; set; }
        public string HGOName { get; set; }
        public string StayDuration { get; set; }
        public string SubAgaintAppNumber { get; set; }
        public string SurName { get; set; }
        public string GivenName { get; set; }
        public string FatherHusbandName { get; set; }
        public string Email { get; set; }
        public string PassportNumber { get; set; }
        public string PassportDateOfExpiry { get; set; }
        public string DateOfBirth { get; set; } // dd/mm/yyyy
        public string CNIC { get; set; }
        public string BloodGroup { get; set; }
        public Gender? Gender { get; set; }
        public string Fiqah { get; set; }
        public string IsMarried { get; set; }
        public string PresentPostalAddress { get; set; }
        public string TehsilCode { get; set; }
        public string MobileNumber { get; set; }
        public string TelephoneNumber { get; set; }

        public string EmployerBusinessName { get; set; }
        public string Designation { get; set; }
        public string OfficialAddress { get; set; }
        public string OfficialContact { get; set; }
        public string OfficialEmail { get; set; }

        public string PerformedHajjInLast5Years { get; set; }
        public string YearsPerformedHajj { get; set; }
        public string WantToHajjEBadal { get; set; }
        public string WantToPerformHajjAsGroupWorker { get; set; }
        public string AreYouMehramOfALady { get; set; }
        public string NomineeName { get; set; }
        public string NomineeRelation { get; set; }
        public string NomineeCNIC { get; set; }
        public string NomineeTelephone { get; set; }
        public string NomineeContact { get; set; }

        public string EmergencyName { get; set; }
        public string EmergencyRelation { get; set; }
        public string EmergencyCellNumber { get; set; }
        public string MehramAppNumber { get; set; }
        public string MehramCode { get; set; }
        public string MehramRelationship { get; set; }
        public string MehramName { get; set; }

        public string HajjPackageDuesPkr { get; set; }
        public string HajjPackageDuesSar { get; set; }
        public string HajjPackageDuesUsd { get; set; }
        public string IndividualAdditionalFacilitiesDuesPkr { get; set; }
        public string IndividualAdditionalFacilitiesDuesSar { get; set; }
        public string IndividualAdditionalFacilitiesDuesUsd { get; set; }

        public string ExtraAdfDuesPkr { get; set; }
        public string ExtraAdfDuesSar { get; set; }
        public string ExtraAdfDuesUsd { get; set; }
        public string DiscountPkr { get; set; }
        public string DiscountSar { get; set; }
        public string DiscountUsd { get; set; }
        public string TotalPkr { get; set; }
        public string TotalSar { get; set; }
        public string TotalUsd { get; set; }
        public string MedinahArrival { get; set; }
        public string FlightType { get; set; }
        public string FlightClass { get; set; }
        public string TicketIncluded { get; set; }
        public string TicketPeriod { get; set; }
        public float? PackageTicketPricePkr { get; set; }
        public float? PackageTicketPriceSar { get; set; }
        public float? PackageTicketPriceUsd { get; set; }

        public float? FlightTicketDifferencesPkr { get; set; }
        public float? FlightTicketDifferencesSar { get; set; }
        public float? FlightTicketDifferencesUsd { get; set; }
        public string POD { get; set; }

        public List<DisplayDataFormTrainingSession> TrainingSessions { get; set; } = new List<DisplayDataFormTrainingSession>();

        public string IntercityTransport { get; set; }
        public string WithinCity { get; set; }
        public string MashairTrasport { get; set; }
        public string AirportTransport { get; set; }

        public List<DisplayDataFormIndividualFacility> IndividualFacilities { get; set; } = new List<DisplayDataFormIndividualFacility>();
        public List<DisplayDataFormGift> Gifts { get; set; } = new List<DisplayDataFormGift>();
        public List<DisplayDataFormExtraFacility> ExtraFacilities { get; set; } = new List<DisplayDataFormExtraFacility>();

        public DisplayDataFormAzizia AziziaAccomodation { get; set; }
        public DisplayDataFormAccomodation MakkahAccomodation { get; set; }
        public DisplayDataFormAccomodation MedinaAccomodation { get; set; }
        public DisplayDataFormMina MinaAccomodation { get; set; }

        public List<string> Remarks { get; set; }
    }

    public record DisplayDataFormTrainingSession(string TrainingSession, string ExpectedVenue, string ExpectedTime);

    public record DisplayDataFormIndividualFacility(int Id, string Description,
        string PricePkr, string PriceSar, string PriceUsd);

    public record DisplayDataFormAccomodation(string Ziarat, string Days, string Distance,
        string FoodPackage,string AccommodationType, string AccomodationName, string CheckIn,
        string RoomSharing, string IADF, string Category);

    public record DisplayDataFormAzizia(string Type, string Name, string Category, string CheckIn, string Days, string Distance, string FoodPackage,
                                    string HaramShuttle, string ReligiousLectures, string IADF);

    public record DisplayDataFormMina(string Category, string FoodPackage, string SofaSitting, string CampTent, string Arafat, string ReligiousLectures);

    public record DisplayDataFormGift(string Gift);

    public record DisplayDataFormExtraFacility(string Description, float? PricePkr, float? PriceSar, float? PriceUsd);


}

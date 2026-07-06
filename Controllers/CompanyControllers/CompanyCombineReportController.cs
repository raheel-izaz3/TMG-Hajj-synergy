using TMG.Data;
using TMG.ManagerClasses;
using Microsoft.AspNetCore.Mvc;
using TMG.Models;
using TMG.Models.CompanyModels;
using Microsoft.EntityFrameworkCore;
using TMG.Models.InvoiceModels;

namespace TMG.Models.CompanyModels
{

    public record CollectionCompanyModel
    {
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public string Package { get; set; }
        public List<CollectionCompanyModelEntry> Entries { get; set; } = new List<CollectionCompanyModelEntry>();
    }



    public record CollectionCompanyModelEntry
    {
        public float TotalAmountUSD { get; set; }
        public float PackageInUSD { get; set; }
        public float ADFInUSD { get; set; }
        public float AddonsInUSD { get; set; }

        public string Company { get; set; }
        public string Package { get; set; }
        public string FCID { get; set; }
        public string FamilyHead { get; set; }
        public float TotalAmountSar { get; set; }
        public float PackageInSAR { get; set; }
        public float ADFInSAR { get; set; }
        public float AddonsInSAR { get; set; }
        public float TotalRecivableSAR { get; set; }

        public float BaseExchangeRate { get; set; }
        public float TotalRecivablePKR { get; set; }
        public float TotalRecivableUSD { get; set; }
        public float TotalRecivedSar { get; set; }
        public float TotalRecivedPkr { get; set; }
        public float TotalRecivedUsd { get; set; }
        public float ReceivableSar { get; set; }
        public float ReceivablePkr { get; set; }
        public float ReceivableUsd { get; set; }
        public float PrerealizationSar { get; set; }
        public float PrerealizationPkr { get; set; }
        public float PrerealizationUsd { get; set; }
        public float RealizationSar { get; set; }
        public float RealizationPkr { get; set; }
        public float RealizationUsd { get; set; }
        public string Spo { get; set; }



    }
}

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyCombineReportController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _http;


        public CompanyCombineReportController(TMGDbContext dbContext, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _http = http;

        }

        [HttpGet]
        public IActionResult Collection()
        {

            ViewBag.Company = _dbContext.CompaniesWithoutTrash;
            ViewBag.Package = _dbContext.PackagesWithoutTrash;
            ViewBag.Spo = _dbContext.SPOsWithoutTrash;

            var Company = _dbContext.CompaniesWithoutTrash.ToList();
            var Package = _dbContext.PackagesWithoutTrash.ToList();

            // CollectionCompanyModel model = new CollectionCompanyModel();

            List<CollectionCompanyModel> model = new();

            var rate = _dbContext.RateOfExchangesWithoutTrash
                .OrderByDescending(e => e.Date)
                .FirstOrDefault(e => e.Currency == Currencies.PKR);

            for (int i = 0; i < Company.Count; i++)
            {

                var _Company = Company.ElementAt(i);
                var _Packages = Package.Where(e => e.CompanyId == _Company.CompanyId).ToList();

                for (var z = 0; z < _Packages.Count; z++)
                {
                    var __Pacakage = _Packages.ElementAt(z);
                    var packageEntry = new CollectionCompanyModel
                    {
                        Package = __Pacakage.Name,
                        CompanyCode = _Company.CompanyCode,
                        CompanyName = _Company.CompanyName
                    };

                    // fam loop

                    var Family = _dbContext.FamilyHeadsWithoutTrash
                    .Include(e => e.SPO)
                    .Include(e => e.Package)
                    .ThenInclude(e => e.Company)
                    .Where(e => e.IsHead == true && e.Package.PackageId == __Pacakage.PackageId)
                    .Select(e => new
                    {
                        e.GivenName,
                        e.FamilyHeadId,
                        e.DateOfBirth,
                        e.Package.Name,
                        e.Package.Company.CompanyCode,
                        e.Package.AdultSar,
                        e.Package.AdultUsd,
                        e.Package.ChildSar,
                        e.Package.ChildUsd,
                        e.Package.InfantUsd,
                        e.Package.InfantSar,
                        e.Package.PackageId,
                        e.Package.SARROE,
                        e.Package.USDROE,
                        e.SarDiscountAmount,
                        e.UsdDiscountAmount,
                        e.AccountId,
                        e.FcId,
                        SPOName = e.SPO.Name,

                    })
                    .ToList();


                    float totalRecivableSar = 0;
                    float totalRecivableUsd = 0;

                    float totalRecivedSar = 0;
                    float totalRecivedUsd = 0;
                    float totalRecivedPkr = 0;

                    float recivableSar = 0;
                    float recivableUsd = 0;
                    float recivablePkr = 0;

                    float prerealizationSar = 0;
                    float prerealizationUsd = 0;
                    float prerealizationPkr = 0;

                    float realizationSar = 0;
                    float realizationUsd = 0;
                    float realizationPkr = 0;


                    for (int j = 0; j < Family.Count; j++)
                    {






                        var _Family = Family.ElementAt(j);

                        //========================================//
                        //addition code for receivable bug jahanzeb added after discussion with ali
                        //var result = _dbContext.PaymentVouchers
                        //.Join(_dbContext.Accounts, p => p.AccountId, a => a.AccountId, (p, a) => new { p, a })
                        //.Join(_dbContext.FamilyHeads, pa => pa.a.AccountId, f => f.AccountId, (pa, f) => new { pa.p, pa.a, f })
                        //.Where(pf => pf.f.IsHead && pf.f.FamilyHeadId == _Family.FamilyHeadId)
                        //.Select(pf => new
                        //{
                        //    pf.p.Amount,
                        //    pf.p.USDROE,
                        //    pf.p.Currency,
                        //    pf.f.FcId,
                        //    PaymentVoucher = pf.p // Include the entire PaymentVoucher entity
                        //})
                        //.ToList();

                        //========================================== //

                        float packageAddonsSar = 0;
                        float packageAddonsUsd = 0;
                        var PackageAddons = _dbContext.PackageAddons
                                            .Where(e => e.PackageId == _Family.PackageId)
                                            .ToList();
                        for (int k = 0; k < PackageAddons.Count; k++)
                        {
                            var _PackageAddons = PackageAddons.ElementAt(k);
                            packageAddonsSar += _PackageAddons.SarPrice;
                            packageAddonsUsd += _PackageAddons.UsdPrice;
                        }
                        int familyMember = _dbContext.FamilyMembers
                                            .Include(e => e.FamilyHead)
                                            .Where(e => e.HeadId == _Family.FamilyHeadId)
                                            .Count();

                        float totalAddonsSar = familyMember * packageAddonsSar;
                        float totalAddonsUsd = familyMember * packageAddonsUsd;
                        var FamilyHeadAdditionalFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Package)
                                            .Where(e => e.FamilyHeadId == _Family.FamilyHeadId)
                                            .Select(e => new
                                            {
                                                e.AdditionalFacilities.AdultSar,
                                                e.AdditionalFacilities.ChildSar,
                                                e.AdditionalFacilities.InfantSar,

                                                e.AdditionalFacilities.AdultUsd,
                                                e.AdditionalFacilities.ChildUsd,
                                                e.AdditionalFacilities.InfantUsd,
                                            })
                                            .ToList();

                        float familyHeadAdditionalFacilitiesSar = 0;
                        float familyHeadAdditionalFacilitiesUsd = 0;
                        for (int k = 0; k < FamilyHeadAdditionalFacilities.Count; k++)
                        {
                            var _FamilyHeadAdditionalFacilities = FamilyHeadAdditionalFacilities.ElementAt(k);
                            if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                            {
                                familyHeadAdditionalFacilitiesSar += (float)_FamilyHeadAdditionalFacilities.InfantSar;
                                familyHeadAdditionalFacilitiesUsd += (float)_FamilyHeadAdditionalFacilities.InfantUsd;
                            }
                            else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                            {
                                familyHeadAdditionalFacilitiesSar += (float)_FamilyHeadAdditionalFacilities.ChildSar;
                                familyHeadAdditionalFacilitiesUsd += (float)_FamilyHeadAdditionalFacilities.ChildUsd;
                            }
                            else
                            {
                                familyHeadAdditionalFacilitiesUsd += (float)_FamilyHeadAdditionalFacilities.AdultUsd;
                                familyHeadAdditionalFacilitiesSar += (float)_FamilyHeadAdditionalFacilities.AdultSar;
                            }
                        }



                        if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                        {
                            totalRecivableSar += (float)_Family.InfantSar;
                            totalRecivableUsd += (float)_Family.InfantUsd;
                        }
                        else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                        {
                            totalRecivableUsd += (float)_Family.ChildUsd;
                            totalRecivableSar += (float)_Family.ChildSar;
                        }
                        else
                        {
                            totalRecivableSar += (float)_Family.AdultSar;
                            totalRecivableUsd += (float)_Family.AdultUsd;
                        }

                        totalRecivableSar += familyHeadAdditionalFacilitiesSar;
                        totalRecivableUsd += familyHeadAdditionalFacilitiesUsd;
                        totalRecivableSar += totalAddonsSar;
                        totalRecivableUsd += totalAddonsUsd;



                        float totalAmountSar = totalRecivableSar;
                        float totalAmountUsd = totalRecivableUsd;
                        float _totalRecivableSar = (totalRecivableSar - _Family.SarDiscountAmount);
                        float _totalRecivableUsd = (totalRecivableUsd - _Family.UsdDiscountAmount);

                        // float totalRecivablePkr = totalRecivableSar * _Family.SARROE;
                        float totalRecivablePkr = totalRecivableUsd * _Family.USDROE;

                        var VoucherEntries = _dbContext.VoucherEntries
                                            .Include(e => e.PaymentVoucher)
                                            .Where(e => e.AccountId == _Family.AccountId)
                                            .Select(e => new
                                            {
                                                e.AccountId,
                                                e.Credit,
                                                e.PaymentVoucher.Currency,
                                                e.PaymentVoucher.SARROE,
                                                e.PaymentVoucher.USDROE,
                                                e.PaymentVoucher.PKRROE
                                            })
                                            .ToList();


                        for (int k = 0; k < VoucherEntries.Count; k++)
                        {
                            var _VoucherEntries = VoucherEntries.ElementAt(k);
                            if (_VoucherEntries.Currency == Currencies.PKR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.SAR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.USD)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            }
                            //else if (_VoucherEntries.Currency == Currencies.AED)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.GBP)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.EURO)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                        }

                        recivableSar = totalAmountSar - totalRecivedSar;
                        recivablePkr = recivableSar * _Family.SARROE; //(rate == null ? 0 : rate.SARROE) ;
                        recivableUsd = recivablePkr / _Family.USDROE;// (rate == null ? 0 : rate.USDROE) ;


                        var Invoice = _dbContext.Invoices
                                          .Join(
                                                  _dbContext.FamilyHeads,
                                                  invoice => invoice.FamilyHeadId,
                                                  family => family.FamilyHeadId,
                                                  (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                                          )
                                          .Where(e => e.FamilyHead.AccountId == _Family.AccountId)
                                          .Select(e => new
                                          {
                                              e.Invoice.AmountRecived,
                                              e.Invoice.InvoiceStatus,
                                              e.Invoice.Currency,
                                              e.Invoice.PKRROE,
                                              e.Invoice.SARROE,
                                              e.Invoice.USDROE,
                                          })
                                          .ToList();

                        for (int k = 0; k < Invoice.Count; k++)
                        {

                            var _Invoice = Invoice.ElementAt(k);
                            if (_Invoice.InvoiceStatus != InvoiceStatus.Cleared)
                            {

                                if (_Invoice.Currency == Currencies.PKR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.SAR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.USD)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                }
                                //else if (_Invoice.Currency == Currencies.AED)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.GBP)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.EURO)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                            }


                        }


                        realizationSar = totalRecivedSar + prerealizationSar;
                        realizationPkr = totalRecivedPkr + prerealizationPkr;
                        realizationUsd = totalRecivedUsd + prerealizationUsd;


                        packageEntry.Entries.Add(new CollectionCompanyModelEntry
                        {
                            PackageInUSD = _Family.AdultSar,
                            AddonsInUSD = totalAddonsUsd,
                            ADFInUSD = familyHeadAdditionalFacilitiesUsd,
                            TotalRecivableUSD = _totalRecivableUsd,

                            Company = _Company.CompanyCode,
                            FamilyHead = _Family.GivenName,
                            Spo = _Family.SPOName,
                            FCID = _Family.FcId,
                            Package = _Family.Name,
                            PackageInSAR = _Family.AdultSar,
                            AddonsInSAR = totalAddonsSar,
                            ADFInSAR = familyHeadAdditionalFacilitiesSar,
                            TotalRecivableSAR = _totalRecivableSar,
                            BaseExchangeRate = _Family.SARROE,
                            TotalAmountSar = totalAmountSar,
                            TotalRecivablePKR = totalRecivablePkr,
                            TotalRecivedSar = totalRecivedSar,
                            TotalRecivedPkr = totalRecivedPkr,
                            TotalRecivedUsd = totalRecivedUsd,
                            ReceivableSar = recivableSar,
                            ReceivablePkr = recivablePkr,
                            ReceivableUsd = recivableUsd,
                            PrerealizationSar = prerealizationSar,
                            PrerealizationPkr = prerealizationPkr,
                            PrerealizationUsd = prerealizationUsd,
                            RealizationSar = realizationSar,
                            RealizationPkr = realizationPkr,
                            RealizationUsd = realizationUsd,
                        });
                    }







                    model.Add(packageEntry);

                }


            }
            return View(model);

        }


        public IActionResult CompanyWise(int id)
        {


            ViewBag.Company = _dbContext.CompaniesWithoutTrash;
            ViewBag.Package = _dbContext.PackagesWithoutTrash;
            ViewBag.Spo = _dbContext.SPOsWithoutTrash;

            var Company = _dbContext.CompaniesWithoutTrash.Where(e => e.CompanyId == id).ToList();
            var Package = _dbContext.PackagesWithoutTrash.ToList();



            // CollectionCompanyModel model = new CollectionCompanyModel();

            List<CollectionCompanyModel> model = new();

            var rate = _dbContext.RateOfExchangesWithoutTrash
                .OrderByDescending(e => e.Date)
                .FirstOrDefault(e => e.Currency == Currencies.PKR);

            for (int i = 0; i < Company.Count; i++)
            {

                var _Company = Company.ElementAt(i);
                var _Packages = Package.Where(e => e.CompanyId == _Company.CompanyId).ToList();

                for (var z = 0; z < _Packages.Count; z++)
                {
                    var __Pacakage = _Packages.ElementAt(z);
                    var packageEntry = new CollectionCompanyModel();

                    packageEntry.Package = __Pacakage.Name;
                    packageEntry.CompanyCode = _Company.CompanyCode;
                    packageEntry.CompanyName = _Company.CompanyName;

                    // fam loop

                    var Family = _dbContext.FamilyHeadsWithoutTrash
                    .Include(e => e.SPO)
                    .Include(e => e.Package)
                    .ThenInclude(e => e.Company)
                    .Where(e => e.IsHead == true && e.Package.PackageId == __Pacakage.PackageId)
                    .Select(e => new
                    {
                        e.GivenName,
                        e.FamilyHeadId,
                        e.DateOfBirth,
                        e.Package.Name,
                        e.Package.Company.CompanyCode,
                        e.Package.AdultSar,
                        e.Package.ChildSar,
                        e.Package.InfantSar,
                        e.Package.PackageId,
                        e.Package.SARROE,
                        e.Package.USDROE,
                        e.SarDiscountAmount,
                        e.AccountId,
                        e.FcId,
                        SPOName = e.SPO.Name,

                    })
                    .ToList();




                    float totalRecivable = 0;

                    float totalRecivedSar = 0;
                    float totalRecivedUsd = 0;
                    float totalRecivedPkr = 0;

                    float recivableSar = 0;
                    float recivableUsd = 0;
                    float recivablePkr = 0;

                    float prerealizationSar = 0;
                    float prerealizationUsd = 0;
                    float prerealizationPkr = 0;

                    float realizationSar = 0;
                    float realizationUsd = 0;
                    float realizationPkr = 0;


                    for (int j = 0; j < Family.Count; j++)
                    {

                        var _Family = Family.ElementAt(j);
                        float packageAddons = 0;
                        var PackageAddons = _dbContext.PackageAddons
                                            .Where(e => e.PackageId == _Family.PackageId)
                                            .ToList();
                        for (int k = 0; k < PackageAddons.Count; k++)
                        {
                            var _PackageAddons = PackageAddons.ElementAt(k);
                            packageAddons += _PackageAddons.SarPrice;
                        }
                        int familyMember = _dbContext.FamilyMembers
                                            .Include(e => e.FamilyHead)
                                            .Where(e => e.HeadId == _Family.FamilyHeadId)
                                            .Count();

                        float totalAddonsSar = familyMember * packageAddons;
                        var FamilyHeadAdditionalFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Package)
                                            .Where(e => e.FamilyHeadId == _Family.FamilyHeadId)
                                            .Select(e => new
                                            {
                                                e.AdditionalFacilities.AdultSar,
                                                e.AdditionalFacilities.ChildSar,
                                                e.AdditionalFacilities.InfantSar
                                            })
                                            .ToList();

                        float familyHeadAdditionalFacilities = 0;
                        for (int k = 0; k < FamilyHeadAdditionalFacilities.Count; k++)
                        {
                            var _FamilyHeadAdditionalFacilities = FamilyHeadAdditionalFacilities.ElementAt(k);
                            if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.InfantSar;
                            }
                            else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.ChildSar;
                            }
                            else
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.AdultSar;
                            }
                        }



                        if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                        {
                            totalRecivable += (float)_Family.InfantSar;
                        }
                        else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                        {
                            totalRecivable += (float)_Family.ChildSar;
                        }
                        else
                        {
                            totalRecivable += (float)_Family.AdultSar;
                        }

                        totalRecivable += familyHeadAdditionalFacilities;
                        totalRecivable += totalAddonsSar;

                        float totalAmountSar = totalRecivable;
                        float totalRecivableSar = (totalRecivable - _Family.SarDiscountAmount);

                        float totalRecivablePkr = totalRecivableSar * _Family.SARROE;


                        var VoucherEntries = _dbContext.VoucherEntries
                                            .Include(e => e.PaymentVoucher)
                                            .Where(e => e.AccountId == _Family.AccountId)
                                            .Select(e => new
                                            {
                                                e.AccountId,
                                                e.Credit,
                                                e.PaymentVoucher.Currency,
                                                e.PaymentVoucher.SARROE,
                                                e.PaymentVoucher.USDROE,
                                                e.PaymentVoucher.PKRROE
                                            })
                                            .ToList();




                        for (int k = 0; k < VoucherEntries.Count; k++)
                        {
                            var _VoucherEntries = VoucherEntries.ElementAt(k);
                            if (_VoucherEntries.Currency == Currencies.PKR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.SAR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.USD)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            }
                            //else if (_VoucherEntries.Currency == Currencies.AED)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.GBP)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.EURO)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                        }

                        recivableSar = totalAmountSar - totalRecivedSar;
                        recivablePkr = recivableSar * rate.SARROE;
                        recivableUsd = recivablePkr * rate.USDROE;


                        var Invoice = _dbContext.Invoices
                                          .Join(
                                                  _dbContext.FamilyHeads,
                                                  invoice => invoice.FamilyHeadId,
                                                  family => family.FamilyHeadId,
                                                  (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                                          )
                                          .Where(e => e.FamilyHead.AccountId == _Family.AccountId)
                                          .Select(e => new
                                          {
                                              e.Invoice.AmountRecived,
                                              e.Invoice.InvoiceStatus,
                                              e.Invoice.Currency,
                                              e.Invoice.PKRROE,
                                              e.Invoice.SARROE,
                                              e.Invoice.USDROE,
                                          })
                                          .ToList();

                        for (int k = 0; k < Invoice.Count; k++)
                        {

                            var _Invoice = Invoice.ElementAt(k);
                            if (_Invoice.InvoiceStatus != InvoiceStatus.Cleared)
                            {

                                if (_Invoice.Currency == Currencies.PKR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.SAR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.USD)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                }
                                //else if (_Invoice.Currency == Currencies.AED)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.GBP)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.EURO)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                            }


                        }


                        realizationSar = totalRecivedSar + prerealizationSar;
                        realizationPkr = totalRecivedPkr + prerealizationPkr;
                        realizationUsd = totalRecivedUsd + prerealizationUsd;


                        packageEntry.Entries.Add(new CollectionCompanyModelEntry
                        {
                            Company = _Company.CompanyCode,
                            FamilyHead = _Family.GivenName,
                            Spo = _Family.SPOName,
                            FCID = _Family.FcId,
                            Package = _Family.Name,
                            PackageInSAR = _Family.AdultSar,
                            AddonsInSAR = totalAddonsSar,
                            ADFInSAR = familyHeadAdditionalFacilities,
                            TotalRecivableSAR = totalRecivableSar,
                            BaseExchangeRate = _Family.SARROE,
                            TotalAmountSar = totalAmountSar,
                            TotalRecivablePKR = totalRecivablePkr,
                            TotalRecivedSar = totalRecivedSar,
                            TotalRecivedPkr = totalRecivedPkr,
                            TotalRecivedUsd = totalRecivedUsd,
                            ReceivableSar = recivableSar,
                            ReceivablePkr = recivablePkr,
                            ReceivableUsd = recivableUsd,
                            PrerealizationSar = prerealizationSar,
                            PrerealizationPkr = prerealizationPkr,
                            PrerealizationUsd = prerealizationUsd,
                            RealizationSar = realizationSar,
                            RealizationPkr = realizationPkr,
                            RealizationUsd = realizationUsd,
                        });
                    }







                    model.Add(packageEntry);

                }


            }

            return View("Collection", model);



        }


        public IActionResult PackageWise(int id)
        {


            ViewBag.Company = _dbContext.CompaniesWithoutTrash;
            ViewBag.Package = _dbContext.PackagesWithoutTrash;
            ViewBag.Spo = _dbContext.SPOsWithoutTrash;

            var Company = _dbContext.CompaniesWithoutTrash.ToList();
            var Package = _dbContext.PackagesWithoutTrash.Where(e => e.PackageId == id).ToList();



            // CollectionCompanyModel model = new CollectionCompanyModel();

            List<CollectionCompanyModel> model = new();

            var rate = _dbContext.RateOfExchangesWithoutTrash
                .OrderByDescending(e => e.Date)
                .FirstOrDefault(e => e.Currency == Currencies.PKR);

            for (int i = 0; i < Company.Count; i++)
            {

                var _Company = Company.ElementAt(i);
                var _Packages = Package.Where(e => e.CompanyId == _Company.CompanyId).ToList();

                for (var z = 0; z < _Packages.Count; z++)
                {
                    var __Pacakage = _Packages.ElementAt(z);
                    var packageEntry = new CollectionCompanyModel();

                    packageEntry.Package = __Pacakage.Name;
                    packageEntry.CompanyCode = _Company.CompanyCode;
                    packageEntry.CompanyName = _Company.CompanyName;

                    // fam loop

                    var Family = _dbContext.FamilyHeadsWithoutTrash
                    .Include(e => e.SPO)
                    .Include(e => e.Package)
                    .ThenInclude(e => e.Company)
                    .Where(e => e.IsHead == true && e.Package.PackageId == __Pacakage.PackageId)
                    .Select(e => new
                    {
                        e.GivenName,
                        e.FamilyHeadId,
                        e.DateOfBirth,
                        e.Package.Name,
                        e.Package.Company.CompanyCode,
                        e.Package.AdultSar,
                        e.Package.ChildSar,
                        e.Package.InfantSar,
                        e.Package.PackageId,
                        e.Package.SARROE,
                        e.Package.USDROE,
                        e.SarDiscountAmount,
                        e.AccountId,
                        e.FcId,
                        SPOName = e.SPO.Name,

                    })
                    .ToList();




                    float totalRecivable = 0;

                    float totalRecivedSar = 0;
                    float totalRecivedUsd = 0;
                    float totalRecivedPkr = 0;

                    float recivableSar = 0;
                    float recivableUsd = 0;
                    float recivablePkr = 0;

                    float prerealizationSar = 0;
                    float prerealizationUsd = 0;
                    float prerealizationPkr = 0;

                    float realizationSar = 0;
                    float realizationUsd = 0;
                    float realizationPkr = 0;


                    for (int j = 0; j < Family.Count; j++)
                    {

                        var _Family = Family.ElementAt(j);
                        float packageAddons = 0;
                        var PackageAddons = _dbContext.PackageAddons
                                            .Where(e => e.PackageId == _Family.PackageId)
                                            .ToList();
                        for (int k = 0; k < PackageAddons.Count; k++)
                        {
                            var _PackageAddons = PackageAddons.ElementAt(k);
                            packageAddons += _PackageAddons.SarPrice;
                        }
                        int familyMember = _dbContext.FamilyMembers
                                            .Include(e => e.FamilyHead)
                                            .Where(e => e.HeadId == _Family.FamilyHeadId)
                                            .Count();

                        float totalAddonsSar = familyMember * packageAddons;
                        var FamilyHeadAdditionalFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Package)
                                            .Where(e => e.FamilyHeadId == _Family.FamilyHeadId)
                                            .Select(e => new
                                            {
                                                e.AdditionalFacilities.AdultSar,
                                                e.AdditionalFacilities.ChildSar,
                                                e.AdditionalFacilities.InfantSar
                                            })
                                            .ToList();

                        float familyHeadAdditionalFacilities = 0;
                        for (int k = 0; k < FamilyHeadAdditionalFacilities.Count; k++)
                        {
                            var _FamilyHeadAdditionalFacilities = FamilyHeadAdditionalFacilities.ElementAt(k);
                            if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.InfantSar;
                            }
                            else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.ChildSar;
                            }
                            else
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.AdultSar;
                            }
                        }



                        if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                        {
                            totalRecivable += (float)_Family.InfantSar;
                        }
                        else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                        {
                            totalRecivable += (float)_Family.ChildSar;
                        }
                        else
                        {
                            totalRecivable += (float)_Family.AdultSar;
                        }

                        totalRecivable += familyHeadAdditionalFacilities;
                        totalRecivable += totalAddonsSar;

                        float totalAmountSar = totalRecivable;
                        float totalRecivableSar = (totalRecivable - _Family.SarDiscountAmount);

                        float totalRecivablePkr = totalRecivableSar * _Family.SARROE;


                        var VoucherEntries = _dbContext.VoucherEntries
                                            .Include(e => e.PaymentVoucher)
                                            .Where(e => e.AccountId == _Family.AccountId)
                                            .Select(e => new
                                            {
                                                e.AccountId,
                                                e.Credit,
                                                e.PaymentVoucher.Currency,
                                                e.PaymentVoucher.SARROE,
                                                e.PaymentVoucher.USDROE,
                                                e.PaymentVoucher.PKRROE
                                            })
                                            .ToList();




                        for (int k = 0; k < VoucherEntries.Count; k++)
                        {
                            var _VoucherEntries = VoucherEntries.ElementAt(k);
                            if (_VoucherEntries.Currency == Currencies.PKR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.SAR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.USD)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            }
                            //else if (_VoucherEntries.Currency == Currencies.AED)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.GBP)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.EURO)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                        }

                        recivableSar = totalAmountSar - totalRecivedSar;
                        recivablePkr = recivableSar * rate.SARROE;
                        recivableUsd = recivablePkr * rate.USDROE;


                        var Invoice = _dbContext.Invoices
                                          .Join(
                                                  _dbContext.FamilyHeads,
                                                  invoice => invoice.FamilyHeadId,
                                                  family => family.FamilyHeadId,
                                                  (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                                          )
                                          .Where(e => e.FamilyHead.AccountId == _Family.AccountId)
                                          .Select(e => new
                                          {
                                              e.Invoice.AmountRecived,
                                              e.Invoice.InvoiceStatus,
                                              e.Invoice.Currency,
                                              e.Invoice.PKRROE,
                                              e.Invoice.SARROE,
                                              e.Invoice.USDROE,
                                          })
                                          .ToList();

                        for (int k = 0; k < Invoice.Count; k++)
                        {

                            var _Invoice = Invoice.ElementAt(k);
                            if (_Invoice.InvoiceStatus != InvoiceStatus.Cleared)
                            {

                                if (_Invoice.Currency == Currencies.PKR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.SAR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.USD)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                }
                                //else if (_Invoice.Currency == Currencies.AED)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.GBP)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.EURO)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                            }


                        }


                        realizationSar = totalRecivedSar + prerealizationSar;
                        realizationPkr = totalRecivedPkr + prerealizationPkr;
                        realizationUsd = totalRecivedUsd + prerealizationUsd;


                        packageEntry.Entries.Add(new CollectionCompanyModelEntry
                        {
                            Company = _Company.CompanyCode,
                            FamilyHead = _Family.GivenName,
                            Spo = _Family.SPOName,
                            FCID = _Family.FcId,
                            Package = _Family.Name,
                            PackageInSAR = _Family.AdultSar,
                            AddonsInSAR = totalAddonsSar,
                            ADFInSAR = familyHeadAdditionalFacilities,
                            TotalRecivableSAR = totalRecivableSar,
                            BaseExchangeRate = _Family.SARROE,
                            TotalAmountSar = totalAmountSar,
                            TotalRecivablePKR = totalRecivablePkr,
                            TotalRecivedSar = totalRecivedSar,
                            TotalRecivedPkr = totalRecivedPkr,
                            TotalRecivedUsd = totalRecivedUsd,
                            ReceivableSar = recivableSar,
                            ReceivablePkr = recivablePkr,
                            ReceivableUsd = recivableUsd,
                            PrerealizationSar = prerealizationSar,
                            PrerealizationPkr = prerealizationPkr,
                            PrerealizationUsd = prerealizationUsd,
                            RealizationSar = realizationSar,
                            RealizationPkr = realizationPkr,
                            RealizationUsd = realizationUsd,
                        });
                    }







                    model.Add(packageEntry);

                }


            }
            return View("Collection", model);




        }


        public IActionResult SpoWise(int id)
        {


            ViewBag.Company = _dbContext.CompaniesWithoutTrash;
            ViewBag.Package = _dbContext.PackagesWithoutTrash;
            ViewBag.Spo = _dbContext.SPOsWithoutTrash;

            var Company = _dbContext.CompaniesWithoutTrash.ToList();
            var Package = _dbContext.PackagesWithoutTrash.ToList();

            List<CollectionCompanyModel> model = new();

            var rate = _dbContext.RateOfExchangesWithoutTrash
                .OrderByDescending(e => e.Date)
                .FirstOrDefault(e => e.Currency == Currencies.PKR);

            for (int i = 0; i < Company.Count; i++)
            {

                var _Company = Company.ElementAt(i);
                var _Packages = Package.Where(e => e.CompanyId == _Company.CompanyId).ToList();

                for (var z = 0; z < _Packages.Count; z++)
                {
                    var __Pacakage = _Packages.ElementAt(z);
                    var packageEntry = new CollectionCompanyModel();

                    packageEntry.Package = __Pacakage.Name;
                    packageEntry.CompanyCode = _Company.CompanyCode;
                    packageEntry.CompanyName = _Company.CompanyName;

                    // fam loop

                    var Family = _dbContext.FamilyHeadsWithoutTrash
                    .Include(e => e.SPO)
                    .Include(e => e.Package)
                    .ThenInclude(e => e.Company)
                    .Where(e => e.IsHead == true && e.Package.PackageId == __Pacakage.PackageId && e.SPOId == id)
                    .Select(e => new
                    {
                        e.GivenName,
                        e.FamilyHeadId,
                        e.DateOfBirth,
                        e.Package.Name,
                        e.Package.Company.CompanyCode,
                        e.Package.AdultSar,
                        e.Package.ChildSar,
                        e.Package.InfantSar,
                        e.Package.PackageId,
                        e.Package.SARROE,
                        e.Package.USDROE,
                        e.SarDiscountAmount,
                        e.AccountId,
                        e.FcId,
                        SPOName = e.SPO.Name,


                    })
                    .ToList();




                    float totalRecivable = 0;

                    float totalRecivedSar = 0;
                    float totalRecivedUsd = 0;
                    float totalRecivedPkr = 0;

                    float recivableSar = 0;
                    float recivableUsd = 0;
                    float recivablePkr = 0;

                    float prerealizationSar = 0;
                    float prerealizationUsd = 0;
                    float prerealizationPkr = 0;

                    float realizationSar = 0;
                    float realizationUsd = 0;
                    float realizationPkr = 0;


                    for (int j = 0; j < Family.Count; j++)
                    {

                        var _Family = Family.ElementAt(j);
                        float packageAddons = 0;
                        var PackageAddons = _dbContext.PackageAddons
                                            .Where(e => e.PackageId == _Family.PackageId)
                                            .ToList();
                        for (int k = 0; k < PackageAddons.Count; k++)
                        {
                            var _PackageAddons = PackageAddons.ElementAt(k);
                            packageAddons += _PackageAddons.SarPrice;
                        }
                        int familyMember = _dbContext.FamilyMembers
                                            .Include(e => e.FamilyHead)
                                            .Where(e => e.HeadId == _Family.FamilyHeadId)
                                            .Count();

                        float totalAddonsSar = familyMember * packageAddons;
                        var FamilyHeadAdditionalFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Package)
                                            .Where(e => e.FamilyHeadId == _Family.FamilyHeadId)
                                            .Select(e => new
                                            {
                                                e.AdditionalFacilities.AdultSar,
                                                e.AdditionalFacilities.ChildSar,
                                                e.AdditionalFacilities.InfantSar
                                            })
                                            .ToList();

                        float familyHeadAdditionalFacilities = 0;
                        for (int k = 0; k < FamilyHeadAdditionalFacilities.Count; k++)
                        {
                            var _FamilyHeadAdditionalFacilities = FamilyHeadAdditionalFacilities.ElementAt(k);
                            if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.InfantSar;
                            }
                            else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.ChildSar;
                            }
                            else
                            {
                                familyHeadAdditionalFacilities += (float)_FamilyHeadAdditionalFacilities.AdultSar;
                            }
                        }



                        if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 2)
                        {
                            totalRecivable += (float)_Family.InfantSar;
                        }
                        else if (DateTimeManager.CalculateAge(_Family.DateOfBirth) < 12)
                        {
                            totalRecivable += (float)_Family.ChildSar;
                        }
                        else
                        {
                            totalRecivable += (float)_Family.AdultSar;
                        }

                        totalRecivable += familyHeadAdditionalFacilities;
                        totalRecivable += totalAddonsSar;

                        float totalAmountSar = totalRecivable;
                        float totalRecivableSar = (totalRecivable - _Family.SarDiscountAmount);

                        float totalRecivablePkr = totalRecivableSar * _Family.SARROE;


                        var VoucherEntries = _dbContext.VoucherEntries
                                            .Include(e => e.PaymentVoucher)
                                            .Where(e => e.AccountId == _Family.AccountId)
                                            .Select(e => new
                                            {
                                                e.AccountId,
                                                e.Credit,
                                                e.PaymentVoucher.Currency,
                                                e.PaymentVoucher.SARROE,
                                                e.PaymentVoucher.USDROE,
                                                e.PaymentVoucher.PKRROE
                                            })
                                            .ToList();




                        for (int k = 0; k < VoucherEntries.Count; k++)
                        {
                            var _VoucherEntries = VoucherEntries.ElementAt(k);
                            if (_VoucherEntries.Currency == Currencies.PKR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.SAR)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit / _VoucherEntries.SARROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedUsd += (_VoucherEntries.Credit / _VoucherEntries.USDROE);
                            }
                            else if (_VoucherEntries.Currency == Currencies.USD)
                            {
                                totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                                totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                                totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            }
                            //else if (_VoucherEntries.Currency == Currencies.AED)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.GBP)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                            //else if (_VoucherEntries.Currency == Currencies.EURO)
                            //{
                            //    totalRecivedPkr += (_VoucherEntries.Credit * _VoucherEntries.PKRROE);
                            //    totalRecivedSar += (_VoucherEntries.Credit * _VoucherEntries.SARROE);
                            //    totalRecivedUsd += (_VoucherEntries.Credit * _VoucherEntries.USDROE);
                            //}
                        }

                        recivableSar = totalAmountSar - totalRecivedSar;
                        recivablePkr = recivableSar * rate.SARROE;
                        recivableUsd = recivablePkr * rate.USDROE;


                        var Invoice = _dbContext.Invoices
                                          .Join(
                                                  _dbContext.FamilyHeads,
                                                  invoice => invoice.FamilyHeadId,
                                                  family => family.FamilyHeadId,
                                                  (invoice, family) => new { Invoice = invoice, FamilyHead = family }
                                          )
                                          .Where(e => e.FamilyHead.AccountId == _Family.AccountId)
                                          .Select(e => new
                                          {
                                              e.Invoice.AmountRecived,
                                              e.Invoice.InvoiceStatus,
                                              e.Invoice.Currency,
                                              e.Invoice.PKRROE,
                                              e.Invoice.SARROE,
                                              e.Invoice.USDROE,
                                          })
                                          .ToList();

                        for (int k = 0; k < Invoice.Count; k++)
                        {

                            var _Invoice = Invoice.ElementAt(k);
                            if (_Invoice.InvoiceStatus != InvoiceStatus.Cleared)
                            {

                                if (_Invoice.Currency == Currencies.PKR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.SAR)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived / _Invoice.SARROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationUsd += (_Invoice.AmountRecived / _Invoice.USDROE);
                                }
                                else if (_Invoice.Currency == Currencies.USD)
                                {
                                    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                }
                                //else if (_Invoice.Currency == Currencies.AED)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.GBP)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                                //else if (_Invoice.Currency == Currencies.EURO)
                                //{
                                //    prerealizationPkr += (_Invoice.AmountRecived * _Invoice.PKRROE);
                                //    prerealizationSar += (_Invoice.AmountRecived * _Invoice.SARROE);
                                //    prerealizationUsd += (_Invoice.AmountRecived * _Invoice.USDROE);
                                //}
                            }


                        }


                        realizationSar = totalRecivedSar + prerealizationSar;
                        realizationPkr = totalRecivedPkr + prerealizationPkr;
                        realizationUsd = totalRecivedUsd + prerealizationUsd;


                        packageEntry.Entries.Add(new CollectionCompanyModelEntry
                        {
                            Company = _Company.CompanyCode,
                            FamilyHead = _Family.GivenName,
                            Spo = _Family.SPOName,
                            FCID = _Family.FcId,
                            Package = _Family.Name,
                            PackageInSAR = _Family.AdultSar,
                            AddonsInSAR = totalAddonsSar,
                            ADFInSAR = familyHeadAdditionalFacilities,
                            TotalRecivableSAR = totalRecivableSar,
                            BaseExchangeRate = _Family.SARROE,
                            TotalAmountSar = totalAmountSar,
                            TotalRecivablePKR = totalRecivablePkr,
                            TotalRecivedSar = totalRecivedSar,
                            TotalRecivedPkr = totalRecivedPkr,
                            TotalRecivedUsd = totalRecivedUsd,
                            ReceivableSar = recivableSar,
                            ReceivablePkr = recivablePkr,
                            ReceivableUsd = recivableUsd,
                            PrerealizationSar = prerealizationSar,
                            PrerealizationPkr = prerealizationPkr,
                            PrerealizationUsd = prerealizationUsd,
                            RealizationSar = realizationSar,
                            RealizationPkr = realizationPkr,
                            RealizationUsd = realizationUsd,
                        });
                    }







                    model.Add(packageEntry);

                }


            }
            return View("Collection", model);




        }

        public IActionResult FilterWise(string filter, int companyId, int packageId, int spoId)
        {

            string value = "";
            int filterId = 0;
            switch (filter)
            {
                case "company":
                    value = "CompanyWise";
                    filterId = companyId;
                    break;

                case "package":
                    value = "PackageWise";
                    filterId = packageId;
                    break;

                case "spo":
                    value = "SpoWise";
                    filterId = spoId;
                    break;
            }

            return RedirectToAction(value, new { id = filterId });




        }


    }
}

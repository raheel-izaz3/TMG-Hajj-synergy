using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;

namespace TMGHajj.Controllers.QurbaniContronllers
{
    public class QurbaniReportController : Controller
    {

        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public QurbaniReportController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult HajjiWiseSummary()
        {
            var members = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join fhe in _dbContext.FamilyHeadExtras on fh.FamilyHeadId equals fhe.FamilyHeadId into fheGroup
                           from fhe in fheGroup.DefaultIfEmpty()
                           join ex in _dbContext.ExtraFacilityDescriptions on fhe.ExtraFacilityDescriptionId equals ex.ExtraFacilityDescriptionId into exGroup
                           from ex in exGroup.DefaultIfEmpty()
                           where fh.Trashed == false && c.Trashed == false && p.Trashed == false
                                 && (fhe.ExtraFacilityDescriptionId == 1 || fhe.ExtraFacilityDescriptionId == null)
                                 && p.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                && fh.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                           select new
                           {
                               c.CompanyCode,
                               PackageName = p.Name,
                               HBNumber = fh.FcId,
                               FullName = string.Concat(fh.GivenName, " ", fh.SurName),
                               ex.Description,
                               fhe.PriceSar,
                               QurbaniType = fhe.PriceSar == 0 ? "Complimentary" :
                                             fhe.PriceSar > 0 ? "Paid" :
                                             (string)null
                           })
              .ToList();

            return View(members);
        }

        public IActionResult PackageWiseSummary()
        {
            var PackageWiseData = (
                from fh in _dbContext.FamilyHeads
                join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                join fhe in _dbContext.FamilyHeadExtras on fh.FamilyHeadId equals fhe.FamilyHeadId into fheGroup
                from fhe in fheGroup.DefaultIfEmpty() // Left join
                where !p.Trashed && !c.Trashed && !fh.Trashed 
                        && p.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && fh.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                group fhe by new { p.PackageId, c.CompanyCode, p.Name } into g
                select new
                {
                    g.Key.PackageId,
                    g.Key.CompanyCode,
                    g.Key.Name,
                    PaidCount = g.Count(fhe => fhe != null && fhe.PriceSar > 0),
                    ComplimentaryCount = g.Count(fhe => fhe != null && fhe.PriceSar == 0),
                    NoQurbaniCount = g.Count(fhe => fhe == null || fhe.PriceSar == null),
                    Cash = "-"
                }
            ).ToList();

            int TotalPaidCount = PackageWiseData.Sum(e => e.PaidCount);
            int TotalComplimentryCount = PackageWiseData.Sum(e => e.ComplimentaryCount);
            int TotalNoQurbaniCount = PackageWiseData.Sum(e => e.NoQurbaniCount);

            return View(new
            {
                PackageWiseData,
                TotalPaidCount,
                TotalComplimentryCount,
                TotalNoQurbaniCount
            });
        }
    }
}

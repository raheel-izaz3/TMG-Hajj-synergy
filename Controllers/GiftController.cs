using TMG.Data;
using TMG.Models;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers
{
    public class GiftController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        public GiftController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }
        public IActionResult Gift()
        {
            return View();
        }

        public IActionResult Suitcase()
        {
            return View();
        }
        public IActionResult SuitCasePackage()
        {
           
            var suitcases = _dbContext.Suitcases
                .Where(e => !e.Trashed);
            
            return View(suitcases);
            //return View();
        }

        public IActionResult SuitcaseCreate()
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
           
            return View();
        }


        [HttpPost]
        public IActionResult SuitcaseCreate(Models.Suitcase Suitcase)
        {

            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
           
            if (!ModelState.IsValid) return View();

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.Suitcases.Add(Suitcase);
                _dbContext.SaveChanges();

                for (int i = 0; i < (Suitcase.PackageIds?.Count ?? 0); i++)
                {
                    if (Suitcase.PackagesSelected[i] == false) continue;

                    _dbContext.SuitcasePackages.Add(new SuitcasePackages
                    {
                        PackageId = Suitcase.PackageIds[i],
                        SuitcaseId = Suitcase.SuitcaseId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(SuitCasePackage));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(Suitcase);
            }


            // _dbContext.Groups.Add(group);
            //_dbContext.SaveChanges();

            // return RedirectToAction(nameof(Index));
        }

        public IActionResult SuitcaseEdit(int id)
        {
            ViewBag.SuitcasePackages = _dbContext.SuitcasePackages.Where(e => e.SuitcaseId == id);
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
           
            Models.Suitcase Suitcase = _dbContext.Suitcases.Find(id);

            return View(Suitcase);
        }


        [HttpPost]
        public IActionResult SuitcaseEdit(Models.Suitcase Suitcase)
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
           
            if (!ModelState.IsValid) return View(Suitcase);


            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.Entry(Suitcase).State = EntityState.Modified;

                _dbContext.SaveChanges();
                _dbContext.SuitcasePackages.Where(e => e.SuitcaseId == Suitcase.SuitcaseId).ExecuteDelete();
                for (int i = 0; i < (Suitcase.PackageIds?.Count ?? 0); i++)
                {
                    if (Suitcase.PackagesSelected[i] == false) continue;

                    _dbContext.SuitcasePackages.Add(new SuitcasePackages
                    {
                        PackageId = Suitcase.PackageIds[i],
                        SuitcaseId = Suitcase.SuitcaseId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(SuitCasePackage));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(Suitcase);
            }

        }
        public IActionResult SubmitClick(string txtfcid)
        {
            string strErrormesssage = string.Empty;
            string strSuccessmesssage = string.Empty;
            try
            {
                var familyHeads = _dbContext.FamilyHeads.Where(fh => fh.FcId == "FC-"+txtfcid && !fh.Trashed).ToList();
                
                foreach (var fh in familyHeads)
                {
                    if(fh.GiftPack)
                    {
                        strErrormesssage = "Already Received GiftPack";
                        break;
                    }
                    else
                    {
                        fh.GiftPack = true;
                    }    
                }
                if(strErrormesssage == "")
                {
                    _dbContext.SaveChanges();
                    strSuccessmesssage = "Record Saved";
                }
                

            }
            catch (Exception ex) { };
            return Json(new { erromessage = strErrormesssage, message = strSuccessmesssage });

        }

        public IActionResult SubmitSuitcaseClick(string txtfcid)
        {
            string strErrormesssage = string.Empty;
            string strSuccessmesssage = string.Empty;
            try
            {
                var familyHeads = _dbContext.FamilyHeads.Where(fh => fh.FcId == "FC-" + txtfcid && !fh.Trashed).ToList();

                foreach (var fh in familyHeads)
                {
                    if (fh.SuitCase)
                    {
                        strErrormesssage = "Already Received GiftPack";
                        break;
                    }
                    else
                    {
                        fh.SuitCase = true;
                    }
                }
                if (strErrormesssage == "")
                {
                    _dbContext.SaveChanges();
                    strSuccessmesssage = "Record Saved";
                }


            }
            catch (Exception ex) { };
            return Json(new { erromessage = strErrormesssage, message = strSuccessmesssage });

        }
    }
}

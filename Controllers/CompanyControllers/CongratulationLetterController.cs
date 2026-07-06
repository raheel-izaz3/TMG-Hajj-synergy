using TMG.Data;
using TMG.Models.CompanyModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace TMG.Controllers.CompanyControllers
{
    public class CongratulationLetterController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CongratulationLetterController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult CongratulationLetter()
        {
            ViewBag.TrashCount = _dbContext.CongratulationLetters.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashGift(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateGift(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditGift(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreGift(_dbContext);

            List<CongratulationLetterIndexModel> model = new();
            var CongratulationLetters = _dbContext.CongratulationLettersWithoutTrash.ToList();
            var CongratulationLetterPackages = _dbContext.CongratulationLetterPackages.Select(e => new
            {
                e.Package.Name,
                e.CongratulationLetterId
            }).ToList();

            for (int i = 0; i < CongratulationLetters.Count; i++)
            {
                CongratulationLetter _ = CongratulationLetters.ElementAt(i);
                CongratulationLetterIndexModel subModel = new()
                {
                    SNo = i + 1,
                    CongratulationLetterId = _.CongratulationLetterId,
                };

                var currPackages = CongratulationLetterPackages.Where(e => e.CongratulationLetterId == _.CongratulationLetterId).ToList();
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

            if (_dbContext.CongratulationLetters.Count() > 0)
            {
                CongratulationLetter agreement = new CongratulationLetter();
                agreement.LetterContent = _dbContext.CongratulationLetters.FirstOrDefault().LetterContent;

                return View(agreement);
            }


            return View();
        }

        [HttpPost]
        public IActionResult Create(CongratulationLetter request)
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

            request.LetterContent = request.LetterContent ?? "";
            _dbContext.CongratulationLetters.Add(request);
            _dbContext.SaveChanges();

            for (int i = 0; i < request.PackageIds.Count; i++)
            {
                if (request.PackagesSelected[i] == false) continue;
                _dbContext.CongratulationLetterPackages.Add(new CongratulationLetterPackages
                {
                    PackageId = request.PackageIds[i],
                    CongratulationLetterId = request.CongratulationLetterId
                });
            }
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(CongratulationLetter));
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

            ViewBag.AgreementPackages = _dbContext.CongratulationLetterPackages
                .Where(a => a.CongratulationLetterId == id)
                .ToList();

            CongratulationLetter agreement = _dbContext.CongratulationLetters.Find(id);

            return View(agreement);
        }

        [HttpPost]
        public IActionResult Edit(CongratulationLetter request)
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

            ViewBag.AgreementPackages = _dbContext.CongratulationLetterPackages
                .Where(a => a.CongratulationLetterId == request.CongratulationLetterId)
                .ToList();

            if (!ModelState.IsValid) return View(request);

            var agreements = _dbContext.CongratulationLetters.ToList();

            CongratulationLetter agreement = _dbContext.CongratulationLetters.Find(request.CongratulationLetterId);


            agreement.UpdatedAt = DateTime.Now;
            
            agreement.LetterContent = request.LetterContent ?? "";

            _dbContext.SaveChanges();

            _dbContext.CongratulationLetterPackages.Where(e => e.CongratulationLetterId == request.CongratulationLetterId).ExecuteDelete();
            for (int i = 0; i < request.PackageIds.Count; i++)
            {
                if (request.PackagesSelected[i] == false) continue;

                _dbContext.CongratulationLetterPackages.Add(new CongratulationLetterPackages
                {
                    PackageId = request.PackageIds[i],
                    CongratulationLetterId = request.CongratulationLetterId
                });
            }
            _dbContext.SaveChanges();

            var agreementsAfterUpdate = _dbContext.CongratulationLetterPackages.Where(e => e.CongratulationLetterId == request.CongratulationLetterId).ToList();
            for (int i = 0; i < agreementsAfterUpdate.Count; i++)
            {
                var currAgreement = agreementsAfterUpdate.ElementAt(i);
                CongratulationLetter _ = _dbContext.CongratulationLetters.FirstOrDefault(e => e.CongratulationLetterId == currAgreement.CongratulationLetterId);
                _.LetterContent = request.LetterContent;
            }

            _dbContext.SaveChanges();

            return RedirectToAction("CongratulationLetter");
        }

        public IActionResult ViewTrash()
        {
            var trashItems = _dbContext.CongratulationLetters
                                         .Where(e => e.Trashed)
                                         .Select(cl => new CongratulationLetterIndexModel
                                         {
                                             CongratulationLetterId = cl.CongratulationLetterId,
                                             Packages = _dbContext.CongratulationLetterPackages
                                                            .Where(clp => clp.CongratulationLetterId == cl.CongratulationLetterId)
                                                            .Select(clp => clp.Package.Name)
                                                            .ToList()
                                         })
                                         .ToList();

            return View(trashItems);
        }


        public IActionResult Trash(int id)
        {
            CongratulationLetter agreement = _dbContext.CongratulationLetters.Find(id)!;
            agreement.Trashed = true;

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(CongratulationLetter));
        }
        public IActionResult Restore(int id)
        {
            CongratulationLetter agreement = _dbContext.CongratulationLetters.Find(id)!;
            agreement.Trashed = false;

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(CongratulationLetter));
        }


    }
}

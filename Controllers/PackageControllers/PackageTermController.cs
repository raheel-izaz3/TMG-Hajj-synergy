using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.PackageModels;

namespace TMG.Controllers.PackageControllers
{
    public class PackageTermController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public PackageTermController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.PackageTerms
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.Trashed).Count();

            /*ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);*/

            return View(_dbContext.PackageTermsWithoutTrash.Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(PackageTerms terms)
        {
            if (!ModelState.IsValid) return View();

            terms.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.PackageTerms.Add(terms);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            PackageTerms terms = _dbContext.PackageTerms.Find(id);

            return View(terms);
        }


        [HttpPost]
        public IActionResult Edit(PackageTerms terms)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(terms).State = EntityState.Modified;
            terms.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.PackageTerms.Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            PackageTerms terms = _dbContext.PackageTerms.Find(id);
            if (terms == null) return RedirectToAction(nameof(Index));

            terms.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            PackageTerms terms  = _dbContext.PackageTerms.Find(id);
            terms.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

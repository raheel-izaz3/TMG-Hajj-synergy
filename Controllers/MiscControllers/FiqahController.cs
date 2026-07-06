using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class FiqahController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public FiqahController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Fiqahs.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.FiqahsWithoutTrash.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Fiqah fiqah)
        {
            if (!ModelState.IsValid) return View();

            fiqah.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Fiqahs.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Fiqah Fiqah = _dbContext.Fiqahs.Find(id);

            return View(Fiqah);
        }


        [HttpPost]
        public IActionResult Edit(Fiqah fiqah)
        {
            if (!ModelState.IsValid) return View();

            fiqah.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Fiqahs.Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            Fiqah fiqah = _dbContext.Fiqahs.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Fiqah fiqah = _dbContext.Fiqahs.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

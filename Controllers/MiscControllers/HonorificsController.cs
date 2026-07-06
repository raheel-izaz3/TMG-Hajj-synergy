using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class HonorificsController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public HonorificsController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Honorifics.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashHonorifics(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateHonorifics(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditHonorifics(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreHonorifics(_dbContext, _http);

            return View(_dbContext.HonorificsWithoutTrash.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Honorifics honorifics)
        {
            if (!ModelState.IsValid) return View();

            honorifics.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Honorifics.Add(honorifics);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Honorifics Honorifics = _dbContext.Honorifics.Find(id);

            return View(Honorifics);
        }


        [HttpPost]
        public IActionResult Edit(Honorifics honorifics)
        {
            if (!ModelState.IsValid) return View();

            honorifics.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(honorifics).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Honorifics.Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            Honorifics honorifics = _dbContext.Honorifics.Find(id);
            if (honorifics == null) return RedirectToAction(nameof(Index));

            honorifics.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Honorifics honorifics = _dbContext.Honorifics.Find(id);
            honorifics.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

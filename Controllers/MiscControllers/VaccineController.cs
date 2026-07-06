using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class VaccineController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public VaccineController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Vaccines.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashVaccine(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateVaccine(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditVaccine(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreVaccine(_dbContext, _http);

            return View(_dbContext.VaccinesWithoutTrash.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Vaccine vaccine)
        {
            if (!ModelState.IsValid) return View();

            vaccine.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.Vaccines.Add(vaccine);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Vaccine Vaccine = _dbContext.Vaccines.Find(id);

            return View(Vaccine);
        }


        [HttpPost]
        public IActionResult Edit(Vaccine vaccine)
        {
            if (!ModelState.IsValid) return View();

            vaccine.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(vaccine).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Vaccines.Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            Vaccine vaccine = _dbContext.Vaccines.Find(id);
            if (vaccine == null) return RedirectToAction(nameof(Index));

            vaccine.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Vaccine vaccine = _dbContext.Vaccines.Find(id);
            vaccine.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.SettingsModels;

namespace TMG.Controllers.MiscControllers
{
    public class HajjSeasonController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public HajjSeasonController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.HajjSeasons.Where(e => e.Trashed).Count();


            return View(_dbContext.HajjSeasons.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(HajjSeasons fiqah)
        {
            if (!ModelState.IsValid)
                return View();

            _dbContext.HajjSeasons.Add(fiqah);
            _dbContext.SaveChanges();

            var applicationSettings = new ApplicationSettings
            {
                SARROE = 0,
                USDROE = 0,
                VaccineCount = 0,
                FamilyCount = 0,
                IndividualCount = 0,
                EnableQurbani = false,
                HajjSeasonsId = fiqah.HajjSeasonsId
            };

            _dbContext.ApplicationSettings.Add(applicationSettings);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            HajjSeasons Fiqah = _dbContext.HajjSeasons.Find(id);

            return View(Fiqah);
        }


        [HttpPost]
        public IActionResult Edit(HajjSeasons fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.HajjSeasons.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            HajjSeasons fiqah = _dbContext.HajjSeasons.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            HajjSeasons fiqah = _dbContext.HajjSeasons.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

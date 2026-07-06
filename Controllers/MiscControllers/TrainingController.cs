using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class TrainingController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public TrainingController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Trainings.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashTraining(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateTraining(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditTraining(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreTraining(_dbContext, _http);

            return View(_dbContext.TrainingsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Training training)
        {
            if (!ModelState.IsValid) return View();

            //training.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Trainings.Add(training);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Training Training = _dbContext.Trainings.Find(id);

            return View(Training);
        }


        [HttpPost]
        public IActionResult Edit(Training training)
        {
            if (!ModelState.IsValid) return View();

            //training.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(training).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Trainings.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Training training = _dbContext.Trainings.Find(id);
            if (training == null) return RedirectToAction(nameof(Index));

            training.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Training training = _dbContext.Trainings.Find(id);
            training.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

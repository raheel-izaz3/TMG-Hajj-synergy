using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class NomineeRelationController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public NomineeRelationController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.NomineeRelations.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanCreateNominee(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateNominee(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditNominee(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreNominee(_dbContext, _http);

            return View(_dbContext.NomineeRelationsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(NomineeRelation nomineeRelation)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.NomineeRelations.Add(nomineeRelation);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            NomineeRelation NomineeRelation = _dbContext.NomineeRelations.Find(id);

            return View(NomineeRelation);
        }


        [HttpPost]
        public IActionResult Edit(NomineeRelation nomineeRelation)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(nomineeRelation).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.NomineeRelations.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            NomineeRelation nomineeRelation = _dbContext.NomineeRelations.Find(id);
            if (nomineeRelation == null) return RedirectToAction(nameof(Index));

            nomineeRelation.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            NomineeRelation nomineeRelation = _dbContext.NomineeRelations.Find(id);
            nomineeRelation.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

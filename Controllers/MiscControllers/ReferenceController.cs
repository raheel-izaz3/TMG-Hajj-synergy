using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class ReferenceController: Controller
    {
        private readonly TMGDbContext _dbContext;

        private readonly IHttpContextAccessor _http;

        public ReferenceController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.References.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashReference(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateReference(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditReference(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreReference(_dbContext, _http);

            return View(_dbContext.ReferencesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Reference reference)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.References.Add(reference);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Reference reference = _dbContext.References.Find(id);

            return View(reference);
        }


        [HttpPost]
        public IActionResult Edit(Reference reference)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(reference).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.References.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Reference reference = _dbContext.References.Find(id);
            if (reference == null) return RedirectToAction(nameof(Index));

            reference.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Reference reference = _dbContext.References.Find(id);
            reference.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class MehramRelationController: Controller
    {
        private readonly TMGDbContext _dbContext;

        private readonly IHttpContextAccessor _http;
        public MehramRelationController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.MehramRelations.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashMehramRel(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateMehramRel(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditMehramRel(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreMehramRel(_dbContext, _http);

            return View(_dbContext.MehramRelationsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(MehramRelation MehramRelation)
        {
            ModelState.Remove("Code");
            if (!ModelState.IsValid) return View();

            _dbContext.MehramRelations.Add(MehramRelation);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            MehramRelation MehramRelation = _dbContext.MehramRelations.Find(id);

            return View(MehramRelation);
        }


        [HttpPost]
        public IActionResult Edit(MehramRelation MehramRelation)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(MehramRelation).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.MehramRelations.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            MehramRelation MehramRelation = _dbContext.MehramRelations.Find(id);
            if (MehramRelation == null) return RedirectToAction(nameof(Index));

            MehramRelation.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            MehramRelation MehramRelation = _dbContext.MehramRelations.Find(id);
            MehramRelation.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

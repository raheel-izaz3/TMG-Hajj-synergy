using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class FamilyHeadRelationController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public FamilyHeadRelationController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.FamilyHeadRelations.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFamilyHeadRel(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFamilyHeadRel(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFamilyHeadRel(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFamilyHeadRel(_dbContext, _http);

            return View(_dbContext.FamilyHeadRelationsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(FamilyHeadRelation familyHead)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.FamilyHeadRelations.Add(familyHead);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            FamilyHeadRelation FamilyHead = _dbContext.FamilyHeadRelations.Find(id);

            return View(FamilyHead);
        }


        [HttpPost]
        public IActionResult Edit(FamilyHeadRelation familyHead)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(familyHead).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.FamilyHeadRelations.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            FamilyHeadRelation familyHead = _dbContext.FamilyHeadRelations.Find(id);
            if (familyHead == null) return RedirectToAction(nameof(Index));

            familyHead.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            FamilyHeadRelation familyHead = _dbContext.FamilyHeadRelations.Find(id);
            familyHead.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

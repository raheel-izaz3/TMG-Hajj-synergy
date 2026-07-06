using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class SPOController: Controller
    {
        private readonly TMGDbContext _dbContext;

        public SPOController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.SPOs.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashReqDocs(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateReqDocs(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditReqDocs(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreReqDocs(_dbContext);

            return View(_dbContext.SPOsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(SPO gift)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.SPOs.Add(gift);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            SPO SPO = _dbContext.SPOs.Find(id);

            return View(SPO);
        }


        [HttpPost]
        public IActionResult Edit(SPO gift)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(gift).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.SPOs.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            SPO gift = _dbContext.SPOs.Find(id);
            if (gift == null) return RedirectToAction(nameof(Index));

            gift.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            SPO gift = _dbContext.SPOs.Find(id);
            gift.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

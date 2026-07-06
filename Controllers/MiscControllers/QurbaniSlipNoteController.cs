using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class QurbaniSlipNoteController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public QurbaniSlipNoteController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.QurbaniSlipNotes.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashQurbaniSlipNotes(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateQurbaniSlipNotes(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditQurbaniSlipNotes(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreQurbaniSlipNotes(_dbContext);

            return View(_dbContext.QurbaniSlipNotesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(QurbaniSlipNotes tc)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.QurbaniSlipNotes.Add(tc);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {

            QurbaniSlipNotes tc = _dbContext.QurbaniSlipNotes.Find(id);

            return View(tc);
        }


        [HttpPost]
        public IActionResult Edit(QurbaniSlipNotes tc)
        {

            if (!ModelState.IsValid) return View();

            _dbContext.Entry(tc).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.QurbaniSlipNotes.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            QurbaniSlipNotes tc = _dbContext.QurbaniSlipNotes.Find(id);
            if (tc == null) return RedirectToAction(nameof(Index));

            tc.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            QurbaniSlipNotes tc = _dbContext.QurbaniSlipNotes.Find(id);
            tc.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

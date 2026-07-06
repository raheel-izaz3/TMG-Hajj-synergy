using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class QurbaniSlipTCController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public QurbaniSlipTCController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.QurbaniSlipTCs.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashQurbaniSlipTC(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateQurbaniSlipTC(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditQurbaniSlipTC(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreQurbaniSlipTC(_dbContext);

            return View(_dbContext.QurbaniSlipTCsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(QurbaniSlipTC tc)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.QurbaniSlipTCs.Add(tc);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {

            QurbaniSlipTC tc = _dbContext.QurbaniSlipTCs.Find(id);

            return View(tc);
        }


        [HttpPost]
        public IActionResult Edit(QurbaniSlipTC tc)
        {

            if (!ModelState.IsValid) return View();

            _dbContext.Entry(tc).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.QurbaniSlipTCs.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            QurbaniSlipTC tc = _dbContext.QurbaniSlipTCs.Find(id);
            if (tc == null) return RedirectToAction(nameof(Index));

            tc.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            QurbaniSlipTC tc = _dbContext.QurbaniSlipTCs.Find(id);
            tc.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

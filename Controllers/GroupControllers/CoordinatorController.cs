using TMG.Data;
using TMG.Models.GroupModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.GroupControllers
{
    public class CoordinatorController : Controller
    {

        private readonly TMGDbContext _dbContext;

        public CoordinatorController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Coordinators.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashCoordinator(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateCoordinator(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditCoordinator(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCoordinator(_dbContext);

            return View(_dbContext.CoordinatorsWithoutTrash.Include(e => e.Group).Include(e => e.CoordinatorRole));
        }

        public IActionResult Create()
        {
            ViewBag.Roles = _dbContext.CoordinatorRolesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash;

            return View();
        }


        [HttpPost]
        public IActionResult Create(Coordinator coordinator)
        {
            ViewBag.Roles = _dbContext.CoordinatorRolesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash;

            if (!ModelState.IsValid) return View();

            _dbContext.Coordinators.Add(coordinator);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Roles = _dbContext.CoordinatorRolesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash;

            Coordinator Coordinator = _dbContext.Coordinators.Find(id);

            return View(Coordinator);
        }


        [HttpPost]
        public IActionResult Edit(Coordinator coordinator)
        {
            ViewBag.Roles = _dbContext.CoordinatorRolesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash;

            if (!ModelState.IsValid) return View();

            _dbContext.Entry(coordinator).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Coordinators.Include(e => e.Group).Include(e => e.CoordinatorRole).Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Coordinator Coordinator = _dbContext.Coordinators.Find(id);
            if (Coordinator == null) return RedirectToAction(nameof(Index));

            Coordinator.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Coordinator Coordinator = _dbContext.Coordinators.Find(id);
            Coordinator.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

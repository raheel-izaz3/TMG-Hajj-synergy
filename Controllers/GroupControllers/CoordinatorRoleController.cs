using TMG.Data;
using TMG.Models.GroupModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.GroupControllers
{
    public class CoordinatorRoleController : Controller
    {

        private readonly TMGDbContext _dbContext;

        public CoordinatorRoleController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CoordinatorRoles.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashCoordinator(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateCoordinator(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditCoordinator(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCoordinator(_dbContext);

            return View(_dbContext.CoordinatorRolesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(CoordinatorRole coordinatorRole)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.CoordinatorRoles.Add(coordinatorRole);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CoordinatorRole CoordinatorRole = _dbContext.CoordinatorRoles.Find(id);

            return View(CoordinatorRole);
        }


        [HttpPost]
        public IActionResult Edit(CoordinatorRole coordinatorRole)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(coordinatorRole).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CoordinatorRoles.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            CoordinatorRole coordinatorRole = _dbContext.CoordinatorRoles.Find(id);
            if (coordinatorRole == null) return RedirectToAction(nameof(Index));

            coordinatorRole.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            CoordinatorRole coordinatorRole = _dbContext.CoordinatorRoles.Find(id);
            coordinatorRole.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

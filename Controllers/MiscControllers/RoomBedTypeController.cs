using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class RoomBedTypeController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public RoomBedTypeController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RoomBedTypes.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashRoomBedType(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateRoomBedType(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditRoomBedType(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreRoomBedType(_dbContext);

            return View(_dbContext.RoomBedTypesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(RoomBedType bed)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.RoomBedTypes.Add(bed);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {

            RoomBedType bed = _dbContext.RoomBedTypes.Find(id);

            return View(bed);
        }


        [HttpPost]
        public IActionResult Edit(RoomBedType bed)
        {

            if (!ModelState.IsValid) return View();

            _dbContext.Entry(bed).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.RoomBedTypes.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            RoomBedType bed = _dbContext.RoomBedTypes.Find(id);
            if (bed == null) return RedirectToAction(nameof(Index));

            bed.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            RoomBedType bed = _dbContext.RoomBedTypes.Find(id);
            bed.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

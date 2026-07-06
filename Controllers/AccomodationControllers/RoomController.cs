using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.LogisticModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccomodationControllers
{
    public class RoomController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public RoomController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Rooms.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashRoom(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateRoom(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditRoom(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreRoom(_dbContext, _http);

            return View(_dbContext.RoomsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Room room)
        {
            if (!ModelState.IsValid) return View();
            //room.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Rooms.Add(room);
            _dbContext.SaveChanges();


            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Room Room = _dbContext.Rooms.Find(id);

            return View(Room);
        }


        [HttpPost]
        public IActionResult Edit(Room room)
        {
            if (!ModelState.IsValid) return View();
            //room.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.Entry(room).State = EntityState.Modified;

            _dbContext.SaveChanges();


            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Rooms.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Room room = _dbContext.Rooms.Find(id);
            if (room == null) return RedirectToAction(nameof(Index));

            room.Trashed = true;
            _dbContext.SaveChanges();


            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Room room = _dbContext.Rooms.Find(id);
            room.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

    }
}

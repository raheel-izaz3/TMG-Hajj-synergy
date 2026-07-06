using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class RoomViewController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public RoomViewController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RoomViews.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashRoomView(_dbContext, _http);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateRoomView(_dbContext, _http);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditRoomView(_dbContext, _http);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreRoomView(_dbContext, _http);

            return View(_dbContext.RoomViewsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(RoomView view)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.RoomViews.Add(view);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            RoomView RoomView = _dbContext.RoomViews.Find(id);

            return View(RoomView);
        }


        [HttpPost]
        public IActionResult Edit(RoomView view)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(view).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.RoomViews.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            RoomView view = _dbContext.RoomViews.Find(id);
            if (view == null) return RedirectToAction(nameof(Index));

            view.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            RoomView view = _dbContext.RoomViews.Find(id);
            view.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

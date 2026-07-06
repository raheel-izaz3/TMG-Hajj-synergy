using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class GiftController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public GiftController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Gifts.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashGift(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateGift(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditGift(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreGift(_dbContext, _http);

            return View(_dbContext.GiftsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Gift gift)
        {
            if (!ModelState.IsValid) return View();

            //gift.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Gifts.Add(gift);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Gift Gift = _dbContext.Gifts.Find(id);

            return View(Gift);
        }


        [HttpPost]
        public IActionResult Edit(Gift gift)
        {
            if (!ModelState.IsValid) return View();

            //gift.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(gift).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Gifts.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Gift gift = _dbContext.Gifts.Find(id);
            if (gift == null) return RedirectToAction(nameof(Index));

            gift.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Gift gift = _dbContext.Gifts.Find(id);
            gift.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

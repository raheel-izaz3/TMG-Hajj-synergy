using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class TehsilController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public TehsilController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Tehsils.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashTehsil(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateTehsil(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditTehsil(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreTehsil(_dbContext, _http);

            return View(_dbContext.TehsilsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Tehsil tehsil)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Tehsils.Add(tehsil);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Tehsil Tehsil = _dbContext.Tehsils.Find(id);

            return View(Tehsil);
        }


        [HttpPost]
        public IActionResult Edit(Tehsil tehsil)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(tehsil).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Tehsils.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Tehsil tehsil = _dbContext.Tehsils.Find(id);
            if (tehsil == null) return RedirectToAction(nameof(Index));

            tehsil.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Tehsil tehsil = _dbContext.Tehsils.Find(id);
            tehsil.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

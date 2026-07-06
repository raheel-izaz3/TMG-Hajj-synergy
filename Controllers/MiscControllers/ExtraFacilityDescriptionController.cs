using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class ExtraFacilityDescriptionController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public ExtraFacilityDescriptionController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.ExtraFacilityDescriptions.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashExtraFacilityDescription(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateExtraFacilityDescription(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditExtraFacilityDescription(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreExtraFacilityDescription(_dbContext);

            return View(_dbContext.ExtraFacilityDescriptionsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(ExtraFacilityDescription desc)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.ExtraFacilityDescriptions.Add(desc);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ExtraFacilityDescription ExtraFacilityDescription = _dbContext.ExtraFacilityDescriptions.Find(id);

            return View(ExtraFacilityDescription);
        }


        [HttpPost]
        public IActionResult Edit(ExtraFacilityDescription desc)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(desc).State = EntityState.Modified;
            desc.UpdatedAt = DateTime.Now;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.ExtraFacilityDescriptions.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            ExtraFacilityDescription desc = _dbContext.ExtraFacilityDescriptions.Find(id);
            if (desc == null) return RedirectToAction(nameof(Index));

            desc.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            ExtraFacilityDescription desc = _dbContext.ExtraFacilityDescriptions.Find(id);
            desc.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

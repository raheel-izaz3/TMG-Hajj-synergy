using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.MiscControllers
{
    public class IndividualFacilityDescriptionController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public IndividualFacilityDescriptionController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.IndividualFacilityDescriptions
                .Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashIndividualFacilityDescription(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateIndividualFacilityDescription(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditIndividualFacilityDescription(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreIndividualFacilityDescription(_dbContext);

            return View(_dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Include(e => e.Place));
        }

        public IActionResult Create()
        {
            ViewBag.RoomId = _dbContext.Rooms
                .ToList();
            ViewBag.PlaceId = _dbContext.Places.ToList();
            ViewBag.AccomodationId = _dbContext.Accomodations.ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(IndividualFacilityDescription desc, bool IsSharingB)
        {
            ViewBag.RoomId = _dbContext.Rooms
                //.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();
            ViewBag.PlaceId = _dbContext.Places.ToList();
            ViewBag.AccomodationId = _dbContext.Accomodations.ToList();

            if (!ModelState.IsValid) return View();

            //desc.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            desc.IsSharing = IsSharingB;

            _dbContext.IndividualFacilityDescriptions.Add(desc);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            IndividualFacilityDescription IndividualFacilityDescription = _dbContext.IndividualFacilityDescriptions.Find(id);

            ViewBag.RoomId = _dbContext.Rooms
                .ToList();
            ViewBag.PlaceId = _dbContext.Places.ToList();
            ViewBag.AccomodationId = _dbContext.Accomodations.ToList();

            return View(IndividualFacilityDescription);
        }


        [HttpPost]
        public IActionResult Edit(IndividualFacilityDescription desc, bool IsSharingB)
        {
            ViewBag.RoomId = _dbContext.Rooms
                .ToList();
            ViewBag.PlaceId = _dbContext.Places.ToList();
            ViewBag.AccomodationId = _dbContext.Accomodations.ToList();

            if (!ModelState.IsValid) return View();

            desc.IsSharing = IsSharingB;
            //desc.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(desc).State = EntityState.Modified;
            desc.UpdatedAt = DateTime.Now;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.IndividualFacilityDescriptions.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            IndividualFacilityDescription desc = _dbContext.IndividualFacilityDescriptions.Find(id);
            if (desc == null) return RedirectToAction(nameof(Index));

            desc.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            IndividualFacilityDescription desc = _dbContext.IndividualFacilityDescriptions.Find(id);
            desc.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

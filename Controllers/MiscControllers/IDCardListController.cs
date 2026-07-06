using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class IDCardListController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public IDCardListController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.IDCardLists.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.IDCardLists.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var assignedFamilyHeadIds = _dbContext.ListFamilies
                .Select(lf => lf.FamilyHeadId)
                .ToList();

            ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == activeSeasonId && e.IsHead && !assignedFamilyHeadIds.Contains(e.FamilyHeadId)
                        && e.HajjiBookingStatus == Models.FamilyModels.HajjiBookingStatus.Booked)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(IDCardList fiqah)
        {
            if (!ModelState.IsValid) return View(fiqah);

            _dbContext.IDCardLists.Add(fiqah);
            _dbContext.SaveChanges();

            for (int i = 0; i < (fiqah.FamilyHeadIds?.Count ?? 0); i++)
            {
                if (fiqah.FamilyHeadSelected[i] == false) continue;

                _dbContext.ListFamilies.Add(new ListFamily
                {
                    FamilyHeadId = fiqah.FamilyHeadIds[i],
                    IDCardListId = fiqah.IDCardListId
                });
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var Fiqah = _dbContext.IDCardLists.Find(id);

            var selectedFamilyHeadIds = _dbContext.ListFamilies
                .Where(e => e.IDCardListId == id)
                .Select(e => e.FamilyHeadId)
                .ToList();

            ViewBag.SelectedFamilyHeads = selectedFamilyHeadIds;

            ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == activeSeasonId && e.IsHead &&
                    (
                        _dbContext.ListFamilies.Any(lf => lf.FamilyHeadId == e.FamilyHeadId && lf.IDCardListId == id) ||

                        !_dbContext.ListFamilies.Any(lf => lf.FamilyHeadId == e.FamilyHeadId)
                    ) && e.HajjiBookingStatus == Models.FamilyModels.HajjiBookingStatus.Booked)
                .ToList();

            return View(Fiqah);
        }

        [HttpPost]
        public IActionResult Edit(IDCardList fiqah)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(fiqah).State = EntityState.Modified;
            _dbContext.SaveChanges();

            _dbContext.ListFamilies.Where(e => e.IDCardListId == fiqah.IDCardListId).ExecuteDelete();

            for (int i = 0; i < (fiqah.FamilyHeadIds?.Count ?? 0); i++)
            {
                if (fiqah.FamilyHeadSelected[i] == false) continue;

                _dbContext.ListFamilies.Add(new ListFamily
                {
                    FamilyHeadId = fiqah.FamilyHeadIds[i],
                    IDCardListId = fiqah.IDCardListId
                });
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.IDCardLists.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            IDCardList fiqah = _dbContext.IDCardLists.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            IDCardList fiqah = _dbContext.IDCardLists.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Print(int id)
        {
            var query = _dbContext.FamilyMembers
                    .Include(fm => fm.FamilyHead)
                        .ThenInclude(fh => fh.Package)
                    .Include(fm => fm.FamilyHead)
                        .ThenInclude(fh => fh.Nationalities)
                    .Join(_dbContext.ListFamilies,
                          fm => fm.HeadId,
                          fl => fl.FamilyHeadId,
                          (fm, fl) => new { fm, fl })
                    .Where(x => x.fl.IDCardListId == id
                            && !x.fm.FamilyHead.Trashed)
                    .Select(x => new
                    {
                        FamilyMember = x.fm,
                        FamilyHead = x.fm.FamilyHead,
                        Package = x.fm.FamilyHead.Package,
                        ListFamily = x.fl,
                        Nationality = x.fm.FamilyHead.Nationalities
                    });

            var result = query.Select(e => new IDCardLists
            {
                FullName = e.FamilyMember.FamilyHead.FullName,
                HBNo = e.FamilyMember.FamilyHead.FcId,
                Gender = e.FamilyMember.FamilyHead.Gender.ToString(),
                PassportNumber = e.FamilyMember.FamilyHead.PassportNumber,
                PackageName = e.FamilyMember.FamilyHead.Package.Name,
                Nationality = e.FamilyMember.FamilyHead.Nationalities.Name,
                CompanyName = e.FamilyMember.FamilyHead.CompanyVisas != null
                    ? e.FamilyMember.FamilyHead.CompanyVisas.CompanyName
                    : "N/A",
                Color = e.FamilyMember.FamilyHead.Package.Color
            }).ToList();

            return View(result);
        }
    }

    public record IDCardLists
    {
        public  string CompanyName { get; set; }
        public string FullName { get; set; }
        public string HBNo { get; set; }
        public string PackageName { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
        public string Color { get; set; }
    }
}

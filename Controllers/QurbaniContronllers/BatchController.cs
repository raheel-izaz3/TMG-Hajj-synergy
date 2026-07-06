using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Controllers.MiscControllers;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.MiscModels;
using TMG.Models.QurbaniModels;

namespace TMG.Controllers.QurbaniContronllers
{
    public class BatchController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public BatchController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Batches.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.Batches.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var assignedFamilyHeadIds = _dbContext.BatchLists
                .Select(lf => lf.FamilyHeadId)
                .ToList();

            //ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
            //    .Where(e => e.HajjSeasonsId == activeSeasonId && e.IsHead && !assignedFamilyHeadIds.Contains(e.FamilyHeadId)
            //            && e.HajjiBookingStatus == Models.FamilyModels.HajjiBookingStatus.Booked)
            //    .ToList();

            ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == activeSeasonId
                         && !assignedFamilyHeadIds.Contains(e.FamilyHeadId)
                         && e.HajjiBookingStatus == Models.FamilyModels.HajjiBookingStatus.Booked)
                .Join(_dbContext.FamilyHeadExtras,
                      fh => fh.FamilyHeadId,
                      fhe => fhe.FamilyHeadId,
                      (fh, fhe) => new { fh, fhe })
                .Join(_dbContext.ExtraFacilityDescriptions,
                      temp => temp.fhe.ExtraFacilityDescriptionId,
                      eadf => eadf.ExtraFacilityDescriptionId,
                      (temp, eadf) => new
                      {
                          FamilyHead = temp.fh,
                          FamilyHeadExtra = temp.fhe,
                          ExtraFacilityDescription = eadf
                      })
                .Where(e => e.ExtraFacilityDescription.ExtraFacilityDescriptionId == 1)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Batch fiqah)
        {
            if (!ModelState.IsValid) return View(fiqah);

            fiqah.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Batches.Add(fiqah);
            _dbContext.SaveChanges();

            for (int i = 0; i < (fiqah.FamilyHeadIds?.Count ?? 0); i++)
            {
                if (fiqah.FamilyHeadSelected[i] == false) continue;

                _dbContext.BatchLists.Add(new BatchList
                {
                    FamilyHeadId = fiqah.FamilyHeadIds[i],
                    BatchId = fiqah.BatchId
                });
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var Fiqah = _dbContext.Batches.Find(id);

            var selectedFamilyHeadIds = _dbContext.BatchLists
                .Where(e => e.BatchId == id)
                .Select(e => e.FamilyHeadId)
                .ToList();

            ViewBag.SelectedFamilyHeads = selectedFamilyHeadIds;

            ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == activeSeasonId &&
                    (
                        _dbContext.BatchLists.Any(lf => lf.FamilyHeadId == e.FamilyHeadId && lf.BatchId == id) ||

                        !_dbContext.BatchLists.Any(lf => lf.FamilyHeadId == e.FamilyHeadId)
                    ) && e.HajjiBookingStatus == Models.FamilyModels.HajjiBookingStatus.Booked)
                .Join(_dbContext.FamilyHeadExtras,
                      fh => fh.FamilyHeadId,
                      fhe => fhe.FamilyHeadId,
                      (fh, fhe) => new { fh, fhe })
                .Join(_dbContext.ExtraFacilityDescriptions,
                      temp => temp.fhe.ExtraFacilityDescriptionId,
                      eadf => eadf.ExtraFacilityDescriptionId,
                      (temp, eadf) => new
                      {
                          FamilyHead = temp.fh,
                          FamilyHeadExtra = temp.fhe,
                          ExtraFacilityDescription = eadf
                      })
                .Where(e => e.ExtraFacilityDescription.ExtraFacilityDescriptionId == 1)
                .ToList();

            return View(Fiqah);
        }

        [HttpPost]
        public IActionResult Edit(Batch fiqah)
        {
            if (!ModelState.IsValid) return View();

            fiqah.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(fiqah).State = EntityState.Modified;
            _dbContext.SaveChanges();

            _dbContext.BatchLists.Where(e => e.BatchId == fiqah.BatchId).ExecuteDelete();

            for (int i = 0; i < (fiqah.FamilyHeadIds?.Count ?? 0); i++)
            {
                if (fiqah.FamilyHeadSelected[i] == false) continue;

                _dbContext.BatchLists.Add(new BatchList
                {
                    FamilyHeadId = fiqah.FamilyHeadIds[i],
                    BatchId = fiqah.BatchId
                });
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Batches.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Batch fiqah = _dbContext.Batches.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Batch fiqah = _dbContext.Batches.Find(id);
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
                    .Join(_dbContext.BatchLists,
                          fm => fm.HeadId,
                          fl => fl.FamilyHeadId,
                          (fm, fl) => new { fm, fl })
                    .Where(x => x.fl.BatchId == id
                            && !x.fm.FamilyHead.Trashed)
                    .Select(x => new
                    {
                        FamilyMember = x.fm,
                        FamilyHead = x.fm.FamilyHead,
                        Package = x.fm.FamilyHead.Package,
                        BatchList = x.fl,
                        Nationality = x.fm.FamilyHead.Nationalities
                    });

            var result = query.Select(e => new Batches
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
            }).ToList();

            return View(result);
        }
    }

    public record Batches
    {
        public string CompanyName { get; set; }
        public string FullName { get; set; }
        public string HBNo { get; set; }
        public string PackageName { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
    }

}

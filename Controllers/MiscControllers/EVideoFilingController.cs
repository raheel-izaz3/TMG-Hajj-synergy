using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class EVideoFilingController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _environment;

        public EVideoFilingController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _http = http;
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.EVideoFilings.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.EVideoFilings
                .Include(e => e.MediaDocument)
                .Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Media = _dbContext.MediaDocuments
                .Where(e => !e.Trashed)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(EVideoFiling fiqah, IFormFile videoFile)
        {
            if (!ModelState.IsValid) return View();

            if (videoFile != null && videoFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Videos");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(videoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    videoFile.CopyTo(fileStream);
                }

                fiqah.Video = "/Videos/" + fileName;
            }

            _dbContext.EVideoFilings.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Media = _dbContext.MediaDocuments
                .Where(e => !e.Trashed)
                .ToList();

            EVideoFiling Fiqah = _dbContext.EVideoFilings.Find(id);

            return View(Fiqah);
        }

        [HttpPost]
        public IActionResult Edit(EVideoFiling fiqah, IFormFile videoFile)
        {
            if (!ModelState.IsValid) return View(fiqah);

            var existingFiling = _dbContext.EVideoFilings.Find(fiqah.EVideoFilingId);
            if (existingFiling == null) return NotFound();

            existingFiling.Name = fiqah.Name;
            existingFiling.DocumentType = fiqah.DocumentType;
            existingFiling.MediaDocumentId = fiqah.MediaDocumentId;
            existingFiling.UpdatedAt = DateTime.Now;

            if (videoFile != null && videoFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingFiling.Video))
                {
                    var existingFilePath = Path.Combine(_environment.WebRootPath, existingFiling.Video.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Videos");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(videoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    videoFile.CopyTo(fileStream);
                }

                existingFiling.Video = "/Videos/" + fileName;
            }

            _dbContext.Entry(existingFiling).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Email(int id, string url)
        {
            ViewBag.Hajji = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead == true
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.Media = _dbContext.EVideoFilings
                .Where(e => !e.Trashed && e.EVideoFilingId == id)
                .Select(e => e.Video)
                .FirstOrDefault();

            return View();
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.EVideoFilings.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            EVideoFiling fiqah = _dbContext.EVideoFilings.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            EVideoFiling fiqah = _dbContext.EVideoFilings.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

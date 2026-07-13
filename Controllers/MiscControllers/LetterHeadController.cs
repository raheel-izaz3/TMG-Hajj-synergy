using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace TMG.Controllers.MiscControllers
{
    public class LetterHeadController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _environment;

        public LetterHeadController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _http = http;
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.LetterHeads.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.LetterHeads.Include(e => e.Company).Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Company = _dbContext.CompaniesWithoutTrash
                .ToList();

            ViewBag.Department = _dbContext.CompanyDepartments
                .Where(e => !e.Trashed 
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(LetterHead fiqah, IFormFile image, IFormFile footImage)
        {
            if (!ModelState.IsValid) return View();

            if (image != null && image.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "LetterHead");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                fiqah.Image = "/LetterHead/" + fileName;
            }

            if (footImage != null && footImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "LetterHead");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(footImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    footImage.CopyTo(fileStream);
                }

                fiqah.FootImage = "/LetterHead/" + fileName;
            }

            _dbContext.LetterHeads.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            LetterHead Fiqah = _dbContext.LetterHeads.Find(id);

            ViewBag.Company = _dbContext.CompaniesWithoutTrash
               .ToList();

            ViewBag.Department = _dbContext.CompanyDepartments
                .Where(e => !e.Trashed
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View(Fiqah);
        }


        [HttpPost]
        public IActionResult Edit(LetterHead fiqah, IFormFile image, IFormFile footImage)
        {
            if (!ModelState.IsValid) return View();

            if (image != null && image.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "LetterHead");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                fiqah.Image = "/LetterHead/" + fileName;
            }

            if (footImage != null && footImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "LetterHead");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(footImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    footImage.CopyTo(fileStream);
                }

                fiqah.FootImage = "/LetterHead/" + fileName;
            }

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.LetterHeads.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            LetterHead fiqah = _dbContext.LetterHeads.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            LetterHead fiqah = _dbContext.LetterHeads.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Print(int id)
        {
            var result = _dbContext.LetterHeads
                .Include(e => e.Company)
                .FirstOrDefault(e => e.LetterHeadId == id);

            return View(result);
        }
    }
}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class EIslamicController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _environment;

        public EIslamicController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _http = http;
            _environment = environment;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.EIslamics.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.EIslamics.Where(e => !e.Trashed).ToList());
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(EIslamic fiqah, IFormFile image)
        {
            if (!ModelState.IsValid) return View();

            if (image != null && image.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "EIslamic");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                fiqah.Image = "/EIslamic/" + fileName;
            }

            _dbContext.EIslamics.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int id)
        {
            EIslamic Fiqah = _dbContext.EIslamics.Find(id);

            return View(Fiqah);
        }
        [HttpPost]
        //public IActionResult Edit(EIslamic fiqah)
        //{
        //    if (!ModelState.IsValid) return View();

        //    _dbContext.Entry(fiqah).State = EntityState.Modified;

        //    _dbContext.SaveChanges();

        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost]
        public IActionResult Edit(EIslamic fiqah, IFormFile image)
        {
            if (!ModelState.IsValid)
                return View(fiqah);

            var existingFiqah = _dbContext.EIslamics.AsNoTracking().FirstOrDefault(e => e.EIslamicId == fiqah.EIslamicId);
            if (existingFiqah == null)
                return RedirectToAction(nameof(Index));

            if (image != null && image.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "EIslamic");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(image.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                fiqah.Image = "/EIslamic/" + fileName;
            }
            else
            {
                fiqah.Image = existingFiqah.Image; 
            }

            _dbContext.Entry(fiqah).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult ViewTrash()
        {
            return View(_dbContext.EIslamics.Where(s => s.Trashed));
        }
        public IActionResult Trash(int id)
        {
            EIslamic fiqah = _dbContext.EIslamics.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Restore(int id)
        {
            EIslamic fiqah = _dbContext.EIslamics.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Print(int id)
        {
            var result = _dbContext.EIslamics
                .FirstOrDefault(e => e.EIslamicId == id
                        && !e.Trashed);

            return View(result);
        }

    }
}

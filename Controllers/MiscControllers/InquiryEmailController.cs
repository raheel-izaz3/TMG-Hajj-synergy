using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class InquiryEmailController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _environment;

        public InquiryEmailController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _http = http;
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.InquiryEmails.Count(e => e.Trashed);

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.InquiryEmails.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(InquiryEmail fiqah, IFormFile file)
        {
            if (!ModelState.IsValid) return View();

            if (file != null && file.Length > 0)
            {
                if (!string.IsNullOrEmpty(fiqah.Attachment))
                {
                    var existingFilePath = Path.Combine(_environment.WebRootPath, fiqah.Attachment.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "InquiryEmail");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                fiqah.Attachment = "/InquiryEmail/" + fileName;
            }

            _dbContext.InquiryEmails.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            InquiryEmail Fiqah = _dbContext.InquiryEmails.Find(id);

            ViewBag.Country = _dbContext.Countries.Where(e => !e.Trashed).ToList();

            return View(Fiqah);
        }

        [HttpPost]
        public IActionResult Edit(InquiryEmail fiqah, IFormFile file)
        {
            if (!ModelState.IsValid) return View();

            var existingData = _dbContext.InquiryEmails.Find(fiqah.InquiryEmailId);
            if (existingData == null) return NotFound();

            existingData.NoOfDays = fiqah.NoOfDays;
            existingData.Type = fiqah.Type;
            existingData.Subject = fiqah.Subject;
            existingData.Message = fiqah.Message;

            if (file != null && file.Length > 0)
            {
                if (!string.IsNullOrEmpty(fiqah.Attachment))
                {
                    var existingFilePath = Path.Combine(_environment.WebRootPath, fiqah.Attachment.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "InquiryEmail");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                existingData.Attachment = "/InquiryEmail/" + fileName;
            }

            _dbContext.Entry(existingData).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.InquiryEmails.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            InquiryEmail fiqah = _dbContext.InquiryEmails.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            InquiryEmail fiqah = _dbContext.InquiryEmails.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

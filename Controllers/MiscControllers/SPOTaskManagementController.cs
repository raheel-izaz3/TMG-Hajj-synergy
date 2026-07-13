using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.FamilyModels;

namespace TMG.Controllers.MiscControllers
{
    public class SPOTaskManagementController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _environment;

        public SPOTaskManagementController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _http = http;
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.SPOTaskManagements.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.SPOTaskManagements.Include(e => e.AssignedSPO).Include(e => e.AssignedBy).Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            ViewBag.SPOs = _dbContext.SPOsWithoutTrash;

            return View();
        }


        [HttpPost]
        public IActionResult Create(SPOTaskManagement fiqah, IFormFile file)
        {
            if (!ModelState.IsValid) return View();

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "SPOTasks");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                fiqah.TaskFile = "/SPOTasks/" + fileName;
            }

            _dbContext.SPOTaskManagements.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            SPOTaskManagement Fiqah = _dbContext.SPOTaskManagements.Find(id);
            ViewBag.SPOs = _dbContext.SPOsWithoutTrash;

            return View(Fiqah);
        }


        [HttpPost]
        public IActionResult Edit(SPOTaskManagement fiqah, IFormFile file)
        {
            if (!ModelState.IsValid) return View();

            var existingFiling = _dbContext.SPOTaskManagements.Find(fiqah.SPOTaskManagementId);
            if (existingFiling == null) return NotFound();

            existingFiling.AssignedSPOId = fiqah.AssignedSPOId;
            existingFiling.AssignedById = fiqah.AssignedById;
            existingFiling.Task = fiqah.Task;
            existingFiling.DateOfAssigned = fiqah.DateOfAssigned;
            existingFiling.UpdatedAt = DateTime.Now;

            if (file != null && file.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingFiling.TaskFile))
                {
                    var existingFilePath = Path.Combine(_environment.WebRootPath, existingFiling.TaskFile.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "SPOTasks");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                existingFiling.TaskFile = "/SPOTasks/" + fileName;
            }

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.SPOTaskManagements.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            SPOTaskManagement fiqah = _dbContext.SPOTaskManagements.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            SPOTaskManagement fiqah = _dbContext.SPOTaskManagements.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

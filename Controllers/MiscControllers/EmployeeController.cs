using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class EmployeeController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _environment;

        public EmployeeController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _http = http;
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Employees.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.Employees.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Employee fiqah, IFormFile CNICFile, IFormFile FamilyCNIC, IFormFile CV, IFormFile OtherDoc, IFormFile AddressFile)
        {
            if (!ModelState.IsValid) return View();

            if (CNICFile != null && CNICFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(CNICFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    CNICFile.CopyTo(fileStream);
                }

                fiqah.CNICCopy = "/Employees/" + fileName;
            }

            if (FamilyCNIC != null && FamilyCNIC.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(FamilyCNIC.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    FamilyCNIC.CopyTo(fileStream);
                }

                fiqah.FamilyCNIC = "/Employees/" + fileName;
            }

            if (CV != null && CV.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(CV.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    CV.CopyTo(fileStream);
                }

                fiqah.CV = "/Employees/" + fileName;
            }

            if (OtherDoc != null && OtherDoc.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(OtherDoc.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    OtherDoc.CopyTo(fileStream);
                }

                fiqah.OtherDoc = "/Employees/" + fileName;
            }

            if (AddressFile != null && AddressFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(AddressFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    AddressFile.CopyTo(fileStream);
                }

                fiqah.AddressProof = "/Employees/" + fileName;
            }

            _dbContext.Employees.Add(fiqah);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Employee Fiqah = _dbContext.Employees.Find(id);

            return View(Fiqah);
        }


        [HttpPost]
        public IActionResult Edit(Employee fiqah, IFormFile CNICFile, IFormFile FamilyCNIC, IFormFile CV, IFormFile OtherDoc, IFormFile AddressFile)
        {
            if (!ModelState.IsValid) return View();

            if (CNICFile != null && CNICFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(CNICFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    CNICFile.CopyTo(fileStream);
                }

                fiqah.CNICCopy = "/Employees/" + fileName;
            }

            if (FamilyCNIC != null && FamilyCNIC.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(FamilyCNIC.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    FamilyCNIC.CopyTo(fileStream);
                }

                fiqah.FamilyCNIC = "/Employees/" + fileName;
            }

            if (CV != null && CV.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(CV.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    CV.CopyTo(fileStream);
                }

                fiqah.CV = "/Employees/" + fileName;
            }

            if (OtherDoc != null && OtherDoc.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(OtherDoc.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    OtherDoc.CopyTo(fileStream);
                }

                fiqah.OtherDoc = "/Employees/" + fileName;
            }

            if (AddressFile != null && AddressFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Employees");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(AddressFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(filePath)) { string dir_dt = Path.GetDirectoryName(filePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    AddressFile.CopyTo(fileStream);
                }

                fiqah.AddressProof = "/Employees/" + fileName;
            }

            _dbContext.Entry(fiqah).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Employees.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Employee fiqah = _dbContext.Employees.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Employee fiqah = _dbContext.Employees.Find(id);
            fiqah.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

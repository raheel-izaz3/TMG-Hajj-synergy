using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.MiscControllers
{
    public class DoctorController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public DoctorController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Doctors.Where(e => e.Trashed).Count();
            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashDoctor(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateDoctor(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditDoctor(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreDoctor(_dbContext);

            return View(_dbContext
                .DoctorsWithoutTrash
            .Include(e => e.Tehsil)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                );
        }

        public IActionResult Create()
        {
            ViewBag.Tehsils = _dbContext.TehsilsWithoutTrash;

            return View();
        }


        [HttpPost]
        public IActionResult Create(Doctor doctor)
        {
            ViewBag.Tehsils = _dbContext.TehsilsWithoutTrash;

            if (!ModelState.IsValid) return View();

            string path = Path.Combine(_host.WebRootPath, "Images",
                "DoctorImages", doctor.SealSignatureFile.FileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                doctor.SealSignatureFile.CopyTo(fs);
            }

            doctor.SealSignaturePath = "Images/DoctorImages/" + doctor.SealSignatureFile.FileName;
            doctor.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Doctors.Add(doctor);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {

            ViewBag.Tehsils = _dbContext.TehsilsWithoutTrash;

            Doctor doctor = _dbContext.Doctors.Find(id);

            return View(doctor);
        }


        [HttpPost]
        public IActionResult Edit(Doctor doctor)
        {
            ViewBag.Tehsils = _dbContext.TehsilsWithoutTrash;

            if (ModelState.ContainsKey("SealSignatureFile"))
            {
                ModelState["SealSignatureFile"].Errors.Clear();
                ModelState["SealSignatureFile"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            if (!ModelState.IsValid) return View(doctor);

            Doctor doc = _dbContext.Doctors.Find(doctor.DoctorId);

            doc.Name = doctor.Name;
            doc.HospitalName = doctor.HospitalName;
            doc.Landline = doctor.Landline;
            doc.CellNumber = doctor.CellNumber;
            doc.TehsilId = doctor.TehsilId;
            doc.UpdatedAt = DateTime.Now;
            doc.District = doctor.District;
            doc.RegistrationNumber = doctor.RegistrationNumber;
            doctor.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext
                .Doctors
                .Include(e => e.Tehsil)
                .Where(s => s.Trashed)
                );
        }

        public IActionResult Trash(int id)
        {
            Doctor doctor = _dbContext.Doctors.Find(id);
            if (doctor == null) return RedirectToAction(nameof(Index));

            doctor.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Doctor doctor = _dbContext.Doctors.Find(id);
            doctor.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangeSeal(int id)
        {
            return View(_dbContext.Doctors.FirstOrDefault(e => e.DoctorId == id));
        }


        [HttpPost]
        public IActionResult ChangeSeal(int DoctorId, IFormFile File)
        {
            Doctor doctor = _dbContext.Doctors.Find(DoctorId);

            // return Ok(new
            // {
            //     DoctorId, File, doctor
            // });

            string path = Path.Combine(_host.WebRootPath, "Images",
                "DoctorImages", File.FileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            doctor.SealSignaturePath = "Images/DoctorImages/" + File.FileName;

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public JsonResult Get(int id)
        {
            Doctor doc = _dbContext
                .Doctors
                .Include(e => e.Tehsil)
                .FirstOrDefault(e => e.DoctorId == id);

            return Json(doc);
        }

    }
}

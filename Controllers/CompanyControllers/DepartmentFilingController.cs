using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using TMG.Models.CompanyModels;

namespace TMG.Controllers.CompanyControllers
{
    public record DepartmentFilingModel
    {
        public int DepartmentFilingId { get; set; }
        public int CompanyId { get; set; }
        public int CompanyDepartmentId { get; set; }
        public DateTime Date { get; set; }
        public IFormFile ImagePath { get; set; }
        public string Note { get; set; }
    }

    public class DepartmentFilingController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public DepartmentFilingController(TMGDbContext context, IHttpContextAccessor http, IWebHostEnvironment host)
        {
            _dbContext = context;
            _http = http;
            _host = host;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.DepartmentFilings.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.DepartmentFilings
                            .Include(e => e.CompanyDepartment)
                            .Include(e => e.Company)
                            .OrderByDescending(e => e.Date)
                            .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Departments = _dbContext.CompanyDepartments
                .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.Company = _dbContext.Companies
                .Where(e => !e.Trashed)
                .ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(DepartmentFilingModel request)
        {
            if (!ModelState.IsValid) return View();

            DepartmentFiling filing = new DepartmentFiling 
            {
                Date = request.Date,
                Note = request.Note,
                CompanyId = request.CompanyId,
                CompanyDepartmentId = request.CompanyDepartmentId,
                HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
            };

            if (request.ImagePath is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "Documents", request.ImagePath.FileName);

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    request.ImagePath.CopyTo(fs);
                }
                filing.Document = ApplicationManager.DocumentImagePath + request.ImagePath.FileName;
            }

            _dbContext.DepartmentFilings.Add(filing);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Departments = _dbContext.CompanyDepartments
                .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.Company = _dbContext.Companies
                .Where(e => !e.Trashed)
                .ToList();

            DepartmentFiling department = _dbContext.DepartmentFilings.Find(id);

            DepartmentFilingModel filing = new DepartmentFilingModel
            {
                CompanyId = department.CompanyId,
                CompanyDepartmentId = department.CompanyDepartmentId,
                Note = department.Note,
                Date = department.Date,
            };

            return View(filing);
        }

        [HttpPost]
        public IActionResult Edit(DepartmentFilingModel request, int id)
        {
            if (!ModelState.IsValid) return View();

            DepartmentFiling filing = _dbContext.DepartmentFilings.Find(id);

            filing.Date = request.Date;
            filing.Note = request.Note;
            filing.CompanyDepartmentId = request.CompanyDepartmentId;
            filing.CompanyId = request.CompanyId;
            filing.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            if (request.ImagePath is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "Documents", request.ImagePath.FileName);

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    request.ImagePath.CopyTo(fs);
                }
                filing.Document = ApplicationManager.DocumentImagePath + request.ImagePath.FileName;
            }
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.DepartmentFilings
                .Include(e => e.Company).Include(e => e.CompanyDepartment)
                .Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            DepartmentFiling filing = _dbContext.DepartmentFilings.Find(id);
            if (filing == null) return RedirectToAction(nameof(Index));

            filing.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            DepartmentFiling filing = _dbContext.DepartmentFilings.Find(id);
            filing.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

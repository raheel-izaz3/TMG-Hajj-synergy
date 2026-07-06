using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using TMG.Models.MiscModels;

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyDepartmentController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyDepartmentController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CompanyDepartments.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.CompanyDepartments.Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(CompanyDepartment department)
        {
            if (!ModelState.IsValid) return View();

            department.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.CompanyDepartments.Add(department);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CompanyDepartment department = _dbContext.CompanyDepartments.Find(id);

            return View(department);
        }


        [HttpPost]
        public IActionResult Edit(CompanyDepartment department)
        {
            if (!ModelState.IsValid) return View();

            department.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(department).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CompanyDepartments.Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            CompanyDepartment department = _dbContext.CompanyDepartments.Find(id);
            if (department == null) return RedirectToAction(nameof(Index));

            department.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            CompanyDepartment department = _dbContext.CompanyDepartments.Find(id);
            department.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

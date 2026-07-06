using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyLicenseController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyLicenseController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            this._dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CompanyLicenseTypes.Where(ct => ct.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashCompanyLicense(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateCompanyLicense(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditCompanyLicense(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCompanyLicense(_dbContext, _http);

            return View(_dbContext.CompanyLicenseTypes.Where(ct => !ct.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompanyLicenseType request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.CompanyLicenseTypes.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CompanyLicenseType contacts = _dbContext.CompanyLicenseTypes.Find(id);
            if (contacts == null) return RedirectToAction(nameof(Index));

            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CompanyLicenseType request)
        {
            if (!ModelState.IsValid) return View(request);

            CompanyLicenseType contact = _dbContext.CompanyLicenseTypes.Find(request.CompanyLicenseTypeId);

            contact.LicenseName = request.LicenseName;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            CompanyLicenseType contact = _dbContext.CompanyLicenseTypes.Find(id);
            if (contact == null) return RedirectToAction(nameof(Index));

            contact.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CompanyLicenseTypes.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            CompanyLicenseType company = _dbContext.CompanyLicenseTypes.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

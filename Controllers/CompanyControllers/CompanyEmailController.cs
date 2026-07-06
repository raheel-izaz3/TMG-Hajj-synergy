using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyEmailController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyEmailController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            this._dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CompanyEmailTypes.Where(ct => ct.Trashed).Count();
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashCompanyEmail(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateCompanyEmail(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditCompanyEmail(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCompanyEmail(_dbContext, _http);

            return View(_dbContext.CompanyEmailTypes.Where(ct => !ct.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompanyEmailType request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.CompanyEmailTypes.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CompanyEmailType contacts = _dbContext.CompanyEmailTypes.Find(id);
            if (contacts == null) return RedirectToAction(nameof(Index));

            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CompanyEmailType request)
        {
            if (!ModelState.IsValid) return View(request);

            CompanyEmailType contact = _dbContext.CompanyEmailTypes.Find(request.CompanyEmailTypeId);

            contact.EmailType = request.EmailType;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            CompanyEmailType contact = _dbContext.CompanyEmailTypes.Find(id);
            if (contact == null) return RedirectToAction(nameof(Index));

            contact.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CompanyEmailTypes.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            CompanyEmailType company = _dbContext.CompanyEmailTypes.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

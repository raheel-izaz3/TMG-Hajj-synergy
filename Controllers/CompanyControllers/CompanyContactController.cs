using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyContactController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyContactController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            this._dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CompanyContactTypes.Where(ct => ct.Trashed).Count();
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashCompanyContact(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateCompanyContact(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditCompanyContact(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCompanyContact(_dbContext, _http);

            return View(_dbContext.CompanyContactTypes.Where(ct => !ct.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompanyContactType request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.CompanyContactTypes.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CompanyContactType contacts = _dbContext.CompanyContactTypes.Find(id);
            if (contacts == null) return RedirectToAction(nameof(Index));

            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CompanyContactType request)
        {
            if (!ModelState.IsValid) return View(request);

            CompanyContactType contact = _dbContext.CompanyContactTypes.Find(request.CompanyContactTypeId);

            contact.ContactType = request.ContactType;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            CompanyContactType contact = _dbContext.CompanyContactTypes.Find(id);
            if (contact == null) return RedirectToAction(nameof(Index));

            contact.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CompanyContactTypes.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            CompanyContactType company = _dbContext.CompanyContactTypes.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

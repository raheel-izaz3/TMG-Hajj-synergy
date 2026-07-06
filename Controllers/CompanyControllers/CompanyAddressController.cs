using Microsoft.AspNetCore;
using TMG.Data;
using TMG.Models;
using TMG.Models.CompanyModels;
using Microsoft.AspNetCore.Mvc;
using TMG.ManagerClasses;

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyAddressController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyAddressController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CompanyAddressTypes.Where(ca => ca.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashCompanyAddress(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateCompanyAddress(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditCompanyAddress(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCompanyAddress(_dbContext, _http);

            return View(_dbContext.CompanyAddressTypes.Where(ca => !ca.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompanyAddressType request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.CompanyAddressTypes.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            CompanyAddressType address = _dbContext.CompanyAddressTypes.Find(id);
            if (address == null) return RedirectToAction(nameof(Index));

            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CompanyAddressType request)
        {
            if (!ModelState.IsValid) return View(request);

            CompanyAddressType address = _dbContext.CompanyAddressTypes.Find(request.CompanyAddressTypeId);

            address.AddressType = request.AddressType;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            CompanyAddressType companyAddress = _dbContext.CompanyAddressTypes.Find(id);
            if (companyAddress == null) return RedirectToAction(nameof(Index));

            companyAddress.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.CompanyAddressTypes.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            CompanyAddressType company = _dbContext.CompanyAddressTypes.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

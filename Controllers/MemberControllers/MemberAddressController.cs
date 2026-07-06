using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.MemberModels;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers.MemberControllers
{
    public class MemberAddressController : Controller
    {
        private readonly TMGDbContext _dbContext;

        private readonly IHttpContextAccessor _http;
        public MemberAddressController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            this._dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.MemberAddressTypes.Where(ct => ct.Trashed).Count();
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashMemberAddress(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateMemberAddress(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditMemberAddress(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreMemberAddress(_dbContext, _http);

            return View(_dbContext.MemberAddressTypes.Where(ct => !ct.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MemberAddressType request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.MemberAddressTypes.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            MemberAddressType addresss = _dbContext.MemberAddressTypes.Find(id);
            if (addresss == null) return RedirectToAction(nameof(Index));

            return View(addresss);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MemberAddressType request)
        {
            if (!ModelState.IsValid) return View(request);

            MemberAddressType address = _dbContext.MemberAddressTypes.Find(request.MemberAddressTypeId);

            address.AddressType = request.AddressType;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            MemberAddressType address = _dbContext.MemberAddressTypes.Find(id);
            if (address == null) return RedirectToAction(nameof(Index));

            address.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.MemberAddressTypes.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            MemberAddressType company = _dbContext.MemberAddressTypes.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.MemberModels;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers.MemberControllers
{
    public class MemberContactController : Controller
    {
        private readonly TMGDbContext _dbContext;

        private readonly IHttpContextAccessor _http;
        public MemberContactController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            this._dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.MemberContactTypes.Where(ct => ct.Trashed).Count();
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashMemberContact(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateMemberContact(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditMemberContact(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreMemberContact(_dbContext, _http);

            return View(_dbContext.MemberContactTypes.Where(ct => !ct.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MemberContactType request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.MemberContactTypes.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            MemberContactType contact = _dbContext.MemberContactTypes.Find(id);
            if (contact == null) return RedirectToAction(nameof(Index));

            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MemberContactType request)
        {
            if (!ModelState.IsValid) return View(request);

            MemberContactType contact = _dbContext.MemberContactTypes.Find(request.MemberContactTypeId);

            contact.ContactType = request.ContactType;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            MemberContactType contact = _dbContext.MemberContactTypes.Find(id);
            if (contact == null) return RedirectToAction(nameof(Index));

            contact.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.MemberContactTypes.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            MemberContactType company = _dbContext.MemberContactTypes.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.MemberModels;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers.MemberControllers
{
    public class MemberEmailController : Controller
    {
        private readonly TMGDbContext _dbContext;

        private readonly IHttpContextAccessor _http;

        public MemberEmailController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            this._dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.MemberEmailTypes.Where(ct => ct.Trashed).Count();
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashMemberEmail(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateMemberEmail(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditMemberEmail(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreMemberEmail(_dbContext, _http);

            return View(_dbContext.MemberEmailTypes.Where(ct => !ct.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MemberEmailType request)
        {
            if (!ModelState.IsValid) return View(request);

            _dbContext.MemberEmailTypes.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            MemberEmailType contacts = _dbContext.MemberEmailTypes.Find(id);
            if (contacts == null) return RedirectToAction(nameof(Index));

            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MemberEmailType request)
        {
            if (!ModelState.IsValid) return View(request);

            MemberEmailType contact = _dbContext.MemberEmailTypes.Find(request.MemberEmailTypeId);

            contact.EmailType = request.EmailType;

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            MemberEmailType contact = _dbContext.MemberEmailTypes.Find(id);
            if (contact == null) return RedirectToAction(nameof(Index));

            contact.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.MemberEmailTypes.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            MemberEmailType company = _dbContext.MemberEmailTypes.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

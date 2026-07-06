using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class PaymentNotesController: Controller
    {
        private readonly TMGDbContext _dbContext;

        private readonly IHttpContextAccessor _http;
        public PaymentNotesController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.PaymentNotes.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashPaymentNotes(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreatePaymentNotes(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditPaymentNotes(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestorePaymentNotes(_dbContext, _http);

            return View(_dbContext.PaymentNotesWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(PaymentNotes PaymentNotes)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.PaymentNotes.Add(PaymentNotes);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            PaymentNotes PaymentNotes = _dbContext.PaymentNotes.Find(id);

            return View(PaymentNotes);
        }


        [HttpPost]
        public IActionResult Edit(PaymentNotes PaymentNotes)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(PaymentNotes).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.PaymentNotes.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            PaymentNotes PaymentNotes = _dbContext.PaymentNotes.Find(id);
            if (PaymentNotes == null) return RedirectToAction(nameof(Index));

            PaymentNotes.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            PaymentNotes PaymentNotes = _dbContext.PaymentNotes.Find(id);
            PaymentNotes.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.AccountModels;

namespace TMG.Controllers.AccountControllers
{
    public class BankAccountContactPersonsController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public BankAccountContactPersonsController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.BankAccountContactPersons.Where(e => e.Trashed).Count();

            return View(_dbContext.BankAccountContactPersons
                        .Include(e => e.CompanyBankAccount)
                        .Include(e => e.Designation)
                        .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Create()
        {
            var CBA = _dbContext.CompanyBankAccounts.Where(c => !c.Trashed).ToList();
            ViewBag.CBA = new SelectList(CBA, "CompanyBankAccountId", "Title");
            ViewBag.Country = _dbContext.Countries
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.City = _dbContext.Cities
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.Designation = _dbContext.Designations
                .Where(e => !e.Trashed)
                .ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(BankAccountContactPerson BCP)
        {

            if (!ModelState.IsValid)
            {
                var CBA = _dbContext.CompanyBankAccounts.Where(c => !c.Trashed).ToList();
                ViewBag.CBA = new SelectList(CBA, "CompanyBankAccountId", "Title");

                return View();
            }

            BCP.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.BankAccountContactPersons.Add(BCP);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var CBA = _dbContext.CompanyBankAccounts.Where(c => !c.Trashed).ToList();
            ViewBag.CBA = new SelectList(CBA, "CompanyBankAccountId", "Title");

            var BCP = _dbContext.BankAccountContactPersons
                .Include(b => b.CompanyBankAccount)
                .Include(e => e.Designation)
                .Where(b => b.BankAccountContactPersonId == id)
                .FirstOrDefault();
            ViewBag.Country = _dbContext.Countries
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.City = _dbContext.Cities
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.Designation = _dbContext.Designations
                .Where(e => !e.Trashed)
                .ToList();

            return View(BCP);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(BankAccountContactPerson BCP)
        {

            if (!ModelState.IsValid)
            {
                var CBA = _dbContext.CompanyBankAccounts.Where(c => !c.Trashed).ToList();
                ViewBag.CBA = new SelectList(CBA, "CompanyBankAccountId", "Title");

                return View();
            }
            BCP.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.Entry(BCP).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.BankAccountContactPersons.Include(e => e.CompanyBankAccount).Include(e => e.Designation).Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            var BCP = _dbContext.BankAccountContactPersons.Find(id);
            if (BCP == null) return RedirectToAction(nameof(Index));

            BCP.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            var BCP = _dbContext.BankAccountContactPersons.Find(id);
            BCP.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

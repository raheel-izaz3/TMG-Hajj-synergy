using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.AccountModels;

namespace TMG.Controllers.AccountControllers
{
    public class ExpenseContactPersonsController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public ExpenseContactPersonsController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.ExpenseContactPersons.Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();

            return View(_dbContext.ExpenseContactPersons.Include(e => e.ExpenseSubType).Include(e => e.Designation).Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Create()
        {
            var ExpenseSubType = _dbContext.ExpenseSubTypes.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
            ViewBag.ExpenseSubType = new SelectList(ExpenseSubType, "ExpenseSubTypeId", "ExpenseSubTypeName");
            
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
        public IActionResult Create(ExpenseContactPerson ECP)
        {

            if (!ModelState.IsValid)
            {
                var ExpenseSubType = _dbContext.ExpenseSubTypes.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
                ViewBag.ExpenseSubType = new SelectList(ExpenseSubType, "ExpenseSubTypeId", "ExpenseSubTypeName");

                return View();
            }

            ECP.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.ExpenseContactPersons.Add(ECP);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var ExpenseSubType = _dbContext.ExpenseSubTypes.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            var ECP = _dbContext.ExpenseContactPersons
                .Include(v => v.ExpenseSubType)
                .Include(v => v.Designation)
                .Where(v => v.ExpenseContactPersonId == id)
                .FirstOrDefault();

            ViewBag.ExpenseSubType = new SelectList(ExpenseSubType, "ExpenseSubTypeId", "ExpenseSubTypeName", ECP.ExpenseSubTypeId);
            ViewBag.Country = _dbContext.Countries
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.City = _dbContext.Cities
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.Designation = _dbContext.Designations
                .Where(e => !e.Trashed)
                .ToList();

            return View(ECP);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ExpenseContactPerson ECP)
        {

            if (!ModelState.IsValid)
            {
                var ExpenseSubType = _dbContext.ExpenseSubTypes.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
                ViewBag.ExpenseSubType = new SelectList(ExpenseSubType, "ExpenseSubTypeId", "ExpenseSubTypeName");

                return View();
            }
            ECP.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.Entry(ECP).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.ExpenseContactPersons.Include(e => e.ExpenseSubType).Include(e => e.Designation).Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            var ECP = _dbContext.ExpenseContactPersons.Find(id);
            if (ECP == null) return RedirectToAction(nameof(Index));

            ECP.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            var ECP = _dbContext.ExpenseContactPersons.Find(id);
            if (ECP == null) return RedirectToAction(nameof(Index));

            ECP.Trashed = false;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}

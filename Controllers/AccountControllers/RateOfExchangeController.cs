using TMG.Data;
using TMG.Models;
using TMG.Models.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace TMG.Controllers.AccountControllers
{
    public class RateOfExchangeController : Controller
    {

        private readonly TMGDbContext _dbContext;


        public RateOfExchangeController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RateOfExchanges.Where(e => e.Trashed).Count();
            return View(_dbContext.RateOfExchangesWithoutTrash);
        }

        public IActionResult Create() {
            return View();
        }

        [HttpPost]

        public IActionResult Create(RateOfExchange request) {

       if (!ModelState.IsValid) return View();

            DateTime currentDate = DateTime.Today;
            DateTime dateTime = DateTime.Parse(currentDate.ToString());
            
            // string formattedDate = dateTime.ToString("yyyy-MM-dd");
            // DateOnly dateOnly = DateOnly.Parse(formattedDate);


            RateOfExchange roe = new RateOfExchange
            {
                PKRROE = request.PKRROE,
                SARROE = request.SARROE,
                USDROE = request.USDROE,
                Date = dateTime,
                Currency = request.Currency
            };

            _dbContext.RateOfExchanges.Add(roe);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult Edit(int id)
        {
            RateOfExchange roe = _dbContext.RateOfExchanges.Find(id);
            return View(roe);
        }


        [HttpPost]
        public IActionResult Edit(RateOfExchange request)
        {
           
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(request).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.RateOfExchanges.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            RateOfExchange roe = _dbContext.RateOfExchanges.Find(id);
            if (roe == null) return RedirectToAction(nameof(Index));

            roe.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            RateOfExchange roe = _dbContext.RateOfExchanges.Find(id);
            roe.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

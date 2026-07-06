using TMG.Data;
using TMG.Models;
using TMG.Models.SettingsModels;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;

namespace TMG.Models
{
    public record SettingsModel {
        public float? SARROE { get; set; }
        public float? USDROE { get; set; }
        public int VaccineCount {  get; set; }
    }

}

namespace TMG.Controllers
{
    public class SettingsController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public SettingsController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ApplicationSettings settings = _dbContext.ApplicationSettings.FirstOrDefault();

            return View(settings);
        }

        [HttpPost]
        public IActionResult UpdateROE(ApplicationSettings settings)
        {
            ApplicationSettings _settings = _dbContext.ApplicationSettings.FirstOrDefault();
            
            _settings.SARROE = settings.SARROE;
            _settings.USDROE = settings.USDROE;

            _dbContext.SaveChanges();

            TempData["roe-updated"] = true;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateVaccineCount(ApplicationSettings settings)
        {
            ApplicationSettings _settings = _dbContext.ApplicationSettings.FirstOrDefault();

            _settings.VaccineCount = settings.VaccineCount;

            _dbContext.SaveChanges();

            TempData["vaccine-updated"] = true;
            return RedirectToAction("Index");
        }

    }
}

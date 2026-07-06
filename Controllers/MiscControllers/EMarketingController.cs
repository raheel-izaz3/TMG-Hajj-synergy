using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
//using TMG.Migrations;
using TMG.Models.MiscModels;

namespace TMG.Controllers.MiscControllers
{
    public class EMarketingController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public EMarketingController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.EMarketings.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);

            return View(_dbContext.EMarketingsWithoutTrash.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(EMarketing marketing,IFormFile BackgroundImage)
        {

            if (BackgroundImage != null)
            {
                // Generate unique file name
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(BackgroundImage.FileName);
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/EMarketing");

                // Check if the directory exists, if not, create it
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string uploadPath = Path.Combine(folderPath, fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    BackgroundImage.CopyTo(fileStream);
                }

                marketing.BackgroundImage = "/Images/EMarketing/" + fileName; // Store relative path
            }


            //marketing.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.EMarketings.Add(marketing);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            EMarketing Marketing = _dbContext.EMarketings.Find(id);

            return View(Marketing);
        }


        [HttpPost]
        public IActionResult Edit(EMarketing marketing, IFormFile? BackgroundImage)
        {

            var existingMarketing = _dbContext.EMarketings.AsNoTracking().FirstOrDefault(m => m.EMarketingId == marketing.EMarketingId);
            if (existingMarketing == null)
            {
                return NotFound();
            }

            if (BackgroundImage != null)
            {
                // Delete Previous Image
                if (!string.IsNullOrEmpty(existingMarketing.BackgroundImage))
                {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingMarketing.BackgroundImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Upload New Image
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(BackgroundImage.FileName);
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/EMarketing");

                // Ensure Directory Exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string uploadPath = Path.Combine(folderPath, fileName);

                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    BackgroundImage.CopyTo(fileStream);
                }

                marketing.BackgroundImage = "/Images/EMarketing/" + fileName; // Update Path
            }
            else
            {
                marketing.BackgroundImage = existingMarketing.BackgroundImage; // Keep Old Image
            }

            _dbContext.Entry(marketing).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult ViewTrash()
        {
            return View(_dbContext.EMarketings.Where(s => s.Trashed).ToList());
        }

        public IActionResult Trash(int id)
        {
            EMarketing marketing = _dbContext.EMarketings.Find(id);
            if (marketing == null) return RedirectToAction(nameof(Index));

            marketing.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            EMarketing marketing = _dbContext.EMarketings.Find(id);
            marketing.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

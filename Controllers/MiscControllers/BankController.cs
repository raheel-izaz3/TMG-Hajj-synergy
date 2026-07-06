using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using TMG.Models.FormModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Core;

namespace TMG.Controllers.MiscControllers
{
    public class BankController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;
        private readonly IHttpContextAccessor _http;

        public BankController(TMGDbContext dbContext, IWebHostEnvironment webEnviroment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webEnviroment = webEnviroment;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Banks.Where(e => e.Trashed)
                .Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashBank(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateBank(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditBank(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreBank(_dbContext, _http);

            return View(_dbContext.BanksWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(BankRegisterRequest request)
        {
            if (!ModelState.IsValid) return View();

            string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "BankLogos", request.File.FileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                request.File.CopyTo(fs);
            }

            Bank bank = new Bank
            {
                Name = request.Name,
                Image = @"Images/BankLogos/" + request.File.FileName,
                //HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")),
            };

            _dbContext.Banks.Add(bank);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Bank bank = _dbContext.Banks.Find(id);

            return View(bank);
        }


        [HttpPost]
        public IActionResult Edit(Bank bank)
        {
            if (!ModelState.IsValid) return View();

            // _dbContext.Entry(bank).State = EntityState.Modified;
            var bankToEdit = _dbContext.Banks.Find(bank.BankId);
            bankToEdit.Name = bank.Name;
            //bank.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangeImage(int id) => View(_dbContext.Banks.Find(id));

        [HttpPost]
        public IActionResult ChangeImage(int BankId, IFormFile File)
        {
            if (!ModelState.IsValid) return View();

            Bank bank = _dbContext.Banks.Find(BankId);

            string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "BankLogos", File.FileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            bank.Image = @"Images/BankLogos/" + File.FileName;
            //bank.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Banks);
        }

        public IActionResult Trash(int id)
        {
            Bank bank = _dbContext.Banks.Find(id);
            if (bank == null) return RedirectToAction(nameof(Index));

            bank.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Bank bank = _dbContext.Banks.Find(id);
            bank.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

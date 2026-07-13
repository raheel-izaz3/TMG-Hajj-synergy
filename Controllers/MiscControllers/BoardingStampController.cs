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
    public class BoardingStampController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;
        private readonly IHttpContextAccessor _http;

        public BoardingStampController(TMGDbContext dbContext, IWebHostEnvironment webEnviroment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webEnviroment = webEnviroment;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.BoardingStamps.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashBank(_dbContext, _http);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateBank(_dbContext, _http);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditBank(_dbContext, _http);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreBank(_dbContext, _http);

            return View(_dbContext.BoardingStampsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(BoardingStamp request)
        {
            if (!ModelState.IsValid) return View();

            string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "BoardingStamps", request.File.FileName);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                request.File.CopyTo(fs);
            }

            BoardingStamp stamp = new BoardingStamp
            {
                FilePath = @"Images/BoardingStamps/" + request.File.FileName
            };

            _dbContext.BoardingStamps.Add(stamp);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            BoardingStamp stamp = _dbContext.BoardingStamps.Find(id);

            return View(stamp);
        }


        [HttpPost]
        public IActionResult Edit(int BoardingStampId, IFormFile File)
        {
            if (!ModelState.IsValid) return View();

            BoardingStamp stamp = _dbContext.BoardingStamps.Find(BoardingStampId);

            string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "BoardingStamps", File.FileName);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            stamp.FilePath = @"Images/BoardingStamps/" + File.FileName;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }



        public IActionResult ViewTrash()
        {
            return View(_dbContext.BoardingStamps.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            BoardingStamp stamp = _dbContext.BoardingStamps.Find(id);
            if (stamp == null) return RedirectToAction(nameof(Index));

            stamp.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            BoardingStamp stamp = _dbContext.BoardingStamps.Find(id);
            stamp.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

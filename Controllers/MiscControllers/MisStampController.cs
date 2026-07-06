using TMG.Data;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

namespace TMG.Controllers.MiscControllers
{
    public class MisStampController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;
        private readonly IHttpContextAccessor _http;

        public MisStampController(TMGDbContext dbContext, IWebHostEnvironment webEnviroment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webEnviroment = webEnviroment;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.MisStamps.Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashBank(_dbContext, _http);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateBank(_dbContext, _http);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditBank(_dbContext, _http);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreBank(_dbContext, _http);
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            return View(_dbContext.MisStampsWithoutTrash.Include(e => e.company));
        }

        public IActionResult Create()
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            return View();
        }


        [HttpPost]
        public IActionResult Create(MisStamp request)
        {

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            if (!ModelState.IsValid) return View();

            string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "MisStamps", request.File.FileName);
            string path1 = Path.Combine(_webEnviroment.WebRootPath, "Images", "SignatureMisStamps", request.FileSignature.FileName);
            string path2 = Path.Combine(_webEnviroment.WebRootPath, "Images", "AccountantSignatures", request.AccountantSignatureFile.FileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                request.File.CopyTo(fs);
            }
            using (FileStream fs = new FileStream(path1, FileMode.Create))
            {
                request.FileSignature.CopyTo(fs);
            }
            using (FileStream fs = new FileStream(path2, FileMode.Create))
            {
                request.AccountantSignatureFile.CopyTo(fs);
            }

            MisStamp stamp = new MisStamp
            {
                FilePath = @"Images/MisStamps/" + request.File.FileName,
                SignatureFilePath = @"Images/SignatureMisStamps/" + request.FileSignature.FileName,
                AccountantSignatureFilePath = @"Images/AccountantSignatures/" + request.AccountantSignatureFile.FileName,
                CompanyId = request.CompanyId
            };

            _dbContext.MisStamps.Add(stamp);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            MisStamp stamp = _dbContext.MisStamps.Find(id);
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            return View(stamp);
        }


        [HttpPost]
        public IActionResult Edit(int MisStampId, IFormFile File, IFormFile FileSignature,
         IFormFile AccountantSignatureFile, int CompanyId)
        {
            if (!ModelState.IsValid) return View();
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            MisStamp stamp = _dbContext.MisStamps.Find(MisStampId);

            if (File is not null)
            {
                string path = Path.Combine(_webEnviroment.WebRootPath, "Images", "MisStamps", File.FileName);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    File.CopyTo(fs);
                }
                stamp.FilePath = @"Images/MisStamps/" + File.FileName;
            }
            if (FileSignature is not null)
            {
                string path1 = Path.Combine(_webEnviroment.WebRootPath, "Images", "SignatureMisStamps", FileSignature.FileName);
                using (FileStream fs = new FileStream(path1, FileMode.Create))
                {
                    FileSignature.CopyTo(fs);
                }
                stamp.SignatureFilePath = @"Images/SignatureMisStamps/" + FileSignature.FileName;

            }
            if (AccountantSignatureFile is not null)
            {
                string path2 = Path.Combine(_webEnviroment.WebRootPath, "Images", "AccountantSignatures", AccountantSignatureFile.FileName);
                using (FileStream fs = new FileStream(path2, FileMode.Create))
                {
                    AccountantSignatureFile.CopyTo(fs);
                }
                stamp.AccountantSignatureFilePath = @"Images/AccountantSignatures/" + AccountantSignatureFile.FileName;

            }

            stamp.CompanyId = CompanyId;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }



        public IActionResult ViewTrash()
        {
            return View(_dbContext.MisStamps.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            MisStamp stamp = _dbContext.MisStamps.Find(id);
            if (stamp == null) return RedirectToAction(nameof(Index));

            stamp.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            MisStamp stamp = _dbContext.MisStamps.Find(id);
            stamp.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

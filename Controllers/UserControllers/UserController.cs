using Azure.Core;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.CompanyModels;
using TMG.Models.FormModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public UserController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Users.Where(u => u.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashUser(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateUser(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditUser(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreUser(_dbContext, _http);

            return View(_dbContext.Users.Include(e => e.Role).Where(u => u.UserId != 1 && u.Trashed == false));
        }

        public IActionResult Create()
        {
            ViewBag.Roles = _dbContext.RolesWithoutTrash;
            ViewBag.RolesCount = _dbContext.RolesWithoutTrash.Count();

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.CompaniesCount = _dbContext.CompaniesWithoutTrash.Count();

            return View();
        }

        [HttpPost]
        public IActionResult Create(RegisterRequest request)
        {
            ViewBag.Roles = _dbContext.RolesWithoutTrash;
            ViewBag.RolesCount = _dbContext.RolesWithoutTrash.Count();

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.CompaniesCount = _dbContext.CompaniesWithoutTrash.Count();

            if (request.CompanyId is null)
            {
                ModelState.AddModelError("CompanyId", "Please Select a company");
            }

            if (request.RoleId == 0)
            {
                ModelState.AddModelError("RoleId", "Please Select a Role");
            }

            try
            {

                User user = new();

                user.Username = request.Username;
                user.Email = request.Email;
                user.PinCode = request.PinCode;

                Models.User.GenerateHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordSalt = passwordSalt;
                user.PasswordHash = passwordHash;

                user.RoleId = request.RoleId;

                if (request.UserSignatureFile is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "UserSignatures", request.UserSignatureFile.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.UserSignatureFile.CopyTo(fs);
                    }
                    user.UserSignature = ApplicationManager.UserSignaturesPath + request.UserSignatureFile.FileName;
                }

                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();

                for (int i = 0; i < request.CompanyId.Count; i++)
                {
                    UserCompanies userCompanies = new UserCompanies
                    {
                        CompanyId = request.CompanyId[i],
                        UserId = user.UserId
                    };

                    _dbContext.UserCompanies.Add(userCompanies);
                }

                _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (Models.User.IsDuplicateEmail(ex))
                {
                    ModelState.AddModelError(Models.User.EmailKey, "A user w/ the provided email already exists.");
                }

                return View(request);
            }


            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int? id)
        {
            User? user = _dbContext.Users.Find(id);

            ViewBag.AssignedCompanies = _dbContext.UserCompanies.Include(uc => uc.Company).Where(uc => uc.UserId == id && !uc.Company.Trashed);

            ViewBag.Roles = _dbContext.RolesWithoutTrash;
            ViewBag.RolesCount = _dbContext.RolesWithoutTrash.Count();

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.CompaniesCount = _dbContext.CompaniesWithoutTrash.Count();


            UserEditRequest userEditRequest = new()
            {
                UserId = user.UserId,
                Email = user.Email,
                Username = user.Username,
                RoleId = user.RoleId,
                UserSignature = user.UserSignature
            };


            if (user != null)
                return View(userEditRequest);
            else
                return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Edit(UserEditRequest request)
        {
            if (request.CompanyId is null)
            {
                ModelState.AddModelError("CompanyId", "Please Select a company");
            }

            if (request.RoleId == 0)
            {
                ModelState.AddModelError("RoleId", "Please Select a Role");
            }

            if (!ModelState.IsValid) return View();
            User? userToEdit = _dbContext.Users.Find(request.UserId);

            ViewBag.Roles = _dbContext.RolesWithoutTrash;
            ViewBag.RolesCount = _dbContext.RolesWithoutTrash.Count();

            ViewBag.Companies = _dbContext.CompaniesWithoutTrash; 
            ViewBag.CompaniesCount = _dbContext.CompaniesWithoutTrash.Count();

            if (userToEdit == null) return View(request);

            userToEdit.Username = request.Username;
            userToEdit.Email = request.Email;
            userToEdit.RoleId = request.RoleId;
            userToEdit.PinCode = request.PinCode;
            userToEdit.UpdatedAt = DateTime.Now;

            if (request.UserSignatureFile is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "UserSignatures", request.UserSignatureFile.FileName);

                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    request.UserSignatureFile.CopyTo(fs);
                }
                userToEdit.UserSignature = ApplicationManager.UserSignaturesPath + request.UserSignatureFile.FileName;
            }

            _dbContext.UserCompanies.Where(uc => uc.UserId == request.UserId).ExecuteDelete();

            for (int i = 0; i < request.CompanyId.Count; i++)
            {
                UserCompanies _userCompanies = new UserCompanies
                {
                    CompanyId = request.CompanyId[i],
                    UserId = request.UserId
                };

                _dbContext.UserCompanies.Add(_userCompanies);
            }


            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangePassword(int id)
        {
            User user = _dbContext.Users.Find(id);
            PasswordChangeRequest req = new();
            req.UserId = user.UserId;

            ViewBag.UserId = int.Parse(_http.HttpContext.Session.GetString("UserId"));

            return View(req);
        }

        [HttpPost]
        public IActionResult ChangePassword(PasswordChangeRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            Models.User.GenerateHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            User? user = _dbContext.Users.Find(request.UserId);
            if (user != null)
            {
                user.PasswordSalt = passwordSalt;
                user.PasswordHash = passwordHash;
                user.PinCode = request.PinCode;

                _dbContext.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            User userToDelete = _dbContext.Users.Find(id);
            if (userToDelete == null) return RedirectToAction(nameof(Index));

            userToDelete.Trashed = true;
            _dbContext.SaveChanges();


            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Users.Where(u => u.Trashed));
        }

        public IActionResult Restore(int id)
        {
            Models.User user = _dbContext.Users.Find(id);
            user.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            User user = _dbContext.Users.Include(u => u.Role).First(u => u.UserId == id);

            ViewBag.UserCompanies = _dbContext.UserCompanies.Include(uc => uc.Company)
                .Where(uc => uc.UserId == user.UserId);

            ViewBag.UserCompanyAddresses = _dbContext.CompanyAddresses
                .Include(c => c.CompanyAddress);

            ViewBag.UserCompanyContacts = _dbContext.CompanyContacts
                .Include(c => c.Company)
                .Include(uc => uc.CompanyContactType);

            ViewBag.UserCompanyEmails = _dbContext.CompanyEmails
               .Include(c => c.Company)
               .Include(uc => uc.CompanyEmailType);

            ViewBag.UserCompaniesCount = _dbContext.UserCompanies.Count();

            return View(user);
        }
    }
}

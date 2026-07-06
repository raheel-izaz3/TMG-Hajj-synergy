using Azure.Core;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.FormModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.UserModels;

namespace TMG.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;


        public RoleController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Roles.Where(r => r.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashRole(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateRole(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditRole(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreRole(_dbContext, _http);
            ViewBag.SetPrivilegeAllowed = PrivilegeManager.CanSetRolePrivilege(_dbContext, _http);

            return View(_dbContext.Roles.Where(r => r.RoleId != 1 && !r.Trashed));
        }

        public IActionResult Create()
        {
            ViewBag.Modules = _dbContext.Modules;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Role role)
        {

            //return Ok(new { role });
            ViewBag.Modules = _dbContext.Modules;

            if (!ModelState.IsValid) return View(role);

            _dbContext.Add(role);
            _dbContext.SaveChanges();

            var permissions = _dbContext.Permissions.ToList();
            for (int i = 0; i < permissions.Count; i++)
            {
                _dbContext.RolePermissions.Add(
                    new RolePermissions { RoleId = role.RoleId, PermissionId = permissions.ElementAt(i).PermissionId, Allowed = false }
                    );
            }

            _dbContext.SaveChanges();

            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            Role roleToDelete = _dbContext.Roles.Find(id)!;
            roleToDelete.Trashed = true;

            var users = _dbContext.Users.Where(u => u.RoleId == roleToDelete.RoleId);

            foreach (User u in users)
            {
                u.RoleId = null;
            }

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {

            Role? role = _dbContext.Roles.Find(id);

            ViewBag.Modules = _dbContext.Modules;

            if (role == null) return RedirectToAction(nameof(Index));

            return View(role);
        }

        [HttpPost]
        public IActionResult Edit(Role role)
        {

            _dbContext.Entry(role).State = EntityState.Modified;

            role.UpdatedAt = DateTime.Now;

            _dbContext.SaveChanges();

            TempData["updated"] = true;

            return RedirectToAction(nameof(Index));
        }

        public IActionResult SetPrivileges(int id)
        {
            var rolePermissions = _dbContext.RolePermissions.Where(rp => rp.RoleId == id).ToList();

            RolePrivilegeRequest request = new RolePrivilegeRequest
            {
                RoleId = id,
                Permissions = new List<bool>(new bool[252]) 
            };

            foreach (var permission in rolePermissions)
            {
                if (permission.PermissionId < request.Permissions.Count) 
                {
                    request.Permissions[permission.PermissionId] = permission.Allowed;
                }
            }

            return View(request);
        }

        [HttpPost]
        public IActionResult SetPrivileges(RolePrivilegeRequest request)
        {
            var existingPermissions = _dbContext.RolePermissions
                .Where(rp => rp.RoleId == request.RoleId)
                .ToList();

            var existingPermissionDict = existingPermissions.ToDictionary(p => p.PermissionId);

            for (int i = 0; i < request.Permissions.Count; i++)
            {
                if (existingPermissionDict.ContainsKey(i))
                {
                    existingPermissionDict[i].Allowed = request.Permissions[i];
                }
                else if (request.Permissions[i]) 
                {
                    _dbContext.RolePermissions.Add(new RolePermissions
                    {
                        RoleId = request.RoleId,
                        PermissionId = i,
                        Allowed = true
                    });
                }
            }

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Roles.Where(r => r.Trashed));
        }

        public IActionResult Restore(int id)
        {
            Role role = _dbContext.Roles.Find(id);
            role.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UpdatePriv()
        {
            var users = _dbContext.Users.ToList();
            var rolePermissions = _dbContext.RolePermissions.ToList();
            var permissions = _dbContext.Permissions.Select(e => e.PermissionId).ToList();

            for (int i = 0; i < users.Count; i++)
            {
                var userPermissions = rolePermissions.Where(e => e.RoleId == users.ElementAt(i).RoleId);
                for (int j = 0; j < permissions.Count(); j++)
                {
                }
            }

            return Ok("Updated!");
        }

        public IActionResult NewSetPrivileges(int id)
        {
            var role = _dbContext.Roles.Find(id);
            return View(role);
        }

        public async Task<JsonResult> UpdatePrivilige(int id, string priv)
        {
            var rolePriv = await _dbContext.RolePrivileges
                .FirstOrDefaultAsync(e => e.RoleId == id && e.Privilege == priv);

            if (rolePriv == null)
            {
                await _dbContext.RolePrivileges.AddAsync(new RolePrivileges()
                {
                    RoleId = id,
                    Privilege = priv
                });

                await _dbContext.SaveChangesAsync();

                return Json($"{priv} added to Role Id: {id}");
            }

            _dbContext.RolePrivileges.Remove(rolePriv);

            await _dbContext.SaveChangesAsync();

            return Json($"{priv} removed to Role Id: {id}");
        }
    
        public async Task<JsonResult> GetPriviliges(int id)
        {
            var claims = await _dbContext.RolePrivileges
                .Where(e => e.RoleId == id)
                .Select(e => e.Privilege)
                .ToListAsync();

            return Json(claims);
        }
    }
}

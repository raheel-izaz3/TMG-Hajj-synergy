using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.GroupControllers
{
    public class GroupController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        public GroupController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Groups
            .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            .Where(e => e.Trashed).Count();
            var groups = _dbContext.Groups
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Include(e => e.Package)
                .Where(e => !e.Trashed);
            List<int> selected = new List<int>();
            foreach (var item in groups)
            {
                var groupMembers = _dbContext.GroupMembers.Where(e => e.GroupId == item.GroupId).ToList();
                selected.Add(groupMembers.Count);
            }

            ViewBag.Selected = selected;
            return View(groups);
        }
        public IActionResult Create()
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            return View();
        }
        // C#
        [HttpPost]
        public IActionResult Create(Models.GroupModels.Group group)
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            if (!ModelState.IsValid) return View();

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                // Persist the group first so GroupId is available
                _dbContext.Groups.Add(group);
                _dbContext.SaveChanges();

                // Prepare set of valid package ids for the current season (and optionally company)
                int activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                var validPackageIds = _dbContext.PackagesWithoutTrash
                    .Where(p => p.HajjSeasonsId == activeSeasonId)
                    .Select(p => p.PackageId)
                    .ToHashSet();

                // Add only those GroupPackages whose PackageId exists
                for (int i = 0; i < (group.PackageIds?.Count ?? 0); i++)
                {
                    if (group.PackagesSelected[i] == false) continue;

                    var pid = group.PackageIds[i];
                    if (!validPackageIds.Contains(pid))
                    {
                        // skip or optionally add a ModelState error / log the missing id
                        // ModelState.AddModelError("", $"Selected package ({pid}) not found.");
                        continue;
                    }

                    _dbContext.GroupPackages.Add(new GroupPackages
                    {
                        PackageId = pid,
                        GroupId = group.GroupId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();

                TempData["created"] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                transaction.Rollback();
                return View(group);
            }
        }
        public IActionResult Edit(int id)
        {
            ViewBag.GroupPackages = _dbContext.GroupPackages.Where(e => e.GroupId == id);
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            Models.GroupModels.Group Group = _dbContext.Groups.Find(id);

            return View(Group);
        }
        [HttpPost]
        public IActionResult Edit(Models.GroupModels.Group group)
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            if (!ModelState.IsValid) return View(group);


            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                group.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                _dbContext.Entry(group).State = EntityState.Modified;

                _dbContext.SaveChanges();
                _dbContext.GroupPackages.Where(e => e.GroupId == group.GroupId).ExecuteDelete();
                for (int i = 0; i < (group.PackageIds?.Count ?? 0); i++)
                {
                    if (group.PackagesSelected[i] == false) continue;

                    _dbContext.GroupPackages.Add(new GroupPackages
                    {
                        PackageId = group.PackageIds[i],
                        GroupId = group.GroupId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(group);
            }

            //_dbContext.Entry(group).State = EntityState.Modified;

            //_dbContext.SaveChanges();

            //return RedirectToAction(nameof(Index));
        }
        public IActionResult ViewTrash()
        {
            return View(_dbContext.Groups
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(s => s.Trashed));
        }
        public IActionResult Trash(int id)
        {
            Models.GroupModels.Group group = _dbContext.Groups.Find(id);
            if (group == null) return RedirectToAction(nameof(Index));

            group.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Restore(int id)
        {
            Models.GroupModels.Group group = _dbContext.Groups.Find(id);
            group.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult AddMember(int groupId)
        {
            Models.GroupModels.Group group = _dbContext.GroupsWithoutTrash.FirstOrDefault(e => e.GroupId == groupId);

            ViewBag.GroupId = groupId;
            ViewBag.GroupCode = group.Code;
            ViewBag.GroupName = group.Name;
            ViewBag.GroupPAX = group.PAX;

            var groupMembers = _dbContext.GroupMembers.Where(e => e.GroupId == groupId && !e.Group.Trashed && !e.FamilyHead.Trashed).ToList(); // Fetching group members once

            ViewBag.GroupMembers = groupMembers;
            ViewBag.GroupMembersCount = groupMembers.Count();

            foreach (var currentMember in groupMembers)
            {
                bool? inSameGroup = null;
                var _m = _dbContext.GroupMembers
                    .Where(a => a.FamilyHead.Trashed == false)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                if (_m is null)
                {
                    inSameGroup = null;
                }
                else if (_m.GroupId == groupId)
                {
                    inSameGroup = true;
                }
                else if (_m.GroupId != groupId)
                {
                    inSameGroup = false;
                }

                ViewBag.InSameGroup = inSameGroup;
            }

            var grouppackages = _dbContext.GroupPackages.Where(e => e.GroupId == group.GroupId);

            List<FamilyMembers> model = new List<FamilyMembers>();
            foreach (var package in grouppackages)
            {
                var familymember = _dbContext.FamilyMembers
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Reference)
                    .Where(e => e.FamilyHead.PackageId == package.PackageId && !e.FamilyHead.Trashed)
                    .OrderBy(e => e.HeadId)
                    .ThenBy(e => e.FamilyHeadId);

                model.AddRange(familymember);
            }


            ViewBag.selectedmemebers = _dbContext.GroupMembers.Where(e => e.GroupId != groupId && !e.Group.Trashed);
            //return View(
            //    _dbContext.FamilyMembers
            //        .Include(e => e.FamilyHead)
            //            .ThenInclude(e => e.Package)
            //                .ThenInclude(e => e.Company)
            //        .Include(e => e.FamilyHead)
            //            .ThenInclude(e => e.Reference)
            //        .Where(e => e.FamilyHead.PackageId == group.PackageId)
            //        .OrderBy(e => e.HeadId)
            //        .ThenBy(e => e.FamilyHeadId)
            //);
            return View(model);
        }
        [HttpPost]
        public IActionResult AddMember(int groupId, int memberId)
        {
            int paxLimit = _dbContext.GroupsWithoutTrash.FirstOrDefault(e => e.GroupId == groupId).PAX;
            int selected = _dbContext.GroupMembers.Where(e => e.GroupId == groupId).Count();

            var exists = _dbContext.GroupMembers
                .Where(e => e.FamilyHeadId == memberId)
                .Join(_dbContext.Groups, gm => gm.GroupId, g => g.GroupId, (gm, g) => new { gm, g })
                .FirstOrDefault(x => x.g.Trashed == false); 

            if (exists is null)
            {
                _dbContext.GroupMembers.Add(
                    new GroupMember { FamilyHeadId = memberId, GroupId = groupId }
                );

                _dbContext.GroupsWithoutTrash.FirstOrDefault(e => e.GroupId == groupId).Selected = selected + 1;

                _dbContext.SaveChanges();

                return Ok(new { status = 200, message = $"Member Id: {memberId} added to group Id: {groupId}" });
            }

            return Ok(new { status = 200, message = $"Member Id: {memberId} already exists in a valid group" });
        }
        [HttpPost]
        public IActionResult RemoveMember(int groupId, int memberId)
        {
            int selected = _dbContext.GroupMembers.Where(e => e.GroupId == groupId).Count();
            var sgm = _dbContext.SubGroupMembers.FirstOrDefault(e => e.FamilyHeadId == memberId && e.SubGroup.GroupId == groupId);
            if (sgm != null)
            {
                _dbContext.SubGroupMembers.Remove(sgm);
            }

            var gm = _dbContext.GroupMembers.FirstOrDefault(e => e.GroupId == groupId && e.FamilyHeadId == memberId);
            if (gm != null)
            {
                _dbContext.GroupMembers.Remove(gm);
                _dbContext.GroupsWithoutTrash.FirstOrDefault(e => e.GroupId == groupId).Selected = selected - 1;
            }

            _dbContext.SaveChanges();

            return Ok(new { status = 200, message = $"Member Id: {memberId} removed from group Id: {groupId}" });
        }
        public IActionResult GetGroups(int id)
        {
            var groups = _dbContext.GroupPackages
                .Include(e => e.Group)
                .Where(e => e.PackageId == id && e.Group.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(e => new
                {
                    GroupId = e.Group.GroupId,
                    Name = e.Group.Name
                })
                .ToList();

            return Ok(groups);
        }
    }
}

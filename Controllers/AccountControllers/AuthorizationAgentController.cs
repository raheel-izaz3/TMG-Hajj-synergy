using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.PackageModels;
using TMG.Models.AccountModels;

namespace TMG.Controllers.AccountControllers
{
    public class AuthorizationAgentController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public AuthorizationAgentController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.AuthorizationAgents
                .Where(e => e.Trashed).Count();

            /*ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);*/

            return View(_dbContext.AuthorizationAgents.Where(e => !e.Trashed).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(AuthorizationAgent agent)
        {
            if (!ModelState.IsValid) return View();

            //terms.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSessonId"));

            _dbContext.AuthorizationAgents.Add(agent);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            AuthorizationAgent terms = _dbContext.AuthorizationAgents.Find(id);

            return View(terms);
        }


        [HttpPost]
        public IActionResult Edit(AuthorizationAgent agent)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(agent).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.AuthorizationAgents.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            AuthorizationAgent Agent = _dbContext.AuthorizationAgents.Find(id);
            if (Agent == null) return RedirectToAction(nameof(Index));

            Agent.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            AuthorizationAgent Agent = _dbContext.AuthorizationAgents.Find(id);
            Agent.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

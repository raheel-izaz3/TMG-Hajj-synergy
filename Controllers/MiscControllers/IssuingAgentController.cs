using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.PackageModels;

namespace TMG.Controllers.MiscControllers
{
    public class IssuingAgentController: Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public IssuingAgentController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.IssuingAgents
                .Where(e => e.Trashed).Count();

            /*ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);*/

            return View(_dbContext.IssuingAgents.Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(IssuingAgent agent)
        {
            if (!ModelState.IsValid) return View();

            agent.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.IssuingAgents.Add(agent);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            IssuingAgent terms = _dbContext.IssuingAgents.Find(id);

            return View(terms);
        }


        [HttpPost]
        public IActionResult Edit(IssuingAgent agent)
        {
            if (!ModelState.IsValid) return View();

            agent.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.Entry(agent).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.IssuingAgents.Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            IssuingAgent Agent = _dbContext.IssuingAgents.Find(id);
            if (Agent == null) return RedirectToAction(nameof(Index));

            Agent.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            IssuingAgent Agent = _dbContext.IssuingAgents.Find(id);
            Agent.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.MiscControllers
{
    public class RequiredDocumentController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public RequiredDocumentController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RequiredDocuments.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashReqDocs(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateReqDocs(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditReqDocs(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreReqDocs(_dbContext, _http);

            return View(_dbContext.RequiredDocumentsWithoutTrash);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(RequiredDocument doc)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.RequiredDocuments.Add(doc);
            _dbContext.SaveChanges();


            var families = _dbContext.FamilyHeads.ToList();
            foreach (var f in families)
            {
                _dbContext.FamilyHeadDocuments.Add(new FamilyHeadDocument
                {
                    RequiredDocumentId = doc.RequiredDocumentId,
                    FamilyHeadId = f.FamilyHeadId,
                    Recieved = false,
                });
            }
            _dbContext.SaveChanges();


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            RequiredDocument RequiredDocument = _dbContext.RequiredDocuments.Find(id);

            return View(RequiredDocument);
        }


        [HttpPost]
        public IActionResult Edit(RequiredDocument doc)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(doc).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.RequiredDocuments.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            RequiredDocument doc = _dbContext.RequiredDocuments.Find(id);
            if (doc == null) return RedirectToAction(nameof(Index));

            doc.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            RequiredDocument doc = _dbContext.RequiredDocuments.Find(id);
            doc.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

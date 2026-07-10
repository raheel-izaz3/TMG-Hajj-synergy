using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.CompanyModels;

namespace TMG.Models.CompanyModels
{
    public record MunazzimIndexModel
    {
        public List<MunazzimIndexEntryModel> Entries { get; set; } = new();
    }

    public record MunazzimIndexEntryModel(int MunazzimId, int ClusterNumber, string Munazzim)
    {
        public List<string> CompanyNames { get; set; } = new();
    }
}

namespace TMG.Controllers.CompanyControllers
{
    public class MunazzimController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public MunazzimController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Munazzims
                .Where(e => e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(ca => ca.Trashed).Count();

            // ViewBag.TrashAllowed = PrivilegeManager.CanTrashMunazzim(_dbContext);
            // ViewBag.CreateAllowed = PrivilegeManager.CanCreateMunazzim(_dbContext);
            // ViewBag.EditAllowed = PrivilegeManager.CanEditMunazzim(_dbContext);
            // ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreMunazzim(_dbContext);

            var munazzims = _dbContext.Munazzims
            .Where(ca => !ca.Trashed)
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();
            var companies = _dbContext.MunazzimCompanies
                            .Include(e => e.Company)
                            .Select(e => new
                            {
                                e.Company.CompanyName,
                                e.Company.CompanyCode,
                                e.CompanyId,
                                e.MunazzimId,
                            })
                            .ToList();

            MunazzimIndexModel model = new();

            for (int i = 0; i < munazzims.Count; i++)
            {
                Munazzim currMunazzim = munazzims.ElementAt(i);
                MunazzimIndexEntryModel entry = new(
                    currMunazzim.MunazzimId, currMunazzim.ClusterNumber, currMunazzim.Name
                );

                List<string> munazzimCompanyNames = companies.Where(e => e.MunazzimId == currMunazzim.MunazzimId)
                                                    .Select(e => e.CompanyName)
                                                    .ToList();

                entry.CompanyNames = munazzimCompanyNames;
                model.Entries.Add(entry);
            }

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Companies = _dbContext
                                    .CompaniesWithoutTrash
                                    .Select(e => new
                                    {
                                        e.CompanyName,
                                        e.CompanyId
                                    });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Munazzim request)
        {
            ViewBag.Companies = _dbContext
                                                .CompaniesWithoutTrash
                                                .Select(e => new
                                                {
                                                    e.CompanyName,
                                                    e.CompanyId,
                                                });

            if (!ModelState.IsValid) return View(request);

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                string stampFolder = Path.Combine(_host.WebRootPath, "Images", "MunazzimStampImages");
                string signatureFolder = Path.Combine(_host.WebRootPath, "Images", "MunazzimSignatureImages");

                // Defensive Check: Automatically create the folders if they are missing
                if (!Directory.Exists(stampFolder)) Directory.CreateDirectory(stampFolder);
                if (!Directory.Exists(signatureFolder)) Directory.CreateDirectory(signatureFolder);

                string stampPath = Path.Combine(_host.WebRootPath, "Images", "MunazzimStampImages", request.StampFile.FileName);
                string signaturePath = Path.Combine(_host.WebRootPath, "Images", "MunazzimSignatureImages", request.SignatureFile.FileName);

                using (FileStream fs = new(stampPath, FileMode.Create))
                {
                    request.StampFile.CopyTo(fs);
                }

                using (FileStream fs = new(signaturePath, FileMode.Create))
                {
                    request.SignatureFile.CopyTo(fs);
                }

                request.SignaturePath = "Images/MunazzimSignatureImages/" + request.SignatureFile.FileName;
                request.StampPath = "Images/MunazzimStampImages/" + request.StampFile.FileName;
                request.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                _dbContext.Munazzims.Add(request);
                _dbContext.SaveChanges();

                // DEFENSIVE FIX: Ensure both company arrays were actually submitted and aren't null
                if (request.CompaniesId != null && request.SelectedCompanies != null)
                {
                    for (int i = 0; i < request.CompaniesId.Count; i++)
                    {
                        // Safe check in case the index sizes don't perfectly match
                        if (i >= request.SelectedCompanies.Count) break;
                        if (request.SelectedCompanies[i] == false) continue;

                        _dbContext.MunazzimCompanies.Add(new MunazzimCompanies
                        {
                            CompanyId = request.CompaniesId[i],
                            MunazzimId = request.MunazzimId,
                        });
                    }
                    _dbContext.SaveChanges();
                }

                transaction.Commit();

                TempData["created"] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return View(request);
            }
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Companies = _dbContext
                                                .CompaniesWithoutTrash
                                                .Select(e => new
                                                {
                                                    e.CompanyName,
                                                    e.CompanyId
                                                });
            Munazzim munazzim = _dbContext.Munazzims.Find(id);
            if (munazzim == null) return RedirectToAction(nameof(Index));

            ViewBag.AssignedCompanies = _dbContext.MunazzimCompanies
                                        .Where(e => e.MunazzimId == id);

            return View(munazzim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Munazzim request)
        {
            ViewBag.Companies = _dbContext
                                    .CompaniesWithoutTrash
                                    .Select(e => new
                                    {
                                        e.CompanyName,
                                        e.CompanyId
                                    });

            if (!ModelState.IsValid) return View(request);

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                request.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                _dbContext.Entry(request).State = EntityState.Modified;
                _dbContext.SaveChanges();

                _dbContext.MunazzimCompanies.Where(e => e.MunazzimId == request.MunazzimId).ExecuteDelete();
                for (int i = 0; i < request.CompaniesId.Count; i++)
                {
                    if (request.SelectedCompanies[i] == false) continue;

                    _dbContext.MunazzimCompanies.Add(new MunazzimCompanies
                    {
                        CompanyId = request.CompaniesId[i],
                        MunazzimId = request.MunazzimId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["updated"] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return View();
            }


        }

        public IActionResult Trash(int id)
        {
            Munazzim munazzim = _dbContext.Munazzims.Find(id);
            if (munazzim == null) return RedirectToAction(nameof(Index));

            munazzim.Trashed = true;
            _dbContext.SaveChanges();


            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            var munazzims = _dbContext.Munazzims
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(ca => ca.Trashed).ToList();
            var companies = _dbContext.MunazzimCompanies
                            .Include(e => e.Company)
                            .Select(e => new
                            {
                                e.Company.CompanyName,
                                e.Company.CompanyCode,
                                e.CompanyId,
                                e.MunazzimId,
                            })
                            .ToList();

            MunazzimIndexModel model = new();

            for (int i = 0; i < munazzims.Count; i++)
            {
                Munazzim currMunazzim = munazzims.ElementAt(i);
                MunazzimIndexEntryModel entry = new(
                    currMunazzim.MunazzimId, currMunazzim.ClusterNumber, currMunazzim.Name
                );

                List<string> munazzimCompanyNames = companies.Where(e => e.MunazzimId == currMunazzim.MunazzimId)
                                                    .Select(e => e.CompanyName)
                                                    .ToList();

                entry.CompanyNames = munazzimCompanyNames;
                model.Entries.Add(entry);
            }

            return View(model);
        }

        public IActionResult Restore(int id)
        {
            Munazzim company = _dbContext.Munazzims.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

    }
}

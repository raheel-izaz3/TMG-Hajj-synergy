using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Models.FormModels;
using TMG.Models.FamilyModels;
using Rotativa;

namespace TMG.Controllers.MiscControllers
{
    public class CompanyVisaController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyVisaController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.CompanyVisas.Where(e => e.Trashed).Count();

            /*ViewBag.TrashAllowed = PrivilegeManager.CanTrashFiqah(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateFiqah(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditFiqah(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFiqah(_dbContext, _http);*/

            return View(_dbContext.CompanyVisas.Include(e => e.Vendor).Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Create()
        {

            ViewBag.Vendors = _dbContext.Vendors
                .Where(e => !e.Trashed & e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            /*ViewBag.Companies = _dbContext.CompaniesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();*/

            return View();
        }


        [HttpPost]
        public IActionResult Create(CompanyVisaRequest visa)
        {
            if (!ModelState.IsValid)
                return View(visa);

            var HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            // Step 1: Create CompanyVisas entity
            var companyVisa = new CompanyVisas
            {
                MaktabNumber = visa.MaktabNumber,
                Location = visa.Location,
                CompanyName = visa.CompanyName,
                Category = visa.Category,
                OfficeAddress = visa.OfficeAddress,
                ContactPersonName = visa.ContactPersonName,
                VendorId = visa.VendorId ?? null,
                HajjSeasonsId = HajjSeasonsId,
                MunazimName = visa.MunazimName,
                SaudiMunazimNo = visa.SaudiMunazimNo,
                HGO = visa.HGO,
                MoraNo = visa.MoraNo,
            };

            // Step 2: Add to DB and save to generate ID
            _dbContext.CompanyVisas.Add(companyVisa);
            _dbContext.SaveChanges(); // So we can get generated CompanyVisasId

            // Step 3: Create related contacts
            if (visa.ContactNumber != null && visa.ContactNumber.Any())
            {
                foreach (var number in visa.ContactNumber)
                {
                    if (!string.IsNullOrWhiteSpace(number))
                    {
                        var contact = new CompanyVisaContacts
                        {
                            ContactNumber = number,
                            CompanyVisasId = companyVisa.CompanyVisasId
                        };
                        _dbContext.CompanyVisaContacts.Add(contact);
                    }
                }
            }

            // Step 4: Save all contacts
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            ViewBag.Vendors = _dbContext.Vendors
                .Where(e => !e.Trashed & e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            // Step 1: Fetch CompanyVisa including related contacts
            var companyVisa = _dbContext.CompanyVisas
                .Include(cv => cv.Vendor)
                .Include(cv => cv.CompanyVisaContacts) // if navigation property exists
                .FirstOrDefault(cv => cv.CompanyVisasId == id);

            if (companyVisa == null)
            {
                return NotFound();
            }

            // Step 2: Fetch related contacts manually (if no navigation property exists)
            ViewBag.Contacts = _dbContext.CompanyVisaContacts
                .Where(c => c.CompanyVisasId == id)
                .ToList();

            // Step 3: Map to form model
            var model = new CompanyVisaEditRequest
            {
                CompanyVisasId = companyVisa.CompanyVisasId,
                MaktabNumber = companyVisa.MaktabNumber,
                Location = companyVisa.Location,
                CompanyName = companyVisa.CompanyName,
                Category = companyVisa.Category,
                OfficeAddress = companyVisa.OfficeAddress,
                ContactPersonName = companyVisa.ContactPersonName,
                VendorId = companyVisa.VendorId,
                MunazimName = companyVisa.MunazimName,
                SaudiMunazimNo = companyVisa.SaudiMunazimNo,
                HGO = companyVisa.HGO,
                MoraNo = companyVisa.MoraNo,
                //ContactNumber = contacts,
                //CompanyVisasId = new List<int> { companyVisa.CompanyVisasId }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(CompanyVisaEditRequest visa)
        {
            if (!ModelState.IsValid)
                return View(visa);


            // Step 1: Get the existing visa entry
            var existingVisa = _dbContext.CompanyVisas
                .Include(e => e.CompanyVisaContacts)
                .FirstOrDefault(cv => cv.CompanyVisasId == visa.CompanyVisasId);

            if (existingVisa == null)
                return NotFound();

            // Step 2: Update visa fields
            existingVisa.MaktabNumber = visa.MaktabNumber;
            existingVisa.Location = visa.Location;
            existingVisa.CompanyName = visa.CompanyName;
            existingVisa.Category = visa.Category;
            existingVisa.OfficeAddress = visa.OfficeAddress;
            existingVisa.ContactPersonName = visa.ContactPersonName;
            existingVisa.VendorId = visa.VendorId ?? null;
            existingVisa.MunazimName = visa.MunazimName;
            existingVisa.SaudiMunazimNo = visa.SaudiMunazimNo;
            existingVisa.HGO = visa.HGO;
            existingVisa.MoraNo = visa.MoraNo;


            // 🔴 Step 3: Delete old contacts
            var oldContacts = _dbContext.CompanyVisaContacts
                .Where(c => c.CompanyVisasId == visa.CompanyVisasId)
                .ToList();

            _dbContext.CompanyVisaContacts.RemoveRange(oldContacts);

            // ✅ Step 4: Add updated contacts from model
            if (visa.ContactNumber != null && visa.ContactNumber.Any())
            {
                foreach (var number in visa.ContactNumber)
                {
                    if (!string.IsNullOrWhiteSpace(number))
                    {
                        var contact = new CompanyVisaContacts
                        {
                            CompanyVisasId = visa.CompanyVisasId,
                            ContactNumber = number
                        };
                        _dbContext.CompanyVisaContacts.Add(contact);
                    }
                }
            }

            // Step 5: Save all changes
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult ViewTrash()
        {
            return View(_dbContext.CompanyVisas.Include(e => e.Vendor).Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            CompanyVisas visa = _dbContext.CompanyVisas.Find(id);
            if (visa == null) return RedirectToAction(nameof(Index));

            visa.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            CompanyVisas visa = _dbContext.CompanyVisas.Find(id);
            if (visa == null) return RedirectToAction(nameof(Index));

            visa.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Print(int id)
        {
            ViewBag.Name = _dbContext.CompanyVisas
                .Where(e => e.CompanyVisasId == id)
                .Select(e => e.CompanyName)
                .FirstOrDefault();

            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.CompanyVisas)
                .Include(e => e.Package)
                .Where(e => e.CompanyVisasId == id);

            var result = query.Select(e => new SummaryDataCompany
            {
                HajjiName = e.FullName,
                HbNo = e.FcId,
                Gender = e.Gender.ToString(),
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

    }

    public record SummaryDataCompany
    {
        public string HajjiName { get; set; }
        public string HbNo { get; set; }
        public string Gender { get; set; }
        public string PackageName { get; set; }
        public string PassportNo { get; set; }
    }
}

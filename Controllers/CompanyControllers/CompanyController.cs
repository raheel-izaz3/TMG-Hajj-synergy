using TMG.Data;
using TMG.ManagerClasses;
using Microsoft.AspNetCore.Mvc;
using TMG.Models.CompanyModels;
using TMG.Models.FormModels.CompanyModels;
using Microsoft.EntityFrameworkCore;
using TMG.Models.PackageModels;
using TMG.Models.AccountModels;
using System.Transactions;
using TMG.Models.MiscModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TMG.Models.CompanyModels
{
    public record CompanyIndexModel
    {
        public List<CompanyIndexModelEntry> Entries { get; set; } = new List<CompanyIndexModelEntry>();
    }

    public record CompanyIndexModelEntry
    {
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public int? ActualQuota { get; set; }
        public int? RemainingQuota { get; set; }
        public List<CompanyPackageListModel> Packages { get; set; }
    }

    public sealed record CompanyPackageListModel(int PackageId, string Name, int HajjBookingCount);
}

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _http;

        public CompanyController(TMGDbContext dbContext, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _http = http;

        }

        public IActionResult Index()
        {
            int userId = Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"));
            int activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            ViewBag.TrashCount = _dbContext.Companies.Where(c => c.Trashed).Count();
            ViewBag.CompanyPackages = _dbContext.PackagesWithoutTrash;
            ViewBag.CompanyPackagesCount = _dbContext.PackagesWithoutTrash.Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashCompany(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateCompany(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditCompany(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCompany(_dbContext, _http);
            ViewBag.CombineAllowed = PrivilegeManager.CanViewCombineReports(_dbContext, _http);
            ViewBag.IndividualAllowed = PrivilegeManager.CanViewIndividualReports(_dbContext, _http);

            var companies = _dbContext.UserCompanies
                .Include(e => e.Company)
                .Where(e => e.UserId == userId && !e.Company.Trashed)
                .Select(e => new
                {
                    e.CompanyId,
                    e.Company.CompanyName,
                    e.Company.Website,
                    e.Company.CompanyCode,
                })
                .ToList();

            var companyQuotas = _dbContext.QuotaCompanies
                .Where(e => e.HajjSeasonsId == activeSeasonId)
                .ToDictionary(e => e.CompanyId, e => e.Quota);

            var packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == activeSeasonId)
                .Select(e => new
                {
                    e.PackageId,
                    e.CompanyId,
                    e.Code
                })
                .ToList();

            var familyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == activeSeasonId)
                .Select(e => new
                {
                    e.FamilyHeadId,
                    e.PackageId,
                    e.Package.CompanyId
                })
                .ToList();

            CompanyIndexModel model = new();

            foreach (var company in companies)
            {
                var companyPackages = packages
                    .Where(e => e.CompanyId == company.CompanyId)
                    .ToList();

                List<CompanyPackageListModel> packagesData = new();

                foreach (var pkg in companyPackages)
                {
                    int count = familyHeads.Count(e => e.PackageId == pkg.PackageId);
                    packagesData.Add(new CompanyPackageListModel(pkg.PackageId, pkg.Code, count));
                }

                int usedQuota = familyHeads
                    .Where(e => e.CompanyId == company.CompanyId)
                    .Count();

                int? actualQuota = companyQuotas.ContainsKey(company.CompanyId) ? companyQuotas[company.CompanyId] : 0;
                int? remainingQuota = actualQuota - usedQuota;

                model.Entries.Add(new CompanyIndexModelEntry
                {
                    CompanyId = company.CompanyId,
                    Name = company.CompanyName,
                    ActualQuota = actualQuota,
                    Website = company.Website,
                    Code = company.CompanyCode,
                    Packages = packagesData,
                    RemainingQuota = remainingQuota
                });
            }

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.AddressTypes = _dbContext.CompanyAddressTypes;
            ViewBag.ContactTypes = _dbContext.CompanyContactTypes;
            ViewBag.EmailTypes = _dbContext.CompanyEmailTypes;
            ViewBag.LicenseTypes = _dbContext.CompanyLicenseTypes;
            ViewBag.CurrencyType = _dbContext.CurrencyTypes.Where(ct => !ct.Trashed).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompanyRegisterRequest request)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    ViewBag.AddressTypes = _dbContext.CompanyAddressTypes;
                    ViewBag.ContactTypes = _dbContext.CompanyContactTypes;
                    ViewBag.EmailTypes = _dbContext.CompanyEmailTypes;
                    ViewBag.LicenseTypes = _dbContext.CompanyLicenseTypes;
                    ViewBag.CurrencyType = _dbContext.CurrencyTypes.Where(ct => !ct.Trashed).ToList();

                    // Basic validations
                    if (request.AddressTypeId.Count == 0)
                        ModelState.AddModelError("Address", "Add at least 1 Address.");
                    if (request.ContactTypeId.Count == 0)
                        ModelState.AddModelError("Contact", "Add at least 1 Contact.");
                    if (request.EmailTypeId.Count == 0)
                        ModelState.AddModelError("Email", "Add at least 1 Email.");
                    if (request.LicenseTypeId.Count == 0)
                        ModelState.AddModelError("LicenseNo", "Add at least 1 License.");
                    if (request.LogoFile == null)
                        ModelState.AddModelError("LogoFile", "Please upload an image.");
                    if (request.StampFile == null)
                        ModelState.AddModelError("StampFile", "Please upload an image.");

                    // Validate extensions
                    string logoExtension = request.LogoFile != null ? Path.GetExtension(request.LogoFile.FileName) : "";
                    string stampExtension = request.StampFile != null ? Path.GetExtension(request.StampFile.FileName) : "";
                    string directorPhotoExtension = request.DirectorPhotoFile != null ? Path.GetExtension(request.DirectorPhotoFile.FileName) : "";

                    if (request.LogoFile != null && !ExtensionManager.AllowedExtensionsForImages.Contains(logoExtension))
                        ModelState.AddModelError(CompanyRegisterRequest.LogoFileKey, "Please choose an image with a proper extension.");
                    if (request.StampFile != null && !ExtensionManager.AllowedExtensionsForImages.Contains(stampExtension))
                        ModelState.AddModelError(CompanyRegisterRequest.StampFileKey, "Please choose an image with a proper extension.");
                    if (request.DirectorPhotoFile != null && !ExtensionManager.AllowedExtensionsForImages.Contains(directorPhotoExtension))
                        ModelState.AddModelError("DirectorPhotoFile", "Please choose a valid image file for Director Photo.");

                    if (!ModelState.IsValid)
                        return View(request);

                    // ============================= FILE PATH SETUP =============================
                    string logoDirectoryPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyLogos");
                    string stampDirectoryPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyStamps");
                    string letterHeaderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "LetterHeader");
                    string letterFooterPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "LetterFooter");
                    string signaturePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanySignatures");
                    string directorPhotoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "DirectorPhotos");

                    Directory.CreateDirectory(logoDirectoryPath);
                    Directory.CreateDirectory(stampDirectoryPath);
                    Directory.CreateDirectory(letterHeaderPath);
                    Directory.CreateDirectory(letterFooterPath);
                    Directory.CreateDirectory(signaturePath);
                    Directory.CreateDirectory(directorPhotoPath);

                    // ============================= FILE SAVING =============================
                    string companyLogoPath = Path.Combine(logoDirectoryPath, request.LogoFile.FileName);
                    string companyStampPath = Path.Combine(stampDirectoryPath, request.StampFile.FileName);
                    string companyHeaderPath = Path.Combine(letterHeaderPath, request.Header.FileName);
                    string companyFooterPath = Path.Combine(letterFooterPath, request.Footer.FileName);
                    string companySignaturePath = Path.Combine(signaturePath, request.Signatures.FileName);
                    string companyDirectorPhotoPath = request.DirectorPhotoFile != null
                        ? Path.Combine(directorPhotoPath, request.DirectorPhotoFile.FileName)
                        : null;

                    if (!string.IsNullOrEmpty(companyLogoPath)) { string dir_dt = Path.GetDirectoryName(companyLogoPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companyLogoPath, FileMode.Create))
                        request.LogoFile.CopyTo(fs);
                    if (!string.IsNullOrEmpty(companyStampPath)) { string dir_dt = Path.GetDirectoryName(companyStampPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companyStampPath, FileMode.Create))
                        request.StampFile.CopyTo(fs);
                    if (!string.IsNullOrEmpty(companyHeaderPath)) { string dir_dt = Path.GetDirectoryName(companyHeaderPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companyHeaderPath, FileMode.Create))
                        request.Header.CopyTo(fs);
                    if (!string.IsNullOrEmpty(companyFooterPath)) { string dir_dt = Path.GetDirectoryName(companyFooterPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companyFooterPath, FileMode.Create))
                        request.Footer.CopyTo(fs);
                    if (!string.IsNullOrEmpty(companySignaturePath)) { string dir_dt = Path.GetDirectoryName(companySignaturePath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companySignaturePath, FileMode.Create))
                        request.Signatures.CopyTo(fs);
                    if (request.DirectorPhotoFile != null)
                    {
                        if (!string.IsNullOrEmpty(companyDirectorPhotoPath)) { string dir_dt = Path.GetDirectoryName(companyDirectorPhotoPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                        using (FileStream fs = new FileStream(companyDirectorPhotoPath, FileMode.Create))
                            request.DirectorPhotoFile.CopyTo(fs);
                    }

                    // ============================= CREATE COMPANY =============================
                    Company company = new Company
                    {
                        CompanyName = request.CompanyName?.ToUpper(),
                        CompanyCode = request.CompanyCode?.ToUpper(),
                        EstablishmentDate = request.EstablishmentDate,
                        Website = request.Website?.ToUpper(),
                        CompanyStatus = request.CompanyStatus?.ToUpper(),
                        CurrentStatus = request.CurrentStatus?.ToUpper(),
                        CurrencyTypeId = request.CurrencyTypeId ?? 0,
                        Logo = @"Images/CompanyLogos/" + request.LogoFile.FileName,
                        Stamp = @"Images/CompanyStamps/" + request.StampFile.FileName,
                        LetterHead = @"Images/LetterHeader/" + request.Header.FileName,
                        LetterHeadFooter = @"Images/LetterFooter/" + request.Footer.FileName,
                        Signature = @"Images/CompanySignatures/" + request.Signatures.FileName,
                        // ========== NEW FIELDS ==========
                        MohraEnrollmentNo = request.MohraEnrollmentNo?.ToUpper(),
                        MunazzamNo = request.MunazzamNo?.ToUpper(),
                        ClusterEnrollmentNo = request.ClusterEnrollmentNo?.ToUpper(),
                        DTSNo = request.DTSNo?.ToUpper(),
                        DTSExpiry = request.DTSExpiry,
                        IATANo = request.IATANo?.ToUpper(),
                        IATAExpiry = request.IATAExpiry,
                        NTN = request.NTN?.ToUpper(),
                        DirectorName = request.DirectorName?.ToUpper(),
                        DirectorPhoto = request.DirectorPhotoFile != null ? @"Images/DirectorPhotos/" + request.DirectorPhotoFile.FileName : null,
                        DirectorCNIC = request.DirectorCNIC?.ToUpper(),
                        DirectorCNICExpiry = request.DirectorCNICExpiry,
                        DirectorDetail = request.DirectorDetail,
                        BankName = request.BankName?.ToUpper(),
                        AccountTitle = request.AccountTitle?.ToUpper(),
                        Branch = request.Branch?.ToUpper(),
                        AccountNumber = request.AccountNumber
                    };

                    _dbContext.Companies.Add(company);
                    _dbContext.SaveChanges();

                    // ============================= OTHER LOGIC SAME AS BEFORE =============================
                    _dbContext.QuotaCompanies.Add(new QuotaCompany
                    {
                        CompanyId = company.CompanyId,
                        Quota = request.Quota,
                        HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    });
                    _dbContext.SaveChanges();

                    _dbContext.UserCompanies.Add(new UserCompanies
                    {
                        CompanyId = company.CompanyId,
                        UserId = 1
                    });
                    _dbContext.SaveChanges();

                    // Addresses
                    foreach (var i in Enumerable.Range(0, request.AddressTypeId.Count))
                    {
                        _dbContext.CompanyAddresses.Add(new CompanyAddresses
                        {
                            CompanyId = company.CompanyId,
                            CompanyAddressId = request.AddressTypeId[i],
                            Address = request.Address[i]?.ToUpper(),
                            Country = request.Country[i]?.ToUpper(),
                            City = request.City[i]?.ToUpper(),
                            PoBox = request.PoBox[i]?.ToUpper(),
                            ZipCode = request.Zip[i]?.ToUpper(),
                            Preferred = request.AddressPreferred[i]
                        });
                    }

                    // Contacts
                    foreach (var i in Enumerable.Range(0, request.ContactTypeId.Count))
                    {
                        _dbContext.CompanyContacts.Add(new CompanyContacts
                        {
                            CompanyContactTypeId = request.ContactTypeId[i],
                            CompanyContact = request.Contact[i]?.ToUpper(),
                            CompanyId = company.CompanyId,
                            Preferred = request.ContactPreferred[i]
                        });
                    }

                    // Emails
                    foreach (var i in Enumerable.Range(0, request.EmailTypeId.Count))
                    {
                        _dbContext.CompanyEmails.Add(new CompanyEmails
                        {
                            CompanyEmailTypeId = request.EmailTypeId[i],
                            CompanyEmail = request.Email[i]?.ToUpper(),
                            CompanyId = company.CompanyId,
                            Preferred = request.EmailPreferred[i]
                        });
                    }

                    // Licenses
                    foreach (var i in Enumerable.Range(0, request.LicenseTypeId.Count))
                    {
                        _dbContext.CompanyLicenses.Add(new CompanyLicenses
                        {
                            CompanyLicenseTypeId = request.LicenseTypeId[i],
                            CompanyLicense = request.LicenseNo[i]?.ToUpper(),
                            IssuePlace = request.IssuePlace[i]?.ToUpper(),
                            IssueDate = DateTimeManager.ParseHtmlDate(request.IssueDate[i]),
                            ValidUpto = DateTimeManager.ParseHtmlDate(request.ValidDate[i]),
                            CompanyId = company.CompanyId,
                            Preferred = request.LicensePreferred[i]
                        });
                    }

                    _dbContext.SaveChanges();

                    // Sub Types and Accounts setup (same as before)
                    var subTypeCurrAsset = new SubType { CompanyId = company.CompanyId, TypeId = 1, Name = "Current Asset" };
                    var subTypeSale = new SubType { CompanyId = company.CompanyId, TypeId = 3, Name = "Sale" };
                    _dbContext.SubTypes.AddRange(subTypeCurrAsset, subTypeSale);
                    _dbContext.SaveChanges();

                    var categoryRec = new Category { SubTypeId = subTypeCurrAsset.SubTypeId, Name = "Receivable", CompanyReceivable = true };
                    var categorySale = new Category { SubTypeId = subTypeSale.SubTypeId, Name = "Service Sale" };
                    var categoryBank = new Category { SubTypeId = subTypeCurrAsset.SubTypeId, Name = "Bank", CompanyBank = true };
                    _dbContext.Categories.AddRange(categoryRec, categorySale, categoryBank);
                    _dbContext.SaveChanges();

                    var accountSale = new Account { CategoryId = categorySale.CategoryId, Name = $"Sale - {company.CompanyCode}", CompanySaleAccount = true };
                    _dbContext.Accounts.Add(accountSale);
                    _dbContext.SaveChanges();

                    transaction.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // log ex.Message if needed
                    return View(request);
                }
            }
        }



        public IActionResult Detail(int id)
        {

            ViewBag.Addresses = _dbContext.CompanyAddresses.Include(e => e.CompanyAddress).Where(e => e.CompanyId == id);
            ViewBag.Emails = _dbContext.CompanyEmails.Include(e => e.CompanyEmailType).Where(e => e.CompanyId == id);
            ViewBag.Contacts = _dbContext.CompanyContacts.Include(e => e.CompanyContactType).Where(e => e.CompanyId == id);
            ViewBag.Licenses = _dbContext.CompanyLicenses.Include(e => e.CompanyLicenseType).Where(e => e.CompanyId == id);

            return View(_dbContext.Companies.Find(id));
        }

        public IActionResult Trash(int id)
        {
            Company company = _dbContext.Companies.Find(id);
            if (company == null) return RedirectToAction(nameof(Index));

            company.Trashed = true;

            //_dbContext.UserCompanies.Where(uc => uc.CompanyId == id).ExecuteDelete();

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Companies.Where(u => u.Trashed));
        }

        public IActionResult Restore(int id)
        {
            Company company = _dbContext.Companies.Find(id);
            company.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.AddressTypes = _dbContext.CompanyAddressTypes;
            ViewBag.ContactTypes = _dbContext.CompanyContactTypes;
            ViewBag.EmailTypes = _dbContext.CompanyEmailTypes;
            ViewBag.LicenseTypes = _dbContext.CompanyLicenseTypes;
            ViewBag.CurrencyType = _dbContext.CurrencyTypes.Where(ct => !ct.Trashed).ToList();

            ViewBag.AssignedAddresses = _dbContext.CompanyAddresses.Where(ca => ca.CompanyId == id);
            ViewBag.AssignedContacts = _dbContext.CompanyContacts.Where(cc => cc.CompanyId == id);
            ViewBag.AssignedEmails = _dbContext.CompanyEmails.Where(cc => cc.CompanyId == id);
            ViewBag.AssignedLicenses = _dbContext.CompanyLicenses.Where(cl => cl.CompanyId == id);

            Company company = _dbContext.Companies.FirstOrDefault(c => c.CompanyId == id);

            int activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            int? quota = _dbContext.QuotaCompanies
                .Where(q => q.CompanyId == id && q.HajjSeasonsId == activeSeasonId)
                .Select(q => q.Quota)
                .FirstOrDefault();

            CompanyEditRequest request = new CompanyEditRequest
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                CompanyCode = company.CompanyCode,
                CompanyStatus = company.CompanyStatus,
                Quota = quota, 
                EstablishmentDate = company.EstablishmentDate,
                Website = company.Website,
                CurrentStatus = company.CurrentStatus,
                CurrencyTypeId = company.CurrencyTypeId,
                LetterHead = company.LetterHead,
                LetterHeadFooter = company.LetterHeadFooter,
                // ✅ Map new fields
                MohraEnrollmentNo = company.MohraEnrollmentNo,
                MunazzamNo = company.MunazzamNo,
                ClusterEnrollmentNo = company.ClusterEnrollmentNo,
                DTSNo = company.DTSNo,
                DTSExpiry = company.DTSExpiry,
                IATANo = company.IATANo,
                IATAExpiry = company.IATAExpiry,
                NTN = company.NTN,

                DirectorName = company.DirectorName,
                DirectorCNIC = company.DirectorCNIC,
                DirectorCNICExpiry = company.DirectorCNICExpiry,
                DirectorDetail = company.DirectorDetail,
                //DirectorPhoto = company.DirectorPhoto,

                BankName = company.BankName,
                AccountTitle = company.AccountTitle,
                Branch = company.Branch,
                AccountNumber = company.AccountNumber
            };

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CompanyEditRequest request)
        {
            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                ViewBag.AddressTypes = _dbContext.CompanyAddressTypes;
                ViewBag.ContactTypes = _dbContext.CompanyContactTypes;
                ViewBag.EmailTypes = _dbContext.CompanyEmailTypes;
                ViewBag.LicenseTypes = _dbContext.CompanyLicenseTypes;

                ViewBag.AssignedAddresses = _dbContext.CompanyAddresses.Where(ca => ca.CompanyId == request.CompanyId);
                ViewBag.AssignedContacts = _dbContext.CompanyContacts.Where(cc => cc.CompanyId == request.CompanyId);
                ViewBag.AssignedEmails = _dbContext.CompanyEmails.Where(cc => cc.CompanyId == request.CompanyId);
                ViewBag.AssignedLicenses = _dbContext.CompanyLicenses.Where(cl => cl.CompanyId == request.CompanyId);


                if (request.AddressTypeId.Count == 0)
                {
                    ModelState.AddModelError("Address", "Add Atleast 1 Address.");
                }
                if (request.ContactTypeId.Count == 0)
                {
                    ModelState.AddModelError("Contact", "Add Atleast 1 Contact.");
                }
                if (request.EmailTypeId.Count == 0)
                {
                    ModelState.AddModelError("Email", "Add Atleast 1 Email.");
                }
                if (request.LicenseTypeId.Count == 0)
                {
                    ModelState.AddModelError("LicenseNo", "Add Atleast 1 License.");
                }

                if (!ModelState.IsValid) return View();

                string letterHeaderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "LetterHeader");
                string letterFoooterPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "LetterFooter");
                string signature = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanySignatures");
                string directorPhotoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "DirectorPhotos");


                Directory.CreateDirectory(letterHeaderPath);
                Directory.CreateDirectory(letterFoooterPath);
                Directory.CreateDirectory(signature);
                Directory.CreateDirectory(directorPhotoPath);

                if (request.Header is not null)
                {
                    string companyHeaderPath = Path.Combine(letterHeaderPath, request.Header.FileName);

                    if (!string.IsNullOrEmpty(companyHeaderPath)) { string dir_dt = Path.GetDirectoryName(companyHeaderPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companyHeaderPath, FileMode.Create))
                    {
                        request.Header.CopyTo(fs);
                    }
                }

                if (request.Footer is not null)
                {
                    string companyFooterPath = Path.Combine(letterFoooterPath, request.Footer.FileName);

                    if (!string.IsNullOrEmpty(companyFooterPath)) { string dir_dt = Path.GetDirectoryName(companyFooterPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companyFooterPath, FileMode.Create))
                    {
                        request.Footer.CopyTo(fs);
                    }

                }

                if (request.Signatures is not null)
                {
                    string companySignature = Path.Combine(signature, request.Signatures.FileName);


                    if (!string.IsNullOrEmpty(companySignature)) { string dir_dt = Path.GetDirectoryName(companySignature); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(companySignature, FileMode.Create))
                    {
                        request.Signatures.CopyTo(fs);
                    }
                }
                if (request.DirectorPhoto != null)
                {
                    using var fs = new FileStream(Path.Combine(directorPhotoPath, request.DirectorPhoto.FileName), FileMode.Create);
                    request.DirectorPhoto.CopyTo(fs);
                }

                Company companyToEdit = _dbContext.Companies.Find(request.CompanyId);
                companyToEdit.CompanyName = request.CompanyName?.ToUpper();
                companyToEdit.CompanyCode = request.CompanyCode?.ToUpper();
                //companyToEdit.Quota = request.Quota;
                companyToEdit.CompanyStatus = request.CompanyStatus?.ToUpper();
                companyToEdit.CurrentStatus = request.CurrentStatus?.ToUpper();
                companyToEdit.Website = request.Website?.ToUpper();
                companyToEdit.EstablishmentDate = request.EstablishmentDate;
                companyToEdit.CurrencyTypeId = request.CurrencyTypeId.HasValue
                ? request.CurrencyTypeId.Value
                : 0;
                companyToEdit.MohraEnrollmentNo = request.MohraEnrollmentNo?.ToUpper();
                companyToEdit.MunazzamNo = request.MunazzamNo?.ToUpper();
                companyToEdit.ClusterEnrollmentNo = request.ClusterEnrollmentNo?.ToUpper();
                companyToEdit.DTSNo = request.DTSNo?.ToUpper();
                companyToEdit.DTSExpiry = request.DTSExpiry;
                companyToEdit.IATANo = request.IATANo?.ToUpper();
                companyToEdit.IATAExpiry = request.IATAExpiry;
                companyToEdit.NTN = request.NTN?.ToUpper();
                companyToEdit.DirectorName = request.DirectorName?.ToUpper();
                companyToEdit.DirectorCNIC = request.DirectorCNIC?.ToUpper();
                companyToEdit.DirectorCNICExpiry = request.DirectorCNICExpiry;
                companyToEdit.DirectorDetail = request.DirectorDetail;
                companyToEdit.BankName = request.BankName?.ToUpper();
                companyToEdit.AccountTitle = request.AccountTitle?.ToUpper();
                companyToEdit.Branch = request.Branch?.ToUpper();
                companyToEdit.AccountNumber = request.AccountNumber;
                companyToEdit.UpdatedAt = DateTime.Now;

                if (request.DirectorPhoto != null)
                    companyToEdit.DirectorPhoto = @"Images/DirectorPhotos/" + request.DirectorPhoto.FileName;

                int activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                bool exists = _dbContext.QuotaCompanies
                    .Any(e => e.CompanyId == request.CompanyId && e.HajjSeasonsId == activeSeasonId);

                if (exists)
                {
                    _dbContext.QuotaCompanies
                        .Where(e => e.CompanyId == request.CompanyId && e.HajjSeasonsId == activeSeasonId)
                        .ExecuteDelete();
                }

                _dbContext.QuotaCompanies.Add(new QuotaCompany
                {
                    CompanyId = request.CompanyId,
                    Quota = request.Quota,
                    HajjSeasonsId = activeSeasonId
                });

                _dbContext.SaveChanges();

                if (request.Header is not null)
                {
                    companyToEdit.LetterHead = @"Images/LetterHeader/" + request.Header.FileName;
                }
                if (request.Footer is not null)
                {
                    companyToEdit.LetterHeadFooter = @"Images/LetterFooter/" + request.Footer.FileName;
                }
                if (request.Signatures is not null)
                {
                    companyToEdit.Signature = @"Images/CompanySignatures/" + request.Signatures.FileName;
                }

                _dbContext.CompanyAddresses.Where(c => c.CompanyId == request.CompanyId).ExecuteDelete();
                _dbContext.CompanyContacts.Where(c => c.CompanyId == request.CompanyId).ExecuteDelete();
                _dbContext.CompanyEmails.Where(c => c.CompanyId == request.CompanyId).ExecuteDelete();
                _dbContext.CompanyLicenses.Where(c => c.CompanyId == request.CompanyId).ExecuteDelete();

                for (int i = 0; i < request.AddressTypeId.Count; i++)
                {
                    CompanyAddresses addresses = new CompanyAddresses
                    {
                        CompanyId = request.CompanyId,
                        CompanyAddressId = request.AddressTypeId[i],
                        Address = request.Address[i]?.ToUpper(),
                        Country = request.Country[i]?.ToUpper(),
                        City = request.City[i]?.ToUpper(),
                        PoBox = request.PoBox[i]?.ToUpper(),
                        ZipCode = request.Zip[i]?.ToUpper(),
                        Preferred = request.AddressPreferred[i]

                    };

                    _dbContext.CompanyAddresses.Add(addresses);
                }

                for (int i = 0; i < request.ContactTypeId.Count; i++)
                {
                    CompanyContacts companyContacts = new CompanyContacts
                    {
                        CompanyContactTypeId = request.ContactTypeId[i],
                        CompanyContact = request.Contact[i]?.ToUpper(),
                        CompanyId = request.CompanyId,
                        Preferred = request.ContactPreferred[i]
                    };

                    _dbContext.CompanyContacts.Add(companyContacts);
                }

                for (int i = 0; i < request.EmailTypeId.Count; i++)
                {
                    CompanyEmails companyEmails = new CompanyEmails
                    {
                        CompanyEmailTypeId = request.EmailTypeId[i],
                        CompanyEmail = request.Email[i]?.ToUpper(),
                        CompanyId = request.CompanyId,
                        Preferred = request.EmailPreferred[i]
                    };

                    _dbContext.CompanyEmails.Add(companyEmails);
                }

                for (int i = 0; i < request.LicenseTypeId.Count; i++)
                {
                    CompanyLicenses companyLicenses = new CompanyLicenses
                    {
                        CompanyLicenseTypeId = request.LicenseTypeId[i],
                        CompanyLicense = request.LicenseNo[i]?.ToUpper(),
                        IssuePlace = request.IssuePlace[i]?.ToUpper(),
                        IssueDate = DateTimeManager.ParseHtmlDate(request.IssueDate[i]),
                        ValidUpto = DateTimeManager.ParseHtmlDate(request.ValidDate[i]),
                        CompanyId = request.CompanyId,
                        Preferred = request.LicensePreferred[i]
                    };
                    _dbContext.CompanyLicenses.Add(companyLicenses);
                }

                _dbContext.SaveChanges();

                transaction.Commit();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(request);
            }

        }

        public IActionResult ChangeLogo(int id) => View(_dbContext.Companies.Find(id));

        public IActionResult ChangeStamp(int id) => View(_dbContext.Companies.Find(id));

        public IActionResult ChangeLetterHeadHajj(int id) => View(_dbContext.Companies.Find(id));

        public IActionResult ChangeLetterHeadCompany(int id) => View(_dbContext.Companies.Find(id));

        [HttpPost]
        public IActionResult ChangeLogo(Company request)
        {

            try
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyLogos", request.LogoFile.FileName);

                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using FileStream fs = new(path, FileMode.Create);

                request.LogoFile.CopyTo(fs);

                Company c = _dbContext.Companies.Find(request.CompanyId);
                c.Logo = @"Images/CompanyLogos/" + request.LogoFile.FileName;

                _dbContext.SaveChanges();

                TempData["logo"] = true;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View(request);
            }
        }

        [HttpPost]
        public IActionResult ChangeStamp(Company request)
        {

            try
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyStamps", request.StampFile.FileName);

                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using FileStream fs = new(path, FileMode.Create);

                request.StampFile.CopyTo(fs);

                Company c = _dbContext.Companies.Find(request.CompanyId);
                c.Stamp = @"Images/CompanyStamps/" + request.StampFile.FileName;

                _dbContext.SaveChanges();

                TempData["stamp"] = true;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View(request);
            }
        }

        [HttpPost]
        public IActionResult ChangeLetterHeadHajj(Company request)
        {

            try
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyLetterHeads", request.LetterHeadFile.FileName);
                string path1 = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyLetterHeads", request.LetterHeadFooterFile.FileName);

                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using FileStream fs = new(path, FileMode.Create);
                if (!string.IsNullOrEmpty(path1)) { string dir_dt = Path.GetDirectoryName(path1); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using FileStream fs1 = new(path1, FileMode.Create);

                request.LetterHeadFile.CopyTo(fs);
                request.LetterHeadFooterFile.CopyTo(fs1);

                Company c = _dbContext.Companies.Find(request.CompanyId);
                c.LetterHead = @"Images/CompanyLetterHeads/" + request.LetterHeadFile.FileName;
                c.LetterHeadFooter = @"Images/CompanyLetterHeads/" + request.LetterHeadFooterFile.FileName;

                _dbContext.SaveChanges();

                TempData["LetterHead"] = true;
                TempData["LetterHeadFooter"] = true;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View(request);
            }
        }

        [HttpPost]
        public IActionResult ChangeLetterHeadCompany(Company request)
        {

            try
            {
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyLetterHeadsCompany", request.LetterHeadFileCompany.FileName);
                string path1 = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "CompanyLetterHeadsCompany", request.LetterHeadFooterFileCompany.FileName);

                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using FileStream fs = new(path, FileMode.Create);
                if (!string.IsNullOrEmpty(path1)) { string dir_dt = Path.GetDirectoryName(path1); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using FileStream fs1 = new(path1, FileMode.Create);

                request.LetterHeadFileCompany.CopyTo(fs);
                request.LetterHeadFooterFileCompany.CopyTo(fs1);

                Company c = _dbContext.Companies.Find(request.CompanyId);
                c.LetterHeadCompany = @"Images/CompanyLetterHeadsCompany/" + request.LetterHeadFileCompany.FileName;
                c.LetterHeadFooterCompany = @"Images/CompanyLetterHeadsCompany/" + request.LetterHeadFooterFileCompany.FileName;

                _dbContext.SaveChanges();

                TempData["LetterHead"] = true;
                TempData["LetterHeadFooter"] = true;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View(request);
            }
        }

        [HttpGet]
        public JsonResult GetAvailableQuota(int companyId)
        {
            Company company = _dbContext.Companies.FirstOrDefault(e => e.CompanyId == companyId && e.Trashed == false);

            if (company == null) return Json(new { availableQuota = 0, msg = "company is null" });

            var quotaDifference = _dbContext.PackagesWithoutTrash
                    .Where(e => e.CompanyId == companyId)
                    .Sum(e => e.Quota);
            //return Json(companyPackages);

            //for (int i = 0; i < companyPackages?.Count(); i++)
            //{
            //    quotaDifference += companyPackages.ElementAt(i).Quota;
            //}

            int? availableQuota = company.Quota - quotaDifference;

            return Json(new { availableQuota });

        }

        [HttpGet]
        public JsonResult GetBaseQuota(int companyId)
        {
            int companyQuota = _dbContext.Companies.FirstOrDefault(e => e.CompanyId == companyId).Quota;

            return Json(new { companyQuota });
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.ClientModels;

namespace TMG.Controllers.ClientControllers
{
    public class PaymentTransactionController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PaymentTransactionController(
            TMGDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
        }

        private int GetActiveSeasonId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var seasonInt = session.GetInt32("ActiveSeasonId");
                if (seasonInt.HasValue && seasonInt.Value > 0)
                {
                    return seasonInt.Value;
                }

                var seasonString = session.GetString("ActiveSeasonId");
                if (!string.IsNullOrWhiteSpace(seasonString) && int.TryParse(seasonString, out var parsedId))
                {
                    return parsedId;
                }
            }
            return 0;
        }

        private async Task PopulateDropdownsAsync(int? selectedFamilyHeadId = null, int? selectedCompanyId = null)
        {
            int seasonId = GetActiveSeasonId();

            var clients = await _context.FamilyHeads
                .AsNoTracking()
                .Where(f => !f.Trashed && (seasonId == 0 || f.HajjSeasonsId == seasonId))
                .OrderBy(f => f.GivenName)
                .Select(f => new
                {
                    Id = f.FamilyHeadId,
                    Name = $"{f.GivenName} {f.SurName} ({(!string.IsNullOrEmpty(f.CNIC) ? f.CNIC : $"#BK-{f.FamilyHeadId}")})"
                })
                .ToListAsync();

            var companies = await _context.Companies
                .AsNoTracking()
                .Where(c => !c.Trashed)
                .OrderBy(c => c.CompanyName)
                .Select(c => new
                {
                    Id = c.CompanyId,
                    Name = c.CompanyName
                })
                .ToListAsync();

            ViewBag.Clients = new SelectList(clients, "Id", "Name", selectedFamilyHeadId);
            ViewBag.Companies = new SelectList(companies, "Id", "Name", selectedCompanyId);
        }

        // GET: /PaymentTransaction/Index
        [HttpGet]
        public async Task<IActionResult> Index(int? familyHeadId, int? companyId, string? paymentType, string? status)
        {
            int seasonId = GetActiveSeasonId();

            var query = _context.ClientCompanyTransactions
                .AsNoTracking()
                .Where(t => !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (familyHeadId.HasValue && familyHeadId.Value > 0)
            {
                query = query.Where(t => t.BookingFor == "Client" && t.FamilyHeadId == familyHeadId.Value);
            }

            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.BookingFor == "Company" && t.CompanyId == companyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(paymentType))
            {
                query = query.Where(t => t.PaymentType == paymentType);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            var rawList = await query
                .OrderByDescending(t => t.PaymentDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            int trashCount = await _context.ClientCompanyTransactions
                .AsNoTracking()
                .CountAsync(t => t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            ViewBag.TrashCount = trashCount;

            var clientIds = rawList.Where(t => t.FamilyHeadId.HasValue).Select(t => t.FamilyHeadId!.Value).Distinct().ToList();
            var companyIds = rawList.Where(t => t.CompanyId.HasValue).Select(t => t.CompanyId!.Value).Distinct().ToList();

            var clientMap = await _context.FamilyHeads
                .AsNoTracking()
                .Where(f => clientIds.Contains(f.FamilyHeadId))
                .ToDictionaryAsync(f => f.FamilyHeadId, f => $"{f.GivenName} {f.SurName}".Trim());

            var companyMap = await _context.Companies
                .AsNoTracking()
                .Where(c => companyIds.Contains(c.CompanyId))
                .ToDictionaryAsync(c => c.CompanyId, c => c.CompanyName);

            var viewModelList = rawList.Select((t, index) =>
            {
                string partnerName = "N/A";
                if (t.BookingFor == "Company" && t.CompanyId.HasValue && companyMap.ContainsKey(t.CompanyId.Value))
                {
                    partnerName = companyMap[t.CompanyId.Value];
                }
                else if (t.FamilyHeadId.HasValue && clientMap.ContainsKey(t.FamilyHeadId.Value))
                {
                    partnerName = clientMap[t.FamilyHeadId.Value];
                }

                string bookingCode = t.SelectedBookingId.HasValue ? $"#BK-{t.SelectedBookingId.Value}" : "-";

                return new ClientCompanyTransactionIndexViewModel
                {
                    Id = t.Id,
                    RowIndex = index + 1,
                    BookingFor = t.BookingFor,
                    FamilyHeadId = t.FamilyHeadId,
                    CompanyId = t.CompanyId,
                    SelectedBookingId = t.SelectedBookingId,
                    PartnerName = partnerName,
                    BookingCode = bookingCode,
                    PaymentType = t.PaymentType,
                    AmountPkr = t.AmountPkr,
                    PaymentDate = t.PaymentDate,
                    ReferenceChequeNo = t.ReferenceChequeNo,
                    ProofFilePath = t.ProofFilePath,
                    Status = t.Status,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                };
            }).ToList();

            await PopulateDropdownsAsync(familyHeadId, companyId);

            ViewBag.SelectedFamilyHeadId = familyHeadId;
            ViewBag.SelectedCompanyId = companyId;
            ViewBag.SelectedPaymentType = paymentType;
            ViewBag.SelectedStatus = status;

            return View(viewModelList);
        }

        // GET: /PaymentTransaction/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();

            var viewModel = new ClientCompanyTransactionFormViewModel
            {
                BookingFor = "Client",
                PaymentDate = DateTime.Today,
                AmountPkr = 0.0m,
                PaymentType = "Cash",
                Status = "Confirmed"
            };

            return View(viewModel);
        }

        // POST: /PaymentTransaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCompanyTransactionFormViewModel model)
        {
            int seasonId = GetActiveSeasonId();

            if (model.BookingFor == "Client" && (!model.FamilyHeadId.HasValue || model.FamilyHeadId.Value <= 0))
            {
                ModelState.AddModelError("FamilyHeadId", "Please select a Client.");
            }
            else if (model.BookingFor == "Company" && (!model.CompanyId.HasValue || model.CompanyId.Value <= 0))
            {
                ModelState.AddModelError("CompanyId", "Please select a Company.");
            }

            if (model.ProofFile != null && model.ProofFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var ext = Path.GetExtension(model.ProofFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ProofFile", "Only JPG, PNG, and PDF files are allowed.");
                }

                if (model.ProofFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProofFile", "File size cannot exceed 2 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model.FamilyHeadId, model.CompanyId);
                return View(model);
            }

            string? proofPath = null;
            if (model.ProofFile != null && model.ProofFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "PaymentProofs");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProofFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProofFile.CopyToAsync(fileStream);
                }

                proofPath = $"/Uploads/PaymentProofs/{uniqueFileName}";
            }

            var transaction = new ClientCompanyTransaction
            {
                HajjSeasonsId = seasonId,
                BookingFor = model.BookingFor,
                FamilyHeadId = model.BookingFor == "Client" ? model.FamilyHeadId : null,
                CompanyId = model.BookingFor == "Company" ? model.CompanyId : null,
                SelectedBookingId = model.SelectedBookingId,
                PaymentType = model.PaymentType,
                AmountPkr = model.AmountPkr,
                PaymentDate = model.PaymentDate,
                ReferenceChequeNo = model.ReferenceChequeNo?.Trim(),
                ProofFilePath = proofPath,
                Status = model.Status,
                Notes = model.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                Trashed = false
            };

            _context.ClientCompanyTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        // GET: /PaymentTransaction/GetBookingsByClient?clientFamilyHeadId={id}
        [HttpGet]
        public async Task<IActionResult> GetBookingsByClient(int clientFamilyHeadId)
        {
            if (clientFamilyHeadId <= 0)
            {
                return Json(new List<BookingLookupDto>());
            }

            var head = await _context.FamilyHeads
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FamilyHeadId == clientFamilyHeadId);

            var list = new List<BookingLookupDto>();
            if (head != null)
            {
                list.Add(new BookingLookupDto
                {
                    Id = head.FamilyHeadId,
                    Code = $"#BK-{head.FamilyHeadId}",
                    DisplayText = $"#BK-{head.FamilyHeadId} - {head.GivenName} {head.SurName}".Trim()
                });
            }

            return Json(list);
        }

        // GET: /PaymentTransaction/GetBookingsByCompany?companyId={id}
        [HttpGet]
        public async Task<IActionResult> GetBookingsByCompany(int companyId)
        {
            if (companyId <= 0)
            {
                return Json(new List<BookingLookupDto>());
            }

            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            var list = new List<BookingLookupDto>();
            if (company != null)
            {
                list.Add(new BookingLookupDto
                {
                    Id = company.CompanyId,
                    Code = $"#CMP-{company.CompanyId}",
                    DisplayText = $"#CMP-{company.CompanyId} ({company.CompanyName} Ledger)"
                });
            }

            return Json(list);
        }

        // GET: /PaymentTransaction/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ClientCompanyTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction == null)
            {
                return NotFound();
            }

            string partnerName = "N/A";
            if (transaction.BookingFor == "Company" && transaction.CompanyId.HasValue)
            {
                var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == transaction.CompanyId.Value);
                if (company != null) partnerName = company.CompanyName;
            }
            else if (transaction.FamilyHeadId.HasValue)
            {
                var client = await _context.FamilyHeads.AsNoTracking().FirstOrDefaultAsync(f => f.FamilyHeadId == transaction.FamilyHeadId.Value);
                if (client != null) partnerName = $"{client.GivenName} {client.SurName}".Trim();
            }

            var viewModel = new ClientCompanyTransactionIndexViewModel
            {
                Id = transaction.Id,
                BookingFor = transaction.BookingFor,
                FamilyHeadId = transaction.FamilyHeadId,
                CompanyId = transaction.CompanyId,
                SelectedBookingId = transaction.SelectedBookingId,
                PartnerName = partnerName,
                BookingCode = transaction.SelectedBookingId.HasValue ? $"#BK-{transaction.SelectedBookingId.Value}" : "-",
                PaymentType = transaction.PaymentType,
                AmountPkr = transaction.AmountPkr,
                PaymentDate = transaction.PaymentDate,
                ReferenceChequeNo = transaction.ReferenceChequeNo,
                ProofFilePath = transaction.ProofFilePath,
                Status = transaction.Status,
                Notes = transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };

            return View(viewModel);
        }

        // GET: /PaymentTransaction/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ClientCompanyTransactions
                .FirstOrDefaultAsync(t => t.Id == id && !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(transaction.FamilyHeadId, transaction.CompanyId);

            var model = new ClientCompanyTransactionFormViewModel
            {
                Id = transaction.Id,
                BookingFor = transaction.BookingFor,
                FamilyHeadId = transaction.FamilyHeadId,
                CompanyId = transaction.CompanyId,
                SelectedBookingId = transaction.SelectedBookingId,
                PaymentType = transaction.PaymentType,
                AmountPkr = transaction.AmountPkr,
                PaymentDate = transaction.PaymentDate,
                ReferenceChequeNo = transaction.ReferenceChequeNo,
                ExistingProofFilePath = transaction.ProofFilePath,
                Status = transaction.Status,
                Notes = transaction.Notes
            };

            return View(model);
        }

        // POST: /PaymentTransaction/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientCompanyTransactionFormViewModel model)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ClientCompanyTransactions
                .FirstOrDefaultAsync(t => t.Id == model.Id && !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction == null)
            {
                return NotFound();
            }

            if (model.BookingFor == "Client" && (!model.FamilyHeadId.HasValue || model.FamilyHeadId.Value <= 0))
            {
                ModelState.AddModelError("FamilyHeadId", "Please select a Client.");
            }
            else if (model.BookingFor == "Company" && (!model.CompanyId.HasValue || model.CompanyId.Value <= 0))
            {
                ModelState.AddModelError("CompanyId", "Please select a Company.");
            }

            if (model.ProofFile != null && model.ProofFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var ext = Path.GetExtension(model.ProofFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ProofFile", "Only JPG, PNG, and PDF files are allowed.");
                }

                if (model.ProofFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProofFile", "File size cannot exceed 2 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model.FamilyHeadId, model.CompanyId);
                model.ExistingProofFilePath = transaction.ProofFilePath;
                return View(model);
            }

            if (model.ProofFile != null && model.ProofFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "PaymentProofs");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProofFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProofFile.CopyToAsync(fileStream);
                }

                transaction.ProofFilePath = $"/Uploads/PaymentProofs/{uniqueFileName}";
            }

            transaction.BookingFor = model.BookingFor;
            transaction.FamilyHeadId = model.BookingFor == "Client" ? model.FamilyHeadId : null;
            transaction.CompanyId = model.BookingFor == "Company" ? model.CompanyId : null;
            transaction.SelectedBookingId = model.SelectedBookingId;
            transaction.PaymentType = model.PaymentType;
            transaction.AmountPkr = model.AmountPkr;
            transaction.PaymentDate = model.PaymentDate;
            transaction.ReferenceChequeNo = model.ReferenceChequeNo?.Trim();
            transaction.Status = model.Status;
            transaction.Notes = model.Notes?.Trim();
            transaction.UpdatedAt = DateTime.UtcNow;

            _context.ClientCompanyTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        // POST or GET: /PaymentTransaction/MoveToTrash/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> MoveToTrash(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ClientCompanyTransactions
                .FirstOrDefaultAsync(t => t.Id == id && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction != null)
            {
                transaction.Trashed = true;
                transaction.UpdatedAt = DateTime.UtcNow;
                _context.ClientCompanyTransactions.Update(transaction);
                await _context.SaveChangesAsync();
                TempData["deleted"] = true;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /PaymentTransaction/Trash
        [HttpGet]
        public async Task<IActionResult> Trash()
        {
            int seasonId = GetActiveSeasonId();

            var rawList = await _context.ClientCompanyTransactions
                .AsNoTracking()
                .Where(t => t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId))
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .ToListAsync();

            var clientIds = rawList.Where(t => t.FamilyHeadId.HasValue).Select(t => t.FamilyHeadId!.Value).Distinct().ToList();
            var companyIds = rawList.Where(t => t.CompanyId.HasValue).Select(t => t.CompanyId!.Value).Distinct().ToList();

            var clientMap = await _context.FamilyHeads
                .AsNoTracking()
                .Where(f => clientIds.Contains(f.FamilyHeadId))
                .ToDictionaryAsync(f => f.FamilyHeadId, f => $"{f.GivenName} {f.SurName}".Trim());

            var companyMap = await _context.Companies
                .AsNoTracking()
                .Where(c => companyIds.Contains(c.CompanyId))
                .ToDictionaryAsync(c => c.CompanyId, c => c.CompanyName);

            var viewModelList = rawList.Select((t, index) =>
            {
                string partnerName = "N/A";
                if (t.BookingFor == "Company" && t.CompanyId.HasValue && companyMap.ContainsKey(t.CompanyId.Value))
                {
                    partnerName = companyMap[t.CompanyId.Value];
                }
                else if (t.FamilyHeadId.HasValue && clientMap.ContainsKey(t.FamilyHeadId.Value))
                {
                    partnerName = clientMap[t.FamilyHeadId.Value];
                }

                return new ClientCompanyTransactionIndexViewModel
                {
                    Id = t.Id,
                    RowIndex = index + 1,
                    BookingFor = t.BookingFor,
                    PartnerName = partnerName,
                    BookingCode = t.SelectedBookingId.HasValue ? $"#BK-{t.SelectedBookingId.Value}" : "-",
                    PaymentType = t.PaymentType,
                    AmountPkr = t.AmountPkr,
                    PaymentDate = t.PaymentDate,
                    ReferenceChequeNo = t.ReferenceChequeNo,
                    ProofFilePath = t.ProofFilePath,
                    Status = t.Status,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                };
            }).ToList();

            return View(viewModelList);
        }

        // POST or GET: /PaymentTransaction/Restore/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Restore(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ClientCompanyTransactions
                .FirstOrDefaultAsync(t => t.Id == id && t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction != null)
            {
                transaction.Trashed = false;
                transaction.UpdatedAt = DateTime.UtcNow;
                _context.ClientCompanyTransactions.Update(transaction);
                await _context.SaveChangesAsync();
                TempData["restored"] = true;
            }

            return RedirectToAction(nameof(Trash));
        }

        // POST or GET: /PaymentTransaction/DeletePermanent/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> DeletePermanent(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ClientCompanyTransactions
                .FirstOrDefaultAsync(t => t.Id == id && t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction != null)
            {
                _context.ClientCompanyTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["deleted"] = true;
            }

            return RedirectToAction(nameof(Trash));
        }
    }
}

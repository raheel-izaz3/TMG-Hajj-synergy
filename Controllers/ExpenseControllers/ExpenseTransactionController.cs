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
using TMG.Models.ExpenseModels;

namespace TMG.Controllers.ExpenseControllers
{
    public class ExpenseTransactionController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ExpenseTransactionController(
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

        private async Task PopulateExpenseHeadsDropdownAsync(int? selectedId = null)
        {
            int seasonId = GetActiveSeasonId();
            var heads = await _context.ExpenseHeads
                .AsNoTracking()
                .Where(h => !h.Trashed && (seasonId == 0 || h.HajjSeasonsId == seasonId))
                .OrderBy(h => h.ExpenseName)
                .ToListAsync();

            ViewBag.ExpenseHeads = new SelectList(heads, "Id", "ExpenseName", selectedId);
        }

        // GET: /ExpenseTransaction/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int seasonId = GetActiveSeasonId();

            var transactions = await _context.ExpenseTransactions
                .AsNoTracking()
                .Where(t => !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId))
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            int trashCount = await _context.ExpenseTransactions
                .AsNoTracking()
                .CountAsync(t => t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            ViewBag.TrashCount = trashCount;

            // Loose lookup of ExpenseHead names
            var headIds = transactions.Select(t => t.ExpenseHeadId).Distinct().ToList();
            var headMap = await _context.ExpenseHeads
                .AsNoTracking()
                .Where(h => headIds.Contains(h.Id))
                .ToDictionaryAsync(h => h.Id, h => h.ExpenseName);

            var viewModelList = transactions.Select((t, index) => new ExpenseTransactionIndexViewModel
            {
                Id = t.Id,
                RowIndex = index + 1,
                ExpenseHeadId = t.ExpenseHeadId,
                ExpenseHeadName = headMap.ContainsKey(t.ExpenseHeadId) ? headMap[t.ExpenseHeadId] : "Uncategorized",
                Type = t.Type,
                Year = t.Year,
                Amount = t.Amount,
                PaymentType = t.PaymentType,
                Currency = t.Currency,
                ExchangeRate = t.ExchangeRate,
                Date = t.Date,
                ReferenceNo = t.ReferenceNo,
                ProofDocumentPath = t.ProofDocumentPath,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList();

            return View(viewModelList);
        }

        // GET: /ExpenseTransaction/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateExpenseHeadsDropdownAsync();

            var viewModel = new ExpenseTransactionFormViewModel
            {
                Year = DateTime.UtcNow.Year,
                Date = DateTime.Today,
                Currency = "PKR",
                ExchangeRate = 1.0m,
                Type = "Operational",
                PaymentType = "Cash"
            };

            return View(viewModel);
        }

        // POST: /ExpenseTransaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseTransactionFormViewModel model)
        {
            int seasonId = GetActiveSeasonId();

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
                await PopulateExpenseHeadsDropdownAsync(model.ExpenseHeadId);
                return View(model);
            }

            string? proofDocumentPath = null;
            if (model.ProofFile != null && model.ProofFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "ExpenseProofs");
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

                proofDocumentPath = $"/Uploads/ExpenseProofs/{uniqueFileName}";
            }

            var transaction = new ExpenseTransaction
            {
                HajjSeasonsId = seasonId,
                ExpenseHeadId = model.ExpenseHeadId,
                Type = model.Type,
                Year = model.Year,
                Amount = model.Amount,
                PaymentType = model.PaymentType,
                Currency = model.Currency,
                ExchangeRate = model.ExchangeRate,
                Date = model.Date,
                ReferenceNo = model.ReferenceNo?.Trim(),
                ProofDocumentPath = proofDocumentPath,
                Description = model.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                Trashed = false
            };

            _context.ExpenseTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        // GET: /ExpenseTransaction/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ExpenseTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction == null)
            {
                return NotFound();
            }

            var head = await _context.ExpenseHeads
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == transaction.ExpenseHeadId);

            var viewModel = new ExpenseTransactionIndexViewModel
            {
                Id = transaction.Id,
                ExpenseHeadId = transaction.ExpenseHeadId,
                ExpenseHeadName = head?.ExpenseName ?? "Uncategorized",
                Type = transaction.Type,
                Year = transaction.Year,
                Amount = transaction.Amount,
                PaymentType = transaction.PaymentType,
                Currency = transaction.Currency,
                ExchangeRate = transaction.ExchangeRate,
                Date = transaction.Date,
                ReferenceNo = transaction.ReferenceNo,
                ProofDocumentPath = transaction.ProofDocumentPath,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            };

            return View(viewModel);
        }

        // GET: /ExpenseTransaction/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ExpenseTransactions
                .FirstOrDefaultAsync(t => t.Id == id && !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction == null)
            {
                return NotFound();
            }

            await PopulateExpenseHeadsDropdownAsync(transaction.ExpenseHeadId);

            var model = new ExpenseTransactionFormViewModel
            {
                Id = transaction.Id,
                ExpenseHeadId = transaction.ExpenseHeadId,
                Type = transaction.Type,
                Year = transaction.Year,
                Amount = transaction.Amount,
                PaymentType = transaction.PaymentType,
                Currency = transaction.Currency,
                ExchangeRate = transaction.ExchangeRate,
                Date = transaction.Date,
                ReferenceNo = transaction.ReferenceNo,
                ExistingProofDocumentPath = transaction.ProofDocumentPath,
                Description = transaction.Description
            };

            return View(model);
        }

        // POST: /ExpenseTransaction/Edit or /ExpenseTransaction/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExpenseTransactionFormViewModel model)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ExpenseTransactions
                .FirstOrDefaultAsync(t => t.Id == model.Id && !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction == null)
            {
                return NotFound();
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
                await PopulateExpenseHeadsDropdownAsync(model.ExpenseHeadId);
                model.ExistingProofDocumentPath = transaction.ProofDocumentPath;
                return View(model);
            }

            if (model.ProofFile != null && model.ProofFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "ExpenseProofs");
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

                transaction.ProofDocumentPath = $"/Uploads/ExpenseProofs/{uniqueFileName}";
            }

            transaction.ExpenseHeadId = model.ExpenseHeadId;
            transaction.Type = model.Type;
            transaction.Year = model.Year;
            transaction.Amount = model.Amount;
            transaction.PaymentType = model.PaymentType;
            transaction.Currency = model.Currency;
            transaction.ExchangeRate = model.ExchangeRate;
            transaction.Date = model.Date;
            transaction.ReferenceNo = model.ReferenceNo?.Trim();
            transaction.Description = model.Description?.Trim();
            transaction.UpdatedAt = DateTime.UtcNow;

            _context.ExpenseTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        // POST or GET: /ExpenseTransaction/MoveToTrash/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> MoveToTrash(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ExpenseTransactions
                .FirstOrDefaultAsync(t => t.Id == id && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction != null)
            {
                transaction.Trashed = true;
                transaction.UpdatedAt = DateTime.UtcNow;
                _context.ExpenseTransactions.Update(transaction);
                await _context.SaveChangesAsync();
                TempData["deleted"] = true;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /ExpenseTransaction/Trash
        [HttpGet]
        public async Task<IActionResult> Trash()
        {
            int seasonId = GetActiveSeasonId();

            var trashedTransactions = await _context.ExpenseTransactions
                .AsNoTracking()
                .Where(t => t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId))
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .ToListAsync();

            var headIds = trashedTransactions.Select(t => t.ExpenseHeadId).Distinct().ToList();
            var headMap = await _context.ExpenseHeads
                .AsNoTracking()
                .Where(h => headIds.Contains(h.Id))
                .ToDictionaryAsync(h => h.Id, h => h.ExpenseName);

            var viewModelList = trashedTransactions.Select((t, index) => new ExpenseTransactionIndexViewModel
            {
                Id = t.Id,
                RowIndex = index + 1,
                ExpenseHeadId = t.ExpenseHeadId,
                ExpenseHeadName = headMap.ContainsKey(t.ExpenseHeadId) ? headMap[t.ExpenseHeadId] : "Uncategorized",
                Type = t.Type,
                Year = t.Year,
                Amount = t.Amount,
                PaymentType = t.PaymentType,
                Currency = t.Currency,
                ExchangeRate = t.ExchangeRate,
                Date = t.Date,
                ReferenceNo = t.ReferenceNo,
                ProofDocumentPath = t.ProofDocumentPath,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList();

            return View(viewModelList);
        }

        // POST or GET: /ExpenseTransaction/Restore/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Restore(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ExpenseTransactions
                .FirstOrDefaultAsync(t => t.Id == id && t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction != null)
            {
                transaction.Trashed = false;
                transaction.UpdatedAt = DateTime.UtcNow;
                _context.ExpenseTransactions.Update(transaction);
                await _context.SaveChangesAsync();
                TempData["restored"] = true;
            }

            return RedirectToAction(nameof(Trash));
        }

        // POST or GET: /ExpenseTransaction/DeletePermanent/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> DeletePermanent(long id)
        {
            int seasonId = GetActiveSeasonId();

            var transaction = await _context.ExpenseTransactions
                .FirstOrDefaultAsync(t => t.Id == id && t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (transaction != null)
            {
                _context.ExpenseTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["deleted"] = true;
            }

            return RedirectToAction(nameof(Trash));
        }
    }
}

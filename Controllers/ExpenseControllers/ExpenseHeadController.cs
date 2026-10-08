using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.ExpenseModels;

namespace TMG.Controllers.ExpenseControllers
{
    public class ExpenseHeadController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExpenseHeadController(TMGDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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

        // GET: /ExpenseHead/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int seasonId = GetActiveSeasonId();

            var expenseHeads = await _context.ExpenseHeads
                .AsNoTracking()
                .Where(e => !e.Trashed && (seasonId == 0 || e.HajjSeasonsId == seasonId))
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            int trashCount = await _context.ExpenseHeads
                .AsNoTracking()
                .CountAsync(e => e.Trashed && (seasonId == 0 || e.HajjSeasonsId == seasonId));

            ViewBag.TrashCount = trashCount;

            var viewModelList = expenseHeads.Select((e, index) => new ExpenseHeadIndexViewModel
            {
                Id = e.Id,
                RowIndex = index + 1,
                ExpenseName = e.ExpenseName,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Trashed = e.Trashed
            }).ToList();

            return View(viewModelList);
        }

        // GET: /ExpenseHead/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ExpenseHeadFormViewModel());
        }

        // POST: /ExpenseHead/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseHeadFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int seasonId = GetActiveSeasonId();

            var newHead = new ExpenseHead
            {
                HajjSeasonsId = seasonId,
                ExpenseName = model.ExpenseName.Trim(),
                CreatedAt = DateTime.UtcNow,
                Trashed = false
            };

            _context.ExpenseHeads.Add(newHead);
            await _context.SaveChangesAsync();

            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        // GET: /ExpenseHead/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int seasonId = GetActiveSeasonId();

            var head = await _context.ExpenseHeads
                .FirstOrDefaultAsync(e => e.Id == id && !e.Trashed && (seasonId == 0 || e.HajjSeasonsId == seasonId));

            if (head == null)
            {
                return NotFound();
            }

            var model = new ExpenseHeadFormViewModel
            {
                Id = head.Id,
                ExpenseName = head.ExpenseName
            };

            return View(model);
        }

        // POST: /ExpenseHead/Edit or /ExpenseHead/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExpenseHeadFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int seasonId = GetActiveSeasonId();

            var head = await _context.ExpenseHeads
                .FirstOrDefaultAsync(e => e.Id == model.Id && !e.Trashed && (seasonId == 0 || e.HajjSeasonsId == seasonId));

            if (head == null)
            {
                return NotFound();
            }

            head.ExpenseName = model.ExpenseName.Trim();
            head.UpdatedAt = DateTime.UtcNow;

            _context.ExpenseHeads.Update(head);
            await _context.SaveChangesAsync();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        // POST or GET: /ExpenseHead/MoveToTrash/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> MoveToTrash(int id)
        {
            int seasonId = GetActiveSeasonId();

            var head = await _context.ExpenseHeads
                .FirstOrDefaultAsync(e => e.Id == id && (seasonId == 0 || e.HajjSeasonsId == seasonId));

            if (head != null)
            {
                head.Trashed = true;
                head.UpdatedAt = DateTime.UtcNow;
                _context.ExpenseHeads.Update(head);
                await _context.SaveChangesAsync();
                TempData["deleted"] = true;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /ExpenseHead/Trash
        [HttpGet]
        public async Task<IActionResult> Trash()
        {
            int seasonId = GetActiveSeasonId();

            var trashedHeads = await _context.ExpenseHeads
                .AsNoTracking()
                .Where(e => e.Trashed && (seasonId == 0 || e.HajjSeasonsId == seasonId))
                .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
                .ToListAsync();

            var viewModelList = trashedHeads.Select((e, index) => new ExpenseHeadIndexViewModel
            {
                Id = e.Id,
                RowIndex = index + 1,
                ExpenseName = e.ExpenseName,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Trashed = e.Trashed
            }).ToList();

            return View(viewModelList);
        }

        // POST or GET: /ExpenseHead/Restore/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Restore(int id)
        {
            int seasonId = GetActiveSeasonId();

            var head = await _context.ExpenseHeads
                .FirstOrDefaultAsync(e => e.Id == id && e.Trashed && (seasonId == 0 || e.HajjSeasonsId == seasonId));

            if (head != null)
            {
                head.Trashed = false;
                head.UpdatedAt = DateTime.UtcNow;
                _context.ExpenseHeads.Update(head);
                await _context.SaveChangesAsync();
                TempData["restored"] = true;
            }

            return RedirectToAction(nameof(Trash));
        }

        // POST or GET: /ExpenseHead/DeletePermanent/{id}
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            int seasonId = GetActiveSeasonId();

            var head = await _context.ExpenseHeads
                .FirstOrDefaultAsync(e => e.Id == id && e.Trashed && (seasonId == 0 || e.HajjSeasonsId == seasonId));

            if (head != null)
            {
                _context.ExpenseHeads.Remove(head);
                await _context.SaveChangesAsync();
                TempData["deleted"] = true;
            }

            return RedirectToAction(nameof(Trash));
        }
    }
}

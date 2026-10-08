using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.ExpenseModels;

namespace TMG.Controllers.ExpenseControllers
{
    public class CompanyLedgerController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyLedgerController(TMGDbContext context, IHttpContextAccessor httpContextAccessor)
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

        private async Task PopulateCompaniesDropdownAsync(int? selectedCompanyId = null)
        {
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

            ViewBag.Companies = new SelectList(companies, "Id", "Name", selectedCompanyId);
        }

        // GET: /CompanyLedger/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await PopulateCompaniesDropdownAsync();

            var viewModel = new CompanyLedgerFilterViewModel();
            return View(viewModel);
        }

        // GET: /CompanyLedger/View?companyId={id}&fromDate={d1}&toDate={d2}
        [HttpGet]
        public async Task<IActionResult> View(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId <= 0)
            {
                return RedirectToAction(nameof(Index));
            }

            int seasonId = GetActiveSeasonId();

            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && !c.Trashed);

            if (company == null)
            {
                return NotFound();
            }

            // Find packages owned by this company
            var companyPackageIds = await _context.Packages
                .AsNoTracking()
                .Where(p => p.CompanyId == companyId && !p.Trashed)
                .Select(p => p.PackageId)
                .ToListAsync();

            // Find bookings linked to this company's packages or directly
            var familyHeads = await _context.FamilyHeads
                .AsNoTracking()
                .Include(f => f.Package)
                .Where(f => !f.Trashed && (seasonId == 0 || f.HajjSeasonsId == seasonId) && (companyPackageIds.Contains(f.PackageId) || f.PackageId > 0))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var familyHeadIds = familyHeads.Select(f => f.FamilyHeadId).ToList();

            // Fetch member counts per family head
            var memberCounts = await _context.FamilyMembers
                .AsNoTracking()
                .Where(m => familyHeadIds.Contains(m.FamilyHeadId))
                .GroupBy(m => m.FamilyHeadId)
                .Select(g => new { FamilyHeadId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.FamilyHeadId, g => g.Count);

            var bookingRows = new List<CompanyBookingRowDto>();
            int bSerial = 1;
            decimal totalBookingAmount = 0m;
            decimal totalReceivedAtBooking = 0m;
            int totalPaxCount = 0;

            foreach (var f in familyHeads)
            {
                int pax = 1 + (memberCounts.ContainsKey(f.FamilyHeadId) ? memberCounts[f.FamilyHeadId] : 0);
                totalPaxCount += pax;

                decimal pkgRate = (f.Package != null && f.Package.AdultPkr > 0) ? (decimal)f.Package.AdultPkr : 750000m;
                decimal bookingTotal = pkgRate * pax;
                decimal initialReceived = 0m; // Initial booking advance if any

                totalBookingAmount += bookingTotal;
                totalReceivedAtBooking += initialReceived;

                bookingRows.Add(new CompanyBookingRowDto
                {
                    SerialNo = bSerial++,
                    BookingDate = f.CreatedAt,
                    BookingCode = $"#BK-{f.FamilyHeadId}",
                    PackageName = f.Package?.Name ?? "Hajj Package",
                    Pax = pax,
                    TotalAmount = bookingTotal,
                    Received = initialReceived,
                    FamilyHeadId = f.FamilyHeadId
                });
            }

            // Query financial ledger transactions for this company
            var txnQuery = _context.ClientCompanyTransactions
                .AsNoTracking()
                .Where(t => t.CompanyId == companyId && t.BookingFor == "Company" && t.Status == "Confirmed" && !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (fromDate.HasValue)
            {
                txnQuery = txnQuery.Where(t => t.PaymentDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                txnQuery = txnQuery.Where(t => t.PaymentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            var transactionsList = await txnQuery
                .OrderBy(t => t.PaymentDate)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();

            decimal totalReceivedViaTxns = transactionsList.Sum(t => t.AmountPkr);

            var txnRows = transactionsList.Select((t, index) => new CompanyTransactionRowDto
            {
                SerialNo = index + 1,
                Date = t.PaymentDate,
                BookingReference = t.SelectedBookingId.HasValue ? $"#BK-{t.SelectedBookingId.Value}" : $"#CMP-{companyId}",
                PaymentType = t.PaymentType,
                ReferenceNo = t.ReferenceChequeNo ?? "-",
                Amount = t.AmountPkr,
                Status = t.Status
            }).ToList();

            var statementViewModel = new CompanyLedgerStatementViewModel
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                TotalBookingsCount = bookingRows.Count,
                TotalPaxCount = totalPaxCount,
                GeneratedOn = DateTime.UtcNow,
                FromDate = fromDate,
                ToDate = toDate,
                TotalBookingAmount = totalBookingAmount,
                TotalReceivedAtBooking = totalReceivedAtBooking,
                TotalReceivedViaTransactions = totalReceivedViaTxns,
                Bookings = bookingRows,
                Transactions = txnRows
            };

            return View(statementViewModel);
        }

        // GET: /CompanyLedger/ExportPdf?companyId={id}&fromDate={d1}&toDate={d2}
        [HttpGet]
        public async Task<IActionResult> ExportPdf(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            var result = await View(companyId, fromDate, toDate);
            ViewBag.IsPdfExport = true;
            return result;
        }
    }
}

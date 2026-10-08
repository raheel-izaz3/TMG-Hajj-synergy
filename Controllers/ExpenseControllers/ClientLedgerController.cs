using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.ExpenseModels;

namespace TMG.Controllers.ExpenseControllers
{
    public class ClientLedgerController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientLedgerController(TMGDbContext context, IHttpContextAccessor httpContextAccessor)
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

        private async Task PopulateClientsDropdownAsync(int? selectedClientId = null)
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

            ViewBag.Clients = new SelectList(clients, "Id", "Name", selectedClientId);
        }

        // GET: /ClientLedger/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await PopulateClientsDropdownAsync();

            var viewModel = new ClientLedgerFilterViewModel();
            return View(viewModel);
        }

        // GET: /ClientLedger/View?clientId={id}&fromDate={d1}&toDate={d2}
        [HttpGet]
        public async Task<IActionResult> View(int clientId, DateTime? fromDate, DateTime? toDate)
        {
            if (clientId <= 0)
            {
                return RedirectToAction(nameof(Index));
            }

            int seasonId = GetActiveSeasonId();

            var client = await _context.FamilyHeads
                .AsNoTracking()
                .Include(f => f.Package)
                .FirstOrDefaultAsync(f => f.FamilyHeadId == clientId && !f.Trashed && (seasonId == 0 || f.HajjSeasonsId == seasonId));

            if (client == null)
            {
                return NotFound();
            }

            var query = _context.ClientCompanyTransactions
                .AsNoTracking()
                .Where(t => t.FamilyHeadId == clientId && t.Status == "Confirmed" && !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.PaymentDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.PaymentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            var transactionsList = await query
                .OrderBy(t => t.PaymentDate)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();

            decimal totalPaid = transactionsList.Sum(t => t.AmountPkr);

            decimal totalBookingAmount = 3750000.00m;
            if (client.Package != null && client.Package.AdultPkr > 0)
            {
                totalBookingAmount = (decimal)client.Package.AdultPkr;
            }

            string packageName = client.Package?.Name ?? "#1 HAJJ PACKAGES";

            var dtoList = transactionsList.Select((t, index) => new ClientLedgerTransactionDto
            {
                SerialNo = index + 1,
                PackageName = packageName,
                PaymentType = t.PaymentType,
                Amount = t.AmountPkr,
                Date = t.PaymentDate,
                ReferenceNo = t.ReferenceChequeNo ?? "-",
                Narration = string.IsNullOrEmpty(t.Notes) ? $"Payment received via {t.PaymentType}" : t.Notes,
                ProofUrl = t.ProofFilePath,
                Status = t.Status
            }).ToList();

            var statementViewModel = new ClientLedgerStatementViewModel
            {
                ClientId = client.FamilyHeadId,
                FullName = $"{client.GivenName} {client.SurName}".Trim(),
                PhoneNumber = !string.IsNullOrEmpty(client.KSAMobileNumber) ? client.KSAMobileNumber : "+92 300 1234567",
                EmailAddress = $"{client.GivenName?.ToLower()}@tmg.com",
                AccountStatus = "Active",
                TotalBookingAmount = totalBookingAmount,
                ReceivedAtBooking = 0.0m,
                TotalPaidViaTransactions = totalPaid,
                FromDate = fromDate,
                ToDate = toDate,
                Transactions = dtoList
            };

            return View(statementViewModel);
        }

        // GET: /ClientLedger/ExportPdf?clientId={id}&fromDate={d1}&toDate={d2}
        [HttpGet]
        public async Task<IActionResult> ExportPdf(int clientId, DateTime? fromDate, DateTime? toDate)
        {
            var result = await View(clientId, fromDate, toDate);
            ViewBag.IsPdfExport = true;
            return result;
        }

        // GET: /ClientLedger/ExportExcel?clientId={id}&fromDate={d1}&toDate={d2}
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int clientId, DateTime? fromDate, DateTime? toDate)
        {
            int seasonId = GetActiveSeasonId();

            var client = await _context.FamilyHeads
                .AsNoTracking()
                .Include(f => f.Package)
                .FirstOrDefaultAsync(f => f.FamilyHeadId == clientId && !f.Trashed && (seasonId == 0 || f.HajjSeasonsId == seasonId));

            if (client == null)
            {
                return NotFound();
            }

            var query = _context.ClientCompanyTransactions
                .AsNoTracking()
                .Where(t => t.FamilyHeadId == clientId && t.Status == "Confirmed" && !t.Trashed && (seasonId == 0 || t.HajjSeasonsId == seasonId));

            if (fromDate.HasValue) query = query.Where(t => t.PaymentDate >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(t => t.PaymentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var transactions = await query.OrderBy(t => t.PaymentDate).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Serial No,Package,Payment Type,Amount (PKR),Date,Ref No,Narration,Status");

            int index = 1;
            string packageName = client.Package?.Name ?? "HAJJ PACKAGE";

            foreach (var t in transactions)
            {
                sb.AppendLine($"{index},\"{packageName}\",\"{t.PaymentType}\",{t.AmountPkr},\"{t.PaymentDate:dd-MMM-yyyy}\",\"{t.ReferenceChequeNo}\",\"{t.Notes}\",\"{t.Status}\"");
                index++;
            }

            var fileName = $"Client_Ledger_{client.GivenName}_{client.FamilyHeadId}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }
    }
}

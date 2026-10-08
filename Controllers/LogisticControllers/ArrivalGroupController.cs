using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.AirlineModels;
using TMG.Models.ArrivalGroupModels;
using TMG.Models.FamilyModels;

namespace TMG.Controllers.LogisticControllers
{
    public class ArrivalGroupController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ArrivalGroupController(TMGDbContext context, IHttpContextAccessor httpContextAccessor)
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

        // GET: ArrivalGroup/Index
        public async Task<IActionResult> Index(string search)
        {
            int seasonId = GetActiveSeasonId();

            var query = _context.ArrivalGroups
                .Include(g => g.Members.Where(m => !m.Trashed))
                .Where(g => !g.Trashed && g.HajjSeasonsId == seasonId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(g =>
                    g.GroupName.ToLower().Contains(term) ||
                    (g.PNRNumber != null && g.PNRNumber.ToLower().Contains(term)) ||
                    (g.FlightNumber != null && g.FlightNumber.ToLower().Contains(term)) ||
                    (g.DepartureCity != null && g.DepartureCity.ToLower().Contains(term)) ||
                    (g.ArrivalCity != null && g.ArrivalCity.ToLower().Contains(term))
                );
            }

            var groupsList = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
            var airlineMap = await _context.Airlines.AsNoTracking().ToDictionaryAsync(a => a.AirlineId, a => a.Name);

            var viewModels = groupsList.Select(g => new ArrivalGroupIndexItemViewModel
            {
                Id = g.Id,
                GroupName = g.GroupName,
                AirlineName = g.AirlineId.HasValue && airlineMap.ContainsKey(g.AirlineId.Value)
                    ? airlineMap[g.AirlineId.Value]
                    : "N/A",
                FlightNumber = g.FlightNumber ?? "N/A",
                FlightArrivalDate = g.FlightArrivalDate,
                FlightArrivalTime = g.FlightArrivalTime ?? "",
                PNRNumber = g.PNRNumber ?? "N/A",
                DepartureCity = g.DepartureCity ?? "",
                ArrivalCity = g.ArrivalCity ?? "",
                PassengerCount = g.Members?.Count(m => !m.Trashed) ?? 0,
                CreatedAt = g.CreatedAt
            }).ToList();

            ViewData["CurrentSearch"] = search;
            return View(viewModels);
        }

        // GET: ArrivalGroup/Create
        public async Task<IActionResult> Create()
        {
            int seasonId = GetActiveSeasonId();

            var viewModel = new ArrivalGroupCreateViewModel
            {
                HajjSeasonsId = seasonId,
                Airlines = await GetAirlineSelectListAsync(),
                Flights = await GetFlightSelectListAsync(),
                Bookings = await GetBookingCardsAsync(seasonId, new List<int>())
            };

            return View(viewModel);
        }

        // POST: ArrivalGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArrivalGroupCreateViewModel model)
        {
            int seasonId = GetActiveSeasonId();
            model.HajjSeasonsId = seasonId;

            if (!ModelState.IsValid)
            {
                model.Airlines = await GetAirlineSelectListAsync(model.AirlineId);
                model.Flights = await GetFlightSelectListAsync(model.FlightId);
                model.Bookings = await GetBookingCardsAsync(seasonId, model.SelectedPassengerIds ?? new List<int>());
                return View(model);
            }

            var arrivalGroup = new ArrivalGroup
            {
                HajjSeasonsId = seasonId,
                GroupName = model.GroupName,
                AirlineId = model.AirlineId,
                FlightId = model.FlightId,
                FlightNumber = model.FlightNumber,
                FlightArrivalDate = model.FlightArrivalDate,
                FlightArrivalTime = model.FlightArrivalTime,
                PNRNumber = model.PNRNumber,
                DepartureCity = model.DepartureCity,
                ArrivalCity = model.ArrivalCity,
                Remarks = model.Remarks,
                CreatedAt = DateTime.UtcNow,
                Trashed = false
            };

            _context.ArrivalGroups.Add(arrivalGroup);
            await _context.SaveChangesAsync();

            if (model.SelectedPassengerIds != null && model.SelectedPassengerIds.Any())
            {
                await SaveGroupMembersAsync(arrivalGroup.Id, model.SelectedPassengerIds, seasonId);
            }

            TempData["SuccessMessage"] = "Arrival Group created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: ArrivalGroup/Details/5
        public async Task<IActionResult> Details(int id)
        {
            int seasonId = GetActiveSeasonId();

            var arrivalGroup = await _context.ArrivalGroups
                .Include(g => g.Members.Where(m => !m.Trashed))
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (arrivalGroup == null)
            {
                return NotFound();
            }

            string airlineName = "N/A";
            if (arrivalGroup.AirlineId.HasValue)
            {
                var airline = await _context.Airlines.FindAsync(arrivalGroup.AirlineId.Value);
                if (airline != null) airlineName = airline.Name;
            }

            ViewBag.AirlineName = airlineName;
            return View(arrivalGroup);
        }

        // GET: ArrivalGroup/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int seasonId = GetActiveSeasonId();

            var arrivalGroup = await _context.ArrivalGroups
                .Include(g => g.Members.Where(m => !m.Trashed))
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (arrivalGroup == null)
            {
                return NotFound();
            }

            var selectedIds = arrivalGroup.Members
                .Select(m => m.IsHead ? (m.FamilyHeadId ?? 0) : (m.FamilyMemberId ?? 0))
                .Where(mId => mId > 0)
                .ToList();

            var viewModel = new ArrivalGroupCreateViewModel
            {
                Id = arrivalGroup.Id,
                HajjSeasonsId = arrivalGroup.HajjSeasonsId,
                GroupName = arrivalGroup.GroupName,
                AirlineId = arrivalGroup.AirlineId,
                FlightId = arrivalGroup.FlightId,
                FlightNumber = arrivalGroup.FlightNumber,
                FlightArrivalDate = arrivalGroup.FlightArrivalDate,
                FlightArrivalTime = arrivalGroup.FlightArrivalTime,
                PNRNumber = arrivalGroup.PNRNumber,
                DepartureCity = arrivalGroup.DepartureCity,
                ArrivalCity = arrivalGroup.ArrivalCity,
                Remarks = arrivalGroup.Remarks,
                SelectedPassengerIds = selectedIds,
                Airlines = await GetAirlineSelectListAsync(arrivalGroup.AirlineId),
                Flights = await GetFlightSelectListAsync(arrivalGroup.FlightId),
                Bookings = await GetBookingCardsAsync(seasonId, selectedIds)
            };

            return View(viewModel);
        }

        // POST: ArrivalGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArrivalGroupCreateViewModel model)
        {
            int seasonId = GetActiveSeasonId();

            if (id != model.Id)
            {
                return BadRequest();
            }

            var arrivalGroup = await _context.ArrivalGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (arrivalGroup == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Airlines = await GetAirlineSelectListAsync(model.AirlineId);
                model.Flights = await GetFlightSelectListAsync(model.FlightId);
                model.Bookings = await GetBookingCardsAsync(seasonId, model.SelectedPassengerIds ?? new List<int>());
                return View(model);
            }

            arrivalGroup.GroupName = model.GroupName;
            arrivalGroup.AirlineId = model.AirlineId;
            arrivalGroup.FlightId = model.FlightId;
            arrivalGroup.FlightNumber = model.FlightNumber;
            arrivalGroup.FlightArrivalDate = model.FlightArrivalDate;
            arrivalGroup.FlightArrivalTime = model.FlightArrivalTime;
            arrivalGroup.PNRNumber = model.PNRNumber;
            arrivalGroup.DepartureCity = model.DepartureCity;
            arrivalGroup.ArrivalCity = model.ArrivalCity;
            arrivalGroup.Remarks = model.Remarks;

            // Remove existing members
            foreach (var m in arrivalGroup.Members)
            {
                m.Trashed = true;
            }

            await _context.SaveChangesAsync();

            // Add newly selected members
            if (model.SelectedPassengerIds != null && model.SelectedPassengerIds.Any())
            {
                await SaveGroupMembersAsync(arrivalGroup.Id, model.SelectedPassengerIds, seasonId);
            }

            TempData["SuccessMessage"] = "Arrival Group updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: ArrivalGroup/MoveToTrash/5
        public async Task<IActionResult> MoveToTrash(int id)
        {
            int seasonId = GetActiveSeasonId();

            var arrivalGroup = await _context.ArrivalGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (arrivalGroup != null)
            {
                arrivalGroup.Trashed = true;
                foreach (var member in arrivalGroup.Members)
                {
                    member.Trashed = true;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Arrival Group moved to trash.";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private async Task<List<SelectListItem>> GetAirlineSelectListAsync(int? selectedId = null)
        {
            var airlines = await _context.Airlines
                .Where(a => !a.Trashed)
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.AirlineId.ToString(),
                    Text = $"{a.Name} ({a.Code})",
                    Selected = selectedId.HasValue && a.AirlineId == selectedId.Value
                })
                .ToListAsync();

            return airlines;
        }

        private async Task<List<SelectListItem>> GetFlightSelectListAsync(int? selectedId = null)
        {
            var flights = await _context.Flights
                .Where(f => !f.Trashed)
                .OrderBy(f => f.OutboundFlightNumber)
                .Select(f => new SelectListItem
                {
                    Value = f.FlightId.ToString(),
                    Text = $"{f.OutboundFlightNumber} - {f.Name}",
                    Selected = selectedId.HasValue && f.FlightId == selectedId.Value
                })
                .ToListAsync();

            return flights;
        }

        private async Task<List<BookingCardViewModel>> GetBookingCardsAsync(int seasonId, List<int> selectedPassengerIds)
        {
            selectedPassengerIds ??= new List<int>();

            var rawMembers = await _context.FamilyMembers
                .Include(fm => fm.FamilyHead)
                .Include(fm => fm.Head)
                .Where(fm => fm.FamilyHead != null && !fm.FamilyHead.Trashed && fm.FamilyHead.HajjSeasonsId == seasonId)
                .AsNoTracking()
                .ToListAsync();

            var cards = rawMembers
                .GroupBy(fm => fm.HeadId)
                .Select(g =>
                {
                    var headMember = g.FirstOrDefault(m => m.IsHead) ?? g.First();
                    string headName = headMember.Head != null
                        ? $"{headMember.Head.GivenName} {headMember.Head.SurName}".Trim()
                        : $"{headMember.FamilyHead.GivenName} {headMember.FamilyHead.SurName}".Trim();

                    if (string.IsNullOrWhiteSpace(headName))
                    {
                        headName = $"Booking #{g.Key}";
                    }

                    return new BookingCardViewModel
                    {
                        HeadId = g.Key,
                        BookingCode = $"BK-{g.Key}",
                        HeadName = headName,
                        Passengers = g.Select(m => new PassengerSelectionItemViewModel
                        {
                            IndividualId = m.FamilyHeadId,
                            FamilyHeadId = m.IsHead ? m.FamilyHeadId : m.HeadId,
                            FamilyMemberId = m.IsHead ? (int?)null : m.FamilyHeadId,
                            IsHead = m.IsHead,
                            FullName = $"{m.FamilyHead.GivenName} {m.FamilyHead.SurName}".Trim(),
                            PassportNumber = m.FamilyHead.PassportNumber ?? "",
                            CNIC = m.FamilyHead.CNIC ?? "",
                            BookingCode = $"BK-{g.Key}",
                            HeadName = headName,
                            IsSelected = selectedPassengerIds.Contains(m.FamilyHeadId)
                        }).OrderByDescending(p => p.IsHead).ThenBy(p => p.FullName).ToList()
                    };
                })
                .OrderBy(card => card.HeadId)
                .ToList();

            return cards;
        }

        private async Task SaveGroupMembersAsync(int arrivalGroupId, List<int> selectedPassengerIds, int seasonId)
        {
            var rawMembers = await _context.FamilyMembers
                .Include(fm => fm.FamilyHead)
                .Where(fm => selectedPassengerIds.Contains(fm.FamilyHeadId) && !fm.FamilyHead.Trashed && fm.FamilyHead.HajjSeasonsId == seasonId)
                .AsNoTracking()
                .ToListAsync();

            var newGroupMembers = new List<ArrivalGroupMember>();

            foreach (var m in rawMembers)
            {
                var pilgrim = m.FamilyHead;
                string fullName = $"{pilgrim.GivenName} {pilgrim.SurName}".Trim();

                newGroupMembers.Add(new ArrivalGroupMember
                {
                    ArrivalGroupId = arrivalGroupId,
                    FamilyHeadId = m.IsHead ? m.FamilyHeadId : m.HeadId,
                    FamilyMemberId = m.IsHead ? (int?)null : m.FamilyHeadId,
                    IsHead = m.IsHead,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? "N/A" : fullName,
                    PassportNumber = pilgrim.PassportNumber ?? "",
                    CNIC = pilgrim.CNIC ?? "",
                    BookingCode = $"BK-{m.HeadId}",
                    AssignedAt = DateTime.UtcNow,
                    Trashed = false
                });
            }

            if (newGroupMembers.Any())
            {
                _context.ArrivalGroupMembers.AddRange(newGroupMembers);
                await _context.SaveChangesAsync();
            }
        }

        #endregion
    }
}

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
using TMG.Models.LogisticModels;
using TMG.Models.OperationalGroupModels;
using TMG.Models.ReservationModels;

namespace TMG.Controllers.LogisticControllers
{
    public class OperationalGroupController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OperationalGroupController(TMGDbContext context, IHttpContextAccessor httpContextAccessor)
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

        // GET: OperationalGroup/Index
        public async Task<IActionResult> Index(GroupCategory? category, string search)
        {
            int seasonId = GetActiveSeasonId();

            var query = _context.OperationalGroups
                .Include(g => g.Members.Where(m => !m.Trashed))
                .Where(g => !g.Trashed && g.HajjSeasonsId == seasonId);

            if (category.HasValue)
            {
                query = query.Where(g => g.Category == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(g =>
                    g.GroupName.ToLower().Contains(term) ||
                    (g.GroupLeaderName != null && g.GroupLeaderName.ToLower().Contains(term)) ||
                    (g.LeaderMobileNumber != null && g.LeaderMobileNumber.ToLower().Contains(term)) ||
                    (g.PNRReference != null && g.PNRReference.ToLower().Contains(term)) ||
                    (g.FlightNumber != null && g.FlightNumber.ToLower().Contains(term)) ||
                    (g.BusIdentifierPlateNo != null && g.BusIdentifierPlateNo.ToLower().Contains(term))
                );
            }

            var groupsList = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();

            // Load master lookup dictionaries for summary display
            var airlineMap = await _context.Airlines.AsNoTracking().ToDictionaryAsync(a => a.AirlineId, a => a.Name);
            var vehicleMap = await _context.Vehicles.AsNoTracking().ToDictionaryAsync(v => v.VehicleId, v => v.VehicleType);
            var hotelMap = await _context.Hotels.AsNoTracking().ToDictionaryAsync(h => h.HotelId, h => h.HotelName);
            var roomMap = await _context.Rooms.AsNoTracking().ToDictionaryAsync(r => r.RoomId, r => r.RoomName);
            var routeMap = await _context.Routes.AsNoTracking().ToDictionaryAsync(r => r.RouteId, r => r.FullRoute);

            var viewModels = groupsList.Select(g =>
            {
                string summary = BuildResourceSummary(g, airlineMap, vehicleMap, hotelMap, roomMap, routeMap);

                return new OperationalGroupIndexItemViewModel
                {
                    Id = g.Id,
                    Category = g.Category,
                    GroupName = g.GroupName,
                    GroupLeaderName = g.GroupLeaderName ?? "N/A",
                    LeaderMobileNumber = g.LeaderMobileNumber ?? "",
                    ResourceSummary = summary,
                    PassengerCount = g.Members?.Count(m => !m.Trashed) ?? 0,
                    BusCapacity = g.BusCapacity,
                    CreatedAt = g.CreatedAt
                };
            }).ToList();

            ViewData["CurrentCategory"] = category;
            ViewData["CurrentSearch"] = search;
            return View(viewModels);
        }

        // GET: OperationalGroup/Create
        public async Task<IActionResult> Create(GroupCategory type = GroupCategory.Flight)
        {
            int seasonId = GetActiveSeasonId();

            var viewModel = new OperationalGroupCreateViewModel
            {
                HajjSeasonsId = seasonId,
                Category = type,
                BusCapacity = 45,
                Airlines = await GetAirlineSelectListAsync(),
                Flights = await GetFlightSelectListAsync(),
                Vehicles = await GetVehicleSelectListAsync(),
                Hotels = await GetHotelSelectListAsync(),
                Rooms = await GetRoomSelectListAsync(),
                Routes = await GetRouteSelectListAsync(),
                Bookings = await GetBookingCardsAsync(seasonId, new List<int>())
            };

            return View(viewModel);
        }

        // POST: OperationalGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OperationalGroupCreateViewModel model)
        {
            int seasonId = GetActiveSeasonId();
            model.HajjSeasonsId = seasonId;

            // Strict capacity check on submission for Bus and General groups
            if ((model.Category == GroupCategory.Bus || model.Category == GroupCategory.General) && model.BusCapacity > 0)
            {
                int selectedCount = model.SelectedPassengerIds?.Count ?? 0;
                if (selectedCount > model.BusCapacity)
                {
                    ModelState.AddModelError("", $"Capacity Limit Exceeded! You selected {selectedCount} pilgrims, which exceeds the max bus seat limit of {model.BusCapacity}.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Airlines = await GetAirlineSelectListAsync(model.AirlineId);
                model.Flights = await GetFlightSelectListAsync(model.FlightId);
                model.Vehicles = await GetVehicleSelectListAsync(model.VehicleId);
                model.Hotels = await GetHotelSelectListAsync(model.HotelId);
                model.Rooms = await GetRoomSelectListAsync(model.RoomId);
                model.Routes = await GetRouteSelectListAsync(model.RouteId);
                model.Bookings = await GetBookingCardsAsync(seasonId, model.SelectedPassengerIds ?? new List<int>());
                return View(model);
            }

            var group = new OperationalGroup
            {
                HajjSeasonsId = seasonId,
                Category = model.Category,
                GroupName = model.GroupName,
                GroupLeaderName = model.GroupLeaderName,
                LeaderMobileNumber = model.LeaderMobileNumber,
                AirlineId = model.AirlineId,
                FlightId = model.FlightId,
                FlightNumber = model.FlightNumber,
                PNRReference = model.PNRReference,
                FlightDate = model.FlightDate,
                VehicleId = model.VehicleId,
                BusIdentifierPlateNo = model.BusIdentifierPlateNo,
                BusCapacity = model.BusCapacity > 0 ? model.BusCapacity : 45,
                HotelId = model.HotelId,
                RoomId = model.RoomId,
                RouteId = model.RouteId,
                ItineraryScheduleId = model.ItineraryScheduleId,
                CreatedAt = DateTime.UtcNow,
                Trashed = false
            };

            _context.OperationalGroups.Add(group);
            await _context.SaveChangesAsync();

            if (model.SelectedPassengerIds != null && model.SelectedPassengerIds.Any())
            {
                await SaveGroupMembersAsync(group.Id, model.SelectedPassengerIds, seasonId);
            }

            TempData["SuccessMessage"] = $"{model.Category} Operational Group created successfully.";
            return RedirectToAction(nameof(Index), new { category = model.Category });
        }

        // GET: OperationalGroup/Details/5
        public async Task<IActionResult> Details(int id)
        {
            int seasonId = GetActiveSeasonId();

            var group = await _context.OperationalGroups
                .Include(g => g.Members.Where(m => !m.Trashed))
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (group == null)
            {
                return NotFound();
            }

            ViewBag.AirlineName = group.AirlineId.HasValue ? (await _context.Airlines.FindAsync(group.AirlineId.Value))?.Name : "N/A";
            ViewBag.VehicleType = group.VehicleId.HasValue ? (await _context.Vehicles.FindAsync(group.VehicleId.Value))?.VehicleType : "N/A";
            ViewBag.HotelName = group.HotelId.HasValue ? (await _context.Hotels.FindAsync(group.HotelId.Value))?.HotelName : "N/A";
            ViewBag.RoomName = group.RoomId.HasValue ? (await _context.Rooms.FindAsync(group.RoomId.Value))?.RoomName : "N/A";
            ViewBag.RouteName = group.RouteId.HasValue ? (await _context.Routes.FindAsync(group.RouteId.Value))?.FullRoute : "N/A";

            return View(group);
        }

        // GET: OperationalGroup/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int seasonId = GetActiveSeasonId();

            var group = await _context.OperationalGroups
                .Include(g => g.Members.Where(m => !m.Trashed))
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (group == null)
            {
                return NotFound();
            }

            var selectedIds = group.Members
                .Select(m => m.IsHead ? (m.FamilyHeadId ?? 0) : (m.FamilyMemberId ?? 0))
                .Where(mId => mId > 0)
                .ToList();

            var viewModel = new OperationalGroupCreateViewModel
            {
                Id = group.Id,
                HajjSeasonsId = group.HajjSeasonsId,
                Category = group.Category,
                GroupName = group.GroupName,
                GroupLeaderName = group.GroupLeaderName,
                LeaderMobileNumber = group.LeaderMobileNumber,
                AirlineId = group.AirlineId,
                FlightId = group.FlightId,
                FlightNumber = group.FlightNumber,
                PNRReference = group.PNRReference,
                FlightDate = group.FlightDate,
                VehicleId = group.VehicleId,
                BusIdentifierPlateNo = group.BusIdentifierPlateNo,
                BusCapacity = group.BusCapacity,
                HotelId = group.HotelId,
                RoomId = group.RoomId,
                RouteId = group.RouteId,
                ItineraryScheduleId = group.ItineraryScheduleId,
                SelectedPassengerIds = selectedIds,
                Airlines = await GetAirlineSelectListAsync(group.AirlineId),
                Flights = await GetFlightSelectListAsync(group.FlightId),
                Vehicles = await GetVehicleSelectListAsync(group.VehicleId),
                Hotels = await GetHotelSelectListAsync(group.HotelId),
                Rooms = await GetRoomSelectListAsync(group.RoomId),
                Routes = await GetRouteSelectListAsync(group.RouteId),
                Bookings = await GetBookingCardsAsync(seasonId, selectedIds)
            };

            return View(viewModel);
        }

        // POST: OperationalGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OperationalGroupCreateViewModel model)
        {
            int seasonId = GetActiveSeasonId();

            if (id != model.Id)
            {
                return BadRequest();
            }

            var group = await _context.OperationalGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (group == null)
            {
                return NotFound();
            }

            if ((model.Category == GroupCategory.Bus || model.Category == GroupCategory.General) && model.BusCapacity > 0)
            {
                int selectedCount = model.SelectedPassengerIds?.Count ?? 0;
                if (selectedCount > model.BusCapacity)
                {
                    ModelState.AddModelError("", $"Capacity Limit Exceeded! You selected {selectedCount} pilgrims, which exceeds the max bus seat limit of {model.BusCapacity}.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Airlines = await GetAirlineSelectListAsync(model.AirlineId);
                model.Flights = await GetFlightSelectListAsync(model.FlightId);
                model.Vehicles = await GetVehicleSelectListAsync(model.VehicleId);
                model.Hotels = await GetHotelSelectListAsync(model.HotelId);
                model.Rooms = await GetRoomSelectListAsync(model.RoomId);
                model.Routes = await GetRouteSelectListAsync(model.RouteId);
                model.Bookings = await GetBookingCardsAsync(seasonId, model.SelectedPassengerIds ?? new List<int>());
                return View(model);
            }

            group.Category = model.Category;
            group.GroupName = model.GroupName;
            group.GroupLeaderName = model.GroupLeaderName;
            group.LeaderMobileNumber = model.LeaderMobileNumber;
            group.AirlineId = model.AirlineId;
            group.FlightId = model.FlightId;
            group.FlightNumber = model.FlightNumber;
            group.PNRReference = model.PNRReference;
            group.FlightDate = model.FlightDate;
            group.VehicleId = model.VehicleId;
            group.BusIdentifierPlateNo = model.BusIdentifierPlateNo;
            group.BusCapacity = model.BusCapacity > 0 ? model.BusCapacity : 45;
            group.HotelId = model.HotelId;
            group.RoomId = model.RoomId;
            group.RouteId = model.RouteId;
            group.ItineraryScheduleId = model.ItineraryScheduleId;

            // Mark existing members as trashed
            foreach (var m in group.Members)
            {
                m.Trashed = true;
            }

            await _context.SaveChangesAsync();

            if (model.SelectedPassengerIds != null && model.SelectedPassengerIds.Any())
            {
                await SaveGroupMembersAsync(group.Id, model.SelectedPassengerIds, seasonId);
            }

            TempData["SuccessMessage"] = $"{model.Category} Operational Group updated successfully.";
            return RedirectToAction(nameof(Index), new { category = model.Category });
        }

        // POST: OperationalGroup/MoveToTrash/5
        public async Task<IActionResult> MoveToTrash(int id)
        {
            int seasonId = GetActiveSeasonId();

            var group = await _context.OperationalGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id && !g.Trashed && g.HajjSeasonsId == seasonId);

            if (group != null)
            {
                var category = group.Category;
                group.Trashed = true;
                foreach (var m in group.Members)
                {
                    m.Trashed = true;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Operational Group moved to trash.";
                return RedirectToAction(nameof(Index), new { category });
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private static string BuildResourceSummary(
            OperationalGroup g,
            Dictionary<int, string> airlines,
            Dictionary<int, string> vehicles,
            Dictionary<int, string> hotels,
            Dictionary<int, string> rooms,
            Dictionary<int, string> routes)
        {
            var parts = new List<string>();

            switch (g.Category)
            {
                case GroupCategory.Flight:
                    if (!string.IsNullOrEmpty(g.FlightNumber)) parts.Add($"Flight: {g.FlightNumber}");
                    if (!string.IsNullOrEmpty(g.PNRReference)) parts.Add($"PNR: {g.PNRReference}");
                    if (g.AirlineId.HasValue && airlines.ContainsKey(g.AirlineId.Value)) parts.Add($"Airline: {airlines[g.AirlineId.Value]}");
                    break;
                case GroupCategory.Bus:
                    if (!string.IsNullOrEmpty(g.BusIdentifierPlateNo)) parts.Add($"Bus: {g.BusIdentifierPlateNo}");
                    if (g.VehicleId.HasValue && vehicles.ContainsKey(g.VehicleId.Value)) parts.Add($"Type: {vehicles[g.VehicleId.Value]}");
                    parts.Add($"Capacity: {g.BusCapacity} Seats");
                    break;
                case GroupCategory.Hotel:
                    if (g.HotelId.HasValue && hotels.ContainsKey(g.HotelId.Value)) parts.Add($"Hotel: {hotels[g.HotelId.Value]}");
                    if (g.RoomId.HasValue && rooms.ContainsKey(g.RoomId.Value)) parts.Add($"Room: {rooms[g.RoomId.Value]}");
                    break;
                case GroupCategory.Route:
                    if (g.RouteId.HasValue && routes.ContainsKey(g.RouteId.Value)) parts.Add($"Route: {routes[g.RouteId.Value]}");
                    break;
                case GroupCategory.General:
                    if (!string.IsNullOrEmpty(g.FlightNumber)) parts.Add($"Flight: {g.FlightNumber}");
                    if (!string.IsNullOrEmpty(g.BusIdentifierPlateNo)) parts.Add($"Bus: {g.BusIdentifierPlateNo}");
                    if (g.HotelId.HasValue && hotels.ContainsKey(g.HotelId.Value)) parts.Add($"Hotel: {hotels[g.HotelId.Value]}");
                    if (g.RouteId.HasValue && routes.ContainsKey(g.RouteId.Value)) parts.Add($"Route: {routes[g.RouteId.Value]}");
                    break;
            }

            return parts.Any() ? string.Join(" | ", parts) : "General Resource Allocation";
        }

        private async Task<List<SelectListItem>> GetAirlineSelectListAsync(int? selectedId = null)
        {
            return await _context.Airlines
                .Where(a => !a.Trashed)
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.AirlineId.ToString(),
                    Text = $"{a.Name} ({a.Code})",
                    Selected = selectedId.HasValue && a.AirlineId == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetFlightSelectListAsync(int? selectedId = null)
        {
            return await _context.Flights
                .Where(f => !f.Trashed)
                .OrderBy(f => f.OutboundFlightNumber)
                .Select(f => new SelectListItem
                {
                    Value = f.FlightId.ToString(),
                    Text = $"{f.OutboundFlightNumber} - {f.Name}",
                    Selected = selectedId.HasValue && f.FlightId == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetVehicleSelectListAsync(int? selectedId = null)
        {
            return await _context.Vehicles
                .Where(v => !v.Trashed)
                .OrderBy(v => v.VehicleType)
                .Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.VehicleType} (Cap: {v.PAX})",
                    Selected = selectedId.HasValue && v.VehicleId == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetHotelSelectListAsync(int? selectedId = null)
        {
            return await _context.Hotels
                .Where(h => !h.Trashed)
                .OrderBy(h => h.HotelName)
                .Select(h => new SelectListItem
                {
                    Value = h.HotelId.ToString(),
                    Text = $"{h.HotelName} ({h.Location})",
                    Selected = selectedId.HasValue && h.HotelId == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetRoomSelectListAsync(int? selectedId = null)
        {
            return await _context.Rooms
                .Where(r => !r.Trashed)
                .OrderBy(r => r.RoomName)
                .Select(r => new SelectListItem
                {
                    Value = r.RoomId.ToString(),
                    Text = $"{r.RoomName} ({r.PAX} Pax Bed)",
                    Selected = selectedId.HasValue && r.RoomId == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetRouteSelectListAsync(int? selectedId = null)
        {
            return await _context.Routes
                .Where(r => !r.Trashed)
                .OrderBy(r => r.StartPlace)
                .Select(r => new SelectListItem
                {
                    Value = r.RouteId.ToString(),
                    Text = $"{r.StartPlace} to {r.Destination}",
                    Selected = selectedId.HasValue && r.RouteId == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<List<OperationalBookingGroupViewModel>> GetBookingCardsAsync(int seasonId, List<int> selectedPassengerIds)
        {
            selectedPassengerIds ??= new List<int>();

            var rawMembers = await _context.FamilyMembers
                .Include(fm => fm.FamilyHead)
                .ThenInclude(fh => fh.Package)
                .ThenInclude(p => p.Company)
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

                    string companyName = headMember.FamilyHead?.Package?.Company?.CompanyName ?? "Default Agency";

                    return new OperationalBookingGroupViewModel
                    {
                        HeadId = g.Key,
                        BookingCode = $"#BK-{g.Key}",
                        HeadName = headName,
                        PaxCount = g.Count(),
                        ClientCompanyName = companyName,
                        Passengers = g.Select(m => new OperationalPassengerSelectionItemViewModel
                        {
                            IndividualId = m.FamilyHeadId,
                            FamilyHeadId = m.IsHead ? m.FamilyHeadId : m.HeadId,
                            FamilyMemberId = m.IsHead ? (int?)null : m.FamilyHeadId,
                            IsHead = m.IsHead,
                            FullName = $"{m.FamilyHead.GivenName} {m.FamilyHead.SurName}".Trim(),
                            CNIC = m.FamilyHead.CNIC ?? "",
                            PassportNumber = m.FamilyHead.PassportNumber ?? "",
                            Gender = m.FamilyHead.Gender.ToString(),
                            BookingCode = $"#BK-{g.Key}",
                            ClientCompanyName = companyName,
                            HeadName = headName,
                            HeadPaxCount = g.Count(),
                            IsSelected = selectedPassengerIds.Contains(m.FamilyHeadId)
                        }).OrderByDescending(p => p.IsHead).ThenBy(p => p.FullName).ToList()
                    };
                })
                .OrderBy(card => card.HeadId)
                .ToList();

            return cards;
        }

        private async Task SaveGroupMembersAsync(int operationalGroupId, List<int> selectedPassengerIds, int seasonId)
        {
            var rawMembers = await _context.FamilyMembers
                .Include(fm => fm.FamilyHead)
                .ThenInclude(fh => fh.Package)
                .ThenInclude(p => p.Company)
                .Where(fm => selectedPassengerIds.Contains(fm.FamilyHeadId) && !fm.FamilyHead.Trashed && fm.FamilyHead.HajjSeasonsId == seasonId)
                .AsNoTracking()
                .ToListAsync();

            var newGroupMembers = new List<OperationalGroupMember>();

            foreach (var m in rawMembers)
            {
                var pilgrim = m.FamilyHead;
                string fullName = $"{pilgrim.GivenName} {pilgrim.SurName}".Trim();
                string companyName = pilgrim.Package?.Company?.CompanyName ?? "Default Agency";

                newGroupMembers.Add(new OperationalGroupMember
                {
                    OperationalGroupId = operationalGroupId,
                    FamilyHeadId = m.IsHead ? m.FamilyHeadId : m.HeadId,
                    FamilyMemberId = m.IsHead ? (int?)null : m.FamilyHeadId,
                    IsHead = m.IsHead,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? "N/A" : fullName,
                    CNIC = pilgrim.CNIC ?? "",
                    PassportNumber = pilgrim.PassportNumber ?? "",
                    Gender = pilgrim.Gender.ToString(),
                    BookingCode = $"#BK-{m.HeadId}",
                    ClientCompanyName = companyName,
                    AssignedAt = DateTime.UtcNow,
                    Trashed = false
                });
            }

            if (newGroupMembers.Any())
            {
                _context.OperationalGroupMembers.AddRange(newGroupMembers);
                await _context.SaveChangesAsync();
            }
        }

        #endregion
    }
}

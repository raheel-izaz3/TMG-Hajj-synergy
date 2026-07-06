using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using TMG.Models.POModels;
using TMG.Models.TransportationBlock;
using TMG.Models.TransportationModels;

namespace TMG.Controllers.TransportationBlock
{

    public class SingleTransportationModel
    {
        public int? FamilyHeadId { get; set; }
        public int? VehicleId { get; set; }
        public int? NoOfVehicle { get; set; }
        public string ConnectedNumber { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public DateTime? FlightArrival { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? FlightDeparture { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? TrainArrival { get; set; }
        public DateTime? TrainDeparture { get; set; }
        public DateTime? ActualCheckIn { get; set; }
        public DateTime? ActualCheckOut { get; set; }
        public int BlockId { get; set; } // Optional
    }

    public class VehicleInfoModel
    {
        public int FamilyHeadId { get; set; }
        public int? NoOfVehicle { get; set; }
        public string ConnectedNumber { get; set; }
    }

    public class TransportationBlocksController : Controller
    {
        public static int GroupId = 0;
        public static int HotelId = 0;
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public TransportationBlocksController(TMGDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _http = contextAccessor;
        }
        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.TransportationBlocks
                .Where(e => !e.Trashed && e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count();


            var itinerary = _dbContext.TransportationBlocks
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Include(e => e.Place)
                .Include(e => e.Package)
                .Include(e => e.Route)
                .Where(e => !e.Trashed);
            List<int> selected = new List<int>();
            foreach (var item in itinerary)
            {
                var groupMembers = _dbContext.TransportationBlockMembers.Where(e => e.TransportationBlocksId == item.TransportationBlocksId).ToList();
                selected.Add(groupMembers.Count);
            }

            ViewBag.Selected = selected;
            return View(itinerary);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Routes = _dbContext.RoutesWithoutTrash
                .ToList();

            ViewBag.Place = _dbContext.PlacesWithoutTrash.ToList();


            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            return View();
        }

        [HttpPost]
        public IActionResult Create(Models.TransportationBlock.TransportationBlocks transportationBlocks)
        {
            ViewBag.Routes = _dbContext.RoutesWithoutTrash
                .ToList();


            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.TransportationBlocks.Add(transportationBlocks);
                _dbContext.SaveChanges();

                transportationBlocks.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                for (int i = 0; i < (transportationBlocks.PackageIds?.Count ?? 0); i++)
                {
                    if (transportationBlocks.PackagesSelected[i] == false) continue;

                    _dbContext.TranspotationBlockGroups.Add(new TranspotationBlockGroups
                    {
                        PackageId = transportationBlocks.PackageIds[i],
                        TransportationBlocksId = transportationBlocks.TransportationBlocksId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(transportationBlocks);
            }

        }

        public IActionResult Edit(int id)
        {
            ViewBag.Routes = _dbContext.RoutesWithoutTrash
               .ToList();

            ViewBag.Place = _dbContext.PlacesWithoutTrash.ToList();

            ViewBag.GroupPackages = _dbContext.TranspotationBlockGroups.Where(e => e.TransportationBlocksId == id);
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            Models.TransportationBlock.TransportationBlocks Group = _dbContext.TransportationBlocks.Include(e => e.Route).FirstOrDefault(e => e.TransportationBlocksId == id);

            return View(Group);
        }

        [HttpPost]
        public IActionResult Edit(Models.TransportationBlock.TransportationBlocks group)
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            if (!ModelState.IsValid) return View(group);


            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                group.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                _dbContext.Entry(group).State = EntityState.Modified;

                _dbContext.SaveChanges();
                _dbContext.TranspotationBlockGroups.Where(e => e.TransportationBlocksId == group.TransportationBlocksId).ExecuteDelete();
                for (int i = 0; i < (group.PackageIds?.Count ?? 0); i++)
                {
                    if (group.PackagesSelected[i] == false) continue;

                    _dbContext.TranspotationBlockGroups.Add(new TranspotationBlockGroups
                    {
                        PackageId = group.PackageIds[i],
                        TransportationBlocksId = group.TransportationBlocksId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(group);
            }

        }

        public IActionResult AddMember(int groupId, int PlaceId, int RouteId)
        {
            Models.TransportationBlock.TransportationBlocks group = _dbContext.TransportationBlocks.Include(e => e.Route).FirstOrDefault(e => e.TransportationBlocksId == groupId);

            ViewBag.GroupId = groupId;
            //ViewBag.GroupCode = group.POHotels.Code;
            //ViewBag.GroupName = group.Name;
            ViewBag.GroupPAX = group.PAX;

            var groupMembers = _dbContext.TransportationBlockMembers.Where(e => e.TransportationBlocksId == groupId && !e.TransportationBlock.Trashed && !e.FamilyHead.Trashed).ToList(); // Fetching group members once


            ViewBag.GroupMembers = groupMembers;
            ViewBag.GroupMembersCount = groupMembers.Count();

            foreach (var currentMember in groupMembers)
            {
                bool? inSameGroup = null;
                var _m = _dbContext.TransportationBlockMembers
                    .Where(a => a.FamilyHead.Trashed == false)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                if (_m is null)
                {
                    inSameGroup = null;
                }
                else if (_m.TransportationBlocksId == groupId)
                {
                    inSameGroup = true;
                }
                else if (_m.TransportationBlocksId != groupId)
                {
                    inSameGroup = false;
                }

                ViewBag.InSameGroup = inSameGroup;
            }

            var grouppackages = _dbContext.TranspotationBlockGroups.Where(e => e.TransportationBlocksId == group.TransportationBlocksId);

            List<FamilyMembers> model = new List<FamilyMembers>();
            foreach (var package in grouppackages)
            {
                var familymember = _dbContext.FamilyMembers
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Reference)
                    .Where(e => e.FamilyHead.PackageId == package.PackageId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
                    .OrderBy(e => e.HeadId)
                    .ThenBy(e => e.FamilyHeadId);

                model.AddRange(familymember);
            }


            ViewBag.selectedmemebers = _dbContext.TransportationBlockMembers.Where(e => e.TransportationBlocksId != groupId && e.TransportationBlock.RouteId == RouteId && !e.TransportationBlock.Trashed);
            //ViewBag.selectedmemebers = new List<ItineraryBlockGroupMmebers>();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddMembers(int groupId, int memberId, int RouteId)
        {
            int paxLimit = _dbContext.TransportationBlocks.FirstOrDefault(e => !e.Trashed && e.TransportationBlocksId == groupId).PAX;
            int selected = _dbContext.TransportationBlockMembers.Where(e => e.TransportationBlocksId == groupId).Count();

            var exists = _dbContext.TransportationBlockMembers
                .Where(e => e.FamilyHeadId == memberId)
                .Join(_dbContext.TransportationBlocks, gm => gm.TransportationBlocksId, g => g.TransportationBlocksId, (gm, g) => new { gm, g })
                .FirstOrDefault(x => x.g.Trashed == false && x.g.RouteId == RouteId);

            if (exists is null)
            {
                _dbContext.TransportationBlockMembers.Add(
                    new TransportationBlockMembers { FamilyHeadId = memberId, TransportationBlocksId = groupId }
                );

                _dbContext.TransportationBlocks.FirstOrDefault(e => e.TransportationBlocksId == groupId).Selected = selected + 1;

                _dbContext.SaveChanges();

                return Ok(new { status = 200, message = $"Member Id: {memberId} added to group Id: {groupId}" });
            }

            return Ok(new { status = 200, message = $"Member Id: {memberId} already exists in a valid group" });
        }

        [HttpPost]
        public IActionResult RemoveMember(int groupId, int memberId)
        {
            int selected = _dbContext.TransportationBlocks.Where(e => e.TransportationBlocksId == groupId).Count();
            /*var sgm = _dbContext.SubGroupMembers.FirstOrDefault(e => e.FamilyHeadId == memberId && e.SubGroup.GroupId == groupId);
            if (sgm != null)
            {
                _dbContext.SubGroupMembers.Remove(sgm);
            }*/

            var gm = _dbContext.TransportationBlockMembers.FirstOrDefault(e => e.TransportationBlocksId == groupId && e.FamilyHeadId == memberId);
            if (gm != null)
            {
                _dbContext.TransportationBlockMembers.Remove(gm);
                _dbContext.TransportationBlocks.FirstOrDefault(e => e.TransportationBlocksId == groupId).Selected = selected - 1;
            }

            _dbContext.SaveChanges();

            return Ok(new { status = 200, message = $"Member Id: {memberId} removed from group Id: {groupId}" });
        }

        /*public IActionResult TransportationBlockSource(int id, int[] PackageId,int RouteId)
        {
            GroupId = id;


            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash
                            .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .ToList();

            var group = _dbContext.TransportationBlocks
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.TransportationBlocksId,
                                    e.Route.FullRoute,
                                    //e.Name,
                                    //GroupCode = e.POHotels.Code,
                                    //GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                })
                                .FirstOrDefault(e => e.TransportationBlocksId == id);



            ViewBag.SubGroupName = group?.FullRoute;
            //ViewBag.GroupCode = group?.GroupCode;
            //ViewBag.GroupName = group?.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _dbContext.TransportationBlockMembers
                        join sg in _dbContext.TransportationBlocks on gm.TransportationBlocksId equals sg.TransportationBlocksId
                        join g in _dbContext.TransportationBlocks on sg.TransportationBlocksId equals g.TransportationBlocksId
                        join fh in _dbContext.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where gm.TransportationBlocksId == id && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.TransportationBlocksId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Code,
                            fh.ReferenceId,
                            //GroupName = g.Name,
                            GroupCode = g.Route.FullRoute,
                            fh.GivenName,
                            fm.FamilyHead.VisaNumber,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId,
                            fh.RouteAsPerPackage,
                        };

            var result = query.ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.PackageId == group.PackageId);
            var familyHeadAdditionalFacilities = _dbContext
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == group.PackageId);

            // var groupMemberRooms = _context.GroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                string roomType = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

               
                model.Members.Add(new RoomListingMember
                {
                    IsHead = _.IsHead,
                    FamilyHeadId = _.FamilyHeadId,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    RoomPax = roomPax ?? 5,
                    VisaNumber = _.VisaNumber,
                    FcId = _.FcId,
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    PackageId = _.PackageId,
                    RouteName = _.RouteAsPerPackage 
                    ? _dbContext.TravelRouteStops
                        .Include(e => e.Route)
                        .Where(e => e.RouteId == RouteId)
                        .Select(e => e.Route.FullRoute)
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadTransports
                        .Include(e => e.Route)
                        .Where(e => e.RouteId == RouteId)
                        .Select(e => e.Route.FullRoute)
                        .FirstOrDefault(),

                    VehicleId = _.RouteAsPerPackage
                    ? _dbContext.TravelRouteStops
                        .Include(e => e.Vehicle)
                        .Where(e => e.RouteId == RouteId)
                        .Select(e => e.Vehicle.VehicleId)
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadTransports
                        .Include(e => e.Vehicle)
                        .Where(e => e.RouteId == RouteId)
                        .Select(e => e.Vehicle.VehicleId)
                        .FirstOrDefault(),
                });
            }

            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = group.CompanyName;
            //model.Group = group.GroupName;
            //model.SubGroup = group.Name;

            
            return View(model);
        }*/

        public async Task<IActionResult> TransportationBlockSource(int id, int[] PackageId, int RouteId, int PlaceId)
        {
            GroupId = id;

            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash
                .ToList();

            var group = _dbContext.TransportationBlocks
                .Include(e => e.Package)
                .ThenInclude(e => e.Company)
                .Include(e => e.Route)
                .Where(e => e.TransportationBlocksId == id)
                .Select(e => new
                {
                    e.TransportationBlocksId,
                    e.Route.FullRoute,
                    e.Package.Company.CompanyName,
                    e.PackageId,
                })
                .FirstOrDefault();

            ViewBag.SubGroupName = group?.FullRoute;



            /*var Counts = await _dbContext.TranspportationBlockReservations
                .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                .GroupBy(t => new { t.VehicleId, t.Vehicle.VehicleType })
                .Select(g => new VehicleStayWithCountViewModel
                {
                    VehicleName = g.Key.VehicleType,
                    StayWithCount = g.Sum(x => x.StayWith ?? 0) // Yeh count nahi, SUM karega
                })
                .ToListAsync();*/

            if (RouteId == 19)
            {
                /*var counts = await _dbContext.TranspportationBlockReservations
                    .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                    .GroupBy(t => t.ArrivalFlightNumber)
                    .Select(g => new VehicleStayWithCountViewModel
                    {
                        ArrivalFlightNumber = g.Key,
                        CheckInDate = g.FirstOrDefault().FlightArrival.Value.ToString("HH:mm"),
                        ArrivalTime = g.FirstOrDefault().ArrivalDate.Value.ToString("yyyy-MM-dd"),
                        VehicleName = g.FirstOrDefault().Vehicle.VehicleType,
                        StayWithCount = g.Sum(x => x.StayWith ?? 0)
                    })
                    .ToListAsync();

                ViewBag.Counts = counts;*/

                var counts = await _dbContext.TranspportationBlockReservations
                    .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                    .GroupBy(t => new { t.ArrivalFlightNumber, t.VehicleId, t.Vehicle.VehicleType })
                    .Select(g => new VehicleStayWithCountViewModel
                    {
                        ArrivalFlightNumber = g.Key.ArrivalFlightNumber,
                        VehicleName = g.Key.VehicleType,
                        CheckInDate = g.FirstOrDefault().FlightArrival.Value.ToString("HH:mm"),
                        ArrivalTime = g.FirstOrDefault().ArrivalDate.Value.ToString("yyyy-MM-dd"),
                        StayWithCount = g.Sum(x => x.StayWith ?? 0)
                    })
                    .ToListAsync();

                ViewBag.Counts = counts;



            }
            else if (RouteId == 20)
            {
                var counts = await _dbContext.TranspportationBlockReservations
                   .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                   .GroupBy(t => new { t.TrainDeparture, t.VehicleId, t.Vehicle.VehicleType })
                   .Select(g => new VehicleStayWithCountViewModel
                   {
                       ArrivalFlightNumber = g.Key.TrainDeparture.Value.ToString("yyyy-MM-ddTHH:mm"),
                       VehicleName = g.Key.VehicleType,
                       RouteId = g.FirstOrDefault().TransportationBlocks.RouteId,
                       StayWithCount = g.Sum(x => x.StayWith ?? 0)
                   })
                   .ToListAsync();

                ViewBag.Counts = counts;
            }
            else if (RouteId == 21)
            {
                var counts = await _dbContext.TranspportationBlockReservations
                   .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                   .GroupBy(t => new { t.TrainArrival, t.VehicleId, t.Vehicle.VehicleType })
                   .Select(g => new VehicleStayWithCountViewModel
                   {
                       ArrivalFlightNumber = g.Key.TrainArrival.Value.ToString("yyyy-MM-ddTHH:mm"),
                       VehicleName = g.Key.VehicleType,
                       RouteId = g.FirstOrDefault().TransportationBlocks.RouteId,
                       StayWithCount = g.Sum(x => x.StayWith ?? 0)
                   })
                   .ToListAsync();

                ViewBag.Counts = counts;
            }
            else if (RouteId == 22)
            {
                var counts = await _dbContext.TranspportationBlockReservations
                   .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                   .GroupBy(t => new { t.TrainArrival, t.VehicleId, t.Vehicle.VehicleType })
                   .Select(g => new VehicleStayWithCountViewModel
                   {
                       ArrivalFlightNumber = g.Key.TrainArrival.Value.ToString("yyyy-MM-ddTHH:mm"),
                       VehicleName = g.Key.VehicleType,
                       RouteId = g.FirstOrDefault().TransportationBlocks.RouteId,
                       StayWithCount = g.Sum(x => x.StayWith ?? 0)
                   })
                   .ToListAsync();

                ViewBag.Counts = counts;
            }
            else if (RouteId == 24)
            {
                var counts = await _dbContext.TranspportationBlockReservations
                   .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                   .GroupBy(t => new { t.ActualCheckOut, t.VehicleId, t.Vehicle.VehicleType })
                   .Select(g => new VehicleStayWithCountViewModel
                   {
                       ArrivalFlightNumber = g.Key.ActualCheckOut.Value.ToString("yyyy-MM-ddTHH:mm"),
                       VehicleName = g.Key.VehicleType,
                       RouteId = g.FirstOrDefault().TransportationBlocks.RouteId,
                       StayWithCount = g.Sum(x => x.StayWith ?? 0)
                   })
                   .ToListAsync();

                ViewBag.Counts = counts;
            }
            else if (RouteId == 45)
            {
                var counts = await _dbContext.TranspportationBlockReservations
                   .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                   .GroupBy(t => new { t.FlightArrival, t.ArrivalDate, t.DepartureDate, t.VehicleId, t.Vehicle.VehicleType })
                   .Select(g => new VehicleStayWithCountViewModel
                   {
                       CheckInDate = g.Key.ArrivalDate.Value.ToString("yyyy-MM-dd"),
                       ArrivalTime = g.Key.FlightArrival.Value.ToString("HH:mm"),
                       VehicleName = g.Key.VehicleType,
                       StayWithCount = g.Sum(x => x.StayWith ?? 0)
                   })
                   .ToListAsync();

                ViewBag.Counts = counts;
            }
            else if (RouteId == 52)
            {
                var counts = await _dbContext.TranspportationBlockReservations
                   .Where(t => t.TransportationBlocksId == id && t.StayWith != null)
                   .GroupBy(t => new { t.FlightArrival, t.ArrivalDate, t.DepartureDate, t.VehicleId, t.Vehicle.VehicleType })
                   .Select(g => new VehicleStayWithCountViewModel
                   {
                       CheckInDate = g.Key.ArrivalDate.Value.ToString("yyyy-MM-dd"),
                       ArrivalTime = g.Key.FlightArrival.Value.ToString("HH:mm"),
                       VehicleName = g.Key.VehicleType,
                       StayWithCount = g.Sum(x => x.StayWith ?? 0)
                   })
                   .ToListAsync();

                ViewBag.Counts = counts;
            }





            //ViewBag.Counts = Counts;


            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _dbContext.TransportationBlockMembers
                        join g in _dbContext.TransportationBlocks on gm.TransportationBlocksId equals g.TransportationBlocksId
                        join fh in _dbContext.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where gm.TransportationBlocksId == id && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.TransportationBlocksId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Code,
                            fh.ReferenceId,
                            GroupCode = g.Route.FullRoute,
                            fh.GivenName,
                            fm.FamilyHead.VisaNumber,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId,
                            fh.RouteAsPerPackage,
                            fh.AccomodationAsPerPackage,
                            fh.Package.TravelRouteId,
                            fh.CompanyVisas
                        };

            var result = query.ToList();

            // Preload families & additional facilities
            var families = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.PackageId == group.PackageId)
                .Select(e => new { e.GivenName, e.SurName, e.FamilyHeadId, e.FcId, e.Package })
                .ToList();

            var additionalFacilities = _dbContext.FamilyHeadAdditionalFacilities
                .Include(e => e.AdditionalFacilities)
                .ThenInclude(e => e.Place)
                .Include(e => e.AdditionalFacilities.Room)
                .Where(e => e.FamilyHead.PackageId == group.PackageId)
                .ToList();

            // Preload routes & transports
            var travelRouteStopData = _dbContext.TravelRouteStops
                .Include(e => e.Route)
                .Include(e => e.Vehicle)
                .Where(e => e.RouteId == RouteId && e.TravelRouteId == result.FirstOrDefault().TravelRouteId)
                .Select(e => new { e.RouteId, e.Route.FullRoute, e.Vehicle.VehicleId })
                .FirstOrDefault();

            var familyHeadTransportData = _dbContext.FamilyHeadTransports
                .Include(e => e.Route)
                .Include(e => e.Vehicle)
                .Where(e => e.RouteId == RouteId)
                .ToList()
                .GroupBy(e => e.FamilyHeadId)
                .ToDictionary(g => g.Key, g => g.First());



            // Build members
            for (int i = 0; i < result.Count; i++)
            {
                var member = result[i];

                var stayWith = families.FirstOrDefault(e => e.FamilyHeadId == member.StayWithId);
                string stayString = stayWith != null ? $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})" : "-";

                var room = additionalFacilities
                    .Where(e => e.AdditionalFacilities.PlaceId == 4 && e.FamilyHeadId == member.FamilyHeadId)
                    .Select(e => new
                    {
                        e.AdditionalFacilities.Room.RoomName,
                        e.AdditionalFacilities.Room.PAX,
                        e.AdditionalFacilities.Room.RoomId
                    })
                    .FirstOrDefault();

                DateTime? CheckInDate = null;
                DateTime? CheckOutDate = null;


                if (RouteId == 20 || RouteId == 21 || RouteId == 22 || RouteId == 24)
                {
                    if (member.AccomodationAsPerPackage)
                    {
                        var accommodation = _dbContext.PackageAccomodtions
                            .Where(e => e.PackageId == member.PackageId && e.PlaceId == PlaceId)
                            .Select(e => new
                            {
                                e.ActualCheckInTime,
                                e.ActualCheckOutTime
                            })
                            .FirstOrDefault();

                        if (accommodation != null)
                        {
                            CheckInDate = accommodation.ActualCheckInTime;
                            CheckOutDate = accommodation.ActualCheckOutTime;
                        }
                    }
                    else
                    {
                        var accommodation = _dbContext.FamilyHeadAccomodations
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.Hotel.PlaceId == PlaceId)
                            .Select(e => new
                            {
                                e.CheckIn,
                                e.CheckOut
                            })
                            .FirstOrDefault();

                        if (accommodation != null)
                        {
                            CheckInDate = accommodation.CheckIn;
                            CheckOutDate = accommodation.CheckOut;
                        }
                    }
                }


                string routeName;
                int? vehicleId;

                if (member.RouteAsPerPackage)
                {
                    routeName = travelRouteStopData?.FullRoute;
                    vehicleId = travelRouteStopData?.VehicleId;
                }
                else
                {
                    var transport = familyHeadTransportData.ContainsKey(member.FamilyHeadId)
                        ? familyHeadTransportData[member.FamilyHeadId]
                        : null;

                    routeName = transport?.Route?.FullRoute;
                    vehicleId = transport?.Vehicle?.VehicleId;
                }


                /*string HotelName = string.Empty;

                if (member.AccomodationAsPerPackage)
                {
                    var accommodation = _dbContext.PackageAccomodtions
                            .Include(e => e.Hotel)
                            .Where(e => e.PackageId == member.PackageId && e.PlaceId == PlaceId)
                            .Select(e => new
                            {
                                e.Hotel.HotelName
                            })
                            .FirstOrDefault();

                    if (accommodation != null)
                    {
                        HotelName = accommodation?.HotelName;
                    }
                }
                else
                {
                    var accommodation = _dbContext.FamilyHeadAccomodations
                           .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.Hotel.PlaceId == PlaceId)
                           .Select(e => new
                           {
                               e.Hotel.HotelName
                           })
                           .FirstOrDefault();

                    if (accommodation != null)
                    {
                        HotelName = accommodation?.HotelName;
                    }
                }

                string NextHotelName = string.Empty;

                if (member.AccomodationAsPerPackage)
                {
                    var accommodation = _dbContext.PackageAccomodtions
                            .Include(e => e.Hotel)
                            .Where(e => e.PackageId == member.PackageId && e.ActualCheckInTime == CheckOutDate && e.PlaceId == 4)
                            .Select(e => new
                            {
                                e.Hotel.HotelName
                            })
                            .FirstOrDefault();

                    if (accommodation != null)
                    {
                        HotelName = accommodation?.HotelName;
                    }
                }
                else
                {
                    var accommodation = _dbContext.FamilyHeadAccomodations
                           .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.CheckIn == CheckOutDate && e.Hotel.PlaceId == 4)
                           .Select(e => new
                           {
                               e.Hotel.HotelName
                           })
                           .FirstOrDefault();

                    if (accommodation != null)
                    {
                        HotelName = accommodation?.HotelName;
                    }
                }*/

                string HotelName = string.Empty;
                string NextHotelName = string.Empty;



                // Current Hotel
                if (member.AccomodationAsPerPackage)
                {
                    var accommodation = _dbContext.PackageAccomodtions
                        .Include(e => e.Hotel)
                        .Where(e => e.PackageId == member.PackageId && e.PlaceId == PlaceId)
                        .Select(e => new
                        {
                            e.Hotel.HotelName
                        })
                        .FirstOrDefault();

                    if (accommodation != null)
                    {
                        HotelName = accommodation?.HotelName;
                    }
                }
                else
                {
                    var accommodation = _dbContext.FamilyHeadAccomodations
                       .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.Hotel.PlaceId == PlaceId)
                       .Select(e => new
                       {
                           e.Hotel.HotelName
                       })
                       .FirstOrDefault();

                    if (accommodation != null)
                    {
                        HotelName = accommodation?.HotelName;
                    }
                }


                if (member.AccomodationAsPerPackage)
                {
                    var nextAccommodation = _dbContext.PackageAccomodtions
                        .Include(e => e.Hotel)
                        .Where(e => e.PackageId == member.PackageId
                            && e.ActualCheckInTime != null
                            && CheckOutDate != null
                            && e.ActualCheckInTime.Value.Date == CheckOutDate.Value.Date
                            && e.PlaceId == 4)
                        .Select(e => new
                        {
                            e.Hotel.HotelName
                        })
                        .FirstOrDefault();

                    if (nextAccommodation != null)
                    {
                        NextHotelName = nextAccommodation?.HotelName;
                    }
                }
                else
                {
                    var nextAccommodation = _dbContext.FamilyHeadAccomodations
                        .Where(e => e.FamilyHeadId == member.FamilyHeadId
                            && e.CheckIn != null
                            && CheckOutDate != null
                            && e.CheckIn.Value.Date == CheckOutDate.Value.Date
                            && e.Hotel.PlaceId == 4)
                        .Select(e => new
                        {
                            e.Hotel.HotelName
                        })
                        .FirstOrDefault();

                    if (nextAccommodation != null)
                    {
                        NextHotelName = nextAccommodation?.HotelName;
                    }
                }

                var vehicleData = _dbContext.TranspportationBlockReservations
                                            .Include(e => e.Vehicle)
                                            .OrderBy(e => e.TranspportationBlockReservationId)
                                            .LastOrDefault(e => e.FamilyHeadId == member.FamilyHeadId && e.TransportationBlocksId == id);


                var routesNames = _dbContext.TransportationBlocks
                           .Where(e => e.TransportationBlocksId == id)
                           .Select(f => new { f.Route.FullRoute }).FirstOrDefault();


                model.Members.Add(new RoomListingMember
                {
                    IsHead = member.IsHead,
                    FamilyHeadId = member.FamilyHeadId,
                    SNo = i + 1,
                    Company = member.CompanyCode,
                    Package = member.PackageName,
                    Group = member.GroupCode,
                    RoomPax = room?.PAX ?? 5,
                    VisaNumber = member.VisaNumber,
                    FcId = member.FcId,
                    Name = $"{member.GivenName} {member.SurName}",
                    Age = member.Age,
                    Gender = member.Gender.ToString(),
                    Passport = member.PassportNumber,
                    PackageId = member.PackageId,
                    RouteName = routesNames?.FullRoute,
                    VehicleId = vehicleId,
                    //CheckOutForReservation = vehicleData?.Date.ToString("yyyy-MM-ddTHH:mm"),
                    TranspotationVehicleId = vehicleData?.VehicleId,
                    NoofVehicle = vehicleData?.StayWith,
                    ConnectedNumber = vehicleData?.ConnectedNumber,
                    ConnectedVehicleNumber = vehicleData?.ConnectedVehicleNumber,
                    ActualCheckIn = CheckInDate,
                    ActualCheckOut = CheckOutDate,
                    CompanyNames = member.CompanyVisas?.CompanyName ?? "N/A",
                    HotelName = HotelName,
                    NextHotelName = NextHotelName != null ? NextHotelName : "N/A",


                    ArrivalFlightNumber = _dbContext.POFlightDetails
                        .Include(p => p.POTicket)
                        .Include(p => p.POTicketFlightData)
                        .Where(pt => pt.POTicket.FamilyMembersId == member.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Arrival)
                        .Select(pt => pt.POTicketFlightData.FlightNumber)
                        .FirstOrDefault(),

                    FlightArrival = _dbContext.POFlightDetails
                            .Include(e => e.POTicket)
                            .Where(e => e.POTicket.FamilyMembersId == member.FamilyHeadId && !e.POTicket.Trashed && e.AirportsType == Models.AirportsType.Arrival)
                            .Select(e => e.FlightArrivalTime)
                            .FirstOrDefault(),

                    ArrivalDate = _dbContext.POFlightDetails
                            .Include(e => e.POTicket)
                            .Where(e => e.POTicket.FamilyMembersId == member.FamilyHeadId && !e.POTicket.Trashed && e.AirportsType == Models.AirportsType.Arrival)
                            .Select(e => e.ArrivalDate)
                            .FirstOrDefault(),

                    FlightDeparture = _dbContext.POFlightDetails
                            .Include(e => e.POTicket)
                            .Where(e => e.POTicket.FamilyMembersId == member.FamilyHeadId && !e.POTicket.Trashed && e.AirportsType == Models.AirportsType.Departure)
                            .Select(e => e.FlightDepartureTime)
                            .FirstOrDefault(),

                    DepartureDate = _dbContext.POFlightDetails
                            .Include(e => e.POTicket)
                            .Where(e => e.POTicket.FamilyMembersId == member.FamilyHeadId && !e.POTicket.Trashed && e.AirportsType == Models.AirportsType.Departure)
                            .Select(e => e.DepartureDate)
                            .FirstOrDefault(),

                    TrainNumber = _dbContext.PNRTrainMembers
                            .Include(e => e.PNRTrain)
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && !e.PNRTrain.Trashed)
                            .Select(e => e.PNRTrain.TrainNumber)
                            .FirstOrDefault(),

                    TrainArrival = _dbContext.PNRTrainMembers
                            .Include(e => e.PNRTrain)
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && !e.PNRTrain.Trashed)
                            .Select(e => e.PNRTrain.Arrival)
                            .FirstOrDefault(),

                    TrainDeparture = _dbContext.PNRTrainMembers
                            .Include(e => e.PNRTrain)
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && !e.PNRTrain.Trashed)
                            .Select(e => e.PNRTrain.Departure)
                            .FirstOrDefault(),
                });
            }

            // Summary
            model.MemberSummary.MaleCount = result.Count(e => e.Gender == Gender.Male);
            model.MemberSummary.AdultMales = result.Count(e => e.Gender == Gender.Male && e.Age >= 12);
            model.MemberSummary.ChildMales = result.Count(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2);
            model.MemberSummary.InfantMales = result.Count(e => e.Gender == Gender.Male && e.Age < 2);

            model.MemberSummary.FemaleCount = result.Count(e => e.Gender == Gender.Female);
            model.MemberSummary.AdultFemales = result.Count(e => e.Gender == Gender.Female && e.Age >= 12);
            model.MemberSummary.ChildFemales = result.Count(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2);
            model.MemberSummary.InfantFemales = result.Count(e => e.Gender == Gender.Female && e.Age < 2);

            model.MemberSummary.TotalMembers = model.MemberSummary.MaleCount + model.MemberSummary.FemaleCount;

            model.Company = group.CompanyName;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(int id, RoomSaveModel request)
        {
            // Step 1: Get all existing records for this ItineraryBlockId
            var existingRecords = _dbContext.TranspportationBlockReservations
                .Where(x => x.TransportationBlocksId == id)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var familyHeadId = request.FamilyHeadIds[i];
                var vehicleId = request.VehicleNumbers[i];
                var StayWith = request.NoofVehicle[i];
                //var FlightArrival = request.FlightArrival[i];
                //var ArrivalDate = request.ArrivalDate[i];
                //var FlightDeparture = request.FlightDeparture[i];
                //var DepartureDate = request.DepartureDate[i];
                //var TrainArrival = request.TrainArrival[i];
                //var TrainDeparture = request.TrainDeparture[i];
                var ConnectedNumbers = request.ConnectedNumber[i];
                var ArrivalFlightNumber = request.ArrivalFlightNumber[i];
                /* var ActualCheckIn = request.ActualCheckIn[i];
                 var ActualCheckOut = request.ActualCheckOut[i];*/
                var FlightArrival = request.FlightArrival != null && request.FlightArrival.Count > i ? request.FlightArrival[i] : null;
                var ArrivalDate = request.ArrivalDate != null && request.ArrivalDate.Count > i ? request.ArrivalDate[i] : null;
                var FlightDeparture = request.FlightDeparture != null && request.FlightDeparture.Count > i ? request.FlightDeparture[i] : null;
                var DepartureDate = request.DepartureDate != null && request.DepartureDate.Count > i ? request.DepartureDate[i] : null;
                var TrainArrival = request.TrainArrival != null && request.TrainArrival.Count > i ? request.TrainArrival[i] : null;
                var TrainDeparture = request.TrainDeparture != null && request.TrainDeparture.Count > i ? request.TrainDeparture[i] : null;
                var ActualCheckIn = request.ActualCheckIn != null && request.ActualCheckIn.Count > i ? request.ActualCheckIn[i] : null;
                var ActualCheckOut = request.ActualCheckOut != null && request.ActualCheckOut.Count > i ? request.ActualCheckOut[i] : null;


                /*string ConnectedNumber = (request.ConnectedNumber != null && i < request.ConnectedNumber.Count && request.ConnectedNumber[i] != null)
                    ? request.ConnectedNumber[i].ToString()
                    : null;

                string ConnectedVehicleNumber = (request.ConnectedVehicleNumber != null && i < request.ConnectedVehicleNumber.Count && request.ConnectedVehicleNumber[i] != null)
                    ? request.ConnectedVehicleNumber[i].ToString()
                    : null;*/

                var existing = existingRecords.FirstOrDefault(x => x.FamilyHeadId == familyHeadId);

                if (!vehicleId.HasValue)
                {
                    // If no room selected, and record exists, then delete it
                    if (existing != null)
                        _dbContext.TranspportationBlockReservations.Remove(existing);

                    continue;
                }

                if (existing != null)
                {
                    // Update existing record
                    existing.FamilyHeadId = familyHeadId;
                    existing.VehicleId = vehicleId.Value;
                    existing.StayWith = StayWith;
                    existing.ConnectedNumber = ConnectedNumbers;
                    existing.FlightArrival = FlightArrival;
                    existing.ArrivalDate = ArrivalDate;
                    existing.FlightDeparture = FlightDeparture;
                    existing.DepartureDate = DepartureDate;
                    existing.TrainArrival = TrainArrival;
                    existing.TrainDeparture = TrainDeparture;
                    existing.ArrivalFlightNumber = ArrivalFlightNumber;
                    existing.ActualCheckIn = ActualCheckIn;
                    existing.ActualCheckOut = ActualCheckOut;
                }
                else
                {
                    // Insert new record
                    _dbContext.TranspportationBlockReservations.Add(new TranspportationBlockReservation
                    {
                        FamilyHeadId = familyHeadId,
                        VehicleId = vehicleId.Value,
                        StayWith = StayWith,
                        FlightArrival = FlightArrival,
                        ArrivalDate = ArrivalDate,
                        FlightDeparture = FlightDeparture,
                        DepartureDate = DepartureDate,
                        TrainArrival = TrainArrival,
                        TrainDeparture = TrainDeparture,
                        ConnectedNumber = ConnectedNumbers,
                        ArrivalFlightNumber = ArrivalFlightNumber,
                        ActualCheckIn = ActualCheckIn,
                        ActualCheckOut = ActualCheckOut,
                        TransportationBlocksId = id
                    });
                }
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult SaveSingleTransportation(SingleTransportationModel model)
        {
            if (model == null || !model.FamilyHeadId.HasValue || !model.VehicleId.HasValue)
                return Json(new { success = false, message = "Invalid data" });

            var existing = _dbContext.TranspportationBlockReservations
                .FirstOrDefault(x => x.FamilyHeadId == model.FamilyHeadId.Value);

            if (existing != null)
            {
                // Update
                existing.VehicleId = model.VehicleId.Value;
                existing.StayWith = model.NoOfVehicle;
                existing.ConnectedNumber = model.ConnectedNumber;
                existing.ArrivalFlightNumber = model.ArrivalFlightNumber;
                existing.FlightArrival = model.FlightArrival;
                existing.ArrivalDate = model.ArrivalDate;
                existing.FlightDeparture = model.FlightDeparture;
                existing.DepartureDate = model.DepartureDate;
                existing.TrainArrival = model.TrainArrival;
                existing.TrainDeparture = model.TrainDeparture;
                existing.ActualCheckIn = model.ActualCheckIn;
                existing.ActualCheckOut = model.ActualCheckOut;
            }
            else
            {
                // Add
                _dbContext.TranspportationBlockReservations.Add(new TranspportationBlockReservation
                {
                    FamilyHeadId = model.FamilyHeadId.Value,
                    VehicleId = model.VehicleId.Value,
                    StayWith = model.NoOfVehicle,
                    ConnectedNumber = model.ConnectedNumber,
                    ArrivalFlightNumber = model.ArrivalFlightNumber,
                    FlightArrival = model.FlightArrival,
                    ArrivalDate = model.ArrivalDate,
                    FlightDeparture = model.FlightDeparture,
                    DepartureDate = model.DepartureDate,
                    TrainArrival = model.TrainArrival,
                    TrainDeparture = model.TrainDeparture,
                    ActualCheckIn = model.ActualCheckIn,
                    ActualCheckOut = model.ActualCheckOut,
                    TransportationBlocksId = model.BlockId // optional if needed
                });
            }

            _dbContext.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult SaveVehicleInfo(int id, VehicleInfoModel model)
        {
            if (model == null || model.FamilyHeadId <= 0)
            {
                return Json(new { success = false, message = "Invalid data received." });
            }

            try
            {
                var existingRecords = _dbContext.TranspportationBlockReservations
                    .Where(x => x.TransportationBlocksId == id)
                    .ToList();

                var existing = existingRecords.FirstOrDefault(x => x.FamilyHeadId == model.FamilyHeadId);

                if (existing != null)
                {
                    existing.StayWith = model.NoOfVehicle;
                    existing.ConnectedNumber = model.ConnectedNumber;

                    _dbContext.SaveChanges();

                    return Json(new { success = true });
                }

                // ✅ Specific message if no match found
                return Json(new { success = false, message = "No matching record found for the given FamilyHeadId." });
            }
            catch (Exception ex)
            {
                // Optional: log the error here
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



        public IActionResult Report(int id, int Groupid, int PlaceId)
        {
            Package package = _dbContext.Packages.Find(id);

            GroupReportModel model = new GroupReportModel
            {
                PackageId = id,
                FamilyMembers = new List<FamilyMemberModel>(),
                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                //Quota = package.Quota
            };

            //var members = _context.FamilyMembers
            //    .Include(e => e.Head)
            //    .Include(e => e.FamilyHead)
            //    .ThenInclude(e => e.FamilyHeadRelation)
            //    .Include(e => e.FamilyHead)
            //    .ThenInclude(e => e.Package)
            //    .ThenInclude(e => e.Company)
            //    .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
            //    .OrderBy(e => e.HeadId)
            //    .ToList();

            var members = _dbContext.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Join(
                    _dbContext.ItineraryBlockGroupMmebers,
                    f => f.FamilyHeadId,
                    gm => gm.FamilyHeadId,
                    (f, gm) => new { FamilyMember = f, ItineraryBlockGroupMmebers = gm }
                )
                .Where(joinResult => joinResult.FamilyMember.FamilyHead.Trashed == false
                                  && joinResult.ItineraryBlockGroupMmebers.ItineraryBlockId == Groupid)
                .OrderBy(joinResult => joinResult.FamilyMember.HeadId)
                .Select(joinResult => joinResult.FamilyMember)
                .ToList();

            var familiesGroups = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .GroupBy(e => e.HeadId)
                .ToList();

            var maleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Male).Count();
            var femaleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Female).Count();

            model.FamilyMembers = new List<FamilyMemberModel>();

            //return Ok(familiesGroups);
            bool sameFamily = true;
            FamilyMembers prevMember = null;



            for (int i = 0, x = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                string memberName = currentMember.FamilyHead.GivenName + " " + currentMember.FamilyHead.SurName;
                string mehramName = "-";
                string mehramRel = "-";
                string fatherHusbandName = currentMember.FamilyHead.FatherHusbandName;

                if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Father)
                {
                    fatherHusbandName += " <b>(F)</b>";
                }
                else if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Husband)
                {
                    fatherHusbandName += " <b>(H)</b>";
                }

                if (currentMember.IsHead)
                {
                    memberName += " <b>(FH)</b>";
                }
                else
                {
                    if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        mehramName = currentMember.Head.GivenName;
                        mehramRel = currentMember.FamilyHead?.FamilyHeadRelation?.Relation ?? "-";
                    }
                }

                if (i > 0)
                {
                    prevMember = members.ElementAt(i - 1);
                    if (prevMember.HeadId != currentMember.HeadId)
                    {
                        sameFamily = false;
                    }
                }

                if (!sameFamily)
                {
                    x++;
                    sameFamily = true;
                }

                // Ensure the index x is within the range of familiesGroups
                int familyPax = (x < familiesGroups.Count) ? familiesGroups.ElementAt(x).Count() : 0;

                var memberInGroup = _dbContext.ItineraryBlockGroupMmebers
                    .Include(e => e.ItineraryBlock)
                    .ThenInclude(e => e.POHotels)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                string cId =
                    $"{currentMember.FamilyHead.Package.Company.CompanyCode}.{currentMember.FamilyHead.IndId}" +
                    $".{currentMember.Head.ExtractedFamilyNumber}({familyPax})" +
                    $".{currentMember.FamilyHead.Package.Name}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $"{memberInGroup.ItineraryBlockId}";
                    groupNumber = memberInGroup?.ItineraryBlock?.POHotels.Code ?? "-";
                    //model.PackageName = memberInGroup.ItineraryBlock.Package.Name;
                    model.PackageCode = memberInGroup.ItineraryBlock.POHotels.Code;
                }

                string megaId = currentMember.FamilyHead.ReferenceId == 0 ? "-" : currentMember.FamilyHead.ReferenceId?.ToString();

                // Main Contact & Address
                var memberContact = _dbContext.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _dbContext.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                string roomName = "-";

                if (PlaceId == 1)
                {
                    var reservation = _dbContext.ItineraryBlockReservationForMakkah
                        .Include(e => e.RoomDetails)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                    roomName = reservation?.RoomDetails?.Room?.RoomName ?? "-";
                }
                else if (PlaceId == 2)
                {
                    var reservation = _dbContext.ItineraryBlockReservationForMadinah
                        .Include(e => e.RoomDetails)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                    roomName = reservation?.RoomDetails?.Room?.RoomName ?? "-";
                }
                else if (PlaceId == 4)
                {
                    var reservation = _dbContext.ItineraryBlockReservationForAzizias
                        .Include(e => e.FloorRoom)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                    roomName = reservation?.FloorRoom?.Room?.RoomName ?? "-";
                }

                int? accountId = currentMember.Head.AccountId;

                PaymentVoucher voucher = _dbContext.PaymentVouchersWithoutTrash
                    .FirstOrDefault(e => e.AccountId == accountId);

                float receivableUsd = (voucher != null && voucher.USDROE != 0) ? voucher.Amount / voucher.USDROE : 0;

                var invoices = _dbContext.InvoicesWithoutTrash
                    .Where(e => e.FamilyHeadId == currentMember.HeadId)
                    .Select(e => new { e.InvoiceId, e.FamilyHeadId });

                string invoicesString = string.Join(", ", invoices.Select(e => "R#" + e.InvoiceId));

                if (!string.IsNullOrEmpty(groupNumber)/* && memberInGroup.GroupId == Groupid*/)
                {
                    model.FamilyMembers.Add(
                        new FamilyMemberModel
                        {
                            FamilyHeadId = currentMember.FamilyHeadId,
                            MegaId = megaId,
                            EHajjId = currentMember.FamilyHead.EHajjId,
                            MOFA = currentMember.FamilyHead.MofaNumber,
                            FID = currentMember.Head.FcId,
                            CID = cId,
                            GNumber = groupNumber,
                            FormNumber = currentMember.FamilyHead.FormNumber,
                            Image = currentMember.FamilyHead.MemberImage,
                            PassportImage = currentMember.FamilyHead.PassportImage,
                            Name = memberName,
                            FatherHusbandName = fatherHusbandName,
                            Mehram = mehramName,
                            MehramRelation = mehramRel,
                            MobileNumber = memberContact?.Contact,
                            Address = memberAddress?.Address,
                            Age = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth),
                            Gender = currentMember.FamilyHead.Gender.ToString(),
                            HajjSerialNumber = currentMember.FamilyHead.HajjAppSerialNumber,
                            VisaNumber = currentMember?.FamilyHead.VisaNumber,
                            TicketNumber = currentMember?.FamilyHead.TicketNumber,
                            PassportNumber = currentMember.FamilyHead.PassportNumber,
                            HajjAppNumber = currentMember.FamilyHead.HajjApplicationNumber,
                            FileNumber = currentMember.FamilyHead.FileNumber,
                            Makkah = roomName,
                            //Medinah = medinahAccommodation?.Room?.RoomCode,
                            //Azizia = aziziaAccommodation?.Room?.RoomCode,
                            //Mina = minaAccommodation?.Room?.RoomCode,
                            FriendId = currentMember.FamilyHead?.FriendId?.ToString() ?? "-",
                            TotalAmount = "$" + receivableUsd.ToString("N0"),
                            Invoices = invoicesString
                        }
                    );
                }
            }

            int familyCount = 0;
            for (int i = 0; i < familiesGroups.Count; i++)
            {
                familyCount++;
            }

            model.PackageSummary = new GroupSummaryModel
            {
                MembersCount = members.Count,
                FamiliesCount = familyCount,
                MalesCount = maleMembersCount,
                FemalesCount = femaleMembersCount
            };

            for (int i = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                int memberAge = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth);
                if (memberAge < 2)
                {
                    model.PackageSummary.InfantsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.InfantMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.InfantFemales++;
                    }
                }
                else if (memberAge < 12)
                {
                    model.PackageSummary.ChildsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.ChildMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.ChildFemales++;
                    }
                }
                else if (memberAge >= 12)
                {
                    model.PackageSummary.AdultsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.AdultMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.AdultFemales++;
                    }
                }
            }



            return View(model);
        }

        public IActionResult ViewTrash()
        {
            var itinerary = _dbContext.TransportationBlocks
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Include(e => e.Package)
                .Include(e => e.Route)
                .Where(e => !e.Trashed);

            return View(itinerary);
        }

        public IActionResult Trash(int id)
        {
            TransportationBlocks fiqah = _dbContext.TransportationBlocks.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            TransportationBlocks fiqah = _dbContext.TransportationBlocks.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = false;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}

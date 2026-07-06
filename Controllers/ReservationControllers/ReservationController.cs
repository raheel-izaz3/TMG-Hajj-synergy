using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.PackageModels;
using TMG.Models.ReservationModels;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using static System.Net.WebRequestMethods;

namespace TMG.Models.ReservationModels
{
    //public record ReservationMemberEntryModel
    //{
    //    public int? FcId { get; set; }
    //    public string GivenName { get; set; }
    //}

    public record ReservationAllocateModel
    {
        public string HotelNumber { get; set; }
        public string HotelName { get; set; }
        public string HotelCode { get; set; }
        public string Place { get; set; }
        public string CompanyName { get; set; }
        public string RSVNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public List<RoomDetailEntryModel> RoomDetails { get; set; } = new List<RoomDetailEntryModel>();
        public List<ReservationAllocateSubMemberEntryModel> SubMembers { get; set; } = new List<ReservationAllocateSubMemberEntryModel>();

        public ReservationRoomAllocationModel ReservationRoomAllocationModel { get; set; } = new ReservationRoomAllocationModel();
    }

    public record RoomDetailEntryModel
    {
        public int Sno { get; set; }
        public string RoomType { get; set; }
        public string RoomCode { get; set; }
        public string RoomNumber { get; set; }
        public int ReservationRoomsId { get; set; }
    }

    public record ReservationRoomAllocationModel
    {
        public List<int> FamilyHeadIds {  get; set; } 
        public List<int?> RoomReservationIds { get; set; } = new List<int?>();
    }

    public record ReservationAllocateSubMemberEntryModel
    {
        public int FamilyHeadId { get; set; }
        public int Sno { get; set; }
        public string CID { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
        public string FNumber { get; set; }
        public string RoomType { get; set; }
    }

    public record ReservationRegisterRequest
    {
        public List<Hotel> Hotels { get; set; } = new List<Hotel>();
        public List<Company> Companies { get; set; } = new List<Company>();
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Package> Packages { get; set; } = new List<Package>();


        public int? RoomAllocationId { get; set; }

        [Required, Display(Name = "Hotel")]
        public int? HotelId { get; set; }

        [Required, Display(Name = "Sub Group")]
        public int SubGroupId { get; set; }

        [Required, Display(Name = "Check In Date")]
        public DateTime CheckInDate { get; set; }

        [Required, Display(Name = "Check Out Date")]
        public DateTime CheckOutDate { get; set; }

        public List<int?> Quantity { get; set; } 
        public List<int?> RoomId { get; set; } 
    }

    public record ReservationIndexModel
    {
        public List<ReservationIndexEntry> Entries { get; set; } = new List<ReservationIndexEntry>();
    }

    public record ReservationIndexEntry
    {
        public Reservation Reservation { get; set; }
        //public int PAX { get; set; }
        public int Rooms { get; set; }
    }


    public record ReservationViewDetailsModel
    {
        public string HotelNumber { get; set; }
        public string HotelName { get; set; }
        public string HotelCode { get; set; }
        public string Place { get; set; }
        public string CompanyName { get; set; }
        public string RSVNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public List<Floor> Floors { get; set; }

        public List<RoomDetailEntryModel> RoomDetails { get; set; } = new List<RoomDetailEntryModel>();
        public List<ReservationViewDetailMember> SubMembers { get; set; } = new List<ReservationViewDetailMember>();

    }

    public record ReservationViewDetailMember
    {
        public int SNo { get; set; }
        public string Name { get; set; }
        public string RoomCode { get; set; }
        public string SingleRoomCode { get; set; }
    }

}

namespace TMG.Controllers.ReservationControllers
{
    public class ReservationController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public ReservationController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext
                .Reservations
                .Where(e => e.Trashed)
                .Count();

            ReservationIndexModel model = new ReservationIndexModel
            {
                Entries = new List<ReservationIndexEntry>() // ✅ Initialize Entries
            };

            var reservations = _dbContext.Reservations
                .Include(e => e.Hotel)
                .Include(e => e.SubGroup)
                .ThenInclude(e => e.Group)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .ToList();

            var rooms = _dbContext.ReservationRooms
                .Include(e => e.Room)
                .ToList();

            foreach (var currRes in reservations)
            {
                ReservationIndexEntry entry = new ReservationIndexEntry
                {
                    Reservation = currRes,
                    Rooms = rooms.Count(e => e.ReservationId == currRes.ReservationId)
                };

                model.Entries.Add(entry); // ✅ No more null reference error
            }

            return View(model);
        }


        public IActionResult Create()
        {
            ReservationRegisterRequest model = new ReservationRegisterRequest();

            model.Hotels = _dbContext.HotelsWithoutTrash.ToList();
            model.Companies = _dbContext.CompaniesWithoutTrash.ToList();
            model.Rooms = _dbContext.RoomsWithoutTrash.ToList();
            model.Packages = _dbContext.PackagesWithoutTrash.Where(e => e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            ViewBag.Model = model;

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(ReservationRegisterRequest request)
        {
            //return Ok(request);

            if (request.Quantity.Count != request.RoomId.Count)
                ModelState.AddModelError("", "");

            if (!ModelState.IsValid) return View(request);

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                Reservation reservation = new Reservation
                {
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    SubGroupId = request.SubGroupId,
                    HotelId = request.HotelId
                };
                _dbContext.Reservations.Add(reservation);
                _dbContext.SaveChanges();

                for (int i = 0; i < request.RoomId.Count; i++)
                {
                    int quantity = (int)request.Quantity.ElementAt(i);
                    for (int j = 0; j < quantity; j++)
                    {
                        _dbContext.ReservationRooms.Add(
                        new ReservationRooms
                        {
                            ReservationId = reservation.ReservationId,
                            RoomId = (int)request.RoomId.ElementAt(i),
                        }
                        );
                    }
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


        public IActionResult Edit(int id)
        {
            RoomAllocation ra = _dbContext.RoomAllocations
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .Include(e => e.ConnectedWithFloorRoom)
                .ThenInclude(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .FirstOrDefault(e => e.RoomAllocationId == id);

            var members = _dbContext.RoomAllocationMembers
                .Where(e => e.RoomAllocationId == id)
                .ToList();

            ReservationRegisterRequest request = new ReservationRegisterRequest
            {
                RoomAllocationId = id,
                Hotels = _dbContext.HotelsWithoutTrash.ToList(),
                CheckInDate = ra.CheckInDate,
                CheckOutDate = ra.CheckOutDate,
                HotelId = ra.FloorRoom.Floor.HotelId,
            };

            //return Ok(request);

            return View(request);
        }

        [HttpPost]
        public IActionResult Edit(ReservationRegisterRequest request)
        {
            return View(request);
        }

        public IActionResult AllocateRoom(int id)
        {
            Reservation reservation = _dbContext
                .Reservations
                .Include(e => e.Hotel)
                .ThenInclude(e => e.Place)
                .Include(e => e.SubGroup)
                .ThenInclude(e => e.Group)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .FirstOrDefault(e => e.ReservationId == id);

            ReservationAllocateModel model = new ReservationAllocateModel();
            model.HotelNumber = reservation.Hotel.HotelNumber.ToString();
            model.HotelCode = reservation.Hotel.HotelCode;
            model.HotelName = reservation.Hotel.HotelName;
            model.Place = reservation.Hotel.Place.PlaceName;
            model.RSVNumber = "RSV-" + reservation.ReservationId.ToString();
            model.CompanyName = reservation.SubGroup.Group.Package.Company.CompanyName;
            model.CheckIn = reservation.CheckInDate;
            model.CheckOut = reservation.CheckOutDate;

            var reservationRooms = _dbContext.ReservationRooms
                .Include(e => e.Room)
                .Where(e => e.ReservationId == id)
                .ToList();

            for (int i = 0; i < reservationRooms.Count; i++)
            {
                var currRoom = reservationRooms.ElementAt(i);
                string roomCode = $"{model.RSVNumber}.{reservation.Hotel.Place.PlaceName}" +
                    $".{reservation.SubGroup.Code}" +
                    $".{reservation.Hotel.Place.PlaceId}.{reservation.HotelId}" +
                    $".{currRoom.Room.RoomCode}" +
                    $".{currRoom.ReservationRoomsId}";

                model.RoomDetails.Add(new RoomDetailEntryModel
                {
                    Sno = i + 1,
                    RoomNumber = "",
                    RoomType = $"{currRoom.Room.RoomName} ({currRoom.Room.RoomCode})",
                    RoomCode = roomCode,
                    ReservationRoomsId = currRoom.ReservationRoomsId,
                });
            }

            var subGroupMembers = _dbContext.SubGroupMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .Include(e => e.FamilyMembers)
                .Where(e => e.SubGroupId == reservation.SubGroupId)
                .OrderBy(e => e.FamilyMembers.HeadId)
                .ToList();
            var familyMembers = _dbContext.FamilyMembers;
            for (int i = 0; i < subGroupMembers.Count; i++)
            {
                var currSubMember = subGroupMembers.ElementAt(i);
                var fm = familyMembers.FirstOrDefault(e => e.FamilyHeadId == currSubMember.FamilyHeadId);
                var famCount = familyMembers.Where(e => e.HeadId == fm.HeadId).Count();

                string cId =
                        $"{currSubMember.FamilyHead.Package.Company.CompanyCode}.{currSubMember.FamilyHead.IndId}" +
                        $"{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                        $".{currSubMember.FamilyHead.Package.Name}.{currSubMember.SubGroup.Group.Name}" +
                        $".{currSubMember.SubGroup.GroupId}";

                model.SubMembers.Add(new ReservationAllocateSubMemberEntryModel
                {
                    Sno = i + 1,
                    Name = currSubMember.FamilyHead.GivenName + (currSubMember.FamilyHead.IsHead ? " <b>(FH)</b>" : ""),
                    PassportNumber = currSubMember.FamilyHead.PassportNumber,
                    Gender = currSubMember.FamilyHead.Gender.ToString(),
                    CID = cId,
                    FamilyHeadId = currSubMember.FamilyHeadId,
                });
            }


            return View(model);
        }


        [HttpPost]
        public IActionResult AllocateRoom(ReservationRoomAllocationModel model)
        {
            //return Ok(model);

            if (model.RoomReservationIds.Count != model.FamilyHeadIds.Count) return View();

            for (int i = 0; i < model.RoomReservationIds.Count; i++)
            {
                _dbContext.ReservationFamilyMembers.Add(new ReservationFamilyMembers
                {
                    FamilyHeadId = model.FamilyHeadIds.ElementAt(i),
                    ReservationRoomsId = (int)model.RoomReservationIds.ElementAt(i),
                });
            }
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ViewDetails(int id)
        {
            Reservation reservation = _dbContext
                .Reservations
                .Include(e => e.Hotel)
                .ThenInclude(e => e.Place)
                .Include(e => e.SubGroup)
                .ThenInclude(e => e.Group)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .FirstOrDefault(e => e.ReservationId == id);

            ReservationViewDetailsModel model = new ReservationViewDetailsModel();
            model.Floors = _dbContext.Floors.Where(e => !e.Trashed && e.HotelId == reservation.HotelId).ToList();
            model.HotelNumber = reservation.Hotel.HotelNumber.ToString();
            model.HotelCode = reservation.Hotel.HotelCode;
            model.HotelName = reservation.Hotel.HotelName;
            model.Place = reservation.Hotel.Place.PlaceName;
            model.RSVNumber = "RSV-" + reservation.ReservationId.ToString();
            model.CompanyName = reservation.SubGroup.Group.Package.Company.CompanyName;
            model.CheckIn = reservation.CheckInDate;
            model.CheckOut = reservation.CheckOutDate;

            var reservationRooms = _dbContext.ReservationRooms
               .Include(e => e.Room)
               .Where(e => e.ReservationId == id)
               .ToList();

            for (int i = 0; i < reservationRooms.Count; i++)
            {
                var currRoom = reservationRooms.ElementAt(i);
                string roomCode = $"{model.RSVNumber}.{reservation.Hotel.Place.PlaceName}" +
                    $".{reservation.SubGroup.Code}" +
                    $".{reservation.Hotel.Place.PlaceId}.{reservation.HotelId}" +
                    $".{currRoom.Room.RoomCode}" +
                    $".{currRoom.ReservationRoomsId}";

                model.RoomDetails.Add(new RoomDetailEntryModel
                {
                    Sno = i + 1,
                    RoomNumber = "",
                    RoomType = $"{currRoom.Room.RoomName} ({currRoom.Room.RoomCode})",
                    RoomCode = roomCode,
                    ReservationRoomsId = currRoom.ReservationRoomsId,
                });
            }

            //var reservationMembers = _dbContext.ReservationFamilyMembers
            //    .Include(e => e.FamilyHead)
            //    .Include(e => e.ReservationRooms)
            //    .ThenInclude(e => e.Room)
            //    .GroupBy(e => e.ReservationRoomsId)
            //    .ToList();

            //var familyMembers = _dbContext.FamilyMembers;

            //for (int i = 0; i < reservationMembers.Count; i++)
            //{
            //    string names = "";
            //    string roomCodes = "";
            //    string roomcode = "";
            //    for (int j = 0; j < reservationMembers.ElementAt(i).Count(); j++)
            //    {
            //        var currMember = reservationMembers.ElementAt(i).ElementAt(j);

            //        names += currMember.FamilyHead.GivenName
            //            + " " + currMember.FamilyHead.SurName +
            //            (currMember.FamilyHead.IsHead ? " <b>(FH)</b>" : "") + "<br>";

            //        roomCodes += $"{model.RSVNumber}.{reservation.Hotel.Place.PlaceName}" +
            //        $".{reservation.SubGroup.Code}" +
            //        $".{reservation.Hotel.Place.PlaceId}.{reservation.HotelId}" +
            //        $".{currMember.ReservationRooms.Room.RoomCode}" +
            //        $".{currMember.ReservationRoomsId}<b>,</b> ";

            //        roomcode = $"{model.RSVNumber}.{reservation.Hotel.Place.PlaceName}" +
            //        $".{reservation.SubGroup.Code}" +
            //        $".{reservation.Hotel.Place.PlaceId}.{reservation.HotelId}" +
            //        $".{currMember.ReservationRooms.Room.RoomCode}" +
            //        $".{currMember.ReservationRoomsId}";
            //    }

            //    model.SubMembers.Add(new ReservationViewDetailMember
            //    {
            //        SNo = i + 1,
            //        Name = names,
            //        RoomCode = roomCodes,
            //        SingleRoomCode = roomcode
            //    });
            //}

            return View(model);
        }

        [HttpPost]
        public IActionResult ViewDetails(List<int> RoomIds, List<int> ConnectedWith)
        {

            for(int i = 0; i < RoomIds.Count; i++)
            {
                _dbContext.ReservationRooms.Find();
            }

            return RedirectToAction("Index");
        }

        public IActionResult HotelpackagesReport(int id)
        {
            var query = (from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join spo in _dbContext.SPOs on fh.SPOId equals spo.SPOId
                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                         join h in _dbContext.Hotels on pa.AccomodationId equals h.AccomodationId
                         join af in _dbContext.AdditionalFacilities on p.PackageId equals af.PackageId
                         join r in _dbContext.Rooms on af.RoomId equals r.RoomId
                         join fhad in _dbContext.FamilyHeadAdditionalFacilities on af.AdditionalFacilitiesId equals fhad.AdditionalFacilitiesId
                         where !fh.Trashed && !p.Trashed && !c.Trashed && !spo.Trashed &&  !h.Trashed  && !r.Trashed
                         group new { c, spo, p, pa, h, r } by new
                         {
                             CompanyName = c.CompanyName,
                             SPOName = spo.Name,
                             PackageName = p.Name,
                             PackageId = p.PackageId,
                             CheckIn = pa.CheckIn,
                             CheckOut = pa.CheckOut,
                             Nights = pa.Nights,
                             HotelName = h.HotelName,
                             RoomName = r.RoomName,
                             RoomPAX = r.PAX
                         } into grouped
                         select new
                         {
                             CompanyName = grouped.Key.CompanyName,
                             SPOName = grouped.Key.SPOName,
                             PackageName = grouped.Key.PackageName,
                             PackageId = grouped.Key.PackageId,
                             CheckIn = grouped.Key.CheckIn,
                             CheckOut = grouped.Key.CheckOut,
                             Nights = grouped.Key.Nights,
                             pax = grouped.Count(),
                             HotelName = grouped.Key.HotelName,
                             RoomName = grouped.Key.RoomName,
                             roomPax = grouped.Key.RoomPAX
                         });

            var result = query.ToList();


            List<HotelRoomSummaryReportModel> Hotelroomlist = new List<HotelRoomSummaryReportModel>();
            foreach (var item in result)
            {
                var hotelroom = new HotelRoomSummaryReportModel();
                hotelroom.CompanyName = item.CompanyName;
                hotelroom.SPO = item.SPOName;
                hotelroom.PackageName = item.PackageName;
                hotelroom.CheckIn = item.CheckIn;
                hotelroom.CheckOut = item.CheckOut;
                hotelroom.Nights = item.Nights;
                hotelroom.Pax = item.pax;
                //hotelroom.SGL = item.RoomName == "Single" ? "" : "";
            }
           








            return View();
        }

        public class HotelRoomSummaryReportModel
        {
            public string? CompanyName { get; set; }
            public string? SPO { get; set; }
            public string? PackageName { get; set; }
            public string? CheckIn { get; set; }
            public string? CheckOut { get; set; }
            public string? Nights { get; set; }
            public int? Pax { get; set; }
            public int? SGL { get; set; }
            public int? DBL { get; set; }
            public int? TRP { get; set; }
            public int? QD { get; set; }
            public int? SHR { get; set; }
            public int? TRMS { get; set; }
            public int? CH_INF { get; set; }
            public int? SH_M { get; set; }
            public int? SH_F { get; set; }

        }
    }
}

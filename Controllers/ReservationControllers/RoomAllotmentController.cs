using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TMG.Controllers.ReservationControllers
{
    public class RoomAllotmentController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;


        public RoomAllotmentController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RoomAllotments.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashRoomAllotment(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateRoomAllotment(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditRoomAllotment(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreRoomAllotment(_dbContext, _http);

            return View(_dbContext.RoomAllotments
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Room)
                .Where(e => !e.Trashed && !e.FloorRoom.Trashed)
                );
        }

        public IActionResult Create()
        {
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;

            return View();
        }

        [HttpPost]
        public IActionResult Create(RoomAllotment request)
        {
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            var rooms = _dbContext.RoomAllotments
                .Where(e => e.FloorRoomId == request.FloorRoomId)
                .ToList();

            bool duplicate = false;
            for (int i = 0; i < rooms.Count; i++)
            {
                var currRoom = rooms.ElementAt(i);
                //if (userEndDate < existingStartDate || userStartDate > existingEndDate)
                if (request.ValidTill < currRoom.ValidFrom || request.ValidFrom > currRoom.ValidTill)
                {

                }
                else
                {
                    duplicate = true;
                }
            }

            if (duplicate)
            {
                ModelState.AddModelError("ValidFrom", $"Room is already alloted within this range");
            }

            if (!ModelState.IsValid) return View(request);

            _dbContext.RoomAllotments.Add(request);
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {

            RoomAllotment ra = _dbContext
                .RoomAllotments
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Room)
                .FirstOrDefault(e => e.RoomAllotmentId == id);

            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.HotelId = ra.FloorRoom.Floor.HotelId;
            ViewBag.FloorId = ra.FloorRoom.FloorId;


            return View(ra);
        }


        [HttpPost]
        public IActionResult Edit(RoomAllotment request)
        {
            RoomAllotment ra = _dbContext
                .RoomAllotments
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Room)
                .FirstOrDefault(e => e.RoomAllotmentId == request.RoomAllotmentId);


            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.HotelId = ra.FloorRoom.Floor.HotelId;
            ViewBag.FloorId = ra.FloorRoom.FloorId;

            var rooms = _dbContext.RoomAllotments
                .Where(e => e.FloorRoomId == request.FloorRoomId && e.RoomAllotmentId != request.RoomAllotmentId)
                .ToList();

            bool duplicate = false;
            for (int i = 0; i < rooms.Count; i++)
            {
                var currRoom = rooms.ElementAt(i);
                //if (userEndDate < existingStartDate || userStartDate > existingEndDate)
                if (request.ValidTill < currRoom.ValidFrom || request.ValidFrom > currRoom.ValidTill)
                {

                }
                else
                {
                    duplicate = true;
                }
            }

            if (duplicate)
            {
                ModelState.AddModelError("ValidFrom", $"Room is already alloted within this range");
            }

            if (!ModelState.IsValid) return View(request);

            ra.UpdatedAt = DateTime.Now;
            ra.FloorRoomId = request.FloorRoomId;
            ra.ValidFrom = request.ValidFrom;
            ra.ValidTill = request.ValidTill;
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext
                .RoomAllotments
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Room)
                .Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            RoomAllotment room = _dbContext.RoomAllotments
                .Find(id);

            room.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Restore(int id)
        {
            RoomAllotment room = _dbContext.RoomAllotments
                 .Find(id);

            room.Trashed = false;
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult BookHotel(int id)
        {
            Hotel h = _dbContext.Hotels.Find(id);
            ViewBag.HotelName = h.HotelName;

            BulkAllotmentRequest req = new BulkAllotmentRequest
            {
                HotelId = id,
            };

            return View(req);
        }

        [HttpPost]
        public IActionResult BookHotel(BulkAllotmentRequest request)
        {
            Hotel h = _dbContext.Hotels.Find(request.HotelId);
            ViewBag.HotelName = h.HotelName;

            if (!ModelState.IsValid) return View(request);

            var rooms = _dbContext
                .FloorRoom
                .Include(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .Where(e => e.Floor.HotelId == request.HotelId)
                .ToList();

            for (int i = 0; i < rooms.Count; i++)
            {
                _dbContext.RoomAllotments.Add(
                    new RoomAllotment
                    {
                        FloorRoomId = rooms.ElementAt(i).FloorRoomId,
                        ValidFrom = request.ValidFrom,
                        ValidTill = request.ValidTill,
                    }
                    );
            }

            _dbContext.SaveChanges();

            return View(request);
        }

        public IActionResult BookFloor(int id)
        {
            Floor floor = _dbContext
                .Floors
                .Include(e => e.Hotel)
                .FirstOrDefault(e => e.FloorId == id);

            ViewBag.FloorNumber = floor.FloorNumber;
            ViewBag.HotelName = floor.Hotel.HotelName;

            BulkAllotmentRequest request = new BulkAllotmentRequest
            {
                FloorId = id,
            };

            return View(request);
        }

        [HttpPost]
        public IActionResult BookFloor(BulkAllotmentRequest request)
        {
            Floor floor = _dbContext
                .Floors
                .Include(e => e.Hotel)
                .FirstOrDefault(e => e.FloorId == request.FloorId);

            ViewBag.FloorNumber = floor.FloorNumber;
            ViewBag.HotelName = floor.Hotel.HotelName;

            if (!ModelState.IsValid) return View(request);

            var rooms = _dbContext
                .FloorRoom
                .Include(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .Where(e => e.FloorId == request.FloorId)
                .ToList();

            for (int i = 0; i < rooms.Count; i++)
            {
                _dbContext.RoomAllotments.Add(
                    new RoomAllotment
                    {
                        FloorRoomId = rooms.ElementAt(i).FloorRoomId,
                        ValidFrom = request.ValidFrom,
                        ValidTill = request.ValidTill,
                    }
                    );
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult GetValidDateRange(int id)
        {
            var room = _dbContext
                .RoomAllotments
                .FirstOrDefault(e => e.FloorRoomId == id);

            return Ok(new
            {
                ValidFrom = DateTimeManager.GetDate(room.ValidFrom),
                ValidTill = DateTimeManager.GetDate(room.ValidTill),
                ValidFromHtml = DateTimeManager.DateToHtmlFormat(room.ValidFrom),
                ValidTillHtml = DateTimeManager.DateToHtmlFormat(room.ValidTill),
            });
        }
    }
}

namespace TMG.Models.ReservationModels
{
    public record BulkAllotmentRequest
    {
        public int? HotelId { get; set; }
        public int? FloorId { get; set; }

        [Required, Display(Name = "Valid From"), DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; }

        [Required, Display(Name = "Valid Till"), DataType(DataType.Date)]
        public DateTime ValidTill { get; set; }
    }
}
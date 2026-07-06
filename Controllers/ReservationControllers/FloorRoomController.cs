using TMG.Data;
using TMG.Models.AccomodationModels;
using TMG.Models.MiscModels;
using TMG.Models.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace TMG.Models.ReservationModels
{
    public record FloorRoomIndexModel(FloorRoom FloorRoom, bool Allocated);

}

namespace TMG.Controllers.ReservationControllers
{
    public class FloorRoomController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;


        public FloorRoomController(TMGDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _http = contextAccessor;
        }

        public IActionResult Index(int id)
        {
            ViewBag.TrashCount = _dbContext.FloorRoom
                .Where(e => e.FloorId == id && e.Trashed).Count();

            var rooms = _dbContext.FloorRoom
                .Include(e => e.Room)
                .Include(e => e.RoomBedType)
                .Where(e => e.FloorId == id && !e.Trashed);

            Floor floor = _dbContext.Floors
                .Include(e => e.Hotel).FirstOrDefault(e => e.FloorId == id);

            ViewBag.FloorId = id;
            ViewBag.FloorNumber = floor.FloorNumber;
            ViewBag.HotelName = floor.Hotel.HotelName;

            List<FloorRoomIndexModel> model = new List<FloorRoomIndexModel>();

            foreach(var r in rooms){
                FloorRoomIndexModel _ = new(r, _dbContext.ReservationFamilyMembers
                    .FirstOrDefault(e => e.FloorRoomId == r.FloorRoomId) is not null);

                model.Add(_);

            }

            return View(model);
        }

        public IActionResult Create(int id)
        {
            Floor floor = _dbContext.Floors
                .Include(e => e.Hotel)
                .FirstOrDefault(e => e.FloorId == id);

            var roomsview = _dbContext.RoomViewsWithoutTrash;

            ViewBag.FloorNumber = floor.FloorNumber;
            ViewBag.HotelName = floor.Hotel.HotelName;

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;
            ViewBag.RoomViews = roomsview;
            var Floorrooms = _dbContext.FloorRoom
                .Include(e => e.Room)
                .Include(e => e.RoomBedType)
                .Where(e => !e.Trashed); //e.FloorId == id &&
            ViewBag.Floorrooms = JsonConvert.SerializeObject(Floorrooms);


            
            FloorRoom room = new FloorRoom();
            room.FloorId = id;

            return View(room);
        }

        [HttpPost]
        public IActionResult Create(FloorRoom room)
        {
            Floor floor = _dbContext.Floors
                 .Include(e => e.Hotel)
                 .FirstOrDefault(e => e.FloorId == room.FloorId);

            ViewBag.FloorNumber = floor.FloorNumber;
            ViewBag.HotelName = floor.Hotel.HotelName;

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;

            //if (!ModelState.IsValid) return View(room);
            for (int i = 0; i < room.RoomView.Count; i++)
            {
                FloorRoom roommodal = new FloorRoom
                {
                    RoomNumber = room.Rooms[i],
                    BathroomType = room.BathRoom[i],
                    RoomBedTypeId = room.BedType[i],
                    RoomId = room.RoomType[i],
                    RoomViewId = room.RoomView[i],
                    FloorId = room.FloorId
                };

                _dbContext.FloorRoom.Add(roommodal);
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Details", "Floor", new { id = floor.Hotel.HotelId });
        }

        public IActionResult Edit(int id)
        {
            FloorRoom room = _dbContext
                .FloorRoom
                .Include(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .FirstOrDefault(e => e.FloorRoomId == id);

            ViewBag.FloorNumber = room.Floor.FloorNumber;
            ViewBag.HotelName = room.Floor.Hotel.HotelName;

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash.ToList();
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;

            return View(room);
        }

        [HttpPost]
        public IActionResult Edit(FloorRoom room)
        {
            FloorRoom _room = _dbContext
                .FloorRoom
                .Include(e => e.Floor)
                .ThenInclude(e => e.Hotel)
                .FirstOrDefault(e => e.FloorRoomId == room.FloorRoomId);

            ViewBag.FloorNumber = _room.Floor.FloorNumber;
            ViewBag.HotelName = _room.Floor.Hotel.HotelName;

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;

            if (!ModelState.IsValid) return View(room);

            _room.RoomNumber = room.RoomNumber;
            _room.PAX = room.PAX;
            _room.RoomId = room.RoomId;
            _room.BathroomType = room.BathroomType;
            _room.RoomBedTypeId = room.RoomBedTypeId;

            _dbContext.SaveChanges();

            return RedirectToAction("Index", new { id = _room.FloorId });
        }


        public IActionResult ViewTrash(int id)
        {
            var rooms = _dbContext.FloorRoom
                .Include(e => e.Room)
                .Include(e => e.RoomBedType)
                .Where(e => e.FloorId == id && e.Trashed);

            Floor floor = _dbContext.Floors
                .Include(e => e.Hotel).FirstOrDefault(e => e.FloorId == id);

            ViewBag.FloorId = id;
            ViewBag.FloorNumber = floor.FloorNumber;
            ViewBag.HotelName = floor.Hotel.HotelName;

            return View(rooms);
        }

        public IActionResult Trash(int id)
        {
            FloorRoom room = _dbContext.FloorRoom
                .FirstOrDefault(e => e.FloorRoomId == id);
            var floorroom = _dbContext.ReservationFamilyMembers
                    .FirstOrDefault(e => e.FloorRoomId == id);

            if (floorroom != null)
            {
                return BadRequest("Already allocated to family");
            }
            else
            {
                room.Trashed = true;
                _dbContext.SaveChanges();
            }
            return RedirectToAction(nameof(Index), new { id = room.FloorId });
        }

        public IActionResult Restore(int id)
        {
            FloorRoom room = _dbContext.FloorRoom
                .FirstOrDefault(e => e.FloorRoomId == id);
            room.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index), new { id = room.FloorId });
        }

        public IActionResult GetRooms(int id)
        {
            int floorId = id;

            var rooms = _dbContext.FloorRoom
                .Include(e => e.Room)
                .Where(e => e.FloorId == floorId && !e.Trashed);

            return Ok(new
            {
                status = 200,
                msg = "Rooms Found!",
                data = rooms
            });
        }

        public IActionResult GetAllotedRooms(int id)
        {
            int floorId = id;

            var rooms = _dbContext.RoomAllotmentsWithoutTrash
                .Include(e => e.FloorRoom)
                .ThenInclude(e => e.Room)
                .Where(e => e.FloorRoom.FloorId == floorId);

            return Ok(new
            {
                status = 200,
                msg = "Rooms Found!",
                data = rooms
            });
        }
    }
}



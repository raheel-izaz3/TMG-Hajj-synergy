using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TMG.Controllers.ReservationControllers
{
    public class FloorController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public FloorController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create(int id)
        {
            FloorRegisterRequest request = new FloorRegisterRequest();
            request.HotelId = id;
            request.FloorCount = 0;
            return View(request);
        }

        [HttpPost]
        public IActionResult Create(FloorRegisterRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            for (int i = 0; i < request.FloorCount; i++)
            {
                _dbContext.Floors.Add(
                    new Floor
                    {
                        HotelId = request.HotelId,
                        FloorNumber = i + 1
                    }
                    );
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Hotel");
        }
        public IActionResult ViewTrash()
        {
            return View(_dbContext.Floors.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            Floor floor = _dbContext.Floors
                .Find(id);
            //if (floor == null) return RedirectToAction(nameof(Details));

            floor.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = floor.HotelId });
        }

        public IActionResult Restore(int id)
        {
            Floor floor = _dbContext.Floors.Find(id);
            floor.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = floor.HotelId });
        }

        public IActionResult Edit(int id)
        {
            Floor floor = _dbContext.Floors.Find(id);

            return View(floor);
        }

        [HttpPost]
        public IActionResult Edit(Floor floor)
        {
            if (!ModelState.IsValid) return View(floor);
            Floor floorToEdit = _dbContext.Floors.Find(floor.FloorId);

            floorToEdit.FloorNumber = floor.FloorNumber;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = floorToEdit.HotelId });
        }
        public IActionResult Details(int id)
        {
            ViewBag.TrashCount = _dbContext.Floors
                .Where(e => e.HotelId == id && e.Trashed).Count();

            Hotel hotel = _dbContext.Hotels.Find(id);
            var floors = _dbContext.Floors
                .Where(e => e.HotelId == id && !e.Trashed)
                .OrderBy(e => e.FloorNumber)
                .ToList();

            var rooms = _dbContext.FloorRoom
                .Include(e => e.Room)
                .Where(e => !e.Trashed)
                .ToList();

            var allotedRoomIds = _dbContext.ReservationFamilyMembers
                .Where(e => e.FloorRoomId != null && e.FloorId != null)
                .GroupBy(e => new { e.FloorId, e.FloorRoomId })
                .ToList();

            FloorDetailsModel model = new FloorDetailsModel();
            model.HotelName = hotel.HotelName;

            for (int i = 0; i < floors.Count; i++)
            {
                Floor currentFloor = floors.ElementAt(i);
                int roomCount = rooms
                    .Where(e => e.FloorId == currentFloor.FloorId && !e.Trashed)
                    .Count();

                int unAllotedRoomCount = roomCount - allotedRoomIds
                                                    .Where(e => e.Key.FloorId == currentFloor.FloorId)
                                                    .Count();

                List<FloorRoomAvailabilityModel> availabilityModel = new();

                foreach (var r in rooms.Where(e => e.FloorId == currentFloor.FloorId))
                {
                    FloorRoomAvailabilityModel _ = new()
                    {
                        FloorRoom = r,
                        Selected = _dbContext.ReservationFamilyMembers
                                        .FirstOrDefault(e => e.FloorRoomId == r.FloorRoomId) is not null,
                        PAXAllocated = (float)_dbContext.ReservationFamilyMembers
                                        .Where(e => e.FloorRoomId == r.FloorRoomId)
                                        .Count() / (float)r.Room.PAX
                    };

                    availabilityModel.Add(_);
                }

                var roomCodes = rooms.Where(e => e.FloorId == currentFloor.FloorId)
                    .Select(e => new
                    {
                        e.RoomId,
                        e.Room.RoomCode
                    })
                    .Distinct()
                    .ToList();

                List<RoomBreakUp> roomBreakUps = new();
                foreach (var r in roomCodes)
                {
                    RoomBreakUp _ = new();
                    _.RoomCode = r.RoomCode;

                    // return Ok(new
                    // {
                    //     currentFloor.FloorId,
                    //     r.RoomId,
                    //     a = _dbContext.ReservationFamilyMembers
                    //     .Select(e => new
                    //     {
                    //         e.FloorId,
                    //         e.FloorRoom.RoomId
                    //     })
                    //     .Where(e => e.FloorId == currentFloor.FloorId && e.RoomId == r.RoomId)
                    //     .Distinct()
                    // });

                    _.Total = _dbContext.FloorRoom
                            .Where(e => e.FloorId == currentFloor.FloorId && e.RoomId == r.RoomId)
                            .Count();
                    _.Allocated = _dbContext.ReservationFamilyMembers
                        .Select(e => new
                        {
                            e.FloorId,
                            e.FloorRoom.RoomId,
                            e.FloorRoomId
                        })
                        .Where(e => e.FloorId == currentFloor.FloorId && e.RoomId == r.RoomId)
                        .Distinct()
                        .Count();
                    _.Balance = _.Total - _.Allocated;

                    roomBreakUps.Add(_);
                }

                model.Entries.Add(
                    new FloorDetailsEntry
                    {
                        Floor = currentFloor,
                        RoomCount = roomCount,
                        UnAllotedRooms = unAllotedRoomCount,
                        Rooms = availabilityModel,
                        RoomData = roomBreakUps,
                    }
                );
            }

            return View(model);
        }

        public IActionResult GetFloors(int id)
        {
            int hotelId = id;
            var floors = _dbContext.Floors
                .Where(e => e.HotelId == id && !e.Trashed)
                .OrderBy(e => e.FloorNumber);

            return Ok(new
            {
                status = 200,
                msg = "Floors Found!",
                data = floors
            });

        }
    }
}

namespace TMG.Models.ReservationModels
{
    public record FloorDetailsModel
    {
        public string HotelName { get; set; }
        public List<FloorDetailsEntry> Entries { get; set; } = new List<FloorDetailsEntry>();
    }

    public record FloorDetailsEntry
    {
        public Floor Floor { get; set; }
        public int RoomCount { get; set; }
        public int UnAllotedRooms { get; set; }
        public List<RoomBreakUp> RoomData { get; set; }
        public List<FloorRoomAvailabilityModel> Rooms { get; set; } = new();
    }

    public record RoomBreakUp
    {
        public string RoomCode { get; set; }
        public int Total { get; set; }
        public int Allocated { get; set; }
        public int Balance { get; set; }
    }

    public record FloorRoomAvailabilityModel
    {
        public FloorRoom FloorRoom { get; set; }
        public bool Selected { get; set; }
        public float PAXAllocated { get; set; }

    }

    public record FloorRegisterRequest
    {
        public int HotelId { get; set; }

        [Required, Display(Name = "How many floors?", Prompt = "Enter Floor Count...")]
        public int FloorCount { get; set; }
    }
}

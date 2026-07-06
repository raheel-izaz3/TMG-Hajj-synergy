using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.ReservationModels;

namespace TMG.Controllers
{
    public class RoomDetailsController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public RoomDetailsController(TMGDbContext context, IHttpContextAccessor http)
        {
            _dbContext = context;
            _http = http;
        }
        public IActionResult Index(int id)
        {
            var RoomDetails = _dbContext.RoomDetails
                .Include(e => e.Room)
                .Include(e => e.RoomBedType)
                .Where(e => e.HotelId == id)
                .ToList();

            return View(RoomDetails);
        }

        public IActionResult Edit(int id) 
        {
            var roomDetails = _dbContext.RoomDetails.FirstOrDefault(e => e.RoomDetailsId == id);
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;

            return View(roomDetails);
        }

        [HttpPost]
        public IActionResult Edit(RoomDetails room)
        {
            var roomDetails = _dbContext.RoomDetails.FirstOrDefault(e => e.RoomDetailsId == room.RoomDetailsId);
            if (roomDetails == null)
            {
                return NotFound();
            }

            // Update RoomDetails Properties
            roomDetails.RoomNumber = room.RoomNumber;
            roomDetails.RoomBedTypeId = room.RoomBedTypeId;

            _dbContext.SaveChanges();

            // Redirect to Hotel Controller's Index Action with HotelId
            return RedirectToAction("Index", "Hotel", new { hotelId = roomDetails.POHotelsId });
        }


    }
}

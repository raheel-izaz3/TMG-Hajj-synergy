using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.ReservationModels;
using TMG.Models.POModels;

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
            var activeSeasonId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            var poHotel = _dbContext.POHotels.FirstOrDefault(ph => ph.HotelId == id && ph.HajjSeasonsId == activeSeasonId);
            
            if (poHotel == null)
            {
                var hotel = _dbContext.Hotels.FirstOrDefault(h => h.HotelId == id);
                if (hotel != null)
                {
                    poHotel = new POHotels
                    {
                        HotelId = id,
                        HajjSeasonsId = activeSeasonId,
                        PlaceId = hotel.PlaceId,
                        Code = $"PO-{hotel.HotelCode}-{DateTime.Now.Year}"
                    };
                    _dbContext.POHotels.Add(poHotel);
                    _dbContext.SaveChanges();
                }
            }
            ViewBag.POHotelsId = poHotel?.POHotelsId ?? 0;

            var RoomDetails = _dbContext.RoomDetails
                .Include(e => e.Room)
                .Include(e => e.RoomBedType)
                .Where(e => e.HotelId == id)
                .ToList();

            return View(RoomDetails);
        }

        public IActionResult Create(int hotelId)
        {
            var activeSeasonId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            
            // Try to find by POHotelsId first
            var poHotel = _dbContext.POHotels.FirstOrDefault(h => h.POHotelsId == hotelId);

            if (poHotel == null)
            {
                // Try to find by HotelId instead
                poHotel = _dbContext.POHotels.FirstOrDefault(h => h.HotelId == hotelId && h.HajjSeasonsId == activeSeasonId);

                if (poHotel == null)
                {
                    // Create POHotels record on the fly for this HotelId
                    var hotel = _dbContext.Hotels.FirstOrDefault(h => h.HotelId == hotelId);
                    if (hotel != null)
                    {
                        poHotel = new POHotels
                        {
                            HotelId = hotelId,
                            HajjSeasonsId = activeSeasonId,
                            PlaceId = hotel.PlaceId,
                            Code = $"PO-{hotel.HotelCode}-{DateTime.Now.Year}"
                        };
                        _dbContext.POHotels.Add(poHotel);
                        _dbContext.SaveChanges();
                    }
                }
            }

            if (poHotel == null)
            {
                // Ultimate fallback to first POHotel
                poHotel = _dbContext.POHotels.FirstOrDefault(h => h.HajjSeasonsId == activeSeasonId) 
                          ?? _dbContext.POHotels.FirstOrDefault();

                if (poHotel == null)
                {
                    return RedirectToAction("Index", "Hotel");
                }
            }

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash.ToList();
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash.ToList();

            var model = new RoomDetails
            {
                HotelId = poHotel.HotelId,
                POHotelsId = poHotel.POHotelsId,
                PlaceId = poHotel.PlaceId
            };

            return View(model);
        }

        // 3. ACTION METHOD: Create (POST - Saves the new room to the database)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RoomDetails room)
        {
            // Removed the faulty fallback that overrode POHotelsId when it was 0, 
            // ensuring the explicitly targeted hotel ID from the form is preserved.

            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = _dbContext.RoomsWithoutTrash.ToList();
                ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash.ToList();
                return View(room);
            }

            if (room.POHotelsId > 0)
            {
                var poHotel = _dbContext.POHotels.FirstOrDefault(h => h.POHotelsId == room.POHotelsId);
                if (poHotel != null)
                {
                    room.HotelId = poHotel.HotelId;
                    room.PlaceId = poHotel.PlaceId;
                }
            }

            _dbContext.RoomDetails.Add(room);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Hotel", new { hotelId = room.POHotelsId });
        }

        public IActionResult Edit(int id)
        {
            var roomDetails = _dbContext.RoomDetails.FirstOrDefault(e => e.RoomDetailsId == id);
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash.ToList();
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash.ToList();

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

            roomDetails.RoomNumber = room.RoomNumber;
            roomDetails.RoomBedTypeId = room.RoomBedTypeId;

            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Hotel", new { hotelId = roomDetails.POHotelsId });
        }
    }
}
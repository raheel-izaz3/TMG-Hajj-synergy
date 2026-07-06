using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TMG.Data;
using TMG.Models.POModels;
using TMG.Models.ReservationModels;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.POControllers
{
    public class POHotelController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;


        public POHotelController(TMGDbContext context, IHttpContextAccessor http)
        {
            _dbContext = context;
            _http = http;
        }
        public IActionResult Index()
        {
            var poHotel = _dbContext.POHotels
                .Include(e => e.Hotel)
                .Include(e => e.Vendor)
                .Include(e => e.Place)
                .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList(); // Fixed the typo here

            ViewBag.Trashed = _dbContext.POHotels.Where(e => e.Trashed == true && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();

            return View(poHotel);

        }

        public IActionResult Create()
        {
            var hotel = _dbContext.HotelsWithoutTrash
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.Hotel = hotel;

            var vendor = _dbContext.Vendors
                .Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();
            ViewBag.Vendor = vendor;

            var roomsview = _dbContext.RoomViewsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash.Where(e => !e.Trashed).ToList();
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;
            ViewBag.RoomViews = roomsview;

            var Floorrooms = _dbContext.FloorRoom
               .Include(e => e.Room)
               .Include(e => e.RoomBedType)
               .Where(e => !e.Trashed);
            ViewBag.Floorrooms = JsonConvert.SerializeObject(Floorrooms);

            ViewBag.BathType = Enum.GetValues(typeof(BathroomType))
                          .Cast<BathroomType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            ViewBag.TaxesType = Enum.GetValues(typeof(Taxes))
                          .Cast<Taxes>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            ViewBag.PaymentMode = Enum.GetValues(typeof(PaymentMode))
                          .Cast<PaymentMode>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            return View();
        }

        [HttpPost]
        /*public IActionResult Create(POHotelViewModel model)
        {
            //return Ok(model);
            var transaction = _dbContext.Database.BeginTransaction();
            int hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            try
            {

                var poHotels = new POHotels
                {
                    NumberOfRooms = model.NumberOfRooms,
                    MunicipilityAmount = model.MunicipilityAmount,
                    MunicipilityPercentage = model.MunicipilityPercentage,
                    MunicipilityFeeAmount = model.MunicipilityFeeAmount,
                    VatAmmount = model.VatAmmount,
                    VatPercentage = model.VatPercentage,
                    VatFeeAmount = model.VatFeeAmount,
                    HandlingCharges = model.HandlingCharges,
                    HandlingPercentage = model.HandlingPercentage,
                    HandlingFeeAmount = model.HandlingFeeAmount,
                    Total = model.Total,
                    SubTotal = model.SubTotal,
                    PaymentMode = model.PaymentMode,
                    TaxesId = model.TaxesId,
                    VendorId = model.VendorId,
                    HotelId = model.HotelId,
                    PlaceId =model.PlaceId,
                    HajjSeasonsId = hajjSeasonId  // Assuming your table has this column
                };

                _dbContext.POHotels.Add(poHotels);
                _dbContext.SaveChanges();

                var POHotelId = poHotels.POHotelsId;


                for (int i = 0; i < model.Amount.Count(); i++)
                {
                    var po = new POHotelRooms
                    {
                        Quantity = i < model.Quantity.Count() ? model.Quantity[i] : (int?)null,
                        Amount = i < model.Amount.Count() ? model.Amount[i] : (float?)null,
                        Arrival = i < model.Arrival.Count() ? (DateTime?)model.Arrival[i] : null,
                        Departure = i < model.Departure.Count() ? (DateTime?)model.Departure[i] : null,
                        Total = i < model.Totals.Count() ? model.Totals[i] : (float?)null,
                        BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                        RoomId = i < model.RoomId.Count() ? model.RoomId[i] : (int?)null,
                        RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] : (int?)null,
                        POHotelsId = POHotelId
                    };

                    _dbContext.POHotelRooms.Add(po);
                    _dbContext.SaveChanges();  // Isko turant save karo taake POHotelRoomsId mil jaye

                    int poHotelRoomId = po.POHotelRoomsId; // Ab is ID ko use karo

                    int roomCount = model.Quantity[i] ?? 0;

                    for (int j = 0; j < roomCount; j++)
                    {
                        var pos = new RoomDetails
                        {
                            Arrival = i < model.Arrival.Count() ? model.Arrival[i] : (DateTime?)null,
                            Departure = i < model.Departure.Count() ? model.Departure[i] : (DateTime?)null,
                            BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                            RoomId = i < model.RoomId.Count() ? model.RoomId[i] ?? (int?)null : (int?)null,
                            RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] ?? (int?)null : (int?)null,
                            POHotelsId = POHotelId,
                            POHotelRoomsId = poHotelRoomId, // Yahan sahi ID assign karo
                            HotelId = model.HotelId,
                            PlaceId = model.PlaceId
                        };

                        _dbContext.RoomDetails.Add(pos);
                    }
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(model);
            }
        }*/

        public IActionResult Create(POHotelViewModel model)
        {
            var transaction = _dbContext.Database.BeginTransaction();
            int hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            try
            {
                // POHotels Table me Data Save Karna
                var poHotels = new POHotels
                {
                    NumberOfRooms = model.NumberOfRooms,
                    MunicipilityAmount = model.MunicipilityAmount,
                    MunicipilityPercentage = model.MunicipilityPercentage,
                    MunicipilityFeeAmount = model.MunicipilityFeeAmount,
                    VatAmmount = model.VatAmmount,
                    VatPercentage = model.VatPercentage,
                    VatFeeAmount = model.VatFeeAmount,
                    HandlingCharges = model.HandlingCharges,
                    HandlingPercentage = model.HandlingPercentage,
                    HandlingFeeAmount = model.HandlingFeeAmount,
                    Total = model.Total,
                    SubTotal = model.SubTotal,
                    PaymentMode = model.PaymentMode,
                    TaxesId = model.TaxesId,
                    VendorId = model.VendorId,
                    HotelId = model.HotelId,
                    PlaceId = model.PlaceId,
                    Code = model.Code,
                    HajjSeasonsId = hajjSeasonId
                };

                _dbContext.POHotels.Add(poHotels);
                _dbContext.SaveChanges();
                int POHotelId = poHotels.POHotelsId;

                if (model.PlaceId == 4)
                {
                    // Last FloorNumber get karna hotel ke mutabiq
                    int lastFloorNumber = _dbContext.Floors
                        .Where(f => f.HotelId == model.HotelId)
                        .OrderByDescending(f => f.FloorNumber)
                        .Select(f => f.FloorNumber)
                        .FirstOrDefault();

                    int newFloorNumber = lastFloorNumber + 1; // Next FloorNumber

                    // Floor Table me entry add karein
                    var floor = new Floor
                    {
                        HotelId = model.HotelId,
                        POHotelsId = POHotelId,
                        FloorNumber = model.FloorNumber,
                    };

                    _dbContext.Floors.Add(floor);
                    _dbContext.SaveChanges();
                    int floorId = floor.FloorId; // Newly created Floor ID


                    // === POHotelRooms Add Karein, RoomDetails Nahi ===
                    for (int i = 0; i < model.Amount.Count(); i++)
                    {
                        var po = new POHotelRooms
                        {
                            Quantity = i < model.Quantity.Count() ? model.Quantity[i] : (int?)null,
                            Amount = i < model.Amount.Count() ? model.Amount[i] : (float?)null,
                            Arrival = i < model.Arrival.Count() ? (DateTime?)model.Arrival[i] : null,
                            Departure = i < model.Departure.Count() ? (DateTime?)model.Departure[i] : null,
                            Total = i < model.Totals.Count() ? model.Totals[i] : (float?)null,
                            BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                            RoomId = i < model.RoomId.Count() ? model.RoomId[i] : (int?)null,
                            RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] : (int?)null,
                            POHotelsId = POHotelId
                        };

                        _dbContext.POHotelRooms.Add(po);
                        _dbContext.SaveChanges();
                        int poHotelRoomId = po.POHotelRoomsId;


                        // FloorRoom Table me Rooms ki Quantity ke mutabiq entries dalein

                        int roomCount = model.Quantity[i] ?? 0;

                        for (int j = 0; j < roomCount; j++)
                        {
                            var floorRoom = new FloorRoom
                            {
                                RoomNumber = model.RoomNumber.FirstOrDefault() ?? "Unknown",
                                BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                                RoomBedTypeId = null, // Null as required
                                RoomId = i < model.RoomId.Count() ? model.RoomId[i] : (int?)null,
                                RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] : (int?)null,
                                FloorId = floorId,
                                POHotelRoomsId = poHotelRoomId
                            };

                            _dbContext.FloorRoom.Add(floorRoom);
                        }
                    }

                }

                // === PLACE ID != 4 === => POHotelRooms & RoomDetails Tables Add
                else
                {
                    for (int i = 0; i < model.Amount.Count(); i++)
                    {
                        var po = new POHotelRooms
                        {
                            Quantity = i < model.Quantity.Count() ? model.Quantity[i] : (int?)null,
                            Amount = i < model.Amount.Count() ? model.Amount[i] : (float?)null,
                            Arrival = i < model.Arrival.Count() ? (DateTime?)model.Arrival[i] : null,
                            Departure = i < model.Departure.Count() ? (DateTime?)model.Departure[i] : null,
                            Total = i < model.Totals.Count() ? model.Totals[i] : (float?)null,
                            BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                            RoomId = i < model.RoomId.Count() ? model.RoomId[i] : (int?)null,
                            RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] : (int?)null,
                            POHotelsId = POHotelId
                        };

                        _dbContext.POHotelRooms.Add(po);
                        _dbContext.SaveChanges();
                        int poHotelRoomId = po.POHotelRoomsId;

                        int roomCount = model.Quantity[i] ?? 0;

                        for (int j = 0; j < roomCount; j++)
                        {
                            var pos = new RoomDetails
                            {
                                Arrival = i < model.Arrival.Count() ? model.Arrival[i] : (DateTime?)null,
                                Departure = i < model.Departure.Count() ? model.Departure[i] : (DateTime?)null,
                                BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                                RoomId = i < model.RoomId.Count() ? model.RoomId[i] ?? (int?)null : (int?)null,
                                RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] ?? (int?)null : (int?)null,
                                POHotelsId = POHotelId,
                                POHotelRoomsId = poHotelRoomId,
                                HotelId = model.HotelId,
                                PlaceId = model.PlaceId
                            };

                            _dbContext.RoomDetails.Add(pos);
                        }
                    }
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var hotel = _dbContext.HotelsWithoutTrash.ToList();

            ViewBag.Hotel = hotel;

            var vendor = _dbContext.Vendors.ToList();
            ViewBag.Vendor = vendor;

            var roomsview = _dbContext.RoomViewsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomTypes = _dbContext.RoomBedTypesWithoutTrash;
            ViewBag.RoomViews = roomsview;

            var Floorrooms = _dbContext.FloorRoom
               .Include(e => e.Room)
               .Include(e => e.RoomBedType)
               .Where(e => !e.Trashed); //e.FloorId == id &&
            ViewBag.Floorrooms = JsonConvert.SerializeObject(Floorrooms);

            ViewBag.BathType = Enum.GetValues(typeof(BathroomType))
                          .Cast<BathroomType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            ViewBag.TaxesType = Enum.GetValues(typeof(Taxes))
                          .Cast<Taxes>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

            ViewBag.PaymentMode = Enum.GetValues(typeof(PaymentMode))
                          .Cast<PaymentMode>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


            var poHotels = _dbContext.POHotels
                .Include(e => e.Hotel)
                .Include(e => e.Vendor)
                .Include(e => e.POHotelRooms)
                .Where(e => e.POHotelsId == id).FirstOrDefault();

            POHotelEditViewModel pO = new POHotelEditViewModel
            {
                POHotelsId = poHotels.POHotelsId,
                NumberOfRooms = poHotels.NumberOfRooms,
                MunicipilityAmount = poHotels.MunicipilityAmount ?? 0f,
                MunicipilityPercentage = poHotels.MunicipilityPercentage ?? 0f,
                MunicipilityFeeAmount = poHotels.MunicipilityFeeAmount ?? 0f,
                VatAmmount = poHotels.VatAmmount ?? 0f,
                VatPercentage = poHotels.VatPercentage ?? 0f,
                VatFeeAmount = poHotels.VatFeeAmount ?? 0f,
                HandlingCharges = poHotels.HandlingCharges ?? 0f,
                HandlingPercentage = poHotels.HandlingPercentage ?? 0f,
                HandlingFeeAmount = poHotels.HandlingFeeAmount ?? 0f,
                Total = poHotels.Total ?? 0f,
                SubTotal = poHotels.SubTotal ?? 0f,
                PaymentMode = poHotels.PaymentMode,
                TaxesId = poHotels.TaxesId,
                VendorId = poHotels.VendorId,
                HotelId = poHotels.HotelId,
                Code = poHotels.Code,
                PlaceId = poHotels.PlaceId,

                POHotelRoomDatas = poHotels.POHotelRooms?.Select(room => new POHotelRoomData
                {
                    POHotelRoomDatasId = room.POHotelRoomsId,
                    Quantity = room.Quantity,
                    Amount = room.Amount,
                    Arrival = room.Arrival,
                    Departure = room.Departure,
                    Total = room.Total,
                    BathroomType = room.BathroomType,
                    RoomId = room.RoomId,
                    RoomViewId = room.RoomViewId
                }).ToList()
            };

            return View(pO);
        }

        [HttpPost]
        public IActionResult Edit(POHotelEditViewModel model)
        {
            var sessionValue = _http.HttpContext.Session.GetString("ActiveSeasonId");
            if (string.IsNullOrEmpty(sessionValue) || !int.TryParse(sessionValue, out int hajjSeasonId))
            {
                return BadRequest("ActiveSeasonId is missing or invalid.");
            }

            var poHotel = _dbContext.POHotels
              .Include(e => e.Hotel)
              .Include(e => e.Vendor)
              .Include(e => e.POHotelRooms)
              .ThenInclude(r => r.RoomDetails)
              .FirstOrDefault(e => e.POHotelsId == model.POHotelsId);

            var existinIds = _dbContext.Floors.Where(e => e.POHotelsId == model.POHotelsId).ToList();

            if (poHotel == null)
            {
                return NotFound("POHotel not found.");
            }

            var existingRoomIds = model.POHotelRoomDatasId?.Where(id => id > 0).ToList() ?? new List<int>();

            var roomsToDelete = _dbContext.POHotelRooms
                .Where(r => r.POHotelsId == model.POHotelsId && !existingRoomIds.Contains(r.POHotelRoomsId))
                .ToList();

            var roomsToDeleteIds = roomsToDelete.Select(r => (int?)r.POHotelRoomsId).ToList();

            var floorRoomsToDelete = _dbContext.FloorRoom
                .Where(fr => roomsToDeleteIds.Contains(fr.POHotelRoomsId))
                .ToList();

            // Get FloorRoom IDs to delete
            var floorRoomIdsToDelete = floorRoomsToDelete.Select(fr => fr.FloorRoomId).ToList();

            // Delete related ItineraryBlockReservationForAzizias records first
            var itineraryReservationsToDelete = _dbContext.ItineraryBlockReservationForAzizias
                .Where(i => floorRoomIdsToDelete.Contains(i.FloorRoomId ?? 0))
                .ToList();

            _dbContext.ItineraryBlockReservationForAzizias.RemoveRange(itineraryReservationsToDelete);

            // Then delete FloorRoom entries
            _dbContext.FloorRoom.RemoveRange(floorRoomsToDelete);

            // Delete RoomDetails
            foreach (var room in roomsToDelete)
            {
                var relatedRoomDetails = _dbContext.RoomDetails
                    .Where(rd => rd.POHotelRoomsId == room.POHotelRoomsId)
                    .ToList();

                _dbContext.RoomDetails.RemoveRange(relatedRoomDetails);
            }

            // Finally delete POHotelRooms
            _dbContext.POHotelRooms.RemoveRange(roomsToDelete);
            _dbContext.SaveChanges();



            // **Step 3: Update POHotel Information**
            poHotel.HotelId = model.HotelId;
            poHotel.VendorId = model.VendorId;
            poHotel.NumberOfRooms = model.NumberOfRooms;
            poHotel.TaxesId = model.TaxesId;
            poHotel.PaymentMode = model.PaymentMode;
            poHotel.Total = model.Total;
            poHotel.MunicipilityAmount = model.MunicipilityAmount;
            poHotel.MunicipilityPercentage = model.MunicipilityPercentage;
            poHotel.MunicipilityFeeAmount = model.MunicipilityFeeAmount;
            poHotel.VatAmmount = model.VatAmmount;
            poHotel.VatPercentage = model.VatPercentage;
            poHotel.VatFeeAmount = model.VatFeeAmount;
            poHotel.HandlingCharges = model.HandlingCharges;
            poHotel.HandlingPercentage = model.HandlingPercentage;
            poHotel.HandlingFeeAmount = model.HandlingFeeAmount;
            poHotel.SubTotal = model.SubTotal;
            poHotel.PlaceId = model.PlaceId;
            poHotel.Code = model.Code;
            poHotel.HajjSeasonsId = hajjSeasonId;

            _dbContext.SaveChanges();

            // **Step 4: Handle POHotelRooms (Update Existing + Add New)**
            if (model.PlaceId == 4)
            {
                for (int i = 0; i < model.Amount.Count(); i++)
                {
                    if (i < existingRoomIds.Count) // **Update Existing Rooms**
                    {
                        var existingRoom = _dbContext.POHotelRooms
                            .Include(r => r.RoomDetails)
                            .Include(e => e.FloorRooms)// Include RoomDetails for manipulation
                            .FirstOrDefault(r => r.POHotelRoomsId == existingRoomIds[i]);

                        if (existingRoom != null)
                        {
                            int previousQuantity = existingRoom.Quantity ?? 0;
                            int newQuantity = model.Quantity[i] ?? 0;

                            existingRoom.Quantity = newQuantity;
                            existingRoom.Amount = model.Amount[i];
                            existingRoom.Arrival = model.Arrival[i];
                            existingRoom.Departure = model.Departure[i];
                            existingRoom.Total = model.Totals[i];
                            existingRoom.BathroomType = model.BathroomType[i];
                            existingRoom.RoomId = model.RoomId[i];
                            existingRoom.RoomViewId = model.RoomViewId[i];

                            _dbContext.SaveChanges();

                            if (newQuantity > previousQuantity)
                            {
                                // **Add Extra RoomDetails**
                                for (int j = previousQuantity; j < newQuantity; j++)
                                {
                                    var roomDetail = new FloorRoom
                                    {
                                        RoomNumber = model.RoomNumber.FirstOrDefault() ?? "Unknown",
                                        BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                                        RoomBedTypeId = null, // Null as required
                                        RoomId = i < model.RoomId.Count() ? model.RoomId[i] : (int?)null,
                                        RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] : (int?)null,
                                        POHotelRoomsId = existingRoom.POHotelRoomsId,
                                        FloorId = existinIds.FirstOrDefault().FloorId
                                    };

                                    _dbContext.FloorRoom.Add(roomDetail);
                                }
                            }
                            else if (newQuantity < previousQuantity)
                            {
                                // **Remove Extra RoomDetails (Starting from Last Added)**
                                var roomDetailsToRemove = existingRoom.FloorRooms
                                    .OrderByDescending(rd => rd.FloorRoomId) // Get Last Added
                                    .Take(previousQuantity - newQuantity)
                                    .ToList();

                                // Fetch FloorRoomIds of rooms being removed
                                var removedFloorRoomIds = roomDetailsToRemove.Select(r => r.FloorRoomId).ToList();

                                // Remove related ReservationFamilyMembers
                                var familyMembersToRemove = _dbContext.ReservationFamilyMembers
                                .Where(rfm => rfm.FloorRoomId.HasValue && removedFloorRoomIds.Contains(rfm.FloorRoomId.Value))
                                .ToList();


                                _dbContext.ReservationFamilyMembers.RemoveRange(familyMembersToRemove);
                                _dbContext.FloorRoom.RemoveRange(roomDetailsToRemove);

                                _dbContext.SaveChanges();
                            }


                            _dbContext.SaveChanges();
                        }
                    }
                    else // **Add New POHotelRoom**
                    {
                        var newRoom = new POHotelRooms
                        {
                            Quantity = model.Quantity[i],
                            Amount = model.Amount[i],
                            Arrival = model.Arrival[i],
                            Departure = model.Departure[i],
                            Total = model.Totals[i],
                            BathroomType = model.BathroomType[i],
                            RoomId = model.RoomId[i],
                            RoomViewId = model.RoomViewId[i],
                            POHotelsId = model.POHotelsId
                        };

                        _dbContext.POHotelRooms.Add(newRoom);
                        _dbContext.SaveChanges(); // Ensure ID is generated

                        int roomCount = model.Quantity[i] ?? 0;
                        for (int j = 0; j < roomCount; j++)
                        {
                            var floorRoom = new FloorRoom
                            {
                                RoomNumber = model.RoomNumber.FirstOrDefault() ?? "Unknown",
                                BathroomType = i < model.BathroomType.Count() ? model.BathroomType[i] : null,
                                RoomBedTypeId = null, // Null as required
                                RoomId = i < model.RoomId.Count() ? model.RoomId[i] : (int?)null,
                                RoomViewId = i < model.RoomViewId.Count() ? model.RoomViewId[i] : (int?)null,
                                FloorId = existinIds.FirstOrDefault().FloorId,
                                POHotelRoomsId = newRoom.POHotelRoomsId
                            };

                            _dbContext.FloorRoom.Add(floorRoom);
                        }

                        _dbContext.SaveChanges();
                    }
                }
            }
            else
            {
                for (int i = 0; i < model.Amount.Count(); i++)
                {
                    /*if (i < existingRoomIds.Count) // **Update Existing Rooms**
                    {
                        var existingRoom = _dbContext.POHotelRooms
                            .Include(r => r.RoomDetails) // Include RoomDetails for manipulation
                            .FirstOrDefault(r => r.POHotelRoomsId == existingRoomIds[i]);

                        if (existingRoom != null)
                        {
                            int previousQuantity = existingRoom.Quantity ?? 0;
                            int newQuantity = model.Quantity[i] ?? 0;

                            existingRoom.Quantity = newQuantity;
                            existingRoom.Amount = model.Amount[i];
                            existingRoom.Arrival = model.Arrival[i];
                            existingRoom.Departure = model.Departure[i];
                            existingRoom.Total = model.Totals[i];
                            existingRoom.BathroomType = model.BathroomType[i];
                            existingRoom.RoomId = model.RoomId[i];
                            existingRoom.RoomViewId = model.RoomViewId[i];

                            _dbContext.SaveChanges();

                            if (newQuantity > previousQuantity)
                            {
                                // **Add Extra RoomDetails**
                                for (int j = previousQuantity; j < newQuantity; j++)
                                {
                                    var roomDetail = new RoomDetails
                                    {
                                        Arrival = model.Arrival[i],
                                        Departure = model.Departure[i],
                                        BathroomType = model.BathroomType[i],
                                        RoomId = model.RoomId[i],
                                        RoomViewId = model.RoomViewId[i],
                                        POHotelsId = model.POHotelsId,
                                        POHotelRoomsId = existingRoom.POHotelRoomsId,
                                        HotelId = model.HotelId,
                                        PlaceId = model.PlaceId
                                    };

                                    _dbContext.RoomDetails.Add(roomDetail);
                                }
                            }
                            else if (newQuantity < previousQuantity)
                            {
                                // **Remove Extra RoomDetails (Starting from Last Added)**
                                var roomDetailsToRemove = existingRoom.RoomDetails
                                    .OrderByDescending(rd => rd.RoomDetailsId) // Get Last Added
                                    .Take(previousQuantity - newQuantity)
                                    .ToList();

                                // **Remove Corresponding SubGroupRooms Entries**
                                var roomDetailsIds = roomDetailsToRemove.Select(rd => rd.RoomDetailsId).ToList();

                                var subGroupRoomsToRemove = _dbContext.SubGroupRooms
                                    .Where(sgr => sgr.RoomDetailsId.HasValue && roomDetailsIds.Contains(sgr.RoomDetailsId.Value))
                                    .ToList();

                                _dbContext.SubGroupRooms.RemoveRange(subGroupRoomsToRemove);

                                _dbContext.RoomDetails.RemoveRange(roomDetailsToRemove);
                            }

                            _dbContext.SaveChanges();

                        }
                    }*/
                    if (i < existingRoomIds.Count) // **Update Existing Rooms**
                    {
                        var existingRoom = _dbContext.POHotelRooms
                            .Include(r => r.RoomDetails)
                            .FirstOrDefault(r => r.POHotelRoomsId == existingRoomIds[i]);

                        if (existingRoom != null)
                        {
                            int previousQuantity = existingRoom.Quantity ?? 0;
                            int newQuantity = model.Quantity[i] ?? 0;

                            existingRoom.Quantity = newQuantity;
                            existingRoom.Amount = model.Amount[i];
                            existingRoom.Arrival = model.Arrival[i];
                            existingRoom.Departure = model.Departure[i];
                            existingRoom.Total = model.Totals[i];
                            existingRoom.BathroomType = model.BathroomType[i];
                            existingRoom.RoomId = model.RoomId[i];
                            existingRoom.RoomViewId = model.RoomViewId[i];

                            _dbContext.SaveChanges();

                            if (newQuantity > previousQuantity)
                            {
                                // **Add Extra RoomDetails**
                                for (int j = previousQuantity; j < newQuantity; j++)
                                {
                                    var roomDetail = new RoomDetails
                                    {
                                        Arrival = model.Arrival[i],
                                        Departure = model.Departure[i],
                                        BathroomType = model.BathroomType[i],
                                        RoomId = model.RoomId[i],
                                        RoomViewId = model.RoomViewId[i],
                                        POHotelsId = model.POHotelsId,
                                        POHotelRoomsId = existingRoom.POHotelRoomsId,
                                        HotelId = model.HotelId,
                                        PlaceId = model.PlaceId
                                    };

                                    _dbContext.RoomDetails.Add(roomDetail);
                                }
                            }
                            else if (newQuantity < previousQuantity)
                            {
                                var roomDetailsToRemove = existingRoom.RoomDetails
                                    .OrderByDescending(rd => rd.RoomDetailsId)
                                    .Take(previousQuantity - newQuantity)
                                    .ToList();

                                var roomDetailsIds = roomDetailsToRemove.Select(rd => rd.RoomDetailsId).ToList();

                                // **Check assignment in Madinah or Makkah**
                                var assignedInMadinah = _dbContext.ItineraryBlockReservationForMadinah
                                    .Any(r => roomDetailsIds.Contains(r.RoomDetailsId ?? 0));
                                var assignedInMakkah = _dbContext.ItineraryBlockReservationForMakkah
                                    .Any(r => roomDetailsIds.Contains(r.RoomDetailsId ?? 0));

                                if ((assignedInMadinah || assignedInMakkah) && !model.ForceDelete) // flag from frontend
                                {
                                    // Stop and notify frontend
                                    return Conflict(new
                                    {
                                        status = 409,
                                        message = "Some of the rooms are assigned in reservations. Do you want to continue?",
                                        requiresConfirmation = true
                                    });
                                }

                                // **Remove related reservations if forceDelete is true**
                                if (model.ForceDelete)
                                {
                                    var madinahReservations = _dbContext.ItineraryBlockReservationForMadinah
                                        .Where(r => roomDetailsIds.Contains(r.RoomDetailsId ?? 0))
                                        .ToList();
                                    _dbContext.ItineraryBlockReservationForMadinah.RemoveRange(madinahReservations);

                                    var makkahReservations = _dbContext.ItineraryBlockReservationForMakkah
                                        .Where(r => roomDetailsIds.Contains(r.RoomDetailsId ?? 0))
                                        .ToList();
                                    _dbContext.ItineraryBlockReservationForMakkah.RemoveRange(makkahReservations);
                                }

                                // **Remove SubGroupRooms + RoomDetails**
                                var subGroupRoomsToRemove = _dbContext.SubGroupRooms
                                    .Where(sgr => sgr.RoomDetailsId.HasValue && roomDetailsIds.Contains(sgr.RoomDetailsId.Value))
                                    .ToList();
                                _dbContext.SubGroupRooms.RemoveRange(subGroupRoomsToRemove);

                                _dbContext.RoomDetails.RemoveRange(roomDetailsToRemove);
                            }

                            _dbContext.SaveChanges();
                        }
                    }

                    else // **Add New POHotelRoom**
                    {
                        var newRoom = new POHotelRooms
                        {
                            Quantity = model.Quantity[i],
                            Amount = model.Amount[i],
                            Arrival = model.Arrival[i],
                            Departure = model.Departure[i],
                            Total = model.Totals[i],
                            BathroomType = model.BathroomType[i],
                            RoomId = model.RoomId[i],
                            RoomViewId = model.RoomViewId[i],
                            POHotelsId = model.POHotelsId
                        };

                        _dbContext.POHotelRooms.Add(newRoom);
                        _dbContext.SaveChanges(); // Ensure ID is generated

                        // **Step 5: Add RoomDetails Based on Quantity**
                        int roomCount = model.Quantity[i] ?? 0;
                        for (int j = 0; j < roomCount; j++)
                        {
                            var roomDetail = new RoomDetails
                            {
                                Arrival = model.Arrival[i],
                                Departure = model.Departure[i],
                                BathroomType = model.BathroomType[i],
                                RoomId = model.RoomId[i],
                                RoomViewId = model.RoomViewId[i],
                                POHotelsId = model.POHotelsId,
                                POHotelRoomsId = newRoom.POHotelRoomsId,
                                HotelId = model.HotelId,
                                PlaceId = model.PlaceId

                            };

                            _dbContext.RoomDetails.Add(roomDetail);
                        }

                        _dbContext.SaveChanges();
                    }
                }
            }


            return RedirectToAction("Index");
        }

        public IActionResult ViewTrash()
        {
            var poHotel = _dbContext.POHotels
                .Include(e => e.Hotel)
                .Include(e => e.Vendor)
                .Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList(); // Fixed the typo here

            ViewBag.Trashed = _dbContext.POHotels.Where(e => e.Trashed == false).Count();

            return View(poHotel);
        }

        public IActionResult Trash(int id)
        {
            var poHotel = _dbContext.POHotels
                .Include(e => e.POHotelRooms)
                .FirstOrDefault(e => e.POHotelsId == id);

            if (poHotel == null)
                return RedirectToAction(nameof(Index));

            poHotel.Trashed = true;

            foreach (var room in poHotel.POHotelRooms)
            {
                room.Trashed = true;
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Restore(int id)
        {
            var poHotel = _dbContext.POHotels
                .Include(e => e.POHotelRooms)
                .FirstOrDefault(e => e.POHotelsId == id);

            if (poHotel == null)
                return RedirectToAction(nameof(Index));

            poHotel.Trashed = false;

            foreach (var room in poHotel.POHotelRooms)
            {
                room.Trashed = false;
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult GetSummary(int id, int placeId)
        {
            if (placeId == 4)
            {
                var poHotel = _dbContext.POHotels
                    .Include(p => p.POHotelRooms)
                        .ThenInclude(pr => pr.Room)
                    .Include(p => p.POHotelRooms)
                        .ThenInclude(pr => pr.FloorRooms)
                    .FirstOrDefault(p => p.POHotelsId == id);

                if (poHotel == null)
                {
                    return NotFound();
                }

                var floorRooms = poHotel.POHotelRooms
                    .SelectMany(pr => pr.FloorRooms.Select(fr => new
                    {
                        FloorRoomId = fr.FloorRoomId,
                        RoomType = pr.Room.RoomName,
                        Pax = pr.Room.PAX,
                        RoomId = pr.Room.RoomId
                    }))
                    .ToList();

                var floorRoomIds = floorRooms.Select(fr => fr.FloorRoomId).ToList();

                var reservations = _dbContext.ItineraryBlockReservationForAzizias
                    .Include(e => e.ItineraryBlock)
                    .Where(r => floorRoomIds.Contains(r.FloorRoomId ?? 0) && !r.ItineraryBlock.Trashed)
                    .ToList();

                var floorRoomMap = floorRooms.ToDictionary(fr => fr.FloorRoomId, fr => new { fr.RoomType, fr.RoomId, fr.Pax });

                var roomSummary = floorRooms
                    .GroupBy(fr => new { fr.RoomId, fr.RoomType, fr.Pax })
                    .Select(g =>
                    {
                        var relatedFloorRoomIds = g.Select(x => x.FloorRoomId).ToList();

                        var allocatedPax = reservations
                            .Count(r => relatedFloorRoomIds.Contains(r.FloorRoomId ?? 0));

                        return new
                        {
                            RoomType = g.Key.RoomType,
                            TotalRooms = g.Count(),
                            Allocated = Math.Round((double)allocatedPax / g.Key.Pax, 2),   // Divide by Pax
                            Balance = Math.Round(g.Count() - ((double)allocatedPax / g.Key.Pax), 2)
                        };
                    })
                    .ToList();

                return PartialView("_SummaryPartial", roomSummary);
            }
            else if (placeId == 2)
            {
                var poHotel = _dbContext.POHotels
                    .Include(p => p.POHotelRooms)
                        .ThenInclude(pr => pr.Room)
                    .Include(p => p.POHotelRooms)
                        .ThenInclude(pr => pr.RoomDetails)
                    .FirstOrDefault(p => p.POHotelsId == id);

                if (poHotel == null)
                {
                    return NotFound();
                }

                var floorRooms = poHotel.POHotelRooms
                    .SelectMany(pr => pr.RoomDetails.Select(fr => new
                    {
                        FloorRoomId = fr.RoomDetailsId,
                        RoomType = pr.Room.RoomName,
                        Pax = pr.Room.PAX,
                        RoomId = pr.Room.RoomId
                    }))
                    .ToList();

                var floorRoomIds = floorRooms.Select(fr => fr.FloorRoomId).ToList();

                var reservations = _dbContext.ItineraryBlockReservationForMadinah
                    .Where(r => floorRoomIds.Contains(r.RoomDetailsId ?? 0))
                    .ToList();

                var floorRoomMap = floorRooms.ToDictionary(fr => fr.FloorRoomId, fr => new { fr.RoomType, fr.RoomId, fr.Pax });

                var roomSummary = floorRooms
                    .GroupBy(fr => new { fr.RoomId, fr.RoomType, fr.Pax })
                    .Select(g =>
                    {
                        var relatedFloorRoomIds = g.Select(x => x.FloorRoomId).ToList();

                        var allocatedPax = reservations
                            .Count(r => relatedFloorRoomIds.Contains(r.RoomDetailsId ?? 0));

                        return new
                        {
                            RoomType = g.Key.RoomType,
                            TotalRooms = g.Count(),
                            Allocated = Math.Round((double)allocatedPax / g.Key.Pax, 2),
                            Balance = Math.Round(g.Count() - ((double)allocatedPax / g.Key.Pax), 2)
                        };
                    })
                    .ToList();

                return PartialView("_SummaryPartial", roomSummary);
            }
            else if (placeId == 1)
            {
                var poHotel = _dbContext.POHotels
                    .Include(p => p.POHotelRooms)
                        .ThenInclude(pr => pr.Room)
                    .Include(p => p.POHotelRooms)
                        .ThenInclude(pr => pr.RoomDetails)
                    .FirstOrDefault(p => p.POHotelsId == id);

                if (poHotel == null)
                {
                    return NotFound();
                }

                var floorRooms = poHotel.POHotelRooms
                    .SelectMany(pr => pr.RoomDetails.Select(fr => new
                    {
                        FloorRoomId = fr.RoomDetailsId,
                        RoomType = pr.Room.RoomName,
                        Pax = pr.Room.PAX,
                        RoomId = pr.Room.RoomId
                    }))
                    .ToList();

                var floorRoomIds = floorRooms.Select(fr => fr.FloorRoomId).ToList();

                var reservations = _dbContext.ItineraryBlockReservationForMakkah
                    .Where(r => floorRoomIds.Contains(r.RoomDetailsId ?? 0))
                    .ToList();

                var floorRoomMap = floorRooms.ToDictionary(fr => fr.FloorRoomId, fr => new { fr.RoomType, fr.RoomId, fr.Pax });

                var roomSummary = floorRooms
                    .GroupBy(fr => new { fr.RoomId, fr.RoomType, fr.Pax })
                    .Select(g =>
                    {
                        var relatedFloorRoomIds = g.Select(x => x.FloorRoomId).ToList();

                        var allocatedPax = reservations
                            .Count(r => relatedFloorRoomIds.Contains(r.RoomDetailsId ?? 0));

                        return new
                        {
                            RoomType = g.Key.RoomType,
                            TotalRooms = g.Count(),
                            Allocated = Math.Round((double)allocatedPax / g.Key.Pax, 2),
                            Balance = Math.Round(g.Count() - ((double)allocatedPax / g.Key.Pax), 2)
                        };
                    })
                    .ToList();

                return PartialView("_SummaryPartial", roomSummary);
            }
            else
            {
                return BadRequest("Invalid placeId."); // yahan error handle kar do
            }
        }
    }
}

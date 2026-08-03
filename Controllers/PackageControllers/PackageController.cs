using TMG.Data;
using TMG.Models.PackageModels;
using TMG.Models.FormModels.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.ManagerClasses;
using TMG.Models.LogisticModels;
using TMG.Models.SettingsModels;
using TMG.Models.FamilyModels;
using TMGHajj.Models.PackageModels;
using TMG.Models.AccomodationModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using System.Linq;
using TMG.Models.AirlineModels;
using iTextSharp.text;
using NuGet.Protocol.Plugins;
using Rotativa.AspNetCore;

namespace TMG.Models.PackageModels
{
    public record PackageIndexModel
    {
        public List<PackageIndexModelEntry> Entries { get; set; } = new List<PackageIndexModelEntry>();
        public int TotalQuota { get; set; } = 0;
        public int TotalBooked { get; set; } = 0;
        public int TotalBalance { get; set; } = 0;
    }

    public record PackageIndexModelEntry
    {
        public int PackageId { get; set; }
        public string Company { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Quota { get; set; }
        public int Booked { get; set; }
        public int Balance { get; set; }
        public bool? Status { get; set; }
    }
}

namespace TMG.Controllers.PackageControllers
{
    public class PackageController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public PackageController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Packages
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.Trashed && e.PackageStatus == PackageStatus.Booked)
                .Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashPackages(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreatePackages(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditPackages(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestorePackages(_dbContext, _http);
            ViewBag.ReportsAllowed = PrivilegeManager.CanViewReportPackages(_dbContext, _http);
            ViewBag.DetailsAllowed = PrivilegeManager.CanViewDetailsPackages(_dbContext, _http);
            ViewBag.CopyAllowed = PrivilegeManager.CanCopyPackages(_dbContext, _http);

            var userCompanyPackages = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName }
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
                        p.Code,
                        p.HajjSeasonsId,
                        p.Status
                    }
                ).Where(e => e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            var members = _dbContext.FamilyHeadsWithoutTrash
                .Select(e => new
                {
                    e.PackageId,
                });

            PackageIndexModel model = new PackageIndexModel();

            for (int i = 0; i < userCompanyPackages?.Count; i++)
            {
                var curr = userCompanyPackages.ElementAt(i);
                int booked = members.Where(e => e.PackageId == curr.PackageId).Count();
                int balance = curr.Quota - booked;

                model.TotalBalance += balance;
                model.TotalBooked += booked;
                model.TotalQuota += curr.Quota;

                model.Entries.Add(
                    new PackageIndexModelEntry
                    {
                        PackageId = curr.PackageId,
                        Company = curr.CompanyName,
                        Name = curr.Name,
                        Code = curr.Code,
                        Quota = curr.Quota,
                        Booked = booked,
                        Balance = balance,
                        Status = curr.Status
                    }
                );
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] StatusUpdateViewModel model)
        {
            if (model == null || model.PackageId <= 0)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            try
            {
                // Update the status in the database
                var entry = _dbContext.Packages.Find(model.PackageId);
                if (entry != null)
                {
                    entry.Status = model.Status;
                    _dbContext.SaveChanges();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Entry not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public IActionResult GetDescriptionsForRoom(int roomId)
        {
            var descriptions = _dbContext.IndividualFacilityDescriptions
                                        //.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                                        .Where(d => d.RoomId == roomId && d.Trashed == false)
                                        .ToList();

            return PartialView("_FacilityDescriptions", descriptions);
        }

        [HttpPost]
        public IActionResult GetDescriptionsEditForRoom(int roomId)
        {
            var facilities = _dbContext.IndividualFacilityDescriptions
                                     .Where(f => f.RoomId == roomId && f.Trashed == false)
                                     .ToList();

            return PartialView("_FacilityDescriptionsEdit", facilities);
        }

        public IActionResult Create()
        {
            ApplicationSettings settings = _dbContext.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .FirstOrDefault();
            ViewBag.SARROE = settings.SARROE;
            ViewBag.USDROE = settings.USDROE;
            ViewBag.TermsnCondition = _dbContext.PackageTermsWithoutTrash.Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            ViewBag.MakkahRooms = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 1)
                .ToList();

            ViewBag.Medinah = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 2)
                .ToList();

            ViewBag.Azizia = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 4)
                .ToList();

            ViewBag.Mina = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 3)
                .ToList();

            ViewBag.FacilityDescriptions = _dbContext.IndividualFacilityDescriptionsWithoutTrash;
            ViewBag.SPOs = _dbContext.SPOsWithoutTrash;
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Companies = _dbContext
                .UserCompanies
                .Include(e => e.Company)
                .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);
            ViewBag.Accomodations = _dbContext.AccomodationsWithoutTrash;
            ViewBag.AccomodationCategories = _dbContext.AccomodationCategoriesWithoutTrash;
            ViewBag.TravelRoutes = _dbContext.TravelRoutesWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;

            ViewBag.TrainingSessionsCount = _dbContext.TrainingsWithoutTrash.Count();
            ViewBag.GiftsCount = _dbContext.GiftsWithoutTrash.Count();
            ViewBag.PaymentNotesCount = _dbContext.PaymentNotesWithoutTrash.Count();

            ViewBag.TrainingSessions = _dbContext.TrainingsWithoutTrash;
            ViewBag.Gifts = _dbContext.GiftsWithoutTrash;
            ViewBag.PaymentNotes = _dbContext.PaymentNotesWithoutTrash;
            ViewBag.RoomName = _dbContext
                .Rooms
                //.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(r => new { r.RoomId, r.RoomName })
                .ToList();
            ViewBag.IndividualFacilityDescriptions = _dbContext.IndividualFacilityDescriptions.ToList();

            var TravelRouteStop = _dbContext.TravelRouteStops
                    .Include(e => e.Route)
                    .Include(e => e.Vehicle)
                    .OrderBy(e => e.CheckIn)
                    .Select(e => new
                    {
                        e.TravelRouteId,
                        e.Route.StartPlace,
                        e.Route.Destination,
                        CheckIn = e.CheckIn.ToString("dd/MMM/yyyy HH:mm"),
                        CheckOut = e.CheckOut.ToString("dd/MMM/yyyy HH:mm"),
                        TransportType = e.TransportType.ToString(),
                        e.Vehicle.VehicleType,
                        //e.Price
                    })
                    .ToList();

            ViewBag.Transportation = TravelRouteStop;

            ViewBag.TransportTypes = Enum.GetValues(typeof(FamilyTranasport))
                .Cast<FamilyTranasport>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;

            ViewBag.FlightType = Enum.GetValues(typeof(FlightType))
                .Cast<FlightType>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                .Cast<FlightClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.IssuingAgent = _dbContext.IssuingAgents.Where(e => !e.Trashed).Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

            ViewBag.Currency = _dbContext.Currencies
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.Railway = _dbContext.RailwaysWithoutTrash;

            ViewBag.TrainClass = Enum.GetValues(typeof(TrainClass))
                .Cast<TrainClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PackageRegisterRequest request)
        {
            ApplicationSettings settings = _dbContext.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))   
                .FirstOrDefault();
            ViewBag.SARROE = settings.SARROE;
            ViewBag.USDROE = settings.USDROE;

            ViewBag.FacilityDescriptions = _dbContext.IndividualFacilityDescriptionsWithoutTrash;
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Companies = _dbContext
                .UserCompanies
                .Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);
            ViewBag.Accomodations = _dbContext.AccomodationsWithoutTrash;
            ViewBag.AccomodationCategories = _dbContext.AccomodationCategoriesWithoutTrash;
            ViewBag.TravelRoutes = _dbContext.TravelRoutesWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;

            ViewBag.TrainingSessionsCount = _dbContext.TrainingsWithoutTrash.Count();
            ViewBag.GiftsCount = _dbContext.GiftsWithoutTrash.Count();
            ViewBag.PaymentNotesCount = _dbContext.PaymentNotesWithoutTrash.Count();

            ViewBag.TrainingSessions = _dbContext.TrainingsWithoutTrash;
            ViewBag.Gifts = _dbContext.GiftsWithoutTrash;
            ViewBag.PaymentNotes = _dbContext.PaymentNotesWithoutTrash;
            ViewBag.RoomName = _dbContext
                .Rooms
                .Select(r => new { r.RoomId, r.RoomName })
                .ToList();
            ViewBag.IndividualFacilityDescriptions = _dbContext.IndividualFacilityDescriptions.ToList();

            #region Accommodation Validations

            /*bool allPlaceSelected = true;
            for (int i = 0; i < request.AccomodationPlaceId.Count; i++)
            {
                if (request.AccomodationPlaceId[i] == null)
                {
                    allPlaceSelected = false;
                    break;
                }
            }

            if (!allPlaceSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationTypeSelected = true;
            for (int i = 0; i < request.AccomodationId.Count; i++)
            {
                if (request.AccomodationId[i] == null)
                {
                    allAccomodationTypeSelected = false;
                    break;
                }
            }

            if (!allAccomodationTypeSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationCategorySelected = true;
            for (int i = 0; i < request.AccomodationCategoryId.Count; i++)
            {
                if (request.AccomodationCategoryId[i] == null)
                {
                    allAccomodationCategorySelected = false;
                    break;
                }
            }

            if (!allAccomodationCategorySelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationSelected = true;
            for (int i = 0; i < request.Accomodation.Count; i++)
            {
                if (request.Accomodation[i] == null)
                {
                    allAccomodationSelected = false;
                    break;
                }
            }

            if (!allAccomodationSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationDaysSelected = true;
            for (int i = 0; i < request.AccomodationDays.Count; i++)
            {
                if (request.AccomodationDays[i] == null)
                {
                    allAccomodationDaysSelected = false;
                    break;
                }
            }

            if (!allAccomodationDaysSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationNightsSelected = true;
            for (int i = 0; i < request.AccomodationNights.Count; i++)
            {
                if (request.AccomodationNights[i] == null)
                {
                    allAccomodationNightsSelected = false;
                    break;
                }
            }

            if (!allAccomodationNightsSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationCheckInSelected = true;
            for (int i = 0; i < request.AccomodationCheckIn.Count; i++)
            {
                if (request.AccomodationCheckIn[i] == null)
                {
                    allAccomodationCheckInSelected = false;
                    break;
                }
            }

            if (!allAccomodationCheckInSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationCheckOutSelected = true;
            for (int i = 0; i < request.AccomodationCheckOut.Count; i++)
            {
                if (request.AccomodationCheckOut[i] == null)
                {
                    allAccomodationCheckOutSelected = false;
                    break;
                }
            }

            if (!allAccomodationCheckOutSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationFoodPackageSelected = true;
            for (int i = 0; i < request.AccomodationFoodPackageId.Count; i++)
            {
                if (request.AccomodationFoodPackageId[i] == null)
                {
                    allAccomodationFoodPackageSelected = false;
                    break;
                }
            }

            if (!allAccomodationFoodPackageSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }*/

            if (request.AccomodationPlaceId != null && request.AccomodationPlaceId.Count > 0)
            {
                bool allPlaceSelected = true;
                for (int i = 0; i < request.AccomodationPlaceId.Count; i++)
                {
                    if (request.AccomodationPlaceId[i] == null)
                    {
                        allPlaceSelected = false;
                        break;
                    }
                }

                if (!allPlaceSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.AccomodationId != null && request.AccomodationId.Count > 0)
            {
                bool allAccomodationTypeSelected = true;
                for (int i = 0; i < request.AccomodationId.Count; i++)
                {
                    if (request.AccomodationId[i] == null)
                    {
                        allAccomodationTypeSelected = false;
                        break;
                    }
                }

                if (!allAccomodationTypeSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.AccomodationCategoryId != null && request.AccomodationCategoryId.Count > 0)
            {
                bool allAccomodationCategorySelected = true;
                for (int i = 0; i < request.AccomodationCategoryId.Count; i++)
                {
                    if (request.AccomodationCategoryId[i] == null)
                    {
                        allAccomodationCategorySelected = false;
                        break;
                    }
                }

                if (!allAccomodationCategorySelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.Accomodation != null && request.Accomodation.Count > 0)
            {
                bool allAccomodationSelected = true;
                for (int i = 0; i < request.Accomodation.Count; i++)
                {
                    if (request.Accomodation[i] == null)
                    {
                        allAccomodationSelected = false;
                        break;
                    }
                }

                if (!allAccomodationSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.AccomodationDays != null && request.AccomodationDays.Count > 0)
            {
                bool allAccomodationDaysSelected = true;
                for (int i = 0; i < request.AccomodationDays.Count; i++)
                {
                    if (request.AccomodationDays[i] == null)
                    {
                        allAccomodationDaysSelected = false;
                        break;
                    }
                }

                if (!allAccomodationDaysSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.AccomodationNights != null && request.AccomodationNights.Count > 0)
            {
                bool allAccomodationNightsSelected = true;
                for (int i = 0; i < request.AccomodationNights.Count; i++)
                {
                    if (request.AccomodationNights[i] == null)
                    {
                        allAccomodationNightsSelected = false;
                        break;
                    }
                }

                if (!allAccomodationNightsSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.AccomodationCheckIn != null && request.AccomodationCheckIn.Count > 0)
            {
                bool allAccomodationCheckInSelected = true;
                for (int i = 0; i < request.AccomodationCheckIn.Count; i++)
                {
                    if (request.AccomodationCheckIn[i] == null)
                    {
                        allAccomodationCheckInSelected = false;
                        break;
                    }
                }

                if (!allAccomodationCheckInSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.AccomodationCheckOut != null && request.AccomodationCheckOut.Count > 0)
            {
                bool allAccomodationCheckOutSelected = true;
                for (int i = 0; i < request.AccomodationCheckOut.Count; i++)
                {
                    if (request.AccomodationCheckOut[i] == null)
                    {
                        allAccomodationCheckOutSelected = false;
                        break;
                    }
                }

                if (!allAccomodationCheckOutSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }

            if (request.AccomodationFoodPackageId != null && request.AccomodationFoodPackageId.Count > 0)
            {
                bool allAccomodationFoodPackageSelected = true;
                for (int i = 0; i < request.AccomodationFoodPackageId.Count; i++)
                {
                    if (request.AccomodationFoodPackageId[i] == null)
                    {
                        allAccomodationFoodPackageSelected = false;
                        break;
                    }
                }

                if (!allAccomodationFoodPackageSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }
            }


            /*bool allAccomodationLoadPerRoomSelected = true;
            for (int i = 0; i < request.AccomodationLoadPerRoomBeds.Count; i++)
            {
                if (request.AccomodationLoadPerRoomBeds[i] == null)
                {
                    allAccomodationLoadPerRoomSelected = false;
                    break;
                }
            }

            if (!allAccomodationLoadPerRoomSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationLoadPerWashroomSelected = true;
            for (int i = 0; i < request.AccomodationLoadPerWashroomPersons.Count; i++)
            {
                if (request.AccomodationLoadPerWashroomPersons[i] == null)
                {
                    allAccomodationLoadPerWashroomSelected = false;
                    break;
                }
            }*/

            /*if (!allAccomodationLoadPerWashroomSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }*/

            #endregion

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                Package package = new Package
                {
                    CompanyId = request.CompanyId,
                    PackageNumber = request.PackageNumber ?? 0,
                    Code = request.PackageCode,
                    Name = request.PackageName,
                    Quota = request.PackageQouta,
                    HajjDues = request.CompulsoryHajjDues,
                    HajjDuration = request.HajjDuration,
                    MedinahArrival = request.MedinaArrival,
                    Days = request.Days,
                    FoodIncluded = false,
                    //QurbaniIncluded = request.PackageQurbaniIncluded,
                    TravelRouteId = request.TravelRouteId,
                    SARROE = 0,
                    PKRROE = request.PKRROE,
                    USDROE = request.USDROE,
                    GBPROE = request.GBPROE,
                    EUROROE = request.EUROROE,
                    AEDROE = request.AEDROE,
                    AdultPkr = request.AdultPkr,
                    AdultSar = request.AdultSar,
                    AdultUsd = request.AdultUsd,
                    AdultGbp = request.AdultGbp,
                    AdultEuro = request.AdultEuro,
                    AdultAed = request.AdultAed,
                    RoomId = request.RoomIds,
                    RoomTypesId = request.RoomTypesId,
                    NearBy = request.NearBy,
                    TermsnCondition = request.TermsnCondition,
                    Content = request.Description,

                    FlightType = request.FlightType,
                    FlightClass = request.FlightClass,
                    POD = request.POD,
                    TransportIntercity = request.TransportIntercity,
                    WithInCity = request.WithInCity,
                    AirportTransport = request.AirportTransfer,
                    TransportWithMashair = request.TransportWithMashair,

                    PackageStatus = PackageStatus.Booked,
                    PackageState = PackageState.Orignal,

                    CategoryZone = request.PackageCategoryZone,

                    FlightTicketPricePkr = request.TicketPricePkr,
                    FlightTicketPriceSar = request.TicketPriceSar,
                    FlightTicketPriceUsd = request.TicketPriceUsd,

                    Maktab = request.Maktab,
                    MaktabNumber = request.MaktabNumber,
                    MaktabAddress = request.MaktabAddress,
                    OfficeAddress = request.OfficeAddress,
                    FlightTicketPeriod = request.TicketPeriod,
                    Color = request.Color,

                    HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                };
                /*if (request.FileImage is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageImages", request.FileImage.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.FileImage.CopyTo(fs);
                    }
                    package.Image = ApplicationManager.PackageImagesPath + request.FileImage.FileName;

                }
                if (request.MINA is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MINA.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MINA.CopyTo(fs);
                    }
                    package.MINA = ApplicationManager.PackageItineraryPath + request.MINA.FileName;
                }
                if (request.ARAFAT is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.ARAFAT.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.ARAFAT.CopyTo(fs);
                    }
                    package.ARAFAT = ApplicationManager.PackageItineraryPath + request.ARAFAT.FileName;
                }
                if (request.MUZDALIFAH is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MUZDALIFAH.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MUZDALIFAH.CopyTo(fs);
                    }
                    package.MUZDALIFAH = ApplicationManager.PackageItineraryPath + request.MUZDALIFAH.FileName;
                }
                if (request.RamiDayOne is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayOne.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayOne.CopyTo(fs);
                    }
                    package.RamiDayOne = ApplicationManager.PackageItineraryPath + request.RamiDayOne.FileName;
                }
                if (request.RamiDayTwo is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayTwo.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayTwo.CopyTo(fs);
                    }
                    package.RamiDayTwo = ApplicationManager.PackageItineraryPath + request.RamiDayTwo.FileName;
                }
                if (request.RamiDayThree is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayThree.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayThree.CopyTo(fs);
                    }
                    package.RamiDayThree = ApplicationManager.PackageItineraryPath + request.RamiDayThree.FileName;
                }*/

                if (request.FileImage != null && !string.IsNullOrWhiteSpace(request.FileImage.FileName))
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageImages", request.FileImage.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.FileImage.CopyTo(fs);
                    }

                    package.Image = ApplicationManager.PackageImagesPath + request.FileImage.FileName;
                }

                if (request.MINA != null && !string.IsNullOrWhiteSpace(request.MINA.FileName))
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MINA.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MINA.CopyTo(fs);
                    }

                    package.MINA = ApplicationManager.PackageItineraryPath + request.MINA.FileName;
                }

                if (request.ARAFAT != null && !string.IsNullOrWhiteSpace(request.ARAFAT.FileName))
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.ARAFAT.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.ARAFAT.CopyTo(fs);
                    }

                    package.ARAFAT = ApplicationManager.PackageItineraryPath + request.ARAFAT.FileName;
                }

                if (request.MUZDALIFAH != null && !string.IsNullOrWhiteSpace(request.MUZDALIFAH.FileName))
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MUZDALIFAH.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MUZDALIFAH.CopyTo(fs);
                    }

                    package.MUZDALIFAH = ApplicationManager.PackageItineraryPath + request.MUZDALIFAH.FileName;
                }

                if (request.RamiDayOne != null && !string.IsNullOrWhiteSpace(request.RamiDayOne.FileName))
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayOne.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayOne.CopyTo(fs);
                    }

                    package.RamiDayOne = ApplicationManager.PackageItineraryPath + request.RamiDayOne.FileName;
                }

                if (request.RamiDayTwo != null && !string.IsNullOrWhiteSpace(request.RamiDayTwo.FileName))
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayTwo.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayTwo.CopyTo(fs);
                    }

                    package.RamiDayTwo = ApplicationManager.PackageItineraryPath + request.RamiDayTwo.FileName;
                }

                if (request.RamiDayThree != null && !string.IsNullOrWhiteSpace(request.RamiDayThree.FileName))
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayThree.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayThree.CopyTo(fs);
                    }

                    package.RamiDayThree = ApplicationManager.PackageItineraryPath + request.RamiDayThree.FileName;
                }

                _dbContext.Packages.Add(package);
                _dbContext.SaveChanges();

                /*for (int i = 0; i < request.AirlineIds?.Count; i++)
                {
                    PackageFlights packageFlights = new PackageFlights
                    {
                        PackageId = package.PackageId,
                        AirlineId = request.AirlineIds[i],
                        DepartureDate = request.DepartureDate[i],
                        DepartureTime = request.DepartureTime[i],
                        ArrivalDate = request.ArrivalDate[i],
                        ArrivalTime = request.ArrivalTime[i],
                        PNRNumber = request.PNRNos[i],
                        FlightClass = request.FlightClasses[i],
                        FlightType = request.FlightTypes[i],
                        Origin = request.Origins[i],
                        Destination = request.Destinations[i],
                        FlightNumber = request.FlightNos[i],
                        TicketAmount = request.TicketAmount[i],
                        IsPreffered = request.IsPreffereds[i]
                    };

                    _dbContext.PackageFlights.Add(packageFlights);
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < request.RailwayIds?.Count; i++)
                {
                    PackageTrains packageTrains = new PackageTrains
                    {
                        PackageId = package.PackageId,
                        RailwayId = request.RailwayIds[i],
                        DepartureDate = request.TrainDepartureDate[i],
                        DepartureTime = request.TrainDepartureTime[i],
                        ArrivalDate = request.TrainArrivalDate[i],
                        ArrivalTime = request.TrainArrivalTime[i],
                        PNRNumber = request.TrainPNRNos[i],
                        TrainClass = request.TrainClasses[i],
                        Origin = request.TrainOrigins[i],
                        Destination = request.TrainDestinations[i],
                        TicketAmount = request.TrainTicketAmount[i],
                        TrainNumber = request.TrainNos[i]
                    };

                    _dbContext.PackageTrains.Add(packageTrains);
                }

                _dbContext.SaveChanges();


                //for (int i = 0; i < request.Description?.Count; i++)
                //{

                //}

                //var newitinerary = new PackageItinerary
                //{
                //    PackageId = package.PackageId,
                //    Description = request.Description,
                //};

                //_dbContext.PackageItinerary.Add(newitinerary);
                //_dbContext.SaveChanges();


                for (int i = 0; i < request.PackageFacilitiesIds?.Count; i++)
                {
                    var facilityId = request.PackageFacilitiesIds[i];

                    var packageCategoryFacility = new PackageCategoryFacilities
                    {
                        PackageId = package.PackageId,
                        IndividualFacilityDescriptionId = facilityId
                    };

                    _dbContext.PackageCategoryFacilities.Add(packageCategoryFacility);
                }


                _dbContext.SaveChanges();
                for (int i = 0; i < request.Terms?.Count; i++)
                {
                    var termText = request.Terms[i];


                    var newTerm = new PackageTermsConditions
                    {
                        PackageId = package.PackageId,
                        Term = termText
                    };
                    _dbContext.PackageTermsConditions.Add(newTerm);

                }

                _dbContext.SaveChanges();*/


                if (request.AirlineIds != null && request.AirlineIds.Count > 0)
                {
                    for (int i = 0; i < request.AirlineIds.Count; i++)
                    {
                        PackageFlights packageFlights = new PackageFlights
                        {
                            PackageId = package.PackageId,
                            AirlineId = request.AirlineIds[i],
                            DepartureDate = request.DepartureDate[i],
                            DepartureTime = request.DepartureTime[i],
                            ArrivalDate = request.ArrivalDate[i],
                            ArrivalTime = request.ArrivalTime[i],
                            PNRNumber = request.PNRNos[i],
                            FlightClass = request.FlightClasses[i],
                            FlightType = request.FlightTypes[i],
                            Origin = request.Origins[i],
                            Destination = request.Destinations[i],
                            FlightNumber = request.FlightNos[i],
                            TicketAmount = request.TicketAmount[i],
                            IsPreffered = request.IsPreffereds[i]
                        };

                        _dbContext.PackageFlights.Add(packageFlights);
                    }

                    _dbContext.SaveChanges();
                }

                if (request.RailwayIds != null && request.RailwayIds.Count > 0)
                {
                    for (int i = 0; i < request.RailwayIds.Count; i++)
                    {
                        PackageTrains packageTrains = new PackageTrains
                        {
                            PackageId = package.PackageId,
                            RailwayId = request.RailwayIds[i],
                            DepartureDate = request.TrainDepartureDate[i],
                            DepartureTime = request.TrainDepartureTime[i],
                            ArrivalDate = request.TrainArrivalDate[i],
                            ArrivalTime = request.TrainArrivalTime[i],
                            PNRNumber = request.TrainPNRNos[i],
                            TrainClass = request.TrainClasses[i],
                            Origin = request.TrainOrigins[i],
                            Destination = request.TrainDestinations[i],
                            TicketAmount = request.TrainTicketAmount[i],
                            TrainNumber = request.TrainNos[i]
                        };

                        _dbContext.PackageTrains.Add(packageTrains);
                    }

                    _dbContext.SaveChanges();
                }

                // Itinerary (if Description is a List)
                /*if (request.Description != null && request.Description.Count > 0)
                {
                    for (int i = 0; i < request.Description.Count; i++)
                    {
                        var newItinerary = new PackageItinerary
                        {
                            PackageId = package.PackageId,
                            Description = request.Description[i]
                        };

                        _dbContext.PackageItinerary.Add(newItinerary);
                    }

                    _dbContext.SaveChanges();
                }*/

                // Package Facilities
                if (request.PackageFacilitiesIds != null && request.PackageFacilitiesIds.Count > 0)
                {
                    for (int i = 0; i < request.PackageFacilitiesIds.Count; i++)
                    {
                        var facilityId = request.PackageFacilitiesIds[i];

                        var packageCategoryFacility = new PackageCategoryFacilities
                        {
                            PackageId = package.PackageId,
                            IndividualFacilityDescriptionId = facilityId
                        };

                        _dbContext.PackageCategoryFacilities.Add(packageCategoryFacility);
                    }

                    _dbContext.SaveChanges();
                }

                // Terms
                if (request.Terms != null && request.Terms.Count > 0)
                {
                    for (int i = 0; i < request.Terms.Count; i++)
                    {
                        var termText = request.Terms[i];

                        var newTerm = new PackageTermsConditions
                        {
                            PackageId = package.PackageId,
                            Term = termText
                        };

                        _dbContext.PackageTermsConditions.Add(newTerm);
                    }

                    _dbContext.SaveChanges();
                }


                /*for (int i = 0; i < request.Accomodation?.Count; i++)
                {
                    _dbContext.PackageAccomodtions.Add(
                        new PackageAccomodations
                        {
                            PackageId = package.PackageId,
                            PlaceId = (int)request.AccomodationPlaceId[i],
                            AccomodationId = (int)request.AccomodationId[i],
                            AccomodationCategoryId = (int)request.AccomodationCategoryId[i],
                            AccomodationName = request.Accomodation[i],
                            DistanceInMeters = request.AccomodationDistance[i],
                            Days = request.AccomodationDays[i],
                            Nights = request.AccomodationNights[i],
                            CheckIn = request.AccomodationCheckIn[i],
                            CheckOut = request.AccomodationCheckOut[i],
                            FoodIncluded = request.AccomodationFoodIncluded[i],
                            FoodPackageId = request.AccomodationFoodPackageId[i],
                            ResidenceIncluded = request.AccomodationResidenceIncluded[i],
                            ActualAccommodation = "",
                            HotelId = request.HotelIds[i],
                            ActualDays = 0,
                            ActualNights = 0,
                            ActualCheckIn = "",
                            ActualCheckOut = "",
                            ActualCheckInTime = request.AccomodationActualCheckInTime[i],
                            ActualCheckOutTime = request.AccomodationActualCheckOutTime[i],
                            *//*LoadPerRoom = (int)request.AccomodationLoadPerRoomBeds[i],
                            LoadPerWashroom = (int)request.AccomodationLoadPerWashroomPersons[i],*//*
                            RoomId = (int)request.AccomodationActualLoadPerRoom[i],
                            GroupZiarat = request.AccomodationGroupZiarat[i],
                            ReligiousLectures = request.AccomodationReligiousLecture[i],
                            DistributionArrangements = request.AccomodationDistribution[i],
                            Camp = request.AccomodationCamp[i],
                            Arafat = request.AccomodationArafat[i],
                            Shuttle = request.AccomodationShuttle[i],
                            Bedding = request.AccomodationBedding[i],
                            Sharing = request.AccomodationSharing[i],
                            SharingType = request.AccomodationSharingType[i],
                            Notes = request.AccomodationNote[i]
                        }
                    );
                }*/

                for (int i = 0; i < request.Accomodation?.Count; i++)
                {
                    _dbContext.PackageAccomodtions.Add(
                        new PackageAccomodations
                        {
                            PackageId = package.PackageId,
                            PlaceId = request.AccomodationPlaceId?[i] ?? 0,
                            AccomodationId = request.AccomodationId?[i] ?? 0,
                            AccomodationCategoryId = request.AccomodationCategoryId?[i] ?? 0,
                            AccomodationName = request.Accomodation[i],
                            DistanceInMeters = request.AccomodationDistance?[i],
                            Days = request.AccomodationDays?[i],
                            Nights = request.AccomodationNights?[i],
                            CheckIn = request.AccomodationCheckIn?[i],
                            CheckOut = request.AccomodationCheckOut?[i],
                            FoodIncluded = false,
                            FoodPackageId = request.AccomodationFoodPackageId?[i],
                            ResidenceIncluded = false,
                            ActualAccommodation = "",
                            HotelId = request.HotelIds?[i],
                            ActualDays = 0,
                            ActualNights = 0,
                            ActualCheckIn = "",
                            ActualCheckOut = "",
                            ActualCheckInTime = request.AccomodationActualCheckInTime?[i],
                            ActualCheckOutTime = request.AccomodationActualCheckOutTime?[i],
                            //RoomId = request.AccomodationActualLoadPerRoom?[i] ?? 0, 
                            GroupZiarat = request.AccomodationGroupZiarat?[i],
                            ReligiousLectures = request.AccomodationReligiousLecture?[i],
                            DistributionArrangements = request.AccomodationDistribution?[i],
                            Camp = request.AccomodationCamp?[i],
                            Arafat = request.AccomodationArafat?[i],
                            Shuttle = request.AccomodationShuttle?[i],
                            Bedding = request.AccomodationBedding?[i],
                            Sharing = request.AccomodationSharing?[i],
                            SharingType = request.AccomodationSharingType?[i],
                            Notes = request.AccomodationNote?[i]
                        }
                    );
                }
                _dbContext.SaveChanges();

                if (request.GiftId is not null)
                {
                    for (int i = 0; i < request.GiftId?.Count; i++)
                    {
                        if (request.GiftSelected[i] == false) continue;

                        _dbContext.PackageGifts.Add(
                            new PackageGifts { PackageId = package.PackageId, GiftId = request.GiftId[i] }
                            );
                    }
                    _dbContext.SaveChanges();
                }

                if (request.TrainingIds is not null)
                {
                    for (int i = 0; i < request.TrainingIds.Count; i++)
                    {
                        if (request.TrainingSelected[i] == false) continue;
                        _dbContext.PackageTrainings.Add(
                            new PackageTrainings
                            {
                                PackageId = package.PackageId,
                                TrainingId = request.TrainingIds[i]
                            }
                            );
                    }

                    _dbContext.SaveChanges();
                }


                transaction.Commit();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
                //return Ok(request);
                return View(request);
            }

        }

        public IActionResult Trash(int id)
        {
            Package packageToDelete = _dbContext.Packages.Find(id);
            if (packageToDelete == null) return RedirectToAction(nameof(Index));

            packageToDelete.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Packages.Include(e => e.Company).Where(s => s.Trashed && s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Restore(int id)
        {
            Package package = _dbContext.Packages.Find(id);
            package.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Detail(int id)
        {
            Package package = _dbContext.Packages
                .Include(e => e.TravelRoute)
                .Include(e => e.Company)
                .Include(e => e.Room)
                .Include(e => e.hajjSeasons)
                .FirstOrDefault(e => e.PackageId == id);
            if (package == null)
            {
                // Redirect back to the list so the app doesn't show a white error screen
                return RedirectToAction("Index");
            }
            ViewBag.Routes = _dbContext.TravelRouteStops
                            .Include(e => e.Vehicle)
                            .Include(e => e.Route)
                            .Where(e => e.TravelRouteId == package.TravelRouteId)
                            .OrderBy(e => e.CheckIn);

            var categoryFacilities = _dbContext.PackageCategoryFacilities
                .Include(e => e.IndividualFacilityDescription)
                .ThenInclude(e => e.Place)
                .Where(e => e.PackageId == id);

            string facilityDescriptions = string.Join("<br/>", categoryFacilities
                .Select(e => $"{e.IndividualFacilityDescription.Description} - {e.IndividualFacilityDescription.Place.PlaceName}"));

            ViewBag.Rooms = facilityDescriptions;

            ViewBag.AssignedAccommodations = _dbContext.PackageAccomodtions
                .Include(e => e.Place)
                .Include(e => e.Accomodation)
                .Include(e => e.AccomodationCategory)
                .Include(e => e.FoodPackage)
                .Include(e => e.Hotel)
                .Where(e => e.PackageId == id);

            var flights = _dbContext.PackageFlights
                                .Include(e => e.Airline)
                                .OrderBy(e => e.DepartureDate)
                                .Where(e => e.PackageId == id).ToList();

            ViewBag.Flights = flights;

            var trains = _dbContext.PackageTrains
                                .Include(e => e.Railway)
                                .OrderBy(e => e.DepartureDate)
                                .Where(e => e.PackageId == id).ToList();

            ViewBag.Trains = trains;

            ViewBag.Terms = _dbContext.PackageTermsConditions
                                .Where(e => e.PackageId == id);


            ViewBag.ItineraryInfo = _dbContext.PackageItinerary
                                .Where(e => e.PackageId == id);

            ViewBag.AssignedGifts = _dbContext.PackageGifts.Include(e => e.Gift).Where(e => e.PackageId == id);

            ViewBag.Training = _dbContext.PackageTrainings
                .Include(e => e.Training)
                .Where(e => e.PackageId == id);

            float amount = 0;
            amount += package.AdultSar;
            amount += trains.Sum(e => e.TicketAmount) ?? 0;
            amount += flights.Sum(e => e.TicketAmount) ?? 0;
            //amount += package.IndividualFacilityDescription?.Price ?? 0f;
            amount += categoryFacilities.Sum(e => e.IndividualFacilityDescription.Price);

            ViewBag.PackageAmount = amount.ToString("N0");

            return View(package);
        }


        public IActionResult Edit(int id)
        {
            ViewBag.SelectedIADFRoom = _dbContext.PackageCategoryFacilities
                .Where(e => e.PackageId == id)
                .Select(e => e.IndividualFacilityDescriptionId)
                .ToList();

            ApplicationSettings settings = _dbContext.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .FirstOrDefault();
            ViewBag.SARROE = settings.SARROE;
            ViewBag.USDROE = settings.USDROE;
            ViewBag.PackageTerms = _dbContext.PackageTermsConditions
                .Where(e => e.PackageId == id)
                .ToList();
            ViewBag.PackageItinerary = _dbContext.PackageItinerary
                .Where(e => e.PackageId == id)
                .ToList();
            ViewBag.PackageImages = _dbContext.Packages
                .Where(e => e.PackageId == id)
                .ToList();

            ViewBag.MakkahRooms = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 1)
                .ToList();

            ViewBag.Medinah = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 2)
                .ToList();

            ViewBag.Azizia = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 4)
                .ToList();

            ViewBag.Mina = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 3)
                .ToList();

            ViewBag.FacilityDescriptions = _dbContext.IndividualFacilityDescriptionsWithoutTrash;
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Companies = _dbContext
                .UserCompanies
                .Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);
            ViewBag.Accomodations = _dbContext.AccomodationsWithoutTrash;
            ViewBag.AccomodationCategories = _dbContext.AccomodationCategoriesWithoutTrash;
            ViewBag.TravelRoutes = _dbContext.TravelRoutesWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;

            ViewBag.TrainingSessionsCount = _dbContext.TrainingsWithoutTrash.Count();
            ViewBag.GiftsCount = _dbContext.GiftsWithoutTrash.Count();
            ViewBag.PaymentNotesCount = _dbContext.PaymentNotesWithoutTrash.Count();

            ViewBag.TrainingSessions = _dbContext.TrainingsWithoutTrash;
            ViewBag.Gifts = _dbContext.GiftsWithoutTrash;
            ViewBag.PaymentNotes = _dbContext.PaymentNotesWithoutTrash;

            ViewBag.AssignedPackageAddons = _dbContext.PackageAddons.Where(e => e.PackageId == id);
            ViewBag.AssignedAccommodations = _dbContext.PackageAccomodtions.Where(e => e.PackageId == id);
            ViewBag.AssignedAddFac = _dbContext.AdditionalFacilities.Include(e => e.IndividiualFacilityDescription)
                .Where(e => e.PackageId == id);
            ViewBag.AssignedGifts = _dbContext.PackageGifts.Where(e => e.PackageId == id);
            ViewBag.AssignedTrainings = _dbContext.PackageTrainings.Where(e => e.PackageId == id);
            ViewBag.AssignedNotes = _dbContext.PackagePaymentNotes.Where(e => e.PackageId == id);
            ViewBag.RoomName = _dbContext
                .Rooms
                .Select(r => new { r.RoomId, r.RoomName })
                .ToList();
            ViewBag.AssignedFacility = _dbContext.PackageCategoryFacilities.Where(e => e.PackageId == id);

            ViewBag.SelectedRoomId = _dbContext.Rooms
                        .Where(r => r.RoomId == id)
                        .FirstOrDefault();


            Package package = _dbContext.Packages.Find(id);
            ViewBag.Facilities = _dbContext.IndividualFacilityDescriptions.Where(e => e.RoomId == package.RoomId && !e.Trashed);

            ViewBag.FlightDetails = _dbContext.PackageFlights
                .Where(e => e.PackageId == id).ToList();

            ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                .Cast<FlightClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.FlightType = Enum.GetValues(typeof(FlightType))
                .Cast<FlightType>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

            ViewBag.TrainDetails = _dbContext.PackageTrains
                .Where(e => e.PackageId == id).ToList();

            ViewBag.Railway = _dbContext.RailwaysWithoutTrash;

            ViewBag.TrainClass = Enum.GetValues(typeof(TrainClass))
                .Cast<TrainClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            var TravelRouteStop = _dbContext.TravelRouteStops
                    .Include(e => e.Route)
                    .Include(e => e.Vehicle)
                    .OrderBy(e => e.CheckIn)
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                    .Select(e => new
                    {
                        e.TravelRouteId,
                        e.Route.StartPlace,
                        e.Route.Destination,
                        CheckIn = e.CheckIn.ToString("dd/MMM/yyyy HH:mm"),
                        CheckOut = e.CheckOut.ToString("dd/MMM/yyyy HH:mm"),
                        TransportType = e.TransportType.ToString(),
                        e.Vehicle.VehicleType,
                        //e.Price
                    })
                    .ToList();

            ViewBag.Transportation = TravelRouteStop;

            ViewBag.Camps = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.Place.PlaceId == 3)
                .ToList();


            PackageEditRequest request = new PackageEditRequest
            {

                PackageId = id,
                CompanyId = package.CompanyId,
                TravelRouteId = package.TravelRouteId,
                PackageCode = package.Code,
                PackageNumber = package.PackageNumber,
                PackageName = package.Name,
                PackageQouta = package.Quota,
                Days = package.Days,
                MedinaArrival = package.MedinahArrival,
                HajjDuration = package.HajjDuration,
                PackageFoodIncluded = package.FoodIncluded,
                //PackageQurbaniIncluded = package.QurbaniIncluded,
                SARROE = package.SARROE,
                PKRROE = package.PKRROE,
                USDROE = package.USDROE,
                GBPROE = package.GBPROE,
                EUROROE = package.EUROROE,
                AEDROE = package.AEDROE,
                AdultPkr = package.AdultPkr,
                AdultSar = package.AdultSar,
                AdultUsd = package.AdultUsd,
                AdultGbp = package.AdultGbp,
                AdultEuro = package.AdultEuro,
                AdultAed = package.AdultAed,
                ChildPkr = package.ChildPkr,
                ChildSar = package.ChildSar,
                ChildUsd = package.ChildUsd,
                ChildGbp = package.ChildGbp,
                ChildEuro = package.ChildEuro,
                ChildAed = package.ChildAed,
                InfantPkr = package.InfantPkr,
                InfantSar = package.InfantSar,
                InfantUsd = package.InfantUsd,
                InfantGbp = package.InfantGbp,
                InfantEuro = package.InfantEuro,
                InfantAed = package.InfantAed,
                RoomIds = package.RoomId,
                RoomTypesId = package.RoomTypesId,
                //Camps = package.IndividualFacilityDescriptionId,
                NearBy = package.NearBy,
                TermsnCondition = package.TermsnCondition,
                Description = package.Content,
                Color = package.Color,

                FlightType = package.FlightType,
                FlightClass = package.FlightClass,
                TransportIntercity = package.TransportIntercity,
                WithInCity = package.WithInCity,
                TransportWithMashair = package.TransportWithMashair,
                AirportTransfer = package.AirportTransport,
                POD = package.POD,

                PackageCategoryZone = package.CategoryZone,

                Maktab = package.Maktab,
                MaktabNumber = package.MaktabNumber,
                MaktabAddress = package.MaktabAddress,
                OfficeAddress= package.OfficeAddress,
                TicketPricePkr = package.FlightTicketPricePkr,
                TicketPriceSar = package.FlightTicketPriceSar,
                TicketPriceUsd = package.FlightTicketPriceUsd,

                TicketPeriod = package.FlightTicketPeriod
            };

            return View(request);
        }

        [HttpPost]
        public IActionResult Edit(PackageEditRequest request)
        {

            ApplicationSettings settings = _dbContext.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .FirstOrDefault();
            ViewBag.SARROE = settings.SARROE;
            ViewBag.USDROE = settings.USDROE;

            ViewBag.PackageTerms = _dbContext.PackageTermsConditions
                .Where(e => e.PackageId == request.PackageId)
                .ToList();
            ViewBag.PackageItinerary = _dbContext.PackageItinerary
                .Where(e => e.PackageId == request.PackageId)
                .ToList();
            ViewBag.PackageImages = _dbContext.Packages
                .Where(e => e.PackageId == request.PackageId)
                .ToList();

            ViewBag.FacilityDescriptions = _dbContext.IndividualFacilityDescriptionsWithoutTrash;
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Companies = _dbContext
                .UserCompanies
                .Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);
            ViewBag.Accomodations = _dbContext.AccomodationsWithoutTrash;
            ViewBag.AccomodationCategories = _dbContext.AccomodationCategoriesWithoutTrash;
            ViewBag.TravelRoutes = _dbContext.TravelRoutesWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomName = _dbContext
                .Rooms
                .Select(r => new { r.RoomId, r.RoomName })
                .ToList();

            ViewBag.TrainingSessionsCount = _dbContext.TrainingsWithoutTrash.Count();
            ViewBag.GiftsCount = _dbContext.GiftsWithoutTrash.Count();
            ViewBag.PaymentNotesCount = _dbContext.PaymentNotesWithoutTrash.Count();
            ViewBag.AssignedFacility = _dbContext.PackageCategoryFacilities
                .Include(e => e.IndividualFacilityDescription)
                .Where(e => e.PackageId == request.PackageId);

            ViewBag.TrainingSessions = _dbContext.TrainingsWithoutTrash;
            ViewBag.Gifts = _dbContext.GiftsWithoutTrash;
            ViewBag.PaymentNotes = _dbContext.PaymentNotesWithoutTrash;

            ViewBag.AssignedPackageAddons = _dbContext.PackageAddons.Where(e => e.PackageId == request.PackageId);
            ViewBag.AssignedAccommodations = _dbContext.PackageAccomodtions.Where(e => e.PackageId == request.PackageId);
            ViewBag.AssignedAddFac = _dbContext.AdditionalFacilities.Where(e => e.PackageId == request.PackageId);
            ViewBag.AssignedGifts = _dbContext.PackageGifts.Where(e => e.PackageId == request.PackageId);
            ViewBag.AssignedNotes = _dbContext.PackagePaymentNotes.Where(e => e.PackageId == request.PackageId);

            ViewBag.AssignedTrainings = _dbContext.PackageTrainings.Where(e => e.PackageId == request.PackageId);
            ViewBag.Facilities = _dbContext.IndividualFacilityDescriptions.Where(e => e.RoomId == request.RoomIds);

            ViewBag.FlightDetails = _dbContext.PackageFlights
                .Where(e => e.PackageId == request.PackageId).ToList();

            ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                .Cast<FlightClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.FlightType = Enum.GetValues(typeof(FlightType))
                .Cast<FlightType>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

            ViewBag.TrainDetails = _dbContext.PackageTrains
                .Where(e => e.PackageId == request.PackageId).ToList();

            ViewBag.Railway = _dbContext.RailwaysWithoutTrash;

            ViewBag.TrainClass = Enum.GetValues(typeof(TrainClass))
                .Cast<TrainClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();



            Package package = _dbContext.Packages.Find(request.PackageId);

            #region Accommodation Validations
            if (request.AccomodationPlaceId is not null)
            {
                bool allPlaceSelected = true;
                for (int i = 0; i < request.AccomodationPlaceId.Count; i++)
                {
                    if (request.AccomodationPlaceId[i] == null)
                    {
                        allPlaceSelected = false;
                        break;
                    }
                }

                if (!allPlaceSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationTypeSelected = true;
                for (int i = 0; i < request.AccomodationId.Count; i++)
                {
                    if (request.AccomodationId[i] == null)
                    {
                        allAccomodationTypeSelected = false;
                        break;
                    }
                }

                if (!allAccomodationTypeSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationCategorySelected = true;
                for (int i = 0; i < request.AccomodationCategoryId.Count; i++)
                {
                    if (request.AccomodationCategoryId[i] == null)
                    {
                        allAccomodationCategorySelected = false;
                        break;
                    }
                }

                if (!allAccomodationCategorySelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationSelected = true;
                for (int i = 0; i < request.Accomodation.Count; i++)
                {
                    if (request.Accomodation[i] == null)
                    {
                        allAccomodationSelected = false;
                        break;
                    }
                }

                if (!allAccomodationSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationDaysSelected = true;
                for (int i = 0; i < request.AccomodationDays.Count; i++)
                {
                    if (request.AccomodationDays[i] == null)
                    {
                        allAccomodationDaysSelected = false;
                        break;
                    }
                }

                if (!allAccomodationDaysSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationNightsSelected = true;
                for (int i = 0; i < request.AccomodationNights.Count; i++)
                {
                    if (request.AccomodationNights[i] == null)
                    {
                        allAccomodationNightsSelected = false;
                        break;
                    }
                }

                if (!allAccomodationNightsSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationCheckInSelected = true;
                for (int i = 0; i < request.AccomodationCheckIn.Count; i++)
                {
                    if (request.AccomodationCheckIn[i] == null)
                    {
                        allAccomodationCheckInSelected = false;
                        break;
                    }
                }

                if (!allAccomodationCheckInSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationCheckOutSelected = true;
                for (int i = 0; i < request.AccomodationCheckOut.Count; i++)
                {
                    if (request.AccomodationCheckOut[i] == null)
                    {
                        allAccomodationCheckOutSelected = false;
                        break;
                    }
                }

                if (!allAccomodationCheckOutSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationFoodPackageSelected = true;
                for (int i = 0; i < request.AccomodationFoodPackageId.Count; i++)
                {
                    if (request.AccomodationFoodPackageId[i] == null)
                    {
                        allAccomodationFoodPackageSelected = false;
                        break;
                    }
                }

                if (!allAccomodationFoodPackageSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                /*bool allAccomodationLoadPerRoomSelected = true;
                for (int i = 0; i < request.AccomodationLoadPerRoomBeds.Count; i++)
                {
                    if (request.AccomodationLoadPerRoomBeds[i] == null)
                    {
                        allAccomodationLoadPerRoomSelected = false;
                        break;
                    }
                }

                if (!allAccomodationLoadPerRoomSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }

                bool allAccomodationLoadPerWashroomSelected = true;
                for (int i = 0; i < request.AccomodationLoadPerWashroomPersons.Count; i++)
                {
                    if (request.AccomodationLoadPerWashroomPersons[i] == null)
                    {
                        allAccomodationLoadPerWashroomSelected = false;
                        break;
                    }
                }*/

                /*if (!allAccomodationLoadPerWashroomSelected)
                {
                    ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
                }*/
            }
            else
            {
                ModelState.AddModelError("AccommodationError", "Add atleast 1 Accommodation");
            }
            #endregion

            #region Training Session
            if (request.TrainingIds.Count == 0)
            {
                ModelState.AddModelError("TrainingId", "Please select a training session.");
            }

            bool giftSelected = false;
            if (request.GiftId is not null)
            {
                for (int i = 0; i < request.GiftId.Count; i++)
                {
                    if (request.GiftSelected[i] == true)
                    {
                        giftSelected = true;
                        break;
                    }
                }
            }
            if (!giftSelected)
            {
                ModelState.AddModelError("GiftError", "Please select atleast 1 gift for training session.");
            }

            #endregion


            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                package.CompanyId = request.CompanyId;
                package.PackageNumber = 0;
                package.Name = request.PackageName;
                package.Code = request.PackageCode;
                package.PackageNumber = request.PackageNumber;
                package.Quota = request.PackageQouta;
                package.Days = request.Days;
                package.HajjDues = 0;
                package.TravelRouteId = request.TravelRouteId;
                package.MedinahArrival = request.MedinaArrival;
                package.HajjDuration = request.HajjDuration;
                package.FoodIncluded = false;
                //package.QurbaniIncluded = request.PackageQurbaniIncluded;

                package.SARROE = 0;
                package.PKRROE = request.PKRROE;
                package.USDROE = request.USDROE;
                package.GBPROE = request.GBPROE;
                package.EUROROE = request.EUROROE;
                package.AEDROE = request.AEDROE;
                package.AdultPkr = request.AdultPkr;
                package.AdultSar = request.AdultSar;
                package.AdultUsd = request.AdultUsd;
                package.AdultGbp = request.AdultGbp;
                package.AdultEuro = request.AdultEuro;
                package.AdultAed = request.AdultAed;
                package.ChildPkr = request.ChildPkr;
                package.ChildSar = request.ChildSar;
                package.ChildUsd = request.ChildUsd;
                package.ChildGbp = request.ChildGbp;
                package.ChildEuro = request.ChildEuro;
                package.ChildAed = request.ChildAed;
                package.InfantPkr = request.InfantPkr;
                package.InfantSar = request.InfantSar;
                package.InfantUsd = request.InfantUsd;
                package.InfantGbp = request.InfantGbp;
                package.InfantAed = request.InfantAed;
                package.RoomId = request.RoomIds;
                package.RoomTypesId = request.RoomTypesId;
                //package.IndividualFacilityDescriptionId = request.Camps;
                package.NearBy = request.NearBy;
                package.TermsnCondition = request.TermsnCondition;
                package.Content = request.Description;
                package.Color = request.Color;

                package.FlightType = request.FlightType;
                package.FlightClass = request.FlightClass;
                package.AirportTransport = request.AirportTransfer;
                package.TransportWithMashair = request.TransportWithMashair;
                package.TransportIntercity = request.TransportIntercity;
                package.WithInCity = request.WithInCity;
                package.POD = request.POD;
                package.FlightTicketPricePkr = request.TicketPricePkr;
                package.FlightTicketPriceSar = request.TicketPriceSar;
                package.FlightTicketPriceUsd = request.TicketPriceUsd;
                package.FlightTicketPeriod = request.TicketPeriod;
                package.Maktab = request.Maktab;
                package.MaktabNumber = request.MaktabNumber;
                package.MaktabAddress = request.MaktabAddress;
                package.OfficeAddress = request.OfficeAddress;
                package.CategoryZone = request.PackageCategoryZone;

                if (request.MINA is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MINA.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MINA.CopyTo(fs);
                    }
                    package.MINA = ApplicationManager.PackageItineraryPath + request.MINA.FileName;
                }
                if (request.ARAFAT is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.ARAFAT.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.ARAFAT.CopyTo(fs);
                    }
                    package.ARAFAT = ApplicationManager.PackageItineraryPath + request.ARAFAT.FileName;
                }
                if (request.MUZDALIFAH is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MUZDALIFAH.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MUZDALIFAH.CopyTo(fs);
                    }
                    package.MUZDALIFAH = ApplicationManager.PackageItineraryPath + request.MUZDALIFAH.FileName;
                }
                if (request.RamiDayOne is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayOne.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayOne.CopyTo(fs);
                    }
                    package.RamiDayOne = ApplicationManager.PackageItineraryPath + request.RamiDayOne.FileName;
                }
                if (request.RamiDayTwo is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayTwo.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayTwo.CopyTo(fs);
                    }
                    package.RamiDayTwo = ApplicationManager.PackageItineraryPath + request.RamiDayTwo.FileName;
                }
                if (request.RamiDayThree is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayThree.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayThree.CopyTo(fs);
                    }
                    package.RamiDayThree = ApplicationManager.PackageItineraryPath + request.RamiDayThree.FileName;
                }

                _dbContext.SaveChanges();

                _dbContext.PackageAddons.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                _dbContext.SaveChanges();

                _dbContext.PackageAccomodtions.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                _dbContext.PackageTermsConditions.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                _dbContext.PackageItinerary.Where(e => e.PackageId == request.PackageId).ExecuteDelete();
                _dbContext.PackageCategoryFacilities.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                _dbContext.PackageFlights.Where(e => e.PackageId == request.PackageId).ExecuteDelete();
                _dbContext.PackageTrains.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                for (int i = 0; i < request.Accomodation.Count; i++)
                {
                    _dbContext.PackageAccomodtions.Add(
                        new PackageAccomodations
                        {
                            PackageId = package.PackageId,
                            PlaceId = (int)request.AccomodationPlaceId[i],
                            AccomodationCategoryId = (int)request.AccomodationCategoryId[i],
                            AccomodationId = (int)request.AccomodationId[i],
                            AccomodationName = request.Accomodation[i],
                            DistanceInMeters = request.AccomodationDistance[i],
                            Days = request.AccomodationDays[i],
                            Nights = request.AccomodationNights[i],
                            CheckIn = request.AccomodationCheckIn[i],
                            CheckOut = request.AccomodationCheckOut[i],
                            FoodIncluded = false,
                            ResidenceIncluded = false,
                            FoodPackageId = (int)request.AccomodationFoodPackageId[i],
                            ActualAccommodation = "",
                            HotelId = request.HotelIds[i],
                            ActualDays = 0,
                            ActualNights = 0,
                            ActualCheckIn = "",
                            ActualCheckOut = "",
                            ActualCheckInTime = request.AccomodationActualCheckInTime[i],
                            ActualCheckOutTime = request.AccomodationActualCheckOutTime[i],
                            /*LoadPerRoom = (int)request.AccomodationLoadPerRoomBeds[i],
                            LoadPerWashroom = (int)request.AccomodationLoadPerWashroomPersons[i],*/
                            //RoomId = (int)request.AccomodationActualLoadPerRoom[i],
                            GroupZiarat = request.AccomodationGroupZiarat[i],
                            ReligiousLectures = request.AccomodationReligiousLecture[i],
                            DistributionArrangements = request.AccomodationDistribution[i],
                            Camp = request.AccomodationCamp[i],
                            Arafat = request.AccomodationArafat[i],
                            Shuttle = request.AccomodationShuttle[i],
                            Bedding = request.AccomodationBedding[i],
                            Sharing = request.AccomodationSharing[i],
                            SharingType = request.AccomodationSharingType[i],
                            Notes = request.AccomodationNote[i]
                        }
                    );
                }

                _dbContext.SaveChanges();

                //var existingFacilities = _dbContext.PackageCategoryFacilities
                //    .Where(pcf => pcf.PackageId == package.PackageId)
                //    .ToList();

                //var existingFacilityIds = existingFacilities
                //    .Select(pcf => pcf.IndividualFacilityDescriptionId)
                //    .ToHashSet();

                //var selectedFacilityIds = new HashSet<int>();

                //for (int i = 0; i < request.PackageFacilitiesIds?.Count; i++)
                //{
                //    var facilityId = (int)request.PackageFacilitiesIds[i];
                //    selectedFacilityIds.Add(facilityId);

                //    if (!existingFacilityIds.Contains(facilityId))
                //    {
                //        var packageCategoryFacility = new PackageCategoryFacilities
                //        {
                //            PackageId = package.PackageId,
                //            IndividualFacilityDescriptionId = facilityId
                //        };

                //        _dbContext.PackageCategoryFacilities.Add(packageCategoryFacility);
                //    }
                //}

                for (int i = 0; i < request.PackageFacilitiesIds?.Count; i++)
                {
                    var facilityId = request.PackageFacilitiesIds[i];

                    var packageCategoryFacility = new PackageCategoryFacilities
                    {
                        PackageId = package.PackageId,
                        IndividualFacilityDescriptionId = facilityId
                    };

                    _dbContext.PackageCategoryFacilities.Add(packageCategoryFacility);
                }

                for (int i = 0; i < request.AirlineIds?.Count; i++)
                {
                    var newFlight = new PackageFlights
                    {
                        PackageId = package.PackageId,
                        AirlineId = request.AirlineIds[i],
                        DepartureDate = request.DepartureDate[i],
                        ArrivalDate = request.ArrivalDate[i],
                        DepartureTime = request.DepartureTime[i],
                        ArrivalTime = request.ArrivalTime[i],
                        PNRNumber = request.PNRNos[i],
                        FlightClass = request.FlightClasses[i],
                        FlightType = request.FlightTypes[i],
                        Origin = request.Origins[i],
                        Destination = request.Destinations[i],
                        FlightNumber = request.FlightNos[i],
                        TicketAmount = request.TicketAmount[i],
                        IsPreffered = request.IsPreffereds[i]
                    };

                    _dbContext.PackageFlights.Add(newFlight);
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < request.RailwayIds?.Count; i++)
                {
                    var newTrain = new PackageTrains
                    {
                        PackageId = package.PackageId,
                        RailwayId = request.RailwayIds[i],
                        DepartureDate = request.TrainDepartureDate[i],
                        ArrivalDate = request.TrainArrivalDate[i],
                        DepartureTime = request.TrainDepartureTime[i],
                        ArrivalTime = request.TrainArrivalTime[i],
                        PNRNumber = request.TrainPNRNos[i],
                        TrainClass = request.TrainClasses[i],
                        Origin = request.TrainOrigins[i],
                        Destination = request.TrainDestinations[i],
                        TicketAmount = request.TrainTicketAmount[i],
                        TrainNumber = request.TrainNos[i]
                    };

                    _dbContext.PackageTrains.Add(newTrain);
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < request.Terms?.Count; i++)
                {

                    var termText = request.Terms[i];

                    var newTerm = new PackageTermsConditions
                    {
                        PackageId = package.PackageId,
                        Term = termText
                    };
                    _dbContext.PackageTermsConditions.Add(newTerm);

                }
                _dbContext.SaveChanges();


                //for (int i = 0; i < request.Description?.Count; i++)
                //{
                //    var DescriptionText = request.Description[i];


                //    var newitinerary = new PackageItinerary
                //    {
                //        PackageId = package.PackageId,
                //        Description = DescriptionText
                //    };
                //    _dbContext.PackageItinerary.Add(newitinerary);

                //}
                //_dbContext.SaveChanges();

                // _dbContext.AdditionalFacilities.Where(e => e.PackageId == request.PackageId).ExecuteDelete();
                var addFacilities = _dbContext.AdditionalFacilities.Where(e => e.PackageId == request.PackageId).Select(e => e.AdditionalFacilitiesId).ToList();
                //if (request.AdditionalCode is not null)
                //{
                //    //return Content("Hello There");

                //    for (int i = 0; i < request.AdditionalFacilityIds.Count; i++)
                //    {
                //        int? id = request.AdditionalFacilityIds.ElementAt(i);
                //        if (id == 0)
                //        {
                //            _dbContext.AdditionalFacilities.Add(
                //                new AdditionalFacilities
                //                {
                //                    PackageId = package.PackageId,
                //                    Code = request.AdditionalCode[i],
                //                    RoomId = request.AdditionalRoomId[i],
                //                    IndividiualFacilityDescriptionId = request.AdditionalFacilityDescriptionId[i],
                //                    PlaceId = request.AdditionalPlaceId[i],
                //                    AccomodationId = request.AdditionalAccomodationId[i],

                //                    SARROE = request.SARROE,
                //                    USDROE = request.USDROE,

                //                    AdultPkr = request.AdditionalAdultPkr[i],
                //                    AdultSar = request.AdditionalAdultSar[i],
                //                    AdultUsd = request.AdditionalAdultUsd[i],
                //                    ChildPkr = request.AdditionalChildPkr[i],
                //                    ChildSar = request.AdditionalChildSar[i],
                //                    ChildUsd = request.AdditionalChildUsd[i],
                //                    InfantPkr = request.AdditionalInfantPkr[i],
                //                    InfantSar = request.AdditionalInfantSar[i],
                //                    InfantUsd = request.AdditionalInfantUsd[i],

                //                    Note = request.AdditionalNotes[i]
                //                }
                //            );

                //            _dbContext.SaveChanges();
                //        }
                //        else if (addFacilities.Contains(id))
                //        {
                //            AdditionalFacilities _edit = _dbContext.AdditionalFacilities.Find(id);

                //            _edit.Code = request.AdditionalCode[i];
                //            _edit.RoomId = request.AdditionalRoomId[i];
                //            _edit.IndividiualFacilityDescriptionId = request.AdditionalFacilityDescriptionId[i];
                //            _edit.PlaceId = request.AdditionalPlaceId[i];
                //            _edit.AccomodationId = request.AdditionalAccomodationId[i];
                //            _edit.SARROE = request.SARROE;
                //            _edit.USDROE = request.USDROE;
                //            _edit.AdultPkr = request.AdditionalAdultPkr[i];
                //            _edit.AdultSar = request.AdditionalAdultSar[i];
                //            _edit.AdultUsd = request.AdditionalAdultUsd[i];
                //            _edit.ChildPkr = request.AdditionalChildPkr[i];
                //            _edit.ChildSar = request.AdditionalChildSar[i];
                //            _edit.ChildUsd = request.AdditionalChildUsd[i];
                //            _edit.InfantPkr = request.AdditionalInfantPkr[i];
                //            _edit.InfantSar = request.AdditionalInfantSar[i];
                //            _edit.InfantUsd = request.AdditionalInfantUsd[i];
                //            _edit.Note = request.AdditionalNotes[i];

                //            _dbContext.SaveChanges();
                //        }
                //    }

                //    var valuesNotInBothLists = addFacilities.Except(request.AdditionalFacilityIds).ToList();

                //    _dbContext.AdditionalFacilities.Where(e => valuesNotInBothLists.Contains(e.AdditionalFacilitiesId)).ExecuteDelete();

                //    _dbContext.SaveChanges();
                //    // return Ok(new {
                //    //     msg = "gh",
                //    //     valuesNotInBothLists
                //    // });

                //    //_dbContext.SaveChanges();
                //}


                _dbContext.PackageGifts.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                for (int i = 0; i < request.GiftId.Count; i++)
                {
                    if (request.GiftSelected[i] == false) continue;

                    _dbContext.PackageGifts.Add(
                        new PackageGifts { PackageId = package.PackageId, GiftId = request.GiftId[i] }
                        );
                }

                _dbContext.SaveChanges();

                _dbContext.PackageTrainings.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                if (request.TrainingIds is not null)
                {
                    for (int i = 0; i < request.TrainingIds.Count; i++)
                    {
                        if (request.TrainingSelected[i] == false) continue;
                        _dbContext.PackageTrainings.Add(
                            new PackageTrainings
                            {
                                PackageId = package.PackageId,
                                TrainingId = request.TrainingIds[i]
                            }
                            );
                    }
                }
                _dbContext.SaveChanges();

                _dbContext.PackagePaymentNotes.Where(e => e.PackageId == request.PackageId).ExecuteDelete();

                // for (int i = 0; i < request.NoteId.Count; i++)
                // {
                //     if (request.NoteSelected[i] == false) continue;

                //     _dbContext.PackagePaymentNotes.Add(
                //         new PackagePaymentNotes { PackageId = package.PackageId, PaymentNotesId = request.NoteId[i] }
                //         );
                // }

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

        public IActionResult Copy(int id)
        {
            ViewBag.SelectedIADFRoom = _dbContext.PackageCategoryFacilities
                .Where(e => e.PackageId == id)
                .Select(e => e.IndividualFacilityDescriptionId)
                .ToList();

            ViewBag.MakkahRooms = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 1)
                .ToList();

            ViewBag.Medinah = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 2)
                .ToList();

            ViewBag.Azizia = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 4)
                .ToList();

            ViewBag.Mina = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.PlaceId == 3)
                .ToList();

            ViewBag.FacilityDescriptions = _dbContext.IndividualFacilityDescriptionsWithoutTrash;
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.CopyTerms = _dbContext.PackageTermsConditions
                .Where(e => e.PackageId == id)
                .ToList();
            ViewBag.CopyItinerary = _dbContext.PackageItinerary
               .Where(e => e.PackageId == id)
               .ToList();
            ViewBag.CopyImages = _dbContext.Packages
               .Where(e => e.PackageId == id)
               .ToList();
            ViewBag.Companies = _dbContext
                .UserCompanies
                .Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);
            ViewBag.Accomodations = _dbContext.AccomodationsWithoutTrash;
            ViewBag.AccomodationCategories = _dbContext.AccomodationCategoriesWithoutTrash;
            ViewBag.TravelRoutes = _dbContext.TravelRoutesWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;

            ViewBag.TrainingSessionsCount = _dbContext.TrainingsWithoutTrash.Count();
            ViewBag.GiftsCount = _dbContext.GiftsWithoutTrash.Count();
            ViewBag.PaymentNotesCount = _dbContext.PaymentNotesWithoutTrash.Count();

            ViewBag.TrainingSessions = _dbContext.TrainingsWithoutTrash;
            ViewBag.Gifts = _dbContext.GiftsWithoutTrash;
            ViewBag.PaymentNotes = _dbContext.PaymentNotesWithoutTrash;

            ViewBag.AssignedPackageAddons = _dbContext.PackageAddons.Where(e => e.PackageId == id);
            ViewBag.AssignedAccommodations = _dbContext.PackageAccomodtions.Where(e => e.PackageId == id);
            ViewBag.AssignedAddFac = _dbContext.AdditionalFacilities.Where(e => e.PackageId == id);
            ViewBag.AssignedGifts = _dbContext.PackageGifts.Where(e => e.PackageId == id);
            ViewBag.AssignedTrainings = _dbContext.PackageTrainings.Where(e => e.PackageId == id);
            ViewBag.AssignedNotes = _dbContext.PackagePaymentNotes.Where(e => e.PackageId == id);
            ViewBag.RoomName = _dbContext
               .Rooms
               .Select(r => new { r.RoomId, r.RoomName })
               .ToList();
            /*ViewBag.AssignedFacilities = _dbContext.PackageCategoryFacilities.Where(e => e.PackageId == id);*/
            ViewBag.AssignedFacility = _dbContext.PackageCategoryFacilities.Where(e => e.PackageId == id);

            Package package = _dbContext.Packages.Find(id);
            ViewBag.Id = id;
            ViewBag.Facilities = _dbContext.IndividualFacilityDescriptions.Where(e => e.RoomId == package.RoomId);

            ViewBag.FlightDetails = _dbContext.PackageFlights
                .Where(e => e.PackageId == id).ToList();

            ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                .Cast<FlightClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.FlightType = Enum.GetValues(typeof(FlightType))
                .Cast<FlightType>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

            ViewBag.TrainDetails = _dbContext.PackageTrains
                .Where(e => e.PackageId == id).ToList();

            ViewBag.Railway = _dbContext.RailwaysWithoutTrash;

            ViewBag.TrainClass = Enum.GetValues(typeof(TrainClass))
                .Cast<TrainClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            var TravelRouteStop = _dbContext.TravelRouteStops
                    .Include(e => e.Route)
                    .Include(e => e.Vehicle)
                    .OrderBy(e => e.CheckIn)
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                    .Select(e => new
                    {
                        e.TravelRouteId,
                        e.Route.StartPlace,
                        e.Route.Destination,
                        CheckIn = e.CheckIn.ToString("dd/MMM/yyyy HH:mm"),
                        CheckOut = e.CheckOut.ToString("dd/MMM/yyyy HH:mm"),
                        TransportType = e.TransportType.ToString(),
                        e.Vehicle.VehicleType,
                        //e.Price
                    })
                    .ToList();

            ViewBag.Transportation = TravelRouteStop;

            ViewBag.Camps = _dbContext.IndividualFacilityDescriptionsWithoutTrash
                .Where(e => e.Place.PlaceId == 3)
                .ToList();


            PackageRegisterRequest request = new PackageRegisterRequest
            {
                CompanyId = package.CompanyId,
                TravelRouteId = package.TravelRouteId,
                PackageCode = package.Code,
                PackageName = package.Name,
                PackageNumber = package.PackageNumber,
                PackageQouta = package.Quota,
                Days = package.Days,
                CompulsoryHajjDues = package.HajjDues,
                MedinaArrival = package.MedinahArrival,
                HajjDuration = package.HajjDuration,
                PackageFoodIncluded = package.FoodIncluded,
                //PackageQurbaniIncluded = package.QurbaniIncluded,
                PackageCategoryZone = package.CategoryZone,
                RoomTypesId = package.RoomTypesId,
                PKRROE = package.PKRROE,
                USDROE = package.USDROE,
                GBPROE = package.GBPROE,
                EUROROE = package.EUROROE,
                AEDROE = package.AEDROE,
                AdultPkr = package.AdultPkr,
                AdultSar = package.AdultSar,
                AdultUsd = package.AdultUsd,
                AdultGbp = package.AdultGbp,
                AdultEuro = package.AdultEuro,
                AdultAed = package.AdultAed,
                ChildPkr = package.ChildPkr,
                ChildSar = package.ChildSar,
                ChildUsd = package.ChildUsd,
                ChildGbp = package.ChildGbp,
                ChildEuro = package.ChildEuro,
                ChildAed = package.ChildAed,
                InfantPkr = package.InfantPkr,
                InfantSar = package.InfantSar,
                InfantUsd = package.InfantUsd,
                InfantGbp = package.InfantGbp,
                InfantEuro = package.InfantEuro,
                InfantAed = package.InfantAed,
                //Camps = package.IndividualFacilityDescriptionId,
                NearBy = package.NearBy,
                TermsnCondition = package.TermsnCondition,
                Description = package.Content,
                Color = package.Color,

                FlightType = package.FlightType,
                FlightClass = package.FlightClass,
                TransportIntercity = package.TransportIntercity,
                TransportWithMashair = package.TransportWithMashair,
                AirportTransfer = package.AirportTransport,
                WithInCity = package.WithInCity,
                POD = package.POD,

                TicketPeriod = package.FlightTicketPeriod,
                TicketPricePkr = package.FlightTicketPricePkr,
                TicketPriceSar = package.FlightTicketPriceSar,
                TicketPriceUsd = package.FlightTicketPriceUsd,
                Maktab = package.Maktab,
                MaktabNumber = package.MaktabNumber,
                MaktabAddress = package.MaktabAddress,
                OfficeAddress = package.OfficeAddress,
            };

            return View(request);
        }

        [HttpPost]
        public IActionResult Copy(PackageRegisterRequest request, int PackageId)
        {

            ApplicationSettings settings = _dbContext.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .FirstOrDefault();
            ViewBag.SARROE = settings.SARROE;
            ViewBag.USDROE = settings.USDROE;
            ViewBag.FacilityDescriptions = _dbContext.IndividualFacilityDescriptionsWithoutTrash;
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Companies = _dbContext
                .UserCompanies
                .Include(e => e.Company)
                        .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);
            ViewBag.Accomodations = _dbContext.AccomodationsWithoutTrash;
            ViewBag.AccomodationCategories = _dbContext.AccomodationCategoriesWithoutTrash;
            ViewBag.TravelRoutes = _dbContext.TravelRoutesWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.RoomName = _dbContext
               .Rooms
               .Select(r => new { r.RoomId, r.RoomName })
               .ToList();
            ViewBag.AssignedFacility = _dbContext.PackageCategoryFacilities.Where(e => e.PackageId == PackageId);
            ViewBag.Facilities = _dbContext.IndividualFacilityDescriptions.Where(e => e.RoomId == request.RoomIds);


            ViewBag.TrainingSessionsCount = _dbContext.TrainingsWithoutTrash.Count();
            ViewBag.GiftsCount = _dbContext.GiftsWithoutTrash.Count();
            ViewBag.PaymentNotesCount = _dbContext.PaymentNotesWithoutTrash.Count();

            ViewBag.TrainingSessions = _dbContext.TrainingsWithoutTrash;
            ViewBag.Gifts = _dbContext.GiftsWithoutTrash;
            ViewBag.PaymentNotes = _dbContext.PaymentNotesWithoutTrash;

            ViewBag.AssignedPackageAddons = _dbContext.PackageAddons.Where(e => e.PackageId == PackageId);
            ViewBag.AssignedAccommodations = _dbContext.PackageAccomodtions.Where(e => e.PackageId == PackageId);
            ViewBag.AssignedAddFac = _dbContext.AdditionalFacilities.Where(e => e.PackageId == PackageId);
            ViewBag.AssignedGifts = _dbContext.PackageGifts.Where(e => e.PackageId == PackageId);
            ViewBag.AssignedTrainings = _dbContext.PackageTrainings.Where(e => e.PackageId == PackageId);
            ViewBag.AssignedNotes = _dbContext.PackagePaymentNotes.Where(e => e.PackageId == PackageId);

            ViewBag.FlightDetails = _dbContext.PackageFlights
                .Where(e => e.PackageId == PackageId).ToList();

            ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                .Cast<FlightClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.FlightType = Enum.GetValues(typeof(FlightType))
                .Cast<FlightType>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

            ViewBag.TrainDetails = _dbContext.PackageTrains
                .Where(e => e.PackageId == PackageId).ToList();

            ViewBag.Railway = _dbContext.RailwaysWithoutTrash;

            ViewBag.TrainClass = Enum.GetValues(typeof(TrainClass))
                .Cast<TrainClass>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            var TravelRouteStop = _dbContext.TravelRouteStops
                    .Include(e => e.Route)
                    .Include(e => e.Vehicle)
                    .OrderBy(e => e.CheckIn)
                    .Select(e => new
                    {
                        e.TravelRouteId,
                        e.Route.StartPlace,
                        e.Route.Destination,
                        CheckIn = e.CheckIn.ToString("dd/MMM/yyyy HH:mm"),
                        CheckOut = e.CheckOut.ToString("dd/MMM/yyyy HH:mm"),
                        TransportType = e.TransportType.ToString(),
                        e.Vehicle.VehicleType,
                        //e.Price
                    })
                    .ToList();

            ViewBag.Transportation = TravelRouteStop;


            bool giftSelected = false;
            for (int i = 0; i < request.GiftId.Count; i++)
            {
                if (request.GiftSelected[i] == true)
                {
                    giftSelected = true;
                    break;
                }
            }

            if (!giftSelected)
            {
                ModelState.AddModelError("GiftError", "Please select atleast 1 gift for training session.");
            }

            #region Accommodation Validations

            bool allPlaceSelected = true;
            for (int i = 0; i < request.AccomodationPlaceId.Count; i++)
            {
                if (request.AccomodationPlaceId[i] == null)
                {
                    allPlaceSelected = false;
                    break;
                }
            }

            if (!allPlaceSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationTypeSelected = true;
            for (int i = 0; i < request.AccomodationId.Count; i++)
            {
                if (request.AccomodationId[i] == null)
                {
                    allAccomodationTypeSelected = false;
                    break;
                }
            }

            if (!allAccomodationTypeSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationCategorySelected = true;
            for (int i = 0; i < request.AccomodationCategoryId.Count; i++)
            {
                if (request.AccomodationCategoryId[i] == null)
                {
                    allAccomodationCategorySelected = false;
                    break;
                }
            }

            if (!allAccomodationCategorySelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationSelected = true;
            for (int i = 0; i < request.Accomodation.Count; i++)
            {
                if (request.Accomodation[i] == null)
                {
                    allAccomodationSelected = false;
                    break;
                }
            }

            if (!allAccomodationSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationDaysSelected = true;
            for (int i = 0; i < request.AccomodationDays.Count; i++)
            {
                if (request.AccomodationDays[i] == null)
                {
                    allAccomodationDaysSelected = false;
                    break;
                }
            }

            if (!allAccomodationDaysSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationNightsSelected = true;
            for (int i = 0; i < request.AccomodationNights.Count; i++)
            {
                if (request.AccomodationNights[i] == null)
                {
                    allAccomodationNightsSelected = false;
                    break;
                }
            }

            if (!allAccomodationNightsSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationCheckInSelected = true;
            for (int i = 0; i < request.AccomodationCheckIn.Count; i++)
            {
                if (request.AccomodationCheckIn[i] == null)
                {
                    allAccomodationCheckInSelected = false;
                    break;
                }
            }

            if (!allAccomodationCheckInSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationCheckOutSelected = true;
            for (int i = 0; i < request.AccomodationCheckOut.Count; i++)
            {
                if (request.AccomodationCheckOut[i] == null)
                {
                    allAccomodationCheckOutSelected = false;
                    break;
                }
            }

            if (!allAccomodationCheckOutSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationFoodPackageSelected = true;
            for (int i = 0; i < request.AccomodationFoodPackageId.Count; i++)
            {
                if (request.AccomodationFoodPackageId[i] == null)
                {
                    allAccomodationFoodPackageSelected = false;
                    break;
                }
            }

            if (!allAccomodationFoodPackageSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            /*bool allAccomodationLoadPerRoomSelected = true;
            for (int i = 0; i < request.AccomodationLoadPerRoomBeds.Count; i++)
            {
                if (request.AccomodationLoadPerRoomBeds[i] == null)
                {
                    allAccomodationLoadPerRoomSelected = false;
                    break;
                }
            }

            if (!allAccomodationLoadPerRoomSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }

            bool allAccomodationLoadPerWashroomSelected = true;
            for (int i = 0; i < request.AccomodationLoadPerWashroomPersons.Count; i++)
            {
                if (request.AccomodationLoadPerWashroomPersons[i] == null)
                {
                    allAccomodationLoadPerWashroomSelected = false;
                    break;
                }
            }

            if (!allAccomodationLoadPerWashroomSelected)
            {
                ModelState.AddModelError("AccommodationError", "Please fill out all required(*) fields.");
            }*/

            #endregion

            //if (!ModelState.IsValid) return View(request);



            var transaction = _dbContext.Database.BeginTransaction();

            try
            {

                Package package = new Package
                {
                    CompanyId = request.CompanyId,
                    PackageNumber = request.PackageNumber,
                    Code = request.PackageCode,
                    Name = request.PackageName,
                    Quota = request.PackageQouta,
                    HajjDues = request.CompulsoryHajjDues,
                    HajjDuration = request.HajjDuration,
                    MedinahArrival = request.MedinaArrival,
                    Days = request.Days,
                    FoodIncluded = false,
                    //QurbaniIncluded = request.PackageQurbaniIncluded,
                    TravelRouteId = request.TravelRouteId,
                    SARROE = 0,
                    RoomId = request.RoomIds,
                    RoomTypesId= request.RoomTypesId,
                    //IndividualFacilityDescriptionId = request.Camps,
                    NearBy = request.NearBy,
                    TermsnCondition = request.TermsnCondition,
                    Content = request.Description,
                    PKRROE = request.PKRROE,
                    USDROE = request.USDROE,
                    GBPROE = request.GBPROE,
                    EUROROE = request.EUROROE,
                    AEDROE = request.AEDROE,
                    AdultPkr = request.AdultPkr,
                    AdultSar = request.AdultSar,
                    AdultUsd = request.AdultUsd,
                    AdultGbp = request.AdultGbp,
                    AdultEuro = request.AdultEuro,
                    AdultAed = request.AdultAed,
                    ChildPkr = request.ChildPkr,
                    ChildSar = request.ChildSar,
                    ChildUsd = request.ChildUsd,
                    ChildGbp = request.ChildGbp,
                    ChildEuro = request.ChildEuro,
                    ChildAed = request.ChildAed,
                    InfantPkr = request.InfantPkr,
                    InfantSar = request.InfantSar,
                    InfantUsd = request.InfantUsd,
                    InfantGbp = request.InfantGbp,
                    InfantEuro = request.InfantEuro,
                    InfantAed = request.InfantAed,
                    Color = request.Color,

                    FlightType = request.FlightType,
                    FlightClass = request.FlightClass,
                    POD = request.POD,
                    TransportIntercity = request.TransportIntercity,
                    WithInCity = request.WithInCity,
                    AirportTransport = request.AirportTransfer,
                    TransportWithMashair = request.TransportWithMashair,

                    FlightTicketPeriod = request.TicketPeriod,
                    FlightTicketPricePkr = request.TicketPricePkr,
                    FlightTicketPriceSar = request.TicketPriceSar,
                    FlightTicketPriceUsd = request.TicketPriceUsd,
                    Maktab = request.Maktab,
                    MaktabNumber= request.MaktabNumber,
                    MaktabAddress =request.MaktabAddress,
                    OfficeAddress = request.OfficeAddress,
                    PackageStatus = PackageStatus.Booked,
                    PackageState = PackageState.Orignal,

                    CategoryZone = request.PackageCategoryZone,
                    HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                };

                if (request.FileImage is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageImages", request.FileImage.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.FileImage.CopyTo(fs);
                    }

                    package.Image = ApplicationManager.PackageImagesPath + request.FileImage.FileName;
                }
                if (request.MINA is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MINA.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MINA.CopyTo(fs);
                    }
                    package.MINA = ApplicationManager.PackageItineraryPath + request.MINA.FileName;
                }
                if (request.ARAFAT is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.ARAFAT.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.ARAFAT.CopyTo(fs);
                    }
                    package.ARAFAT = ApplicationManager.PackageItineraryPath + request.ARAFAT.FileName;
                }
                if (request.MUZDALIFAH is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.MUZDALIFAH.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.MUZDALIFAH.CopyTo(fs);
                    }
                    package.MUZDALIFAH = ApplicationManager.PackageItineraryPath + request.MUZDALIFAH.FileName;
                }
                if (request.RamiDayOne is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayOne.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayOne.CopyTo(fs);
                    }
                    package.RamiDayOne = ApplicationManager.PackageItineraryPath + request.RamiDayOne.FileName;
                }
                if (request.RamiDayTwo is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayTwo.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayTwo.CopyTo(fs);
                    }
                    package.RamiDayTwo = ApplicationManager.PackageItineraryPath + request.RamiDayTwo.FileName;
                }
                if (request.RamiDayThree is not null)
                {
                    string path = Path.Combine(_host.WebRootPath, "Images", "PackageItinerary", request.RamiDayThree.FileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        request.RamiDayThree.CopyTo(fs);
                    }
                    package.RamiDayThree = ApplicationManager.PackageItineraryPath + request.RamiDayThree.FileName;
                }
                _dbContext.Packages.Add(package);
                _dbContext.SaveChanges();

                //for (int i = 0; i < request.Description?.Count; i++)
                //{
                //    var DescriptionText = request.Description[i];

                //    var newitinerary = new PackageItinerary
                //    {
                //        PackageId = package.PackageId,
                //        Description = DescriptionText
                //    };

                //    _dbContext.PackageItinerary.Add(newitinerary);
                //}

                //_dbContext.SaveChanges();

                for (int i = 0; i < request.AirlineIds?.Count; i++)
                {
                    PackageFlights packageFlights = new PackageFlights
                    {
                        PackageId = package.PackageId,
                        AirlineId = request.AirlineIds[i],
                        DepartureDate = request.DepartureDate[i],
                        ArrivalDate = request.ArrivalDate[i],
                        DepartureTime = request.DepartureTime[i],
                        ArrivalTime = request.ArrivalTime[i],
                        PNRNumber = request.PNRNos[i],
                        FlightClass = request.FlightClasses[i],
                        FlightType = request.FlightTypes[i],
                        Origin = request.Origins[i],
                        Destination = request.Destinations[i],
                        FlightNumber = request.FlightNos[i],
                        TicketAmount = request.TicketAmount[i],
                        IsPreffered = request.IsPreffereds[i]
                    };

                    _dbContext.PackageFlights.Add(packageFlights);
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < request.RailwayIds?.Count; i++)
                {
                    PackageTrains packageTrains = new PackageTrains
                    {
                        PackageId = package.PackageId,
                        RailwayId = request.RailwayIds[i],
                        DepartureDate = request.TrainDepartureDate[i],
                        ArrivalDate = request.TrainArrivalDate[i],
                        DepartureTime = request.TrainDepartureTime[i],
                        ArrivalTime = request.TrainArrivalTime[i],
                        PNRNumber = request.TrainPNRNos[i],
                        TrainClass = request.TrainClasses[i],
                        Origin = request.TrainOrigins[i],
                        Destination = request.TrainDestinations[i],
                        TicketAmount = request.TrainTicketAmount[i],
                        TrainNumber = request.TrainNos[i]
                    };

                    _dbContext.PackageTrains.Add(packageTrains);
                }

                _dbContext.SaveChanges();

                for (int i = 0; i < request.Accomodation.Count; i++)
                {
                    _dbContext.PackageAccomodtions.Add(
                        new PackageAccomodations
                        {
                            PackageId = package.PackageId,
                            PlaceId = (int)request.AccomodationPlaceId[i],
                            AccomodationId = (int)request.AccomodationId[i],
                            AccomodationCategoryId = (int)request.AccomodationCategoryId[i],
                            AccomodationName = request.Accomodation[i],
                            DistanceInMeters = request.AccomodationDistance[i],
                            Days = request.AccomodationDays[i],
                            Nights = request.AccomodationNights[i],
                            CheckIn = request.AccomodationCheckIn[i],
                            CheckOut = request.AccomodationCheckOut[i],
                            FoodIncluded = false,
                            FoodPackageId = (int)request.AccomodationFoodPackageId[i],
                            ResidenceIncluded = false,
                            ActualAccommodation = "",
                            HotelId = request.HotelIds.Count > 0 ? request.HotelIds[i] : 0,
                            ActualDays = 0,
                            ActualNights = 0,
                            ActualCheckIn = "",
                            ActualCheckOut = "",
                            ActualCheckInTime = request.AccomodationActualCheckInTime[i],
                            ActualCheckOutTime = request.AccomodationActualCheckOutTime[i],
                            /*LoadPerRoom = (int)request.AccomodationLoadPerRoomBeds[i],
                            LoadPerWashroom = (int)request.AccomodationLoadPerWashroomPersons[i],*/
                            //RoomId = (int)request.AccomodationActualLoadPerRoom[i],
                            GroupZiarat = request.AccomodationGroupZiarat[i],
                            ReligiousLectures = request.AccomodationReligiousLecture[i],
                            DistributionArrangements = request.AccomodationDistribution[i],
                            Camp = request.AccomodationCamp[i],
                            Arafat = request.AccomodationArafat[i],
                            Shuttle = request.AccomodationShuttle[i],
                            Bedding = request.AccomodationBedding[i],
                            Sharing = request.AccomodationSharing[i],
                            SharingType = request.AccomodationSharingType[i],
                            Notes = request.AccomodationNote[i]
                        }
                    );
                }

                for (int i = 0; i < request.PackageFacilitiesIds?.Count; i++)
                {
                    var facilityId = (int)request.PackageFacilitiesIds[i];

                    var packageCategoryFacility = new PackageCategoryFacilities
                    {
                        PackageId = package.PackageId,
                        IndividualFacilityDescriptionId = facilityId
                    };

                    _dbContext.PackageCategoryFacilities.Add(packageCategoryFacility);
                }

                for (int i = 0; i < request.Terms?.Count; i++)
                {

                    var termText = request.Terms[i];

                    var newTerm = new PackageTermsConditions
                    {
                        PackageId = package.PackageId,
                        Term = termText
                    };
                    _dbContext.PackageTermsConditions.Add(newTerm);

                }
                _dbContext.SaveChanges();

                for (int i = 0; i < request.GiftId.Count; i++)
                {
                    if (request.GiftSelected[i] == false) continue;

                    _dbContext.PackageGifts.Add(
                        new PackageGifts { PackageId = package.PackageId, GiftId = request.GiftId[i] }
                        );
                }
                _dbContext.SaveChanges();

                for (int i = 0; i < request.TrainingIds.Count; i++)
                {
                    if (request.TrainingSelected[i] == false) continue;
                    _dbContext.PackageTrainings.Add(
                        new PackageTrainings
                        {
                            PackageId = package.PackageId,
                            TrainingId = request.TrainingIds[i]
                        });
                }

                _dbContext.SaveChanges();

                transaction.Commit();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
                return Ok(request);
            }


        }

        public IActionResult ChangeImage(int id) => View(_dbContext.Packages.Find(id));

        [HttpPost]
        public IActionResult ChangeImage(int PackageId, IFormFile File)
        {
            Package package = _dbContext.Packages.Find(PackageId);

            string path = Path.Combine(_host.WebRootPath, "Images",
                "PackageImages", File.FileName);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            package.Image = "Images/PackageImages/" + File.FileName;

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult GetPackages(int id)
        {
            var packages = _dbContext
                .PackagesWithoutTrash
                .Where(e => e.CompanyId == id && e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            return Ok(packages);
        }

        public IActionResult GetAdditionalFacilities(int id)
        {
            return Ok(
                _dbContext
                .PackageCategoryFacilities
                .Include(e => e.IndividualFacilityDescription)
                .Where(e => e.PackageId == id)
                );
        }

        public IActionResult GetTransport(int id)
        {
            Package p = _dbContext.Packages.Find(id);
            TravelRoute travelRoute = _dbContext.TravelRoutes.Find(p.TravelRouteId);
            var travelStops = _dbContext.TravelRouteStops
                .Include(e => e.Route)
                .Where(e => e.TravelRouteId == travelRoute.TravelRouteId);

            return Ok(travelStops);
        }

        public float GetPackageTotal(int id, int age, float discount,
            FlightTicketType flightTicketType, float flightTicketDifferencesPkr)
        {
            float total = 0f;

            var packageAddonPrices = _dbContext.PackageAddons
                                    .Where(e => e.PackageId == id)
                                    .Select(e => e.PkrPrice)
                                    .Sum();

            float packageAdditionaFacPrices = 0f;

            var package = _dbContext.Packages
            .Select(e => new
            {
                e.InfantPkr,
                e.AdultPkr,
                e.ChildPkr,
                e.PackageId,
                e.FlightTicketPricePkr,
            })
            .FirstOrDefault(e => e.PackageId == id);

            if (age < 2)
            {
                total += package.InfantPkr;
                packageAdditionaFacPrices = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .Where(e => e.FamilyHeadId == id)
                            .Select(e => e.AdditionalFacilities.InfantPkr)
                            .Sum() ?? 0f;
            }
            if (age < 12)
            {
                total += package.ChildPkr;

                packageAdditionaFacPrices = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .Where(e => e.FamilyHeadId == id)
                            .Select(e => e.AdditionalFacilities.ChildPkr)
                            .Sum() ?? 0f;
            }
            else
            {
                total += package.AdultPkr;

                packageAdditionaFacPrices = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .Where(e => e.FamilyHeadId == id)
                            .Select(e => e.AdditionalFacilities.AdultPkr)
                            .Sum() ?? 0f;
            }

            if (flightTicketType == FlightTicketType.Included)
            {
                total += package.FlightTicketPricePkr;
            }
            else if (flightTicketType == FlightTicketType.IncludedWithDifferences)
            {
                total += (package.FlightTicketPricePkr) + flightTicketDifferencesPkr;
            }

            total += packageAdditionaFacPrices;
            total += packageAddonPrices;
            total -= discount;

            return total;
        }

    }
}

namespace TMG.Models.PackageModels
{
    public record PackageQoutationModel
    {
        public string ValidDate { get; set; }
        public string QoutationDate { get; set; }
        public int QoutationId { get; set; }

        public string PackageName { get; set; }
        public string TentativeDays { get; set; }
        public string CategoryName { get; set; }

        public string CustomerName { get; set; }
        public int NumberOfMembers { get; set; }

        public string CustomerContact { get; set; }
        public string CustomerEmail { get; set; }

        public string TentativeDepartureDate { get; set; }
        public string TentativeArrivalDate { get; set; }
        public string Airline { get; set; }

        public List<string> CheckIns { get; set; } = new List<string>();
        public List<string> CheckOuts { get; set; } = new List<string>();
        public List<string> Nights { get; set; } = new List<string>();
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> AccommodationNames { get; set; } = new List<string>();
        public List<string> RoomTypes { get; set; } = new List<string>();
        public List<string> RemarksDistances { get; set; } = new List<string>();

        public List<string> AdditionalFacilityDetails { get; set; } = new List<string>();
        public List<string> AddonDetails { get; set; } = new List<string>();

        public string PackageAmountSharingAdult { get; set; }
        public string PackageAmountSharingChild { get; set; }
        public string PackageAmountSharingInfant { get; set; }
        public string IndividualAdditionalFacilityAmount { get; set; }
        public string AFAdultPerPerson { get; set; }
        public string AFChildPerPerson { get; set; }
        public string AFInfantPerPerson { get; set; }
        public string AddonAmount { get; set; }
        public string AdultPerPerson { get; set; }
        public string ChildPerPerson { get; set; }
        public string InfantPerPerson { get; set; }
        public float DiscountAmount { get; set; }
        public string GrandTotal { get; set; }
    }
}


using TMG.Data;
using TMG.Models.DemandModels;
using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.DemandControllers
{
    public class DemandController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public DemandController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Flight()
        {
            List<FlightDemandModel> model = new();

            var packages = _dbContext.PackagesWithoutTrash
            .Include(e => e.Company)
                    .Select(e => new
                    {
                        e.Name,
                        e.Company.CompanyCode,
                        e.PackageId
                    }).ToList();

            var accomodations = _dbContext.PackageAccomodtions
            .Include(e => e.Place)
            .ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash
                   .Include(e => e.Package)
                   .ThenInclude(e => e.Company)
                   .Select(e => new
                   {
                       e.FamilyHeadId,
                       e.PackageId
                   });

            for (int i = 0; i < packages.Count; i++)
            {
                var currPackage = packages.ElementAt(i);
                var currAccommodations = accomodations.Where(e => e.PackageId == currPackage.PackageId);

                var firstAccommodation = currAccommodations.OrderBy(e => e.PackageAccomodationsId).ElementAtOrDefault(0);
                var lastAccommodation = currAccommodations.OrderByDescending(e => e.PackageAccomodationsId).ElementAtOrDefault(0);

                int bookingsCount = families.Where(e => e.PackageId == currPackage.PackageId).Count();

                FlightDemandModel _model = new()
                {
                    SNo = i + 1,
                    Company = currPackage.CompanyCode,
                    Package = currPackage.Name,
                    NumberOfPassengers = bookingsCount,
                    OutboundPlace = firstAccommodation.Place.PlaceName,
                    OutboundArrival = firstAccommodation.CheckIn,
                    InboundPlace = lastAccommodation.Place.PlaceName,
                    InboundDeparture = lastAccommodation.CheckOut
                };

                model.Add(_model);
            }

            return View(model);
        }

        public IActionResult Train()
        {

            List<TrainDemandModel> model = new();

            var packages = _dbContext.PackagesWithoutTrash
            .Include(e => e.Company)
                    .Select(e => new
                    {
                        e.Name,
                        e.Company.CompanyCode,
                        e.PackageId
                    }).ToList();

            var accomodations = _dbContext.PackageAccomodtions
            .Include(e => e.Place)
            .ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash
                   .Include(e => e.Package)
                   .ThenInclude(e => e.Company)
                   .Select(e => new
                   {
                       e.FamilyHeadId,
                       e.PackageId
                   });

            for (int i = 0; i < packages.Count; i++)
            {
                var currPackage = packages.ElementAt(i);
                var currAccommodations = accomodations.Where(e => e.PackageId == currPackage.PackageId);
                int bookingsCount = families.Where(e => e.PackageId == currPackage.PackageId).Count();

                const int MEDINAH_ID = 2;

                TrainDemandModel _model;
                if (currAccommodations.ElementAt(0)?.PlaceId == MEDINAH_ID)
                {
                    _model = new(i + 1, currPackage.CompanyCode,
                    currPackage.Name, bookingsCount, currAccommodations.ElementAt(0).CheckOut, ""
                    );
                }
                else
                {
                    var medinahAccommodation = currAccommodations.FirstOrDefault(e => e.PlaceId == MEDINAH_ID);

                    _model = new(i + 1, currPackage.CompanyCode,
                    currPackage.Name, bookingsCount, "", medinahAccommodation?.CheckIn
                    );
                }

                model.Add(_model);
            }

            return View(model);
        }

        public IActionResult Room()
        {
            string tableHeaders = "";
            var rooms = _dbContext.RoomsWithoutTrash.ToList();

            for (int i = 0; i < rooms.Count; i++)
            {
                tableHeaders += $"<th>{rooms.ElementAt(i).RoomName}</th>";
            }

            RoomDemandModel model = new(tableHeaders);

            var packages = _dbContext.PackagesWithoutTrash
                            .Include(e => e.Company)
                            .Select(e => new
                            {
                                e.Name,
                                e.Company.CompanyCode,
                                e.PackageId
                            }).ToList();

            var packageAccommodations = _dbContext.PackageAccomodtions.Include(e => e.Accomodation).ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash
            .Select(e => new
            {
                e.FamilyHeadId,
                e.PackageId
            }).ToList();

            var familyAdditionalFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.FamilyHead)
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.IndividiualFacilityDescription)
                                            .Select(e => new
                                            {
                                                e.FamilyHeadId,
                                                e.FamilyHead.PackageId,
                                                e.AdditionalFacilitiesId,
                                                e.AdditionalFacilities.PlaceId,
                                                e.AdditionalFacilities.AccomodationId,
                                                e.AdditionalFacilities.Room.PAX,
                                                e.AdditionalFacilities.RoomId
                                            })
                                            .ToList();
            const int MAKKAH_ID = 1;
            const int MEDINA_ID = 2;
            const int AZIZIA_ID = 4;

            for (int i = 0; i < packages.Count; i++)
            {
                var currPackage = packages.ElementAt(i);

                var makkahAccommodation = packageAccommodations
                                    .FirstOrDefault(e => e.PlaceId == MAKKAH_ID && e.PackageId == currPackage.PackageId);
                var medinaAccommodation = packageAccommodations
                                    .FirstOrDefault(e => e.PlaceId == MEDINA_ID && e.PackageId == currPackage.PackageId);
                var aziziaAccommodation = packageAccommodations
                                    .FirstOrDefault(e => e.PlaceId == AZIZIA_ID && e.PackageId == currPackage.PackageId);

                var hasMakkahAccommodation = makkahAccommodation is not null;

                var hasMedinaAccommodation = medinaAccommodation is not null;

                var hasAziziaAccommodation = aziziaAccommodation is not null;

                float makkahMembersInSharing = 0;
                float medinaMembersInSharing = 0;
                float aziziaMembersInSharing = 0;

                float makkahMembersTotal = 0;
                float medinaMembersTotal = 0;
                float aziziaMembersTotal = 0;

                var packageFamilies = families.Where(e => e.PackageId == currPackage.PackageId).ToList();
                RoomDemandEntryModel _makkahModel = new();
                RoomDemandEntryModel _medinaModel = new();
                RoomDemandEntryModel _aziziaModel = new();

                if (hasMakkahAccommodation)
                {
                    _makkahModel = new()
                    {
                        SNo = i + 1,
                        Place = "Makkah",
                        CheckIn = makkahAccommodation.CheckIn,
                        CheckOut = makkahAccommodation.CheckOut,
                        Hotel = makkahAccommodation.AccomodationName,
                        Package = currPackage.Name,
                        Company = currPackage.CompanyCode,
                        Type = makkahAccommodation.Accomodation.AccomodationName
                    };
                }
                if (hasAziziaAccommodation)
                {
                    _aziziaModel = new()
                    {
                        SNo = i + 3,
                        Place = "Azizia",
                        CheckIn = aziziaAccommodation.CheckIn,
                        CheckOut = aziziaAccommodation.CheckOut,
                        Hotel = aziziaAccommodation.AccomodationName,
                        Package = currPackage.Name,
                        Company = currPackage.CompanyCode,
                        Type = aziziaAccommodation.Accomodation.AccomodationName
                    };
                }
                if (hasMedinaAccommodation)
                {
                    _medinaModel = new()
                    {
                        SNo = i + 2,
                        Place = "Medina",
                        CheckIn = medinaAccommodation.CheckIn,
                        CheckOut = medinaAccommodation.CheckOut,
                        Hotel = medinaAccommodation.AccomodationName,
                        Package = currPackage.Name,
                        Company = currPackage.CompanyCode,
                        Type = medinaAccommodation.Accomodation.AccomodationName
                    };
                }

                for (int j = 0; j < packageFamilies.Count; j++)
                {
                    var currMember = packageFamilies.ElementAt(j);
                    var memberFacilities = familyAdditionalFacilities.Where(e => e.FamilyHeadId == currMember.FamilyHeadId);

                    if (memberFacilities.Where(e => e.PlaceId == MAKKAH_ID).Count() == 0)
                    {
                        makkahMembersInSharing += 0.2f;
                    }
                    if (memberFacilities.Where(e => e.PlaceId == MEDINA_ID).Count() == 0)
                    {
                        medinaMembersInSharing += 0.2f;
                    }
                    if (memberFacilities.Where(e => e.PlaceId == AZIZIA_ID).Count() == 0)
                    {
                        aziziaMembersInSharing += 0.2f;
                    }

                }

                for (int k = 0; k < rooms.Count; k++)
                {
                    var currRoomType = rooms.ElementAt(k);
                    float makkahBookings = 0f;
                    float medinaBookings = 0f;
                    float aziziaBookings = 0f;

                    var familiesInRoomTypeMakkah = familyAdditionalFacilities
                                            .Where(e => e.RoomId == currRoomType.RoomId && e.PlaceId == MAKKAH_ID && e.PackageId == currPackage.PackageId);
                    var familiesInRoomTypeMedina = familyAdditionalFacilities
                                            .Where(e => e.RoomId == currRoomType.RoomId && e.PlaceId == MEDINA_ID && e.PackageId == currPackage.PackageId);

                    
                    var familiesInRoomTypeAzizia = familyAdditionalFacilities
                                            .Where(e => e.RoomId == currRoomType.RoomId && e.PlaceId == AZIZIA_ID && e.PackageId == currPackage.PackageId);

                    makkahBookings = (float)familiesInRoomTypeMakkah.Count() / (float)currRoomType.PAX;
                    medinaBookings = (float)familiesInRoomTypeMedina.Count() / (float)currRoomType.PAX;
                    aziziaBookings = (float)familiesInRoomTypeAzizia.Count() / (float)currRoomType.PAX;

                    makkahMembersTotal += makkahBookings;
                    medinaMembersTotal += medinaBookings;
                    aziziaMembersTotal += aziziaBookings;

                    _makkahModel.Demand.Add(makkahBookings);
                    _medinaModel.Demand.Add(medinaBookings);
                    _aziziaModel.Demand.Add(aziziaBookings);
                }

                if (hasMakkahAccommodation)
                {
                    _makkahModel.Sharing = makkahMembersInSharing;
                    _makkahModel.Total = makkahMembersInSharing + makkahMembersTotal;
                    model.Entries.Add(_makkahModel);
                }
                if (hasMedinaAccommodation)
                {
                    _medinaModel.Sharing = medinaMembersInSharing;
                    _medinaModel.Total = medinaMembersInSharing + medinaMembersTotal;

                    model.Entries.Add(_medinaModel);
                }
                if (hasAziziaAccommodation)
                {
                    _aziziaModel.Sharing = aziziaMembersInSharing;
                    _aziziaModel.Total = aziziaMembersInSharing + aziziaMembersTotal;

                    model.Entries.Add(_aziziaModel);
                }

            }

            // return Ok(model);
            return View(model);
        }

    }
}

namespace TMG.Models.DemandModels
{
    public record FlightDemandModel
    {
        public int SNo { get; set; }
        public string Company { get; set; }
        public string Package { get; set; }
        public int NumberOfPassengers { get; set; }
        public string OutboundPlace { get; set; }
        public string OutboundArrival { get; set; }
        public string InboundPlace { get; set; }
        public string InboundDeparture { get; set; }
    }

    public record TrainDemandModel(int SNo, string Company, string Package, int NumberOfPassengers,
        string OutboundArrival, string InboundDeparture);

    public record RoomDemandModel(string RoomTableHeaders)
    {
        public List<RoomDemandEntryModel> Entries { get; set; } = new();
    }

    public record RoomDemandEntryModel
    {
        public int SNo { get; set; }
        public string Company { get; set; }
        public string Package { get; set; }
        public string Place { get; set; }
        public string Hotel { get; set; }
        public string Type { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public float Sharing { get; set; }
        public float Total { get; set; }

        public List<float> Demand { get; set; } = new();
        public List<float> RemainingStocks { get; set; } = new();
        public float RemainingStockTotal { get; set; } = new();
    };
}
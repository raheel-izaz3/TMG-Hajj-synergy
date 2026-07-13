using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AirlineModels;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyHeadReportModels;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using TMG.Models.AccountModels;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using TMG.Models.AccomodationModels;
using TMG.Models.Updated_Accounts;
using static TMG.Controllers.ReportController;
using ImageMagick;
using System.Runtime.Intrinsics.X86;


namespace TMG.Controllers
{
    public class FamilyHeadReportController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;


        public FamilyHeadReportController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }

        //public IActionResult Test()
        //{
        //    return Ok(new
        //    {
        //        BasePrice = PackagePriceManager.GetBasePrice(_dbContext, 255),
        //        FlightPrice = PackagePriceManager.GetFlightPrice(_dbContext, 255),
        //        TrainPrice = PackagePriceManager.GetTrainPrice(_dbContext, 255),
        //        RoomPrice = PackagePriceManager.GetRoomPrice(_dbContext, 255),
        //        TotalPrice = PackagePriceManager.GetTotalPrice(_dbContext, 255),
        //    });
        //}

        public IActionResult DataFormv3(int id)
        {

            try
            {
                ViewBag.HajjSeason = _dbContext.HajjSeasons
                                    .FirstOrDefault(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

                ViewBag.Documents = _dbContext.RequiredDocumentsWithoutTrash;

                var membersOfFamily = _dbContext.FamilyMembers
                                        .Include(e => e.Head)
                                        .Include(e => e.FamilyHead)
                                        .ThenInclude(e => e.RelationWithMehram)
                                        .Include(e => e.FamilyHead)
                                        .ThenInclude(e => e.Fiqah)
                                        .Include(e => e.FamilyHead)
                                        .ThenInclude(e => e.Package)
                                        .ThenInclude(e => e.Company)
                                        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                                        .ToList();

                ViewBag.TermsAndCondition = _dbContext.Packages
                    .FirstOrDefault(e => e.PackageId == membersOfFamily.ElementAtOrDefault(0).FamilyHead.PackageId)
                    .TermsnCondition;

                DataFormModel superModel = new DataFormModel();
                superModel.BaseCurrency = membersOfFamily.ElementAtOrDefault(0).FamilyHead.Currencies.ToString();
                superModel.MunazzimName = _dbContext.MunazzimCompanies
                                            .Select(e => new
                                            {
                                                e.CompanyId,
                                                e.Munazzim.Name
                                            })
                                            .FirstOrDefault(e => e.CompanyId == membersOfFamily.ElementAt(0).FamilyHead.Package.CompanyId)?
                                            .Name;

                superModel.BasePackagePrice = PackagePriceManager.GetTotalBasePrice(_dbContext, id);
                superModel.FlightPrice = PackagePriceManager.GetTotalFlightPrice(_dbContext, id);
                superModel.TrainPrice = PackagePriceManager.GetTotalTrainPrice(_dbContext, id);
                superModel.RoomPrice = PackagePriceManager.GetTotalRoomPrice(_dbContext, id);
                superModel.AddonsPrice = PackagePriceManager.GetTotalAddonsPrice(_dbContext, id);
                superModel.AddonsDiscount = PackagePriceManager.GetTotalAddonsDiscount(_dbContext, id);
                superModel.TotalDiscount = PackagePriceManager.GetTotalDiscount(_dbContext, id);
                superModel.TotalPackagePrice = PackagePriceManager.GetOverallTotalPrice(_dbContext, id);

                for (int i = 0; i < membersOfFamily.Count; i++)
                {
                    FamilyMembers currMember = membersOfFamily.ElementAt(i);

                    var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                    var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

                    string cId =
                            $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
                            $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                            $".{currMember.FamilyHead.Package.Name}";

                    List<FamilyHeadDocument> familyHeadDocuments = _dbContext.FamilyHeadDocuments
                                                            .Include(e => e.RequiredDocument)
                                                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.RequiredDocument.Trashed)
                                                            .ToList();

                    FamilyHeadAddress headAddress = _dbContext
                                        .FamilyHeadAddresses
                                        .Include(e => e.Tehsil)
                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                    FamilyHeadContact preferredContact = _dbContext
                        .FamilyHeadContacts
                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                    FamilyHeadContact otherContact = _dbContext
                        .FamilyHeadContacts
                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

                    FamilyHeadNominee headNominee = _dbContext
                        .FamilyHeadNominees
                        .Include(e => e.NomineeRelation)
                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                    var trainings = _dbContext
                        .PackageTrainings
                        .Include(e => e.Training)
                        .Where(e => e.PackageId == currMember.FamilyHead.PackageId).ToList();

                    Flight flight = _dbContext.Flights
                        .FirstOrDefault(e => e.FlightId == currMember.FamilyHead.FlightId);

                    var gifts = _dbContext
                        .PackageGifts
                        .Include(e => e.Gift)
                        .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
                        .ToList();

                    var extraFacilities = _dbContext
                        .FamilyHeadExtras
                        .Include(e => e.ExtraFacilityDescription)
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .ToList();

                    string presentPostalAddress = $"{headAddress?.Address}, {headAddress?.City}, {headAddress?.ZipCode}, {headAddress?.Country}";
                    string packageNameDuration = $"{currMember?.FamilyHead.Package.Name} " +
                        $"({currMember?.FamilyHead.Package.Days})";

                    var officialAddress = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberAddressTypeId == 2);
                    var officialContact = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberContactTypeId == 2);
                    var officialEmail = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberEmailTypeId == 2);

                    string officialAddressString = officialAddress is not null ?
                                            $"{officialAddress?.Address}, {officialAddress?.City}, {officialAddress?.ZipCode}, {officialAddress?.Country}" : "";

                    string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred)?.Email;

                    UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                    DataFormModelEntry model = new DataFormModelEntry
                    {
                        Documents = familyHeadDocuments,
                        SignedDate = DateTimeManager.GetDate(userHajjAgreement?.SignedOn),
                        CID = cId,
                        ShowSignature = userHajjAgreement?.SignDataForm ?? false,
                        SignatureBase64 = currMember?.FamilyHead.SignatureBase64,
                        ApplicantType = currMember?.FamilyHead.ApplicantType.ToString(),
                        FamilyHeadImage = currMember?.FamilyHead.MemberImage ?? "",
                        HajjAppNumber = currMember?.FamilyHead.HajjApplicationNumber ?? "-",
                        EnrollmentNumber = currMember?.FamilyHead.ENumber ?? "",
                        MoraPackageNumber = currMember?.FamilyHead.Package.PackageNumber,
                        PackageNameDuration = packageNameDuration,
                        HGOName = currMember?.FamilyHead.Package.Company.CompanyName,
                        SurName = currMember?.FamilyHead.SurName,
                        GivenName = currMember?.FamilyHead.GivenName,
                        FatherHusbandName = currMember?.FamilyHead.FatherHusbandName,
                        PassportNumber = currMember?.FamilyHead.PassportNumber,
                        PassportDateOfExpiry = currMember?.FamilyHead.PassportDateOfExpiry?.ToString("dd-MMM-yyyy"),
                        DateOfBirth = currMember?.FamilyHead.DateOfBirth.ToString("dd-MMM-yyyy"),
                        CNIC = currMember?.FamilyHead.CNIC,
                        BloodGroup = currMember?.FamilyHead.BloodGroup,
                        Gender = currMember?.FamilyHead.Gender,
                        Fiqah = currMember?.FamilyHead.Fiqah.Name.ToString(),
                        IsMarried = (bool)currMember?.FamilyHead.IsMarried ? "YES" : "NO",
                        YearsPerformedHajj = currMember?.FamilyHead.YearsPerformedHajj,
                        PresentPostalAddress = presentPostalAddress,
                        TehsilCode = headAddress?.Tehsil?.Name,
                        MobileNumber = preferredContact?.Contact,
                        TelephoneNumber = otherContact?.Contact,

                        Email = email,
                        OfficialAddress = officialAddressString,
                        OfficialContact = officialContact?.Contact,
                        EmployerBusinessName = currMember.FamilyHead.EmployedAt,
                        Designation = currMember.FamilyHead.Designation,
                        OfficialEmail = officialEmail?.Email,

                        PerformedHajjInLast5Years = (bool)currMember?.FamilyHead.PerformedHajjInLastFiveYears ? "YES" : "NO",
                        WantToHajjEBadal = (bool)currMember?.FamilyHead.WantToPerformHajjEBadal ? "YES" : "NO",
                        WantToPerformHajjAsGroupWorker = (bool)currMember?.FamilyHead.WantToPerformAsGroupWorker ? "YES" : "NO",
                        NomineeName = headNominee?.Name,
                        NomineeRelation = headNominee?.NomineeRelation?.Relation,
                        NomineeCNIC = headNominee?.CNIC,
                        NomineeContact = headNominee?.Contact,
                        NomineeTelephone = "",

                        StayDuration = currMember?.FamilyHead.Package.HajjDuration,
                        SubAgaintAppNumber = currMember?.FamilyHead.SubstitutedAgainstAppNumber,

                        MedinahArrival = currMember?.FamilyHead.Package.MedinahArrival,
                        FlightType = currMember?.FamilyHead.Package.FlightType,
                        FlightClass = currMember?.FamilyHead.FlightClass,
                        TicketPeriod = currMember?.FamilyHead.Package.FlightTicketPeriod,
                        POD = currMember?.FamilyHead.POD,
                        // TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString(),

                        PackageTicketPricePkr = currMember?.FamilyHead.Package.FlightTicketPricePkr,
                        PackageTicketPriceSar = currMember?.FamilyHead.Package.FlightTicketPriceSar,
                        PackageTicketPriceUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd,

                        FlightTicketDifferencesPkr = currMember?.FamilyHead.FlightTicketDifferencesPkr,
                        FlightTicketDifferencesSar = currMember?.FamilyHead.FlightTicketDifferencesSar,
                        FlightTicketDifferencesUsd = currMember?.FamilyHead.FlightTicketDifferencesUsd,

                        IntercityTransport = currMember?.FamilyHead.Package.TransportIntercity,
                        AirportTransport = currMember?.FamilyHead.Package.AirportTransport,
                        MashairTrasport = currMember?.FamilyHead.Package.TransportWithMashair,
                        WithinCity = currMember?.FamilyHead.Package.WithInCity
                    };

                    var routes = new List<DataFormModelEntry>();

                    if (fm.FamilyHead.RouteAsPerPackage == true)
                    {
                        routes = _dbContext.TravelRouteStops
                            .Include(e => e.Route)
                            .Include(e => e.Vehicle)
                            .Where(e => e.TravelRouteId == currMember.FamilyHead.Package.TravelRouteId)
                            .OrderBy(e => e.CheckIn)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.TravelRouteStopsId,
                                TravelRoutes = e.Route.StartPlace + " to " + e.Route.Destination,
                                CheckIn = e.CheckIn,
                                CheckOut = e.CheckOut,
                                VehicleName = e.Vehicle.VehicleType
                            })
                            .ToList();
                    }
                    else
                    {
                        routes = _dbContext.FamilyHeadTransports
                            .Include(e => e.Route)
                            .Include(e => e.Vehicle)
                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                            .OrderBy(e => e.CheckIn)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.FamilyHeadTransportId,
                                TravelRoutes = e.Route.StartPlace + " to " + e.Route.Destination,
                                CheckIn = e.CheckIn,
                                //CheckOut = e.CheckOut,
                                VehicleName = e.Vehicle.VehicleType
                            })
                            .ToList();
                    }

                    if (fm.IsHead)
                    {
                        ViewBag.Routes = routes;
                    }

                    var flights = new List<DataFormModelEntry>();

                    if (fm.FamilyHead.FlightAsPerPackage == true)
                    {
                        flights = _dbContext.PackageFlights
                            .Include(e => e.Airline)
                            .Where(e => e.PackageId == currMember.FamilyHead.PackageId && e.IsPreffered == true)
                            .OrderBy(e => e.DepartureDate)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.PackageFlightsId,
                                Origin = e.Origin,
                                Destination = e.Destination,
                                Airline = e.Airline.Name,
                                ArrivalDateTime = e.ArrivalDate,
                                DepartureDateTime = e.DepartureDate,
                                DepartureTime = e.DepartureTime,
                                ArrivalTime = e.ArrivalTime,
                            })
                            .ToList();
                    }
                    else
                    {
                        flights = _dbContext.FamilyHeadFlight
                            .Include(e => e.Airline)
                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                            .OrderBy(e => e.DepartureDateTime)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.FamilyHeadFlightId,
                                Origin = e.Origin,
                                Destination = e.Destination,
                                Airline = e.Airline.Name,
                                ArrivalDateTime = e.ArrivalDateTime,
                                DepartureDateTime = e.DepartureDateTime,
                                ArrivalTime = e.ArrivalTime,
                                DepartureTime = e.DepartureTime
                            })
                            .ToList();
                    }

                    if (fm.IsHead)
                    {
                        ViewBag.Flights = flights;
                    }

                    var trains = new List<DataFormModelEntry>();

                    if (fm.FamilyHead.TrainAsPerPackage == true)
                    {
                        trains = _dbContext.PackageTrains
                            .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
                            .OrderBy(e => e.DepartureDate)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.PackageTrainsId,
                                TrainOrigin = e.Origin,
                                TrainDestination = e.Destination,
                                TrainClass = e.TrainClass.ToString(),
                                TrainArrivalDateTime = e.ArrivalDate,
                                TrainDepartureDateTime = e.DepartureDate,
                                TrainArrivalTime = e.ArrivalTime,
                                TrainDepartureTime = e.DepartureTime
                            })
                            .ToList();
                    }
                    else
                    {
                        trains = _dbContext.FamilyHeadTrain
                            .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                            .OrderBy(e => e.DepartureDateTime)
                            .Select(e => new DataFormModelEntry
                            {
                                Id = e.FamilyHeadTrainId,
                                TrainOrigin = e.Origin,
                                TrainDestination = e.Destination,
                                TrainClass = e.TrainClass.ToString(),
                                TrainArrivalDateTime = e.ArrivalDateTime,
                                TrainDepartureDateTime = e.DepartureDateTime,
                                TrainArrivalTime = e.ArrivalTime,
                                TrainDepartureTime = e.DepartureTime,
                            })
                            .ToList();
                    }

                    if (fm.IsHead)
                    {
                        ViewBag.Trains = trains;
                    }

                    model.Remarks = _dbContext.FamilyHeadRemarks.Where(e => e.FamilyHeadId == currMember.FamilyHeadId).Select(e => e.Remarks).ToList();


                    for (int k = 0; k < extraFacilities.Count; k++)
                    {
                        FamilyHeadExtra currExtra = extraFacilities.ElementAt(k);

                        model.ExtraFacilities.Add(
                            new DataFormExtraFacility(
                                currExtra.ExtraFacilityDescription?.Description,
                                0f,
                                0f,
                                0f)
                            );
                    }


                    for (int k = 0; k < gifts.Count; k++)
                    {
                        PackageGifts currGift = gifts.ElementAt(k);
                        model.Gifts.Add(new DataFormGift(currGift.Gift.Giveaway));
                    }

                    if (currMember.FamilyHead.Gender == Gender.Male)
                    {
                        FamilyHead mehram = _dbContext.FamilyHeads.FirstOrDefault(e => e.MehramFamilyHeadId == currMember.FamilyHeadId && !e.Trashed
                                                                    && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
                        model.AreYouMehramOfALady = mehram is not null ? "YES" : "NO";
                    }
                    else
                    {
                        FamilyHead mehram = _dbContext.FamilyHeads
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHead.MehramFamilyHeadId && !e.Trashed
                                                                && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
                        model.AreYouMehramOfALady = "-";
                        model.MehramAppNumber = mehram?.HajjApplicationNumber;
                        model.MehramRelationship = currMember?.FamilyHead.RelationWithMehram?.Relation;
                        model.MehramSignatureBase64 = mehram?.SignatureBase64;
                        model.MehramName = mehram?.FullName;
                    }

                    for (int j = 0; j < trainings.Count; j++)
                    {
                        PackageTrainings _t = trainings.ElementAt(j);
                        model.TrainingSessions.Add(new DataFormTrainingSession(
                            _t.Training.TrainingSession, _t.Training.ExpectedTrainingVenue,
                            _t.Training.ExpectedTrainingTime
                            ));
                    }


                    PackageAccomodations aziziaAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .Include(e => e.Hotel)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 4);

                    var makkahAccomadation = _dbContext.PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .Include(e => e.Hotel)

                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);

                    PackageAccomodations secondMakkahAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .Include(e => e.Hotel)
                        .OrderByDescending(e => e.PackageAccomodationsId)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);


                    string makkahRoomTypeDescription = null;
                    string medinahRoomTypeDescription = null;
                    string aziziaRoomTypeDescription = null;
                    string minaRoomTypeDescription = null;
                    string secondMakkahRoomTypeDescription = null;

                    if (makkahAccomadation != null)
                    {
                        if (makkahAccomadation.PackageAccomodationsId == secondMakkahAccomadation.PackageAccomodationsId)
                        {
                            secondMakkahAccomadation = null;
                        }
                    }

                    PackageAccomodations medinaAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 2);

                    PackageAccomodations minaAccomadation = _dbContext
                        .PackageAccomodtions
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 3);

                    if (aziziaAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                                    .Include(e => e.AdditionalFacilities)
                                                                    .ThenInclude(e => e.Room)
                                                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                                    && e.AdditionalFacilities.PlaceId == 4);

                        model.AziziaAccomodation = new DataFormAzizia(
                            aziziaAccomadation.Accomodation.AccomodationName,
                            aziziaAccomadation.Hotel?.HotelName ?? aziziaAccomadation.AccomodationName,
                            aziziaAccomadation.AccomodationCategory.Category,
                            aziziaAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? aziziaAccomadation.CheckIn,
                            aziziaAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? aziziaAccomadation.CheckOut,
                            aziziaAccomadation.Days,
                            aziziaAccomadation.DistanceInMeters?.ToString(),
                            aziziaAccomadation.FoodPackage?.Package,
                            //_indId is null ? aziziaAccomadation.LoadPerRoom : _indId.AdditionalFacilities.Room.PAX,
                            //_indId is null ? aziziaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                            aziziaAccomadation.Shuttle,
                            aziziaAccomadation.ReligiousLectures,
                            _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : ""

                        );

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    //&& e.IndividualFacilityDescription.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                    && e.IndividualFacilityDescription.PlaceId == aziziaAccomadation.PlaceId)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        aziziaRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.AziziaRoom = aziziaRoomTypeDescription;
                    }

                    if (makkahAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                .Include(e => e.AdditionalFacilities)
                                                .ThenInclude(e => e.Room)
                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 1);

                        model.MakkahAccomodation = new DataFormAccomodation(
                        makkahAccomadation.GroupZiarat,
                        makkahAccomadation.Days,
                        makkahAccomadation.DistanceInMeters?.ToString(),
                        makkahAccomadation.FoodPackage?.Package,
                        //_indId is null ? makkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                        makkahAccomadation.Accomodation.AccomodationName,
                        makkahAccomadation.Hotel?.HotelName ?? makkahAccomadation.AccomodationName,
                        makkahAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? makkahAccomadation.CheckIn,
                        makkahAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? makkahAccomadation.CheckOut,
                        //_indId is null ? makkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        makkahAccomadation.Sharing,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                        makkahAccomadation.AccomodationCategory.Category);

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    //&& e.IndividualFacilityDescription.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                    && e.IndividualFacilityDescription.PlaceId == makkahAccomadation.PlaceId)
                            .Include(e => e.IndividualFacilityDescription)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        makkahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.MakkahRoom = makkahRoomTypeDescription;
                    }

                    if (secondMakkahAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                .Include(e => e.AdditionalFacilities)
                                                .ThenInclude(e => e.Room)
                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 1);

                        model.SecondMakkahAccomodation = new DataFormAccomodation(
                        secondMakkahAccomadation.GroupZiarat,
                        secondMakkahAccomadation.Days,
                        secondMakkahAccomadation.DistanceInMeters?.ToString(),
                        secondMakkahAccomadation.FoodPackage?.Package,
                        //_indId is null ? secondMakkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                        secondMakkahAccomadation.Accomodation.AccomodationName,
                        secondMakkahAccomadation.Hotel?.HotelName ?? secondMakkahAccomadation.AccomodationName,
                        secondMakkahAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? secondMakkahAccomadation.CheckIn,
                        secondMakkahAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? secondMakkahAccomadation.CheckOut,
                        //_indId is null ? secondMakkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        secondMakkahAccomadation.Sharing,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                        secondMakkahAccomadation.AccomodationCategory.Category

                    );
                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    //&& e.IndividualFacilityDescription.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                    && e.IndividualFacilityDescription.PlaceId == secondMakkahAccomadation.PlaceId)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        secondMakkahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.SecondMakkah = secondMakkahRoomTypeDescription;

                    }

                    if (medinaAccomadation is not null)
                    {
                        var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                .Include(e => e.AdditionalFacilities)
                                                .ThenInclude(e => e.Room)
                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 2);

                        model.MedinaAccomodation = new DataFormAccomodation(
                        medinaAccomadation.GroupZiarat,
                        medinaAccomadation.Days,
                        medinaAccomadation.DistanceInMeters?.ToString(),
                        medinaAccomadation.FoodPackage?.Package,
                        //_indId is null ? medinaAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                        medinaAccomadation.Accomodation.AccomodationName,
                        medinaAccomadation.Hotel?.HotelName ?? medinaAccomadation.AccomodationName,
                        medinaAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? medinaAccomadation.CheckIn,
                        medinaAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? medinaAccomadation.CheckOut,
                        //_indId is null ? medinaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        medinaAccomadation.Sharing,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                        medinaAccomadation.AccomodationCategory.Category
                    );

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    //&& e.IndividualFacilityDescription.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                    && e.IndividualFacilityDescription.PlaceId == medinaAccomadation.PlaceId)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        medinahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.MedinahRoom = medinahRoomTypeDescription;

                        //  return Ok(new {
                        //         model.MedinaAccomodation
                        //     });
                    }

                    if (minaAccomadation is not null)
                    {
                        model.MinaAccomodation = new DataFormMina(
                        minaAccomadation.Package.CategoryZone,
                        minaAccomadation.FoodPackage?.Package,
                        minaAccomadation.Bedding,
                        minaAccomadation.Camp,
                        minaAccomadation.Arafat,
                        minaAccomadation.ReligiousLectures
                       );

                        var facilityDescription = _dbContext.PackageCategoryFacilities
                            .Include(e => e.IndividualFacilityDescription)
                            .Where(e => !e.IndividualFacilityDescription.Trashed
                                    //&& e.IndividualFacilityDescription.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                    && e.IndividualFacilityDescription.PlaceId == 3)
                            .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                        minaRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                        ViewBag.MinaRoom = minaRoomTypeDescription;
                    }

                    superModel.Entries.Add(model);

                }

                return View(superModel);
            }
            catch (Exception e)
            {
                //return Ok(new
                //{
                //    e.Message,
                //});
                throw e;
            }

        }

        public IActionResult ConvertToJpeg(string form)
        {
            var converter = new CoreHtmlToImage.HtmlConverter();
            //form = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head>\r\n\r\n    <link rel=\"icon\" type=\"image/x-icon\" href=\"/favicon.ico\">\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.0.1/css/bootstrap.min.css\" integrity=\"sha512-Ez0cGzNzHR1tYAv56860NLspgUGuQw16GiOOp/I2LuTmpSK9xDXlgJz3XN4cnpXWDmkNBKXR/VDMTCnAaEooxA==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\">\r\n\r\n    <title>TMG Report</title>\r\n    <style>\r\n        @media print {\r\n            .hideInPrint {\r\n                display: none;\r\n            }\r\n        }\r\n\r\n        .page-break {\r\n            page-break-after: always;\r\n        }\r\n\r\n       \r\n    </style>\r\n\r\n</head>\r\n\r\n<body>\r\n    \r\n\r\n\r\n<style>\r\n    .user-img img,\r\n    .img-stamp {\r\n        height: 100%;\r\n        width: 100%;\r\n        object-fit: contain;\r\n    }\r\n\r\n    .user-img {\r\n        height: 150px;\r\n    }\r\n\r\n    .doc-approval table {\r\n        height: 100px;\r\n        text-transform: capitalize;\r\n        text-align: center;\r\n    }\r\n\r\n    .doc-approval table td:not(td[class='seal-sig-doc']) {\r\n        vertical-align: middle;\r\n    }\r\n\r\n    .urdu {\r\n        src: url('https://www.fontsaddict.com/fontface/jameel-noori-nastaleeq.ttf');\r\n        font-family: 'Jameel Noori Nastaleeq';\r\n    }\r\n\r\n    @media print {\r\n        .hide-in-print {\r\n            display: none;\r\n        }\r\n\r\n        .img-stamp {\r\n            object-fit: contain;\r\n        }\r\n\r\n        title {\r\n            display: none\r\n        }\r\n\r\n        @page {\r\nmargin: 0;\r\n            margin-top: 20px;\r\n            margin-inline: 30px;\r\n\r\n        }\r\n\r\n@top {\r\n            content: none;\r\n        }\r\n\r\n        @bottom {\r\n            content: none;\r\n        }\r\n\r\n        .seal-sig-doc p {\r\n            text-wrap: nowrap;\r\n            font-size: 6pt !important;\r\n            font-weight: 100 !important;\r\n        }\r\n\r\n        .seal-sig-doc {\r\n            overflow-y: hidden !important;\r\n        }\r\n\r\n        .stamp-img {\r\n            border-top: .5px solid !important;\r\n        }\r\n    }\r\n\r\n    .mr-2 {\r\n        margin-right: 1%;\r\n    }\r\n\r\n    * {\r\n        font-size: 8pt;\r\n        line-height: 12px;\r\n    }\r\n\r\n    .fillable {\r\n        font-family: monospace;\r\n        text-transform: uppercase;\r\n    }\r\n\r\n    table td {\r\n        border: 0.5px solid black !important;\r\n    }\r\n\r\n    .seal-sig-doc {\r\n        vertical-align: bottom !important;\r\n        margin: 0 !important;\r\n        padding: 0 !important;\r\n    }\r\n\r\n    .seal-sig-doc p {\r\n        text-wrap: nowrap;\r\n        font-size: xx-small;\r\n        font-weight: 100 !important;\r\n    }\r\n\r\n    .halaf-nama {\r\n        direction: rtl !important;\r\n    }\r\n\r\n    .page-break {\r\n        page-break-after: always;\r\n        /* or 'auto' or 'avoid' */\r\n    }\r\n</style>\r\n<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js\"></script>\r\n\r\n\r\n\r\n<div class=\"row hide-in-print p-3\">\r\n    <div>\r\n        <input type=\"checkbox\" name=\"coronaCheckBox\" onchange=\"hidediv(this)\" value=\"Corona\">\r\n        <label>Corona</label>\r\n    </div>\r\n    <div>\r\n        <input type=\"checkbox\" name=\"doctorCheckBox\" onchange=\"onDoctorCheck(this)\" value=\"none\">\r\n        <label>None</label>\r\n    </div>\r\n        <div>\r\n            <input type=\"checkbox\" name=\"doctorCheckBox\" onchange=\"onDoctorCheck(this)\" value=\"3\">\r\n            <label>Dr. Abdul Razzaq Siddiqui</label>\r\n        </div>\r\n        <div>\r\n            <input type=\"checkbox\" name=\"doctorCheckBox\" onchange=\"onDoctorCheck(this)\" value=\"4\">\r\n            <label>Dr. Abdul Farooq Ahmed</label>\r\n        </div>\r\n</div>\r\n\r\n<button onclick=\"btncapture()\">Capture</button>\r\n<div id=\"capture\">\r\n\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n    <section class=\"container-fluid med-form-main border border-dark p-3\">\r\n        <div class=\"wrapper\">\r\n            <h2 class=\"h5 text-center fw-bolder\"><u>MEDICAL CERTIFICATE - PRIVATE HAJJ SCHEME 2024</u></h2>\r\n            <h4 class=\"h4\">Instructions for Medical Officer</h4>\r\n            <ol>\r\n                <li>\r\n                    Doctors are requested to be careful while filling the medical certificate. Uncontrolled\r\n                    diabetes,uncontrolled hypertension, severeischemic, heart diseases, very old/ weak persons, disabled\r\n                    (without\r\n                    helper), applicants suffering from any contagious disease andpregnant ladies (who will be five month\r\n                    pregnant at the time of departure for Hajj) may not be declared fit for Hajj.\r\n                </li>\r\n                <li>\r\n                    A medical checkup may also be held at Haji Camp and applicants having above symptoms will not be\r\n                    allowed to travel and the rentof buildings at Makkah and Madina may also be deducted.\r\n                </li>\r\n                <li>\r\n                    Any case of medical repatriation arising out of a mis-declaration of medical fitness have to be\r\n                    funded by the applicant.\r\n                </li>\r\n            </ol>\r\n            <p class=\"h6\">\r\n                Note: ln case of mis-declaration or concealment of any medical condition, the Ministry reserves the\r\n                right to initiateproceeding against the applicant/ certifying doctor. Such concealment may culminate in\r\n                his deportation\r\n                on his expense and\r\n            </p>\r\n            <p class=\"text-capitalize\">\r\n                (to be attested by medical officer of any federal/provincial government/semi government/armed forces\r\n                /autonomous bodies/corporation's hospitals.)\r\n\r\n            </p>\r\n            <div class=\"row\">\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of doctor:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-name\"></p>\r\n                </div>\r\n                <div class=\"col-6 d-flex\">\r\n                    <span class=\"h6 text-capitalize  mr-2 text-nowrap\">name of hospital:</span>\r\n                    <p class=\"col border-bottom border-dark px-2 fillable\" id=\"doctor-hospital\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex\">\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">tehsil:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-tehsil\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">district:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-district\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">landline:</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-landline\"></p>\r\n\r\n                    <span class=\"h6 text-capitalize mr-2\">cell no.</span>\r\n                    <p class=\"col border-bottom border-dark fillable\" id=\"doctor-cell\"></p>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center\">\r\n                    <span class=\"h6 text-capitalize mr-2\">\r\n                        registration no of doctor with pakistan medical or dental\r\n                        council\r\n                    </span>\r\n                    <table class=\"col-3 border text-center text-capitalize\">\r\n                        <tbody><tr id=\"doctor-pmdc\">\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                            <td class=\"p-2 fillable\"></td>\r\n                        </tr>\r\n                    </tbody></table>\r\n                </div>\r\n                <div class=\"d-flex\">\r\n                    <p class=\"h6 text-nowrap mr-2\"> it is certified that i have examined Mr./Ms.</p>\r\n                    <span class=\"col border-bottom border-dark fillable\">AMAN's Member</span>\r\n                </div>\r\n                <div class=\"col-12 d-flex align-items-center mt-2\">\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">S/o, W/o, d/o </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">bearing CNIC </span>\r\n                    <span class=\"col border-bottom border-dark fillable\">42201-4367520-7</span>\r\n\r\n                    <span class=\"h6 text-capitalize  text-capitalize text-center mr-2\">&amp; Passport No </span>\r\n                    <span class=\"col border-bottom border-dark fillable\"></span>\r\n                </div>\r\n                <p>and my opinion is given below.</p>\r\n                <table class=\"table border\">\r\n                    <tbody><tr>\r\n                        <td class=\"fw-bold text-capitalize\">age</td>\r\n                        <td class=\"fw-bold text-capitalize\">blood group</td>\r\n                        <td class=\"text-capitalize\">allergy to following medicines chronic diseases</td>\r\n                        <td class=\"text-capitalize\">chronic diseases requiring regular medication</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\">13</td>\r\n                        <td class=\"fillable align-middle text-center\" rowspan=\"3\"></td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(i)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n                            <span>(ii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n                        </td>\r\n\r\n                    </tr>\r\n                    <tr>\r\n\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n                        <td style=\"padding-top: 12px;\">\r\n\r\n                            <span>(iii)</span>\r\n                            <span class=\"border-bottom border-dark d-inline-block w-50 fillable\">\r\n\r\n                            </span>\r\n\r\n                        </td>\r\n\r\n                    </tr>\r\n                </tbody></table>\r\n                \r\n                <div class=\"row\">\r\n                <div id=\"coronadiv\" style=\"display:none;\">\r\n                <h5 class=\"h5\">Detail of Corona Vaccine Administered </h5> \r\n                <div class=\"col\">\r\n                        <table class=\"table \">\r\n                            <tbody><tr>\r\n                                <td class=\"border-0 text-center\">Dose</td>\r\n                                <td class=\"border-0 text-center\">Covid-19 Vaccine Name </td>\r\n                                <td class=\"border-0 text-center\">Batch</td>\r\n                                <td class=\"border-0 text-center\">Date</td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">1st Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">2nd Dose</td>\r\n                                <td class=\"fillable \"></td>\r\n\r\n\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    1st Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    2nd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"h6 \">\r\n                                    3rd Booster\r\n                                    <small class=\"fw-light\">(if applicable)</small>\r\n                                </td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                                <td class=\"fillable \"></td>\r\n                            </tr>\r\n                        </tbody></table>\r\n\r\n                    </div>\r\n                  \r\n                </div>\r\n                      <div class=\"col-3 user-img\">\r\n                        <img src=\"/\">\r\n                    </div>\r\n                    <div class=\"row\">\r\n                        <p class=\"h6\" style=\"display:none;\">*Attach NADRA issued Corona Vaccination certificate </p>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table \">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-4\">applicant is fit for hajj journey</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div id=\"stampPlaceholder\" style=\"height: 100px;\"></div>\r\n                                        <img src=\"\" class=\"stamp-img \" height=\"100px\" id=\"doctor-seal-1\">\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n                        <div class=\"doc-approval col-6\">\r\n                            <table class=\"table\">\r\n                                <tbody><tr>\r\n                                    <td class=\"col-2\">can perform hajj with helper</td>\r\n                                    <td class=\"col-2\">requires the help of a wheel chair</td>\r\n                                    <td class=\"col-6 seal-sig-doc bprder-dark align-end overflow-hidden\">\r\n                                        <div style=\"height: 100px;\"></div>\r\n                                        <p class=\"fw-bold text-capitalize text-muted\">\r\n                                            seal and signature of the doctor\r\n                                        </p>\r\n                                    </td>\r\n                                </tr>\r\n                            </tbody></table>\r\n                        </div>\r\n\r\n                    </div>\r\n                    <p class=\"text-center h6 urdu\">\r\n                        حلف نامه\r\n                    </p>\r\n                    <p class=\"text-end urdu\">\r\n                        :میں حلفیہ اقرار کرتا / کرتی ہوں کہ\r\n                    </p>\r\n                    <ol class=\"halaf-nama urdu\">\r\n                        <li class=\"\">\r\n                            یں مسلمان ہوں اور حضرت محمد صلى الله عليه وسلم کو آخری نبی مانتا مانتی ہوں اور\r\n                            آپ ماہ کے آخری نبی ہونے پر دل و جان سے یقین رکھتا رکھتی ہوں۔\r\n                        </li>\r\n                        <li class=\"\">\r\n                            مزید براں اس بات کی تصدیق کرتا / کرتی ہوں کہ خاتم النبیین والمرسلین حضرت محمد صلى\r\n                            الله عليه وسلم کی مکمل حتمی اور غیر مشروط ختم نبوت پر ایمان رکھتا رکھتی ہوں اور یہ کہ میں نہ\r\n                            کسی ایسے شخص پر ایمان رکھتا رکھتی ہوں اور نہ اسے بطور پیغمبر یا مذہبی مصلح مجدد مانتا امانتی\r\n                            ہوں جس نے حضرت محمد صلى الله عليه وسلم کے بعد نبی یا پیغمبر ہونے کا کسی بھی مفہوم یا کسی بھی\r\n                            تشریح کے لحاظ سے دعوی کیا یا کرتا کرتی ہو اور نہ ہی میں قادیانی گروپ یالا ہوری گروپ جو خود\r\n                            کو احمدی یا کسی اور نام سے موسوم کرتے ہیں سے تعلق رکھتا رکھتی ہوں۔ اور ایسے دعوے دار پر لعنت\r\n                            بھیجتا بھیجتی ہوں، بلکہ کسی بھی ایسے دعوے دار با شمول مرزا غلام احمد قادیانی کو جھوٹا\r\n                            سمجھتا/ سمجھتی ہوں۔\r\n                        </li>\r\n                    </ol>\r\n                    <p class=\"h6 text-end d-flex urdu\">\r\n                        <span class=\"col border-bottom mx-3\">\r\n                                <img style=\"visibility: hidden;\" width=\"100\" height=\"50px\">\r\n\r\n\r\n                        </span>\r\n                        <span class=\"urdu\"> :دستخط درخواست گزار</span>\r\n                    </p>\r\n\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </section>\r\n    <div class=\"page-break\"></div>\r\n</div>\r\n<script>\r\n\r\nfunction btncapture() {\r\n    var form = document.documentElement.outerHTML;\r\n\r\n     $.ajax({\r\n         type: 'POST',\r\n         url: '/FamilyHeadReport/ConvertToJpeg',\r\n         data: {form : form},\r\n         success: function (data) {\r\n           /*  var link = document.createElement('a');\r\n            link.href = URL.createObjectURL(new Blob([data], { type: 'image/jpeg' }));\r\n            link.download = 'image.jpeg';\r\n            link.click();  */\r\n             console.log(data);\r\n         },\r\n         error: function (error) {\r\n              console.error('Error:', error);\r\n         }\r\n     });\r\n }\r\n /* function btncapture() {\r\n    $.ajax({\r\n        type: 'POST',\r\n        url: '/FamilyHeadReport/ConvertToJpeg',\r\n        contentType: 'application/json',\r\n        data: JSON.stringify({ form: document.documentElement.outerHTML }), // or the appropriate HTML content\r\n        \r\n        success: function (data) {\r\n            console.log(data);\r\n        },\r\n        error: function (error) {\r\n            console.error('Error:', error);\r\n        }\r\n    });\r\n} */\r\n    function onDoctorCheck(elem) {\r\n        const checkBoxes = document.getElementsByName(\"doctorCheckBox\")\r\n        const id = elem.value\r\n\r\n        const name = document.querySelectorAll(\"#doctor-name\")\r\n        const hospital = document.querySelectorAll(\"#doctor-hospital\")\r\n        const tehsil = document.querySelectorAll(\"#doctor-tehsil\")\r\n        const district = document.querySelectorAll(\"#doctor-district\")\r\n        const landline = document.querySelectorAll(\"#doctor-landline\")\r\n        const cell = document.querySelectorAll(\"#doctor-cell\")\r\n        const pmdc = document.querySelectorAll(\"#doctor-pmdc\")\r\n        const seal1 = document.querySelectorAll(\"#doctor-seal-1\")\r\n        const seal2 = document.querySelectorAll(\"#doctor-seal-2\")\r\n\r\n        if (elem.checked) {\r\n            for (let c of checkBoxes) {\r\n                if (c.value == id) {\r\n                    c.checked = true;\r\n                }\r\n                else {\r\n                    c.checked = false;\r\n                }\r\n            }\r\n\r\n            if (id != \"none\") {\r\n                fetch(\"/Doctor/Get/\" + id)\r\n                    .then(res => res.json())\r\n                    .then(res => {\r\n                        for (let i = 0; i < name.length; i++) {\r\n                            name[i].innerHTML = res.name;\r\n                            hospital[i].innerHTML = res.hospitalName;\r\n                            tehsil[i].innerHTML = res.tehsil.name;\r\n                            district[i].innerHTML = res.district;\r\n                            landline[i].innerHTML = res.landline;\r\n                            cell[i].innerHTML = res.cellNumber;\r\n                            seal1[i].src = \"/\" + res.sealSignaturePath;\r\n\r\n                            const stampPlaceholders = document.querySelectorAll(\"#stampPlaceholder\")\r\n                            for (let p of stampPlaceholders) {\r\n                                p.style.display = \"none\"\r\n                            }\r\n\r\n                            pmdc[i].innerHTML = `\r\n                                        <td class=\"fillable text-uppercase\">pmdc</td>\r\n                                    `\r\n\r\n                            for (let x = 0; x < res.registrationNumber.length; x++) {\r\n                                pmdc[i].innerHTML += ` <td class=\"p-2 fillable\">\r\n                                ${res.registrationNumber[x]}</td>`\r\n                            }\r\n                        }\r\n                    })\r\n            }\r\n            else {\r\n                for (let i = 0; i < name.length; i++) {\r\n                    name[i].innerHTML = \"\";\r\n                    hospital[i].innerHTML = \"\";\r\n                    tehsil[i].innerHTML = \"\";\r\n                    district[i].innerHTML = \"\";\r\n                    landline[i].innerHTML = \"\";\r\n                    cell[i].innerHTML = \"\";\r\n\r\n                    seal1[i].src = \"\";\r\n\r\n                    const stampPlaceholders = document.querySelectorAll(\"#stampPlaceholder\")\r\n                    for (let p of stampPlaceholders) {\r\n                        p.style.display = \"block\"\r\n                    }\r\n\r\n                    pmdc[i].innerHTML = `\r\n                            <td class=\"fillable text-uppercase\">pmdc</td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        <td class=\"p-2 fillable\"></td>\r\n                                        `\r\n                }\r\n            }\r\n        }\r\n\r\n    }\r\n    function hidediv(elem) {\r\n            \r\n            if (elem.checked) { \r\n                const stampPlaceholders = document.querySelectorAll(\"#coronadiv\")\r\n                    for (let p of stampPlaceholders) {\r\n                        p.style.display = \"block\"\r\n                    }\r\n               \r\n            } else {\r\n                 const stampPlaceholders = document.querySelectorAll(\"#coronadiv\")\r\n                    for (let p of stampPlaceholders) {\r\n                        p.style.display = \"none\"\r\n                    }\r\n              \r\n            }\r\n        }\r\n\r\n\r\n</script>\r\n\r\n\r\n<!-- Visual Studio Browser Link -->\r\n<script type=\"text/javascript\" src=\"/_vs/browserLink\" async=\"async\" id=\"__browserLink_initializationData\" data-requestid=\"46edba0f6fc54f3db012739511255130\" data-requestmappingfromserver=\"false\" data-connecturl=\"http://localhost:60153/cb95ed154a4f4f7cbaaec3c6c3519bf0/browserLink\"></script>\r\n<!-- End Browser Link -->\r\n<script src=\"/_framework/aspnetcore-browser-refresh.js\"></script>\r\n\r\n\r\n</body></html>";
            var html = form;
            var bytes = converter.FromHtmlString(html);
            System.IO.File.WriteAllBytes("image.jpeg", bytes);

            // Return a FileResult to download the image
            return File(bytes, "image/jpeg", "image.jpeg");
        }

        public IActionResult MedicalAffidavitIndvidualView(int id)
        {

            var membersOfFamily = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHeadId == id)
                .ToList();

            var vaccines = _dbContext.VaccinesWithoutTrash
                .Select(e => new
                {
                    e.VaccineName,
                    e.VaccineId,
                });

            MedicalAffidavitModel model = new();

            model.Doctors = _dbContext.DoctorsWithoutTrash.ToList();

            for (int i = 0; i < membersOfFamily.Count; i++)
            {



                FamilyMembers currMember = membersOfFamily.ElementAt(i);
                MedicalAffidavitModelEntry entry = new();

                UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                entry.ShowSignature = userHajjAgreement?.SignMedicalAffidavit ?? false;
                entry.SignatureBase64 = currMember.FamilyHead.SignatureBase64;
                entry.Name = currMember.FamilyHead.GivenName;
                entry.FatherHusbandName = currMember.FamilyHead.FatherHusbandName;
                entry.Age = DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth);
                entry.PassportNumber = currMember.FamilyHead.PassportNumber;
                entry.BloodGroup = currMember.FamilyHead.BloodGroup;
                entry.ImagePath = currMember.FamilyHead.MemberImage;
                entry.CNIC = currMember.FamilyHead.CNIC;

                entry.Vaccine1 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId1)?.VaccineName;
                entry.Vaccine2 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId2)?.VaccineName;
                entry.Vaccine3 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId3)?.VaccineName;
                entry.Vaccine4 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId4)?.VaccineName;
                entry.Vaccine5 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId5)?.VaccineName;

                entry.DoseDate1 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate1);
                entry.DoseDate2 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate2);
                entry.DoseDate3 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate3);
                entry.DoseDate4 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate4);
                entry.DoseDate5 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate5);

                model.Entries.Add(entry);
            }

            return View(model);
        }

        public IActionResult MedicalAffidavit(int id)
        {

            var membersOfFamily = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .ToList();

            var vaccines = _dbContext.VaccinesWithoutTrash
                .Select(e => new
                {
                    e.VaccineName,
                    e.VaccineId,
                });

            MedicalAffidavitModel model = new();

            model.Doctors = _dbContext.DoctorsWithoutTrash.ToList();

            for (int i = 0; i < membersOfFamily.Count; i++)
            {

                FamilyMembers currMember = membersOfFamily.ElementAt(i);
                MedicalAffidavitModelEntry entry = new();

                UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                entry.ShowSignature = userHajjAgreement?.SignMedicalAffidavit ?? false;
                entry.SignatureBase64 = currMember.FamilyHead.SignatureBase64;
                entry.Name = currMember.FamilyHead.GivenName;
                entry.FatherHusbandName = currMember.FamilyHead.FatherHusbandName;
                entry.Age = DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth);
                entry.PassportNumber = currMember.FamilyHead.PassportNumber;
                entry.BloodGroup = currMember.FamilyHead.BloodGroup;
                entry.ImagePath = currMember.FamilyHead.MemberImage;
                entry.CNIC = currMember.FamilyHead.CNIC;

                entry.Vaccine1 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId1)?.VaccineName;
                entry.Vaccine2 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId2)?.VaccineName;
                entry.Vaccine3 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId3)?.VaccineName;
                entry.Vaccine4 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId4)?.VaccineName;
                entry.Vaccine5 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId5)?.VaccineName;

                entry.DoseDate1 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate1);
                entry.DoseDate2 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate2);
                entry.DoseDate3 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate3);
                entry.DoseDate4 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate4);
                entry.DoseDate5 = DateTimeManager.GetDate(currMember.FamilyHead.DoseDate5);

                model.Entries.Add(entry);
            }

            return View(model);
        }

        public IActionResult NewMedicalAffidavit(int id)
        {

            var membersOfFamily = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .ToList();

            var vaccines = _dbContext.VaccinesWithoutTrash
                .Select(e => new
                {
                    e.VaccineName,
                    e.VaccineId,
                });

            MedicalAffidavitModel model = new();

            model.Doctors = _dbContext.DoctorsWithoutTrash.ToList();

            for (int i = 0; i < membersOfFamily.Count; i++)
            {

                FamilyMembers currMember = membersOfFamily.ElementAt(i);
                MedicalAffidavitModelEntry entry = new();

                UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                entry.ShowSignature = userHajjAgreement?.SignMedicalAffidavit ?? false;
                entry.SignatureBase64 = currMember.FamilyHead.SignatureBase64;
                entry.Name = currMember.FamilyHead.GivenName;
                entry.FatherHusbandName = currMember.FamilyHead.FatherHusbandName;
                entry.Age = DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth);
                entry.PassportNumber = currMember.FamilyHead.PassportNumber;
                entry.BloodGroup = currMember.FamilyHead.BloodGroup;
                entry.ImagePath = currMember.FamilyHead.MemberImage;
                entry.CNIC = currMember.FamilyHead.CNIC;

                entry.Vaccine1 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId1)?.VaccineName;
                entry.Vaccine2 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId2)?.VaccineName;
                entry.Vaccine3 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId3)?.VaccineName;
                entry.Vaccine4 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId4)?.VaccineName;
                entry.Vaccine5 = vaccines
                    .FirstOrDefault(e => e.VaccineId == currMember.FamilyHead.VaccineId5)?.VaccineName;

                entry.DoseDate1 = currMember.FamilyHead.DoseDate1?.ToString("dd/MM/yyyy");
                entry.DoseDate2 = currMember.FamilyHead.DoseDate2?.ToString("dd/MM/yyyy");
                entry.DoseDate3 = currMember.FamilyHead.DoseDate3?.ToString("dd/MM/yyyy");
                entry.DoseDate4 = currMember.FamilyHead.DoseDate4?.ToString("dd/MM/yyyy");
                entry.DoseDate5 = currMember.FamilyHead.DoseDate5?.ToString("dd/MM/yyyy");

                model.Entries.Add(entry);
            }

            return View(model);
        }

        public IActionResult HajiPackageInformation(int id)
        {
            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .Where(e => e.HeadId == id)
                            .ToList();

            var familyHead = _dbContext.FamilyHeads
                                .Include(e => e.SPO)
                                .Include(e => e.Package)
                                .FirstOrDefault(e => e.FamilyHeadId == id);

            var package = _dbContext.Packages.First(e => e.PackageId == familyHead.PackageId);

            var company = _dbContext.Companies.Find(package.CompanyId);
            string companyEmail = _dbContext.CompanyEmails.FirstOrDefault(e => e.Preferred && e.CompanyId == package.CompanyId).CompanyEmail;
            var companyAddress = _dbContext.CompanyAddresses.FirstOrDefault(e => e.Preferred && e.CompanyId == package.CompanyId);
            string companyContact = _dbContext.CompanyContacts.FirstOrDefault(e => e.Preferred && e.CompanyId == package.CompanyId).CompanyContact;

            HajjPackageInformationModel model = new()
            {
                CompanyName = company.CompanyName,
                CompanyAddress = $"{companyAddress.Address}, {companyAddress.ZipCode}, {companyAddress.City}, {companyAddress.Country}",
                CompanyLogo = company.Logo,
                CompanyEmail = companyEmail,
                CompanyPhone = companyContact,
                CompanyWebsite = company.Website,

                SPOName = familyHead.SPO?.Name,
                FamilyHeadName = $"{familyHead.FullName} ({familyHead.FcId})",
                PackageName = familyHead.Package.Name,
                TotalPax = members.Count,
                AdultPax = members.Where(e => e.FamilyHead.Age >= 12).Count(),
                ChildPax = members.Where(e => e.FamilyHead.Age < 12 && e.FamilyHead.Age >= 2).Count(),
                InfantPax = members.Where(e => e.FamilyHead.Age < 2).Count(),
            };

            var familyIds = members.Select(e => e.FamilyHeadId).ToList();

            var facilities = _dbContext
                            .FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .ThenInclude(e => e.Room)
                            .Include(e => e.AdditionalFacilities)
                            .ThenInclude(e => e.IndividiualFacilityDescription)
                            .Where(e => familyIds.Contains(e.FamilyHeadId))
                            .ToList();

            var extras = _dbContext.FamilyHeadExtras
                            .Where(e => familyIds.Contains(e.FamilyHeadId))
                            .ToList();

            var remarks = _dbContext.FamilyHeadRemarks
                            .Where(e => familyIds.Contains(e.FamilyHeadId))
                            .ToList();

            float? ppExtras = _dbContext.FamilyHeadExtras
                                .Select(e => new
                                {
                                    e.PriceUsd,
                                    e.FamilyHeadId
                                })
                                .Where(e => e.FamilyHeadId == id)
                                .Sum(e => e.PriceUsd);

            // var facilitiesGroupByFacilityId = facilities
            //                 .GroupBy(e => e.AdditionalFacilities.RoomId)
            //                 .ToList();

            var facilitiesGroupByFacilityId = facilities
                            .GroupBy(e => e.AdditionalFacilitiesId)
                            .ToList();

            var facilitiesGroupByRoomId = facilities
                            .GroupBy(e => e.AdditionalFacilities.RoomId)
                            .ToList();

            string facilitiesString = "";
            string roomTableHeadersString = "";

            float adultPackagePP = 0f;

            adultPackagePP += package.AdultUsd;
            float addons = _dbContext.PackageAddons
                    .Where(e => e.PackageId == package.PackageId)
                    .Sum(e => e.UsdPrice);

            adultPackagePP += addons;
            List<float> roomTotals = new();
            List<int> pax = new();

            // return Ok(new {
            //     facilitiesGroupByFacilityId
            // });

            var roomListing = from fm in _dbContext.FamilyMembers
                              join fh in _dbContext.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                              join fhaf in _dbContext.FamilyHeadAdditionalFacilities on fm.FamilyHeadId equals fhaf.FamilyHeadId
                              join af in _dbContext.AdditionalFacilities on fhaf.AdditionalFacilitiesId equals af.AdditionalFacilitiesId
                              join r in _dbContext.Rooms on af.RoomId equals r.RoomId
                              where fm.HeadId == id
                              group new { r.RoomId, r.RoomName, r.PAX } by new { r.RoomId, r.RoomName, r.PAX } into g
                              select new
                              {
                                  RoomId = g.Key.RoomId,
                                  RoomName = g.Key.RoomName,
                                  RoomCount = g.Count() / g.Key.PAX
                              };

            string roomListingString = "";

            foreach (var r in roomListing)
            {
                roomListingString += roomListingString.Equals("") ? $"{r.RoomName}: {r.RoomCount}" : $", {r.RoomName}: {r.RoomCount}";
                model.RoomTableHeaders.Add(r.RoomName);
            }

            for (int i = 0; i < facilitiesGroupByRoomId.Count; i++)
            {

                var _currArray = facilitiesGroupByRoomId.ElementAt(i);
                // var roomName = _currArray.ElementAt(i).AdditionalFacilities.Room.RoomName;
                // var quantity = _currArray.Count() / _currArray.ElementAt(i).AdditionalFacilities.Room.PAX;

                // model.RoomTableHeaders.Add(roomName);
                float facTotal = 0f;
                for (int j = 0; j < facilitiesGroupByFacilityId.Count; j++)
                {
                    var __ = facilitiesGroupByFacilityId.ElementAt(j);
                    if (__.ElementAt(0).AdditionalFacilities.RoomId == _currArray.ElementAt(0).AdditionalFacilities.RoomId)
                    {
                        facTotal += __.ElementAt(0).AdditionalFacilities.AdultUsd ?? 0f;
                    }
                }

                adultPackagePP += facTotal;
                model.AdultPackageAmountPP.Add("$" + adultPackagePP.ToString("N0"));

                roomTotals.Add(adultPackagePP);

                pax.Add(_currArray.ElementAt(i).AdditionalFacilities.Room.PAX);

                adultPackagePP -= facTotal;
                // adultPackagePP -= _currArray.ElementAt(0).AdditionalFacilities.AdultUsd ?? 0f;
            }


            model.Occupancy = roomListingString;
            model.HeaderCountForColSpan = model.RoomTableHeaders.Count;

            facilities = facilities.OrderBy(e => e.AdditionalFacilities.IndividiualFacilityDescriptionId).ToList();
            for (int i = 0; i < facilities.Count; i++)
            {
                var _currFac = facilities.ElementAt(i);
                model.IADF.Add($"{_currFac.AdditionalFacilities?.IndividiualFacilityDescriptionId}. {_currFac.AdditionalFacilities?.IndividiualFacilityDescription?.Description}");
            }

            model.IADF = model.IADF.Distinct().ToList();

            for (int i = 0; i < extras.Count; i++)
            {
                model.EIADF.Add($"{i + 1}. {extras.ElementAt(i).Description}");
            }

            model.EIADF = model.EIADF.Distinct().ToList();

            for (int i = 0; i < remarks.Count; i++)
            {
                model.Remarks.Add($"{i + 1}. {remarks.ElementAt(i).Remarks}");
            }

            float totalDiscount = members.Select(e => e.FamilyHead.UsdDiscountAmount).Sum();
            model.TotalDiscount = "$" + totalDiscount.ToString("N0");

            // int? accountId = _dbContext.FamilyHeads.Select(e => new
            // {
            //     e.AccountId,
            //     e.FamilyHeadId
            // }).FirstOrDefault(e => e.FamilyHeadId == id).AccountId;

            // float receivable = _dbContext.PaymentVouchers
            //                         .FirstOrDefault(e => e.AccountId == accountId)
            //                         .Amount;

            // RoeRequest famRoe = _dbContext.RoeRequests
            //     .Where(e => e.Currency == Currencies.USD)
            //     .FirstOrDefault(e => e.FamilyHeadId == id);
            // float usdRoe = 1f;
            // if (famRoe is null)
            // {
            //     RateOfExchange roe = _dbContext.RateOfExchanges
            //                             .Where(e => e.Currency == Currencies.USD)
            //                             .OrderByDescending(e => e.RateOfExchangeID)
            //                             .FirstOrDefault();

            //     usdRoe = roe.PKRROE;
            // }
            // else
            // {
            //     usdRoe = famRoe.PKRROE;
            // }

            // receivable /= usdRoe;

            float total = 0f;

            // model.NetTotal = "$" + receivable.ToString("N0");
            // model.InvoiceGrandTotal = "$" + (receivable + totalDiscount).ToString("N0");
            model.TicketPricePP = "$" + package.FlightTicketPriceUsd.ToString("N0");

            model.ExtraPricePP = "$" + ppExtras?.ToString("N0");

            List<float> adultTotals = new();

            if (model.AdultPax == 1)
            {
                for (int i = 0; i < roomTotals.Count; i++)
                {
                    float _currTotal = roomTotals.ElementAt(i);
                    float adultTotal = _currTotal + (ppExtras ?? 0f) + (package.FlightTicketPriceUsd);
                    adultTotals.Add(adultTotal);
                    model.TotalAdultPackagePP.Add("$" + adultTotal.ToString("N0"));
                }

            }
            else
            {
                for (int i = 0; i < roomTotals.Count; i++)
                {
                    float _currTotal = roomTotals.ElementAt(i);
                    float adultTotal = _currTotal + (ppExtras ?? 0f) + (package.FlightTicketPriceUsd);
                    adultTotals.Add(adultTotal * pax.ElementAt(i));
                    model.TotalAdultPackagePP.Add("$" + adultTotal.ToString("N0"));
                }
            }

            float? childTotal = 0f;
            float? infantTotal = 0f;

            if (model.ChildPax > 0)
            {
                childTotal = package.ChildUsd + addons + ppExtras + (package.FlightTicketPriceUsd);
                model.ChildTotalPP = "$" + childTotal?.ToString("N0");
                childTotal *= model.ChildPax;
            }
            else
            {
                model.ChildTotalPP = "$0";
            }

            if (model.InfantPax > 0)
            {
                infantTotal = package.InfantUsd + addons + ppExtras + (package.FlightTicketPriceUsd);

                model.InfantTotalPP = "$" + infantTotal?.ToString("N0");
                infantTotal *= model.InfantPax;

            }
            else
            {
                model.InfantTotalPP = "$0";
            }


            model.AdultTotalUsd = "$" + adultTotals.Sum().ToString("N0");
            model.ChildTotalUsd = "$" + childTotal?.ToString("N0");
            model.InfantTotalUsd = "$" + infantTotal?.ToString("N0");

            total += adultTotals.Sum();
            total += childTotal ?? 0f;
            total += infantTotal ?? 0f;

            model.InvoiceGrandTotal = "$" + total.ToString("N0");
            model.NetTotal = "$" + (total - totalDiscount).ToString("N0");


            return View(model);
        }

        public IActionResult Addresses(int id)
        {
            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Reference)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Honorifics)
                            .Where(e => e.HeadId == id && !e.FamilyHead.Trashed);


            var memberIds = members.Select(e => e.FamilyHeadId).ToList();

            var head = members.FirstOrDefault(e => e.IsHead);
            Package p = _dbContext.Packages.Find(head.FamilyHead.PackageId);

            string address = _dbContext.FamilyHeadAddresses
                                .FirstOrDefault(e => e.FamilyHeadId == head.FamilyHeadId && e.Preferred)?.Address;

            string email = _dbContext.FamilyHeadEmails
                                .FirstOrDefault(e => e.FamilyHeadId == head.FamilyHeadId && e.Preferred)?.Email;

            string contact = _dbContext.FamilyHeadContacts
                                .FirstOrDefault(e => e.FamilyHeadId == head.FamilyHeadId && e.Preferred)?.Contact;

            string cId =
                        $"{head.FamilyHead.Package.Company.CompanyCode}.{head.FamilyHead.IndId}" +
                        $".{head.Head.ExtractedFamilyNumber}.({members.Count()})" +
                        $".{head.FamilyHead.Package.Name}";

            var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == head.FamilyHeadId);
            if (memberInGroup != null)
            {
                cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
            }


            List<string> memberStrings = new();

            int mIter = 1;
            float discount = 0f;
            float totalAmount = 0f;
            foreach (var m in members)
            {
                string _cId =
                        $"{m.FamilyHead.Package.Company.CompanyCode}.{m.FamilyHead.IndId}" +
                        $".{m.Head.ExtractedFamilyNumber}.({members.Count()})" +
                        $".{m.FamilyHead.Package.Name}";
                memberStrings.Add($"{mIter}. {m.FamilyHead.Honorifics?.Title} {m.FamilyHead.FullName} ({_cId})");

                discount += m.FamilyHead.UsdDiscountAmount;

                var indFacilities = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .Where(e => memberIds.Contains(e.FamilyHeadId))
                            .ToList();

                if (m.FamilyHead.Age < 2)
                {
                    totalAmount += p.InfantUsd;
                    totalAmount += indFacilities
                            .Where(e => e.FamilyHeadId == m.FamilyHeadId)
                            .Select(e => e.AdditionalFacilities.InfantUsd)
                            .Sum() ?? 0;
                }
                else if (m.FamilyHead.Age < 12)
                {
                    totalAmount += p.ChildUsd;
                    totalAmount += indFacilities
                            .Where(e => e.FamilyHeadId == m.FamilyHeadId)
                            .Select(e => e.AdditionalFacilities.ChildUsd)
                            .Sum() ?? 0;
                }
                else
                {
                    totalAmount += p.AdultUsd;
                    totalAmount += indFacilities
                            .Where(e => e.FamilyHeadId == m.FamilyHeadId)
                            .Select(e => e.AdditionalFacilities.AdultUsd)
                            .Sum() ?? 0;
                }

                if (m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                {
                    totalAmount += p.FlightTicketPriceUsd;
                }
                if (m.FamilyHead.FlightTicketType != FlightTicketType.IncludedWithDifferences)
                {
                    totalAmount += m.FamilyHead.FlightTicketDifferencesUsd ?? 0;
                }

            }

            var indFacs = _dbContext.FamilyHeadAdditionalFacilities
                            .Include(e => e.AdditionalFacilities)
                            .Where(e => memberIds.Contains(e.FamilyHeadId));

            /*var medinaAcc = _dbContext.FamilyHeadAdditionalFacilities
                                        .Include(e => e.AdditionalFacilities)
                                        .ThenInclude(e => e.IndividiualFacilityDescription)
                                        .FirstOrDefault(e => e.FamilyHeadId == id && e.AdditionalFacilities.PlaceId == 2);

            var makkahAcc = _dbContext.FamilyHeadAdditionalFacilities
                                        .Include(e => e.AdditionalFacilities)
                                        .ThenInclude(e => e.IndividiualFacilityDescription)
                                        .FirstOrDefault(e => e.FamilyHeadId == id && e.AdditionalFacilities.PlaceId == 1);

            var aziziaAcc = _dbContext.FamilyHeadAdditionalFacilities
                                        .Include(e => e.AdditionalFacilities)
                                        .ThenInclude(e => e.IndividiualFacilityDescription)
                                        .FirstOrDefault(e => e.FamilyHeadId == id && e.AdditionalFacilities.PlaceId == 4);*/

            var medinaAcc = _dbContext.PackageCategoryFacilities
                                        .Include(e => e.IndividualFacilityDescription)
                                        .FirstOrDefault(e => e.PackageId == p.PackageId && e.IndividualFacilityDescription.PlaceId == 2);

            var makkahAcc = _dbContext.PackageCategoryFacilities
                                        .Include(e => e.IndividualFacilityDescription)
                                        .FirstOrDefault(e => e.PackageId == p.PackageId && e.IndividualFacilityDescription.PlaceId == 1);

            var aziziaAcc = _dbContext.PackageCategoryFacilities
                                       .Include(e => e.IndividualFacilityDescription)
                                       .FirstOrDefault(e => e.PackageId == p.PackageId && e.IndividualFacilityDescription.PlaceId == 4);

            var invoicesCleared = _dbContext.InvoicesWithoutTrash
                                    .Where(e => e.FamilyHeadId == id && e.InvoiceStatus == InvoiceStatus.Cleared);

            int accountId = (int)_dbContext.FamilyHeads.Find(id).AccountId;
            var voucher = _dbContext.PaymentVouchers.FirstOrDefault(e => e.AccountId == accountId);

            float receivableUsd = voucher.Amount / voucher.USDROE;

            float paymentReceived = 0;
            foreach (var inv in invoicesCleared)
            {
                float converted = 0;
                if (inv.Currency == Currencies.PKR)
                {
                    converted = inv.AmountRecived / inv.USDROE;
                }
                else if (inv.Currency == Currencies.SAR)
                {
                    converted = inv.AmountRecived / inv.USDROE;
                }
                else if (inv.Currency == Currencies.USD)
                {
                    converted = inv.AmountRecived;
                }
                paymentReceived += converted;
            }




            AddressReportModel model = new()
            {
                AmountReceived = "$" + paymentReceived.ToString("N0"),
                Discount = "$" + discount.ToString("N0"),
                HeadCID = cId,
                HeadCNIC = head.FamilyHead.CNIC,
                HeadPassport = head.FamilyHead.PassportNumber,
                Package = head.FamilyHead.Package.Name,
                HeadCategory = head.FamilyHead.ApplicantType.ToString(),
                HajjAppNo = head.FamilyHead.HajjApplicationNumber,
                HajjAppSerial = head.FamilyHead.HajjAppSerialNumber,
                MegaReference = head.FamilyHead.Reference?.ReferenceName,
                PAX = members.Count(),
                HeadName = $"{head.FamilyHead.Honorifics?.Title} {head.FamilyHead.FullName}",
                HeadAddress = address,
                HeadEmail = email,
                HeadContact = contact,
                Members = memberStrings,
                Group = memberInGroup?.Group.Name,
                MedinahAccommodation = medinaAcc?.IndividualFacilityDescription?.Description,
                MakkahAccommodation = makkahAcc?.IndividualFacilityDescription?.Description,
                AziziahAccommodation = aziziaAcc?.IndividualFacilityDescription?.Description,
                MinaAccommodation = "AS PER PACKAGE",
                TotalAmount = "$" + (receivableUsd + discount).ToString("N0"),
                NetReceivable = "$" + receivableUsd.ToString("N0"),
                BalanceReceivable = "$" + (receivableUsd - paymentReceived).ToString("N0")
            };

            return View(model);
        }

        public IActionResult EHajjReport(int id)
        {

            FamilyHead member = _dbContext
                                    .FamilyHeads
                                    .Include(e => e.SPO)
                                    .Include(e => e.Reference)
                                    .Include(e => e.Package)
                                    .ThenInclude(e => e.Company)
                                    .FirstOrDefault(e => e.FamilyHeadId == id);

            int headId = _dbContext.FamilyMembers.First(e => e.FamilyHeadId == id).HeadId;
            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == headId && !e.FamilyHead.Trashed)
                            .ToList();

            string headCid =
                                    $"{member.Package.Company.CompanyCode}.{member.IndId}" +
                                    $".{member.ExtractedFamilyNumber}.({members.Count()})" +
                                    $".{member.Package.Name}";

            var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == member.FamilyHeadId);
            if (memberInGroup != null)
            {
                headCid += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
            }

            string contact = _dbContext.FamilyHeadContacts
                                .FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Contact;

            var accomodations = _dbContext.PackageAccomodtions
                                .Where(e => e.PackageId == member.PackageId)
                                .ToList();

            var medinaAcc = accomodations.FirstOrDefault(e => e.PlaceId == 2);
            var makkahAcc = accomodations.FirstOrDefault(e => e.PlaceId == 1);
            var aziziaAcc = accomodations.FirstOrDefault(e => e.PlaceId == 4);

            EHajjReportModel model = new()
            {
                HeadCID = headCid,
                Package = member.Package.Name,
                Category = member.ApplicantType.ToString(),
                CNIC = member.CNIC,
                SPO = member.SPO?.Name,
                Mega = member.Reference?.ReferenceName,
                PAX = members.Count,
                EhajjPath = member.Ehajj,
                PassportImagePath = member.PassportImage,
                Group = memberInGroup?.Group.Name,
                Contact = contact,
                MedinaCheckIn = medinaAcc?.CheckIn,
                MedinaCheckOut = medinaAcc?.CheckOut,
                MedinaHotel = medinaAcc?.AccomodationName,
                MakkahHotel = makkahAcc?.AccomodationName,
                AziziaHotel = aziziaAcc?.AccomodationName

            };

            int i = 1;
            foreach (var m in members)
            {
                model.Members.Add($"{i++}. {m.FamilyHead.Package.Company.CompanyCode}-{m.FamilyHead.IndId} {m.FamilyHead.FullName}");
            }

            return View(model);
        }

        public IActionResult EHajjDataForm(int id)
        {

            int headId = _dbContext.FamilyMembers.First(e => e.FamilyHeadId == id).HeadId;
            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.SPO)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Reference)
                            .Where(e => e.HeadId == headId && !e.FamilyHead.Trashed)
                            .ToList();

            EHajjReportModelList supermodel = new EHajjReportModelList();
            for (int j = 0; j < members.Count; j++)
            {
                var _currMember = members.ElementAt(j);
                // FamilyHead member = _dbContext
                //                    .FamilyHeads
                //                    .Include(e => e.SPO)
                //                    .Include(e => e.Reference)
                //                    .Include(e => e.Package)
                //                    .ThenInclude(e => e.Company)
                //                    .FirstOrDefault(e => e.FamilyHeadId == members[j].FamilyHeadId);

                string headCid =
                                        $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                                        $".{_currMember.FamilyHead.ExtractedFamilyNumber}.({members.Count()})" +
                                        $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                        .Include(e => e.Group)
                        .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);
                if (memberInGroup != null)
                {
                    headCid += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                string contact = _dbContext.FamilyHeadContacts
                                    .FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Contact;

                var accomodations = _dbContext.PackageAccomodtions
                                    .Where(e => e.PackageId == _currMember.FamilyHead.PackageId)
                                    .ToList();

                var medinaAcc = accomodations.FirstOrDefault(e => e.PlaceId == 2);
                var makkahAcc = accomodations.FirstOrDefault(e => e.PlaceId == 1);
                var aziziaAcc = accomodations.FirstOrDefault(e => e.PlaceId == 4);

                EHajjReportModel model = new()
                {
                    Verified = _currMember.FamilyHead.EHajjVerified,
                    FamilyHeadId = _currMember.FamilyHeadId,
                    HeadCID = headCid,
                    Package = _currMember.FamilyHead.Package.Name,
                    Category = _currMember.FamilyHead.ApplicantType.ToString(),
                    CNIC = _currMember.FamilyHead.CNIC,
                    SPO = _currMember.FamilyHead.SPO?.Name,
                    Mega = _currMember.FamilyHead.Reference?.ReferenceName,
                    PAX = members.Count,
                    EhajjPath = _currMember.FamilyHead.Ehajj,
                    PassportImagePath = _currMember.FamilyHead.PassportImage,
                    Group = memberInGroup?.Group.Name,
                    Contact = contact,
                    MedinaCheckIn = medinaAcc?.CheckIn,
                    MedinaCheckOut = medinaAcc?.CheckOut,
                    MedinaHotel = medinaAcc?.AccomodationName,
                    MakkahHotel = makkahAcc?.AccomodationName,
                    AziziaHotel = aziziaAcc?.AccomodationName

                };


                int i = 1;
                foreach (var m in members)
                {
                    model.Members.Add($"{i++}. {m.FamilyHead.Package.Company.CompanyCode}-{m.FamilyHead.IndId} {m.FamilyHead.FullName}");
                }
                supermodel.Entries.Add(model);
            }
            return View(supermodel);
        }

        public IActionResult CongratulationLetterReport(int id)
        {
            CongratulationLetterReportModel model = new CongratulationLetterReportModel();
            FamilyHead fh = _dbContext.FamilyHeadsWithoutTrash
                .Include(a => a.SPO)
                .Include(e => e.Honorifics)
                .FirstOrDefault(e => e.FamilyHeadId == id);

            CongratulationLetterPackages LetterPackages = _dbContext.CongratulationLetterPackages
            .FirstOrDefault(e => e.PackageId == fh.PackageId);

            CongratulationLetter Letter = null;
            if (LetterPackages is not null)
            {
                Letter = _dbContext.CongratulationLetters
            .FirstOrDefault(e => e.CongratulationLetterId == LetterPackages.CongratulationLetterId);
            }


            Package package = _dbContext.Packages.Find(fh.PackageId);

            Company packageCompany = null;
            if (package is not null)
            {
                packageCompany = _dbContext.Companies.Find(package.CompanyId);
            }


            //Family members
            var membersOfFamily = _dbContext.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.RelationWithMehram)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Fiqah)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .ToList();
            for (int i = 0; i < membersOfFamily.Count; i++)
            {
                FamilyMembers currMember = membersOfFamily.ElementAt(i);

                var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();


                var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                string cId =
                        $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
                        $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                        $".{currMember.FamilyHead.Package.Name}";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }



                FamilyHeadAddress headAddress = _dbContext
                                    .FamilyHeadAddresses
                                    .Include(e => e.Tehsil)
                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact preferredContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact otherContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

                model.CID = cId;
                model.CONTACT = preferredContact.Contact;
                model.ADDRESS = headAddress?.Address;
                if (model.membersID == null)
                {
                    model.membersID = new List<string>();
                }
                model.membersID.Add("AK." + currMember.FamilyHead.IndId + " - " + currMember.FamilyHead.FullName);
            }

            model.LetterPackages = LetterPackages;
            model.Letter = Letter;
            model.package = package;
            model.packageCompany = packageCompany;
            model.HeadFullName = fh.FullName;
            model.GUESTNAME = fh.FullName;
            model.CNIC = fh.CNIC;
            model.PASSPORT = fh.PassportNumber;
            model.PAX = membersOfFamily.Count.ToString();
            model.SPO = fh.SPO?.Name;
            model.CATEGORY = fh.ApplicantType.ToString();






            return View(model);
        }

        public IActionResult CongratulationLetterReportHajjWithAlKhair(int id)
        {
            CongratulationLetterReportModel model = new CongratulationLetterReportModel();
            FamilyHead fh = _dbContext.FamilyHeadsWithoutTrash
                .Include(a => a.SPO)
                .Include(e => e.Honorifics)
                .FirstOrDefault(e => e.FamilyHeadId == id);

            CongratulationLetterPackages LetterPackages = _dbContext.CongratulationLetterPackages
            .FirstOrDefault(e => e.PackageId == fh.PackageId);

            CongratulationLetter Letter = null;
            if (LetterPackages is not null)
            {
                Letter = _dbContext.CongratulationLetters
            .FirstOrDefault(e => e.CongratulationLetterId == LetterPackages.CongratulationLetterId);
            }


            Package package = _dbContext.Packages.Find(fh.PackageId);

            Company packageCompany = null;
            if (package is not null)
            {
                packageCompany = _dbContext.Companies.Find(package.CompanyId);
            }


            //Family members
            var membersOfFamily = _dbContext.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.RelationWithMehram)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Fiqah)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .ToList();

            for (int i = 0; i < membersOfFamily.Count; i++)
            {
                FamilyMembers currMember = membersOfFamily.ElementAt(i);

                var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();


                var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                string cId =
                        $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
                        $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                        $".{currMember.FamilyHead.Package.Name}";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                FamilyHeadAddress headAddress = _dbContext
                                    .FamilyHeadAddresses
                                    .Include(e => e.Tehsil)
                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact preferredContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact otherContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

                model.CID = cId;
                model.CONTACT = preferredContact?.Contact;
                model.ADDRESS = headAddress?.Address;
                if (model.membersID == null)
                {
                    model.membersID = new List<string>();
                }
                model.membersID.Add("AK." + currMember.FamilyHead.IndId + " - " + currMember.FamilyHead.FullName);
            }

            model.LetterPackages = LetterPackages;
            model.Letter = Letter;
            model.package = package;
            model.packageCompany = packageCompany;
            model.HeadFullName = fh.FullName;
            model.GUESTNAME = fh.FullName;
            model.CNIC = fh.CNIC;
            model.PASSPORT = fh.PassportNumber;
            model.PAX = membersOfFamily.Count.ToString();
            model.SPO = fh.SPO?.Name;
            model.CATEGORY = fh.ApplicantType.ToString();






            return View(model);
        }

        public IActionResult Itinerary(int id)
        {


            var members = _dbContext.FamilyMembers
                .Join(_dbContext.FamilyHeads,
                    fm => fm.FamilyHeadId,
                    fh => fh.FamilyHeadId,
                    (fm, fh) => new { fm, fh })
                .Join(_dbContext.Packages,
                    fmh => fmh.fh.PackageId,
                    p => p.PackageId,
                    (fmh, p) => new { fmh.fm, fmh.fh, p })
                .Join(_dbContext.Companies,
                    fmhp => fmhp.p.CompanyId,
                    c => c.CompanyId,
                    (fmhp, c) => new { fmhp.fm, fmhp.fh, fmhp.p, c })
                .Where(result => result.fm.HeadId == id && !result.fh.Trashed)
                .Select(result => new
                {
                    result.fh.FcId,
                    CompanyCode = result.c.CompanyCode,
                    IndId = result.fh.IndId,
                    result.fm.HeadId,
                    result.fm.FamilyHeadId,
                    PackageName = result.p.Name,
                    result.fh.FullName,
                    PassportNumber = result.fh.PassportNumber,
                    TicketNumber = result.fh.TicketNumber,
                    PackageId = result.fh.PackageId,
                    Qurbani = _dbContext.FamilyHeadExtras
                                .Any(fe => fe.ExtraFacilityDescriptionId == 1 && fe.FamilyHeadId == result.fh.FamilyHeadId)
                                ? "Yes"
                                : "No"
                })
                .Distinct()
                .AsNoTracking()
                .ToList();

            var pnrMembers = _dbContext.PNRMembers
                                .Include(e => e.PNR)
                                .ThenInclude(e => e.Flight)
                                .ThenInclude(e => e.Airline)
                                .Where(
                                    e => members
                                        .Select(e => e.FamilyHeadId)
                                        .Contains(e.FamilyHeadId)
                                    )
                                .AsNoTracking()
                                .ToList();

            var accommodations = _dbContext.PackageAccomodtions
                                    .AsNoTracking()
                                    .Include(e => e.Hotel)
                                    .Include(e => e.FoodPackage)
                                    .Where(e => e.PackageId == (members.Count > 0 ? members.ElementAt(0).PackageId : 0))
                                    .OrderBy(e => e.ActualCheckInTime)
                                    .ToList();

            ItineraryReportModel model = new()
            {
                AirlineName = pnrMembers.ElementAtOrDefault(0)?.PNR?.Flight.Airline.Name,
                AirlineLogo = pnrMembers.ElementAtOrDefault(0)?.PNR?.Flight.Airline.AirlineLogo
            };
            int FamilyHeadId = members.Count > 0 ? members.ElementAt(0).FamilyHeadId : 0;
            var flightQuery = (from f in _dbContext.Flights
                               join fp in _dbContext.FlightPackages on f.FlightId equals fp.FlightId
                               join p in _dbContext.Packages on fp.PackageId equals p.PackageId
                               join pnr in _dbContext.PNRs on f.FlightId equals pnr.FlightId
                               join pnrm in _dbContext.PNRMembers on pnr.PNRId equals pnrm.PNRId
                               join ss in _dbContext.Sectors on f.FlightId equals ss.FlightId
                               join fh in _dbContext.FamilyHeads on pnrm.FamilyHeadId equals fh.FamilyHeadId
                               where fh.FamilyHeadId == FamilyHeadId && ss.DepartureDateTime != null && ss.ArrivalDateTime != null
                               group new { f, ss, pnr } by new
                               {
                                   pnr.Type,
                                   ss.FlightType,
                                   ss.FlightNo,
                                   f.OutboundFlightNumber,
                                   f.InboundFlightNumber,
                                   ss.Destination,
                                   ss.DepartureDateTime,
                                   ss.ArrivalDateTime
                               } into grp
                               orderby grp.Key.DepartureDateTime ascending, grp.Key.ArrivalDateTime ascending, grp.Key.FlightType ascending
                               select new
                               {
                                   PNRType = grp.Key.Type,
                                   FlightType = grp.Key.FlightType,
                                   OutboundFlightNumber = grp.Key.FlightNo,
                                   InboundFlightNumber = grp.Key.FlightNo,
                                   Destination = grp.Key.Destination,
                                   DepartureDate = grp.Key.DepartureDateTime,
                                   ArrivalDate = grp.Key.ArrivalDateTime
                               }).ToList();


            int familyPax = members.Count;
            foreach (var m in members)
            {
                var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);

                string cId =
                        $"{m.CompanyCode}" +
                        $".{m.IndId}" +
                        $".{m.HeadId}({familyPax})" +
                        $".{m.PackageName}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }

                ItineraryReportMemberModel _memberModel = new()
                {
                    Name = m.FullName,
                    Passport = m.PassportNumber,
                    TicketNumber = m.TicketNumber,
                    PnrNumber = pnrMembers.FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId)?.PNR?.Name,
                    Qurbani = m.Qurbani,
                    CID = cId
                };

                model.Members.Add(_memberModel);
            }

            foreach (var sector in flightQuery)
            {
                ItineraryReportSectorModel _sectorModel = new()
                {
                    FlightNumber = sector.FlightType == 0 ?
                        sector.InboundFlightNumber : sector.OutboundFlightNumber,
                    Sector = sector.Destination,
                    DepartureDate = sector.DepartureDate?.ToString("dd MMM"),
                    DepartureTime = sector.DepartureDate?.ToString("HH:mm"),
                    ArrivalDate = sector.ArrivalDate?.ToString("dd MMM"),
                    ArrivalTime = sector.ArrivalDate?.ToString("HH:mm"),
                    Luggage = "",
                    Class = sector.PNRType == 0 ? "Economy" : "Business"
                };

                model.FlightSectors.Add(_sectorModel);
            }

            var rooms = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(
                                e => members
                                    .Select(e => e.FamilyHeadId)
                                    .Contains(e.FamilyHeadId)
                                )
                        .Select(e => new
                        {
                            e.FamilyHeadId,
                            e.AdditionalFacilities.Room.RoomName,
                            e.AdditionalFacilities.PlaceId,
                            e.AdditionalFacilitiesId,
                            e.AdditionalFacilities.Room.PAX,
                        })
                        .GroupBy(e => new { e.AdditionalFacilitiesId, e.RoomName, e.PAX, e.PlaceId })
                        .Select(e => new
                        {
                            e.Key.RoomName,
                            Qty = (float)e.Count() / (float)e.Key.PAX,
                            e.Key.PlaceId
                        })
                        .ToList();


            foreach (var acc in accommodations)
            {
                string roomString = "", quantityString = "";

                roomString = string.Join(" / ", rooms.Where(e => e.PlaceId == acc.PlaceId).Select(e => e.RoomName));
                quantityString = string.Join(" / ", rooms.Where(e => e.PlaceId == acc.PlaceId).Select(e => e.Qty));

                ItineraryReportAccommodationModel _accModel = new()
                {
                    HotelName = acc.Hotel?.HotelName,
                    Nights = acc.Nights,
                    CheckIn = acc.ActualCheckInTime?.ToString("dd MMM"),
                    CheckOut = acc.ActualCheckOutTime?.ToString("dd MMM"),
                    MealPlan = acc.FoodPackage.Package,
                    RoomType = roomString,
                    Quantity = quantityString
                };

                model.Accommodations.Add(_accModel);
            }

            var travelRoute = _dbContext
                .Packages
                .Select(e => new
                {
                    e.PackageId,
                    e.TravelRouteId,
                    e.TravelRoute
                })
                .FirstOrDefault(e => e.PackageId == members.ElementAt(0).PackageId).TravelRoute;

            var stops = _dbContext.TravelRouteStops
                    .Include(e => e.Route)
                    .Where(e => e.TravelRouteId == travelRoute.TravelRouteId)
                    .Select(e => new
                    {
                        Route = $"{e.Route.StartPlace} - {e.Route.Destination}",
                        Date = e.CheckIn.ToString("dd MMM")
                    });

            var routeData = _dbContext.
                            FamilyHeadItineraryVehicles
                            .Include(e => e.Vehicle)
                            .Include(e => e.Route)
                            .Include(e => e.TravelRoute)
                            .Where(e => e.FamilyHeadId == id)
                            .ToList();


            foreach (var stop in routeData)
            {
                model.Routes.Add(new ItineraryReportRouteModel
                {
                    Route = $"{stop.Route.StartPlace} to {stop.Route.Destination}",
                    Vehicle = stop.Vehicle.VehicleType,
                    Quantity = stop.Quantity.ToString(),
                    Date = _dbContext.TravelRouteStops.Select(e => new
                    {
                        e.RouteId,
                        e.TravelRouteId,
                        e.CheckIn
                    }).FirstOrDefault(e => e.RouteId == stop.RouteId && e.TravelRouteId == stop.TravelRouteId).CheckIn.ToString("dd/MMM"),
                    Type = stop.VehicleType.ToString(),
                    Datetime = _dbContext.TravelRouteStops.Select(e => new
                    {
                        e.RouteId,
                        e.TravelRouteId,
                        e.CheckIn
                    }).FirstOrDefault(e => e.RouteId == stop.RouteId && e.TravelRouteId == stop.TravelRouteId).CheckIn
                });
            }
            model.Routes = model.Routes.OrderBy(m => m.Datetime).ToList();

            model.FamilyHeadId = id;

            // EXTRA ADDITIONAL FACILITY
            List<int> memberIds = _dbContext.FamilyMembers
                                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                                .Select(e => e.FamilyHeadId)
                                .ToList();
            var extras = _dbContext.FamilyHeadExtras
                       .Include(e => e.ExtraFacilityDescription)
                       .Include(e => e.FamilyHead)
                       .ThenInclude(e => e.Package)
                       .ThenInclude(e => e.Company)
                       .Where(e => memberIds.Contains(e.FamilyHeadId)
                       && e.ExtraFacilityDescriptionId != null);


            var extrasGrouped = extras
                        .GroupBy(e => e.ExtraFacilityDescriptionId);

            var indFacilities = _dbContext.FamilyHeadAdditionalFacilities
                                .Include(e => e.AdditionalFacilities)
                                .ThenInclude(e => e.IndividiualFacilityDescription)
                                .Include(e => e.FamilyHead)
                                .ThenInclude(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Where(e => memberIds.Contains(e.FamilyHeadId))
                                .GroupBy(e => e.AdditionalFacilities.IndividiualFacilityDescriptionId);

            List<ADFDetails> eIADFDetails = new();
            List<ADFDetails> iADFDetails = new();

            int s = 1;
            foreach (var e in extrasGrouped)
            {
                string _extraFacMembers = "";
                ADFDetails _d = new();
                foreach (var obj in e)
                {
                    _d.Sr = s;
                    _d.DescriptionId = (int)obj.ExtraFacilityDescriptionId;
                    _d.Description = obj.ExtraFacilityDescription?.Description;
                    _extraFacMembers += _extraFacMembers.Equals("") ? $"{obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}"
                        : $", {obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}";
                    _d.Ids = _extraFacMembers;
                }
                s++;
                eIADFDetails.Add(_d);
            }

            int t = 1;
            foreach (var e in indFacilities)
            {
                string _indFacMembers = "";
                ADFDetails _d = new();
                foreach (var obj in e)
                {
                    _d.Sr = t;
                    _d.DescriptionId = obj.AdditionalFacilitiesId;
                    _d.Description = obj.AdditionalFacilities.IndividiualFacilityDescription.Description;
                    _indFacMembers += _indFacMembers.Equals("") ? $"{obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}"
                        : $", {obj.FamilyHead.Package.Company.CompanyCode}-{obj.FamilyHead.IndId}";
                    _d.Ids = _indFacMembers;
                }
                t++;
                iADFDetails.Add(_d);
            }

            var remarks = _dbContext.FamilyHeadRemarks
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => memberIds.Contains(e.FamilyHeadId))
                            .Select(e => new
                            {
                                e.Remarks,
                                e.FamilyHeadId,
                                e.FamilyHead.IndId,
                                e.FamilyHead.Package.Company.CompanyCode
                            })
                            .ToList();

            int rIter = 1;
            foreach (var r in remarks)
            {
                model.Remarks.Add(new MemberRemarks
                {
                    Sr = rIter,
                    Remarks = r.Remarks,
                    IndivdualId = $"{r.CompanyCode}-{r.IndId}"
                });
                rIter++;
            }

            model.EIADFDetails = eIADFDetails;
            model.IADFDetails = iADFDetails;
            // return Ok(model);
            return View(model);
        }

        public IActionResult ItineraryRouteVehicles(int id)
        {
            ViewBag.Vehicles = _dbContext.VehiclesWithoutTrash;
            ItineraryRouteVehiclesModel model = new();
            model.FamilyHeadId = id;
            int packageId = _dbContext.FamilyHeads
                        .Select(e => new
                        {
                            e.FamilyHeadId,
                            e.PackageId
                        })
                        .FirstOrDefault(e => e.FamilyHeadId == id).PackageId;

            var packageRouteId = _dbContext.Packages
                                    .Select(e => new
                                    {
                                        e.PackageId,
                                        e.TravelRouteId
                                    }).FirstOrDefault(e => e.PackageId == packageId).TravelRouteId;

            var stops = _dbContext.TravelRouteStops
                                    .Include(e => e.Route)
                                    .Where(e => e.TravelRouteId == packageRouteId)
                                    .OrderBy(e => e.CheckIn);


            foreach (var s in stops)
            {
                model.Rows.Add(new ItineraryRouteVehiclesRowModel
                {
                    Route = $"{s.Route.StartPlace} to {s.Route.Destination}",
                    TravelRouteId = s.TravelRouteId,
                    RouteId = s.RouteId,
                });
            }
            ViewBag.itineraryVehicles = _dbContext.FamilyHeadItineraryVehicles.Where(e => e.FamilyHeadId == id).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult ItineraryRouteVehicles(ItineraryRouteVehiclesSaveModel request)
        {
            _dbContext.FamilyHeadItineraryVehicles.Where(e => e.FamilyHeadId == request.FamilyHeadId).ExecuteDelete();

            for (int i = 0; i < request.VehicleIds.Count; i++)
            {
                _dbContext.FamilyHeadItineraryVehicles.Add(new FamilyHeadItineraryVehicles
                {
                    FamilyHeadId = request.FamilyHeadId,
                    VehicleId = request.VehicleIds.ElementAt(i),
                    Quantity = request.Quantities.ElementAt(i),
                    VehicleType = (VehicleType)request.VehicleTypes.ElementAt(i),
                    TravelRouteId = request.TravelRouteIds.ElementAt(i),
                    RouteId = request.RouteIds.ElementAt(i),
                });

            }

            _dbContext.SaveChanges();
            return RedirectToAction("Itinerary", new { id = request.FamilyHeadId });
        }

        public IActionResult Booklet(int id)
        {

            var headId = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id);
            ViewBag.HeadId = headId.HeadId;

            // ============ Itinerary Start =======
            var members = _dbContext.FamilyMembers
                   .Include(e => e.Head)
                   .Include(e => e.FamilyHead)
                   .ThenInclude(e => e.Package)
                   .ThenInclude(e => e.Company)
                   .Where(e => e.HeadId == headId.HeadId && !e.FamilyHead.Trashed)
                   .Select(e => new
                   {
                       e.FamilyHead.Package.Company.CompanyCode,
                       e.FamilyHead.IndId,
                       e.Head.ExtractedFamilyNumber,
                       e.HeadId,
                       e.FamilyHeadId,
                       PackageName = e.FamilyHead.Package.Name,
                       e.FamilyHead.FullName,
                       e.FamilyHead.PassportNumber,
                       e.FamilyHead.TicketNumber,
                       e.FamilyHead.PackageId
                   })
                   .AsNoTracking()
                   .ToList();

            ViewBag.FamilyHeadIds = members.Select(e => e.FamilyHeadId).ToList();

            var pnrMembers = _dbContext.PNRMembers
                                .Include(e => e.PNR)
                                .ThenInclude(e => e.Flight)
                                .ThenInclude(e => e.Airline)
                                .Where(
                                    e => members
                                        .Select(e => e.FamilyHeadId)
                                        .Contains(e.FamilyHeadId)
                                    )
                                .AsNoTracking()
                                .ToList();

            var accommodations = _dbContext.PackageAccomodtions
                                    .AsNoTracking()
                                    .Include(e => e.Hotel)
                                    .Include(e => e.FoodPackage)
                                    .Where(e => e.PackageId == members.ElementAt(0).PackageId)
                                    .OrderBy(e => e.ActualCheckInTime)
                                    .ToList();

            ItineraryReportModel model = new()
            {
                AirlineName = pnrMembers.ElementAtOrDefault(0)?.PNR?.Flight.Airline.Name,
                AirlineLogo = pnrMembers.ElementAtOrDefault(0)?.PNR?.Flight.Airline.AirlineLogo
            };

            var flightQuery = (from f in _dbContext.Flights
                               join fp in _dbContext.FlightPackages on f.FlightId equals fp.FlightId
                               join p in _dbContext.Packages on fp.PackageId equals p.PackageId
                               join pnr in _dbContext.PNRs on f.FlightId equals pnr.FlightId
                               join pnrm in _dbContext.PNRMembers on pnr.PNRId equals pnrm.PNRId
                               join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                               join fh in _dbContext.FamilyHeads on pnrm.FamilyHeadId equals fh.FamilyHeadId
                               where fh.FamilyHeadId == members.ElementAt(0).FamilyHeadId && s.DepartureDateTime != null && s.ArrivalDateTime != null
                               group new { f, s, pnr } by new
                               {
                                   pnr.Type,
                                   s.FlightType,
                                   f.OutboundFlightNumber,
                                   f.InboundFlightNumber,
                                   s.Destination,
                                   s.DepartureDateTime,
                                   s.ArrivalDateTime
                               } into grp
                               orderby grp.Key.DepartureDateTime ascending, grp.Key.ArrivalDateTime ascending, grp.Key.FlightType ascending
                               select new
                               {
                                   PNRType = grp.Key.Type,
                                   FlightType = grp.Key.FlightType,
                                   OutboundFlightNumber = grp.Key.OutboundFlightNumber,
                                   InboundFlightNumber = grp.Key.InboundFlightNumber,
                                   Destination = grp.Key.Destination,
                                   DepartureDate = grp.Key.DepartureDateTime,
                                   ArrivalDate = grp.Key.ArrivalDateTime
                               }).ToList();


            int familyPax = members.Count;
            foreach (var m in members)
            {
                var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);

                string cId =
                        $"{m.CompanyCode}" +
                        $".{m.IndId}" +
                        $".{m.HeadId}({familyPax})" +
                        $".{m.PackageName}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }

                ItineraryReportMemberModel _memberModel = new()
                {
                    Name = m.FullName,
                    Passport = m.PassportNumber,
                    TicketNumber = m.TicketNumber,
                    PnrNumber = pnrMembers.FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId)?.PNR?.Name,
                    CID = cId
                };

                model.Members.Add(_memberModel);
            }

            // return Ok(new {
            //     flightQuery
            // });

            foreach (var sector in flightQuery)
            {
                ItineraryReportSectorModel _sectorModel = new()
                {
                    FlightNumber = sector.FlightType == 0 ?
                        sector.InboundFlightNumber : sector.OutboundFlightNumber,
                    Sector = sector.Destination,
                    DepartureDate = sector.DepartureDate?.ToString("dd MMM"),
                    DepartureTime = sector.DepartureDate?.ToString("hh:mm tt"),
                    ArrivalDate = sector.ArrivalDate?.ToString("dd MMM"),
                    ArrivalTime = sector.ArrivalDate?.ToString("hh:mm tt"),
                    Luggage = "",
                    Class = sector.PNRType == 0 ? "Economy" : "Business"
                };

                model.FlightSectors.Add(_sectorModel);
            }

            var rooms = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(
                                e => members
                                    .Select(e => e.FamilyHeadId)
                                    .Contains(e.FamilyHeadId)
                                )
                        .Select(e => new
                        {
                            e.FamilyHeadId,
                            e.AdditionalFacilities.Room.RoomName,
                            e.AdditionalFacilities.PlaceId,
                            e.AdditionalFacilitiesId,
                            e.AdditionalFacilities.Room.PAX,
                        })
                        .GroupBy(e => new { e.AdditionalFacilitiesId, e.RoomName, e.PAX, e.PlaceId })
                        .Select(e => new
                        {
                            e.Key.RoomName,
                            Qty = e.Count() / e.Key.PAX,
                            e.Key.PlaceId
                        })
                        .ToList();


            foreach (var acc in accommodations)
            {
                string roomString = "", quantityString = "";

                roomString = string.Join(" / ", rooms.Where(e => e.PlaceId == acc.PlaceId).Select(e => e.RoomName));
                quantityString = string.Join(" / ", rooms.Where(e => e.PlaceId == acc.PlaceId).Select(e => e.Qty));

                ItineraryReportAccommodationModel _accModel = new()
                {
                    HotelName = acc.Hotel?.HotelName,
                    Nights = acc.Nights,
                    CheckIn = acc.ActualCheckInTime?.ToString("dd MMM"),
                    CheckOut = acc.ActualCheckOutTime?.ToString("dd MMM"),
                    MealPlan = acc.FoodPackage.Package,
                    RoomType = roomString,
                    Quantity = quantityString
                };

                model.Accommodations.Add(_accModel);
            }

            var travelRoute = _dbContext
                .Packages
                .Select(e => new
                {
                    e.PackageId,
                    e.TravelRouteId,
                    e.TravelRoute
                })
                .FirstOrDefault(e => e.PackageId == members.ElementAt(0).PackageId).TravelRoute;

            var stops = _dbContext.TravelRouteStops
                    .Include(e => e.Route)
                    .Where(e => e.TravelRouteId == travelRoute.TravelRouteId)
                    .Select(e => new
                    {
                        Route = $"{e.Route.StartPlace} to {e.Route.Destination}",
                        Date = e.CheckIn.ToString("dd MMM")
                    });

            var routeData = _dbContext.
                            FamilyHeadItineraryVehicles
                            .Include(e => e.Vehicle)
                            .Include(e => e.Route)
                            .Include(e => e.TravelRoute)
                            .Where(e => e.FamilyHeadId == id)
                            .ToList();


            foreach (var stop in routeData)
            {
                model.Routes.Add(new ItineraryReportRouteModel
                {
                    Route = $"{stop.Route.StartPlace} to {stop.Route.Destination}",
                    Vehicle = stop.Vehicle.VehicleType,
                    Quantity = stop.Quantity.ToString(),
                    Date = _dbContext.TravelRouteStops.Select(e => new
                    {
                        e.RouteId,
                        e.TravelRouteId,
                        e.CheckIn
                    }).FirstOrDefault(e => e.RouteId == stop.RouteId && e.TravelRouteId == stop.TravelRouteId).CheckIn.ToString("dd/MMM"),
                    Type = stop.VehicleType.ToString()
                });
            }


            //======================================================== Itinerary End =========================================================


            //======================================================== DataFormV3 Start =========================================================


            var membersOfFamily = _dbContext.FamilyMembers
            .Include(e => e.Head)
            .Include(e => e.FamilyHead)
            .ThenInclude(e => e.RelationWithMehram)
            .Include(e => e.FamilyHead)
            .ThenInclude(e => e.Fiqah)
            .Include(e => e.FamilyHead)
            .ThenInclude(e => e.Package)
            .ThenInclude(e => e.Company)
            .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
            .ToList();

            DataFormModel superModel = new DataFormModel();


            for (int i = 0; i < membersOfFamily.Count; i++)
            {
                FamilyMembers currMember = membersOfFamily.ElementAt(i);

                var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

                string cId =
                        $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
                        $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                        $".{currMember.FamilyHead.Package.Name}";

                FamilyHeadAddress headAddress = _dbContext
                                    .FamilyHeadAddresses
                                    .Include(e => e.Tehsil)
                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact preferredContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact otherContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

                FamilyHeadNominee headNominee = _dbContext
                    .FamilyHeadNominees
                    .Include(e => e.NomineeRelation)
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                var trainings = _dbContext
                    .PackageTrainings
                    .Include(e => e.Training)
                    .Where(e => e.PackageId == currMember.FamilyHead.PackageId).ToList();

                Flight flight = _dbContext.Flights
                    .FirstOrDefault(e => e.FlightId == currMember.FamilyHead.FlightId);

                var gifts = _dbContext
                    .PackageGifts
                    .Include(e => e.Gift)
                    .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
                    .ToList();

                var extraFacilities = _dbContext
                    .FamilyHeadExtras
                    .Include(e => e.ExtraFacilityDescription)
                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                    .ToList();

                string presentPostalAddress = $"{headAddress?.Address}, {headAddress?.City}, {headAddress?.ZipCode}, {headAddress?.Country}";
                string packageNameDuration = $"{currMember?.FamilyHead.Package.Name} " +
                    $"({currMember?.FamilyHead.Package.Days})";

                var officialAddress = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberAddressTypeId == 2);
                var officialContact = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberContactTypeId == 2);
                var officialEmail = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberEmailTypeId == 2);

                string officialAddressString = officialAddress is not null ?
                                        $"{officialAddress?.Address}, {officialAddress?.City}, {officialAddress?.ZipCode}, {officialAddress?.Country}" : "";

                string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred)?.Email;

                UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                DataFormModelEntry _model = new DataFormModelEntry
                {
                    SignedDate = DateTimeManager.GetDate(userHajjAgreement?.SignedOn),
                    CID = cId,
                    ShowSignature = userHajjAgreement?.SignDataForm ?? false,
                    SignatureBase64 = currMember?.FamilyHead.SignatureBase64,
                    ApplicantType = currMember?.FamilyHead.ApplicantType.ToString(),
                    FamilyHeadImage = currMember?.FamilyHead.MemberImage ?? "",
                    HajjAppNumber = currMember?.FamilyHead.HajjApplicationNumber ?? "-",
                    EnrollmentNumber = currMember?.FamilyHead.ENumber ?? "",
                    MoraPackageNumber = currMember?.FamilyHead.Package.PackageNumber,
                    PackageNameDuration = packageNameDuration,
                    HGOName = currMember?.FamilyHead.Package.Company.CompanyName,
                    SurName = currMember?.FamilyHead.SurName,
                    GivenName = currMember?.FamilyHead.GivenName,
                    FatherHusbandName = currMember?.FamilyHead.FatherHusbandName,
                    PassportNumber = currMember?.FamilyHead.PassportNumber,
                    PassportDateOfExpiry = currMember?.FamilyHead.PassportDateOfExpiry?.ToString("dd-MMM-yyyy"),
                    DateOfBirth = currMember?.FamilyHead.DateOfBirth.ToString("dd-MMM-yyyy"),
                    CNIC = currMember?.FamilyHead.CNIC,
                    BloodGroup = currMember?.FamilyHead.BloodGroup,
                    Gender = currMember?.FamilyHead.Gender,
                    Fiqah = currMember?.FamilyHead.Fiqah.Name.ToString(),
                    IsMarried = (bool)currMember?.FamilyHead.IsMarried ? "YES" : "NO",
                    YearsPerformedHajj = currMember?.FamilyHead.YearsPerformedHajj,
                    PresentPostalAddress = presentPostalAddress,
                    TehsilCode = headAddress?.Tehsil?.Name,
                    MobileNumber = preferredContact?.Contact,
                    TelephoneNumber = otherContact?.Contact,

                    Email = email,
                    OfficialAddress = officialAddressString,
                    OfficialContact = officialContact?.Contact,
                    EmployerBusinessName = currMember.FamilyHead.EmployedAt,
                    Designation = currMember.FamilyHead.Designation,
                    OfficialEmail = officialEmail?.Email,

                    PerformedHajjInLast5Years = (bool)currMember?.FamilyHead.PerformedHajjInLastFiveYears ? "YES" : "NO",
                    WantToHajjEBadal = (bool)currMember?.FamilyHead.WantToPerformHajjEBadal ? "YES" : "NO",
                    WantToPerformHajjAsGroupWorker = (bool)currMember?.FamilyHead.WantToPerformAsGroupWorker ? "YES" : "NO",
                    NomineeName = headNominee?.Name,
                    NomineeRelation = headNominee?.NomineeRelation?.Relation,
                    NomineeCNIC = headNominee?.CNIC,
                    NomineeContact = headNominee?.Contact,
                    NomineeTelephone = "",

                    StayDuration = currMember?.FamilyHead.Package.HajjDuration,
                    SubAgaintAppNumber = currMember?.FamilyHead.SubstitutedAgainstAppNumber,

                    MedinahArrival = currMember?.FamilyHead.Package.MedinahArrival,
                    FlightType = currMember?.FamilyHead.Package.FlightType,
                    FlightClass = currMember?.FamilyHead.FlightClass,
                    TicketPeriod = currMember?.FamilyHead.Package.FlightTicketPeriod,
                    POD = currMember?.FamilyHead.POD,
                    // TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString(),

                    PackageTicketPricePkr = currMember?.FamilyHead.Package.FlightTicketPricePkr,
                    PackageTicketPriceSar = currMember?.FamilyHead.Package.FlightTicketPriceSar,
                    PackageTicketPriceUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd,

                    FlightTicketDifferencesPkr = currMember?.FamilyHead.FlightTicketDifferencesPkr,
                    FlightTicketDifferencesSar = currMember?.FamilyHead.FlightTicketDifferencesSar,
                    FlightTicketDifferencesUsd = currMember?.FamilyHead.FlightTicketDifferencesUsd,

                    IntercityTransport = currMember?.FamilyHead.Package.TransportIntercity,
                    AirportTransport = currMember?.FamilyHead.Package.AirportTransport,
                    MashairTrasport = currMember?.FamilyHead.Package.TransportWithMashair,
                    WithinCity = currMember?.FamilyHead.Package.WithInCity
                };

                _model.Remarks = _dbContext.FamilyHeadRemarks.Where(e => e.FamilyHeadId == currMember.FamilyHeadId).Select(e => e.Remarks).ToList();

                float? additionalFacilitiesAmountPkr = 0;
                float? totalPkr = 0;

                float? additionalFacilitiesAmountSar = 0;
                float? totalSar = 0;

                float? additionalFacilitiesAmountUsd = 0;
                float? totalUsd = 0;

                _model.TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString();

                if (currMember?.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                {
                    _model.TicketIncluded = "Included With Differences";
                    totalPkr = currMember?.FamilyHead.Package.FlightTicketPricePkr + currMember?.FamilyHead.FlightTicketDifferencesPkr;
                    totalSar = currMember?.FamilyHead.Package.FlightTicketPriceSar + currMember?.FamilyHead.FlightTicketDifferencesSar;
                    totalUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd + currMember?.FamilyHead.FlightTicketDifferencesUsd;
                }
                else if (currMember?.FamilyHead.FlightTicketType == FlightTicketType.Excluded)
                {
                    _model.PackageTicketPricePkr = 0;
                    _model.PackageTicketPriceSar = 0;
                    _model.PackageTicketPriceUsd = 0;
                }
                else
                {
                    totalPkr = currMember?.FamilyHead.Package.FlightTicketPricePkr;
                    totalSar = currMember?.FamilyHead.Package.FlightTicketPriceSar;
                    totalUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd;
                }

                float totalExtraAdfPkr = 0, totalExtraAdfSar = 0, totalExtraAdfUsd = 0;
                for (int k = 0; k < extraFacilities.Count; k++)
                {
                    FamilyHeadExtra currExtra = extraFacilities.ElementAt(k);

                    totalExtraAdfPkr += currExtra.PricePkr ?? 0;
                    totalExtraAdfSar += currExtra.PriceSar ?? 0;
                    totalExtraAdfUsd += currExtra.PriceUsd ?? 0;

                    _model.ExtraFacilities.Add(
                        new DataFormExtraFacility(
                            currExtra.ExtraFacilityDescription?.Description,
                            currExtra.PricePkr,
                            currExtra.PriceSar,
                            currExtra.PriceUsd)
                        );
                }

                _model.ExtraAdfDuesPkr = totalExtraAdfPkr.ToString("N0");
                _model.ExtraAdfDuesSar = totalExtraAdfSar.ToString("N0");
                _model.ExtraAdfDuesUsd = totalExtraAdfUsd.ToString("N0");

                for (int k = 0; k < gifts.Count; k++)
                {
                    PackageGifts currGift = gifts.ElementAt(k);
                    _model.Gifts.Add(new DataFormGift(currGift.Gift.Giveaway));
                }

                if (currMember.FamilyHead.Gender == Gender.Male)
                {
                    FamilyHead mehram = _dbContext.FamilyHeads.FirstOrDefault(e => e.MehramFamilyHeadId == currMember.FamilyHeadId && !e.Trashed);
                    _model.AreYouMehramOfALady = mehram is not null ? "YES" : "NO";
                }
                else
                {
                    FamilyHead mehram = _dbContext.FamilyHeads
                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHead.MehramFamilyHeadId && !e.Trashed);
                    _model.AreYouMehramOfALady = "-";
                    _model.MehramAppNumber = mehram?.HajjApplicationNumber;
                    _model.MehramRelationship = currMember?.FamilyHead.RelationWithMehram?.Relation;
                    _model.MehramSignatureBase64 = mehram?.SignatureBase64;
                    _model.MehramName = mehram?.FullName;
                }

                for (int j = 0; j < trainings.Count; j++)
                {
                    PackageTrainings _t = trainings.ElementAt(j);
                    _model.TrainingSessions.Add(new DataFormTrainingSession(
                        _t.Training.TrainingSession, _t.Training.ExpectedTrainingVenue,
                        _t.Training.ExpectedTrainingTime
                        ));
                }

                var individualFacilities =
                    _dbContext.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .ThenInclude(e => e.IndividiualFacilityDescription)
                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                    .ToList();

                if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) < 2)
                {

                    totalPkr += currMember?.FamilyHead.Package.InfantPkr;
                    totalSar += currMember?.FamilyHead.Package.InfantSar;
                    totalUsd += currMember?.FamilyHead.Package.InfantUsd;

                    additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .Sum(e => e.AdditionalFacilities.InfantPkr);

                    additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .Sum(e => e.AdditionalFacilities.InfantSar);

                    additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .Sum(e => e.AdditionalFacilities.InfantUsd);

                    _model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.InfantPkr + additionalFacilitiesAmountPkr)?.ToString("N0");
                    _model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.InfantSar + additionalFacilitiesAmountSar)?.ToString("N0");
                    _model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.InfantUsd + additionalFacilitiesAmountUsd)?.ToString("N0");

                    for (int k = 0; k < individualFacilities.Count; k++)
                    {
                        var _if = individualFacilities.ElementAt(k);

                        _model.IndividualFacilities.Add(
                            new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if.AdditionalFacilities
                            .IndividiualFacilityDescription?.Description,
                            _if.AdditionalFacilities.InfantPkr?.ToString("N0"),
                            _if.AdditionalFacilities.InfantSar?.ToString("N0"),
                            _if.AdditionalFacilities.InfantUsd?.ToString("N0")));
                    }
                }
                else if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) < 12)
                {
                    totalPkr += currMember?.FamilyHead.Package.ChildPkr;
                    totalSar += currMember?.FamilyHead.Package.ChildSar;
                    totalUsd += currMember?.FamilyHead.Package.ChildUsd;

                    additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .Sum(e => e.AdditionalFacilities.ChildPkr);

                    additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .Sum(e => e.AdditionalFacilities.ChildSar);

                    additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .Sum(e => e.AdditionalFacilities.ChildUsd);

                    _model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.ChildPkr + additionalFacilitiesAmountPkr)?.ToString("N0");
                    _model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.ChildSar + additionalFacilitiesAmountSar)?.ToString("N0");
                    _model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.ChildUsd + additionalFacilitiesAmountUsd)?.ToString("N0");


                    for (int k = 0; k < individualFacilities.Count; k++)
                    {
                        var _if = individualFacilities.ElementAt(k);

                        _model.IndividualFacilities.Add(
                            new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if.AdditionalFacilities.IndividiualFacilityDescription?.Description,
                            _if.AdditionalFacilities.ChildPkr?.ToString("N0"),
                            _if.AdditionalFacilities.ChildSar?.ToString("N0"),
                            _if.AdditionalFacilities.ChildUsd?.ToString("N0")
                            ));
                    }
                }
                else if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) >= 12)
                {
                    totalPkr += currMember?.FamilyHead.Package.AdultPkr;
                    totalSar += currMember?.FamilyHead.Package.AdultSar;
                    totalUsd += currMember?.FamilyHead.Package.AdultUsd;

                    additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .Sum(e => e.AdditionalFacilities.AdultPkr);

                    additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
                       .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                       .Sum(e => e.AdditionalFacilities.AdultSar);

                    additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
                       .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                       .Sum(e => e.AdditionalFacilities.AdultUsd);

                    _model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.AdultPkr + additionalFacilitiesAmountPkr)?.ToString("N0");
                    _model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.AdultSar + additionalFacilitiesAmountSar)?.ToString("N0");
                    _model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.AdultUsd + additionalFacilitiesAmountUsd)?.ToString("N0");



                    for (int k = 0; k < individualFacilities.Count; k++)
                    {
                        var _if = individualFacilities.ElementAt(k);

                        _model.IndividualFacilities.Add(
                            new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if?.AdditionalFacilities?.IndividiualFacilityDescription?.Description,
                            _if.AdditionalFacilities?.AdultPkr?.ToString("N0"),
                            _if.AdditionalFacilities?.AdultSar?.ToString("N0"),
                            _if.AdditionalFacilities?.AdultUsd?.ToString("N0")
                            ));
                    }
                }

                _model.IndividualAdditionalFacilitiesDuesPkr = additionalFacilitiesAmountPkr?.ToString("N0");
                _model.IndividualAdditionalFacilitiesDuesSar = additionalFacilitiesAmountSar?.ToString("N0");
                _model.IndividualAdditionalFacilitiesDuesUsd = additionalFacilitiesAmountUsd?.ToString("N0");

                _model.DiscountPkr =
                    $"{currMember?.FamilyHead.PkrDiscountAmount.ToString("N0")}";

                _model.DiscountSar =
                    $"{currMember?.FamilyHead.SarDiscountAmount.ToString("N0")}";

                _model.DiscountUsd =
                    $"{currMember?.FamilyHead.UsdDiscountAmount.ToString("N0")}";

                totalPkr += additionalFacilitiesAmountPkr;
                totalSar += additionalFacilitiesAmountSar;
                totalUsd += additionalFacilitiesAmountUsd;

                totalPkr += totalExtraAdfPkr;
                totalSar += totalExtraAdfSar;
                totalUsd += totalExtraAdfUsd;

                totalPkr -= currMember?.FamilyHead.PkrDiscountAmount;
                totalSar -= currMember?.FamilyHead.SarDiscountAmount;
                totalUsd -= currMember?.FamilyHead.UsdDiscountAmount;

                _model.TotalPkr = totalPkr?.ToString("N0");
                _model.TotalSar = totalSar?.ToString("N0");
                _model.TotalUsd = totalUsd?.ToString("N0");

                PackageAccomodations aziziaAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 4);



                var makkahAccomadation = _dbContext.PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);

                PackageAccomodations secondMakkahAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .OrderByDescending(e => e.PackageAccomodationsId)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);
                if (makkahAccomadation != null)
                {
                    if (makkahAccomadation.PackageAccomodationsId == secondMakkahAccomadation.PackageAccomodationsId)
                    {
                        secondMakkahAccomadation = null;
                    }
                }
                PackageAccomodations medinaAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 2);

                PackageAccomodations minaAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 3);

                if (aziziaAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                                .Include(e => e.AdditionalFacilities)
                                                                .ThenInclude(e => e.Room)
                                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                                && e.AdditionalFacilities.PlaceId == 4);

                    _model.AziziaAccomodation = new DataFormAzizia(
                        aziziaAccomadation.Accomodation.AccomodationName,
                        aziziaAccomadation.AccomodationName,
                        aziziaAccomadation.AccomodationCategory.Category,
                        aziziaAccomadation.CheckIn,
                        aziziaAccomadation.CheckOut,
                        aziziaAccomadation.Days,
                        aziziaAccomadation.DistanceInMeters?.ToString(),
                        aziziaAccomadation.FoodPackage?.Package,
                        //_indId is null ? aziziaAccomadation.LoadPerRoom : _indId.AdditionalFacilities.Room.PAX,
                        //_indId is null ? aziziaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        aziziaAccomadation.Shuttle,
                        aziziaAccomadation.ReligiousLectures,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : ""

                    );
                }

                if (makkahAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Room)
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                            && e.AdditionalFacilities.PlaceId == 1);

                    _model.MakkahAccomodation = new DataFormAccomodation(
                    makkahAccomadation.GroupZiarat,
                    makkahAccomadation.Days,
                    makkahAccomadation.DistanceInMeters?.ToString(),
                    makkahAccomadation.FoodPackage?.Package,
                    //_indId is null ? makkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                    makkahAccomadation.Accomodation.AccomodationName,
                    makkahAccomadation.AccomodationName,
                    makkahAccomadation.CheckIn,
                    makkahAccomadation.CheckOut,
                    //_indId is null ? makkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                    makkahAccomadation.Sharing,
                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                    makkahAccomadation.AccomodationCategory.Category

                );
                }

                if (secondMakkahAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Room)
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                            && e.AdditionalFacilities.PlaceId == 1);

                    _model.SecondMakkahAccomodation = new DataFormAccomodation(
                    secondMakkahAccomadation.GroupZiarat,
                    secondMakkahAccomadation.Days,
                    secondMakkahAccomadation.DistanceInMeters?.ToString(),
                    secondMakkahAccomadation.FoodPackage?.Package,
                    //_indId is null ? secondMakkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                    secondMakkahAccomadation.Accomodation.AccomodationName,
                    secondMakkahAccomadation.AccomodationName,
                    secondMakkahAccomadation.CheckIn,
                    secondMakkahAccomadation.CheckOut,
                    //_indId is null ? secondMakkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                    secondMakkahAccomadation.Sharing,
                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                    secondMakkahAccomadation.AccomodationCategory.Category

                );
                }

                if (medinaAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Room)
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                            && e.AdditionalFacilities.PlaceId == 2);

                    _model.MedinaAccomodation = new DataFormAccomodation(
                    medinaAccomadation.GroupZiarat,
                    medinaAccomadation.Days,
                    medinaAccomadation.DistanceInMeters?.ToString(),
                    medinaAccomadation.FoodPackage?.Package,
                    //_indId is null ? medinaAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                    medinaAccomadation.Accomodation.AccomodationName,
                    medinaAccomadation.AccomodationName,
                    medinaAccomadation.CheckIn,
                    medinaAccomadation.CheckOut,
                    //_indId is null ? medinaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                    medinaAccomadation.Sharing,
                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                    medinaAccomadation.AccomodationCategory.Category
                );

                    //  return Ok(new {
                    //         model.MedinaAccomodation
                    //     });
                }

                if (minaAccomadation is not null)
                {
                    _model.MinaAccomodation = new DataFormMina(
                    minaAccomadation.Package.CategoryZone,
                    minaAccomadation.FoodPackage?.Package,
                    minaAccomadation.Bedding,
                    minaAccomadation.Camp,
                    minaAccomadation.Arafat,
                    minaAccomadation.ReligiousLectures
                   );
                }

                superModel.Entries.Add(_model);

            }
            //======================================================== DataFormV3 End =========================================================



            //======================================================== Hajj Agreement Start =========================================================


            var agreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == headId.FamilyHeadId);



            //======================================================== Hajj Agreement End =========================================================





            //======================================================== Receipt Start =========================================================
            var head = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id);
            var invoice = _dbContext.InvoicesWithoutTrash.FirstOrDefault(e => e.FamilyHeadId == head.HeadId);

            ViewBag.InvoiceId = invoice.InvoiceId;

            var query = (from fh in _dbContext.FamilyHeads
                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                         join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                         join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                         join inv in _dbContext.Invoices on fh.FamilyHeadId equals inv.FamilyHeadId
                         where inv.InvoiceId == invoice.InvoiceId && !fh.Trashed && !c.Trashed && !inv.Trashed
                         select new
                         {
                             EnrollmentNO = cl.CompanyLicense,
                             MunazimName = m.ClusterNumber.ToString() + "-" + m.Name,
                             fh.HajjApplicationNumber,
                             ApplicantName = fh.GivenName + " " + fh.SurName,
                             scheme = fh.ApplicantType,
                             PackageName = p.Name,
                             PackageId = p.PackageId,
                             fh.FamilyHeadId,
                             inv.AmountRecived,
                             inv.ModeOfPayment,
                             ReferenceNo = inv.Instrument,
                             inv.Date,
                             inv.Remarks,
                             MemberImage = fh.MemberImage
                         }).ToList();

            int FamilyHeadId = 0;
            int PackageId = 0;
            foreach (var item in query)
            {
                FamilyHeadId = item.FamilyHeadId;
                PackageId = item.PackageId;
                break;
            }
            var packageaccomodation = _dbContext.PackageAccomodtions.Where(e => e.PackageId == PackageId && e.PlaceId == 1);
            var headid = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == FamilyHeadId);
            var familyhead = _dbContext.FamilyMembers
                        .Include(e => e.FamilyHead)
                        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                        .ToList();
            Package _p = _dbContext.Packages
                            .Include(e => e.Company)
                            .FirstOrDefault(e => e.PackageId == PackageId);

            float ticketTotal = 0;
            float ticketDiffTotal = 0;
            float discount = 0;
            float packagePriceTotal = 0;

            float infantPackageUsd = 0, childPackageUsd = 0, adultPackageUsd = 0;

            foreach (var f in familyhead)
            {
                if (f.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                {
                    ticketTotal += _p.FlightTicketPriceUsd;
                }
                if (f.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                {
                    ticketDiffTotal += f.FamilyHead.FlightTicketDifferencesUsd ?? 0;
                }

                discount += f.FamilyHead.UsdDiscountAmount;

                if (f.FamilyHead.Age < 2)
                {
                    packagePriceTotal += _p.InfantUsd;
                    infantPackageUsd = _p.InfantUsd;
                }
                else if (f.FamilyHead.Age < 12)
                {
                    packagePriceTotal += _p.ChildUsd;
                    childPackageUsd = _p.ChildUsd;

                }
                else
                {
                    packagePriceTotal += _p.AdultUsd;
                    adultPackageUsd = _p.AdultUsd;
                }
            }
            HajjReceiptModel recModel = new HajjReceiptModel();
            if (query.Count > 0)
            {
                recModel.EnrollmentNo = query[0].EnrollmentNO ?? "";
                recModel.MunazamCompany = query[0].MunazimName ?? "";
                recModel.HajjApplicationNumber = query[0].HajjApplicationNumber ?? "";
                recModel.ApplicantName = query[0].ApplicantName ?? "";
                recModel.Scheme = query[0].scheme == null ? "" : query[0].scheme.ToString();
                recModel.HajjPackage = query[0].PackageName ?? "";
                recModel.MakkahAccommodation = "NO";
                recModel.PackageAmount = packagePriceTotal.ToString();
                recModel.AdditionFacilities = "NO";
                recModel.AdditionalAmount = "0";
                recModel.TotalAmount = packagePriceTotal.ToString();
                recModel.AmountReceived = query[0].AmountRecived == null ? "0" : query[0].AmountRecived.ToString();
                recModel.PaymentMode = query[0].ModeOfPayment == null ? "" : query[0].ModeOfPayment.ToString();
                recModel.ReferenceNo = query[0].ReferenceNo == null ? "" : query[0].ReferenceNo.ToString();
                recModel.Amount = query[0].AmountRecived == null ? "0" : query[0].AmountRecived.ToString();
                recModel.Date = query[0].Date == null ? "" : query[0].Date.ToString("dd/MM/yyyy");
                recModel.Remarks = query[0].Remarks == null ? "" : query[0].Remarks.ToString();
                recModel.MemberImage = query[0].MemberImage == null ? "" : query[0].MemberImage.ToString();
            }

            //======================================================== Receipt End =========================================================

            //======================================================== Visa, Insurance && Passport Start =========================================================
            var visaImgs = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(a => a.HeadId == id && !a.FamilyHead.Trashed)
                .Select(e => e.FamilyHead.VisaImage)
                .ToList();

            var passportImgs = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(a => a.HeadId == id && !a.FamilyHead.Trashed)
                .Select(e => e.FamilyHead.PassportImage)
                .ToList();

            //======================================================== Visa, Insurance && Passport End =========================================================

            ViewBag.FamilyHeadId = headId.FamilyHeadId;
            ViewBag.Itinerary = model;
            ViewBag.DataForm = superModel;
            ViewBag.Agreement = agreement;
            ViewBag.Visas = visaImgs;
            ViewBag.Passports = passportImgs;
            ViewBag.Receipt = recModel;

            return View();
        }

        public IActionResult DownloadBookletImages(int id)
        {

            var members = _dbContext.FamilyHeads
            .Where(a => !a.Trashed)
            .ToList();

            FamilyHead head = _dbContext.FamilyHeads.Find(id);

            string zipName = $"{head.FcId}'s Images.zip";

            using (MemoryStream ms = new())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    if (head.VisaImage is not null)
                    {
                        string path = Path.Combine(_host.WebRootPath, head.VisaImage);
                        var entry = zip.CreateEntry($"{head.FcId}/{head.VisaImage}".Replace("/VisaImages/", "/VisaImages/" + head.FamilyHeadId + "/"));
                        var fileBytes = System.IO.File.ReadAllBytes(path);
                        using (var fileStream = new MemoryStream(fileBytes))
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                    if (head.InsuranceImage is not null)
                    {
                        string path = Path.Combine(_host.WebRootPath, head.InsuranceImage);
                        var entry = zip.CreateEntry($"{head.FcId}/{head.InsuranceImage}".Replace("/InsuranceImages/", "/InsuranceImages/" + head.FamilyHeadId + "/"));
                        var fileBytes = System.IO.File.ReadAllBytes(path);
                        using (var fileStream = new MemoryStream(fileBytes))
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                    if (head.PassportImage is not null)
                    {
                        string path = Path.Combine(_host.WebRootPath, head.PassportImage);
                        var entry = zip.CreateEntry($"{head.FcId}/{head.PassportImage}".Replace("/MIS/", "/MIS/" + head.FamilyHeadId + "/"));
                        var fileBytes = System.IO.File.ReadAllBytes(path);
                        using (var fileStream = new MemoryStream(fileBytes))
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                    if (head.AirlineTicketImage is not null)
                    {
                        string path = Path.Combine(_host.WebRootPath, head.AirlineTicketImage);
                        var entry = zip.CreateEntry($"{head.FcId}/{head.AirlineTicketImage}".Replace("/AirlineTickets/", "/AirlineTickets/" + head.FamilyHeadId + "/"));
                        var fileBytes = System.IO.File.ReadAllBytes(path);
                        using (var fileStream = new MemoryStream(fileBytes))
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                    if (head.TrainTicketImage is not null)
                    {
                        string path = Path.Combine(_host.WebRootPath, head.TrainTicketImage);
                        var entry = zip.CreateEntry($"{head.FcId}/{head.TrainTicketImage}".Replace("/TrainTickets/", "/TrainTickets/" + head.FamilyHeadId + "/"));
                        var fileBytes = System.IO.File.ReadAllBytes(path);
                        using (var fileStream = new MemoryStream(fileBytes))
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                return File(ms.ToArray(), "application/zip", zipName);
            }
        }

        public IActionResult DownloadDocuments(int id)
        {
            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Include(e => e.Head)
                            .Where(e => e.FamilyHeadId == id)
                            .ToList();

            FamilyHead head = _dbContext.FamilyHeads.Find(id);

            string zipName = $"{head.FcId}'s Documents.zip";

            using (MemoryStream ms = new())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    members.ForEach(m =>
                    {
                        if (m.FamilyHead.VisaImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.VisaImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.VisaImage}".Replace("/VisaImages/", "/VisaImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.InsuranceImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.InsuranceImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.InsuranceImage}".Replace("/InsuranceImages/", "/InsuranceImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.MISPdfPath is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.MISPdfPath);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.MISPdfPath}".Replace("/MIS/", "/MIS/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.AirlineTicketImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.AirlineTicketImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.AirlineTicketImage}".Replace("/AirlineTickets/", "/AirlineTickets/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.TrainTicketImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.TrainTicketImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.TrainTicketImage}".Replace("/TrainTickets/", "/TrainTickets/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    });
                }

                return File(ms.ToArray(), "application/zip", zipName);
            }
        }


        public IActionResult TicketBoardingStamp(int id)
        {
            ViewBag.BoardingStamps = _dbContext.BoardingStampsWithoutTrash;
            ViewBag.Ticket = _dbContext.FamilyHeads.Find(id).AirlineTicketImage;
            ViewBag.FamilyHeadId = id;

            return View();
        }

        public IActionResult MarkBoardingStamp(int familyheadId, int stampId)
        {
            FamilyHead fh = _dbContext.FamilyHeads.Find(familyheadId);

            string file = Path.Combine(_host.WebRootPath, fh.AirlineTicketImage);

            string outputFile = Path.Combine(_host.WebRootPath, "Images", "BoardedAirlineTickets", fh.FamilyHeadId + "BoardedAirlineTicket.pdf");

            fh.BoardedAirlineTicketImage = @"Images\BoardedAirlineTickets\" + fh.FamilyHeadId + "BoardedAirlineTicket.pdf";
            _dbContext.SaveChanges();

            BoardingStamp stamp = _dbContext.BoardingStamps.Find(stampId);
            string stampFile = Path.Combine(_host.WebRootPath, stamp.FilePath);

            // string base64Path = request.SignatureBase64;
            float x = 250, y = 100;

            AddImageToPdf(file, outputFile, stampFile, x, y);

            return Ok(new
            {
                familyheadId,
                stampId
            });
        }

        public void AddImageToPdf(string pdfFilePath, string outputFilePath, string imagePath, float x, float y)
        {
            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            using (PdfWriter pdfWriter = new PdfWriter(outputFilePath))
            using (PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter))
            {
                iText.Layout.Document document = new iText.Layout.Document(pdfDoc);

                PdfCanvas canvas = new PdfCanvas(pdfDoc.GetFirstPage());

                ImageData imageData = ImageDataFactory.Create(imagePath);
                Image image = new Image(imageData);

                float currentWidth = image.GetImageScaledWidth();
                float currentHeight = image.GetImageScaledHeight();

                float newWidth = currentWidth * 0.6f;
                float newHeight = currentHeight * 0.6f;

                float scaleFactor = 0.55f;
                canvas.SaveState()
                      .ConcatMatrix(scaleFactor, 0, 0, scaleFactor, x, y);

                image.SetFixedPosition(x, y);
                document.Add(image);

                canvas.RestoreState();
            }
        }

        public IActionResult DownloadTicketBoardingStamp(int id)
        {
            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Include(e => e.Head)
                            .Where(e => e.FamilyHeadId == id)
                            .ToList();

            FamilyHead head = _dbContext.FamilyHeads.Find(id);

            string zipName = $"{head.FcId}'s Boarded Ticket.zip";

            using (MemoryStream ms = new())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    members.ForEach(m =>
                    {
                        if (m.FamilyHead.BoardedAirlineTicketImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.BoardedAirlineTicketImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.BoardedAirlineTicketImage}".Replace("/BoardedTicket/", "/BoardedTicket/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    });
                }

                return File(ms.ToArray(), "application/zip", zipName);
            }

        }

        public IActionResult DownloadSignature(int id)
        {
            var members = _dbContext.FamilyMembers
                           .Include(e => e.FamilyHead)
                           .Include(e => e.Head)
                           .Where(e => e.FamilyHeadId == id)
                           .ToList();

            FamilyHead head = _dbContext.FamilyHeads.Find(id);

            string zipName = $"{head.FcId}'s Signature .zip";

            using (MemoryStream ms = new())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    members.ForEach(m =>
                    {
                        if (m.FamilyHead.SignatureBase64 is not null)
                        {
                            string base64String = m.FamilyHead.SignatureBase64;

                            // Remove the data URL prefix if present
                            if (base64String.Contains("base64,"))
                            {
                                base64String = base64String.Substring(base64String.IndexOf("base64,") + 7);
                            }

                            try
                            {
                                // Decode the base64 string
                                byte[] imageBytes = Convert.FromBase64String(base64String);

                                // Create an entry in the zip archive
                                string entryName = $"{head.FcId}/Signature_{m.FamilyHead.FamilyHeadId}.png";
                                var entry = zip.CreateEntry(entryName);
                                using (var entryStream = entry.Open())
                                {
                                    entryStream.Write(imageBytes, 0, imageBytes.Length);
                                }
                            }
                            catch (FormatException)
                            {
                                // Handle the case where the string is not a valid base64 string
                                // You can log this error or handle it as needed
                            }
                        }
                    });
                }

                return File(ms.ToArray(), "application/zip", zipName);
            }
        }

        public IActionResult GiftReport(int id)
        {
            ViewBag.PackageName = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .Where(e => e.HeadId == id)
                .Select(e => e.FamilyHead.Package.Name)
                .FirstOrDefault();

            var packageId = _dbContext.FamilyMembers
               .Include(e => e.FamilyHead)
               .ThenInclude(e => e.Package)
               .Where(e => e.HeadId == id)
               .Select(e => e.FamilyHead.PackageId)
               .FirstOrDefault();

            ViewBag.Gift = _dbContext.PackageGifts
                .Include(e => e.Gift)
                .Where(e => e.PackageId == packageId)
                .Select(e => e.Gift.Giveaway)
                .FirstOrDefault();

            var members = (from fm in _dbContext.FamilyMembers
                           join fh in _dbContext.FamilyHeads.Where(fh => !fh.Trashed) on fm.FamilyHeadId equals fh.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           where fm.HeadId == id
                           select new
                           {
                               PackageName = p.Name,
                               FcId = fh.FcId,
                               fh.FullName,
                               GiftPack = fh.GiftPack ? "YES" : "NO",
                               FamilyHeadId = fh.FamilyHeadId

                           })
                .AsNoTracking()
                .ToList();

            ViewBag.Results = members;

            return View();
        }
        public IActionResult SuitCaseReport(int id)
        {


            var members = (from fm in _dbContext.FamilyMembers
                           join fh in _dbContext.FamilyHeads.Where(fh => !fh.Trashed) on fm.FamilyHeadId equals fh.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join sp in _dbContext.SuitcasePackages on p.PackageId equals sp.PackageId
                           join s in _dbContext.Suitcases on sp.SuitcaseId equals s.SuitcaseId
                           where fm.HeadId == id
                           select new
                           {

                               PackageName = p.Name,
                               FcId = fh.FcId,
                               fh.FullName,
                               SuitCase = fh.SuitCase ? "YES" : "NO",
                               Color = s.Color,
                           })
                            .AsNoTracking()
                            .ToList();


            ViewBag.Results = members;


            return View();
        }

        public IActionResult Cards(int id)
        {

            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);




            string[] suffixesToRemove = { "LIMITED", "SERVICES", "PRIVATE LIMITED", "(PRIVATE)", "& UMRAH", "FOR HAJJ" };

            model.Munazzim = RemoveSuffixes(munazzim?.Munazzim.Name ?? string.Empty, suffixesToRemove);
            model.Company = RemoveSuffixes(package.Company.CompanyName, suffixesToRemove);


            // Function to remove suffixes from a given string
            string RemoveSuffixes(string name, string[] suffixes)
            {
                foreach (string suffix in suffixes)
                {
                    name = Regex.Replace(name, $@"\s*{Regex.Escape(suffix)}\s*", "", RegexOptions.IgnoreCase).Trim();
                }
                return name;
            }

            model.Package = package.Package.Name;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;


            if (munazzim.Munazzim.Name == "AL BAYAN MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-407-M#20046";
            }
            else if (munazzim.Munazzim.Name == "AL MAIRAJ MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-408-M#20047";
            }
            else if (munazzim.Munazzim.Name == "AL HAMID MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-415-M#20048";
            }

            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.DOB = _currMember.FamilyHead.DateOfBirth.ToString("dd/MMM/yyyy");
                _model.PhoneNo = _dbContext.FamilyHeadContacts
                    .Where(e => e.FamilyHeadId == _currMember.FamilyHeadId
                            && e.Preferred)
                    .Select(e => e.Contact)
                    .FirstOrDefault();

                _model.ZoneNo = _currMember.FamilyHead.Package.CategoryZone;

                _model.ArrFlightNo = _dbContext.POFlightDetails
                    .Include(e => e.POTicket)
                    .Include(e => e.POTicketFlightData)
                    .Where(e => e.POTicket.FamilyMembersId == _currMember.FamilyHeadId
                            && e.AirportsType == AirportsType.Arrival)
                    .Select(e => e.POTicketFlightData.FlightNumber)
                    .FirstOrDefault();

                _model.ArrDate= _dbContext.POFlightDetails
                    .Include(e => e.POTicket)
                    .Include(e => e.POTicketFlightData)
                    .Where(e => e.POTicket.FamilyMembersId == _currMember.FamilyHeadId
                            && e.AirportsType == AirportsType.Arrival)
                    .Select(e => e.ArrivalDate)
                    .FirstOrDefault();

                _model.DepFlightNo = _dbContext.POFlightDetails
                    .Include(e => e.POTicket)
                    .Include(e => e.POTicketFlightData)
                    .Where(e => e.POTicket.FamilyMembersId == _currMember.FamilyHeadId
                            && e.AirportsType == AirportsType.Departure)
                    .Select(e => e.POTicketFlightData.FlightNumber)
                    .FirstOrDefault();


                _model.DepDate = _dbContext.POFlightDetails
                    .Include(e => e.POTicket)
                    .Include(e => e.POTicketFlightData)
                    .Where(e => e.POTicket.FamilyMembersId == _currMember.FamilyHeadId
                            && e.AirportsType == AirportsType.Departure)
                    .Select(e => e.DepartureDate)
                    .FirstOrDefault();

                _model.Munazzim = _dbContext.CompanyVisas
                    .Where(e => e.CompanyVisasId == _currMember.FamilyHead.CompanyVisasId)
                    .Select(e => e.MunazimName)
                    .FirstOrDefault();

                _model.MaktabNumber = _dbContext.CompanyVisas
                    .Where(e => e.CompanyVisasId == _currMember.FamilyHead.CompanyVisasId)
                    .Select(e => e.MaktabNumber)
                    .FirstOrDefault();

                _model.MunazzimNo = _dbContext.CompanyVisas
                    .Where(e => e.CompanyVisasId == _currMember.FamilyHead.CompanyVisasId)
                    .Select(e => e.SaudiMunazimNo)
                    .FirstOrDefault();

                _model.MoraNo = _dbContext.CompanyVisas
                    .Where(e => e.CompanyVisasId == _currMember.FamilyHead.CompanyVisasId)
                    .Select(e => e.MoraNo)
                    .FirstOrDefault();

                var hotelnamemakkah = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 1
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.MakkahAccomodation = hotelnamemakkah.FirstOrDefault();

                var hotellocationmakkah = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 1
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.MakkahLocation = hotellocationmakkah.FirstOrDefault();


                //madina
                var hotelnamemedina = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 2
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.MedinahAccomodation = hotelnamemedina.FirstOrDefault();

                var hotellocationmedina = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 2
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.MedinahLocation = hotellocationmedina.FirstOrDefault();







                //azizia
                var hotelnameazizia = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 4
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.AziziaAccomodation = hotelnameazizia.FirstOrDefault();

                var hotellocationazizai = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 4
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.AziziaLocation = hotellocationazizai.FirstOrDefault();






                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);
            }

            return View(model);
        }

        public IActionResult IdCard(int id)
        {

            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);


            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;



            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(fh => fh.CompanyVisas)
                            .ThenInclude(cv => cv.Vendor)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Maktab = _currMember.FamilyHead.Package?.Maktab;
                _model.MaktabNumber = _currMember.FamilyHead.CompanyVisas?.MaktabNumber ?? "N/A";
                _model.CompanyName = _currMember.FamilyHead.CompanyVisas?.CompanyName ?? "N/A";
                _model.ClusterName = _currMember.FamilyHead.CompanyVisas?.Vendor?.ClusterName ?? "N/A";
                _model.OfficeAddress = _currMember.FamilyHead.CompanyVisas?.OfficeAddress ?? "N/A";
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.HbNo = _currMember.FamilyHead.FcId;
                _model.CompanyVisaName = _dbContext.CompanyVisas
                    .Where(e => e.CompanyVisasId == _currMember.FamilyHead.CompanyVisasId)
                    .Select(e => e.CompanyName)
                    .FirstOrDefault();

                _model.MakkahAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MakkahLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                         .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.MedinahAccomodation = (from fh in _dbContext.FamilyHeads
                                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                              select new
                                              {
                                                  FamilyHead = fh,
                                                  FamilyMembers = fm,
                                                  Package = p,
                                                  PackageAccomodations = pa,
                                                  Hotel = h
                                              })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MedinahLocation = (from fh in _dbContext.FamilyHeads
                                          join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                          join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                          where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                          select new
                                          {
                                              FamilyHead = fh,
                                              FamilyMembers = fm,
                                              Package = p,
                                              PackageAccomodations = pa,
                                              Hotel = h
                                          })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.AziziaAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;


                _model.AziziaLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);
            }

            return View(model);
        }

        public IActionResult LuggageCard(int id)
        {

            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);


            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;


            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.HbNo = _currMember.FamilyHead.FcId;
                _model.CompanyVisaName = _dbContext.CompanyVisas
                    .Where(e => e.CompanyVisasId == _currMember.FamilyHead.CompanyVisasId)
                    .Select(e => e.CompanyName)
                    .FirstOrDefault();

                _model.MakkahAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MakkahLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                         .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.MedinahAccomodation = (from fh in _dbContext.FamilyHeads
                                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                              select new
                                              {
                                                  FamilyHead = fh,
                                                  FamilyMembers = fm,
                                                  Package = p,
                                                  PackageAccomodations = pa,
                                                  Hotel = h
                                              })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MedinahLocation = (from fh in _dbContext.FamilyHeads
                                          join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                          join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                          where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                          select new
                                          {
                                              FamilyHead = fh,
                                              FamilyMembers = fm,
                                              Package = p,
                                              PackageAccomodations = pa,
                                              Hotel = h
                                          })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.AziziaAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;


                _model.AziziaLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);
            }

            return View(model);
        }

        public IActionResult NusukCard(int id)
        {
            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);


            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;


            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.DOB = _currMember.FamilyHead.DateOfBirth.ToString("dd/MM/yyyy");
                _model.KSANo = _currMember.FamilyHead.KSAMobileNumber;
                _model.NusukId = _currMember.FamilyHead.NusukId;

                var hotelnamemakkah = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 1
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.MakkahAccomodation = hotelnamemakkah.FirstOrDefault();

                var hotellocationmakkah = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 1
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.MakkahLocation = hotellocationmakkah.FirstOrDefault();


                //madina
                var hotelnamemedina = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 2
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.MedinahAccomodation = hotelnamemedina.FirstOrDefault();

                var hotellocationmedina = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 2
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.MedinahLocation = hotellocationmedina.FirstOrDefault();







                //azizia
                var hotelnameazizia = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 4
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.AziziaAccomodation = hotelnameazizia.FirstOrDefault();

                var hotellocationazizai = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 4
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.AziziaLocation = hotellocationazizai.FirstOrDefault();


                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);
            }

            return View(model);
        }

        public IActionResult SendCards(int id)
        {
            ViewBag.Id = id;

            var query = (from fhs in _dbContext.FamilyHeads
                         join fm in _dbContext.FamilyMembers on fhs.FamilyHeadId equals fm.FamilyHeadId
                         where !fhs.Trashed && fm.HeadId == id
                         select new
                         {
                             fhs.GivenName,
                             fhs.SurName,
                             fhs.FcId,
                             fm.HeadId,
                             fhs.FamilyHeadId
                         }).OrderBy(e => e.HeadId)
                        .ToList();

            var queryForTicketImage = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.CompanyVisas)
                .Where(e =>
                    (e.HajjiBookingStatus == HajjiBookingStatus.Booked || e.HajjiBookingStatus == HajjiBookingStatus.Tentative)
                    && e.FamilyHeadId == id && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                );




            ViewBag.HeadId = query.Any() ? query.First().HeadId : 0;
            ViewBag.HeadName = query.Any() ? query.First().GivenName + " " + query.First().SurName : "";
            ViewBag.Members = query;

            CardModel model = new();

            var visaCardList = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => !e.FamilyHead.Trashed && e.HeadId == id)
                .Select(e => new VisaCardsViewModel
                {
                    VisaImage = string.IsNullOrEmpty(e.FamilyHead.VisaImage) ? "N/A" : e.FamilyHead.VisaImage.Replace("\\", "/"),
                    FileExtension = string.IsNullOrEmpty(e.FamilyHead.VisaImage) ? "N/A" : Path.GetExtension(e.FamilyHead.VisaImage).ToLower()
                })
                .ToList();

            model.VisaCards = visaCardList;


            // Generate PDF canvas info here:
            var pdfCards = new List<object>();
            for (int i = 0; i < visaCardList.Count; i++)
            {
                var card = visaCardList[i];
                if (card.FileExtension == ".pdf")
                {
                    var url = Url.Content("~/" + card.VisaImage); // Make sure this URL is valid
                    pdfCards.Add(new { Url = url, CanvasId = $"visa-pdf-canvas-{i}" }); // Use 'i' as the unique index
                }
            }

            ViewBag.PdfCards = pdfCards;

            var TicketImage = _dbContext.POFlightDetails
                .Include(p => p.POTicket)
                .Where(pt => pt.POTicket.FamilyHeadId == id && !pt.POTicket.Trashed)
                .Select(e => new TicketCardsViewModel
                {
                    TicketImage = string.IsNullOrEmpty(e.TicketImages) ? "N/A" : e.TicketImages.Replace("\\", "/"),
                    TicketFileExtension = string.IsNullOrEmpty(e.TicketImages) ? "N/A" : Path.GetExtension(e.TicketImages).ToLower()
                })
                .Where(t => t.TicketImage != "N/A") // ⛔ Filter out invalid/null image records
                .ToList();

            model.TicketCards = TicketImage;

            // 🔽 Generate ViewBag.PdfTicketCards (for script)
            var pdfTicketCards = new List<object>();
            int pdfIndex = 0;

            foreach (var card in TicketImage)
            {
                if (card.TicketFileExtension == ".pdf")
                {
                    var url = card.TicketImage.StartsWith("/") ? "~" + card.TicketImage : "~/" + card.TicketImage;
                    var resolvedUrl = Url.Content(url); // ✅ Properly resolves ~/ style path
                    pdfTicketCards.Add(new
                    {
                        Url = resolvedUrl,
                        CanvasId = $"ticket-pdf-canvas-{pdfIndex}"
                    });
                    pdfIndex++;
                }
            }

            ViewBag.PdfTicketCards = pdfTicketCards;



            var package = (from fhd in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fhd.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fhd.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fhd.Trashed
                           select new
                           {
                               FamilyHead = fhd,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);

            string[] suffixesToRemove = { "LIMITED", "SERVICES", "PRIVATE LIMITED", "(PRIVATE)", "& UMRAH", "FOR HAJJ" };

            model.Munazzim = RemoveSuffixes(munazzim?.Munazzim.Name ?? string.Empty, suffixesToRemove);
            model.Company = RemoveSuffixes(package.Company.CompanyName, suffixesToRemove);


            // Function to remove suffixes from a given string
            string RemoveSuffixes(string name, string[] suffixes)
            {
                foreach (string suffix in suffixes)
                {
                    name = Regex.Replace(name, $@"\s*{Regex.Escape(suffix)}\s*", "", RegexOptions.IgnoreCase).Trim();
                }
                return name;
            }

            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;

            if (munazzim.Munazzim.Name == "AL BAYAN MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-407-M#20046";
            }
            else if (munazzim.Munazzim.Name == "AL MAIRAJ MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-408-M#20047";
            }
            else if (munazzim.Munazzim.Name == "AL HAMID MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-415-M#20048";
            }

            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(fh => fh.CompanyVisas)
                            .ThenInclude(cv => cv.Vendor)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();



            ViewBag.Id = id;

            FamilyHead _ = _dbContext.FamilyHeads
              .Include(e => e.SPO)
              .Include(e => e.Reference)
              .FirstOrDefault(e => e.FamilyHeadId == id);

            var fam = _dbContext.FamilyMembers
                       .Include(e => e.FamilyHead)
                       .Where(e => e.HeadId == id && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
                       .Select(e => e.FamilyHead.FullName)
                       .ToList();


            Package _p = _dbContext.Packages
                        .Include(e => e.Company)
                        .FirstOrDefault(e => e.PackageId == _.PackageId);

            Company company = _.Package.Company;



            var companyAddress = _dbContext.CompanyAddresses
                                       .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
            var companyEmail = _dbContext.CompanyEmails
                                       .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);
            var companyContact = _dbContext.CompanyContacts
                                        .FirstOrDefault(e => e.CompanyId == company.CompanyId && e.Preferred);


            string FamilyHeadAddress = _dbContext.FamilyHeadAddresses
                            .Where(e => e.FamilyHeadId == id)
                            .Select(e => e.Address)
                            .FirstOrDefault();

            string FamilyHeadContact = _dbContext.FamilyHeadContacts
                                        .Where(e => e.FamilyHeadId == id)
                                        .Select(e => e.Contact)
                                        .FirstOrDefault();

            string addressString = "";

            if (companyAddress is not null)
            {
                addressString = $"{companyAddress.Address}, {companyAddress.ZipCode}, {companyAddress.City}, {companyAddress.Country}";
            }

            var receipts = _dbContext.NewInvoicesWithoutTrash
                .Include(e => e.CompanyBankAccount)
                .Where(e => e.ReceiptStatus == ReceiptStatus.Cleared && e.FamilyHeadId == id)
                .ToList();

            var receivedAmount = receipts
                    .Select(e => AmountConverter.GetConvertedAmount(e.AmountRecieved, _.Currencies ?? Currencies.SAR, e.Currencies, e.ROE))
                    .Sum();

            var totalPackageAmount = PackagePriceManager.GetOverallTotalPrice(_dbContext, id);

            var refundAmount = (from npv in _dbContext.NewPaymentVouchers
                                join np in _dbContext.NewPayments on npv.NewPaymentVoucherId equals np.NewPaymentVoucherId
                                join a in _dbContext.Account on np.AccountsId equals a.AccountsId
                                join fha in _dbContext.FamilyHeads on a.AccountsId equals fha.ReceivableAccountsId
                                where fha.FamilyHeadId == id
                                select npv).ToList();


            var refundAmountTotal = refundAmount
                    .Select(e => e.TotalDebit)
                    .Sum();

            var jvAmountList = _dbContext.JVs
                .Include(e => e.Debit)
                .Include(e => e.Credit)
                .Where(e => (e.DebitId == id || e.CreditId == id) && !e.Trashed)
                .ToList();

            float? jvAdjustment = 0;
            foreach (var jv in jvAmountList)
            {
                if (jv.CreditId == id)
                {
                    jvAdjustment -= jv.TotalCredit;
                }
                else if (jv.DebitId == id)
                {
                    jvAdjustment += jv.TotalDebit;
                }
            }

            var remainingAmount = (totalPackageAmount - receivedAmount) + refundAmountTotal + jvAdjustment;
            //var remainingAmount = totalPackageAmount - receivedAmount;

            ViewBag.CompanyAddress = addressString;
            ViewBag.CompanyEmail = companyEmail?.CompanyEmail;
            ViewBag.CompanyContact = companyContact?.CompanyContact;
            ViewBag.CompanySite = company.Website;


            model.Address = FamilyHeadAddress;
            model.Contact = FamilyHeadContact;
            model.BaseCurrency = _.Currencies.ToString();
            model.Currencies = _.Currencies ?? Currencies.SAR;
            model.Category = _.ApplicantType.ToString();
            model.CNIC = _.CNIC;
            model.CompanyLogo = company.Logo;
            model.CompanyName = company.CompanyName;
            model.Date = DateTime.Now.ToString("dd/MMM/yyyy");
            model.GuestName = _.FullName;
            model.MegaReference = _.Reference?.ReferenceName;
            model.MembersForMiniStatement = fam;
            model.Passport = _.PassportNumber;
            model.PAX = fam.Count.ToString();
            model.Purpose = "Hajj - " + _dbContext.HajjSeasons
            .FirstOrDefault(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            ?.Season;
            model.SPO = _.SPO?.Name;
            model.Receipts = receipts;
            model.TotalPackage = totalPackageAmount.ToString("N2");
            model.ReceivedAmount = receivedAmount.ToString("N2");
            model.RefundAmount = refundAmountTotal?.ToString("N2");
            model.Payments = refundAmount;
            model.JV = jvAmountList;
            model.RemainingAmount = remainingAmount?.ToString("N2");
            model.PrintedBy = _http.HttpContext.Session.GetString("Username");
            model.HBNo = _.FcId;
            model.PackageDetails = _.Package?.Name;

            ViewBag.Email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Email)
                .FirstOrDefault();

            ViewBag.HeadId = id;

            ViewBag.Documents = _dbContext.RequiredDocumentsWithoutTrash;

            var membersOfFamily = _dbContext.FamilyMembers
                                       .Include(e => e.Head)
                                       .Include(e => e.FamilyHead)
                                       .ThenInclude(e => e.RelationWithMehram)
                                       .Include(e => e.FamilyHead)
                                       .ThenInclude(e => e.Fiqah)
                                       .Include(e => e.FamilyHead)
                                       .ThenInclude(e => e.Package)
                                       .ThenInclude(e => e.Company)
                                       .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                                       .ToList();

            ViewBag.TermsAndCondition = _dbContext.Packages
                .FirstOrDefault(e => e.PackageId == membersOfFamily.ElementAtOrDefault(0).FamilyHead.PackageId)
                .TermsnCondition;

            CardModel superModel = new CardModel();
            superModel.BaseCurrencies = membersOfFamily.ElementAtOrDefault(0).FamilyHead.Currencies.ToString();
            superModel.MunazzimName = _dbContext.MunazzimCompanies
                                        .Select(e => new
                                        {
                                            e.CompanyId,
                                            e.Munazzim.Name
                                        })
                                        .FirstOrDefault(e => e.CompanyId == membersOfFamily.ElementAt(0).FamilyHead.Package.CompanyId)?
                                        .Name;


            //NewItinerary

            ViewBag.CordinatorMakkah = _dbContext.Cordinators
                .Where(e => !e.Trashed && e.PlaceId == 1)
                .ToList();

            ViewBag.CordinatorMedinah = _dbContext.Cordinators
                .Where(e => !e.Trashed && e.PlaceId == 2)
                .ToList();

            ViewBag.HajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            ViewBag.HbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            ViewBag.PkgName = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Package.Name)
                .FirstOrDefault();

            //NewItineraryReportModel model = new();

            FamilyHead fh = _dbContext.FamilyHeads
                .Include(e => e.Package)
                .FirstOrDefault(e => e.FamilyHeadId == id);

            ViewBag.CompanyLogo = _dbContext.PackagesWithoutTrash
                .Include(e => e.Company)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.Company.CompanyId == fh.Package.CompanyId)
                .Select(e => e.Company.Logo)
                .FirstOrDefault();

            if (fh.FlightAsPerPackage == true)
            {
                ViewBag.AirlineLogo = _dbContext.PackageFlights
                                .Include(e => e.Airline)
                                .Where(e => !e.Airline.Trashed)
                                .Select(e => e.Airline.AirlineLogo)
                                .Distinct()
                                .ToList();
            }
            else
            {
                ViewBag.AirlineLogo = _dbContext.FamilyHeadFlight
                                .Include(e => e.Airline)
                                .Where(e => !e.Airline.Trashed)
                                .Select(e => e.Airline.AirlineLogo)
                                .Distinct()
                                .ToList();
            }

            var bookingMembers = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                            .AsSplitQuery()
                            .AsNoTracking();




            var MemberExist = _dbContext.POTickets.Any(e => e.FamilyMembersId.HasValue &&
                                        _dbContext.FamilyMembers
                                                  .Where(f => f.HeadId == id)
                                                  .Select(f => f.FamilyHeadId)
                                                  .Contains(e.FamilyMembersId.Value));
            ViewBag.MembersExist = MemberExist;


            if (MemberExist == true)
            {

                var membersForItinerary = _dbContext.FamilyMembers
                       .Include(e => e.FamilyHead)
                       .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                       .AsSplitQuery()
                       .AsNoTracking()
                       .Select(e => new NewItineraryReportMemberModel
                       {
                           Name = e.FamilyHead.FullName,
                           HbNo = e.FamilyHead.FcId,
                           Gender = e.FamilyHead.Gender.ToString(),
                           Passport = e.FamilyHead.PassportNumber,
                           Visa = e.FamilyHead.VisaNumber,
                           TicketNumber = _dbContext.POFlightDetails
                                   .Include(p => p.POTicket)
                                   .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId && !pt.POTicket.Trashed)
                                   .Select(p => p.TicketNumber)
                                   .FirstOrDefault()
                       })
                       .ToList();

                model.MembersForItinerary = membersForItinerary;

            }
            else
            {
                var membersForItinerary = _dbContext.FamilyMembers
                           .Include(e => e.FamilyHead)
                           .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                           .AsSplitQuery()
                           .AsNoTracking()
                           .Select(e => new NewItineraryReportMemberModel
                           {
                               Name = e.FamilyHead.FullName,
                               Gender = e.FamilyHead.Gender.ToString(),
                               Passport = e.FamilyHead.PassportNumber,
                               Visa = e.FamilyHead.VisaNumber,

                           })
                           .ToList();

                model.MembersForItinerary = membersForItinerary;

            }

            //if (MemberExist == true)
            //{

            //    var membersForItinerary = _dbContext.POTickets
            //        .Include(e => e.FamilyMembers) // This assumes you have a FamilyMember navigation property
            //        .Include(pt => pt.FlightDetails)
            //        .Where(e => e.FamilyHeadId == id && !e.FamilyHead.Trashed && !e.Trashed)
            //        .Select(e => new NewItineraryReportMemberModel
            //        {
            //            Name = e.FamilyMembers.FamilyHead.FullName,
            //            Gender = e.FamilyMembers.FamilyHead.Gender.ToString(),
            //            Passport = e.FamilyMembers.FamilyHead.PassportNumber,
            //            Visa = e.FamilyMembers.FamilyHead.VisaNumber,
            //            TicketNumber = e.FlightDetails.FirstOrDefault().TicketNumber
            //        })
            //        .ToList();

            //    model.MembersForItinerary = membersForItinerary;

            //}
            //else
            //{
            //    var membersForItinerary = _dbContext.FamilyMembers
            //               .Include(e => e.FamilyHead)
            //               .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
            //               .AsSplitQuery()
            //               .AsNoTracking()
            //               .Select(e => new NewItineraryReportMemberModel
            //               {
            //                   Name = e.FamilyHead.FullName,
            //                   Gender = e.FamilyHead.Gender.ToString(),
            //                   Passport = e.FamilyHead.PassportNumber,
            //                   Visa = e.FamilyHead.VisaNumber,

            //               })
            //               .ToList();

            //    model.MembersForItinerary = membersForItinerary;

            //}

            if (_dbContext.POTickets.Any(e => e.FamilyMembersId.HasValue &&
                                        _dbContext.FamilyMembers
                                                  .Where(f => f.HeadId == id)
                                                  .Select(f => f.FamilyHeadId)
                                                  .Contains(e.FamilyMembersId.Value)))
            {
                // Step 1: Get all family member IDs under the FamilyHead
                var memberIds = _dbContext.FamilyMembers
                    .Where(f => f.HeadId == id)
                    .Select(f => f.FamilyHeadId)
                    .ToList();

                // Step 2: Get all ticket data for those family members
                var flights = _dbContext.POTickets
                    .Include(e => e.FlightDetails)
                    .Where(e => !e.Trashed && e.FamilyMembersId.HasValue && memberIds.Contains(e.FamilyMembersId.Value))
                    .AsNoTracking()
                    .Select(e => new NewItineraryReportFlightModel
                    {
                        Package = e.FamilyHead.Package.Name,
                        Type = e.TicketType.ToString() ?? "",
                        FlightDetails = e.FlightDetails.Select(fd => new FlightDetailModel
                        {
                            Flight = fd.TicketNumber,
                            Sector = $"{fd.Departure.AirportName}-{fd.Arrival.AirportName}",
                            DepartureDate = fd.DepartureDate.HasValue ? fd.DepartureDate.Value.ToString("dd/MMM/yyyy") : "",
                            DepartureTime = DateTimeManager.DateToHtmlFormat(fd.DepartureTime),
                            ArrivalDate = fd.ArrivalDate.HasValue ? fd.ArrivalDate.Value.ToString("dd/MMM/yyyy") : "",
                            ArrivalTime = DateTimeManager.DateToHtmlFormat(fd.ArrivalTime),
                            Class = fd.FlightClass != null ? fd.FlightClass.ToString() : "N/A"
                        }).ToList(),
                        Luggage = "As per airline policy"
                    })
                    .ToList();


                model.Flights = flights;
            }
            else if (fh.FlightAsPerPackage)
            {
                var flights = _dbContext.PackageFlights
                                                .Where(e => e.PackageId == fh.PackageId)
                                                .OrderBy(e => e.DepartureDate)
                                                .AsNoTracking()
                                                .Select(e => new NewItineraryReportFlightModel
                                                {
                                                    Package = "As Per Package",
                                                    Type = e.FlightType.Value.ToString() ?? "",
                                                    Flight = e.FlightNumber,
                                                    Sector = $"{e.Origin}-{e.Destination}",
                                                    DepartureDate = e.DepartureDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    DepartureTime = e.DepartureTime.Value.ToString("HH:mm") ?? "",
                                                    ArrivalDate = e.ArrivalDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                    Class = e.FlightClass.Value.ToString() ?? "N/A",
                                                    Luggage = "As per airline policy"
                                                })
                                                .ToList();
                model.Flights = flights;
            }
            else
            {
                var flights = _dbContext.FamilyHeadFlight
                                                .Where(e => e.FamilyHeadId == id && e.TicketStatus == TicketStatus.Active)
                                                .Include(e => e.FamilyHead)
                                                .OrderBy(e => e.DepartureDateTime)
                                                .AsNoTracking()
                                                .Select(e => new NewItineraryReportFlightModel
                                                {
                                                    Package = "Customize",
                                                    Type = e.FlightType.Value.ToString() ?? "",
                                                    Flight = e.FlightNo,
                                                    Sector = $"{e.Origin}-{e.Destination}",
                                                    DepartureDate = e.DepartureDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    DepartureTime = e.DepartureDateTime.Value.ToString("HH:mm") ?? "",
                                                    ArrivalDate = e.ArrivalDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                    Class = e.FlightClass.Value.ToString() ?? "N/A",
                                                    Luggage = "As per airline policy"
                                                })
                                                .ToList();
                model.Flights = flights;
            }


            if (fh.TrainAsPerPackage)
            {
                var trains = _dbContext.PackageTrains
                                               .Where(e => e.PackageId == fh.PackageId)
                                               .OrderBy(e => e.DepartureDate)
                                               .AsNoTracking()
                                               .Select(e => new NewItineraryReportTrainModel
                                               {
                                                   Package = "As Per Package",
                                                   Train = e.TrainNumber,
                                                   Sector = $"{e.Origin}-{e.Destination}",
                                                   DepartureDate = e.DepartureDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   DepartureTime = e.DepartureTime.Value.ToString("HH:mm") ?? "",
                                                   ArrivalDate = e.ArrivalDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                   Class = e.TrainClass.Value.ToString() ?? "N/A",
                                                   Luggage = "As per train policy"
                                               })
                                               .ToList();

                model.Trains = trains;
            }
            else
            {
                var trains = _dbContext.FamilyHeadTrain
                                               .Where(e => e.FamilyHeadId == id && e.TicketStatus == TicketStatus.Active)
                                               .Include(e => e.FamilyHead)
                                               .OrderBy(e => e.DepartureDateTime)
                                               .AsNoTracking()
                                               .Select(e => new NewItineraryReportTrainModel
                                               {
                                                   Package = "Customize",
                                                   Train = e.TrainNo,
                                                   Sector = $"{e.Origin}-{e.Destination}",
                                                   DepartureDate = e.DepartureDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   DepartureTime = e.DepartureTime.Value.ToString("HH:mm") ?? "",
                                                   ArrivalDate = e.ArrivalDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                   Class = e.TrainClass.Value.ToString() ?? "N/A",
                                                   Luggage = "As per train policy"
                                               })
                                               .ToList();

                model.Trains = trains;
            }


            if (fh.RouteAsPerPackage)
            {
                var routes = _dbContext.TravelRouteStops
                                    .Include(e => e.TravelRoute)
                                    .Include(e => e.Vehicle)
                                    .Where(e => e.TravelRouteId == fh.Package.TravelRouteId)
                                    .OrderBy(e => e.CheckIn)
                                    .AsNoTracking()
                                    .Select(e => new NewItineraryReportRouteModel
                                    {
                                        Date = e.CheckIn.ToString("dd/MMM") ?? "",
                                        Route = $"{e.Route.StartPlace} - {e.Route.Destination}",
                                        Type = e.TransportType.ToString(),
                                        Vehicle = e.Vehicle.VehicleType ?? ""
                                    })
                                    .ToList();

                model.Routes = routes;
            }
            else
            {
                var routes = _dbContext.FamilyHeadTransports
                                    .Include(e => e.Vehicle)
                                    .Where(e => e.FamilyHeadId == id)
                                    .OrderBy(e => e.CheckIn)
                                    .AsNoTracking()
                                    .Select(e => new NewItineraryReportRouteModel
                                    {
                                        Date = e.CheckIn.Value.ToString("dd/MMM") ?? "",
                                        Route = $"{e.Route.StartPlace} - {e.Route.Destination}",
                                        Type = e.TransportType.ToString(),
                                        Vehicle = e.Vehicle.VehicleType ?? ""
                                    })
                                    .ToList();

                model.Routes = routes;
            }

            var packageAccommodations = _dbContext
                                    .PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .Where(e => e.PackageId == fh.PackageId)
                                    .ToList();

            var memberAccommodations = _dbContext
                                        .FamilyHeadAccomodations
                                        .Where(e => bookingMembers.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                                        .ToList();

            var familyMembers = _dbContext.FamilyMembers
                .Include(fm => fm.FamilyHead)
                .ThenInclude(fh => fh.Package)
                .Where(fm => fm.HeadId == id && !fm.FamilyHead.Trashed)
                .ToList();

            model.Accommodations = new List<NewItineraryReportAccommodationsModel>();

            var asPerPackageMembers = familyMembers
                .Where(fm => fm.FamilyHead.AccomodationAsPerPackage)
                .ToList();

            var packageIds = asPerPackageMembers
                .Select(fm => fm.FamilyHead.PackageId)
                .Distinct()
                .ToList();

            if (packageIds.Any())
            {
                var accommodations = (from pcf in _dbContext.PackageCategoryFacilities
                                      join ifd in _dbContext.IndividualFacilityDescriptions
                                          on pcf.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                      join pa in _dbContext.PackageAccomodtions
                                          on new { ifd.PlaceId, pcf.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId into hotelGroup
                                      from h in hotelGroup.DefaultIfEmpty()
                                      join fp in _dbContext.FoodPackages on pa.FoodPackageId equals fp.FoodPackageId
                                      join r in _dbContext.Rooms on ifd.RoomId equals r.RoomId
                                      where packageIds.Contains(pa.PackageId)
                                            && pa.ActualCheckInTime.HasValue
                                            && pa.ActualCheckOutTime.HasValue
                                      orderby pa.ActualCheckInTime
                                      select new
                                      {
                                          HotelName = h.HotelName ?? pa.AccomodationName,
                                          RoomName = r.RoomName,
                                          CheckIn = pa.ActualCheckInTime,
                                          CheckOut = pa.ActualCheckOutTime,
                                          MealPlan = fp.Package,
                                          pa.Nights,
                                          r.PAX,
                                          ifd.PlaceId
                                      })
                                     .AsEnumerable()
                                     .Select(x => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = x.HotelName?.Trim(),
                                         RoomType = x.RoomName?.Trim(),
                                         CheckIn = x.CheckIn?.ToString("dd/MMM"),
                                         CheckOut = x.CheckOut?.ToString("dd/MMM"),
                                         MealPlan = x.MealPlan?.Trim(),
                                         Nights = x.Nights,
                                         PAX = x.PAX,
                                         PlaceId = x.PlaceId
                                     })
                                     .GroupBy(x => new
                                     {
                                         x.Hotel,
                                         x.RoomType,
                                         x.CheckIn,
                                         x.CheckOut,
                                         x.MealPlan,
                                         x.Nights,
                                         x.PAX,
                                         x.PlaceId
                                     })
                                     .Select(g => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = g.Key.Hotel,
                                         RoomType = g.Key.RoomType,
                                         CheckIn = g.Key.CheckIn,
                                         CheckOut = g.Key.CheckOut,
                                         MealPlan = g.Key.MealPlan,
                                         Nights = g.Key.Nights,
                                         Count = asPerPackageMembers.Count(),
                                         PlaceId = g.Key.PlaceId
                                     })
                                     .ToList();

                model.Accommodations.AddRange(accommodations);
            }

            var notAsPerPackageMembers = familyMembers
                .Where(fm => !fm.FamilyHead.AccomodationAsPerPackage)
                .Select(fm => fm.FamilyHeadId)
                .ToList();

            if (notAsPerPackageMembers.Any())
            {
                var accommodations = (from fha in _dbContext.FamilyHeadAccomodations
                                      join ifd in _dbContext.IndividualFacilityDescriptions
                                          on fha.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                      join pa in _dbContext.PackageAccomodtions
                                          on new { ifd.PlaceId, fh.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                      join fp in _dbContext.FoodPackages
                                          on pa.FoodPackageId equals fp.FoodPackageId
                                      join h in _dbContext.Hotels on fha.HotelId equals h.HotelId
                                      join r in _dbContext.Rooms on ifd.RoomId equals r.RoomId
                                      where notAsPerPackageMembers.Contains(fha.FamilyHeadId)
                                            && fha.CheckIn.HasValue
                                            && fha.CheckOut.HasValue
                                      orderby fha.CheckIn
                                      select new
                                      {
                                          h.HotelName,
                                          r.RoomName,
                                          fha.CheckIn,
                                          fha.CheckOut,
                                          fp.Package,
                                          Night = fha.Night ?? pa.Nights,
                                          r.PAX,
                                          ifd.PlaceId
                                      })
                                     .AsEnumerable()
                                     .Select(x => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = x.HotelName?.Trim(),
                                         RoomType = x.RoomName?.Trim(),
                                         CheckIn = x.CheckIn?.ToString("dd/MMM"),
                                         CheckOut = x.CheckOut?.ToString("dd/MMM"),
                                         MealPlan = x.Package?.Trim(),
                                         Nights = x.Night,
                                         PAX = x.PAX,
                                         PlaceId = x.PlaceId
                                     })
                                     .GroupBy(x => new
                                     {
                                         x.Hotel,
                                         x.RoomType,
                                         x.CheckIn,
                                         x.CheckOut,
                                         x.MealPlan,
                                         x.Nights,
                                         x.PAX,
                                         x.PlaceId
                                     })
                                     .Select(g => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = g.Key.Hotel,
                                         RoomType = g.Key.RoomType,
                                         CheckIn = g.Key.CheckIn,
                                         CheckOut = g.Key.CheckOut,
                                         MealPlan = g.Key.MealPlan,
                                         Nights = g.Key.Nights,
                                         Count = g.Count(),
                                         PlaceId = g.Key.PlaceId
                                     })
                                     .ToList();

                model.Accommodations.AddRange(accommodations);
            }

            /* if (fh.AccomodationAsPerPackage)
             {
                 model.Accommodations = (from pcf in _dbContext.PackageCategoryFacilities
                                         join ifd in _dbContext.IndividualFacilityDescriptions
                                             on pcf.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                         join pa in _dbContext.PackageAccomodtions
                                             on new { ifd.PlaceId, pcf.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                         join h in _dbContext.Hotels
                                             on pa.HotelId equals h.HotelId into hotelGroup
                                         from h in hotelGroup.DefaultIfEmpty()
                                         join fp in _dbContext.FoodPackages
                                             on pa.FoodPackageId equals fp.FoodPackageId
                                         join r in _dbContext.Rooms
                                             on ifd.RoomId equals r.RoomId
                                         where pa.PackageId == fh.PackageId &&
                                               (pa.PlaceId == Place.MAKKAH_ID || pa.PlaceId == Place.MEDINA_ID || pa.PlaceId == Place.AZIZIA_ID
                                                 || pa.PlaceId == Place.MINA_ID)
                                         orderby pa.ActualCheckInTime ascending
                                         select new NewItineraryReportAccommodationsModel
                                         {
                                             Hotel = h.HotelName ?? pa.AccomodationName,
                                             MealPlan = fp.Package,
                                             RoomType = r.RoomName,
                                             CheckIn = pa.ActualCheckInTime.HasValue
                                                 ? pa.ActualCheckInTime.Value.ToString("dd/MMM")
                                                 : pa.CheckIn,
                                             CheckOut = pa.ActualCheckOutTime.HasValue
                                                 ? pa.ActualCheckOutTime.Value.ToString("dd/MMM")
                                                 : pa.CheckOut,
                                             Nights = pa.Nights
                                         }).ToList();
             }
             else
             {

                 var packageAccommodation = _dbContext.PackageAccomodtions
                    .Where(pa => pa.PackageId == fh.PackageId)
                    .ToDictionary(pa => new { pa.PlaceId, pa.PackageId });

                 var memberIds = _dbContext.FamilyMembers
                     .Where(f => f.HeadId == id)
                     .Select(f => f.FamilyHeadId)
                     .ToList();

                 model.Accommodations = (from fha in _dbContext.FamilyHeadAccomodations
                                         join ifd in _dbContext.IndividualFacilityDescriptions
                                             on fha.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                         join pa in _dbContext.PackageAccomodtions
                                             on new { ifd.PlaceId, fh.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                         join fp in _dbContext.FoodPackages
                                             on pa.FoodPackageId equals fp.FoodPackageId
                                         join h in _dbContext.Hotels
                                             on fha.HotelId equals h.HotelId
                                         join r in _dbContext.Rooms
                                             on ifd.RoomId equals r.RoomId
                                         where memberIds.Contains(fha.FamilyHeadId)
                                               && fha.CheckIn.HasValue
                                               && fha.CheckOut.HasValue
                                         orderby fha.CheckIn
                                         select new
                                         {
                                             h.HotelName,
                                             r.RoomName,
                                             fha.CheckIn,
                                             fha.CheckOut,
                                             fp.Package,
                                             pa.Nights
                                         })
                                     .AsEnumerable()
                                     .Select(x => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = x.HotelName.Trim(),
                                         RoomType = x.RoomName.Trim(),
                                         CheckIn = x.CheckIn.Value.ToString("dd/MMM"),
                                         CheckOut = x.CheckOut.Value.ToString("dd/MMM"),
                                         MealPlan = x.Package.Trim(),
                                         Nights = x.Nights
                                     })
                                     .GroupBy(x => new
                                     {
                                         x.Hotel,
                                         x.RoomType,
                                         x.CheckIn,
                                         x.CheckOut,
                                         x.MealPlan,
                                         x.Nights
                                     })
                                     .Select(g => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = g.Key.Hotel,
                                         RoomType = g.Key.RoomType,
                                         CheckIn = g.Key.CheckIn,
                                         CheckOut = g.Key.CheckOut,
                                         MealPlan = g.Key.MealPlan,
                                         Nights = g.Key.Nights,
                                         Count = g.Count() // <-- Number of matching records
                                     })
                                     .ToList();
             }*/


            ViewBag.Mina = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.MINA)
                .FirstOrDefault();

            ViewBag.Arafat = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.ARAFAT)
                .FirstOrDefault();

            ViewBag.MUZDALIFAH = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.MUZDALIFAH)
                .FirstOrDefault();

            ViewBag.RamiDayOne = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.RamiDayOne)
                .FirstOrDefault();

            ViewBag.RamiDayTwo = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.RamiDayTwo)
                .FirstOrDefault();

            ViewBag.RamiDayThree = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.RamiDayThree)
                .FirstOrDefault();

            ViewBag.TermsnCondition = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.TermsnCondition)
                .FirstOrDefault();

            ViewBag.Content = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.Content)
                .FirstOrDefault();


            //End

            ViewBag.ID = id;

            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Maktab = _currMember.FamilyHead.Package?.Maktab;
                _model.MaktabNumber = _currMember.FamilyHead.CompanyVisas?.MaktabNumber ?? "N/A";
                _model.CompanyName = _currMember.FamilyHead.CompanyVisas?.CompanyName ?? "N/A";
                _model.ClusterName = _currMember.FamilyHead.CompanyVisas?.Vendor?.ClusterName ?? "N/A";
                _model.OfficeAddress = _currMember.FamilyHead.CompanyVisas?.OfficeAddress ?? "N/A";
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.HbNo = _currMember.FamilyHead.FcId;


                _model.MakkahAccomodation = (from fhn in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fhn.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fhn.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fhn.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fhn,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MakkahLocation = (from fhm in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fhm.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fhm.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fhm.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fhm,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                         .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.MedinahAccomodation = (from fhn in _dbContext.FamilyHeads
                                              join fm in _dbContext.FamilyMembers on fhn.FamilyHeadId equals fm.FamilyHeadId
                                              join p in _dbContext.Packages on fhn.PackageId equals p.PackageId
                                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                              where !p.Trashed && !fhn.Trashed && !p.Trashed && !h.Trashed
                                              select new
                                              {
                                                  FamilyHead = fhn,
                                                  FamilyMembers = fm,
                                                  Package = p,
                                                  PackageAccomodations = pa,
                                                  Hotel = h
                                              })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MedinahLocation = (from fho in _dbContext.FamilyHeads
                                          join fm in _dbContext.FamilyMembers on fho.FamilyHeadId equals fm.FamilyHeadId
                                          join p in _dbContext.Packages on fho.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                          join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                          where !p.Trashed && !fho.Trashed && !p.Trashed && !h.Trashed
                                          select new
                                          {
                                              FamilyHead = fho,
                                              FamilyMembers = fm,
                                              Package = p,
                                              PackageAccomodations = pa,
                                              Hotel = h
                                          })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.AziziaAccomodation = (from fhp in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fhp.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fhp.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fhp.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fhp,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;


                _model.AziziaLocation = (from fhq in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fhq.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fhq.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fhq.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fhq,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                var result = (from fhi in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fhi.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fhi.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fhi.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fhi,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);
            }

            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                LugguageCardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.HbNo = _currMember.FamilyHead.FcId;

                _model.MakkahAccomodation = (from fhg in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fhg.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fhg.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fhg,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MakkahLocation = (from fhf in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fhf.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fhf.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fhf.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fhf,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                         .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.MedinahAccomodation = (from fht in _dbContext.FamilyHeads
                                              join fm in _dbContext.FamilyMembers on fht.FamilyHeadId equals fm.FamilyHeadId
                                              join p in _dbContext.Packages on fht.PackageId equals p.PackageId
                                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                              where !p.Trashed && !fht.Trashed && !p.Trashed && !h.Trashed
                                              select new
                                              {
                                                  FamilyHead = fht,
                                                  FamilyMembers = fm,
                                                  Package = p,
                                                  PackageAccomodations = pa,
                                                  Hotel = h
                                              })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MedinahLocation = (from fhw in _dbContext.FamilyHeads
                                          join fm in _dbContext.FamilyMembers on fhw.FamilyHeadId equals fm.FamilyHeadId
                                          join p in _dbContext.Packages on fhw.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                          join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                          where !p.Trashed && !fhw.Trashed && !p.Trashed && !h.Trashed
                                          select new
                                          {
                                              FamilyHead = fhw,
                                              FamilyMembers = fm,
                                              Package = p,
                                              PackageAccomodations = pa,
                                              Hotel = h
                                          })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.AziziaAccomodation = (from fhw in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fhw.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fhw.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fhw.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fhw,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;


                _model.AziziaLocation = (from fhw in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fhw.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fhw.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fhw.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fhw,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                var result = (from fhw in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fhw.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fhw.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fhw.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fhw,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.LugguageMembers.Add(_model);
            }

            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                GovermentCardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;

                var hotelnamemakkah = from fhg in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fhg.Trashed
                                            && pa.PlaceId == 1
                                            && fhg.FamilyHeadId == id
                                      select h.HotelName;


                _model.MakkahAccomodation = hotelnamemakkah.FirstOrDefault();

                var hotellocationmakkah = from fhg in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fhg.Trashed
                                                && pa.PlaceId == 1
                                                && fhg.FamilyHeadId == id
                                          select h.Location;


                _model.MakkahLocation = hotellocationmakkah.FirstOrDefault();


                //madina
                var hotelnamemedina = from fhg in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fhg.Trashed
                                            && pa.PlaceId == 2
                                            && fhg.FamilyHeadId == id
                                      select h.HotelName;


                _model.MedinahAccomodation = hotelnamemedina.FirstOrDefault();

                var hotellocationmedina = from fhg in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fhg.Trashed
                                                && pa.PlaceId == 2
                                                && fhg.FamilyHeadId == id
                                          select h.Location;


                _model.MedinahLocation = hotellocationmedina.FirstOrDefault();







                //azizia
                var hotelnameazizia = from fhg in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fhg.Trashed
                                            && pa.PlaceId == 4
                                            && fhg.FamilyHeadId == id
                                      select h.HotelName;


                _model.AziziaAccomodation = hotelnameazizia.FirstOrDefault();

                var hotellocationazizai = from fhg in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fhg.Trashed
                                                && pa.PlaceId == 4
                                                && fhg.FamilyHeadId == id
                                          select h.Location;


                _model.AziziaLocation = hotellocationazizai.FirstOrDefault();






                var result = (from fhg in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fhg.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fhg.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fhg.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fhg,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.GovermentMembers.Add(_model);
            }

            for (int i = 0; i < membersOfFamily.Count; i++)
            {
                FamilyMembers currMember = membersOfFamily.ElementAt(i);

                var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
                var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

                string cId =
                        $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
                        $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                        $".{currMember.FamilyHead.Package.Name}";

                List<FamilyHeadDocument> familyHeadDocuments = _dbContext.FamilyHeadDocuments
                                                        .Include(e => e.RequiredDocument)
                                                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.RequiredDocument.Trashed)
                                                        .ToList();

                FamilyHeadAddress headAddress = _dbContext
                                    .FamilyHeadAddresses
                                    .Include(e => e.Tehsil)
                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact preferredContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                FamilyHeadContact otherContact = _dbContext
                    .FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

                FamilyHeadNominee headNominee = _dbContext
                    .FamilyHeadNominees
                    .Include(e => e.NomineeRelation)
                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

                var trainings = _dbContext
                    .PackageTrainings
                    .Include(e => e.Training)
                    .Where(e => e.PackageId == currMember.FamilyHead.PackageId).ToList();

                Flight flight = _dbContext.Flights
                    .FirstOrDefault(e => e.FlightId == currMember.FamilyHead.FlightId);

                var gifts = _dbContext
                    .PackageGifts
                    .Include(e => e.Gift)
                    .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
                    .ToList();

                var extraFacilities = _dbContext
                    .FamilyHeadExtras
                    .Include(e => e.ExtraFacilityDescription)
                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                    .ToList();

                string presentPostalAddress = $"{headAddress?.Address}, {headAddress?.City}, {headAddress?.ZipCode}, {headAddress?.Country}";
                string packageNameDuration = $"{currMember?.FamilyHead.Package.Name} " +
                    $"({currMember?.FamilyHead.Package.Days})";

                var officialAddress = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberAddressTypeId == 2);
                var officialContact = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberContactTypeId == 2);
                var officialEmail = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberEmailTypeId == 2);

                string officialAddressString = officialAddress is not null ?
                                        $"{officialAddress?.Address}, {officialAddress?.City}, {officialAddress?.ZipCode}, {officialAddress?.Country}" : "";

                string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred)?.Email;

                UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

                DataFormModelEntry Hajjmodel = new DataFormModelEntry
                {
                    Documents = familyHeadDocuments,
                    SignedDate = DateTimeManager.GetDate(userHajjAgreement?.SignedOn),
                    CID = cId,
                    ShowSignature = userHajjAgreement?.SignDataForm ?? false,
                    SignatureBase64 = currMember?.FamilyHead.SignatureBase64,
                    ApplicantType = currMember?.FamilyHead.ApplicantType.ToString(),
                    FamilyHeadImage = currMember?.FamilyHead.MemberImage ?? "",
                    HajjAppNumber = currMember?.FamilyHead.HajjApplicationNumber ?? "-",
                    EnrollmentNumber = currMember?.FamilyHead.ENumber ?? "",
                    MoraPackageNumber = currMember?.FamilyHead.Package.PackageNumber,
                    PackageNameDuration = packageNameDuration,
                    HGOName = currMember?.FamilyHead.Package.Company.CompanyName,
                    SurName = currMember?.FamilyHead.SurName,
                    GivenName = currMember?.FamilyHead.GivenName,
                    FatherHusbandName = currMember?.FamilyHead.FatherHusbandName,
                    PassportNumber = currMember?.FamilyHead.PassportNumber,
                    PassportDateOfExpiry = currMember?.FamilyHead.PassportDateOfExpiry?.ToString("dd-MMM-yyyy"),
                    DateOfBirth = currMember?.FamilyHead.DateOfBirth.ToString("dd-MMM-yyyy"),
                    CNIC = currMember?.FamilyHead.CNIC,
                    BloodGroup = currMember?.FamilyHead.BloodGroup,
                    Gender = currMember?.FamilyHead.Gender,
                    Fiqah = currMember?.FamilyHead.Fiqah.Name.ToString(),
                    IsMarried = (bool)currMember?.FamilyHead.IsMarried ? "YES" : "NO",
                    YearsPerformedHajj = currMember?.FamilyHead.YearsPerformedHajj,
                    PresentPostalAddress = presentPostalAddress,
                    TehsilCode = headAddress?.Tehsil?.Name,
                    MobileNumber = preferredContact?.Contact,
                    TelephoneNumber = otherContact?.Contact,

                    Email = email,
                    OfficialAddress = officialAddressString,
                    OfficialContact = officialContact?.Contact,
                    EmployerBusinessName = currMember.FamilyHead.EmployedAt,
                    Designation = currMember.FamilyHead.Designation,
                    OfficialEmail = officialEmail?.Email,

                    PerformedHajjInLast5Years = (bool)currMember?.FamilyHead.PerformedHajjInLastFiveYears ? "YES" : "NO",
                    WantToHajjEBadal = (bool)currMember?.FamilyHead.WantToPerformHajjEBadal ? "YES" : "NO",
                    WantToPerformHajjAsGroupWorker = (bool)currMember?.FamilyHead.WantToPerformAsGroupWorker ? "YES" : "NO",
                    NomineeName = headNominee?.Name,
                    NomineeRelation = headNominee?.NomineeRelation?.Relation,
                    NomineeCNIC = headNominee?.CNIC,
                    NomineeContact = headNominee?.Contact,
                    NomineeTelephone = "",

                    StayDuration = currMember?.FamilyHead.Package.HajjDuration,
                    SubAgaintAppNumber = currMember?.FamilyHead.SubstitutedAgainstAppNumber,

                    MedinahArrival = currMember?.FamilyHead.Package.MedinahArrival,
                    FlightType = currMember?.FamilyHead.Package.FlightType,
                    FlightClass = currMember?.FamilyHead.FlightClass,
                    TicketPeriod = currMember?.FamilyHead.Package.FlightTicketPeriod,
                    POD = currMember?.FamilyHead.POD,
                    // TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString(),

                    PackageTicketPricePkr = currMember?.FamilyHead.Package.FlightTicketPricePkr,
                    PackageTicketPriceSar = currMember?.FamilyHead.Package.FlightTicketPriceSar,
                    PackageTicketPriceUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd,

                    FlightTicketDifferencesPkr = currMember?.FamilyHead.FlightTicketDifferencesPkr,
                    FlightTicketDifferencesSar = currMember?.FamilyHead.FlightTicketDifferencesSar,
                    FlightTicketDifferencesUsd = currMember?.FamilyHead.FlightTicketDifferencesUsd,

                    IntercityTransport = currMember?.FamilyHead.Package.TransportIntercity,
                    AirportTransport = currMember?.FamilyHead.Package.AirportTransport,
                    MashairTrasport = currMember?.FamilyHead.Package.TransportWithMashair,
                    WithinCity = currMember?.FamilyHead.Package.WithInCity
                };

                var routes = new List<DataFormModelEntry>();

                if (fm.FamilyHead.RouteAsPerPackage == true)
                {
                    routes = _dbContext.TravelRouteStops
                        .Include(e => e.Route)
                        .Include(e => e.Vehicle)
                        .Where(e => e.TravelRouteId == currMember.FamilyHead.Package.TravelRouteId)
                        .OrderBy(e => e.CheckIn)
                        .Select(e => new DataFormModelEntry
                        {
                            Id = e.TravelRouteStopsId,
                            TravelRoutes = e.Route.StartPlace + " to " + e.Route.Destination,
                            CheckIn = e.CheckIn,
                            CheckOut = e.CheckOut,
                            VehicleName = e.Vehicle.VehicleType
                        })
                        .ToList();
                }
                else
                {
                    routes = _dbContext.FamilyHeadTransports
                        .Include(e => e.Route)
                        .Include(e => e.Vehicle)
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                        .OrderBy(e => e.CheckIn)
                        .Select(e => new DataFormModelEntry
                        {
                            Id = e.FamilyHeadTransportId,
                            TravelRoutes = e.Route.StartPlace + " to " + e.Route.Destination,
                            CheckIn = e.CheckIn,
                            //CheckOut = e.CheckOut,
                            VehicleName = e.Vehicle.VehicleType
                        })
                        .ToList();
                }

                if (fm.IsHead)
                {
                    ViewBag.Routes = routes;
                }

                var flights = new List<DataFormModelEntry>();

                if (fm.FamilyHead.FlightAsPerPackage == true)
                {
                    flights = _dbContext.PackageFlights
                        .Include(e => e.Airline)
                        .Where(e => e.PackageId == currMember.FamilyHead.PackageId && e.IsPreffered == true)
                        .OrderBy(e => e.DepartureDate)
                        .Select(e => new DataFormModelEntry
                        {
                            Id = e.PackageFlightsId,
                            Origin = e.Origin,
                            Destination = e.Destination,
                            Airline = e.Airline.Name,
                            ArrivalDateTime = e.ArrivalDate,
                            DepartureDateTime = e.DepartureDate,
                            DepartureTime = e.DepartureTime,
                            ArrivalTime = e.ArrivalTime,
                        })
                        .ToList();
                }
                else
                {
                    flights = _dbContext.FamilyHeadFlight
                        .Include(e => e.Airline)
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                        .OrderBy(e => e.DepartureDateTime)
                        .Select(e => new DataFormModelEntry
                        {
                            Id = e.FamilyHeadFlightId,
                            Origin = e.Origin,
                            Destination = e.Destination,
                            Airline = e.Airline.Name,
                            ArrivalDateTime = e.ArrivalDateTime,
                            DepartureDateTime = e.DepartureDateTime,
                            ArrivalTime = e.ArrivalTime,
                            DepartureTime = e.DepartureTime
                        })
                        .ToList();
                }

                if (fm.IsHead)
                {
                    ViewBag.Flights = flights;
                }

                var trains = new List<DataFormModelEntry>();

                if (fm.FamilyHead.TrainAsPerPackage == true)
                {
                    trains = _dbContext.PackageTrains
                        .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
                        .OrderBy(e => e.DepartureDate)
                        .Select(e => new DataFormModelEntry
                        {
                            Id = e.PackageTrainsId,
                            TrainOrigin = e.Origin,
                            TrainDestination = e.Destination,
                            TrainClass = e.TrainClass.ToString(),
                            TrainArrivalDateTime = e.ArrivalDate,
                            TrainDepartureDateTime = e.DepartureDate,
                            TrainArrivalTime = e.ArrivalTime,
                            TrainDepartureTime = e.DepartureTime
                        })
                        .ToList();
                }
                else
                {
                    trains = _dbContext.FamilyHeadTrain
                        .Where(e => e.FamilyHeadId == currMember.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                        .OrderBy(e => e.DepartureDateTime)
                        .Select(e => new DataFormModelEntry
                        {
                            Id = e.FamilyHeadTrainId,
                            TrainOrigin = e.Origin,
                            TrainDestination = e.Destination,
                            TrainClass = e.TrainClass.ToString(),
                            TrainArrivalDateTime = e.ArrivalDateTime,
                            TrainDepartureDateTime = e.DepartureDateTime,
                            TrainArrivalTime = e.ArrivalTime,
                            TrainDepartureTime = e.DepartureTime,
                        })
                        .ToList();
                }

                if (fm.IsHead)
                {
                    ViewBag.Trains = trains;
                }

                Hajjmodel.Remarks = _dbContext.FamilyHeadRemarks.Where(e => e.FamilyHeadId == currMember.FamilyHeadId).Select(e => e.Remarks).ToList();


                for (int k = 0; k < extraFacilities.Count; k++)
                {
                    FamilyHeadExtra currExtra = extraFacilities.ElementAt(k);

                    Hajjmodel.ExtraFacilities.Add(
                        new DataFormExtraFacility(
                            currExtra.ExtraFacilityDescription?.Description,
                            0f,
                            0f,
                            0f)
                        );
                }


                for (int k = 0; k < gifts.Count; k++)
                {
                    PackageGifts currGift = gifts.ElementAt(k);
                    Hajjmodel.Gifts.Add(new DataFormGift(currGift.Gift.Giveaway));
                }

                if (currMember.FamilyHead.Gender == Gender.Male)
                {
                    FamilyHead mehram = _dbContext.FamilyHeads.FirstOrDefault(e => e.MehramFamilyHeadId == currMember.FamilyHeadId && !e.Trashed);
                    Hajjmodel.AreYouMehramOfALady = mehram is not null ? "YES" : "NO";
                }
                else
                {
                    FamilyHead mehram = _dbContext.FamilyHeads
                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHead.MehramFamilyHeadId && !e.Trashed);
                    Hajjmodel.AreYouMehramOfALady = "-";
                    Hajjmodel.MehramAppNumber = mehram?.HajjApplicationNumber;
                    Hajjmodel.MehramRelationship = currMember?.FamilyHead.RelationWithMehram?.Relation;
                    Hajjmodel.MehramSignatureBase64 = mehram?.SignatureBase64;
                    Hajjmodel.MehramName = mehram?.FullName;
                }

                for (int j = 0; j < trainings.Count; j++)
                {
                    PackageTrainings _t = trainings.ElementAt(j);
                    Hajjmodel.TrainingSessions.Add(new DataFormTrainingSession(
                        _t.Training.TrainingSession, _t.Training.ExpectedTrainingVenue,
                        _t.Training.ExpectedTrainingTime
                        ));
                }


                PackageAccomodations aziziaAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .Include(e => e.Hotel)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 4);

                var makkahAccomadation = _dbContext.PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .Include(e => e.Hotel)

                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);

                PackageAccomodations secondMakkahAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .Include(e => e.Hotel)
                    .OrderByDescending(e => e.PackageAccomodationsId)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);


                string makkahRoomTypeDescription = null;
                string medinahRoomTypeDescription = null;
                string aziziaRoomTypeDescription = null;
                string minaRoomTypeDescription = null;
                string secondMakkahRoomTypeDescription = null;

                if (makkahAccomadation != null)
                {
                    if (makkahAccomadation.PackageAccomodationsId == secondMakkahAccomadation.PackageAccomodationsId)
                    {
                        secondMakkahAccomadation = null;
                    }
                }

                PackageAccomodations medinaAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 2);

                PackageAccomodations minaAccomadation = _dbContext
                    .PackageAccomodtions
                    .Include(e => e.Accomodation)
                    .Include(e => e.FoodPackage)
                    .Include(e => e.AccomodationCategory)
                    .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 3);

                if (aziziaAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                                                .Include(e => e.AdditionalFacilities)
                                                                .ThenInclude(e => e.Room)
                                                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                                                && e.AdditionalFacilities.PlaceId == 4);

                    Hajjmodel.AziziaAccomodation = new DataFormAzizia(
                        aziziaAccomadation.Accomodation.AccomodationName,
                        aziziaAccomadation.Hotel?.HotelName ?? aziziaAccomadation.AccomodationName,
                        aziziaAccomadation.AccomodationCategory.Category,
                        aziziaAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? aziziaAccomadation.CheckIn,
                        aziziaAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? aziziaAccomadation.CheckOut,
                        aziziaAccomadation.Days,
                        aziziaAccomadation.DistanceInMeters?.ToString(),
                        aziziaAccomadation.FoodPackage?.Package,
                        //_indId is null ? aziziaAccomadation.LoadPerRoom : _indId.AdditionalFacilities.Room.PAX,
                        //_indId is null ? aziziaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                        aziziaAccomadation.Shuttle,
                        aziziaAccomadation.ReligiousLectures,
                        _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : ""

                    );

                    var facilityDescription = _dbContext.PackageCategoryFacilities
                        .Include(e => e.IndividualFacilityDescription)
                        .Where(e => !e.IndividualFacilityDescription.Trashed
                                && e.IndividualFacilityDescription.PlaceId == aziziaAccomadation.PlaceId)
                        .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                    aziziaRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                    ViewBag.AziziaRoom = aziziaRoomTypeDescription;
                }

                if (makkahAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Room)
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                            && e.AdditionalFacilities.PlaceId == 1);

                    Hajjmodel.MakkahAccomodation = new DataFormAccomodation(
                    makkahAccomadation.GroupZiarat,
                    makkahAccomadation.Days,
                    makkahAccomadation.DistanceInMeters?.ToString(),
                    makkahAccomadation.FoodPackage?.Package,
                    //_indId is null ? makkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                    makkahAccomadation.Accomodation.AccomodationName,
                    makkahAccomadation.Hotel?.HotelName ?? makkahAccomadation.AccomodationName,
                    makkahAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? makkahAccomadation.CheckIn,
                    makkahAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? makkahAccomadation.CheckOut,
                    //_indId is null ? makkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                    makkahAccomadation.Sharing,
                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                    makkahAccomadation.AccomodationCategory.Category);

                    var facilityDescription = _dbContext.PackageCategoryFacilities
                        .Where(e => !e.IndividualFacilityDescription.Trashed
                                && e.IndividualFacilityDescription.PlaceId == makkahAccomadation.PlaceId)
                        .Include(e => e.IndividualFacilityDescription)
                        .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                    makkahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                    ViewBag.MakkahRoom = makkahRoomTypeDescription;
                }

                if (secondMakkahAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Room)
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                            && e.AdditionalFacilities.PlaceId == 1);

                    Hajjmodel.SecondMakkahAccomodation = new DataFormAccomodation(
                    secondMakkahAccomadation.GroupZiarat,
                    secondMakkahAccomadation.Days,
                    secondMakkahAccomadation.DistanceInMeters?.ToString(),
                    secondMakkahAccomadation.FoodPackage?.Package,
                    //_indId is null ? secondMakkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                    secondMakkahAccomadation.Accomodation.AccomodationName,
                    secondMakkahAccomadation.Hotel?.HotelName ?? secondMakkahAccomadation.AccomodationName,
                    secondMakkahAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? secondMakkahAccomadation.CheckIn,
                    secondMakkahAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? secondMakkahAccomadation.CheckOut,
                    //_indId is null ? secondMakkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                    secondMakkahAccomadation.Sharing,
                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                    secondMakkahAccomadation.AccomodationCategory.Category

                );
                    var facilityDescription = _dbContext.PackageCategoryFacilities
                        .Include(e => e.IndividualFacilityDescription)
                        .Where(e => !e.IndividualFacilityDescription.Trashed
                                && e.IndividualFacilityDescription.PlaceId == secondMakkahAccomadation.PlaceId)
                        .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                    secondMakkahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                    ViewBag.SecondMakkah = secondMakkahRoomTypeDescription;

                }

                if (medinaAccomadation is not null)
                {
                    var _indId = _dbContext.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Room)
                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
                                            && e.AdditionalFacilities.PlaceId == 2);

                    Hajjmodel.MedinaAccomodation = new DataFormAccomodation(
                    medinaAccomadation.GroupZiarat,
                    medinaAccomadation.Days,
                    medinaAccomadation.DistanceInMeters?.ToString(),
                    medinaAccomadation.FoodPackage?.Package,
                    //_indId is null ? medinaAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
                    medinaAccomadation.Accomodation.AccomodationName,
                    medinaAccomadation.Hotel?.HotelName ?? medinaAccomadation.AccomodationName,
                    medinaAccomadation.ActualCheckInTime?.ToString("dd/MMM") ?? medinaAccomadation.CheckIn,
                    medinaAccomadation.ActualCheckOutTime?.ToString("dd/MMM") ?? medinaAccomadation.CheckOut,
                    //_indId is null ? medinaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
                    medinaAccomadation.Sharing,
                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
                    medinaAccomadation.AccomodationCategory.Category
                );

                    var facilityDescription = _dbContext.PackageCategoryFacilities
                        .Include(e => e.IndividualFacilityDescription)
                        .Where(e => !e.IndividualFacilityDescription.Trashed
                                && e.IndividualFacilityDescription.PlaceId == medinaAccomadation.PlaceId)
                        .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                    medinahRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                    ViewBag.MedinahRoom = medinahRoomTypeDescription;

                    //  return Ok(new {
                    //         model.MedinaAccomodation
                    //     });
                }

                if (minaAccomadation is not null)
                {
                    Hajjmodel.MinaAccomodation = new DataFormMina(
                    minaAccomadation.Package.CategoryZone,
                    minaAccomadation.FoodPackage?.Package,
                    minaAccomadation.Bedding,
                    minaAccomadation.Camp,
                    minaAccomadation.Arafat,
                    minaAccomadation.ReligiousLectures
                   );

                    var facilityDescription = _dbContext.PackageCategoryFacilities
                        .Include(e => e.IndividualFacilityDescription)
                        .Where(e => !e.IndividualFacilityDescription.Trashed
                                && e.IndividualFacilityDescription.PlaceId == 3)
                        .FirstOrDefault(desc => desc.PackageId == fm.FamilyHead.PackageId);

                    minaRoomTypeDescription = facilityDescription?.IndividualFacilityDescription.Description;

                    ViewBag.MinaRoom = minaRoomTypeDescription;
                }

                model.Entries.Add(Hajjmodel);

            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult UploadBiometric(int id)
        {
            FamilyHead head = _dbContext.FamilyHeads.Find(id);
            Package package = _dbContext.Packages.Find(head.PackageId);
            ViewBag.PackageName = package.Name;

            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                            .ToList();

            List<FamilyBiometricUploadModel> model = new();

            int i = 1;
            foreach (var m in members)
            {
                var fm = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);
                var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

                string cId = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" +
                $".{fm.Head.ExtractedFamilyNumber}.({famCount}).{fm.FamilyHead.Package.Name}";

                var uploadModel = new FamilyBiometricUploadModel
                {
                    FamilyHeadId = m.FamilyHeadId,
                    FullName = m.FamilyHead.FullName,
                    SNo = i,
                    CID = cId,
                    Saved = fm.FamilyHead.BiometricImage is not null
                };

                model.Add(uploadModel);

                i++;
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult UploadBiometric(BioSaveModel request)
        {
            FamilyHead head = _dbContext.FamilyHeads.Find(request.FamilyHeadId);
            Package package = _dbContext.Packages.Find(head.PackageId);
            ViewBag.PackageName = package.Name;

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                string path = Path.Combine(_host.WebRootPath, "Images", "BiometricImages", request.File.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (FileStream fs = new(path, FileMode.Create))
                {
                    request.File.CopyTo(fs);
                }
                FamilyHead _ = _dbContext.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadId);
                _.BiometricImage = @"Images/BiometricImages/" + request.File.FileName;
                _.ENumber = request.ENo;
                _dbContext.SaveChanges();

                transaction.Commit();
                return Ok(new
                {
                    status = 200,
                    msg = "Upload Done!"
                });
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return BadRequest(new
                {
                    status = 500,
                    msg = "Error!"
                });
            }

        }

        public IActionResult VerifyBiometric(int id)
        {
            FamilyHead head = _dbContext.FamilyHeads.Find(id);
            Package package = _dbContext.Packages.Find(head.PackageId);
            ViewBag.PackageName = package.Name;
            ViewBag.IsAdmin = Convert.ToInt32(_http.HttpContext.Session.GetString("RoleId")) == 1;

            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                            .ToList();

            List<VerifyBioViewModel> model = new();

            int i = 1;
            foreach (var m in members)
            {
                var fm = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);
                var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

                string cId = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" +
                $".{fm.Head.ExtractedFamilyNumber}.({famCount}).{fm.FamilyHead.Package.Name}";

                bool verified = fm.FamilyHead.ENumberVerified;

                VerifyBioViewModel _ = new()
                {
                    SNo = i,
                    FamilyHeadId = fm.FamilyHeadId,
                    FullName = fm.FamilyHead.FullName,
                    CID = cId,
                    Eno = fm.FamilyHead.ENumber,
                    Image = fm.FamilyHead.BiometricImage,
                    Saved = verified
                };

                model.Add(_);

                i++;
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult VerifyBiometric(VerifyBioSaveModel request)
        {
            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                FamilyHead _ = _dbContext.FamilyHeads.Find(request.FamilyHeadId);

                _.ENumber = request.ENo;
                _.ENumberVerified = true;
                _dbContext.SaveChanges();

                transaction.Commit();
                return Ok(new
                {
                    status = 200,
                    msg = "Verified"
                });
            }
            catch (Exception e)
            {
                transaction.Rollback();

                return BadRequest(new
                {
                    msg = "Error"
                });
            }
        }

        public IActionResult DeleteBiometric(int id)
        {
            _dbContext.FamilyHeads.FirstOrDefault(e => e.FamilyHeadId == id).BiometricImage = null;
            _dbContext.SaveChanges();

            int headId = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == id).HeadId;
            return RedirectToAction(nameof(VerifyBiometric), new { id = headId });
        }

        public IActionResult DownloadProof(int id)
        {
            var receipts = _dbContext.Invoices
                    .FirstOrDefault(e => e.FamilyHeadId == id);

            FamilyHead head = _dbContext.FamilyHeads.Find(id);

            var zipName = $"{head.FcId}'s Proof of Payments.zip";
            using (MemoryStream ms = new MemoryStream())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    _dbContext.Invoices.Where(e => e.FamilyHeadId == id).ToList().ForEach(file =>
                    {
                        if (file.ImagePath is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, file.ImagePath);
                            var entry = zip.CreateEntry(file.ImagePath);
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        else if (file.ImageBase64 is not null)
                        {
                            string result = Regex.Replace(file.ImageBase64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                            var bytes = Convert.FromBase64String(result);
                            byte[] fileBytes = Convert.FromBase64String(result);
                            if (fileBytes.Length > 0)
                            {
                                string entryName = $"Images/Invoices/Image Converted {file.InvoiceId}.jpg"; // You may adjust the file name as needed
                                var entry = zip.CreateEntry(entryName);
                                using (var entryStream = entry.Open())
                                {
                                    entryStream.Write(fileBytes, 0, fileBytes.Length);
                                }
                            }
                        }
                    });
                }
                return File(ms.ToArray(), "application/zip", zipName);

            }
        }

        public IActionResult DownloadMedia(int id)
        {
            var members = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Include(e => e.Head)
                            .Where(e => e.HeadId == id)
                            .ToList();

            FamilyHead head = _dbContext.FamilyHeads.Find(id);

            string zipName = $"{head.FcId}'s Media.zip";

            using (MemoryStream ms = new())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    members.ForEach(m =>
                    {
                        if (m.FamilyHead.MemberImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.MemberImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.MemberImage}".Replace("/MemberImages/", "/MemberImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.PassportTokenImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.PassportTokenImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.PassportTokenImage}".Replace("/PassportTokenImages/", "/PassportTokenImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.Ehajj is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.Ehajj);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.Ehajj}".Replace("/Ehajj/", "/Ehajj/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.PassportImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.PassportImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.PassportImage}".Replace("/PassportImages/", "/PassportImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.VisaImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.VisaImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.VisaImage}".Replace("/VisaImages/", "/VisaImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.MemberVaccinationImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.MemberVaccinationImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.MemberVaccinationImage}".Replace("/MemberVaccImages/", "/MemberVaccImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                        if (m.FamilyHead.BiometricImage is not null)
                        {
                            string path = Path.Combine(_host.WebRootPath, m.FamilyHead.BiometricImage);
                            var entry = zip.CreateEntry($"{head.FcId}/{m.FamilyHead.BiometricImage}".Replace("/BiometricImages/", "/BiometricImages/" + m.FamilyHead.FamilyHeadId + "/"));
                            var fileBytes = System.IO.File.ReadAllBytes(path);
                            using (var fileStream = new MemoryStream(fileBytes))
                            using (var entryStream = entry.Open())
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    });
                }

                return File(ms.ToArray(), "application/zip", zipName);
            }
        }

        public async Task<JsonResult> VerifyEHajjVisa(int id)
        {
            var _ = _dbContext.FamilyHeads.Find(id);
            _.EHajjVerified = true;
            await _dbContext.SaveChangesAsync();
            return Json(new
            {
                status = 200,
                msg = "Member Verified"
            });
        }

        public IActionResult NewItinerary(int id)
        {
            ViewBag.CordinatorMakkah = _dbContext.Cordinators
                .Where(e => !e.Trashed && e.PlaceId == 1)
                .ToList();

            ViewBag.CordinatorMedinah = _dbContext.Cordinators
                .Where(e => !e.Trashed && e.PlaceId == 2)
                .ToList();

            ViewBag.HajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            ViewBag.HbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            ViewBag.PkgName = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Package.Name)
                .FirstOrDefault();

            ViewBag.ID = id;

            NewItineraryReportModel model = new();

            FamilyHead fh = _dbContext.FamilyHeads
                .Include(e => e.Package)
                .FirstOrDefault(e => e.FamilyHeadId == id);

            ViewBag.CompanyLogo = _dbContext.PackagesWithoutTrash
                .Include(e => e.Company)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.Company.CompanyId == fh.Package.CompanyId)
                .Select(e => e.Company.Logo)
                .FirstOrDefault();

            if (fh.FlightAsPerPackage == true)
            {
                ViewBag.AirlineLogo = _dbContext.PackageFlights
                                .Include(e => e.Airline)
                                .Where(e => !e.Airline.Trashed)
                                .Select(e => e.Airline.AirlineLogo)
                                .Distinct()
                                .ToList();
            }
            else
            {
                ViewBag.AirlineLogo = _dbContext.FamilyHeadFlight
                                .Include(e => e.Airline)
                                .Where(e => !e.Airline.Trashed)
                                .Select(e => e.Airline.AirlineLogo)
                                .Distinct()
                                .ToList();
            }

            var bookingMembers = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                            .AsSplitQuery()
                            .AsNoTracking();




            var MemberExist = _dbContext.POTickets.Any(e => e.FamilyMembersId.HasValue &&
                                        _dbContext.FamilyMembers
                                                  .Where(f => f.HeadId == id)
                                                  .Select(f => f.FamilyHeadId)
                                                  .Contains(e.FamilyMembersId.Value));
            ViewBag.MembersExist = MemberExist;

            if (MemberExist == true)
            {

                //var membersForItinerary = _dbContext.POTickets
                //    .Include(f => f.FamilyMembers)
                //    .Include(e => e.FamilyHead)// This assumes you have a FamilyMember navigation property
                //    .Include(pt => pt.FlightDetails)
                //    .Where(e => e.FamilyMembers.HeadId == id && !e.FamilyHead.Trashed && !e.Trashed)
                //    .Select(e => new NewItineraryReportMemberModel
                //    {
                //        Name = e.FamilyMembers.FamilyHead.FullName,
                //        Gender = e.FamilyMembers.FamilyHead.Gender.ToString(),
                //        Passport = e.FamilyMembers.FamilyHead.PassportNumber,
                //        Visa = e.FamilyMembers.FamilyHead.VisaNumber,
                //        TicketNumber = e.FlightDetails.FirstOrDefault().TicketNumber
                //    })
                //    .ToList();

                var membersForItinerary = _dbContext.FamilyMembers
                        .Include(e => e.FamilyHead)
                        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                        .AsSplitQuery()
                        .AsNoTracking()
                        .Select(e => new NewItineraryReportMemberModel
                        {
                            Name = e.FamilyHead.FullName,
                            HbNo = e.FamilyHead.FcId,
                            Gender = e.FamilyHead.Gender.ToString(),
                            Passport = e.FamilyHead.PassportNumber,
                            Visa = e.FamilyHead.VisaNumber,
                            TicketNumber = _dbContext.POFlightDetails
                                    .Include(p => p.POTicket)
                                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId && !pt.POTicket.Trashed)
                                    .Select(p => p.TicketNumber)
                                    .FirstOrDefault()
                        })
                        .ToList();

                model.Members = membersForItinerary;

            }
            else
            {
                var membersForItinerary = _dbContext.FamilyMembers
                           .Include(e => e.FamilyHead)
                           .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                           .AsSplitQuery()
                           .AsNoTracking()
                           .Select(e => new NewItineraryReportMemberModel
                           {
                               Name = e.FamilyHead.FullName,
                               HbNo = e.FamilyHead.FcId,
                               Gender = e.FamilyHead.Gender.ToString(),
                               Passport = e.FamilyHead.PassportNumber,
                               Visa = e.FamilyHead.VisaNumber,

                           })
                           .ToList();

                model.Members = membersForItinerary;

            }

            if (_dbContext.POTickets.Any(e => e.FamilyMembersId.HasValue &&
                                        _dbContext.FamilyMembers
                                                  .Where(f => f.HeadId == id)
                                                  .Select(f => f.FamilyHeadId)
                                                  .Contains(e.FamilyMembersId.Value)))
            {
                // Step 1: Get all family member IDs under the FamilyHead
                var memberIds = _dbContext.FamilyMembers
                    .Where(f => f.HeadId == id)
                    .Select(f => f.FamilyHeadId)
                    .ToList();

                // Step 2: Get all ticket data for those family members
                var flights = _dbContext.POTickets
                    .Include(e => e.FlightDetails)
                    .Where(e => !e.Trashed && e.FamilyMembersId.HasValue && memberIds.Contains(e.FamilyMembersId.Value))
                    .AsNoTracking()
                    .Select(e => new NewItineraryReportFlightModel
                    {
                        Package = e.FamilyHead.Package.Name,
                        Type = e.TicketType.ToString() ?? "",
                        FlightDetails = e.FlightDetails.Select(fd => new FlightDetailModel
                        {
                            Flight = fd.TicketNumber,
                            Sector = $"{fd.Departure.AirportName}-{fd.Arrival.AirportName}",
                            DepartureDate = fd.DepartureDate.HasValue ? fd.DepartureDate.Value.ToString("dd/MMM/yyyy") : "",
                            DepartureTime = DateTimeManager.DateToHtmlFormat(fd.DepartureTime),
                            ArrivalDate = fd.ArrivalDate.HasValue ? fd.ArrivalDate.Value.ToString("dd/MMM/yyyy") : "",
                            ArrivalTime = DateTimeManager.DateToHtmlFormat(fd.ArrivalTime),
                            Class = fd.FlightClass != null ? fd.FlightClass.ToString() : "N/A"
                        }).ToList(),
                        Luggage = "As per airline policy"
                    })
                    .ToList();


                model.Flights = flights;
            }
            else if (fh.FlightAsPerPackage)
            {
                var flights = _dbContext.PackageFlights
                                                .Where(e => e.PackageId == fh.PackageId)
                                                .OrderBy(e => e.DepartureDate)
                                                .AsNoTracking()
                                                .Select(e => new NewItineraryReportFlightModel
                                                {
                                                    Package = "As Per Package",
                                                    Type = e.FlightType.Value.ToString() ?? "",
                                                    Flight = e.FlightNumber,
                                                    Sector = $"{e.Origin}-{e.Destination}",
                                                    DepartureDate = e.DepartureDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    DepartureTime = e.DepartureTime.Value.ToString("HH:mm") ?? "",
                                                    ArrivalDate = e.ArrivalDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                    Class = e.FlightClass.Value.ToString() ?? "N/A",
                                                    Luggage = "As per airline policy"
                                                })
                                                .ToList();
                model.Flights = flights;
            }
            else
            {
                var flights = _dbContext.FamilyHeadFlight
                                                .Where(e => e.FamilyHeadId == id && e.TicketStatus == TicketStatus.Active)
                                                .Include(e => e.FamilyHead)
                                                .OrderBy(e => e.DepartureDateTime)
                                                .AsNoTracking()
                                                .Select(e => new NewItineraryReportFlightModel
                                                {
                                                    Package = "Customize",
                                                    Type = e.FlightType.Value.ToString() ?? "",
                                                    Flight = e.FlightNo,
                                                    Sector = $"{e.Origin}-{e.Destination}",
                                                    DepartureDate = e.DepartureDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    DepartureTime = e.DepartureDateTime.Value.ToString("HH:mm") ?? "",
                                                    ArrivalDate = e.ArrivalDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                    ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                    Class = e.FlightClass.Value.ToString() ?? "N/A",
                                                    Luggage = "As per airline policy"
                                                })
                                                .ToList();
                model.Flights = flights;
            }


            if (fh.TrainAsPerPackage)
            {
                var trains = _dbContext.PackageTrains
                                               .Where(e => e.PackageId == fh.PackageId)
                                               .OrderBy(e => e.DepartureDate)
                                               .AsNoTracking()
                                               .Select(e => new NewItineraryReportTrainModel
                                               {
                                                   Package = "As Per Package",
                                                   Train = e.TrainNumber,
                                                   Sector = $"{e.Origin}-{e.Destination}",
                                                   DepartureDate = e.DepartureDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   DepartureTime = e.DepartureTime.Value.ToString("HH:mm") ?? "",
                                                   ArrivalDate = e.ArrivalDate.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                   Class = e.TrainClass.Value.ToString() ?? "N/A",
                                                   Luggage = "As per train policy"
                                               })
                                               .ToList();

                model.Trains = trains;
            }
            else
            {
                var trains = _dbContext.FamilyHeadTrain
                                               .Where(e => e.FamilyHeadId == id && e.TicketStatus == TicketStatus.Active)
                                               .Include(e => e.FamilyHead)
                                               .OrderBy(e => e.DepartureDateTime)
                                               .AsNoTracking()
                                               .Select(e => new NewItineraryReportTrainModel
                                               {
                                                   Package = "Customize",
                                                   Train = e.TrainNo,
                                                   Sector = $"{e.Origin}-{e.Destination}",
                                                   DepartureDate = e.DepartureDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   DepartureTime = e.DepartureTime.Value.ToString("HH:mm") ?? "",
                                                   ArrivalDate = e.ArrivalDateTime.Value.ToString("dd/MMM/yyyy") ?? "",
                                                   ArrivalTime = e.ArrivalTime.Value.ToString("HH:mm") ?? "",
                                                   Class = e.TrainClass.Value.ToString() ?? "N/A",
                                                   Luggage = "As per train policy"
                                               })
                                               .ToList();

                model.Trains = trains;
            }


            if (fh.RouteAsPerPackage)
            {
                var routes = _dbContext.TravelRouteStops
                                    .Include(e => e.TravelRoute)
                                    .Include(e => e.Vehicle)
                                    .Where(e => e.TravelRouteId == fh.Package.TravelRouteId)
                                    .OrderBy(e => e.CheckIn)
                                    .AsNoTracking()
                                    .Select(e => new NewItineraryReportRouteModel
                                    {
                                        Date = e.CheckIn.ToString("dd/MMM") ?? "",
                                        Route = $"{e.Route.StartPlace} - {e.Route.Destination}",
                                        Type = e.TransportType.ToString(),
                                        Vehicle = e.Vehicle.VehicleType ?? ""
                                    })
                                    .ToList();

                model.Routes = routes;
            }
            else
            {
                var routes = _dbContext.FamilyHeadTransports
                                    .Include(e => e.Vehicle)
                                    .Where(e => e.FamilyHeadId == id)
                                    .OrderBy(e => e.CheckIn)
                                    .AsNoTracking()
                                    .Select(e => new NewItineraryReportRouteModel
                                    {
                                        Date = e.CheckIn.Value.ToString("dd/MMM") ?? "",
                                        Route = $"{e.Route.StartPlace} - {e.Route.Destination}",
                                        Type = e.TransportType.ToString(),
                                        Vehicle = e.Vehicle.VehicleType ?? ""
                                    })
                                    .ToList();

                model.Routes = routes;
            }

            var packageAccommodations = _dbContext
                                    .PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .Where(e => e.PackageId == fh.PackageId)
                                    .ToList();

            var memberAccommodations = _dbContext
                                        .FamilyHeadAccomodations
                                        .Where(e => bookingMembers.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                                        .ToList();



            /*if (fh.AccomodationAsPerPackage)
            {
                var Count = _dbContext.FamilyMembers
                    .Where(f => f.HeadId == id && f.FamilyHead.Trashed)
                    .Select(f => f.FamilyHead.PackageId)
                    .Count();

                model.Accommodations = (from pcf in _dbContext.PackageCategoryFacilities
                                        join ifd in _dbContext.IndividualFacilityDescriptions
                                            on pcf.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                        join pa in _dbContext.PackageAccomodtions
                                            on new { ifd.PlaceId, pcf.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                        join h in _dbContext.Hotels
                                            on pa.HotelId equals h.HotelId into hotelGroup
                                        from h in hotelGroup.DefaultIfEmpty()
                                        join fp in _dbContext.FoodPackages
                                            on pa.FoodPackageId equals fp.FoodPackageId
                                        join r in _dbContext.Rooms
                                            on ifd.RoomId equals r.RoomId
                                        where pa.PackageId == fh.PackageId &&
                                              (pa.PlaceId == Place.MAKKAH_ID || pa.PlaceId == Place.MEDINA_ID || pa.PlaceId == Place.AZIZIA_ID
                                                || pa.PlaceId == Place.MINA_ID)
                                        orderby pa.ActualCheckInTime ascending
                                        select new NewItineraryReportAccommodationsModel
                                        {
                                            Hotel = h.HotelName ?? pa.AccomodationName,
                                            MealPlan = fp.Package,
                                            RoomType = r.RoomName,
                                            CheckIn = pa.ActualCheckInTime.HasValue
                                                ? pa.ActualCheckInTime.Value.ToString("dd/MMM")
                                                : pa.CheckIn,
                                            CheckOut = pa.ActualCheckOutTime.HasValue
                                                ? pa.ActualCheckOutTime.Value.ToString("dd/MMM")
                                                : pa.CheckOut,
                                            Nights = pa.Nights,
                                            Count = Count
                                        }).ToList();


                *//*model.Accommodations = (
                                        from pcf in _dbContext.PackageCategoryFacilities
                                        join ifd in _dbContext.IndividualFacilityDescriptions
                                            on pcf.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                        join pa in _dbContext.PackageAccomodtions
                                            on new { ifd.PlaceId, pcf.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                        join h in _dbContext.Hotels
                                            on pa.HotelId equals h.HotelId into hotelGroup
                                        from h in hotelGroup.DefaultIfEmpty()
                                        join fp in _dbContext.FoodPackages
                                            on pa.FoodPackageId equals fp.FoodPackageId
                                        join r in _dbContext.Rooms
                                            on ifd.RoomId equals r.RoomId
                                        join pam in _dbContext.PackageAccomodtions
                                            on pa.PackageAccomodationsId equals pam.PackageAccomodationsId into pamGroup
                                        where packageid.Contains(pa.PackageId) &&
                                              (pa.PlaceId == Place.MAKKAH_ID || pa.PlaceId == Place.MEDINA_ID ||
                                               pa.PlaceId == Place.AZIZIA_ID || pa.PlaceId == Place.MINA_ID)
                                        orderby pa.ActualCheckInTime ascending
                                        select new NewItineraryReportAccommodationsModel
                                        {
                                            Hotel = h.HotelName ?? pa.AccomodationName,
                                            MealPlan = fp.Package,
                                            RoomType = r.RoomName,
                                            CheckIn = pa.ActualCheckInTime.HasValue
                                                ? pa.ActualCheckInTime.Value.ToString("dd/MMM")
                                                : pa.CheckIn,
                                            CheckOut = pa.ActualCheckOutTime.HasValue
                                                ? pa.ActualCheckOutTime.Value.ToString("dd/MMM")
                                                : pa.CheckOut,
                                            Nights = pa.Nights,
                                            Count = pamGroup.Count()
                                        }).ToList();*/

            /*model.Accommodations = (
                                    from pcf in _dbContext.PackageCategoryFacilities
                                    join ifd in _dbContext.IndividualFacilityDescriptions
                                        on pcf.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                    join pa in _dbContext.PackageAccomodtions
                                        on new { ifd.PlaceId, pcf.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                    join h in _dbContext.Hotels
                                        on pa.HotelId equals h.HotelId into hotelGroup
                                    from h in hotelGroup.DefaultIfEmpty()
                                    join fp in _dbContext.FoodPackages
                                        on pa.FoodPackageId equals fp.FoodPackageId
                                    join r in _dbContext.Rooms
                                        on ifd.RoomId equals r.RoomId
                                    join pam in _dbContext.PackageAccomodtions
                                        on pa.PackageAccomodationsId equals pam.PackageAccomodationsId into pamGroup
                                    where Count.Contains(pa.PackageId) &&
                                          (pa.PlaceId == Place.MAKKAH_ID || pa.PlaceId == Place.MEDINA_ID ||
                                           pa.PlaceId == Place.AZIZIA_ID || pa.PlaceId == Place.MINA_ID)
                                    orderby pa.ActualCheckInTime
                                    select new
                                    {
                                        Hotel = h.HotelName ?? pa.AccomodationName,
                                        MealPlan = fp.Package,
                                        RoomType = r.RoomName,
                                        CheckIn = pa.ActualCheckInTime.HasValue
                                                ? pa.ActualCheckInTime.Value.ToString("dd/MMM")
                                                : pa.CheckIn,
                                        CheckOut = pa.ActualCheckOutTime.HasValue
                                                ? pa.ActualCheckOutTime.Value.ToString("dd/MMM")
                                                : pa.CheckOut,
                                        Nights = pa.Nights,
                                        Count = pamGroup.Count()
                                    })
                                    .AsEnumerable()
                                    .Select(x => new NewItineraryReportAccommodationsModel
                                    {
                                        Hotel = x.Hotel?.Trim(),
                                        RoomType = x.RoomType?.Trim(),
                                        MealPlan = x.MealPlan?.Trim(),
                                        CheckIn = x.CheckIn,
                                        CheckOut = x.CheckOut,
                                        Nights = x.Nights,
                                        Count = x.Count,
                                    })
                                    .GroupBy(x => new
                                    {
                                        x.Hotel,
                                        x.RoomType,
                                        x.CheckIn,
                                        x.CheckOut,
                                        x.MealPlan,
                                        x.Nights,
                                        x.Count
                                    })
                                .Select(g => new NewItineraryReportAccommodationsModel
                                {
                                    Hotel = g.Key.Hotel,
                                    RoomType = g.Key.RoomType,
                                    CheckIn = g.Key.CheckIn,
                                    CheckOut = g.Key.CheckOut,
                                    MealPlan = g.Key.MealPlan,
                                    Nights = g.Key.Nights,
                                    Count = g.Key.Count
                                })
                                .ToList();*//*

        }
        else
        {
            var packageAccommodation = _dbContext.PackageAccomodtions
                .Where(pa => pa.PackageId == fh.PackageId)
                .ToDictionary(pa => new { pa.PlaceId, pa.PackageId });

            var memberIds = _dbContext.FamilyMembers
                .Where(f => f.HeadId == id)
                .Select(f => f.FamilyHeadId)
                .ToList();

            *//*model.Accommodations = (from fha in _dbContext.FamilyHeadAccomodations
                                    join ifd in _dbContext.IndividualFacilityDescriptions
                                        on fha.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                    join pa in _dbContext.PackageAccomodtions
                                        on new { ifd.PlaceId, fh.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                    join fp in _dbContext.FoodPackages
                                        on pa.FoodPackageId equals fp.FoodPackageId
                                    join h in _dbContext.Hotels
                                        on fha.HotelId equals h.HotelId
                                    join r in _dbContext.Rooms
                                        on ifd.RoomId equals r.RoomId
                                    where memberIds.Contains(fha.FamilyHeadId) && fha.CheckIn.HasValue && fha.CheckOut.HasValue
                                    orderby fha.CheckIn
                                    select new NewItineraryReportAccommodationsModel
                                    {
                                        Hotel = h.HotelName,
                                        RoomType = r.RoomName,
                                        CheckIn = fha.CheckIn.Value.ToString("dd/MMM"),
                                        CheckOut = fha.CheckOut.Value.ToString("dd/MMM"),
                                        MealPlan = fp.Package,
                                        Nights = pa.Nights
                                    })
                                    .GroupBy(x => new { x.Hotel, x.RoomType, x.CheckIn, x.CheckOut, x.MealPlan, x.Nights })
                                    .Select(g => g.First())
                                    .ToList();*//*

            model.Accommodations = (from fha in _dbContext.FamilyHeadAccomodations
                                    join ifd in _dbContext.IndividualFacilityDescriptions
                                        on fha.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                    join pa in _dbContext.PackageAccomodtions
                                        on new { ifd.PlaceId, fh.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                    join fp in _dbContext.FoodPackages
                                        on pa.FoodPackageId equals fp.FoodPackageId
                                    join h in _dbContext.Hotels
                                        on fha.HotelId equals h.HotelId
                                    join r in _dbContext.Rooms
                                        on ifd.RoomId equals r.RoomId
                                    where memberIds.Contains(fha.FamilyHeadId)
                                          && fha.CheckIn.HasValue
                                          && fha.CheckOut.HasValue
                                    orderby fha.CheckIn
                                    select new
                                    {
                                        h.HotelName,
                                        r.RoomName,
                                        fha.CheckIn,
                                        fha.CheckOut,
                                        fp.Package,
                                        pa.Nights,
                                        r.PAX
                                    })
                                .AsEnumerable()
                                .Select(x => new NewItineraryReportAccommodationsModel
                                {
                                    Hotel = x.HotelName.Trim(),
                                    RoomType = x.RoomName.Trim(),
                                    CheckIn = x.CheckIn.Value.ToString("dd/MMM"),
                                    CheckOut = x.CheckOut.Value.ToString("dd/MMM"),
                                    MealPlan = x.Package.Trim(),
                                    Nights = x.Nights,
                                    PAX = x.PAX
                                })
                                .GroupBy(x => new
                                {
                                    x.Hotel,
                                    x.RoomType,
                                    x.CheckIn,
                                    x.CheckOut,
                                    x.MealPlan,
                                    x.Nights,
                                    x.PAX
                                })
                                .Select(g => new NewItineraryReportAccommodationsModel
                                {
                                    Hotel = g.Key.Hotel,
                                    RoomType = g.Key.RoomType,
                                    CheckIn = g.Key.CheckIn,
                                    CheckOut = g.Key.CheckOut,
                                    MealPlan = g.Key.MealPlan,
                                    Nights = g.Key.Nights,
                                    Count = g.Count()
                                })
                                .ToList();



        }*/
            var familyMembers = _dbContext.FamilyMembers
     .Include(fm => fm.FamilyHead)
         .ThenInclude(fh => fh.Package)
     .Where(fm => fm.HeadId == id && !fm.FamilyHead.Trashed)
     .ToList();

            model.Accommodations = new List<NewItineraryReportAccommodationsModel>();

            // Handle AsPerPackage == true (collect all unique packageIds)
            var asPerPackageMembers = familyMembers
                .Where(fm => fm.FamilyHead.AccomodationAsPerPackage)
                .ToList();

            var packageIds = asPerPackageMembers
                .Select(fm => fm.FamilyHead.PackageId)
                .Distinct()
                .ToList();

            if (packageIds.Any())
            {
                var accommodations = (from pcf in _dbContext.PackageCategoryFacilities
                                      join ifd in _dbContext.IndividualFacilityDescriptions
                                          on pcf.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                      join pa in _dbContext.PackageAccomodtions
                                          on new { ifd.PlaceId, pcf.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId into hotelGroup
                                      from h in hotelGroup.DefaultIfEmpty()
                                      join fp in _dbContext.FoodPackages on pa.FoodPackageId equals fp.FoodPackageId
                                      join r in _dbContext.Rooms on ifd.RoomId equals r.RoomId
                                      where packageIds.Contains(pa.PackageId)
                                            && pa.ActualCheckInTime.HasValue
                                            && pa.ActualCheckOutTime.HasValue
                                      orderby pa.ActualCheckInTime
                                      select new
                                      {
                                          HotelName = h.HotelName ?? pa.AccomodationName,
                                          RoomName = r.RoomName,
                                          CheckIn = pa.ActualCheckInTime,
                                          CheckOut = pa.ActualCheckOutTime,
                                          MealPlan = fp.Package,
                                          pa.Nights,
                                          r.PAX,
                                          ifd.PlaceId
                                      })
                                     .AsEnumerable()
                                     .Select(x => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = x.HotelName?.Trim(),
                                         RoomType = x.RoomName?.Trim(),
                                         CheckIn = x.CheckIn?.ToString("dd/MMM"),
                                         CheckOut = x.CheckOut?.ToString("dd/MMM"),
                                         MealPlan = x.MealPlan?.Trim(),
                                         Nights = x.Nights,
                                         PAX = x.PAX,
                                         PlaceId = x.PlaceId
                                     })
                                     .GroupBy(x => new
                                     {
                                         x.Hotel,
                                         x.RoomType,
                                         x.CheckIn,
                                         x.CheckOut,
                                         x.MealPlan,
                                         x.Nights,
                                         x.PAX,
                                         x.PlaceId
                                     })
                                     .Select(g => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = g.Key.Hotel,
                                         RoomType = g.Key.RoomType,
                                         CheckIn = g.Key.CheckIn,
                                         CheckOut = g.Key.CheckOut,
                                         MealPlan = g.Key.MealPlan,
                                         Nights = g.Key.Nights,
                                         Count = asPerPackageMembers.Count(),
                                         PlaceId = g.Key.PlaceId
                                     })
                                     .ToList();

                model.Accommodations.AddRange(accommodations);
            }

            // Handle custom accommodations for those not AsPerPackage
            var notAsPerPackageMembers = familyMembers
                .Where(fm => !fm.FamilyHead.AccomodationAsPerPackage)
                .Select(fm => fm.FamilyHeadId)
                .ToList();

            if (notAsPerPackageMembers.Any())
            {
                var accommodations = (from fha in _dbContext.FamilyHeadAccomodations
                                      join ifd in _dbContext.IndividualFacilityDescriptions
                                          on fha.IndividualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                      join pa in _dbContext.PackageAccomodtions
                                          on new { ifd.PlaceId, fh.PackageId } equals new { pa.PlaceId, pa.PackageId }
                                      join fp_fha in _dbContext.FoodPackages on fha.FoodPackageId equals fp_fha.FoodPackageId into fha_fp
                                      from fp_fha in fha_fp.DefaultIfEmpty()
                                      join fp_pa in _dbContext.FoodPackages on pa.FoodPackageId equals fp_pa.FoodPackageId into pa_fp
                                      from fp_pa in pa_fp.DefaultIfEmpty()
                                      join h in _dbContext.Hotels on fha.HotelId equals h.HotelId
                                      join r in _dbContext.Rooms on ifd.RoomId equals r.RoomId
                                      where notAsPerPackageMembers.Contains(fha.FamilyHeadId)
                                            && fha.CheckIn.HasValue
                                            && fha.CheckOut.HasValue
                                      orderby fha.CheckIn
                                      select new
                                      {
                                          h.HotelName,
                                          r.RoomName,
                                          fha.CheckIn,
                                          fha.CheckOut,
                                          Package = fha.FoodPackageId != null ? fp_fha.Package : fp_pa.Package,
                                          Night = fha.Night ?? pa.Nights,
                                          r.PAX,
                                          ifd.PlaceId
                                      })
                                     .AsEnumerable()
                                     .Select(x => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = x.HotelName?.Trim(),
                                         RoomType = x.RoomName?.Trim(),
                                         CheckIn = x.CheckIn?.ToString("dd/MMM"),
                                         CheckOut = x.CheckOut?.ToString("dd/MMM"),
                                         MealPlan = x.Package?.Trim(),
                                         Nights = x.Night,
                                         PAX = x.PAX,
                                         PlaceId = x.PlaceId
                                     })
                                     .GroupBy(x => new
                                     {
                                         x.Hotel,
                                         x.RoomType,
                                         x.CheckIn,
                                         x.CheckOut,
                                         x.MealPlan,
                                         x.Nights,
                                         x.PAX,
                                         x.PlaceId
                                     })
                                     .Select(g => new NewItineraryReportAccommodationsModel
                                     {
                                         Hotel = g.Key.Hotel,
                                         RoomType = g.Key.RoomType,
                                         CheckIn = g.Key.CheckIn,
                                         CheckOut = g.Key.CheckOut,
                                         MealPlan = g.Key.MealPlan,
                                         Nights = g.Key.Nights,
                                         Count = g.Count(),
                                         PlaceId = g.Key.PlaceId
                                     })
                                     .ToList();

                model.Accommodations.AddRange(accommodations);
            }

            ViewBag.Mina = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.MINA)
                .FirstOrDefault();

            ViewBag.Arafat = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.ARAFAT)
                .FirstOrDefault();

            ViewBag.MUZDALIFAH = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.MUZDALIFAH)
                .FirstOrDefault();

            ViewBag.RamiDayOne = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.RamiDayOne)
                .FirstOrDefault();

            ViewBag.RamiDayTwo = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.RamiDayTwo)
                .FirstOrDefault();

            ViewBag.RamiDayThree = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.RamiDayThree)
                .FirstOrDefault();

            ViewBag.TermsnCondition = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.TermsnCondition)
                .FirstOrDefault();

            ViewBag.Content = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.PackageId == fh.PackageId)
                .Select(e => e.Content)
                .FirstOrDefault();


            //return Ok(model);
            return View(model);
        }

        public IActionResult ReceiptReport(int id)
        {
            FamilyHead fh = _dbContext.FamilyHeads
                .Include(e => e.Package).FirstOrDefault(e => e.FamilyHeadId == id);

            Currencies? baseCurrency = fh.Currencies;

            ViewBag.BaseCurrency = baseCurrency.ToString();
            ViewBag.HajiName = fh.FullName + $" ({fh.FcId})";
            ViewBag.PackageName = fh.Package.Name;

            float packageAmount = PackageManager.TotalPackageAmount(_dbContext, fh.PackageId, id);

            ViewBag.PackageAmount = packageAmount.ToString("N0");

            var invoices = _dbContext.NewInvoicesWithoutTrash
                        //.Include(e => e.Bank)
                        .Where(e => e.FamilyHeadId == id)
                        .ToList();

            var model = new List<object>();

            float totalReceivedAmount = 0f;

            foreach (var item in invoices)
            {
                float convertedClearedAmount = 0f;

                if (baseCurrency == Currencies.SAR)
                {
                    if (item.Currencies == Currencies.SAR)
                    {
                        convertedClearedAmount += item.AmountRecieved;
                    }
                    else if (item.Currencies == Currencies.PKR)
                    {
                        convertedClearedAmount += item.AmountRecieved / item.ROE;
                    }
                    else if (item.Currencies == Currencies.USD)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                    else if (item.Currencies == Currencies.AED)
                    {
                        convertedClearedAmount += item.AmountRecieved / item.ROE;
                    }
                    else if (item.Currencies == Currencies.GBP)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                    else if (item.Currencies == Currencies.EURO)
                    {
                        convertedClearedAmount += item.AmountRecieved / item.ROE;
                    }
                }
                else if (baseCurrency == Currencies.PKR)
                {
                    if (item.Currencies == Currencies.SAR)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                    else if (item.Currencies == Currencies.PKR)
                    {
                        convertedClearedAmount += item.AmountRecieved;
                    }
                    else if (item.Currencies == Currencies.USD)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                    else if (item.Currencies == Currencies.AED)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                    else if (item.Currencies == Currencies.GBP)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                    else if (item.Currencies == Currencies.EURO)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                }
                else if (baseCurrency == Currencies.USD)
                {
                    if (item.Currencies == Currencies.SAR)
                    {
                        convertedClearedAmount += item.AmountRecieved / item.ROE;
                    }
                    else if (item.Currencies == Currencies.PKR)
                    {
                        convertedClearedAmount += item.AmountRecieved / item.ROE;
                    }
                    else if (item.Currencies == Currencies.USD)
                    {
                        convertedClearedAmount += item.AmountRecieved;
                    }
                    else if (item.Currencies == Currencies.AED)
                    {
                        convertedClearedAmount += item.AmountRecieved / item.ROE;
                    }
                    else if (item.Currencies == Currencies.GBP)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                    else if (item.Currencies == Currencies.EURO)
                    {
                        convertedClearedAmount += item.AmountRecieved * item.ROE;
                    }
                }

                totalReceivedAmount += convertedClearedAmount;

                var _ = new
                {
                    Date = item.Date,
                    //BankName = item.Bank.Name,
                    Amount = item.AmountRecieved,
                    ROE = item.ROE,
                    Currency = item.Currencies.ToString(),
                    ConvertedAmount = convertedClearedAmount
                };

                model.Add(_);

            }

            ViewBag.ReceivedAmount = totalReceivedAmount.ToString("N0");
            ViewBag.RemainingAmount = (packageAmount - totalReceivedAmount).ToString("N0");

            //return Ok(invoices);
            return View(model);
        }

        public IActionResult NusukArrivalReport(int id)
        {
            var familyMembers = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.Countries)
                .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.FamilyHeadContacts)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .ToList();

            var flight = (from pf in _dbContext.POFlightDetails
                          join pt in _dbContext.POTickets on pf.POTicketId equals pt.POTicketId
                          join ptf in _dbContext.POTicketFlightDatas on pf.POTicketFlightDataId equals ptf.POTicketFlightDataId
                          where pt.FamilyHeadId == id
                          select new
                          {
                              pf,
                              pt,
                              ptf,
                              Airline = ptf.Airline,
                              ArrivalTime = ptf.ArrivalTime,
                              FlightNumber = ptf.FlightNumber,
                              ArrivalDate = pf.ArrivalDate,
                              FromCity = ptf.Arrival.Code,
                              ToCity = ptf.Departure.Code,
                              AirportName = ptf.Departure.AirportName,
                              FlightClass = pf.FlightClass
                          }).FirstOrDefault();

            if (flight == null)
            {
                return NotFound("Flight data not found.");
            }

            var familyHeads = familyMembers.Select(e => new NusukArrivalMembers
            {
                FirstName = e.FamilyHead.GivenName,
                LastName = e.FamilyHead.SurName,
                PassportNo = e.FamilyHead.PassportNumber,
                PhoneNo = e.FamilyHead.FamilyHeadContacts.FirstOrDefault(c => c.Preferred).Contact
            }).ToList();

            var PAX = familyMembers.Count();

            NusukArrivalData model = new()
            {
                AirlineName = flight.Airline?.Name ?? "N/A",
                FlightNo = flight.FlightNumber ?? "N/A",
                FlightTime = flight.ArrivalTime.ToString("HH:mm"),
                FlightDate = flight.ArrivalDate?.ToString("dd-MMM-yyyy"),
                AirportName = flight.AirportName ?? "N/A",
                ToCity = flight.ToCity ?? "N/A",
                FromCity = flight.FromCity ?? "N/A",
                CountryName = familyMembers.FirstOrDefault()?.FamilyHead?.Countries?.Name ?? "N/A",
                FamilyDataEntries = familyHeads,
                TotalNoOfPilgirms = PAX,
                FlightClass = flight.FlightClass.ToString()
            };

            return View(model);
        }

        public IActionResult QurbaniSlip(int id)
        {
            var result = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => new CardMemberModel
                {
                    HajiName = e.FullName,
                    HbNo = e.FcId,
                    PackageName = e.Package.Name
                }).FirstOrDefault();

            return View(result);
        }

        public IActionResult Certificate(int id)
        {
            ViewBag.Id = id;

            ViewBag.HajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            ViewBag.HbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            return View();
        }

        public IActionResult FeedBack(int id)
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.FamilyHeadEmails)
                .Include(e => e.FamilyHeadContacts)
                .Include(e => e.Package)
                .Include(e => e.CompanyVisas)
                .Include(e => e.FamilyHeadAddresses)
                .Where(e => e.FamilyHeadId == id);

            var result = query.Select(e => new FeedbackForm
            {
                HajjiName = e.FullName,
                MunazzamName = e.CompanyVisas.MunazimName,
                MunazzamMoraNo = e.CompanyVisas.MoraNo,
                MunazzamNo = e.CompanyVisas.SaudiMunazimNo,
                HajjDuration = e.Package.HajjDuration,
                MaktabNo = e.CompanyVisas.MaktabNumber,
                ZoneNo = e.Package.CategoryZone,
                PhoneNo = e.FamilyHeadContacts.FirstOrDefault(c => c.Preferred).Contact,
                HajjApplicationNo = e.HajjApplicationNumber,
                Email = e.FamilyHeadEmails.FirstOrDefault(em => em.Preferred).Email,
                Address = e.FamilyHeadAddresses.FirstOrDefault(a => a.Preferred).Address
            }).FirstOrDefault();

            return View(result);
        }

        //public IActionResult DataFormv1(int id)
        //{

        //    try
        //    {
        //        var membersOfFamily = _dbContext.FamilyMembers
        //        .Include(e => e.Head)
        //        .Include(e => e.FamilyHead)
        //        .ThenInclude(e => e.RelationWithMehram)
        //        .Include(e => e.FamilyHead)
        //        .ThenInclude(e => e.Fiqah)
        //        .Include(e => e.FamilyHead)
        //        .ThenInclude(e => e.Package)
        //        .ThenInclude(e => e.Company)
        //        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
        //        .ToList();

        //        DataFormModel superModel = new DataFormModel();

        //        for (int i = 0; i < membersOfFamily.Count; i++)
        //        {
        //            FamilyMembers currMember = membersOfFamily.ElementAt(i);

        //            var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
        //            var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

        //            string cId =
        //                    $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
        //                    $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
        //                    $".{currMember.FamilyHead.Package.Name}";

        //            FamilyHeadAddress headAddress = _dbContext
        //                                .FamilyHeadAddresses
        //                                .Include(e => e.Tehsil)
        //                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

        //            FamilyHeadContact preferredContact = _dbContext
        //                .FamilyHeadContacts
        //                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

        //            FamilyHeadContact otherContact = _dbContext
        //                .FamilyHeadContacts
        //                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

        //            FamilyHeadNominee headNominee = _dbContext
        //                .FamilyHeadNominees
        //                .Include(e => e.NomineeRelation)
        //                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

        //            var trainings = _dbContext
        //                .PackageTrainings
        //                .Include(e => e.Training)
        //                .Where(e => e.PackageId == currMember.FamilyHead.PackageId).ToList();

        //            Flight flight = _dbContext.Flights
        //                .FirstOrDefault(e => e.FlightId == currMember.FamilyHead.FlightId);

        //            var gifts = _dbContext
        //                .PackageGifts
        //                .Include(e => e.Gift)
        //                .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
        //                .ToList();

        //            var extraFacilities = _dbContext
        //                .FamilyHeadExtras
        //                .Include(e => e.ExtraFacilityDescription)
        //                .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                .ToList();

        //            string presentPostalAddress = $"{headAddress?.Address}, {headAddress?.City}, {headAddress?.ZipCode}, {headAddress?.Country}";
        //            string packageNameDuration = $"{currMember?.FamilyHead.Package.Name} " +
        //                $"({currMember?.FamilyHead.Package.Days})";

        //            var officialAddress = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberAddressTypeId == 2);
        //            var officialContact = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberContactTypeId == 2);
        //            var officialEmail = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberEmailTypeId == 2);

        //            string officialAddressString = officialAddress is not null ?
        //                                    $"{officialAddress?.Address}, {officialAddress?.City}, {officialAddress?.ZipCode}, {officialAddress?.Country}" : "";

        //            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred)?.Email;

        //            UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

        //            DataFormModelEntry model = new DataFormModelEntry
        //            {
        //                SignedDate = DateTimeManager.GetDate(userHajjAgreement?.SignedOn),
        //                CID = cId,
        //                ShowSignature = userHajjAgreement?.SignDataForm ?? false,
        //                SignatureBase64 = currMember?.FamilyHead.SignatureBase64,
        //                ApplicantType = currMember?.FamilyHead.ApplicantType.ToString(),
        //                FamilyHeadImage = currMember?.FamilyHead.MemberImage ?? "",
        //                HajjAppNumber = currMember?.FamilyHead.HajjApplicationNumber ?? "-",
        //                EnrollmentNumber = currMember?.FamilyHead.ENumber ?? "",
        //                MoraPackageNumber = currMember?.FamilyHead.Package.PackageNumber,
        //                PackageNameDuration = packageNameDuration,
        //                HGOName = currMember?.FamilyHead.Package.Company.CompanyName,
        //                SurName = currMember?.FamilyHead.SurName,
        //                GivenName = currMember?.FamilyHead.GivenName,
        //                FatherHusbandName = currMember?.FamilyHead.FatherHusbandName,
        //                PassportNumber = currMember?.FamilyHead.PassportNumber,
        //                PassportDateOfExpiry = currMember?.FamilyHead.PassportDateOfExpiry?.ToString("dd-MMM-yyyy"),
        //                DateOfBirth = currMember?.FamilyHead.DateOfBirth.ToString("dd-MMM-yyyy"),
        //                CNIC = currMember?.FamilyHead.CNIC,
        //                BloodGroup = currMember?.FamilyHead.BloodGroup,
        //                Gender = currMember?.FamilyHead.Gender,
        //                Fiqah = currMember?.FamilyHead.Fiqah.Name.ToString(),
        //                IsMarried = (bool)currMember?.FamilyHead.IsMarried ? "YES" : "NO",
        //                YearsPerformedHajj = currMember?.FamilyHead.YearsPerformedHajj,
        //                PresentPostalAddress = presentPostalAddress,
        //                TehsilCode = headAddress?.Tehsil?.Name,
        //                MobileNumber = preferredContact?.Contact,
        //                TelephoneNumber = otherContact?.Contact,

        //                Email = email,
        //                OfficialAddress = officialAddressString,
        //                OfficialContact = officialContact?.Contact,
        //                EmployerBusinessName = currMember.FamilyHead.EmployedAt,
        //                Designation = currMember.FamilyHead.Designation,
        //                OfficialEmail = officialEmail?.Email,

        //                PerformedHajjInLast5Years = (bool)currMember?.FamilyHead.PerformedHajjInLastFiveYears ? "YES" : "NO",
        //                WantToHajjEBadal = (bool)currMember?.FamilyHead.WantToPerformHajjEBadal ? "YES" : "NO",
        //                WantToPerformHajjAsGroupWorker = (bool)currMember?.FamilyHead.WantToPerformAsGroupWorker ? "YES" : "NO",
        //                NomineeName = headNominee?.Name,
        //                NomineeRelation = headNominee?.NomineeRelation?.Relation,
        //                NomineeCNIC = headNominee?.CNIC,
        //                NomineeContact = headNominee?.Contact,
        //                NomineeTelephone = "",

        //                StayDuration = currMember?.FamilyHead.Package.HajjDuration,
        //                SubAgaintAppNumber = currMember?.FamilyHead.SubstitutedAgainstAppNumber,

        //                MedinahArrival = currMember?.FamilyHead.Package.MedinahArrival,
        //                FlightType = currMember?.FamilyHead.Package.FlightType,
        //                FlightClass = currMember?.FamilyHead.FlightClass,
        //                TicketPeriod = currMember?.FamilyHead.Package.FlightTicketPeriod,
        //                POD = currMember?.FamilyHead.POD,
        //                // TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString(),

        //                PackageTicketPricePkr = currMember?.FamilyHead.Package.FlightTicketPricePkr,
        //                PackageTicketPriceSar = currMember?.FamilyHead.Package.FlightTicketPriceSar,
        //                PackageTicketPriceUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd,

        //                FlightTicketDifferencesPkr = currMember?.FamilyHead.FlightTicketDifferencesPkr,
        //                FlightTicketDifferencesSar = currMember?.FamilyHead.FlightTicketDifferencesSar,
        //                FlightTicketDifferencesUsd = currMember?.FamilyHead.FlightTicketDifferencesUsd,

        //                IntercityTransport = currMember?.FamilyHead.Package.TransportIntercity,
        //                AirportTransport = currMember?.FamilyHead.Package.AirportTransport,
        //                MashairTrasport = currMember?.FamilyHead.Package.TransportWithMashair,
        //                WithinCity = currMember?.FamilyHead.Package.WithInCity
        //            };

        //            model.Remarks = _dbContext.FamilyHeadRemarks.Where(e => e.FamilyHeadId == currMember.FamilyHeadId).Select(e => e.Remarks).ToList();

        //            float? additionalFacilitiesAmountPkr = 0;
        //            float? totalPkr = 0;

        //            float? additionalFacilitiesAmountSar = 0;
        //            float? totalSar = 0;

        //            float? additionalFacilitiesAmountUsd = 0;
        //            float? totalUsd = 0;

        //            model.TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString();

        //            if (currMember?.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
        //            {
        //                model.TicketIncluded = "Included With Differences";
        //                totalPkr = currMember?.FamilyHead.Package.FlightTicketPricePkr + currMember?.FamilyHead.FlightTicketDifferencesPkr;
        //                totalSar = currMember?.FamilyHead.Package.FlightTicketPriceSar + currMember?.FamilyHead.FlightTicketDifferencesSar;
        //                totalUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd + currMember?.FamilyHead.FlightTicketDifferencesUsd;
        //            }
        //            else if (currMember?.FamilyHead.FlightTicketType == FlightTicketType.Excluded)
        //            {
        //                model.PackageTicketPricePkr = 0;
        //                model.PackageTicketPriceSar = 0;
        //                model.PackageTicketPriceUsd = 0;
        //            }
        //            else
        //            {
        //                totalPkr = currMember?.FamilyHead.Package.FlightTicketPricePkr;
        //                totalSar = currMember?.FamilyHead.Package.FlightTicketPriceSar;
        //                totalUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd;
        //            }

        //            float totalExtraAdfPkr = 0, totalExtraAdfSar = 0, totalExtraAdfUsd = 0;
        //            for (int k = 0; k < extraFacilities.Count; k++)
        //            {
        //                FamilyHeadExtra currExtra = extraFacilities.ElementAt(k);

        //                totalExtraAdfPkr += currExtra.PricePkr ?? 0;
        //                totalExtraAdfSar += currExtra.PriceSar ?? 0;
        //                totalExtraAdfUsd += currExtra.PriceUsd ?? 0;

        //                model.ExtraFacilities.Add(
        //                    new DataFormExtraFacility(
        //                        currExtra.ExtraFacilityDescription?.Description,
        //                        currExtra.PricePkr,
        //                        currExtra.PriceSar,
        //                        currExtra.PriceUsd)
        //                    );
        //            }

        //            model.ExtraAdfDuesPkr = totalExtraAdfPkr.ToString("N0");
        //            model.ExtraAdfDuesSar = totalExtraAdfSar.ToString("N0");
        //            model.ExtraAdfDuesUsd = totalExtraAdfUsd.ToString("N0");

        //            for (int k = 0; k < gifts.Count; k++)
        //            {
        //                PackageGifts currGift = gifts.ElementAt(k);
        //                model.Gifts.Add(new DataFormGift(currGift.Gift.Giveaway));
        //            }

        //            if (currMember.FamilyHead.Gender == Gender.Male)
        //            {
        //                FamilyHead mehram = _dbContext.FamilyHeads.FirstOrDefault(e => e.MehramFamilyHeadId == currMember.FamilyHeadId && !e.Trashed);
        //                model.AreYouMehramOfALady = mehram is not null ? "YES" : "NO";
        //            }
        //            else
        //            {
        //                FamilyHead mehram = _dbContext.FamilyHeads
        //                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHead.MehramFamilyHeadId && !e.Trashed);
        //                model.AreYouMehramOfALady = "-";
        //                model.MehramAppNumber = mehram?.HajjApplicationNumber;
        //                model.MehramRelationship = currMember?.FamilyHead.RelationWithMehram?.Relation;
        //                model.MehramSignatureBase64 = mehram?.SignatureBase64;
        //                model.MehramName = mehram?.FullName;
        //            }

        //            for (int j = 0; j < trainings.Count; j++)
        //            {
        //                PackageTrainings _t = trainings.ElementAt(j);
        //                model.TrainingSessions.Add(new DataFormTrainingSession(
        //                    _t.Training.TrainingSession, _t.Training.ExpectedTrainingVenue,
        //                    _t.Training.ExpectedTrainingTime
        //                    ));
        //            }

        //            var individualFacilities =
        //                _dbContext.FamilyHeadAdditionalFacilities
        //                .Include(e => e.AdditionalFacilities)
        //                .ThenInclude(e => e.IndividiualFacilityDescription)
        //                .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                .ToList();

        //            if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) < 2)
        //            {
        //                totalPkr += currMember?.FamilyHead.Package.InfantPkr;
        //                totalSar += currMember?.FamilyHead.Package.InfantSar;
        //                totalUsd += currMember?.FamilyHead.Package.InfantUsd;

        //                additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.InfantPkr);

        //                additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.InfantSar);

        //                additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.InfantUsd);

        //                model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.InfantPkr)?.ToString("N0");
        //                model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.InfantSar)?.ToString("N0");
        //                model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.InfantUsd)?.ToString("N0");

        //                for (int k = 0; k < individualFacilities.Count; k++)
        //                {
        //                    var _if = individualFacilities.ElementAt(k);

        //                    model.IndividualFacilities.Add(
        //                        new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if.AdditionalFacilities
        //                        .IndividiualFacilityDescription?.Description,
        //                        _if.AdditionalFacilities.InfantPkr?.ToString("N0"),
        //                        _if.AdditionalFacilities.InfantSar?.ToString("N0"),
        //                        _if.AdditionalFacilities.InfantUsd?.ToString("N0")));
        //                }
        //            }
        //            else if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) < 12)
        //            {
        //                totalPkr += currMember?.FamilyHead.Package.ChildPkr;
        //                totalSar += currMember?.FamilyHead.Package.ChildSar;
        //                totalUsd += currMember?.FamilyHead.Package.ChildUsd;

        //                additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.ChildPkr);

        //                additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.ChildSar);

        //                additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.ChildUsd);

        //                model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.ChildPkr)?.ToString("N0");
        //                model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.ChildSar)?.ToString("N0");
        //                model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.ChildUsd)?.ToString("N0");


        //                for (int k = 0; k < individualFacilities.Count; k++)
        //                {
        //                    var _if = individualFacilities.ElementAt(k);

        //                    model.IndividualFacilities.Add(
        //                        new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if.AdditionalFacilities.IndividiualFacilityDescription?.Description,
        //                        _if.AdditionalFacilities.ChildPkr?.ToString("N0"),
        //                        _if.AdditionalFacilities.ChildSar?.ToString("N0"),
        //                        _if.AdditionalFacilities.ChildUsd?.ToString("N0")
        //                        ));
        //                }
        //            }
        //            else if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) >= 12)
        //            {
        //                totalPkr += currMember?.FamilyHead.Package.AdultPkr;
        //                totalSar += currMember?.FamilyHead.Package.AdultSar;
        //                totalUsd += currMember?.FamilyHead.Package.AdultUsd;

        //                additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.AdultPkr);

        //                additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.AdultSar);

        //                additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.AdultUsd);

        //                model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.AdultPkr)?.ToString("N0");
        //                model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.AdultSar)?.ToString("N0");
        //                model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.AdultUsd)?.ToString("N0");

        //                for (int k = 0; k < individualFacilities.Count; k++)
        //                {
        //                    var _if = individualFacilities.ElementAt(k);

        //                    model.IndividualFacilities.Add(
        //                        new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId,
        //                        _if?.AdditionalFacilities?.IndividiualFacilityDescription?.Description,
        //                        _if.AdditionalFacilities?.AdultPkr?.ToString("N0"),
        //                        _if.AdditionalFacilities?.AdultSar?.ToString("N0"),
        //                        _if.AdditionalFacilities?.AdultUsd?.ToString("N0")
        //                    ));
        //                }
        //            }

        //            model.IndividualAdditionalFacilitiesDuesPkr = additionalFacilitiesAmountPkr?.ToString("N0");
        //            model.IndividualAdditionalFacilitiesDuesSar = additionalFacilitiesAmountSar?.ToString("N0");
        //            model.IndividualAdditionalFacilitiesDuesUsd = additionalFacilitiesAmountUsd?.ToString("N0");

        //            model.DiscountPkr =
        //                $"{currMember?.FamilyHead.PkrDiscountAmount.ToString("N0")}";

        //            model.DiscountSar =
        //                $"{currMember?.FamilyHead.SarDiscountAmount.ToString("N0")}";

        //            model.DiscountUsd =
        //                $"{currMember?.FamilyHead.UsdDiscountAmount.ToString("N0")}";

        //            totalPkr += additionalFacilitiesAmountPkr;
        //            totalSar += additionalFacilitiesAmountSar;
        //            totalUsd += additionalFacilitiesAmountUsd;

        //            totalPkr += totalExtraAdfPkr;
        //            totalSar += totalExtraAdfSar;
        //            totalUsd += totalExtraAdfUsd;

        //            totalPkr -= currMember?.FamilyHead.PkrDiscountAmount;
        //            totalSar -= currMember?.FamilyHead.SarDiscountAmount;
        //            totalUsd -= currMember?.FamilyHead.UsdDiscountAmount;

        //            model.TotalPkr = totalPkr?.ToString("N0");
        //            model.TotalSar = totalSar?.ToString("N0");
        //            model.TotalUsd = totalUsd?.ToString("N0");

        //            PackageAccomodations aziziaAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 4);

        //            PackageAccomodations makkahAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);

        //            PackageAccomodations medinaAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 2);

        //            PackageAccomodations minaAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 3);

        //            if (aziziaAccomadation is not null)
        //            {
        //                var _indId = _dbContext.FamilyHeadAdditionalFacilities
        //                                                            .Include(e => e.AdditionalFacilities)
        //                                                            .ThenInclude(e => e.Room)
        //                                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
        //                                                            && e.AdditionalFacilities.PlaceId == 4);

        //                model.AziziaAccomodation = new DataFormAzizia(
        //                    aziziaAccomadation.Accomodation.AccomodationName,
        //                    aziziaAccomadation.AccomodationName,
        //                    aziziaAccomadation.AccomodationCategory.Category,
        //                    aziziaAccomadation.CheckIn,
        //                    aziziaAccomadation.CheckOut,
        //                    aziziaAccomadation.Days,
        //                    aziziaAccomadation.DistanceInMeters?.ToString(),
        //                    aziziaAccomadation.FoodPackage?.Package,
        //                    /*aziziaAccomadation.LoadPerRoom,
        //                    aziziaAccomadation.LoadPerWashroom,*/
        //                    aziziaAccomadation.Shuttle,
        //                    aziziaAccomadation.ReligiousLectures,
        //                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : ""

        //                );
        //            }

        //            if (makkahAccomadation is not null)
        //            {
        //                var _indId = _dbContext.FamilyHeadAdditionalFacilities
        //                                        .Include(e => e.AdditionalFacilities)
        //                                        .ThenInclude(e => e.Room)
        //                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
        //                                        && e.AdditionalFacilities.PlaceId == 1);

        //                model.MakkahAccomodation = new DataFormAccomodation(
        //                makkahAccomadation.GroupZiarat,
        //                makkahAccomadation.Days,
        //                makkahAccomadation.DistanceInMeters?.ToString(),
        //                makkahAccomadation.FoodPackage?.Package,
        //                /*makkahAccomadation.LoadPerRoom.ToString(),*/
        //                makkahAccomadation.Accomodation.AccomodationName,
        //                makkahAccomadation.AccomodationName,
        //                makkahAccomadation.CheckIn,
        //                /*makkahAccomadation.LoadPerWashroom,*/
        //                makkahAccomadation.Sharing,
        //                _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
        //                makkahAccomadation.AccomodationCategory.Category

        //            );
        //            }

        //            if (medinaAccomadation is not null)
        //            {
        //                var _indId = _dbContext.FamilyHeadAdditionalFacilities
        //                                        .Include(e => e.AdditionalFacilities)
        //                                        .ThenInclude(e => e.Room)
        //                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
        //                                        && e.AdditionalFacilities.PlaceId == 2);

        //                model.MedinaAccomodation = new DataFormAccomodation(
        //                medinaAccomadation.GroupZiarat,
        //                medinaAccomadation.Days,
        //                medinaAccomadation.DistanceInMeters?.ToString(),
        //                medinaAccomadation.FoodPackage?.Package,
        //                /*medinaAccomadation.LoadPerRoom.ToString(),*/
        //                medinaAccomadation.Accomodation.AccomodationName,
        //                medinaAccomadation.AccomodationName,
        //                medinaAccomadation.CheckIn,
        //                /*medinaAccomadation.LoadPerWashroom,*/
        //                medinaAccomadation.Sharing,
        //                _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
        //                medinaAccomadation.AccomodationCategory.Category
        //            );

        //                //  return Ok(new {
        //                //         model.MedinaAccomodation
        //                //     });
        //            }

        //            if (minaAccomadation is not null)
        //            {
        //                model.MinaAccomodation = new DataFormMina(
        //                minaAccomadation.Package.CategoryZone,
        //                minaAccomadation.FoodPackage?.Package,
        //                minaAccomadation.Bedding,
        //                minaAccomadation.Camp,
        //                minaAccomadation.Arafat,
        //                minaAccomadation.ReligiousLectures
        //               );
        //            }

        //            superModel.Entries.Add(model);

        //        }

        //        return View(superModel);
        //    }
        //    catch (Exception e)
        //    {
        //        return Ok(new
        //        {
        //            e.Message,
        //        });
        //    }

        //}

        //public IActionResult DataFormv2(int id)
        //{

        //    try
        //    {
        //        var membersOfFamily = _dbContext.FamilyMembers
        //        .Include(e => e.Head)
        //        .Include(e => e.FamilyHead)
        //        .ThenInclude(e => e.RelationWithMehram)
        //        .Include(e => e.FamilyHead)
        //        .ThenInclude(e => e.Fiqah)
        //        .Include(e => e.FamilyHead)
        //        .ThenInclude(e => e.Package)
        //        .ThenInclude(e => e.Company)
        //        .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
        //        .ToList();

        //        DataFormModel superModel = new DataFormModel();

        //        for (int i = 0; i < membersOfFamily.Count; i++)
        //        {
        //            FamilyMembers currMember = membersOfFamily.ElementAt(i);

        //            var fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);
        //            var famCount = _dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count();

        //            string cId =
        //                    $"{currMember.FamilyHead.Package.Company.CompanyCode}.{currMember.FamilyHead.IndId}" +
        //                    $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
        //                    $".{currMember.FamilyHead.Package.Name}";

        //            FamilyHeadAddress headAddress = _dbContext
        //                                .FamilyHeadAddresses
        //                                .Include(e => e.Tehsil)
        //                                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

        //            FamilyHeadContact preferredContact = _dbContext
        //                .FamilyHeadContacts
        //                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

        //            FamilyHeadContact otherContact = _dbContext
        //                .FamilyHeadContacts
        //                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && !e.Preferred);

        //            FamilyHeadNominee headNominee = _dbContext
        //                .FamilyHeadNominees
        //                .Include(e => e.NomineeRelation)
        //                .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred);

        //            var trainings = _dbContext
        //                .PackageTrainings
        //                .Include(e => e.Training)
        //                .Where(e => e.PackageId == currMember.FamilyHead.PackageId).ToList();

        //            Flight flight = _dbContext.Flights
        //                .FirstOrDefault(e => e.FlightId == currMember.FamilyHead.FlightId);

        //            var gifts = _dbContext
        //                .PackageGifts
        //                .Include(e => e.Gift)
        //                .Where(e => e.PackageId == currMember.FamilyHead.PackageId)
        //                .ToList();

        //            var extraFacilities = _dbContext
        //                .FamilyHeadExtras
        //                .Include(e => e.ExtraFacilityDescription)
        //                .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                .ToList();

        //            string presentPostalAddress = $"{headAddress?.Address}, {headAddress?.City}, {headAddress?.ZipCode}, {headAddress?.Country}";
        //            string packageNameDuration = $"{currMember?.FamilyHead.Package.Name} " +
        //                $"({currMember?.FamilyHead.Package.Days})";

        //            var officialAddress = _dbContext.FamilyHeadAddresses.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberAddressTypeId == 2);
        //            var officialContact = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberContactTypeId == 2);
        //            var officialEmail = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.MemberEmailTypeId == 2);

        //            string officialAddressString = officialAddress is not null ?
        //                                    $"{officialAddress?.Address}, {officialAddress?.City}, {officialAddress?.ZipCode}, {officialAddress?.Country}" : "";

        //            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId && e.Preferred)?.Email;

        //            UserHajjAgreement userHajjAgreement = _dbContext.UserHajjAgreements.FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId);

        //            DataFormModelEntry model = new DataFormModelEntry
        //            {
        //                SignedDate = DateTimeManager.GetDate(userHajjAgreement?.SignedOn),
        //                CID = cId,
        //                ShowSignature = userHajjAgreement?.SignDataForm ?? false,
        //                SignatureBase64 = currMember?.FamilyHead.SignatureBase64,
        //                ApplicantType = currMember?.FamilyHead.ApplicantType.ToString(),
        //                FamilyHeadImage = currMember?.FamilyHead.MemberImage ?? "",
        //                HajjAppNumber = currMember?.FamilyHead.HajjApplicationNumber ?? "-",
        //                EnrollmentNumber = currMember?.FamilyHead.ENumber ?? "",
        //                MoraPackageNumber = currMember?.FamilyHead.Package.PackageNumber,
        //                PackageNameDuration = packageNameDuration,
        //                HGOName = currMember?.FamilyHead.Package.Company.CompanyName,
        //                SurName = currMember?.FamilyHead.SurName,
        //                GivenName = currMember?.FamilyHead.GivenName,
        //                FatherHusbandName = currMember?.FamilyHead.FatherHusbandName,
        //                PassportNumber = currMember?.FamilyHead.PassportNumber,
        //                PassportDateOfExpiry = currMember?.FamilyHead.PassportDateOfExpiry?.ToString("dd-MMM-yyyy"),
        //                DateOfBirth = currMember?.FamilyHead.DateOfBirth.ToString("dd-MMM-yyyy"),
        //                CNIC = currMember?.FamilyHead.CNIC,
        //                BloodGroup = currMember?.FamilyHead.BloodGroup,
        //                Gender = currMember?.FamilyHead.Gender,
        //                Fiqah = currMember?.FamilyHead.Fiqah.Name.ToString(),
        //                IsMarried = (bool)currMember?.FamilyHead.IsMarried ? "YES" : "NO",
        //                YearsPerformedHajj = currMember?.FamilyHead.YearsPerformedHajj,
        //                PresentPostalAddress = presentPostalAddress,
        //                TehsilCode = headAddress?.Tehsil?.Name,
        //                MobileNumber = preferredContact?.Contact,
        //                TelephoneNumber = otherContact?.Contact,

        //                Email = email,
        //                OfficialAddress = officialAddressString,
        //                OfficialContact = officialContact?.Contact,
        //                EmployerBusinessName = currMember.FamilyHead.EmployedAt,
        //                Designation = currMember.FamilyHead.Designation,
        //                OfficialEmail = officialEmail?.Email,

        //                PerformedHajjInLast5Years = (bool)currMember?.FamilyHead.PerformedHajjInLastFiveYears ? "YES" : "NO",
        //                WantToHajjEBadal = (bool)currMember?.FamilyHead.WantToPerformHajjEBadal ? "YES" : "NO",
        //                WantToPerformHajjAsGroupWorker = (bool)currMember?.FamilyHead.WantToPerformAsGroupWorker ? "YES" : "NO",
        //                NomineeName = headNominee?.Name,
        //                NomineeRelation = headNominee?.NomineeRelation?.Relation,
        //                NomineeCNIC = headNominee?.CNIC,
        //                NomineeContact = headNominee?.Contact,
        //                NomineeTelephone = "",

        //                StayDuration = currMember?.FamilyHead.Package.HajjDuration,
        //                SubAgaintAppNumber = currMember?.FamilyHead.SubstitutedAgainstAppNumber,

        //                MedinahArrival = currMember?.FamilyHead.Package.MedinahArrival,
        //                FlightType = currMember?.FamilyHead.Package.FlightType,
        //                FlightClass = currMember?.FamilyHead.FlightClass,
        //                TicketPeriod = currMember?.FamilyHead.Package.FlightTicketPeriod,
        //                POD = currMember?.FamilyHead.POD,
        //                // TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString(),

        //                PackageTicketPricePkr = currMember?.FamilyHead.Package.FlightTicketPricePkr,
        //                PackageTicketPriceSar = currMember?.FamilyHead.Package.FlightTicketPriceSar,
        //                PackageTicketPriceUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd,

        //                FlightTicketDifferencesPkr = currMember?.FamilyHead.FlightTicketDifferencesPkr,
        //                FlightTicketDifferencesSar = currMember?.FamilyHead.FlightTicketDifferencesSar,
        //                FlightTicketDifferencesUsd = currMember?.FamilyHead.FlightTicketDifferencesUsd,

        //                IntercityTransport = currMember?.FamilyHead.Package.TransportIntercity,
        //                AirportTransport = currMember?.FamilyHead.Package.AirportTransport,
        //                MashairTrasport = currMember?.FamilyHead.Package.TransportWithMashair,
        //                WithinCity = currMember?.FamilyHead.Package.WithInCity
        //            };

        //            model.Remarks = _dbContext.FamilyHeadRemarks.Where(e => e.FamilyHeadId == currMember.FamilyHeadId).Select(e => e.Remarks).ToList();

        //            float? additionalFacilitiesAmountPkr = 0;
        //            float? totalPkr = 0;

        //            float? additionalFacilitiesAmountSar = 0;
        //            float? totalSar = 0;

        //            float? additionalFacilitiesAmountUsd = 0;
        //            float? totalUsd = 0;

        //            model.TicketIncluded = currMember?.FamilyHead.FlightTicketType.ToString();

        //            if (currMember?.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
        //            {
        //                model.TicketIncluded = "Included With Differences";
        //                totalPkr = currMember?.FamilyHead.Package.FlightTicketPricePkr + currMember?.FamilyHead.FlightTicketDifferencesPkr;
        //                totalSar = currMember?.FamilyHead.Package.FlightTicketPriceSar + currMember?.FamilyHead.FlightTicketDifferencesSar;
        //                totalUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd + currMember?.FamilyHead.FlightTicketDifferencesUsd;
        //            }
        //            else if (currMember?.FamilyHead.FlightTicketType == FlightTicketType.Excluded)
        //            {
        //                model.PackageTicketPricePkr = 0;
        //                model.PackageTicketPriceSar = 0;
        //                model.PackageTicketPriceUsd = 0;
        //            }
        //            else
        //            {
        //                totalPkr = currMember?.FamilyHead.Package.FlightTicketPricePkr;
        //                totalSar = currMember?.FamilyHead.Package.FlightTicketPriceSar;
        //                totalUsd = currMember?.FamilyHead.Package.FlightTicketPriceUsd;
        //            }

        //            float totalExtraAdfPkr = 0, totalExtraAdfSar = 0, totalExtraAdfUsd = 0;
        //            for (int k = 0; k < extraFacilities.Count; k++)
        //            {
        //                FamilyHeadExtra currExtra = extraFacilities.ElementAt(k);

        //                totalExtraAdfPkr += currExtra.PricePkr ?? 0;
        //                totalExtraAdfSar += currExtra.PriceSar ?? 0;
        //                totalExtraAdfUsd += currExtra.PriceUsd ?? 0;

        //                model.ExtraFacilities.Add(
        //                    new DataFormExtraFacility(
        //                        currExtra.ExtraFacilityDescription?.Description,
        //                        currExtra.PricePkr,
        //                        currExtra.PriceSar,
        //                        currExtra.PriceUsd)
        //                    );
        //            }

        //            model.ExtraAdfDuesPkr = totalExtraAdfPkr.ToString("N0");
        //            model.ExtraAdfDuesSar = totalExtraAdfSar.ToString("N0");
        //            model.ExtraAdfDuesUsd = totalExtraAdfUsd.ToString("N0");

        //            for (int k = 0; k < gifts.Count; k++)
        //            {
        //                PackageGifts currGift = gifts.ElementAt(k);
        //                model.Gifts.Add(new DataFormGift(currGift.Gift.Giveaway));
        //            }

        //            if (currMember.FamilyHead.Gender == Gender.Male)
        //            {
        //                FamilyHead mehram = _dbContext.FamilyHeads.FirstOrDefault(e => e.MehramFamilyHeadId == currMember.FamilyHeadId && !e.Trashed);
        //                model.AreYouMehramOfALady = mehram is not null ? "YES" : "NO";
        //            }
        //            else
        //            {
        //                FamilyHead mehram = _dbContext.FamilyHeads
        //                                    .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHead.MehramFamilyHeadId && !e.Trashed);
        //                model.AreYouMehramOfALady = "-";
        //                model.MehramAppNumber = mehram?.HajjApplicationNumber;
        //                model.MehramRelationship = currMember?.FamilyHead.RelationWithMehram?.Relation;
        //                model.MehramSignatureBase64 = mehram?.SignatureBase64;
        //                model.MehramName = mehram?.FullName;
        //            }

        //            for (int j = 0; j < trainings.Count; j++)
        //            {
        //                PackageTrainings _t = trainings.ElementAt(j);
        //                model.TrainingSessions.Add(new DataFormTrainingSession(
        //                    _t.Training.TrainingSession, _t.Training.ExpectedTrainingVenue,
        //                    _t.Training.ExpectedTrainingTime
        //                    ));
        //            }

        //            var individualFacilities =
        //                _dbContext.FamilyHeadAdditionalFacilities
        //                .Include(e => e.AdditionalFacilities)
        //                .ThenInclude(e => e.IndividiualFacilityDescription)
        //                .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                .ToList();

        //            if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) < 2)
        //            {

        //                totalPkr += currMember?.FamilyHead.Package.InfantPkr;
        //                totalSar += currMember?.FamilyHead.Package.InfantSar;
        //                totalUsd += currMember?.FamilyHead.Package.InfantUsd;

        //                additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.InfantPkr);

        //                additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.InfantSar);

        //                additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.InfantUsd);

        //                model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.InfantPkr + additionalFacilitiesAmountPkr)?.ToString("N0");
        //                model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.InfantSar + additionalFacilitiesAmountSar)?.ToString("N0");
        //                model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.InfantUsd + additionalFacilitiesAmountUsd)?.ToString("N0");

        //                for (int k = 0; k < individualFacilities.Count; k++)
        //                {
        //                    var _if = individualFacilities.ElementAt(k);

        //                    model.IndividualFacilities.Add(
        //                        new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if.AdditionalFacilities
        //                        .IndividiualFacilityDescription?.Description,
        //                        _if.AdditionalFacilities.InfantPkr?.ToString("N0"),
        //                        _if.AdditionalFacilities.InfantSar?.ToString("N0"),
        //                        _if.AdditionalFacilities.InfantUsd?.ToString("N0")));
        //                }
        //            }
        //            else if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) < 12)
        //            {
        //                totalPkr += currMember?.FamilyHead.Package.ChildPkr;
        //                totalSar += currMember?.FamilyHead.Package.ChildSar;
        //                totalUsd += currMember?.FamilyHead.Package.ChildUsd;

        //                additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.ChildPkr);

        //                additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.ChildSar);

        //                additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.ChildUsd);

        //                model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.ChildPkr + additionalFacilitiesAmountPkr)?.ToString("N0");
        //                model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.ChildSar + additionalFacilitiesAmountSar)?.ToString("N0");
        //                model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.ChildUsd + additionalFacilitiesAmountUsd)?.ToString("N0");


        //                for (int k = 0; k < individualFacilities.Count; k++)
        //                {
        //                    var _if = individualFacilities.ElementAt(k);

        //                    model.IndividualFacilities.Add(
        //                        new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if.AdditionalFacilities.IndividiualFacilityDescription?.Description,
        //                        _if.AdditionalFacilities.ChildPkr?.ToString("N0"),
        //                        _if.AdditionalFacilities.ChildSar?.ToString("N0"),
        //                        _if.AdditionalFacilities.ChildUsd?.ToString("N0")
        //                        ));
        //                }
        //            }
        //            else if (DateTimeManager.CalculateAge(currMember.FamilyHead.DateOfBirth) >= 12)
        //            {
        //                totalPkr += currMember?.FamilyHead.Package.AdultPkr;
        //                totalSar += currMember?.FamilyHead.Package.AdultSar;
        //                totalUsd += currMember?.FamilyHead.Package.AdultUsd;

        //                additionalFacilitiesAmountPkr = _dbContext.FamilyHeadAdditionalFacilities
        //                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                    .Sum(e => e.AdditionalFacilities.AdultPkr);

        //                additionalFacilitiesAmountSar = _dbContext.FamilyHeadAdditionalFacilities
        //                   .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                   .Sum(e => e.AdditionalFacilities.AdultSar);

        //                additionalFacilitiesAmountUsd = _dbContext.FamilyHeadAdditionalFacilities
        //                   .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
        //                   .Sum(e => e.AdditionalFacilities.AdultUsd);

        //                model.HajjPackageDuesPkr = (currMember?.FamilyHead.Package.AdultPkr + additionalFacilitiesAmountPkr)?.ToString("N0");
        //                model.HajjPackageDuesSar = (currMember?.FamilyHead.Package.AdultSar + additionalFacilitiesAmountSar)?.ToString("N0");
        //                model.HajjPackageDuesUsd = (currMember?.FamilyHead.Package.AdultUsd + additionalFacilitiesAmountUsd)?.ToString("N0");



        //                for (int k = 0; k < individualFacilities.Count; k++)
        //                {
        //                    var _if = individualFacilities.ElementAt(k);

        //                    model.IndividualFacilities.Add(
        //                        new DataFormIndividualFacility(_if.AdditionalFacilities.IndividiualFacilityDescription.IndividualFacilityDescriptionId, _if?.AdditionalFacilities?.IndividiualFacilityDescription?.Description,
        //                        _if.AdditionalFacilities?.AdultPkr?.ToString("N0"),
        //                        _if.AdditionalFacilities?.AdultSar?.ToString("N0"),
        //                        _if.AdditionalFacilities?.AdultUsd?.ToString("N0")
        //                        ));
        //                }
        //            }

        //            model.IndividualAdditionalFacilitiesDuesPkr = additionalFacilitiesAmountPkr?.ToString("N0");
        //            model.IndividualAdditionalFacilitiesDuesSar = additionalFacilitiesAmountSar?.ToString("N0");
        //            model.IndividualAdditionalFacilitiesDuesUsd = additionalFacilitiesAmountUsd?.ToString("N0");

        //            model.DiscountPkr =
        //                $"{currMember?.FamilyHead.PkrDiscountAmount.ToString("N0")}";

        //            model.DiscountSar =
        //                $"{currMember?.FamilyHead.SarDiscountAmount.ToString("N0")}";

        //            model.DiscountUsd =
        //                $"{currMember?.FamilyHead.UsdDiscountAmount.ToString("N0")}";

        //            totalPkr += additionalFacilitiesAmountPkr;
        //            totalSar += additionalFacilitiesAmountSar;
        //            totalUsd += additionalFacilitiesAmountUsd;

        //            totalPkr += totalExtraAdfPkr;
        //            totalSar += totalExtraAdfSar;
        //            totalUsd += totalExtraAdfUsd;

        //            totalPkr -= currMember?.FamilyHead.PkrDiscountAmount;
        //            totalSar -= currMember?.FamilyHead.SarDiscountAmount;
        //            totalUsd -= currMember?.FamilyHead.UsdDiscountAmount;

        //            model.TotalPkr = totalPkr?.ToString("N0");
        //            model.TotalSar = totalSar?.ToString("N0");
        //            model.TotalUsd = totalUsd?.ToString("N0");

        //            PackageAccomodations aziziaAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 4);

        //            PackageAccomodations makkahAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 1);

        //            PackageAccomodations medinaAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 2);

        //            PackageAccomodations minaAccomadation = _dbContext
        //                .PackageAccomodtions
        //                .Include(e => e.Accomodation)
        //                .Include(e => e.FoodPackage)
        //                .Include(e => e.AccomodationCategory)
        //                .FirstOrDefault(e => e.PackageId == currMember.FamilyHead.PackageId && e.PlaceId == 3);

        //            if (aziziaAccomadation is not null)
        //            {
        //                var _indId = _dbContext.FamilyHeadAdditionalFacilities
        //                                                            .Include(e => e.AdditionalFacilities)
        //                                                            .ThenInclude(e => e.Room)
        //                                                            .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
        //                                                            && e.AdditionalFacilities.PlaceId == 4);

        //                model.AziziaAccomodation = new DataFormAzizia(
        //                    aziziaAccomadation.Accomodation.AccomodationName,
        //                    aziziaAccomadation.AccomodationName,
        //                    aziziaAccomadation.AccomodationCategory.Category,
        //                    aziziaAccomadation.CheckIn,
        //                    aziziaAccomadation.CheckOut,
        //                    aziziaAccomadation.Days,
        //                    aziziaAccomadation.DistanceInMeters?.ToString(),
        //                    aziziaAccomadation.FoodPackage?.Package,
        //                    /*_indId is null ? aziziaAccomadation.LoadPerRoom : _indId.AdditionalFacilities.Room.PAX,
        //                    _indId is null ? aziziaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,*/
        //                    aziziaAccomadation.Shuttle,
        //                    aziziaAccomadation.ReligiousLectures,
        //                    _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : ""

        //                );
        //            }

        //            if (makkahAccomadation is not null)
        //            {
        //                var _indId = _dbContext.FamilyHeadAdditionalFacilities
        //                                        .Include(e => e.AdditionalFacilities)
        //                                        .ThenInclude(e => e.Room)
        //                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
        //                                        && e.AdditionalFacilities.PlaceId == 1);

        //                model.MakkahAccomodation = new DataFormAccomodation(
        //                makkahAccomadation.GroupZiarat,
        //                makkahAccomadation.Days,
        //                makkahAccomadation.DistanceInMeters?.ToString(),
        //                makkahAccomadation.FoodPackage?.Package,
        //                /* _indId is null ? makkahAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),*/
        //                makkahAccomadation.Accomodation.AccomodationName,
        //                makkahAccomadation.AccomodationName,
        //                makkahAccomadation.CheckIn,
        //                //_indId is null ? makkahAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
        //                makkahAccomadation.Sharing,
        //                _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
        //                makkahAccomadation.AccomodationCategory.Category

        //            );
        //            }

        //            if (medinaAccomadation is not null)
        //            {
        //                var _indId = _dbContext.FamilyHeadAdditionalFacilities
        //                                        .Include(e => e.AdditionalFacilities)
        //                                        .ThenInclude(e => e.Room)
        //                                        .FirstOrDefault(e => e.FamilyHeadId == currMember.FamilyHeadId
        //                                        && e.AdditionalFacilities.PlaceId == 2);

        //                model.MedinaAccomodation = new DataFormAccomodation(
        //                medinaAccomadation.GroupZiarat,
        //                medinaAccomadation.Days,
        //                medinaAccomadation.DistanceInMeters?.ToString(),
        //                medinaAccomadation.FoodPackage?.Package,
        //                //_indId is null ? medinaAccomadation.LoadPerRoom.ToString() : _indId.AdditionalFacilities.Room.PAX.ToString(),
        //                medinaAccomadation.Accomodation.AccomodationName,
        //                medinaAccomadation.AccomodationName,
        //                medinaAccomadation.CheckIn,
        //                //_indId is null ? medinaAccomadation.LoadPerWashroom : _indId.AdditionalFacilities.Room.PAX,
        //                medinaAccomadation.Sharing,
        //                _indId is not null ? $"*{_indId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "",
        //                medinaAccomadation.AccomodationCategory.Category
        //            );

        //                //  return Ok(new {
        //                //         model.MedinaAccomodation
        //                //     });
        //            }

        //            if (minaAccomadation is not null)
        //            {
        //                model.MinaAccomodation = new DataFormMina(
        //                minaAccomadation.Package.CategoryZone,
        //                minaAccomadation.FoodPackage?.Package,
        //                minaAccomadation.Bedding,
        //                minaAccomadation.Camp,
        //                minaAccomadation.Arafat,
        //                minaAccomadation.ReligiousLectures
        //               );
        //            }

        //            superModel.Entries.Add(model);

        //        }

        //        return View(superModel);
        //    }
        //    catch (Exception e)
        //    {
        //        return Ok(new
        //        {
        //            e.Message,
        //        });
        //    }

        //}
    }
}

namespace TMG.Models.FamilyHeadReportModels
{

    public record FeedbackForm
    {
        public string HajjiName { get; set; }
        public string MunazzamName { get; set; }
        public string MunazzamMoraNo { get; set; }
        public string MunazzamNo { get; set; }
        public string HajjDuration { get; set; }
        public string MaktabNo { get; set; }
        public string ZoneNo { get; set; }
        public string PhoneNo { get; set; }
        public string HajjApplicationNo { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

    public record NusukArrivalData
    {
        public string CountryName { get; set; }
        public string MunazzimName { get; set; }
        public string MunazzimId { get; set; }
        public int TotalNoOfPilgirms { get; set; }
        public string FlightNo { get; set; }
        public string AirlineName { get; set; }
        public string FlightDate { get; set; }
        public string FlightTime { get; set; }
        public string ToCity { get; set; }
        public string AirportName { get; set; }
        public string ArrivalTerminal { get; set; }
        public string FromCity { get; set; }
        public string FlightClass { get; set; }

        public List<NusukArrivalMembers> FamilyDataEntries { get; set; }
    }

    public record NusukArrivalMembers
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PassportNo { get; set; }
        public string PhoneNo { get; set; }
    }

    public class VisaCardsViewModel
    {
        public string VisaImage { get; set; }
        public string FileExtension { get; set; }
    }

    public class TicketCardsViewModel
    {
        public string TicketImage { get; set; }
        public string TicketFileExtension { get; set; }
    }

    public record CardModel
    {
        public string Munazzim { get; set; }
        public string MunazimNo { get; set; }
        public List<VisaCardsViewModel> VisaCards { get; set; } = new();
        public List<TicketCardsViewModel> TicketCards { get; set; } = new();

        public string Company { get; set; }
        public string CompanyImg { get; set; }
        public string CompanyLicenseNo { get; set; }
        public string Package { get; set; }
        public string Maktab { get; set; }
        public string MakkahAccomodation { get; set; }
        public string MakkahLocation { get; set; }
        public string MedinahAccomodation { get; set; }
        public string MedinahLocation { get; set; }
        public string AziziaAccomodation { get; set; }
        public string AziziaLocation { get; set; }
        //public string HbNos { get; set; }
        public List<CardMemberModel> Members { get; set; } = new();
        public List<LugguageCardMemberModel> LugguageMembers { get; set; } = new();
        public List<GovermentCardMemberModel> GovermentMembers { get; set; } = new();


        //NewItinerary

        public List<NewItineraryReportMemberModel> MembersForItinerary { get; set; } = new();
        public List<NewItineraryReportFlightModel> Flights { get; set; } = new();
        public List<NewItineraryReportFlightWOPCKGModel> WOPKGFlights { get; set; } = new();
        public List<NewItineraryReportTrainModel> Trains { get; set; } = new();
        public List<NewItineraryReportTrainWOPKGModel> WOPKGTrains { get; set; } = new();
        public List<NewItineraryReportRouteModel> Routes { get; set; } = new();
        public List<NewItineraryReportAccommodationsModel> Accommodations { get; set; } = new();
        public NewItineraryReportAccommodationsModel MakkahAccommodation { get; set; } = new();
        public NewItineraryReportAccommodationsModel MedinaAccommodation { get; set; } = new();
        public NewItineraryReportAccommodationsModel AziziaAccommodation { get; set; } = new();
        public NewItineraryReportAccommodationsModel MinaAccommodation { get; set; } = new();

        //ForMiniStatement

        public string PrintedBy { get; set; }
        public string TotalPackage { get; set; }
        public string RemainingAmount { get; set; }
        public string RefundAmount { get; set; }
        public string ReceivedAmount { get; set; }
        public Currencies Currencies { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string GuestName { get; set; }
        public string HBNo { get; set; }
        public string PackageDetails { get; set; }
        public string Contact { get; set; }
        public string CNIC { get; set; }
        public string Passport { get; set; }
        public string Address { get; set; }
        public string Date { get; set; }
        public string SPO { get; set; }
        public string Purpose { get; set; }
        public string Category { get; set; }
        public string MegaReference { get; set; }
        public string PAX { get; set; }
        public string BaseCurrency { get; set; }
        public List<string> MembersForMiniStatement { get; set; } = new();
        public List<NewInvoice> Receipts { get; set; } = new();
        public List<NewPaymentVoucher> Payments { get; set; } = new();
        public List<JV> JV { get; set; } = new();

        //ForHajjAggrement

        public string BaseCurrencies { get; set; }
        public string MunazzimName { get; set; }
        public float BasePackagePrice { get; set; } = 0f;
        public float FlightPrice { get; set; } = 0f;
        public float TrainPrice { get; set; } = 0f;
        public float RoomPrice { get; set; } = 0f;
        public float AddonsPrice { get; set; } = 0f;
        public float AddonsDiscount { get; set; } = 0f;
        public float TotalDiscount { get; set; } = 0f;
        public float TotalPackagePrice { get; set; } = 0f;


        public List<DataFormModelEntry> Entries { get; set; } = new List<DataFormModelEntry>();
    }

    public record CardMemberModel
    {
        public string HajiName { get; set; }
        public string HajiImg { get; set; }
        public string HajiSurName { get; set; }
        public string FatherName { get; set; }
        public string CID { get; set; }
        public string HbNo { get; set; }
        public string Passport { get; set; }
        public string Gender { get; set; }
        public string KSANo { get; set; }
        public string HajjAppNo { get; set; }
        public string VisaNo { get; set; }
        public string NusukId { get; set; }
        public string DOB { get; set; }
        public string minaAccomodation { get; set; }
        public string MakkahAccomodation { get; set; }
        public string MakkahLocation { get; set; }
        public string MedinahAccomodation { get; set; }
        public string MedinahLocation { get; set; }
        public string AziziaAccomodation { get; set; }
        public string AziziaLocation { get; set; }
        public string PackageName { get; set; }
        public string CompanyImg { get; set; }
        public string Maktab { get; set; }
        public string MaktabNumber { get; set; }
        public string CompanyName { get; set; }
        public string ClusterName { get; set; }
        public string OfficeAddress { get; set; }

        public string CompanyVisaName { get; set; }
        public string ArrFlightNo { get; set; }
        public DateTime? ArrDate { get; set; }
        public string DepFlightNo { get; set; }
        public DateTime? DepDate { get; set; }
        public string PhoneNo { get; set; }
        public string ZoneNo { get; set; }
        public string Munazzim { get; set; }
        public string MunazzimNo { get; set; }
        public string MoraNo { get; set; }
    }

    public record LugguageCardMemberModel
    {
        public string HajiName { get; set; }
        public string HajiImg { get; set; }
        public string HajiSurName { get; set; }
        public string FatherName { get; set; }
        public string CID { get; set; }
        public string HbNo { get; set; }
        public string Passport { get; set; }
        public string Gender { get; set; }
        public string KSANo { get; set; }
        public string HajjAppNo { get; set; }
        public string VisaNo { get; set; }
        public string NusukId { get; set; }
        public string DOB { get; set; }
        public string minaAccomodation { get; set; }
        public string MakkahAccomodation { get; set; }
        public string MakkahLocation { get; set; }
        public string MedinahAccomodation { get; set; }
        public string MedinahLocation { get; set; }
        public string AziziaAccomodation { get; set; }
        public string AziziaLocation { get; set; }
        public string PackageName { get; set; }
        public string CompanyImg { get; set; }
        public string Maktab { get; set; }
        public string MaktabNumber { get; set; }
        public string CompanyName { get; set; }
        public string ClusterName { get; set; }
        public string OfficeAddress { get; set; }




    }

    public record GovermentCardMemberModel
    {
        public string HajiName { get; set; }
        public string HajiImg { get; set; }
        public string HajiSurName { get; set; }
        public string FatherName { get; set; }
        public string CID { get; set; }
        public string HbNo { get; set; }
        public string Passport { get; set; }
        public string Gender { get; set; }
        public string KSANo { get; set; }
        public string HajjAppNo { get; set; }
        public string VisaNo { get; set; }
        public string NusukId { get; set; }
        public string DOB { get; set; }
        public string minaAccomodation { get; set; }
        public string MakkahAccomodation { get; set; }
        public string MakkahLocation { get; set; }
        public string MedinahAccomodation { get; set; }
        public string MedinahLocation { get; set; }
        public string AziziaAccomodation { get; set; }
        public string AziziaLocation { get; set; }
        public string PackageName { get; set; }
        public string CompanyImg { get; set; }
        public string Maktab { get; set; }
        public string MaktabNumber { get; set; }
        public string CompanyName { get; set; }
        public string ClusterName { get; set; }
        public string OfficeAddress { get; set; }




    }

    public record ItineraryRouteVehiclesSaveModel
    {
        public int FamilyHeadId { get; set; }
        public List<int> VehicleIds { get; set; }
        public List<int> VehicleTypes { get; set; }
        public List<int> Quantities { get; set; }
        public List<int> TravelRouteIds { get; set; }
        public List<int> RouteIds { get; set; }

    }

    public record ItineraryRouteVehiclesModel
    {
        public int FamilyHeadId { get; set; }
        public List<ItineraryRouteVehiclesRowModel> Rows { get; set; } = new();
    }
    public record ItineraryRouteVehiclesRowModel
    {
        public string Route { get; set; }
        public int TravelRouteId { get; set; }
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public VehicleType VehicleType { get; set; }
        public int Quantity { get; set; } = 0;
    }


    public record ItineraryReportModel
    {
        public int FamilyHeadId { get; set; }
        public string AirlineName { get; set; }
        public string AirlineLogo { get; set; }
        public List<ItineraryReportMemberModel> Members { get; set; } = new();
        public List<ItineraryReportSectorModel> FlightSectors { get; set; } = new();
        public List<ItineraryReportAccommodationModel> Accommodations { get; set; } = new();
        public List<ItineraryReportRouteModel> Routes { get; set; } = new();

        public List<ADFDetails> EIADFDetails { get; set; } = new();
        public List<ADFDetails> IADFDetails { get; set; } = new();
        public List<MemberRemarks> Remarks { get; set; } = new();
    }

    public record ItineraryReportMemberModel
    {
        public string CID { get; set; }
        public string Name { get; set; }
        public string Passport { get; set; }
        public string TicketNumber { get; set; }
        public string PnrNumber { get; set; }
        public string Qurbani { get; set; }

    }

    public record ItineraryReportSectorModel
    {
        public string FlightNumber { get; set; }
        public string Sector { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Luggage { get; set; }
        public string Class { get; set; }
    }

    public record ItineraryReportAccommodationModel
    {
        public string HotelName { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string Nights { get; set; }
        public string RoomType { get; set; }
        public string Quantity { get; set; }
        public string MealPlan { get; set; }
        public string View { get; set; }
        public string ConfimationNumber { get; set; }
    }

    public record ItineraryReportRouteModel
    {
        public string Route { get; set; }
        public string Vehicle { get; set; }
        public string Type { get; set; }
        public string Quantity { get; set; }
        public string Date { get; set; }
        public DateTime Datetime { get; set; }
    }


    public record CongratulationLetterReportModel
    {
        public CongratulationLetterPackages LetterPackages { get; set; }
        public CongratulationLetter Letter { get; set; }
        public Package package { get; set; }
        public Company packageCompany { get; set; }
        public string? HeadFullName { get; set; }
        public string? CID { get; set; }
        public string? GUESTNAME { get; set; }
        public string? CONTACT { get; set; }
        public string? CNIC { get; set; }
        public string? PASSPORT { get; set; }
        public string? ADDRESS { get; set; }
        public string? PAX { get; set; }
        public string? SPO { get; set; }
        public string? CATEGORY { get; set; }
        public List<string>? membersID { get; set; }

    }

    public record EHajjReportModelList
    {
        public List<EHajjReportModel> Entries { get; set; } = new List<EHajjReportModel>();
    }
    public record EHajjReportModel
    {
        public bool Verified { get; set; }
        public int FamilyHeadId { get; set; }
        public string HeadCID { get; set; }
        public string Package { get; set; }
        public string Group { get; set; }
        public string Category { get; set; }
        public string Contact { get; set; }
        public string CNIC { get; set; }
        public string SPO { get; set; }
        public string Mega { get; set; }
        public int PAX { get; set; }
        public List<string> Members { get; set; } = new();
        public string PassportImagePath { get; set; }
        public string EhajjPath { get; set; }
        public string MedinaCheckIn { get; set; }
        public string MedinaCheckOut { get; set; }
        public string RawdahPermitDate { get; set; }
        public string MedinaContractAndCapacity { get; set; }
        public string MedinaHotel { get; set; }
        public string MakkahContractAndCapacity { get; set; }
        public string MakkahHotel { get; set; }
        public string AziziaContractAndCapacity { get; set; }
        public string AziziaHotel { get; set; }
        public string Ehajj { get; set; }

    }

    public record AddressReportModel
    {
        public string HeadCID { get; set; }
        public string HeadCNIC { get; set; }
        public string HeadPassport { get; set; }
        public string Package { get; set; }
        public string HeadCategory { get; set; }
        public string HeadName { get; set; }
        public string HeadAddress { get; set; }
        public string HeadEmail { get; set; }
        public string HeadContact { get; set; }
        public string MedinahAccommodation { get; set; }
        public string MakkahAccommodation { get; set; }
        public string AziziahAccommodation { get; set; }
        public string MinaAccommodation { get; set; }
        public string HajjAppSerial { get; set; }
        public string HajjAppNo { get; set; }
        public string MegaReference { get; set; }
        public List<string> Members { get; set; } = new();
        public string TotalAmount { get; set; }
        public string Discount { get; set; }
        public string NetReceivable { get; set; }
        public string AmountReceived { get; set; }
        public string BalanceReceivable { get; set; }
        public string Group { get; set; }
        public int PAX { get; set; }

    }
}

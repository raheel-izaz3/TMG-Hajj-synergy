
using TMG.Data;
using TMG.Models.FamilyHeadReportModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers;

public class BookletController : Controller
{
    public readonly IWebHostEnvironment _host;
    private readonly TMGDbContext _context;

    private const string TEMPLATE_PATH = "BookletTemplates";

    public BookletController(TMGDbContext context, IWebHostEnvironment host)
    {
        _context = context;
        _host = host;
    }

    public IActionResult ConvertItienraryToPdf(int id){

        string path = Path.Combine(_host.WebRootPath, TEMPLATE_PATH, "Itinerary.html");
        string content = "";
        using (StreamReader s = new(path)){
            content = s.ReadToEnd();
        }

        ItineraryReportModel model = GetItineraryReportModel(id);


        return Ok(content);
    }


    public ItineraryReportModel GetItineraryReportModel(int id){
        
            var members = (from fm in _context.FamilyMembers
                           join fh in _context.FamilyHeads.Where(fh => !fh.Trashed) on fm.FamilyHeadId equals fh.FamilyHeadId
                           join p in _context.Packages on fh.PackageId equals p.PackageId
                           join c in _context.Companies on p.CompanyId equals c.CompanyId
                           join fe in _context.FamilyHeadExtras on fh.FamilyHeadId equals fe.FamilyHeadId into feGroup
                           from fe in feGroup.DefaultIfEmpty()
                           where fm.HeadId == id
                           select new
                           {
                               CompanyCode = c.CompanyCode,
                               IndId = fh.IndId,
                               FcId = fh.FcId,
                               fm.HeadId,
                               fm.FamilyHeadId,
                               PackageName = p.Name,
                               fh.FullName,
                               fh.PassportNumber,
                               fh.TicketNumber,
                               fh.PackageId,
                               Qurbani = (fe != null && fe.ExtraFacilityDescriptionId == 1) ? "Yes" : "No"
                           })
                          .AsNoTracking()
                          .ToList();


            var pnrMembers = _context.PNRMembers
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

            var accommodations = _context.PackageAccomodtions
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

            var flightQuery = (from f in _context.Flights
                               join fp in _context.FlightPackages on f.FlightId equals fp.FlightId
                               join p in _context.Packages on fp.PackageId equals p.PackageId
                               join pnr in _context.PNRs on f.FlightId equals pnr.FlightId
                               join pnrm in _context.PNRMembers on pnr.PNRId equals pnrm.PNRId
                               join s in _context.Sectors on f.FlightId equals s.FlightId
                               join fh in _context.FamilyHeads on pnrm.FamilyHeadId equals fh.FamilyHeadId
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
                var memberInGroup = _context.GroupMembers
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

            var rooms = _context.FamilyHeadAdditionalFacilities
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
                    CheckIn = acc.ActualCheckInTime?.ToString("dd MMM (hh tt)"),
                    CheckOut = acc.ActualCheckOutTime?.ToString("dd MMM" + " - " + "12 Noon"),
                    MealPlan = acc.FoodPackage.Package,
                    RoomType = roomString,
                    Quantity = quantityString
                };

                model.Accommodations.Add(_accModel);
            }

            var travelRoute = _context
                .Packages
                .Select(e => new
                {
                    e.PackageId,
                    e.TravelRouteId,
                    e.TravelRoute
                })
                .FirstOrDefault(e => e.PackageId == members.ElementAt(0).PackageId).TravelRoute;

            var stops = _context.TravelRouteStops
                    .Include(e => e.Route)
                    .Where(e => e.TravelRouteId == travelRoute.TravelRouteId)
                    .Select(e => new
                    {
                        Route = $"{e.Route.StartPlace} to {e.Route.Destination}",
                        Date = e.CheckIn.ToString("dd MMM")
                    });

            var routeData = _context.
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
                    Date = _context.TravelRouteStops.Select(e => new
                    {
                        e.RouteId,
                        e.TravelRouteId,
                        e.CheckIn
                    }).FirstOrDefault(e => e.RouteId == stop.RouteId && e.TravelRouteId == stop.TravelRouteId).CheckIn.ToString("dd/MMM"),
                    //Date = _context.TravelRouteStops
                    //        .OrderBy(e => e.CheckIn)
                    //        .Select(e => new
                    //        {
                    //            e.RouteId,
                    //            e.TravelRouteId,
                    //            e.CheckIn
                    //        })
                    //        .FirstOrDefault(e => e.RouteId == stop.RouteId && e.TravelRouteId == stop.TravelRouteId).CheckIn.ToString("dd/MMM"),
                    Type = stop.VehicleType.ToString()
                });
            }
            model.Routes = model.Routes.OrderBy(m => m.Date).ToList();


            model.FamilyHeadId = id;

            return model;
    }
}
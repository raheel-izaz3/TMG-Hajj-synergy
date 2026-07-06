using System.Text.RegularExpressions;
using TMG.Data;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.ReportModels;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.ManagerClasses;
using TMG.Controllers.FamilyControllers;
using TMG.Models.GroupModels;
using static iTextSharp.text.pdf.AcroFields;
using static System.Net.WebRequestMethods;
using System;
using SkiaSharp;

namespace TMG.Controllers.AirlineControllers;

public class FlightReportController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;

    public FlightReportController(TMGDbContext dbContext, IHttpContextAccessor http)
    {
        _dbContext = dbContext;
        _http = http;
    }

    public IActionResult RiyazUlJaanahCheckInSelect() => View();

    public IActionResult RiyazUlJaanahCheckIn(DateTime Date)
    {

        var query = from f in _dbContext.Flights
                    join fp in _dbContext.FlightPackages on f.FlightId equals fp.FlightId
                    join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                    join p in _dbContext.PNRs on f.FlightId equals p.FlightId
                    join pm in _dbContext.PNRMembers on p.PNRId equals pm.PNRId
                    join fh in _dbContext.FamilyHeads on pm.FamilyHeadId equals fh.FamilyHeadId
                    join pack in _dbContext.Packages on fp.PackageId equals pack.PackageId
                    join c in _dbContext.Companies on pack.CompanyId equals c.CompanyId
                    join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                    join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                    join pa in _dbContext.PackageAccomodtions on pack.PackageId equals pa.PackageId
                    where s.Destination.EndsWith("-MED") && pa.PlaceId == 2 &&
                          s.ArrivalDateTime.ToString().StartsWith(Date.ToString("yyyy-MM-dd")) &&
                          s.FlightType == FlightType.Outbound && pa.ActualCheckOutTime != null && s.ArrivalDateTime != null
                    group new { pack, m, c, s, pa, fh } by new
                    {
                        pack.PackageId,
                        m.MunazzimId,
                        c.CompanyId,
                        pack.Name,
                        c.CompanyCode,
                        MunazzimName = m.Name,
                        s.ArrivalDateTime,
                        pa.ActualCheckOutTime,
                        pa.Days
                    } into grouped
                    select new
                    {
                        Name = grouped.Key.Name,
                        CompanyCode = grouped.Key.CompanyCode,
                        PackageName = grouped.Key.Name,
                        MaleCount = grouped.Sum(x => x.fh.Gender == Gender.Male ? 1 : 0),
                        FemaleCount = grouped.Sum(x => x.fh.Gender == Gender.Female ? 1 : 0),
                        ArrivalTime = grouped.Key.ArrivalDateTime.Value.TimeOfDay,
                        CheckOutDate = grouped.Select(x => x.pa.ActualCheckOutTime.Value.ToString("dd MMM")).FirstOrDefault(),
                        Days = grouped.Key.Days,
                        grouped.Key.MunazzimName,
                        grouped.Key.MunazzimId,
                    };

        var data = query.ToList();
        var maleCount = data.Sum(e => e.MaleCount);
        var femaleCount = data.Sum(e => e.FemaleCount);

        var munazzims = _dbContext.Munazzims.ToList();

        RiyazUlJannahCheckInModel model = new();
        model.Arrival = Date;

        foreach (var munazzim in munazzims)
        {
            var m = data
                    .Where(e => e.MunazzimId == munazzim.MunazzimId)
                    .ToList();

            RiyazUlJannahCheckInGroupModel _groupModel = new();
            _groupModel.MunazzimName = munazzim.Name;

            foreach (var row in m)
            {
                RiyazUlJannahCheckInGroupRowModel _rowModel = new()
                {
                    MunazzimName = row.MunazzimName,
                    HGOName = row.CompanyCode,
                    PackageName = row.PackageName,
                    MaleCount = row.MaleCount,
                    FemaleCount = row.FemaleCount,
                    InfantCount = 0,
                    ArrivalTime = row.ArrivalTime.ToString(),
                    MedinaCheckOut = row.CheckOutDate,
                    Days = row.Days
                };

                _groupModel.TotalMaleCount += row.MaleCount;
                _groupModel.TotalFemaleCount += row.FemaleCount;

                _groupModel.Rows.Add(_rowModel);
            }

            model.MunazzimGroups.Add(_groupModel);
        }

        return View(model);
    }

    public IActionResult MunazzimReport(int id)
    {
        var result = (
             from fh in _dbContext.FamilyHeads
             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
             join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
             join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
             join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
             join fp in _dbContext.FlightPackages on p.PackageId equals fp.PackageId
             join f in _dbContext.Flights on fp.FlightId equals f.FlightId
             join pnrm in _dbContext.PNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
             join pnr in _dbContext.PNRs on pnrm.PNRId equals pnr.PNRId
             where !fh.Trashed && !p.Trashed && !c.Trashed && !m.Trashed
                 && f.FlightId == id
                 && pnr.FlightId == f.FlightId
                 && pa.PlaceId == 3
             group new { fh, m, c, pa } by new
             {
                 m.MunazzimId,
                 m.Name,
                 c.CompanyName,
                 fh.FamilyHeadId,
                 fh.GivenName,
                 fh.SurName,
                 fh.FcId,
                 fh.PassportNumber,
                 fh.Gender
             } into g
             select new
             {
                 MunazzimId = g.Key.MunazzimId,
                 Munazzam = g.Key.Name,
                 HGO = g.Key.CompanyName,
                 Maktab = g.Any(x => x.pa.AccomodationName.EndsWith("A")) ? "1" : "3",
                 g.Key.FcId,
                 FullName = g.Key.GivenName + " " + g.Key.SurName,
                 PassportNumber = g.Key.PassportNumber,
                 Gender = g.Key.Gender == Gender.Female ? "Female" : "Male"
             }
         ).ToList();

        var groupedResult = result.GroupBy(r => r.MunazzimId).Select(g => g.ToList()).ToList();

        ViewBag.Result = groupedResult;


        return View();
    }

    public IActionResult PNRReport(int id)
    {
        Flight flight = _dbContext.Flights.Find(id);
        var sectors = _dbContext.Sectors.Where(e => e.FlightId == id).ToList();

        List<Sector> flightSectors = _dbContext.Sectors
                .Include(a => a.Flight)
                .Where(a => a.FlightId == id)
                .ToList();

        var pnrs = _dbContext.PNRs.Where(e => e.FlightId == id).ToList();

        var pnrMembers = _dbContext.PNRMembers
            .Include(e => e.PNR)
            .Where(e => e.PNR.FlightId == id && !e.FamilyHead.Trashed)
            .Select(e => new
            {
                e.FamilyHead.GivenName,
                e.FamilyHead.SurName,
                e.FamilyHead.Gender,
                e.FamilyHead.PassportNumber,
                e.FamilyHead.PassportDateOfExpiry,
                e.FamilyHead.DateOfBirth,
                e.PNRId,
                e.FamilyHead.Honorifics.Title
            })
            .ToList();

        PNRReportModel model = new PNRReportModel();

        for (int i = 0; i < sectors.Count; i++)
        {
            Sector currSector = sectors.ElementAt(i);
            model.Sectors.Add(new PNRReportSector
            {
                SectorNumber = i + 1,
                FlightNumber = currSector.FlightNo,
                Destination = currSector.Destination,
                DepartureDate = DateTimeManager.GetDate(currSector.DepartureDateTime),
                DepartureTime = currSector.DepartureDateTime?.ToString("HH:mm"),
                ArrivalDate = DateTimeManager.GetDate(currSector.ArrivalDateTime),
                ArrivalTime = currSector.ArrivalDateTime?.ToString("HH:mm"),
            });
        }

        for (int i = 0; i < pnrs.Count; i++)
        {
            PNR currPnr = pnrs.ElementAt(i);
            var currPnrMembers = pnrMembers.Where(e => e.PNRId == currPnr.PNRId).ToList();

            PNRReportEntryModel entry = new PNRReportEntryModel();
            entry.PNR = currPnr.Name;

            for (int j = 0; j < currPnrMembers.Count; j++)
            {
                var currMember = currPnrMembers.ElementAt(j);
                entry.PNRMembers.Add(new PNRMembersReportModel
                {
                    SNo = j + 1,
                    Title = currMember.Title,
                    GivenName = currMember.GivenName,
                    SurName = currMember.SurName,
                    Gender = currMember.Gender.ToString(),
                    PassportNumber = currMember.PassportNumber,
                    PassportDOE = DateTimeManager.GetDate(currMember.PassportDateOfExpiry),
                    DateOfBirth = DateTimeManager.GetDate(currMember.DateOfBirth),
                });
            }

            model.Entries.Add(entry);
        }


        return View(model);
    }

    public IActionResult FlightSummaryDailyReport()
    {

        var flight = _dbContext.Flights
            .Include(a => a.Airline)
            .ToList();

        var places = _dbContext.Places
        .ToList();

        foreach (var place in places)
        {
            if (place.PlaceId == 2)
            {
                place.PlaceName = "Medinah";
            }
            else
            {
                place.PlaceName = "Jeddah";
            }
        }

        var uniquePlaces = places.GroupBy(p => p.PlaceName)
                                 .Select(g => g.First())
                                 .ToList();




        ViewBag.Flight = flight.Where(a => !a.Trashed);
        ViewBag.Place = uniquePlaces;


        ViewBag.TrashCount = _dbContext.Places.Where(e => e.Trashed).Count();
        ViewBag.Places = _dbContext.Places.Where(a => !a.Trashed).ToList();
        return View();

    }

    public IActionResult FlightSummaryDailyReportGenerate(int id, DateTime Date)
    {
        string PlaceValue = (id == 1 ? "JED" : "MED");

        var flightsByDate = _dbContext.Sectors
 .Where(a =>
                a.ArrivalDateTime.Value.Date == Date.Date)
            .Select(a => a.ArrivalDateTime.Value.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var time = (from f in _dbContext.Flights
                    join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                    where s.ArrivalDateTime.Value.Date == Date.Date
                        && s.FlightType == FlightType.Outbound
                        && s.ArrivalDateTime == _dbContext.Sectors
                           .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Outbound)
                           .Max(s2 => s2.ArrivalDateTime)
                        && s.Destination.EndsWith(PlaceValue)
                        && !f.Trashed
                    group s by f.OutboundFlightNumber into g
                    select new
                    {
                        OutboundFlightNumber = g.Key,
                        ArrivalDateTime = g.Max(s => s.ArrivalDateTime)
                    })
                       .OrderBy(a => a.ArrivalDateTime)
          .ToList();




        var flights = (from f in _dbContext.Flights
                       join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                       join al in _dbContext.Airlines on f.AirlineId equals al.AirlineId
                       where s.ArrivalDateTime.Value.Date == Date.Date
                           && s.FlightType == FlightType.Outbound
                           && s.ArrivalDateTime == _dbContext.Sectors
                              .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Outbound)
                              .Max(s2 => s2.ArrivalDateTime)
                           && s.Destination.EndsWith(PlaceValue)
                           && !f.Trashed
                       group s by new { f.OutboundFlightNumber, s.ArrivalDateTime.Value.Date } into g
                       select new
                       {
                           FNo = g.Key.OutboundFlightNumber,
                           ArrivalDateTime = g.Max(a => a.ArrivalDateTime)
                       })
                       .OrderBy(a => a.ArrivalDateTime)
                       .ToList();

        var place = _dbContext.Places
                .Where(a => a.PlaceId == id && !a.Trashed)
                .ToList();

        //var companies = _dbContext.FlightPackages
        //    .Include(a => a.Flight)
        //    .Where(a => a.Flight.OutboundDepartureDateTime.Date == Date && !a.Flight.Trashed)
        //    .Include(fp => fp.Package)
        //    .ThenInclude(p => p.Company)
        //    .Select(fp => fp.Package.Company)

        //    .Distinct()
        //    .ToList();
        var companies = (from fp in _dbContext.FlightPackages
                         join f in _dbContext.Flights on fp.FlightId equals f.FlightId
                         join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                         join p in _dbContext.Packages on fp.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         //where f.OutboundDepartureDateTime.Date == Date.Date
                         //      && s.Destination.EndsWith(PlaceValue)
                         //      && f.Trashed == false
                         where s.ArrivalDateTime.Value.Date == Date.Date
                           && s.FlightType == FlightType.Outbound
                           && s.ArrivalDateTime == _dbContext.Sectors
                              .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Outbound)
                              .Max(s2 => s2.ArrivalDateTime)
                           && s.Destination.EndsWith(PlaceValue)
                           && !f.Trashed
                         select c).Distinct();

        var dateFilter = Date.Date;





        //var results = (from fh in _dbContext.FamilyHeads
        //               join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
        //               join p in _dbContext.Packages on fh.PackageId equals p.PackageId
        //               join fp in _dbContext.FlightPackages on p.PackageId equals fp.PackageId
        //               join f in _dbContext.Flights on fp.FlightId equals f.FlightId
        //               join s in _dbContext.Sectors on f.FlightId equals s.FlightId
        //               join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
        //               join pnr in _dbContext.PNRs on f.FlightId equals pnr.FlightId
        //               join pnrm in _dbContext.PNRMembers on pnr.PNRId equals pnrm.PNRId
        //               join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
        //               where f.OutboundArrivalDateTime.Date == Date.Date
        //                     && s.FlightType == (FlightType) 1
        //                     && pnrm.FamilyHeadId == fm.FamilyHeadId
        //                     && pa.PlaceId == id
        //                     && f.Trashed == false
        //                     && fh.Trashed == false
        //               group new { fh, fm, p, fp, f, s, c, pnr, pnrm, pa } by new { c.CompanyCode, f.OutboundFlightNumber } into g
        //               select new
        //               {
        //                   g.Key.CompanyCode,
        //                   ArrivalDateTime = g.Max(x => x.s.ArrivalDateTime),
        //                   g.Key.OutboundFlightNumber,
        //                   FamilyHeadCount = g.Select(x => x.fh.FamilyHeadId).Distinct().Count()
        //               })
        //  .ToList();

        var results = (from fh in _dbContext.FamilyHeads
                       join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                       join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                       join fp in _dbContext.FlightPackages on p.PackageId equals fp.PackageId
                       join f in _dbContext.Flights on fp.FlightId equals f.FlightId
                       join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                       join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                       join pnr in _dbContext.PNRs on f.FlightId equals pnr.FlightId
                       join pnrm in _dbContext.PNRMembers on pnr.PNRId equals pnrm.PNRId
                       //join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                       //where s.ArrivalDateTime.Value.Date == Date.Date
                       //      && s.FlightType == (FlightType)1
                       //      && pnrm.FamilyHeadId == fm.FamilyHeadId
                       //      && s.Destination.EndsWith(PlaceValue)
                       //      //&& pa.PlaceId == id
                       //      && f.Trashed == false
                       //      && fh.Trashed == false
                       where s.ArrivalDateTime.Value.Date == Date.Date
                            && s.FlightType == FlightType.Outbound
                            && s.ArrivalDateTime == _dbContext.Sectors
                               .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Outbound)
                               .Max(s2 => s2.ArrivalDateTime)
                            && pnrm.FamilyHeadId == fm.FamilyHeadId
                            && s.Destination.EndsWith(PlaceValue)
                            && !f.Trashed
                            && !fh.Trashed
                       group new { fh, fm, p, fp, f, s, c, pnr, pnrm } by new { c.CompanyCode, f.OutboundFlightNumber } into g
                       select new
                       {
                           g.Key.CompanyCode,
                           ArrivalDateTime = g.Max(x => x.s.ArrivalDateTime),
                           g.Key.OutboundFlightNumber,
                           FamilyHeadCount = g.Select(x => x.fh.FamilyHeadId).Distinct().Count(),
                           FlightId = g.Min(x => x.s.FlightId)

                       }).OrderBy(a => a.ArrivalDateTime.Value.TimeOfDay)
        .ToList();


        Dictionary<int, int> flightIdsDict = new Dictionary<int, int>();
        int snoflight = 0;

        foreach (var item in flights)
        {
            var flightIds = _dbContext.Flights
.Where(f => f.OutboundArrivalDateTime.Date == Date.Date &&
            f.OutboundFlightNumber == item.FNo &&
            !f.Trashed)
.OrderByDescending(f => f.FlightId)
.Select(f => new { f.FlightId, f.OutboundFlightNumber, f.OutboundArrivalDateTime })
.FirstOrDefault();
            if (flightIds != null)
            {
                flightIdsDict.Add(snoflight + 1, Convert.ToInt32(flightIds.FlightId));
                snoflight++;
            }

        }


        ViewBag.Place = place;
        ViewBag.flight = flightsByDate;
        ViewBag.time = time;
        ViewBag.Flights = flights;
        ViewBag.Companies = companies;
        ViewBag.Results = results;
        if (flightIdsDict.Count > 0)
        {
            ViewBag.FlightId = flightIdsDict;
        }
        else
        {
            ViewBag.FlightId = 0;
        }


        return View();
    }

    public IActionResult FlightReportFilter(int id)
    {
        var activeSeasonId = Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

        var familyHeadFlightCounts = _dbContext.FamilyHeadFlight
            .Where(e => e.FamilyHead.Package.HajjSeasonsId == activeSeasonId
                        && !e.FamilyHead.Package.Trashed && e.FamilyHead.Package.CompanyId == id)
            .GroupBy(e => e.FlightClass)
            .Select(g => new
            {
                FlightClass = g.Key,
                Count = g.Count()
            })
            .ToList();

        var packageFlightCounts = _dbContext.PackageFlights
            .Where(e => e.Package.HajjSeasonsId == activeSeasonId && !e.Package.Trashed
                            && e.Package.CompanyId == id)
            .GroupBy(e => e.FlightClass)
            .Select(g => new
            {
                FlightClass = g.Key,
                Count = g.Count()
            })
            .ToList();

        var combinedCounts = Enum.GetValues(typeof(FlightClass))
            .Cast<FlightClass>()
            .Select(fc => new
            {
                FlightClass = fc.ToString(),
                FamilyHeadFlightCount = familyHeadFlightCounts.FirstOrDefault(f => f.FlightClass == fc)?.Count ?? 0,
                PackageFlightCount = packageFlightCounts.FirstOrDefault(p => p.FlightClass == fc)?.Count ?? 0
            })
            .ToList();

        ViewBag.FlightSummary = combinedCounts;
        ViewBag.CompanyId = id;

        return View();
    }

    public IActionResult FlightSummary(string flightClass, string type, int id)
    {
        var familyHeadFlights = _dbContext.FamilyHeadFlight
            .Include(e => e.Airline)
            .Include(e => e.FamilyHead)
            .ThenInclude(e => e.Package)
            .Where(e => e.FamilyHead.Package.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            .Where(e => !e.FamilyHead.Package.Trashed && e.FamilyHead.Package.CompanyId == id)
            .AsEnumerable() 
            .Where(f => f.FlightClass.ToString() == flightClass)  
            .ToList();

        var packageFlights = _dbContext.PackageFlights
            .Include(e => e.Airline)
            .Include(e => e.Package)
            .Where(e => e.Package.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            .Where(e => !e.Package.Trashed && e.Package.CompanyId == id)
            .AsEnumerable()  
            .Where(f => f.FlightClass.ToString() == flightClass)  
            .ToList();

        List<FlightSummaryModel> flightSummaries = new List<FlightSummaryModel>();

        if (type == "FamilyHead")
        {
            flightSummaries = familyHeadFlights.Select(f => new FlightSummaryModel
            {
                Airline = f.Airline.Name,
                Class = f.FlightClass.ToString(),
                ArrivalDate = f.ArrivalDateTime?.ToString("dd/MMM/yyyy"),
                ArrivalTime = f.ArrivalDateTime?.ToString("HH:mm"),
                DepartureDate = f.DepartureDateTime?.ToString("dd/MMM/yyyy"),
                DepartureTime = f.DepartureDateTime?.ToString("HH:mm"),
                Origin = f.Origin,
                Destination = f.Destination,
                FlightNo = f.FlightNo,
                PNRNo = f.PNRNo,
                Total = f.Total ?? 0f,
                PackageName = f.FamilyHead.Package.Name
            }).ToList();
        }
        else if (type == "Package")
        {
            flightSummaries = packageFlights.Select(f => new FlightSummaryModel
            {
                Airline = f.Airline.Name,
                Class = f.FlightClass.ToString(),
                //ArrivalDate = f.ArrivalDateTime?.ToString("dd/MMM/yyyy"),
                //ArrivalTime = f.ArrivalDateTime?.ToString("HH:mm"),
                //DepartureDate = f.DepartureDateTime?.ToString("dd/MMM/yyyy"),
                //DepartureTime = f.DepartureDateTime?.ToString("HH:mm"),
                Origin = f.Origin,
                Destination = f.Destination,
                FlightNo = f.FlightNumber,
                PNRNo = f.PNRNumber,
                Total = f.TicketAmount ?? 0f,
                PackageName = f.Package.Name
            }).ToList();
        }

        return View(flightSummaries);
    }

    public IActionResult FlightMembersDetailsArrival(int id)
    {

        var outbondarrival = _dbContext.Flights
                            .Where(f => f.FlightId == id)
                            .Select(f => new { f.OutboundArrivalDateTime, f.OutboundFlightNumber })
                            .ToList();


        var date = outbondarrival[0].OutboundArrivalDateTime.Date;
        var flightnumber = outbondarrival[0].OutboundFlightNumber;
        var query = (from fh in _dbContext.FamilyHeads
                     join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                     join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                     join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                     join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                     from gm in gmGroup.DefaultIfEmpty()
                     join g in _dbContext.Groups.Where(g => !g.Trashed) on gm.GroupId equals g.GroupId into gGroup
                     from g in gGroup.DefaultIfEmpty()
                     join cm in _dbContext.EHajjContractMembers on fh.FamilyHeadId equals cm.FamilyHeadId into cmGroup
                     from cm in cmGroup.DefaultIfEmpty()
                     join ehc in _dbContext.EHajjContracts on cm.EhajjContractId equals ehc.EhajjContractId into ehcGroup
                     from ehc in ehcGroup.DefaultIfEmpty()
                     join h in _dbContext.Hotels on ehc.HotelId equals h.HotelId into hGroup
                     from h in hGroup.DefaultIfEmpty()
                     join pnrm in _dbContext.PNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
                     join pnr in _dbContext.PNRs on pnrm.PNRId equals pnr.PNRId
                     join f in _dbContext.Flights on pnr.FlightId equals f.FlightId
                     where f.OutboundArrivalDateTime.Date == date && f.OutboundFlightNumber == flightnumber && !fh.Trashed
                     orderby fh.FamilyHeadId
                     select new
                     {
                         FmId = fh.FamilyHeadId,
                         IsHead = fh.IsHead,
                         FullName = fh.GivenName + " " + fh.SurName,
                         MemberImage = fh.MemberImage ?? "",

                         //              CID = (c.CompanyCode + "." + fh.IndId + "." + fh.FcId.Substring(3) + ".(" +
                         //              _dbContext.FamilyMembers.Count(fm => fm.HeadId == fh.FamilyHeadId) + ")" +
                         //              p.Name + (gm != null ? g.Name + g.GroupId.ToString() : "")) ?? "",


                         CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                         $".{fm.Head.ExtractedFamilyNumber}.({_dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count()})" +
                         $".{fm.FamilyHead.Package.Name}",

                         PackageName = p.Name ?? "",
                         PassportNumber = fh.PassportNumber ?? "",
                         Gender = fh.Gender,
                         EhajjContractNumber = ehc.EhajjContractNumber ?? "",
                         HotelName = h.HotelName ?? "",
                         CheckIn = ehc.CheckIn != null ? ehc.CheckIn : DateTime.MinValue,
                         CheckOut = ehc.CheckOut == null ? DateTime.MinValue : ehc.CheckOut,
                         HotelReservationNumber = ehc.HotelReservationNumber ?? "",
                         FcId = fh.FcId ?? "",
                         PNR = pnr.Name ?? "",
                         FlightNo = f.OutboundFlightNumber ?? "",
                         AssignedVehicle = fh.AssignedVehicle ?? 0
                     })
                    .Distinct();

        var result = query.ToList();


        var pnrs = (from fh in _dbContext.FamilyHeads
                    join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                    join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                    join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                    from gm in gmGroup.DefaultIfEmpty()
                    join g in _dbContext.Groups.Where(g => !g.Trashed) on gm.GroupId equals g.GroupId into gGroup
                    from g in gGroup.DefaultIfEmpty()
                    join cm in _dbContext.EHajjContractMembers on fh.FamilyHeadId equals cm.FamilyHeadId into cmGroup
                    from cm in cmGroup.DefaultIfEmpty()
                    join ehc in _dbContext.EHajjContracts on cm.EhajjContractId equals ehc.EhajjContractId into ehcGroup
                    from ehc in ehcGroup.DefaultIfEmpty()
                    join h in _dbContext.Hotels on ehc.HotelId equals h.HotelId into hGroup
                    from h in hGroup.DefaultIfEmpty()
                    join pnrm in _dbContext.PNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
                    join pnr in _dbContext.PNRs on pnrm.PNRId equals pnr.PNRId
                    join f in _dbContext.Flights on pnr.FlightId equals f.FlightId
                    where f.OutboundArrivalDateTime.Date == date && f.OutboundFlightNumber == flightnumber && !fh.Trashed
                    select new
                    {
                        PNR = pnr.Name
                    }).Distinct()
                    .ToList();


        // ================= For Vehicles ======================== // 

        var vehicles = _dbContext.Vehicles
            .Where(a => !a.Trashed)
            .ToList();

        var assignedVehicles = (from v in _dbContext.Vehicles
                                join fh in _dbContext.FamilyHeads on v.VehicleId equals fh.AssignedVehicle
                                where !v.Trashed && !v.Trashed && fh.AssignedVehicle == v.VehicleId
                                select new
                                {
                                    VehicleId = v.VehicleId,
                                    Type = v.VehicleType,
                                    AssignedVehicle = fh.AssignedVehicle,
                                    Name = fh.GivenName
                                });

        ViewBag.Assigned = assignedVehicles;
        ViewBag.Vehicle = vehicles;
        ViewBag.details = result;
        ViewBag.PNR = pnrs;

        return View();

    }

    public IActionResult FlightMembersDetailsArrivalFamilywise(string id)
    {


        var query = (from fh in _dbContext.FamilyHeads
                     join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                     join fhc in _dbContext.FamilyHeadContacts on new { fh.FamilyHeadId, Preferred = true } equals new { fhc.FamilyHeadId, fhc.Preferred } into fhcGroup
                     from fhc in fhcGroup.DefaultIfEmpty()
                     join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                     join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                     join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                     from gm in gmGroup.DefaultIfEmpty()
                     join g in _dbContext.Groups on gm.GroupId equals g.GroupId into gGroup
                     from g in gGroup.DefaultIfEmpty()
                     join cm in _dbContext.EHajjContractMembers on fh.FamilyHeadId equals cm.FamilyHeadId into cmGroup
                     from cm in cmGroup.DefaultIfEmpty()
                     join ehc in _dbContext.EHajjContracts on cm.EhajjContractId equals ehc.EhajjContractId into ehcGroup
                     from ehc in ehcGroup.DefaultIfEmpty()
                     join h in _dbContext.Hotels on ehc.HotelId equals h.HotelId into hGroup
                     from h in hGroup.DefaultIfEmpty()
                     join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId into mcGroup
                     from mc in mcGroup.DefaultIfEmpty()
                     join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId into mGroup
                     from m in mGroup.DefaultIfEmpty()
                     join pnrm in _dbContext.PNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
                     join pnr in _dbContext.PNRs on pnrm.PNRId equals pnr.PNRId
                     join f in _dbContext.Flights on pnr.FlightId equals f.FlightId
                     where fh.FcId == id && !fh.Trashed
                     select new
                     {

                         FullName = fh.GivenName + " " + fh.SurName,
                         MemberImage = fh.MemberImage ?? "",
                         CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                         $".{fm.Head.ExtractedFamilyNumber}.({_dbContext.FamilyMembers.Where(e => e.HeadId == fm.HeadId).Count()})" +
                         $".{fm.FamilyHead.Package.Name}",
                         PackageName = p.Name ?? "",
                         PassportNumber = fh.PassportNumber ?? "",
                         fh.Gender,
                         EhajjContractNumber = ehc.EhajjContractNumber ?? "",
                         Contact = fhc.Contact ?? "",
                         HotelName = h.HotelName ?? "",
                         Location = h.Location ?? "",
                         CheckIn = ehc.CheckIn != null ? ehc.CheckIn : DateTime.MinValue,
                         CheckOut = ehc.CheckOut == null ? DateTime.MinValue : ehc.CheckOut,
                         HotelReservationNumber = ehc.HotelReservationNumber ?? "",
                         QR = ehc.QrPath ?? "",
                         ArrivalDate = pnr.Flight.OutboundArrivalDateTime != null ? pnr.Flight.OutboundArrivalDateTime : DateTime.MinValue,
                         FlightNo = pnr.Flight.OutboundFlightNumber ?? "",
                         Nights = ehc.CheckOut != null && ehc.CheckIn != null
                 ? (ehc.CheckOut.Day - ehc.CheckIn.Day)
                 : 0,
                         CompanyName = c.CompanyName ?? "",
                         MunazimName = m.Name ?? "",
                         PNR = pnrm.PNR.Name ?? "",
                     }).Distinct();



        var vehicles = _dbContext.Vehicles
            .Join(_dbContext.FamilyHeads,
                v => v.VehicleId,
                fh => fh.AssignedVehicle,
                (v, fh) => new { Vehicle = v, FamilyHead = fh })
            .Where(x => x.FamilyHead.FcId == id && !x.FamilyHead.Trashed && !x.Vehicle.Trashed)
            .GroupBy(x => x.Vehicle.VehicleType)
            .Select(g => new
            {
                VehicleCount = g.Count(),
                Type = g.Key
            });


        var vehicleCount = _dbContext.Vehicles
 .Join(_dbContext.FamilyHeads,
       v => v.VehicleId,
       fh => fh.AssignedVehicle,
       (v, fh) => new { Vehicle = v, FamilyHead = fh })
 .Where(x => x.FamilyHead.FcId == id)
 .Select(x => x.Vehicle.VehicleId)
 .Distinct()
 .Count();



        var result = query.ToList();

        int maleCount = result.Count(a => a.Gender == 0);
        int femaleCount = result.Count(a => (int)a.Gender == 1);
        int total = maleCount + femaleCount;

        ViewBag.Male = maleCount;
        ViewBag.Female = femaleCount;
        ViewBag.PAX = total;

        ViewBag.details = result;

        ViewBag.Vehicle = vehicles;
        ViewBag.VehiclesCount = vehicleCount;
        return View();

    }

    public IActionResult FlightSummaryDailyReportDeparture()
    {

        var flight = _dbContext.Flights
            .Include(a => a.Airline)
            .ToList();

        var places = _dbContext.Places
        .ToList();

        foreach (var place in places)
        {
            if (place.PlaceId == 2)
            {
                place.PlaceName = "Medinah";
            }
            else
            {
                place.PlaceName = "Jeddah";
            }
        }

        var uniquePlaces = places.GroupBy(p => p.PlaceName)
                                 .Select(g => g.First())
                                 .ToList();




        ViewBag.Flight = flight.Where(a => !a.Trashed);
        ViewBag.Place = uniquePlaces;


        ViewBag.TrashCount = _dbContext.Places.Where(e => e.Trashed).Count();
        ViewBag.Places = _dbContext.Places.Where(a => !a.Trashed).ToList();
        return View();

    }

    public IActionResult FlightSummaryDailyReportGenerateDeparture(int id, DateTime Date)
    {
        string PlaceValue = (id == 1 ? "JED" : "MED");

        var flightsByDate = _dbContext.Sectors
 .Where(a =>
                a.DepartureDateTime.Value.Date == Date.Date)
            .Select(a => a.DepartureDateTime.Value.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var time = (from f in _dbContext.Flights
                    join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                    where s.DepartureDateTime.Value.Date == Date.Date
                        && s.FlightType == FlightType.Inbound
                        && s.DepartureDateTime == _dbContext.Sectors
                           .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Inbound)
                           .Min(s2 => s2.DepartureDateTime)
                        && s.Destination.StartsWith(PlaceValue)
                        && !f.Trashed
                    group s by f.InboundFlightNumber into g
                    select new
                    {
                        InboundFlightNumber = g.Key,
                        DepartureDateTime = g.Min(s => s.DepartureDateTime)
                    })
                       .OrderBy(a => a.DepartureDateTime)
          .ToList();




        var flights = (from f in _dbContext.Flights
                       join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                       join al in _dbContext.Airlines on f.AirlineId equals al.AirlineId
                       where s.DepartureDateTime.Value.Date == Date.Date
                           && s.FlightType == FlightType.Inbound
                           && s.DepartureDateTime == _dbContext.Sectors
                              .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Inbound)
                              .Min(s2 => s2.DepartureDateTime)
                           && s.Destination.StartsWith(PlaceValue)
                           && !f.Trashed
                       group s by new { f.InboundFlightNumber, s.DepartureDateTime.Value.Date } into g
                       select new
                       {
                           FNo = g.Key.InboundFlightNumber,
                           DepartureDateTime = g.Min(a => a.DepartureDateTime)
                       })
                       .OrderBy(a => a.DepartureDateTime)
                       .ToList();

        var place = _dbContext.Places
                .Where(a => a.PlaceId == id && !a.Trashed)
                .ToList();


        var companies = (from fp in _dbContext.FlightPackages
                         join f in _dbContext.Flights on fp.FlightId equals f.FlightId
                         join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                         join p in _dbContext.Packages on fp.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         where s.DepartureDateTime.Value.Date == Date.Date
                           && s.FlightType == FlightType.Inbound
                           && s.DepartureDateTime == _dbContext.Sectors
                              .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Inbound)
                              .Min(s2 => s2.DepartureDateTime)
                           && s.Destination.StartsWith(PlaceValue)
                           && !f.Trashed
                         select c).Distinct();

        var dateFilter = Date.Date;





        var results = (from fh in _dbContext.FamilyHeads
                       join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                       join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                       join fp in _dbContext.FlightPackages on p.PackageId equals fp.PackageId
                       join f in _dbContext.Flights on fp.FlightId equals f.FlightId
                       join s in _dbContext.Sectors on f.FlightId equals s.FlightId
                       join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                       join pnr in _dbContext.PNRs on f.FlightId equals pnr.FlightId
                       join pnrm in _dbContext.PNRMembers on pnr.PNRId equals pnrm.PNRId
                       where s.DepartureDateTime.Value.Date == Date.Date
                            && s.FlightType == FlightType.Inbound
                            && s.DepartureDateTime == _dbContext.Sectors
                               .Where(s2 => s2.FlightId == s.FlightId && s2.FlightType == FlightType.Inbound)
                               .Min(s2 => s2.DepartureDateTime)
                            && pnrm.FamilyHeadId == fm.FamilyHeadId
                            && s.Destination.StartsWith(PlaceValue)
                            && !f.Trashed
                            && !fh.Trashed
                       group new { fh, fm, p, fp, f, s, c, pnr, pnrm } by new { c.CompanyCode, f.InboundFlightNumber } into g
                       select new
                       {
                           g.Key.CompanyCode,
                           DepartureDateTime = g.Min(x => x.s.DepartureDateTime),
                           g.Key.InboundFlightNumber,
                           FamilyHeadCount = g.Select(x => x.fh.FamilyHeadId).Distinct().Count(),
                           FlightId = g.Min(x => x.s.FlightId)

                       }).OrderBy(a => a.DepartureDateTime.Value.TimeOfDay)
        .ToList();


        Dictionary<int, int> flightIdsDict = new Dictionary<int, int>();
        int snoflight = 0;

        foreach (var item in flights)
        {
            var flightIds = _dbContext.Flights
.Where(f => f.InboundDepartureDateTime.Value.Date == Date.Date &&
            f.InboundFlightNumber == item.FNo &&
            !f.Trashed)
.OrderByDescending(f => f.FlightId)
.Select(f => new { f.FlightId, f.InboundFlightNumber, f.OutboundDepartureDateTime })
.FirstOrDefault();
            if (flightIds != null)
            {
                flightIdsDict.Add(snoflight + 1, Convert.ToInt32(flightIds.FlightId));
                snoflight++;
            }

        }


        ViewBag.Place = place;
        ViewBag.flight = flightsByDate;
        ViewBag.time = time;
        ViewBag.Flights = flights;
        ViewBag.Companies = companies;
        ViewBag.Results = results;
        if (flightIdsDict.Count > 0)
        {
            ViewBag.FlightId = flightIdsDict;
        }
        else
        {
            ViewBag.FlightId = 0;
        }


        return View();
    }

}
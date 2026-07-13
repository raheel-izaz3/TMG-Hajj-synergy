using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IO.Packaging;
using static TMG.Controllers.ReportController;
using iText.Svg.Renderers.Impl;
using TMG.Models.GroupModels;

namespace TMG.Controllers.AirlineControllers
{
    public class PNRController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public PNRController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.PNRs
                .Where(ca => ca.Trashed && ca.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count();

            var pnrs = _dbContext
                .PNRsWithoutTrash
                .Include(e => e.Flight)
                .Include(e => e.Airline)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            var pnrMembers = _dbContext
                .PNRMembers
                .Include(e => e.PNR)
                .Where(e => !e.PNR.Trashed && e.PNR.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            PNRIndexModel model = new PNRIndexModel();

            for (int i = 0; i < pnrs.Count; i++)
            {
                PNR currPnr = pnrs.ElementAt(i);
                int booked = pnrMembers.Where(e => e.PNRId == currPnr.PNRId).Count();
                int balance = currPnr.Capacity - booked;

                model.Entries.Add(
                    new PNRIndexEntryModel(
                        currPnr.PNRId, currPnr.Name, currPnr.PNRNo, currPnr.PNRType.ToString(),
                        currPnr.Airline.Name,
                        currPnr.Type.ToString(), currPnr.Capacity, booked, balance
                        )
                    );
            }

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Flights = _dbContext.FlightsWithoutTrash;

            ViewBag.NewFlights = _dbContext.POTicketFlightDatas
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.Airline = _dbContext.AirlinesWithoutTrash;

            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(PNR pnr)
        {
            ViewBag.Flights = _dbContext.FlightsWithoutTrash;

            ViewBag.NewFlights = _dbContext.POTicketFlightDatas
             .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
             .ToList();

            ViewBag.Airline = _dbContext.AirlinesWithoutTrash;

            ViewBag.Packages = _dbContext.PackagesWithoutTrash
               .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
               .ToList();

            var activeFlight = _dbContext.Flights.FirstOrDefault(e => !e.Trashed);
            if (activeFlight == null)
            {
                ModelState.AddModelError("FlightId", "No active Flight exists in the system. Please register a Flight before creating a PNR.");
                return View(pnr);
            }

            pnr.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            pnr.FlightId = activeFlight.FlightId;


            _dbContext.PNRs.Add(pnr);
            _dbContext.SaveChanges();

            for (int i = 0; i < (pnr.PackageIds?.Count ?? 0); i++)
            {
                if (pnr.PackagesSelected[i] == false) continue;

                _dbContext.PNRPackages.Add(new PNRPackage
                {
                    PackageId = pnr.PackageIds[i],
                    PNRId = pnr.PNRId
                });
            }
            _dbContext.SaveChanges();

            TempData["created"] = true;
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            PNR pnr = _dbContext.PNRs
                //.Include(e => e.Flight)
                .FirstOrDefault(e => e.PNRId == id);

            ViewBag.CurrentCapacity = pnr.Capacity;
            int booked = _dbContext.PNRMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.PNRId == id && !e.FamilyHead.Trashed)
                .Count();
            ViewBag.Booked = booked;
            ViewBag.Balance = pnr.Capacity - booked;

            ViewBag.NewFlights = _dbContext.POTicketFlightDatas
            .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            .ToList();

            ViewBag.Airline = _dbContext.AirlinesWithoutTrash;

            ViewBag.Packages = _dbContext.PackagesWithoutTrash
               .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
               .ToList();

            ViewBag.PNRPackages = _dbContext.PNRPackages.Where(e => e.PNRId == id);

            //ViewBag.EconomySeats = pnr.Flight.EconomySeats;
            //ViewBag.BusinessSeats = pnr.Flight.BusinessSeats;


            return View(pnr);
        }

        [HttpPost]
        public IActionResult Edit(PNR pnr)
        {
            PNR pnrToEdit = _dbContext.PNRs
                .Include(e => e.Flight)
                .FirstOrDefault(e => e.PNRId == pnr.PNRId);

            ViewBag.CurrentCapacity = pnr.Capacity;
            int booked = _dbContext.PNRMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.PNRId == pnr.PNRId && !e.FamilyHead.Trashed)
                .Count();
            ViewBag.Booked = booked;
            ViewBag.Balance = pnr.Capacity - booked;

            ViewBag.EconomySeats = pnr.Flight?.EconomySeats;
            ViewBag.BusinessSeats = pnr.Flight?.BusinessSeats;

            if (!ModelState.IsValid) return View(pnr);

            var activeFlight = _dbContext.Flights.FirstOrDefault(e => !e.Trashed);
            if (activeFlight == null)
            {
                ModelState.AddModelError("FlightId", "No active Flight exists in the system. Please register a Flight first.");
                return View(pnr);
            }

            pnrToEdit.FlightId = activeFlight.FlightId;
            pnrToEdit.Type = pnr.Type;
            pnrToEdit.PNRType = pnr.PNRType;
            pnrToEdit.Name = pnr.Name;
            pnrToEdit.PNRNo = pnr.PNRNo;
            pnrToEdit.POTicketFlightDataId = pnr.POTicketFlightDataId;
            pnrToEdit.AirlineId = pnr.AirlineId;
            pnrToEdit.Capacity = pnr.Capacity;
            pnrToEdit.UpdatedAt = DateTime.Now;
            pnrToEdit.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.SaveChanges();

            _dbContext.PNRPackages.Where(e => e.PNRId == pnr.PNRId).ExecuteDelete();

            for (int i = 0; i < (pnr.PackageIds?.Count ?? 0); i++)
            {
                if (pnr.PackagesSelected[i] == false) continue;

                _dbContext.PNRPackages.Add(new PNRPackage
                {
                    PackageId = pnr.PackageIds[i],
                    PNRId = pnr.PNRId
                });
            }
            _dbContext.SaveChanges();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }


        //public IActionResult AddMembers(int id)
        //{
        //    PNR pnr = _dbContext.PNRs.Find(id);
        //    Flight flight = _dbContext.Flights.Find(pnr.FlightId);
        //    var flightPackages = _dbContext.FlightPackages.Where(e => e.FlightId == pnr.FlightId).Select(e => e.PackageId).ToList();

        //    var members = _dbContext.FamilyMembers
        //    .Join(
        //        _dbContext.FamilyHeads,
        //        fm => fm.FamilyHeadId,
        //        fh => fh.FamilyHeadId,
        //        (fm, fh) => new { FamilyMember = fm, FamilyHead = fh })
        //    .Join(
        //        _dbContext.Packages,
        //        join1 => join1.FamilyHead.PackageId,
        //        p => p.PackageId,
        //        (join1, p) => new { join1.FamilyMember, join1.FamilyHead, Package = p })
        //    .Join(
        //        _dbContext.Companies,
        //        join2 => join2.Package.CompanyId,
        //        c => c.CompanyId,
        //        (join2, c) => new { join2.FamilyMember, join2.FamilyHead, join2.Package, Company = c })
        //    .Where(  e =>  flightPackages.Contains(e.FamilyHead.PackageId) && !e.FamilyHead.Trashed 
        //            && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
        //    .OrderBy(joined => joined.FamilyMember.HeadId)
        //    .ThenBy(joined => joined.FamilyMember.FamilyHeadId)

        //    .Select(joined => new
        //    {
        //        joined.FamilyMember.FamilyHeadId,
        //        joined.FamilyMember.HeadId,
        //        joined.FamilyHead.GivenName,
        //        joined.FamilyHead.SurName,
        //        joined.FamilyHead.FatherHusbandName,
        //        joined.FamilyHead.MehramRelationEnum,
        //        joined.FamilyMember.IsHead,
        //        joined.Company.CompanyId,
        //        PackageName = joined.Package.Name,
        //        joined.FamilyHead.ReferenceId,
        //        joined.Company.CompanyName,
        //        joined.Company.CompanyCode,
        //        joined.FamilyHead.DateOfBirth,
        //        joined.FamilyHead.Gender,
        //        joined.FamilyHead.PassportNumber,
        //        joined.FamilyHead.PassportDateOfExpiry,
        //        joined.FamilyHead.FriendId,
        //        joined.FamilyHead.PackageId,
        //        joined.FamilyHead.IndId
        //    })
        //    .ToList();

        //    var selectedMembers = _dbContext.PNRMembers.Where(e => e.PNRId == id && e.FamilyHead.Trashed == false);

        //    int selected = _dbContext.PNRMembers
        //        .Where(e => e.PNRId == id && e.FamilyHead.Trashed == false)
        //        .Count();

        //    var familiesGroups = _dbContext.FamilyMembers
        //        .Include(e => e.FamilyHead)
        //        .Where(a => a.FamilyHead.Trashed == false)
        //        .Select(e => new
        //        {
        //            e.HeadId,
        //            e.FamilyHead.FlightId
        //        })
        //        .ToList();


        //    PNRAddMembersModel model = new PNRAddMembersModel(
        //        flight.Name,
        //        pnr.Name,
        //        pnr.Capacity,
        //        selected,
        //        id
        //        );

        //    bool sameFamily = true;
        //    dynamic prevMember = null;
        //    for (int i = 0, x = 0; i < members.Count; i++)
        //    {
        //        var currentMember = members.ElementAt(i);
        //        string memberName = currentMember.GivenName;
        //        string fatherHusbandName = currentMember.FatherHusbandName;

        //        if (currentMember.MehramRelationEnum == MehramRelationEnum.Father)
        //        {
        //            fatherHusbandName += " <b>(F)</b>";
        //        }
        //        else if (currentMember.MehramRelationEnum == MehramRelationEnum.Husband)
        //        {
        //            fatherHusbandName += " <b>(H)</b>";
        //        }

        //        if (currentMember.IsHead)
        //        {
        //            memberName += " <b>(FH)</b>";
        //        }

        //        if (i > 0)
        //        {
        //            prevMember = members.ElementAt(i - 1);
        //            if (prevMember.HeadId != currentMember.HeadId)
        //            {
        //                sameFamily = false;
        //            }
        //        }
        //        if (!sameFamily)
        //        {
        //            x++;
        //            sameFamily = true;
        //        }

        //        var memberInGroup = _dbContext.GroupMembers
        //            .Include(e => e.Group)
        //            .Where(a => a.FamilyHead.Trashed == false)
        //            .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

        //        int familyPax = familiesGroups
        //            .Where(e => e.HeadId == currentMember.HeadId)
        //            .Count();

        //        string cId =
        //                $"{currentMember.CompanyCode}" +
        //                $".{currentMember.IndId}" +
        //                $".{currentMember.HeadId}({familyPax})" +
        //                $".{currentMember.PackageName}";

        //        string groupNumber = "";
        //        if (memberInGroup != null)
        //        {
        //            cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
        //            groupNumber = memberInGroup?.Group?.Code ?? "-";
        //        }

        //        string megaId = "";
        //        if (currentMember.ReferenceId == 0)
        //        {
        //            megaId = "-";
        //        }
        //        else
        //        {
        //            currentMember.ReferenceId?.ToString();
        //        }

        //        bool currMemberSelected = selectedMembers
        //            .Where( a => a.FamilyHead.Trashed == false)
        //            .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId) != null;

        //        bool? inSamePnr = null;
        //        var _m = _dbContext.PNRMembers
        //            .Where(a => a.FamilyHead.Trashed == false)
        //            .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);


        //        if (_m is null)
        //        {
        //            inSamePnr = null;
        //        }
        //        else if (_m?.PNRId == id)
        //        {
        //            inSamePnr = true;
        //        }
        //        else if (_m?.PNRId != id)
        //        {
        //            inSamePnr = false;
        //        }

        //        model.Entries.Add(new PNRMemberModel(
        //            currentMember.CompanyCode, 
        //            currentMember.PackageName, 
        //            currentMember.ReferenceId,
        //            groupNumber, $"FC-{currentMember.FamilyHeadId}",
        //            cId, currentMember.GivenName+' '+currentMember.SurName,
        //            DateTimeManager.CalculateAge(currentMember.DateOfBirth),
        //            currentMember.Gender.ToString(),
        //            currentMember.PassportNumber, 
        //            DateTimeManager.GetDate(currentMember.PassportDateOfExpiry),
        //            currentMember.DateOfBirth.ToString("dd/MM/yyy"),
        //            currentMember.FriendId, 
        //            currMemberSelected,
        //            currentMember.IsHead, 
        //            currentMember.FamilyHeadId, 
        //            currentMember.HeadId, 
        //            inSamePnr
        //            ));
        //    }

        //    return View(model);
        //}

        //public IActionResult AddMembers(int id)
        //{
        //    var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

        //    PNR pnr = _dbContext.PNRs.Find(id);
        //    PNRPackage package = _dbContext.PNRPackages.Find(pnr.PNRId);
        //    Flight flight = _dbContext.Flights.Find(pnr.FlightId);

        //    var members = _dbContext.FamilyMembers
        //        .Join(
        //            _dbContext.FamilyHeads.Where(fh => fh.HajjSeasonsId == activeSeasonId && !fh.Trashed),
        //            fm => fm.FamilyHeadId,
        //            fh => fh.FamilyHeadId,
        //            (fm, fh) => new { FamilyMember = fm, FamilyHead = fh })
        //        .Join(
        //            _dbContext.Packages,
        //            join1 => join1.FamilyHead.PackageId,
        //            p => p.PackageId,
        //            (join1, p) => new { join1.FamilyMember, join1.FamilyHead, Package = p })
        //        .Join(
        //            _dbContext.Companies,
        //            join2 => join2.Package.CompanyId,
        //            c => c.CompanyId,
        //            (join2, c) => new { join2.FamilyMember, join2.FamilyHead, join2.Package, Company = c })
        //        .Where(e => e.FamilyMember.FamilyHead.PackageId == package.PackageId)
        //        .OrderBy(joined => joined.FamilyMember.HeadId)
        //        .ThenBy(joined => joined.FamilyMember.FamilyHeadId)
        //        .Select(joined => new
        //        {
        //            joined.FamilyMember.FamilyHeadId,
        //            joined.FamilyMember.HeadId,
        //            joined.FamilyHead.GivenName,
        //            joined.FamilyHead.SurName,
        //            joined.FamilyHead.FatherHusbandName,
        //            joined.FamilyHead.MehramRelationEnum,
        //            joined.FamilyMember.IsHead,
        //            joined.Company.CompanyId,
        //            PackageName = joined.Package.Name,
        //            joined.FamilyHead.ReferenceId,
        //            joined.Company.CompanyName,
        //            joined.Company.CompanyCode,
        //            joined.FamilyHead.DateOfBirth,
        //            joined.FamilyHead.Gender,
        //            joined.FamilyHead.PassportNumber,
        //            joined.FamilyHead.PassportDateOfExpiry,
        //            joined.FamilyHead.FriendId,
        //            joined.FamilyHead.PackageId,
        //            joined.FamilyHead.IndId
        //        })
        //        .ToList();

        //    var selectedMembers = _dbContext.PNRMembers
        //        .Where(e => e.PNRId == id && e.FamilyHead.Trashed == false && e.FamilyHead.HajjSeasonsId == activeSeasonId);

        //    int selected = selectedMembers.Count();

        //    var familiesGroups = _dbContext.FamilyMembers
        //        .Include(e => e.FamilyHead)
        //        .Where(a => a.FamilyHead.Trashed == false && a.FamilyHead.HajjSeasonsId == activeSeasonId)
        //        .Select(e => new
        //        {
        //            e.HeadId,
        //            e.FamilyHead.FlightId
        //        })
        //        .ToList();

        //    PNRAddMembersModel model = new PNRAddMembersModel(
        //        flight.Name,
        //        pnr.Name,
        //        pnr.Capacity,
        //        selected,
        //        id
        //    );

        //    bool sameFamily = true;
        //    dynamic prevMember = null;

        //    for (int i = 0, x = 0; i < members.Count; i++)
        //    {
        //        var currentMember = members.ElementAt(i);
        //        string memberName = currentMember.GivenName;
        //        string fatherHusbandName = currentMember.FatherHusbandName;

        //        if (currentMember.MehramRelationEnum == MehramRelationEnum.Father)
        //        {
        //            fatherHusbandName += " <b>(F)</b>";
        //        }
        //        else if (currentMember.MehramRelationEnum == MehramRelationEnum.Husband)
        //        {
        //            fatherHusbandName += " <b>(H)</b>";
        //        }

        //        if (currentMember.IsHead)
        //        {
        //            memberName += " <b>(FH)</b>";
        //        }

        //        if (i > 0)
        //        {
        //            prevMember = members.ElementAt(i - 1);
        //            if (prevMember.HeadId != currentMember.HeadId)
        //            {
        //                sameFamily = false;
        //            }
        //        }

        //        if (!sameFamily)
        //        {
        //            x++;
        //            sameFamily = true;
        //        }

        //        var memberInGroup = _dbContext.GroupMembers
        //            .Include(e => e.Group)
        //            .Where(a => a.FamilyHead.Trashed == false && a.FamilyHead.HajjSeasonsId == activeSeasonId)
        //            .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

        //        int familyPax = familiesGroups
        //            .Where(e => e.HeadId == currentMember.HeadId)
        //            .Count();

        //        string cId = $"{currentMember.CompanyCode}.{currentMember.IndId}.{currentMember.HeadId}({familyPax}).{currentMember.PackageName}";

        //        string groupNumber = "";
        //        if (memberInGroup != null)
        //        {
        //            cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
        //            groupNumber = memberInGroup?.Group?.Code ?? "-";
        //        }

        //        string megaId = currentMember.ReferenceId == 0 ? "-" : currentMember.ReferenceId?.ToString();

        //        bool currMemberSelected = selectedMembers
        //            .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId) != null;

        //        bool? inSamePnr = null;
        //        var _m = _dbContext.PNRMembers
        //            .Where(a => a.FamilyHead.Trashed == false && a.FamilyHead.HajjSeasonsId == activeSeasonId)
        //            .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

        //        if (_m is null)
        //        {
        //            inSamePnr = null;
        //        }
        //        else if (_m?.PNRId == id)
        //        {
        //            inSamePnr = true;
        //        }
        //        else
        //        {
        //            inSamePnr = false;
        //        }

        //        model.Entries.Add(new PNRMemberModel(
        //            currentMember.CompanyCode,
        //            currentMember.PackageName,
        //            currentMember.ReferenceId,
        //            groupNumber,
        //            $"FC-{currentMember.FamilyHeadId}",
        //            cId,
        //            currentMember.GivenName + ' ' + currentMember.SurName,
        //            DateTimeManager.CalculateAge(currentMember.DateOfBirth),
        //            currentMember.Gender.ToString(),
        //            currentMember.PassportNumber,
        //            DateTimeManager.GetDate(currentMember.PassportDateOfExpiry),
        //            currentMember.DateOfBirth.ToString("dd/MM/yyyy"),
        //            currentMember.FriendId,
        //            currMemberSelected,
        //            currentMember.IsHead,
        //            currentMember.FamilyHeadId,
        //            currentMember.HeadId,
        //            inSamePnr
        //        ));
        //    }

        //    return View(model);
        //}

        public IActionResult AddMembers(int id)
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            PNR pnr = _dbContext.PNRs.Find(id);
            var pnrPackageList = _dbContext.PNRPackages.Where(e => e.PNRId == pnr.PNRId).ToList();
            Flight flight = _dbContext.Flights.Find(pnr.FlightId);

            var members = new List<MemberInfoModel>();

            foreach (var package in pnrPackageList)
            {
                var packageMembers = _dbContext.FamilyMembers
                    .Join(_dbContext.FamilyHeads.Where(fh => fh.HajjSeasonsId == activeSeasonId && !fh.Trashed && fh.HajjiBookingStatus == HajjiBookingStatus.Booked),
                        fm => fm.FamilyHeadId,
                        fh => fh.FamilyHeadId,
                        (fm, fh) => new { FamilyMember = fm, FamilyHead = fh })
                    .Join(_dbContext.Packages,
                        join1 => join1.FamilyHead.PackageId,
                        p => p.PackageId,
                        (join1, p) => new { join1.FamilyMember, join1.FamilyHead, Package = p })
                    .Join(_dbContext.Companies,
                        join2 => join2.Package.CompanyId,
                        c => c.CompanyId,
                        (join2, c) => new { join2.FamilyMember, join2.FamilyHead, join2.Package, Company = c })
                    .Where(e => e.FamilyMember.FamilyHead.PackageId == package.PackageId)
                    .OrderBy(e => e.FamilyMember.HeadId)
                    .ThenBy(e => e.FamilyMember.FamilyHeadId)
                    .Select(e => new MemberInfoModel
                    {
                        FamilyHeadId = e.FamilyMember.FamilyHeadId,
                        HeadId = e.FamilyMember.HeadId,
                        GivenName = e.FamilyHead.GivenName,
                        SurName = e.FamilyHead.SurName,
                        FatherHusbandName = e.FamilyHead.FatherHusbandName,
                        MehramRelationEnum = e.FamilyHead.MehramRelationEnum,
                        IsHead = e.FamilyMember.IsHead,
                        CompanyId = e.Company.CompanyId,
                        PackageName = e.Package.Name,
                        ReferenceId = e.FamilyHead.ReferenceId,
                        CompanyName = e.Company.CompanyName,
                        CompanyCode = e.Company.CompanyCode,
                        DateOfBirth = e.FamilyHead.DateOfBirth,
                        Gender = e.FamilyHead.Gender,
                        PassportNumber = e.FamilyHead.PassportNumber,
                        PassportDateOfExpiry = e.FamilyHead.PassportDateOfExpiry,
                        PackageId = e.FamilyHead.PackageId,
                        IndId = e.FamilyHead.IndId,
                        HbNo = e.FamilyHead.FcId
                    })
                    .ToList();

                members.AddRange(packageMembers);
            }

            var selectedMembers = _dbContext.PNRMembers
                .Where(e => e.PNRId == id &&
                            e.FamilyHead.Trashed == false &&
                            e.FamilyHead.HajjSeasonsId == activeSeasonId &&
                            e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
                .ToList();

            int selected = selectedMembers.Count();

            var familiesGroups = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHead.Trashed == false &&
                            e.FamilyHead.HajjSeasonsId == activeSeasonId &&
                            e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
                .Select(e => new { e.HeadId, e.FamilyHead.FlightId })
                .ToList();

            PNRAddMembersModel model = new PNRAddMembersModel(
                flight.Name,
                pnr.Name,
                pnr.Capacity,
                selected,
                id
            );

            bool sameFamily = true;
            MemberInfoModel prevMember = null;

            for (int i = 0, x = 0; i < members.Count; i++)
            {
                var current = members[i];
                string memberName = current.GivenName;
                string fatherName = current.FatherHusbandName;

                if (current.MehramRelationEnum == MehramRelationEnum.Father)
                    fatherName += " <b>(F)</b>";
                else if (current.MehramRelationEnum == MehramRelationEnum.Husband)
                    fatherName += " <b>(H)</b>";

                if (current.IsHead)
                    memberName += " <b>(FH)</b>";

                if (i > 0)
                {
                    prevMember = members[i - 1];
                    if (prevMember.HeadId != current.HeadId)
                        sameFamily = false;
                }

                if (!sameFamily)
                {
                    x++;
                    sameFamily = true;
                }

                var groupInfo = _dbContext.GroupMembers
                    .Include(g => g.Group)
                    .FirstOrDefault(e => e.FamilyHead.Trashed == false &&
                                         e.FamilyHead.HajjSeasonsId == activeSeasonId &&
                                         e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked &&
                                         e.FamilyHeadId == current.FamilyHeadId);

                int familyPax = familiesGroups.Count(f => f.HeadId == current.HeadId);

                string cId = $"{current.CompanyCode}.{current.IndId}.{current.HeadId}({familyPax}).{current.PackageName}";

                string groupNumber = "-";
                if (groupInfo != null)
                {
                    cId += $".{groupInfo.Group.Name}.{groupInfo.GroupId}";
                    groupNumber = groupInfo.Group?.Code ?? "-";
                }

                string megaId = current.ReferenceId == 0 ? "-" : current.ReferenceId.ToString();

                bool isSelected = selectedMembers.Any(m => m.FamilyHeadId == current.FamilyHeadId);

                bool? inSamePnr = null;
                var _m = _dbContext.PNRMembers
                    .FirstOrDefault(e => e.FamilyHeadId == current.FamilyHeadId &&
                                         e.FamilyHead.Trashed == false &&
                                         e.FamilyHead.HajjSeasonsId == activeSeasonId &&
                                         e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked);

                inSamePnr = _m == null ? (bool?)null : _m.PNRId == id;

                model.Entries.Add(new PNRMemberModel(
                    current.CompanyCode,
                    current.PackageName,
                    current.ReferenceId,
                    groupNumber,
                    current.HbNo,
                    cId,
                    current.GivenName + " " + current.SurName,
                    DateTimeManager.CalculateAge(current.DateOfBirth),
                    current.Gender.ToString(),
                    current.PassportNumber,
                    DateTimeManager.GetDate(current.PassportDateOfExpiry),
                    current.DateOfBirth.ToString("dd/MM/yyyy"),
                    current.FriendId,
                    isSelected,
                    current.IsHead,
                    current.FamilyHeadId,
                    current.HeadId,
                    inSamePnr
                ));
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AddMember(int familyHeadId, int pnrId)
        {
            _dbContext.PNRMembers
                .Add(new PNRMembers
                {
                    FamilyHeadId = familyHeadId,
                    PNRId = pnrId
                });
            _dbContext.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Added"
            });
        }

        public IActionResult Trash(int id)
        {
            PNR pnr = _dbContext.PNRs.Find(id);
            if (pnr == null) return RedirectToAction(nameof(Index));

            pnr.Trashed = true;

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }
        public IActionResult ViewTrashPNR()
        {
            var trashedPNRs = _dbContext.PNRs
                .Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            return View(trashedPNRs);
        }

        public IActionResult Restore(int id)
        {
            PNR pnr = _dbContext.PNRs.Find(id);
            pnr.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveMember(int familyHeadId, int pnrId)
        {
            PNRMembers pnrMember = _dbContext.PNRMembers
                .FirstOrDefault(e => e.FamilyHeadId == familyHeadId && e.PNRId == pnrId);

            _dbContext.PNRMembers
                .Remove(pnrMember);

            _dbContext.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Removed",
                pnrMember
            });
        }

        public JsonResult GetRemainingCapacity(int flightId, int typeId)
        {
            int alloted = _dbContext.PNRs
                .Where(e => e.FlightId == flightId && (int)e.Type == typeId)
                .Sum(e => e.Capacity);

            Flight flight = _dbContext.Flights.FirstOrDefault(e => e.FlightId == flightId);

            int remainingCapacity = 0;
            if (typeId == (int)PNRType.Economy)
            {
                remainingCapacity = flight.EconomySeats - alloted;
            }
            else if (typeId == (int)PNRType.Business)
            {
                remainingCapacity = flight.BusinessSeats - alloted;
            }

            return Json(remainingCapacity);
        }

        public IActionResult Print(int id)
        {
            var query = (from pm in _dbContext.PNRMembers
                         join fh in _dbContext.FamilyHeads on pm.FamilyHeadId equals fh.FamilyHeadId
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         //join gm in _dbContext.GroupMembers on pm.FamilyHeadId equals gm.FamilyHeadId into gmJoin
                         //from gm in gmJoin.DefaultIfEmpty()
                         //join g in _dbContext.GroupsWithoutTrash on gm.GroupId equals g.GroupId into gJoin
                         //from g in gJoin.DefaultIfEmpty()
                         //join pt in _dbContext.POTickets on fh.FamilyHeadId equals pt.FamilyMembersId into ptJoin
                         //from pt in ptJoin.DefaultIfEmpty()
                         //join pfd in _dbContext.POFlightDetails on pt.POTicketId equals pfd.POTicketId into pfdJoin
                         //from pfd in pfdJoin.DefaultIfEmpty()
                         where pm.PNRId == id
                         select new
                         {
                             PNRMember = pm,
                             FamilyHead = fh,
                             Package = p,
                             //GroupMember = gm,
                             //Group = g,
                             //POTicket = pt,
                             //POFlightDetail = pfd,
                         });

            var result = query.Select(e => new PNRData
            {
                HajjiName = e.FamilyHead.FullName,
                HBNo = e.FamilyHead.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.FamilyHead.PassportNumber,
                Gender = e.FamilyHead.Gender.ToString(),
                Honorifics = e.FamilyHead.Honorifics.Title,
                DOB = e.FamilyHead.DateOfBirth.ToString("dd/MMM/yyyy"),
                PassportDOE = e.FamilyHead.PassportDateOfExpiry != null
                      ? e.FamilyHead.PassportDateOfExpiry.Value.ToString("dd/MMM/yyyy")
                      : "N/A",
                GroupName = _dbContext.GroupMembers
                    .Include(gm => gm.Group)
                    .Where(gm => gm.FamilyHeadId == e.FamilyHead.FamilyHeadId && !gm.Group.Trashed)
                    .Select(gm => gm.Group.Name)
                    .FirstOrDefault(),

                PNRNo = _dbContext.POFlightDetails
                        .Include(pt => pt.POTicket)
                        .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHead.FamilyHeadId)
                        .Select(f => f.PNRNo)
                        .FirstOrDefault(),

                TicketNo = _dbContext.POFlightDetails
                        .Include(pt => pt.POTicket)
                        .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHead.FamilyHeadId
                                && !pt.POTicket.Trashed)
                        .Select(f => f.TicketNumber)
                        .FirstOrDefault(),

                TicketImage = _dbContext.POFlightDetails
                        .Include(pt => pt.POTicket)
                        .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHead.FamilyHeadId
                                && !pt.POTicket.Trashed)
                        .Select(f => f.TicketImages)
                        .FirstOrDefault()
            }).ToList();

            return View(result);
        }
    }
}

namespace TMG.Models.AirlineModels
{
    public record PNRData
    {
        public string Honorifics { get; set; }
        public string GroupName { get; set; }
        public string DOB { get; set; }
        public string PackageName { get; set; }
        public string CompanyName { get; set; }
        public string HajjiName { get; set; }
        public string HBNo { get; set; }
        public string Gender { get; set; }
        public string PassportNo { get; set; }
        public string PassportDOE { get; set; }
        public string TicketNo { get; set; }
        public string PNRNo { get; set; }
        public string TicketImage { get; set; }
        public string Coach { get; set; }
        public string SeatNo { get; set; }
    }
    public record PNRIndexModel
    {
        public List<PNRIndexEntryModel> Entries { get; set; } = new List<PNRIndexEntryModel>();
    }
    public record PNRIndexEntryModel(int PNRId,
        string Name, string Number, string Type,
        string FlightName, string PNRType, int Capacity, int Booked, int Balance);

    public record PNRAddMembersModel(string Flight, string PNR, int PAX, int Selected,
        //string DepartureArrivalDateTime,
        int PNRId)
    {
        public List<PNRMemberModel> Entries { get; set; } = new List<PNRMemberModel>();
    }

    public record PNRMemberModel(string Company, string Package, int? MegaId, string Group,
        string FCId, string CId, string Name, int Age, string Gender, string Passport, string PassportExpiry, string DOB,
        int? FriendId, bool Selected, bool IsHead, int FamilyHeadId, int HeadId, bool? InSamePnr);

    public class MemberInfoModel
    {
        public int FamilyHeadId { get; set; }
        public int HeadId { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string FatherHusbandName { get; set; }
        public MehramRelationEnum MehramRelationEnum { get; set; }
        public bool IsHead { get; set; }
        public int CompanyId { get; set; }
        public string PackageName { get; set; }
        public int? ReferenceId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportDateOfExpiry { get; set; }
        public int FriendId { get; set; }
        public int PackageId { get; set; }
        public string IndId { get; set; }
        public string HbNo { get; set; }
    }



}
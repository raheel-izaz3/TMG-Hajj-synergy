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
    public class MinaController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public MinaController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Minas
                .Where(ca => ca.Trashed && ca.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count();

            var pnrs = _dbContext
                .MinasWithoutTrash
                .Include(e => e.Flight)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            var pnrMembers = _dbContext
                .MinaMembers
                .Include(e => e.Mina)
                .Where(e => !e.Mina.Trashed && e.Mina.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            MinaIndexModel model = new MinaIndexModel();

            for (int i = 0; i < pnrs.Count; i++)
            {
                Mina currPnr = pnrs.ElementAt(i);
                int booked = pnrMembers.Where(e => e.MinaId == currPnr.MinaId).Count();
                int balance = currPnr.Capacity - booked;

                model.Entries.Add(
                    new MinaIndexEntryModel(
                        currPnr.MinaId, currPnr.Name, currPnr.Code, currPnr.CampNumber, currPnr.CheckIn?.ToString("dd/MMM/yyyy"), currPnr.CheckOut?.ToString("dd/MMM/yyyy"),
                        currPnr.Capacity, booked, balance
                        )
                    );
            }

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.CompanyVisa = _dbContext.CompanyVisas
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && !e.Trashed)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Mina pnr)
        {
            ViewBag.Flights = _dbContext.FlightsWithoutTrash;

            ViewBag.NewFlights = _dbContext.POTicketFlightDatas
             .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
             .ToList();

            ViewBag.Airline = _dbContext.RailwaysWithoutTrash;

            ViewBag.Packages = _dbContext.PackagesWithoutTrash
               .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
               .ToList();

            pnr.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            pnr.FlightId = 1;


            _dbContext.Minas.Add(pnr);
            _dbContext.SaveChanges();

            for (int i = 0; i < (pnr.PackageIds?.Count ?? 0); i++)
            {
                if (pnr.PackagesSelected[i] == false) continue;

                _dbContext.MinaPackages.Add(new MinaPackage
                {
                    PackageId = pnr.PackageIds[i],
                    MinaId = pnr.MinaId
                });
            }
            _dbContext.SaveChanges();

            TempData["created"] = true;
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Mina pnr = _dbContext.Minas
                .FirstOrDefault(e => e.MinaId == id);

            ViewBag.CurrentCapacity = pnr.Capacity;
            int booked = _dbContext.MinaMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.MinaId == id && !e.FamilyHead.Trashed)
                .Count();
            ViewBag.Booked = booked;
            ViewBag.Balance = pnr.Capacity - booked;

            ViewBag.Packages = _dbContext.PackagesWithoutTrash
               .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
               .ToList();

            ViewBag.MinaPackages = _dbContext.MinaPackages.Where(e => e.MinaId == id);

            ViewBag.CompanyVisa = _dbContext.CompanyVisas
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && !e.Trashed)
                .ToList();

            return View(pnr);
        }

        [HttpPost]
        public IActionResult Edit(Mina pnr)
        {
            Mina pnrToEdit = _dbContext.Minas
                .Include(e => e.Flight)
                .FirstOrDefault(e => e.MinaId == pnr.MinaId);

            ViewBag.CurrentCapacity = pnr.Capacity;
            int booked = _dbContext.MinaMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.MinaId == pnr.MinaId && !e.FamilyHead.Trashed)
                .Count();
            ViewBag.Booked = booked;
            ViewBag.Balance = pnr.Capacity - booked;

            ViewBag.EconomySeats = pnr.Flight?.EconomySeats;
            ViewBag.BusinessSeats = pnr.Flight?.BusinessSeats;

            if (!ModelState.IsValid) return View(pnr);

            pnrToEdit.FlightId = 1;
            pnrToEdit.Name = pnr.Name;
            pnrToEdit.Code = pnr.Code;
            pnrToEdit.Capacity = pnr.Capacity;
            pnrToEdit.CampNumber = pnr.CampNumber;
            pnrToEdit.CheckIn = pnr.CheckIn;
            pnrToEdit.CheckOut = pnr.CheckOut;
            pnrToEdit.CompanyVisasId = pnr.CompanyVisasId;
            pnrToEdit.Gender = pnr.Gender;
            pnrToEdit.Price = pnr.Price;
            pnrToEdit.UpdatedAt = DateTime.Now;
            pnrToEdit.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            _dbContext.SaveChanges();

            _dbContext.MinaPackages.Where(e => e.MinaId == pnr.MinaId).ExecuteDelete();

            for (int i = 0; i < (pnr.PackageIds?.Count ?? 0); i++)
            {
                if (pnr.PackagesSelected[i] == false) continue;

                _dbContext.MinaPackages.Add(new MinaPackage
                {
                    PackageId = pnr.PackageIds[i],
                    MinaId = pnr.MinaId
                });
            }
            _dbContext.SaveChanges();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddMembers(int id)
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            Mina pnr = _dbContext.Minas.Find(id);
            var pnrPackageList = _dbContext.MinaPackages.Where(e => e.MinaId == pnr.MinaId).ToList();
            Flight flight = _dbContext.Flights.Find(pnr.FlightId);

            var members = new List<MemberInfoModel>();

            foreach (var package in pnrPackageList)
            {
                var packageMembers = _dbContext.FamilyMembers
                    .Join(
                        _dbContext.FamilyHeads.Where(fh =>
                            fh.HajjSeasonsId == activeSeasonId &&
                            !fh.Trashed &&
                            fh.HajjiBookingStatus == HajjiBookingStatus.Booked &&
                            fh.CompanyVisasId == pnr.CompanyVisasId &&             
                            fh.Gender == pnr.Gender                                
                        ),
                        fm => fm.FamilyHeadId,
                        fh => fh.FamilyHeadId,
                        (fm, fh) => new { FamilyMember = fm, FamilyHead = fh }
                    )
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

            var selectedMembers = _dbContext.MinaMembers
                .Where(e => e.MinaId == id &&
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
                var _m = _dbContext.MinaMembers
                    .FirstOrDefault(e => e.FamilyHeadId == current.FamilyHeadId &&
                                         e.FamilyHead.Trashed == false &&
                                         e.FamilyHead.HajjSeasonsId == activeSeasonId &&
                                         e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked);

                inSamePnr = _m == null ? (bool?)null : _m.MinaId == id;

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
            _dbContext.MinaMembers
                .Add(new MinaMember
                {
                    FamilyHeadId = familyHeadId,
                    MinaId = pnrId
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
            Mina pnr = _dbContext.Minas.Find(id);
            if (pnr == null) return RedirectToAction(nameof(Index));

            pnr.Trashed = true;

            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));
        }
        public IActionResult ViewTrashPNR()
        {
            var trashedMinas = _dbContext.Minas
                .Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            return View(trashedMinas);
        }

        public IActionResult Restore(int id)
        {
            Mina pnr = _dbContext.Minas.Find(id);
            pnr.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveMember(int familyHeadId, int pnrId)
        {
            MinaMember pnrMember = _dbContext.MinaMembers
                .FirstOrDefault(e => e.FamilyHeadId == familyHeadId && e.MinaId == pnrId);

            _dbContext.MinaMembers
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
            int alloted = _dbContext.Minas
                .Where(e => e.FlightId == flightId)
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
            ViewBag.Departure = _dbContext.MinasWithoutTrash
                .Where(e => e.MinaId == id)
                .Select(e => e.CheckIn)
                .FirstOrDefault();

            ViewBag.Arrival = _dbContext.MinasWithoutTrash
                .Where(e => e.MinaId == id)
                .Select(e => e.CheckOut)
                .FirstOrDefault();

            ViewBag.TrainNumber = _dbContext.MinasWithoutTrash
                .Where(e => e.MinaId == id)
                .Select(e => e.CampNumber)
                .FirstOrDefault();

            ViewBag.Name = _dbContext.MinasWithoutTrash
                .Where(e => e.MinaId == id)
                .Select(e => e.Name)
                .FirstOrDefault();

            ViewBag.Number = _dbContext.MinasWithoutTrash
               .Where(e => e.MinaId == id)
               .Select(e => e.Code)
               .FirstOrDefault();

            ViewBag.Price = _dbContext.MinasWithoutTrash
               .Where(e => e.MinaId == id)
               .Select(e => e.Price)
               .FirstOrDefault();

            var query = (from mm in _dbContext.MinaMembers
                         join fh in _dbContext.FamilyHeads on mm.FamilyHeadId equals fh.FamilyHeadId
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         where mm.MinaId == id
                         select new
                         {
                             MinaMember = mm,
                             FamilyHead = fh,
                             Package = p,
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
            }).ToList();

            return View(result);
        }
    }
}



namespace TMG.Models.AirlineModels
{
    public record MinaIndexModel
    {
        public List<MinaIndexEntryModel> Entries { get; set; } = new List<MinaIndexEntryModel>();
    }
    public record MinaIndexEntryModel(int MinaId,
        string Name, string Code, string CampNumber, string CheckIn, string CheckOut, int Capacity, int Booked, int Balance);
}
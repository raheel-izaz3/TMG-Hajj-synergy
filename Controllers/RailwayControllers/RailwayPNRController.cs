using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.RailwayModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.RailwayControllers
{
    public class RailwayPNRController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public RailwayPNRController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Edit(int id)
        {
            RailwayPNR pnr = _dbContext.RailwayPNRs
                .Include(e => e.RailwayTravel)
                .FirstOrDefault(e => e.RailwayPNRId == id);

            ViewBag.CurrentCapacity = pnr?.Capacity;
            int booked = _dbContext.RailwayPNRMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.RailwayPNRId == id && !e.FamilyHead.Trashed)
                .Count();
            ViewBag.Booked = booked;
            ViewBag.Balance = pnr?.Capacity - booked;

            ViewBag.EconomySeats = pnr?.RailwayTravel.EconomySeats;
            ViewBag.BusinessSeats = pnr?.RailwayTravel.BusinessSeats;

            return View(pnr);
        }

        [HttpPost]
        public IActionResult Edit(RailwayPNR pnr)
        {
            RailwayPNR _pnr = _dbContext.RailwayPNRs
                .Include(e => e.RailwayTravel)
                .FirstOrDefault(e => e.RailwayPNRId == pnr.RailwayPNRId);

            ViewBag.CurrentCapacity = pnr.Capacity;
            int booked = _dbContext.RailwayPNRMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.RailwayPNRId == pnr.RailwayPNRId && !e.FamilyHead.Trashed)
                .Count();
            ViewBag.Booked = booked;
            ViewBag.Balance = pnr.Capacity - booked;

            ViewBag.EconomySeats = _pnr.RailwayTravel.EconomySeats;
            ViewBag.BusinessSeats = _pnr.RailwayTravel.BusinessSeats;

            if (!ModelState.IsValid) return View(pnr);

            _pnr.Type = pnr.Type;
            _pnr.Name = pnr.Name;
            _pnr.Capacity = pnr.Capacity;
            _pnr.UpdatedAt = DateTime.Now;

            _dbContext.SaveChanges();

            return RedirectToAction("PNRList", "RailwayTravel", new { id = pnr.RailwayTravelId });
        }


        public IActionResult AddMembers(int id)
        {
            RailwayPNR pnr = _dbContext.RailwayPNRs.Find(id);
            RailwayTravel travel = _dbContext.RailwayTravels.Find(pnr.RailwayTravelId);
            var travelPackages = _dbContext.RailwayTravelPackages.Where(e => e.RailwayTravelId == pnr.RailwayTravelId).Select(e => e.PackageId);

            var members = _dbContext.FamilyMembers
            .Join(
                _dbContext.FamilyHeads,
                fm => fm.FamilyHeadId,
                fh => fh.FamilyHeadId,
                (fm, fh) => new { FamilyMember = fm, FamilyHead = fh })
            .Join(
                _dbContext.Packages,
                join1 => join1.FamilyHead.PackageId,
                p => p.PackageId,
                (join1, p) => new { join1.FamilyMember, join1.FamilyHead, Package = p })
            .Join(
                _dbContext.Companies,
                join2 => join2.Package.CompanyId,
                c => c.CompanyId,
                (join2, c) => new { join2.FamilyMember, join2.FamilyHead, join2.Package, Company = c })
                .Where(e => travelPackages.Contains(e.Package.PackageId) && !e.FamilyHead.Trashed)
            // .Join(
            //     _dbContext.RailwayTravelPackages,
            //     join3 => join3.Package.PackageId,
            //     rtp => rtp.PackageId,
            //     (join3, rtp) => new
            //     {
            //         join3.FamilyMember,
            //         join3.FamilyHead,
            //         join3.Package,
            //         join3.Company,
            //         RailwayTravelPackage = rtp
            //     })
            .OrderBy(joined => joined.FamilyMember.HeadId)
            .ThenBy(joined => joined.FamilyMember.FamilyHeadId) // Add additional ordering if needed
            .Select(joined => new
            {
                joined.FamilyMember.FamilyHeadId,
                joined.FamilyMember.HeadId,
                joined.FamilyHead.GivenName,
                joined.FamilyHead.SurName,
                joined.FamilyHead.FatherHusbandName,
                joined.FamilyHead.MehramRelationEnum,
                joined.FamilyMember.IsHead,
                joined.Company.CompanyId,
                PackageName = joined.Package.Name,
                joined.FamilyHead.ReferenceId,
                joined.Company.CompanyName,
                joined.Company.CompanyCode,
                joined.FamilyHead.DateOfBirth,
                joined.FamilyHead.Gender,
                joined.FamilyHead.PassportNumber,
                joined.FamilyHead.PassportDateOfExpiry,
                joined.FamilyHead.FriendId,
                joined.FamilyHead.PackageId,
                joined.FamilyHead.IndId
            })
            .ToList();


            var selectedMembers = _dbContext.RailwayPNRMembers.Where(e => e.RailwayPNRId == id);

            int selected = _dbContext.RailwayPNRMembers
                .Where(e => e.RailwayPNRId == id)
                .Count();

            var familiesGroups = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Select(e => new
                {
                    e.HeadId,
                    e.FamilyHead.FlightId
                })
                .ToList();

            RailwayPNRAddMembersModel model = new RailwayPNRAddMembersModel(
                travel.Name,
                pnr.Name,
                pnr.Capacity,
                selected,
                $"{travel.OutboundDepartureDateTime} - {travel.OutboundArrivalDateTime}",
                id
                );

            bool sameFamily = true;
            dynamic prevMember = null;
            for (int i = 0, x = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                string memberName = currentMember.GivenName;
                string fatherHusbandName = currentMember.FatherHusbandName;

                if (currentMember.MehramRelationEnum == MehramRelationEnum.Father)
                {
                    fatherHusbandName += " <b>(F)</b>";
                }
                else if (currentMember.MehramRelationEnum == MehramRelationEnum.Husband)
                {
                    fatherHusbandName += " <b>(H)</b>";
                }

                if (currentMember.IsHead)
                {
                    memberName += " <b>(FH)</b>";
                }

                if (i > 0)
                {
                    prevMember = members.ElementAt(i - 1);
                    if (prevMember.HeadId != currentMember.HeadId)
                    {
                        sameFamily = false;
                    }
                }
                if (!sameFamily)
                {
                    x++;
                    sameFamily = true;
                }

                var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                int familyPax = familiesGroups
                    .Where(e => e.HeadId == currentMember.HeadId)
                    .Count();

                string cId =
                        $"{currentMember.CompanyCode}" +
                        $".{currentMember.IndId}" +
                        $".{currentMember.HeadId}({familyPax})" +
                        $".{currentMember.PackageName}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }


                bool currMemberSelected = selectedMembers
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId) != null;

                bool? inSamePnr = null;
                var _m = _dbContext.RailwayPNRMembers
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);


                if (_m is null)
                {
                    inSamePnr = null;
                }
                else if (_m?.RailwayPNRId == id)
                {
                    inSamePnr = true;
                }
                else if (_m?.RailwayPNRId != id)
                {
                    inSamePnr = false;
                }

                model.Entries.Add(new RailwayPNRMemberModel(
                    currentMember.CompanyCode, currentMember.PackageName, currentMember.ReferenceId,
                    groupNumber, $"HB-{currentMember.FamilyHeadId}", cId, currentMember.GivenName+' '+currentMember.SurName,
                    DateTimeManager.CalculateAge(currentMember.DateOfBirth), currentMember.Gender.ToString(),
                    currentMember.PassportNumber, DateTimeManager.GetDate(currentMember.PassportDateOfExpiry),
                    currentMember.FriendId, currMemberSelected,
                    currentMember.IsHead, currentMember.FamilyHeadId, currentMember.HeadId, inSamePnr));
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AddMember(int familyHeadId, int pnrId)
        {
            _dbContext.RailwayPNRMembers.Add(new RailwayPNRMember
            {
                FamilyHeadId = familyHeadId,
                RailwayPNRId = pnrId
            });
            _dbContext.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Added"
            });
        }

        [HttpPost]
        public IActionResult RemoveMember(int familyHeadId, int pnrId)
        {
            RailwayPNRMember pnrMember = _dbContext.RailwayPNRMembers
                .FirstOrDefault(e => e.FamilyHeadId == familyHeadId && e.RailwayPNRId == pnrId);

            _dbContext.RailwayPNRMembers
                .Remove(pnrMember);

            _dbContext.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "Removed",
                pnrMember
            });
        }
    }
}

namespace TMG.Models.RailwayModels
{
    public record RailwayPNRAddMembersModel(string Railway, string PNR, int PAX, int Selected,
        string DepartureArrivalDateTime,
        int PNRId)
    {
        public List<RailwayPNRMemberModel> Entries { get; set; } = new List<RailwayPNRMemberModel>();
    }

    public record RailwayPNRMemberModel(string Company, string Package, int? MegaId, string Group,
        string FCId, string CId, string Name, int Age, string Gender, string Passport, string PassportExpiry,
        int? FriendId, bool Selected, bool IsHead, int FamilyHeadId, int HeadId, bool? InSamePnr);
}
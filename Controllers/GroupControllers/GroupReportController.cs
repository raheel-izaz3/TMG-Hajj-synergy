using Humanizer;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.PackageModels;
using TMG.Models.ReservationModels;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System;
using iText.Bouncycastle.Crypto;
using TMG.Models.QurbaniModels;
using System.Threading.Tasks.Dataflow;
using TMG.Models.FamilyHeadReportModels;
//using static SkiaSharp.HarfBuzz.SKShaper;
//using AspNetCore;


namespace TMG.Models.GroupModels
{
    public record GroupReportModel
    {

        public int PackageId { get; set; }
        public string CompanyName { get; set; }
        public string PackageName { get; set; }
        public string PackageCode { get; set; }
        public string PackageDays { get; set; }
        public string PackageDuration { get; set; }
        public string MedinahArrival { get; set; }
        public int Quota { get; set; }
        public int TotalEHajj { get; set; }
        public int totalpax { get; set; }
        public int totalmale { get; set; }
        public int totalfemale { get; set; }
        public int totalchild { get; set; }
        public int totalinfant { get; set; }
        public int totalregular { get; set; }
        public int totalsponser { get; set; }
        public int totalbiostatus { get; set; }
        public int totalagreementsign { get; set; }
        public int totaldataformsign { get; set; }

        public List<FamilyMemberModel> FamilyMembers { get; set; }
        public List<AirlineSummaryReportModel> AirlineSummary { get; set; }
        public List<TrainSummaryModel> TrainSummary { get; set; }
        public GroupSummaryModel PackageSummary { get; set; }
        public RoomCalculation RoomCalculation { get; set; } = new();
    }
    public record GroupSummaryModel
    {
        public int FamiliesCount { get; set; }
        public int MembersCount { get; set; }
        public int MalesCount { get; set; }
        public int FemalesCount { get; set; }

        public int AdultsCount { get; set; }
        public int ChildsCount { get; set; }
        public int InfantsCount { get; set; }

        public int AdultMales { get; set; }
        public int AdultFemales { get; set; }

        public int ChildMales { get; set; }
        public int ChildFemales { get; set; }

        public int InfantMales { get; set; }
        public int InfantFemales { get; set; }
    }

    public record GroupUnAllocatedMembersModel
    {
        public string FcId { get; set; }
        public string CID { get; set; }
        public string GroupName { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string RoomType { get; set; }
        public int SrNo { get; set; }
    }
}
namespace TMG.Controllers.GroupControllers
{
    public class GroupReportController : Controller
    {

        public static int GroupId = 0;
        public static int HotelId = 0;
        private readonly TMGDbContext _context;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public GroupReportController(TMGDbContext context, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _context = context;
            _host = host;
            _http = http;
        }

        public IActionResult UnAllocatedMember()
        {
            List<GroupUnAllocatedMembersModel> model = new();

            var members = _context.GroupMembers
                            .Select(e => new
                            {
                                FHTrashed = e.FamilyHead.Trashed,
                                GroupTrashed = e.Group.Trashed,
                                e.FamilyHead,
                                e.FamilyHeadId,
                                e.GroupId,
                                e.FamilyHead.FcId,
                                e.FamilyHead.FullName,
                                e.FamilyHead.Gender,
                                e.Group.Name,
                                e.FamilyHead.Package.Company.CompanyCode,
                                PackageName = e.FamilyHead.Package.Name,
                                e.FamilyHead.IndId,
                                e.FamilyHead.ExtractedFamilyNumber

                            })
                            .Where(e => !e.FHTrashed && !e.GroupTrashed
                                    && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .AsNoTracking()
                            .ToList();

            int i = 1;
            foreach (var m in members)
            {
                if (_context.ReservationFamilyMembers
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.ReservationFamilyMembersId,
                        e.FloorRoomId
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId)?.FloorRoomId != null)
                {
                    continue;
                }

                GroupUnAllocatedMembersModel _ = new()
                {
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    SrNo = i++,
                    GroupName = m.Name,
                };

                int headId = _context.FamilyMembers
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.HeadId
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId).HeadId;
                int familyPax = _context.FamilyMembers
                    .Select(e => new
                    {
                        e.HeadId
                    })
                    .AsNoTracking()
                    .Where(e => e.HeadId == headId).Count();
                string cId =
                        $"{m.CompanyCode}.{m.IndId}" +
                        $".{m?.ExtractedFamilyNumber}({familyPax})" +
                        $".{m.Name}";

                var memberInGroup = _context.GroupMembers
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Group.Name,
                        e.GroupId,
                        e.Group.Trashed
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && !e.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Name}.{memberInGroup.GroupId}";
                }


                _.CID = cId;
                _.RoomType = _context.FamilyHeadAdditionalFacilities
                                .Select(e => new
                                {
                                    e.FamilyHeadId,
                                    e.AdditionalFacilitiesId,
                                    e.AdditionalFacilities.PlaceId,
                                    e.AdditionalFacilities.Room.RoomCode
                                })
                                .AsNoTracking()
                                .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId
                                && e.PlaceId == 4)?
                                .RoomCode ?? "Sharing";

                model.Add(_);

            }


            // return Ok(model);
            return View(model);
        }

        public IActionResult Report(int id, int Groupid)
        {
            Package package = _context.Packages.Find(id);

            GroupReportModel model = new GroupReportModel
            {
                PackageId = id,
                FamilyMembers = new List<FamilyMemberModel>(),
                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                //Quota = package.Quota
            };

            //var members = _context.FamilyMembers
            //    .Include(e => e.Head)
            //    .Include(e => e.FamilyHead)
            //    .ThenInclude(e => e.FamilyHeadRelation)
            //    .Include(e => e.FamilyHead)
            //    .ThenInclude(e => e.Package)
            //    .ThenInclude(e => e.Company)
            //    .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
            //    .OrderBy(e => e.HeadId)
            //    .ToList();

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.Package)
                        .ThenInclude(e => e.Company)
                .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.CompanyVisas)
                .Join(
                    _context.GroupMembers,
                    f => f.FamilyHeadId,
                    gm => gm.FamilyHeadId,
                    (f, gm) => new { FamilyMember = f, GroupMember = gm }
                )
                .Where(joinResult => joinResult.FamilyMember.FamilyHead.Trashed == false
                                  && joinResult.GroupMember.GroupId == Groupid)
                .OrderBy(joinResult => joinResult.FamilyMember.HeadId)
                .Select(joinResult => joinResult.FamilyMember)
                .ToList();

            var familiesGroups = _context.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .GroupBy(e => e.HeadId)
                .ToList();

            var maleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Male).Count();
            var femaleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Female).Count();

            model.FamilyMembers = new List<FamilyMemberModel>();

            //return Ok(familiesGroups);
            bool sameFamily = true;
            FamilyMembers prevMember = null;

            for (int i = 0, x = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                string memberName = currentMember.FamilyHead.GivenName + " " + currentMember.FamilyHead.SurName;
                string mehramName = "-";
                string mehramRel = "-";
                string fatherHusbandName = currentMember.FamilyHead.FatherHusbandName;

                if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Father)
                {
                    fatherHusbandName += " <b>(F)</b>";
                }
                else if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Husband)
                {
                    fatherHusbandName += " <b>(H)</b>";
                }

                if (currentMember.IsHead)
                {
                    memberName += " <b>(FH)</b>";
                }
                else
                {
                    if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        mehramName = currentMember.Head.GivenName;
                        mehramRel = currentMember.FamilyHead?.FamilyHeadRelation?.Relation ?? "-";
                    }
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

                // Ensure the index x is within the range of familiesGroups
                int familyPax = (x < familiesGroups.Count) ? familiesGroups.ElementAt(x).Count() : 0;

                var memberInGroup = _context.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                string cId =
                    $"{currentMember.FamilyHead.Package.Company.CompanyCode}.{currentMember.FamilyHead.IndId}" +
                    $".{currentMember.Head.ExtractedFamilyNumber}({familyPax})" +
                    $".{currentMember.FamilyHead.Package.Name}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                    model.PackageName = memberInGroup.Group.Name;
                    model.PackageCode = memberInGroup.Group.Code;
                }

                string megaId = currentMember.FamilyHead.ReferenceId == 0 ? "-" : currentMember.FamilyHead.ReferenceId?.ToString();

                // Main Contact & Address
                var memberContact = _context.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _context.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                // Makkah Accommodation
                /*var makkahAccommodation = _context.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 1);

                // Medina Accommodation
                var medinahAccommodation = _context.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 2);

                // Mina Accommodation
                var minaAccommodation = _context.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 3);

                // Azizia Accommodation
                var aziziaAccommodation = _context.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 4);*/

                int? accountId = currentMember.Head.AccountId;

                PaymentVoucher voucher = _context.PaymentVouchersWithoutTrash
                    .FirstOrDefault(e => e.AccountId == accountId);

                float receivableUsd = (voucher != null && voucher.USDROE != 0) ? voucher.Amount / voucher.USDROE : 0;

                var invoices = _context.InvoicesWithoutTrash
                    .Where(e => e.FamilyHeadId == currentMember.HeadId)
                    .Select(e => new { e.InvoiceId, e.FamilyHeadId });

                string invoicesString = string.Join(", ", invoices.Select(e => "R#" + e.InvoiceId));

                if (!string.IsNullOrEmpty(groupNumber)/* && memberInGroup.GroupId == Groupid*/)
                {
                    model.FamilyMembers.Add(
                        new FamilyMemberModel
                        {
                            FamilyHeadId = currentMember.FamilyHeadId,
                            MegaId = megaId,
                            EHajjId = currentMember.FamilyHead.EHajjId,
                            MOFA = currentMember.FamilyHead.MofaNumber,
                            FID = currentMember.Head.FcId,
                            CID = cId,
                            GNumber = groupNumber,
                            FormNumber = currentMember.FamilyHead.FormNumber,
                            Image = currentMember.FamilyHead.MemberImage,
                            PassportImage = currentMember.FamilyHead.PassportImage,
                            Name = memberName,
                            FatherHusbandName = fatherHusbandName,
                            Mehram = mehramName,
                            MehramRelation = mehramRel,
                            MobileNumber = memberContact?.Contact,
                            Address = memberAddress?.Address,
                            Age = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth),
                            Gender = currentMember.FamilyHead.Gender.ToString(),
                            HajjSerialNumber = currentMember.FamilyHead.HajjAppSerialNumber,
                            VisaNumber = currentMember?.FamilyHead.VisaNumber,
                            TicketNumber = currentMember?.FamilyHead.TicketNumber,
                            PassportNumber = currentMember.FamilyHead.PassportNumber,
                            HajjAppNumber = currentMember.FamilyHead.HajjApplicationNumber,
                            FileNumber = currentMember.FamilyHead.FileNumber,
                            //Makkah = makkahAccommodation?.Room?.RoomCode,
                            //Medinah = medinahAccommodation?.Room?.RoomCode,
                            //Azizia = aziziaAccommodation?.Room?.RoomCode,
                            //Mina = minaAccommodation?.Room?.RoomCode,
                            FriendId = currentMember.FamilyHead?.FriendId?.ToString() ?? "-",
                            TotalAmount = "$" + receivableUsd.ToString("N0"),
                            Invoices = invoicesString,
                            CompanyVisaName = currentMember.FamilyHead.CompanyVisas.CompanyName
                        }
                    );
                }
            }

            int familyCount = 0;
            for (int i = 0; i < familiesGroups.Count; i++)
            {
                familyCount++;
            }

            model.PackageSummary = new GroupSummaryModel
            {
                MembersCount = members.Count,
                FamiliesCount = familyCount,
                MalesCount = maleMembersCount,
                FemalesCount = femaleMembersCount
            };

            for (int i = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                int memberAge = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth);
                if (memberAge < 2)
                {
                    model.PackageSummary.InfantsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.InfantMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.InfantFemales++;
                    }
                }
                else if (memberAge < 12)
                {
                    model.PackageSummary.ChildsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.ChildMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.ChildFemales++;
                    }
                }
                else if (memberAge >= 12)
                {
                    model.PackageSummary.AdultsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.AdultMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.AdultFemales++;
                    }
                }
            }

            var facilities = _context.AdditionalFacilities
                .Include(e => e.Room)
                .Where(e => e.PackageId == id)
                .ToList();

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = _context.AdditionalFacilities
    .Join(_context.Rooms, af => af.RoomId, r => r.RoomId, (af, r) => new { AdditionalFacility = af, Room = r })
    .Join(_context.FamilyHeads, join1 => join1.AdditionalFacility.PackageId, fh => fh.PackageId, (join1, fh) => new { join1.AdditionalFacility, join1.Room, FamilyHead = fh })
    .Join(_context.GroupMembers, join2 => join2.FamilyHead.FamilyHeadId, gm => gm.FamilyHeadId, (join2, gm) => new { join2.AdditionalFacility, join2.Room, join2.FamilyHead, GroupMember = gm })
    .Where(join3 => join3.AdditionalFacility.PackageId == id && join3.GroupMember.GroupId == GroupId)
    .GroupBy(join4 => new { join4.AdditionalFacility.RoomId, join4.AdditionalFacility.PlaceId, join4.Room.PAX, join4.Room.RoomCode, join4.Room.RoomName })
    .Select(grouped => new
    {
        RoomId = grouped.Key.RoomId,
        PlaceId = grouped.Key.PlaceId,
        PAX = grouped.Key.PAX,
        RoomCode = grouped.Key.RoomCode,
        RoomName = grouped.Key.RoomName
    })
    .ToList();


            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var famFacilities = _context.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .Where(e => e.AdditionalFacilities.PackageId == id);

            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == MAKKAH_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Makkah.Add(_room);
                }
                else if (_currFac.PlaceId == MEDINA_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == MEDINA_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Medina.Add(_room);
                }
                else if (_currFac.PlaceId == MASHAIR_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == MASHAIR_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Azizia.Add(_room);
                }
                else if (_currFac.PlaceId == AZIZIA_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == AZIZIA_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Mashair.Add(_room);
                }


            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Report(PackageReportSaveModel request)
        {
            //return Ok(request);

            var families = _context.FamilyHeadsWithoutTrash
                //.Where(e => e.grou == request.PackageId)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var currentMember = families.FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadIds[i]);
                //currentMember.MofaNumber = request.MOFAs.ElementAt(i) ?? "";
                currentMember.MofaNumber = string.IsNullOrEmpty(request.MOFAs[i]) ? "" : request.MOFAs[i].ToString();
                currentMember.HajjApplicationNumber = request.HajjAppNumbers.ElementAt(i) ?? "";
                currentMember.HajjAppSerialNumber = request.HajjSerials.ElementAt(i) ?? "";
                currentMember.TicketNumber = request.TicketNumbers.ElementAt(i) ?? "";
                currentMember.VisaNumber = request.VisaNumbers.ElementAt(i) ?? "";
                currentMember.EHajjId = request.EHajjIds.ElementAt(i) ?? "";
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Package");
        }

        public IActionResult AziziaRoomListing(int id)
        {

            var group = _context.Groups
                                    .Select(e => new
                                    {
                                        e.GroupId,
                                        GroupName = e.Name,
                                        e.Package.Company.CompanyName,
                                        e.PackageId
                                    })
                                    .FirstOrDefault(e => e.GroupId == id);

            var accommodation = _context
                                .PackageAccomodtions
                                .Select(e => new
                                {
                                    e.Hotel.HotelName,
                                    e.PlaceId,
                                    e.PackageId,
                                    e.ActualCheckInTime,
                                    e.ActualCheckOutTime,
                                })
                                .FirstOrDefault(e => e.PackageId == group.PackageId && e.PlaceId == 4);

            var groupReservation = _context.Reservations.FirstOrDefault(e => e.GroupId == id);

            var groupReservationMembers = _context.ReservationFamilyMembers
                                                .Include(e => e.FamilyHead)
                                                .Include(e => e.Floor)
                                                .Include(e => e.FloorRoom)
                                                .ThenInclude(e => e.Room)
                                                .Include(e => e.FamilyHead)
                                                .ThenInclude(e => e.Package)
                                                .ThenInclude(e => e.Company)
                                                .Where(e => e.FloorRoomId != null && e.ReservationId == groupReservation.ReservationId)
                                                .OrderBy(e => e.FloorId)
                                                .ThenBy(e => e.FloorRoomId)
                                                .GroupBy(e => new { e.FloorId, e.FloorRoomId })
                                                .ToList();

            AziziaRoomListingModel model = new()
            {
                CompanyName = group.CompanyName,
                GroupName = group.GroupName,
                CheckIn = accommodation.ActualCheckInTime?.Date.ToString(),
                CheckOut = accommodation.ActualCheckOutTime?.Date.ToString(),
                HotelName = accommodation.HotelName,
            };

            int x = 0;
            for (int i = 0; i < groupReservationMembers.Count; i++)
            {
                var _currGroup = groupReservationMembers.ElementAt(i);

                for (int j = 0; j < _currGroup.Count(); j++)
                {
                    var _currMember = _currGroup.ElementAt(j);
                    var _currMemberHead = _context.FamilyMembers
                                            .Include(e => e.Head)
                                            .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);

                    int memberCount = _context.FamilyMembers
                                            .Count(e => e.HeadId == _currMemberHead.HeadId);

                    string cId =
                       $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                       $".{_currMemberHead.Head.ExtractedFamilyNumber}.({memberCount})" +
                       $".{_currMember.FamilyHead.Package.Name}";

                    var memberInGroup = _context.GroupMembers
                            .Include(e => e.Group)
                            .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);
                    if (memberInGroup != null)
                    {
                        cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    }

                    var packageRoomType = _context.FamilyHeadAdditionalFacilities
                                            .Include(e => e.AdditionalFacilities)
                                            .ThenInclude(e => e.Room)
                                            .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId
                                            && e.AdditionalFacilities.PlaceId == 4)?.AdditionalFacilities.Room.RoomName;

                    AziziaRoomListingMemberModel _memberModel = new()
                    {
                        PackageRoomType = packageRoomType,
                        FcId = _currMember.FamilyHead.FcId,
                        FamilyHeadId = _currMember.FamilyHeadId,
                        IsHead = j == 0,
                        SrNo = ++x,
                        MegaId = _currMember.FamilyHead.ReferenceId?.ToString(),
                        Photo = _currMember.FamilyHead.MemberImage,
                        FullName = _currMember.FamilyHead.FullName,
                        Gender = _currMember.FamilyHead.Gender.ToString(),
                        MOBKSA = _currMember.FamilyHead.KSAMobileNumber,
                        PP = _currMember.FamilyHead.PassportNumber,
                        Visa = _currMember.FamilyHead.VisaNumber,
                        RowSpan = _currGroup.Count(),
                        FloorNumber = $"F#{_currMember.Floor.FloorNumber}",
                        RoomNumber = _currMember.FloorRoom.RoomNumber,
                        RoomType = _currMember.FloorRoom.Room.RoomCode,
                        Email = _context.FamilyHeadEmails.FirstOrDefault(e => e.Preferred && e.FamilyHeadId == _currMember.FamilyHeadId)?.Email,
                        CID = cId
                    };

                    model.Members.Add(_memberModel);
                }
            }

            return View(model);
        }

        public IActionResult AziziaRoomListingSource(int id, int PackageId)
        {
            GroupId = id;

            var group = _context.Groups
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.GroupId,
                                    e.Code,
                                    e.Name,
                                    GroupCode = e.Code,
                                    GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                })
                                .FirstOrDefault(e => e.GroupId == id);


            ViewBag.SubGroupName = group?.Code;
            ViewBag.GroupCode = group?.GroupCode;
            ViewBag.GroupName = group?.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _context.GroupMembers
                        join sg in _context.Groups on gm.GroupId equals sg.GroupId
                        join g in _context.Groups on sg.GroupId equals g.GroupId
                        join fh in _context.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _context.Packages on fh.PackageId equals p.PackageId
                        join c in _context.Companies on p.CompanyId equals c.CompanyId
                        where gm.GroupId == id && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.GroupId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Code,
                            fh.ReferenceId,
                            GroupName = g.Name,
                            GroupCode = g.Code,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId
                        };

            var result = query.ToList();

            var families = _context.FamilyHeadsWithoutTrash.Where(e => e.PackageId == group.PackageId);
            var familyHeadAdditionalFacilities = _context
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == group.PackageId);

            // var groupMemberRooms = _context.GroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                string roomType = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                // string stayRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                // string stayFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                // string connectedRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                // string connectedFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;


                var roomNumberData = _context.ReservationFamilyMembers
                                        .Include(e => e.Floor)
                                        .Include(e => e.FloorRoom)
                                        .ThenInclude(e => e.Room)
                                        .OrderBy(e => e.ReservationFamilyMembersId)
                                        .LastOrDefault(e => e.FamilyHeadId == _.FamilyHeadId);

                model.Members.Add(new RoomListingMember
                {
                    IsHead = _.IsHead,
                    FloorId = roomNumberData?.FloorId,
                    FloorRoomId = roomNumberData?.FloorRoomId,
                    FamilyHeadId = _.FamilyHeadId,
                    RoomId = roomId,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    RoomType = roomType ?? "Sharing",
                    RoomPax = roomPax ?? 5,
                    FcId = _.FcId,
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    StayWith = stayString,
                    FloorNumber = roomNumberData?.Floor?.FloorNumber.ToString(),
                    RoomNumberPax = roomNumberData?.FloorRoom?.Room?.PAX,
                    RoomNumberWithCode = $"{roomNumberData?.FloorRoom?.RoomNumber} ({roomNumberData?.FloorRoom?.Room?.RoomCode})",
                    PackageId = _.PackageId
                });
            }

            // for (int i = 0; i < groupRooms.Count; i++)
            // {
            //     var __ = groupRooms.ElementAt(i);

            //     model.Rooms.Add(new RoomListingRoom
            //     {
            //         RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
            //         Quantity = __.Quantity
            //     });
            // }

            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = group.CompanyName;
            model.Group = group.GroupName;
            model.SubGroup = group.Name;

            //=====================================//
            if (model.Members.Count > 0)
            {
                //model.Members.RemoveAll(e=> e.FloorRoomId != null);

                var packageId = model.Members[0].PackageId;
                var placeId = 4; //azizia

                var hotelIds = _context.PackageAccomodtions
                    .Where(pa => pa.PackageId == packageId && pa.PlaceId == placeId)
                    .Select(pa => pa.HotelId)
                    .ToList();

                int? firstHotelId = hotelIds.FirstOrDefault(); // Get the first hotel id if available, otherwise default to null or 0
                HotelId = Convert.ToInt32(firstHotelId);
                var floorNumbers = _context.Floors
                    .Where(fr => fr.HotelId == firstHotelId && !fr.Trashed)
                    //.Where(fr => !fr.Trashed)
                    .Select(fr => fr.FloorNumber.ToString())
                    // .Distinct()
                    .ToList();

                ViewBag.floorNumber = floorNumbers;
                ViewBag.HotelId = firstHotelId;
            }
            else
            {
                ViewBag.floorNumber = null;
                ViewBag.HotelId = null;

            }

            /*const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = _context.AdditionalFacilities
                .Join(_context.Rooms, af => af.RoomId, r => r.RoomId, (af, r) => new { AdditionalFacility = af, Room = r })
                .Join(_context.FamilyHeads, join1 => join1.AdditionalFacility.PackageId, fh => fh.PackageId, (join1, fh) => new { join1.AdditionalFacility, join1.Room, FamilyHead = fh })
                .Join(_context.GroupMembers, join2 => join2.FamilyHead.FamilyHeadId, gm => gm.FamilyHeadId, (join2, gm) => new { join2.AdditionalFacility, join2.Room, join2.FamilyHead, GroupMember = gm })
                .Where(join3 => join3.GroupMember.GroupId == id)
                .GroupBy(join4 => new { join4.AdditionalFacility.RoomId, join4.AdditionalFacility.PlaceId, join4.Room.PAX, join4.Room.RoomCode, join4.Room.RoomName })
                .Select(grouped => new
                {
                    RoomId = grouped.Key.RoomId,
                    PlaceId = grouped.Key.PlaceId,
                    PAX = grouped.Key.PAX,
                    RoomCode = grouped.Key.RoomCode,
                    RoomName = grouped.Key.RoomName
                })
                .ToList();


            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var groupPackages = _context.GroupPackages
                                          .Where(e => e.GroupId == id)
                                          .Select(e => e.PackageId)
                                          .ToList();

            var famFacilities = _context.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .Join(_context.GroupMembers, e => e.FamilyHeadId, e => e.FamilyHeadId, (fhaf, gm) => new { fhaf, gm })
                    .Where(e => groupPackages.Contains(e.fhaf.AdditionalFacilities.PackageId));


            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MAKKAH_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Makkah.Add(_room);
                }
                else if (_currFac.PlaceId == MEDINA_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MEDINA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Medina.Add(_room);
                }
                else if (_currFac.PlaceId == MASHAIR_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MASHAIR_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Azizia.Add(_room);
                }
                else if (_currFac.PlaceId == AZIZIA_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == AZIZIA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Mashair.Add(_room);
                }


            }*/
            return View(model);
        }

        public IActionResult AziziaUnAllocatedMembers(int id, int PackageId)
        {
            var groups = _context.GroupsWithoutTrash;

            foreach (var _g in groups)
            {


            }
            GroupId = id;

            var group = _context.Groups
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.GroupId,
                                    e.Code,
                                    e.Name,
                                    GroupCode = e.Code,
                                    GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                })
                                .FirstOrDefault(e => e.GroupId == id);


            ViewBag.SubGroupName = group?.Code;
            ViewBag.GroupCode = group?.GroupCode;
            ViewBag.GroupName = group?.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _context.GroupMembers
                        join sg in _context.Groups on gm.GroupId equals sg.GroupId
                        join g in _context.Groups on sg.GroupId equals g.GroupId
                        join fh in _context.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _context.Packages on fh.PackageId equals p.PackageId
                        join c in _context.Companies on p.CompanyId equals c.CompanyId
                        where gm.GroupId == id
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.GroupId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Code,
                            fh.ReferenceId,
                            GroupName = g.Name,
                            GroupCode = g.Code,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId
                        };

            var result = query.ToList();

            var families = _context.FamilyHeadsWithoutTrash.Where(e => e.PackageId == group.PackageId);
            var familyHeadAdditionalFacilities = _context
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == group.PackageId);

            // var groupMemberRooms = _context.GroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                string roomType = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                // string stayRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                // string stayFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                // string connectedRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                // string connectedFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;


                var roomNumberData = _context.ReservationFamilyMembers
                                        .Include(e => e.Floor)
                                        .Include(e => e.FloorRoom)
                                        .ThenInclude(e => e.Room)
                                        .OrderBy(e => e.ReservationFamilyMembersId)
                                        .LastOrDefault(e => e.FamilyHeadId == _.FamilyHeadId);

                model.Members.Add(new RoomListingMember
                {
                    IsHead = _.IsHead,
                    FloorId = roomNumberData?.FloorId,
                    FloorRoomId = roomNumberData?.FloorRoomId,
                    FamilyHeadId = _.FamilyHeadId,
                    RoomId = roomId,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    RoomType = roomType ?? "Sharing",
                    RoomPax = roomPax ?? 5,
                    FcId = _.FcId,
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    StayWith = stayString,
                    FloorNumber = roomNumberData?.Floor?.FloorNumber.ToString(),
                    RoomNumberPax = roomNumberData?.FloorRoom?.Room?.PAX,
                    RoomNumberWithCode = $"{roomNumberData?.FloorRoom?.RoomNumber} ({roomNumberData?.FloorRoom?.Room?.RoomCode})",
                    PackageId = _.PackageId
                });
            }

            // for (int i = 0; i < groupRooms.Count; i++)
            // {
            //     var __ = groupRooms.ElementAt(i);

            //     model.Rooms.Add(new RoomListingRoom
            //     {
            //         RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
            //         Quantity = __.Quantity
            //     });
            // }

            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = group.CompanyName;
            model.Group = group.GroupName;
            model.SubGroup = group.Name;

            //=====================================//
            if (model.Members.Count > 0)
            {
                model.Members.RemoveAll(e => e.FloorRoomId != null);

                var packageId = model.Members[0].PackageId;
                var placeId = 4; //azizia

                var hotelIds = _context.PackageAccomodtions
                    .Where(pa => pa.PackageId == packageId && pa.PlaceId == placeId)
                    .Select(pa => pa.HotelId)
                    .ToList();

                int? firstHotelId = hotelIds.FirstOrDefault(); // Get the first hotel id if available, otherwise default to null or 0
                HotelId = Convert.ToInt32(firstHotelId);
                var floorNumbers = _context.Floors
                    .Where(fr => fr.HotelId == firstHotelId && !fr.Trashed)
                    .Select(fr => fr.FloorNumber.ToString())
                    // .Distinct()
                    .ToList();

                ViewBag.floorNumber = floorNumbers;
                ViewBag.HotelId = firstHotelId;
            }
            else
            {
                ViewBag.floorNumber = null;
                ViewBag.HotelId = null;

            }


            return View(model);
        }

        [HttpGet]
        public JsonResult GetRooms(int floorNumber, int hotelId)
        {
            // var roomNumbers = _context.FloorRoom
            //      .Where(fr => fr.Floor.FloorNumber == floorNumber && fr.Floor.HotelId == hotelId)
            //                  .Select(fr => new
            //                  {
            //                      fr.FloorRoomId,
            //                      RoomCode = $"{fr.RoomNumber} ({fr.Room.RoomCode})",
            //                      pax = fr.Room.PAX
            //                  })
            //                  .ToList();

            var roomNumbers = (from fr in _context.FloorRoom
                               join f in _context.Floors on fr.FloorId equals f.FloorId
                               join r in _context.Rooms on fr.RoomId equals r.RoomId
                               join rfm in _context.ReservationFamilyMembers on fr.FloorRoomId equals rfm.FloorRoomId into rfmGroup
                               from rfm in rfmGroup.DefaultIfEmpty()
                               where f.FloorNumber == floorNumber && f.HotelId == hotelId
                               //where f.FloorNumber == floorNumber
                               select new
                               {
                                   fr.FloorRoomId,
                                   fr.RoomNumber,
                                   r.RoomCode,
                                   r.PAX,
                                   ReservationFamilyMemberExists = rfm != null
                               })
             .AsEnumerable()
             .GroupBy(x => new { x.FloorRoomId, x.RoomNumber, x.RoomCode, x.PAX })
             .Select(g => new
             {
                 FloorRoomId = g.Key.FloorRoomId,
                 RoomInfo = $"{g.Key.RoomNumber} ({g.Key.RoomCode})",
                 Count = g.Count(x => x.ReservationFamilyMemberExists),
                 PAX = g.Key.PAX,
                 Availability = g.Count() == g.Key.PAX ? "Maxed" : "Open"
             })
             .Where(x => x.Availability == "Open")
             .ToList();

            // var roomNumbers = (from fr in _context.FloorRoom
            //                    join rfm in _context.ReservationFamilyMembers
            //                    on fr.FloorRoomId equals rfm.FloorRoomId into frGroup
            //                    from rfm in frGroup.DefaultIfEmpty()
            //                    where rfm == null && fr.Floor.FloorNumber == floorNumber && fr.Floor.HotelId == hotelId
            //                    select new
            //                    {
            //                        fr.FloorRoomId,
            //                        RoomCode = $"{fr.RoomNumber} ({fr.Room.RoomCode})",
            //                        pax = fr.Room.PAX
            //                    })
            //           .ToList();



            return Json(roomNumbers);
        }


        [HttpPost]
        public IActionResult Save(int id, RoomSaveModel request)
        {
            var accommodations = _context.PackageAccomodtions
                .Join(
                    _context.Groups,
                    pa => pa.PackageId,
                    g => g.PackageId,
                    (pa, g) => new { pa, g }
                )
                .Join(
                    _context.SubGroups,
                    pair => pair.g.GroupId,
                    sg => sg.GroupId,
                    (pair, sg) => new { pair.pa, sg }
                )
                .Where(result => result.sg.GroupId == id && result.pa.PlaceId == 4)
                .Select(result => result.pa)
                .ToList();

            _context.SubGroupMemberRooms.Where(e => e.SubGroupId == id).ExecuteDelete();
            // var subGroup = _context.SubGroupsWithoutTrash.Where(e => e.GroupId == id).FirstOrDefault();
            var previousReservation = _context.Reservations.Where(r => r.GroupId == id); //&& r.SubGroupId == null
            var reservationid = previousReservation.Count() > 0 ? previousReservation.FirstOrDefault().ReservationId : 0;
            _context.ReservationFamilyMembers.Where(e => e.ReservationId == reservationid).ExecuteDelete();
            //_context.Reservations.Where(e => e.ReservationId == reservationid).ExecuteDelete();


            if (reservationid != 0)
            {

            }
            else
            {
                Reservation newReservation = new Reservation
                {
                    HotelId = HotelId,
                    CheckInDate = accommodations.Count > 0 ? Convert.ToDateTime(accommodations[0].ActualCheckInTime) : DateTime.MinValue,
                    CheckOutDate = accommodations.Count > 0 ? Convert.ToDateTime(accommodations[0].ActualCheckOutTime) : DateTime.MinValue,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Trashed = false,
                    GroupId = id
                };

                _context.Reservations.Add(newReservation);
                _context.SaveChanges();
                reservationid = newReservation.ReservationId;
            }

            //Reservation newReservation = new Reservation
            //{
            //    HotelId = HotelId,
            //    CheckInDate = accommodations.Count > 0 ? Convert.ToDateTime(accommodations[0].ActualCheckInTime) : DateTime.MinValue,
            //    CheckOutDate = accommodations.Count > 0 ? Convert.ToDateTime(accommodations[0].ActualCheckOutTime) : DateTime.MinValue,
            //    CreatedAt = DateTime.Now,
            //    UpdatedAt = DateTime.Now,
            //    Trashed = false,
            //    GroupId = id
            //};

            //_context.Reservations.Add(newReservation);
            //_context.SaveChanges();
            //int reservationId = newReservation.ReservationId;


            /* for (int i = 0; i < request.FamilyHeadIds.Count; i++)
             {
                 var floors = _context.Floors
                         .Where(e => e.FloorNumber == Convert.ToInt32(request.StayFloorNumbers[i]) && e.HotelId == HotelId);

                 var floorid = floors.Count() > 0 ? _context.Floors
                         .Where(e => e.FloorNumber == Convert.ToInt32(request.StayFloorNumbers[i]) && e.HotelId == HotelId)
                         .FirstOrDefault().FloorId : 0;

                 var floorRoomid = request.StayRoomNumbers[i];
                 _context.ReservationFamilyMembers.Add(new ReservationFamilyMembers
                 {
                     FamilyHeadId = request.FamilyHeadIds[i],
                     FloorId = Convert.ToInt32(floorid),
                     FloorRoomId = floorRoomid,
                     ReservationId = reservationid
                 });
             }
             _context.SaveChanges();*/

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var floor = _context.Floors
                    .FirstOrDefault(e => e.FloorNumber == Convert.ToInt32(request.StayFloorNumbers[i]) && e.HotelId == HotelId);

                if (floor == null)
                {
                    Console.WriteLine($"⚠️ Warning: No floor found for FloorNumber {request.StayFloorNumbers[i]} and HotelId {HotelId}");
                    continue; // Skip this iteration if FloorId does not exist
                }

                var floorid = floor.FloorId;
                var floorRoomid = request.StayRoomNumbers[i];

                _context.ReservationFamilyMembers.Add(new ReservationFamilyMembers
                {
                    FamilyHeadId = request.FamilyHeadIds[i],
                    FloorId = floorid,  // Now it's guaranteed to exist
                    FloorRoomId = floorRoomid,
                    ReservationId = reservationid
                });
            }

            _context.SaveChanges();


            return RedirectToAction("Index", "Group");
        }


        [HttpPost]
        public IActionResult NewRoomListing(NewRoomListingSaveModel request)
        {
            var members = _context.FamilyHeads
                        .Where(e => request.FamilyHeadIds.Contains(e.FamilyHeadId))
                        .ToList();

            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                var familyHead = members.FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);

                familyHead.VisaNumber = request.VisaNumbers.ElementAt(i);
                familyHead.KSAMobileNumber = request.MOBKSAs.ElementAt(i);
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Group");
        }

        public IActionResult MakkahMedinaRoomListingSource(int id)
        {
            var _group = _context.Groups
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.GroupId,
                                    e.Code,
                                    e.Name,
                                    GroupCode = e.Code,
                                    GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                    // e.PlaceId
                                })
                                .FirstOrDefault(e => e.GroupId == id);

            // var groupRooms = _context.GroupRooms
            //                     .Include(e => e.SubGroup)
            //                     .Include(e => e.Room)
            //                     .Where(e => e.SubGroupId == id)
            //                     .ToList();

            // ViewBag.SubGroupName = group.Code;
            ViewBag.GroupCode = _group.GroupCode;
            ViewBag.GroupName = _group.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _context.GroupMembers
                        join g in _context.Groups on gm.GroupId equals g.GroupId
                        join fh in _context.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _context.Packages on fh.PackageId equals p.PackageId
                        join c in _context.Companies on p.CompanyId equals c.CompanyId
                        where gm.GroupId == _group.GroupId
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Code,
                            fh.ReferenceId,
                            GroupName = g.Name,
                            GroupCode = g.Code,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId
                        };

            var result = query.ToList();

            var families = _context.FamilyHeadsWithoutTrash.Where(e => e.PackageId == _group.PackageId);
            var familyHeadAdditionalFacilities = _context
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == _group.PackageId);

            var _groupMemberRooms = _context.SubGroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                // string roomType = familyHeadAdditionalFacilities
                //                 .FirstOrDefault(e => e.PlaceId == _group.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                //int? roomPax = familyHeadAdditionalFacilities
                //                .FirstOrDefault(e => e.PlaceId == _group.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                //int? roomId = familyHeadAdditionalFacilities
                //                .FirstOrDefault(e => e.PlaceId == _group.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                //string stayRoomNumber = _groupMemberRooms
                //    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                //string stayFloorNumber = _groupMemberRooms
                //    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                //string connectedRoomNumber = _groupMemberRooms
                //    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                //string connectedFloorNumber = _groupMemberRooms
                //    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;

                model.Members.Add(new RoomListingMember
                {

                    FamilyHeadId = _.FamilyHeadId,
                    // RoomId = roomId,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    // RoomType = roomType ?? "Sharing",
                    // RoomPax = roomPax ?? 5,
                    FcId = _.FcId,
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    StayWith = stayString,
                    //RoomNumber = stayRoomNumber,
                    //FloorNumber = stayFloorNumber,
                    //ConnectedFloorNumber = connectedFloorNumber,
                    //ConnectedRoomNumber = connectedRoomNumber
                });
            }


            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = _group.CompanyName;
            model.Group = _group.GroupName;
            model.SubGroup = _group.Name;

            return View(model);
        }

        public IActionResult MedinaRoomListingSource(int id)
        {
            ViewBag.SubGroupId = id;
            var result = (
            from query in (
                from fh in _context.FamilyHeads
                join sgm in _context.SubGroupMembers on fh.FamilyHeadId equals sgm.FamilyHeadId
                //join sgmr in _context.SubGroupMemberRooms on sgm.SubGroupId equals sgmr.SubGroupId into sgmrJoin
                // from sgmr in sgmrJoin.DefaultIfEmpty()
                join fhad in _context.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhad.FamilyHeadId into fhadJoin
                from fhad in fhadJoin.DefaultIfEmpty()
                join adf in _context.AdditionalFacilities on fhad.AdditionalFacilitiesId equals adf.AdditionalFacilitiesId into adfJoin
                from adf in adfJoin.DefaultIfEmpty()
                join r in _context.Rooms on adf.RoomId equals r.RoomId into rJoin
                from r in rJoin.DefaultIfEmpty()
                join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                join fhe in _context.FamilyHeadEmails on fh.FamilyHeadId equals fhe.FamilyHeadId into fheJoin
                from fhe in fheJoin.DefaultIfEmpty()
                join p in _context.Packages on fh.PackageId equals p.PackageId
                join pa in _context.PackageAccomodtions on fh.PackageId equals pa.PackageId
                join sg in _context.SubGroups on sgm.SubGroupId equals sg.SubGroupId
                join c in _context.Companies on p.CompanyId equals c.CompanyId
                join mc in _context.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
                join h in _context.Hotels on pa.HotelId equals h.HotelId
                where sg.SubGroupId == id
                    && pa.PlaceId == 2
                    && ((_context.FamilyHeadAdditionalFacilities.Include(e => e.AdditionalFacilities).Where(e => e.FamilyHeadId == fh.FamilyHeadId && e.AdditionalFacilities.PlaceId == 2).Count() == 0) || adf.PlaceId == 2)
                    && !fh.Trashed
                    && !r.Trashed
                    && (fhe != null && fhe.Preferred)
                select new
                {
                    FamilyHeadId = fh.FamilyHeadId,
                    PackageAccomodationsId = pa.PackageAccomodationsId,
                    MunazimName = m.Name,
                    CompanyName = c.CompanyName,
                    GroupName = sg.Name,
                    PackageName = p.Name,
                    HotelName = h.HotelName,
                    CheckIn = pa.ActualCheckInTime.HasValue ? pa.ActualCheckInTime.Value.Date : (DateTime?)null,
                    CheckOut = pa.ActualCheckOutTime.HasValue ? pa.ActualCheckOutTime.Value.Date : (DateTime?)null,
                    FcId = fh.FcId,
                    MemberImage = fh.MemberImage,
                    FullName = fh.GivenName + " " + fh.SurName,
                    Gender = fh.Gender,
                    Email = _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == fm.HeadId).Select(e => e.Email).FirstOrDefault(),
                    KSAMobileNumber = fh.KSAMobileNumber,
                    PassportNumber = fh.PassportNumber,
                    VisaNumber = fh.VisaNumber,
                    RoomCode = r.RoomCode,
                    // FloorNumber = sgmr.FloorNumber,
                    // RoomNumber = sgmr.RoomNumber,
                    HajjApplicationNumber = fh.HajjApplicationNumber,
                    HeadId = fm.HeadId,
                    RoomName = r.RoomName,
                    PAX = r.PAX,
                    RowSpan = 0,
                    CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                   $".{fm.Head.ExtractedFamilyNumber}.({_context.FamilyMembers.Count(e => e.HeadId == fm.HeadId)})" +
                   $".{fm.FamilyHead.Package.Name}",
                }
            )
            select new
            {
                query.FamilyHeadId,
                query.PackageAccomodationsId,
                query.MunazimName,
                query.CompanyName,
                query.GroupName,
                query.PackageName,
                query.HotelName,
                query.CheckIn,
                query.CheckOut,
                query.FcId,
                query.MemberImage,
                query.FullName,
                query.Gender,
                query.Email,
                query.KSAMobileNumber,
                query.PassportNumber,
                query.VisaNumber,
                RoomCodeNew = (_context.AdditionalFacilities
    .Any(adf => adf.PlaceId == 2 && _context.FamilyHeadAdditionalFacilities
        .Any(e => e.FamilyHeadId == query.FamilyHeadId && e.AdditionalFacilitiesId == adf.AdditionalFacilitiesId)))
            ? query.RoomCode
            : "Sharing",
                //query.FloorNumber,
                // query.RoomNumber,
                query.HajjApplicationNumber,
                query.HeadId,
                query.RoomName,
                query.PAX,
                query.RowSpan,
                query.CID


            }
        ).Distinct().OrderBy(e => e.FcId).ToList();


            ViewBag.Modal = result;
            if (result.Count > 0)
            {
                ViewBag.CheckIn = result[0].CheckIn;
                ViewBag.CheckOut = result[0].CheckOut;
                ViewBag.MunazimName = result[0].MunazimName;
                ViewBag.CompanyName = result[0].CompanyName;
                ViewBag.HotelName = result[0].HotelName;
                ViewBag.GroupName = result[0].GroupName;


            }

            int totalmemebers = 0;
            int malecount = 0;
            int femalecount = 0;
            int dblroomcount = 0;
            int triroomcount = 0;
            int quadroomcount = 0;
            int quintroomcount = 0;
            int sharingroomcount = 0;
            foreach (var item in result)
            {
                totalmemebers = totalmemebers + 1;
                if (item.Gender == (Gender)0)
                {
                    malecount = malecount + 1;
                }
                else
                {
                    femalecount = femalecount + 1;
                }

                if (item.RoomCodeNew == "DB-02")
                {
                    dblroomcount = dblroomcount + 1;
                }
                else if (item.RoomCodeNew == "TR-3")
                {
                    triroomcount = triroomcount + 1;
                }
                else if (item.RoomCodeNew == "QD-4")
                {
                    quadroomcount = quadroomcount + 1;
                }
                else if (item.RoomCodeNew == "QT-5")
                {
                    quintroomcount = quintroomcount + 1;
                }
                else
                {
                    sharingroomcount = sharingroomcount + 1;
                }



            }

            ViewBag.dblroom = (float)dblroomcount / 2;
            ViewBag.triroom = (float)triroomcount / 3;
            ViewBag.quadroom = (float)quadroomcount / 4;
            ViewBag.quintroom = (float)quintroomcount / 5;
            ViewBag.sharingroom = (float)sharingroomcount / 4;

            ViewBag.FemaleCount = femalecount;
            ViewBag.MaleCount = malecount;
            ViewBag.TotalMember = totalmemebers;
            ViewBag.rooms = _context.SubGroupMemberRooms
            .Where(sgmr => sgmr.SubGroupId == id)
            .ToList();


            var visapassport = from sgm in _context.SubGroupMembers
                               join fh in _context.FamilyHeads on sgm.FamilyHeadId equals fh.FamilyHeadId
                               where sgm.SubGroupId == id
                               select new
                               {
                                   fh.PassportImage,
                                   fh.VisaImage
                               };

            var resultList = visapassport.ToList();

            List<visapassportmodel> visamodel = new List<visapassportmodel>();

            foreach (var item in resultList)
            {
                string passportPath = Path.Combine(_host.WebRootPath, item.PassportImage == null ? "" : item.PassportImage);
                string VisaPath = Path.Combine(_host.WebRootPath, item.VisaImage == null ? "" : item.VisaImage);
                if (System.IO.File.Exists(passportPath) && System.IO.File.Exists(VisaPath))
                {
                    visamodel.Add(new visapassportmodel
                    {
                        visa = item.VisaImage,
                        passport = item.PassportImage
                    });
                }

            }
            ViewBag.passportVisa = visamodel;

            return View();
        }

        public IActionResult MakkahRoomListingSource(int id)
        {
            ViewBag.SubGroupId = id;
            var result = (
            from query in (
                from fh in _context.FamilyHeads
                join sgm in _context.SubGroupMembers on fh.FamilyHeadId equals sgm.FamilyHeadId
                //join sgmr in _context.SubGroupMemberRooms on sgm.SubGroupId equals sgmr.SubGroupId into sgmrJoin
                // from sgmr in sgmrJoin.DefaultIfEmpty()
                join fhad in _context.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhad.FamilyHeadId into fhadJoin
                from fhad in fhadJoin.DefaultIfEmpty()
                join adf in _context.AdditionalFacilities on fhad.AdditionalFacilitiesId equals adf.AdditionalFacilitiesId into adfJoin
                from adf in adfJoin.DefaultIfEmpty()
                join r in _context.Rooms on adf.RoomId equals r.RoomId into rJoin
                from r in rJoin.DefaultIfEmpty()
                join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                join fhe in _context.FamilyHeadEmails on fh.FamilyHeadId equals fhe.FamilyHeadId into fheJoin
                from fhe in fheJoin.DefaultIfEmpty()
                join p in _context.Packages on fh.PackageId equals p.PackageId
                join pa in _context.PackageAccomodtions on fh.PackageId equals pa.PackageId
                join sg in _context.SubGroups on sgm.SubGroupId equals sg.SubGroupId
                join c in _context.Companies on p.CompanyId equals c.CompanyId
                join mc in _context.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
                join h in _context.Hotels on pa.HotelId equals h.HotelId
                where sg.SubGroupId == id
                    && pa.PlaceId == 1
                    && ((_context.FamilyHeadAdditionalFacilities.Include(e => e.AdditionalFacilities).Where(e => e.FamilyHeadId == fh.FamilyHeadId && e.AdditionalFacilities.PlaceId == 1)
                    .Count() == 0) || adf.PlaceId == 1)
                    && !fh.Trashed
                    && !r.Trashed
                    && (fhe != null && fhe.Preferred)
                select new
                {
                    FamilyHeadId = fh.FamilyHeadId,
                    PackageAccomodationsId = pa.PackageAccomodationsId,
                    MunazimName = m.Name,
                    CompanyName = c.CompanyName,
                    GroupName = sg.Name,
                    PackageName = p.Name,
                    HotelName = h.HotelName,
                    CheckIn = pa.ActualCheckInTime.HasValue ? pa.ActualCheckInTime.Value.Date : (DateTime?)null,
                    CheckOut = pa.ActualCheckOutTime.HasValue ? pa.ActualCheckOutTime.Value.Date : (DateTime?)null,
                    FcId = fh.FcId,
                    MemberImage = fh.MemberImage,
                    FullName = fh.GivenName + " " + fh.SurName,
                    Gender = fh.Gender,
                    Email = _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == fm.HeadId).Select(e => e.Email).FirstOrDefault(),
                    KSAMobileNumber = fh.KSAMobileNumber,
                    PassportNumber = fh.PassportNumber,
                    VisaNumber = fh.VisaNumber,
                    RoomCode = r.RoomCode,
                    // FloorNumber = sgmr.FloorNumber,
                    // RoomNumber = sgmr.RoomNumber,
                    HajjApplicationNumber = fh.HajjApplicationNumber,
                    HeadId = fm.HeadId,
                    RoomName = r.RoomName,
                    PAX = r.PAX,
                    RowSpan = 0,
                    CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                   $".{fm.Head.ExtractedFamilyNumber}.({_context.FamilyMembers.Count(e => e.HeadId == fm.HeadId)})" +
                   $".{fm.FamilyHead.Package.Name}",
                }
            )
            select new
            {
                query.FamilyHeadId,
                query.PackageAccomodationsId,
                query.MunazimName,
                query.CompanyName,
                query.GroupName,
                query.PackageName,
                query.HotelName,
                query.CheckIn,
                query.CheckOut,
                query.FcId,
                query.MemberImage,
                query.FullName,
                query.Gender,
                query.Email,
                query.KSAMobileNumber,
                query.PassportNumber,
                query.VisaNumber,
                RoomCodeNew = (_context.AdditionalFacilities
    .Any(adf => adf.PlaceId == 1 && _context.FamilyHeadAdditionalFacilities
        .Any(e => e.FamilyHeadId == query.FamilyHeadId && e.AdditionalFacilitiesId == adf.AdditionalFacilitiesId)))
            ? query.RoomCode
            : "Sharing",
                //query.FloorNumber,
                // query.RoomNumber,
                query.HajjApplicationNumber,
                query.HeadId,
                query.RoomName,
                query.PAX,
                query.RowSpan,
                query.CID


            }
        ).Distinct().OrderBy(e => e.FcId).ToList();


            ViewBag.Modal = result;
            if (result.Count > 0)
            {
                ViewBag.CheckIn = result[0].CheckIn;
                ViewBag.CheckOut = result[0].CheckOut;
                ViewBag.MunazimName = result[0].MunazimName;
                ViewBag.CompanyName = result[0].CompanyName;
                ViewBag.HotelName = result[0].HotelName;
                ViewBag.GroupName = result[0].GroupName;


            }

            int totalmemebers = 0;
            int malecount = 0;
            int femalecount = 0;
            int dblroomcount = 0;
            int triroomcount = 0;
            int quadroomcount = 0;
            int quintroomcount = 0;
            int sharingroomcount = 0;
            foreach (var item in result)
            {
                totalmemebers = totalmemebers + 1;
                if (item.Gender == (Gender)0)
                {
                    malecount = malecount + 1;
                }
                else
                {
                    femalecount = femalecount + 1;
                }

                if (item.RoomCodeNew == "DB-02")
                {
                    dblroomcount = dblroomcount + 1;
                }
                else if (item.RoomCodeNew == "TR-3")
                {
                    triroomcount = triroomcount + 1;
                }
                else if (item.RoomCodeNew == "QD-4")
                {
                    quadroomcount = quadroomcount + 1;
                }
                else if (item.RoomCodeNew == "QT-5")
                {
                    quintroomcount = quintroomcount + 1;
                }
                else
                {
                    sharingroomcount = sharingroomcount + 1;
                }
            }

            ViewBag.dblroom = (float)dblroomcount / 2;
            ViewBag.triroom = (float)triroomcount / 3;
            ViewBag.quadroom = (float)quadroomcount / 4;
            ViewBag.quintroom = (float)quintroomcount / 5;
            ViewBag.sharingroom = (float)sharingroomcount / 4;

            ViewBag.FemaleCount = femalecount;
            ViewBag.MaleCount = malecount;
            ViewBag.TotalMember = totalmemebers;

            ViewBag.rooms = _context.SubGroupMemberRooms
                    .Where(sgmr => sgmr.SubGroupId == id)
                    .ToList();

            var visapassport = from sgm in _context.SubGroupMembers
                               join fh in _context.FamilyHeads on sgm.FamilyHeadId equals fh.FamilyHeadId
                               where sgm.SubGroupId == id
                               select new
                               {
                                   fh.PassportImage,
                                   fh.VisaImage
                               };

            var resultList = visapassport.ToList();

            List<visapassportmodel> visamodel = new List<visapassportmodel>();

            foreach (var item in resultList)
            {
                string passportPath = Path.Combine(_host.WebRootPath, item.PassportImage == null ? "" : item.PassportImage);
                string VisaPath = Path.Combine(_host.WebRootPath, item.VisaImage == null ? "" : item.VisaImage);
                if (System.IO.File.Exists(passportPath) && System.IO.File.Exists(VisaPath))
                {
                    visamodel.Add(new visapassportmodel
                    {
                        visa = item.VisaImage,
                        passport = item.PassportImage
                    });
                }

            }
            ViewBag.passportVisa = visamodel;
            return View();
        }
        public IActionResult MakkahMedinaSave(int id, MakkahMedinaRoomSaveModel request)
        {
            _context.SubGroupMemberRooms.Where(e => e.SubGroupId == id).ExecuteDelete();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {

                _context.SubGroupMemberRooms.Add(new SubGroupMemberRooms
                {
                    FamilyHeadId = request.FamilyHeadIds[i],
                    RoomId = request.RoomIds[i],
                    SubGroupId = id,
                    RoomNumber = request.StayRoomNumbers[i]?.Trim(),
                    FloorNumber = request.StayFloorNumbers[i]?.Trim(),
                    ConnectedFloorNumber = request.ConnectedFloorNumbers[i]?.Trim(),
                    ConnectedRoomNumber = request.ConnectedRoomNumbers[i]?.Trim(),
                });
            }
            _context.SaveChanges();

            return RedirectToAction("Index", "SubGroup");
        }


        [HttpPost]
        public IActionResult MedinaSave(int id, MedinaRoomListSaveModel request)
        {
            _context.SubGroupMemberRooms.Where(e => e.SubGroupId == id).ExecuteDelete();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {

                _context.SubGroupMemberRooms.Add(new SubGroupMemberRooms
                {
                    FamilyHeadId = request.FamilyHeadIds[i],
                    //RoomId = request.RoomIds[i],
                    SubGroupId = id,
                    PlaceId = 2,
                    RoomNumber = request.StayRoomNumbers[i]?.Trim(),
                    FloorNumber = request.StayFloorNumbers[i]?.Trim(),
                    //ConnectedFloorNumber = request.ConnectedFloorNumbers[i]?.Trim(),
                    //ConnectedRoomNumber = request.ConnectedRoomNumbers[i]?.Trim(),
                });
            }
            _context.SaveChanges();

            return RedirectToAction("Index", "SubGroup");
        }

        public IActionResult MakkahSave(int id, MedinaRoomListSaveModel request)
        {
            _context.SubGroupMemberRooms.Where(e => e.SubGroupId == id).ExecuteDelete();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {

                _context.SubGroupMemberRooms.Add(new SubGroupMemberRooms
                {
                    FamilyHeadId = request.FamilyHeadIds[i],
                    //RoomId = request.RoomIds[i],
                    SubGroupId = id,
                    PlaceId = 1,
                    RoomNumber = request.StayRoomNumbers[i]?.Trim(),
                    FloorNumber = request.StayFloorNumbers[i]?.Trim(),
                    //ConnectedFloorNumber = request.ConnectedFloorNumbers[i]?.Trim(),
                    //ConnectedRoomNumber = request.ConnectedRoomNumbers[i]?.Trim(),
                });
            }
            _context.SaveChanges();

            return RedirectToAction("Index", "SubGroup");
        }


        public IActionResult IndividualTokensV2(int id)
        {

            List<QurbaniIndividualReceiptModel> model = new();

            //var members = _context.FamilyMembers
            //        .Include(e => e.FamilyHead)
            //        .ThenInclude(e => e.Package)
            //        .ThenInclude(e => e.Company)
            //        .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
            //        .ToList();

            var members = (from fm in _context.FamilyMembers
                           join fh in _context.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                           join p in _context.Packages on fh.PackageId equals p.PackageId
                           join c in _context.Companies on p.CompanyId equals c.CompanyId
                           join gm in _context.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                           join g in _context.Groups on gm.GroupId equals g.GroupId
                           where g.GroupId == id && !fh.Trashed && !g.Trashed
                           select new
                           {
                               fm.HeadId,
                               fh.FamilyHeadId,
                               fh.FullName,
                               fh.QurbaniToken,
                               fh.KSAMobileNumber,
                               fh.FcId,
                               fh.IndId,
                               fh.Package,
                               fh.Package.Company,
                               fh.Package.Company.CompanyCode,
                               fh.ExtractedFamilyNumber,
                           }).ToList();

            foreach (var m in members)
            {
                QurbaniIndividualReceiptModel _ = new()
                {
                    HeadId = m.HeadId,
                    Name = m.FullName,
                    FcId = m.FcId,
                    TokenNumber = "Q" + m.QurbaniToken?.ToString(),
                    MobNumber = _context.FamilyHeadContacts
                                        .AsNoTracking()
                                        .FirstOrDefault(e =>
                                            e.FamilyHeadId == m.FamilyHeadId &&
                                            e.Preferred)?
                                        .Contact,
                    KSANumber = m.KSAMobileNumber
                };

                int pax = members.Where(e => e.HeadId == m.HeadId).Count();

                string cId =
                            $"{m.CompanyCode}.{m.IndId}" +
                            $".{m.ExtractedFamilyNumber}.({pax})" +
                            $".{m.Package.Name}";

                var memberInGroup = _context.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _.CID = cId;

                model.Add(_);

            }

            return View(model);
        }

        public IActionResult GroupWiseNusukCard(int id)
        {
            CardModel model = new();

            var package = (from fh in _context.FamilyHeads
                           join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _context.Packages on fh.PackageId equals p.PackageId
                           join c in _context.Companies on p.CompanyId equals c.CompanyId
                           join cl in _context.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           join gm in _context.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                           join g in _context.Groups on gm.GroupId equals g.GroupId
                           where !p.Trashed && !c.Trashed && !fh.Trashed && g.GroupId == id
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                            .ToList();

            for (int k = 0; k < package.Count(); k++)
            {
                var currpackage = package.ElementAt(k);

                var munazzim = _context.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == currpackage.Company.CompanyId);


                model.Package = currpackage.Package.Name;
                model.Company = currpackage.Company.CompanyName;
                model.CompanyLicenseNo = currpackage.CompanyLicense.CompanyLicense;
                model.Munazzim = munazzim.Munazzim.Name;
            }




            var members = _context.FamilyMembers
    .Join(_context.FamilyHeads,
        fm => fm.FamilyHeadId,
        fh => fh.FamilyHeadId,
        (fm, fh) => new { fm, fh })
    .Join(_context.Packages,
        combined => combined.fh.PackageId,
        p => p.PackageId,
        (combined, p) => new { combined.fm, combined.fh, p })
    .Join(_context.Companies,
        combined => combined.p.CompanyId,
        c => c.CompanyId,
        (combined, c) => new { combined.fm, combined.fh, combined.p, c })
    .Join(_context.GroupMembers,
        combined => combined.fh.FamilyHeadId,
        gm => gm.FamilyHeadId,
        (combined, gm) => new { combined.fm, combined.fh, combined.p, combined.c, gm })
    .Join(_context.Groups,
        combined => combined.gm.GroupId,
        g => g.GroupId,
        (combined, g) => new { combined.fm, combined.fh, combined.p, combined.c, combined.gm, g })
    .Where(combined => !combined.fh.Trashed && combined.g.GroupId == id)
    .OrderBy(combined => combined.fm.HeadId)
    .ThenBy(combined => combined.fm.FamilyHeadId)
    .Select(combined => combined.fm)
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



                var result = (from fh in _context.FamilyHeads
                              join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _context.Packages on fh.PackageId equals p.PackageId
                              join pa in _context.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _context.Places on pa.PlaceId equals pl.PlaceId
                              join h in _context.Hotels on pl.PlaceId equals h.PlaceId
                              join gm in _context.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                              join g in _context.Groups on gm.GroupId equals g.GroupId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h,
                                  Group = g
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.Group.GroupId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";

                int pax = _context.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _context.GroupMembers
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


        public IActionResult MinaRoomListingSource(int id)
        {
            ViewBag.SubGroupId = id;
            var result = (
            from query in (
                from fh in _context.FamilyHeads
                join sgm in _context.SubGroupMembers on fh.FamilyHeadId equals sgm.FamilyHeadId
                join fhad in _context.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhad.FamilyHeadId into fhadJoin
                from fhad in fhadJoin.DefaultIfEmpty()
                join adf in _context.AdditionalFacilities on fhad.AdditionalFacilitiesId equals adf.AdditionalFacilitiesId into adfJoin
                from adf in adfJoin.DefaultIfEmpty()
                join r in _context.Rooms on adf.RoomId equals r.RoomId into rJoin
                from r in rJoin.DefaultIfEmpty()
                join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                join fhe in _context.FamilyHeadEmails on fh.FamilyHeadId equals fhe.FamilyHeadId into fheJoin
                from fhe in fheJoin.DefaultIfEmpty()
                join p in _context.Packages on fh.PackageId equals p.PackageId
                join pa in _context.PackageAccomodtions on fh.PackageId equals pa.PackageId
                join sg in _context.SubGroups on sgm.SubGroupId equals sg.SubGroupId
                join c in _context.Companies on p.CompanyId equals c.CompanyId
                join mc in _context.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
                join h in _context.Hotels on pa.HotelId equals h.HotelId
                where sg.SubGroupId == id
                    && pa.PlaceId == 3
                    && ((_context.FamilyHeadAdditionalFacilities.Include(e => e.AdditionalFacilities)
                    .Where(e => e.FamilyHeadId == fh.FamilyHeadId && e.AdditionalFacilities.PlaceId == 3)
                    .Count() == 0) || adf.PlaceId == 3)
                    && !fh.Trashed
                    && !r.Trashed
                    && (fhe != null && fhe.Preferred)
                select new
                {
                    FamilyHeadId = fh.FamilyHeadId,
                    PackageAccomodationsId = pa.PackageAccomodationsId,
                    MunazimName = m.Name,
                    CompanyName = c.CompanyName,
                    GroupName = sg.Name,
                    PackageName = p.Name,
                    HotelName = h.HotelName,
                    CheckIn = pa.ActualCheckInTime.HasValue ? pa.ActualCheckInTime.Value.Date : (DateTime?)null,
                    CheckOut = pa.ActualCheckOutTime.HasValue ? pa.ActualCheckOutTime.Value.Date : (DateTime?)null,
                    FcId = fh.FcId,
                    MemberImage = fh.MemberImage,
                    FullName = fh.GivenName + " " + fh.SurName,
                    Gender = fh.Gender,
                    Email = _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == fm.HeadId).Select(e => e.Email).FirstOrDefault(),
                    KSAMobileNumber = fh.KSAMobileNumber,
                    PassportNumber = fh.PassportNumber,
                    VisaNumber = fh.VisaNumber,
                    RoomCode = r.RoomCode,
                    HajjApplicationNumber = fh.HajjApplicationNumber,
                    HeadId = fm.HeadId,
                    RoomName = r.RoomName,
                    PAX = r.PAX,
                    RowSpan = 0,
                    CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                   $".{fm.Head.ExtractedFamilyNumber}.({_context.FamilyMembers.Count(e => e.HeadId == fm.HeadId)})" +
                   $".{fm.FamilyHead.Package.Name}",
                }
            )
            select new
            {
                query.FamilyHeadId,
                query.PackageAccomodationsId,
                query.MunazimName,
                query.CompanyName,
                query.GroupName,
                query.PackageName,
                query.HotelName,
                query.CheckIn,
                query.CheckOut,
                query.FcId,
                query.MemberImage,
                query.FullName,
                query.Gender,
                query.Email,
                query.KSAMobileNumber,
                query.PassportNumber,
                query.VisaNumber,
                RoomCodeNew = (_context.AdditionalFacilities
    .Any(adf => adf.PlaceId == 3 && _context.FamilyHeadAdditionalFacilities
        .Any(e => e.FamilyHeadId == query.FamilyHeadId && e.AdditionalFacilitiesId == adf.AdditionalFacilitiesId)))
            ? query.RoomCode
            : "Sharing",
                query.HajjApplicationNumber,
                query.HeadId,
                query.RoomName,
                query.PAX,
                query.RowSpan,
                query.CID


            }
        ).Distinct().OrderBy(e => e.FcId).ToList();


            ViewBag.Modal = result;
            if (result.Count > 0)
            {
                ViewBag.CheckIn = result[0].CheckIn;
                ViewBag.CheckOut = result[0].CheckOut;
                ViewBag.MunazimName = result[0].MunazimName;
                ViewBag.CompanyName = result[0].CompanyName;
                ViewBag.HotelName = result[0].HotelName;
                ViewBag.GroupName = result[0].GroupName;


            }

            int totalmemebers = 0;
            int malecount = 0;
            int femalecount = 0;
            int dblroomcount = 0;
            int triroomcount = 0;
            int quadroomcount = 0;
            int quintroomcount = 0;
            int sharingroomcount = 0;
            foreach (var item in result)
            {
                totalmemebers = totalmemebers + 1;
                if (item.Gender == (Gender)0)
                {
                    malecount = malecount + 1;
                }
                else
                {
                    femalecount = femalecount + 1;
                }

                if (item.RoomCodeNew == "DB-02")
                {
                    dblroomcount = dblroomcount + 1;
                }
                else if (item.RoomCodeNew == "TR-3")
                {
                    triroomcount = triroomcount + 1;
                }
                else if (item.RoomCodeNew == "QD-4")
                {
                    quadroomcount = quadroomcount + 1;
                }
                else if (item.RoomCodeNew == "QT-5")
                {
                    quintroomcount = quintroomcount + 1;
                }
                else
                {
                    sharingroomcount = sharingroomcount + 1;
                }
            }

            ViewBag.dblroom = (float)dblroomcount / 2;
            ViewBag.triroom = (float)triroomcount / 3;
            ViewBag.quadroom = (float)quadroomcount / 4;
            ViewBag.quintroom = (float)quintroomcount / 5;
            ViewBag.sharingroom = (float)sharingroomcount / 4;

            ViewBag.FemaleCount = femalecount;
            ViewBag.MaleCount = malecount;
            ViewBag.TotalMember = totalmemebers;

            ViewBag.rooms = _context.SubGroupMemberRooms
                    .Where(sgmr => sgmr.SubGroupId == id)
                    .ToList();

            var visapassport = from sgm in _context.SubGroupMembers
                               join fh in _context.FamilyHeads on sgm.FamilyHeadId equals fh.FamilyHeadId
                               where sgm.SubGroupId == id
                               select new
                               {
                                   fh.PassportImage,
                                   fh.VisaImage
                               };

            var resultList = visapassport.ToList();

            List<visapassportmodel> visamodel = new List<visapassportmodel>();

            foreach (var item in resultList)
            {
                string passportPath = Path.Combine(_host.WebRootPath, item.PassportImage == null ? "" : item.PassportImage);
                string VisaPath = Path.Combine(_host.WebRootPath, item.VisaImage == null ? "" : item.VisaImage);
                if (System.IO.File.Exists(passportPath) && System.IO.File.Exists(VisaPath))
                {
                    visamodel.Add(new visapassportmodel
                    {
                        visa = item.VisaImage,
                        passport = item.PassportImage
                    });
                }

            }
            ViewBag.passportVisa = visamodel;
            return View();
        }

        [HttpPost]
        public IActionResult MinaSave(int id, MedinaRoomListSaveModel request)
        {
            _context.SubGroupMemberRooms.Where(e => e.SubGroupId == id).ExecuteDelete();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {

                _context.SubGroupMemberRooms.Add(new SubGroupMemberRooms
                {
                    FamilyHeadId = request.FamilyHeadIds[i],
                    SubGroupId = id,
                    PlaceId = 3,
                    RoomNumber = request.StayRoomNumbers[i]?.Trim(),
                    FloorNumber = request.StayFloorNumbers[i]?.Trim(),
                });
            }
            _context.SaveChanges();

            return RedirectToAction("MinaIndex", "SubGroup");
        }

        public IActionResult Addresses(int id)
        {
            ViewBag.GroupName = _context.GroupsWithoutTrash
                .Where(e => e.GroupId == id)
                .Select(e => e.Name)
                .FirstOrDefault();

            var familyMembers = (from gm in _context.GroupMembers
                                 join fm in _context.FamilyMembers on gm.FamilyHeadId equals fm.FamilyHeadId
                                 join fh in _context.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                                 where gm.GroupId == id && !fh.Trashed
                                 select new
                                 {
                                     FamilyHeadId = fh.FamilyHeadId,
                                     HeadId = fm.HeadId,
                                     Gender = fh.Gender
                                 }).ToList();

            var memberCounts = familyMembers
                .GroupBy(m => m.HeadId)
                .Select(g => new
                {
                    FamilyHeadId = g.First().FamilyHeadId,
                    HeadId = g.Key,
                    PAX = g.Count(),
                    MaleCount = g.Count(m => m.Gender == Gender.Male),
                    FemaleCount = g.Count(m => m.Gender == Gender.Female)
                })
                .ToList();

            var query = (from gm in _context.GroupMembers
                         join fm in _context.FamilyMembers on gm.FamilyHeadId equals fm.FamilyHeadId
                         join fh in _context.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                         join fha in _context.FamilyHeadAddresses on fh.FamilyHeadId equals fha.FamilyHeadId into fhaGroup
                         from fha in fhaGroup.DefaultIfEmpty()
                         join p in _context.Packages on fh.PackageId equals p.PackageId
                         join fhc in _context.FamilyHeadContacts on fh.FamilyHeadId equals fhc.FamilyHeadId into fhcGroup
                         from fhc in fhcGroup.DefaultIfEmpty()
                         join fhe in _context.FamilyHeadEmails on fh.FamilyHeadId equals fhe.FamilyHeadId into fheGroup
                         from fhe in fheGroup.DefaultIfEmpty()
                         join rf in _context.References on fh.ReferenceId equals rf.ReferenceId into rfGroup
                         from rf in rfGroup.DefaultIfEmpty()
                         where (fha == null || fha.Preferred) && gm.GroupId == id && fh.IsHead
                               && (fhc == null || fhc.Preferred) && (fhe == null || fhe.Preferred)
                         select new
                         {
                             GroupMember = gm,
                             FamilyMember = fm,
                             FamilyHead = fh,
                             Address = fha,
                             Package = p,
                             Contact = fhc,
                             Email = fhe,
                             Reference = rf
                         }).ToList();

            var result = query.Select(e => new GroupData
            {
                HajjiName = e.FamilyHead.FullName,
                Address = e.Address?.Address,
                PassportNo = e.FamilyHead.PassportNumber,
                CNIC = e.FamilyHead.CNIC,
                ReferenceName = e.Reference?.ReferenceName,
                ApplicantType = e.FamilyHead.ApplicantType.ToString(),
                PackageName = e.Package?.Name,
                Email = e.Email?.Email,
                Phone = e.Contact?.Contact,
                PAX = memberCounts
                    .Where(x => x.FamilyHeadId == e.FamilyHead.FamilyHeadId)
                    .Select(x => x.PAX)
                    .FirstOrDefault(),
                MaleCount = memberCounts
                    .Where(x => x.FamilyHeadId == e.FamilyHead.FamilyHeadId)
                    .Select(x => x.MaleCount)
                    .FirstOrDefault(),
                FemaleCount = memberCounts
                    .Where(x => x.FamilyHeadId == e.FamilyHead.FamilyHeadId)
                    .Select(x => x.FemaleCount)
                    .FirstOrDefault()
            }).ToList();

            return View(result);
        }
    }

    public record GroupData
    {
        public string HajjiName { get; set; }
        public string Address { get; set; }
        public string PassportNo { get; set; }
        public string PackageName { get; set; }
        public string ReferenceName { get; set; }
        public string CNIC { get; set; }
        public string ApplicantType { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int PAX { get; set; }
    }

    public class visapassportmodel
    {
        public string visa { get; set; }
        public string passport { get; set; }
    }
    public record MedinaRoomListSaveModel
    {
        public List<int> FamilyHeadIds { get; set; }
        public List<int> RoomIds { get; set; }
        public List<string> StayFloorNumbers { get; set; }
        public List<string> StayRoomNumbers { get; set; }
        public List<string> ConnectedFloorNumbers { get; set; }
        public List<string> ConnectedRoomNumbers { get; set; }
    }
}

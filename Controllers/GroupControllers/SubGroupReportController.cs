using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.PackageModels;
using TMG.Models.ReservationModels;
using iText.Commons.Actions.Contexts;
using iText.StyledXmlParser.Jsoup.Select;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.GroupControllers
{

    public class SubGroupReportController : Controller
    {
        public static int SubGroupId = 0;
        public static int HotelId = 0;
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;


        public SubGroupReportController(TMGDbContext context, IHttpContextAccessor http)
        {
            _dbContext = context;
            _http = http;
        }

        public IActionResult AziziaRoomListing(int id)
        {

            var subGroup = _dbContext.SubGroups
                                    .Select(e => new
                                    {
                                        e.PlaceId,
                                        e.SubGroupId,
                                        SubGroupName = e.Name,
                                        GroupName = e.Group.Name,
                                        e.Group.Package.Company.CompanyName,
                                        e.Group.PackageId
                                    })
                                    .FirstOrDefault(e => e.SubGroupId == id);

            var accommodation = _dbContext
                                .PackageAccomodtions
                                .Select(e => new
                                {
                                    e.Hotel.HotelName,
                                    e.PlaceId,
                                    e.PackageId,
                                    e.ActualCheckInTime,
                                    e.ActualCheckOutTime,
                                })
                                .FirstOrDefault(e => e.PackageId == subGroup.PackageId && e.PlaceId == subGroup.PlaceId);

            var subGroupReservation = _dbContext.Reservations.FirstOrDefault(e => e.SubGroupId == id);

            var subGroupReservationMembers = _dbContext.ReservationFamilyMembers
                                                .Include(e => e.FamilyHead)
                                                .Include(e => e.Floor)
                                                .Include(e => e.FloorRoom)
                                                .ThenInclude(e => e.Room)
                                                .Include(e => e.FamilyHead)
                                                .ThenInclude(e => e.Package)
                                                .ThenInclude(e => e.Company)
                                                .Where(e => e.FloorRoomId != null && e.ReservationId == subGroupReservation.ReservationId)
                                                .OrderBy(e => e.FloorId)
                                                .ThenBy(e => e.FloorRoomId)
                                                .GroupBy(e => new { e.FloorId, e.FloorRoomId })
                                                .ToList();

            AziziaRoomListingModel model = new()
            {
                CompanyName = subGroup.CompanyName,
                SubGroupName = subGroup.SubGroupName,
                GroupName = subGroup.GroupName,
                CheckIn = accommodation.ActualCheckInTime?.Date.ToString(),
                CheckOut = accommodation.ActualCheckOutTime?.Date.ToString(),
                HotelName = accommodation.HotelName,
            };

            for (int i = 0; i < subGroupReservationMembers.Count; i++)
            {
                var _currGroup = subGroupReservationMembers.ElementAt(i);

                for (int j = 0; j < _currGroup.Count(); j++)
                {
                    var _currMember = _currGroup.ElementAt(j);
                    var _currMemberHead = _dbContext.FamilyMembers
                                            .Include(e => e.Head)
                                            .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);

                    int memberCount = _dbContext.FamilyMembers
                                            .Count(e => e.HeadId == _currMemberHead.HeadId);

                    string cId =
                       $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                       $".{_currMemberHead.Head.ExtractedFamilyNumber}.({memberCount})" +
                       $".{_currMember.FamilyHead.Package.Name}";

                    var memberInGroup = _dbContext.GroupMembers
                            .Include(e => e.Group)
                            .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);
                    if (memberInGroup != null)
                    {
                        cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    }

                    var packageRoomType = _dbContext.FamilyHeadAdditionalFacilities
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
                        SrNo = (i + 1 * j + 2),
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
                        Email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.Preferred && e.FamilyHeadId == _currMember.FamilyHeadId)?.Email,
                        CID = cId
                    };

                    model.Members.Add(_memberModel);
                }
            }

            return View(model);
        }

        public IActionResult AziziaRoomListingSource(int id)
        {
            SubGroupId = id;

            var subGroup = _dbContext.SubGroups
                                .Include(e => e.Group)
                                .ThenInclude(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.SubGroupId,
                                    e.GroupId,
                                    e.Code,
                                    e.Name,
                                    GroupCode = e.Group.Code,
                                    GroupName = e.Group.Name,
                                    e.Group.Package.Company.CompanyName,
                                    e.Group.PackageId,
                                    e.PlaceId
                                })
                                .FirstOrDefault(e => e.SubGroupId == id);

            var subGroupRooms = _dbContext.SubGroupRooms
                                .Include(e => e.SubGroup)
                                .Include(e => e.Room)
                                .Where(e => e.SubGroupId == id)
                                .ToList();

            ViewBag.SubGroupName = subGroup?.Code;
            ViewBag.GroupCode = subGroup?.GroupCode;
            ViewBag.GroupName = subGroup?.GroupName;

            // return Ok(subGroupRooms);
            RoomListingModel model = new();
            model.SubGroupId = id;

            var query = from sgm in _dbContext.SubGroupMembers
                        join sg in _dbContext.SubGroups on sgm.SubGroupId equals sg.SubGroupId
                        join g in _dbContext.Groups on sg.GroupId equals g.GroupId
                        join fh in _dbContext.FamilyHeads on sgm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where sgm.SubGroupId == id
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            sgm.SubGroupId,
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

            var families = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.PackageId == subGroup.PackageId);
            var familyHeadAdditionalFacilities = _dbContext
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
                                                .Where(e => e.PackageId == subGroup.PackageId);

            var subGroupMemberRooms = _dbContext.SubGroupMemberRooms.Where(e => e.SubGroupId == id);
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
                                .FirstOrDefault(e => e.PlaceId == subGroup.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == subGroup.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == subGroup.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                var roomNumberData = _dbContext.ReservationFamilyMembers
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

            for (int i = 0; i < subGroupRooms.Count; i++)
            {
                var __ = subGroupRooms.ElementAt(i);

                model.Rooms.Add(new RoomListingRoom
                {
                    RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
                    Quantity = __.Quantity
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

            model.Company = subGroup.CompanyName;
            model.Group = subGroup.GroupName;
            model.SubGroup = subGroup.Name;

            //=====================================//
            if (model.Members.Count > 0)
            {


                var packageId = model.Members[0].PackageId;
                var placeId = 4; //azizia

                var hotelIds = _dbContext.PackageAccomodtions
                     .Where(pa => pa.PackageId == packageId && pa.PlaceId == placeId)
                     .Select(pa => pa.HotelId)
                     .ToList();

                int? firstHotelId = hotelIds.FirstOrDefault(); // Get the first hotel id if available, otherwise default to null or 0
                HotelId = Convert.ToInt32(firstHotelId);
                var floorNumbers = _dbContext.FloorRoom
                    .Where(fr => fr.Floor.HotelId == firstHotelId && fr.Floor != null && !fr.Floor.Trashed)
                    .Select(fr => fr.Floor.FloorNumber.ToString())
                    .Distinct()
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


        public IActionResult MakkahMedinaRoomListingSource(int id, int PlaceId)
        {
           

            var subGroup = _dbContext.SubGroups
                                .Include(e => e.Group)
                                .ThenInclude(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.SubGroupId,
                                    e.GroupId,
                                    e.Code,
                                    e.Name,
                                    GroupCode = e.Group.Code,
                                    GroupName = e.Group.Name,
                                    e.Group.Package.Company.CompanyName,
                                    e.Group.PackageId,
                                    e.PlaceId
                                })
                                .FirstOrDefault(e => e.SubGroupId == id);

            var subGroupRooms = _dbContext.SubGroupRooms
                                .Include(e => e.SubGroup)
                                .Include(e => e.Room)
                                .Where(e => e.SubGroupId == id && e.PlaceId == PlaceId)
                                .ToList();

            ViewBag.SubGroupName = subGroup.Code;
            ViewBag.GroupCode = subGroup.GroupCode;
            ViewBag.GroupName = subGroup.GroupName;

            // return Ok(subGroupRooms);
            RoomListingModel model = new();
            model.SubGroupId = id;

            var query = from sgm in _dbContext.SubGroupMembers
                        join sg in _dbContext.SubGroups on sgm.SubGroupId equals sg.SubGroupId
                        join g in _dbContext.Groups on sg.GroupId equals g.GroupId
                        join fh in _dbContext.FamilyHeads on sgm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where sgm.SubGroupId == subGroup.SubGroupId
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            sgm.SubGroupId,
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

            var families = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.PackageId == subGroup.PackageId);
            var familyHeadAdditionalFacilities = _dbContext
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
                                                .Where(e => e.PackageId == subGroup.PackageId);

            var subGroupMemberRooms = _dbContext.SubGroupMemberRooms.Where(e => e.SubGroupId == id);
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
                                .FirstOrDefault(e => e.PlaceId == subGroup.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == subGroup.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == subGroup.PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                string stayRoomNumber = subGroupMemberRooms
                    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                string stayFloorNumber = subGroupMemberRooms
                    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                string connectedRoomNumber = subGroupMemberRooms
                    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                string connectedFloorNumber = subGroupMemberRooms
                    .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;

                model.Members.Add(new RoomListingMember
                {
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
                    RoomNumber = stayRoomNumber,
                    FloorNumber = stayFloorNumber,
                    ConnectedFloorNumber = connectedFloorNumber,
                    ConnectedRoomNumber = connectedRoomNumber
                });
            }

            for (int i = 0; i < subGroupRooms.Count; i++)
            {
                var __ = subGroupRooms.ElementAt(i);

                model.Rooms.Add(new RoomListingRoom
                {
                    RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
                    Quantity = __.Quantity,
                    Pax = __.Room.PAX
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

            model.Company = subGroup.CompanyName;
            model.Group = subGroup.GroupName;
            model.SubGroup = subGroup.Name;

            return View(model);
        }

        [HttpGet]
        public JsonResult GetRooms(int floorNumber, int hotelId)
        {
            var roomNumbers = _dbContext.FloorRoom
                 .Where(fr => fr.Floor.FloorNumber == floorNumber && fr.Floor.HotelId == hotelId)
                             .Select(fr => new
                             {
                                 fr.FloorRoomId,
                                 RoomCode = $"{fr.RoomNumber} ({fr.Room.RoomCode})",
                                 pax = fr.Room.PAX
                             })
                             .ToList();


            return Json(roomNumbers);
        }

        public IActionResult RoomListing(int id)
        {
            var memberRooms = _dbContext
                .SubGroupMemberRooms
                .Include(e => e.SubGroup)
                .ThenInclude(e => e.Group)
                .Include(e => e.SubGroup)
                .Include(e => e.FamilyHead)
                .Include(e => e.Room)
                .Where(e => e.SubGroupId == id)
                .OrderBy(e => e.FloorNumber)
                .ThenBy(e => e.RoomNumber)
                .ToList();

            var subGroup = _dbContext.SubGroups
                .Include(e => e.Group)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .FirstOrDefault(e => e.SubGroupId == id);

            var memberKey = memberRooms.Where(e => e.FamilyHead.StayWithId is null).ToList();
            var memberStay = memberRooms.Where(e => e.FamilyHead.StayWithId is not null).ToList();


            FinalRoomListing model = new()
            {
                Company = subGroup.Group.Package.Company.CompanyName,
                SubGroup = subGroup.Name,
                Group = subGroup.Group.Name
            };

            for (int i = 0; i < memberKey.Count; i++)
            {
                var _ = memberKey.ElementAt(i);
                var _inhabitants = memberStay.Where(e => e.FamilyHead.StayWithId == _.FamilyHeadId).ToList();
                var _inhabitantsCount = _inhabitants.Count == 0 ? 1 : _inhabitants.Count;

                string html = $"""
                    <tr style="border-top: 3px solid black">
                        <td class="text-center">{i + 1}</td>
                        <td class="text-center">AKF-{_.FamilyHeadId}</td>
                        <td class="text-center">{_.SubGroup.Group.Code}</td>
                        <td class="text-center">{_.FamilyHead.FullName}</td>
                        <td class="text-center">{_.FamilyHead.FatherHusbandName}</td>
                        <td class="text-center">{_.FamilyHead.Gender}</td>
                        <td class="text-center">{_.FamilyHead.PassportNumber}</td>
                        <td class="text-center">{_.FamilyHead.VisaNumber}</td>
                        <td class="text-center" rowspan="{_inhabitants.Count() + 1}">{_.Room.RoomName} ({_.Room.RoomCode})</td>
                        <td class="text-center" rowspan="{_inhabitants.Count() + 1}">F #{_.FloorNumber}, R #{_.RoomNumber}</td>
                    </tr>
                """;

                if (_inhabitants?.Count > 0)
                {
                    for (int j = 0; j < _inhabitants?.Count; j++)
                    {
                        var _i = _inhabitants.ElementAt(j);
                        html += $"""
                        <tr>
                            <td class="text-center">{j + i + 2}</td>
                            <td class="text-center">AKF-{_i.FamilyHeadId}</td>
                            <td class="text-center">{_i.SubGroup.Group.Code}</td>
                            <td class="text-center">{_i.FamilyHead.FullName}</td>
                            <td class="text-center">{_i.FamilyHead.FatherHusbandName}</td>
                            <td class="text-center">{_i.FamilyHead.Gender}</td>
                            <td class="text-center">{_i.FamilyHead.PassportNumber}</td>
                            <td class="text-center">{_i.FamilyHead.VisaNumber}</td>
                        </tr>
                    """;
                    }

                }

                model.MemberGroups.Add(new FinalRoomMemberGroups
                {
                    Row = html
                });

            }

            return View(model);
        }

        public IActionResult MakkahMedinaSave(int id, MakkahMedinaRoomSaveModel request)
        {
            // return Ok(new {
            //     request
            // });
            _dbContext.SubGroupMemberRooms.Where(e => e.SubGroupId == id).ExecuteDelete();

            SubGroup subGroup = _dbContext.SubGroups.Find(id);

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                _dbContext.SubGroupMemberRooms.Add(new SubGroupMemberRooms
                {
                    GroupId = subGroup.GroupId,
                    FamilyHeadId = request.FamilyHeadIds[i],
                    // RoomId = request.RoomIds[i],
                    SubGroupId = id,
                    RoomNumber = request.StayRoomNumbers[i]?.Trim(),
                    FloorNumber = request.StayFloorNumbers[i]?.Trim(),
                    ConnectedFloorNumber = request.ConnectedFloorNumbers[i]?.Trim(),
                    ConnectedRoomNumber = request.ConnectedRoomNumbers[i]?.Trim(),
                });
            }
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "SubGroup");
        }

        public IActionResult Save(int id, RoomSaveModel request)
        {
            var accommodations = _dbContext.PackageAccomodtions
                .Join(
                    _dbContext.Groups,
                    pa => pa.PackageId,
                    g => g.PackageId,
                    (pa, g) => new { pa, g }
                )
                .Join(
                    _dbContext.SubGroups,
                    pair => pair.g.GroupId,
                    sg => sg.GroupId,
                    (pair, sg) => new { pair.pa, sg }
                )
                .Where(result => result.sg.SubGroupId == id && result.pa.PlaceId == 4)
                .Select(result => result.pa)
                .ToList();

            _dbContext.SubGroupMemberRooms.Where(e => e.SubGroupId == id).ExecuteDelete();

            var group = _dbContext.SubGroupsWithoutTrash.Where(e => e.SubGroupId == id).FirstOrDefault();

            var previousReservation = _dbContext.Reservations.Where(r => (r.SubGroupId == id || r.SubGroupId == null) && r.GroupId == group.GroupId);
            var reservationid = previousReservation.Count() > 0 ? previousReservation.FirstOrDefault().ReservationId : 0;
            _dbContext.ReservationFamilyMembers.Where(e => e.ReservationId == reservationid).ExecuteDelete();
            //_dbContext.Reservations.Where(e => e.ReservationId == reservationid).ExecuteDelete();
            if (reservationid != 0)
            {
                var reservation = _dbContext.Reservations.Where(e => e.ReservationId == reservationid);
                if (reservation != null)
                {
                    foreach (var item in reservation)
                    {
                        item.SubGroupId = id;
                    }
                    _dbContext.SaveChanges();
                }
            }
            else
            {
                Reservation newReservation = new Reservation
                {
                    HotelId = HotelId,
                    SubGroupId = id,
                    CheckInDate = accommodations.Count > 0 ? Convert.ToDateTime(accommodations[0].ActualCheckInTime) : DateTime.MinValue,
                    CheckOutDate = accommodations.Count > 0 ? Convert.ToDateTime(accommodations[0].ActualCheckOutTime) : DateTime.MinValue,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Trashed = false,
                    GroupId = group.GroupId
                };

                _dbContext.Reservations.Add(newReservation);
                _dbContext.SaveChanges();
                reservationid = newReservation.ReservationId;
            }

            //int reservationId = 




            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var floors = _dbContext.FloorRoom.Where(e => e.Floor.FloorNumber == Convert.ToInt32(request.StayFloorNumbers[i])).Select(e => e.FloorId).Distinct();
                var floorid = floors.Count() > 0 ? _dbContext.FloorRoom.Where(e => e.Floor.FloorNumber == Convert.ToInt32(request.StayFloorNumbers[i])).Select(e => e.FloorId).Distinct().FirstOrDefault() : 0;
                var floorRoomid = request.StayRoomNumbers[i];
                _dbContext.ReservationFamilyMembers.Add(new ReservationFamilyMembers
                {
                    FamilyHeadId = request.FamilyHeadIds[i],
                    FloorId = Convert.ToInt32(floorid),
                    FloorRoomId = floorRoomid,
                    ReservationId = reservationid
                });
            }
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "SubGroup");
        }

        public IActionResult MakkahMedinaRoomListing(int id)
        {
            var subGroup = _dbContext.SubGroups
                            .Select(e => new
                            {
                                e.SubGroupId,
                                e.Name,
                                e.GroupId,
                                GroupName = e.Group.Name,
                                e.PlaceId,
                                e.Place.PlaceName,
                                e.Group.Package.PackageId,
                                PackageName = e.Group.Package.Name,
                                e.Group.Package.CompanyId,
                                e.Group.Package.Company.CompanyCode,
                                e.Group.Package.Company.CompanyName,
                            })
                            .FirstOrDefault(e => e.SubGroupId == id);

            var hotel = _dbContext.PackageAccomodtions
                                .Select(e => new
                                {
                                    e.PackageId,
                                    e.PlaceId,
                                    e.Hotel.HotelName,
                                    e.ActualCheckInTime,
                                    e.ActualCheckOutTime
                                })
                                .FirstOrDefault(e => e.PackageId == subGroup.PackageId
                                && e.PlaceId == subGroup.PlaceId);

            var subGroupMemberRooms = _dbContext.SubGroupMemberRooms
                        .Join(
                            _dbContext.Groups,
                            s => s.GroupId,
                            g => g.GroupId,
                            (sgm, g) => new { sgm, g }
                        )
                        .Join(
                            _dbContext.Packages,
                            g => g.g.PackageId,
                            p => p.PackageId,
                            (g, p) => new { g, p }
                        )
                        .Join(
                            _dbContext.Companies,
                            p => p.p.CompanyId,
                            c => c.CompanyId,
                            (p, c) => new { p, c }
                        )
                        .Where(e => e.p.g.sgm.SubGroupId == id && e.p.g.sgm.FloorNumber != null && e.p.g.sgm.RoomNumber != null)
                        .GroupBy(e => new
                        {
                            e.p.g.sgm.FloorNumber,
                            e.p.g.sgm.RoomNumber
                        })
                        .ToList();

            return Ok(subGroupMemberRooms);

            NewRoomListingModel model = new()
            {
                SubGroupName = subGroup.Name,
                GroupName = subGroup.GroupName,
                CompanyName = subGroup.CompanyName,
                HotelName = hotel.HotelName,
                CheckIn = hotel.ActualCheckInTime?.ToString(),
                CheckOut = hotel.ActualCheckOutTime?.ToString(),
            };



            return Ok(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult MakkahMedinaRoomListing(NewRoomListingSaveModel request)
        {

            var members = _dbContext.FamilyHeads
                        .Where(e => request.FamilyHeadIds.Contains(e.FamilyHeadId))
                        .ToList();

            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                var familyHead = members.FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);

                familyHead.VisaNumber = request.VisaNumbers.ElementAt(i);
                familyHead.KSAMobileNumber = request.MOBKSAs.ElementAt(i);
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index", "SubGroup");
        }
        public IActionResult RiyazUlJaanahReport(int id)
        {
            //for Male
            var query = _dbContext.FamilyHeads
    .Join(_dbContext.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
    .Join(_dbContext.Companies, fph => fph.p.CompanyId, c => c.CompanyId, (fph, c) => new { fph.fh, fph.p, c })
    .Join(_dbContext.MunazzimCompanies, fpc => fpc.c.CompanyId, mc => mc.CompanyId, (fpc, mc) => new { fpc.fh, fpc.p, fpc.c, mc })
    .Join(_dbContext.Munazzims, fpcm => fpcm.mc.MunazzimId, m => m.MunazzimId, (fpcm, m) => new { fpcm.fh, fpcm.p, fpcm.c, fpcm.mc, m })
    .Join(_dbContext.SubGroupMembers, fpcmm => fpcmm.fh.FamilyHeadId, sgm => sgm.FamilyHeadId, (fpcmm, sgm) => new { fpcmm.fh, fpcmm.p, fpcmm.c, fpcmm.mc, fpcmm.m, sgm })
    .Join(_dbContext.SubGroups, fpcmm_sg => fpcmm_sg.sgm.SubGroupId, sg => sg.SubGroupId, (fpcmm_sg, sg) => new { fpcmm_sg.fh, fpcmm_sg.p, fpcmm_sg.c, fpcmm_sg.mc, fpcmm_sg.m, fpcmm_sg.sgm, sg })
    .Join(_dbContext.Groups, fpcmm_sg_g => fpcmm_sg_g.sg.GroupId, g => g.GroupId, (fpcmm_sg_g, g) => new { fpcmm_sg_g.fh, fpcmm_sg_g.p, fpcmm_sg_g.c, fpcmm_sg_g.mc, fpcmm_sg_g.m, fpcmm_sg_g.sgm, fpcmm_sg_g.sg, g })
    .Join(_dbContext.PNRMembers, fpcmm_sg_g_pnrm => fpcmm_sg_g_pnrm.fh.FamilyHeadId, pnrm => pnrm.FamilyHeadId, (fpcmm_sg_g_pnrm, pnrm) => new { fpcmm_sg_g_pnrm.fh, fpcmm_sg_g_pnrm.p, fpcmm_sg_g_pnrm.c, fpcmm_sg_g_pnrm.mc, fpcmm_sg_g_pnrm.m, fpcmm_sg_g_pnrm.sgm, fpcmm_sg_g_pnrm.sg, fpcmm_sg_g_pnrm.g, pnrm })
    .Join(_dbContext.PNRs, fpcmm_sg_g_pnrm_pnr => fpcmm_sg_g_pnrm_pnr.pnrm.PNRId, pnr => pnr.PNRId, (fpcmm_sg_g_pnrm_pnr, pnr) => new { fpcmm_sg_g_pnrm_pnr.fh, fpcmm_sg_g_pnrm_pnr.p, fpcmm_sg_g_pnrm_pnr.c, fpcmm_sg_g_pnrm_pnr.mc, fpcmm_sg_g_pnrm_pnr.m, fpcmm_sg_g_pnrm_pnr.sgm, fpcmm_sg_g_pnrm_pnr.sg, fpcmm_sg_g_pnrm_pnr.g, fpcmm_sg_g_pnrm_pnr.pnrm, pnr })
    .Join(_dbContext.Sectors, fpcmm_sg_g_pnrm_pnr_s => fpcmm_sg_g_pnrm_pnr_s.pnr.FlightId, s => s.FlightId, (fpcmm_sg_g_pnrm_pnr_s, s) => new { fpcmm_sg_g_pnrm_pnr_s.fh, fpcmm_sg_g_pnrm_pnr_s.p, fpcmm_sg_g_pnrm_pnr_s.c, fpcmm_sg_g_pnrm_pnr_s.mc, fpcmm_sg_g_pnrm_pnr_s.m, fpcmm_sg_g_pnrm_pnr_s.sgm, fpcmm_sg_g_pnrm_pnr_s.sg, fpcmm_sg_g_pnrm_pnr_s.g, fpcmm_sg_g_pnrm_pnr_s.pnrm, fpcmm_sg_g_pnrm_pnr_s.pnr, s })
    .Join(_dbContext.FamilyMembers, fpcmm_sg_g_pnrm_pnr_s_fm => fpcmm_sg_g_pnrm_pnr_s_fm.fh.FamilyHeadId, fm => fm.FamilyHeadId, (fpcmm_sg_g_pnrm_pnr_s_fm, fm) => new { fpcmm_sg_g_pnrm_pnr_s_fm.fh, fpcmm_sg_g_pnrm_pnr_s_fm.p, fpcmm_sg_g_pnrm_pnr_s_fm.c, fpcmm_sg_g_pnrm_pnr_s_fm.mc, fpcmm_sg_g_pnrm_pnr_s_fm.m, fpcmm_sg_g_pnrm_pnr_s_fm.sgm, fpcmm_sg_g_pnrm_pnr_s_fm.sg, fpcmm_sg_g_pnrm_pnr_s_fm.g, fpcmm_sg_g_pnrm_pnr_s_fm.pnrm, fpcmm_sg_g_pnrm_pnr_s_fm.pnr, fpcmm_sg_g_pnrm_pnr_s_fm.s, fm })
    
    .Where(fpcmm_sg_g => fpcmm_sg_g.sg.SubGroupId == id && fpcmm_sg_g.fh.Gender == (Gender)0)
    .AsEnumerable() // Switch to client-side evaluation from this point onward
    .Where(fpcmm_sg_g => fpcmm_sg_g.s.ArrivalDateTime == _dbContext.Sectors
                           .Where(x => x.FlightId == fpcmm_sg_g.pnr.FlightId && x.FlightType == (FlightType)1)
                           .Max(e => e.ArrivalDateTime))
    .OrderBy(fpcmm_sg_g => fpcmm_sg_g.fh.FamilyHeadId)
    .Select((x, index) => new
    {
        SerialNumber = index + 1,
        MunazzimName = x.m.Name,
        CompanyName = x.c.CompanyCode,
        PackageName = x.p.Name,
        Name = x.fh.GivenName + " " + x.fh.SurName,
        FamilyHeadId = x.fh.FamilyHeadId,
        EHajjId = x.fh.EHajjId,
        MofaNumber = x.fh.MofaNumber,
        VisaNumber = x.fh.VisaNumber,
        SubGroupName = x.sg.Name,
        FCid = x.fh.FcId,
        CID = $"{x.c.CompanyCode}.{x.fh.IndId}" + x.fh.FcId.Substring(3) +
               $".{x.fh.ExtractedFamilyNumber}.({_dbContext.FamilyMembers.Count(e => e.HeadId == x.fm.HeadId)})" +
               $".{x.p.Name}",
        Image = x.fh.MemberImage,
        Gender = x.fh.Gender,
        Passport = x.fh.PassportNumber,
        PNRName = x.pnr.Name,
        ArrivalDateTime = x.s.ArrivalDateTime,
        DepartureDateTime = x.s.DepartureDateTime
    }).OrderBy(x => x.FCid)
    .ToList();


            ViewBag.report = query;


            ViewBag.report = query;


            ViewBag.report = query;
            ViewBag.MunazzimName = query[0].MunazzimName;
            ViewBag.CompanyName = query[0].CompanyName;
            ViewBag.PackageName = query[0].PackageName;
            ViewBag.SubGroupName = query[0].SubGroupName;
            return View();
        }

        public IActionResult RiyazUlJaanahReportFemale(int id)
        {
            //for Male
            var query = _dbContext.FamilyHeads
    .Join(_dbContext.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
    .Join(_dbContext.Companies, fph => fph.p.CompanyId, c => c.CompanyId, (fph, c) => new { fph.fh, fph.p, c })
    .Join(_dbContext.MunazzimCompanies, fpc => fpc.c.CompanyId, mc => mc.CompanyId, (fpc, mc) => new { fpc.fh, fpc.p, fpc.c, mc })
    .Join(_dbContext.Munazzims, fpcm => fpcm.mc.MunazzimId, m => m.MunazzimId, (fpcm, m) => new { fpcm.fh, fpcm.p, fpcm.c, fpcm.mc, m })
    .Join(_dbContext.SubGroupMembers, fpcmm => fpcmm.fh.FamilyHeadId, sgm => sgm.FamilyHeadId, (fpcmm, sgm) => new { fpcmm.fh, fpcmm.p, fpcmm.c, fpcmm.mc, fpcmm.m, sgm })
    .Join(_dbContext.SubGroups, fpcmm_sg => fpcmm_sg.sgm.SubGroupId, sg => sg.SubGroupId, (fpcmm_sg, sg) => new { fpcmm_sg.fh, fpcmm_sg.p, fpcmm_sg.c, fpcmm_sg.mc, fpcmm_sg.m, fpcmm_sg.sgm, sg })
    .Join(_dbContext.Groups, fpcmm_sg_g => fpcmm_sg_g.sg.GroupId, g => g.GroupId, (fpcmm_sg_g, g) => new { fpcmm_sg_g.fh, fpcmm_sg_g.p, fpcmm_sg_g.c, fpcmm_sg_g.mc, fpcmm_sg_g.m, fpcmm_sg_g.sgm, fpcmm_sg_g.sg, g })
    .Join(_dbContext.PNRMembers, fpcmm_sg_g_pnrm => fpcmm_sg_g_pnrm.fh.FamilyHeadId, pnrm => pnrm.FamilyHeadId, (fpcmm_sg_g_pnrm, pnrm) => new { fpcmm_sg_g_pnrm.fh, fpcmm_sg_g_pnrm.p, fpcmm_sg_g_pnrm.c, fpcmm_sg_g_pnrm.mc, fpcmm_sg_g_pnrm.m, fpcmm_sg_g_pnrm.sgm, fpcmm_sg_g_pnrm.sg, fpcmm_sg_g_pnrm.g, pnrm })
    .Join(_dbContext.PNRs, fpcmm_sg_g_pnrm_pnr => fpcmm_sg_g_pnrm_pnr.pnrm.PNRId, pnr => pnr.PNRId, (fpcmm_sg_g_pnrm_pnr, pnr) => new { fpcmm_sg_g_pnrm_pnr.fh, fpcmm_sg_g_pnrm_pnr.p, fpcmm_sg_g_pnrm_pnr.c, fpcmm_sg_g_pnrm_pnr.mc, fpcmm_sg_g_pnrm_pnr.m, fpcmm_sg_g_pnrm_pnr.sgm, fpcmm_sg_g_pnrm_pnr.sg, fpcmm_sg_g_pnrm_pnr.g, fpcmm_sg_g_pnrm_pnr.pnrm, pnr })
    .Join(_dbContext.Sectors, fpcmm_sg_g_pnrm_pnr_s => fpcmm_sg_g_pnrm_pnr_s.pnr.FlightId, s => s.FlightId, (fpcmm_sg_g_pnrm_pnr_s, s) => new { fpcmm_sg_g_pnrm_pnr_s.fh, fpcmm_sg_g_pnrm_pnr_s.p, fpcmm_sg_g_pnrm_pnr_s.c, fpcmm_sg_g_pnrm_pnr_s.mc, fpcmm_sg_g_pnrm_pnr_s.m, fpcmm_sg_g_pnrm_pnr_s.sgm, fpcmm_sg_g_pnrm_pnr_s.sg, fpcmm_sg_g_pnrm_pnr_s.g, fpcmm_sg_g_pnrm_pnr_s.pnrm, fpcmm_sg_g_pnrm_pnr_s.pnr, s })
    .Join(_dbContext.FamilyMembers, fpcmm_sg_g_pnrm_pnr_s_fm => fpcmm_sg_g_pnrm_pnr_s_fm.fh.FamilyHeadId, fm => fm.FamilyHeadId, (fpcmm_sg_g_pnrm_pnr_s_fm, fm) => new { fpcmm_sg_g_pnrm_pnr_s_fm.fh, fpcmm_sg_g_pnrm_pnr_s_fm.p, fpcmm_sg_g_pnrm_pnr_s_fm.c, fpcmm_sg_g_pnrm_pnr_s_fm.mc, fpcmm_sg_g_pnrm_pnr_s_fm.m, fpcmm_sg_g_pnrm_pnr_s_fm.sgm, fpcmm_sg_g_pnrm_pnr_s_fm.sg, fpcmm_sg_g_pnrm_pnr_s_fm.g, fpcmm_sg_g_pnrm_pnr_s_fm.pnrm, fpcmm_sg_g_pnrm_pnr_s_fm.pnr, fpcmm_sg_g_pnrm_pnr_s_fm.s, fm })

    .Where(fpcmm_sg_g => fpcmm_sg_g.sg.SubGroupId == id && fpcmm_sg_g.fh.Gender == (Gender)1)
    .AsEnumerable() // Switch to client-side evaluation from this point onward
    .Where(fpcmm_sg_g => fpcmm_sg_g.s.ArrivalDateTime == _dbContext.Sectors
                           .Where(x => x.FlightId == fpcmm_sg_g.pnr.FlightId && x.FlightType == (FlightType)1)
                           .Max(e => e.ArrivalDateTime))
    .OrderBy(fpcmm_sg_g => fpcmm_sg_g.fh.FamilyHeadId)
    .Select((x, index) => new
    {
        SerialNumber = index + 1,
        MunazzimName = x.m.Name,
        CompanyName = x.c.CompanyCode,
        PackageName = x.p.Name,
        Name = x.fh.GivenName + " " + x.fh.SurName,
        FamilyHeadId = x.fh.FamilyHeadId,
        EHajjId = x.fh.EHajjId,
        MofaNumber = x.fh.MofaNumber,
        VisaNumber = x.fh.VisaNumber,
        SubGroupName = x.sg.Name,
        FCid = x.fh.FcId,
        CID = $"{x.c.CompanyCode}.{x.fh.IndId}" + x.fh.FcId.Substring(3) +
               $".{x.fh.ExtractedFamilyNumber}.({_dbContext.FamilyMembers.Count(e => e.HeadId == x.fm.HeadId)})" +
               $".{x.p.Name}",
        Image = x.fh.MemberImage,
        Gender = x.fh.Gender,
        Passport = x.fh.PassportNumber,
        PNRName = x.pnr.Name,
        ArrivalDateTime = x.s.ArrivalDateTime,
        DepartureDateTime = x.s.DepartureDateTime
    }).OrderBy(x => x.FCid)
    .ToList();


            ViewBag.report = query;


            ViewBag.report = query;


            ViewBag.report = query;
            ViewBag.MunazzimName = query[0].MunazzimName;
            ViewBag.CompanyName = query[0].CompanyName;
            ViewBag.PackageName = query[0].PackageName;
            ViewBag.SubGroupName = query[0].SubGroupName;
            return View();
        }
    }

}

namespace TMG.Models.GroupModels
{

    public record MakkahMedinaRoomSaveModel
    {
        public List<int> FamilyHeadIds { get; set; }
        public List<int> RoomIds { get; set; }
        public List<string> StayFloorNumbers { get; set; }
        public List<string> StayRoomNumbers { get; set; }
        public List<string> ConnectedFloorNumbers { get; set; }
        public List<string> ConnectedRoomNumbers { get; set; }
    }

    public record AziziaRoomListingModel
    {
        public string CompanyName { get; set; }
        public string GroupName { get; set; }
        public string SubGroupName { get; set; }
        public string HotelName { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public List<AziziaRoomListingMemberModel> Members { get; set; } = new();
    }

    public record AziziaRoomListingMemberModel
    {
        public bool IsHead { get; set; }
        public int FamilyHeadId { get; set; }
        public int SrNo { get; set; }
        public string FcId { get; set; }
        public string PackageRoomType { get; set; }
        public string MegaId { get; set; }
        public string CID { get; set; }
        public string Photo { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string MOBKSA { get; set; }
        public string PP { get; set; }
        public string Visa { get; set; }
        public string RoomType { get; set; }
        public string FloorNumber { get; set; }
        public string RoomNumber { get; set; }
        public string Remarks { get; set; }
        public int RowSpan { get; set; }
    }


    public record NewRoomListingSaveModel
    {
        public List<int> FamilyHeadIds { get; set; }
        public List<string> MOBKSAs { get; set; }
        public List<string> VisaNumbers { get; set; }
    }
    public record NewRoomListingModel
    {
        public string CompanyName { get; set; }
        public string SubGroupName { get; set; }
        public string GroupName { get; set; }
        public string HotelName { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }

        public int TotalMembers { get; set; }

        public int TotalMale { get; set; }
        public int TotalMaleAdult { get; set; }
        public int TotalMaleChild { get; set; }
        public int TotalMaleInfant { get; set; }

        public int TotalFemale { get; set; }
        public int TotalFemaleAdult { get; set; }
        public int TotalFemaleChild { get; set; }
        public int TotalFemaleInfant { get; set; }

        public List<NewRoomListingMemberModel> Members { get; set; } = new();
    }

    public record NewRoomListingMemberModel
    {
        public bool IsHead { get; set; }
        public int FamilyHeadId { get; set; }
        public int SrNo { get; set; }
        public string MegaId { get; set; }
        public string CID { get; set; }
        public string Photo { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string MOBKSA { get; set; }
        public string PP { get; set; }
        public string Visa { get; set; }
        public string RoomType { get; set; }
        public string FloorNumber { get; set; }
        public string RoomNumber { get; set; }
        public string Remarks { get; set; }
        public int RowSpan { get; set; }
    }

    public record RoomListingModel
    {
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public string Company { get; set; }
        public string SubGroup { get; set; }
        public string Group { get; set; }
        public List<RoomListingMember> Members { get; set; } = new();
        public List<RoomListingRoom> Rooms { get; set; } = new();
        public RoomListingMemberSummary MemberSummary { get; set; } = new();

        public RoomCalculation RoomCalculation { get; set; } = new();
        public List<RoomAssignment> RoomAssignment { get; set; } = new List<RoomAssignment>();
        public List<VehicleStayWithCountViewModel> VehicleStayWithCountViewModel { get; set; } = new List<VehicleStayWithCountViewModel>();
    }

    public class RoomAssignment
    {
        public string RoomName { get; set; }
        public int Quantity { get; set; }
    }

    public class VehicleStayWithCountViewModel
    {
        public string VehicleName { get; set; }
        public string CheckInDate { get; set; }
        public int? RouteId { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public string ArrivalTime { get; set; }
        public int StayWithCount { get; set; }
    }


    public record RoomSaveModel
    {
        public int HotelId { get; set; }
        public int SubGroupId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int GroupId { get; set; }
        public int ItineraryBlockId { get; set; }
        public List<int> FamilyHeadIds { get; set; }
        public List<int> RoomIds { get; set; }
        public List<string?> StayFloorNumbers { get; set; }
        public List<int?> StayRoomNumbers { get; set; }
        public List<int?> VehicleNumbers { get; set; }
        public List<string?> ConnectedFloorNumbers { get; set; }
        public List<string?> ConnectedRoomNumbers { get; set; }
        public List<string?> StayWith { get; set; }
        public List<int?> NoofVehicle { get; set; }
        public List<DateTime?> FlightArrival { get; set; }
        public List<DateTime?> ArrivalDate { get; set; }
        public List<DateTime?> FlightDeparture { get; set; }
        public List<DateTime?> DepartureDate { get; set; }
        public List<DateTime?> TrainArrival { get; set; }
        public List<DateTime?> TrainDeparture { get; set; }
        public List<DateTime?> ActualCheckIn { get; set; }
        public List<DateTime?> ActualCheckOut { get; set; }
        public List<string?> ConnectedNumber { get; set; }
        public List<string?> ArrivalFlightNumber { get; set; }
        public List<string?> ConnectedVehicleNumber { get; set; }
        public List<int?> StayFloorRoomId { get; set; }
    }

    public record RoomListingMember
    {
        public bool IsHead { get; set; }
        public int FamilyHeadId { get; set; }
        public int GroupId { get; set; }
        public int? RoomId { get; set; }
        public string PassportImage { get; set; }
        public string PassporFileExtension { get; set; }
        public string VisaImage { get; set; }
        public string FileExtension { get; set; }
        public string VisaNumber { get; set; }
        public string Email { get; set; }
        public string CompanyNames { get; set; }
        public string HotelName { get; set; }
        public string NextHotelName { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        public string CheckInForAirline { get; set; }
        public string CheckOutForAirline { get; set; }
        public string CheckInForTrain { get; set; }
        public string CheckOutForTrain { get; set; }
        public string CheckOutForReservation { get; set; }
        public string TrainNumber { get; set; }

        public DateTime? FlightArrival { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? FlightDeparture { get; set; }
        public DateTime? DepartureDate { get; set; }

        public DateTime? TrainDeparture { get; set; }
        public DateTime? TrainArrival { get; set; }
        public DateTime? ActualCheckIn { get; set; }
        public DateTime? ActualCheckOut { get; set; }

        public int SNo { get; set; }
        public string Company { get; set; }
        public string Package { get; set; }
        public string Group { get; set; }
        public string RoomType { get; set; }
        public int RoomPax { get; set; }
        public string FcId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Passport { get; set; }
        public string FloorNumber { get; set; }
        public string RoomNumber { get; set; }
        public string StayWith { get; set; }
        public int? NoofVehicle { get; set; }
        public string ConnectedFloorNumber { get; set; }
        public string ConnectedRoomNumber { get; set; }
        public string ConnectedNumber { get; set; }
        public string ConnectedVehicleNumber { get; set; }
        public int PackageId { get; set; }
        public int HotelId { get; set; }

        public int? FloorId { get; set; }
        public int? FloorRoomId { get; set; }
        public int? RoomNumberPax { get; set; }
        public string RoomNumberWithCode { get; set; }
        public string RouteName { get; set; }
        public int? VehicleId { get; set; }
        public int? TranspotationVehicleId { get; set; }
        public string Arrival { get; set; }
        public string Departure { get; set; }
        public string GroupName { get; set; }
    }

    public record RoomListingMemberSummary
    {
        public int TotalMembers { get; set; }
        public int MaleCount { get; set; }
        public int AdultMales { get; set; }
        public int ChildMales { get; set; }
        public int InfantMales { get; set; }

        public int FemaleCount { get; set; }
        public int AdultFemales { get; set; }
        public int ChildFemales { get; set; }
        public int InfantFemales { get; set; }
    }

    public record RoomListingRoom
    {
        public string RoomCode { get; set; }
        public int Quantity { get; set; }
        public int Pax { get; set; }
    }

    public record FinalRoomListing
    {
        public string Company { get; set; }
        public string SubGroup { get; set; }
        public string Group { get; set; }

        public List<FinalRoomMemberGroups> MemberGroups { get; set; } = new();
    }

    public record FinalRoomMemberGroups
    {
        public string Row { get; set; }
        public int SNo { get; set; }
        public string FcId { get; set; }
        public string GroupCode { get; set; }
        public string FullName { get; set; }
        public string FatherHusbandName { get; set; }
        public string Gender { get; set; }
        public string Passport { get; set; }
        public string VisaNumber { get; set; }
        public string RoomType { get; set; }
        public string RoomNumber { get; set; }

    }
}
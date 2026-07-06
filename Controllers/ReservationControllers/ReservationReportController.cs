using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TMG.Controllers.ReservationControllers
{
    public class ReservationReportController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public ReservationReportController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult HotelRoomSummary(int HotelId, string MedinaArrival)
        {
            HotelRoomSummaryModel model = new()
            {
                MedinaArrival = MedinaArrival + " HAJJ"
            };

            var rooms = _dbContext.RoomsWithoutTrash.AsNoTracking().ToList();
            var hotels = _dbContext.HotelsWithoutTrash.Where(e => e.HotelId == HotelId).AsNoTracking().ToList();

            var accommodations = _dbContext.PackageAccomodtions
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .AsNoTracking()
                                .ToList();

            var subGroupMembers = _dbContext.SubGroupMembers
                                    .Include(e => e.FamilyHead)
                                    .Include(e => e.SubGroup)
                                    .ThenInclude(e => e.Group)
                                    .ThenInclude(e => e.Package)
                                    .Where(e => !e.SubGroup.Trashed
                                            && !e.SubGroup.Group.Trashed
                                            && !e.FamilyHead.Trashed
                                            && !e.SubGroup.Group.Package.Trashed)
                                    .AsNoTracking()
                                    .Select(e => new
                                    {
                                        e.FamilyHeadId,
                                        e.SubGroup.Group.PackageId,
                                        e.SubGroup.PlaceId,
                                        e.SubGroup.Group.Package.MedinahArrival
                                    })
                                    .ToList();

            var iadfs = _dbContext.FamilyHeadAdditionalFacilities
                .Include(e => e.FamilyHead)
                .Include(e => e.AdditionalFacilities)
                .ThenInclude(e => e.Package)
                .Include(e => e.AdditionalFacilities)
                .ThenInclude(e => e.Room)
                .Where(e => !e.FamilyHead.Trashed)
                .AsNoTracking()
                .ToList();

            var subGroupMembersIadfsQuery =
                                           from fh in _dbContext.FamilyHeads
                                           join fhaf in _dbContext.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhaf.FamilyHeadId
                                           join af in _dbContext.AdditionalFacilities on fhaf.AdditionalFacilitiesId equals af.AdditionalFacilitiesId
                                           join r in _dbContext.Rooms on af.RoomId equals r.RoomId
                                           join p in _dbContext.Packages on af.PackageId equals p.PackageId
                                           join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                           where !fh.Trashed
                                           select new
                                           {
                                               r.RoomCode,
                                               fh,
                                               fhaf,
                                               af,
                                               p,
                                               pa
                                           };

            var subGroupMembersIadfs = subGroupMembersIadfsQuery.ToList();

            var checkIns = accommodations
                    .Where(e => e.ActualCheckInTime != null
                    && e.HotelId == HotelId
                    && e.ActualCheckInTime.Value.Year == 2024
                    && e.Package.MedinahArrival == MedinaArrival)
                    .OrderBy(e => e.ActualCheckInTime?.Month)
                    .ThenBy(e => e.ActualCheckInTime?.Date)
                    .Select(e => e.ActualCheckInTime?.Date)
                    .Distinct()
                    .ToList();

            List<DateTime> checkInDateTimes = accommodations
                    .Where(e => e.ActualCheckInTime != null
                    && e.HotelId == HotelId
                    && e.ActualCheckInTime.Value.Year == 2024
                    && e.Package.MedinahArrival == MedinaArrival)
                    .OrderBy(e => e.ActualCheckInTime?.Month)
                    .ThenBy(e => e.ActualCheckInTime?.Date)
                    .Select(e => e.ActualCheckInTime.Value.Date)
                    .Distinct().ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash
                .ToList();

            for (int i = 0; i < checkIns.Count; i++)
            {
                model.CheckInHeaders += $"<th class='calendar'>{checkIns.ElementAt(i)?.ToString("dd/MMM")}</th>";
            }

            for (int i = 0; i < rooms.Count; i++)
            {
                model.RoomHeaders += $"<th>{rooms.ElementAt(i).RoomCode}</th>";
            }

            for (int i = 0; i < hotels.Count; i++)
            {
                var _currHotel = hotels.ElementAt(i);
                HotelGroupModel hotelGroup = new()
                {
                    HotelName = _currHotel.HotelName
                };

                var currAccommodationsByArrival = accommodations
                        .Where(e => e.HotelId == _currHotel.HotelId && e.Package.MedinahArrival.ToUpper().Equals(MedinaArrival))
                        .ToList();

                float totalSharingRequired = 0f;
                float totalRoomModel = 0f;

                double totalDouble = 0f, totalTriple = 0f, totalQuad = 0f, totalQuint = 0f, totalSharingQuad = 0f, totalSharingRoom = 0f;

                for (int j = 0; j < currAccommodationsByArrival.Count; j++)
                {
                    var _currAccom = currAccommodationsByArrival.ElementAt(j);

                    var roomQuery = from af in _dbContext.AdditionalFacilities
                                    join fhad in _dbContext.FamilyHeadAdditionalFacilities on af.AdditionalFacilitiesId equals fhad.AdditionalFacilitiesId
                                    join fh in _dbContext.FamilyHeads on fhad.FamilyHeadId equals fh.FamilyHeadId
                                    join h in _dbContext.Hotels on af.PlaceId equals h.PlaceId
                                    join r in _dbContext.Rooms on af.RoomId equals r.RoomId
                                    join p in _dbContext.Packages on af.PackageId equals p.PackageId
                                    where h.HotelId == HotelId && p.PackageId == _currAccom.PackageId
                                    && _currAccom.Package.MedinahArrival.ToUpper().Equals(MedinaArrival)
                                    && _currAccom.ActualCheckInTime != null
                                    && !fh.Trashed
                                    select new
                                    {
                                        af,
                                        r.PAX,
                                        DBL = af.RoomId == 1 ? (1.0 / r.PAX) : 0,
                                        Triple = af.RoomId == 2 ? (1.0 / r.PAX) : 0,
                                        Quad = af.RoomId == 3 ? (1.0 / r.PAX) : 0,
                                        Quint = af.RoomId == 4 ? (1.0 / r.PAX) : 0,
                                        SHQD = /*af.RoomId == ? (1.0 / r.PAX) : */ 0,
                                        SHR = af.RoomId == 5 ? (1.0 / r.PAX) : 0
                                    };

                    var roomResult = roomQuery.ToList();

                    var dblSum = roomResult.Sum(item => item.DBL);
                    var triSum = roomResult.Sum(item => item.Triple);
                    var quadSum = roomResult.Sum(item => item.Quad);
                    var quintSum = roomResult.Sum(item => item.Quint);
                    var ShQdSum = roomResult.Sum(item => item.SHQD);
                    var ShRSum = roomResult.Sum(item => item.SHR);

                    totalDouble += dblSum;
                    totalTriple += triSum;
                    totalQuad += quadSum;
                    totalQuint += quintSum;
                    totalSharingQuad += ShQdSum;
                    totalSharingRoom += ShRSum;

                    float pax = families
                                    .Where(e => e.PackageId == _currAccom.PackageId)
                                    .Count();

                    var membersInSharingQuery = from fh in _dbContext.FamilyHeads
                                                join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                                //join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                                where p.PackageId == _currAccom.PackageId
                                                join fhaf in _dbContext.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhaf.FamilyHeadId into gj
                                                from subfhaf in gj.DefaultIfEmpty()
                                                where subfhaf == null && !fh.Trashed
                                                select new
                                                {
                                                    FamilyHead = fh,
                                                    Package = p,
                                                    FamilyHeadAdditionalFacilities = subfhaf
                                                    // PackageAccomodations = pa
                                                };

                    var membersInSharing = membersInSharingQuery.ToList();

                    List<RoomCountModel> roomCountModels = new();
                    foreach (var m in membersInSharing)
                    {
                        float sharingPax = 0.2f;
                        roomCountModels.Add(new RoomCountModel
                        {
                            RoomCount = sharingPax,
                            //CheckIn = m.PackageAccomodations.ActualCheckInTime?.Date
                        });
                    }

                    totalSharingRequired += roomCountModels.Sum(e => e.RoomCount);

                    HotelPackageRoomRowModel _row = new()
                    {
                        Company = _currAccom.Package.Company.CompanyCode,
                        Package = _currAccom.Package.Name,
                        In = _currAccom.ActualCheckInTime?.Date.ToString("dd/MMM"),
                        Out = _currAccom.ActualCheckOutTime?.Date.ToString("dd/MMM"),
                        Nights = _currAccom.Nights,
                        Pax = pax,
                        HotelId = _currHotel.HotelId,
                        BasicSharingRoomCount = Math.Round(roomCountModels.Sum(e => e.RoomCount), 2),
                        DoubleRoomCount = Math.Round(dblSum, 2),
                        TripleRoomCount = Math.Round(triSum, 2),
                        QuadRoomCount = Math.Round(quadSum, 2),
                        QuintRoomCount = Math.Round(quintSum, 2),
                        SharingQuadCount = Math.Round(0.0f, 2),
                        SharingRoomCount = Math.Round(ShRSum, 2),
                    };

                    List<float> totals = new();
                    for (int r = 0; r < rooms.Count; r++)
                    {
                        var _currRoom = rooms.ElementAt(r);
                        var membersWithCurrRoomType = subGroupMembersIadfs
                                                        .Where(
                                                            e => e.af.RoomId == _currRoom.RoomId
                                                            && e.af.PlaceId == _currAccom.PlaceId
                                                            && e.pa.PlaceId == _currAccom.PlaceId
                                                            && e.af.PackageId == _currAccom.PackageId
                                                            && e.p.MedinahArrival.ToUpper().Equals(MedinaArrival)
                                                            && e.pa.HotelId == _currHotel.HotelId
                                                        ).ToList();

                        float totalRequired = subGroupMembersIadfs
                                                        .Where(
                                                            e => e.af.RoomId == _currRoom.RoomId
                                                            && e.af.PlaceId == _currAccom.PlaceId
                                                            && e.pa.PlaceId == _currAccom.PlaceId
                                                            && e.p.MedinahArrival.ToUpper().Equals(MedinaArrival)
                                                            && e.pa.HotelId == _currHotel.HotelId
                                                        ).ToList().Count / _currRoom.PAX;

                        float roomPax = membersWithCurrRoomType.Count() != 0 ? ((float)membersWithCurrRoomType.Count() / (float)_currRoom.PAX) : 0f;
                        _row.Rooms.Add(new RoomCountModel
                        {
                            RoomCount = (float)Math.Round(roomPax, 2),
                            CheckIn = _currAccom.ActualCheckInTime?.Date
                        });
                        // if(_currRoom.RoomId == 4) {
                        //     return Ok(new {
                        //         membersWithCurrRoomType,
                        //         c = membersWithCurrRoomType.Count(),
                        //         _currRoom.PAX,
                        //         _currRoom.RoomCode,
                        //         roomPax

                        //     });
                        // }
                        totals.Add(totalRequired);
                    }

                    // if (_row.Package.Equals("SILVER 16"))
                    // {
                    //     return Ok(new
                    //     {
                    //         _row.Package,
                    //         t = totals
                    //     });
                    // }

                    for (int c = 0; c < checkInDateTimes.Count; c++)
                    {
                        DateTime _currCheckIn = checkInDateTimes.ElementAt(c);
                        // var count = _row.Rooms.Where(e => e.CheckIn == _currCheckIn.Date).Sum(e => e.RoomCount);
                        // count += roomCountModels.Where(e => e.CheckIn == _currCheckIn.Date).Sum(e => e.RoomCount);

                        float count = _row.Rooms.Where(e =>
                            DateTimeManager.IsDateInRange(_currCheckIn.Date, _currAccom.ActualCheckInTime?.Date, _currAccom.ActualCheckOutTime?.Date.AddDays(-1)))
                            .Sum(e => e.RoomCount);
                        count += roomCountModels.Where(e =>
                            DateTimeManager.IsDateInRange(_currCheckIn.Date, _currAccom.ActualCheckInTime?.Date, _currAccom.ActualCheckOutTime?.Date.AddDays(-1)))
                            .Sum(e => e.RoomCount);


                        // return Ok(new {
                        //     c = _row.Rooms.Where(e =>
                        //         DateTimeManager.IsDateInRange(_currCheckIn.Date, _currAccom.ActualCheckInTime?.Date, _currAccom.ActualCheckOutTime?.Date.AddDays(-1))),
                        //         roomCountModels

                        // });

                        _row.CheckIns.Add(new RoomCountModel
                        {
                            RoomCount = count,
                            CheckIn = _currCheckIn.Date,
                        });
                    }

                    // return Ok(new {
                    //     _row.CheckIns
                    // });

                    hotelGroup.BeforeHajjRowsTotalRequired = totals;
                    hotelGroup.BeforeHajjRowsTotalRequiredBasicSharing = Math.Round(totalSharingRequired, 2);
                    hotelGroup.BeforeHajjRows.Add(_row);
                }

                List<float> checkInTotals = new();
                for (int c = 0; c < checkInDateTimes.Count; c++)
                {
                    DateTime _currCheckIn = checkInDateTimes.ElementAt(c);
                    double sum = 0f;
                    foreach (var row in hotelGroup.BeforeHajjRows)
                    {
                        sum += Math.Round(row.CheckIns.Where(
                            e => e.CheckIn?.Date == _currCheckIn.Date
                            ).Sum(e => e.RoomCount), 2);
                    }
                    hotelGroup.BeforeHajjRowsTotalCheckIns.Add(sum);
                    hotelGroup.BeforeHajjRowsTotalRequiredDouble = Math.Round(totalDouble, 2);
                    hotelGroup.BeforeHajjRowsTotalRequiredTriple = Math.Round(totalTriple, 2);
                    hotelGroup.BeforeHajjRowsTotalRequiredQuad = Math.Round(totalQuad, 2);
                    hotelGroup.BeforeHajjRowsTotalRequiredQuint = Math.Round(totalQuint, 2);
                    hotelGroup.BeforeHajjRowsTotalRequiredSharingQuad = Math.Round(totalSharingQuad, 2);
                    hotelGroup.BeforeHajjRowsTotalRequiredSharingRoom = Math.Round(totalSharingRoom, 2);
                }

                model.Groups.Add(hotelGroup);
            }


            return View(model);
        }

        public IActionResult HotelRoomSummarySelect()
        {
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            return View();
        }
        public IActionResult PackageCheckInCheckOutSummarySelect()
        {

            return View();
        }

        public IActionResult DailyHotelRoomSummarySelect()
        {
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;
            return View();
        }

        // public IActionResult DailyHotelRoomSummary(int HotelId, string MedinaArrival, DateTime Date)
        // {
        //     HotelRoomSummaryModel model = new();

        //     var rooms = _dbContext.RoomsWithoutTrash.AsNoTracking().ToList();
        //     var hotels = _dbContext.HotelsWithoutTrash.Where(e => e.HotelId == HotelId).AsNoTracking().ToList();

        //     var accommodations = _dbContext.PackageAccomodtions
        //                         .Include(e => e.Package)
        //                         .ThenInclude(e => e.Company)
        //                         .AsNoTracking()
        //                         .ToList();

        //     var subGroupMembers = _dbContext.SubGroupMembers
        //                             .Include(e => e.FamilyHead)
        //                             .Include(e => e.SubGroup)
        //                             .ThenInclude(e => e.Group)
        //                             .ThenInclude(e => e.Package)
        //                             .Where(e => !e.SubGroup.Trashed
        //                                     && !e.SubGroup.Group.Trashed
        //                                     && !e.FamilyHead.Trashed
        //                                     && !e.SubGroup.Group.Package.Trashed)
        //                             .AsNoTracking()
        //                             .Select(e => new
        //                             {
        //                                 e.FamilyHeadId,
        //                                 e.SubGroup.Group.PackageId,
        //                                 e.SubGroup.PlaceId,
        //                                 e.SubGroup.Group.Package.MedinahArrival
        //                             })
        //                             .ToList();

        //     var iadfs = _dbContext.FamilyHeadAdditionalFacilities
        //         .Include(e => e.FamilyHead)
        //         .Include(e => e.AdditionalFacilities)
        //         .ThenInclude(e => e.Package)
        //         .Include(e => e.AdditionalFacilities)
        //         .ThenInclude(e => e.Room)
        //         .Where(e => !e.FamilyHead.Trashed)
        //         .AsNoTracking()
        //         .ToList();

        //     var subGroupMembersIadfsQuery = from fh in _dbContext.FamilyHeads
        //                                     join fhaf in _dbContext.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhaf.FamilyHeadId
        //                                     join af in _dbContext.AdditionalFacilities on fhaf.AdditionalFacilitiesId equals af.AdditionalFacilitiesId
        //                                     join r in _dbContext.Rooms on af.RoomId equals r.RoomId
        //                                     join p in _dbContext.Packages on af.PackageId equals p.PackageId
        //                                     join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
        //                                     where !fh.Trashed
        //                                     select new
        //                                     {
        //                                         r.RoomCode,
        //                                         fh,
        //                                         fhaf,
        //                                         af,
        //                                         p,
        //                                         pa
        //                                     };


        //     var subGroupMembersIadfs = subGroupMembersIadfsQuery.ToList();

        //     var checkIns = accommodations
        //             .Where(e => e.ActualCheckInTime != null && e.HotelId == HotelId
        //             && e.Package.MedinahArrival == MedinaArrival
        //             && e.ActualCheckInTime.Value.Year == 2024
        //             && e.ActualCheckInTime.Value.Date == Date)
        //             .OrderBy(e => e.ActualCheckInTime?.Month)
        //             .ThenBy(e => e.ActualCheckInTime?.Date)
        //             .Select(e => e.ActualCheckInTime?.Date)
        //             .Distinct()
        //             .ToList();

        //     List<DateTime> checkInDateTimes = accommodations
        //             .Where(e => e.ActualCheckInTime != null
        //             && e.HotelId == HotelId
        //             && e.ActualCheckInTime.Value.Year == 2024
        //             && e.Package.MedinahArrival == MedinaArrival
        //             && e.ActualCheckInTime.Value.Date == Date)
        //             .OrderBy(e => e.ActualCheckInTime?.Month)
        //             .ThenBy(e => e.ActualCheckInTime?.Date)
        //             .Select(e => e.ActualCheckInTime.Value.Date)
        //             .Distinct().ToList();

        //     var families = _dbContext.FamilyHeadsWithoutTrash
        //         .ToList();

        //     for (int i = 0; i < checkIns.Count; i++)
        //     {
        //         model.CheckInHeaders += $"<th class='calendar'>{checkIns.ElementAt(i)?.ToString("dd/MMM")}</th>";
        //     }

        //     for (int i = 0; i < rooms.Count; i++)
        //     {
        //         model.RoomHeaders += $"<th>{rooms.ElementAt(i).RoomCode}</th>";
        //     }

        //     for (int i = 0; i < hotels.Count; i++)
        //     {
        //         var _currHotel = hotels.ElementAt(i);
        //         HotelGroupModel hotelGroup = new()
        //         {
        //             HotelName = _currHotel.HotelName
        //         };

        //         var currAccommodationsByArrival = accommodations
        //                 .Where(e => e.HotelId == _currHotel.HotelId
        //                 && e.Package.MedinahArrival.ToUpper().Equals(MedinaArrival)
        //                 && e.ActualCheckInTime != null
        //                 && e.ActualCheckInTime.Value.Date == Date)
        //                 .ToList();

        //         float totalSharingRequired = 0f;
        //         float totalRoomModel = 0f;

        //         double totalDouble = 0f, totalTriple = 0f, totalQuad = 0f, totalQuint = 0f;

        //         for (int j = 0; j < currAccommodationsByArrival.Count; j++)
        //         {
        //             var _currAccom = currAccommodationsByArrival.ElementAt(j);

        //             var roomQuery = from af in _dbContext.AdditionalFacilities
        //                             join fhad in _dbContext.FamilyHeadAdditionalFacilities on af.AdditionalFacilitiesId equals fhad.AdditionalFacilitiesId
        //                             join fh in _dbContext.FamilyHeads on fhad.FamilyHeadId equals fh.FamilyHeadId
        //                             join h in _dbContext.Hotels on af.PlaceId equals h.PlaceId
        //                             join r in _dbContext.Rooms on af.RoomId equals r.RoomId
        //                             join p in _dbContext.Packages on af.PackageId equals p.PackageId
        //                             where !fh.Trashed && h.HotelId == HotelId && p.PackageId == _currAccom.PackageId
        //                             && _currAccom.Package.MedinahArrival.ToUpper().Equals(MedinaArrival)
        //                             && _currAccom.ActualCheckInTime != null
        //                             && _currAccom.ActualCheckInTime.Value.Date == Date
        //                             select new
        //                             {
        //                                 af,
        //                                 r.PAX,
        //                                 DBL = af.RoomId == 2 ? (1.0 / r.PAX) : 0,
        //                                 Triple = af.RoomId == 3 ? (1.0 / r.PAX) : 0,
        //                                 Quad = af.RoomId == 4 ? (1.0 / r.PAX) : 0,
        //                                 Quint = af.RoomId == 5 ? (1.0 / r.PAX) : 0
        //                             };

        //             var roomResult = roomQuery.ToList();

        //             var dblSum = roomResult.Sum(item => item.DBL);
        //             var triSum = roomResult.Sum(item => item.Triple);
        //             var quadSum = roomResult.Sum(item => item.Quad);
        //             var quintSum = roomResult.Sum(item => item.Quint);

        //             totalDouble += dblSum;
        //             totalTriple += triSum;
        //             totalQuad += quadSum;
        //             totalQuint += quintSum;

        //             float pax = families
        //                             .Where(e => e.PackageId == _currAccom.PackageId)
        //                             .Count();

        //             var membersInSharingQuery = from fh in _dbContext.FamilyHeads
        //                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
        //                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
        //                                         where p.PackageId == _currAccom.PackageId
        //                                         join fhaf in _dbContext.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhaf.FamilyHeadId into gj
        //                                         from subfhaf in gj.DefaultIfEmpty()
        //                                         where subfhaf == null && !fh.Trashed
        //                                         select new
        //                                         {
        //                                             FamilyHead = fh,
        //                                             Package = p,
        //                                             FamilyHeadAdditionalFacilities = subfhaf,
        //                                             PackageAccomodations = pa
        //                                         };

        //             var membersInSharing = membersInSharingQuery.ToList();

        //             List<RoomCountModel> roomCountModels = new();
        //             foreach (var m in membersInSharing)
        //             {
        //                 float sharingPax = 0.2f;
        //                 roomCountModels.Add(new RoomCountModel
        //                 {
        //                     RoomCount = sharingPax,
        //                     CheckIn = m.PackageAccomodations.ActualCheckInTime?.Date,
        //                     CheckOut = m.PackageAccomodations.ActualCheckOutTime?.Date,
        //                 });
        //             }

        //             totalSharingRequired += roomCountModels.Sum(e => e.RoomCount);

        //             HotelPackageRoomRowModel _row = new()
        //             {
        //                 Company = _currAccom.Package.Company.CompanyCode,
        //                 Package = _currAccom.Package.Name,
        //                 In = _currAccom.ActualCheckInTime?.Date.ToString("dd/MMM"),
        //                 Out = _currAccom.ActualCheckOutTime?.Date.ToString("dd/MMM"),
        //                 Nights = _currAccom.Nights,
        //                 Pax = pax,
        //                 HotelId = _currHotel.HotelId,
        //                 SharingRoomCount = Math.Round(roomCountModels.Sum(e => e.RoomCount), 2),
        //                 DoubleRoomCount = Math.Round(dblSum, 2),
        //                 TripleRoomCount = Math.Round(triSum, 2),
        //                 QuadRoomCount = Math.Round(quadSum, 2),
        //                 QuintRoomCount = Math.Round(quintSum, 2),
        //             };

        //             List<float> totals = new();
        //             for (int r = 0; r < rooms.Count; r++)
        //             {
        //                 var _currRoom = rooms.ElementAt(r);
        //                 var membersWithCurrRoomType = subGroupMembersIadfs
        //                                                 .Where(
        //                                                     e => e.af.RoomId == _currRoom.RoomId
        //                                                     && e.af.PlaceId == _currAccom.PlaceId
        //                                                     && e.pa.PlaceId == _currAccom.PlaceId
        //                                                     && e.af.PackageId == _currAccom.PackageId
        //                                                     && e.p.MedinahArrival.ToUpper().Equals(MedinaArrival)
        //                                                     && e.pa.HotelId == _currHotel.HotelId
        //                                                 ).ToList();

        //                 float totalRequired = subGroupMembersIadfs
        //                                                 .Where(
        //                                                     e => e.af.RoomId == _currRoom.RoomId
        //                                                     && e.af.PlaceId == _currAccom.PlaceId
        //                                                     && e.pa.PlaceId == _currAccom.PlaceId
        //                                                     && e.p.MedinahArrival.ToUpper().Equals(MedinaArrival)
        //                                                     && e.pa.HotelId == _currHotel.HotelId
        //                                                 ).ToList().Count / _currRoom.PAX;

        //                 float roomPax = membersWithCurrRoomType.Count() != 0 ? membersWithCurrRoomType.Count() / _currRoom.PAX : 0;
        //                 _row.Rooms.Add(new RoomCountModel
        //                 {
        //                     RoomCount = roomPax,
        //                     CheckIn = _currAccom.ActualCheckInTime?.Date,
        //                     CheckOut = _currAccom.ActualCheckOutTime?.Date,
        //                 });
        //                 totals.Add(totalRequired);
        //             }

        //             for (int c = 0; c < checkInDateTimes.Count; c++)
        //             {
        //                 DateTime _currCheckIn = checkInDateTimes.ElementAt(c);
        //                 // var count = _row.Rooms.Where(e => e.CheckIn == _currCheckIn.Date).Sum(e => e.RoomCount);
        //                 // count += roomCountModels.Where(e => e.CheckIn == _currCheckIn.Date).Sum(e => e.RoomCount);

        //                 float count = _row.Rooms.Where(e =>
        //                     DateTimeManager.IsDateInRange(e.CheckIn.Value.Date, _currAccom.ActualCheckInTime.Value.Date, _currAccom.ActualCheckOutTime.Value.Date))
        //                     .Sum(e => e.RoomCount);
        //                 count += roomCountModels.Where(e =>
        //                     DateTimeManager.IsDateInRange(e.CheckIn.Value.Date, _currAccom.ActualCheckInTime.Value.Date, _currAccom.ActualCheckOutTime.Value.Date))
        //                     .Sum(e => e.RoomCount);

        //                 _row.CheckIns.Add(new RoomCountModel
        //                 {
        //                     RoomCount = count,
        //                     CheckIn = _currCheckIn.Date,
        //                 });
        //             }

        //             hotelGroup.BeforeHajjRowsTotalRequired = totals;
        //             hotelGroup.BeforeHajjRowsTotalRequiredSharing = Math.Round(totalSharingRequired, 2);
        //             hotelGroup.BeforeHajjRows.Add(_row);
        //         }

        //         List<float> checkInTotals = new();
        //         for (int c = 0; c < checkInDateTimes.Count; c++)
        //         {
        //             DateTime _currCheckIn = checkInDateTimes.ElementAt(c);
        //             double sum = 0f;
        //             foreach (var row in hotelGroup.BeforeHajjRows)
        //             {
        //                 sum += Math.Round(row.CheckIns.Where(e => e.CheckIn?.Date == _currCheckIn.Date).Sum(e => e.RoomCount), 2);
        //             }
        //             hotelGroup.BeforeHajjRowsTotalCheckIns.Add(sum);
        //             hotelGroup.BeforeHajjRowsTotalRequiredDouble = Math.Round(totalDouble, 2);
        //             hotelGroup.BeforeHajjRowsTotalRequiredTriple = Math.Round(totalTriple, 2);
        //             hotelGroup.BeforeHajjRowsTotalRequiredQuad = Math.Round(totalQuad, 2);
        //             hotelGroup.BeforeHajjRowsTotalRequiredQuint = Math.Round(totalQuint, 2);
        //         }

        //         model.Groups.Add(hotelGroup);
        //     }


        //     return View(model);
        // }

        public IActionResult PlaceWiseCheckInOut() => View();

        public IActionResult PlaceWiseCheckInReport(int PlaceId, string CheckInOut)
        {

            ViewBag.PlaceName = _dbContext.Places.FirstOrDefault(e => e.PlaceId == PlaceId).PlaceName;
            ViewBag.CheckInOut = CheckInOut;

            var accommodations = _dbContext.PackageAccomodtions
                                            .Include(e => e.Package)
                                            .ThenInclude(e => e.Company)
                                            .Where(e => e.ActualCheckInTime != null
                                                && e.ActualCheckInTime != null && e.PlaceId == PlaceId)
                                            .Select(e => new
                                            {
                                                e.ActualCheckInTime,
                                                e.ActualCheckOutTime,
                                                e.PlaceId,
                                            })
                                            .AsNoTracking()
                                            .ToList();


            var checkIns = accommodations
                                .Where(e => e.ActualCheckInTime != null
                                && e.PlaceId == PlaceId
                                && e.ActualCheckInTime.Value.Year == 2024)
                                .OrderBy(e => e.ActualCheckInTime?.Month)
                                .ThenBy(e => e.ActualCheckInTime?.Date)
                                .Select(e => e.ActualCheckInTime?.Date)
                                .Distinct()
                                .ToList();

            var checkOuts = accommodations
                                .Where(e => e.ActualCheckOutTime != null
                                && e.PlaceId == PlaceId
                                && e.ActualCheckOutTime.Value.Year == 2024)
                                .OrderBy(e => e.ActualCheckOutTime?.Month)
                                .ThenBy(e => e.ActualCheckOutTime?.Date)
                                .Select(e => e.ActualCheckOutTime?.Date)
                                .Distinct()
                                .ToList();


            var members = (from fh in _dbContext.FamilyHeadsWithoutTrash
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                           where pa.ActualCheckInTime != null && pa.ActualCheckInTime.Value.Year == 2024
                           && pa.ActualCheckOutTime != null && pa.ActualCheckOutTime.Value.Year == 2024
                           && pa.PlaceId == PlaceId
                           select new
                           {
                               fh,
                               pa,
                               p
                           }).ToList();


            List<DateTime> checkInDateTimes = accommodations
                    .Where(e => e.ActualCheckInTime != null
                    && e.PlaceId == PlaceId
                    && e.ActualCheckInTime.Value.Year == 2024)
                    .OrderBy(e => e.ActualCheckInTime?.Month)
                    .ThenBy(e => e.ActualCheckInTime?.Date)
                    .Select(e => e.ActualCheckInTime.Value.Date)
                    .Distinct().ToList();

            List<DateTime> checkOutDateTimes = accommodations
                    .Where(e => e.ActualCheckOutTime != null
                    && e.PlaceId == PlaceId
                    && e.ActualCheckOutTime.Value.Year == 2024)
                    .OrderBy(e => e.ActualCheckOutTime?.Month)
                    .ThenBy(e => e.ActualCheckOutTime?.Date)
                    .Select(e => e.ActualCheckOutTime.Value.Date)
                    .Distinct().ToList();


            var munazzimCompanies = _dbContext.MunazzimCompanies
                        .Include(e => e.Company)
                        .Include(e => e.Munazzim)
                        .Where(e => !e.Company.Trashed && !e.Munazzim.Trashed)
                        .Select(e => new
                        {
                            e.CompanyId,
                            e.MunazzimId,
                            e.Company.CompanyCode,
                            e.Munazzim.ClusterNumber,
                            e.Munazzim.Name,
                        })
                        .AsNoTracking()
                        .ToList();

            var munazzims = _dbContext.Munazzims
                            .Where(e => !e.Trashed)
                            .AsNoTracking()
                            .ToList();

            List<PlaceWiseCheckInOutReportModel> model = new();

            foreach (var m in munazzims)
            {
                PlaceWiseCheckInOutReportModel _ = new();
                _.MunazzimName = m.Name;
                _.MunazzimId = m.MunazzimId;
                _.InOutString = CheckInOut;
                foreach (var mc in munazzimCompanies.Where(e => e.MunazzimId == m.MunazzimId))
                {
                    _.CompanyCodes.Add(mc.CompanyCode);
                }

                if (CheckInOut.Equals("In"))
                {
                    foreach (var checkIn in checkInDateTimes)
                    {
                        PlaceWiseCheckInOutReportRowModel row = new();
                        row.Date = checkIn.ToString("dd-MMM");
                        foreach (var mc in munazzimCompanies.Where(e => e.MunazzimId == m.MunazzimId))
                        {
                            int _count = members.Count(e => e.p.CompanyId == mc.CompanyId && e.pa.ActualCheckInTime.Value.Date == checkIn);
                            row.Counts.Add(new PlaceWiseCheckInOutReportRowCompanyCountModel
                            {
                                Count = _count,
                                CompanyId = mc.CompanyId
                            });
                            row.RowTotal += _count;
                        }

                        if (row.RowTotal == 0) continue;
                        _.Row.Add(row);
                    }
                }
                else
                {
                    foreach (var checkOut in checkOutDateTimes)
                    {
                        PlaceWiseCheckInOutReportRowModel row = new();
                        row.Date = checkOut.ToString("dd-MMM");
                        foreach (var mc in munazzimCompanies.Where(e => e.MunazzimId == m.MunazzimId))
                        {
                            int _count = members.Count(e => e.p.CompanyId == mc.CompanyId && e.pa.ActualCheckOutTime.Value.Date == checkOut);
                            row.Counts.Add(new PlaceWiseCheckInOutReportRowCompanyCountModel
                            {
                                Count = _count,
                                CompanyId = mc.CompanyId
                            });
                            row.RowTotal += _count;
                        }

                        if (row.RowTotal == 0) continue;
                        _.Row.Add(row);
                    }
                }



                int rowColumnsTotal = 0;
                foreach (var r in _.Row)
                {
                    foreach (var rc in r.Counts)
                    {
                        rowColumnsTotal += rc.Count;
                    }
                }
                _.RowColumnsTotal = rowColumnsTotal;


                foreach (var mc in munazzimCompanies.Where(e => e.MunazzimId == m.MunazzimId))
                {
                    int colCount = 0;
                    foreach (var r in _.Row)
                    {
                        colCount += r.Counts.Where(e => e.CompanyId == mc.CompanyId).Sum(e => e.Count);
                    }
                    _.ColumnTotals.Add(colCount);
                }

                model.Add(_);
            }



            return View(model);
        }

        public IActionResult PackageandPlaceCheckInCheckOutReport(string MedinaArrival)
        {
            if (MedinaArrival.ToUpper() == "BEFORE")
            {
                return RedirectToAction("CheckInCheckOutReportBeforeHajj");// CheckInCheckOutReportBeforeHajj();
            }
            else
            {
                return RedirectToAction("CheckInCheckOutReportAfterHajj");

            }
            //return View();
        }
        public IActionResult CheckInCheckOutReportBeforeHajj()
        {
            List<PlaceAndPackageWiseCheckInCheckOut> model = new List<PlaceAndPackageWiseCheckInCheckOut>();
            var medina = from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                         join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                         where pa.PlaceId == 2 && p.MedinahArrival == "before" && !fh.Trashed
                         group new { fh, p, pa, c, m } by new { m.Name, c.CompanyCode, PackageName = p.Name } into grp
                         select new
                         {
                             MunazzimName = grp.Key.Name,
                             CompanyCode = grp.Key.CompanyCode,
                             PackageName = grp.Key.PackageName,
                             Pax = grp.Count(),
                             CheckIn = grp.Max(item => item.pa.ActualCheckInTime),
                             CheckOut = grp.Max(item => item.pa.ActualCheckOutTime)
                         };

            var makkah = from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                         join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                         where (pa.PlaceId == 1 || pa.PlaceId == 4) && p.MedinahArrival == "before" && !fh.Trashed
                         group new { fh, p, pa, c, m } by new { m.Name, c.CompanyCode, PackageName = p.Name } into grp
                         select new
                         {
                             MunazzimName = grp.Key.Name,
                             CompanyCode = grp.Key.CompanyCode,
                             PackageName = grp.Key.PackageName,
                             Pax = grp.Count(),
                             CheckIn = grp.Max(item => item.pa.ActualCheckInTime),
                             CheckOut = grp.Max(item => item.pa.ActualCheckOutTime)
                         };
            var result = makkah.ToList();
            foreach (var item in medina)
            {
                var model1 = new PlaceAndPackageWiseCheckInCheckOut
                {
                    MunazzimName = item.MunazzimName,
                    CompanyCode = item.CompanyCode,
                    PackageName = item.PackageName,
                    Pax = item.Pax,
                    MedinaCheckIn = item.CheckIn?.ToString("dd-MM-yyyy"),
                    MedinaCheckOut = item.CheckOut?.ToString("dd-MM-yyyy"),
                };
                model.Add(model1);

            }

            for (int j = 0; j < result.Count; j++)
            {
                if (model.Count > 0)
                {

                    for (int i = 0; i < model.Count; i++)
                    {
                        if (i == j)
                        {
                            model[i].MakkahCheckIn = result[j].CheckIn?.ToString("dd-MM-yyyy");
                            model[i].MakkahCheckOut = result[j].CheckOut?.ToString("dd-MM-yyyy");

                        }

                    }
                }
            }

            return View(model);
        }

        public IActionResult CheckInCheckOutReportAfterHajj()
        {
            List<PlaceAndPackageWiseCheckInCheckOut> model = new List<PlaceAndPackageWiseCheckInCheckOut>();
            var medina = from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                         join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                         where pa.PlaceId == 2 && p.MedinahArrival == "after" && !fh.Trashed
                         group new { fh, p, pa, c, m } by new { m.Name, c.CompanyCode, PackageName = p.Name } into grp
                         select new
                         {
                             MunazzimName = grp.Key.Name,
                             CompanyCode = grp.Key.CompanyCode,
                             PackageName = grp.Key.PackageName,
                             Pax = grp.Count(),
                             CheckIn = grp.Max(item => item.pa.ActualCheckInTime),
                             CheckOut = grp.Max(item => item.pa.ActualCheckOutTime)
                         };

            var makkah = from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                         join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                         where (pa.PlaceId == 1 || pa.PlaceId == 4) && p.MedinahArrival == "after" && !fh.Trashed
                         group new { fh, p, pa, c, m } by new { m.Name, c.CompanyCode, PackageName = p.Name } into grp
                         select new
                         {
                             MunazzimName = grp.Key.Name,
                             CompanyCode = grp.Key.CompanyCode,
                             PackageName = grp.Key.PackageName,
                             Pax = grp.Count(),
                             CheckIn = grp.Max(item => item.pa.ActualCheckInTime),
                             CheckOut = grp.Max(item => item.pa.ActualCheckOutTime)
                         };
            var result = makkah.ToList();
            foreach (var item in medina)
            {
                var model1 = new PlaceAndPackageWiseCheckInCheckOut
                {
                    MunazzimName = item.MunazzimName,
                    CompanyCode = item.CompanyCode,
                    PackageName = item.PackageName,
                    Pax = item.Pax,
                    MedinaCheckIn = item.CheckIn?.ToString("dd-MM-yyyy"),
                    MedinaCheckOut = item.CheckOut?.ToString("dd-MM-yyyy"),
                };
                model.Add(model1);

            }

            for (int j = 0; j < result.Count; j++)
            {
                if (model.Count > 0)
                {

                    for (int i = 0; i < model.Count; i++)
                    {
                        if (i == j)
                        {
                            model[i].MakkahCheckIn = result[j].CheckIn?.ToString("dd-MM-yyyy");
                            model[i].MakkahCheckOut = result[j].CheckOut?.ToString("dd-MM-yyyy");

                        }

                    }
                }
            }
            return View(model);
        }

    }
}

namespace TMG.Models.ReservationModels
{
    public class PlaceAndPackageWiseCheckInCheckOut
    {
        public string MunazzimName { get; set; }
        public string CompanyCode { get; set; }
        public string PackageName { get; set; }
        public int Pax { get; set; }
        public string MedinaCheckIn { get; set; }
        public string MedinaCheckOut { get; set; }
        public string MakkahCheckIn { get; set; }
        public string MakkahCheckOut { get; set; }
    }
    public record PlaceWiseCheckInOutReportModel
    {
        public string InOutString { get; set; }
        public int MunazzimId { get; set; }
        public string MunazzimName { get; set; }
        public List<string> CompanyCodes { get; set; } = new();

        public List<PlaceWiseCheckInOutReportRowModel> Row { get; set; } = new();
        public List<int> ColumnTotals { get; set; } = new();
        public int RowColumnsTotal { get; set; }
    }

    public record PlaceWiseCheckInOutReportRowCompanyCountModel
    {
        public int CompanyId { get; set; }
        public int Count { get; set; }
    }

    public record PlaceWiseCheckInOutReportRowModel
    {
        public string Date { get; set; }
        public List<PlaceWiseCheckInOutReportRowCompanyCountModel> Counts { get; set; } = new();
        public int RowTotal { get; set; }
    }

    public record HotelRoomSummaryModel
    {
        public string RoomHeaders { get; set; }
        public string CheckInHeaders { get; set; }
        public string MedinaArrival { get; set; }
        public List<HotelGroupModel> Groups { get; set; } = new();

    }

    public record HotelGroupModel
    {
        public string HotelName { get; set; }
        public List<HotelPackageRoomRowModel> BeforeHajjRows { get; set; } = new();
        public double BeforeHajjRowsTotalRequiredBasicSharing { get; set; }
        public double BeforeHajjRowsTotalRequiredDouble { get; set; }
        public double BeforeHajjRowsTotalRequiredTriple { get; set; }
        public double BeforeHajjRowsTotalRequiredQuad { get; set; }
        public double BeforeHajjRowsTotalRequiredQuint { get; set; }
        public double BeforeHajjRowsTotalRequiredSharingQuad { get; set; }
        public double BeforeHajjRowsTotalRequiredSharingRoom { get; set; }
        public List<float> BeforeHajjRowsTotalRequired { get; set; } = new();
        public List<double> BeforeHajjRowsTotalCheckIns { get; set; } = new();
        public List<HotelPackageRoomRowModel> AfterHajjRows { get; set; } = new();
    }

    public record HotelPackageRoomRowModel
    {
        public string Company { get; set; }
        public int HotelId { get; set; }
        public string SPO { get; set; }
        public string Package { get; set; }
        public string In { get; set; }
        public string Out { get; set; }
        public string Nights { get; set; }
        public float Pax { get; set; }
        public double BasicSharingRoomCount { get; set; }
        public double DoubleRoomCount { get; set; }
        public double TripleRoomCount { get; set; }
        public double QuadRoomCount { get; set; }
        public double QuintRoomCount { get; set; }
        public double SharingQuadCount { get; set; }
        public double SharingRoomCount { get; set; }
        public List<RoomCountModel> Rooms { get; set; } = new();
        public List<RoomCountModel> CheckIns { get; set; } = new();

    }

    public record RoomCountModel
    {
        public float RoomCount { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
    }

}

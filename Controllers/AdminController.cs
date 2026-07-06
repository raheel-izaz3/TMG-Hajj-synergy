using iText.Commons.Actions.Contexts;
using MessagePack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;
using TMG.Controllers.POControllers;
using TMG.Data;
using TMG.Models.EHajjModels;
using TMG.Models.FamilyModels;
using TMG.Models.FormModels;
using TMG.Models.InvoiceModels;
using TMG.Models.ReservationModels;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _http;

        public AdminController(TMGDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }


        /*public record AziziaRoomCountModel
        {
            public string RoomCode { get; set; }
            public string HotelName { get; set; }
            public float RoomCount { get; set; }
            public int HotelId { get; set; }
        }*/

        public IActionResult Index(int hotelId)
        {
            ViewBag.QurbaniCount = GetQurbaniCount();
            ViewBag.PremiumBedCount = GetPremiumBedCount();

            ViewBag.Hotels = _context.HotelsWithoutTrash
                //.Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.RecentReceiptsCleared = GetRecentReceiptsCleared();
            ViewBag.RecentReceiptsInTransit = GetRecentReceiptsInTransit();

            const int ADMIN_ROLE_ID = 1;

            ViewBag.IsAdmin = Convert.ToInt32(_http.HttpContext.Session.GetString("RoleId")) == ADMIN_ROLE_ID;

            ViewBag.UserCount = _context.UsersWithoutTrash.Count();

            ViewBag.CompanyCount = _context.CompaniesWithoutTrash
                .Count();

            ViewBag.PackageCount = _context.Packages.Where(e => !e.Trashed)
                 .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count();

            ViewBag.Families = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Where(e => e.IsHead)
                .Count();

            ViewBag.Members = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && (e.HajjiBookingStatus == HajjiBookingStatus.Booked || e.HajjiBookingStatus == HajjiBookingStatus.Tentative))
                .Count();

            ViewBag.Tentative = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Tentative);

            ViewBag.BookRegular = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.ApplicantType == ApplicantType.Regular);

            ViewBag.BookOverseas = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.ApplicantType == ApplicantType.Overseas);

            ViewBag.BookIntercity = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.ApplicantType == ApplicantType.Intercity);

            ViewBag.MaleCount = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count(e => e.Gender == Gender.Male
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            ViewBag.FemaleCount = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count(e => e.Gender == Gender.Female
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            ViewBag.GroupsCount = _context.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count();

            ViewBag.AdultCounts = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .AsEnumerable()
                .Count(e => e.IsAdult
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            ViewBag.ChildCount = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .AsEnumerable()
                .Count(e => e.IsChild
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            ViewBag.InfantCount = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .AsEnumerable()
                .Count(e => e.IsInfant
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            ViewBag.RegularQuerys = _context.CustomerPortfolios
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && !e.Trashed && e.ApplicantType == ApplicantType.Regular)
                .Count();

            ViewBag.OverseasQuerys = _context.CustomerPortfolios
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && !e.Trashed && e.ApplicantType == ApplicantType.Overseas)
                .Count();

            ViewBag.Cancelled = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Cancelled);

            ViewBag.ClientData = _context.ClientDatas
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && !e.Trashed)
                .Count();

            ViewBag.POTicket = _context.POTickets
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && !e.Trashed)
                .Select(e => e.FamilyMembersId) // ya FamilyHeadId (jo column hai us hisaab se)
                .Distinct()
                .Count();


            var activeSeasonId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var latestTickets = _context.POTickets
                .Where(e => e.HajjSeasonsId == activeSeasonId && !e.Trashed)
                .GroupBy(e => e.FamilyMembersId) // ya FamilyHeadId agar wahan relation hai
                .Select(g => g.OrderByDescending(x => x.POTicketId).FirstOrDefault()) // latest record per member
                .ToList();

            ViewBag.POTicketIssue = latestTickets.Count(e => (int)e.IssuedType == 0);
            ViewBag.POTicketReIssue = latestTickets.Count(e => (int)e.IssuedType == 1);
            ViewBag.POTicketRefund = latestTickets.Count(e => (int)e.IssuedType == 2);
            ViewBag.POTicketSelf = latestTickets.Count(e => (int)e.IssuedType == 3);
            ViewBag.POTicketVoid = latestTickets.Count(e => (int)e.IssuedType == 4);

            ViewBag.GiveAway = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.DeliveryDate != null)
                .Count();

            ViewBag.Visa = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.VisaImage != null)
                .Count();

            ViewBag.DD = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.DDDate != null)
                .Count();

            ViewBag.Hajj = _context.DataForms
                .Where(e => !e.Trashed)
                .Count();

            var aziziaCount = (from fh in _context.FamilyHeadsWithoutTrash
                               join p in _context.PackagesWithoutTrash on fh.PackageId equals p.PackageId
                               join pcf in _context.PackageCategoryFacilities on p.PackageId equals pcf.PackageId
                               join iadf in _context.IndividualFacilityDescriptionsWithoutTrash on pcf.IndividualFacilityDescriptionId equals iadf.IndividualFacilityDescriptionId
                               where iadf.PlaceId == 4 && fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                    && fh.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                    && fh.IsHead == true
                               select new
                               {
                                   FamilyHead = fh,
                                   Package = p,
                                   PackageCategoryFacility = pcf,
                                   IndividualFacilityDescription = iadf
                               }).Count();

            ViewBag.Azizias = aziziaCount;

            //Get Azizia Room Count
            var hajjSeasonId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            //var aziziaRoomCounts = GetAziziaRoomCount(hotelId);

            //ViewBag.AziziaRoomCounts = aziziaRoomCounts; // Pass the data to the view

            // DASHBOARD CHARTS WORK BELOW

            var totalQuota = GetTotalPackageQouta();

            // Query 2: Total Bookings
            var totalBookings = GetOverAllQuotaTotalBooking();


            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("Total_Quota", totalQuota.ToString());
            dict.Add("Total_Bookings", totalBookings.ToString());


            ViewBag.OverAllQuota = totalQuota;
            ViewBag.OverAllQuotaTotalBooking = totalBookings;

            //==================================================================// 2ND

            var bookingCountsDaybyDay = GetBookingsByDay();

            ViewBag.bookingCountsDaybyDay = JsonConvert.SerializeObject(bookingCountsDaybyDay);

            //==================================================================// 3RD

            ViewBag.totalBookingsByMunazim = JsonConvert.SerializeObject(GetTotalBookingsByMunazzim());

            ViewBag.totalBookingsByPackage = JsonConvert.SerializeObject(GetTotalBookingsByPackage());

            ViewBag.totalBookingsByCompany = GetTotalBookingsByCompany();

            ViewBag.hajiStatsByPackage = GetHajjStatsByPackage();

            var currentDate = DateTime.Now;



            ViewBag.hajiStatsByCompany = GetHajiStatsByCompany();


            // =================== WITHOUT AZIZIA BULDING COUNT END =======================================================================

            var aziziaBuildings = (
                from r in _context.Rooms
                join ad in _context.AdditionalFacilities on r.RoomId equals ad.RoomId
                join fha in _context.FamilyHeadAdditionalFacilities on ad.AdditionalFacilitiesId equals fha.AdditionalFacilitiesId
                join acc in _context.Accomodations on ad.AccomodationId equals acc.AccomodationId
                join p in _context.Places on ad.PlaceId equals p.PlaceId
                where !r.Trashed && ad.PlaceId == 4 && acc.AccomodationId == 3 && p.PlaceName == "Azizia"
                group new { r, acc } by new { acc.AccomodationId, acc.AccomodationName, r.PAX, r.RoomName } into g
                orderby g.Key.AccomodationId
                select new
                {
                    TotalRooms = Math.Round((decimal)g.Count() / g.Key.PAX, 2),
                    g.Key.AccomodationName,
                    g.Key.AccomodationId,
                    g.Key.RoomName
                }
            ).ToList();


            var roomCounts = (
                from mainquery in
                (
                    from r in _context.Rooms
                    where !r.Trashed
                    join ad in _context.AdditionalFacilities on r.RoomId equals ad.RoomId
                    join fha in _context.FamilyHeadAdditionalFacilities on ad.AdditionalFacilitiesId equals fha.AdditionalFacilitiesId
                    group r by new { r.RoomId, r.RoomName, r.PAX } into grouped
                    select new
                    {
                        TotalRooms = Math.Round((decimal)grouped.Count() / grouped.Key.PAX, 2),
                        RoomName = grouped.Key.RoomName,
                        RoomId = grouped.Key.RoomId
                    }
                )
                orderby mainquery.RoomId
                select mainquery
            ).ToList();

            var convertedAziziaBuildings = aziziaBuildings.Select(building => new
            {
                TotalRooms = building.TotalRooms,
                RoomName = building.RoomName,
                AccomodationName = building.AccomodationName,
                AccomodationId = building.AccomodationId
            }).ToList();

            var combinedRoomCounts = roomCounts.Cast<object>()
                .Concat(convertedAziziaBuildings.Cast<object>())
                .Select(room => room as dynamic)
                .ToList();

            Dictionary<string, decimal> roomTotals = new Dictionary<string, decimal>();

            foreach (var room in combinedRoomCounts)
            {
                string roomName = room.RoomName;
                decimal totalRooms = room.TotalRooms;

                if (roomTotals.ContainsKey(roomName))
                {
                    roomTotals[roomName] += totalRooms;
                }
                else
                {
                    roomTotals.Add(roomName, totalRooms);
                }
            }

            var roomCountsList = roomTotals.Select(kv => new
            {
                RoomName = kv.Key,
                TotalRooms = kv.Value
            }).ToList();

            ViewBag.roomCounts = JsonConvert.SerializeObject(roomCountsList);


            var result = (from r in _context.Rooms
                          join ad in _context.AdditionalFacilities on r.RoomId equals ad.RoomId
                          join fha in _context.FamilyHeadAdditionalFacilities on ad.AdditionalFacilitiesId equals fha.AdditionalFacilitiesId
                          join acc in _context.Accomodations on ad.AccomodationId equals acc.AccomodationId
                          join p in _context.Places on ad.PlaceId equals p.PlaceId
                          join fh in _context.FamilyHeads on fha.FamilyHeadId equals fh.FamilyHeadId
                          where !r.Trashed && ad.PlaceId == 4 && acc.AccomodationId == 1 && p.PlaceName == "Azizia" && !fh.Trashed
                          group r by new { r.RoomId, r.RoomCode, r.PAX } into g
                          orderby g.Key.RoomId
                          select new
                          {
                              TotalRooms = Math.Round((decimal)g.Count() / g.Key.PAX, 2),
                              RoomName = g.Key.RoomCode,
                              RoomId = g.Key.RoomId
                          }).ToList();


            ViewBag.AziziaRooms = JsonConvert.SerializeObject(result);


            //===============================================================================// AZIZA BUILDINGS


            var aziziaBuilding = (from r in _context.Rooms
                                  join ad in _context.AdditionalFacilities on r.RoomId equals ad.RoomId
                                  join fha in _context.FamilyHeadAdditionalFacilities on ad.AdditionalFacilitiesId equals fha.AdditionalFacilitiesId
                                  join acc in _context.Accomodations on ad.AccomodationId equals acc.AccomodationId
                                  join p in _context.Places on ad.PlaceId equals p.PlaceId
                                  join fh in _context.FamilyHeads on fha.FamilyHeadId equals fh.FamilyHeadId
                                  where !r.Trashed && ad.PlaceId == 4 && acc.AccomodationId == 3 && p.PlaceName == "Azizia" && !fh.Trashed
                                  group new { r, acc } by new { acc.AccomodationId, acc.AccomodationName, r.PAX, r.RoomCode, r.RoomId } into g
                                  orderby g.Key.RoomId
                                  select new
                                  {
                                      TotalRooms = Math.Round((decimal)g.Count() / g.Key.PAX, 2),
                                      g.Key.AccomodationName,
                                      g.Key.AccomodationId,
                                      g.Key.RoomCode
                                  }).ToList();


            ViewBag.AziziaBuilding = JsonConvert.SerializeObject(aziziaBuilding);



            var resultmakkah = (from r in _context.Rooms
                                join ad in _context.AdditionalFacilities on r.RoomId equals ad.RoomId
                                join fha in _context.FamilyHeadAdditionalFacilities on ad.AdditionalFacilitiesId equals fha.AdditionalFacilitiesId
                                join fh in _context.FamilyHeads on fha.FamilyHeadId equals fh.FamilyHeadId
                                where !r.Trashed && ad.PlaceId == 1 && !fh.Trashed
                                group r by new { r.RoomId, r.RoomCode, r.PAX } into g
                                orderby g.Key.RoomId
                                select new
                                {
                                    TotalRooms = Math.Round((decimal)g.Count() / g.Key.PAX, 2),
                                    RoomName = g.Key.RoomCode,
                                    RoomId = g.Key.RoomId
                                }).ToList();
            ViewBag.MaccaRooms = JsonConvert.SerializeObject(resultmakkah);



            var resultmadina = (from r in _context.Rooms
                                join ad in _context.AdditionalFacilities on r.RoomId equals ad.RoomId
                                join fha in _context.FamilyHeadAdditionalFacilities on ad.AdditionalFacilitiesId equals fha.AdditionalFacilitiesId
                                join fh in _context.FamilyHeads on fha.FamilyHeadId equals fh.FamilyHeadId
                                where !r.Trashed && ad.PlaceId == 2 && !fh.Trashed
                                group r by new { r.RoomId, r.RoomCode, r.PAX } into g
                                orderby g.Key.RoomId
                                select new
                                {
                                    TotalRooms = Math.Round((decimal)g.Count() / g.Key.PAX, 2),
                                    RoomName = g.Key.RoomCode,
                                    RoomId = g.Key.RoomId
                                }).ToList();
            ViewBag.MadinaRooms = JsonConvert.SerializeObject(resultmadina);

            //========================================================================//

            var query1 = from fh in _context.FamilyHeads
                         join p in _context.Packages on fh.PackageId equals p.PackageId
                         join uha in _context.UserHajjAgreements on fh.FamilyHeadId equals uha.FamilyHeadId into uhaGroup
                         from uha in uhaGroup.DefaultIfEmpty()
                         join fhe in _context.FamilyHeadExtras on fh.FamilyHeadId equals fhe.FamilyHeadId into fheJoin
                         from fhe in fheJoin.DefaultIfEmpty()
                         where ((!fh.Trashed && !p.Trashed) &&
                                ((fh.Gender == Gender.Male && (currentDate.Year - fh.DateOfBirth.Year) > 12) ||
                                 (fh.Gender == Gender.Male && (currentDate.Year - fh.DateOfBirth.Year) <= 12 && (currentDate.Year - fh.DateOfBirth.Year) > 2) ||
                                 (fh.Gender == Gender.Female && (currentDate.Year - fh.DateOfBirth.Year) > 12) ||
                                 (fh.Gender == Gender.Female && (currentDate.Year - fh.DateOfBirth.Year) <= 12 && (currentDate.Year - fh.DateOfBirth.Year) > 2) ||
                                 ((currentDate.Year - fh.DateOfBirth.Year) <= 2)))
                         group new { fh, p, uha, fhe } by p.Name into grouped
                         select new
                         {
                             PackageName = grouped.Key,
                             TotalPax = grouped.Count(),
                             TotalMale = grouped.Sum(x => (x.fh.Gender == Gender.Male && !x.fh.Trashed && (currentDate.Year - x.fh.DateOfBirth.Year) > 12) ? 1 : 0),
                             TotalMaleChild = grouped.Sum(x => (x.fh.Gender == Gender.Male && !x.fh.Trashed && (currentDate.Year - x.fh.DateOfBirth.Year) <= 12 && (currentDate.Year - x.fh.DateOfBirth.Year) > 2) ? 1 : 0),
                             TotalFemale = grouped.Sum(x => (x.fh.Gender == Gender.Female && !x.fh.Trashed && (currentDate.Year - x.fh.DateOfBirth.Year) > 12) ? 1 : 0),
                             TotalFemaleChild = grouped.Sum(x => (x.fh.Gender == Gender.Female && !x.fh.Trashed && (currentDate.Year - x.fh.DateOfBirth.Year) <= 12 && (currentDate.Year - x.fh.DateOfBirth.Year) > 2) ? 1 : 0),
                             TotalInfant = grouped.Sum(x => (!x.fh.Trashed && (currentDate.Year - x.fh.DateOfBirth.Year) <= 2) ? 1 : 0),
                             QurbaniIncluded = grouped.Sum(x => x.fhe.ExtraFacilityDescriptionId == 1 ? 1 : 0),
                             BioVerifiedCount = grouped.Sum(x => (!x.fh.Trashed && x.fh.ENumberVerified) ? 1 : 0),
                             EHajjVerified = grouped.Sum(x => (!x.fh.Trashed && x.fh.EHajjVerified) ? 1 : 0),
                             DataFormVerified = grouped.Sum(x => (!x.fh.Trashed && x.fh.Verified) ? 1 : 0),
                             DataFormSigned = grouped.Count(x => !x.fh.Trashed && x.fh.SignatureBase64 != null),
                             AgreementSigned = grouped.Sum(x => (!x.fh.Trashed && x.uha.SignatureBase64 != null) ? 1 : 0)
                         };


            var MalefemalechartbyPackage = query1.ToList();


            ViewBag.MalefemalechartbyPackage = MalefemalechartbyPackage;


            string sqlQuery = @"
    SELECT 
	 ROW_NUMBER() OVER (ORDER BY GroupName) as  QueryResultGroupId,
    GroupName,
    SUM(qurbani) AS QurbaniCount,
    SUM(DataFormSigned) AS DataFormSigned,
    SUM(AgreementSigned) AS AgreementSigned,
    COUNT(BioVerifiedCount) AS BioVerifiedCount,
    COUNT(EHajjVerified) AS EHajjVerified,
    COUNT(DataFormVerified) AS DataFormVerified
FROM (
    SELECT 
        g.Name AS GroupName,
        COUNT(CASE WHEN fhe.ExtraFacilityDescriptionId = 1 THEN 1 ELSE NULL END) AS qurbani,
        COUNT(case when uha.SignatureBase64 is null then null else 1 end) as DataFormSigned,
        COUNT(case when fh.SignatureBase64 is null then null else 1 end) as AgreementSigned,
        COUNT(fh.ENumberVerified) as BioVerifiedCount,
        COUNT(fh.EHajjVerified) as EHajjVerified,
        COUNT(fh.Verified) as DataFormVerified
    FROM 
        FamilyHeads fh
    JOIN 
        Packages p ON fh.PackageId = p.PackageId
    JOIN 
        Groups g ON g.PackageId = p.PackageId
    JOIN 
        GroupMembers gm ON gm.FamilyHeadId = fh.FamilyHeadId
    LEFT JOIN 
        FamilyHeadExtras fhe ON fhe.FamilyHeadId = fh.FamilyHeadId
    LEFT JOIN 
        UserHajjAgreements uha on uha.FamilyHeadId = fh.FamilyHeadId
    WHERE 
        fh.Trashed = 0 AND p.Trashed = 0 AND g.Trashed = 0  
    GROUP BY 
        g.Name, g.GroupId
) AS mainquery 
GROUP BY 
    GroupName;
";

            // Execute the SQL query
            var result2 = _context.QueryResultGroups.FromSqlRaw(sqlQuery).ToList();
            ViewBag.Groupcount = result2;

            // ================================ E-Hajj Contract Summary Start ================================

            var makkahCount = _context.EHajjContracts
                                    .Join(_context.Hotels, ehc => ehc.HotelId, h => h.HotelId, (ehc, h) => new { EHajjContract = ehc, Hotel = h })
                                    .Where(x => x.Hotel.PlaceId == 1)
                                    .Count();

            var medinaCount = _context.EHajjContracts
                                    .Join(_context.Hotels, ehc => ehc.HotelId, h => h.HotelId, (ehc, h) => new { EHajjContract = ehc, Hotel = h })
                                    .Where(x => x.Hotel.PlaceId == 2)
                                    .Count();

            var anotherMakkahCount = _context.EHajjContracts
                                            .Join(_context.Hotels, ehc => ehc.HotelId, h => h.HotelId, (ehc, h) => new { EHajjContract = ehc, Hotel = h })
                                            .Where(x => x.Hotel.PlaceId == 4)
                                            .Count();


            var munazzim = from ehc in _context.EHajjContracts
                           join h in _context.Hotels on ehc.HotelId equals h.HotelId
                           join p in _context.Packages on ehc.PackageId equals p.PackageId
                           join mc in _context.MunazzimCompanies on p.CompanyId equals mc.CompanyId
                           join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
                           group new { Munazzim = m, Contract = ehc } by m.Name into g
                           select new
                           {
                               Name = g.Key,
                               MunazzimCount = g.Key.Count(),
                               MakkahCount = g.Count(e => e.Contract.Hotel.PlaceId == 1),
                               MedinaCount = g.Count(e => e.Contract.Hotel.PlaceId == 2),
                               AziziaCount = g.Count(e => e.Contract.Hotel.PlaceId == 4)
                           };


            ViewBag.Makkah = makkahCount;
            ViewBag.Medina = medinaCount;
            ViewBag.Azizia = anotherMakkahCount;

            var spoRoomCount = GetSpoWiseCount();

            ViewBag.SPOsCount = spoRoomCount;

            var hgoCount = GetHGOCount();

            ViewBag.HGOCount = hgoCount;

            var eHajjCount = GetEHajjCount();

            ViewBag.EHajjCount = eHajjCount;

            var aziziaRoomCounts = GetAziziaRoomCount(hotelId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RoomCountTable", aziziaRoomCounts);  // Pura table update hoga
            }

            ViewBag.RoomCount = aziziaRoomCounts;

            return View();  // Model pass karna behtar hai

            //return View();
        }

        public IEnumerable<Invoice> GetRecentReceiptsCleared() => _context.InvoicesWithoutTrash
                                                                    .Where(e => e.InvoiceStatus == InvoiceStatus.Cleared)
             .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                                                                    .OrderByDescending(e => e.InvoiceId)
                                                                    .Take(5)
                                                                    .Include(e => e.FamilyHead)
                                                                    .ToList();

        public IEnumerable<Invoice> GetRecentReceiptsInTransit() => _context.InvoicesWithoutTrash
                                                                    .Where(e => e.InvoiceStatus == InvoiceStatus.ClearingInTransit)
             .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                                                                    .OrderByDescending(e => e.InvoiceId)
                                                                    .Take(5)
                                                                    .Include(e => e.FamilyHead)
                                                                    .ToList();


        private IEnumerable<object> GetHajiStatsByCompany() =>
            (from fh in _context.FamilyHeads
             join p in _context.Packages on fh.PackageId equals p.PackageId
             join c in _context.Companies on p.CompanyId equals c.CompanyId
             join uha in _context.UserHajjAgreements on fh.FamilyHeadId equals uha.FamilyHeadId into uhaJoin
             from uha in uhaJoin.DefaultIfEmpty()
             join fhe in _context.FamilyHeadExtras on fh.FamilyHeadId equals fhe.FamilyHeadId into fheJoin
             from fhe in fheJoin.DefaultIfEmpty()
             where !fh.Trashed && !c.Trashed && !p.Trashed
             group new { fh, c, uha, fhe } by new { c.CompanyName, c.Quota, c.CompanyId } into grouped
             orderby grouped.Key.CompanyId
             select new
             {
                 CompanyName = grouped.Key.CompanyName,
                 Quota = grouped.Key.Quota,
                 HajisBooked = grouped.Count(),
                 RemainingHajis = grouped.Key.Quota - grouped.Count(),
                 TotalMale = grouped.Sum(x => (x.fh.Gender == Gender.Male && (DateTime.Now.Year - x.fh.DateOfBirth.Year) > 12) ? 1 : 0),
                 TotalMaleChild = grouped.Sum(x => (x.fh.Gender == Gender.Male && (DateTime.Now.Year - x.fh.DateOfBirth.Year) <= 12 && (DateTime.Now.Year - x.fh.DateOfBirth.Year) > 2) ? 1 : 0),
                 TotalFemale = grouped.Sum(x => (x.fh.Gender == Gender.Female && (DateTime.Now.Year - x.fh.DateOfBirth.Year) > 12) ? 1 : 0),
                 TotalFemaleChild = grouped.Sum(x => (x.fh.Gender == Gender.Female && (DateTime.Now.Year - x.fh.DateOfBirth.Year) <= 12 && (DateTime.Now.Year - x.fh.DateOfBirth.Year) > 2) ? 1 : 0),
                 TotalInfant = grouped.Sum(x => ((DateTime.Now.Year - x.fh.DateOfBirth.Year) <= 2) ? 1 : 0),
                 BioVerifiedCount = grouped.Count(x => x.fh.ENumberVerified),
                 EHajjVerified = grouped.Count(x => x.fh.EHajjVerified),
                 DataFormVerified = grouped.Count(x => x.fh.Verified),
                 DataFormSigned = grouped.Sum(x => x.fh.SignatureBase64 != null ? 1 : 0),
                 AgreementSigned = grouped.Sum(x => x.uha.SignatureBase64 != null ? 1 : 0),
                 QurbaniCount = grouped.Sum(x => x.fhe.ExtraFacilityDescriptionId == 1 ? 1 : 0)
             }).ToList();

        private IEnumerable<object> GetHajjStatsByPackage() => _context.FamilyHeads
                .Where(fh => !fh.Trashed)
                .Join(_context.Packages,
                    f => f.PackageId,
                    p => p.PackageId,
                    (f, p) => new { FamilyHead = f, Package = p })
                .Where(joined => !joined.Package.Trashed)
                .GroupBy(joined => new { joined.Package.Name, joined.Package.Quota })
                .Select(group => new
                {
                    PackageName = group.Key.Name,
                    Quota = group.Key.Quota,
                    HajisBooked = group.Count(),
                    RemainingHajis = group.Key.Quota - group.Count()
                })
                .ToList();

        public int GetOverAllQuotaTotalBooking() => _context.FamilyHeadsWithoutTrash.Count();

        public int GetQurbaniCount() => _context.FamilyHeadExtras
                                    .Select(e => new
                                    {
                                        e.FamilyHead,
                                        e.FamilyHead.Trashed,
                                        e.ExtraFacilityDescriptionId,
                                        e.FamilyHead.HajjSeasonsId
                                    })
             .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                                    .Count(e => !e.Trashed && e.ExtraFacilityDescriptionId == 1
                                                && e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked);

        public int GetPremiumBedCount() => _context.FamilyHeadExtras
                                    .Select(e => new
                                    {
                                        e.FamilyHead,
                                        e.FamilyHead.Trashed,
                                        e.ExtraFacilityDescriptionId,
                                        e.FamilyHead.HajjSeasonsId
                                    })
             .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                                    .Count(e => !e.Trashed && e.ExtraFacilityDescriptionId == 16
                                                && e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked);

        public int? GetTotalPackageQouta() => _context.Companies
                .Where(c => !c.Trashed)
                .Join(_context.Packages,
                    c => c.CompanyId,
                    p => p.CompanyId,
                    (c, p) => new { Company = c, Package = p })
                .Where(joined => !joined.Package.Trashed)
                .Sum(joined => joined.Package.Quota);

        public IEnumerable<object> GetBookingsByDay() => _context.FamilyHeads
                .Where(fh => !fh.Trashed)
             .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .GroupBy(fh => fh.CreatedAt.Date)
                .Select(g => new
                {
                    BookingDate = g.Key.ToString("dd/MM/yyyy"),
                    Bookings = g.Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked)
                })
                .ToList();

        public IEnumerable<object> GetTotalBookingsByMunazzim() => _context.FamilyHeads
                                        .Where(fh => !fh.Trashed)
                                        .Join(_context.PackagesWithoutTrash,
                                            f => f.PackageId,
                                            p => p.PackageId,
                                            (f, p) => new { FamilyHead = f, Package = p })
                                        .Join(_context.CompaniesWithoutTrash,
                                            fp => fp.Package.CompanyId,
                                            c => c.CompanyId,
                                            (fp, c) => new { FamilyHeadPackage = fp, Company = c })
                                        .Join(_context.MunazzimCompanies,
                                            fpc => fpc.Company.CompanyId,
                                            mc => mc.CompanyId,
                                            (fpc, mc) => new { FamilyHeadPackageCompany = fpc, MunazzimCompany = mc })
                                        .Join(_context.Munazzims,
                                            fpcm => fpcm.MunazzimCompany.MunazzimId,
                                            m => m.MunazzimId,
                                            (fpcm, m) => new { FamilyHeadPackageCompanyMunazzim = fpcm, Munazzim = m })
                                        .Where(e => !e.Munazzim.Trashed
                                                && e.Munazzim.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                                        .GroupBy(joined => new { joined.Munazzim.MunazzimId, joined.Munazzim.Name })
                                        .Select(group => new
                                        {
                                            TotalBookings = group.Count(),
                                            MunazzimName = group.Key.Name
                                        })
                                        .ToList();

        public IEnumerable<object> GetTotalBookingsByPackage() => from f in _context.FamilyHeads
                                                                  join p in _context.Packages on f.PackageId equals p.PackageId
                                                                  where !f.Trashed && !p.Trashed
                                                                  group p by new { p.Name, p.PackageId, p.Quota } into g
                                                                  select new
                                                                  {
                                                                      TotalBookings = g.Count(),
                                                                      TotalBookingsName = g.Key.Name,
                                                                      TotalQuota = g.Key.Quota
                                                                  };

        public IEnumerable<object> GetTotalBookingsByCompany() => (from m in _context.Munazzims
                                                                   join mc in _context.MunazzimCompanies on m.MunazzimId equals mc.MunazzimId
                                                                   join c in _context.Companies on mc.CompanyId equals c.CompanyId
                                                                   join p in _context.Packages on c.CompanyId equals p.CompanyId
                                                                   join fh in _context.FamilyHeads on p.PackageId equals fh.PackageId
                                                                   join fhe in _context.FamilyHeadExtras on fh.FamilyHeadId equals fhe.FamilyHeadId into fheGroup
                                                                   from fhe in fheGroup.DefaultIfEmpty()
                                                                   join uha in _context.UserHajjAgreements on fh.FamilyHeadId equals uha.FamilyHeadId into uhaGroup
                                                                   from uha in uhaGroup.DefaultIfEmpty()
                                                                   where !fh.Trashed && !p.Trashed && !c.Trashed && !m.Trashed
                                                                   group new { m, c, fh, fhe, uha } by new { m.Name, c.Quota } into grouped
                                                                   select new
                                                                   {
                                                                       CompanyName = grouped.Key.Name,
                                                                       Quota = grouped.Key.Quota,
                                                                       TotalBookings = grouped.Count(),
                                                                       Balance = grouped.Key.Quota - grouped.Count(),
                                                                       QurbaniCount = grouped.Count(item => item.fhe.ExtraFacilityDescriptionId == 1),
                                                                       DataFormSigned = grouped.Count(item => item.uha.SignatureBase64 != null),
                                                                       AgreementSigned = grouped.Count(item => item.fh.SignatureBase64 != null),
                                                                       BioVerifiedCount = grouped.Count(item => item.fh.ENumberVerified != null),
                                                                       EHajjVerified = grouped.Count(item => item.fh.EHajjVerified != null),
                                                                       DataFormVerified = grouped.Count(item => item.fh.Verified != null)
                                                                   }).GroupBy(x => x.CompanyName).Select(group => new
                                                                   {
                                                                       CompanyName = group.Key,
                                                                       Quota = group.Sum(x => x.Quota),
                                                                       TotalBookings = group.Sum(x => x.TotalBookings),
                                                                       Balance = group.Sum(x => x.Balance),
                                                                       QurbaniCount = group.Sum(x => x.QurbaniCount),
                                                                       DataFormSigned = group.Sum(x => x.DataFormSigned),
                                                                       AgreementSigned = group.Sum(x => x.AgreementSigned),
                                                                       BioVerifiedCount = group.Count(x => x.BioVerifiedCount != 0),
                                                                       EHajjVerified = group.Count(x => x.EHajjVerified != 0),
                                                                       DataFormVerified = group.Count(x => x.DataFormVerified != 0)
                                                                   }).ToList();

        //public List<AziziaRoomCountModel> GetAziziaRoomCount(int hajjSeasonId)
        //{
        //    var roomCodes = (from r in _context.Rooms
        //                     join idaf in _context.IndividualFacilityDescriptions on r.RoomId equals idaf.RoomId
        //                     where idaf.PlaceId == 4 && r.HajjSeasonsId == hajjSeasonId
        //                     group r by new { r.RoomId, r.RoomCode, r.PAX } into g
        //                     orderby g.Key.RoomId
        //                     select new RoomCode { RoomId = g.Key.RoomId, RoomCodes = g.Key.RoomCode, RoomPax = g.Key.PAX }).ToList();

        //    List<AziziaRoomCountModel> model = new();

        //    var familyheads = _context.FamilyHeadsWithoutTrash
        //                        .Where(e => e.HajjSeasonsId == hajjSeasonId)
        //                        .ToList();

        //    var hotels = _context.HotelsWithoutTrash
        //        .Where(e => e.HajjSeasonsId == hajjSeasonId)
        //        .ToList();

        //    var hotel = _context.HotelsWithoutTrash
        //                        .Where(e => e.PlaceId == 4
        //                                && e.HajjSeasonsId == hajjSeasonId)
        //                        .ToList();

        //    ViewBag.Hotel = hotel;

        //    foreach (var hoteel in hotel)
        //    {
        //        AziziaRoomCountModel _ = new();

        //        foreach (var room in roomCodes)
        //        {
        //            _.RoomCode = room.RoomCodes;


        //            ViewBag.HotelName = _context.HotelsWithoutTrash
        //                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
        //                .Select(e => e.HotelName)
        //                .FirstOrDefault();

        //            foreach (var member in familyheads)
        //            {
        //                if (member.AccomodationAsPerPackage)
        //                {
        //                    _.RoomCount += (float)_context.PackageAccomodtions
        //                       .Where(e => e.PackageId == member.PackageId && e.HotelId == hoteel.HotelId
        //                       && e.Hotel.PlaceId == 4 && e.Package.RoomId == room.RoomId)
        //                       .Count() / (float)room.RoomPax;
        //                }

        //                else
        //                {
        //                    _.RoomCount += (float)_context.FamilyHeadAccomodations
        //                                    .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.IndividualFacilityDescription.RoomId == room.RoomId
        //                                    && e.IndividualFacilityDescription.PlaceId == 4).Count() / (float)room.RoomPax;
        //                }
        //            }
        //        }

        //        model.Add(_);
        //    }

        //    return model;
        //}

        /*public IActionResult Index(int hotelId)
        {
            var aziziaRoomCounts = GetAziziaRoomCount(hotelId);
            ViewBag.RoomCount = aziziaRoomCounts;
            return View();   // Model pass karna behtar hai
        }*/

        public List<AziziaRoomCountModel> GetAziziaRoomCount(int hotelId)
        {
            var hajjSeasonId = Convert.ToInt32(HttpContext.Session.GetString("ActiveSeasonId"));

            var roomCodes = _context.IndividualFacilityDescriptionsWithoutTrash
                .Include(r => r.Room)
                .Where(r => !r.Room.Trashed)
                .GroupBy(r => new { r.Room.RoomId, r.Room.RoomCode, r.Room.PAX })
                .OrderBy(g => g.Key.RoomId)
                .Select(g => new
                {
                    RoomId = g.Key.RoomId,
                    RoomCode = g.Key.RoomCode,
                    RoomPax = g.Key.PAX
                })
                .ToList();

            List<AziziaRoomCountModel> model = new();

            var hotels = _context.HotelsWithoutTrash
                //.Where(e => e.HajjSeasonsId == hajjSeasonId)
                .ToList();

            if (hotelId > 0)
            {
                hotels = hotels.Where(h => h.HotelId == hotelId).ToList();
            }

            var familyheads = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .ToList();

            foreach (var hotel in hotels)
            {
                foreach (var room in roomCodes)
                {
                    AziziaRoomCountModel view = new()
                    {
                        HotelId = hotel.HotelId,
                        HotelName = hotel.HotelName,
                        RoomId = room.RoomId,
                        RoomCode = room.RoomCode,
                        RoomCount = 0
                    };

                    float packageRoomCount = _context.FamilyHeads
                        .Join(_context.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
                        .Join(_context.PackageAccomodtions, combined => combined.p.PackageId, pa => pa.PackageId, (combined, pa) => new { combined.fh, combined.p, pa })
                        .Where(joined => joined.p.RoomId == room.RoomId && !joined.fh.Trashed && joined.pa.HotelId == hotel.HotelId
                                && joined.fh.HajjSeasonsId == hajjSeasonId   
                                && joined.fh.AccomodationAsPerPackage == true
                                && joined.pa.PlaceId != 4 && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked)
                        .Count() / room.RoomPax;

                    float packageRoomCountS = _context.FamilyHeads
                        .Join(_context.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
                        .Join(_context.PackageAccomodtions, combined => combined.p.PackageId, pa => pa.PackageId, (combined, pa) => new { combined.fh, combined.p, pa })
                        .Where(joined => !joined.fh.Trashed
                                && joined.pa.HotelId == hotel.HotelId
                                && joined.p.RoomTypesId == room.RoomId
                                && joined.fh.HajjSeasonsId == hajjSeasonId   
                                && joined.pa.PlaceId == 4 && joined.fh.AccomodationAsPerPackage == true
                                && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked)
                        .Count() / room.RoomPax;

                    float packageRoomCountS_Male = _context.FamilyHeads
                        .Join(_context.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
                        .Join(_context.PackageAccomodtions, combined => combined.p.PackageId, pa => pa.PackageId, (combined, pa) => new { combined.fh, combined.p, pa })
                        .Where(joined => !joined.fh.Trashed
                                && joined.pa.HotelId == hotel.HotelId
                                && joined.fh.HajjSeasonsId == hajjSeasonId   
                                && joined.p.RoomTypesId == room.RoomId && joined.fh.AccomodationAsPerPackage == true
                                && joined.pa.PlaceId == 4 && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                && joined.fh.Gender == Gender.Male) 
                        .Count();

                    float individualRoomCountS_Male = _context.FamilyHeads
                        .Join(_context.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
                        .Join(_context.FamilyHeadAccomodations, combined => combined.fh.FamilyHeadId, pa => pa.FamilyHeadId, (combined, pa) => new { combined.fh, combined.p, pa })
                        .Where(joined => !joined.fh.Trashed
                                && joined.pa.HotelId == hotel.HotelId
                                && joined.fh.HajjSeasonsId == hajjSeasonId   
                                && joined.pa.IndividualFacilityDescription.RoomId == room.RoomId && joined.fh.AccomodationAsPerPackage == false
                                && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                && joined.fh.Gender == Gender.Male)
                        .Count();

                    float packageRoomCountS_Female = _context.FamilyHeads
                        .Join(_context.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
                        .Join(_context.PackageAccomodtions, combined => combined.p.PackageId, pa => pa.PackageId, (combined, pa) => new { combined.fh, combined.p, pa })
                        .Where(joined => !joined.fh.Trashed
                                && joined.pa.HotelId == hotel.HotelId
                                && joined.fh.HajjSeasonsId == hajjSeasonId   
                                && joined.p.RoomTypesId == room.RoomId && joined.fh.AccomodationAsPerPackage == true
                                && joined.pa.PlaceId == 4 && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                && joined.fh.Gender == Gender.Female) 
                        .Count();

                    float individualRoomCountS_Female = _context.FamilyHeads
                        .Join(_context.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
                        .Join(_context.FamilyHeadAccomodations, combined => combined.fh.FamilyHeadId, pa => pa.FamilyHeadId, (combined, pa) => new { combined.fh, combined.p, pa })
                        .Where(joined => !joined.fh.Trashed
                                && joined.pa.HotelId == hotel.HotelId
                                && joined.fh.HajjSeasonsId == hajjSeasonId   
                                && joined.pa.IndividualFacilityDescription.RoomId == room.RoomId && joined.fh.AccomodationAsPerPackage == false
                                && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                && joined.fh.Gender == Gender.Female)
                        .Count();

                    float individualRoomCount = _context.FamilyHeads
                        .Join(_context.FamilyHeadAccomodations,
                              fh => fh.FamilyHeadId,
                              fha => fha.FamilyHeadId,
                              (fh, fha) => new { fh, fha })
                        .Join(_context.IndividualFacilityDescriptions,
                              combined => combined.fha.IndividualFacilityDescriptionId,
                              iadf => iadf.IndividualFacilityDescriptionId,
                              (combined, iadf) => new { combined.fh, combined.fha, iadf })
                        .Join(_context.Packages,
                              combined => combined.fh.PackageId,
                              p => p.PackageId,
                              (combined, p) => new { combined.fh, combined.fha, combined.iadf, p })
                        .Where(x => x.iadf.RoomId == room.RoomId
                                    && x.fha.HotelId == hotel.HotelId
                                    && x.fh.HajjSeasonsId == hajjSeasonId   
                                    && x.fh.AccomodationAsPerPackage == false
                                    && x.fh.HajjiBookingStatus == HajjiBookingStatus.Booked)
                        .Count() / room.RoomPax;

                    view.RoomCount += packageRoomCount;
                    view.RoomCount += packageRoomCountS;
                    view.MaleRoomCount += packageRoomCountS_Male;
                    view.MaleRoomCount += individualRoomCountS_Male;
                    view.FemaleRoomCount += individualRoomCountS_Female;
                    view.FemaleRoomCount += packageRoomCountS_Female;
                    view.RoomCount += individualRoomCount;

                    model.Add(view);
                }
            }

            return model;
        }

        public List<SPOsData> GetSpoWiseCount()
        {
            var hajjSeasonId = Convert.ToInt32(HttpContext.Session.GetString("ActiveSeasonId"));

            var spo = _context.SPOsWithoutTrash;

            List<SPOsData> model = new();

            var familyheads = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .ToList();

            foreach (var item in spo)
            {
                var tentativeCount = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId && e.HajjiBookingStatus == HajjiBookingStatus.Tentative);

                var bookedCount = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

                var count = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId);

                if (tentativeCount > 0 || bookedCount > 0 || count > 0)
                {
                    SPOsData view = new()
                    {
                        SPOId = item.SPOId,
                        SPOName = item.Name,
                        SPOEmail = item.Email,
                        Tentative = tentativeCount,
                        Booked = bookedCount,
                        Count = count
                    };

                    model.Add(view);
                }
            }

            return model;
        }


        //[HttpGet("spo-wise-count")]
        /*public JsonResult GetSpoWiseCounts()
        {
            var session = _http.HttpContext?.Session;
            if (session == null || string.IsNullOrEmpty(session.GetString("ActiveSeasonId")))
            {
                return new JsonResult(new { success = false, message = "ActiveSeasonId not found in session." });
            }

            var hajjSeasonId = Convert.ToInt32(session.GetString("ActiveSeasonId"));
            var spo = _context.SPOsWithoutTrash.ToList();
            var familyheads = _context.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .ToList();

            List<SPOsData> model = new();

            foreach (var item in spo)
            {
                var tentativeCount = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId && e.HajjiBookingStatus == HajjiBookingStatus.Tentative);

                var bookedCount = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

                var count = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId);

                if (tentativeCount > 0 || bookedCount > 0 || count > 0)
                {
                    model.Add(new SPOsData
                    {
                        SPOId = item.SPOId,
                        SPOName = item.Name,
                        SPOEmail = item.Email,
                        Tentative = tentativeCount,
                        Booked = bookedCount,
                        Count = count
                    });
                }
            }

            return new JsonResult(new { success = true, data = model });
        }*/


        public List<HGOs> GetHGOCount()
        {
            var hajjSeasonId = Convert.ToInt32(HttpContext.Session.GetString("ActiveSeasonId"));

            //var company = _context.CompaniesWithoutTrash
            //    .ToList();

            var company = (from c in _context.CompaniesWithoutTrash
                           join q in _context.QuotaCompanies
                           on c.CompanyId equals q.CompanyId
                           where q.HajjSeasonsId == hajjSeasonId
                           select new
                           {
                               CompanyId = c.CompanyId,
                               CompanyName = c.CompanyName,
                               QuotaId = q.QuotaCompanyId,
                               QuotaLimit = q.Quota,
                           }).ToList();

            List<HGOs> model = new();

            var familyheads = _context.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .ToList();

            foreach (var item in company)
            {
                var hgoCount = (float)familyheads
                    .Count(e => e.Package.CompanyId == item.CompanyId && e.HajjApplicationNumber is not null);

                if (hgoCount > 0)
                {
                    HGOs view = new()
                    {
                        CompanyId = item.CompanyId,
                        CompanyName = item.CompanyName,
                        CompanyQuota = item.QuotaLimit,
                        HGO = hgoCount,
                    };

                    model.Add(view);
                }
            }

            return model;
        }

        public List<HGOs> GetEHajjCount()
        {
            var hajjSeasonId = Convert.ToInt32(HttpContext.Session.GetString("ActiveSeasonId"));

            var company = (from c in _context.CompaniesWithoutTrash
                           join q in _context.QuotaCompanies
                           on c.CompanyId equals q.CompanyId
                           where q.HajjSeasonsId == hajjSeasonId
                           select new
                           {
                               CompanyId = c.CompanyId,
                               CompanyName = c.CompanyName,
                               QuotaId = q.QuotaCompanyId,
                               QuotaLimit = q.Quota,
                           }).ToList();

            List<HGOs> model = new();

            var familyheads = _context.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .ToList();

            foreach (var item in company)
            {
                var hgoCount = (float)familyheads
                    .Count(e => e.Package.CompanyId == item.CompanyId && e.EHajjId is not null);

                if (hgoCount > 0)
                {
                    HGOs view = new()
                    {
                        CompanyId = item.CompanyId,
                        CompanyName = item.CompanyName,
                        CompanyQuota = item.QuotaLimit,
                        HGO = hgoCount,
                    };

                    model.Add(view);
                }
            }

            return model;
        }

        [HttpGet]
        public JsonResult GetHajjSeasons()
        {
            var options = _context.HajjSeasons
                .Where(e => !e.Trashed)
                .Select(x => new { id = x.HajjSeasonsId, name = x.Season })
                .OrderByDescending(e => e.id)
                .ToList();

            return Json(options);
        }

        [HttpGet]
        public JsonResult GetSelectedHajjSeason()
        {
            // Retrieve the selected Hajj session ID from the session
            var selectedSessionId = _http.HttpContext.Session.GetString("ActiveSeasonId");

            // Return it as JSON
            return Json(new { selectedSessionId });
        }

        [HttpPost]
        public JsonResult SetHajjSessions(int sessionId)
        {
            _http.HttpContext.Session.SetString("ActiveSeasonId", sessionId.ToString());

            return Json(new { success = true, message = "Hajj Session Updated!" });
        }
    }

    public class QueryResultGroup
    {
        public long? QueryResultGroupId { get; set; }
        public string GroupName { get; set; }
        public int QurbaniCount { get; set; }
        public int DataFormSigned { get; set; }
        public int AgreementSigned { get; set; }
        public int BioVerifiedCount { get; set; }
        public int EHajjVerified { get; set; }
        public int DataFormVerified { get; set; }
    }

    public record HGOs
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int? CompanyQuota { get; set; }
        public float HGO { get; set; }
    }
    public record SPOsData
    {
        public int SPOId { get; set; }
        public string SPOName { get; set; }
        public string SPOEmail { get; set; }
        public float Tentative { get; set; }
        public float Booked { get; set; }
        public float Count { get; set; }
    }

    // Define a class for RoomCode if not already defined
    public class RoomCode
    {
        public int RoomId { get; set; }
        public string RoomCodes { get; set; }
        public int RoomPax { get; set; }
    }
}

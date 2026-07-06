using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Controllers.CompanyControllers;
using TMG.Controllers.FamilyControllers;
using TMG.Controllers.POControllers;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.DemandReportModels;
using TMG.Models.FamilyModels;
using TMG.Models.FormModels;
using TMG.Models.InvoiceModels;
using TMG.Models.MiscModels;
using TMG.Models.ReportModels;
using TMG.Models.ReservationModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static iTextSharp.text.pdf.AcroFields;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers
{
    public record BankStatment
    {
        public DateTime Date { get; set; }
        public string CompanyName { get; set; }
        public string BankName { get; set; }
        public string HBNo { get; set; }
        public string Name { get; set; }
        public string CollectionCurrency { get; set; }
        public string Currency { get; set; }
        public string Remarks { get; set; }
        public string Voucher { get; set; }
        public string Proof { get; set; }
        public float? Debit { get; set; }
        public float? Credit { get; set; }
        public float? Balance { get; set; }
    }
    public record BankStatementData
    {
        public DateTime Date { get; set; }
        public string CompanyName { get; set; }
        public string BankName { get; set; }
        public string HBNo { get; set; }
        public string Name { get; set; }
        public string CollectionCurrency { get; set; }
        public string Currency { get; set; }
        public string Remarks { get; set; }
        public string Voucher { get; set; }
        public string Proof { get; set; }
        public float DebitAmount { get; set; }
        public float CreditAmount { get; set; }
        public float Balance { get; set; }
        public float? DebitBankCharges { get; set; }
        public float? DebitOPAPTax { get; set; }
        public float? DebitRemittanceTax { get; set; }
        public float? DebitThirdPartyTax { get; set; }
        public float? DebitOtherTaxes { get; set; }
    }
    public class CompanyBankSummaryViewModel
    {
        public string CompanyName { get; set; }
        public List<ReceiptSummary> Banks { get; set; }
    }
    public record ReceiptSummary
    {
        public string ReceiptNo { get; set; }
        public string ClientName { get; set; }
        public string HbNo { get; set; }
        public float Amount { get; set; }
        public string Status { get; set; }
        public string DepositBank { get; set; }
        public string Currency { get; set; }
        public string HajjiCurrency { get; set; }
    }

    public record VendorSummary
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }

    public record PackageSummary
    {
        public string PackageName { get; set; }
        public int PackageId { get; set; }
        public string FullName { get; set; }
        public string HBNO { get; set; }
        public string RoomType { get; set; }
        public int HajjiCount { get; set; }
        public int TentativeCount { get; set; }
        public int BookedCount { get; set; }
        public int TMGIssued { get; set; }
        public int ToIssued { get; set; }
        public int BookedF { get; set; }
        public int BookedB { get; set; }
        public int BookedE { get; set; }
        public int Self { get; set; }
        public int Tent { get; set; }
    }

    public partial class ReportController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public ReportController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult CompanyWiseSummary()
        {
            ViewBag.CompanyWiseSummary = _dbContext.Companies
                .ToList();

            return View();
        }

        public IActionResult CompanyWiseHaji()
        {
            ViewBag.CompanyWiseHajji = _dbContext.Companies
                .ToList();

            return View();
        }

        public IActionResult PackageWiseReport()
        {
            ViewBag.PackageWiseReport = _dbContext.Packages
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View();
        }

        public IActionResult PackageWiseFlightSummarySelection()
        {
            ViewBag.PackageWiseSummary = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View();
        }

        public IActionResult PackageWiseFlightSummary(int id)
        {
            var result = _dbContext.CombinedFlightsResult
                .FromSqlRaw(@"
        WITH CombinedFlights AS (
            SELECT 
                CONCAT(pf.Origin, ' - ', pf.Destination) AS Location, 
                CAST(pf.DepartureDate AS DATE) AS [DepartureDate],
                CASE 
                    WHEN pf.FlightClass = 0 AND fh.FlightAsPerPackage = 1 AND fh.FlightTicketType IN (0, 2) THEN 1 
                    ELSE 0 
                END AS [EconomySeats],
                CASE 
                    WHEN pf.FlightClass = 1 AND fh.FlightAsPerPackage = 1 AND fh.FlightTicketType IN (0, 2) THEN 1 
                    ELSE 0 
                END AS [BusinessSeats],
                CASE 
                    WHEN fh.FlightTicketType = 1 THEN 1 
                    ELSE 0 
                END AS [SelfTickets]
            FROM 
                PackageFlights AS pf
            JOIN 
                FamilyHeads AS fh ON fh.PackageId = pf.PackageId
            WHERE pf.PackageId = {0} and pf.DepartureDate is not null

            UNION ALL

            SELECT 
                CONCAT(fhf.Origin, ' - ', fhf.Destination) AS Location, 
                CAST(fhf.DepartureDateTime AS DATE) AS [DepartureDate],
                CASE 
                    WHEN fhf.FlightClass = 0 AND fh.FlightAsPerPackage = 0 AND fh.FlightTicketType IN (0, 2) THEN 1 
                    ELSE 0 
                END AS [EconomySeats],
                CASE 
                    WHEN fhf.FlightClass = 1 AND fh.FlightAsPerPackage = 0 AND fh.FlightTicketType IN (0, 2) THEN 1 
                    ELSE 0 
                END AS [BusinessSeats],
                CASE 
                    WHEN fh.FlightTicketType = 1 THEN 1 
                    ELSE 0 
                END AS [SelfTickets]
            FROM 
                FamilyHeadFlight AS fhf
            JOIN 
                FamilyHeads AS fh ON fh.FamilyHeadId = fhf.FamilyHeadId
            WHERE fh.PackageId = {0} and fhf.DepartureDateTime is not null and fh.trashed = 0 and fhf.TicketStatus = 0
        )

        SELECT 
            Location, 
            [DepartureDate], 
            SUM([EconomySeats]) AS [TotalEconomySeats],
            SUM([BusinessSeats]) AS [TotalBusinessSeats],
            SUM([SelfTickets]) AS [TotalSelfTickets]
        FROM 
            CombinedFlights
        GROUP BY 
            Location, [DepartureDate];
    ", id).ToList();

            ViewBag.PackageId = id;

            return View(result);

        }

        public IActionResult PackageWiseFlightsSummary(string location, string seatType, string departureDate, int id)
        {
            DateTime parsedDepartureDate = DateTime.ParseExact(departureDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            FlightClass flightClassValue = seatType.ToLower() == "economy" ? FlightClass.Economy : FlightClass.Business;

            int hajjId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var query1 = from fh in _dbContext.FamilyHeads
                         join pf in _dbContext.PackageFlights on fh.PackageId equals pf.PackageId
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         where !fh.Trashed
                               && fh.FlightAsPerPackage
                               && fh.HajjSeasonsId == hajjId
                               && pf.DepartureDate.HasValue
                               && pf.DepartureDate.Value.Date == parsedDepartureDate.Date
                               && pf.FlightClass == flightClassValue
                               && (pf.Origin.Trim().ToUpper() + " - " + pf.Destination.Trim().ToUpper()) == location.Trim().ToUpper()
                               && pf.PackageId == id
                         select new
                         {
                             c.CompanyCode,
                             p.Name,
                             pf.FlightClass,
                             fh.FcId,
                             fh.GivenName,
                             Honorifics = fh.Honorifics.Title,
                             fh.SurName,
                             fh.PassportNumber,
                             fh.PassportDateOfIssue,
                             fh.PassportDateOfExpiry,
                             DepartureDate = pf.DepartureDate.Value,
                             Location = pf.Origin + " - " + pf.Destination,
                             pf.DepartureTime
                         };

            var query2 = from fh in _dbContext.FamilyHeads
                         join fhf in _dbContext.FamilyHeadFlight.Include(f => f.FamilyHead) on fh.FamilyHeadId equals fhf.FamilyHeadId
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                         where !fh.Trashed
                               && !fh.FlightAsPerPackage
                               && fh.HajjSeasonsId == hajjId
                               && fhf.DepartureDateTime.HasValue
                               && fhf.DepartureDateTime.Value.Date == parsedDepartureDate.Date
                               && fhf.FlightClass == flightClassValue
                               && (fhf.Origin.Trim().ToUpper() + " - " + fhf.Destination.Trim().ToUpper()) == location.Trim().ToUpper()
                               && fhf.FamilyHead.PackageId == id
                               && fhf.TicketStatus == TicketStatus.Active
                         select new
                         {
                             c.CompanyCode,
                             p.Name,
                             fhf.FlightClass,
                             fh.FcId,
                             fh.GivenName,
                             Honorifics = fh.Honorifics.Title,
                             fh.SurName,
                             fh.PassportNumber,
                             fh.PassportDateOfIssue,
                             fh.PassportDateOfExpiry,
                             DepartureDate = fhf.DepartureDateTime.Value.Date,
                             Location = fhf.Origin + " - " + fhf.Destination,
                             fhf.DepartureTime
                         };

            var combinedQuery = query1
                .Concat(query2)  // Used Concat instead of Union
                .AsEnumerable()
                .Select(x => new CombinedFlightSummaryDetail
                {
                    Honorifics = x.Honorifics,
                    CompanyCode = x.CompanyCode,
                    Name = x.Name,
                    FlightClass = x.FlightClass.ToString(),
                    Fcid = x.FcId,
                    GivenName = x.GivenName,
                    SurName = x.SurName,
                    PassportNumber = x.PassportNumber,
                    PassportDateOfIssue = x.PassportDateOfIssue?.ToString("dd/MMM/yyyy") ?? "",
                    PassportDateOfExpiry = x.PassportDateOfExpiry?.ToString("dd/MMM/yyyy") ?? "",
                    DepartureDate = x.DepartureDate.ToString("dd/MMM/yyyy"),
                    Location = x.Location,
                    DepartureTime = x.DepartureTime?.ToString("HH:mm") ?? "N/A"
                });

            return View(combinedQuery.ToList());
        }

        public IActionResult PackageWiseTrainSummarySelection()
        {
            ViewBag.PackageWiseSummary = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View();
        }

        public IActionResult PackageWiseTrainSummary(int id)
        {
            var result = _dbContext.CombinedTrainsResult
                .FromSqlRaw(@"
        WITH CombinedTrains AS (
    SELECT 
        CONCAT(pf.Origin, ' - ', pf.Destination) AS Location, 
        CAST(pf.DepartureDate AS DATE) AS [DepartureDate],
        CASE 
            WHEN pf.TrainClass = 0 AND fh.TrainAsPerPackage = 1 THEN 1 
            ELSE 0 
        END AS [EconomySeats],
        CASE 
            WHEN pf.TrainClass = 1 AND fh.TrainAsPerPackage = 1 THEN 1 
            ELSE 0 
        END AS [BusinessSeats]
    FROM 
        PackageTrains AS pf
    JOIN 
        FamilyHeads AS fh ON fh.PackageId = pf.PackageId
    WHERE pf.PackageId = {0} and pf.DepartureDate is not null and fh.trashed = 0

    UNION ALL

    SELECT 
        CONCAT(fhf.Origin, ' - ', fhf.Destination) AS Location, 
        CAST(fhf.DepartureDateTime AS DATE) AS [DepartureDate],
        CASE 
            WHEN fhf.TrainClass = 0 AND fh.TrainAsPerPackage = 0 THEN 1 
            ELSE 0 
        END AS [EconomySeats],
        CASE 
            WHEN fhf.TrainClass = 1 AND fh.TrainAsPerPackage = 0 THEN 1 
            ELSE 0 
        END AS [BusinessSeats]
    FROM 
        FamilyHeadTrain AS fhf
    JOIN 
        FamilyHeads AS fh ON fh.FamilyHeadId = fhf.FamilyHeadId
    WHERE fh.PackageId = {0} and fhf.DepartureDateTime is not null and fh.trashed = 0 and fhf.TicketStatus = 0
)

SELECT 
    Location, 
    [DepartureDate], 
    SUM([EconomySeats]) AS [TotalEconomySeats],
    SUM([BusinessSeats]) AS [TotalBusinessSeats]
FROM 
    CombinedTrains
GROUP BY 
    Location, [DepartureDate];



    ", id).ToList();

            ViewBag.PackageId = id;

            return View(result);

        }

        public IActionResult PackageWiseTrainsSummary(string location, string seatType, string departureDate, int id)
        {
            DateTime parsedDepartureDate = DateTime.ParseExact(departureDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

            TrainClass trainClass = seatType switch
            {
                "economy" => TrainClass.Economy,
                "business" => TrainClass.Business,
                _ => throw new ArgumentException("Invalid seat type", nameof(seatType))
            };

            // Ensure the date is formatted as a string in 'yyyy-MM-dd' format
            string formattedDepartureDate = parsedDepartureDate.ToString("yyyy-MM-dd");

            int hajjId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var query =
                  (from fh in _dbContext.FamilyHeads
                   join pf in _dbContext.PackageTrains on fh.PackageId equals pf.PackageId
                   join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                   join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                   join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                   join g in _dbContext.Groups on gm.GroupId equals g.GroupId
                   where !fh.Trashed
                         && fh.TrainAsPerPackage
                         && fh.HajjSeasonsId == hajjId
                         && pf.DepartureDate.HasValue
                         && pf.DepartureDate.Value.Date == parsedDepartureDate.Date
                         && pf.TrainClass == trainClass
                         && (pf.Origin.ToUpper() + " - " + pf.Destination.ToUpper()) == location.ToUpper()
                         && g.GroupId == id
                   select new
                   {
                       c.CompanyCode,
                       p.Name,
                       g.Code,
                       pf.TrainClass,
                       fh.FcId,
                       Honorifics = fh.Honorifics.Title,
                       fh.GivenName,
                       fh.SurName,
                       fh.PassportNumber,
                       fh.PassportDateOfIssue,
                       fh.PassportDateOfExpiry,
                       DepartureDate = pf.DepartureDate.Value.Date,
                       DepartureTime = pf.DepartureTime,
                       Location = pf.Origin + " - " + pf.Destination
                   })
                  .Union(
                      from fh in _dbContext.FamilyHeads
                      join fhf in _dbContext.FamilyHeadTrain on fh.FamilyHeadId equals fhf.FamilyHeadId
                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                      join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                      join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                      join g in _dbContext.Groups on gm.GroupId equals g.GroupId
                      where !fh.Trashed
                            && !fh.TrainAsPerPackage
                            && fh.HajjSeasonsId == hajjId
                            && fhf.DepartureDateTime.HasValue
                            && fhf.DepartureDateTime.Value.Date == parsedDepartureDate.Date
                            && fhf.TrainClass == trainClass
                            && (fhf.Origin.ToUpper() + " - " + fhf.Destination.ToUpper()) == location.ToUpper()
                            && g.GroupId == id
                      select new
                      {
                          c.CompanyCode,
                          p.Name,
                          g.Code,
                          fhf.TrainClass,
                          fh.FcId,
                          Honorifics = fh.Honorifics.Title,
                          fh.GivenName,
                          fh.SurName,
                          fh.PassportNumber,
                          fh.PassportDateOfIssue,
                          fh.PassportDateOfExpiry,
                          DepartureDate = fhf.DepartureDateTime.Value.Date,
                          DepartureTime = fhf.DepartureTime,
                          Location = fhf.Origin + " - " + fhf.Destination
                      })
                  .AsEnumerable() // Bring the data to the client side
                  .Select(x => new
                  {
                      Honorifics = x.Honorifics,
                      x.CompanyCode,
                      x.Name,
                      x.Code,
                      TrainClass = x.TrainClass.ToString(),
                      x.FcId,
                      x.GivenName,
                      x.SurName,
                      x.PassportNumber,
                      PassportDateOfIssue = x.PassportDateOfIssue?.ToString() ?? "",
                      PassportDateOfExpiry = x.PassportDateOfExpiry?.ToString() ?? "",
                      DepartureDate = x.DepartureDate.ToString("dd/MMM/yyyy"),
                      DepartureTime = x.DepartureTime?.ToString() ?? "N/A",
                      x.Location
                  }).ToList();

            return View(query);
        }

        public IActionResult GroupWiseFlightSummarySelection()
        {
            ViewBag.GroupWiseSummary = _dbContext.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View();
        }

        public IActionResult GroupWiseFlightSummary(int id)
        {
            var result = _dbContext.CombinedFlightsResult
                .FromSqlRaw(@"
                SELECT
    Location,
    DepartureDate,
    SUM(EconomySeats) AS [TotalEconomySeats],
    SUM(BusinessSeats) AS [TotalBusinessSeats],
    SUM(SelfTickets) AS [TotalSelfTickets]
FROM (
    SELECT
        CONCAT(UPPER(pf.Origin), ' - ', UPPER(pf.Destination)) AS Location, 
        CAST(pf.DepartureDate AS DATE) AS [DepartureDate],
        SUM(CASE 
            WHEN pf.FlightClass = 0 AND fh.FlightAsPerPackage = 1 AND fh.FlightTicketType IN (0, 2) THEN 1 
            ELSE 0 
        END) AS [EconomySeats],
        SUM(CASE 
            WHEN pf.FlightClass = 1 AND fh.FlightAsPerPackage = 1 AND fh.FlightTicketType IN (0, 2) THEN 1 
            ELSE 0 
        END) AS [BusinessSeats],
        SUM(CASE 
            WHEN fh.FlightTicketType = 1 THEN 1 
            ELSE 0 
        END) AS [SelfTickets]
    FROM Groups AS g
    INNER JOIN GroupMembers AS gm ON gm.GroupId = g.GroupId
    INNER JOIN familyheads AS fh ON fh.FamilyHeadId = gm.FamilyHeadId
    INNER JOIN packages AS p ON fh.PackageId = p.PackageId
    INNER JOIN PackageFlights AS pf ON pf.PackageId = p.PackageId
    WHERE g.GroupId = {0} AND fh.Trashed = 0 and pf.DepartureDate is not null
    AND fh.FlightAsPerPackage = 1
    GROUP BY 
        CONCAT(UPPER(pf.Origin), ' - ', UPPER(pf.Destination)), 
        CAST(pf.DepartureDate AS DATE)

    UNION ALL

    SELECT
        CONCAT(UPPER(fhf.Origin), ' - ', UPPER(fhf.Destination)) AS Location, 
        CAST(fhf.DepartureDateTime AS DATE) AS [DepartureDate],
        SUM(CASE 
            WHEN fhf.FlightClass = 0 AND fh.FlightAsPerPackage = 0 AND fh.FlightTicketType IN (0, 2) THEN 1 
            ELSE 0 
        END) AS [EconomySeats],
        SUM(CASE 
            WHEN fhf.FlightClass = 1 AND fh.FlightAsPerPackage = 0 AND fh.FlightTicketType IN (0, 2) THEN 1 
            ELSE 0 
        END) AS [BusinessSeats],
        SUM(CASE 
            WHEN fh.FlightTicketType = 1 THEN 1 
            ELSE 0 
        END) AS [SelfTickets]
    FROM Groups AS g
    INNER JOIN GroupMembers AS gm ON gm.GroupId = g.GroupId
    INNER JOIN familyheads AS fh ON fh.FamilyHeadId = gm.FamilyHeadId
    INNER JOIN packages AS p ON fh.PackageId = p.PackageId
    INNER JOIN FamilyHeadFlight AS fhf ON fhf.FamilyHeadId = fh.FamilyHeadId
    WHERE g.GroupId = {0} AND fh.Trashed = 0 and fhf.DepartureDateTime is not null
    AND fh.FlightAsPerPackage = 0 and fhf.TicketStatus = 0
    GROUP BY 
        CONCAT(UPPER(fhf.Origin), ' - ', UPPER(fhf.Destination)), 
        CAST(fhf.DepartureDateTime AS DATE)
) AS CombinedResults
GROUP BY 
    Location,
    DepartureDate



    ", id).ToList();

            ViewBag.PackageId = id;

            return View(result);

        }

        public IActionResult GroupWiseFlightsSummary(string location, string seatType, string departureDate, int id)
        {
            DateTime parsedDepartureDate = DateTime.ParseExact(departureDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

            FlightClass flightClass = seatType switch
            {
                "economy" => FlightClass.Economy,
                "business" => FlightClass.Business,
                _ => throw new ArgumentException("Invalid seat type", nameof(seatType))
            };

            //Ensure the date is formatted as a string in 'yyyy-MM-dd' format
            string formattedDepartureDate = parsedDepartureDate.ToString("yyyy-MM-dd");

            int hajjId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            var query =
                    (from fh in _dbContext.FamilyHeads
                     join pf in _dbContext.PackageFlights on fh.PackageId equals pf.PackageId
                     join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                     join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                     join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                     join g in _dbContext.Groups on gm.GroupId equals g.GroupId
                     where !fh.Trashed
                           && fh.FlightAsPerPackage
                           && fh.HajjSeasonsId == hajjId
                           && pf.DepartureDate.HasValue
                           && pf.DepartureDate.Value.Date == parsedDepartureDate.Date
                           && pf.FlightClass == flightClass
                           && (pf.Origin.ToUpper() + " - " + pf.Destination.ToUpper()) == location.ToUpper()
                           && g.GroupId == id
                     select new
                     {
                         c.CompanyCode,
                         p.Name,
                         g.Code,
                         pf.FlightClass,
                         fh.FcId,
                         Honorifics = fh.Honorifics.Title,
                         fh.GivenName,
                         fh.SurName,
                         fh.PassportNumber,
                         fh.PassportDateOfIssue,
                         fh.PassportDateOfExpiry,
                         DepartureDate = pf.DepartureDate.Value.Date,
                         Location = pf.Origin + " - " + pf.Destination,
                         pf.DepartureTime
                     })
                    .Union(
                        from fh in _dbContext.FamilyHeads
                        join fhf in _dbContext.FamilyHeadFlight on fh.FamilyHeadId equals fhf.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                        join g in _dbContext.Groups on gm.GroupId equals g.GroupId
                        where !fh.Trashed
                              && !fh.FlightAsPerPackage
                              && fh.HajjSeasonsId == hajjId
                              && fhf.DepartureDateTime.HasValue
                              && fhf.DepartureDateTime.Value.Date == parsedDepartureDate.Date
                              && fhf.FlightClass == flightClass && fhf.TicketStatus == TicketStatus.Active
                              && (fhf.Origin.ToUpper() + " - " + fhf.Destination.ToUpper()) == location.ToUpper()
                              && g.GroupId == id
                        select new
                        {
                            c.CompanyCode,
                            p.Name,
                            g.Code,
                            fhf.FlightClass,
                            fh.FcId,
                            Honorifics = fh.Honorifics.Title,
                            fh.GivenName,
                            fh.SurName,
                            fh.PassportNumber,
                            fh.PassportDateOfIssue,
                            fh.PassportDateOfExpiry,
                            DepartureDate = fhf.DepartureDateTime.Value.Date,
                            Location = fhf.Origin + " - " + fhf.Destination,
                            fhf.DepartureTime
                        })
                    .AsEnumerable() // Bring the data to the client side
                    .Select(x => new
                    {
                        Honorifics = x.Honorifics,
                        x.CompanyCode,
                        x.Name,
                        x.Code,
                        FlightClass = x.FlightClass.ToString(),
                        x.FcId,
                        x.GivenName,
                        x.SurName,
                        x.PassportNumber,
                        PassportDateOfIssue = x.PassportDateOfIssue?.ToString() ?? "",
                        PassportDateOfExpiry = x.PassportDateOfExpiry?.ToString() ?? "",
                        DepartureDate = x.DepartureDate.ToString("dd/MMM/yyyy"),
                        x.Location,
                        DepartureTime = x.DepartureTime?.ToString() ?? "N/A"
                    }).ToList();


            return View(query);
        }

        public IActionResult GroupWiseTrainSummarySelection()
        {
            ViewBag.GroupWiseSummary = _dbContext.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            return View();
        }

        public IActionResult GroupWiseTrainSummary(int id)
        {
            var result = _dbContext.CombinedTrainsResult
                .FromSqlRaw(@"
        WITH CombinedTrains AS (
    SELECT 
        CONCAT(pf.Origin, ' - ', pf.Destination) AS Location, 
        CAST(pf.DepartureDate AS DATE) AS [DepartureDate],
        CASE 
            WHEN pf.TrainClass = 0 AND fh.TrainAsPerPackage = 1 THEN 1 
            ELSE 0 
        END AS [EconomySeats],
        CASE 
            WHEN pf.TrainClass = 1 AND fh.TrainAsPerPackage = 1 THEN 1 
            ELSE 0 
        END AS [BusinessSeats]
    FROM 
        PackageTrains AS pf
    JOIN 
        FamilyHeads AS fh ON fh.PackageId = pf.PackageId
	JOIN 
        GroupMembers AS gm ON fh.FamilyHeadId = gm.FamilyHeadId
    WHERE gm.GroupId = {0} and pf.DepartureDate is not null

    UNION ALL

    SELECT 
        CONCAT(fhf.Origin, ' - ', fhf.Destination) AS Location, 
        CAST(fhf.DepartureDateTime AS DATE) AS [DepartureDate],
        CASE 
            WHEN fhf.TrainClass = 0 AND fh.TrainAsPerPackage = 0 THEN 1 
            ELSE 0 
        END AS [EconomySeats],
        CASE 
            WHEN fhf.TrainClass = 1 AND fh.TrainAsPerPackage = 0 THEN 1 
            ELSE 0 
        END AS [BusinessSeats]
    FROM 
        FamilyHeadTrain AS fhf
    JOIN 
        FamilyHeads AS fh ON fh.FamilyHeadId = fhf.FamilyHeadId
	JOIN
        GroupMembers AS gm ON fh.FamilyHeadId = gm.FamilyHeadId
    WHERE gm.GroupId = {0} and fhf.DepartureDateTime is not null
)

SELECT 
    Location, 
    [DepartureDate], 
    SUM([EconomySeats]) AS [TotalEconomySeats],
    SUM([BusinessSeats]) AS [TotalBusinessSeats]
FROM 
    CombinedTrains
GROUP BY 
    Location, [DepartureDate];

    ", id).ToList();

            ViewBag.PackageId = id;

            return View(result);

        }

        public IActionResult GroupWiseTrainsSummary(string location, string seatType, string departureDate, int id)
        {
            DateTime parsedDepartureDate = DateTime.ParseExact(departureDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

            TrainClass trainClass = seatType switch
            {
                "economy" => TrainClass.Economy,
                "business" => TrainClass.Business,
                _ => throw new ArgumentException("Invalid seat type", nameof(seatType))
            };

            // Ensure the date is formatted as a string in 'yyyy-MM-dd' format
            string formattedDepartureDate = parsedDepartureDate.ToString("yyyy-MM-dd");

            int hajjId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var query =
                  (from fh in _dbContext.FamilyHeads
                   join pf in _dbContext.PackageTrains on fh.PackageId equals pf.PackageId
                   join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                   join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                   join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                   join g in _dbContext.Groups on gm.GroupId equals g.GroupId
                   where !fh.Trashed
                         && fh.TrainAsPerPackage
                         && fh.HajjSeasonsId == hajjId
                         && pf.DepartureDate.HasValue
                         && pf.DepartureDate.Value.Date == parsedDepartureDate.Date
                         && pf.TrainClass == trainClass
                         && (pf.Origin.ToUpper() + " - " + pf.Destination.ToUpper()) == location.ToUpper()
                         && g.GroupId == id
                   select new
                   {
                       c.CompanyCode,
                       p.Name,
                       g.Code,
                       pf.TrainClass,
                       fh.FcId,
                       Honorifics = fh.Honorifics.Title,
                       fh.GivenName,
                       fh.SurName,
                       fh.PassportNumber,
                       fh.PassportDateOfIssue,
                       fh.PassportDateOfExpiry,
                       DepartureDate = pf.DepartureDate.Value.Date,
                       DepartureTime = pf.DepartureTime,
                       Location = pf.Origin + " - " + pf.Destination
                   })
                  .Union(
                      from fh in _dbContext.FamilyHeads
                      join fhf in _dbContext.FamilyHeadTrain on fh.FamilyHeadId equals fhf.FamilyHeadId
                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                      join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                      join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId
                      join g in _dbContext.Groups on gm.GroupId equals g.GroupId
                      where !fh.Trashed
                            && !fh.TrainAsPerPackage
                            && fh.HajjSeasonsId == hajjId
                            && fhf.DepartureDateTime.HasValue
                            && fhf.DepartureDateTime.Value.Date == parsedDepartureDate.Date
                            && fhf.TrainClass == trainClass
                            && (fhf.Origin.ToUpper() + " - " + fhf.Destination.ToUpper()) == location.ToUpper()
                            && g.GroupId == id
                      select new
                      {
                          c.CompanyCode,
                          p.Name,
                          g.Code,
                          fhf.TrainClass,
                          fh.FcId,
                          Honorifics = fh.Honorifics.Title,
                          fh.GivenName,
                          fh.SurName,
                          fh.PassportNumber,
                          fh.PassportDateOfIssue,
                          fh.PassportDateOfExpiry,
                          DepartureDate = fhf.DepartureDateTime.Value.Date,
                          DepartureTime = fhf.DepartureTime,
                          Location = fhf.Origin + " - " + fhf.Destination
                      })
                  .AsEnumerable() // Bring the data to the client side
                  .Select(x => new
                  {
                      Honorifics = x.Honorifics,
                      x.CompanyCode,
                      x.Name,
                      x.Code,
                      TrainClass = x.TrainClass.ToString(),
                      x.FcId,
                      x.GivenName,
                      x.SurName,
                      x.PassportNumber,
                      PassportDateOfIssue = x.PassportDateOfIssue?.ToString() ?? "",
                      PassportDateOfExpiry = x.PassportDateOfExpiry?.ToString() ?? "",
                      DepartureDate = x.DepartureDate.ToString("dd/MMM/yyyy"),
                      DepartureTime = x.DepartureTime?.ToString() ?? "N/A",
                      x.Location
                  }).ToList();

            return View(query);
        }

        public IActionResult RoomDemandReportSelection()
        {
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash;

            return View();
        }

        public IActionResult RoomDemandReport(DateTime Date, int HotelId)
        {
            RoomDemandReportModel model = new();

            var rooms = _dbContext.RoomsWithoutTrash
                .Select(e => new
                {
                    e.RoomId,
                    e.PAX,
                    e.RoomCode
                })
                .OrderBy(e => e.RoomId)
                .ToList();


            model.RoomWiseHeaders = string.Join("", rooms.Select(e => $"<th>{e.RoomCode}</th>"));

            var packages = _dbContext.PackagesWithoutTrash
                .Include(e => e.Company)
                .Select(e => new
                {
                    e.PackageId,
                    e.Name,
                    e.Company.CompanyCode,
                    e.HajjSeasonsId
                }).Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            var packageAccommodations = _dbContext.PackageAccomodtions
                                        .Include(e => e.Package)
                                        .Where(e => !e.Package.Trashed)
                                        .Where(s => s.Package.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            Hotel hotel = _dbContext.Hotels
                .Include(e => e.Place)
                .FirstOrDefault(e => e.HotelId == HotelId);

            var familyHeadAccommodations = _dbContext.FamilyHeadAccomodations
                                            .Select(e => new
                                            {
                                                e.FamilyHeadId,
                                                e.IndividualFacilityDescription.RoomId,
                                                e.CheckIn,
                                                e.IndividualFacilityDescription.PlaceId,
                                                e.HotelId,
                                                e.IndividualFacilityDescription.Room.PAX,
                                                e.FamilyHead.Trashed,
                                                e.FamilyHead.HajjSeasonsId,
                                                e.FamilyHead.PackageId
                                            })
                                            .Where(e => !e.Trashed)
                                            .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            int i = 0;
            foreach (var package in packages)
            {
                RoomDemandReportRowModel row = new RoomDemandReportRowModel()
                {
                    SNo = ++i,
                    Company = package.CompanyCode,
                    Package = package.Name,
                    PackageCheckOut = packageAccommodations
                                        .FirstOrDefault(e => e.PackageId == package.PackageId && e.HotelId == HotelId)?
                                        .ActualCheckOutTime?.ToString("dd/MMM/yyyy HH:mm"),
                    Hotel = hotel.HotelName,
                    Place = hotel.Place.PlaceName,
                    CheckIn = Date.ToString("dd/MMM/yyyy"),
                };

                foreach (var room in rooms)
                {
                    float pax = (float)familyHeadAccommodations
                        .Where(e => e.PackageId == package.PackageId && e.HotelId == HotelId
                        && e.CheckIn.HasValue && e.CheckIn.Value.Date == Date.Date && e.RoomId == room.RoomId).Count() / (float)room.PAX;
                    row.RoomCounts.Add(pax);
                }


                model.Rows.Add(row);
            }

            return View(model);
        }

        //public IActionResult CompanyWiseRailwayTravelSummarySelection()
        //{
        //    ViewBag.Companies = _dbContext.CompaniesWithoutTrash
        //                    .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")));

        //    return View();
        //}

        //public IActionResult CompanyWiseRailwayTravelSummary()
        //{
        //    var companies = _dbContext.CompaniesWithoutTrash
        //                    .Select(e => new
        //                    {
        //                        e.CompanyId,
        //                        e.CompanyName,
        //                        e.HajjSeasonsId
        //                    })
        //                    .Where(e => e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
        //                    .ToList();

        //    var members = _dbContext.FamilyHeadsWithoutTrash
        //                    .Select(e => new
        //                    {
        //                        e.Package.CompanyId,
        //                        e.PackageId,
        //                        e.FlightTicketType,
        //                        e.IsTicketExcluded,
        //                        e.FamilyHeadId,
        //                        PackageTrashed = e.Package.Trashed,
        //                        e.FlightAsPerPackage
        //                    })
        //                    .Where(e => companies.Select(e => e.CompanyId).Contains(e.CompanyId) && !e.PackageTrashed)
        //                    .ToList();

        //    var packageFlights = _dbContext.PackageFlights
        //                        .Where(e => members.Select(e => e.PackageId).Contains(e.PackageId))
        //                        .ToList();

        //    var memberFlights = _dbContext.FamilyHeadFlight
        //                                .Where(e => members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
        //                                .ToList();

        //    var model = new List<CompanyWiseRailwayTravelReportModel>();

        //    foreach (var item in companies)
        //    {
        //        int economySeats = 0, businessSeats = 0, selfSeats = 0;

        //        var companyMembers = members.Where(e => e.CompanyId == item.CompanyId);

        //        foreach (var m in companyMembers)
        //        {
        //            if (m.IsTicketExcluded) selfSeats++;
        //            else if (m.FlightAsPerPackage)
        //            {
        //                economySeats += packageFlights.Where(e => e.PackageId == m.PackageId && e.FlightClass == FlightClass.Economy).Count();
        //                businessSeats += packageFlights.Where(e => e.PackageId == m.PackageId && e.FlightClass == FlightClass.Business).Count();
        //            }
        //            else if (!m.FlightAsPerPackage)
        //            {
        //                economySeats += memberFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId && e.FlightClass == FlightClass.Economy).Count();
        //                businessSeats += memberFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId && e.FlightClass == FlightClass.Business).Count();
        //            }

        //        }
        //        var _ = new CompanyWiseRailwayTravelReportModel()
        //        {
        //            CompanyId = item.CompanyId,
        //            CompanyName = item.CompanyName,
        //            BusinessSeats = businessSeats,
        //            EconomySeats = economySeats,
        //            SelfSeats = selfSeats
        //        };
        //        model.Add(_);
        //    }


        //    //return Ok(model);
        //    return View(model);
        //}

        //public IActionResult CompanyWisePODWiseFlightSeats(int id, string seatType)
        //{
        //    var companies = _dbContext.CompaniesWithoutTrash
        //                    .Select(e => new
        //                    {
        //                        e.CompanyId,
        //                        e.CompanyName,
        //                        e.HajjSeasonsId
        //                    })
        //                    .Where(e => e.CompanyId == id)
        //                    .ToList();

        //    var members = _dbContext.FamilyHeadsWithoutTrash
        //                    .Select(e => new
        //                    {
        //                        e.Package.CompanyId,
        //                        e.PackageId,
        //                        e.FlightTicketType,
        //                        e.IsTicketExcluded,
        //                        e.FamilyHeadId,
        //                        PackageTrashed = e.Package.Trashed,
        //                        e.FlightAsPerPackage
        //                    })
        //                    .Where(e => companies.Select(e => e.CompanyId).Contains(e.CompanyId) && !e.PackageTrashed)
        //                    .ToList();

        //    var packageFlights = _dbContext.PackageFlights
        //                        .Where(e => members.Select(e => e.PackageId).Contains(e.PackageId))
        //                        .ToList();

        //    var memberFlights = _dbContext.FamilyHeadFlight
        //                                .Where(e => members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
        //                                .ToList();

        //    var uniqueOrigins = new List<string>();

        //    uniqueOrigins.AddRange(packageFlights.Select(e => e.Origin.ToUpper()));
        //    uniqueOrigins.AddRange(memberFlights.Select(e => e.Origin.ToUpper()));

        //    uniqueOrigins = uniqueOrigins.Distinct().ToList();


        //    return Ok(new
        //    {
        //        id,
        //        seatType,
        //        uniqueOrigins
        //    });
        //}

        //public IActionResult PackageWiseFlightSummarySelection()
        //{
        //    ViewBag.Packages = _dbContext.PackagesWithoutTrash
        //        .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
        //        .ToList();

        //    return View();
        //}

        //public IActionResult PackageWiseFlightSummary(int id)
        //{
        //    return Ok();
        //}

        public IActionResult BookingWiseReceiptReportSelection(int id)
        {
            ViewBag.Heads = _dbContext.FamilyHeadsWithoutTrash
                            .Where(e => e.IsHead)
                            .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .ToList();

            return View();
        }

        public IActionResult BankReceiptReport(int id)
        {
            ViewBag.Banks = _dbContext.CompanyBankAccountsWithoutTrash;
            ViewBag.Company = _dbContext.CompaniesWithoutTrash;

            return View();
        }

        public IActionResult ReceiptReportBank(int id, string startDate, string endDate)
        {
            var hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            DateTime startDateTime = DateTime.Parse(startDate);

            DateTime endDateTime = string.IsNullOrEmpty(endDate)
                ? DateTime.Now
                : DateTime.Parse(endDate);


            float balanceCF = _dbContext.NewInvoicesWithoutTrash
                    .Where(e => e.CompanyBankAccountId == id
                                && e.HajjSeasonsId == hajjSeasonId
                                && e.Date < startDateTime)
                    .AsEnumerable()
                    .Sum(e => e.AmountRecieved);

            float runningBalance = balanceCF;

            var bankWiseReport = _dbContext.NewInvoicesWithoutTrash
                .Include(e => e.CompanyBankAccount)
                .Include(e => e.FamilyHead)
                .Where(e => e.CompanyBankAccountId == id
                            && e.HajjSeasonsId == hajjSeasonId
                            && e.Date >= startDateTime
                            && e.Date <= endDateTime)
                .OrderBy(e => e.Date)
                .AsEnumerable()
                .Select(e =>
                {
                    float debit = e.AmountRecieved < 0 ? Math.Abs(e.AmountRecieved) : 0;
                    float credit = e.AmountRecieved > 0 ? e.AmountRecieved : 0;

                    runningBalance += credit - debit;

                    return new ReceiptReportModel
                    {
                        Date = e.Date,
                        BankName = e.CompanyBankAccount.Title,
                        HajjiName = e.FamilyHead.GivenName + " " + e.FamilyHead.SurName,
                        HBNumber = e.FamilyHead.FcId,
                        CollectionCurrency = e.Currencies.ToString(),
                        HajjiCurrency = e.FamilyHead.Currencies.ToString(),
                        Remarks = e.Remarks,
                        Debit = debit > 0 ? (float?)debit : null,
                        Credit = credit > 0 ? credit : 0,
                        Balance = runningBalance
                    };
                })
                .ToList();

            ViewBag.Result = bankWiseReport;

            ViewBag.BankName = _dbContext.CompanyBankAccounts
                .Where(e => e.CompanyBankAccountId == id)
                .Select(e => e.Title)
                .FirstOrDefault();

            ViewBag.Period = startDateTime.ToString("dd/MMM/yyyy") + " to " + endDateTime.ToString("dd/MMM/yyyy");

            ViewBag.AccountNumber = _dbContext.CompanyBankAccounts
                .Where(e => e.CompanyBankAccountId == id)
                .Select(e => e.Number)
                .FirstOrDefault();



            ViewBag.BalanceCF = balanceCF;
            ViewBag.Balance = runningBalance;

            ViewBag.BankCurrency = _dbContext.CompanyBankAccounts
                .Where(e => e.CompanyBankAccountId == id)
                .Select(e => e.AccountCurrency.ToString())
                .FirstOrDefault();

            return View();
        }

        public IActionResult CompanyBankReportSelection()
        {
            ViewBag.Company = _dbContext.CompaniesWithoutTrash;
            ViewBag.CompanyBankAccount = _dbContext.BanksWithoutTrash;

            return View();
        }

        public IActionResult CompanyBankReport(int? companyId, string endDate, int? companyBankAccountId)
        {
            var hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            DateTime? endDateTime = null;

            if (!string.IsNullOrEmpty(endDate))
                endDateTime = DateTime.Parse(endDate);

            var closingBalancesQuery = _dbContext.CompanyBankAccounts.AsQueryable();

            if (companyId.HasValue && companyId > 0)
            {
                closingBalancesQuery = closingBalancesQuery.Where(cba => cba.CompanyId == companyId);
            }

            if (companyBankAccountId.HasValue && companyBankAccountId > 0)
            {
                closingBalancesQuery = closingBalancesQuery.Where(cba => cba.BankId == companyBankAccountId);
            }

            var closingBalances = closingBalancesQuery
                .Join(_dbContext.NewInvoicesWithoutTrash
                        .Where(ni => ni.Date <= endDateTime
                                    && ni.HajjSeasonsId == hajjSeasonId),
                        cba => cba.CompanyBankAccountId,
                        ni => ni.CompanyBankAccountId,
                        (cba, ni) => new
                        {
                            cba.Title,
                            ni.AmountRecieved,
                            cba.AccountCurrency
                        })
                .AsEnumerable()
                .GroupBy(x => x.Title)
                .Select(g =>
                {
                    float runningBalance = 0;

                    var groupedTransactions = g.Select(x =>
                    {
                        float debit = x.AmountRecieved < 0 ? Math.Abs(x.AmountRecieved) : 0;
                        float credit = x.AmountRecieved > 0 ? x.AmountRecieved : 0;

                        runningBalance += credit - debit;

                        return new
                        {
                            CompanyBankAccount = x.Title,
                            Currency = Enum.GetName(typeof(Currencies), x.AccountCurrency),
                            Debit = debit > 0 ? (float?)debit : null,
                            Credit = credit > 0 ? credit : 0,
                            Balance = runningBalance
                        };
                    }).ToList();

                    return new
                    {
                        CompanyBankAccount = g.Key,
                        Currency = groupedTransactions.First().Currency,
                        Debit = groupedTransactions.Sum(x => x.Debit),
                        Credit = groupedTransactions.Sum(x => x.Credit),
                        Balance = groupedTransactions.Last().Balance
                    };
                })
                .ToList();



            if (companyId.HasValue && companyId > 0)
            {
                ViewBag.CompanyName = _dbContext.Companies
                    .Where(c => c.CompanyId == companyId)
                    .Select(c => c.CompanyName)
                    .FirstOrDefault();
            }
            else
            {
                ViewBag.CompanyName = "All TMG";
            }

            if (companyBankAccountId.HasValue && companyBankAccountId > 0)
            {
                ViewBag.BankName = _dbContext.BanksWithoutTrash
                .Where(e => e.BankId == companyBankAccountId)
                .Select(e => e.Name)
                .FirstOrDefault();
            }
            else
            {
                ViewBag.BankName = "All TMG Banks";
            }

            ViewBag.Result = closingBalances;

            ViewBag.Period = endDateTime.Value.ToString("dd/MMM/yyyy");

            //return Ok(closingBalances);

            return View();
        }

        public IActionResult NewBankStatmentSelection()
        {
            ViewBag.Company = _dbContext.CompaniesWithoutTrash
                .ToList();

            return View();
        }

        public JsonResult GetCompanyBankAccount(int id)
        {
            var companyBankAccount = _dbContext.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyId == id)
                .Select(bank => new
                {
                    bank.CompanyBankAccountId,
                    bank.Title,
                    bank.Number,
                    AccountCurrency = bank.AccountCurrency.ToString()
                })
                .ToList();

            return Json(new { success = true, data = companyBankAccount });
        }

        public IActionResult NewBankStatmentReport(int? companyId, int? companyBankAccountId, string companyBankAccountTitle, DateTime startDate, DateTime endDate)
        {
            ViewBag.CompanyName = _dbContext.CompaniesWithoutTrash
                .Where(e => e.CompanyId == companyId)
                .Select(e => e.CompanyName)
                .FirstOrDefault();

            ViewBag.CompanyImage = _dbContext.CompaniesWithoutTrash
                .Where(e => e.CompanyId == companyId)
                .Select(e => e.Logo)
                .FirstOrDefault();

            ViewBag.CompanyBankName = _dbContext.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyBankAccountId == companyBankAccountId)
                .Select(e => e.Title)
                .FirstOrDefault();

            ViewBag.AccountNumber = _dbContext.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyBankAccountId == companyBankAccountId)
                .Select(e => e.IBAN)
                .FirstOrDefault();

            ViewBag.Iban = _dbContext.CompanyBankAccountsWithoutTrash
                .Where(e => e.CompanyBankAccountId == companyBankAccountId)
                .Select(e => e.IBAN)
                .FirstOrDefault();

            var currency = _dbContext.CompanyBankAccountsWithoutTrash
                    .Where(e => e.CompanyBankAccountId == companyBankAccountId)
                    .Select(e => e.AccountCurrency)
                    .FirstOrDefault();

            try
            {
                var totalInvoiceCredit = _dbContext.NewInvoicesWithoutTrash
                    .Where(invoice => (companyId == null || invoice.CompanyBankAccount.CompanyId == companyId) &&
                                      (companyBankAccountId == null || invoice.CompanyBankAccountId == companyBankAccountId) &&
                                      invoice.ClearanceDate < startDate
                                        && invoice.ReceiptStatus == ReceiptStatus.Cleared)
                    .Sum(invoice => (float?)invoice.AmountRecieved) ?? 0;

                var totalPaymentDebit = _dbContext.NewPaymentVouchers
                    .Where(payment => payment.Date < startDate)
                    .Where(payment => (companyId == null || payment.Payments.Any(p => p.Accounts.CompanyId == companyId)) &&
                                        (companyBankAccountId == null || payment.Payments.Any(p => p.AccountsId == companyBankAccountId)))
                    .Sum(payment => (float?)payment.TotalDebit) ?? 0;

                float balanceCF = totalInvoiceCredit - totalPaymentDebit;
                ViewBag.BalanceCF = balanceCF;

                var invoiceQuery = _dbContext.NewInvoicesWithoutTrash
                                   .Where(invoice => (companyId == null || invoice.CompanyBankAccount.CompanyId == companyId))
                                   .Where(invoice => (companyBankAccountId == null || invoice.CompanyBankAccountId == companyBankAccountId))
                                   .Where(invoice => invoice.ClearanceDate >= startDate && invoice.ClearanceDate <= endDate
                                            && invoice.ReceiptStatus == ReceiptStatus.Cleared)
                                   .Select(invoice => new BankStatementData
                                   {
                                       Date = invoice.ClearanceDate,
                                       Remarks = invoice.Remarks ?? "",
                                       DebitAmount = 0f,
                                       CreditAmount = invoice.AmountRecieved,
                                       Voucher = invoice.ReceiptNumber,
                                       Name = invoice.FamilyHead.FullName,
                                       HBNo = invoice.FamilyHead.FcId,
                                       Currency = invoice.Currencies.ToString(),
                                       Proof = invoice.ImagePath
                                   }).ToList();

                var paymentQuery = from payment in _dbContext.NewPaymentVouchers
                                   from newPayment in payment.Payments
                                   from cba in _dbContext.CompanyBankAccountsWithoutTrash.Where(e => e.BankAssetAccountId == newPayment.AccountsId)
                                   where (companyId == null || newPayment.Accounts.CompanyId == companyId && cba.CompanyBankAccountId == companyBankAccountId)
                                   where payment.Date >= startDate && payment.Date <= endDate && newPayment.Accounts.Currencies == currency
                                   select new BankStatementData
                                   {
                                       Date = payment.Date,
                                       Remarks = newPayment.AccountNature == AccountNature.Debit ? payment.DebitRemarks : payment.CreditRemarks ?? "",
                                       DebitAmount = newPayment.AccountNature == AccountNature.Credit ? payment.TotalCredit ?? 0f : 0f,
                                       CreditAmount = newPayment.AccountNature == AccountNature.Debit ? payment.TotalDebit ?? 0f : 0f,
                                       Voucher = payment.PaymentVoucherNo,
                                       Name = newPayment.Accounts.Name,
                                       HBNo = "",
                                       Currency = newPayment.Accounts.Currencies.ToString(),
                                       Proof = payment.File
                                   };

                var bankChargesQuery = from payment in _dbContext.NewPaymentVouchers
                                       from newPayment in payment.Payments
                                       from cba in _dbContext.CompanyBankAccountsWithoutTrash.Where(e => e.BankAssetAccountId == newPayment.AccountsId)
                                       where (companyId == null || newPayment.Accounts.CompanyId == companyId)
                                       where payment.Date >= startDate && payment.Date <= endDate && cba.CompanyBankAccountId == companyBankAccountId
                                       where newPayment.AccountNature == AccountNature.Credit && payment.DebitBankCharges.HasValue
                                       where payment.DebitBankCharges > 0 && newPayment.Accounts.Currencies == currency
                                       select new BankStatementData
                                       {
                                           Date = payment.Date,
                                           Remarks = "Bank Charges",
                                           DebitAmount = payment.DebitBankCharges ?? 0f,
                                           CreditAmount = 0f,
                                           Voucher = payment.PaymentVoucherNo,
                                           Name = newPayment.Accounts.Name,
                                           HBNo = "",
                                           Currency = newPayment.Accounts.Currencies.ToString()
                                       };

                var allData = invoiceQuery
                    .Union(paymentQuery)
                    .Union(bankChargesQuery)
                    .OrderBy(x => x.Date)
                    .ToList();

                float runningBalance = balanceCF;

                var finalReport = allData.Select(item =>
                {
                    runningBalance += (item.CreditAmount - item.DebitAmount);
                    return new BankStatment
                    {
                        Voucher = item.Voucher,
                        Name = item.Name,
                        HBNo = item.HBNo,
                        Date = item.Date,
                        Remarks = item.Remarks,
                        Debit = item.DebitAmount,
                        Credit = item.CreditAmount,
                        Balance = runningBalance,
                        Currency = item.Currency,
                        Proof = item.Proof
                    };
                }).ToList();

                return View(finalReport);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult RecieptSummarySelection()
        {
            ViewBag.Company = _dbContext.CompaniesWithoutTrash
                .ToList();

            return View();
        }

        public IActionResult RecieptSummaryReport(int? companyId, string fromDate, string toDate)
        {
            DateTime startDate = DateTime.Parse(fromDate);
            DateTime endDate = DateTime.Parse(toDate);

            var result = _dbContext.NewInvoicesWithoutTrash
                .Where(ni =>
                    ni.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")) &&
                    ni.Date >= startDate && ni.Date <= endDate &&
                    (companyId == null || companyId == 0 ||
                     _dbContext.CompanyBankAccounts.Any(cba => cba.CompanyBankAccountId == ni.CompanyBankAccountId && cba.CompanyId == companyId)))
                .Join(_dbContext.CompanyBankAccounts,
                        ni => ni.CompanyBankAccountId,
                        cba => cba.CompanyBankAccountId,
                        (ni, cba) => new { ni, cba })
                .Join(_dbContext.FamilyHeads,
                        temp => temp.ni.FamilyHeadId,
                        fh => fh.FamilyHeadId,
                        (temp, fh) => new
                        {
                            temp.cba.CompanyId,
                            ReceiptNumber = temp.ni.ReceiptNumber,
                            ClientName = fh.GivenName + " " + fh.SurName,
                            Amount = temp.ni.AmountRecieved,
                            Currency = temp.ni.Currencies,
                            DepositBank = temp.cba.Title,
                            Status = temp.ni.ReceiptStatus.ToString(),
                            HbNo = fh.FcId,
                            HajjiCurrency = fh.Currencies.ToString()
                        })
                .GroupBy(x => new { x.CompanyId, x.Currency })
                .Select(g => new CompanyBankSummaryViewModel
                {
                    CompanyName = _dbContext.Companies.FirstOrDefault(c => c.CompanyId == g.Key.CompanyId).CompanyName,
                    Banks = g.Select(b => new ReceiptSummary
                    {
                        ReceiptNo = b.ReceiptNumber,
                        ClientName = b.ClientName,
                        Amount = b.Amount,
                        DepositBank = b.DepositBank,
                        Status = b.Status,
                        Currency = b.Currency.ToString(),
                        HbNo = b.HbNo,
                        HajjiCurrency = b.HajjiCurrency
                    }).ToList()
                })
                .ToList();

            foreach (var company in result)
            {
                foreach (var bank in company.Banks)
                {
                    bank.Currency = bank.Currency.ToString();
                }
            }

            return View(result);
        }

        public IActionResult VendorReportSelection()
        {
            ViewBag.VendorAgent = _dbContext.Vendors
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);

            ViewBag.Country = _dbContext.Countries.Where(e => !e.Trashed);
            ViewBag.City = _dbContext.Cities.Where(e => !e.Trashed);

            return View();
        }

        public IActionResult VendorReport(int vendorId, int cityId, int countryId)
        {
            ViewBag.VendorName = _dbContext.Vendors
                .Where(e => e.VendorId == vendorId)
                .Select(e => e.VendorName)
                .FirstOrDefault();

            ViewBag.Address = _dbContext.Vendors
                .Where(e => e.VendorId == vendorId)
                .Select(e => e.BeneficiaryAddress)
                .FirstOrDefault();

            ViewBag.AccountTitle = _dbContext.Vendors
                .Where(e => e.VendorId == vendorId)
                .Select(e => e.AccountTitle)
                .FirstOrDefault();

            ViewBag.AccountNo = _dbContext.Vendors
                .Where(e => e.VendorId == vendorId)
                .Select(e => e.AccountNumber)
                .FirstOrDefault();

            ViewBag.BAddress = _dbContext.Vendors
                .Where(e => e.VendorId == vendorId)
                .Select(e => e.BankAddress)
                .FirstOrDefault();

            ViewBag.IBAN = _dbContext.Vendors
                .Where(e => e.VendorId == vendorId)
                .Select(e => e.IBAN)
                .FirstOrDefault();

            ViewBag.SwiftCode = _dbContext.Vendors
                .Where(e => e.VendorId == vendorId)
                .Select(e => e.SwiftCode)
                .FirstOrDefault();

            var result = _dbContext.VendorContactPersons
                .Include(e => e.Vendor)
                .Include(e => e.Designation)
                .Where(e => e.VendorId == vendorId
                        && e.CountryId == countryId && e.CityId == cityId)
                .Select(e => new VendorSummary
                {
                    Name = e.Name,
                    Position = e.Designation.Name,
                    Email = e.Email,
                    Mobile = e.Contact,
                    Address = e.Address,
                    Status = e.ActiveStatus.ToString()
                })
                .ToList();

            return View(result);
        }

        public IActionResult PackageSummary()
        {
            ViewBag.Packages = _dbContext.PackagesWithoutTrash.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            return View();
        }

        public IActionResult PackageSummaryReport(int[] PackageIds)
        {
            ViewBag.PersonCount = _dbContext.FamilyHeadsWithoutTrash.Where(e => PackageIds.Contains(e.PackageId)).Count();

            var packagereport = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .ThenInclude(e => e.Room)
                .Where(e => PackageIds.Contains(e.PackageId))
                .Select(e => new PackageSummary
                {
                    PackageName = e.Package.Name,
                    FullName = e.FullName,
                    HBNO = e.FcId,
                    RoomType = e.Package.Room.RoomCode
                })
                .ToList();

            return View(packagereport);
        }

        public IActionResult HotelSummary(int hotelId, int roomId)
        {
            List<HajjSummaryData> result = new List<HajjSummaryData>();
            int maleCount = 0;
            int femaleCount = 0;
            int roomPax = 1;

            var hajjSeasonId = Convert.ToInt32(HttpContext.Session.GetString("ActiveSeasonId"));

            var hasAsPerPackage = _dbContext.FamilyHeads
                .Any(fh => fh.AccomodationAsPerPackage == true
                        && fh.HajjSeasonsId == hajjSeasonId
                        && !fh.Trashed
                        && fh.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var hasCustomAccommodation = _dbContext.FamilyHeads
                .Any(fh => fh.AccomodationAsPerPackage == false
                        && fh.HajjSeasonsId == hajjSeasonId
                        && !fh.Trashed
                        && fh.HajjiBookingStatus == HajjiBookingStatus.Booked);

            if (hasAsPerPackage)
            {
                var asPerPackageResult = (from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join g in _dbContext.GroupsWithoutTrash on fh.PackageId equals g.PackageId into gj
                                          from g in gj.DefaultIfEmpty()
                                          where pa.HotelId == hotelId
                                                && ((pa.PlaceId == 4 && p.RoomTypesId == roomId) || (pa.PlaceId != 4 && p.RoomId == roomId))
                                                && fh.AccomodationAsPerPackage == true && fh.HajjSeasonsId == hajjSeasonId
                                                && !fh.Trashed && fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                          orderby fh.FcId ascending
                                          select new HajjSummaryData
                                          {
                                              HajjiName = fh.GivenName + " " + fh.SurName,
                                              HBNo = fh.FcId,
                                              PackageName = p.Name,
                                              RoomPax = (pa.PlaceId == 4 ? p.RoomTypes.PAX : p.Room.PAX),
                                              Gender = fh.Gender.ToString(),
                                              PassportNo = fh.PassportNumber,
                                              GroupName = g != null ? g.Name : "N/A",
                                              CheckIn = pa.ActualCheckInTime.ToString(),
                                              CheckOut = pa.ActualCheckOutTime.ToString()
                                          }).ToList();

                result.AddRange(asPerPackageResult);

                maleCount += asPerPackageResult.Count(r => r.Gender == Gender.Male.ToString());
                femaleCount += asPerPackageResult.Count(r => r.Gender == Gender.Female.ToString());

                roomPax = asPerPackageResult.FirstOrDefault()?.RoomPax ?? roomPax;
            }

            if (hasCustomAccommodation)
            {
                var customResult = (from fh in _dbContext.FamilyHeads
                                    join fpa in _dbContext.FamilyHeadAccomodations on fh.FamilyHeadId equals fpa.FamilyHeadId
                                    join g in _dbContext.GroupsWithoutTrash on fh.PackageId equals g.PackageId into gj
                                    from g in gj.DefaultIfEmpty()
                                    join r in _dbContext.Rooms on fpa.IndividualFacilityDescription.RoomId equals r.RoomId into rj
                                    from r in rj.DefaultIfEmpty()
                                    where fpa.HotelId == hotelId && fpa.IndividualFacilityDescription.RoomId == roomId
                                          && fh.AccomodationAsPerPackage == false && fh.HajjSeasonsId == hajjSeasonId
                                          && !fh.Trashed && fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                    orderby fh.FcId ascending
                                    select new HajjSummaryData
                                    {
                                        HajjiName = fh.GivenName + " " + fh.SurName,
                                        HBNo = fh.FcId,
                                        PackageName = fh.Package.Name,
                                        RoomPax = r != null ? r.PAX : 1,
                                        Gender = fh.Gender.ToString(),
                                        PassportNo = fh.PassportNumber,
                                        GroupName = g != null ? g.Name : "N/A",
                                        CheckIn = fpa.CheckIn.ToString(),
                                        CheckOut = fpa.CheckOut.ToString()
                                    }).ToList();

                result.AddRange(customResult);

                maleCount += customResult.Count(r => r.Gender == Gender.Male.ToString());
                femaleCount += customResult.Count(r => r.Gender == Gender.Female.ToString());

                if (roomPax == 1)
                    roomPax = customResult.FirstOrDefault()?.RoomPax ?? roomPax;
            }

            ViewBag.MaleCount = maleCount;
            ViewBag.FemaleCount = femaleCount;
            ViewBag.MaleRatio = roomPax > 0 ? (float)maleCount / roomPax : 0;
            ViewBag.FemaleRatio = roomPax > 0 ? (float)femaleCount / roomPax : 0;

            return View(result);
        }

        public IActionResult SPOSummaryReport(int spoId, HajjiBookingStatus status)
        {
            ViewBag.SPOName = _dbContext.SPOsWithoutTrash
                .Where(e => e.SPOId == spoId)
                .Select(e => e.Name)
                .FirstOrDefault();

            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var query = (from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId

                         join c in _dbContext.CompaniesWithoutTrash on p.CompanyId equals c.CompanyId

                         join fhe in _dbContext.FamilyHeadEmails
                             .Where(e => e.Preferred == true) // ✅ Preferred Email Only
                             on fh.FamilyHeadId equals fhe.FamilyHeadId into fheGroup
                         from fhe in fheGroup.DefaultIfEmpty()

                         join fhc in _dbContext.FamilyHeadContacts
                             .Where(c => c.Preferred == true) // ✅ Preferred Contact Only
                             on fh.FamilyHeadId equals fhc.FamilyHeadId into fhcGroup
                         from fhc in fhcGroup.DefaultIfEmpty()

                         join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                         from gm in gmGroup.DefaultIfEmpty()

                         join g in _dbContext.GroupsWithoutTrash on gm.GroupId equals g.GroupId into gGroup
                         from g in gGroup.DefaultIfEmpty()

                         where fh.SPOId == spoId && !fh.Trashed
                               && fh.HajjSeasonsId == activeSeasonId
                               && fh.HajjiBookingStatus == status
                         select new
                         {
                             FamilyHead = fh,
                             Package = p,
                             Company = c,
                             FamilyHeadEmail = fhe,
                             FamilyHeadContact = fhc,
                             Group = g
                         }).ToList();


            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FamilyHead.FullName,
                HBNo = e.FamilyHead.FcId,
                PackageName = e.Package.Name,
                CompanyCode = e.Company.CompanyCode,
                Gender = e.FamilyHead.Gender.ToString(),
                PassportNo = e.FamilyHead.PassportNumber,
                Number = e.FamilyHeadContact != null ? e.FamilyHeadContact.Contact : "N/A",
                Email = e.FamilyHeadEmail != null ? e.FamilyHeadEmail.Email : "N/A",
                GroupName = e.Group != null ? e.Group.Name : "N/A",
                CNIC = !string.IsNullOrEmpty(e.FamilyHead.CnicUpload) ? "✔️" : "❌",
                Passport = !string.IsNullOrEmpty(e.FamilyHead.PassportImage) ? "✔️" : "❌",
                Member = !string.IsNullOrEmpty(e.FamilyHead.MemberImage) ? "✔️" : "❌",
                BloodGroup = !string.IsNullOrEmpty(e.FamilyHead.BloodProof) ? "✔️" : "❌",
                Vaccination = !string.IsNullOrEmpty(e.FamilyHead.MemberVaccinationImage) ? "✔️" : "❌",
                Polio = !string.IsNullOrEmpty(e.FamilyHead.PolioUpload) ? "✔️" : "❌",
                Covid = !string.IsNullOrEmpty(e.FamilyHead.CovidUpload) ? "✔️" : "❌",
                Mangatitise = !string.IsNullOrEmpty(e.FamilyHead.MangatitiseUpload) ? "✔️" : "❌",
                Influenza = !string.IsNullOrEmpty(e.FamilyHead.InfluenzaUpload) ? "✔️" : "❌",
                NextOfKin = _dbContext.FamilyHeadNominees
                            .Any(n => n.FamilyHeadId == e.FamilyHead.FamilyHeadId) ? "✔️" : "❌",
                DOB = e.FamilyHead.DateOfBirth,
            }).ToList();

            return View(result);
        }

        public IActionResult MemberReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.CompanyVisas)
                .Where(e =>
                    (e.HajjiBookingStatus == HajjiBookingStatus.Booked || e.HajjiBookingStatus == HajjiBookingStatus.Tentative)
                    && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                );

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                Gender = e.Gender.ToString(),
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber,
                DOB = e.DateOfBirth,
                CompanyNames = e.CompanyVisas.CompanyName != null
                ? e.CompanyVisas.CompanyName : "N/A",
                Visa = e.VisaImage != null ? "✔️" : "❌",
                ApplicantType = e.ApplicantType.ToString(),
                HGO = e.HajjApplicationNumber,
                VisaNo = e.VisaNumber,
                MisUpload = e.MISPdfPath != null ? "✔️" : "❌",
                MoufaaNo = e.MofaNumber,
                CNIC = e.CNIC,
                MoraNo = e.CompanyVisas.MoraNo != null
                ? e.CompanyVisas.MoraNo : "N/A",
                MunazzamName = e.CompanyVisas.MunazimName != null
                ? e.CompanyVisas.MunazimName : "N/A",
                MunazzamNo = e.CompanyVisas.SaudiMunazimNo != null
                ? e.CompanyVisas.SaudiMunazimNo : "N/A",

                Address = _dbContext.FamilyHeadAddresses
                    .Where(a => a.FamilyHeadId == e.FamilyHeadId
                            && a.Preferred)
                    .Select(a => a.Address)
                    .FirstOrDefault(),

                Number = _dbContext.FamilyHeadContacts
                    .Where(c => c.FamilyHeadId == e.FamilyHeadId
                            && c.Preferred)
                    .Select(c => c.Contact)
                    .FirstOrDefault(),

                Email = _dbContext.FamilyHeadEmails
                    .Where(c => c.FamilyHeadId == e.FamilyHeadId
                            && c.Preferred)
                    .Select(c => c.Email)
                    .FirstOrDefault(),

                //DepartureTicketNo = _dbContext.POFlightDetails
                //    .Include(p => p.POTicket)
                //    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                //                && !pt.POTicket.Trashed
                //                && pt.AirportsType == AirportsType.Departure
                //                && pt.POTicket.IssuedType != IssuedType.Refund)
                //    .OrderBy(pt =>
                //        pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                //        pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                //        pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3
                //    )
                //    .Select(pt => pt.TicketNumber)
                //    .LastOrDefault(),

                //ArrivalTicketNo = _dbContext.POFlightDetails
                //    .Include(p => p.POTicket)
                //    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                //                && !pt.POTicket.Trashed
                //                && pt.AirportsType == AirportsType.Arrival
                //                && pt.POTicket.IssuedType != IssuedType.Refund)
                //    .OrderBy(pt =>
                //        pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                //        pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                //        pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3
                //    )
                //    .Select(pt => pt.TicketNumber)
                //    .LastOrDefault(),

                DepartureTicketNo = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Departure)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = pt.TicketNumber
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                ArrivalTicketNo = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Arrival)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = pt.TicketNumber
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                TicketImageOneway = (
                    from pt in _dbContext.POTickets
                    join fd in _dbContext.POFlightDetails on pt.POTicketId equals fd.POTicketId
                    where pt.FamilyMembersId == e.FamilyHeadId
                            && !pt.Trashed
                            && pt.FareType == FareType.OneWay
                            && pt.IssuedType != IssuedType.Refund
                    orderby (pt.IssuedType == IssuedType.Issued ? 0 :
                             pt.IssuedType == IssuedType.ReIssued ? 1 :
                             pt.IssuedType == IssuedType.Self ? 2 : 3),
                            fd.POFlightDetailId
                    group fd by pt into g
                    orderby (g.Key.IssuedType == IssuedType.Issued ? 0 :
                             g.Key.IssuedType == IssuedType.ReIssued ? 1 :
                             g.Key.IssuedType == IssuedType.Self ? 2 : 3)
                    select g.OrderBy(fd => fd.POFlightDetailId)
                            .Select(fd => fd.TicketImages != null ? "✔️" : "❌")
                            .FirstOrDefault()
                ).LastOrDefault() ?? "❌",

                TicketImageReturn = (
                    from pt in _dbContext.POTickets
                    join fd in _dbContext.POFlightDetails on pt.POTicketId equals fd.POTicketId
                    where pt.FamilyMembersId == e.FamilyHeadId
                            && !pt.Trashed
                            && pt.FareType == FareType.RoundTrip
                            && pt.IssuedType != IssuedType.Refund
                    orderby (pt.IssuedType == IssuedType.Issued ? 0 :
                             pt.IssuedType == IssuedType.ReIssued ? 1 :
                             pt.IssuedType == IssuedType.Self ? 2 : 3),
                            fd.POFlightDetailId
                    group fd by pt into g
                    orderby (g.Key.IssuedType == IssuedType.Issued ? 0 :
                             g.Key.IssuedType == IssuedType.ReIssued ? 1 :
                             g.Key.IssuedType == IssuedType.Self ? 2 : 3)
                    select g.OrderBy(fd => fd.POFlightDetailId)
                            .Select(fd => fd.TicketImages != null ? "✔️" : "❌")
                            .FirstOrDefault()
                ).LastOrDefault() ?? "❌",

                DepartureAirportName = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Departure)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Departure)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = pt.Departure.Code
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                DepartureFlightNumber = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Departure)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Departure)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = pt.POTicketFlightData.FlightNumber
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                DepartureTime = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Departure)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Departure)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = DateTimeManager.TimeToHtmlFormat(pt.FlightDepartureTime)
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),

                DepartureDate = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Departure)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Departure)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = DateTimeManager.DateToHtmlFormat(pt.DepartureDate)
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                PNRTicketDeparture = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Arrival)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Departure)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = pt.PNRNo
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                ArrivalAirportName = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Arrival)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Arrival)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = pt.Arrival.Code
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),

                ArrivalFlightNumber = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.POTicketFlightData)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Arrival)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = pt.POTicketFlightData.FlightNumber
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                ArrivalTime = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Arrival)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Arrival)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = DateTimeManager.TimeToHtmlFormat(pt.FlightArrivalTime)
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                ArrivalDate = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Arrival)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Arrival)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => new {
                        pt.POTicket.IssuedType,
                        Value = DateTimeManager.DateToHtmlFormat(pt.ArrivalDate)
                    })
                    .AsEnumerable()
                    .Select(x => x.IssuedType == IssuedType.Refund ? null : x.Value)
                    .FirstOrDefault(),


                PNRTicketArrival = _dbContext.POFlightDetails
                    .Include(p => p.POTicket)
                    .Include(p => p.Arrival)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                                && !pt.POTicket.Trashed
                                && pt.AirportsType == AirportsType.Arrival
                                && pt.POTicket.IssuedType != IssuedType.Refund)
                    .OrderByDescending(pt => pt.POTicket.CreatedAt)            // naya sabse upar
                    .ThenBy(pt => pt.POTicket.IssuedType == IssuedType.Issued ? 0 :
                                  pt.POTicket.IssuedType == IssuedType.ReIssued ? 1 :
                                  pt.POTicket.IssuedType == IssuedType.Self ? 2 : 3)
                    .Select(pt => pt.PNRNo)
                    .FirstOrDefault(),

                Makkah = _dbContext.ItineraryBlockReservationForMakkah
                    .Any(m => _dbContext.FamilyHeadsWithoutTrash
                        .Where(fm => fm.FamilyHeadId == e.FamilyHeadId)
                        .Select(fm => fm.FamilyHeadId)
                        .Contains(m.FamilyHeadId)) ? "✔️" : "❌",

                RouteOne = _dbContext.TranspportationBlockReservations
                    .Any(m => _dbContext.FamilyHeadsWithoutTrash
                        .Where(fm => fm.FamilyHeadId == e.FamilyHeadId)
                        .Select(fm => fm.FamilyHeadId)
                        .Contains(m.FamilyHeadId)) ? "✔️" : "❌",

                MakkahHotel = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 1)
                        .Select(pa => pa.Hotel.HotelName)
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 1)
                        .Select(i => i.Hotel.HotelName)
                        .FirstOrDefault(),

                MakkahCheckIn = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 1)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckInTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 1)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckIn))
                        .FirstOrDefault(),

                MakkahCheckOut = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 1)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckOutTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 1)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckOut))
                        .FirstOrDefault(),

                Madinah = _dbContext.ItineraryBlockReservationForMadinah
                    .Any(md => _dbContext.FamilyHeadsWithoutTrash
                        .Where(fm => fm.FamilyHeadId == e.FamilyHeadId)
                        .Select(fm => fm.FamilyHeadId)
                        .Contains(md.FamilyHeadId)) ? "✔️" : "❌",

                MedinahHotel = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 2)
                        .Select(pa => pa.Hotel.HotelName)
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 2)
                        .Select(i => i.Hotel.HotelName)
                        .FirstOrDefault(),

                MedinahCheckIn = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 2)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckInTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 2)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckIn))
                        .FirstOrDefault(),

                MedinahCheckOut = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 2)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckOutTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 2)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckOut))
                        .FirstOrDefault(),

                Azizias = _dbContext.ItineraryBlockReservationForAzizias
                    .Any(a => _dbContext.FamilyHeadsWithoutTrash
                        .Where(fm => fm.FamilyHeadId == e.FamilyHeadId)
                        .Select(fm => fm.FamilyHeadId)
                        .Contains(a.FamilyHeadId)) ? "✔️" : "❌",

                AziziaHotel = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 4)
                        .Select(pa => pa.Hotel.HotelName)
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 4)
                        .Select(i => i.Hotel.HotelName)
                        .FirstOrDefault(),

                AziziaCheckIn = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 4)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckInTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 4)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckIn))
                        .FirstOrDefault(),

                AziziaCheckOut = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 4)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckOutTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 4)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckOut))
                        .FirstOrDefault(),

                Mina = _dbContext.MinaMembers
                    .Include(e => e.Mina)
                    .Where(m => m.FamilyHeadId == e.FamilyHeadId
                            && !m.Mina.Trashed)
                    .Select(m => m.FamilyHeadId)
                    .Contains(e.FamilyHeadId) ? "✔️" : "❌",

                MinaHotel = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 3)
                        .Select(pa => pa.Hotel.HotelName)
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 3)
                        .Select(i => i.Hotel.HotelName)
                        .FirstOrDefault(),

                MinaCheckIn = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 3)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckInTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 3)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckIn))
                        .FirstOrDefault(),

                MinaCheckOut = e.AccomodationAsPerPackage
                    ? _dbContext.PackageAccomodtions
                        .Where(pa => pa.PackageId == e.PackageId && pa.PlaceId == 3)
                        .Select(pa => DateTimeManager.DateToHtmlFormat(pa.ActualCheckOutTime))
                        .FirstOrDefault()
                    : _dbContext.FamilyHeadAccomodations
                        .Where(i => i.FamilyHeadId == e.FamilyHeadId && i.Hotel.PlaceId == 3)
                        .Select(i => DateTimeManager.DateToHtmlFormat(i.CheckOut))
                        .FirstOrDefault(),

                GroupName = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .Where(g => g.FamilyHeadId == e.FamilyHeadId
                            && !g.Group.Trashed)
                    .Select(e => e.Group.Name)
                    .FirstOrDefault(),

                GiveAway = e.DeliveryDate != null ? "✔️" : "❌",

                PNR = _dbContext.PNRMembers
                    .Include(e => e.PNR)
                    .Where(e => !e.PNR.Trashed)
                        .Where(p => p.FamilyHeadId == e.FamilyHeadId)
                    .Select(fm => fm.FamilyHeadId)
                    .Contains(e.FamilyHeadId) ? "✔️" : "❌",

                Remarks = _dbContext.FamilyHeadRemarks
                .Where(j => j.FamilyHeadId == e.FamilyHeadId)
                .Select(e => e.Remarks)
                .FirstOrDefault() ?? "N/A",

                PNRTrain = _dbContext.PNRTrainMembers
                    .Include(e => e.PNRTrain)
                    .Where(e => !e.PNRTrain.Trashed)
                        .Where(p => p.FamilyHeadId == e.FamilyHeadId)
                    .Select(fm => fm.FamilyHeadId)
                    .Contains(e.FamilyHeadId) ? "✔️" : "❌",

                DD = e.DDDate != null ? "✔️" : "❌"

            }).ToList();

            return View(result);

        }

        public IActionResult SPOPaymentReport(int spoId)
        {
            ViewBag.SPOName = _dbContext.SPOsWithoutTrash
                .Where(e => e.SPOId == spoId)
                .Select(e => e.Name)
                .FirstOrDefault();

            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var query = (from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join fhe in _dbContext.FamilyHeadEmails
                          .Where(e => e.Preferred == true)
                          on fh.FamilyHeadId equals fhe.FamilyHeadId into fheGroup
                         from fhe in fheGroup.DefaultIfEmpty()
                         join fhc in _dbContext.FamilyHeadContacts
                             .Where(c => c.Preferred == true)
                             on fh.FamilyHeadId equals fhc.FamilyHeadId into fhcGroup
                         from fhc in fhcGroup.DefaultIfEmpty()
                         join gm in _dbContext.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                         from gm in gmGroup.DefaultIfEmpty()
                         join g in _dbContext.Groups on gm.GroupId equals g.GroupId into gGroup
                         from g in gGroup.DefaultIfEmpty()
                         where fh.SPOId == spoId && !fh.Trashed
                               && fh.HajjSeasonsId == activeSeasonId && fh.IsHead && fhe.Preferred && fhc.Preferred
                         select new
                         {
                             FamilyHead = fh,
                             Package = p,
                             FamilyHeadEmail = fhe,
                             FamilyHeadContact = fhc,
                             Group = g
                         }).ToList();

            var familyHeadIds = query.Select(e => e.FamilyHead.FamilyHeadId).ToList();

            var receipts = _dbContext.NewInvoicesWithoutTrash
              .Include(e => e.CompanyBankAccount)
              .Where(e => e.ReceiptStatus == ReceiptStatus.Cleared && familyHeadIds.Contains(e.FamilyHeadId))
              .ToList();

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FamilyHead.FullName,
                HBNo = e.FamilyHead.FcId,
                PackageName = e.Package.Name,
                Gender = e.FamilyHead.Gender.ToString(),
                PassportNo = e.FamilyHead.PassportNumber,
                Number = e.FamilyHeadContact != null ? e.FamilyHeadContact.Contact : "N/A",
                Email = e.FamilyHeadEmail != null ? e.FamilyHeadEmail.Email : "N/A",
                GroupName = e.Group != null ? e.Group.Name : "N/A",
                CNIC = !string.IsNullOrEmpty(e.FamilyHead.CnicUpload) ? "✔️" : "❌",
                Passport = !string.IsNullOrEmpty(e.FamilyHead.PassportImage) ? "✔️" : "❌",
                Member = !string.IsNullOrEmpty(e.FamilyHead.MemberImage) ? "✔️" : "❌",
                BloodGroup = !string.IsNullOrEmpty(e.FamilyHead.BloodProof) ? "✔️" : "❌",
                Vaccination = !string.IsNullOrEmpty(e.FamilyHead.MemberVaccinationImage) ? "✔️" : "❌",
                Polio = !string.IsNullOrEmpty(e.FamilyHead.PolioUpload) ? "✔️" : "❌",
                Covid = !string.IsNullOrEmpty(e.FamilyHead.CovidUpload) ? "✔️" : "❌",
                Mangatitise = !string.IsNullOrEmpty(e.FamilyHead.MangatitiseUpload) ? "✔️" : "❌",
                Influenza = !string.IsNullOrEmpty(e.FamilyHead.InfluenzaUpload) ? "✔️" : "❌",
                NextOfKin = _dbContext.FamilyHeadNominees
                            .Any(n => n.FamilyHeadId == e.FamilyHead.FamilyHeadId) ? "✔️" : "❌",
                TotalPackage = e.FamilyHead.IsHead ? $"{e.FamilyHead.Currencies} {PackagePriceManager.GetOverallTotalPriceForAll(_dbContext, new List<int> { e.FamilyHead.FamilyHeadId }).ToString("N2")}" : "N/A",
                ReceivedAmount = $"{e.FamilyHead.Currencies} {receipts
                    .Where(r => r.FamilyHeadId == e.FamilyHead.FamilyHeadId)
                    .Select(r => AmountConverter.GetConvertedAmount(r.AmountRecieved, e.FamilyHead.Currencies ?? Currencies.SAR, r.Currencies, r.ROE))
                    .Sum().ToString("N2")}",
                Balance = e.FamilyHead.IsHead ?
                    $"{e.FamilyHead.Currencies} {(PackagePriceManager.GetOverallTotalPriceForAll(_dbContext, new List<int> { e.FamilyHead.FamilyHeadId }) - receipts
                        .Where(r => r.FamilyHeadId == e.FamilyHead.FamilyHeadId)
                        .Select(r => AmountConverter.GetConvertedAmount(r.AmountRecieved, e.FamilyHead.Currencies ?? Currencies.SAR, r.Currencies, r.ROE))
                        .Sum()).ToString("N2")}" : "N/A"
            }).ToList();

            return View(result);
        }

        public IActionResult HGOSummaryReport(int companyid)
        {
            ViewBag.CompanyName = _dbContext.CompaniesWithoutTrash
                .Where(e => e.CompanyId == companyid)
                .Select(e => e.CompanyName)
                .FirstOrDefault();

            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var query = _dbContext.FamilyHeadsWithoutTrash
                 .Include(e => e.Package)
                     .ThenInclude(e => e.Company)
                 .GroupJoin(_dbContext.Groups,
                            fh => fh.PackageId,
                            g => g.PackageId,
                            (fh, groups) => new { FamilyHead = fh, Groups = groups.DefaultIfEmpty() })
                 .SelectMany(e => e.Groups, (e, g) => new { FamilyHead = e.FamilyHead, Group = g })
                 .Where(e => e.FamilyHead.Package.CompanyId == companyid
                             && e.FamilyHead.HajjApplicationNumber != null && e.FamilyHead.HajjSeasonsId == activeSeasonId);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FamilyHead.FullName,
                HBNo = e.FamilyHead.FcId,
                PackageName = e.FamilyHead.Package.Name,
                Gender = e.FamilyHead.Gender.ToString(),
                PassportNo = e.FamilyHead.PassportNumber,
                VisaNo = e.FamilyHead.VisaNumber,
                MoufaaNo = e.FamilyHead.MofaNumber,
                GroupName = e.Group != null ? e.Group.Name : "N/A",
                Visa = !string.IsNullOrEmpty(e.FamilyHead.VisaImage) ? "✔️" : "❌",
                VisaP = e.FamilyHead.VisaImage,
            }).ToList();

            return View(result);
        }

        public IActionResult PackageSelection()
        {
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(e => new { e.PackageNumber, e.Code })
                .Distinct()
                .ToList();

            return View();
        }

        public IActionResult POTicketWiseReport(int[] id)
        {
            var result = _dbContext.PackagesWithoutTrash
                .Where(e => id.Contains(e.PackageNumber ?? 0))
                .Select(e => new PackageSummary
                {
                    PackageName = e.Name,
                    PackageId = e.PackageId,
                    HajjiCount = _dbContext.FamilyHeadsWithoutTrash
                        .Count(fh => fh.PackageId == e.PackageId
                                && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled),

                    TentativeCount = _dbContext.FamilyHeadsWithoutTrash
                        .Count(fh => fh.PackageId == e.PackageId
                                     && fh.HajjiBookingStatus == HajjiBookingStatus.Tentative),

                    BookedCount = _dbContext.FamilyHeadsWithoutTrash
                        .Count(fh => fh.PackageId == e.PackageId
                                     && fh.HajjiBookingStatus == HajjiBookingStatus.Booked),

                    TMGIssued = _dbContext.FamilyHeadsWithoutTrash
                    .Count(fh => fh.PackageId == e.PackageId && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled
                            && fh.TicketStatus == TktStatus.Issued),

                    ToIssued = _dbContext.FamilyHeadsWithoutTrash
                    .Count(fh => fh.PackageId == e.PackageId && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled
                            && fh.TicketStatus == TktStatus.NotIssued),

                    BookedF = _dbContext.FamilyHeadsWithoutTrash
                        .Where(fh => fh.PackageId == e.PackageId &&
                                     fh.FamilyHeadFlights.Any(fhf => fhf.FlightClass == FlightClass.FirstClass))
                        .Select(fh => fh.FamilyHeadId)
                        .Distinct()
                        .Count(),

                    BookedE = _dbContext.FamilyHeadsWithoutTrash
                        .Where(fh => fh.PackageId == e.PackageId &&
                                     fh.FamilyHeadFlights.Any(fhf => fhf.FlightClass == FlightClass.Economy))
                        .Select(fh => fh.FamilyHeadId)
                        .Distinct()
                        .Count(),

                    BookedB = _dbContext.FamilyHeadsWithoutTrash
                        .Where(fh => fh.PackageId == e.PackageId &&
                                     fh.FamilyHeadFlights.Any(fhf => fhf.FlightClass == FlightClass.Business))
                        .Select(fh => fh.FamilyHeadId)
                        .Distinct()
                        .Count(),

                    Self = _dbContext.FamilyHeadsWithoutTrash
                        .Count(fh => fh.PackageId == e.PackageId
                            && fh.TicketStatus == TktStatus.Self && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled),

                    Tent = _dbContext.FamilyHeadsWithoutTrash
                        .Count(fh => fh.PackageId == e.PackageId && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled
                            && fh.TicketStatus == TktStatus.Tentative)
                })
                .ToList();

            return View(result);
        }

        public IActionResult Summary(int packageId, string type = null)
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.PackageId == packageId && e.HajjiBookingStatus != HajjiBookingStatus.Cancelled);

            if (type == "Booked")
            {
                query = query.Where(fh => fh.HajjiBookingStatus == HajjiBookingStatus.Booked);
            }
            else if (type == "Tentative")
            {
                query = query.Where(fh => fh.HajjiBookingStatus == HajjiBookingStatus.Tentative);
            }
            else if (type == "Self")
            {
                //query = query.Where(fh => fh.FamilyHeadFlights.Any(fhf => fhf.FlightClass == FlightClass.Self));
                query = query.Where(fh => fh.TicketStatus == TktStatus.Self && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
            }
            else if (type == "TMGIssued")
            {
                //query = query.Where(fh => _dbContext.POTickets.Any(pt => pt.FamilyMembersId == fh.FamilyHeadId));
                query = query.Where(fh => fh.TicketStatus == TktStatus.Issued && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
            }
            else if (type == "ToIssued")
            {
                query = query.Where(fh => fh.TicketStatus == TktStatus.NotIssued && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
            }
            else if (type == "Tent")
            {
                query = query.Where(fh => fh.TicketStatus == TktStatus.Tentative && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
            }

            var result = query.ToList();
            return View(result);
        }

        public IActionResult ListSummaries(string packageIds, string type)
        {
            if (string.IsNullOrWhiteSpace(packageIds))
            {
                // Return empty view or redirect with message
                return View(new List<FamilyHead>()); // or your relevant model
            }

            var packageIdList = packageIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var pid) ? pid : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (!packageIdList.Any())
            {
                return View(new List<FamilyHead>());
            }

            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => packageIdList.Contains(e.PackageId) && e.HajjiBookingStatus != HajjiBookingStatus.Cancelled);

            // Apply filters based on 'type' parameter
            switch (type?.Trim())
            {
                case "Booked":
                    query = query.Where(fh => fh.HajjiBookingStatus == HajjiBookingStatus.Booked);
                    break;

                case "Tentative":
                    query = query.Where(fh => fh.HajjiBookingStatus == HajjiBookingStatus.Tentative);
                    break;

                case "Self":
                    query = query.Where(fh => fh.TicketStatus == TktStatus.Self && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
                    break;

                case "TMGIssued":
                    query = query.Where(fh => fh.TicketStatus == TktStatus.Issued && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
                    break;

                case "ToIssued":
                    query = query.Where(fh => fh.TicketStatus == TktStatus.NotIssued && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
                    break;

                case "Tent":
                    query = query.Where(fh => fh.TicketStatus == TktStatus.Tentative && fh.HajjiBookingStatus != HajjiBookingStatus.Cancelled);
                    break;

                // Optional: default case if needed
                default:
                    break;
            }

            var result = query.ToList();
            return View(result);
        }

        public IActionResult OtherServiceReport()
        {
            var extraAdf = _dbContext.ExtraFacilityDescriptionsWithoutTrash;

            List<OtherServices> model = new();

            foreach (var item in extraAdf)
            {
                OtherServices view = new()
                {
                    EAdfId = item.ExtraFacilityDescriptionId,
                    Name = item.Description,
                    Count = _dbContext.FamilyHeadsWithoutTrash
                         .Join(_dbContext.FamilyHeadExtras,
                               fh => fh.FamilyHeadId,
                               fhe => fhe.FamilyHeadId,
                               (fh, fhe) => new { fh, fhe })
                         .Where(joined => joined.fhe.ExtraFacilityDescriptionId == item.ExtraFacilityDescriptionId
                                && joined.fh.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                                && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked)
                         .Count()
                };

                model.Add(view);
            }

            return View(model);
        }

        public IActionResult ServiceReport(int id)
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
             .Join(_dbContext.FamilyHeadExtras,
                   fh => fh.FamilyHeadId,
                   fhe => fhe.FamilyHeadId,
                    (fh, fhe) => new { fh, fhe })
              .Where(joined => joined.fhe.ExtraFacilityDescriptionId == id
                        && joined.fh.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new OtherServices
            {
                Name = e.fh.FullName,
                HbNo = e.fh.FcId,
                Remarks = _dbContext.FamilyHeadRemarks
                .Where(j => j.FamilyHeadId == e.fh.FamilyHeadId)
                .Select(e => e.Remarks)
                .FirstOrDefault() ?? "No Remarks"
            }).ToList();

            return View(result);
        }

        public IActionResult ApplicantReport(ApplicantType applicantType)
        {
            ViewBag.Family = _dbContext.FamilyHeadsWithoutTrash
             .Count(e => e.ApplicantType == applicantType
                     && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                     && e.HajjiBookingStatus == HajjiBookingStatus.Booked && e.IsHead);

            ViewBag.MaleCount = _dbContext.FamilyHeadsWithoutTrash
                .Count(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked && e.Gender == Gender.Male);

            ViewBag.FemaleCount = _dbContext.FamilyHeadsWithoutTrash
                .Count(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked && e.Gender == Gender.Female);

            ViewBag.PAX = _dbContext.FamilyHeadsWithoutTrash
                .Count(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Countries)
                .Where(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PassportNo = e.PassportNumber,
                Gender = e.Gender.ToString(),
                CountryName = e.Countries.Name,
                CityName = e.FamilyHeadAddresses.FirstOrDefault(e => e.Preferred).Cities.Name,
                Address = e.FamilyHeadAddresses.FirstOrDefault(e => e.Preferred).Address,
                Number = e.FamilyHeadContacts.FirstOrDefault(e => e.Preferred).Contact
            }).ToList();

            return View(result);
        }

        public IActionResult NewReport(Gender gender)
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.Gender == gender && e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                Gender = e.Gender.ToString(),
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult CancelReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjiBookingStatus == HajjiBookingStatus.Cancelled
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                Gender = e.Gender.ToString(),
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult FamilyReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.IsHead);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult TentativeReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.HajjiBookingStatus == HajjiBookingStatus.Tentative);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult AdultReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .AsEnumerable()
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.IsAdult && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult ChildReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .AsEnumerable()
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.IsChild && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult InfantReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .AsEnumerable()
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.IsInfant && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult QueriesReport(ApplicantType applicantType)
        {
            var query = _dbContext.CustomerPortfolios
                .Where(e => e.ApplicantType == applicantType && !e.Trashed
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                PassportNo = e.PassportNumber,
                Gender = e.Gender.ToString(),
            }).ToList();

            return View(result);
        }

        public IActionResult ClientDataReport()
        {
            var query = _dbContext.ClientDatas
                .Where(e => !e.Trashed
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                Gender = e.Gender.ToString(),
                PassportNo = e.PassportNumber
            }).ToList();

            return View(result);
        }

        public IActionResult PackageCodeSelection()
        {
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(e => new { e.PackageNumber, e.Code })
                .Distinct()
                .ToList();

            return View();
        }

        public IActionResult PackageRoaming(int[] id)
        {
            var allPlaces = _dbContext.PlacesWithoutTrash
                .Select(p => p.PlaceName)
                .ToList();

            var rawData = (from fh in _dbContext.FamilyHeadsWithoutTrash
                           join p in _dbContext.PackagesWithoutTrash on fh.PackageId equals p.PackageId
                           join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                           where id.Contains(p.PackageNumber ?? 0)
                           select new
                           {
                               fh.FullName,
                               fh.FcId,
                               fh.Gender,
                               PackageName = p.Name,
                               RoomType = p.Room.RoomName,
                               p.MaktabNumber,
                               Place = pa.Hotel.Place.PlaceName,
                               HotelName = pa.Hotel.HotelName,
                               CheckIn = pa.ActualCheckInTime,
                               CheckOut = pa.ActualCheckOutTime
                           }).ToList();

            var groupedData = rawData
                .GroupBy(x => new { x.FullName, x.FcId, x.PackageName, x.Gender, x.RoomType, x.MaktabNumber })
                .Select(g =>
                {
                    var model = new HajjSummaryData
                    {
                        HajjiName = g.Key.FullName,
                        HBNo = g.Key.FcId,
                        PackageName = g.Key.PackageName,
                        Gender = g.Key.Gender.ToString(),
                        RoomType = g.Key.RoomType,
                        MaktabNumber = g.Key.MaktabNumber,
                        CheckIn = "-",
                        CheckOut = "-",
                        PlaceHotelMap = new Dictionary<string, PlaceHotelInfo>()
                    };

                    foreach (var place in allPlaces)
                    {
                        var hotel = g.FirstOrDefault(x => x.Place == place);
                        if (hotel != null)
                        {
                            model.PlaceHotelMap[place] = new PlaceHotelInfo
                            {
                                HotelName = hotel.HotelName,
                                CheckIn = hotel.CheckIn?.ToString("dd/MMM/yyyy"),
                                CheckOut = hotel.CheckOut?.ToString("dd/MMM/yyyy")
                            };
                        }
                        else
                        {
                            model.PlaceHotelMap[place] = new PlaceHotelInfo
                            {
                                HotelName = "-",
                                CheckIn = "-",
                                CheckOut = "-"
                            };
                        }
                    }

                    return model;
                })
                .ToList();

            ViewBag.Places = allPlaces;
            return View(groupedData);
        }

        public IActionResult HotelSelection()
        {
            ViewBag.Hotel = _dbContext.HotelsWithoutTrash
                .ToList();

            return View();
        }

        public IActionResult HotelReport(int id)
        {
            ViewBag.Id = id;

            ViewBag.HotelName = _dbContext.HotelsWithoutTrash
                .Where(e => e.HotelId == id)
                .Select(e => e.HotelName)
                .FirstOrDefault();

            var hajjSeasonId = Convert.ToInt32(HttpContext.Session.GetString("ActiveSeasonId"));

            var roomCodes = _dbContext.IndividualFacilityDescriptionsWithoutTrash
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

            var hotels = _dbContext.HotelsWithoutTrash
                .Where(e => e.HotelId == id)
                .ToList();

            var checkInOutDatesFromPackages = _dbContext.PackageAccomodtions
                .Where(pa => pa.HotelId == id && pa.ActualCheckInTime.HasValue && pa.ActualCheckOutTime.HasValue)
                .Select(pa => new
                {
                    CheckIn = pa.ActualCheckInTime.Value.Date,
                    CheckOut = pa.ActualCheckOutTime.Value.Date
                });

            var checkInOutDatesFromFamily = _dbContext.FamilyHeadAccomodations
                .Where(fha => fha.HotelId == id && fha.CheckIn.HasValue && fha.CheckOut.HasValue)
                .Select(fha => new
                {
                    CheckIn = fha.CheckIn.Value.Date,
                    CheckOut = fha.CheckOut.Value.Date
                });

            var checkInOutDates = checkInOutDatesFromPackages
                .Union(checkInOutDatesFromFamily)
                .Distinct()
                .ToList();

            foreach (var room in roomCodes)
            {
                foreach (var date in checkInOutDates)
                {
                    AziziaRoomCountModel view = new()
                    {
                        RoomId = room.RoomId,
                        RoomCode = room.RoomCode,
                        RoomCount = 0,
                        CheckIn = date.CheckIn,
                        CheckOut = date.CheckOut
                    };

                    float packageRoomCount = _dbContext.FamilyHeads
                        .Join(_dbContext.Packages, fh => fh.PackageId, p => p.PackageId, (fh, p) => new { fh, p })
                        .Join(_dbContext.PackageAccomodtions, combined => combined.p.PackageId, pa => pa.PackageId, (combined, pa) => new { combined.fh, combined.p, pa })
                        .Where(joined => joined.p.RoomId == room.RoomId
                                && !joined.fh.Trashed
                                && joined.pa.HotelId == id
                                && joined.fh.AccomodationAsPerPackage == true && joined.fh.HajjSeasonsId == hajjSeasonId
                                && joined.pa.ActualCheckInTime.Value.Date == date.CheckIn
                                && joined.pa.ActualCheckOutTime.Value.Date == date.CheckOut
                                && joined.fh.HajjiBookingStatus == HajjiBookingStatus.Booked)
                        .Count() / (float)room.RoomPax;

                    float individualRoomCount = _dbContext.FamilyHeads
                        .Join(_dbContext.FamilyHeadAccomodations,
                              fh => fh.FamilyHeadId,
                              fha => fha.FamilyHeadId,
                              (fh, fha) => new { fh, fha })
                        .Join(_dbContext.IndividualFacilityDescriptions,
                              combined => combined.fha.IndividualFacilityDescriptionId,
                              iadf => iadf.IndividualFacilityDescriptionId,
                              (combined, iadf) => new { combined.fh, combined.fha, iadf })
                        .Join(_dbContext.Packages,
                              combined => combined.fh.PackageId,
                              p => p.PackageId,
                              (combined, p) => new { combined.fh, combined.fha, combined.iadf, p })
                        .Where(x => x.iadf.RoomId == room.RoomId
                                    && x.fha.HotelId == id
                                    && x.fha.CheckIn.Value.Date == date.CheckIn
                                    && x.fha.CheckOut.Value.Date == date.CheckOut
                                    && x.fh.AccomodationAsPerPackage == false && x.fh.HajjSeasonsId == hajjSeasonId
                                    && x.fh.HajjiBookingStatus == HajjiBookingStatus.Booked)
                        .Count() / (float)room.RoomPax;

                    view.RoomCount += packageRoomCount;
                    view.RoomCount += individualRoomCount;

                    bool alreadyExists = model.Any(m =>
                        m.RoomId == view.RoomId &&
                        m.CheckIn.Value.Date == view.CheckIn.Value.Date &&
                        m.CheckOut.Value.Date == view.CheckOut.Value.Date
                    );

                    if (!alreadyExists && (view.RoomCount > 0 || view.MaleRoomCount > 0 || view.FemaleRoomCount > 0))
                    {
                        model.Add(view);
                    }
                }
            }

            return View(model);
        }

        public IActionResult HotelClick(DateTime checkIn, DateTime checkOut, int roomId, int hotelId)
        {
            List<HajjSummaryData> result = new List<HajjSummaryData>();

            int roomPax = 1;

            var hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var hasAsPerPackage = _dbContext.FamilyHeads
                .Any(fh => fh.AccomodationAsPerPackage == true
                        && !fh.Trashed && fh.HajjSeasonsId == hajjSeasonId
                        && fh.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var hasCustomAccommodation = _dbContext.FamilyHeads
                .Any(fh => fh.AccomodationAsPerPackage == false
                        && !fh.Trashed && fh.HajjSeasonsId == hajjSeasonId
                        && fh.HajjiBookingStatus == HajjiBookingStatus.Booked);

            if (hasAsPerPackage)
            {
                var asPerPackageResult = (from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join g in _dbContext.GroupsWithoutTrash on fh.PackageId equals g.PackageId into gj
                                          from g in gj.DefaultIfEmpty()
                                          where pa.HotelId == hotelId
                                                && ((pa.PlaceId == 4 && p.RoomTypesId == roomId) || (pa.PlaceId != 4 && p.RoomId == roomId))
                                                && fh.AccomodationAsPerPackage == true && fh.HajjSeasonsId == hajjSeasonId
                                                && pa.ActualCheckInTime.HasValue
                                                && pa.ActualCheckOutTime.HasValue
                                                && pa.ActualCheckInTime.Value.Date == checkIn.Date
                                                && pa.ActualCheckOutTime.Value.Date == checkOut.Date
                                                && !fh.Trashed && fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                          orderby fh.FcId ascending
                                          select new HajjSummaryData
                                          {
                                              HajjiName = fh.GivenName + " " + fh.SurName,
                                              HBNo = fh.FcId,
                                              PackageName = p.Name,
                                              RoomPax = (pa.PlaceId == 4 ? p.RoomTypes.PAX : p.Room.PAX),
                                              Gender = fh.Gender.ToString(),
                                              PassportNo = fh.PassportNumber,
                                          }).ToList();

                result.AddRange(asPerPackageResult);

                roomPax = asPerPackageResult.FirstOrDefault()?.RoomPax ?? roomPax;
            }

            if (hasCustomAccommodation)
            {
                var customResult = (from fh in _dbContext.FamilyHeads
                                    join fpa in _dbContext.FamilyHeadAccomodations on fh.FamilyHeadId equals fpa.FamilyHeadId
                                    join g in _dbContext.GroupsWithoutTrash on fh.PackageId equals g.PackageId into gj
                                    from g in gj.DefaultIfEmpty()
                                    join r in _dbContext.Rooms on fpa.IndividualFacilityDescription.RoomId equals r.RoomId into rj
                                    from r in rj.DefaultIfEmpty()
                                    where fpa.HotelId == hotelId && fpa.IndividualFacilityDescription.RoomId == roomId
                                          && fh.AccomodationAsPerPackage == false && fh.HajjSeasonsId == hajjSeasonId
                                          && fpa.CheckIn.HasValue
                                          && fpa.CheckOut.HasValue
                                          && fpa.CheckIn.Value.Date == checkIn.Date
                                          && fpa.CheckOut.Value.Date == checkOut.Date
                                          && !fh.Trashed && fh.HajjiBookingStatus == HajjiBookingStatus.Booked
                                    orderby fh.FcId ascending
                                    select new HajjSummaryData
                                    {
                                        HajjiName = fh.GivenName + " " + fh.SurName,
                                        HBNo = fh.FcId,
                                        PackageName = fh.Package.Name,
                                        RoomPax = r != null ? r.PAX : 1,
                                        Gender = fh.Gender.ToString(),
                                        PassportNo = fh.PassportNumber,
                                        GroupName = g != null ? g.Name : "N/A",
                                    }).ToList();

                result.AddRange(customResult);

                if (roomPax == 1)
                    roomPax = customResult.FirstOrDefault()?.RoomPax ?? roomPax;
            }

            return View(result);
        }

        public IActionResult FlightSelection()
        {
            ViewBag.Hotel = _dbContext.HotelsWithoutTrash
                .ToList();

            return View();
        }

        public IActionResult GiveAway()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.DeliveryDate != null);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                Gender = e.Gender.ToString(),
                PassportNo = e.PassportNumber,
                DeliveryDate = e.DeliveryDate.Value.Date
            }).ToList();

            return View(result);
        }

        public IActionResult VisaReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && e.VisaImage != null);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PassportNo = e.PassportNumber,
                PackageName = e.Package.Name,
                Gender = e.Gender.ToString()
            }).ToList();

            return View(result);
        }

        public IActionResult ClientOutStandingReport()
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            var query = (from fh in _dbContext.FamilyHeads
                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                         join s in _dbContext.SPOsWithoutTrash on fh.SPOId equals s.SPOId
                         join c in _dbContext.CompaniesWithoutTrash on p.CompanyId equals c.CompanyId
                         where !fh.Trashed
                               && fh.HajjSeasonsId == activeSeasonId && fh.IsHead
                         select new
                         {
                             FamilyHead = fh,
                             Package = p,
                             SPO = s,
                             Company = c
                         }).ToList();

            var familyHeadIds = query.Select(e => e.FamilyHead.FamilyHeadId).ToList();

            var receipts = _dbContext.NewInvoicesWithoutTrash
              .Include(e => e.CompanyBankAccount)
              .Where(e => e.ReceiptStatus == ReceiptStatus.Cleared && familyHeadIds.Contains(e.FamilyHeadId))
              .ToList();

            var result = query
                .Select((e, index) =>
                {
                    var totalPackage = e.FamilyHead.IsHead
                        ? PackagePriceManager.GetOverallTotalPriceForAll(_dbContext, new List<int> { e.FamilyHead.FamilyHeadId })
                        : 0;

                    var discount = e.FamilyHead.IsHead
                        ? PackagePriceManager.GetTotalDiscountForAll(_dbContext, new List<int> { e.FamilyHead.FamilyHeadId })
                        : 0;

                    var received = receipts
                        .Where(r => r.FamilyHeadId == e.FamilyHead.FamilyHeadId)
                        .Select(r => AmountConverter.GetConvertedAmount(
                            r.AmountRecieved,
                            e.FamilyHead.Currencies ?? Currencies.SAR,
                            r.Currencies,
                            r.ROE))
                        .Sum();

                    var balance = totalPackage - received;

                    return new HajjSummaryData
                    {
                        SerialNumber = index + 1,  
                        SPOName = e.SPO.Name,
                        CompanyCode = e.Company.CompanyCode,
                        HajjiName = e.FamilyHead.FullName,
                        HBNo = e.FamilyHead.FcId,
                        PackageName = e.Package.Name,
                        TotalPackage = $"{e.FamilyHead.Currencies} {(totalPackage + discount).ToString("N2")}",
                        Discount = $"{e.FamilyHead.Currencies} {discount.ToString("N2")}",
                        Gross = $"{e.FamilyHead.Currencies} {totalPackage.ToString("N2")}",
                        ReceivedAmount = $"{e.FamilyHead.Currencies} {received.ToString("N2")}",
                        Balance = $"{e.FamilyHead.Currencies} {balance.ToString("N2")}",

                        RawCurrency = e.FamilyHead.Currencies?.ToString() ?? "N/A",
                        RawTotalPackage = totalPackage + discount,
                        RawDiscount = discount,
                        RawGross = totalPackage,
                        RawReceivedAmount = received,
                        RawBalance = balance
                    };
                }).ToList();

            var totals = result
                .GroupBy(r => r.RawCurrency)
                .Select(g => new
                {
                    Currency = g.Key,
                    TotalPackage = g.Sum(x => x.RawTotalPackage),
                    Discount = g.Sum(x => x.RawDiscount),
                    Gross = g.Sum(x => x.RawGross),
                    ReceivedAmount = g.Sum(x => x.RawReceivedAmount),
                    Balance = g.Sum(x => x.RawBalance)
                }).ToList();

            ViewBag.CurrencyWiseTotals = totals;

            return View(result);
        }

        public record OtherServices
        {
            public int EAdfId { get; set; }
            public string Name { get; set; }
            public string HbNo { get; set; }
            public int Count { get; set; }
            public string Remarks { get; set; }
        }

        public class PlaceHotelInfo
        {
            public string HotelName { get; set; }
            public string CheckIn { get; set; }
            public string CheckOut { get; set; }
        }


        public record HajjSummaryData
        {
            public int SerialNumber { get; set; }
            public int? SPOId { get; set; }
            public string SPOName { get; set; }
            public string RoomType { get; set; }
            public string PackageName { get; set; }
            public string CompanyName { get; set; }
            public string HajjiName { get; set; }
            public string HBNo { get; set; }
            public string Gender { get; set; }
            public string PassportNo { get; set; }
            public string CompanyNames { get; set; }
            public string VisaNo { get; set; }
            public string MoufaaNo { get; set; }
            public string Number { get; set; }
            public string Email { get; set; }
            public string GroupName { get; set; }
            public string CNIC { get; set; }
            public string Passport { get; set; }
            public string Member { get; set; }
            public string BloodGroup { get; set; }
            public string Vaccination { get; set; }
            public string Polio { get; set; }
            public string Covid { get; set; }
            public string Mangatitise { get; set; }
            public string Influenza { get; set; }
            public string NextOfKin { get; set; }
            public string TotalPackage { get; set; }
            public string ReceivedAmount { get; set; }
            public string Balance { get; set; }
            public string RoomCode { get; set; }
            public string CompanyCode { get; set; }
            public float RoomCount { get; set; }
            public float MaleRoomCount { get; set; }
            public float FemaleRoomCount { get; set; }
            public int RoomPax { get; set; }
            public string Visa { get; set; }
            public string TicketImageOneway { get; set; }
            public string TicketImageReturn { get; set; }
            public string DepartureAirportName { get; set; }
            public string DepartureTime { get; set; }
            public string DepartureDate { get; set; }
            public string ArrivalAirportName { get; set; }
            public string ArrivalTime { get; set; }
            public string ArrivalDate { get; set; }
            public string PNRTicketDeparture { get; set; }
            public string PNRTicketArrival { get; set; }
            public float? GrossClient { get; set; }
            public float? GrossAirline { get; set; }

            public string Makkah { get; set; }
            public string RouteOne { get; set; }
            public string MakkahHotel { get; set; }
            public string Madinah { get; set; }
            public string MedinahHotel { get; set; }
            public string Azizias { get; set; }
            public string AziziaHotel { get; set; }
            public string VisaP { get; set; }
            public string CountryName { get; set; }
            public string CityName { get; set; }
            public string Address { get; set; }
            public DateTime DOB { get; set; }
            public string Age { get; set; }
            public string CheckIn { get; set; }
            public string CheckOut { get; set; }
            public string HotelName { get; set; }
            public string MaktabNumber { get; set; }
            public string GiveAway { get; set; }
            public DateTime DeliveryDate { get; set; }
            public string PNR { get; set; }
            public string PNRTrain { get; set; }
            public string Remarks { get; set; }
            public string ApplicantType { get; set; }
            public string MisUpload { get; set; }
            public string HGO { get; set; }
            public string MakkahCheckIn { get; set; }
            public string MedinahCheckIn { get; set; }
            public string AziziaCheckIn { get; set; }
            public string MakkahCheckOut { get; set; }
            public string MedinahCheckOut { get; set; }
            public string AziziaCheckOut { get; set; }
            public string Mina { get; set; }
            public string MinaHotel { get; set; }
            public string MinaCheckIn { get; set; }
            public string MinaCheckOut { get; set; }
            public string DD { get; set; }
            public string ArrivalFlightNumber { get; set; }
            public string DepartureFlightNumber { get; set; }
            public string MaktabNo { get; set; }
            public string FHName { get; set; }
            public string MoraNo { get; set; }
            public string MunazzamNo { get; set; }
            public string MunazzamName { get; set; }
            public string DepartureTicketNo { get; set; }
            public string ArrivalTicketNo { get; set; }

            public string RawCurrency { get; set; }

            public float RawTotalPackage { get; set; }
            public float RawGross { get; set; }
            public string Gross { get; set; }
            public string Discount { get; set; }
            public float RawDiscount { get; set; }
            public float RawReceivedAmount { get; set; }
            public float RawBalance { get; set; }

            public Dictionary<string, PlaceHotelInfo> PlaceHotelMap { get; set; }
        }

    }
}

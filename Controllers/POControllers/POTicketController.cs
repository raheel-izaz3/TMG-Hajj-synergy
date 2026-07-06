using System.Linq;
using Azure;
using iText.StyledXmlParser.Jsoup.Nodes;
using iTextSharp.text;
using jsreport.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Versioning;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.MiscModels;
using TMG.Models.POModels;
using TMG.Models.Updated_Accounts;
using static System.Net.WebRequestMethods;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static iTextSharp.text.pdf.AcroFields;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace TMG.Controllers.POControllers;

public enum TicketType
{
    Domestic, International
}

/*public enum FlightConditionType
{
    OneWay = 0, 
    Return = 1,
}*/

public enum FareType
{
    OneWay, RoundTrip
}

public enum CityTax
{
    AT, XT
}

public enum PostingType
{
    Individual = 0,
    HGO = 1,
    Self = 2
}

public enum TicketCategory
{
    Refundable,
    NonRefundable
}

public record FlightDetail
{
    public string HajjiName { get; set; }
    public string FlightNumber { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public int FlightClass { get; set; }
    public string FlightClassName { get; set; }
    public int Airline { get; set; }
    public string AirlineName { get; set; }
    public string TicketNumber { get; set; }
    public string PNRNo { get; set; }
    public string PNRGDS { get; set; }
    public string Passenger { get; set; }
    public string PreferredMeal { get; set; }
    public string FFNumber { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? ArrivalTime { get; set; }
}

public record MemberDetail
{
    public int FamilyHeadId { get; set; }
    public string HajjiName { get; set; }
    public string HbNo { get; set; }
    public string Currency { get; set; }

}

public record POTicketIndex
{
    public int POTicketId { get; set; }
    public string FullName { get; set; }
    public string HBNo { get; set; }
    public string AirlineName { get; set; }
    public string PostingName { get; set; }
    public string VendorName { get; set; }
    public DateTime Date { get; set; }
    public string PONumber { get; set; }
    public string TPRFNo { get; set; }
    public float? GrossAirline { get; set; }
    public float? GrossAirlineReIssue { get; set; }
    public float? GrossAirlineReFund { get; set; }
    public string TicketingAgentName { get; set; }
    public string PostingAgentName { get; set; }
    public string RecoveryAgentName { get; set; }
    public string SPOAgentName { get; set; }
    public string IssueType { get; set; }
    public int Count { get; set; }
    public string PNRNo { get; set; }
}

public record POTicketInvoiceForView
{
    public int? POTicketId { get; set; }
    public int? POTicketHeadId { get; set; }
    public string FullName { get; set; }
    public string IssueType { get; set; }
    public bool IsPrimary { get; set; }
    public string FareType { get; set; }
    public string AirlineName { get; set; }
    public float? FareAmount { get; set; }

}

public class POTicketController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<POTicketController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public POTicketController(TMGDbContext dbContext, IHttpContextAccessor contextAccessor, ILogger<POTicketController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _dbContext = dbContext;
        _http = contextAccessor;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        ViewBag.Trashed = _dbContext.POTickets
            .Where(e => e.Trashed && e.IsPrimary)
            .Count();

        ViewBag.TrashAllowed = PrivilegeManager.CanTrashPOTicket(_dbContext, _http);
        ViewBag.CreateAllowed = PrivilegeManager.CanCreatePOTicket(_dbContext, _http);
        ViewBag.EditAllowed = PrivilegeManager.CanEditPOTicket(_dbContext, _http);
        ViewBag.RestoreAllowed = PrivilegeManager.CanRestorePOTicket(_dbContext, _http);

        var result = _dbContext.POTickets
                        .Include(e => e.FamilyHead)
                        .Include(e => e.FlightDetails)
                        .ThenInclude(e => e.Airline)
                        .Include(e => e.Vendor)
                        .Where(
                                e => e.Trashed == false
                                && e.IsPrimary
                                && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        )
                        .OrderBy(e => e.FamilyHead.FcId)
                        .Select(e => new POTicketIndex
                        {
                            POTicketId = e.POTicketId,
                            FullName = e.FamilyHead.FullName,
                            HBNo = e.FamilyHead.FcId,
                            PostingName = _dbContext.CompaniesWithoutTrash
                                .Where(c => c.CompanyId == e.CompanyId)
                                .Select(c => c.CompanyName)
                                .FirstOrDefault(),
                            AirlineName =
                            e.FareType == FareType.RoundTrip
                            ? (
                                // Return ticket
                                string.Join(" - ",
                                    e.FlightDetails
                                        .Where(fd => (int)fd.AirportsType == 1 || (int)fd.AirportsType == 2) // Departure & Arrival
                                        .Select(fd => fd.Airline.Name)
                                        .Distinct() // same airline repeat na ho
                                        .ToList()
                                )
                            )
                            : (
                                // OneWay tickets → collect unique airline names for same FamilyHead
                                string.Join(" - ",
                                    _dbContext.POTickets
                                        .Where(pt => pt.FamilyHeadId == e.FamilyHeadId
                                                  && pt.FareType == FareType.OneWay
                                                  && !pt.Trashed
                                                  && pt.HajjSeasonsId == e.HajjSeasonsId)
                                        .SelectMany(pt => pt.FlightDetails.Select(fd => fd.Airline.Name))
                                        .Distinct()
                                        .ToList()
                                )
                            ),
                            VendorName = e.Vendor.VendorName,
                            Date = e.Date,
                            PONumber = e.PONumber,
                            TPRFNo = e.TPRFNo,
                            PNRNo = e.FlightDetails.FirstOrDefault().PNRNo,
                            GrossAirline = _dbContext.POTickets
                            .Where(pt => pt.FamilyHeadId == e.FamilyHeadId
                                      && (int)pt.IssuedType == 0 // Issue
                                      && pt.HajjSeasonsId == e.HajjSeasonsId
                                      && !pt.Trashed)
                            .Sum(pt => (float?)pt.GrossAirline) ?? 0,
                            GrossAirlineReIssue = _dbContext.POTickets
                            .Where(pt => pt.FamilyHeadId == e.FamilyHeadId
                                      && (int)pt.IssuedType == 1 // ReIssue
                                      && pt.HajjSeasonsId == e.HajjSeasonsId
                                      && !pt.Trashed)
                            .Sum(pt => (float?)pt.GrossAirline) ?? 0,

                                                    GrossAirlineReFund = _dbContext.POTickets
                            .Where(pt => pt.FamilyHeadId == e.FamilyHeadId
                                      && (int)pt.IssuedType == 2 // Refund
                                      && pt.HajjSeasonsId == e.HajjSeasonsId
                                      && !pt.Trashed)
                            .Sum(pt => (float?)pt.GrossAirline) ?? 0,
                            IssueType = e.IssuedType.ToString(),
                            TicketingAgentName = _dbContext.SPOsWithoutTrash
                            .Where(s => s.SPOId == e.TicketingAgentId)
                            .Select(e => e.Name)
                            .FirstOrDefault(),
                            PostingAgentName = _dbContext.SPOsWithoutTrash
                            .Where(s => s.SPOId == e.PostingAgentId)
                            .Select(e => e.Name)
                            .FirstOrDefault(),
                            RecoveryAgentName = _dbContext.SPOsWithoutTrash
                            .Where(s => s.SPOId == e.RecoveryOfficerId)
                            .Select(e => e.Name)
                            .FirstOrDefault(),
                            SPOAgentName = _dbContext.SPOsWithoutTrash
                            .Where(s => s.SPOId == e.BookingAgentId)
                            .Select(e => e.Name)
                            .FirstOrDefault(),
                            Count = _dbContext.POTicketInvoices
                            .Where(pti => pti.POTicketHeadId == e.POTicketId)
                            .Join(_dbContext.POTickets,
                                pti => pti.POTicketId,
                                pt => pt.POTicketId,
                                (pti, pt) => new { pti, pt })
                            .Join(_dbContext.FamilyHeads,
                                combined => combined.pt.FamilyMembersId,
                                fh => fh.FamilyHeadId,
                                (combined, fh) => fh.FamilyHeadId)
                            .Distinct()
                            .Count()

                        }).ToList();

        return View(result);
    }

    public IActionResult ViewInvoice(int id)
    {
        ViewBag.TrashedCounts = _dbContext.POTicketInvoices
            .Join(_dbContext.POTickets,
                pti => pti.POTicketId,
                pt => pt.POTicketId,
                (pti, pt) => new { pti, pt })
            .Where(combined => combined.pti.POTicketHeadId == id && combined.pt.Trashed)
            .Select(combined => combined.pti)  // Agar aapko POTicketInvoices ki saari details chahiye hon
            .Count();

        //var result = _dbContext.POTicketInvoices
        //    .Join(_dbContext.POTickets,
        //        pti => pti.POTicketId,
        //        pt => pt.POTicketId,
        //        (pti, pt) => new { pti, pt })
        //    .Where(combined => combined.pti.POTicketHeadId == id && combined.pt.Trashed)
        //    .Select(combined => combined.pti)  // Agar aapko POTicketInvoices ki saari details chahiye hon
        //    .ToList();


        var invoices = _dbContext.POTicketInvoices
            .Where(pti => pti.POTicketHeadId == id)
            .Join(_dbContext.POTickets,
                pti => pti.POTicketId,
                pt => pt.POTicketId,
                (pti, pt) => new { pti, pt })
            .Where(combined => !combined.pt.Trashed)
            .Join(_dbContext.FamilyHeads,
                combined => combined.pt.FamilyMembersId,
                fh => fh.FamilyHeadId,
                (combined, fh) => new POTicketInvoiceForView
                {
                    POTicketId = combined.pti.POTicketId,
                    POTicketHeadId = id,
                    FullName = fh.FullName,
                    IssueType = combined.pt.IssuedType.ToString(),
                    IsPrimary = combined.pt.IsPrimary,
                    FareType = combined.pt.FareType.ToString(),
                    FareAmount = combined.pt.GrossAirline,
                    // 👇 Ticket-wise AirlineName
                    AirlineName = combined.pt.FareType == FareType.RoundTrip
                        ? string.Join(" - ",
                            combined.pt.FlightDetails
                                .Where(fd => (int)fd.AirportsType == 1 || (int)fd.AirportsType == 2)
                                .Select(fd => fd.Airline.Name)
                                .Distinct()
                                .ToList()
                        )
                        : string.Join(" - ",
                            combined.pt.FlightDetails
                                .Select(fd => fd.Airline.Name)
                                .Distinct()
                                .ToList()
                        )
                })
            .ToList();


        return View(invoices);
    }

    public IActionResult Create()
    {

        var flight = _dbContext.POTicketFlightDatas.Where(e => !e.Trashed).ToList();
        ViewBag.Flight = flight;

        ViewBag.Companies = _dbContext.Companies.Where(c => !c.Trashed);

        ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead &&
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();


        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);

        ViewBag.TicketType = Enum.GetValues(typeof(TicketType))
                          .Cast<TicketType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.FareType = Enum.GetValues(typeof(FareType))
                          .Cast<FareType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.AirportsType = Enum.GetValues(typeof(AirportsType))
                         .Cast<AirportsType>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

        ViewBag.CityTax = _dbContext.CityTaxes.Where(e => !e.Trashed).ToList();
        ViewBag.SubClasses = _dbContext.SubClasses.Where(e => !e.Trashed).ToList();

        ViewBag.PostingType = Enum.GetValues(typeof(PostingType))
                          .Cast<PostingType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.IssuedType = Enum.GetValues(typeof(IssuedType))
                          .Cast<IssuedType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.TicketCategory = Enum.GetValues(typeof(TicketCategory))
                          .Cast<TicketCategory>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                          .Cast<FlightClass>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

        ViewBag.SPOS = _dbContext.SPOs.Where(s => !s.Trashed);

        ViewBag.Airlines = _dbContext.Airlines.Where(a => !a.Trashed);

        return View();
    }

    [HttpPost]
    public IActionResult Create(POTicketViewModel request)
    {
        //return Ok(request);
        var transaction = _dbContext.Database.BeginTransaction();
        int hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

        try
        {
            #region PaymentVoucherNumber
            var lastPO = _dbContext.POTickets
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .OrderByDescending(x => x.PONumber)
                .Select(x => x.PONumber)
                .FirstOrDefault();

            int nextPONumber = 1;

            if (!string.IsNullOrEmpty(lastPO) && lastPO.StartsWith("PO-TKT-"))
            {
                var numericPart = lastPO.Substring(7);
                nextPONumber = int.Parse(numericPart) + 1;
            }

            string formattedPONumber = $"PO-TKT-{nextPONumber:D3}";
            #endregion

            // Create POTickets
            int? firstPOTicketId = null; // To store the first POTicketId for setting POTicketHeadId
            int headId = 0; // Initialize outside


            // Dictionary to map FamilyMembersId to their corresponding POTicketId
            var familyMemberToPOTicketMap = new Dictionary<int, int>();

            var addedFamilyHeadIds = new HashSet<int>();

            bool isFirstTicket = true; // Flag to track the first ticket

            foreach (var flightdetails in request.PoInvoice)
            {

                var newInvoicei = new POTicket
                {
                    XONumber = flightdetails.XONumber,
                    PassportNumber = flightdetails.PassportNumber,
                    DateofBirth = flightdetails.DateofBirth,
                    PassportExpiry = flightdetails.PassportExpiry,
                    PONumber = formattedPONumber,
                    Date = flightdetails.Date,
                    DueDate = flightdetails.DueDate,
                    TPRFNo = flightdetails.TPRFNo,
                    TicketType = flightdetails.TicketType,
                    FareType = flightdetails.FareType,
                    FamilyHeadId = flightdetails.familyHeadId,
                    FamilyMembersId = flightdetails.FamilyMembersId,
                    PostingType = flightdetails.PostingType,
                    IssueDate = flightdetails.IssueDate,
                    CompanyId = flightdetails.CompanyId,
                    VendorTypeId = flightdetails.VendorTypeId,
                    VendorId = flightdetails.VendorId,
                    BookingAgentId = flightdetails.BookingAgentId,
                    TicketingAgentId = flightdetails.TicketingAgentId,
                    RecoveryOfficerId = flightdetails.RecoveryOfficerId,
                    PostingAgentId = flightdetails.PostingAgentId,
                    BasicFare = flightdetails.basicFare,
                    CommissionPersentage = flightdetails.CommissionPersentage,
                    CommissionAmount = flightdetails.Commission,
                    Discount = flightdetails.Discount,
                    IssuedType = flightdetails.IssuedType,
                    ConersionRate = flightdetails.ConersionRate ?? 0,
                    ntax = flightdetails.ntax,
                    RG = flightdetails.rg,
                    SF = flightdetails.sf,
                    SP = flightdetails.sp,
                    ydAmount = flightdetails.ydAmount,
                    gtaxAmount = flightdetails.gtaxAmount,
                    pzAmount = flightdetails.pzAmount,
                    qaAmount = flightdetails.qaAmount,
                    rtaxAmount = flightdetails.rtaxAmount,
                    etaxAmount = flightdetails.etaxAmount,
                    ioAmount = flightdetails.ioAmount,
                    YR = flightdetails.yr,
                    ttax = flightdetails.ttax,
                    Ftax = flightdetails.Ftax,
                    Zrtax = flightdetails.Zrtax,
                    Aetax = flightdetails.Aetax,
                    Tptax = flightdetails.Tptax,
                    Catax = flightdetails.Catax,
                    Sqtax = flightdetails.Sqtax,
                    Rctax = flightdetails.Rctax,
                    Xttax = flightdetails.Xttax,
                    Aytax = flightdetails.Aytax,
                    Ustax = flightdetails.Ustax,
                    Xatax = flightdetails.Xatax,
                    Xftax = flightdetails.Xftax,
                    Xytax = flightdetails.Xytax,
                    Yctax = flightdetails.Yctax,
                    Ubtax = flightdetails.Ubtax,
                    Gbtax = flightdetails.Gbtax,
                    SVC = flightdetails.svc,
                    SRB = flightdetails.srb,
                    PK = flightdetails.pk,
                    PTT = flightdetails.ptt,
                    PB = flightdetails.pb,
                    YQ = flightdetails.yq,
                    DifferenceOfFare = 0,
                    ReIssuePenulty = 0,
                    RefundPenulty = 0,
                    SeatMappingPenulty = 0,
                    NoShowPenulty = 0,
                    ExtraBaggage = 0,
                    GrossClientFromInvoice = flightdetails.GrossClientFromInvoice ?? 0,
                    NetClientFromInvoice = flightdetails.NetClientFromInvoice ?? 0,
                    NetTaxesForRefund = 0,
                    SVCNet = flightdetails.SVCNet,
                    OtherTaxes = flightdetails.OtherTaxes,
                    AgentPersentage = flightdetails.AgentPersentage,
                    AgentAmount = flightdetails.AgentAmount,
                    AgentFixedAmount = flightdetails.AgentFixedAmount,

                    WithHoldingPers = flightdetails.WTHPersentage,
                    WithHoldingAmount = flightdetails.WTHAmount,
                    KPR = flightdetails.kpr,
                    KBP = flightdetails.kbp,
                    SPTaxPers = flightdetails.spTaxPercentage,
                    SPTaxAmount = flightdetails.spTax,
                    finalkbAmount = flightdetails.finalkbAmount,
                    GrossClient = flightdetails.grossClient,
                    NetClient = flightdetails.netClient,
                    GrossAirline = flightdetails.grossAirline,
                    NetAirline = flightdetails.netAirline,
                    NetTotal = flightdetails.NetTotal,
                    NetTax = flightdetails.NetTax,
                    PassengerCategory = flightdetails.PassengerCategory,
                    FrequentFlyer = flightdetails.FrequentFlyer,
                    IsPrimary = isFirstTicket,
                    HajjSeasonsId = hajjSeasonId
                };

                _dbContext.POTickets.Add(newInvoicei);
                _dbContext.SaveChanges();

                if (firstPOTicketId == null)
                {
                    firstPOTicketId = newInvoicei.POTicketId;
                }

                var poTicketInvi = new POTicketInvoices
                {
                    POTicketHeadId = firstPOTicketId.Value,
                    POTicketId = newInvoicei.POTicketId
                };

                _dbContext.POTicketInvoices.Add(poTicketInvi);
                _dbContext.SaveChanges();

                if (newInvoicei.FamilyMembersId.HasValue)
                {
                    int familyMemberId = newInvoicei.FamilyMembersId.Value;
                    if (!familyMemberToPOTicketMap.ContainsKey(familyMemberId))
                    {
                        familyMemberToPOTicketMap[familyMemberId] = newInvoicei.POTicketId;
                    }
                }

                isFirstTicket = false; // Set to false after first iteration

                if (flightdetails.FlightDetails != null && flightdetails.FlightDetails.Any())
                {
                    foreach (var flight in flightdetails.FlightDetails)
                    {
                        if (flight.FamilyMembersId.HasValue && familyMemberToPOTicketMap.TryGetValue(flight.FamilyMembersId.Value, out int poticketId))
                        {
                            POFlightDetail POFlightDet = new POFlightDetail
                            {
                                TicketCategory = flight.TicketCategory,
                                SeatNumber = flight.SeatNumber,
                                TicketNumber = flight.TicketNumber,
                                POTicketFlightDataId = flight.POTicketFlightDataId,
                                DepartureAirportId = flight.DepartureAirportId,
                                ArrivalAirportId = flight.ArrivalAirportId,
                                FlightDepartureTime = flight.FlightDepartureTime,
                                FlightArrivalTime = flight.FlightArrivalTime,
                                PNRNo = flight.PNRNo,
                                PNRGDSNo = flight.PNRGDSNo,
                                FlightClass = flight.FlightClass,
                                AirportsType = flight.AirportsType,
                                SubClassesId = flight.SubClass,
                                TransectTime = flight.TransectTime,
                                AirlineId = flight.Airline,
                                PreferedMeals = flight.PreferedMeals,
                                ArrivalDate = flight.ArrivalDate,
                                DepartureDate = flight.DepartureDate,
                                CityTaxId = flight.CityTax,
                                Amount = flight.Amount,
                                POTicketId = newInvoicei.POTicketId // Use mapped POTicketId
                            };

                            _dbContext.POFlightDetails.Add(POFlightDet);

                            _dbContext.SaveChanges();

                        }
                    }
                }
            }


            _dbContext.SaveChanges();
            transaction.Commit();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return View(request);
        }
    }

    public IActionResult AddPassenger(int id)
    {

        ViewBag.POTicketId = id;

        ViewBag.PONumber = _dbContext.POTickets.Where(e => e.POTicketId == id).Select(e => e.PONumber).FirstOrDefault();


        var flight = _dbContext.POTicketFlightDatas.Where(e => !e.Trashed).ToList();
        ViewBag.Flight = flight;

        ViewBag.Companies = _dbContext.Companies.Where(c => !c.Trashed);

        ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead &&
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();


        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);

        ViewBag.TicketType = Enum.GetValues(typeof(TicketType))
                          .Cast<TicketType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.AirportsType = Enum.GetValues(typeof(AirportsType))
                        .Cast<AirportsType>()
                        .Select(e => new { Id = (int)e, Name = e.ToString() })
                        .ToList();

        ViewBag.FareType = Enum.GetValues(typeof(FareType))
                          .Cast<FareType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.CityTax = _dbContext.CityTaxes.Where(e => !e.Trashed).ToList();
        ViewBag.SubClasses = _dbContext.SubClasses.Where(e => !e.Trashed).ToList();

        ViewBag.PostingType = Enum.GetValues(typeof(PostingType))
                          .Cast<PostingType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.TicketCategory = Enum.GetValues(typeof(TicketCategory))
                          .Cast<TicketCategory>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                          .Cast<FlightClass>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

        ViewBag.SPOS = _dbContext.SPOs.Where(s => !s.Trashed);

        ViewBag.Airlines = _dbContext.Airlines.Where(a => !a.Trashed);

        return View();
    }

    [HttpPost]
    public IActionResult AddPassenger(POTicketViewModel request)
    {
        //return Ok(request);
        var transaction = _dbContext.Database.BeginTransaction();
        int hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));

        try
        {
            #region PaymentVoucherNumber
            var lastPO = _dbContext.POTickets
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .OrderByDescending(x => x.PONumber)
                .Select(x => x.PONumber)
                .FirstOrDefault();

            int nextPONumber = 1;

            if (!string.IsNullOrEmpty(lastPO) && lastPO.StartsWith("PO-TKT-"))
            {
                var numericPart = lastPO.Substring(7);
                nextPONumber = int.Parse(numericPart) + 1;
            }

            string formattedPONumber = $"PO-TKT-{nextPONumber:D3}";
            #endregion

            // Create POTickets
            int? firstPOTicketId = null; // To store the first POTicketId for setting POTicketHeadId
            int headId = 0; // Initialize outside


            // Dictionary to map FamilyMembersId to their corresponding POTicketId
            var familyMemberToPOTicketMap = new Dictionary<int, int>();

            var addedFamilyHeadIds = new HashSet<int>();

            bool isFirstTicket = true; // Flag to track the first ticket

            foreach (var flightdetails in request.PoInvoice)
            {

                //var familyHeadId = flightdetails.FamilyMembersId;

                //if (addedFamilyHeadIds.Contains(familyHeadId))
                //{
                //    continue;
                //}

                //addedFamilyHeadIds.Add(familyHeadId);

                var newInvoicei = new POTicket
                {
                    XONumber = flightdetails.XONumber,
                    PassportNumber = flightdetails.PassportNumber,
                    DateofBirth = flightdetails.DateofBirth,
                    PassportExpiry = flightdetails.PassportExpiry,
                    PONumber = flightdetails.PONumber,
                    Date = flightdetails.Date,
                    DueDate = flightdetails.DueDate,
                    TPRFNo = flightdetails.TPRFNo,
                    TicketType = flightdetails.TicketType,
                    FareType = flightdetails.FareType,
                    FamilyHeadId = flightdetails.familyHeadId,
                    FamilyMembersId = flightdetails.FamilyMembersId,
                    PostingType = flightdetails.PostingType,
                    IssuedType = IssuedType.Issued,
                    IssueDate = flightdetails.IssueDate,
                    CompanyId = flightdetails.CompanyId,
                    VendorTypeId = flightdetails.VendorTypeId,
                    VendorId = flightdetails.VendorId,
                    BookingAgentId = flightdetails.BookingAgentId,
                    TicketingAgentId = flightdetails.TicketingAgentId,
                    RecoveryOfficerId = flightdetails.RecoveryOfficerId,
                    PostingAgentId = flightdetails.PostingAgentId,
                    BasicFare = flightdetails.basicFare,
                    CommissionPersentage = flightdetails.CommissionPersentage,
                    CommissionAmount = flightdetails.Commission,
                    Discount = flightdetails.Discount,
                    AgentPersentage = flightdetails.AgentPersentage,
                    AgentAmount = flightdetails.AgentAmount,
                    AgentFixedAmount = flightdetails.AgentFixedAmount,

                    ntax = flightdetails.ntax,
                    RG = flightdetails.rg,
                    SF = flightdetails.sf,
                    SP = flightdetails.sp,
                    ydAmount = flightdetails.ydAmount,
                    gtaxAmount = flightdetails.gtaxAmount,
                    pzAmount = flightdetails.pzAmount,
                    qaAmount = flightdetails.qaAmount,
                    rtaxAmount = flightdetails.rtaxAmount,
                    etaxAmount = flightdetails.etaxAmount,
                    ioAmount = flightdetails.ioAmount,
                    YR = flightdetails.yr,
                    ttax = flightdetails.ttax,
                    Ftax = flightdetails.Ftax,
                    Zrtax = flightdetails.Zrtax,
                    Aetax = flightdetails.Aetax,
                    Tptax = flightdetails.Tptax,
                    Catax = flightdetails.Catax,
                    Sqtax = flightdetails.Sqtax,
                    Rctax = flightdetails.Rctax,
                    Xttax = flightdetails.Xttax,
                    Aytax = flightdetails.Aytax,
                    Ustax = flightdetails.Ustax,
                    Xatax = flightdetails.Xatax,
                    Xftax = flightdetails.Xftax,
                    Xytax = flightdetails.Xytax,
                    Yctax = flightdetails.Yctax,
                    Ubtax = flightdetails.Ubtax,
                    Gbtax = flightdetails.Gbtax,
                    SVC = flightdetails.svc,
                    SRB = flightdetails.srb,
                    PK = flightdetails.pk,
                    PTT = flightdetails.ptt,
                    PB = flightdetails.pb,
                    YQ = flightdetails.yq,
                    OtherTaxes = flightdetails.OtherTaxes,

                    WithHoldingPers = flightdetails.WTHPersentage,
                    WithHoldingAmount = flightdetails.WTHAmount,
                    KPR = flightdetails.kpr,
                    KBP = flightdetails.kbp,
                    SPTaxPers = flightdetails.spTaxPercentage,
                    SPTaxAmount = flightdetails.spTax,
                    finalkbAmount = flightdetails.finalkbAmount,
                    GrossClient = flightdetails.grossClient,
                    NetClient = flightdetails.netClient,
                    GrossAirline = flightdetails.grossAirline,
                    NetAirline = flightdetails.netAirline,
                    NetTotal = flightdetails.NetTotal,
                    NetTax = flightdetails.NetTax,
                    PassengerCategory = flightdetails.PassengerCategory,
                    FrequentFlyer = flightdetails.FrequentFlyer,
                    IsPrimary = false, // First ticket gets true, rest get false
                    HajjSeasonsId = hajjSeasonId
                };

                _dbContext.POTickets.Add(newInvoicei);
                _dbContext.SaveChanges();

                if (firstPOTicketId == null)
                {
                    firstPOTicketId = newInvoicei.POTicketId;
                }

                var poTicketInvi = new POTicketInvoices
                {
                    POTicketHeadId = flightdetails.POTicketId,
                    POTicketId = newInvoicei.POTicketId
                };

                _dbContext.POTicketInvoices.Add(poTicketInvi);
                _dbContext.SaveChanges();

                if (newInvoicei.FamilyMembersId.HasValue)
                {
                    int familyMemberId = newInvoicei.FamilyMembersId.Value;
                    if (!familyMemberToPOTicketMap.ContainsKey(familyMemberId))
                    {
                        familyMemberToPOTicketMap[familyMemberId] = newInvoicei.POTicketId;
                    }
                }

                isFirstTicket = false; // Set to false after first iteration

                if (flightdetails.FlightDetails != null && flightdetails.FlightDetails.Any())
                {
                    foreach (var flight in flightdetails.FlightDetails)
                    {
                        if (flight.FamilyMembersId.HasValue && familyMemberToPOTicketMap.TryGetValue(flight.FamilyMembersId.Value, out int poticketId))
                        {
                            POFlightDetail POFlightDet = new POFlightDetail
                            {
                                TicketCategory = flight.TicketCategory,
                                SeatNumber = flight.SeatNumber,
                                TicketNumber = flight.TicketNumber,
                                //FamilyHeadId = flight.familyHeadId,
                                //FamilyMembersId = flight.FamilyMembersId,
                                POTicketFlightDataId = flight.POTicketFlightDataId,
                                DepartureAirportId = flight.DepartureAirportId,
                                ArrivalAirportId = flight.ArrivalAirportId,
                                FlightDepartureTime = flight.FlightDepartureTime,
                                FlightArrivalTime = flight.FlightArrivalTime,
                                PNRNo = flight.PNRNo,
                                PNRGDSNo = flight.PNRGDSNo,
                                FlightClass = flight.FlightClass,
                                AirportsType = flight.AirportsType,
                                SubClassesId = flight.SubClass,
                                TransectTime = flight.TransectTime,
                                AirlineId = flight.Airline,
                                PreferedMeals = flight.PreferedMeals,
                                ArrivalDate = flight.ArrivalDate,
                                DepartureDate = flight.DepartureDate,
                                CityTaxId = flight.CityTax,
                                Amount = flight.Amount,
                                POTicketId = newInvoicei.POTicketId // Use mapped POTicketId
                            };

                            _dbContext.POFlightDetails.Add(POFlightDet);
                        }
                    }
                }
            }


            _dbContext.SaveChanges();
            transaction.Commit();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return View(request);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {

        var flight = _dbContext.POTicketFlightDatas.Where(e => !e.Trashed).ToList();
        ViewBag.Flight = flight;

        ViewBag.Companies = _dbContext.Companies.Where(c => !c.Trashed);

        ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead &&
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.FamilyMembers = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Vendor = _dbContext.Vendors.ToList();

        ViewBag.TicketType = Enum.GetValues(typeof(TicketType))
                          .Cast<TicketType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.AirportsType = Enum.GetValues(typeof(AirportsType))
                         .Cast<AirportsType>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

        ViewBag.FareType = Enum.GetValues(typeof(FareType))
                          .Cast<FareType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.CityTax = _dbContext.CityTaxes.Where(e => !e.Trashed);

        ViewBag.PostingType = Enum.GetValues(typeof(PostingType))
                          .Cast<PostingType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.TicketCategory = Enum.GetValues(typeof(TicketCategory))
                          .Cast<TicketCategory>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                          .Cast<FlightClass>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        //ViewBag.IssuedType = Enum.GetValues(typeof(IssuedType))
        //                  .Cast<IssuedType>()
        //                  .Select(e => new { Id = (int)e, Name = e.ToString() })
        //                  .ToList();

        ViewBag.IssuedType = Enum.GetValues(typeof(IssuedType))
                          .Cast<IssuedType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

        ViewBag.SPOS = _dbContext.SPOs.Where(s => !s.Trashed);

        ViewBag.Airlines = _dbContext.Airlines.Where(a => !a.Trashed);

        ViewBag.SubClasses = _dbContext.SubClasses.Where(e => !e.Trashed).ToList();

        POTicket po = await _dbContext.POTickets.Include(e => e.Vendor).SingleOrDefaultAsync(e => e.POTicketId == id);

        if (po == null)
        {
            return Json(new { msg = "Not Found" });
        }


        ViewBag.FlightDetails = await _dbContext.POFlightDetails
            .Include(d => d.Departure)
            .Include(d => d.Arrival)
            .Include(d => d.SubClasses)
            .Include(d => d.POTicketFlightData)
            .Where(e => e.POTicketId == po.POTicketId)
            .ToListAsync();

        POTicketEditModel ticket = new POTicketEditModel
        {
            POTicketId = po.POTicketId,
            PONumber = po.PONumber,
            XONumber = po.XONumber,
            Date = po.Date,
            ReIssuedTicketNumber = po.ReIssuedTicketNumber,
            DueDate = po.DueDate,
            familyHeadId = po.FamilyHeadId,
            TPRFNo = po.TPRFNo,
            TicketType = po.TicketType,
            FareType = po.FareType,
            FamilyMembersId = po.FamilyMembersId,
            PostingType = po.PostingType,
            IssueDate = po.IssueDate,
            CompanyId = po.CompanyId,
            VendorTypeId = po.VendorTypeId,
            VendorId = po.VendorId,
            BookingAgentId = po.BookingAgentId,
            TicketingAgentId = po.TicketingAgentId,
            RecoveryOfficerId = po.RecoveryOfficerId,
            PostingAgentId = po.PostingAgentId,
            BasicFare = po.BasicFare,
            CommissionPercentage = po.CommissionPersentage,
            Commission = po.CommissionAmount,
            AgentPersentage = po.AgentPersentage,
            AgentAmount = po.AgentAmount ?? 0,
            AgentFixedAmount = po.AgentFixedAmount,
            IssuedType = po.IssuedType,
            ConersionRate = po.ConersionRate,
            ntax = po.ntax,
            rg = po.RG,
            sf = po.SF,
            sp = po.SP,
            ydAmount = po.ydAmount,
            gtaxAmount = po.gtaxAmount,
            pzAmount = po.pzAmount,
            qaAmount = po.qaAmount,
            rtaxAmount = po.rtaxAmount,
            etaxAmount = po.etaxAmount,
            ioAmount = po.ioAmount,
            yr = po.YR,
            ttax = po.ttax,
            Ftax = po.Ftax,
            Zrtax = po.Zrtax,
            Aetax = po.Aetax,
            Tptax = po.Tptax,
            Catax = po.Catax,
            Sqtax = po.Sqtax,
            Rctax = po.Rctax,
            Xttax = po.Xttax,
            Aytax = po.Aytax,
            Ustax = po.Ustax,
            Xatax = po.Xatax,
            Xftax = po.Xftax,
            Xytax = po.Xytax,
            Yctax = po.Yctax,
            Ubtax = po.Ubtax,
            Gbtax = po.Gbtax,
            svc = po.SVC,
            srb = po.SRB,
            pk = po.PK,
            ptt = po.PTT,
            pb = po.PB,
            yq = po.YQ,
            OtherTaxes = po.OtherTaxes ?? 0,

            WithHoldingPercentage = po.WithHoldingPers,
            withholding = po.WithHoldingAmount,
            Discount = po.Discount,
            KPR = po.KPR,
            KBP = po.KBP,
            SPTax = po.SPTaxAmount,
            finalkbAmount = po.finalkbAmount,
            SPTaxPercentage = po.SPTaxPers,
            GrossClient = po.GrossClient,
            NetClient = po.NetClient,
            GrossAirline = po.GrossAirline,
            NetAirline = po.NetAirline,
            NetTax = po.NetTax,
            NetTotal = po.NetTotal ?? 0,
            PassportNumber = po.PassportNumber,
            DateofBirth = po.DateofBirth,
            PassportExpiry = po.PassportExpiry,
            PassengerCategory = po.PassengerCategory,
            FrequentFlyer = po.FrequentFlyer,
            ReIssuePenulty = po.ReIssuePenulty ?? 0,
            DifferenceOfFare = po.DifferenceOfFare ?? 0,
            RefundPenulty = po.RefundPenulty ?? 0,
            SeatMappingPenulty = po.SeatMappingPenulty ?? 0,
            NoShowPenulty = po.NoShowPenulty ?? 0,
            ExtraBaggage = po.ExtraBaggage ?? 0,
            ClientFareAmounFromInvoice = po.GrossClientFromInvoice,
            NetTaxesForRefund = po.NetTaxesForRefund ?? 0,
            //Discount = po.Discount
        };

        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(POTicketEditModel request)
    {
        //return Ok(request);
        if (request == null)
        {
            return BadRequest("Request cannot be null");
        }

        var sessionValue = _http.HttpContext.Session.GetString("ActiveSeasonId");
        if (string.IsNullOrEmpty(sessionValue) || !int.TryParse(sessionValue, out int hajjSeasonId))
        {
            return BadRequest("ActiveSeasonId is missing or invalid.");
        }

        try
        {
            var poInvoice = await _dbContext.POTickets
                .Include(p => p.FlightDetails)
                .FirstOrDefaultAsync(p => p.POTicketId == request.POTicketId);

            if (poInvoice == null)
            {
                return NotFound($"POTicket with ID {request.POTicketId} not found.");
            }

            // Remove all existing flight details for this POTicket
            var existingFlights = _dbContext.POFlightDetails
                .Where(f => f.POTicketId == request.POTicketId);

            _dbContext.POFlightDetails.RemoveRange(existingFlights);
            await _dbContext.SaveChangesAsync(); // Ensure deletions are saved before adding new records

            // Update PO Invoice main details
            poInvoice.XONumber = request.XONumber;
            poInvoice.Date = request.Date;
            poInvoice.DueDate = request.DueDate;
            poInvoice.TPRFNo = request.TPRFNo;
            poInvoice.TicketType = request.TicketType;
            poInvoice.IssuedType = request.IssuedType;
            poInvoice.FareType = request.FareType;
            poInvoice.FamilyHeadId = request.familyHeadId;
            poInvoice.FamilyMembersId = request.FamilyMembersId;
            poInvoice.PostingType = request.PostingType;
            poInvoice.IssueDate = request.IssueDate;
            poInvoice.CompanyId = request.CompanyId;
            poInvoice.VendorTypeId = request.VendorTypeId;
            poInvoice.VendorId = request.VendorId;
            poInvoice.BookingAgentId = request.BookingAgentId;
            poInvoice.TicketingAgentId = request.TicketingAgentId;
            poInvoice.RecoveryOfficerId = request.RecoveryOfficerId;
            poInvoice.PostingAgentId = request.PostingAgentId;
            poInvoice.BasicFare = request.BasicFare ?? 0;
            poInvoice.CommissionPersentage = request.CommissionPercentage ?? 0;
            poInvoice.CommissionAmount = request.Commission ?? 0;
            poInvoice.Discount = request.Discount ?? 0;
            poInvoice.NetTotal = request.NetTotal ?? 0;
            poInvoice.AgentPersentage = request.AgentPersentage ?? 0;
            poInvoice.AgentAmount = request.AgentAmount ?? 0;
            poInvoice.AgentFixedAmount = request.AgentFixedAmount ?? 0;
            poInvoice.IssuedType = request.IssuedType;
            poInvoice.ReIssuedTicketNumber = request.ReIssuedTicketNumber ?? "UnKnown";
            poInvoice.ntax = request.ntax ?? 0;
            poInvoice.RG = request.rg ?? 0;
            poInvoice.SF = request.sf ?? 0;
            poInvoice.SP = request.sp ?? 0;
            poInvoice.ydAmount = request.ydAmount ?? 0;
            poInvoice.gtaxAmount = request.gtaxAmount ?? 0;
            poInvoice.pzAmount = request.pzAmount ?? 0;
            poInvoice.qaAmount = request.qaAmount ?? 0;
            poInvoice.rtaxAmount = request.rtaxAmount ?? 0;
            poInvoice.etaxAmount = request.etaxAmount ?? 0;
            poInvoice.ioAmount = request.ioAmount ?? 0;
            poInvoice.YR = request.yr ?? 0;
            poInvoice.ttax = request.ttax ?? 0;
            poInvoice.Ftax = request.Ftax ?? 0;
            poInvoice.Zrtax = request.Zrtax ?? 0;
            poInvoice.Aetax = request.Aetax ?? 0;
            poInvoice.Tptax = request.Tptax ?? 0;
            poInvoice.Catax = request.Catax ?? 0;
            poInvoice.Sqtax = request.Sqtax ?? 0;
            poInvoice.Rctax = request.Rctax ?? 0;
            poInvoice.Xttax = request.Xttax ?? 0;
            poInvoice.Aytax = request.Aytax ?? 0;
            poInvoice.Ustax = request.Ustax ?? 0;
            poInvoice.Xatax = request.Xatax ?? 0;
            poInvoice.Xftax = request.Xftax ?? 0;
            poInvoice.Xytax = request.Xytax ?? 0;
            poInvoice.Yctax = request.Yctax ?? 0;
            poInvoice.Ubtax = request.Ubtax ?? 0;
            poInvoice.Gbtax = request.Gbtax ?? 0;
            poInvoice.ReIssuePenulty = request.ReIssuePenulty ?? 0;
            poInvoice.DifferenceOfFare = request.DifferenceOfFare ?? 0;
            poInvoice.RefundPenulty = request.RefundPenulty ?? 0;
            poInvoice.SeatMappingPenulty = request.SeatMappingPenulty ?? 0;
            poInvoice.NoShowPenulty = request.NoShowPenulty ?? 0;
            poInvoice.ExtraBaggage = request.ExtraBaggage ?? 0;
            poInvoice.SVC = request.svc ?? 0;
            poInvoice.SRB = request.srb ?? 0;
            poInvoice.PK = request.pk ?? 0;
            poInvoice.PTT = request.ptt ?? 0;
            poInvoice.PB = request.pb ?? 0;
            poInvoice.YQ = request.yq ?? 0;
            poInvoice.OtherTaxes = request.OtherTaxes ?? 0;
            poInvoice.WithHoldingPers = request.WithHoldingPercentage ?? 0;
            poInvoice.WithHoldingAmount = request.withholding ?? 0;
            poInvoice.KPR = request.KPR ?? 0;
            poInvoice.KBP = request.KBP ?? 0;
            poInvoice.SPTaxPers = request.SPTaxPercentage ?? 0;
            poInvoice.SPTaxAmount = request.SPTax ?? 0;
            poInvoice.finalkbAmount = request.finalkbAmount ?? 0;
            poInvoice.GrossClient = request.GrossClient ?? 0;
            poInvoice.NetClient = request.NetClient ?? 0;
            poInvoice.GrossAirline = request.GrossAirline ?? 0;
            poInvoice.NetTax = request.NetTax;
            poInvoice.PassportNumber = request.PassportNumber;
            poInvoice.PassportExpiry = request.PassportExpiry;
            poInvoice.DateofBirth = request.DateofBirth;
            poInvoice.PassengerCategory = request.PassengerCategory;
            poInvoice.FrequentFlyer = request.FrequentFlyer;
            poInvoice.HajjSeasonsId = hajjSeasonId;
            poInvoice.GrossClientFromInvoice = request.ClientFareAmounFromInvoice ?? 0f;
            poInvoice.NetClientFromInvoice = request.NetClientFromInvoice ?? 0f;
            poInvoice.SVCNet = request.SVCNet ?? 0f;
            poInvoice.NetTaxesForRefund = request.NetTaxesForRefund ?? 0f;
            poInvoice.ConersionRate = request.ConersionRate ?? 0f;
            // Add new flight details from the request
            List<POFlightDetail> newFlightDetails = new List<POFlightDetail>();

            /*for (int i = 0; i < request.TicketNumber.Count(); i++)
            {
                var newFlight = new POFlightDetail
                {
                    POTicketId = request.POTicketId,
                    TicketCategory = request.TicketCategory[i],
                    SeatNumber = request.SeatNumber[i],
                    TicketNumber = request.TicketNumber[i],
                    POTicketFlightDataId = request.POTicketFlightDataId[i],
                    DepartureAirportId = request.DepartureAirportId[i],
                    ArrivalAirportId = request.ArrivalAirportId[i],
                    FlightDepartureTime = request.FlightDepartureTime[i],
                    FlightArrivalTime = request.FlightArrivalTime[i],
                    PNRNo = request.PNRNo[i],
                    PNRGDSNo = request.PNRGDSNo[i],
                    PreferedMeals = request.PreferedMeals[i],
                    FlightClass = request.FlightClass.ElementAtOrDefault(i), // Returns null if index is out of range
                    //AirportsType = request.AirportsType.ElementAtOrDefault(i),
                    TicketImages = request.Url[i],
                    AirlineId = request.Airline[i],
                    ArrivalDate = request.ArrivalDate[i],
                    DepartureDate = request.DepartureDate[i],
                    CityTaxId = request.CityTax[i],
                    TransectTime = request.TransectTime[i],
                    SubClassesId = request.SubClass[i],
                    Amount = request.Amount[i]
                };

                newFlightDetails.Add(newFlight);
            }*/

            for (int i = 0; i < request.TicketNumber.Count(); i++)
            {
                string ticketImagePath = null;

                if (request.Url != null && request.Url.Count > i && request.Url[i] != null && request.Url[i].Length > 0)
                {
                    var file = request.Url[i];
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/TicketImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Save relative path
                    ticketImagePath = "/Images/TicketImages/" + fileName;
                }
                else if (request.ExistingUrls != null && request.ExistingUrls.Count > i)
                {
                    // Use the old path if no new file was uploaded
                    ticketImagePath = request.ExistingUrls[i];
                }

                var newFlight = new POFlightDetail
                {
                    POTicketId = request.POTicketId,
                    TicketCategory = request.TicketCategory[i],
                    SeatNumber = request.SeatNumber[i],
                    TicketNumber = request.TicketNumber[i],
                    POTicketFlightDataId = request.POTicketFlightDataId[i],
                    DepartureAirportId = request.DepartureAirportId[i],
                    ArrivalAirportId = request.ArrivalAirportId[i],
                    FlightDepartureTime = request.FlightDepartureTime[i],
                    FlightArrivalTime = request.FlightArrivalTime[i],
                    PNRNo = request.PNRNo[i],
                    PNRGDSNo = request.PNRGDSNo[i],
                    PreferedMeals = request.PreferedMeals[i],
                    FlightClass = request.FlightClass.ElementAtOrDefault(i),
                    AirportsType = request.AirportsType[i],
                    TicketImages = ticketImagePath,
                    AirlineId = request.Airline[i],
                    ArrivalDate = request.ArrivalDate[i],
                    DepartureDate = request.DepartureDate[i],
                    CityTaxId = request.CityTax[i],
                    TransectTime = request.TransectTime[i],
                    SubClassesId = request.SubClass[i],
                    Amount = request.Amount[i]
                };

                newFlightDetails.Add(newFlight);
            }



            // Add new flights
            await _dbContext.POFlightDetails.AddRangeAsync(newFlightDetails);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", new { id = poInvoice.POTicketId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while editing the PO Ticket");
            return StatusCode(500, "Internal server error");
        }
    }

    public async Task<IActionResult> CopyData(int id)
    {
        ViewBag.POTickedHeadId = id;
        var flight = _dbContext.POTicketFlightDatas.Where(e => !e.Trashed).ToList();
        ViewBag.Flight = flight;

        ViewBag.Companies = _dbContext.Companies.Where(c => !c.Trashed);

        ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead &&
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.FamilyMembers = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Vendor = _dbContext.Vendors.ToList();

        ViewBag.TicketType = Enum.GetValues(typeof(TicketType))
                          .Cast<TicketType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.AirportsType = Enum.GetValues(typeof(AirportsType))
                         .Cast<AirportsType>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

        ViewBag.FareType = Enum.GetValues(typeof(FareType))
                          .Cast<FareType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.CityTax = _dbContext.CityTaxes.Where(e => !e.Trashed);

        ViewBag.PostingType = Enum.GetValues(typeof(PostingType))
                          .Cast<PostingType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.TicketCategory = Enum.GetValues(typeof(TicketCategory))
                          .Cast<TicketCategory>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                          .Cast<FlightClass>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.IssuedType = Enum.GetValues(typeof(IssuedType))
                          .Cast<IssuedType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

        ViewBag.SPOS = _dbContext.SPOs.Where(s => !s.Trashed);

        ViewBag.Airlines = _dbContext.Airlines.Where(a => !a.Trashed);

        ViewBag.SubClasses = _dbContext.SubClasses.Where(e => !e.Trashed).ToList();

        POTicket po = await _dbContext.POTickets.Include(e => e.Vendor).SingleOrDefaultAsync(e => e.POTicketId == id);

        if (po == null)
        {
            return Json(new { msg = "Not Found" });
        }


        ViewBag.FlightDetails = await _dbContext.POFlightDetails
            .Include(d => d.Departure)
            .Include(d => d.Arrival)
            .Include(d => d.SubClasses)
            .Include(d => d.POTicketFlightData)
            .Where(e => e.POTicketId == po.POTicketId)
            .ToListAsync();

        POTicketEditModel ticket = new POTicketEditModel
        {
            POTicketId = po.POTicketId,
            PONumber = po.PONumber,
            XONumber = po.XONumber,
            Date = po.Date,
            DueDate = po.DueDate,
            familyHeadId = po.FamilyHeadId,
            TPRFNo = po.TPRFNo,
            TicketType = po.TicketType,
            FareType = po.FareType,
            FamilyMembersId = po.FamilyMembersId,
            PostingType = po.PostingType,
            IssueDate = po.IssueDate,
            CompanyId = po.CompanyId,
            VendorTypeId = po.VendorTypeId,
            VendorId = po.VendorId,
            BookingAgentId = po.BookingAgentId,
            TicketingAgentId = po.TicketingAgentId,
            RecoveryOfficerId = po.RecoveryOfficerId,
            PostingAgentId = po.PostingAgentId,
            BasicFare = po.BasicFare,
            CommissionPercentage = po.CommissionPersentage,
            Commission = po.CommissionAmount,
            AgentPersentage = po.AgentPersentage,
            AgentAmount = po.AgentAmount ?? 0,
            AgentFixedAmount = po.AgentFixedAmount,
            IssuedType = po.IssuedType,

            ntax = po.ntax,
            rg = po.RG,
            sf = po.SF,
            sp = po.SP,
            ydAmount = po.ydAmount,
            gtaxAmount = po.gtaxAmount,
            pzAmount = po.pzAmount,
            qaAmount = po.qaAmount,
            rtaxAmount = po.rtaxAmount,
            etaxAmount = po.etaxAmount,
            ioAmount = po.ioAmount,
            yr = po.YR,
            ttax = po.ttax,
            Ftax = po.Ftax,
            Zrtax = po.Zrtax,
            Aetax = po.Aetax,
            Tptax = po.Tptax,
            Catax = po.Catax,
            Sqtax = po.Sqtax,
            Rctax = po.Rctax,
            Xttax = po.Xttax,
            Aytax = po.Aytax,
            Ustax = po.Ustax,
            Xatax = po.Xatax,
            Xftax = po.Xftax,
            Xytax = po.Xytax,
            Yctax = po.Yctax,
            Ubtax = po.Ubtax,
            Gbtax = po.Gbtax,
            svc = po.SVC,
            srb = po.SRB,
            pk = po.PK,
            ptt = po.PTT,
            pb = po.PB,
            yq = po.YQ,
            ReIssuePenulty = po.ReIssuePenulty,
            DifferenceOfFare = po.DifferenceOfFare,
            RefundPenulty = po.RefundPenulty,
            SeatMappingPenulty = po.SeatMappingPenulty,
            NoShowPenulty = po.NoShowPenulty,
            ExtraBaggage = po.ExtraBaggage,
            OtherTaxes = po.OtherTaxes ?? 0,

            WithHoldingPercentage = po.WithHoldingPers,
            withholding = po.WithHoldingAmount,
            Discount = po.Discount,
            KPR = po.KPR,
            KBP = po.KBP,
            SPTax = po.SPTaxAmount,
            finalkbAmount = po.finalkbAmount,
            SPTaxPercentage = po.SPTaxPers,
            GrossClient = po.GrossClient,
            NetClient = po.NetClient,
            GrossAirline = po.GrossAirline,
            NetAirline = po.NetAirline,
            NetTax = po.NetTax,
            NetTotal = po.NetTotal ?? 0,
            PassportNumber = po.PassportNumber,
            DateofBirth = po.DateofBirth,
            PassportExpiry = po.PassportExpiry,
            PassengerCategory = po.PassengerCategory,
            FrequentFlyer = po.FrequentFlyer
            //Discount = po.Discount
        };

        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> CopyData(POTicketEditModel request)
    {
        //return Ok(request);
        if (request == null)
        {
            return BadRequest("Request cannot be null");
        }

        var sessionValue = _http.HttpContext.Session.GetString("ActiveSeasonId");
        if (string.IsNullOrEmpty(sessionValue) || !int.TryParse(sessionValue, out int hajjSeasonId))
        {
            return BadRequest("ActiveSeasonId is missing or invalid.");
        }

        try
        {

            var lastPO = _dbContext.POTickets
               .Where(e => e.HajjSeasonsId == hajjSeasonId)
               .OrderByDescending(x => x.PONumber)
               .Select(x => x.PONumber)
               .FirstOrDefault();

            int nextPONumber = 1;

            if (!string.IsNullOrEmpty(lastPO) && lastPO.StartsWith("PO-TKT-"))
            {
                var numericPart = lastPO.Substring(7);
                nextPONumber = int.Parse(numericPart) + 1;
            }

            string formattedPONumber = $"PO-TKT-{nextPONumber:D3}";

            // Create POTickets
            int? firstPOTicketId = null; // To store the first POTicketId for setting POTicketHeadId
            int headId = 0; // Initialize outside


            // Dictionary to map FamilyMembersId to their corresponding POTicketId
            var familyMemberToPOTicketMap = new Dictionary<int, int>();

            var addedFamilyHeadIds = new HashSet<int>();

            // Create a new POTicket (new entry)
            var newPoInvoice = new POTicket
            {
                XONumber = request.XONumber,
                PONumber = formattedPONumber,
                Date = request.Date,
                DueDate = request.DueDate,
                TPRFNo = request.TPRFNo,
                TicketType = request.TicketType,
                FareType = request.FareType,
                FamilyHeadId = request.familyHeadId,
                FamilyMembersId = request.FamilyMembersId,
                PostingType = request.PostingType,
                IssueDate = request.IssueDate,
                CompanyId = request.CompanyId,
                VendorTypeId = request.VendorTypeId,
                VendorId = request.VendorId,
                BookingAgentId = request.BookingAgentId,
                TicketingAgentId = request.TicketingAgentId,
                RecoveryOfficerId = request.RecoveryOfficerId,
                PostingAgentId = request.PostingAgentId,
                BasicFare = request.BasicFare ?? 0,
                CommissionPersentage = request.CommissionPercentage ?? 0,
                CommissionAmount = request.Commission ?? 0,
                Discount = request.Discount ?? 0,
                NetTotal = request.NetTotal ?? 0,
                AgentPersentage = request.AgentPersentage ?? 0,
                AgentAmount = request.AgentAmount ?? 0,
                AgentFixedAmount = request.AgentFixedAmount ?? 0,
                IssuedType = request.IssuedType,

                ntax = request.ntax ?? 0f,
                RG = request.rg ?? 0f,
                SF = request.sf ?? 0f,
                SP = request.sp ?? 0f,
                ydAmount = request.ydAmount ?? 0f,
                gtaxAmount = request.gtaxAmount ?? 0f,
                pzAmount = request.pzAmount ?? 0f,
                qaAmount = request.qaAmount ?? 0f,
                rtaxAmount = request.rtaxAmount ?? 0f,
                etaxAmount = request.etaxAmount ?? 0f,
                ioAmount = request.ioAmount ?? 0f,
                YR = request.yr ?? 0f,
                ttax = request.ttax ?? 0f,
                Ftax = request.Ftax ?? 0f,
                Zrtax = request.Zrtax ?? 0f,
                Aetax = request.Aetax ?? 0f,
                Tptax = request.Tptax ?? 0f,
                Catax = request.Catax ?? 0f,
                Sqtax = request.Sqtax ?? 0f,
                Rctax = request.Rctax ?? 0f,
                Xttax = request.Xttax ?? 0f,
                Aytax = request.Aytax ?? 0f,
                Ustax = request.Ustax ?? 0f,
                Xatax = request.Xatax ?? 0f,
                Xftax = request.Xftax ?? 0f,
                Xytax = request.Xytax ?? 0f,
                Yctax = request.Yctax ?? 0f,
                Ubtax = request.Ubtax ?? 0f,
                Gbtax = request.Gbtax ?? 0f,
                SVC = request.svc ?? 0f,
                SRB = request.srb ?? 0f,
                PK = request.pk ?? 0f,
                PTT = request.ptt ?? 0f,
                PB = request.pb ?? 0f,
                YQ = request.yq ?? 0f,
                ReIssuePenulty = request.ReIssuePenulty ?? 0f,
                DifferenceOfFare = request.DifferenceOfFare ?? 0f,
                RefundPenulty = request.RefundPenulty ?? 0f,
                SeatMappingPenulty = request.SeatMappingPenulty ?? 0f,
                NoShowPenulty = request.NoShowPenulty ?? 0f,
                ExtraBaggage = request.NoShowPenulty ?? 0f,
                OtherTaxes = request.OtherTaxes ?? 0f,
                WithHoldingPers = request.WithHoldingPercentage ?? 0f,
                WithHoldingAmount = request.withholding ?? 0f,
                KPR = request.KPR ?? 0f,
                KBP = request.KBP ?? 0f,
                SPTaxPers = request.SPTaxPercentage ?? 0f,
                SPTaxAmount = request.SPTax ?? 0f,
                finalkbAmount = request.finalkbAmount ?? 0f,
                GrossClient = request.GrossClient ?? 0f,
                NetClient = request.NetClient ?? 0f,
                GrossAirline = request.GrossAirline ?? 0f,
                NetTax = request.NetTax,
                PassportNumber = request.PassportNumber,
                PassportExpiry = request.PassportExpiry,
                DateofBirth = request.DateofBirth,
                PassengerCategory = request.PassengerCategory,
                FrequentFlyer = request.FrequentFlyer,
                IsPrimary = true, // First ticket gets true, rest get false
                HajjSeasonsId = hajjSeasonId
            };

            _dbContext.POTickets.Add(newPoInvoice); // Add the new POTicket entry
            await _dbContext.SaveChangesAsync(); // Save the new POTicket

            if (firstPOTicketId == null)
            {
                firstPOTicketId = newPoInvoice.POTicketId;
            }

            var poTicketInvi = new POTicketInvoices
            {
                POTicketHeadId = firstPOTicketId.Value,
                POTicketId = newPoInvoice.POTicketId
            };

            _dbContext.POTicketInvoices.Add(poTicketInvi);
            _dbContext.SaveChanges();

            // Add new flight details from the request
            List<POFlightDetail> newFlightDetails = new List<POFlightDetail>();

            for (int i = 0; i < request.TicketNumber.Count(); i++)
            {
                string ticketImagePath = null;

                if (request.Url != null && request.Url.Count > i && request.Url[i] != null && request.Url[i].Length > 0)
                {
                    var file = request.Url[i];
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/TicketImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Save relative path
                    ticketImagePath = "/Images/TicketImages/" + fileName;
                }
                else if (request.ExistingUrls != null && request.ExistingUrls.Count > i)
                {
                    // Use the old path if no new file was uploaded
                    ticketImagePath = request.ExistingUrls[i];
                }

                var newFlight = new POFlightDetail
                {
                    POTicketId = newPoInvoice.POTicketId, // Use the new PO Ticket ID
                    TicketCategory = request.TicketCategory[i],
                    SeatNumber = request.SeatNumber[i],
                    TicketNumber = request.TicketNumber[i],
                    POTicketFlightDataId = request.POTicketFlightDataId[i],
                    DepartureAirportId = request.DepartureAirportId[i],
                    ArrivalAirportId = request.ArrivalAirportId[i],
                    FlightDepartureTime = request.FlightDepartureTime[i],
                    FlightArrivalTime = request.FlightArrivalTime[i],
                    PNRNo = request.PNRNo[i],
                    PNRGDSNo = request.PNRGDSNo[i],
                    PreferedMeals = request.PreferedMeals[i],
                    FlightClass = request.FlightClass.ElementAtOrDefault(i),
                    AirportsType = request.AirportsType[i],
                    TicketImages = ticketImagePath,
                    AirlineId = request.Airline[i],
                    ArrivalDate = request.ArrivalDate[i],
                    DepartureDate = request.DepartureDate[i],
                    CityTaxId = request.CityTax[i],
                    TransectTime = request.TransectTime[i],
                    SubClassesId = request.SubClass[i],
                    Amount = request.Amount[i]
                };

                newFlightDetails.Add(newFlight);
            }

            // Add new flight details
            await _dbContext.POFlightDetails.AddRangeAsync(newFlightDetails);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", new { id = newPoInvoice.POTicketId }); // Redirect to the new POTicket's page
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while copying the PO Ticket");
            return StatusCode(500, "Internal server error");
        }
    }

    public async Task<IActionResult> ReIssueForm(int id)
    {
        ViewBag.HeadId = _dbContext.POTicketInvoices
            .Where(e => e.POTicketId == id)
            .Select(e => e.POTicketHeadId)
            .FirstOrDefault();

        ViewBag.POTickedHeadId = id;

        var flight = _dbContext.POTicketFlightDatas.Where(e => !e.Trashed).ToList();
        ViewBag.Flight = flight;

        ViewBag.Companies = _dbContext.Companies.Where(c => !c.Trashed);

        ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead &&
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.FamilyMembers = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Vendor = _dbContext.Vendors.ToList();

        ViewBag.TicketType = Enum.GetValues(typeof(TicketType))
                          .Cast<TicketType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.AirportsType = Enum.GetValues(typeof(AirportsType))
                         .Cast<AirportsType>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

        ViewBag.FareType = Enum.GetValues(typeof(FareType))
                          .Cast<FareType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.CityTax = _dbContext.CityTaxes.Where(e => !e.Trashed);

        ViewBag.PostingType = Enum.GetValues(typeof(PostingType))
                          .Cast<PostingType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.TicketCategory = Enum.GetValues(typeof(TicketCategory))
                          .Cast<TicketCategory>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                          .Cast<FlightClass>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.IssuedType = Enum.GetValues(typeof(IssuedType))
                          .Cast<IssuedType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

        ViewBag.SPOS = _dbContext.SPOs.Where(s => !s.Trashed);

        ViewBag.Airlines = _dbContext.Airlines.Where(a => !a.Trashed);

        ViewBag.SubClasses = _dbContext.SubClasses.Where(e => !e.Trashed).ToList();

        POTicket po = await _dbContext.POTickets.Include(e => e.Vendor).SingleOrDefaultAsync(e => e.POTicketId == id);

        if (po == null)
        {
            return Json(new { msg = "Not Found" });
        }


        ViewBag.FlightDetails = await _dbContext.POFlightDetails
            .Include(d => d.Departure)
            .Include(d => d.Arrival)
            .Include(d => d.SubClasses)
            .Include(d => d.POTicketFlightData)
            .Where(e => e.POTicketId == po.POTicketId)
            .ToListAsync();

        ViewBag.TicketNumber = await _dbContext.POFlightDetails
            .Include(d => d.Departure)
            .Include(d => d.Arrival)
            .Include(d => d.SubClasses)
            .Include(d => d.POTicketFlightData)
            .Where(e => e.POTicketId == po.POTicketId)
            .Select(tn => tn.TicketNumber)
            .FirstOrDefaultAsync();



        // Pehle re-issued tickets ka penalties ka sum nikal lo
        var reIssueTotal = await _dbContext.POTickets
            .Where(x => x.FamilyMembersId == po.FamilyMembersId
                        && x.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && x.POTicketId != po.POTicketId // exclude current ticket
                        && (int)x.IssuedType == 1)            // sirf ReIssue wale
            .SumAsync(x => (float?)(
                (x.RefundPenulty ?? 0) +
                (x.SeatMappingPenulty ?? 0) +
                (x.NoShowPenulty ?? 0) +
                (x.ExtraBaggage ?? 0) +
                (x.ReIssuePenulty ?? 0) +
                (x.DifferenceOfFare ?? 0)
            )) ?? 0;


        POTicketEditModel ticket = new POTicketEditModel
        {
            POTicketId = po.POTicketId,
            PONumber = po.PONumber,
            XONumber = po.XONumber,
            Date = po.Date,
            DueDate = po.DueDate,
            familyHeadId = po.FamilyHeadId,
            TPRFNo = po.TPRFNo,
            TicketType = po.TicketType,
            FareType = po.FareType,
            FamilyMembersId = po.FamilyMembersId,
            PostingType = po.PostingType,
            IssueDate = po.IssueDate,
            CompanyId = po.CompanyId,
            VendorTypeId = po.VendorTypeId,
            VendorId = po.VendorId,
            BookingAgentId = po.BookingAgentId,
            TicketingAgentId = po.TicketingAgentId,
            RecoveryOfficerId = po.RecoveryOfficerId,
            PostingAgentId = po.PostingAgentId,

            CommissionPercentage = po.CommissionPersentage,
            Commission = po.CommissionAmount,
            AgentPersentage = po.AgentPersentage,
            AgentAmount = po.AgentAmount ?? 0,
            AgentFixedAmount = po.AgentFixedAmount,
            IssuedType = po.IssuedType,
            ConersionRate = po.ConersionRate,
            ntax = po.ntax,
            rg = po.RG,
            sf = po.SF,
            sp = po.SP,
            ydAmount = po.ydAmount,
            gtaxAmount = po.gtaxAmount,
            pzAmount = po.pzAmount,
            qaAmount = po.qaAmount,
            rtaxAmount = po.rtaxAmount,
            etaxAmount = po.etaxAmount,
            ioAmount = po.ioAmount,
            yr = po.YR,
            ttax = po.ttax,
            Ftax = po.Ftax,
            Zrtax = po.Zrtax,
            Aetax = po.Aetax,
            Tptax = po.Tptax,
            Catax = po.Catax,
            Sqtax = po.Sqtax,
            Rctax = po.Rctax,
            Xttax = po.Xttax,
            Aytax = po.Aytax,
            Ustax = po.Ustax,
            Xatax = po.Xatax,
            Xftax = po.Xftax,
            Xytax = po.Xytax,
            Yctax = po.Yctax,
            Ubtax = po.Ubtax,
            Gbtax = po.Gbtax,
            svc = po.SVC,
            srb = po.SRB,
            pk = po.PK,
            ptt = po.PTT,
            pb = po.PB,
            yq = po.YQ,
            ReIssuePenulty = po.ReIssuePenulty,
            DifferenceOfFare = po.DifferenceOfFare,
            RefundPenulty = po.RefundPenulty,
            SeatMappingPenulty = po.SeatMappingPenulty,
            NoShowPenulty = po.NoShowPenulty,
            ExtraBaggage = po.ExtraBaggage,
            BasicFare = (po.BasicFare ?? 0),

            OtherTaxes = po.OtherTaxes ?? 0,

            WithHoldingPercentage = po.WithHoldingPers,
            withholding = po.WithHoldingAmount,
            Discount = po.Discount,
            KPR = po.KPR,
            KBP = po.KBP,
            SPTax = po.SPTaxAmount,
            finalkbAmount = po.finalkbAmount,
            SPTaxPercentage = po.SPTaxPers,
            GrossClient = po.GrossClient,
            NetClient = po.NetClient,
            GrossAirline = po.GrossAirline,
            NetAirline = po.NetAirline,
            NetTax = po.NetTax,
            NetTotal = po.NetTotal ?? 0,
            PassportNumber = po.PassportNumber,
            DateofBirth = po.DateofBirth,
            PassportExpiry = po.PassportExpiry,
            PassengerCategory = po.PassengerCategory,
            FrequentFlyer = po.FrequentFlyer,
            ClientFareAmounFromInvoice = po.GrossClientFromInvoice,
            //Discount = po.Discount
        };

        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> ReIssueForm(POTicketEditModel request)
    {
        //return Ok(request);
        if (request == null)
        {
            return BadRequest("Request cannot be null");
        }

        var sessionValue = _http.HttpContext.Session.GetString("ActiveSeasonId");
        if (string.IsNullOrEmpty(sessionValue) || !int.TryParse(sessionValue, out int hajjSeasonId))
        {
            return BadRequest("ActiveSeasonId is missing or invalid.");
        }

        try
        {

            var lastPO = _dbContext.POTickets
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .OrderByDescending(x => x.PONumber)
                .Select(x => x.PONumber)
                .FirstOrDefault();

            int nextPONumber = 1;

            if (!string.IsNullOrEmpty(lastPO) && lastPO.StartsWith("PO-TKT-"))
            {
                var numericPart = lastPO.Substring(7);
                nextPONumber = int.Parse(numericPart) + 1;
            }

            string formattedPONumber = $"PO-TKT-{nextPONumber:D3}";

            // Create a new POTicket (new entry)
            var newPoInvoice = new POTicket
            {
                XONumber = request.XONumber,
                PONumber = request.PONumber,
                Date = request.Date,
                DueDate = request.DueDate,
                TPRFNo = request.TPRFNo,
                TicketType = request.TicketType,
                FareType = request.FareType,
                FamilyHeadId = request.familyHeadId,
                FamilyMembersId = request.FamilyMembersId,
                PostingType = request.PostingType,
                IssueDate = request.IssueDate,
                CompanyId = request.CompanyId,
                VendorTypeId = request.VendorTypeId,
                VendorId = request.VendorId,
                BookingAgentId = request.BookingAgentId,
                TicketingAgentId = request.TicketingAgentId,
                RecoveryOfficerId = request.RecoveryOfficerId,
                PostingAgentId = request.PostingAgentId,
                BasicFare = request.BasicFare ?? 0,
                CommissionPersentage = request.CommissionPercentage ?? 0,
                CommissionAmount = request.Commission ?? 0,
                Discount = request.Discount ?? 0,
                NetTotal = request.NetTotal ?? 0,
                AgentPersentage = request.AgentPersentage ?? 0,
                AgentAmount = request.AgentAmount ?? 0,
                AgentFixedAmount = request.AgentFixedAmount ?? 0,
                IssuedType = request.IssuedType,
                ConersionRate = request.ConersionRate,
                ntax = request.ntax ?? 0f,
                RG = request.rg ?? 0f,
                SF = request.sf ?? 0f,
                SP = request.sp ?? 0f,
                ydAmount = request.ydAmount ?? 0f,
                gtaxAmount = request.gtaxAmount ?? 0f,
                pzAmount = request.pzAmount ?? 0f,
                qaAmount = request.qaAmount ?? 0f,
                rtaxAmount = request.rtaxAmount ?? 0f,
                etaxAmount = request.etaxAmount ?? 0f,
                ioAmount = request.ioAmount ?? 0f,
                YR = request.yr ?? 0f,
                ttax = request.ttax ?? 0f,
                Ftax = request.Ftax ?? 0f,
                Zrtax = request.Zrtax ?? 0f,
                Aetax = request.Aetax ?? 0f,
                Tptax = request.Tptax ?? 0f,
                Catax = request.Catax ?? 0f,
                Sqtax = request.Sqtax ?? 0f,
                Rctax = request.Rctax ?? 0f,
                Xttax = request.Xttax ?? 0f,
                Aytax = request.Aytax ?? 0f,
                Ustax = request.Ustax ?? 0f,
                Xatax = request.Xatax ?? 0f,
                Xftax = request.Xftax ?? 0f,
                Xytax = request.Xytax ?? 0f,
                Yctax = request.Yctax ?? 0f,
                Ubtax = request.Ubtax ?? 0f,
                Gbtax = request.Gbtax ?? 0f,
                SVC = request.svc ?? 0f,
                SRB = request.srb ?? 0f,
                PK = request.pk ?? 0f,
                PTT = request.ptt ?? 0f,
                PB = request.pb ?? 0f,
                YQ = request.yq ?? 0f,
                ReIssuePenulty = request.ReIssuePenulty ?? 0f,
                DifferenceOfFare = request.DifferenceOfFare ?? 0f,
                RefundPenulty = request.RefundPenulty ?? 0f,
                SeatMappingPenulty = request.SeatMappingPenulty ?? 0f,
                NoShowPenulty = request.NoShowPenulty ?? 0f,
                ExtraBaggage = request.NoShowPenulty ?? 0f,
                OtherTaxes = request.OtherTaxes ?? 0f,
                WithHoldingPers = request.WithHoldingPercentage ?? 0f,
                WithHoldingAmount = request.withholding ?? 0f,
                KPR = request.KPR ?? 0f,
                KBP = request.KBP ?? 0f,
                SPTaxPers = request.SPTaxPercentage ?? 0f,
                SPTaxAmount = request.SPTax ?? 0f,
                finalkbAmount = request.finalkbAmount ?? 0f,
                GrossClient = request.GrossClient ?? 0f,
                NetClient = request.NetClient ?? 0f,
                GrossAirline = request.GrossAirline ?? 0f,
                NetTax = request.NetTax,
                PassportNumber = request.PassportNumber,
                PassportExpiry = request.PassportExpiry,
                DateofBirth = request.DateofBirth,
                PassengerCategory = request.PassengerCategory,
                FrequentFlyer = request.FrequentFlyer,
                IsPrimary = false, // First ticket gets true, rest get false
                HajjSeasonsId = hajjSeasonId,
                GrossClientFromInvoice = request.ClientFareAmounFromInvoice ?? 0,
                ReIssuedTicketNumber = request.ReIssuedTicketNumber,
            };

            _dbContext.POTickets.Add(newPoInvoice); // Add the new POTicket entry
            await _dbContext.SaveChangesAsync(); // Save the new POTicket

            // Add a new POTicketInvoices entry for the new POTicket
            var poTicketInvi = new POTicketInvoices
            {
                POTicketHeadId = request.POTicketHeadId,
                POTicketId = newPoInvoice.POTicketId // Use the new PO Ticket ID
            };

            _dbContext.POTicketInvoices.Add(poTicketInvi);
            await _dbContext.SaveChangesAsync(); // Save the new POTicketInvoices

            // Add new flight details from the request
            List<POFlightDetail> newFlightDetails = new List<POFlightDetail>();

            for (int i = 0; i < request.TicketNumber.Count(); i++)
            {
                string ticketImagePath = null;

                if (request.Url != null && request.Url.Count > i && request.Url[i] != null && request.Url[i].Length > 0)
                {
                    var file = request.Url[i];
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/TicketImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Save relative path
                    ticketImagePath = "/Images/TicketImages/" + fileName;
                }
                else if (request.ExistingUrls != null && request.ExistingUrls.Count > i)
                {
                    // Use the old path if no new file was uploaded
                    ticketImagePath = request.ExistingUrls[i];
                }

                var newFlight = new POFlightDetail
                {
                    POTicketId = newPoInvoice.POTicketId, // Use the new PO Ticket ID
                    TicketCategory = request.TicketCategory[i],
                    SeatNumber = request.SeatNumber[i],
                    TicketNumber = request.TicketNumber[i],
                    POTicketFlightDataId = request.POTicketFlightDataId[i],
                    DepartureAirportId = request.DepartureAirportId[i],
                    ArrivalAirportId = request.ArrivalAirportId[i],
                    FlightDepartureTime = request.FlightDepartureTime[i],
                    FlightArrivalTime = request.FlightArrivalTime[i],
                    PNRNo = request.PNRNo[i],
                    PNRGDSNo = request.PNRGDSNo[i],
                    PreferedMeals = request.PreferedMeals[i],
                    FlightClass = request.FlightClass.ElementAtOrDefault(i),
                    AirportsType = request.AirportsType[i],
                    TicketImages = ticketImagePath,
                    AirlineId = request.Airline[i],
                    ArrivalDate = request.ArrivalDate[i],
                    DepartureDate = request.DepartureDate[i],
                    CityTaxId = request.CityTax[i],
                    TransectTime = request.TransectTime[i],
                    SubClassesId = request.SubClass[i],
                    Amount = request.Amount[i]
                };

                newFlightDetails.Add(newFlight);
            }

            // Add new flight details
            await _dbContext.POFlightDetails.AddRangeAsync(newFlightDetails);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", new { id = newPoInvoice.POTicketId }); // Redirect to the new POTicket's page
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while copying the PO Ticket");
            return StatusCode(500, "Internal server error");
        }
    }

    public async Task<IActionResult> NewTicket(int id)
    {
        ViewBag.HeadId = _dbContext.POTicketInvoices
            .Where(e => e.POTicketId == id)
            .Select(e => e.POTicketHeadId)
            .FirstOrDefault();

        var flight = _dbContext.POTicketFlightDatas.Where(e => !e.Trashed).ToList();
        ViewBag.Flight = flight;

        ViewBag.Companies = _dbContext.Companies.Where(c => !c.Trashed);

        ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead &&
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.FamilyMembers = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Vendor = _dbContext.Vendors.ToList();

        ViewBag.TicketType = Enum.GetValues(typeof(TicketType))
                          .Cast<TicketType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.AirportsType = Enum.GetValues(typeof(AirportsType))
                         .Cast<AirportsType>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

        ViewBag.FareType = Enum.GetValues(typeof(FareType))
                          .Cast<FareType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.CityTax = _dbContext.CityTaxes.Where(e => !e.Trashed);

        ViewBag.PostingType = Enum.GetValues(typeof(PostingType))
                          .Cast<PostingType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.TicketCategory = Enum.GetValues(typeof(TicketCategory))
                          .Cast<TicketCategory>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                          .Cast<FlightClass>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.IssuedType = Enum.GetValues(typeof(IssuedType))
                          .Cast<IssuedType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

        ViewBag.SPOS = _dbContext.SPOs.Where(s => !s.Trashed);

        ViewBag.Airlines = _dbContext.Airlines.Where(a => !a.Trashed);

        ViewBag.SubClasses = _dbContext.SubClasses.Where(e => !e.Trashed).ToList();

        POTicket po = await _dbContext.POTickets.Include(e => e.Vendor).SingleOrDefaultAsync(e => e.POTicketId == id);

        if (po == null)
        {
            return Json(new { msg = "Not Found" });
        }


        ViewBag.FlightDetails = await _dbContext.POFlightDetails
            .Include(d => d.Departure)
            .Include(d => d.Arrival)
            .Include(d => d.SubClasses)
            .Include(d => d.POTicketFlightData)
            .Where(e => e.POTicketId == po.POTicketId)
            .ToListAsync();

        POTicketEditModel ticket = new POTicketEditModel
        {
            POTicketId = po.POTicketId,
            PONumber = po.PONumber,
            XONumber = po.XONumber,
            Date = po.Date,
            DueDate = po.DueDate,
            familyHeadId = po.FamilyHeadId,
            TPRFNo = po.TPRFNo,
            TicketType = po.TicketType,
            FareType = po.FareType,
            FamilyMembersId = po.FamilyMembersId,
            PostingType = po.PostingType,
            IssueDate = po.IssueDate,
            CompanyId = po.CompanyId,
            VendorTypeId = po.VendorTypeId,
            VendorId = po.VendorId,
            BookingAgentId = po.BookingAgentId,
            TicketingAgentId = po.TicketingAgentId,
            RecoveryOfficerId = po.RecoveryOfficerId,
            PostingAgentId = po.PostingAgentId,
            BasicFare = po.BasicFare,
            CommissionPercentage = po.CommissionPersentage,
            Commission = po.CommissionAmount,
            AgentPersentage = po.AgentPersentage,
            AgentAmount = po.AgentAmount ?? 0,
            AgentFixedAmount = po.AgentFixedAmount,
            IssuedType = po.IssuedType,

            ntax = po.ntax,
            rg = po.RG,
            sf = po.SF,
            sp = po.SP,
            ydAmount = po.ydAmount,
            gtaxAmount = po.gtaxAmount,
            pzAmount = po.pzAmount,
            qaAmount = po.qaAmount,
            rtaxAmount = po.rtaxAmount,
            etaxAmount = po.etaxAmount,
            ioAmount = po.ioAmount,
            yr = po.YR,
            ttax = po.ttax,
            Ftax = po.Ftax,
            Zrtax = po.Zrtax,
            Aetax = po.Aetax,
            Tptax = po.Tptax,
            Catax = po.Catax,
            Sqtax = po.Sqtax,
            Rctax = po.Rctax,
            Xttax = po.Xttax,
            Aytax = po.Aytax,
            Ustax = po.Ustax,
            Xatax = po.Xatax,
            Xftax = po.Xftax,
            Xytax = po.Xytax,
            Yctax = po.Yctax,
            Ubtax = po.Ubtax,
            Gbtax = po.Gbtax,
            svc = po.SVC,
            srb = po.SRB,
            pk = po.PK,
            ptt = po.PTT,
            pb = po.PB,
            yq = po.YQ,
            //ReIssuePenulty = po.ReIssuePenulty,
            //DifferenceOfFare = po.DifferenceOfFare,
            //RefundPenulty = po.RefundPenulty,
            //SeatMappingPenulty = po.SeatMappingPenulty,
            //NoShowPenulty = po.NoShowPenulty,
            //ExtraBaggage = po.ExtraBaggage,
            OtherTaxes = po.OtherTaxes ?? 0,

            WithHoldingPercentage = po.WithHoldingPers,
            withholding = po.WithHoldingAmount,
            Discount = po.Discount,
            KPR = po.KPR,
            KBP = po.KBP,
            SPTax = po.SPTaxAmount,
            finalkbAmount = po.finalkbAmount,
            SPTaxPercentage = po.SPTaxPers,
            GrossClient = po.GrossClient,
            NetClient = po.NetClient,
            GrossAirline = po.GrossAirline,
            NetAirline = po.NetAirline,
            NetTax = po.NetTax,
            NetTotal = po.NetTotal ?? 0,
            PassportNumber = po.PassportNumber,
            DateofBirth = po.DateofBirth,
            PassportExpiry = po.PassportExpiry,
            PassengerCategory = po.PassengerCategory,
            FrequentFlyer = po.FrequentFlyer,
            ReIssuePenulty = po.ReIssuePenulty ?? 0,
            DifferenceOfFare = po.DifferenceOfFare ?? 0,
            RefundPenulty = po.RefundPenulty ?? 0,
            SeatMappingPenulty = po.SeatMappingPenulty ?? 0,
            NoShowPenulty = po.NoShowPenulty ?? 0,
            ExtraBaggage = po.ExtraBaggage ?? 0,
            //Discount = po.Discount
        };

        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> NewTicket(POTicketEditModel request)
    {
        //return Ok(request);
        if (request == null)
        {
            return BadRequest("Request cannot be null");
        }

        var sessionValue = _http.HttpContext.Session.GetString("ActiveSeasonId");
        if (string.IsNullOrEmpty(sessionValue) || !int.TryParse(sessionValue, out int hajjSeasonId))
        {
            return BadRequest("ActiveSeasonId is missing or invalid.");
        }

        try
        {

            var lastPO = _dbContext.POTickets
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .OrderByDescending(x => x.PONumber)
                .Select(x => x.PONumber)
                .FirstOrDefault();

            int nextPONumber = 1;

            if (!string.IsNullOrEmpty(lastPO) && lastPO.StartsWith("PO-TKT-"))
            {
                var numericPart = lastPO.Substring(7);
                nextPONumber = int.Parse(numericPart) + 1;
            }

            string formattedPONumber = $"PO-TKT-{nextPONumber:D3}";

            // Create a new POTicket (new entry)
            var newPoInvoice = new POTicket
            {
                XONumber = request.XONumber,
                PONumber = formattedPONumber,
                Date = request.Date,
                DueDate = request.DueDate,
                TPRFNo = request.TPRFNo,
                TicketType = request.TicketType,
                FareType = request.FareType,
                FamilyHeadId = request.familyHeadId,
                FamilyMembersId = request.FamilyMembersId,
                PostingType = request.PostingType,
                IssueDate = request.IssueDate,
                CompanyId = request.CompanyId,
                VendorTypeId = request.VendorTypeId,
                VendorId = request.VendorId,
                BookingAgentId = request.BookingAgentId,
                TicketingAgentId = request.TicketingAgentId,
                RecoveryOfficerId = request.RecoveryOfficerId,
                PostingAgentId = request.PostingAgentId,
                BasicFare = request.BasicFare ?? 0,
                CommissionPersentage = request.CommissionPercentage ?? 0,
                CommissionAmount = request.Commission ?? 0,
                Discount = request.Discount ?? 0,
                NetTotal = request.NetTotal ?? 0,
                AgentPersentage = request.AgentPersentage ?? 0,
                AgentAmount = request.AgentAmount ?? 0,
                AgentFixedAmount = request.AgentFixedAmount ?? 0,
                IssuedType = request.IssuedType,

                ntax = request.ntax ?? 0f,
                RG = request.rg ?? 0f,
                SF = request.sf ?? 0f,
                SP = request.sp ?? 0f,
                ydAmount = request.ydAmount ?? 0f,
                gtaxAmount = request.gtaxAmount ?? 0f,
                pzAmount = request.pzAmount ?? 0f,
                qaAmount = request.qaAmount ?? 0f,
                rtaxAmount = request.rtaxAmount ?? 0f,
                etaxAmount = request.etaxAmount ?? 0f,
                ioAmount = request.ioAmount ?? 0f,
                YR = request.yr ?? 0f,
                ttax = request.ttax ?? 0f,
                Ftax = request.Ftax ?? 0f,
                Zrtax = request.Zrtax ?? 0f,
                Aetax = request.Aetax ?? 0f,
                Tptax = request.Tptax ?? 0f,
                Catax = request.Catax ?? 0f,
                Sqtax = request.Sqtax ?? 0f,
                Rctax = request.Rctax ?? 0f,
                Xttax = request.Xttax ?? 0f,
                Aytax = request.Aytax ?? 0f,
                Ustax = request.Ustax ?? 0f,
                Xatax = request.Xatax ?? 0f,
                Xftax = request.Xftax ?? 0f,
                Xytax = request.Xytax ?? 0f,
                Yctax = request.Yctax ?? 0f,
                Ubtax = request.Ubtax ?? 0f,
                Gbtax = request.Gbtax ?? 0f,
                SVC = request.svc ?? 0f,
                SRB = request.srb ?? 0f,
                PK = request.pk ?? 0f,
                PTT = request.ptt ?? 0f,
                PB = request.pb ?? 0f,
                YQ = request.yq ?? 0f,
                ReIssuePenulty = request.ReIssuePenulty ?? 0f,
                DifferenceOfFare = request.DifferenceOfFare ?? 0f,
                RefundPenulty = request.RefundPenulty ?? 0f,
                SeatMappingPenulty = request.SeatMappingPenulty ?? 0f,
                NoShowPenulty = request.NoShowPenulty ?? 0f,
                ExtraBaggage = request.NoShowPenulty ?? 0f,
                OtherTaxes = request.OtherTaxes ?? 0f,
                WithHoldingPers = request.WithHoldingPercentage ?? 0f,
                WithHoldingAmount = request.withholding ?? 0f,
                KPR = request.KPR ?? 0f,
                KBP = request.KBP ?? 0f,
                SPTaxPers = request.SPTaxPercentage ?? 0f,
                SPTaxAmount = request.SPTax ?? 0f,
                finalkbAmount = request.finalkbAmount ?? 0f,
                GrossClient = request.GrossClient ?? 0f,
                NetClient = request.NetClient ?? 0f,
                GrossAirline = request.GrossAirline ?? 0f,
                NetTax = request.NetTax,
                PassportNumber = request.PassportNumber,
                PassportExpiry = request.PassportExpiry,
                DateofBirth = request.DateofBirth,
                PassengerCategory = request.PassengerCategory,
                FrequentFlyer = request.FrequentFlyer,
                IsPrimary = false, // First ticket gets true, rest get false
                HajjSeasonsId = hajjSeasonId
            };

            _dbContext.POTickets.Add(newPoInvoice); // Add the new POTicket entry
            await _dbContext.SaveChangesAsync(); // Save the new POTicket

            // Add a new POTicketInvoices entry for the new POTicket
            var poTicketInvi = new POTicketInvoices
            {
                POTicketHeadId = request.POTicketHeadId,
                POTicketId = newPoInvoice.POTicketId // Use the new PO Ticket ID
            };

            _dbContext.POTicketInvoices.Add(poTicketInvi);
            await _dbContext.SaveChangesAsync(); // Save the new POTicketInvoices

            // Add new flight details from the request
            List<POFlightDetail> newFlightDetails = new List<POFlightDetail>();

            for (int i = 0; i < request.TicketNumber.Count(); i++)
            {
                string ticketImagePath = null;

                if (request.Url != null && request.Url.Count > i && request.Url[i] != null && request.Url[i].Length > 0)
                {
                    var file = request.Url[i];
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/TicketImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Save relative path
                    ticketImagePath = "/Images/TicketImages/" + fileName;
                }
                else if (request.ExistingUrls != null && request.ExistingUrls.Count > i)
                {
                    // Use the old path if no new file was uploaded
                    ticketImagePath = request.ExistingUrls[i];
                }

                var newFlight = new POFlightDetail
                {
                    POTicketId = newPoInvoice.POTicketId, // Use the new PO Ticket ID
                    TicketCategory = request.TicketCategory[i],
                    SeatNumber = request.SeatNumber[i],
                    TicketNumber = request.TicketNumber[i],
                    POTicketFlightDataId = request.POTicketFlightDataId[i],
                    DepartureAirportId = request.DepartureAirportId[i],
                    ArrivalAirportId = request.ArrivalAirportId[i],
                    FlightDepartureTime = request.FlightDepartureTime[i],
                    FlightArrivalTime = request.FlightArrivalTime[i],
                    PNRNo = request.PNRNo[i],
                    PNRGDSNo = request.PNRGDSNo[i],
                    PreferedMeals = request.PreferedMeals[i],
                    FlightClass = request.FlightClass.ElementAtOrDefault(i),
                    AirportsType = request.AirportsType[i],
                    TicketImages = ticketImagePath,
                    AirlineId = request.Airline[i],
                    ArrivalDate = request.ArrivalDate[i],
                    DepartureDate = request.DepartureDate[i],
                    CityTaxId = request.CityTax[i],
                    TransectTime = request.TransectTime[i],
                    SubClassesId = request.SubClass[i],
                    Amount = request.Amount[i]
                };

                newFlightDetails.Add(newFlight);
            }

            // Add new flight details
            await _dbContext.POFlightDetails.AddRangeAsync(newFlightDetails);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", new { id = newPoInvoice.POTicketId }); // Redirect to the new POTicket's page
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while copying the PO Ticket");
            return StatusCode(500, "Internal server error");
        }
    }

    public async Task<IActionResult> RefundForm(int id)
    {
        ViewBag.HeadId = _dbContext.POTicketInvoices
            .Where(e => e.POTicketId == id)
            .Select(e => e.POTicketHeadId)
            .FirstOrDefault();

        ViewBag.POTickedHeadId = id;

        var flight = _dbContext.POTicketFlightDatas.Where(e => !e.Trashed).ToList();
        ViewBag.Flight = flight;

        ViewBag.Companies = _dbContext.Companies.Where(c => !c.Trashed);

        ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead &&
                        e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.FamilyMembers = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Vendor = _dbContext.Vendors.ToList();

        ViewBag.TicketType = Enum.GetValues(typeof(TicketType))
                          .Cast<TicketType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.AirportsType = Enum.GetValues(typeof(AirportsType))
                         .Cast<AirportsType>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

        ViewBag.FareType = Enum.GetValues(typeof(FareType))
                          .Cast<FareType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.CityTax = _dbContext.CityTaxes.Where(e => !e.Trashed);

        ViewBag.PostingType = Enum.GetValues(typeof(PostingType))
                          .Cast<PostingType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.TicketCategory = Enum.GetValues(typeof(TicketCategory))
                          .Cast<TicketCategory>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();

        ViewBag.FlightClass = Enum.GetValues(typeof(FlightClass))
                          .Cast<FlightClass>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.IssuedType = Enum.GetValues(typeof(IssuedType))
                          .Cast<IssuedType>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


        ViewBag.Airline = _dbContext.Airlines.Where(e => !e.Trashed).ToList();

        ViewBag.SPOS = _dbContext.SPOs.Where(s => !s.Trashed);

        ViewBag.Airlines = _dbContext.Airlines.Where(a => !a.Trashed);

        ViewBag.SubClasses = _dbContext.SubClasses.Where(e => !e.Trashed).ToList();

        POTicket po = await _dbContext.POTickets.Include(e => e.Vendor).SingleOrDefaultAsync(e => e.POTicketId == id);

        if (po == null)
        {
            return Json(new { msg = "Not Found" });
        }


        ViewBag.FlightDetails = await _dbContext.POFlightDetails
            .Include(d => d.Departure)
            .Include(d => d.Arrival)
            .Include(d => d.SubClasses)
            .Include(d => d.POTicketFlightData)
            .Where(e => e.POTicketId == po.POTicketId)
            .ToListAsync();


        // First Issued Ticket
        var firstIssued = await _dbContext.POTickets
            .Where(e => e.FamilyMembersId == po.FamilyMembersId && e.IssuedType == IssuedType.Issued && !e.Trashed)
            .OrderBy(e => e.POTicketId) // Sab se pehla record
            .FirstOrDefaultAsync();

        // All ReIssued Tickets
        var reIssuedTickets = await _dbContext.POTickets
            .Where(e => e.FamilyMembersId == po.FamilyMembersId && e.IssuedType == IssuedType.ReIssued && e.TPRFNo == po.TPRFNo && !e.Trashed )
            .ToListAsync();

        // Base Fare = First Issued ka BasicFare
        float finalBasicFare = firstIssued?.BasicFare ?? 0;

        // Add ReIssued ka DifferenceOfFare
        float reIssuedDiff = reIssuedTickets.Sum(r => r.DifferenceOfFare ?? 0);

        finalBasicFare += reIssuedDiff;


        var relatedTickets = await _dbContext.POTickets
            .Where(e => e.FamilyMembersId == po.FamilyMembersId && e.IssuedType != IssuedType.Refund && e.TPRFNo == po.TPRFNo && !e.Trashed)
            .ToListAsync();

        float ntaxTotal = relatedTickets.Sum(t => t.ntax ?? 0);
        float rgTotal = relatedTickets.Sum(t => t.RG ?? 0);
        float sfTotal = relatedTickets.Sum(t => t.SF ?? 0);
        float spTotal = relatedTickets.Sum(t => t.SP ?? 0);
        float ydTotal = relatedTickets.Sum(t => t.ydAmount ?? 0);
        float gtaxTotal = relatedTickets.Sum(t => t.gtaxAmount ?? 0);
        float pzTotal = relatedTickets.Sum(t => t.pzAmount ?? 0);
        float qaTotal = relatedTickets.Sum(t => t.qaAmount ?? 0);
        float rtaxTotal = relatedTickets.Sum(t => t.rtaxAmount ?? 0);
        float etaxTotal = relatedTickets.Sum(t => t.etaxAmount ?? 0);
        float ioTotal = relatedTickets.Sum(t => t.ioAmount ?? 0);
        float yrTotal = relatedTickets.Sum(t => t.YR ?? 0);
        float ttaxTotal = relatedTickets.Sum(t => t.ttax ?? 0);
        float FtaxTotal = relatedTickets.Sum(t => t.Ftax ?? 0);
        float ZrtaxTotal = relatedTickets.Sum(t => t.Zrtax ?? 0);
        float AetaxTotal = relatedTickets.Sum(t => t.Aetax ?? 0);
        float TptaxTotal = relatedTickets.Sum(t => t.Tptax ?? 0);
        float CataxTotal = relatedTickets.Sum(t => t.Catax ?? 0);
        float SqtaxTotal = relatedTickets.Sum(t => t.Sqtax ?? 0);
        float RctaxTotal = relatedTickets.Sum(t => t.Rctax ?? 0);
        float XttaxTotal = relatedTickets.Sum(t => t.Xttax ?? 0);
        float AytaxTotal = relatedTickets.Sum(t => t.Aytax ?? 0);
        float UstaxTotal = relatedTickets.Sum(t => t.Ustax ?? 0);
        float XataxTotal = relatedTickets.Sum(t => t.Xatax ?? 0);
        float XftaxTotal = relatedTickets.Sum(t => t.Xftax ?? 0);
        float XytaxTotal = relatedTickets.Sum(t => t.Xytax ?? 0);
        float YctaxTotal = relatedTickets.Sum(t => t.Yctax ?? 0);
        float UbtaxTotal = relatedTickets.Sum(t => t.Ubtax ?? 0);
        float GbtaxTotal = relatedTickets.Sum(t => t.Gbtax ?? 0);
        float svcTotal = relatedTickets.Sum(t => t.SVC ?? 0);
        float srbTotal = relatedTickets.Sum(t => t.SRB ?? 0);
        float pkTotal = relatedTickets.Sum(t => t.PK ?? 0);
        float pttTotal = relatedTickets.Sum(t => t.PTT ?? 0);
        float pbTotal = relatedTickets.Sum(t => t.PB ?? 0);
        float yqTotal = relatedTickets.Sum(t => t.YQ ?? 0);
        float othertTaxes = relatedTickets.Sum(t => t.OtherTaxes ?? 0);
        float kprTaxes = relatedTickets.Sum(t => t.KPR ?? 0);
        float kbpTaxes = relatedTickets.Sum(t => t.KBP ?? 0);
        float finalkbpAmount = relatedTickets.Sum(t => t.finalkbAmount ?? 0);
        float NetTaxes = relatedTickets.Sum(t => t.NetTax);

        POTicketEditModel ticket = new POTicketEditModel
        {
            POTicketId = po.POTicketId,
            PONumber = po.PONumber,
            XONumber = po.XONumber,
            Date = po.Date,
            DueDate = po.DueDate,
            familyHeadId = po.FamilyHeadId,
            TPRFNo = po.TPRFNo,
            TicketType = po.TicketType,
            FareType = po.FareType,
            FamilyMembersId = po.FamilyMembersId,
            PostingType = po.PostingType,
            IssueDate = po.IssueDate,
            CompanyId = po.CompanyId,
            VendorTypeId = po.VendorTypeId,
            VendorId = po.VendorId,
            BookingAgentId = po.BookingAgentId,
            IssuedType = po.IssuedType,
            TicketingAgentId = po.TicketingAgentId,
            RecoveryOfficerId = po.RecoveryOfficerId,
            PostingAgentId = po.PostingAgentId,
            BasicFare = finalBasicFare,
            Discount = po.Discount,
            CommissionPercentage = po.CommissionPersentage,
            Commission = po.CommissionAmount,
            AgentPersentage = po.AgentPersentage,
            AgentAmount = po.AgentAmount ?? 0,
            AgentFixedAmount = po.AgentFixedAmount,
            NetTotal = po.NetTotal ?? 0,

            //Penulty
            ReIssuePenulty = po.ReIssuePenulty,
            DifferenceOfFare = po.DifferenceOfFare,
            RefundPenulty = po.RefundPenulty,
            SeatMappingPenulty = po.SeatMappingPenulty,
            NoShowPenulty = po.NoShowPenulty,
            ExtraBaggage = po.ExtraBaggage,

            //Taxes
            ntax = ntaxTotal,
            rg = rgTotal,
            sf = sfTotal,
            sp = spTotal,
            ydAmount = ydTotal,
            gtaxAmount = gtaxTotal,
            pzAmount = pzTotal,
            qaAmount = qaTotal,
            rtaxAmount = rtaxTotal,
            etaxAmount = etaxTotal,
            ioAmount = ioTotal,
            yr = yrTotal,
            ttax = ttaxTotal,
            Ftax = FtaxTotal,
            Zrtax = ZrtaxTotal,
            Aetax = AetaxTotal,
            Tptax = TptaxTotal,
            Catax = CataxTotal,
            Sqtax = SqtaxTotal,
            Rctax = RctaxTotal,
            Xttax = XttaxTotal,
            Aytax = AytaxTotal,
            Ustax = UstaxTotal,
            Xatax = XataxTotal,
            Xftax = XftaxTotal,
            Xytax = XytaxTotal,
            Yctax = YctaxTotal,
            Ubtax = UbtaxTotal,
            Gbtax = GbtaxTotal,
            svc = po.SVC,
            srb = srbTotal,
            pk = pkTotal,
            ptt = pttTotal,
            pb = pbTotal,
            yq = yqTotal,
            OtherTaxes = othertTaxes,
            WithHoldingPercentage = po.WithHoldingPers,
            withholding = po.WithHoldingAmount,
            NetTax = NetTaxes,


            KPR = kprTaxes,
            KBP = kbpTaxes,
            SPTax = po.SPTaxAmount,
            SPTaxPercentage = po.SPTaxPers,
            finalkbAmount = finalkbpAmount,

            //Total
            GrossClient = po.GrossClient,
            GrossAirline = po.GrossAirline,
            NetAirline = po.NetAirline,
            NetClient = po.NetClient,

            PassportNumber = po.PassportNumber,
            DateofBirth = po.DateofBirth,
            PassportExpiry = po.PassportExpiry,
            PassengerCategory = po.PassengerCategory,
            FrequentFlyer = po.FrequentFlyer,
            ConersionRate = po.ConersionRate,
            ClientFareAmounFromInvoice = po.GrossClientFromInvoice,
            //Discount = po.Discount
        };

        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> RefundForm(POTicketEditModel request)
    {
        //return Ok(request);
        if (request == null)
        {
            return BadRequest("Request cannot be null");
        }

        var sessionValue = _http.HttpContext.Session.GetString("ActiveSeasonId");
        if (string.IsNullOrEmpty(sessionValue) || !int.TryParse(sessionValue, out int hajjSeasonId))
        {
            return BadRequest("ActiveSeasonId is missing or invalid.");
        }

        try
        {

            // Create a new POTicket (new entry)
            var newPoInvoice = new POTicket
            {
                XONumber = request.XONumber,
                PONumber = request.PONumber,
                Date = request.Date,
                DueDate = request.DueDate,
                TPRFNo = request.TPRFNo,
                TicketType = request.TicketType,
                FareType = request.FareType,
                FamilyHeadId = request.familyHeadId,
                FamilyMembersId = request.FamilyMembersId,
                PostingType = request.PostingType,
                IssueDate = request.IssueDate,
                CompanyId = request.CompanyId,
                VendorTypeId = request.VendorTypeId,
                VendorId = request.VendorId,
                BookingAgentId = request.BookingAgentId,
                TicketingAgentId = request.TicketingAgentId,
                RecoveryOfficerId = request.RecoveryOfficerId,
                PostingAgentId = request.PostingAgentId,
                BasicFare = request.BasicFare ?? 0,
                CommissionPersentage = request.CommissionPercentage ?? 0,
                CommissionAmount = request.Commission ?? 0,
                Discount = request.Discount ?? 0,
                NetTotal = request.NetTotal ?? 0,
                AgentPersentage = request.AgentPersentage ?? 0,
                AgentAmount = request.AgentAmount ?? 0,
                AgentFixedAmount = request.AgentFixedAmount ?? 0,
                IssuedType = request.IssuedType,
                ConersionRate = request.ConersionRate ?? 0,
                ntax = request.ntax ?? 0f,
                RG = request.rg ?? 0f,
                SF = request.sf ?? 0f,
                SP = request.sp ?? 0f,
                ydAmount = request.ydAmount ?? 0f,
                gtaxAmount = request.gtaxAmount ?? 0f,
                pzAmount = request.pzAmount ?? 0f,
                qaAmount = request.qaAmount ?? 0f,
                rtaxAmount = request.rtaxAmount ?? 0f,
                etaxAmount = request.etaxAmount ?? 0f,
                ioAmount = request.ioAmount ?? 0f,
                YR = request.yr ?? 0f,
                ttax = request.ttax ?? 0f,
                Ftax = request.Ftax ?? 0f,
                Zrtax = request.Zrtax ?? 0f,
                Aetax = request.Aetax ?? 0f,
                Tptax = request.Tptax ?? 0f,
                Catax = request.Catax ?? 0f,
                Sqtax = request.Sqtax ?? 0f,
                Rctax = request.Rctax ?? 0f,
                Xttax = request.Xttax ?? 0f,
                Aytax = request.Aytax ?? 0f,
                Ustax = request.Ustax ?? 0f,
                Xatax = request.Xatax ?? 0f,
                Xftax = request.Xftax ?? 0f,
                Xytax = request.Xytax ?? 0f,
                Yctax = request.Yctax ?? 0f,
                Ubtax = request.Ubtax ?? 0f,
                Gbtax = request.Gbtax ?? 0f,
                SVC = request.svc ?? 0f,
                SRB = request.srb ?? 0f,
                PK = request.pk ?? 0f,
                PTT = request.ptt ?? 0f,
                PB = request.pb ?? 0f,
                YQ = request.yq ?? 0f,
                ReIssuePenulty = request.ReIssuePenulty ?? 0f,
                DifferenceOfFare = request.DifferenceOfFare ?? 0f,
                RefundPenulty = request.RefundPenulty ?? 0f,
                NetTaxesForRefund = request.NetTaxesForRefund ?? 0f,
                SeatMappingPenulty = request.SeatMappingPenulty ?? 0f,
                NoShowPenulty = request.NoShowPenulty ?? 0f,
                ExtraBaggage = request.NoShowPenulty ?? 0f,
                OtherTaxes = request.OtherTaxes ?? 0f,
                WithHoldingPers = request.WithHoldingPercentage ?? 0f,
                WithHoldingAmount = request.withholding ?? 0f,
                KPR = request.KPR ?? 0f,
                KBP = request.KBP ?? 0f,
                SPTaxPers = request.SPTaxPercentage ?? 0f,
                SPTaxAmount = request.SPTax ?? 0f,
                finalkbAmount = request.finalkbAmount ?? 0f,
                GrossClient = request.GrossClient ?? 0f,
                NetClient = request.NetClient ?? 0f,
                GrossAirline = request.GrossAirline ?? 0f,
                NetTax = request.NetTax,
                PassportNumber = request.PassportNumber,
                PassportExpiry = request.PassportExpiry,
                DateofBirth = request.DateofBirth,
                PassengerCategory = request.PassengerCategory,
                FrequentFlyer = request.FrequentFlyer,
                GrossClientFromInvoice = request.ClientFareAmounFromInvoice ?? 0,
                IsPrimary = false, // First ticket gets true, rest get false
                HajjSeasonsId = hajjSeasonId
            };

            _dbContext.POTickets.Add(newPoInvoice); // Add the new POTicket entry
            await _dbContext.SaveChangesAsync(); // Save the new POTicket

            // Add a new POTicketInvoices entry for the new POTicket
            var poTicketInvi = new POTicketInvoices
            {
                POTicketHeadId = request.POTicketHeadId,
                POTicketId = newPoInvoice.POTicketId // Use the new PO Ticket ID
            };

            _dbContext.POTicketInvoices.Add(poTicketInvi);
            await _dbContext.SaveChangesAsync(); // Save the new POTicketInvoices

            // Add new flight details from the request
            List<POFlightDetail> newFlightDetails = new List<POFlightDetail>();

            for (int i = 0; i < request.TicketNumber.Count(); i++)
            {
                string ticketImagePath = null;

                if (request.Url != null && request.Url.Count > i && request.Url[i] != null && request.Url[i].Length > 0)
                {
                    var file = request.Url[i];
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/TicketImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Save relative path
                    ticketImagePath = "/Images/TicketImages/" + fileName;
                }
                else if (request.ExistingUrls != null && request.ExistingUrls.Count > i)
                {
                    // Use the old path if no new file was uploaded
                    ticketImagePath = request.ExistingUrls[i];
                }

                var newFlight = new POFlightDetail
                {
                    POTicketId = newPoInvoice.POTicketId, // Use the new PO Ticket ID
                    TicketCategory = request.TicketCategory[i],
                    SeatNumber = request.SeatNumber[i],
                    TicketNumber = request.TicketNumber[i],
                    POTicketFlightDataId = request.POTicketFlightDataId[i],
                    DepartureAirportId = request.DepartureAirportId[i],
                    ArrivalAirportId = request.ArrivalAirportId[i],
                    FlightDepartureTime = request.FlightDepartureTime[i],
                    FlightArrivalTime = request.FlightArrivalTime[i],
                    PNRNo = request.PNRNo[i],
                    PNRGDSNo = request.PNRGDSNo[i],
                    PreferedMeals = request.PreferedMeals[i],
                    FlightClass = request.FlightClass.ElementAtOrDefault(i),
                    AirportsType = request.AirportsType[i],
                    TicketImages = ticketImagePath,
                    AirlineId = request.Airline[i],
                    ArrivalDate = request.ArrivalDate[i],
                    DepartureDate = request.DepartureDate[i],
                    CityTaxId = request.CityTax[i],
                    TransectTime = request.TransectTime[i],
                    SubClassesId = request.SubClass[i],
                    Amount = request.Amount[i]
                };

                newFlightDetails.Add(newFlight);
            }

            // Add new flight details
            await _dbContext.POFlightDetails.AddRangeAsync(newFlightDetails);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", new { id = newPoInvoice.POTicketId }); // Redirect to the new POTicket's page
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while copying the PO Ticket");
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPost]
    public JsonResult UpdateAirportType(int poFlightDetailId, int? airportType)
    {
        try
        {
            var flightDetail = _dbContext.POFlightDetails.FirstOrDefault(x => x.POFlightDetailId == poFlightDetailId);
            if (flightDetail != null)
            {
                // If airportType is null, reset the value
                flightDetail.AirportsType = (AirportsType?)airportType;
                _dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Flight detail not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


    public IActionResult GetFlightDetails(int flightId)
    {
        var familyHead = _dbContext.FamilyHeads.Find(flightId);

        var flightData = _dbContext.POTicketFlightDatas
            .Where(f => f.POTicketFlightDataId == flightId)
            .Select(f => new
            {
                ArrivalId = f.Arrival != null ? f.Arrival.AirportId : 0,
                DepartureId = f.Departure != null ? f.Departure.AirportId : 0,
                flightNumber = f.FlightNumber,
                departure = f.Departure != null ? f.Departure.AirportName : null,
                arrival = f.Arrival != null ? f.Arrival.AirportName : null,
                departureTime = f.DepartureTime.ToString("HH:mm"),
                arrivalTime = f.ArrivalTime.ToString("HH:mm"),
                ArrivalCode = f.Arrival != null ? f.Arrival.Code : null,
                DepatureCode = f.Departure != null ? f.Departure.Code : null,
                AirlineName = f.Airline != null ? f.Airline.Name : null,
                AirlineIds = f.Airline != null ? f.Airline.AirlineId : 0,
                Passenger = familyHead != null ? (
                                familyHead.IsAdult ? "Adult" :
                                familyHead.IsChild ? "Child" :
                                familyHead.IsInfant ? "Infant" : "") : "Unknown",
            })
            .FirstOrDefault();

        if (flightData == null)
        {
            return NotFound(new { message = "Flight not found." });
        }

        return Json(flightData);
    }


    [HttpGet]
    public IActionResult GetMemberData(int id)
    {
        var member = _dbContext.FamilyHeads
            .Include(e => e.FFNumbers)
            .Where(f => f.FamilyHeadId == id)
            .Select(f => new
            {
                Passenger = f.IsAdult ? "Adult" : f.IsChild ? "Child" : f.IsInfant ? "Infant" : "",
                PassportNumber = f.PassportNumber,
                DateOfBirth = f.DateOfBirth,
                PassportExpiry = f.PassportDateOfExpiry,
                FrequentFlyer = f.FFNumbers.FirstOrDefault().FFNumber
            })
            .FirstOrDefault();

        if (member == null)
        {
            return NotFound(new { message = "Member not found." });
        }

        return Json(member);
    }

    public JsonResult GetFamilyMembers(int id)
    {
        var member = _dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == id)
            .Select(e => new MemberDetail
            {
                FamilyHeadId = e.FamilyHead.FamilyHeadId,
                HajjiName = e.FamilyHead.FullName,
                HbNo = e.FamilyHead.FcId,
                Currency = e.FamilyHead.Currencies.ToString()
            })
            .ToList();

        return Json(member);
    }

    public async Task<IActionResult> Print(int id)
    {
        var result = from pti in _dbContext.POTicketInvoices
                     join pt in _dbContext.POTickets on pti.POTicketId equals pt.POTicketId
                     join f in _dbContext.POFlightDetails on pti.POTicketId equals f.POTicketId
                     join fm in _dbContext.FamilyMembers on pt.FamilyMembersId equals fm.FamilyHeadId into fmGroup
                     from fm in fmGroup.DefaultIfEmpty()  // Left join

                     join fh in _dbContext.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId into fhGroup
                     from fh in fhGroup.DefaultIfEmpty()  // Left join

                     join v in _dbContext.Vendors on pt.VendorId equals v.VendorId into vGroup
                     from v in vGroup.DefaultIfEmpty()  // Left join

                     join dep in _dbContext.Airports on f.DepartureAirportId equals dep.AirportId into depGroup
                     from dep in depGroup.DefaultIfEmpty()

                     join arr in _dbContext.Airports on f.ArrivalAirportId equals arr.AirportId into arrGroup
                     from arr in arrGroup.DefaultIfEmpty()

                     join airline in _dbContext.Airlines on f.AirlineId equals airline.AirlineId into airlineGroup
                     from airline in airlineGroup.DefaultIfEmpty()

                     join subClass in _dbContext.SubClasses on f.SubClassesId equals subClass.SubClassesId into subClassGroup
                     from subClass in subClassGroup.DefaultIfEmpty()

                     join flight in _dbContext.POTicketFlightDatas on f.POTicketFlightDataId equals flight.POTicketFlightDataId into flightGroup
                     from flight in flightGroup.DefaultIfEmpty()

                     where pti.POTicketHeadId == id && (pt == null || !pt.Trashed)

                     select new
                     {
                         pt.PONumber,
                         pt.Date,
                         pt.XONumber,
                         v.VendorName,
                         v.AccountNumber,
                         v.VATNo,
                         pt.TicketType,
                         pt.FareType,
                         pt.NetAirline,
                         fm.FamilyHeadId,
                         f.POTicketFlightDataId,
                         f.SeatNumber,
                         DepartureAirportName = dep.Code,
                         ArrivalAirportName = arr.Code,
                         AirlineName = airline.Name,
                         SubClassName = subClass.Name,
                         ClientName = fh.FullName ?? "N/A",
                         HbNo = fh.FcId ?? "N/A",
                         FlightNumber = flight.FlightNumber,
                         TicketNumber = f.TicketNumber,
                         f.FlightClass,
                         f.FlightDepartureTime,
                         f.DepartureDate,
                         f.ArrivalDate,
                         f.FlightArrivalTime,
                         f.PNRNo,
                         pt.CommissionAmount,
                         pt.SVC,
                         pt.WithHoldingAmount,
                         pt.GrossAirline,
                         pt.BasicFare,
                         pt.IssueDate,
                         pt.SPTaxAmount,
                         pt.SP,
                         pt.RG,
                         pt.SF,
                         pt.SRB,
                         pt.PK,
                         pt.PTT,
                         pt.PB,
                         pt.YQ,
                         pt.YR,
                         pt.KPR,
                         pt.KBP,
                         pt.GrossClient,
                         pt.NetClient,
                     };

        var resultList = await result.ToListAsync();

        var viewModel = new POTicketViewModel
        {
            PoInvoice = resultList.Select(r => new POTicketInvoice
            {
                PONumber = r.PONumber,
                Date = r.Date,
                XONumber = r.XONumber,
                Vendor = new Vendor
                {
                    VendorName = r.VendorName,
                    AccountNumber = r.AccountNumber,
                    VATNo = r.VATNo
                },
                TicketType = r.TicketType,
                FareType = r.FareType,
                //NetAirline = r.NetAirline,
            }).ToList(),
            POFlightDetails = resultList.Select(r => new POFlightDetailVM
            {
                POTicketFlightDataId = r.POTicketFlightDataId,
                SeatNumber = r.SeatNumber,
                DepartureAirportName = r.DepartureAirportName,
                ArrivalAirportName = r.ArrivalAirportName,
                FlightClass = r.FlightClass,
                SubClassName = r.SubClassName,
                FlightDepartureTime = r.FlightDepartureTime,
                FlightArrivalTime = r.FlightArrivalTime,
                AirlineName = r.AirlineName,
                ClientName = r.ClientName,
                FlightNumber = r.FlightNumber,
                TicketNumber = r.TicketNumber,
                WTH = r.WithHoldingAmount,
                Comission = r.CommissionAmount,
                SPTaxAm = r.SPTaxAmount,
                SVC = r.SVC,
                PNRNo = r.PNRNo,
                NetAirline = r.NetAirline,
                DepDate = r.DepartureDate,
                ArrivalDate = r.ArrivalDate,
                IssueDate = r.IssueDate?.ToString("dd/MMM/yyyy"),
                HbNo = r.HbNo,
                BasicFare = r.BasicFare,
                GrossAirline = r.GrossAirline,
                GrossClient = r.GrossClient,
                NetClient = r.NetClient,
                SP = r.SP,
                RG = r.RG,
                SF = r.SF,
                SRB = r.SRB,
                PK = r.PK,
                PTT = r.PTT,
                PB = r.PB,
                YQ = r.YQ,
                YR = r.YR,
                KPR = r.KPR,
                KBP = r.KBP
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult UploadImage(IFormFile file, int flightId)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        // Define upload path
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/TicketImages");

        // Ensure directory exists
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generate unique file name
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file to server
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        // Save relative path to database
        var ticketImagePath = "/Images/TicketImages/" + fileName;

        // Find flight detail by ID
        var flightDetail = _dbContext.POFlightDetails.FirstOrDefault(f => f.POFlightDetailId == flightId);
        if (flightDetail != null)
        {
            flightDetail.TicketImages = ticketImagePath;
            _dbContext.SaveChanges();
        }
        else
        {
            return Json(new { success = false, message = "Flight record not found!" });
        }

        return Json(new { success = true, filePath = ticketImagePath });
    }

    public IActionResult ViewTrash()
    {
        return View(_dbContext.POTickets
                        .Where(
                                e => e.Trashed == true
                                && e.IsPrimary
                                && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        )
                        .ToList()
                    );
    }

    public IActionResult Trash(int id)
    {
        var poticket = _dbContext.POTicketInvoices
                .Join(_dbContext.POTickets,
                      pti => pti.POTicketId,
                      pt => pt.POTicketId,
                      (pti, pt) => new { pti, pt })
                .Where(x => x.pti.POTicketHeadId == id)
                .ToList();


        if (poticket == null)
            return RedirectToAction(nameof(Index));

        foreach (var ticket in poticket)
        {
            ticket.pt.Trashed = true;
        }

        /*foreach (var flight in poticket.FlightDetails)
        {
            flight.Trashed = true;
        }*/

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Restore(int id)
    {
        var poticket = _dbContext.POTickets
            .Include(e => e.FlightDetails)
            .FirstOrDefault(e => e.POTicketId == id);

        if (poticket == null)
            return RedirectToAction(nameof(Index));

        poticket.Trashed = false;

        foreach (var flight in poticket.FlightDetails)
        {
            flight.Trashed = false;
        }

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult ViewTrashTicket(int id)
    {
        var memberIds = _dbContext
                .POTicketInvoices.Where(e => e.POTicketHeadId == id)
                .Select(e => e.POTicketId)
                .ToList();
        return View(
            _dbContext. POTickets.Where(s =>
                memberIds.Contains(s.POTicketId)
                && s.Trashed
                && s.HajjSeasonsId
                    == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
            )
        );

    }

    public IActionResult TrashTicket(int id)
    {
        var poticket = _dbContext.POTickets
            .Where(p => p.POTicketId == id)
            .FirstOrDefault();


        if (poticket == null)
            return RedirectToAction(nameof(Index));

        poticket.Trashed = true;

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult RestoreTicket(int id)
    {
        var poticket = _dbContext.POTickets
            .Where(p => p.POTicketId == id)
            .FirstOrDefault();


        if (poticket == null)
            return RedirectToAction(nameof(Index));

        poticket.Trashed = false;

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult GetGrossClientAmount(int memberId)
    {
        var member = _dbContext.FamilyHeads
            .FirstOrDefault(f => f.FamilyHeadId == memberId);

        if (member == null)
            return Json(new { amount = 0 });

        float amount = 0;

        if (member.FlightAsPerPackage)
        {
            // Agar FlightAsPerPackage true hai → PackageFlights ka SUM
            amount = _dbContext.PackageFlights
                .Where(pf => pf.PackageId == member.PackageId)
                .Sum(pf => (float?)pf.TicketAmount) ?? 0;
        }
        else
        {
            // Agar FlightAsPerPackage false hai → FamilyHeadFlights ka SUM
            amount = _dbContext.FamilyHeadFlight
                .Where(fh => fh.FamilyHeadId == memberId && fh.TicketStatus == 0)
                .Sum(fh => (float?)fh.FareAmount) ?? 0;
        }

        return Json(new { amount });
    }


}

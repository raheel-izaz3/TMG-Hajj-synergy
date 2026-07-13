using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AirlineModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AirlineControllers
{
    public class AirlineController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public AirlineController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _http = http;
            _host = host;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Airlines.Where(ca => ca.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashAirlines(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateAirlines(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditAirlines(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreAirlines(_dbContext, _http);

            return View(_dbContext.Airlines.Where(ca => !ca.Trashed));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Airline request)
        {
            if (!ModelState.IsValid) return View(request);

            string saveLogo = "";
            if (request.AirlineLogoPic is not null)
            {
                saveLogo = Path.Combine(_host.WebRootPath, "Images", "AirlineLogo", request.AirlineLogoPic.FileName);
                if (!string.IsNullOrEmpty(saveLogo)) { string dir_dt = Path.GetDirectoryName(saveLogo); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (FileStream fs = new FileStream(saveLogo, FileMode.Create))
                {
                    request.AirlineLogoPic.CopyTo(fs);
                }
                request.AirlineLogo = "Images/AirlineLogo/" + request.AirlineLogoPic.FileName;
            }
            else
            {
                request.AirlineLogo = "";
            }



            _dbContext.Airlines.Add(request);
            _dbContext.SaveChanges();

            TempData["created"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Airline airline = _dbContext.Airlines.Find(id);
            if (airline == null) return RedirectToAction(nameof(Index));

            return View(airline);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Airline request)
        {
            if (!ModelState.IsValid) return View(request);

            Airline airline = _dbContext.Airlines.Find(request.AirlineId);

            airline.Name = request.Name;
            airline.Code = request.Code;
            airline.Country = request.Country;
            airline.CallSign = request.CallSign;
            airline.IATACode = request.IATACode;
            airline.ICAOCode = request.ICAOCode;
            airline.FFNumber = request.FFNumber;

            _dbContext.SaveChanges();

            TempData["updated"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            Airline airline = _dbContext.Airlines.Find(id);
            if (airline == null) return RedirectToAction(nameof(Index));

            airline.Trashed = true;
            _dbContext.SaveChanges();

            TempData["deleted"] = true;
            return RedirectToAction(nameof(Index));

        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Airlines.Where(ca => ca.Trashed));
        }

        public IActionResult Restore(int id)
        {
            Airline airline = _dbContext.Airlines.Find(id);
            airline.Trashed = false;
            _dbContext.SaveChanges();

            TempData["restored"] = true;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary(int id)
        {
            Airline airline = _dbContext.Airlines.Find(id);

            var sectors = _dbContext.Sectors
                .Include(e => e.Flight)
                .Where(e => e.Flight.AirlineId == id)
                .ToList();

            var pnrs = _dbContext.PNRs
                .Include(e => e.Flight)
                .Where(e => e.Flight.AirlineId == id)
                .ToList();

            var pnrMembers = _dbContext.PNRMembers
                .Include(e => e.PNR)
                .ThenInclude(e => e.Flight)
                .ThenInclude(e => e.Airline)
                .Where(e => e.PNR.Flight.AirlineId == id)
                .Select(e => new 
                {
                    e.PNR.Flight.AirlineId,
                    e.FamilyHeadId,
                    e.PNRId
                });

            var packages = _dbContext.PackagesWithoutTrash
                .Include(e => e.Company)
                .Select(e => new
                {
                    e.Name, e.Company.CompanyName, e.Company.CompanyCode
                }) 
                .ToList();

            var flightPackages = _dbContext.FlightPackages
                .Include(e => e.Flight)
                .Include(e => e.Package)
                .ThenInclude(e => e.Company)
                .Select(e => new
                {
                    e.Package.Name,
                    e.Package.Company.CompanyCode,
                    e.Package.Company.CompanyName,
                    e.FlightId
                })
                .ToList();


            AirlineSummaryModel model = new();
           
            if(sectors.Count() > 0)
            {
                int highestSectorCount = sectors
                    .GroupBy(e => e.FlightId)
                    .OrderByDescending(e => e.Count()).ElementAt(0).Count();

            string tableHeader = "";

            tableHeader += $"<th>#</th>" +
                $"<th>PNR #</th>" +
                $"<th>PNR Name</th>" +
                $"<th>Class</th>" +
                $"<th>Company</th>" +
                $"<th>Package</th>" +
                $"<th>Airline</th>" +
                $"<th>Sector #1</th>" +
                $"<th>Flight #</th>" +
                $"<th>FlightNo</th>" +
                $"<th>Departure Date</th>" +
                $"<th>Departure Time</th>" +
                $"<th>Arrival Date</th>" +
                $"<th>Arrival Time</th>" +
                $"<th>PNR Capacity</th>" +
                $"<th>PNR Booked</th>" +
                $"<th>PNR Balance</th>";

            for(int i = 1; i < highestSectorCount; i++)
            {
                tableHeader += $"<th>Sector #{i + 1}</th>" +
                $"<th>Flight #</th>" +
                $"<th>FlightNo</th>" +
                $"<th>Departure Date</th>" +
                $"<th>Departure Time</th>";
            }

            model.TableHeader = tableHeader;
            model.Airline = airline.Name;
            model.HighestSectorCount = highestSectorCount;

            for(int i = 0; i < pnrs.Count; i++)
            {
                AirlineSummaryEntryModel entry = new();

                PNR currPnr = pnrs.ElementAt(i);

                List<string> pnrPackages = flightPackages
                    .Where(e => e.FlightId == currPnr.FlightId)
                    .Select(e => e.Name)
                    .Distinct()
                    .ToList();

                List<string> pnrPackageCompanyCodes = flightPackages
                    .Where(e => e.FlightId == currPnr.FlightId)
                    .Select(e => e.CompanyCode)
                    .Distinct()
                    .ToList();

                var currPnrFlightSectors = sectors
                    .Where(e => e.FlightId == currPnr.FlightId)
                    .ToList();

                var firstSector = currPnrFlightSectors.ElementAt(0);

                string packageString = string.Join(", ", pnrPackages);
                string companyString = string.Join(", ", pnrPackageCompanyCodes);


                int booked = pnrMembers.Where(e => e.PNRId == currPnr.PNRId).Count();
                int balance = currPnr.Capacity - booked;

                entry.FlightId = currPnr.FlightId;
                entry.SNo = i + 1;
                entry.PNRNumber = currPnr.Name;
                entry.PNRName = "";
                entry.Class = currPnr.Type.ToString().Substring(0, 3).ToUpper();
                entry.Company = companyString;
                entry.Package = packageString;
                entry.Airline = airline.Code;
                
                entry.FirstSectorFlightNumber = firstSector.FlightType == FlightType.Inbound ? 
                    currPnr.Flight.InboundFlightNumber : currPnr.Flight.OutboundFlightNumber;

                entry.FirstSectorDestination = firstSector.Destination;
                entry.FirstSectorFlightNo = firstSector.FlightNo;
                entry.FirstSectorDepartureDate = DateTimeManager.GetDate(firstSector.DepartureDateTime);
                entry.FirstSectorDepartureTime = firstSector.DepartureDateTime?.ToString("HH:mm");

                entry.FirstSectorArrivalDate = DateTimeManager.GetDate(firstSector.ArrivalDateTime);
                entry.FirstSectorArrivalTime = firstSector.ArrivalDateTime?.ToString("HH:mm");
                
                entry.Capacity = currPnr.Capacity;
                entry.Booked = booked;
                entry.Balance = balance;

                for(int j = 1; j < highestSectorCount; j++)
                {
                    Sector s = currPnrFlightSectors.ElementAtOrDefault(j);

                    var sectorModel = new AirlineSummarySectorModel
                    {
                        Destination = s?.Destination,                      
                        Date = DateTimeManager.GetDate(s?.DepartureDateTime),
                        Time = s?.DepartureDateTime?.ToString("HH:mm"),
                        FlightNo = s?.FlightNo,
                    };

                    if (s is null)
                    {
                        sectorModel.FlightNumber = "";
                    }
                    else{
                        sectorModel.FlightNumber = s?.FlightType == FlightType.Inbound ?
                                currPnr.Flight.InboundFlightNumber : currPnr.Flight.OutboundFlightNumber;
                    }

                    entry.Sectors.Add(sectorModel);

                }

                model.Entries.Add(entry);
            }

            //return Ok(model);
            return View(model);
            }
            else
            {
                return View(model);
            }
        }


        public IActionResult ChangeLogo(int id) => View(_dbContext.Airlines.Find(id));

        [HttpPost]
        public IActionResult ChangeLogo(int AirlineId, IFormFile File)
        {

            Airline _airline = _dbContext.Airlines.Find(AirlineId);

            string fileName = Path.GetFileNameWithoutExtension(File.FileName);
            string extension = Path.GetExtension(File.FileName);
            string randomFileName = fileName + "_" + Guid.NewGuid().ToString().Substring(0, 8) + extension;

            string relativePath = Path.Combine("Images", "AirlineLogo", randomFileName);
            string path = Path.Combine(_host.WebRootPath, relativePath);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            _airline.AirlineLogo = "" + relativePath.Replace("\\", "/");
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}

namespace TMG.Models.AirlineModels 
{
    public record AirlineSummaryModel
    {
        public string Airline { get; set; }
        public string TableHeader { get; set; }
        public int HighestSectorCount { get; set; }
        public List<AirlineSummaryEntryModel> Entries { get; set; } = new();

    }

    public record AirlineSummaryEntryModel
    {
        public int? FlightId { get; set; }
        public int SNo { get; set; }
        public string PNRNumber { get; set; }
        public string PNRName { get; set; }
        public string Class { get; set; }
        public string Company { get; set; }
        public string Package { get; set; }
        public string Airline { get; set; }
        public string FirstSectorFlightNumber { get; set; }
        public string FirstSectorDestination { get; set; }
        public string FirstSectorFlightNo { get; set; }
        public string FirstSectorDepartureDate { get; set; }
        public string FirstSectorDepartureTime { get; set; }
        public string FirstSectorArrivalDate { get; set; }
        public string FirstSectorArrivalTime { get; set; }
        public int Capacity { get; set; }
        public int Booked { get; set; }
        public int Balance { get; set; }
        public List<AirlineSummarySectorModel> Sectors { get; set; } = new();
    }

    public record AirlineSummarySectorModel 
    {
        public string FlightNumber { get; set; }
        public string Destination { get; set; }
        public string FlightNo { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }

}


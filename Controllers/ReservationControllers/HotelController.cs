using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.ReservationModels;
using TMG.Models.FormModels.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using iText.Commons.Actions.Contexts;
using TMG.Models.FamilyModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SautinSoft;

namespace TMG.Models.ReservationModels
{
    public record HotelIndexModel
    {
        public List<HotelIndexEntryModel> Entries { get; set; } = new List<HotelIndexEntryModel>();
    }

    public record HotelIndexEntryModel
    {
        public Hotel Hotel { get; set; }
        public int AllotedRooms { get; set; }
    }
}



namespace TMG.Controllers.ReservationControllers
{
    public class HotelController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _http;

        public HotelController(TMGDbContext dbContext, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Hotels.Where(e => e.Trashed).Count();

            ViewBag.TrashAllowed = PrivilegeManager.CanTrashHotel(_dbContext, _http);
            ViewBag.CreateAllowed = PrivilegeManager.CanCreateHotel(_dbContext, _http);
            ViewBag.EditAllowed = PrivilegeManager.CanEditHotel(_dbContext, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreHotel(_dbContext, _http);

            HotelIndexModel model = new HotelIndexModel();
            var hotels = _dbContext
                .HotelsWithoutTrash
                .Include(e => e.Accomodation)
                .Include(e => e.AccomodationCategory)
                .Include(e => e.Place)
                .ToList();

            var roomAllotments = _dbContext
                .RoomAllotmentsWithoutTrash;

            for (int i = 0; i < hotels.Count; i++)
            {
                Hotel h = hotels.ElementAt(i);
                HotelIndexEntryModel _model = new HotelIndexEntryModel();
                _model.Hotel = h;
                _model.AllotedRooms = roomAllotments
                    .Where(e => e.FloorRoom.Floor.HotelId == h.HotelId)
                    .Count();

                model.Entries.Add(_model);
            }



            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Places = new SelectList(_dbContext.PlacesWithoutTrash, "PlaceId", "PlaceName");
            ViewBag.Accomodations = new SelectList(_dbContext.AccomodationsWithoutTrash, "AccomodationId", "AccomodationName");
            ViewBag.AccomodationCategories = new SelectList(_dbContext.AccomodationCategoriesWithoutTrash, "AccomodationCategoryId", "Category");

            return View();
        }

        [HttpPost]
        public IActionResult Create(HotelRegisterRequest request)
        {
            ViewBag.Places = new SelectList(_dbContext.PlacesWithoutTrash, "PlaceId", "PlaceName");
            ViewBag.Accomodations = new SelectList(_dbContext.AccomodationsWithoutTrash, "AccomodationId", "AccomodationName");
            ViewBag.AccomodationCategories = new SelectList(_dbContext.AccomodationCategoriesWithoutTrash, "AccomodationCategoryId", "Category");

            string ext = Path.GetExtension(request.HotelLogoFile?.FileName);
            if (!ExtensionManager.ValidateFileExtensions(ext))
            {
                ModelState.AddModelError("HotelLogoFile", "Please enter a valid image file");
            }

            if (!ModelState.IsValid) return View();

            string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "HotelLogos", request.HotelLogoFile.FileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                request.HotelLogoFile.CopyTo(fs);
            }

            Hotel hotel = new Hotel
            {
                HotelNumber = request.HotelNumber,
                HotelCode = request.HotelCode,
                HotelName = request.HotelName,
                HotelAddress = request.HotelAddress,
                HotelContact = request.HotelContact,
                HotelEmail = request.HotelEmail,
                HotelLogo = @"Images/HotelLogos/" + request.HotelLogoFile.FileName,
                PlaceId = request.PlaceId,
                AccomodationCategoryId = request.AccomodationCategoryId,
                AccomodationId = request.AccomodationId,
                Location = request.Location,
                //HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
            };

            _dbContext.Hotels.Add(hotel);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Hotel hotel = _dbContext.Hotels.Find(id);

            ViewBag.Places = new SelectList(_dbContext.PlacesWithoutTrash, "PlaceId", "PlaceName", hotel.PlaceId);
            ViewBag.Accomodations = new SelectList(_dbContext.AccomodationsWithoutTrash, "AccomodationId", "AccomodationName", hotel.AccomodationId);
            ViewBag.AccomodationCategories = new SelectList(_dbContext.AccomodationCategoriesWithoutTrash, "AccomodationCategoryId", "Category", hotel.AccomodationCategoryId);

            HotelEditRequest request = new HotelEditRequest
            {
                HotelId = id,
                HotelNumber = hotel.HotelNumber,
                HotelName = hotel.HotelName,
                HotelCode = hotel.HotelCode,
                HotelEmail = hotel.HotelEmail,
                HotelContact = hotel.HotelContact,
                HotelAddress = hotel.HotelAddress,
                PlaceId = hotel.PlaceId,
                AccomodationId = hotel.AccomodationId,
                AccomodationCategoryId = hotel.AccomodationCategoryId,
                Location = hotel.Location,
            };

            return View(request);
        }

        [HttpPost]
        public IActionResult Edit(HotelEditRequest request)
        {
            Hotel hotel = _dbContext.Hotels.Find(request.HotelId);

            ViewBag.Places = new SelectList(_dbContext.PlacesWithoutTrash, "PlaceId", "PlaceName", hotel.PlaceId);
            ViewBag.Accomodations = new SelectList(_dbContext.AccomodationsWithoutTrash, "AccomodationId", "AccomodationName", hotel.AccomodationId);
            ViewBag.AccomodationCategories = new SelectList(_dbContext.AccomodationCategoriesWithoutTrash, "AccomodationCategoryId", "Category", hotel.AccomodationCategoryId);

            if (!ModelState.IsValid) return View(request);

            hotel.HotelNumber = request.HotelNumber;
            hotel.HotelCode = request.HotelCode;
            hotel.HotelName = request.HotelName;
            hotel.HotelAddress = request.HotelAddress;
            hotel.HotelEmail = request.HotelEmail;
            hotel.HotelContact = request.HotelContact;
            hotel.AccomodationCategoryId = request.AccomodationCategoryId;
            hotel.AccomodationId = request.AccomodationId;
            hotel.PlaceId = request.PlaceId;
            hotel.Location = request.Location;
            //hotel.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));


            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangeLogo(int id)
        {
            HotelLogoUpdateRequest request = new HotelLogoUpdateRequest
            {
                HotelId = id
            };
            return View(request);
        }

        [HttpPost]
        public IActionResult ChangeLogo(HotelLogoUpdateRequest request)
        {
            Hotel hotel = _dbContext.Hotels.Find(request.HotelId);

            string ext = Path.GetExtension(request.NewLogo?.FileName);
            if (!ExtensionManager.ValidateFileExtensions(ext))
            {
                ModelState.AddModelError("HotelLogoFile", "Please enter a valid image file");
            }

            if (!ModelState.IsValid) return View();
            //string path = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "HotelLogos");

            //using (FileStream fs = new(path, FileMode.Create))
            //{
            //    request.HotelLogoFile.CopyTo(fs);
            //}

            hotel.HotelLogo = @"Images/HotelLogos/" + request.NewLogo.FileName;
            //hotel.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Trash(int id)
        {
            Hotel hotelToDelete = _dbContext.Hotels.Find(id);
            if (hotelToDelete == null) return RedirectToAction(nameof(Index));

            hotelToDelete.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.Hotels.Where(u => u.Trashed)
                .Include(e => e.AccomodationCategory).Include(e => e.Accomodation).Include(e => e.Place));
        }

        public IActionResult Restore(int id)
        {
            Hotel hotel = _dbContext.Hotels.Find(id);
            hotel.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AvailabilityChart(int id)
        {
            ViewBag.HotelName = _dbContext.Hotels.Find(id).HotelName;
            var query = from f in _dbContext.Floors
                        join fr in _dbContext.FloorRoom on f.FloorId equals fr.FloorId
                        join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                        join h in _dbContext.Hotels on f.HotelId equals h.HotelId
                        where h.HotelId == id
                        join rfm in _dbContext.ReservationFamilyMembers on fr.FloorRoomId equals rfm.FloorRoomId into rfmGroup
                        from rfmSub in rfmGroup.DefaultIfEmpty()
                        group new { f, fr, r, h, rfmSub } by new
                        {
                            f.FloorId,
                            fr.FloorRoomId,
                            f.FloorNumber,
                            r.PAX,
                            r.RoomCode,
                            fr.RoomNumber
                        } into grouped
                        select new
                        {
                            FloorNumber = grouped.Key.FloorNumber,
                            RoomNumber = grouped.Key.RoomNumber,
                            RoomCode = grouped.Key.RoomCode,
                            PAX = grouped.Key.PAX,
                            AllocatedPAX = grouped.Count(g => g.rfmSub != null),
                            Availability = grouped.Key.PAX <= grouped.Count(g => g.rfmSub != null) ? "Closed" : (grouped.Count(g => g.rfmSub != null) == 0 ? "Open" : "Occupied")
                        };

            // Execute the query and then format RoomNumber in memory
            var results = query.AsNoTracking()
                               .OrderBy(x => x.FloorNumber)
                               .ThenBy(x => x.RoomNumber)
                               .ToList()
                               .Select(x => new
                               {
                                   FloorNumber = x.FloorNumber,
                                   RoomNumber = $"{x.RoomNumber} ({x.RoomCode})", // String formatting here
                                   PAX = x.PAX,
                                   AllocatedPAX = x.AllocatedPAX,
                                   Availability = x.Availability
                               }).ToList();

            return View(results);
        }

        public IActionResult CheckInReport(int id)
        {
            ViewBag.HotelId = id;
            return View();
        }

        public IActionResult CheckInReportView(int HotelId, DateTime Date)
        {
                var query = from fh in _dbContext.FamilyHeads
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                        join rfm in _dbContext.ReservationFamilyMembers on fh.FamilyHeadId equals rfm.FamilyHeadId
                        join res in _dbContext.Reservations on rfm.ReservationId equals res.ReservationId
                        join g in _dbContext.Groups on res.GroupId equals g.GroupId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                        join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                        join f in _dbContext.Floors on rfm.FloorId equals f.FloorId into floorGroup
                        from f in floorGroup.DefaultIfEmpty()
                        join fr in _dbContext.FloorRoom on rfm.FloorRoomId equals fr.FloorRoomId into roomGroup
                        from fr in roomGroup.DefaultIfEmpty()
                        join r in _dbContext.Rooms on fr.RoomId equals r.RoomId into rGroup
                        from r in rGroup.DefaultIfEmpty()
                        where 
                              !fh.Trashed && res.HotelId == HotelId && pa.PlaceId == 4 && pa.ActualCheckInTime.Value.Date == Date.Date && !g.Trashed
                            select new
                        {
                            MunazimName = m.Name,
                            CompanyCode = c.CompanyCode,
                            PackageNumber = p.Name,
                            GroupName = g.Name,
                            fh.FcId,
                            CID = $"{fm.FamilyHead.Package.Company.CompanyCode}.{fm.FamilyHead.IndId}" + fh.FcId.Substring(3) +
                   $".{fm.Head.ExtractedFamilyNumber}.({_dbContext.FamilyMembers.Count(e => e.HeadId == fm.HeadId)})" +
                   $".{fm.FamilyHead.Package.Name}",
                            FullName = fh.GivenName + " " + fh.SurName,
                            checkin = pa.ActualCheckInTime.Value.Date,
                            checkout = pa.ActualCheckOutTime.Value.Date,
                            fh.PassportImage,
                            fh.MemberImage,
                            fh.PassportNumber,
                            fh.VisaImage,
                            fh.VisaNumber,
                            fh.HajjApplicationNumber,
                            RoomCodeNew = r.RoomCode ?? "Sharing",
                            FloorNumber = f != null ? f.FloorNumber : 0,
                            RoomNumber = fr != null ? fr.RoomNumber : "",
                            fh.Gender
                        };

            var result = query.ToList();
            int TotalMale = 0;
            int TotalFemale = 0;

            foreach (var item in result)
            {
                TotalMale = item.Gender == Gender.Male ? TotalMale + 1 : TotalMale;
                TotalFemale = item.Gender == Gender.Female ? TotalFemale + 1 : TotalFemale;

                SautinSoft.PdfFocus pdf = new SautinSoft.PdfFocus();

                pdf.OpenPdf(item.VisaImage);

                if (pdf.PageCount > 0)
                {
                    string fullName = $"{item.FullName}.tiff";

                    string outputFolder = @"D:\HUMS\wwwroot\Images\Visapng";

                    string outputPath = Path.Combine(outputFolder, fullName);

                    pdf.ImageOptions.Dpi = 300;

                    pdf.ToMultipageTiff(outputPath);
                }
            }
            var hotelName = (from hotel in _dbContext.Hotels
                             where hotel.HotelId == HotelId
                             select hotel.HotelName).FirstOrDefault();
            ViewBag.HotelName = hotelName;
            ViewBag.TotalMale = TotalMale;
            ViewBag.TotalFemale = TotalFemale;
            ViewBag.Data = result;
            ViewBag.HotelId = HotelId;
            ViewBag.Date = Date.Date;

            int totalmemebers = 0;
            int dblroomcount = 0;
            int triroomcount = 0;
            int quadroomcount = 0;
            int quintroomcount = 0;
            int sharingroomcount = 0;
            foreach (var item in result)
            {
                totalmemebers = totalmemebers + 1;
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
            ViewBag.TotalMember = totalmemebers;


         
            return View();
        }

        public IActionResult PdfToImgVIsa(int HotelId, DateTime Date)
        {
            var query = from fh in _dbContext.FamilyHeads
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                        join rfm in _dbContext.ReservationFamilyMembers on fh.FamilyHeadId equals rfm.FamilyHeadId
                        join res in _dbContext.Reservations on rfm.ReservationId equals res.ReservationId
                        join g in _dbContext.Groups on res.GroupId equals g.GroupId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                        join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                        join f in _dbContext.Floors on rfm.FloorId equals f.FloorId into floorGroup
                        from f in floorGroup.DefaultIfEmpty()
                        join fr in _dbContext.FloorRoom on rfm.FloorRoomId equals fr.FloorRoomId into roomGroup
                        from fr in roomGroup.DefaultIfEmpty()
                        join r in _dbContext.Rooms on fr.RoomId equals r.RoomId into rGroup
                        from r in rGroup.DefaultIfEmpty()
                        where !fh.Trashed && f.HotelId == HotelId && pa.PlaceId == 4 && pa.ActualCheckInTime.Value.Date == Date.Date
                        select new
                        {
                            fh.VisaImage,
                        };

            var results = query.ToList();


         

            return View();
        }

       
    }
}

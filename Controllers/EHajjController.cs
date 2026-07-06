using TMG.Data;
using TMG.Models.CompanyModels;
using TMG.Models.EHajjModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers
{
    public class EHajjController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public EHajjController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.EHajjContracts.Where(e => e.Trashed).Count();

            return View(_dbContext.EHajjContractsWithoutTrash
            .Include(e => e.Company)
            .Include(e => e.Package)
            .Include(e => e.FoodPackage)
            .Include(e => e.Hotel)
            );
        }

        public IActionResult Create()
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash.Include(e => e.AccomodationCategory);
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;

            return View();
        }


        [HttpPost]
        public IActionResult Create(EHajjModelRequest request)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash.Include(e => e.AccomodationCategory);
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;

            //if (!ModelState.IsValid) return View();
            
            string savePathVouchers = "";
            string savePathContract = "";
            string savePathQr = "";

            //if (request.Voucherpath != null && request.Voucherpath.Length > 0)
            //{
            //    savePathVouchers = Path.Combine(_host.WebRootPath, "Images", "EHajjVouchers");
            //}
            //if (request.Contractpath != null && request.Contractpath.Length > 0)
            //{
            //    savePathContract = Path.Combine(_host.WebRootPath, "Images", "EHajjContract");
            //}
            //if (request.QrPath != null && request.QrPath.Length > 0)
            //{
            //    savePathQr = Path.Combine(_host.WebRootPath, "Images", "EHajjQr");
            //}


            if (request.Voucherpath is not null)
            {
                savePathVouchers = Path.Combine(_host.WebRootPath, "Images", "EHajjVouchers", request.Voucherpath.FileName);
                using (FileStream fs = new FileStream(savePathVouchers, FileMode.Create))
                {
                    request.Voucherpath.CopyTo(fs);
                }
            }
            if (request.Contractpath is not null)
            {
                savePathContract = Path.Combine(_host.WebRootPath, "Images", "EHajjContract", request.Contractpath.FileName);
                using (FileStream fs = new FileStream(savePathContract, FileMode.Create))
                {
                    request.Contractpath.CopyTo(fs);
                }
            }
            if (request.QrPath is not null)
            {
                savePathQr = Path.Combine(_host.WebRootPath, "Images", "EHajjQr", request.QrPath.FileName);
                using (FileStream fs = new FileStream(savePathQr, FileMode.Create))
                {
                    request.QrPath.CopyTo(fs);
                }
            }


            // Ensure the directory exists
            //if (!Directory.Exists(savePathVouchers))
            //{
            //    Directory.CreateDirectory(savePathVouchers);
            //}
            //if (!Directory.Exists(savePathContract))
            //{
            //    Directory.CreateDirectory(savePathContract);
            //}
            //if (!Directory.Exists(savePathQr))
            //{
            //    Directory.CreateDirectory(savePathQr);
            //}

            // Create the file path Vouchers
            //var filePath = Path.Combine(savePathVouchers, request.Voucherpath.FileName);

            //// Save the file
            //using (var stream = new FileStream(filePath, FileMode.Create))
            //{
            //    request.Voucherpath.CopyTo(stream);
            //}

            // Create the file path Contract
            //var filePath1 = Path.Combine(savePathVouchers, request.Contractpath.FileName);

            //// Save the file
            //using (var stream = new FileStream(filePath1, FileMode.Create))
            //{
            //    request.Contractpath.CopyTo(stream);
            //}

            //var filePath2 = Path.Combine(savePathQr, request.QrPath.FileName);

            // Save the file
            //using (var stream = new FileStream(filePath2, FileMode.Create))
            //{
            //    request.QrPath.CopyTo(stream);
            //}

            EHajjContract eHajjContract = new EHajjContract
            {
                CompanyId = request.CompanyId,
                PackageId = request.PackageId,
                HotelId = request.HotelId,
                FoodPackageId = request.FoodPackageId,
                Capacity = request.Capacity,
                EhajjContractNumber = request.EhajjContractNumber,
                VoucherNumber = request.VoucherNumber,
                Amount = request.Amount,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                Voucherpath = "Images/EHajjVouchers/" + request.Voucherpath.FileName,
                QrPath = "Images/EHajjQr/"+request.QrPath.FileName,
                Contractpath = "Images/EHajjContract/" + request.Contractpath.FileName,
            };

            _dbContext.EHajjContracts.Add(eHajjContract);
            _dbContext.SaveChanges();

            for (int i = 0; i < request.RoomIds.Count; i++)
            {
                var r = _dbContext.Rooms.FirstOrDefault(e => e.RoomId == request.RoomIds[i]);
                int pax = request.IndividualRoomsQuantity.ElementAt(i) * r.PAX;
                _dbContext.EHajjRooms.Add(new EHajjRooms
                {
                    EhajjContractId = eHajjContract.EhajjContractId,
                    RoomId = request.RoomIds[i],
                    Rooms = request.IndividualRoomsQuantity.ElementAt(i),
                    Paxs = pax,
                });
            }
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash.Include(e => e.AccomodationCategory);
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;

            ViewBag.AssignedRooms = _dbContext.EHajjRooms.Where(e => e.EhajjContractId == id);

            EHajjContract eHajjContract = _dbContext.EHajjContracts.Find(id);
            var rooms = _dbContext.EHajjRooms.Where(e => e.EhajjContractId == id).ToList();

            List<int> roomslist = new List<int>();
            List<int> roomspaxs = new List<int>();
            List<int> IndividualRoomsQuantity = new List<int>();
            List<int> IndividualPaxQuantity = new List<int>();
            int roomsquatity = 0;
            int paxsquantity = 0;
            foreach (var ro in rooms)
            {
                roomslist.Add(ro.RoomId);
                roomspaxs.Add(ro.Paxs);
                IndividualRoomsQuantity.Add(ro.Rooms);
                IndividualPaxQuantity.Add(ro.Paxs);
                roomsquatity = roomsquatity + ro.Rooms;
                paxsquantity = paxsquantity + ro.Paxs;
            }
            string Contractpath = Path.Combine(eHajjContract.Contractpath);
            string Voucherpath = Path.Combine(eHajjContract.Voucherpath);

            EHajjModelRequest res = new EHajjModelRequest
            {
                EhajjContractId = id,
                CompanyId = eHajjContract.CompanyId,
                PackageId = eHajjContract.PackageId,
                HotelId = eHajjContract.HotelId,
                FoodPackageId = eHajjContract.FoodPackageId,
                Capacity = eHajjContract.Capacity,
                EhajjContractNumber = eHajjContract.EhajjContractNumber,
                VoucherNumber = eHajjContract.VoucherNumber,
                Amount = eHajjContract.Amount,
                Contractpathstr = Contractpath,
                Voucherpathstr = Voucherpath,
                CheckIn = eHajjContract.CheckIn,
                CheckOut = eHajjContract.CheckOut,
                RoomIds = roomslist,
                Paxs = roomspaxs,
                RoomsQuantity = roomsquatity,
                PaxsQuantity = paxsquantity,
                IndividualRoomsQuantity = IndividualRoomsQuantity,
                IndividualPaxQuantity = IndividualPaxQuantity
            };

            return View(res);
        }


        [HttpPost]
        public IActionResult Edit(EHajjModelRequest request)
        {
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;
            ViewBag.Hotels = _dbContext.HotelsWithoutTrash.Include(e => e.AccomodationCategory);
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;
            ViewBag.FoodPackages = _dbContext.FoodPackagesWithoutTrash;

            ViewBag.AssignedRooms = _dbContext.EHajjRooms.Where(e => e.EhajjContractId == request.EhajjContractId);

            EHajjContract edit = _dbContext.EHajjContracts.Find(request.EhajjContractId);

            edit.CompanyId = request.CompanyId;
            edit.PackageId = request.PackageId;
            edit.HotelId = request.HotelId;
            edit.FoodPackageId = request.FoodPackageId;
            edit.Capacity = request.Capacity;
            edit.EhajjContractNumber = request.EhajjContractNumber;
            edit.VoucherNumber = request.VoucherNumber;
            edit.Amount = request.Amount;
            edit.CheckIn = request.CheckIn;
            edit.CheckOut = request.CheckOut;
            _dbContext.SaveChanges();

            _dbContext.EHajjRooms.Where(e => e.EhajjContractId == request.EhajjContractId).ExecuteDelete();

            for (int i = 0; i < request.RoomIds.Count; i++)
            {
                var r = _dbContext.Rooms.FirstOrDefault(e => e.RoomId == request.RoomIds[i]);
                int pax = request.IndividualRoomsQuantity.ElementAt(i) * r.PAX;
                _dbContext.EHajjRooms.Add(new EHajjRooms
                {
                    EhajjContractId = request.EhajjContractId,
                    RoomId = request.RoomIds[i],
                    Rooms = request.IndividualRoomsQuantity.ElementAt(i),
                    Paxs = pax,
                });
            }
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.EHajjContracts
            .Include(e => e.Company)
            .Include(e => e.Package)
            .Include(e => e.Hotel)
            .Include(e => e.FoodPackage)
            .Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            EHajjContract EHajjContract = _dbContext.EHajjContracts.Find(id);
            if (EHajjContract == null) return RedirectToAction(nameof(Index));

            EHajjContract.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            EHajjContract EHajjContract = _dbContext.EHajjContracts.Find(id);
            EHajjContract.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddMember(int id)
        {

            EHajjContract EHajjContract = _dbContext.EHajjContractsWithoutTrash
            .Include(e => e.Company)
            .Include(e => e.Package)
            .Include(e => e.FoodPackage)
            .Include(e => e.Hotel)
            .ThenInclude(e => e.Place)
            .FirstOrDefault(e => e.EhajjContractId == id);

            ViewBag.Company = EHajjContract.Company.CompanyName;
            ViewBag.Package = EHajjContract.Package.Name;
            ViewBag.Hotel = EHajjContract.Hotel.HotelName;
            ViewBag.Place = EHajjContract.Hotel.Place.PlaceName;
            ViewBag.CheckIn = EHajjContract.CheckIn;
            ViewBag.CheckOut = EHajjContract.CheckOut;
            ViewBag.Capacity = EHajjContract.Capacity;
            ViewBag.Used = EHajjContract.Used;
            ViewBag.UnUsed = EHajjContract.Capacity - EHajjContract.Used;

            var eHajjMembers = _dbContext.EHajjContractMembers.Where(e => e.EhajjContractId == id);

            ViewBag.EHajjMembers = eHajjMembers;
            ViewBag.EHajjContractId = id;
            // ViewBag.EHajjMembersCount = eHajjMembers.Count();

            EHajjRooms eHajjRooms = _dbContext.EHajjRooms.FirstOrDefault(e => e.EhajjContractId == id);

            var query = from fh in _dbContext.FamilyHeads
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where fh.PackageId == EHajjContract.PackageId
                        && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            p.Name,
                            fh.ReferenceId,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.Age,
                            fh.PassportNumber,
                            fh.FriendId,
                        };

            var result = query.ToList();

            return View(result);
        }

        [HttpPost]
        public IActionResult AddMember(int contractId, int memberId)
        {
            int paxLimit = _dbContext.EHajjContractsWithoutTrash.FirstOrDefault(e => e.EhajjContractId == contractId).Capacity;
            int selected = _dbContext.EHajjContractMembers.Where(e => e.EhajjContractId == contractId).Count();

            if (selected == paxLimit)
            {
                return BadRequest("Limit Reached");
            }

            FamilyMembers fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == memberId);

            _dbContext.EHajjContractMembers.Add(
                new EHajjContractMember
                {
                    FamilyHeadId = memberId,
                    EhajjContractId = contractId
                }
                );

            _dbContext.SaveChanges();

            _dbContext.EHajjContractsWithoutTrash.FirstOrDefault(e => e.EhajjContractId == contractId).Used++;
            _dbContext.SaveChanges();

            return Ok(new { status = 200, message = $"Member Id: {memberId} added to contract Id: {contractId}" });
        }

        [HttpPost]
        public IActionResult RemoveMember(int contractId, int memberId)
        {
            _dbContext.EHajjContractMembers.Remove(
                _dbContext.EHajjContractMembers.FirstOrDefault(e => e.EhajjContractId == contractId && e.FamilyHeadId == memberId)
                );

            _dbContext.SaveChanges();

            _dbContext.EHajjContractsWithoutTrash.FirstOrDefault(e => e.EhajjContractId == contractId).Used--;
            _dbContext.SaveChanges();

            return Ok(new { status = 200, message = $"Member Id: {memberId} removed from contract Id: {contractId}" });
        }

        public JsonResult GetPackages(int id)
        {
            var packages = _dbContext.PackagesWithoutTrash
                .Select(e => new
                {
                    e.PackageId,
                    e.Name,
                    e.CompanyId
                })
                .Where(e => e.CompanyId == id);

            return Json(packages);
        }

        public IActionResult ChangeContractImage(int id) => View(_dbContext.EHajjContracts.Find(id));

        [HttpPost]
        public IActionResult ChangeContractImage(int EHajjContractId, IFormFile File)
         {

            EHajjContract contract = _dbContext.EHajjContracts.Find(EHajjContractId);

            string fileName = Path.GetFileNameWithoutExtension(File.FileName);
            string extension = Path.GetExtension(File.FileName);
            string randomFileName = fileName + "_" + Guid.NewGuid().ToString().Substring(0, 8) + extension;

            string relativePath = Path.Combine("Images", "EHajjContract", randomFileName);
            string path = Path.Combine(_host.WebRootPath, relativePath);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            contract.Contractpath = "" + relativePath.Replace("\\", "/");
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangeVoucherImage(int id) => View(_dbContext.EHajjContracts.Find(id));

        [HttpPost]
        public IActionResult ChangeVoucherImage(int EHajjContractId, IFormFile File)
        {

            EHajjContract contract = _dbContext.EHajjContracts.Find(EHajjContractId);

            string fileName = Path.GetFileNameWithoutExtension(File.FileName);
            string extension = Path.GetExtension(File.FileName);
            string randomFileName = fileName + "_" + Guid.NewGuid().ToString().Substring(0, 8) + extension;
            
            string relativePath = Path.Combine("Images", "EHajjVouchers", randomFileName);
            string path = Path.Combine(_host.WebRootPath, relativePath);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            contract.Voucherpath = "" + relativePath.Replace("\\", "/");
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangeQRImage(int id) => View(_dbContext.EHajjContracts.Find(id));

        [HttpPost]
        public IActionResult ChangeQRImage(int EHajjContractId, IFormFile File)
        {

            EHajjContract contract = _dbContext.EHajjContracts.Find(EHajjContractId);

            string fileName = Path.GetFileNameWithoutExtension(File.FileName);
            string extension = Path.GetExtension(File.FileName);
            string randomFileName = fileName + "_" + Guid.NewGuid().ToString().Substring(0, 8) + extension;
            
            string relativePath = Path.Combine("Images", "EHajjQr", randomFileName);
            string path = Path.Combine(_host.WebRootPath, relativePath);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                File.CopyTo(fs);
            }

            contract.QrPath = "" + relativePath.Replace("\\", "/");
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }




    }
}


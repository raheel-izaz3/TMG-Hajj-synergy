using TMG.Data;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.SettingsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TMG.Controllers.AccountControllers
{
    public class RoeRequestController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _webEnviroment;

        private readonly IHttpContextAccessor _http;
        public RoeRequestController(TMGDbContext dbContext, IWebHostEnvironment webEnviroment, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _webEnviroment = webEnviroment;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.RoeRequests.Where(e => e.Trashed).Count();         

            ViewBag.RoeRequest = _dbContext.RoeRequestsWithoutTrash
                            .Include(e => e.FamilyHead)
                            .Join(
                                _dbContext.Users,
                                roeRequest => roeRequest.CreatedBy,
                                user => user.UserId,
                                (roeRequest, user) => new { RoeRequest = roeRequest, User = user }
                            );

            return View();
        }

        public IActionResult Create()
        {
            ViewBag.FamilyHead = _dbContext.FamilyHeadsWithoutTrash;   
            return View();
        }


        [HttpPost]
        public IActionResult Create(RoeRequest request)
        {

            if (!ModelState.IsValid) return View();

            int user_id =  Convert.ToInt32(_http.HttpContext.Session.GetString("UserId"));
        
          
            RoeRequest roe = new RoeRequest
            {
                RoeRequestId = request.RoeRequestId,
                CreatedBy = user_id,
                ApprovedBy = 0,
                Currency = request.Currency,
                PKRROE = request.PKRROE,
                SARROE = request.SARROE,
                USDROE = request.USDROE,
                Status = request.Status,
                Remarks = request.Remarks,
                FamilyHeadId = request.FamilyHeadId
            };

            _dbContext.RoeRequests.Add(roe);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            RoeRequest bank = _dbContext.RoeRequests.Find(id);

            ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash;

            return View(bank);
        }


        [HttpPost]
        public IActionResult Edit(RoeRequest roe)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash;
                return View(roe);
            }

            RoeRequest _Roe = _dbContext.RoeRequests
                .Include(r => r.FamilyHead) 
                .FirstOrDefault(r => r.RoeRequestId == roe.RoeRequestId);

            if (_Roe == null)
            {
                return NotFound();
            }

            _Roe.Currency = roe.Currency;
            _Roe.PKRROE = roe.PKRROE;
            _Roe.SARROE = roe.SARROE;
            _Roe.USDROE = roe.USDROE;
            _Roe.Remarks = roe.Remarks;
            _Roe.FamilyHeadId = roe.FamilyHeadId; 

            try
            {
                _dbContext.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the changes.");
                ViewBag.FamilyHeads = _dbContext.FamilyHeadsWithoutTrash;
                return View(roe); 
            }
        }

        public IActionResult ChangeImage(int id)
        {
            return View();
        }


        public IActionResult ViewTrash()
        {
            
             ViewBag.RoeRequest = _dbContext.RoeRequests
                            .Include(e => e.FamilyHead)
                            .Join(
                                _dbContext.Users,
                                roeRequest => roeRequest.CreatedBy,
                                user => user.UserId,
                                (roeRequest, user) => new { RoeRequest = roeRequest, User = user }
                            )
                            .Where(e => e.RoeRequest.Trashed);

            return View();
        }

        public IActionResult Trash(int id)
        {
            RoeRequest roe = _dbContext.RoeRequests.Find(id);
            if (roe == null) return RedirectToAction(nameof(Index));

            roe.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

         public IActionResult Approved(int id)
        {
            RoeRequest roe = _dbContext.RoeRequests.Find(id);
            if (roe == null) return RedirectToAction(nameof(Index));

            roe.Status = Status.Approved;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

         public IActionResult Reject(int id)
        {
            RoeRequest roe = _dbContext.RoeRequests.Find(id);
            if (roe == null) return RedirectToAction(nameof(Index));

              roe.Status = Status.Reject;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        

        public IActionResult Restore(int id)
        {
            RoeRequest roe = _dbContext.RoeRequests.Find(id);
            roe.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }

}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.FormModels.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TMG.Models.MiscModels;
using Microsoft.AspNetCore.Http;

namespace TMG.Controllers
{
    public class AuthController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public AuthController(TMGDbContext humsDbContext, IHttpContextAccessor http)
        {
            _dbContext = humsDbContext;
            _http = http;
        }

        public IActionResult Login()
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Admin");
            //}

            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            User loggedInUser = _dbContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Email.Equals(request.Email));

            if (loggedInUser == null
                || !Models.User.VerifyPassword(request.Password, loggedInUser.PasswordHash, loggedInUser.PasswordSalt))
            {
                request.ErrorStatusCode = 0;
                return View(request);
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, loggedInUser.Email),
                new Claim(ClaimTypes.Name, loggedInUser.Username),
                new Claim(ClaimTypes.Role, loggedInUser.Role?.RoleName ?? "null"),
            };


            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties authProps = new AuthenticationProperties
            {
                AllowRefresh = true,
            };

            HttpContext.SignInAsync(new ClaimsPrincipal(identity), authProps);

            TempData["loggedIn"] = true;
            var hajjSeasons = _dbContext.HajjSeasons
               .OrderByDescending(e => e.HajjSeasonsId)
               .FirstOrDefault(e => !e.Trashed);

            _http.HttpContext.Session.SetString("ActiveSeasonId", hajjSeasons.HajjSeasonsId.ToString());
            _http.HttpContext.Session.SetString("Username", loggedInUser.Username);
            _http.HttpContext.Session.SetString("UserEmail", loggedInUser.Email);
            _http.HttpContext.Session.SetString("UserId", loggedInUser.UserId.ToString());
            _http.HttpContext.Session.SetString("RoleId", loggedInUser.RoleId.ToString());

            _http.HttpContext.Session.SetString("CanViewUser", PrivilegeManager.CanViewUser(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewRole", PrivilegeManager.CanViewRole(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewCompany", PrivilegeManager.CanViewCompany(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewCompanyAddress", PrivilegeManager.CanViewCompanyAddress(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewCompanyContact", PrivilegeManager.CanViewCompanyContact(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewCompanyEmail", PrivilegeManager.CanViewCompanyEmail(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewCompanyLicense", PrivilegeManager.CanViewCompanyLicense(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewStaffMembers", PrivilegeManager.CanViewStaffMember(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewDesignation", PrivilegeManager.CanViewDesignation(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewVehicle", PrivilegeManager.CanViewVehicle(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewRoute", PrivilegeManager.CanViewRoute(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewTravelRoute", PrivilegeManager.CanViewTravelRoute(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewRoom", PrivilegeManager.CanViewRoom(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewPlace", PrivilegeManager.CanViewPlace(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewAccomodation", PrivilegeManager.CanViewAccomodation(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewAccomodationCategory", PrivilegeManager.CanViewAccomodationCategory(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewFoodPackage", PrivilegeManager.CanViewFoodPackage(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewGift", PrivilegeManager.CanViewGift(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewTraining", PrivilegeManager.CanViewTraining(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewBank", PrivilegeManager.CanViewBank(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewHonorifics", PrivilegeManager.CanViewHonorifics(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewFiqah", PrivilegeManager.CanViewFiqah(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewTehsil", PrivilegeManager.CanViewTehsil(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewReference", PrivilegeManager.CanViewReference(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewFamilyHeadRel", PrivilegeManager.CanViewFamilyHeadRel(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewMehramRel", PrivilegeManager.CanViewMehramRel(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewNominee", PrivilegeManager.CanViewTraining(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewReqDocs", PrivilegeManager.CanViewReqDocs(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewPaymentNotes", PrivilegeManager.CanViewPaymentNotes(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewVaccine", PrivilegeManager.CanViewVaccine(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewQuotationPayPol", PrivilegeManager.CanViewQuotationPayPol(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewQuotationTC", PrivilegeManager.CanViewQuotationTC(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewAirlines", PrivilegeManager.CanViewAirlines(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewAirlineClass", PrivilegeManager.CanViewAirlineClass(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewAirlineFare", PrivilegeManager.CanViewAirlineFare(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewMemberType", PrivilegeManager.CanViewMemberType(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewMemberAddress", PrivilegeManager.CanViewMemberAddress(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewMemberContact", PrivilegeManager.CanViewMemberContact(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewMemberEmail", PrivilegeManager.CanViewMemberEmail(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewFamilies", PrivilegeManager.CanViewFamilies(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewReceipt", PrivilegeManager.CanViewReceipt(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewClientStatement", PrivilegeManager.CanViewClientStatement(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewMiniStatement", PrivilegeManager.CanViewMiniStatement(_dbContext, _http).ToString());

            _http.HttpContext.Session.SetString("CanViewHotel", PrivilegeManager.CanViewHotel(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewRoomAllotment", PrivilegeManager.CanViewHotel(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewRoomSummaryReport", PrivilegeManager.CanViewRoomSummaryReport(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewDailyRoomSummaryReport", PrivilegeManager.CanViewDailyRoomSummaryReport(_dbContext, _http).ToString());
            _http.HttpContext.Session.SetString("CanViewPOTicket", PrivilegeManager.CanViewPOTicket(_dbContext, _http).ToString());

            //_http.HttpContext.Session.SetString("CanViewRecordsPerPage", PrivilegeManager.CanViewRecordsPerPage(_dbContext, _http).ToString());
            //_http.HttpContext.Session.SetString("CanViewQurbaniType", PrivilegeManager.CanViewQurbaniType(_dbContext, _http).ToString());


            if (loggedInUser.RoleId != null)
            {
                _http.HttpContext.Session.SetString("RoleName", loggedInUser.Role.RoleName.ToString());
                _http.HttpContext.Session.SetString("UserModuleAllowed", loggedInUser.Role.UserModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("CompanyModuleAllowed", loggedInUser.Role.CompanyModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("AccountsModuleAllowed", loggedInUser.Role.AccountsModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("PackageModuleAllowed", loggedInUser.Role.PackageModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("TransportationModuleAllowed", loggedInUser.Role.TransportationModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("MiscModuleAllowed", loggedInUser.Role.MiscModuleAllowed.ToString());
                //_http.HttpContext.Session.SetString("RegistrationModuleAllowed", loggedInUser.Role.RegistrationModuleAllowed.ToString());
                //_http.HttpContext.Session.SetString("MemberModuleAllowed", loggedInUser.Role.MemberModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("FamilyModuleAllowed", loggedInUser.Role.FamilyModuleAllowed.ToString());
                //_http.HttpContext.Session.SetString("ReportsModuleAllowed", loggedInUser.Role.ReportsModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("QurbaniModuleAllowed", loggedInUser.Role.QurbaniModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("GroupsModuleAllowed", loggedInUser.Role.GroupsModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("ReservationModuleAllowed", loggedInUser.Role.ReservationModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("InvoiceModuleAllowed", loggedInUser.Role.ReceiptModuleAllowed.ToString());
                //_http.HttpContext.Session.SetString("LogisticsModuleAllowed", loggedInUser.Role.LogisticsModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("AccomodationModuleAllowed", loggedInUser.Role.AccommodationModuleAllowed.ToString());
                _http.HttpContext.Session.SetString("PurchaseOrderModuleAllowed", loggedInUser.Role.PurchaseOrderModuleAllowed.ToString());

            }
            else
            {
                _http.HttpContext.Session.SetString("RoleName", "null");
            }

            return RedirectToAction("Index", "Admin");

        }

        public IActionResult HajjSeason()
        {
            var hajjSeasons = _dbContext.HajjSeasons
                   
                   .ToList();

            return PartialView("_HajjSeason", hajjSeasons);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();

            TempData["loggedOut"] = true;
            return RedirectToAction(nameof(Login));
        }

    }
}

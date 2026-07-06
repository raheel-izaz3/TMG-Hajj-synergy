using TMG.Data;
using TMG.Models;
using static TMG.ManagerClasses.PrivilegeManager;

namespace TMG.ManagerClasses
{
    public static class NewPrivilegeManager
    {

        #region Hotel
        public static bool CanTrashHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashHotel.ToString())
                 != null;
        }

        public static bool CanCreateHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateHotel.ToString())
                 != null;
        }

        public static bool CanEditHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditHotel.ToString())
                 != null;
        }

        public static bool CanViewHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewHotel.ToString())
                 != null;
        }

        public static bool CanRestoreHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreHotel.ToString())
                 != null;
        }


        #endregion

        #region Room Allotment
        public static bool CanTrashRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashRoomAllotment.ToString())
                 != null;
        }

        public static bool CanCreateRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateRoomAllotment.ToString())
                 != null;
        }

        public static bool CanEditRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditRoomAllotment.ToString())
                 != null;
        }

        public static bool CanViewRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewRoomAllotment.ToString())
                 != null;
        }

        public static bool CanViewPOHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId"))
                && rp.Privilege == Privileges.ViewPOTicket.ToString())
                 != null;
        }

        public static bool CanRestoreRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreRoomAllotment.ToString())
                 != null;
        }


        #endregion

        public static bool CanViewRoomSummaryReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewRoomSummaryReport.ToString())
                 != null;
        }
        public static bool CanViewDailyRoomSummaryReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewDailyRoomSummaryReport.ToString())
                 != null;
        }

        #region User
        public static bool CanTrashUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashUser.ToString())
                 != null;
        }

        public static bool CanCreateUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateUser.ToString())
                 != null;
        }

        public static bool CanEditUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditUser.ToString())
                 != null;
        }

        public static bool CanViewUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewUser.ToString())
                 != null;
        }

        public static bool CanRestoreUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreUser.ToString())
                 != null;
        }
        #endregion

        #region Role
        public static bool CanTrashRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashRole.ToString())
                 != null;
        }

        public static bool CanCreateRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateRole.ToString())
                 != null;
        }

        public static bool CanEditRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditRole.ToString())
                 != null;
        }

        public static bool CanViewRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewRole.ToString())
                 != null;
        }

        public static bool CanRestoreRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreRole.ToString())
                 != null;
        }

        public static bool CanSetRolePrivilege(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.SetRolePrivelege.ToString())
                 != null;
        }
        #endregion

        #region Company
        public static bool CanTrashCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashCompany.ToString())
                 != null;
        }

        public static bool CanCreateCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateCompany.ToString())
                 != null;
        }

        public static bool CanEditCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditCompany.ToString())
                 != null;
        }

        public static bool CanViewCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewCompany.ToString())
                 != null;
        }

        public static bool CanRestoreCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreCompany.ToString())
                 != null;
        }

        public static bool CanViewIndividualReports(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.IndividualReports.ToString())
                 != null;
        }

        public static bool CanViewCombineReports(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CombineReports.ToString())
                 != null;
        }
        #endregion

        #region Company Address
        public static bool CanTrashCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashCompanyAddress.ToString())
                 != null;
        }

        public static bool CanCreateCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateCompanyAddress.ToString())
                 != null;
        }

        public static bool CanEditCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditCompanyAddress.ToString())
                 != null;
        }

        public static bool CanViewCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewCompanyAddress.ToString())
                 != null;
        }

        public static bool CanRestoreCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreCompanyAddress.ToString())
                 != null;
        }


        #endregion

        #region Company Contact
        public static bool CanTrashCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashCompanyContact.ToString())
                 != null;
        }

        public static bool CanCreateCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateCompanyContact.ToString())
                 != null;
        }

        public static bool CanEditCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditCompanyContact.ToString())
                 != null;
        }

        public static bool CanViewCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewCompanyContact.ToString())
                 != null;
        }

        public static bool CanRestoreCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreCompanyContact.ToString())
                 != null;
        }


        #endregion

        #region Company Email
        public static bool CanTrashCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashCompanyEmail.ToString())
                 != null;
        }

        public static bool CanCreateCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateCompanyEmail.ToString())
                 != null;
        }

        public static bool CanEditCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditCompanyEmail.ToString())
                 != null;
        }

        public static bool CanViewCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewCompanyEmail.ToString())
                 != null;
        }

        public static bool CanRestoreCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreCompanyEmail.ToString())
                 != null;
        }


        #endregion

        #region Company License
        public static bool CanTrashCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashCompanyLicense.ToString())
                 != null;
        }

        public static bool CanCreateCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateCompanyLicense.ToString())
                 != null;
        }

        public static bool CanEditCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditCompanyLicense.ToString())
                 != null;
        }

        public static bool CanViewCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewCompanyLicense.ToString())
                 != null;
        }

        public static bool CanRestoreCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreCompanyLicense.ToString())
                 != null;
        }


        #endregion

        #region Staff Members
        public static bool CanTrashStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashStaffMember.ToString())
                 != null;
        }

        public static bool CanCreateStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateStaffMember.ToString())
                 != null;
        }

        public static bool CanEditStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditStaffMember.ToString())
                 != null;
        }

        public static bool CanViewStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewStaffMember.ToString())
                 != null;
        }

        public static bool CanRestoreStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreStaffMember.ToString())
                 != null;
        }


        #endregion

        #region Designation
        public static bool CanTrashDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashDesignation.ToString())
                 != null;
        }

        public static bool CanCreateDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateDesignation.ToString())
                 != null;
        }

        public static bool CanEditDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditDesignation.ToString())
                 != null;
        }

        public static bool CanViewDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewDesignation.ToString())
                 != null;
        }

        public static bool CanRestoreDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreDesignation.ToString())
                 != null;
        }


        #endregion



        #region Vehicle
        public static bool CanTrashVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashVehicle.ToString())
                 != null;
        }

        public static bool CanCreateVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateVehicle.ToString())
                 != null;
        }

        public static bool CanEditVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditVehicle.ToString())
                 != null;
        }

        public static bool CanViewVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewVehicle.ToString())
                 != null;
        }

        public static bool CanRestoreVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreVehicle.ToString())
                 != null;
        }


        #endregion

        #region Route
        public static bool CanTrashRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashRoute.ToString())
                 != null;
        }

        public static bool CanCreateRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateRoute.ToString())
                 != null;
        }

        public static bool CanEditRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditRoute.ToString())
                 != null;
        }

        public static bool CanViewRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewRoute.ToString())
                 != null;
        }

        public static bool CanRestoreRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreRoute.ToString())
                 != null;
        }


        #endregion

        #region Travel Route
        public static bool CanTrashTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashTravelRoute.ToString())
                 != null;
        }

        public static bool CanCreateTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateTravelRoute.ToString())
                 != null;
        }

        public static bool CanEditTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditTravelRoute.ToString())
                 != null;
        }

        public static bool CanViewTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewTravelRoute.ToString())
                 != null;
        }

        public static bool CanRestoreTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreTravelRoute.ToString())
                 != null;
        }


        #endregion

        #region Reports
        public static bool CanViewLogisticReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewLogisticReport.ToString())
                 != null;
        }
        #endregion



        #region Room
        public static bool CanTrashRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashRoom.ToString())
                 != null;
        }

        public static bool CanCreateRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateRoom.ToString())
                 != null;
        }

        public static bool CanEditRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditRoom.ToString())
                 != null;
        }

        public static bool CanViewRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewRoom.ToString())
                 != null;
        }

        public static bool CanRestoreRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreRoom.ToString())
                 != null;
        }


        #endregion

        #region Place
        public static bool CanTrashPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashPlace.ToString())
                 != null;
        }

        public static bool CanCreatePlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreatePlace.ToString())
                 != null;
        }

        public static bool CanEditPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditPlace.ToString())
                 != null;
        }

        public static bool CanViewPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewPlace.ToString())
                 != null;
        }

        public static bool CanRestorePlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestorePlace.ToString())
                 != null;
        }


        #endregion

        #region Accomodation
        public static bool CanTrashAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashAccomodation.ToString())
                 != null;
        }

        public static bool CanCreateAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateAccomodation.ToString())
                 != null;
        }

        public static bool CanEditAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditAccomodation.ToString())
                 != null;
        }

        public static bool CanViewAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewAccomodation.ToString())
                 != null;
        }

        public static bool CanRestoreAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreAccomodation.ToString())
                 != null;
        }


        #endregion

        #region Accomodation Category
        public static bool CanTrashAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashAccomodationCategory.ToString())
                 != null;
        }

        public static bool CanCreateAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateAccomodationCategory.ToString())
                 != null;
        }

        public static bool CanEditAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditAccomodationCategory.ToString())
                 != null;
        }

        public static bool CanViewAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewAccomodationCategory.ToString())
                 != null;
        }

        public static bool CanRestoreAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreAccomodationCategory.ToString())
                 != null;
        }


        #endregion

        #region Food Package
        public static bool CanTrashFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashFoodPackage.ToString())
                 != null;
        }

        public static bool CanCreateFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateFoodPackage.ToString())
                 != null;
        }

        public static bool CanEditFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditFoodPackage.ToString())
                 != null;
        }

        public static bool CanViewFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewFoodPackage.ToString())
                 != null;
        }

        public static bool CanRestoreFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreFoodPackage.ToString())
                 != null;
        }


        #endregion

        #region Gift
        public static bool CanTrashGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashGift.ToString())
                 != null;
        }

        public static bool CanCreateGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateGift.ToString())
                 != null;
        }

        public static bool CanEditGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditGift.ToString())
                 != null;
        }

        public static bool CanViewGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewGift.ToString())
                 != null;
        }

        public static bool CanRestoreGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreGift.ToString())
                 != null;
        }


        #endregion

        #region Training
        public static bool CanTrashTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashTraining.ToString())
                 != null;
        }

        public static bool CanCreateTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateTraining.ToString())
                 != null;
        }

        public static bool CanEditTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditTraining.ToString())
                 != null;
        }

        public static bool CanViewTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewTraining.ToString())
                 != null;
        }

        public static bool CanRestoreTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreTraining.ToString())
                 != null;
        }


        #endregion

        #region Banks
        public static bool CanTrashBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashBank.ToString())
                 != null;
        }

        public static bool CanCreateBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateBank.ToString())
                 != null;
        }

        public static bool CanEditBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditBank.ToString())
                 != null;
        }

        public static bool CanViewBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewBank.ToString())
                 != null;
        }

        public static bool CanRestoreBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreBank.ToString())
                 != null;
        }


        #endregion

        #region Honorifics
        public static bool CanTrashHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashHonorifics.ToString())
                 != null;
        }

        public static bool CanCreateHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateHonorifics.ToString())
                 != null;
        }

        public static bool CanEditHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditHonorifics.ToString())
                 != null;
        }

        public static bool CanViewHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewHonorifics.ToString())
                 != null;
        }

        public static bool CanRestoreHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreHonorifics.ToString())
                 != null;
        }


        #endregion

        #region Fiqahs
        public static bool CanTrashFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashFiqah.ToString())
                 != null;
        }

        public static bool CanCreateFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateFiqah.ToString())
                 != null;
        }

        public static bool CanEditFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditFiqah.ToString())
                 != null;
        }

        public static bool CanViewFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewFiqah.ToString())
                 != null;
        }

        public static bool CanRestoreFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreFiqah.ToString())
                 != null;
        }


        #endregion

        #region Tehsils
        public static bool CanTrashTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashTehsil.ToString())
                 != null;
        }

        public static bool CanCreateTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateTehsil.ToString())
                 != null;
        }

        public static bool CanEditTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditTehsil.ToString())
                 != null;
        }

        public static bool CanViewTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewTehsil.ToString())
                 != null;
        }

        public static bool CanRestoreTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreTehsil.ToString())
                 != null;
        }


        #endregion

        #region References
        public static bool CanTrashReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashReference.ToString())
                 != null;
        }

        public static bool CanCreateReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateReference.ToString())
                 != null;
        }

        public static bool CanEditReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditReference.ToString())
                 != null;
        }

        public static bool CanViewReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewReference.ToString())
                 != null;
        }

        public static bool CanRestoreReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreReference.ToString())
                 != null;
        }


        #endregion

        #region Family Head 
        public static bool CanTrashFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashFamilyHeadRel.ToString())
                 != null;
        }

        public static bool CanCreateFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateFamilyHeadRel.ToString())
                 != null;
        }

        public static bool CanEditFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditFamilyHeadRel.ToString())
                 != null;
        }

        public static bool CanViewFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewFamilyHeadRel.ToString())
                 != null;
        }

        public static bool CanRestoreFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreFamilyHeadRel.ToString())
                 != null;
        }


        #endregion

        #region Mehram Relations 
        public static bool CanTrashMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashMehramRel.ToString())
                 != null;
        }

        public static bool CanCreateMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateMehramRel.ToString())
                 != null;
        }

        public static bool CanEditMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditMehramRel.ToString())
                 != null;
        }

        public static bool CanViewMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewMehramRel.ToString())
                 != null;
        }

        public static bool CanRestoreMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreMehramRel.ToString())
                 != null;
        }


        #endregion

        #region Nominee Relations 
        public static bool CanTrashNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashNominee.ToString())
                 != null;
        }

        public static bool CanCreateNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateNominee.ToString())
                 != null;
        }

        public static bool CanEditNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditNominee.ToString())
                 != null;
        }

        public static bool CanViewNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewNominee.ToString())
                 != null;
        }

        public static bool CanRestoreNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreNominee.ToString())
                 != null;
        }


        #endregion

        #region Required Documents
        public static bool CanTrashReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashReqDocs.ToString())
                 != null;
        }

        public static bool CanCreateReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateReqDocs.ToString())
                 != null;
        }

        public static bool CanEditReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditReqDocs.ToString())
                 != null;
        }

        public static bool CanViewReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewReqDocs.ToString())
                 != null;
        }

        public static bool CanRestoreReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreReqDocs.ToString())
                 != null;
        }


        #endregion

        #region Payment Notes
        public static bool CanTrashPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashPaymentNotes.ToString())
                 != null;
        }

        public static bool CanCreatePaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreatePaymentNotes.ToString())
                 != null;
        }

        public static bool CanEditPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditPaymentNotes.ToString())
                 != null;
        }

        public static bool CanViewPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewPaymentNotes.ToString())
                 != null;
        }

        public static bool CanRestorePaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestorePaymentNotes.ToString())
                 != null;
        }


        #endregion

        #region Vaccine
        public static bool CanTrashVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashVaccine.ToString())
                 != null;
        }

        public static bool CanCreateVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateVaccine.ToString())
                 != null;
        }

        public static bool CanEditVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditVaccine.ToString())
                 != null;
        }

        public static bool CanViewVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewVaccine.ToString())
                 != null;
        }

        public static bool CanRestoreVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreVaccine.ToString())
                 != null;
        }


        #endregion

        #region Quotatation Payment Pol
        public static bool CanTrashQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashQuotationPayPol.ToString())
                 != null;
        }

        public static bool CanCreateQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateQuotationPayPol.ToString())
                 != null;
        }

        public static bool CanEditQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditQuotationPayPol.ToString())
                 != null;
        }

        public static bool CanViewQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewQuotationPayPol.ToString())
                 != null;
        }

        public static bool CanRestoreQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreQuotationPayPol.ToString())
                 != null;
        }


        #endregion

        #region Quotatation T & C
        public static bool CanTrashQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashQuotationTC.ToString())
                 != null;
        }

        public static bool CanCreateQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateQuotationTC.ToString())
                 != null;
        }

        public static bool CanEditQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditQuotationTC.ToString())
                 != null;
        }

        public static bool CanViewQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewQuotationTC.ToString())
                 != null;
        }

        public static bool CanRestoreQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreQuotationTC.ToString())
                 != null;
        }


        #endregion

        #region Packages
        public static bool CanTrashPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashPackages.ToString())
                 != null;
        }

        public static bool CanCreatePackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreatePackages.ToString())
                 != null;
        }

        public static bool CanEditPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditPackages.ToString())
                 != null;
        }

        public static bool CanViewPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewPackages.ToString())
                 != null;
        }

        public static bool CanRestorePackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestorePackages.ToString())
                 != null;
        }

        public static bool CanViewReportPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ReportPackages.ToString())
                 != null;
        }
        public static bool CanCopyPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CopyPackages.ToString())
                 != null;
        }
        public static bool CanViewDetailsPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.DetailsPackages.ToString())
                 != null;
        }


        #endregion

        #region Quotation Packages
        public static bool CanTrashQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashQuotationPackages.ToString())
                 != null;
        }

        public static bool CanCustomizeQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CustomizeQuotationPackages.ToString())
                 != null;
        }

        public static bool CanEditQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditQuotationPackages.ToString())
                 != null;
        }

        public static bool CanViewQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewQuotationPackages.ToString())
                 != null;
        }

        public static bool CanRestoreQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreQuotationPackages.ToString())
                 != null;
        }

        public static bool CanGetQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.GetQuotationPackages.ToString())
                 != null;
        }

        public static bool CanViewDetailsQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.DetailsQuotationPackages.ToString())
                 != null;
        }

        public static bool CanBookQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.BookQuotationPackages.ToString())
                 != null;
        }

        #endregion


        #region Member Type
        public static bool CanTrashMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashMemberType.ToString())
                 != null;
        }

        public static bool CanCreateMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateMemberType.ToString())
                 != null;
        }

        public static bool CanEditMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditMemberType.ToString())
                 != null;
        }

        public static bool CanViewMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewMemberType.ToString())
                 != null;
        }

        public static bool CanRestoreMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreMemberType.ToString())
                 != null;
        }


        #endregion

        #region Member Address
        public static bool CanTrashMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashMemberAddress.ToString())
                 != null;
        }

        public static bool CanCreateMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateMemberAddress.ToString())
                 != null;
        }

        public static bool CanEditMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditMemberAddress.ToString())
                 != null;
        }

        public static bool CanViewMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewMemberAddress.ToString())
                 != null;
        }

        public static bool CanRestoreMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreMemberAddress.ToString())
                 != null;
        }


        #endregion

        #region Member Contact
        public static bool CanTrashMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashMemberContact.ToString())
                 != null;
        }

        public static bool CanCreateMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateMemberContact.ToString())
                 != null;
        }

        public static bool CanEditMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditMemberContact.ToString())
                 != null;
        }

        public static bool CanViewMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewMemberContact.ToString())
                 != null;
        }

        public static bool CanRestoreMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreMemberContact.ToString())
                 != null;
        }


        #endregion

        #region Member Email
        public static bool CanTrashMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashMemberEmail.ToString())
                 != null;
        }

        public static bool CanCreateMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateMemberEmail.ToString())
                 != null;
        }

        public static bool CanEditMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditMemberEmail.ToString())
                 != null;
        }

        public static bool CanViewMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewMemberEmail.ToString())
                 != null;
        }

        public static bool CanRestoreMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreMemberEmail.ToString())
                 != null;
        }


        #endregion


        #region Airlines
        public static bool CanTrashAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashAirlines.ToString())
                 != null;
        }

        public static bool CanCreateAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateAirlines.ToString())
                 != null;
        }

        public static bool CanEditAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditAirlines.ToString())
                 != null;
        }

        public static bool CanViewAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewAirlines.ToString())
                 != null;
        }

        public static bool CanRestoreAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreAirlines.ToString())
                 != null;
        }


        #endregion

        #region Airline Class
        public static bool CanTrashAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashAirlineClass.ToString())
                 != null;
        }

        public static bool CanCreateAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateAirlineClass.ToString())
                 != null;
        }

        public static bool CanEditAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditAirlineClass.ToString())
                 != null;
        }

        public static bool CanViewAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewAirlineClass.ToString())
                 != null;
        }

        public static bool CanRestoreAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreAirlineClass.ToString())
                 != null;
        }


        #endregion

        #region Airline Fare
        public static bool CanTrashAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashAirlineFare.ToString())
                 != null;
        }

        public static bool CanCreateAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateAirlineFare.ToString())
                 != null;
        }

        public static bool CanEditAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditAirlineFare.ToString())
                 != null;
        }

        public static bool CanViewAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewAirlineFare.ToString())
                 != null;
        }

        public static bool CanRestoreAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreAirlineFare.ToString())
                 != null;
        }


        #endregion



        #region Families
        public static bool CanTrashFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashFamily.ToString())
                 != null;
        }

        public static bool CanCreateFamilyHead(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateFamilyHead.ToString())
                 != null;
        }

        public static bool CanEditFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditFamily.ToString())
                 != null;
        }

        public static bool CanViewFamilies(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewFamilies.ToString())
                 != null;
        }

        public static bool CanRestoreFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreFamily.ToString())
                 != null;
        }

        public static bool CanViewFamilyMembers(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewFamilyMembers.ToString())
                 != null;
        }

        public static bool CanViewFamilyLetterHead(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewFamilyLetterHead.ToString())
                 != null;
        }

        public static bool CanViewFamilyInvoice(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewFamilyInvoice.ToString())
                 != null;
        }

        public static bool CanCreateFamilyMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateFamilyMember.ToString())
                 != null;
        }

        public static bool CanVerifyFamilyMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.VerifyMember.ToString())
                 != null;
        }
        #endregion

        #region Receipts
        public static bool CanTrashReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.TrashReceipts.ToString())
                != null;
        }

        public static bool CanCreateReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.CreateReceipts.ToString())
                != null;
        }

        public static bool CanEditReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.EditReceipts.ToString())
                != null;
        }

        public static bool CanViewReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewReceipts.ToString())
                != null;
        }

        public static bool CanRestoreReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.RestoreReceipts.ToString())
                != null;
        }

        public static bool CanChangeReceiptStatus(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ChangeReceiptStatus.ToString())
                != null;
        }

        public static bool CanViewSlip(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewSlip.ToString())
                != null;
        }

        #endregion

        public static bool CanViewClientStatement(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewClientStatement.ToString())
                 != null;
        }

        public static bool CanViewMiniStatement(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePrivileges
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) 
                && rp.Privilege == Privileges.ViewMiniStatement.ToString())
                 != null;
        }
    }
}

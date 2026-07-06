using TMG.Data;
using TMG.Models;
using static TMG.ManagerClasses.PrivilegeManager;

namespace TMG.ManagerClasses
{
    public static class PrivilegeManager
    {

        #region Hotel
        public static bool CanTrashHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashHotel)
                .Allowed;
        }

        public static bool CanCreateHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateHotel)
                .Allowed;
        }

        public static bool CanEditHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditHotel)
                .Allowed;
        }

        public static bool CanViewHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewHotel)
                .Allowed;
        }

        public static bool CanRestoreHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreHotel)
                .Allowed;
        }


        #endregion

        #region Room Allotment
        public static bool CanTrashRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRoomAllotment)
                .Allowed;
        }

        public static bool CanCreateRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRoomAllotment)
                .Allowed;
        }

        public static bool CanEditRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRoomAllotment)
                .Allowed;
        }

        public static bool CanViewRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoomAllotment)
                .Allowed;
        }

        public static bool CanRestoreRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRoomAllotment)
                .Allowed;
        }


        #endregion

        public static bool CanViewRoomSummaryReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoomSummaryReport)
                .Allowed;
        }
        public static bool CanViewDailyRoomSummaryReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewDailyRoomSummaryReport)
                .Allowed;
        }

        #region User
        public static bool CanTrashUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashUser)
                .Allowed;
        }

        public static bool CanCreateUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateUser)
                .Allowed;
        }

        public static bool CanEditUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditUser)
                .Allowed;
        }

        public static bool CanViewUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewUser)
                .Allowed;
        }

        public static bool CanRestoreUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreUser)
                .Allowed;
        }
        #endregion

        #region Role
        public static bool CanTrashRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRole)
                .Allowed;
        }

        public static bool CanCreateRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRole)
                .Allowed;
        }

        public static bool CanEditRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRole)
                .Allowed;
        }

        public static bool CanViewRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRole)
                .Allowed;
        }

        public static bool CanRestoreRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRole)
                .Allowed;
        }

        public static bool CanSetRolePrivilege(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.SetRolePrivelege)
                .Allowed;
        }
        #endregion

        #region Company
        public static bool CanTrashCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompany)
                .Allowed;
        }

        public static bool CanCreateCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompany)
                .Allowed;
        }

        public static bool CanEditCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompany)
                .Allowed;
        }

        public static bool CanViewCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompany)
                .Allowed;
        }

        public static bool CanRestoreCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompany)
                .Allowed;
        }

        public static bool CanViewIndividualReports(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.IndividualReports)
                .Allowed;
        }

        public static bool CanViewCombineReports(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CombineReports)
                .Allowed;
        }
        #endregion

        #region Company Address
        public static bool CanTrashCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyAddress)
                .Allowed;
        }

        public static bool CanCreateCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyAddress)
                .Allowed;
        }

        public static bool CanEditCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyAddress)
                .Allowed;
        }

        public static bool CanViewCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyAddress)
                .Allowed;
        }

        public static bool CanRestoreCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyAddress)
                .Allowed;
        }


        #endregion

        #region Company Contact
        public static bool CanTrashCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyContact)
                .Allowed;
        }

        public static bool CanCreateCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyContact)
                .Allowed;
        }

        public static bool CanEditCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyContact)
                .Allowed;
        }

        public static bool CanViewCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyContact)
                .Allowed;
        }

        public static bool CanRestoreCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyContact)
                .Allowed;
        }


        #endregion

        #region Company Email
        public static bool CanTrashCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyEmail)
                .Allowed;
        }

        public static bool CanCreateCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyEmail)
                .Allowed;
        }

        public static bool CanEditCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyEmail)
                .Allowed;
        }

        public static bool CanViewCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyEmail)
                .Allowed;
        }

        public static bool CanRestoreCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyEmail)
                .Allowed;
        }


        #endregion

        #region Company License
        public static bool CanTrashCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyLicense)
                .Allowed;
        }

        public static bool CanCreateCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyLicense)
                .Allowed;
        }

        public static bool CanEditCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyLicense)
                .Allowed;
        }

        public static bool CanViewCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyLicense)
                .Allowed;
        }

        public static bool CanRestoreCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyLicense)
                .Allowed;
        }


        #endregion

        #region Staff Members
        public static bool CanTrashStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashStaffMember)
                .Allowed;
        }

        public static bool CanCreateStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateStaffMember)
                .Allowed;
        }

        public static bool CanEditStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditStaffMember)
                .Allowed;
        }

        public static bool CanViewStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewStaffMember)
                .Allowed;
        }

        public static bool CanRestoreStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreStaffMember)
                .Allowed;
        }


        #endregion

        #region Designation
        public static bool CanTrashDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashDesignation)
                .Allowed;
        }

        public static bool CanCreateDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateDesignation)
                .Allowed;
        }

        public static bool CanEditDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditDesignation)
                .Allowed;
        }

        public static bool CanViewDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewDesignation)
                .Allowed;
        }

        public static bool CanRestoreDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreDesignation)
                .Allowed;
        }


        #endregion



        #region Vehicle
        public static bool CanTrashVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashVehicle)
                .Allowed;
        }

        public static bool CanCreateVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateVehicle)
                .Allowed;
        }

        public static bool CanEditVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditVehicle)
                .Allowed;
        }

        public static bool CanViewVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewVehicle)
                .Allowed;
        }

        public static bool CanRestoreVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreVehicle)
                .Allowed;
        }


        #endregion

        #region Route
        public static bool CanTrashRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRoute)
                .Allowed;
        }

        public static bool CanCreateRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRoute)
                .Allowed;
        }

        public static bool CanEditRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRoute)
                .Allowed;
        }

        public static bool CanViewRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoute)
                .Allowed;
        }

        public static bool CanRestoreRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRoute)
                .Allowed;
        }


        #endregion

        #region Travel Route
        public static bool CanTrashTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashTravelRoute)
                .Allowed;
        }

        public static bool CanCreateTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateTravelRoute)
                .Allowed;
        }

        public static bool CanEditTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditTravelRoute)
                .Allowed;
        }

        public static bool CanViewTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewTravelRoute)
                .Allowed;
        }

        public static bool CanRestoreTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreTravelRoute)
                .Allowed;
        }


        #endregion

        #region Reports
        public static bool CanViewLogisticReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewLogisticReport)
                .Allowed;
        }
        #endregion



        #region Room
        public static bool CanTrashRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRoom)
                .Allowed;
        }

        public static bool CanCreateRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRoom)
                .Allowed;
        }

        public static bool CanEditRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRoom)
                .Allowed;
        }

        public static bool CanViewRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoom)
                .Allowed;
        }

        public static bool CanRestoreRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRoom)
                .Allowed;
        }


        #endregion

        #region Place
        public static bool CanTrashPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPlace)
                .Allowed;
        }

        public static bool CanCreatePlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePlace)
                .Allowed;
        }

        public static bool CanEditPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPlace)
                .Allowed;
        }

        public static bool CanViewPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPlace)
                .Allowed;
        }

        public static bool CanRestorePlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePlace)
                .Allowed;
        }


        #endregion

        #region Accomodation
        public static bool CanTrashAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAccomodation)
                .Allowed;
        }

        public static bool CanCreateAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAccomodation)
                .Allowed;
        }

        public static bool CanEditAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAccomodation)
                .Allowed;
        }

        public static bool CanViewAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAccomodation)
                .Allowed;
        }

        public static bool CanRestoreAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAccomodation)
                .Allowed;
        }


        #endregion

        #region Accomodation Category
        public static bool CanTrashAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAccomodationCategory)
                .Allowed;
        }

        public static bool CanCreateAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAccomodationCategory)
                .Allowed;
        }

        public static bool CanEditAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAccomodationCategory)
                .Allowed;
        }

        public static bool CanViewAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAccomodationCategory)
                .Allowed;
        }

        public static bool CanRestoreAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAccomodationCategory)
                .Allowed;
        }


        #endregion

        #region Food Package
        public static bool CanTrashFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFoodPackage)
                .Allowed;
        }

        public static bool CanCreateFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFoodPackage)
                .Allowed;
        }

        public static bool CanEditFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFoodPackage)
                .Allowed;
        }

        public static bool CanViewFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFoodPackage)
                .Allowed;
        }

        public static bool CanRestoreFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFoodPackage)
                .Allowed;
        }


        #endregion

        #region Gift
        public static bool CanTrashGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashGift)
                .Allowed;
        }

        public static bool CanCreateGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateGift)
                .Allowed;
        }

        public static bool CanEditGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditGift)
                .Allowed;
        }

        public static bool CanViewGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewGift)
                .Allowed;
        }

        public static bool CanRestoreGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreGift)
                .Allowed;
        }


        #endregion

        #region Training
        public static bool CanTrashTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashTraining)
                .Allowed;
        }

        public static bool CanCreateTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateTraining)
                .Allowed;
        }

        public static bool CanEditTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditTraining)
                .Allowed;
        }

        public static bool CanViewTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewTraining)
                .Allowed;
        }

        public static bool CanRestoreTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreTraining)
                .Allowed;
        }


        #endregion

        #region Banks
        public static bool CanTrashBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashBank)
                .Allowed;
        }

        public static bool CanCreateBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateBank)
                .Allowed;
        }

        public static bool CanEditBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditBank)
                .Allowed;
        }

        public static bool CanViewBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewBank)
                .Allowed;
        }

        public static bool CanRestoreBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreBank)
                .Allowed;
        }


        #endregion

        #region Honorifics
        public static bool CanTrashHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashHonorifics)
                .Allowed;
        }

        public static bool CanCreateHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateHonorifics)
                .Allowed;
        }

        public static bool CanEditHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditHonorifics)
                .Allowed;
        }

        public static bool CanViewHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewHonorifics)
                .Allowed;
        }

        public static bool CanRestoreHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreHonorifics)
                .Allowed;
        }


        #endregion

        #region Fiqahs
        public static bool CanTrashFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFiqah)
                .Allowed;
        }

        public static bool CanCreateFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFiqah)
                .Allowed;
        }

        public static bool CanEditFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFiqah)
                .Allowed;
        }

        public static bool CanViewFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFiqah)
                .Allowed;
        }

        public static bool CanRestoreFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFiqah)
                .Allowed;
        }


        #endregion

        #region Tehsils
        public static bool CanTrashTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashTehsil)
                .Allowed;
        }

        public static bool CanCreateTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateTehsil)
                .Allowed;
        }

        public static bool CanEditTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditTehsil)
                .Allowed;
        }

        public static bool CanViewTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewTehsil)
                .Allowed;
        }

        public static bool CanRestoreTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreTehsil)
                .Allowed;
        }


        #endregion

        #region References
        public static bool CanTrashReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashReference)
                .Allowed;
        }

        public static bool CanCreateReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateReference)
                .Allowed;
        }

        public static bool CanEditReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditReference)
                .Allowed;
        }

        public static bool CanViewReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewReference)
                .Allowed;
        }

        public static bool CanRestoreReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreReference)
                .Allowed;
        }


        #endregion

        #region Family Head 
        public static bool CanTrashFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFamilyHeadRel)
                .Allowed;
        }

        public static bool CanCreateFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFamilyHeadRel)
                .Allowed;
        }

        public static bool CanEditFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFamilyHeadRel)
                .Allowed;
        }

        public static bool CanViewFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyHeadRel)
                .Allowed;
        }

        public static bool CanRestoreFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFamilyHeadRel)
                .Allowed;
        }


        #endregion

        #region Mehram Relations 
        public static bool CanTrashMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMehramRel)
                .Allowed;
        }

        public static bool CanCreateMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMehramRel)
                .Allowed;
        }

        public static bool CanEditMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMehramRel)
                .Allowed;
        }

        public static bool CanViewMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMehramRel)
                .Allowed;
        }

        public static bool CanRestoreMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMehramRel)
                .Allowed;
        }


        #endregion

        #region Nominee Relations 
        public static bool CanTrashNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashNominee)
                .Allowed;
        }

        public static bool CanCreateNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateNominee)
                .Allowed;
        }

        public static bool CanEditNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditNominee)
                .Allowed;
        }

        public static bool CanViewNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewNominee)
                .Allowed;
        }

        public static bool CanRestoreNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreNominee)
                .Allowed;
        }


        #endregion

        #region Required Documents
        public static bool CanTrashReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashReqDocs)
                .Allowed;
        }

        public static bool CanCreateReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateReqDocs)
                .Allowed;
        }

        public static bool CanEditReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditReqDocs)
                .Allowed;
        }

        public static bool CanViewReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewReqDocs)
                .Allowed;
        }

        public static bool CanRestoreReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreReqDocs)
                .Allowed;
        }


        #endregion

        #region Payment Notes
        public static bool CanTrashPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPaymentNotes)
                .Allowed;
        }

        public static bool CanCreatePaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePaymentNotes)
                .Allowed;
        }

        public static bool CanEditPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPaymentNotes)
                .Allowed;
        }

        public static bool CanViewPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPaymentNotes)
                .Allowed;
        }

        public static bool CanRestorePaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePaymentNotes)
                .Allowed;
        }


        #endregion

        #region Vaccine
        public static bool CanTrashVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashVaccine)
                .Allowed;
        }

        public static bool CanCreateVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateVaccine)
                .Allowed;
        }

        public static bool CanEditVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditVaccine)
                .Allowed;
        }

        public static bool CanViewVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewVaccine)
                .Allowed;
        }

        public static bool CanRestoreVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreVaccine)
                .Allowed;
        }


        #endregion

        #region Quotatation Payment Pol
        public static bool CanTrashQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashQuotationPayPol)
                .Allowed;
        }

        public static bool CanCreateQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateQuotationPayPol)
                .Allowed;
        }

        public static bool CanEditQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditQuotationPayPol)
                .Allowed;
        }

        public static bool CanViewQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewQuotationPayPol)
                .Allowed;
        }

        public static bool CanRestoreQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreQuotationPayPol)
                .Allowed;
        }


        #endregion

        #region Quotatation T & C
        public static bool CanTrashQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashQuotationTC)
                .Allowed;
        }

        public static bool CanCreateQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateQuotationTC)
                .Allowed;
        }

        public static bool CanEditQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditQuotationTC)
                .Allowed;
        }

        public static bool CanViewQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewQuotationTC)
                .Allowed;
        }

        public static bool CanRestoreQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreQuotationTC)
                .Allowed;
        }


        #endregion

        #region Packages
        public static bool CanTrashPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPackages)
                .Allowed;
        }

        public static bool CanCreatePackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePackages)
                .Allowed;
        }

        public static bool CanEditPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPackages)
                .Allowed;
        }

        public static bool CanViewPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPackages)
                .Allowed;
        }

        public static bool CanRestorePackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePackages)
                .Allowed;
        }

        public static bool CanViewReportPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ReportPackages)
                .Allowed;
        }
        public static bool CanCopyPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CopyPackages)
                .Allowed;
        }
        public static bool CanViewDetailsPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.DetailsPackages)
                .Allowed;
        }


        #endregion

        #region Quotation Packages
        public static bool CanTrashQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashQuotationPackages)
                .Allowed;
        }

        public static bool CanCustomizeQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CustomizeQuotationPackages)
                .Allowed;
        }

        public static bool CanEditQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditQuotationPackages)
                .Allowed;
        }

        public static bool CanViewQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewQuotationPackages)
                .Allowed;
        }

        public static bool CanRestoreQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreQuotationPackages)
                .Allowed;
        }

        public static bool CanGetQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.GetQuotationPackages)
                .Allowed;
        }

        public static bool CanViewDetailsQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.DetailsQuotationPackages)
                .Allowed;
        }

        public static bool CanBookQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.BookQuotationPackages)
                .Allowed;
        }

        #endregion


        #region Member Type
        public static bool CanTrashMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberType)
                .Allowed;
        }

        public static bool CanCreateMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberType)
                .Allowed;
        }

        public static bool CanEditMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberType)
                .Allowed;
        }

        public static bool CanViewMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberType)
                .Allowed;
        }

        public static bool CanRestoreMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberType)
                .Allowed;
        }


        #endregion

        #region Member Address
        public static bool CanTrashMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberAddress)
                .Allowed;
        }

        public static bool CanCreateMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberAddress)
                .Allowed;
        }

        public static bool CanEditMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberAddress)
                .Allowed;
        }

        public static bool CanViewMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberAddress)
                .Allowed;
        }

        public static bool CanRestoreMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberAddress)
                .Allowed;
        }


        #endregion

        #region Member Contact
        public static bool CanTrashMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberContact)
                .Allowed;
        }

        public static bool CanCreateMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberContact)
                .Allowed;
        }

        public static bool CanEditMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberContact)
                .Allowed;
        }

        public static bool CanViewMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberContact)
                .Allowed;
        }

        public static bool CanRestoreMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberContact)
                .Allowed;
        }


        #endregion

        #region Member Email
        public static bool CanTrashMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberEmail)
                .Allowed;
        }

        public static bool CanCreateMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberEmail)
                .Allowed;
        }

        public static bool CanEditMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberEmail)
                .Allowed;
        }

        public static bool CanViewMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberEmail)
                .Allowed;
        }

        public static bool CanRestoreMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberEmail)
                .Allowed;
        }


        #endregion


        #region Airlines
        public static bool CanTrashAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAirlines)
                .Allowed;
        }

        public static bool CanCreateAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAirlines)
                .Allowed;
        }

        public static bool CanEditAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAirlines)
                .Allowed;
        }

        public static bool CanViewAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAirlines)
                .Allowed;
        }

        public static bool CanRestoreAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAirlines)
                .Allowed;
        }


        #endregion

        #region Airline Class
        public static bool CanTrashAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAirlineClass)
                .Allowed;
        }

        public static bool CanCreateAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAirlineClass)
                .Allowed;
        }

        public static bool CanEditAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAirlineClass)
                .Allowed;
        }

        public static bool CanViewAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAirlineClass)
                .Allowed;
        }

        public static bool CanRestoreAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAirlineClass)
                .Allowed;
        }


        #endregion

        #region Airline Fare
        public static bool CanTrashAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAirlineFare)
                .Allowed;
        }

        public static bool CanCreateAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAirlineFare)
                .Allowed;
        }

        public static bool CanEditAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAirlineFare)
                .Allowed;
        }

        public static bool CanViewAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAirlineFare)
                .Allowed;
        }

        public static bool CanRestoreAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAirlineFare)
                .Allowed;
        }


        #endregion

        #region POTicket

        public static bool CanViewPOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPOTicket)
                .Allowed;
        }

        public static bool CanCreatePOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePOTicket)
                .Allowed;
        }

        public static bool CanEditPOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPOTicket)
                .Allowed;
        }

        public static bool CanTrashPOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPOTicket)
                .Allowed;
        }

        public static bool CanRestorePOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePOTicket)
                .Allowed;
        }

        #endregion

        #region Families
        public static bool CanTrashFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFamily)
                .Allowed;
        }

        public static bool CanCreateFamilyHead(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFamilyHead)
                .Allowed;
        }

        public static bool CanEditFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFamily)
                .Allowed;
        }

        public static bool CanViewFamilies(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilies)
                .Allowed;
        }

        public static bool CanRestoreFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFamily)
                .Allowed;
        }

        public static bool CanViewFamilyMembers(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyMembers)
                .Allowed;
        }

        public static bool CanViewFamilyLetterHead(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyLetterHead)
                .Allowed;
        }

        public static bool CanViewFamilyInvoice(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyInvoice)
                .Allowed;
        }

        public static bool CanCreateFamilyMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFamilyMember)
                .Allowed;
        }

        public static bool CanVerifyFamilyMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.VerifyMember)
                .Allowed;
        }


        #endregion


        #region Receipts
        public static bool CanTrashReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashReceipts)
                .Allowed;
        }

        public static bool CanCreateReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateReceipts)
                .Allowed;
        }

        public static bool CanEditReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditReceipts)
                .Allowed;
        }

        public static bool CanViewReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewReceipts)
                .Allowed;
        }

        public static bool CanRestoreReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreReceipts)
                .Allowed;
        }

        public static bool CanChangeReceiptStatus(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ChangeReceiptStatus)
                .Allowed;
        }

        public static bool CanViewSlip(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewSlip)
                .Allowed;
        }

        #endregion

        public static bool CanViewClientStatement(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewClientStatement)
                .Allowed;
        }

        public static bool CanViewMiniStatement(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMiniStatement)
                .Allowed;
        }
    }
}

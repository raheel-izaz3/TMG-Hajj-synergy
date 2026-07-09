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
                ?.Allowed ?? false;
        }

        public static bool CanCreateHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateHotel)
                ?.Allowed ?? false;
        }

        public static bool CanEditHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditHotel)
                ?.Allowed ?? false;
        }

        public static bool CanViewHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewHotel)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreHotel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreHotel)
                ?.Allowed ?? false;
        }


        #endregion

        #region Room Allotment
        public static bool CanTrashRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRoomAllotment)
                ?.Allowed ?? false;
        }

        public static bool CanCreateRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRoomAllotment)
                ?.Allowed ?? false;
        }

        public static bool CanEditRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRoomAllotment)
                ?.Allowed ?? false;
        }

        public static bool CanViewRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoomAllotment)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreRoomAllotment(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRoomAllotment)
                ?.Allowed ?? false;
        }


        #endregion

        public static bool CanViewRoomSummaryReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoomSummaryReport)
                ?.Allowed ?? false;
        }
        public static bool CanViewDailyRoomSummaryReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewDailyRoomSummaryReport)
                ?.Allowed ?? false;
        }

        #region User
        public static bool CanTrashUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashUser)
                ?.Allowed ?? false;
        }

        public static bool CanCreateUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateUser)
                ?.Allowed ?? false;
        }

        public static bool CanEditUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditUser)
                ?.Allowed ?? false;
        }

        public static bool CanViewUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewUser)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreUser(TMGDbContext dbContext, IHttpContextAccessor http)
        {

            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreUser)
                ?.Allowed ?? false;
        }
        #endregion

        #region Role
        public static bool CanTrashRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRole)
                ?.Allowed ?? false;
        }

        public static bool CanCreateRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRole)
                ?.Allowed ?? false;
        }

        public static bool CanEditRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRole)
                ?.Allowed ?? false;
        }

        public static bool CanViewRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRole)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreRole(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRole)
                ?.Allowed ?? false;
        }

        public static bool CanSetRolePrivilege(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.SetRolePrivelege)
                ?.Allowed ?? false;
        }
        #endregion

        #region Company
        public static bool CanTrashCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompany)
                ?.Allowed ?? false;
        }

        public static bool CanCreateCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompany)
                ?.Allowed ?? false;
        }

        public static bool CanEditCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompany)
                ?.Allowed ?? false;
        }

        public static bool CanViewCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompany)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreCompany(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompany)
                ?.Allowed ?? false;
        }

        public static bool CanViewIndividualReports(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.IndividualReports)
                ?.Allowed ?? false;
        }

        public static bool CanViewCombineReports(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CombineReports)
                ?.Allowed ?? false;
        }
        #endregion

        #region Company Address
        public static bool CanTrashCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyAddress)
                ?.Allowed ?? false;
        }

        public static bool CanCreateCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyAddress)
                ?.Allowed ?? false;
        }

        public static bool CanEditCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyAddress)
                ?.Allowed ?? false;
        }

        public static bool CanViewCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyAddress)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreCompanyAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyAddress)
                ?.Allowed ?? false;
        }


        #endregion

        #region Company Contact
        public static bool CanTrashCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyContact)
                ?.Allowed ?? false;
        }

        public static bool CanCreateCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyContact)
                ?.Allowed ?? false;
        }

        public static bool CanEditCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyContact)
                ?.Allowed ?? false;
        }

        public static bool CanViewCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyContact)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreCompanyContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyContact)
                ?.Allowed ?? false;
        }


        #endregion

        #region Company Email
        public static bool CanTrashCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyEmail)
                ?.Allowed ?? false;
        }

        public static bool CanCreateCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyEmail)
                ?.Allowed ?? false;
        }

        public static bool CanEditCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyEmail)
                ?.Allowed ?? false;
        }

        public static bool CanViewCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyEmail)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreCompanyEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyEmail)
                ?.Allowed ?? false;
        }


        #endregion

        #region Company License
        public static bool CanTrashCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashCompanyLicense)
                ?.Allowed ?? false;
        }

        public static bool CanCreateCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateCompanyLicense)
                ?.Allowed ?? false;
        }

        public static bool CanEditCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditCompanyLicense)
                ?.Allowed ?? false;
        }

        public static bool CanViewCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewCompanyLicense)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreCompanyLicense(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreCompanyLicense)
                ?.Allowed ?? false;
        }


        #endregion

        #region Staff Members
        public static bool CanTrashStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashStaffMember)
                ?.Allowed ?? false;
        }

        public static bool CanCreateStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateStaffMember)
                ?.Allowed ?? false;
        }

        public static bool CanEditStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditStaffMember)
                ?.Allowed ?? false;
        }

        public static bool CanViewStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewStaffMember)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreStaffMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreStaffMember)
                ?.Allowed ?? false;
        }


        #endregion

        #region Designation
        public static bool CanTrashDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashDesignation)
                ?.Allowed ?? false;
        }

        public static bool CanCreateDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateDesignation)
                ?.Allowed ?? false;
        }

        public static bool CanEditDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditDesignation)
                ?.Allowed ?? false;
        }

        public static bool CanViewDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewDesignation)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreDesignation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreDesignation)
                ?.Allowed ?? false;
        }


        #endregion



        #region Vehicle
        public static bool CanTrashVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashVehicle)
                ?.Allowed ?? false;
        }

        public static bool CanCreateVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateVehicle)
                ?.Allowed ?? false;
        }

        public static bool CanEditVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditVehicle)
                ?.Allowed ?? false;
        }

        public static bool CanViewVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewVehicle)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreVehicle(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreVehicle)
                ?.Allowed ?? false;
        }


        #endregion

        #region Route
        public static bool CanTrashRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRoute)
                ?.Allowed ?? false;
        }

        public static bool CanCreateRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRoute)
                ?.Allowed ?? false;
        }

        public static bool CanEditRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRoute)
                ?.Allowed ?? false;
        }

        public static bool CanViewRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoute)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRoute)
                ?.Allowed ?? false;
        }


        #endregion

        #region Travel Route
        public static bool CanTrashTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashTravelRoute)
                ?.Allowed ?? false;
        }

        public static bool CanCreateTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateTravelRoute)
                ?.Allowed ?? false;
        }

        public static bool CanEditTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditTravelRoute)
                ?.Allowed ?? false;
        }

        public static bool CanViewTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewTravelRoute)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreTravelRoute(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreTravelRoute)
                ?.Allowed ?? false;
        }


        #endregion

        #region Reports
        public static bool CanViewLogisticReport(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewLogisticReport)
                ?.Allowed ?? false;
        }
        #endregion



        #region Room
        public static bool CanTrashRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashRoom)
                ?.Allowed ?? false;
        }

        public static bool CanCreateRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateRoom)
                ?.Allowed ?? false;
        }

        public static bool CanEditRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditRoom)
                ?.Allowed ?? false;
        }

        public static bool CanViewRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewRoom)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreRoom(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreRoom)
                ?.Allowed ?? false;
        }


        #endregion

        #region Place
        public static bool CanTrashPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPlace)
                ?.Allowed ?? false;
        }

        public static bool CanCreatePlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePlace)
                ?.Allowed ?? false;
        }

        public static bool CanEditPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPlace)
                ?.Allowed ?? false;
        }

        public static bool CanViewPlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPlace)
                ?.Allowed ?? false;
        }

        public static bool CanRestorePlace(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePlace)
                ?.Allowed ?? false;
        }


        #endregion

        #region Accomodation
        public static bool CanTrashAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAccomodation)
                ?.Allowed ?? false;
        }

        public static bool CanCreateAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAccomodation)
                ?.Allowed ?? false;
        }

        public static bool CanEditAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAccomodation)
                ?.Allowed ?? false;
        }

        public static bool CanViewAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAccomodation)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreAccomodation(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAccomodation)
                ?.Allowed ?? false;
        }


        #endregion

        #region Accomodation Category
        public static bool CanTrashAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAccomodationCategory)
                ?.Allowed ?? false;
        }

        public static bool CanCreateAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAccomodationCategory)
                ?.Allowed ?? false;
        }

        public static bool CanEditAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAccomodationCategory)
                ?.Allowed ?? false;
        }

        public static bool CanViewAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAccomodationCategory)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreAccomodationCategory(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAccomodationCategory)
                ?.Allowed ?? false;
        }


        #endregion

        #region Food Package
        public static bool CanTrashFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFoodPackage)
                ?.Allowed ?? false;
        }

        public static bool CanCreateFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFoodPackage)
                ?.Allowed ?? false;
        }

        public static bool CanEditFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFoodPackage)
                ?.Allowed ?? false;
        }

        public static bool CanViewFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFoodPackage)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreFoodPackage(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFoodPackage)
                ?.Allowed ?? false;
        }


        #endregion

        #region Gift
        public static bool CanTrashGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashGift)
                ?.Allowed ?? false;
        }

        public static bool CanCreateGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateGift)
                ?.Allowed ?? false;
        }

        public static bool CanEditGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditGift)
                ?.Allowed ?? false;
        }

        public static bool CanViewGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewGift)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreGift(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreGift)
                ?.Allowed ?? false;
        }


        #endregion

        #region Training
        public static bool CanTrashTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashTraining)
                ?.Allowed ?? false;
        }

        public static bool CanCreateTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateTraining)
                ?.Allowed ?? false;
        }

        public static bool CanEditTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditTraining)
                ?.Allowed ?? false;
        }

        public static bool CanViewTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewTraining)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreTraining(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreTraining)
                ?.Allowed ?? false;
        }


        #endregion

        #region Banks
        public static bool CanTrashBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashBank)
                ?.Allowed ?? false;
        }

        public static bool CanCreateBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateBank)
                ?.Allowed ?? false;
        }

        public static bool CanEditBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditBank)
                ?.Allowed ?? false;
        }

        public static bool CanViewBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewBank)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreBank(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreBank)
                ?.Allowed ?? false;
        }


        #endregion

        #region Honorifics
        public static bool CanTrashHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashHonorifics)
                ?.Allowed ?? false;
        }

        public static bool CanCreateHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateHonorifics)
                ?.Allowed ?? false;
        }

        public static bool CanEditHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditHonorifics)
                ?.Allowed ?? false;
        }

        public static bool CanViewHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewHonorifics)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreHonorifics(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreHonorifics)
                ?.Allowed ?? false;
        }


        #endregion

        #region Fiqahs
        public static bool CanTrashFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFiqah)
                ?.Allowed ?? false;
        }

        public static bool CanCreateFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFiqah)
                ?.Allowed ?? false;
        }

        public static bool CanEditFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFiqah)
                ?.Allowed ?? false;
        }

        public static bool CanViewFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFiqah)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreFiqah(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFiqah)
                ?.Allowed ?? false;
        }


        #endregion

        #region Tehsils
        public static bool CanTrashTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashTehsil)
                ?.Allowed ?? false;
        }

        public static bool CanCreateTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateTehsil)
                ?.Allowed ?? false;
        }

        public static bool CanEditTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditTehsil)
                ?.Allowed ?? false;
        }

        public static bool CanViewTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewTehsil)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreTehsil(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreTehsil)
                ?.Allowed ?? false;
        }


        #endregion

        #region References
        public static bool CanTrashReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashReference)
                ?.Allowed ?? false;
        }

        public static bool CanCreateReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateReference)
                ?.Allowed ?? false;
        }

        public static bool CanEditReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditReference)
                ?.Allowed ?? false;
        }

        public static bool CanViewReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewReference)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreReference(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreReference)
                ?.Allowed ?? false;
        }


        #endregion

        #region Family Head 
        public static bool CanTrashFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFamilyHeadRel)
                ?.Allowed ?? false;
        }

        public static bool CanCreateFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFamilyHeadRel)
                ?.Allowed ?? false;
        }

        public static bool CanEditFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFamilyHeadRel)
                ?.Allowed ?? false;
        }

        public static bool CanViewFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyHeadRel)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreFamilyHeadRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFamilyHeadRel)
                ?.Allowed ?? false;
        }


        #endregion

        #region Mehram Relations 
        public static bool CanTrashMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMehramRel)
                ?.Allowed ?? false;
        }

        public static bool CanCreateMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMehramRel)
                ?.Allowed ?? false;
        }

        public static bool CanEditMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMehramRel)
                ?.Allowed ?? false;
        }

        public static bool CanViewMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMehramRel)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreMehramRel(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMehramRel)
                ?.Allowed ?? false;
        }


        #endregion

        #region Nominee Relations 
        public static bool CanTrashNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashNominee)
                ?.Allowed ?? false;
        }

        public static bool CanCreateNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateNominee)
                ?.Allowed ?? false;
        }

        public static bool CanEditNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditNominee)
                ?.Allowed ?? false;
        }

        public static bool CanViewNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewNominee)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreNominee(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreNominee)
                ?.Allowed ?? false;
        }


        #endregion

        #region Required Documents
        public static bool CanTrashReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashReqDocs)
                ?.Allowed ?? false;
        }

        public static bool CanCreateReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateReqDocs)
                ?.Allowed ?? false;
        }

        public static bool CanEditReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditReqDocs)
                ?.Allowed ?? false;
        }

        public static bool CanViewReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewReqDocs)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreReqDocs(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreReqDocs)
                ?.Allowed ?? false;
        }


        #endregion

        #region Payment Notes
        public static bool CanTrashPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPaymentNotes)
                ?.Allowed ?? false;
        }

        public static bool CanCreatePaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePaymentNotes)
                ?.Allowed ?? false;
        }

        public static bool CanEditPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPaymentNotes)
                ?.Allowed ?? false;
        }

        public static bool CanViewPaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPaymentNotes)
                ?.Allowed ?? false;
        }

        public static bool CanRestorePaymentNotes(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePaymentNotes)
                ?.Allowed ?? false;
        }


        #endregion

        #region Vaccine
        public static bool CanTrashVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashVaccine)
                ?.Allowed ?? false;
        }

        public static bool CanCreateVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateVaccine)
                ?.Allowed ?? false;
        }

        public static bool CanEditVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditVaccine)
                ?.Allowed ?? false;
        }

        public static bool CanViewVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewVaccine)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreVaccine(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreVaccine)
                ?.Allowed ?? false;
        }


        #endregion

        #region Quotatation Payment Pol
        public static bool CanTrashQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashQuotationPayPol)
                ?.Allowed ?? false;
        }

        public static bool CanCreateQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateQuotationPayPol)
                ?.Allowed ?? false;
        }

        public static bool CanEditQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditQuotationPayPol)
                ?.Allowed ?? false;
        }

        public static bool CanViewQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewQuotationPayPol)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreQuotationPayPol(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreQuotationPayPol)
                ?.Allowed ?? false;
        }


        #endregion

        #region Quotatation T & C
        public static bool CanTrashQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashQuotationTC)
                ?.Allowed ?? false;
        }

        public static bool CanCreateQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateQuotationTC)
                ?.Allowed ?? false;
        }

        public static bool CanEditQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditQuotationTC)
                ?.Allowed ?? false;
        }

        public static bool CanViewQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewQuotationTC)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreQuotationTC(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreQuotationTC)
                ?.Allowed ?? false;
        }


        #endregion

        #region Packages
        public static bool CanTrashPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPackages)
                ?.Allowed ?? false;
        }

        public static bool CanCreatePackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePackages)
                ?.Allowed ?? false;
        }

        public static bool CanEditPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPackages)
                ?.Allowed ?? false;
        }

        public static bool CanViewPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPackages)
                ?.Allowed ?? false;
        }

        public static bool CanRestorePackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePackages)
                ?.Allowed ?? false;
        }

        public static bool CanViewReportPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ReportPackages)
                ?.Allowed ?? false;
        }
        public static bool CanCopyPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CopyPackages)
                ?.Allowed ?? false;
        }
        public static bool CanViewDetailsPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.DetailsPackages)
                ?.Allowed ?? false;
        }


        #endregion

        #region Quotation Packages
        public static bool CanTrashQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashQuotationPackages)
                ?.Allowed ?? false;
        }

        public static bool CanCustomizeQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CustomizeQuotationPackages)
                ?.Allowed ?? false;
        }

        public static bool CanEditQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditQuotationPackages)
                ?.Allowed ?? false;
        }

        public static bool CanViewQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewQuotationPackages)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreQuotationPackages)
                ?.Allowed ?? false;
        }

        public static bool CanGetQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.GetQuotationPackages)
                ?.Allowed ?? false;
        }

        public static bool CanViewDetailsQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.DetailsQuotationPackages)
                ?.Allowed ?? false;
        }

        public static bool CanBookQuotationPackages(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.BookQuotationPackages)
                ?.Allowed ?? false;
        }

        #endregion


        #region Member Type
        public static bool CanTrashMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberType)
                ?.Allowed ?? false;
        }

        public static bool CanCreateMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberType)
                ?.Allowed ?? false;
        }

        public static bool CanEditMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberType)
                ?.Allowed ?? false;
        }

        public static bool CanViewMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberType)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreMemberType(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberType)
                ?.Allowed ?? false;
        }


        #endregion

        #region Member Address
        public static bool CanTrashMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberAddress)
                ?.Allowed ?? false;
        }

        public static bool CanCreateMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberAddress)
                ?.Allowed ?? false;
        }

        public static bool CanEditMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberAddress)
                ?.Allowed ?? false;
        }

        public static bool CanViewMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberAddress)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreMemberAddress(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberAddress)
                ?.Allowed ?? false;
        }


        #endregion

        #region Member Contact
        public static bool CanTrashMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberContact)
                ?.Allowed ?? false;
        }

        public static bool CanCreateMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberContact)
                ?.Allowed ?? false;
        }

        public static bool CanEditMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberContact)
                ?.Allowed ?? false;
        }

        public static bool CanViewMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberContact)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreMemberContact(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberContact)
                ?.Allowed ?? false;
        }


        #endregion

        #region Member Email
        public static bool CanTrashMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashMemberEmail)
                ?.Allowed ?? false;
        }

        public static bool CanCreateMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateMemberEmail)
                ?.Allowed ?? false;
        }

        public static bool CanEditMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditMemberEmail)
                ?.Allowed ?? false;
        }

        public static bool CanViewMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMemberEmail)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreMemberEmail(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreMemberEmail)
                ?.Allowed ?? false;
        }


        #endregion


        #region Airlines
        public static bool CanTrashAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAirlines)
                ?.Allowed ?? false;
        }

        public static bool CanCreateAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAirlines)
                ?.Allowed ?? false;
        }

        public static bool CanEditAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAirlines)
                ?.Allowed ?? false;
        }

        public static bool CanViewAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAirlines)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreAirlines(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAirlines)
                ?.Allowed ?? false;
        }


        #endregion

        #region Airline Class
        public static bool CanTrashAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAirlineClass)
                ?.Allowed ?? false;
        }

        public static bool CanCreateAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAirlineClass)
                ?.Allowed ?? false;
        }

        public static bool CanEditAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAirlineClass)
                ?.Allowed ?? false;
        }

        public static bool CanViewAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAirlineClass)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreAirlineClass(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAirlineClass)
                ?.Allowed ?? false;
        }


        #endregion

        #region Airline Fare
        public static bool CanTrashAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashAirlineFare)
                ?.Allowed ?? false;
        }

        public static bool CanCreateAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateAirlineFare)
                ?.Allowed ?? false;
        }

        public static bool CanEditAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditAirlineFare)
                ?.Allowed ?? false;
        }

        public static bool CanViewAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewAirlineFare)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreAirlineFare(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreAirlineFare)
                ?.Allowed ?? false;
        }


        #endregion

        #region POTicket

        public static bool CanViewPOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewPOTicket)
                ?.Allowed ?? false;
        }

        public static bool CanCreatePOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreatePOTicket)
                ?.Allowed ?? false;
        }

        public static bool CanEditPOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditPOTicket)
                ?.Allowed ?? false;
        }

        public static bool CanTrashPOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashPOTicket)
                ?.Allowed ?? false;
        }

        public static bool CanRestorePOTicket(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestorePOTicket)
                ?.Allowed ?? false;
        }

        #endregion

        #region Families
        public static bool CanTrashFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashFamily)
                ?.Allowed ?? false;
        }

        public static bool CanCreateFamilyHead(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFamilyHead)
                ?.Allowed ?? false;
        }

        public static bool CanEditFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditFamily)
                ?.Allowed ?? false;
        }

        public static bool CanViewFamilies(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilies)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreFamily(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreFamily)
                ?.Allowed ?? false;
        }

        public static bool CanViewFamilyMembers(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyMembers)
                ?.Allowed ?? false;
        }

        public static bool CanViewFamilyLetterHead(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyLetterHead)
                ?.Allowed ?? false;
        }

        public static bool CanViewFamilyInvoice(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewFamilyInvoice)
                ?.Allowed ?? false;
        }

        public static bool CanCreateFamilyMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateFamilyMember)
                ?.Allowed ?? false;
        }

        public static bool CanVerifyFamilyMember(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.VerifyMember)
                ?.Allowed ?? false;
        }


        #endregion


        #region Receipts
        public static bool CanTrashReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.TrashReceipts)
                ?.Allowed ?? false;
        }

        public static bool CanCreateReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.CreateReceipts)
                ?.Allowed ?? false;
        }

        public static bool CanEditReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.EditReceipts)
                ?.Allowed ?? false;
        }

        public static bool CanViewReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewReceipts)
                ?.Allowed ?? false;
        }

        public static bool CanRestoreReceipt(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.RestoreReceipts)
                ?.Allowed ?? false;
        }

        public static bool CanChangeReceiptStatus(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ChangeReceiptStatus)
                ?.Allowed ?? false;
        }

        public static bool CanViewSlip(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewSlip)
                ?.Allowed ?? false;
        }

        #endregion

        public static bool CanViewClientStatement(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewClientStatement)
                ?.Allowed ?? false;
        }

        public static bool CanViewMiniStatement(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            return dbContext.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == Convert.ToInt32(http.HttpContext.Session.GetString("RoleId")) && rp.PermissionId == (int)Privileges.ViewMiniStatement)
                ?.Allowed ?? false;
        }
    }
}

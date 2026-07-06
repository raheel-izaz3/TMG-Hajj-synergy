using TMG.Data;
using TMG.Models;
using static TMG.ManagerClasses.PrivilegeManager;

namespace TMG.ManagerClasses
{

    public enum Privileges
    {
        ViewUser = 1,
        CreateUser = 2,
        EditUser = 3,
        TrashUser = 4,
        RestoreUser = 5,

        ViewRole = 6,
        CreateRole = 7,
        EditRole = 8,
        TrashRole = 9,
        RestoreRole = 10,
        SetRolePrivelege = 11,

        ViewCompany = 12,
        CreateCompany = 13,
        EditCompany = 14,
        TrashCompany = 15,
        RestoreCompany = 16,
        IndividualReports = 17,
        CombineReports = 18,

        ViewCompanyAddress = 19,
        CreateCompanyAddress = 20,
        EditCompanyAddress = 21,
        TrashCompanyAddress = 22,
        RestoreCompanyAddress = 23,

        ViewCompanyContact = 24,
        CreateCompanyContact = 25,
        EditCompanyContact = 26,
        TrashCompanyContact = 27,
        RestoreCompanyContact = 28,

        ViewCompanyEmail = 29,
        CreateCompanyEmail = 30,
        EditCompanyEmail = 31,
        TrashCompanyEmail = 32,
        RestoreCompanyEmail = 33,

        ViewCompanyLicense = 34,
        CreateCompanyLicense = 35,
        EditCompanyLicense = 36,
        TrashCompanyLicense = 37,
        RestoreCompanyLicense = 38,

        ViewStaffMember = 39,
        CreateStaffMember = 40,
        EditStaffMember = 41,
        TrashStaffMember = 42,
        RestoreStaffMember = 43,

        ViewDesignation = 44,
        CreateDesignation = 45,
        EditDesignation = 46,
        TrashDesignation = 47,
        RestoreDesignation = 48,

        ViewVehicle = 49,
        CreateVehicle = 50,
        EditVehicle = 51,
        TrashVehicle = 52,
        RestoreVehicle = 53,

        ViewRoute = 54,
        CreateRoute = 55,
        EditRoute = 56,
        TrashRoute = 57,
        RestoreRoute = 58,

        ViewTravelRoute = 59,
        CreateTravelRoute = 60,
        EditTravelRoute = 61,
        TrashTravelRoute = 62,
        RestoreTravelRoute = 63,

        ViewLogisticReport = 64,

        ViewRoom = 65,
        CreateRoom = 66,
        EditRoom = 67,
        TrashRoom = 68,
        RestoreRoom = 69,

        ViewPlace = 70,
        CreatePlace = 71,
        EditPlace = 72,
        TrashPlace = 73,
        RestorePlace = 74,

        ViewAccomodation = 75,
        CreateAccomodation = 76,
        EditAccomodation = 77,
        TrashAccomodation = 78,
        RestoreAccomodation = 79,

        ViewAccomodationCategory = 80,
        CreateAccomodationCategory = 81,
        EditAccomodationCategory = 82,
        TrashAccomodationCategory = 83,
        RestoreAccomodationCategory = 84,

        ViewFoodPackage = 85,
        CreateFoodPackage = 86,
        EditFoodPackage = 87,
        TrashFoodPackage = 88,
        RestoreFoodPackage = 89,

        ViewGift = 90,
        CreateGift = 91,
        EditGift = 92,
        TrashGift = 93,
        RestoreGift = 94,

        ViewTraining = 95,
        CreateTraining = 96,
        EditTraining = 97,
        TrashTraining = 98,
        RestoreTraining = 99,

        ViewBank = 100,
        CreateBank = 101,
        EditBank = 102,
        TrashBank = 103,
        RestoreBank = 104,

        ViewHonorifics = 105,
        CreateHonorifics = 106,
        EditHonorifics = 107,
        TrashHonorifics = 108,
        RestoreHonorifics = 109,

        ViewFiqah = 110,
        CreateFiqah = 111,
        EditFiqah = 112,
        TrashFiqah = 113,
        RestoreFiqah = 114,

        ViewTehsil = 115,
        CreateTehsil = 116,
        EditTehsil = 117,
        TrashTehsil = 118,
        RestoreTehsil = 119,

        ViewReference = 120,
        CreateReference = 121,
        EditReference = 122,
        TrashReference = 123,
        RestoreReference = 124,

        ViewFamilyHeadRel = 125,
        CreateFamilyHeadRel = 126,
        EditFamilyHeadRel = 127,
        TrashFamilyHeadRel = 128,
        RestoreFamilyHeadRel = 129,

        ViewMehramRel = 130,
        CreateMehramRel = 131,
        EditMehramRel = 132,
        TrashMehramRel = 133,
        RestoreMehramRel = 134,

        ViewNominee = 135,
        CreateNominee = 136,
        EditNominee = 137,
        TrashNominee = 138,
        RestoreNominee = 139,

        ViewReqDocs = 140,
        CreateReqDocs = 141,
        EditReqDocs = 142,
        TrashReqDocs = 143,
        RestoreReqDocs = 144,

        ViewPaymentNotes = 145,
        CreatePaymentNotes = 146,
        EditPaymentNotes = 147,
        TrashPaymentNotes = 148,
        RestorePaymentNotes = 149,

        ViewVaccine = 150,
        CreateVaccine = 151,
        EditVaccine = 152,
        TrashVaccine = 153,
        RestoreVaccine = 154,

        ViewQuotationPayPol = 155,
        CreateQuotationPayPol = 156,
        EditQuotationPayPol = 157,
        TrashQuotationPayPol = 158,
        RestoreQuotationPayPol = 159,

        ViewQuotationTC = 160,
        CreateQuotationTC = 161,
        EditQuotationTC = 162,
        TrashQuotationTC = 163,
        RestoreQuotationTC = 164,

        ViewPackages = 165,
        CreatePackages = 166,
        EditPackages = 167,
        TrashPackages = 168,
        RestorePackages = 169,
        ReportPackages = 170,
        CopyPackages = 171,
        DetailsPackages = 172,

        ViewQuotationPackages = 173,
        CustomizeQuotationPackages = 174,
        EditQuotationPackages = 175,
        TrashQuotationPackages = 176,
        RestoreQuotationPackages = 177,
        GetQuotationPackages = 178,
        DetailsQuotationPackages = 179,
        BookQuotationPackages = 180,


        ViewMemberType = 181,
        CreateMemberType = 182,
        EditMemberType = 183,
        TrashMemberType = 184,
        RestoreMemberType = 185,

        ViewMemberAddress = 186,
        CreateMemberAddress = 187,
        EditMemberAddress = 188,
        TrashMemberAddress = 189,
        RestoreMemberAddress = 190,

        ViewMemberContact = 191,
        CreateMemberContact = 192,
        EditMemberContact = 193,
        TrashMemberContact = 194,
        RestoreMemberContact = 195,

        ViewMemberEmail = 196,
        CreateMemberEmail = 197,
        EditMemberEmail = 198,
        TrashMemberEmail = 199,
        RestoreMemberEmail = 200,

        ViewAirlines = 201,
        CreateAirlines = 202,
        EditAirlines = 203,
        TrashAirlines = 204,
        RestoreAirlines = 205,

        ViewAirlineClass = 206,
        CreateAirlineClass = 207,
        EditAirlineClass = 208,
        TrashAirlineClass = 209,
        RestoreAirlineClass = 210,

        ViewAirlineFare = 211,
        CreateAirlineFare = 212,
        EditAirlineFare = 213,
        TrashAirlineFare = 214,
        RestoreAirlineFare = 215,

        ViewFamilies = 216,
        CreateFamilyHead = 217,
        EditFamily = 218,
        TrashFamily = 219,
        RestoreFamily = 220,
        ViewFamilyMembers = 221,
        ViewFamilyLetterHead = 222,
        ViewFamilyInvoice = 223,
        CreateFamilyMember = 224,
        VerifyMember = 225,


        ViewReceipts = 226,
        CreateReceipts = 227,
        EditReceipts = 228,
        TrashReceipts = 229,
        RestoreReceipts = 230,
        ChangeReceiptStatus = 231,
        ViewSlip = 232,

        ViewClientStatement = 233,
        ViewMiniStatement = 234,

        ViewHotel = 235,
        CreateHotel = 236,
        EditHotel = 237,
        TrashHotel = 238,
        RestoreHotel = 239,

        ViewRoomAllotment = 240,
        CreateRoomAllotment = 241,
        EditRoomAllotment = 242,
        TrashRoomAllotment = 243,
        RestoreRoomAllotment = 244,

        ViewRoomSummaryReport = 245,
        ViewDailyRoomSummaryReport = 246,

        ViewPOTicket = 247,
        CreatePOTicket = 248,
        EditPOTicket = 249,
        TrashPOTicket = 250,
        RestorePOTicket = 251,
    }

}

using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.AirlineModels;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.InvoiceModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using TMG.Models.QurbaniModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using TMG.Models.AccomodationModels;
using TMG.Models.ReservationModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace TMG.Models.PackageModels
{
    public record PackageReportModel
    {
        public int Bookings { get; set; }
        public float TotalBasicPackage { get; set; } = 0f;
        public float TotalIADFAmount { get; set; } = 0f;
        public float TotalEIADFAmount { get; set; } = 0f;
        public float TotalTicketAmount { get; set; } = 0f;
        public float TotalTrainTicketAmount { get; set; } = 0f;
        public float TotalDiscount { get; set; } = 0f;
        public float GrandTotalPackage { get; set; } = 0f;
        public float TotalReceivedAmount { get; set; } = 0f;
        public float TotalBalanceBeforeTransitAmount { get; set; } = 0f;
        public float TotalPaymentInTransit { get; set; } = 0f;
        public float TotalFinalBalanceReceivable { get; set; } = 0f;
        public int PackageId { get; set; }
        public string CompanyName { get; set; }
        public string PackageName { get; set; }
        public string PackageCode { get; set; }
        public string PackageDays { get; set; }
        public string PackageDuration { get; set; }
        public string MedinahArrival { get; set; }
        public int Quota { get; set; }
        public int TotalEHajj { get; set; }
        public int totalpax { get; set; }
        public int totalmale { get; set; }
        public int totalfemale { get; set; }
        public int totalchild { get; set; }
        public int totalinfant { get; set; }
        public int totalregular { get; set; }
        public int totalsponser { get; set; }
        public int totalbiostatus { get; set; }
        public int totalagreementsign { get; set; }
        public int totaldataformsign { get; set; }

        public List<FamilyMemberModel> FamilyMembers { get; set; }
        public List<AirlineSummaryReportModel> AirlineSummary { get; set; }
        public List<TrainSummaryModel> TrainSummary { get; set; }
        public PackageSummaryModel PackageSummary { get; set; }
        public RoomCalculation RoomCalculation { get; set; } = new();
    }

    public record FamilyMemberModel
    {
        //family head details
        public string BaseCurrency { get; set; }
        public ApplicantType ApplicantType { get; set; }
        public string CompanyCode { get; set; }
        public string PackageName { get; set; }
        public string Group { get; set; }
        public string BasicPackageAmount { get; set; }
        public string RoomsAmount { get; set; }
        public string EIADFAmount { get; set; }
        public string DiscountAmount { get; set; }
        public string TicketAmount { get; set; }
        public string TrainTicketAmount { get; set; }
        public string AdditionalTransportAmount { get; set; }
        public int EHajj { get; set; }
        public int FamilyHeadId { get; set; }
        public string CompanyName { get; set; }
        public bool IsHead { get; set; }
        public string? HeadName { get; set; }
        public string? SPOName { get; set; }
        public int RowSpan { get; set; }
        public string? POD { get; set; }
        public string? pax { get; set; }
        public string? M { get; set; }
        public string? F { get; set; }
        public string? C { get; set; }
        public string? I { get; set; }
        public string? remarks { get; set; }
        public string? PackageAmount { get; set; }
        public float AverageUSDROE { get; set; }
        public string? ReceivedAmountTotal { get; set; }
        public string? ReceivedAmountUSD { get; set; }
        public string? ReceivedAmountPKR { get; set; }
        public string? BalanceBeforeTransit { get; set; }
        public string? PaymentinTransit { get; set; }
        public string? FinalBalanceReceivable { get; set; }
        public string? ReceivedAmountPercent { get; set; }
        public string? LastPaymentRecDate { get; set; }
        public string? Days { get; set; }
        public string? ReceiptNo { get; set; }
        public string? Residency { get; set; }
        public string? BioStatus { get; set; }
        public string? AgreementSign { get; set; }
        public string? DateFromSign { get; set; }
        public string? Category { get; set; }

        public string MegaId { get; set; }
        public string EHajjId { get; set; }
        public string MOFA { get; set; }
        public string FID { get; set; }
        public string CID { get; set; }
        public string GNumber { get; set; }
        public string FormNumber { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string FatherHusbandName { get; set; }
        public string Mehram { get; set; }
        public string MehramRelation { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string HajjSerialNumber { get; set; }
        public string PassportNumber { get; set; }
        public string HajjAppNumber { get; set; }
        public string VisaNumber { get; set; }
        public string TicketNumber { get; set; }
        public string FileNumber { get; set; }
        public string Makkah { get; set; }
        public string Medinah { get; set; }
        public string Azizia { get; set; }
        public string Mina { get; set; }
        public string FriendId { get; set; }
        public string TotalAmount { get; set; }
        public string Invoices { get; set; }
        public string PassportImage { get; set; }
        public string CompanyVisaName { get; set; }
    }

    public record PackageSummaryModel
    {
        public int FamiliesCount { get; set; }
        public int MembersCount { get; set; }
        public int MalesCount { get; set; }
        public int FemalesCount { get; set; }

        public int AdultsCount { get; set; }
        public int ChildsCount { get; set; }
        public int InfantsCount { get; set; }

        public int AdultMales { get; set; }
        public int AdultFemales { get; set; }

        public int ChildMales { get; set; }
        public int ChildFemales { get; set; }

        public int InfantMales { get; set; }
        public int InfantFemales { get; set; }
    }

    public record RoomCalculation
    {
        public List<AccommodationRooms> Makkah { get; set; } = new();
        public List<AccommodationRooms> Medina { get; set; } = new();
        public List<AccommodationRooms> Azizia { get; set; } = new();
        public List<AccommodationRooms> Mashair { get; set; } = new();
    }

    public record AccommodationRooms
    {
        public string RoomType { get; set; }
        public int TotalMembers { get; set; }
        public int Pax { get; set; }
        public float Total { get; set; }
    }

    public record RequiredDocumentsReportModel
    {
        public string PackageName { get; set; }
        public string PackageCode { get; set; }
        public string TableHeader { get; set; }

        public List<string> RequiredDocHeaders { get; set; } = new();
        public int FamilyMembersCount { get; set; }

        public List<int> TotalReceived { get; set; } = new();
        public List<int> TotalPending { get; set; } = new();

        public List<RequiredDocumentFamilyMemberGroupModel> Groups { get; set; } = new List<RequiredDocumentFamilyMemberGroupModel>();
    }

    public record RequiredDocumentFamilyMemberGroupModel
    {
        public string FCId { get; set; }
        public List<FamilyMemberRequiredDocuments> Documents { get; set; } = new List<FamilyMemberRequiredDocuments>();
    }

    public record FamilyMemberRequiredDocuments
    {
        public int FamilyHeadId { get; set; }
        public string FcId { get; set; }
        public string Name { get; set; }
        public List<DocumentEntry> Entries { get; set; } = new List<DocumentEntry>();
    }

    public record DocumentEntry
    {
        public RequiredDocument RequiredDocument { get; set; }
        public bool Checked { get; set; }
    }


    public record AgreementReportModel
    {
        public HajjAgreement HajjAgreement { get; set; }
    }

    public record DocumentReceivedSaveModel
    {
        public List<int> DocumentIds { get; set; } = new();
        public List<bool> DocumentSelected { get; set; } = new();
        public List<int> FamilyHeadIds { get; set; } = new();
    }

    public record PassportReportModel
    {
        public string CompanyName { get; set; }
        public string PackageName { get; set; }

        public int TotalPassports { get; set; }
        public int TotalTokens { get; set; }
        public int TotalPax { get; set; }
        public List<PassportReportMemberModel> FamilyMembers { get; set; } = new();
    }
    // public record ItineraryReportModel
    // {
    //     public string CompanyLogo { get; set; }
    //     public string PackageName { get; set; }

    //     public string PackageCode { get; set; }
    //     public string Tenure { get; set; }
    //     public string Days { get; set; }
    //     public string MadinaAccomodationName { get; set; }
    //     public string MakkahAccomodationName { get; set; }
    //     public string MinhaAccomodationName { get; set; }
    //     public string AziziaAccomodationName { get; set; }
    //     public string MadinaCheckIn { get; set; }
    //     public string MadinaCheckOut { get; set; }
    //     public string MakkahCheckIn { get; set; }
    //     public string MakkahCheckOut { get; set; }
    //     public string MinhaCheckIn { get; set; }
    //     public string MinhaCheckOut { get; set; }
    //     public string AziziaCheckIn { get; set; }
    //     public string AziziaCheckOut { get; set; }

    // }

    public record PassportReportMemberModel
    {
        public int SrNo { get; set; }
        public string MegaReference { get; set; }
        public string SPO { get; set; }
        public string FCID { get; set; }
        public string FullName { get; set; }
        public string PassportReceived { get; set; }
        public int PAX { get; set; }
        public int RowSpan { get; set; }
        public bool IsHead { get; set; }
        public string TokenReceived { get; set; }
        public string TokenRemarks { get; set; }

    }

}

namespace TMG.Controllers.PackageControllers
{
    public class PackageReportController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public PackageReportController(TMGDbContext context, IHttpContextAccessor http, IWebHostEnvironment host)
        {
            _context = context;
            _http = http;
            _host = host;
        }

        public IActionResult FamilyHeadKSANumberReport(int id)
        {

            Package package = _context.Packages.Find(id);

            PackageQurbaniReportModel model = new()
            {
                USDROE = package.USDROE,
                SARROE = package.SARROE,
                PackageId = id,
                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                ///*Quota = package.Quota*/
            };

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .OrderBy(e => e.HeadId)
                .ThenBy(e => e.FamilyHeadId)
                .AsNoTracking()
                .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);

                PackageQurbaniReportMemberModel _model = new()
                {
                    FamilyHeadId = _currMember.FamilyHeadId,
                    IsHead = _currMember.IsHead,
                    HeadId = _currMember.HeadId,
                    SrNo = i + 1,
                    FullName = _currMember.FamilyHead.FullName,
                    Passport = _currMember.FamilyHead.PassportNumber,
                    Gender = _currMember.FamilyHead.Gender.ToString(),
                    FcId = _currMember.Head.FcId,
                    QurbaniType = _currMember.FamilyHead.QurbaniType,
                    // QurbaniPkr = _currMember.FamilyHead.QurbaniPkr,
                    // QurbaniSar = _currMember.FamilyHead.QurbaniSar,
                    // QurbaniUsd = _currMember.FamilyHead.QurbaniUsd,

                };
                model.Members.Add(_model);
            }
            return View(model);
        }

        public IActionResult UploadKSANumber(int id)
        {
            return View(_context.FamilyHeads.Find(id));
        }

        [HttpPost]
        public IActionResult UploadKSANumber(FamilyHead request)
        {
            FamilyHead fh = _context.FamilyHeads.Find(request.FamilyHeadId);

            if (request.KSAMobileNumber.Length > 0)
            {
                if (request.KSAMobileNumber.ElementAt(0).Equals('0'))
                {
                    request.KSAMobileNumber = request.KSAMobileNumber.Remove(0, 1);
                    request.KSAMobileNumber = "+966 " + request.KSAMobileNumber;
                }
            }

            fh.KSAMobileNumber = request.KSAMobileNumber;
            _context.SaveChanges();

            return RedirectToAction("KSANumberSaved");
        }

        public IActionResult KSANumberSaved() => View();

        public IActionResult Report(int id)
        {
            Package package = _context.Packages.AsNoTracking().FirstOrDefault(e => e.PackageId == id);

            PackageReportModel model = new PackageReportModel
            {
                PackageId = id,
                FamilyMembers = new List<FamilyMemberModel>(),

                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                ///*Quota = package.Quota*/,
            };

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .AsNoTracking()
                .OrderBy(e => e.HeadId)
                .ToList();

            model.Bookings = members.Count;

            var familiesGroups = _context.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .AsNoTracking()
                .GroupBy(e => e.HeadId)
                .ToList();

            var maleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Male).Count();
            var femaleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Female).Count();

            model.FamilyMembers = new List<FamilyMemberModel>();

            //return Ok(familiesGroups);
            bool sameFamily = true;
            FamilyMembers prevMember = null;
            for (int i = 0, x = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                string memberName = currentMember.FamilyHead.GivenName + " " + currentMember.FamilyHead.SurName;
                string mehramName = "-";
                string mehramRel = "-";
                string fatherHusbandName = currentMember.FamilyHead.FatherHusbandName;

                var RoomsTypeAssignedToMembers = from af in _context.AdditionalFacilities
                                                 join fhad in _context.FamilyHeadAdditionalFacilities
                                                 on af.AdditionalFacilitiesId equals fhad.AdditionalFacilitiesId
                                                 join ifd in _context.IndividualFacilityDescriptions
                                                 on af.IndividiualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                                 where fhad.FamilyHeadId == currentMember.FamilyHeadId
                                                 select new
                                                 {
                                                     AdditionalFacility = af,
                                                     IndividualFacilityDescription = ifd
                                                 };

                var Makkahroomtypemember = RoomsTypeAssignedToMembers.Where(e => e.AdditionalFacility.PlaceId == 1); //makkah
                var Medinaroomtypemember = RoomsTypeAssignedToMembers.Where(e => e.AdditionalFacility.PlaceId == 2);//medina
                var Aziziaroomtypemember = RoomsTypeAssignedToMembers.Where(e => e.AdditionalFacility.PlaceId == 4); //azizia


                if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Father)
                {
                    fatherHusbandName += " <b>(F)</b>";
                }
                else if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Husband)
                {
                    fatherHusbandName += " <b>(H)</b>";
                }

                if (currentMember.IsHead)
                {
                    memberName += " <b>(FH)</b>";
                }
                else
                {
                    if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        mehramName = currentMember.Head.GivenName;
                        mehramRel = currentMember.FamilyHead?.FamilyHeadRelation?.Relation ?? "-";
                    }
                }

                if (i > 0)
                {
                    prevMember = members.ElementAt(i - 1);
                    if (prevMember.HeadId != currentMember.HeadId)
                    {
                        sameFamily = false;
                    }
                }
                if (!sameFamily)
                {
                    x++;
                    sameFamily = true;
                }

                var memberInGroup = _context.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                int familyPax = familiesGroups.ElementAt(x).Count();

                string cId =
                        $"{currentMember.FamilyHead.Package.Company.CompanyCode}.{currentMember.FamilyHead.IndId}" +
                        $".{currentMember.Head.ExtractedFamilyNumber}({familyPax})" +
                        $".{currentMember.FamilyHead.Package.Name}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }

                string megaId = "";
                if (currentMember.FamilyHead.ReferenceId == 0)
                {
                    megaId = "-";
                }
                else
                {
                    currentMember.FamilyHead.ReferenceId?.ToString();
                }

                // Main Contact & Address
                var memberContact = _context.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _context.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var makkahAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == id && e.IndividualFacilityDescription.PlaceId == Place.MAKKAH_ID);

                var medinaAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == id && e.IndividualFacilityDescription.PlaceId == Place.MEDINA_ID);

                var minaAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == id && e.IndividualFacilityDescription.PlaceId == Place.MINA_ID);

                var aziziaAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == id && e.IndividualFacilityDescription.PlaceId == Place.AZIZIA_ID);

                float totalAmount = PackageManager.GetTotalPackageAmountInSarForEntireFamily(_context, id, currentMember.FamilyHeadId);

                var invoices = _context.InvoicesWithoutTrash.Select(e => new { e.InvoiceId, e.FamilyHeadId }).Where(e => e.FamilyHeadId == currentMember.HeadId);
                string invoicesString = string.Join(", ", invoices.Select(e => "R#" + e.InvoiceId));

                model.FamilyMembers.Add(
                    new FamilyMemberModel
                    {
                        ApplicantType = currentMember.FamilyHead.ApplicantType,
                        FamilyHeadId = currentMember.FamilyHeadId,
                        MegaId = megaId,
                        EHajjId = currentMember.FamilyHead.EHajjId,
                        MOFA = currentMember.FamilyHead.MofaNumber,
                        FID = currentMember.Head.FcId,
                        CID = cId,
                        GNumber = groupNumber,
                        FormNumber = currentMember.FamilyHead.FormNumber,
                        Image = currentMember.FamilyHead.MemberImage,
                        Name = memberName,
                        FatherHusbandName = fatherHusbandName,
                        Mehram = mehramName,
                        MehramRelation = mehramRel,
                        MobileNumber = memberContact?.Contact,
                        Address = memberAddress?.Address,
                        Age = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth),
                        Gender = currentMember.FamilyHead.Gender.ToString(),
                        HajjSerialNumber = currentMember.FamilyHead.HajjAppSerialNumber,
                        VisaNumber = currentMember?.FamilyHead.VisaNumber,
                        TicketNumber = currentMember?.FamilyHead.TicketNumber,
                        PassportNumber = currentMember.FamilyHead.PassportNumber,
                        HajjAppNumber = currentMember.FamilyHead.HajjApplicationNumber,
                        FileNumber = currentMember.FamilyHead.FileNumber,
                        Makkah = makkahAcc?.IndividualFacilityDescription.Room.RoomCode,//string.Join("", Makkahroomtypemember.Select(e => e.AdditionalFacility.Room.RoomCode)),// makkahAccomodation?.Room?.RoomCode,
                        Medinah = medinaAcc?.IndividualFacilityDescription.Room.RoomCode,//string.Join("", Medinaroomtypemember.Select(e => e.AdditionalFacility.Room.RoomCode)),//medinahAccomodation?.Room?.RoomCode,
                        Azizia = aziziaAcc?.IndividualFacilityDescription.Room.RoomCode,//string.Join("", Aziziaroomtypemember.Select(e => e.AdditionalFacility.Room.RoomCode)),//aziziaAccomodation?.Room?.RoomCode,
                        Mina = minaAcc?.IndividualFacilityDescription.Room.RoomCode,//minaAccomodation?.Room?.RoomCode,
                        FriendId = currentMember.FamilyHead?.FriendId?.ToString() ?? "-",
                        TotalAmount = totalAmount.ToString("N0"),
                        Invoices = invoicesString
                    }
                    );
            }


            int familyCount = 0;
            for (int i = 0; i < familiesGroups.Count; i++)
            {
                familyCount++;
            }

            model.PackageSummary = new PackageSummaryModel
            {
                MembersCount = members.Count,
                FamiliesCount = familyCount,
                MalesCount = maleMembersCount,
                FemalesCount = femaleMembersCount
            };

            for (int i = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                int memberAge = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth);
                if (memberAge < 2)
                {
                    model.PackageSummary.InfantsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.InfantMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.InfantFemales++;
                    }
                }
                else if (memberAge < 12)
                {
                    model.PackageSummary.ChildsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.ChildMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.ChildFemales++;
                    }
                }
                else if (memberAge >= 12)
                {
                    model.PackageSummary.AdultsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.AdultMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.AdultFemales++;
                    }
                }
            }

            var facilities = _context.AdditionalFacilities
                .Include(e => e.Room)
                .Where(e => e.PackageId == id)
                .ToList();

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 3, MASHAIR_ID = 4; //AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = (from af in _context.AdditionalFacilities
                                                 join r in _context.Rooms on af.RoomId equals r.RoomId
                                                 join p in _context.Packages on af.PackageId equals p.PackageId
                                                 join fh in _context.FamilyHeads on p.PackageId equals fh.PackageId
                                                 where af.PackageId == id && !fh.Trashed
                                                 select new
                                                 {
                                                     RoomId = af.RoomId,
                                                     PlaceId = af.PlaceId,
                                                     PAX = r.PAX,
                                                     RoomCode = r.RoomCode,
                                                     RoomName = r.RoomName
                                                 })
            .Distinct() // Ensures unique results similar to GROUP BY all selected columns
            .ToList();

            var famFacilities = _context.FamilyHeadAdditionalFacilities
                            .Include(fha => fha.AdditionalFacilities)
                            .Include(fha => fha.FamilyHead)
                            .Where(fha => fha.AdditionalFacilities.PackageId == id && !fha.FamilyHead.Trashed)
                            .Select(fha => new
                            {
                                FamilyHeadAdditionalFacilities = fha,
                                AdditionalFacilities = fha.AdditionalFacilities,
                                FamilyHead = fha.FamilyHead
                            })
                            .ToList();
            //var famFacilities = _context.FamilyHeadAdditionalFacilities
            //        .Include(e => e.AdditionalFacilities)
            //        .Where(e => e.AdditionalFacilities.PackageId == id);

            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = _context.PackageCategoryFacilities
                                        .Where(e => e.IndividualFacilityDescription.PlaceId == MAKKAH_ID &&
                                                e.IndividualFacilityDescription.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Makkah.Add(_room);
                }
                else if (_currFac.PlaceId == MEDINA_ID)
                {
                    int totalMembers = _context.PackageCategoryFacilities
                                        .Where(e => e.IndividualFacilityDescription.PlaceId == MEDINA_ID &&
                                                e.IndividualFacilityDescription.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Medina.Add(_room);
                }
                else if (_currFac.PlaceId == MASHAIR_ID)
                {
                    int totalMembers = _context.PackageCategoryFacilities
                                         .Where(e => e.IndividualFacilityDescription.PlaceId == MASHAIR_ID &&
                                                 e.IndividualFacilityDescription.RoomId == _currFac.RoomId)?
                                                 .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Azizia.Add(_room);
                }
                else if (_currFac.PlaceId == AZIZIA_ID)
                {
                    int totalMembers = _context.PackageCategoryFacilities
                                        .Where(e => e.IndividualFacilityDescription.PlaceId == AZIZIA_ID &&
                                                e.IndividualFacilityDescription.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Mashair.Add(_room);
                }


            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Report(PackageReportSaveModel request)
        {
            //return Ok(request);

            var families = _context.FamilyHeadsWithoutTrash
                .Where(e => e.PackageId == request.PackageId)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var currentMember = families.FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadIds[i]);
                currentMember.ApplicantType = request.Categories.ElementAt(i);
                currentMember.MofaNumber = request.MOFAs.ElementAt(i) ?? "";
                currentMember.HajjApplicationNumber = request.HajjAppNumbers.ElementAt(i) ?? "";
                currentMember.HajjAppSerialNumber = request.HajjSerials.ElementAt(i) ?? "";
                currentMember.TicketNumber = request.TicketNumbers.ElementAt(i) ?? "";
                currentMember.VisaNumber = request.VisaNumbers.ElementAt(i) ?? "";
                currentMember.EHajjId = request.EHajjIds.ElementAt(i) ?? "";
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Package");
        }

        public IActionResult RequiredDocuments(int id)
        {
            Package package = _context.Packages.Find(id);
            var documents = _context.RequiredDocumentsWithoutTrash.ToList();

            RequiredDocumentsReportModel model = new RequiredDocumentsReportModel();
            model.RequiredDocHeaders = documents.Select(e => e.Documents).ToList();

            var totalReceived = _context.FamilyHeadDocuments
                .Join(_context.FamilyHeads,
                    fhd => fhd.FamilyHeadId,
                    fh => fh.FamilyHeadId,
                    (fhd, fh) => new { fhd, fh })
                .Where(joined => joined.fh.PackageId == id && !joined.fh.Trashed)
                .GroupBy(joined => joined.fhd.RequiredDocumentId)
                .Select(group => new
                {
                    RequiredDocumentId = group.Key,
                    TotalReceived = group.Count(item => item.fhd.Recieved)
                })
                .ToList();

            var totalPending = _context.FamilyHeadDocuments
                .Join(_context.FamilyHeads,
                    fhd => fhd.FamilyHeadId,
                    fh => fh.FamilyHeadId,
                    (fhd, fh) => new { fhd, fh })
                .Where(joined => joined.fh.PackageId == id && !joined.fh.Trashed)
                .GroupBy(joined => joined.fhd.RequiredDocumentId)
                .Select(group => new
                {
                    RequiredDocumentId = group.Key,
                    TotalPending = group.Count(item => !item.fhd.Recieved)
                })
                .ToList();

            model.TotalPending = totalPending.Select(e => e.TotalPending).ToList();
            model.TotalReceived = totalReceived.Select(e => e.TotalReceived).ToList();

            var members = _context.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .ToList();

            model.FamilyMembersCount = members.Count;

            var memberHeads = _context.FamilyMembers
                .Where(e => e.IsHead && e.FamilyHead.PackageId == id
                && !e.FamilyHead.Trashed)
                .ToList();

            var memberDocuments = _context.FamilyHeadDocuments
                .Include(e => e.RequiredDocument)
                .Where(e => e.FamilyHead.PackageId == id
                && !e.FamilyHead.Trashed)
                .ToList();

            model.PackageName = package.Name;
            model.PackageCode = package.Code;

            string tableHeader = "";
            for (int i = 0; i < documents.Count; i++)
            {
                tableHeader += "<th></th>";
            }

            model.TableHeader = tableHeader;

            for (int i = 0; i < memberHeads.Count; i++)
            {
                var currMemberHead = memberHeads.ElementAt(i);

                var group = new RequiredDocumentFamilyMemberGroupModel();
                group.FCId = currMemberHead.FamilyHead.FcId;

                var currMemberHeadFamily = members
                    .Where(e => e.HeadId == currMemberHead.FamilyHeadId)
                    .ToList();

                for (int j = 0; j < currMemberHeadFamily.Count; j++)
                {
                    FamilyMemberRequiredDocuments fmDoc = new FamilyMemberRequiredDocuments();
                    var _curr = currMemberHeadFamily.ElementAt(j);
                    fmDoc.Name = _curr.FamilyHead.GivenName;
                    fmDoc.FamilyHeadId = _curr.FamilyHead.FamilyHeadId;
                    fmDoc.FcId = _curr.FamilyHead.IndId;

                    var currMemberDocuments = memberDocuments
                            .Where(e => e.FamilyHeadId == _curr.FamilyHeadId)
                            .ToList();

                    for (int k = 0; k < documents.Count; k++)
                    {
                        var currDocument = documents.ElementAt(k);
                        bool docChecked = currMemberDocuments
                            .FirstOrDefault(e => e.RequiredDocumentId == currDocument.RequiredDocumentId)?
                            .Recieved ?? false;

                        fmDoc.Entries.Add(new DocumentEntry
                        {
                            RequiredDocument = currDocument,
                            Checked = docChecked
                        });

                    }

                    group.Documents.Add(fmDoc);

                }

                model.Groups.Add(group);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult RequiredDocuments(DocumentReceivedSaveModel request)
        {
            // return Ok(new {
            //     docCount = request.DocumentIds.Count,
            //     famCount = request.FamilyHeadIds.Count,
            //     selCount = request.DocumentSelected.Count,
            // });
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var reqDocs = _context.FamilyHeadDocuments.ToList();
                for (int i = 0; i < request.DocumentIds.Count; i++)
                {
                    FamilyHeadDocument document = reqDocs.FirstOrDefault(e =>
                                                            e.RequiredDocumentId == request.DocumentIds.ElementAt(i)
                                                            && e.FamilyHeadId == request.FamilyHeadIds.ElementAt(i));

                    if (document is not null)
                    {
                        document.Recieved = request.DocumentSelected.ElementAt(i);
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
                return RedirectToAction("Index", "Package");
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return Ok(new
                {
                    e.Message
                });

                return View();
            }
        }

        public IActionResult Agreement(int id)
        {
            // HajjAgreement agreement = _context.HajjAgreements.FirstOrDefault(e => e.PackageId == id);
            var packageAgreement = _context.HajjAgreementPackages.Include(e => e.HajjAgreement).FirstOrDefault(e => e.PackageId == id);
            AgreementReportModel model = new AgreementReportModel
            {
                HajjAgreement = packageAgreement.HajjAgreement
            };

            return View(model);
        }

        public IActionResult DraftAgreement(int id)
        {
            // HajjAgreement hajjAgreement = _context.HajjAgreements.FirstOrDefault(e => e.PackageId == id);
            var packageAgreement = _context.HajjAgreementPackages.Include(e => e.HajjAgreement).FirstOrDefault(e => e.PackageId == id);
            HajjAgreement hajjAgreement = packageAgreement.HajjAgreement;

            return View(hajjAgreement);
        }

        public IActionResult IndividualAdditionalFacilityReport(int id)
        {
            Package package = _context.Packages.Find(id);

            var familyHeads = _context.FamilyMembers
                            .Where(e => e.IsHead && e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                            .Include(e => e.FamilyHead)
                            .ToList();

            var members = _context.FamilyMembers
                            .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                            .Include(e => e.FamilyHead)
                            .ToList();

            var iadfDescriptions = _context.AdditionalFacilities
                                    .Include(e => e.IndividiualFacilityDescription)
                                    .Where(e => e.PackageId == id)
                                    .ToList();

            var memberFacilities = _context.FamilyHeadAdditionalFacilities
                                    .Include(e => e.FamilyHead)
                                    .Include(e => e.AdditionalFacilities)
                                    .Where(e => e.FamilyHead.PackageId == id)
                                    .ToList();

            PackageFacilitiesReportModel model = new()
            {
                PackageName = package.Name,
                PackageCode = package.Code,
                FamilyMembersCount = members.Count,
                RoomHeaders = iadfDescriptions.Select(e => e.IndividiualFacilityDescription.Description).ToList()
            };

            for (int i = 0; i < iadfDescriptions.Count; i++)
            {
                var currDesc = iadfDescriptions.ElementAt(i);
                var descFacilities = _context.FamilyHeadAdditionalFacilities
                        .Where(e => e.AdditionalFacilities.IndividiualFacilityDescriptionId == currDesc.IndividiualFacilityDescriptionId &&
                                e.AdditionalFacilities.PackageId == id && !e.FamilyHead.Trashed)
                        .ToList();

                model.RoomCounts.Add(descFacilities.Count);

            }

            for (int i = 0; i < familyHeads.Count; i++)
            {
                var currHead = familyHeads.ElementAt(i);
                var currFamilyMembers = members.Where(e => e.HeadId == currHead.FamilyHeadId)
                                            .ToList();

                var headModel = new PackageFacilitiesReportModelHeads
                {
                    FCId = currHead.FamilyHead.FcId
                };

                for (int j = 0; j < currFamilyMembers.Count; j++)
                {
                    var currMember = currFamilyMembers.ElementAt(j);
                    var currMemberFac = memberFacilities
                                    .Where(e => e.FamilyHeadId == currMember.FamilyHeadId)
                                    .ToList();

                    var headModelMember = new PackageFacilitiesReportModelMembers
                    {
                        FamilyHeadId = currMember.FamilyHeadId,
                        FCId = currMember.FamilyHead.FcId,
                        Name = currMember.FamilyHead.FullName,
                    };


                    for (int k = 0; k < iadfDescriptions.Count; k++)
                    {
                        var currRoom = iadfDescriptions.ElementAt(k);
                        bool roomChecked = currMemberFac
                            .FirstOrDefault(e => e.AdditionalFacilities.IndividiualFacilityDescriptionId == currRoom.IndividiualFacilityDescriptionId) is not null;

                        headModelMember.Rooms.Add(new PackageFacilitiesReportModelRoomList
                        {
                            IADF = currRoom,
                            Checked = roomChecked
                        });

                    }

                    headModel.Members.Add(headModelMember);

                }

                model.Heads.Add(headModel);

            }

            return View(model);
        }

        [HttpPost]
        public IActionResult IndividualAdditionalFacilityReport(SaveIADFReportModel request)
        {
            // return Ok(new {
            //     request
            // });

            var transaction = _context.Database.BeginTransaction();

            try
            {
                for (int i = 0; i < request.FacilityIds.Count; i++)
                {

                    if (request.FacilitySelected[i] == true)
                    {
                        var facExists = _context.FamilyHeadAdditionalFacilities
                                    .FirstOrDefault(e => e.AdditionalFacilitiesId == request.FacilityIds[i] && e.FamilyHeadId == request.FamilyHeadIds[i]) is not null;

                        if (facExists) continue;

                        _context.FamilyHeadAdditionalFacilities.Add(new FamilyHeadAdditionalFacilities
                        {
                            AdditionalFacilitiesId = request.FacilityIds[i],
                            FamilyHeadId = request.FamilyHeadIds[i]
                        });
                    }
                    else
                    {
                        _context.FamilyHeadAdditionalFacilities.Where(e =>
                                                    e.AdditionalFacilitiesId == request.FacilityIds[i] && e.FamilyHeadId == request.FamilyHeadIds[i])
                                                    .ExecuteDelete();
                    }
                }
                _context.SaveChanges();

                transaction.Commit();

                return RedirectToAction("Index", "Package");
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return Ok(e.Message);
            }


        }

        public IActionResult PackageSummaryReport(int id)
        {
            Package package = _context.Packages.Find(id);

            PackageReportModel model = new PackageReportModel
            {
                FamilyMembers = new List<FamilyMemberModel>(),
                AirlineSummary = new List<AirlineSummaryReportModel>(),
                TrainSummary = new List<TrainSummaryModel>(),

                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                ///*Quota = package.Quota*/,
            };

            string companyName = _context.CompaniesWithoutTrash.FirstOrDefault(e => e.CompanyId == package.CompanyId).CompanyName;
            model.CompanyName = companyName;

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .OrderBy(e => e.HeadId)
                .ToList();

            var memberHeads = members.Where(e => e.IsHead).ToList();

            var memberIds = members.Select(e => e.FamilyHeadId).ToList();
            var memberHeadIds = memberHeads.Select(e => e.FamilyHeadId).ToList();

            var eiadfs = _context.FamilyHeadExtras
                    .Where(e => memberIds.Contains(e.FamilyHeadId))
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.PriceSar
                    })
                    .ToList();

            model.totalpax = members.Count;

            var familiesGroups = _context.FamilyMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .GroupBy(e => e.HeadId)
                .ToList();

            var maleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Male).Count();
            var femaleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Female).Count();

            var invoices = _context.InvoicesWithoutTrash.ToList();

            model.FamilyMembers = new List<FamilyMemberModel>();

            bool sameFamily = true;
            FamilyMembers prevMember = null;
            for (int i = 0, x = 0; i < memberHeads.Count; i++)
            {
                var currentMember = memberHeads.ElementAt(i);
                string memberName = currentMember.FamilyHead.GivenName;
                string mehramName = "-";
                string mehramRel = "-";
                string fatherHusbandName = currentMember.FamilyHead.FatherHusbandName;

                int headID = _context.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId).HeadId;

                int _FamilyPAX = _context.FamilyMembers.Where(a => a.HeadId == headID).Count();

                if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Father)
                {
                    fatherHusbandName += " <b>(F)</b>";
                }
                else if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Husband)
                {
                    fatherHusbandName += " <b>(H)</b>";
                }

                if (currentMember.IsHead)
                {
                    memberName += " <b>(FH)</b>";
                }
                else
                {
                    if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        mehramName = currentMember.Head.GivenName;
                        mehramRel = currentMember.FamilyHead?.FamilyHeadRelation?.Relation ?? "-";
                    }
                }

                if (i > 0)
                {
                    prevMember = members.ElementAt(i - 1);
                    if (prevMember.HeadId != currentMember.HeadId)
                    {
                        sameFamily = false;
                    }
                }
                if (!sameFamily)
                {
                    x++;
                    sameFamily = true;
                }
                var memberInGroup = _context.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                var agreement = _context.UserHajjAgreements
                    .Include(e => e.FamilyHead)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                int familyPax = familiesGroups.ElementAt(x).Count();

                string cId =
                        $"{currentMember.FamilyHead.Package.Company.CompanyCode}.{currentMember.FamilyHead.IndId}" +
                        $".{currentMember.HeadId}({familyPax})" +
                        $".{currentMember.FamilyHead.Package.Name}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }

                string megaId = "";
                if (currentMember.FamilyHead.ReferenceId == 0)
                {
                    megaId = "-";
                }
                else
                {
                    currentMember.FamilyHead.ReferenceId?.ToString();
                }

                // Main Contact & Address
                var memberContact = _context.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _context.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);


                var memberage = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth);

                var remarks = _context.FamilyHeads
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.StayWithId != null);


                var currMemberInvoices = invoices
                                    .Where(e => e.FamilyHeadId == currentMember.HeadId);

                var invoicesCleared = currMemberInvoices
                                 .Where(e => e.InvoiceStatus == InvoiceStatus.Cleared);

                float invoicesClearedPkr = currMemberInvoices
                                    .Where(e => e.InvoiceStatus == InvoiceStatus.Cleared && e.Currency == Currencies.PKR)
                                    .Sum(e => e.AmountRecived);

                float invoicesClearedUsd = currMemberInvoices
                                    .Where(e => e.InvoiceStatus == InvoiceStatus.Cleared && e.Currency == Currencies.USD)
                                    .Sum(e => e.AmountRecived);

                float invoicesClearedSar = currMemberInvoices
                                    .Where(e => e.InvoiceStatus == InvoiceStatus.Cleared
                                    && e.Currency == Currencies.SAR)
                                    .Sum(e => e.AmountRecived);


                var invoicesInTransit = currMemberInvoices
                                    .Where(e => e.InvoiceStatus == InvoiceStatus.ClearingInTransit);

                var lastInvoice = currMemberInvoices
                        .OrderByDescending(e => e.InvoiceId)
                        .FirstOrDefault();

                float totalPaymentReceivedSar = 0;

                foreach (var inv in invoicesCleared)
                {
                    float converted = 0;
                    if (inv.Currency == Currencies.PKR)
                    {
                        converted = inv.AmountRecived / inv.SARROE;
                    }
                    else if (inv.Currency == Currencies.SAR)
                    {
                        converted = inv.AmountRecived;

                    }
                    //else if (inv.Currency == Currencies.AED)
                    //{
                    //    converted = inv.AmountRecived / inv.USDROE;
                    //}
                    else if (inv.Currency == Currencies.USD)
                    {
                        converted = inv.AmountRecived * inv.USDROE;
                    }
                    totalPaymentReceivedSar += converted;
                }


                float paymentInTrasitSar = 0;

                foreach (var inv in invoicesInTransit)
                {
                    float converted = 0;
                    if (inv.Currency == Currencies.PKR)
                    {
                        converted = inv.AmountRecived / inv.SARROE;
                    }
                    else if (inv.Currency == Currencies.SAR)
                    {
                        converted = inv.AmountRecived;
                    }
                    //else if (inv.Currency == Currencies.AED)
                    //{
                    //    converted = inv.AmountRecived / inv.USDROE;
                    //}
                    else if (inv.Currency == Currencies.USD)
                    {
                        converted = inv.AmountRecived * inv.SARROE;
                    }
                    paymentInTrasitSar += converted;
                }

                int daysDifference = 0;
                if (lastInvoice is not null)
                {
                    TimeSpan difference = DateTime.Today - lastInvoice.ReceiptDate;
                    daysDifference = (int)difference.TotalDays;
                }

                float basicPackageAmount = 0f, iadfAmount = 0f, eiadfAmount = 0f, ticketAmount = 0f, discountAmount = 0f;

                var headMembers = members.Where(e => e.HeadId == currentMember.HeadId).ToList();
                var headMemberIds = headMembers.Select(e => e.FamilyHeadId).ToList();
                var headMembersEIadfs = eiadfs.Where(e => headMemberIds.Contains(e.FamilyHeadId)).ToList();


                foreach (var m in headMembers)
                {
                    if (m.FamilyHead.Age < 2)
                    {
                        basicPackageAmount += currentMember.FamilyHead.Package.InfantSar;
                    }
                    else if (m.FamilyHead.Age < 12)
                    {
                        basicPackageAmount += currentMember.FamilyHead.Package.ChildSar;
                    }
                    else
                    {
                        basicPackageAmount += currentMember.FamilyHead.Package.AdultSar;
                    }

                    if (m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                    {

                        ticketAmount += currentMember.FamilyHead.Package.FlightTicketPriceSar;
                        if (m.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                        {
                            ticketAmount += m.FamilyHead.FlightTicketDifferencesSar ?? 0f;
                        }
                    }

                    discountAmount += m.FamilyHead.SarDiscountAmount;
                }

                eiadfAmount = headMembersEIadfs.Sum(e => e.PriceSar) ?? 0f;

                model.TotalBasicPackage += basicPackageAmount;
                model.TotalEIADFAmount += eiadfAmount;
                model.TotalDiscount += discountAmount;
                model.TotalTicketAmount += ticketAmount;

                float totalPackageAmount = basicPackageAmount + iadfAmount + eiadfAmount + ticketAmount - discountAmount;

                model.GrandTotalPackage += totalPackageAmount;

                float balanceBeforeTrasit = totalPackageAmount - totalPaymentReceivedSar;
                model.TotalBalanceBeforeTransitAmount += balanceBeforeTrasit;

                float balanceAfterTrasit = totalPackageAmount - (totalPaymentReceivedSar + paymentInTrasitSar);
                model.TotalFinalBalanceReceivable += balanceAfterTrasit;

                model.TotalReceivedAmount += totalPaymentReceivedSar;

                double percOfPaymentReceived = Math.Round((totalPaymentReceivedSar / totalPackageAmount) * 100, 2);

                model.FamilyMembers.Add(
                    new FamilyMemberModel
                    {
                        BasicPackageAmount = basicPackageAmount.ToString("N0"),
                        EIADFAmount = eiadfAmount.ToString("N0"),
                        TicketAmount = ticketAmount.ToString("N0"),
                        DiscountAmount = discountAmount.ToString("N0"),
                        EHajj = currentMember.FamilyHead.Ehajj is not null ? 1 : 0,
                        RowSpan = _FamilyPAX,
                        IsHead = currentMember.IsHead,
                        HeadName = currentMember.FamilyHead.FullName,
                        POD = currentMember.FamilyHead.POD,
                        pax = familyPax.ToString(),
                        M = headMembers.Count(e => e.FamilyHead.IsAdult && e.FamilyHead.Gender == Gender.Male).ToString(),
                        F = headMembers.Count(e => e.FamilyHead.IsAdult && e.FamilyHead.Gender == Gender.Female).ToString(),
                        C = headMembers.Count(e => e.FamilyHead.IsChild).ToString(),
                        I = headMembers.Count(e => e.FamilyHead.IsInfant).ToString(),
                        remarks = remarks == null ? "" : remarks.FullName,
                        PackageAmount = totalPackageAmount.ToString("N0"),
                        ReceivedAmountUSD = totalPaymentReceivedSar.ToString("N0"),
                        ReceivedAmountPKR = invoicesClearedPkr.ToString("N0"),
                        BalanceBeforeTransit = balanceBeforeTrasit.ToString("N0"),
                        PaymentinTransit = paymentInTrasitSar.ToString("N0"),
                        FinalBalanceReceivable = balanceAfterTrasit.ToString("N0"),
                        ReceivedAmountPercent = percOfPaymentReceived.ToString() + "%",
                        Days = daysDifference.ToString(),
                        LastPaymentRecDate = DateTimeManager.GetDate(lastInvoice?.ReceiptDate),
                        ReceiptNo = lastInvoice is not null ? $"{currentMember.Head.FcId} R#{lastInvoice?.InvoiceId}" : "",
                        Residency = currentMember.FamilyHead.Residency,
                        BioStatus = currentMember.FamilyHead.ENumberVerified == true ? "1" : "0",
                        AgreementSign = agreement == null ? "0" : "1",
                        DateFromSign = agreement == null ? "0" : (agreement.SignDataForm ? "1" : "0"),
                        Category = currentMember.FamilyHead.ApplicantType.ToString(),

                        MegaId = megaId,
                        EHajjId = currentMember.FamilyHead.EHajjId,
                        MOFA = currentMember.FamilyHead.MofaNumber,
                        FID = $"{currentMember.FamilyHead.Package.Company.CompanyCode}-{currentMember.FamilyHead.IndId} ({currentMember.Head.FcId})",
                        CID = cId,
                        GNumber = groupNumber,
                        FormNumber = currentMember.FamilyHead.FormNumber,
                        Image = currentMember.FamilyHead.MemberImage,
                        Name = memberName,
                        FatherHusbandName = fatherHusbandName,
                        Mehram = mehramName,
                        MehramRelation = mehramRel,
                        MobileNumber = memberContact?.Contact,
                        Address = memberAddress?.Address,
                        Age = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth),
                        Gender = currentMember.FamilyHead.Gender.ToString(),
                        HajjSerialNumber = currentMember.FamilyHead.EHajjSerial,
                        PassportNumber = currentMember.FamilyHead.PassportNumber,
                        HajjAppNumber = currentMember.FamilyHead.HajjApplicationNumber,
                        FileNumber = currentMember.FamilyHead.FileNumber,
                        FriendId = currentMember.FamilyHead?.FriendId?.ToString() ?? "-",
                        TotalAmount = "",
                        Invoices = ""
                    }
                    );
            }


            int familyCount = 0;
            for (int i = 0; i < familiesGroups.Count; i++)
            {
                familyCount++;
            }

            model.totalmale = members.Count(e => e.FamilyHead.IsAdult && e.FamilyHead.Gender == Gender.Male);
            model.totalfemale = members.Count(e => e.FamilyHead.IsAdult && e.FamilyHead.Gender == Gender.Female);
            model.totalchild += members.Count(e => e.FamilyHead.IsChild);
            model.totalinfant += members.Count(e => e.FamilyHead.IsInfant);

            foreach (var item in model.FamilyMembers)
            {
                model.totalregular += Convert.ToInt32(string.IsNullOrEmpty(item.Category) ? "0" : item.Category.ToLower() == "Regular".ToLower() ? "1" : "0");
                model.totalsponser += Convert.ToInt32(string.IsNullOrEmpty(item.Category) ? "0" : item.Category.ToLower() == "Sponser".ToLower() ? "1" : "0");
            }


            model.PackageSummary = new PackageSummaryModel
            {
                MembersCount = members.Count,
                FamiliesCount = familyCount,
                MalesCount = maleMembersCount,
                FemalesCount = femaleMembersCount
            };

            for (int i = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                int memberAge = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth);
                if (memberAge < 2)
                {
                    model.PackageSummary.InfantsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.InfantMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.InfantFemales++;
                    }
                }
                else if (memberAge < 12)
                {
                    model.PackageSummary.ChildsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.ChildMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.ChildFemales++;
                    }
                }
                else if (memberAge >= 12)
                {
                    model.PackageSummary.AdultsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.AdultMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.AdultFemales++;
                    }
                }
            }

            var facilities = _context.AdditionalFacilities
                .Include(e => e.Room)
                .Where(e => e.PackageId == id)
                .ToList();

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = (from af in _context.AdditionalFacilities
                                                 join r in _context.Rooms on af.RoomId equals r.RoomId
                                                 where af.PackageId == id
                                                 group new { af, r } by new { af.RoomId, af.PlaceId, r.PAX, r.RoomCode, r.RoomName } into grouped
                                                 select new
                                                 {
                                                     grouped.Key.RoomId,
                                                     grouped.Key.PlaceId,
                                                     grouped.Key.PAX,
                                                     grouped.Key.RoomCode,
                                                     grouped.Key.RoomName
                                                 }).ToList();

            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var famFacilities = _context.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .Where(e => e.AdditionalFacilities.PackageId == id);

            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == MAKKAH_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Makkah.Add(_room);
                }
                else if (_currFac.PlaceId == MEDINA_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == MEDINA_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Medina.Add(_room);
                }
                else if (_currFac.PlaceId == MASHAIR_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == MASHAIR_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Azizia.Add(_room);
                }
                else if (_currFac.PlaceId == AZIZIA_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.AdditionalFacilities.PlaceId == AZIZIA_ID &&
                                                e.AdditionalFacilities.RoomId == _currFac.RoomId)?
                                                .Count() ?? 0;

                    AccommodationRooms _room = new();
                    _room.RoomType = $"{_currFac.RoomName} ({_currFac.RoomCode})";
                    _room.Pax = _currFac.PAX;
                    _room.TotalMembers = totalMembers;

                    _room.Total = totalMembers > 0 ? ((float)totalMembers / (float)_currFac.PAX) : 0f;

                    model.RoomCalculation.Mashair.Add(_room);
                }


            }


            var flight = _context.Flights
               .Where(a => !a.Trashed)
               .Select(a => a.OutboundArrivalDateTime.Date)
               .Distinct()
               .OrderBy(d => d)
               .ToList();

            var time = flight.Select(a =>
                _context.Flights
                    .Where(a => !a.Trashed)
                    .Include(a => a.Airline)
                    .OrderBy(a => a.OutboundArrivalDateTime)
                    .ToList()
            ).ToList();

            var flights = _context.Flights
                .Where(a => !a.Trashed)
                .Include(a => a.Airline)
                .ToList();

            var place = _context.Places
                    .Where(a => !a.Trashed)
                    .ToList();

            var companies = _context.FlightPackages
                .Include(a => a.Flight)
                .Where(a => !a.Flight.Trashed && a.Package.Company.CompanyId == id)
                .Include(fp => fp.Package)
                .ThenInclude(p => p.Company)
                .Select(fp => fp.Package.Company)
                .Distinct()
                .ToList();

            var flightList = new AirlineSummaryReportModel();

            var AirLineArrival = (from f in _context.Flights
                                  join fp in _context.FlightPackages on f.FlightId equals fp.FlightId
                                  join p in _context.Packages on fp.PackageId equals p.PackageId
                                  join pnr in _context.PNRs on f.FlightId equals pnr.FlightId
                                  join pnrm in _context.PNRMembers on pnr.PNRId equals pnrm.PNRId
                                  join s in _context.Sectors on f.FlightId equals s.FlightId
                                  join fh in _context.FamilyHeads on pnrm.FamilyHeadId equals fh.FamilyHeadId
                                  where p.PackageId == id && s.FlightType == (FlightType)1
                                  group new { fh, pnr, s } by new { pnr.Name, pnr.PNRId, s.ArrivalDateTime, s.Destination, s.SectorId } into grp
                                  select new
                                  {
                                      Pax = grp.Count(),
                                      PNRName = grp.Key.Name,
                                      PnrNo = grp.Key.PNRId,
                                      Arrival = grp.Key.ArrivalDateTime.Value.Date,
                                      Sector = grp.Key.Destination + "(" + grp.Key.SectorId.ToString() + ")",
                                      Time = grp.Key.ArrivalDateTime.Value.TimeOfDay
                                  }).ToList();


            var AirLineDeparture = (from f in _context.Flights
                                    join fp in _context.FlightPackages on f.FlightId equals fp.FlightId
                                    join p in _context.Packages on fp.PackageId equals p.PackageId
                                    join pnr in _context.PNRs on f.FlightId equals pnr.FlightId
                                    join pnrm in _context.PNRMembers on pnr.PNRId equals pnrm.PNRId
                                    join s in _context.Sectors on f.FlightId equals s.FlightId
                                    join fh in _context.FamilyHeads on pnrm.FamilyHeadId equals fh.FamilyHeadId
                                    where p.PackageId == id && s.FlightType == (FlightType)0
                                    group new { fh, pnr, s } by new { pnr.Name, pnr.PNRId, s.ArrivalDateTime, s.Destination, s.SectorId } into grp
                                    select new
                                    {
                                        Pax = grp.Count(),
                                        PNRName = grp.Key.Name,
                                        PnrNo = grp.Key.PNRId,
                                        Arrival = grp.Key.ArrivalDateTime.Value.Date,
                                        Sector = grp.Key.Destination + "(" + grp.Key.SectorId.ToString() + ")",
                                        Time = grp.Key.ArrivalDateTime.Value.TimeOfDay
                                    }).ToList();
            foreach (var fl in AirLineArrival)
            {
                var AirlineSummaryReportModel = new AirlineSummaryReportModel
                {
                    Pax = fl.Pax.ToString(),
                    PNRName = fl.PNRName,
                    PNRNo = fl.PnrNo.ToString(),
                    Arrival = fl.Arrival.ToString(),
                    Sector = fl.Sector,
                    Time = fl.Time.ToString()
                };
                model.AirlineSummary.Add(AirlineSummaryReportModel);
            }
            for (int j = 0; j < AirLineDeparture.Count; j++)
            {
                if (model.AirlineSummary.Count > 0)
                {

                    for (int i = 0; i < model.AirlineSummary.Count; i++)
                    {
                        if (i == j)
                        {
                            model.AirlineSummary[i].Departure = AirLineDeparture[j].Arrival.ToString();
                            model.AirlineSummary[i].DepSector = AirLineDeparture[j].Sector.ToString();
                            model.AirlineSummary[i].DepTime = AirLineDeparture[j].Time.ToString();

                        }

                    }
                }
            }
            //model.AirlineSummary.Add(flightList);

            return View(model);

        }

        public IActionResult EIADFReport(int id)
        {
            var package = _context.Packages.Select(e => new
            {
                e.Name,
                e.Code,
                e.PackageId
            }).FirstOrDefault(e => e.PackageId == id);

            var totalExtras = from fhe in _context.FamilyHeadExtras
                              join e in _context.ExtraFacilityDescriptions on fhe.ExtraFacilityDescriptionId equals e.ExtraFacilityDescriptionId
                              join fh in _context.FamilyHeads on fhe.FamilyHeadId equals fh.FamilyHeadId
                              where fh.PackageId == id
                              group e by e.Description into g
                              select new
                              {
                                  Description = g.Key,
                                  Total = g.Count()
                              };

            var extras = _context.FamilyHeadExtras
                .Include(e => e.ExtraFacilityDescription)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.SPO)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Reference)
                .Where(e => e.FamilyHead.PackageId == id)
                .OrderBy(e => e.FamilyHeadId)
                .GroupBy(e => e.FamilyHeadId)
                .ToList();

            // return Ok(extras);

            EIADFReportModel model = new()
            {
                PackageName = package.Name,
                PackageCode = package.Code,
                Descriptions = totalExtras.ToDictionary(x => x.Description, x => x.Total)
            };

            for (int i = 0; i < extras.Count; i++)
            {
                var currExtra = extras.ElementAt(i);
                var currExtraMember = extras.ElementAt(i).FirstOrDefault();
                EIADFMembersModel _m = new()
                {
                    SR = i + 1,
                    FullName = currExtraMember?.FamilyHead.FullName,
                    FCID = $"{currExtraMember?.FamilyHead.FcId}",
                    SPO = currExtraMember?.FamilyHead.SPO?.Name,
                    MegaReference = currExtraMember?.FamilyHead.Reference?.ReferenceName
                };

                string extrasString = "";
                for (int j = 0; j < currExtra.Count(); j++)
                {
                    string desc = currExtra.ElementAt(j).ExtraFacilityDescription?.Description;
                    extrasString += extrasString.Equals("") ? $"{desc}" : $", {desc}";
                }
                _m.EIADF = extrasString;

                model.Members.Add(_m);


            }

            return View(model);

        }

        public IActionResult PassportReport(int id)
        {

            FamilyHead a = _context.FamilyHeads.
                Include(a => a.Package)
                .ThenInclude(a => a.Company)
                .FirstOrDefault();

            PassportReportModel model = new PassportReportModel
            {
                //FamilyMembers = new List<FamilyMemberModel>(),

                PackageName = a.Package?.Name,
                CompanyName = a.Package?.Company?.CompanyName,

            };

            var members = _context.FamilyMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(a => a.Package)
                .ThenInclude(a => a.Company)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.SPO)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Reference)
                .Where(a => a.FamilyHead.PackageId == id && !a.FamilyHead.Trashed)
                .OrderBy(e => e.HeadId)
                .ToList();

            model.TotalPax = members.Count();

            int i = 1;
            foreach (var member in members)
            {

                var FamilyMem = new PassportReportMemberModel
                {
                    IsHead = member.IsHead,
                    PAX = members.Where(e => e.HeadId == member.FamilyHeadId).Count(),
                    SrNo = i,
                    MegaReference = member.FamilyHead.Reference?.ReferenceName,
                    SPO = member.FamilyHead.SPO?.Name,
                    FCID = $"{member.FamilyHead.FcId}",
                    FullName = member.FamilyHead.FullName,
                    PassportReceived = string.IsNullOrEmpty(member.FamilyHead.PassportImage) ? "Yes" : "-",
                    TokenReceived = string.IsNullOrEmpty(member.FamilyHead.PassportTokenImage) ? "-" : "Yes",
                    TokenRemarks = member.FamilyHead.TokenRemarks,
                };
                model.FamilyMembers.Add(FamilyMem);
                i++;
            }

            model.TotalPassports = members.Where(a => a.FamilyHead.PassportImage != null).Count();
            model.TotalTokens = members.Where(a => a.FamilyHead.PassportTokenImage != null).Count();


            //model.totalpax = members.Count;

            return View(model);
        }

        public IActionResult IdCardReport(int id)
        {
            IdCardReportModel model = new();

            var package = _context.Packages
                            .Include(e => e.Company)
                            .FirstOrDefault(e => e.PackageId == id);


            var munazzim = _context.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);

            var minaAccommodation = _context.PackageAccomodtions
                                    .FirstOrDefault(e => e.PlaceId == 3 && e.PackageId == id);


            model.Package = package.Name;
            model.Company = package.Company.CompanyName;
            model.Munazzim = munazzim.Munazzim.Name;
            model.Maktab = minaAccommodation?.AccomodationName;


            var members = _context.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.FamilyHead.PackageId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                IdCardReportMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName;
                _model.HajiSurName = _currMember.FamilyHead.SurName;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;

                int pax = members.Where(e => e.HeadId == _currMember.HeadId).Count();

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _context.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);
            }

            return View(model);
        }

        public IActionResult IdCardReportV2(int id)
        {
            IdCardReportModel model = new();


            var package = (from p in _context.Packages
                           join c in _context.Companies on p.CompanyId equals c.CompanyId
                           join cl in _context.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed
                           select new
                           {
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.Package.PackageId == id);

            var munazzim = _context.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);

            var minaAccommodation = _context.PackageAccomodtions
                                    .FirstOrDefault(e => e.PlaceId == 3 && e.PackageId == id);

            var aziziaAcco = _context.PackageAccomodtions
                                    .AsNoTracking()
                                    .Include(e => e.Hotel)
                                    .Include(e => e.FoodPackage)
                                    .Where(e => e.PackageId == id && e.PlaceId == 4)
                                    .OrderBy(e => e.ActualCheckInTime)
                                    .FirstOrDefault();

            var makkahAcc = _context.PackageAccomodtions
                                   .AsNoTracking()
                                   .Include(e => e.Hotel)
                                   .Include(e => e.FoodPackage)
                                   .Where(e => e.PackageId == id && e.PlaceId == 1)
                                   .OrderBy(e => e.ActualCheckInTime)
                                   .FirstOrDefault();

            var medinahAcc = _context.PackageAccomodtions
                                   .AsNoTracking()
                                   .Include(e => e.Hotel)
                                   .Include(e => e.FoodPackage)
                                   .Where(e => e.PackageId == id && e.PlaceId == 2)
                                   .OrderBy(e => e.ActualCheckInTime)
                                   .FirstOrDefault();


            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;
            model.Maktab = minaAccommodation != null ? minaAccommodation.AccomodationName : "";
            model.MakkahAccomodation = makkahAcc != null ? makkahAcc.Hotel?.HotelName : "";
            model.MedinahAccomodation = medinahAcc != null ? medinahAcc.Hotel?.HotelName : "";
            model.AziziaAccomodation = aziziaAcco != null ? aziziaAcco.Hotel?.HotelName : "";


            var members = _context.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.FamilyHead.PackageId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                IdCardReportMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;

                _model.MakkahAccomodation = _context.PackageAccomodtions
        .Select(e => new { e.PlaceId, e.PackageId, e.Hotel.HotelName })
        .FirstOrDefault(e => e.PlaceId == 1 && e.PackageId == id)?.HotelName;

                _model.MakkahLocation = _context.PackageAccomodtions
                .Select(e => new { e.PlaceId, e.PackageId, e.Hotel.Location })
                .FirstOrDefault(e => e.PlaceId == 1 && e.PackageId == id)?.Location;


                _model.MedinahAccomodation = _context.PackageAccomodtions
                .Select(e => new { e.PlaceId, e.PackageId, e.Hotel.HotelName })
                .FirstOrDefault(e => e.PlaceId == 2 && e.PackageId == id)?.HotelName;

                _model.MedinahLocation = _context.PackageAccomodtions
                .Select(e => new { e.PlaceId, e.PackageId, e.Hotel.Location })
                .FirstOrDefault(e => e.PlaceId == 2 && e.PackageId == id)?.Location;


                _model.AziziaAccomodation = _context.PackageAccomodtions.Select(e => new
                {
                    e.PlaceId,
                    e.PackageId,
                    e.Hotel.HotelName
                }).FirstOrDefault(a => a.PlaceId == 4 && a.PackageId == id)?.HotelName;


                _model.AziziaLocation = _context.PackageAccomodtions.Select(e => new
                {
                    e.PlaceId,
                    e.PackageId,
                    e.Hotel.Location
                }).FirstOrDefault(a => a.PlaceId == 4 && a.PackageId == id)?.Location;

                int pax = members.Where(e => e.HeadId == _currMember.HeadId).Count();

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _context.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);
            }

            return View(model);
        }

        public IActionResult QurbaniReport(int id)
        {

            Package package = _context.Packages.Find(id);

            PackageQurbaniReportModel model = new()
            {
                USDROE = package.USDROE,
                SARROE = package.SARROE,
                PackageId = id,
                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                //Quota = package.Quota
            };

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                .OrderBy(e => e.HeadId)
                .ThenBy(e => e.FamilyHeadId)
                .AsNoTracking()
                .ToList();

            var qurbaniPaidExtras = _context.FamilyHeadExtras
                    .Where(e => e.ExtraFacilityDescriptionId == 1)
                    .ToList();

            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                bool _qurbaniPaid = qurbaniPaidExtras
                    .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && e.ExtraFacilityDescriptionId == 1)?
                    .PriceUsd > 0;

                bool _qurbaniComplimentry = qurbaniPaidExtras
                    .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && e.ExtraFacilityDescriptionId == 1)?
                    .PriceUsd == 0;

                PackageQurbaniReportMemberModel _model = new()
                {
                    FamilyHeadId = _currMember.FamilyHeadId,
                    IsHead = _currMember.IsHead,
                    HeadId = _currMember.HeadId,
                    SrNo = i + 1,
                    FullName = _currMember.FamilyHead.FullName,
                    Passport = _currMember.FamilyHead.PassportNumber,
                    Gender = _currMember.FamilyHead.Gender.ToString(),
                    FcId = _currMember.Head.FcId,
                    QurbaniType = _currMember.FamilyHead.QurbaniType,
                    PaidQurbani = _qurbaniPaid,
                    ComplimentryQurbani = _qurbaniComplimentry,

                };
                model.Members.Add(_model);
            }


            return View(model);
        }

        [HttpPost]
        public IActionResult QurbaniReport(PackageQurbaniSaveModel request)
        {

            // return Ok(new {
            //     request
            // });
            var members = _context.FamilyHeadsWithoutTrash
                            .Where(e => e.PackageId == request.PackageId && e.FcId != null)
                            .ToList();

            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);

                if (request.QurbaniPaid.ElementAt(i))
                {
                    _currMember.QurbaniType = QurbaniTypes.Paid;
                }
                else if (request.QurbaniComplimentry.ElementAt(i))
                {
                    _currMember.QurbaniType = QurbaniTypes.Complimentry;
                }
                else
                {
                    _currMember.QurbaniType = QurbaniTypes.Excluded;
                }

                var nextQurbaniToken = _context.FamilyHeads
                            .Where(e => e.QurbaniToken != null)
                            .OrderByDescending(e => e.QurbaniToken)
                            .Select(e => e.QurbaniToken)
                            .FirstOrDefault();

                if (_currMember.QurbaniToken != null) continue;

                if (nextQurbaniToken != null)
                    nextQurbaniToken++;
                else
                    nextQurbaniToken = 1;


                _currMember.QurbaniToken = nextQurbaniToken;

                _context.SaveChanges();
            }


            Package package = _context.Packages.Find(request.PackageId);

            PackageQurbaniReportModel model = new()
            {
                USDROE = package.USDROE,
                SARROE = package.SARROE,
                PackageId = request.PackageId,
                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                //Quota = package.Quota
            };

            var __members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.FamilyHead.PackageId == request.PackageId && !e.FamilyHead.Trashed)
                .OrderBy(e => e.HeadId)
                .ThenBy(e => e.FamilyHeadId)
                .AsNoTracking()
                .ToList();

            var qurbaniPaidExtras = _context.FamilyHeadExtras
                    .Where(e => e.ExtraFacilityDescriptionId == 1)
                    .ToList();

            for (int i = 0; i < __members.Count; i++)
            {
                var _currMember = __members.ElementAt(i);
                bool _qurbaniPaid = qurbaniPaidExtras
                    .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && e.ExtraFacilityDescriptionId == 1)?
                    .PriceUsd > 0;

                bool _qurbaniComplimentry = qurbaniPaidExtras
                    .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && e.ExtraFacilityDescriptionId == 1)?
                    .PriceUsd == 0;

                PackageQurbaniReportMemberModel _model = new()
                {
                    FamilyHeadId = _currMember.FamilyHeadId,
                    IsHead = _currMember.IsHead,
                    HeadId = _currMember.HeadId,
                    SrNo = i + 1,
                    FullName = _currMember.FamilyHead.FullName,
                    Passport = _currMember.FamilyHead.PassportNumber,
                    Gender = _currMember.FamilyHead.Gender.ToString(),
                    FcId = _currMember.Head.FcId,
                    QurbaniType = _currMember.FamilyHead.QurbaniType,
                    PaidQurbani = _qurbaniPaid,
                    ComplimentryQurbani = _qurbaniComplimentry,

                };
                model.Members.Add(_model);
            }


            return View(model);
        }

        public IActionResult QurbaniLinkReport(int id)
        {

            Package package = _context.Packages.Find(id);

            PackageQurbaniReportModel model = new()
            {
                USDROE = package.USDROE,
                SARROE = package.SARROE,
                PackageId = id,
                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                //Quota = package.Quota
            };

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed && e.FamilyHead.QurbaniType != 0 && e.IsHead)
                .OrderBy(e => e.HeadId)
                .ThenBy(e => e.FamilyHeadId)
                .AsNoTracking()
                .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);

                PackageQurbaniReportMemberModel _model = new()
                {
                    FamilyHeadId = _currMember.FamilyHeadId,
                    IsHead = _currMember.IsHead,
                    HeadId = _currMember.HeadId,
                    SrNo = i + 1,
                    FullName = _currMember.FamilyHead.FullName,
                    Passport = _currMember.FamilyHead.PassportNumber,
                    Gender = _currMember.FamilyHead.Gender.ToString(),
                    FcId = _currMember.Head.FcId,
                    QurbaniType = _currMember.FamilyHead.QurbaniType,
                    // QurbaniPkr = _currMember.FamilyHead.QurbaniPkr,
                    // QurbaniSar = _currMember.FamilyHead.QurbaniSar,
                    // QurbaniUsd = _currMember.FamilyHead.QurbaniUsd,

                };
                model.Members.Add(_model);
            }
            return View(model);
        }

        static string GenerateToken(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public IActionResult QurbaniQueueForm(int id)
        {
            //Slip data
            BatchSlipsModel model = new BatchSlipsModel();

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                .OrderBy(e => e.HeadId)
                .ToList();

            var familiesGroups = _context.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => !e.FamilyHead.Trashed)
                .GroupBy(e => e.HeadId)
                .ToList();

            var contacts = _context.FamilyHeadContacts;
            var groupMembers = _context.GroupMembers
                .Include(e => e.Group);

            var tcs = _context.QurbaniSlipTCsWithoutTrash.ToList();
            var notes = _context.QurbaniSlipNotes.ToList();

            string tcString = "";
            for (int i = 0; i < tcs.Count; i++)
            {
                tcString += $"<p style='margin-bottom:0;'>{i + 1}. {tcs.ElementAt(i).SlipTC}</p>";
            }

            string noteString = "";
            for (int i = 0; i < notes.Count; i++)
            {
                noteString += $"<p style='margin-bottom:0;'>{i + 1}. {notes.ElementAt(i).Note}</p>";
            }

            model.TCsString = tcString;
            model.NotesString = noteString;

            int memberssaved = 0;
            foreach (var item in members)
            {
                //generate token
                string token = GenerateToken(8);

                BatchSlipEntryModel entry = new BatchSlipEntryModel();
                FamilyHeadContact fc = contacts
                        .FirstOrDefault(e => e.Preferred && e.FamilyHeadId == item.FamilyHeadId);
                GroupMember memberInGroup = groupMembers.FirstOrDefault(e => e.FamilyHeadId == item.FamilyHeadId);
                int fPax = members
                        .Where(e => e.HeadId == item.HeadId)
                        .Count();
                entry.TokenNumber = token;
                entry.FHName = item.FamilyHead.FatherHusbandName;
                entry.Name = item.FamilyHead.GivenName;
                entry.FCID = item.FamilyHead.FcId;
                entry.MobileNumber = fc?.Contact;
                entry.KSAMobileNumber = item.FamilyHead.KSAMobileNumber;
                entry.CompanyName = item.FamilyHead.Package.Company.CompanyName;
                entry.CID = $"{item.FamilyHead.Package.Company.CompanyCode}.{item.FamilyHead.IndId}" +
                    $".{item.FamilyHead.ExtractedFamilyNumber}.({fPax}).{item.FamilyHead.Package.Name}";
                entry.FamilyHeadId = item.FamilyHeadId;
                entry.USDPrice = _context.FamilyHeadExtras.FirstOrDefault(e => e.FamilyHeadId == id)?.PriceUsd?.ToString("N0") ?? "0";
                if (memberInGroup != null)
                {
                    entry.CID += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    entry.GroupNumber = memberInGroup.GroupId.ToString();
                }
                var Qurbanique = _context.QurbaniQueue.Where(e => e.FamilyHeadId == item.FamilyHeadId);
                entry.saved = Qurbanique.Count() > 0;
                memberssaved = entry.saved ? memberssaved + 1 : memberssaved;
                entry.BatchNumber = _context.QurbaniQueue
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.QurbaniBatch.Name
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == item.FamilyHeadId)?
                    .Name ?? "";
                entry.WhatsappGroupLink = _context.QurbaniQueue
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.QurbaniBatch.WhatsappGroupLink
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == item.FamilyHeadId)?
                    .WhatsappGroupLink;

                entry.BatchCompleted = _context.QurbaniQueue
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.QurbaniBatch.Completed
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == item.FamilyHeadId)?
                    .Completed ?? false;

                entry.QurbaniEnabled = _context.ApplicationSettings
                    .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                    .First()
                    .EnableQurbani;

                model.Entries.Add(entry);
            }

            //----------------------------------------------------------//
            var count = model.Entries.Count();
            var members1 = _context.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.HeadId == id)
                            .ToList();
            model.displayconfirm = count == memberssaved;
            ViewBag.FamilyheadId = id;
            return View(model);
        }

        public IActionResult QurbaniQueueFormSave(List<int> Ids)
        {
            foreach (var item in Ids)
            {
                string insertQuery = "INSERT INTO QurbaniQueue (FamilyHeadId, StartTime) VALUES (@FamilyHeadId, @StartTime)";
                object[] parameters = {
                new SqlParameter("@FamilyHeadId", item),
                new SqlParameter("@StartTime", DateTime.Now)
                };

                _context.Database.ExecuteSqlRaw(insertQuery, parameters);
            }
            string message = "";
            //var members = _context.QurbaniQueue
            //                .Where(e => e.FamilyHeadId == id)
            //                .ToList();
            //if(members.Count == 0)
            //{
            //    string insertQuery = "INSERT INTO QurbaniQueue (FamilyHeadId, StartTime) VALUES (@FamilyHeadId, @StartTime)";
            //    object[] parameters = {
            //    new SqlParameter("@FamilyHeadId", id),
            //    new SqlParameter("@StartTime", DateTime.Now)
            //    };

            //    _context.Database.ExecuteSqlRaw(insertQuery, parameters);
            //    message = "You are successfully added into the waiting queue, out team will get back to you in next 6 hours Thanks";
            //}
            //else
            //{
            //    message = "You are already into the waiting queue Thanks";
            //}
            return Ok();// Json(new { message = message });
        }

        public IActionResult VisaInsuranceReport(int id)
        {
            VisaInsuranceReportModel model = new();

            var families = _context
                            .FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ToList();

            foreach (var m in families)
            {
                VisaInsuranceReportRowModel _ = new VisaInsuranceReportRowModel
                {
                    IsHead = m.IsHead,
                    FamilyHeadId = m.FamilyHeadId,
                    FcId = m.FamilyHead.FcId,
                    FullName = m.FamilyHead.FullName,
                    VisaImage = m.FamilyHead.VisaImage,
                    InsuranceImage = m.FamilyHead.InsuranceImage,
                    MISImage = m.FamilyHead.MISPdfPath,
                    AirlineImage = m.FamilyHead.AirlineTicketImage,
                    TrainImage = m.FamilyHead.TrainTicketImage
                };

                model.Rows.Add(_);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult UploadDocuments(UploadDocumentSaveModel request)
        {
            //return Ok(request);

            FamilyHead head = _context.FamilyHeads.Find(request.FamilyHeadId);
            string visaPath = "", insurancePath = "", misFile = "",
                airlinePath = "", trainPath = "";

            var transaction = _context.Database.BeginTransaction();
            try
            {

                if (request.VisaFile is not null)
                {
                    visaPath = Path.Combine(_host.WebRootPath, "Images", "VisaImages", request.VisaFile.FileName);
                    using (FileStream fs = new(visaPath, FileMode.Create))
                    {
                        request.VisaFile.CopyTo(fs);
                    }
                    head.VisaImage = @"Images/VisaImages/" + request.VisaFile.FileName;
                }
                if (request.InsuranceFile is not null)
                {
                    insurancePath = Path.Combine(_host.WebRootPath, "Images", "InsuranceImages", request.InsuranceFile.FileName);
                    using (FileStream fs = new(insurancePath, FileMode.Create))
                    {
                        request.InsuranceFile.CopyTo(fs);
                    }
                    head.InsuranceImage = @"Images/InsuranceImages/" + request.InsuranceFile.FileName;
                }
                if (request.MISFile is not null)
                {
                    //string file = Path.Combine(_host.WebRootPath, familyHead.MISPdfPath);


                    misFile = Path.Combine(_host.WebRootPath, "Pdfs", "MIS", request.MISFile.FileName);
                    using (FileStream fs = new(misFile, FileMode.Create))
                    {
                        request.MISFile.CopyTo(fs);
                    }
                    head.MISPdfPath = @"Pdfs/MIS/" + request.MISFile.FileName;

                    string outputFile = Path.Combine(_host.WebRootPath, "Pdfs", "SignedMIS", head.PassportNumber + "SignedMISForm.pdf");

                    head.SignedMISPdfPath = @"Pdfs\SignedMIS\" + head.PassportNumber + "SignedMISForm.pdf";
                    //_context.SaveChanges();

                    string base64Path = head.SignatureBase64;
                    float x = 125, y = 525;



                    // get the company of the user
                    var company = from c in _context.Companies
                                  join p in _context.Packages on c.CompanyId equals p.CompanyId
                                  join fh in _context.FamilyHeads on p.PackageId equals fh.PackageId
                                  where fh.FamilyHeadId == request.FamilyHeadId
                                  select new { c, p, fh };

                    // If you need the result in a specific format, you can project it into a custom object or an anonymous type.
                    var companyresult = company.ToList();
                    int comapnyId = 0;
                    foreach (var item in companyresult)
                    {
                        comapnyId = item.c.CompanyId;
                    }
                    //Stamps 
                    var MisStamps = _context.MisStampsWithoutTrash.FirstOrDefault(e => e.CompanyId == comapnyId);
                    string MisFilePath = @"" + MisStamps.FilePath;
                    string MisSignatureFilePath = @"" + MisStamps.SignatureFilePath;

                    AddImageToPdf(misFile, outputFile, base64Path, x, y, MisFilePath, MisSignatureFilePath);
                }
                if (request.AirlineFile is not null)
                {
                    airlinePath = Path.Combine(_host.WebRootPath, "Images", "AirlineTickets", request.AirlineFile.FileName);
                    using (FileStream fs = new(airlinePath, FileMode.Create))
                    {
                        request.AirlineFile.CopyTo(fs);
                    }
                    head.AirlineTicketImage = @"Images/AirlineTickets/" + request.AirlineFile.FileName;
                }
                if (request.TrainFile is not null)
                {
                    trainPath = Path.Combine(_host.WebRootPath, "Images", "TrainTickets", request.TrainFile.FileName);
                    using (FileStream fs = new(trainPath, FileMode.Create))
                    {
                        request.TrainFile.CopyTo(fs);
                    }
                    head.TrainTicketImage = @"Images/TrainTickets/" + request.TrainFile.FileName;
                }

                _context.SaveChanges();

                transaction.Commit();
                return Ok(new
                {
                    status = 200,
                    msg = "Upload Done!"
                });
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return BadRequest(new
                {
                    status = 500,
                    msg = "Error!"
                });
            }
        }

        public void AddImageToPdf(string pdfFilePath, string outputFilePath, string base64Image, float x, float y, string MisFilePath, string MisSignatureFilePath)
        {
            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            using (PdfWriter pdfWriter = new PdfWriter(outputFilePath))
            using (PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter))
            {
                Document document = new Document(pdfDoc);

                PdfPage firstPage = pdfDoc.GetPage(1);
                PdfPage secondPage = pdfDoc.GetPage(2);
                PdfCanvas canvas = new PdfCanvas(secondPage);
                PdfCanvas canvas1 = new PdfCanvas(firstPage);

                base64Image = base64Image.Replace("data:image/png;base64,", "");

                byte[] imageBytes = Convert.FromBase64String(base64Image);
                ImageData imageData = ImageDataFactory.Create(imageBytes);
                Image image = new Image(imageData);
                float scaleFactor = 0.55f;
                float scaleFactor1 = 0.60f;
                float scaleFactor2 = 0.80f;
                canvas.SaveState()
                      .ConcatMatrix(scaleFactor, 0, 0, scaleFactor, x, y)
                      .AddImageAt(imageData, 0, 0, false);

                //MisStamp

                string file = Path.Combine(_host.WebRootPath, MisFilePath);
                string file1 = Path.Combine(_host.WebRootPath, MisSignatureFilePath);
                byte[] MisStampImageBytes = System.IO.File.ReadAllBytes(file);
                ImageData MisStampimageData = ImageDataFactory.Create(MisStampImageBytes);
                Image MisStampimage = new Image(MisStampimageData);

                //float scaleFactor1 = 0.25f;
                canvas.SaveState()
                      .ConcatMatrix(scaleFactor1, 0, 0, scaleFactor1, 350, 40)
                      .AddImageAt(MisStampimageData, 0, 0, false);

                canvas1.SaveState()
                      .ConcatMatrix(scaleFactor1, 0, 0, scaleFactor1, 25, 350)
                      .AddImageAt(MisStampimageData, 0, 0, false);

                //MisStampSignature
                byte[] MisStampSigimageBytes = System.IO.File.ReadAllBytes(file1);
                ImageData MisStampSigimageData = ImageDataFactory.Create(MisStampSigimageBytes);
                Image MisStampSigimage = new Image(MisStampSigimageData);
                canvas.SaveState()
                     .ConcatMatrix(scaleFactor2, 0, 0, scaleFactor2, 450, 40)
                      .AddImageAt(MisStampSigimageData, 0, 0, false);

                canvas1.SaveState()
                     .ConcatMatrix(scaleFactor2, 0, 0, scaleFactor2, 750, 40)
                      .AddImageAt(MisStampSigimageData, 0, 0, false);
                //MisStampSigimage.SetFixedPosition(325, y);
                //document.Add(MisStampSigimage);


            }
        }

        public IActionResult QRCodeGift(int id)
        {
            List<QRModel> model = new List<QRModel>();
            var query = from fh in _context.FamilyHeads
                        join p in _context.Packages on fh.PackageId equals p.PackageId
                        where !fh.Trashed && p.PackageId == id
                        orderby p.Name
                        select new
                        {
                            PackageName = p.Name,
                            FcId = fh.FcId,
                            HajjiName = fh.GivenName + " " + fh.SurName,
                            FamilyHeadId = fh.FamilyHeadId
                        };
            var result = query.ToList();

            foreach (var item in result)
            {

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(item.FamilyHeadId.ToString(), QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                model.Add(new QRModel
                {
                    PackageName = item.PackageName,
                    FcId = item.FcId,
                    HajjiName = item.HajjiName,
                    qrCode = "https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=" + item.FamilyHeadId.ToString()
                });
            }

            return View(model);
        }

        public IActionResult GetQRForGift(int id)
        {
            var members = (from fh in _context.FamilyHeads
                           join p in _context.Packages on fh.PackageId equals p.PackageId
                           where !fh.Trashed && p.PackageId == id
                           orderby p.Name
                           select new
                           {
                               PackageName = p.Name,
                               FcId = fh.FcId,
                               HajjName = fh.GivenName + " " + fh.SurName,
                               FamilyHeadId = fh.FamilyHeadId
                           }).ToList();

            ViewBag.Members = members;
            return View();
        }

        public IActionResult GiftReport(int id)
        {

            var query = (from fh in _context.FamilyHeads
                         join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                         join p in _context.Packages on fh.PackageId equals p.PackageId
                         join c in _context.Companies on p.CompanyId equals c.CompanyId
                         join gm in _context.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                         from gm in gmGroup.DefaultIfEmpty()
                         join g in _context.Groups.Where(g => !g.Trashed) on gm.GroupId equals g.GroupId into gGroup
                         from g in gGroup.DefaultIfEmpty()
                         where p.PackageId == id && !fh.Trashed && !g.Trashed
                         group new { fh, fm, p, c, g } by new
                         {
                             FamilyHeadId = fh.FamilyHeadId,
                             fmFamilyHeadID = fm.FamilyHeadId,
                             headid = fm.HeadId,
                             FullName = fh.GivenName + " " + fh.SurName,
                             Gender = fh.Gender == Gender.Female ? "Female" : "Male",
                             CompanyCode = c.CompanyCode,
                             IndId = fh.IndId,
                             ffcId = fh.FcId,
                             FcId = fh.FcId,
                             GroupName = g != null ? g.Name : "",
                             PackageName = p.Name,
                             GroupId = g != null ? g.GroupId.ToString() : "",
                         } into grouped
                         orderby grouped.Key.headid ascending
                         select new
                         {
                             FullName = grouped.Key.FullName,
                             Gender = grouped.Key.Gender,
                             TotalFemale = grouped.Sum(x => x.fh.Gender == Gender.Female ? 1 : 0),
                             TotalMale = grouped.Sum(x => x.fh.Gender == Gender.Male ? 1 : 0),



                             CID = $"{grouped.Key.CompanyCode}.{grouped.Key.IndId}.{(grouped.Key.FcId != null ? grouped.Key.FcId.Remove(0, 3) : "")}" +
                       $".({grouped.Select(x => x.fm.HeadId).Distinct().Count()}).{grouped.Key.PackageName}" +
                       $".({_context.FamilyMembers.Include(e => e.FamilyHead).Where(e => e.HeadId == grouped.Key.headid && !e.FamilyHead.Trashed).Count()})" +
                       $".{grouped.Key.PackageName}" + $"{(grouped.Key.GroupName != "" ? "." + grouped.Key.GroupName : "")}",



                             ffcId = grouped.Key.FcId,
                             PackageName = grouped.Key.PackageName,
                             Total = grouped.Count()
                         }).ToList();



            ViewBag.Results = query;

            return View();
        }

        public IActionResult SuitCaseReport(int id)
        {
            var query = (from fh in _context.FamilyHeads
                         join fm in _context.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                         join p in _context.Packages on fh.PackageId equals p.PackageId
                         join c in _context.Companies on p.CompanyId equals c.CompanyId
                         join gm in _context.GroupMembers on fh.FamilyHeadId equals gm.FamilyHeadId into gmGroup
                         from gm in gmGroup.DefaultIfEmpty()
                         join g in _context.Groups.Where(g => !g.Trashed) on gm.GroupId equals g.GroupId into gGroup
                         from g in gGroup.DefaultIfEmpty()
                         join sp in _context.SuitcasePackages on p.PackageId equals sp.PackageId
                         join s in _context.Suitcases on sp.SuitcaseId equals s.SuitcaseId
                         where !fh.Trashed && !p.Trashed && p.PackageId == id && !s.Trashed && !g.Trashed
                         group new { fh, p, c, fm, s, g } by new
                         {
                             headid = fm.HeadId,
                             FamilyHeadID = fh.FamilyHeadId,
                             p.Name,
                             c.CompanyName,
                             s.SuitcaseCode,
                             fh.FcId,
                             fh.GivenName,
                             fh.SurName,
                             fh.SuitCase,
                             fh.Gender,
                             c.CompanyCode,
                             fh.IndId,
                             GroupName = g != null ? g.Name : "",
                             GroupId = g != null ? g.GroupId.ToString() : "",
                         } into g
                         orderby g.Key.headid ascending
                         select new
                         {
                             PackageName = g.Key.Name,
                             CompanyName = g.Key.CompanyName,
                             Total = g.Count(),
                             Code = g.Key.SuitcaseCode,
                             FcId = g.Key.FcId,

                             CID = $"{g.Key.CompanyCode}.{g.Key.IndId}.{(g.Key.FcId != null ? g.Key.FcId.Remove(0, 3) : "")}" +
                                   $".({_context.FamilyMembers.Include(e => e.FamilyHead).Where(e => e.HeadId == g.Key.headid && !e.FamilyHead.Trashed).Count()}).{g.Key.Name}" +
                                   $"{(g.Key.GroupName != "" ? "." + g.Key.GroupId : "")}",


                             GivenName = g.Key.GivenName + " " + g.Key.SurName,
                             Gender = g.Key.Gender == Gender.Female ? "Female" : "Male",
                             TotalFemale = g.Sum(x => x.fh.Gender == Gender.Female ? 1 : 0),
                             TotalMale = g.Sum(x => x.fh.Gender == Gender.Male ? 1 : 0)
                         })
                         .Distinct()
                         .ToList();



            ViewBag.Results = query;
            return View();

        }

        public IActionResult MakkahRoomingList(int id)
        {

            const int MAKKAH_ID = 1;

            var query = (from fm in _context.FamilyMembers
                         join fh in _context.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                         join p in _context.Packages on fh.PackageId equals p.PackageId
                         join c in _context.Companies on p.CompanyId equals c.CompanyId
                         join fhadf in _context.FamilyHeadAdditionalFacilities on fh.FamilyHeadId equals fhadf.FamilyHeadId
                         join adf in _context.AdditionalFacilities on fhadf.AdditionalFacilitiesId equals adf.AdditionalFacilitiesId
                         join place in _context.Places on adf.PlaceId equals place.PlaceId
                         join room in _context.Rooms on adf.RoomId equals room.RoomId
                         where place.PlaceId == MAKKAH_ID && !fh.Trashed && p.PackageId == id
                         orderby fm.HeadId ascending, fm.FamilyHeadId ascending, room.RoomId ascending
                         select new
                         {
                             fm.HeadId,
                             fm.FamilyHeadId,
                             fm.IsHead,
                             fh.FullName,
                             fh.Gender,
                             fh.VisaNumber,
                             fh.PassportNumber,
                             fh.HajjApplicationNumber,
                             room.RoomName,
                             room.PAX,
                             fh.FcId
                         }).ToList();

            PackageMakkahRoomListing model = new();

            int rowCount = 0;
            bool sameFamily = true, sameRoomType = true;
            int? prevMemberHeadId = null;
            string prevMemberRoom = "";

            for (int i = 0; i < query.Count; i++)
            {
                var _member = query.ElementAt(i);


                int _memberPax = _context.FamilyMembers
                                    .Include(e => e.FamilyHead)
                                    .Where(e => e.HeadId == _member.HeadId && e.FamilyHead.Trashed)
                                    .Count();
                var _roomPax = _context.FamilyHeadAdditionalFacilities
                        .Include(e => e.AdditionalFacilities)
                        .Where(e => e.FamilyHeadId == _member.HeadId && e.AdditionalFacilities.PlaceId == 1)
                        .Select(e => new
                        {
                            e.AdditionalFacilities.PlaceId,
                            e.AdditionalFacilities.IndividiualFacilityDescription.Description,
                            e.AdditionalFacilities.Room.PAX
                        }).ToList();


                if (i > 0)
                {
                    prevMemberHeadId = query.ElementAt(i - 1).HeadId;
                    prevMemberRoom = query.ElementAt(i - 1).RoomName;
                }
                // else
                // {
                //     prevMemberHeadId = query.ElementAt(0).HeadId;
                //     prevMemberRoom = query.ElementAt(0).RoomName;
                // }

                sameFamily = prevMemberHeadId == _member.HeadId;
                sameRoomType = prevMemberRoom.Equals(_member.RoomName);

                PackageMakkahRoomListingRow rowModel = new()
                {
                    SrNo = i + 1,
                    FamilyHeadId = _member.FamilyHeadId,
                    HeadId = _member.HeadId,
                    FcId = _member.FcId,
                    FullName = _member.FullName,
                    Gender = _member.Gender.ToString(),
                    HajjApplicationNumber = _member.HajjApplicationNumber,
                    VisaNumber = _member.VisaNumber,
                    PassportNumber = _member.PassportNumber,
                    RoomType = _member.RoomName,
                    RoomPAX = _member.PAX,
                    SameFamily = sameFamily,
                    SameRoomType = sameRoomType
                };


                if (!sameRoomType || !sameFamily || rowCount == _member.PAX)
                {
                    rowCount = 1;
                }
                else
                {
                    rowCount++;
                }



                rowModel.RowSpan = rowCount;

                model.Rows.Add(rowModel);
            }


            // return Ok(model);
            return View(model);
        }

        public IActionResult TrainTicket(int id)
        {
            TrainTicketReportModel model = new();

            var families = _context
                            .FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ToList();

            foreach (var m in families)
            {
                TrainTicketReportRowModel _ = new TrainTicketReportRowModel
                {
                    IsHead = m.IsHead,
                    FamilyHeadId = m.FamilyHeadId,
                    FcId = m.FamilyHead.FcId,
                    FullName = m.FamilyHead.FullName,
                    VisaImage = m.FamilyHead.VisaImage,
                    InsuranceImage = m.FamilyHead.InsuranceImage,
                    MISImage = m.FamilyHead.MISPdfPath,
                    AirlineImage = m.FamilyHead.AirlineTicketImage,
                    TrainImage = m.FamilyHead.TrainTicketImage
                };

                model.Rows.Add(_);
            }
            return View(model);
        }

        public IActionResult NusukID(int id)
        {
            NusukReportModel model = new();

            var families = _context
                            .FamilyMembers
                            .Include(e => e.FamilyHead)
                            .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ToList();

            foreach (var m in families)
            {
                NusukReportRowModel _ = new NusukReportRowModel
                {
                    IsHead = m.IsHead,
                    FamilyHeadId = m.FamilyHeadId,
                    FcId = m.FamilyHead.FcId,
                    FullName = m.FamilyHead.FullName,
                    NusukId = m.FamilyHead.NusukId
                };

                model.Rows.Add(_);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveNusukID(NusukSaveModel request)
        {
            //return Ok(request);

            FamilyHead head = _context.FamilyHeads.Find(request.FamilyHeadId);
            string trainPath = "";

            var transaction = _context.Database.BeginTransaction();
            try
            {
                if (!string.IsNullOrEmpty(request.NusukId))
                {
                    head.NusukId = request.NusukId.Trim();
                }

                _context.SaveChanges();

                transaction.Commit();
                return Ok(new
                {
                    status = 200,
                    msg = "Saved"
                });
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return BadRequest(new
                {
                    status = 500,
                    msg = "Error!"
                });
            }
        }

        public IActionResult IndividualTokens(int id)
        {

            List<QurbaniIndividualReceiptModel> model = new();

            var members = _context.FamilyMembers
                    .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.Package)
                    .ThenInclude(e => e.Company)
                    .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
                    .ToList();

            foreach (var m in members)
            {
                QurbaniIndividualReceiptModel _ = new()
                {
                    HeadId = m.HeadId,
                    Name = m.FamilyHead.FullName,
                    FcId = m.FamilyHead.FcId,
                    TokenNumber = "Q" + m.FamilyHead.QurbaniToken?.ToString(),
                    MobNumber = _context.FamilyHeadContacts
                                        .AsNoTracking()
                                        .FirstOrDefault(e =>
                                            e.FamilyHeadId == m.FamilyHead.FamilyHeadId &&
                                            e.Preferred)?
                                        .Contact,
                    KSANumber = m.FamilyHead.KSAMobileNumber
                };

                int pax = members.Where(e => e.HeadId == m.HeadId).Count();

                string cId =
                            $"{m.FamilyHead.Package.Company.CompanyCode}.{m.FamilyHead.IndId}" +
                            $".{m.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{m.FamilyHead.Package.Name}";

                var memberInGroup = _context.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _.CID = cId;

                model.Add(_);

            }

            return View(model);
        }


        [HttpPost]
        public IActionResult UploadTrainTicket(UploadDocumentSaveModel request)
        {
            //return Ok(request);

            FamilyHead head = _context.FamilyHeads.Find(request.FamilyHeadId);
            string trainPath = "";

            var transaction = _context.Database.BeginTransaction();
            try
            {
                if (request.TrainFile is not null)
                {
                    trainPath = Path.Combine(_host.WebRootPath, "Images", "TrainTickets", request.TrainFile.FileName);
                    using (FileStream fs = new(trainPath, FileMode.Create))
                    {
                        request.TrainFile.CopyTo(fs);
                    }
                    head.TrainTicketImage = @"Images/TrainTickets/" + request.TrainFile.FileName;
                }

                _context.SaveChanges();

                transaction.Commit();
                return Ok(new
                {
                    status = 200,
                    msg = "Upload Done!"
                });
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return BadRequest(new
                {
                    status = 500,
                    msg = "Error!"
                });
            }
        }


        public IActionResult ReportV2()
        {
            Package package = _context.Packages.FirstOrDefault();

            PackageReportModel model = new PackageReportModel
            {
                FamilyMembers = new List<FamilyMemberModel>(),

                PackageName = package?.Name,
                PackageCode = package?.Code,
                PackageDays = package?.Days,
                PackageDuration = package?.HajjDuration,
                MedinahArrival = package?.MedinahArrival,
            };

            var members = _context.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => !e.FamilyHead.Trashed 
                        && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .AsNoTracking()
                .OrderBy(e => e.HeadId)
                .ToList();

            model.Bookings = members.Count();

            var familiesGroups = _context.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => !e.FamilyHead.Trashed)
                .AsNoTracking()
                .GroupBy(e => e.HeadId)
                .ToList();

            var maleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Male).Count();
            var femaleMembersCount = members.Where(e => e.FamilyHead.Gender == Gender.Female).Count();

            model.FamilyMembers = new List<FamilyMemberModel>();


            var minaAccomodation = (from fh in _context.FamilyHeads
                                    join p in _context.Packages on fh.PackageId equals p.PackageId
                                    join pa in _context.PackageAccomodtions on p.PackageId equals pa.PackageId
                                    //join r in _context.Rooms on pa.RoomId equals r.RoomId
                                    where fh.Trashed == false && pa.PlaceId == 3
                                    select new
                                    {
                                        fh.FamilyHeadId,
                                        //r.RoomCode

                                    }).ToList();

            //return Ok(familiesGroups);
            bool sameFamily = true;
            FamilyMembers prevMember = null;
            for (int i = 0, x = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                string memberName = currentMember.FamilyHead.GivenName + " " + currentMember.FamilyHead.SurName;
                string mehramName = "-";
                string mehramRel = "-";
                string fatherHusbandName = currentMember.FamilyHead.FatherHusbandName;

                var RoomsTypeAssignedToMembers = from af in _context.AdditionalFacilities
                                                 join fhad in _context.FamilyHeadAdditionalFacilities
                                                 on af.AdditionalFacilitiesId equals fhad.AdditionalFacilitiesId
                                                 join ifd in _context.IndividualFacilityDescriptions
                                                 on af.IndividiualFacilityDescriptionId equals ifd.IndividualFacilityDescriptionId
                                                 where fhad.FamilyHeadId == currentMember.FamilyHeadId
                                                 select new
                                                 {
                                                     AdditionalFacility = af,
                                                     IndividualFacilityDescription = ifd
                                                 };

                var Makkahroomtypemember = RoomsTypeAssignedToMembers.Where(e => e.AdditionalFacility.PlaceId == 1); // makkah
                var Medinaroomtypemember = RoomsTypeAssignedToMembers.Where(e => e.AdditionalFacility.PlaceId == 2); // medina
                var Aziziaroomtypemember = RoomsTypeAssignedToMembers.Where(e => e.AdditionalFacility.PlaceId == 4); // azizia

                if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Father)
                {
                    fatherHusbandName += " <b>(F)</b>";
                }
                else if (currentMember.FamilyHead.MehramRelationEnum == MehramRelationEnum.Husband)
                {
                    fatherHusbandName += " <b>(H)</b>";
                }

                if (currentMember.IsHead)
                {
                    memberName += " <b>(FH)</b>";
                }
                else
                {
                    if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        mehramName = currentMember.Head.GivenName;
                        mehramRel = currentMember.FamilyHead?.FamilyHeadRelation?.Relation ?? "-";
                    }
                }

                if (i > 0)
                {
                    prevMember = members.ElementAt(i - 1);
                    if (prevMember.HeadId != currentMember.HeadId)
                    {
                        sameFamily = false;
                    }
                }
                if (!sameFamily)
                {
                    x++;
                    sameFamily = true;
                }

                var memberInGroup = _context.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                int familyPax = familiesGroups.ElementAt(x).Count();

                string cId =
                        $"{currentMember.FamilyHead.Package.Company.CompanyCode}.{currentMember.FamilyHead.IndId}" +
                        $".{currentMember.Head.ExtractedFamilyNumber}({familyPax})" +
                        $".{currentMember.FamilyHead.Package.Name}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Group?.Code ?? "-";
                }

                string megaId = "";
                if (currentMember.FamilyHead.ReferenceId == 0)
                {
                    megaId = "-";
                }
                else
                {
                    currentMember.FamilyHead.ReferenceId?.ToString();
                }

                // Main Contact & Address
                var memberContact = _context.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _context.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                float totalAmount = PackageManager.GetTotalPackageAmountInSarForEntireFamily(_context, currentMember.FamilyHead.PackageId, currentMember.FamilyHeadId);

                var makkahAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == currentMember.FamilyHead.PackageId && e.IndividualFacilityDescription.PlaceId == Place.MAKKAH_ID);

                var medinaAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == currentMember.FamilyHead.PackageId && e.IndividualFacilityDescription.PlaceId == Place.MEDINA_ID);

                var minaAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == currentMember.FamilyHead.PackageId && e.IndividualFacilityDescription.PlaceId == Place.MINA_ID);

                var aziziaAcc = _context.PackageCategoryFacilities
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.PackageId == currentMember.FamilyHead.PackageId && e.IndividualFacilityDescription.PlaceId == Place.AZIZIA_ID);

                var invoices = _context.InvoicesWithoutTrash.Select(e => new { e.InvoiceId, e.FamilyHeadId }).Where(e => e.FamilyHeadId == currentMember.HeadId);
                string invoicesString = string.Join(", ", invoices.Select(e => "R#" + e.InvoiceId));


                model.FamilyMembers.Add(
                    new FamilyMemberModel
                    {
                        ApplicantType = currentMember.FamilyHead.ApplicantType,
                        FamilyHeadId = currentMember.FamilyHeadId,
                        MegaId = megaId,
                        EHajjId = currentMember.FamilyHead.EHajjId,
                        MOFA = currentMember.FamilyHead.MofaNumber,
                        FID = currentMember.Head.FcId,
                        CID = cId,
                        GNumber = groupNumber,
                        FormNumber = currentMember.FamilyHead.FormNumber,
                        Image = currentMember.FamilyHead.MemberImage,
                        Name = memberName,
                        FatherHusbandName = fatherHusbandName,
                        Mehram = mehramName,
                        MehramRelation = mehramRel,
                        MobileNumber = memberContact?.Contact,
                        Address = memberAddress?.Address,
                        Age = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth),
                        Gender = currentMember.FamilyHead.Gender.ToString(),
                        HajjSerialNumber = currentMember.FamilyHead.HajjAppSerialNumber,
                        VisaNumber = currentMember?.FamilyHead.VisaNumber,
                        TicketNumber = currentMember?.FamilyHead.TicketNumber,
                        PassportNumber = currentMember.FamilyHead.PassportNumber,
                        HajjAppNumber = currentMember.FamilyHead.HajjApplicationNumber,
                        FileNumber = currentMember.FamilyHead.FileNumber,
                        Makkah = makkahAcc?.IndividualFacilityDescription.Room.RoomCode,
                        Medinah = medinaAcc?.IndividualFacilityDescription.Room.RoomCode,
                        Azizia = aziziaAcc?.IndividualFacilityDescription.Room.RoomCode,
                        Mina = minaAcc?.IndividualFacilityDescription.Room.RoomCode,
                        FriendId = currentMember.FamilyHead?.FriendId?.ToString() ?? "-",
                        TotalAmount = totalAmount.ToString("N0"),
                        Invoices = invoicesString,
                        PackageName = currentMember.FamilyHead.Package.Name ?? ""

                    }
                );
            }

            int familyCount = familiesGroups.Count;

            model.PackageSummary = new PackageSummaryModel
            {
                MembersCount = members.Count,
                FamiliesCount = familyCount,
                MalesCount = maleMembersCount,
                FemalesCount = femaleMembersCount
            };

            for (int i = 0; i < members.Count; i++)
            {
                var currentMember = members.ElementAt(i);
                int memberAge = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth);
                if (memberAge < 2)
                {
                    model.PackageSummary.InfantsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.InfantMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.InfantFemales++;
                    }
                }
                else if (memberAge >= 2 && memberAge < 9)
                {
                    model.PackageSummary.ChildsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.ChildMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.ChildFemales++;
                    }
                }
                else if (memberAge >= 9)
                {
                    model.PackageSummary.AdultsCount++;
                    if (currentMember.FamilyHead.Gender == Gender.Male)
                    {
                        model.PackageSummary.AdultMales++;
                    }
                    else if (currentMember.FamilyHead.Gender == Gender.Female)
                    {
                        model.PackageSummary.AdultFemales++;
                    }
                }
            }

            return View(model);
        }

        public IActionResult ReceiptReport(int id)
        {
            var newInvoices = _context.NewInvoice
                                .Include(e => e.FamilyHead)
                                //.ThenInclude(e => e.Currency)
                                //.Include(e => e.Currency)
                                .Where(e => e.FamilyHead.PackageId == id)
                                .ToList();

            ViewBag.Currencies = _context.Currencies.Where(e => !e.Trashed);
            ViewBag.PackageId = id;

            return View(newInvoices);
        }

        [HttpPost]
        public IActionResult ReceiptReport(PackageReceiptReportModel request)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < request.NewInvoiceIds.Count; i++)
                    {
                        NewInvoice inv = _context.NewInvoice.Find(request.NewInvoiceIds[i]);

                        inv.AmountRecieved = request.Amounts[i];
                        inv.Date = request.Dates[i];
                        inv.ROE = request.ROEs[i];
                        inv.Remarks = request.Remarks[i];
                        inv.Currencies = request.CollectionCurrencyIds[i];

                        _context.SaveChanges();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BadRequest(new { message = "Transaction failed", error = ex.Message });
                }
            }

            var newInvoices = _context.NewInvoice
                               .Include(e => e.FamilyHead)
                               //.ThenInclude(e => e.Currency)
                               //.Include(e => e.Currency)
                               .Where(e => e.FamilyHead.PackageId == request.PackageId)
                               .ToList();

            ViewBag.Currencies = _context.Currencies.Where(e => !e.Trashed);
            ViewBag.PackageId = request.PackageId;


            return View(newInvoices);
        }

    }
}


namespace TMG.Models.PackageModels
{
    public sealed record PackageReceiptReportModel
    {
        public int PackageId { get; set; }
        public List<int> NewInvoiceIds { get; set; }
        public List<int> FamilyHeadIds { get; set; }
        public List<Currencies> CollectionCurrencyIds { get; set; }
        public List<DateTime> Dates { get; set; }
        public List<float> Amounts { get; set; }
        public List<float> ROEs { get; set; }
        public List<string> Remarks { get; set; }
    }

    public record TrainTicketReportModel
    {
        public List<TrainTicketReportRowModel> Rows { get; set; } = new();
    }
    public record NusukReportModel
    {
        public List<NusukReportRowModel> Rows { get; set; } = new();
    }
    public record TrainTicketReportRowModel
    {
        public bool IsHead { get; set; }
        public int SrNo { get; set; }
        public int FamilyHeadId { get; set; }
        public string FcId { get; set; }
        public string FullName { get; set; }
        public string VisaImage { get; set; }
        public string InsuranceImage { get; set; }
        public string MISImage { get; set; }
        public string AirlineImage { get; set; }
        public string TrainImage { get; set; }
    }

    public record NusukReportRowModel
    {
        public bool IsHead { get; set; }
        public int SrNo { get; set; }
        public int FamilyHeadId { get; set; }
        public string FcId { get; set; }
        public string FullName { get; set; }
        public string NusukId { get; set; }
    }

    public class PackageMakkahRoomListing
    {
        public List<PackageMakkahRoomListingRow> Rows { get; set; } = new();
    }

    public class PackageMakkahRoomListingRow
    {
        public int FamilyHeadId { get; set; }
        public int HeadId { get; set; }
        public int SrNo { get; set; }
        public string FcId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
        public string HajjApplicationNumber { get; set; }
        public string VisaNumber { get; set; }
        public string RoomType { get; set; }
        public int RowSpan { get; set; }
        public int RoomPAX { get; set; }
        public int FloorNumber { get; set; }
        public int RoomNumber { get; set; }
        public bool SameFamily { get; set; }
        public bool SameRoomType { get; set; }
    }

    public class QRModel
    {
        public string PackageName { get; set; }
        public string FcId { get; set; }
        public string HajjiName { get; set; }
        public byte[] qrCodeImage { get; set; }
        public string qrCode { get; set; }
    }
    public class UploadDocumentSaveModel
    {
        public int FamilyHeadId { get; set; }
        public IFormFile VisaFile { get; set; }
        public IFormFile InsuranceFile { get; set; }
        public IFormFile AirlineFile { get; set; }
        public IFormFile TrainFile { get; set; }
        public IFormFile MISFile { get; set; }
    }
    public class NusukSaveModel
    {
        public int FamilyHeadId { get; set; }
        public string NusukId { get; set; }
    }
    public record VisaInsuranceReportModel
    {
        public List<VisaInsuranceReportRowModel> Rows { get; set; } = new();
    }

    public record VisaInsuranceReportRowModel
    {
        public bool IsHead { get; set; }
        public int SrNo { get; set; }
        public int FamilyHeadId { get; set; }
        public string FcId { get; set; }
        public string FullName { get; set; }
        public string VisaImage { get; set; }
        public string InsuranceImage { get; set; }
        public string MISImage { get; set; }
        public string AirlineImage { get; set; }
        public string TrainImage { get; set; }
    }

    public class QurbaniQueueModel
    {
        public int SNo { get; set; }
        public int FullName { get; set; }
    }
    public record PackageQurbaniSaveModel
    {
        public int PackageId { get; set; }
        public List<int> FamilyHeadIds { get; set; }
        // public List<bool> QurbaniExcluded { get; set; }
        public List<bool> QurbaniPaid { get; set; }
        public List<bool> QurbaniComplimentry { get; set; }
        // public List<float> PkrAmounts { get; set; }
        // public List<float> SarAmounts { get; set; }
        // public List<float> UsdAmounts { get; set; }
    }

    public record PackageQurbaniReportModel
    {
        public float SARROE { get; set; }
        public float USDROE { get; set; }
        public int PackageId { get; set; }
        public List<int> FamilyHeadIds { get; set; }
        // public List<bool> QurbaniExcluded { get; set; }
        public List<bool> QurbaniPaid { get; set; }
        public List<bool> QurbaniComplimentry { get; set; }
        // public List<float> PkrAmounts { get; set; }
        // public List<float> SarAmounts { get; set; }
        // public List<float> UsdAmounts { get; set; }

        public string CompanyName { get; set; }
        public string PackageName { get; set; }
        public string PackageCode { get; set; }
        public string PackageDays { get; set; }
        public string PackageDuration { get; set; }
        public string MedinahArrival { get; set; }
        public int Quota { get; set; }
        public List<PackageQurbaniReportMemberModel> Members { get; set; } = new();
    }

    public record PackageQurbaniReportMemberModel
    {
        public int FamilyHeadId { get; set; }
        public int HeadId { get; set; }
        public bool IsHead { get; set; }
        public int SrNo { get; set; }
        public string FullName { get; set; }
        public string Passport { get; set; }
        public string Gender { get; set; }
        public string FcId { get; set; }
        public QurbaniTypes QurbaniType { get; set; }
        public bool PaidQurbani { get; set; }
        public bool ComplimentryQurbani { get; set; }
        // public float QurbaniPkr { get; set; }
        // public float QurbaniSar { get; set; }
        // public float QurbaniUsd { get; set; }

    }

    public record IdCardReportModel
    {
        public string Munazzim { get; set; }
        public string Company { get; set; }
        public string CompanyLicenseNo { get; set; }
        public string Package { get; set; }
        public string Maktab { get; set; }
        public string MakkahAccomodation { get; set; }
        public string MakkahLocation { get; set; }
        public string MedinahAccomodation { get; set; }
        public string MedinahLocation { get; set; }
        public string AziziaAccomodation { get; set; }
        public string AziziaLocation { get; set; }
        public List<IdCardReportMemberModel> Members { get; set; } = new();
    }

    public record IdCardReportMemberModel
    {
        public string HajiName { get; set; }
        public string HajiImg { get; set; }
        public string HajiSurName { get; set; }
        public string FatherName { get; set; }
        public string CID { get; set; }
        public string Passport { get; set; }
        public string Gender { get; set; }
        public string HajjAppNo { get; set; }
        public string MakkahAccomodation { get; set; }
        public string MakkahLocation { get; set; }
        public string MedinahAccomodation { get; set; }
        public string MedinahLocation { get; set; }
        public string AziziaAccomodation { get; set; }
        public string AziziaLocation { get; set; }




    }


    public record PackageReportSaveModel
    {
        public int PackageId { get; set; }
        public List<ApplicantType> Categories { get; set; }
        public List<int> FamilyHeadIds { get; set; }
        public List<string> EHajjIds { get; set; }
        public List<string> MOFAs { get; set; }
        public List<string> HajjSerials { get; set; }
        public List<string> HajjAppNumbers { get; set; }
        public List<string> VisaNumbers { get; set; }
        public List<string> TicketNumbers { get; set; }
    }

    public record EIADFReportModel
    {
        public string PackageName { get; set; }
        public string PackageCode { get; set; }

        public Dictionary<string, int> Descriptions { get; set; }
        public List<EIADFMembersModel> Members { get; set; } = new();
    }

    public record EIADFMembersModel
    {
        public int SR { get; set; }
        public string MegaReference { get; set; }
        public string SPO { get; set; }
        public string FCID { get; set; }
        public string FullName { get; set; }
        public string EIADF { get; set; }
    }

    public class AirlineSummaryReportModel
    {
        public string? POD { get; set; }
        public string? Pax { get; set; }
        public string? PNRName { get; set; }
        public string? PNRNo { get; set; }
        public string? Arrival { get; set; }
        public string? Sector { get; set; }
        public string? Time { get; set; }
        public string? Departure { get; set; }
        public string? DepSector { get; set; }
        public string? DepTime { get; set; }
    }
    public class TrainSummaryModel
    {
        public string? POD { get; set; }
        public string? Pax { get; set; }
        public string? PNRName { get; set; }
        public string? PNRNo { get; set; }
        public string? Departure { get; set; }
        public string? Sector { get; set; }
        public string? Time { get; set; }
        public string? Arrival { get; set; }
        public string? ArrSector { get; set; }
        public string? ArrTime { get; set; }
    }
    public record PackageFacilitiesReportModel
    {
        public string PackageName { get; set; }
        public string PackageCode { get; set; }
        public List<string> RoomHeaders { get; set; } = new();
        public List<int> RoomCounts { get; set; } = new();
        public int FamilyMembersCount { get; set; }
        public List<PackageFacilitiesReportModelHeads> Heads { get; set; } = new();
    }

    public record PackageFacilitiesReportModelHeads
    {
        public string FCId { get; set; }
        public List<PackageFacilitiesReportModelMembers> Members { get; set; } = new();
    }

    public record PackageFacilitiesReportModelMembers
    {
        public string SNo { get; set; }
        public string Name { get; set; }
        public string FCId { get; set; }
        public int FamilyHeadId { get; set; }
        public List<PackageFacilitiesReportModelRoomList> Rooms { get; set; } = new();
    }

    public record PackageFacilitiesReportModelRoomList
    {
        public AdditionalFacilities IADF { get; set; }
        public bool Checked { get; set; }
    }

    public record SaveIADFReportModel
    {
        public List<bool> FacilitySelected { get; set; }
        public List<int> FacilityIds { get; set; }
        public List<int> FamilyHeadIds { get; set; }
    }
}



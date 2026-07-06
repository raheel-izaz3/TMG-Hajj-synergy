using TMG.Data;
using TMG.ManagerClasses;
using Microsoft.AspNetCore.Mvc;
using TMG.Models;
using TMG.Models.CompanyModels;
using TMG.Models.FormModels.CompanyModels;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using TMG.Models.PackageModels;
using TMG.Models.AccountModels;
using TMG.Models.InvoiceModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using iText.Commons.Actions.Contexts;
using SkiaSharp;
using TMG.Models.AccomodationModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace TMG.Models.CompanyModels
{
    public record PackageListReportModel
    {
        public List<PackageReportModel> packageReportModels { get; set; } = new();
        public List<AirlineSummaryReportModel> AirlineSummary { get; set; } = new();
        public List<TrainSummaryModel> TrainSummary { get; set; } = new();
    }

    public record RequiredDocumentsCompanyReportModel
    {
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public List<string> RequiredDocHeaders { get; set; } = new();
        public int FamilyMembersCount { get; set; }
        public List<int> TotalPending { get; set; } = new();
        public List<int> TotalReceived { get; set; } = new();

        public List<PackageGroup> PackageGroups { get; set; } = new();
    }

    public record PackageGroup
    {
        public string PackageName { get; set; }
        public string PackageCode { get; set; }

        public List<CompanyRequiredDocumentFamilyMemberGroupModel> Families { get; set; } = new();
    }

    public record CompanyRequiredDocumentFamilyMemberGroupModel
    {
        public string FCId { get; set; }
        public List<CompanyFamilyMemberRequiredDocuments> Documents { get; set; } = new List<CompanyFamilyMemberRequiredDocuments>();
    }

    public record CompanyFamilyMemberRequiredDocuments
    {
        public int FamilyHeadId { get; set; }
        public string Name { get; set; }
        public List<CompanyDocumentEntry> Entries { get; set; } = new List<CompanyDocumentEntry>();
    }

    public record CompanyDocumentEntry
    {
        public RequiredDocument RequiredDocument { get; set; }
        public bool Checked { get; set; }
    }

}

namespace TMG.Controllers.CompanyControllers
{
    public class CompanyReportController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public CompanyReportController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult RequiredDocuments(int id)
        {
            // Package package = _dbContext.Packages.Find(id);
            var company = _dbContext.Companies.Select(e => new
            {
                e.CompanyId,
                e.CompanyCode,
                e.CompanyName
            }).FirstOrDefault(e => e.CompanyId == id);
            var documents = _dbContext.RequiredDocumentsWithoutTrash.ToList();

            var familyHeads = _dbContext.FamilyHeadsWithoutTrash
                            .Include(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Select(e => new
                            {
                                e.Package.CompanyId,
                                e.PackageId,
                                e.FamilyHeadId,
                                e.IsHead,
                                e.FullName,
                                e.FcId
                            })
                            .Where(e => e.CompanyId == id && e.IsHead)
                            .ToList();

            var families = _dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .ThenInclude(e => e.SPO)
            .Include(e => e.FamilyHead)
            .ThenInclude(e => e.Package)
            .ThenInclude(e => e.Company)
            .Where(e => !e.FamilyHead.Trashed)
            .Select(e => new
            {
                CompanyId = e.FamilyHead.Package.CompanyId,
                PackageId = e.FamilyHead.PackageId,
                FamilyHeadId = e.FamilyHead.FamilyHeadId,
                FullName = e.FamilyHead.GivenName,
                SPO = e.FamilyHead.SPO.Name,
                Trashed = e.FamilyHead.Trashed,
                HeadId = e.HeadId
            })
             .ToList();

            var totalReceived = _dbContext.FamilyHeadDocuments
               .Join(_dbContext.FamilyHeads,
                   fhd => fhd.FamilyHeadId,
                   fh => fh.FamilyHeadId,
                   (fhd, fh) => new { fhd, fh })
               .Where(joined => joined.fh.Package.CompanyId == id && !joined.fh.Trashed)
               .GroupBy(joined => joined.fhd.RequiredDocumentId)
               .Select(group => new
               {
                   RequiredDocumentId = group.Key,
                   TotalReceived = group.Count(item => item.fhd.Recieved)
               })
               .ToList();

            var totalPending = _dbContext.FamilyHeadDocuments
                .Join(_dbContext.FamilyHeads,
                    fhd => fhd.FamilyHeadId,
                    fh => fh.FamilyHeadId,
                    (fhd, fh) => new { fhd, fh })
                .Where(joined => joined.fh.Package.CompanyId == id && !joined.fh.Trashed)
                .GroupBy(joined => joined.fhd.RequiredDocumentId)
                .Select(group => new
                {
                    RequiredDocumentId = group.Key,
                    TotalPending = group.Count(item => !item.fhd.Recieved)
                })
                .ToList();

            RequiredDocumentsCompanyReportModel model = new()
            {
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                TotalPending = totalPending.Select(e => e.TotalPending).ToList(),
                TotalReceived = totalReceived.Select(e => e.TotalReceived).ToList(),
                RequiredDocHeaders = documents.Select(e => e.Documents).ToList(),
                FamilyMembersCount = families.Count
            };

            var packages = _dbContext.PackagesWithoutTrash.Where(e => e.CompanyId == id).ToList();

            var memberDocuments = _dbContext.FamilyHeadDocuments
                .Include(e => e.RequiredDocument)
                .Where(e => e.FamilyHead.Package.CompanyId == id)
                .ToList();

            for (int i = 0; i < packages.Count; i++)
            {
                var currPackage = packages.ElementAt(i);
                var currPackageHeads = familyHeads
                        .Where(e => e.PackageId == currPackage.PackageId)
                        .ToList();

                PackageGroup packageGroup = new()
                {
                    PackageCode = currPackage.Code,
                    PackageName = currPackage.Name
                };

                for (int j = 0; j < currPackageHeads.Count; j++)
                {
                    var _currFam = currPackageHeads.ElementAt(j);
                    var _currFamMembers = families.Where(e => e.HeadId == _currFam.FamilyHeadId).ToList();

                    var famModel = new CompanyRequiredDocumentFamilyMemberGroupModel()
                    {
                        FCId = $"{_currFam.FcId}"
                    };

                    for (int k = 0; k < _currFamMembers.Count; k++)
                    {
                        var _currFamMember = _currFamMembers.ElementAt(k);
                        var memberModel = new CompanyFamilyMemberRequiredDocuments
                        {
                            FamilyHeadId = _currFamMember.FamilyHeadId,
                            Name = _currFamMember.FullName
                        };

                        var currMemberDocuments = memberDocuments
                            .Where(e => e.FamilyHeadId == _currFamMember.FamilyHeadId)
                            .ToList();

                        for (int l = 0; l < documents.Count; l++)
                        {
                            var currDocument = documents.ElementAt(l);
                            bool docChecked = currMemberDocuments
                                .FirstOrDefault(e => e.RequiredDocumentId == currDocument.RequiredDocumentId)?
                                .Recieved ?? false;

                            memberModel.Entries.Add(new CompanyDocumentEntry
                            {
                                RequiredDocument = currDocument,
                                Checked = docChecked
                            });

                        }

                        famModel.Documents.Add(memberModel);
                    }

                    packageGroup.Families.Add(famModel);
                }

                model.PackageGroups.Add(packageGroup);

            }


            // return Ok(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult RequiredDocuments(DocumentReceivedSaveModel request)
        {
            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var reqDocs = _dbContext.FamilyHeadDocuments.ToList();
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

                _dbContext.SaveChanges();
                transaction.Commit();
                return RedirectToAction("Index", "Company");
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

        public IActionResult CompanySummaryReportV2(int id)
        {
            var packages = _dbContext.PackagesWithoutTrash
                .Include(a => a.Company)
                .Where(a => a.CompanyId == id)
                .ToList();

            var packageIds = packages.Select(e => e.PackageId).ToList();

            PackageReportModel model = new PackageReportModel();

            var company = _dbContext.CompaniesWithoutTrash
                                .Select(e => new
                                {
                                    e.CompanyId,
                                    e.CompanyCode,
                                    e.CompanyName,
                                })
                                .FirstOrDefault(e => e.CompanyId == id);

            model.CompanyName = company.CompanyName;

            var members = _dbContext.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.SPO)
                .Where(e => packageIds.Contains(e.FamilyHead.PackageId) && !e.FamilyHead.Trashed
                        && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .OrderBy(e => e.FamilyHead.PackageId)
                .ThenBy(e => e.HeadId)
                .ToList();

            var familyHeadTransports = _dbContext.FamilyHeadTransports
                                            .Where(e => e.VehicleId != null)
                                            .Select(e => new
                                            {
                                                e.Vehicle.Price,
                                                e.FamilyHead.PackageId,
                                                e.FamilyHead.Trashed,
                                                e.FamilyHeadId,
                                                e.FamilyHead.RouteAsPerPackage
                                            })
                                            .Where(e => packageIds.Contains(e.PackageId) && !e.Trashed && !e.RouteAsPerPackage)
                                            .ToList();

            var familyHeadFlights = _dbContext.FamilyHeadFlight
                                        .Select(e => new
                                        {
                                            e.FamilyHeadId,
                                            e.FamilyHead.PackageId,
                                            e.FamilyHead.FlightAsPerPackage,
                                            e.FamilyHead.Trashed,
                                            e.Taxes,
                                            e.Commission,
                                            e.FareAmount, // amount assumed as SAR
                                            e.TicketStatus,
                                            e.Total
                                        })
                                        .Where(e => packageIds.Contains(e.PackageId) && !e.Trashed && !e.FlightAsPerPackage && e.TicketStatus.HasValue && e.TicketStatus == TicketStatus.Active)
                                        .ToList();

            var familyHeadTrains = _dbContext.FamilyHeadTrain
                                        .Select(e => new
                                        {
                                            e.FamilyHeadId,
                                            e.FamilyHead.PackageId,
                                            e.FamilyHead.TrainAsPerPackage,
                                            e.FamilyHead.Trashed,
                                            e.Taxes,
                                            e.Commission,
                                            e.FareAmount, // amount assumed as SAR
                                            e.TicketStatus,
                                            e.Total
                                        })
                                        .Where(e => packageIds.Contains(e.PackageId) && !e.Trashed && !e.TrainAsPerPackage && e.TicketStatus.HasValue && e.TicketStatus == TicketStatus.Active)
                                        .ToList();

            var familyHeadAccommodations = _dbContext.FamilyHeadAccomodations
                                            .Select(e => new
                                            {
                                                e.FamilyHeadId,
                                                e.FamilyHead.PackageId,
                                                e.FamilyHead.Trashed,
                                                e.IndividualFacilityDescription.Price
                                            })
                                            .Where(e => packageIds.Contains(e.PackageId) && !e.Trashed)
                                            .ToList();

            var memberHeads = members.Where(e => e.IsHead).ToList();

            var memberIds = members.Select(e => e.FamilyHeadId).ToList();
            var memberHeadIds = memberHeads.Select(e => e.FamilyHeadId).ToList();

            var eiadfs = _dbContext.FamilyHeadExtras
                    .Where(e => memberIds.Contains(e.FamilyHeadId))
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Price,
                        e.Discount,
                    })
                    .ToList();

            model.totalpax = members.Count;

            var invoices = _dbContext.NewInvoicesWithoutTrash.ToList();

            model.FamilyMembers = new List<FamilyMemberModel>();

            for (int i = 0, x = 0; i < memberHeads.Count; i++)
            {
                var currentMember = memberHeads.ElementAt(i);

                string memberName = currentMember.FamilyHead.GivenName;
                string mehramName = "-";
                string mehramRel = "-";
                string fatherHusbandName = currentMember.FamilyHead.FatherHusbandName;

                int headID = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId).HeadId;

                int _FamilyPAX = _dbContext.FamilyMembers.Where(a => a.HeadId == headID).Count();

                var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                int familyPax = members
                                    .Where(e => e.HeadId == currentMember.FamilyHeadId)
                                    .Count();

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

                // Main Contact & Address
                var memberContact = _dbContext.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _dbContext.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberage = DateTimeManager.CalculateAge(currentMember.FamilyHead.DateOfBirth);

                var remarks = _dbContext.FamilyHeads
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.StayWithId != null);

                var currMemberInvoices = invoices
                                    .Where(e => e.FamilyHeadId == currentMember.HeadId);

                var invoicesCleared = currMemberInvoices
                                .Where(e => e.ReceiptStatus == ReceiptStatus.Cleared);

                float totalPaymentReceivedSar = 0;

                float paymentInTrasitSar = 0;

                float basicPackageAmount = 0f, eiadfAmount = 0f, discountAmount = 0f;

                var headMembers = members.Where(e => e.HeadId == currentMember.HeadId).ToList();
                var headMemberIds = headMembers.Select(e => e.FamilyHeadId).ToList();
                var headMembersEIadfs = eiadfs.Where(e => headMemberIds.Contains(e.FamilyHeadId)).ToList();
                var headMemberTransports = familyHeadTransports.Where(e => headMemberIds.Contains(e.FamilyHeadId)).ToList();

                foreach (var m in headMembers)
                {
                    if (m.FamilyHead.IsInfant)
                    {
                        basicPackageAmount += currentMember.FamilyHead.Package.InfantSar;
                    }
                    else if (m.FamilyHead.IsChild)
                    {
                        basicPackageAmount += currentMember.FamilyHead.Package.ChildSar;
                    }
                    else
                    {
                        basicPackageAmount += currentMember.FamilyHead.Package.AdultSar;
                    }

                    //if (m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                    //{
                    //    ticketAmount += currentMember.FamilyHead.Package.FlightTicketPriceSar;
                    //    if (m.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                    //    {
                    //        ticketAmount += m.FamilyHead.FlightTicketDifferencesSar ?? 0f;
                    //    }
                    //}

                    discountAmount += m.FamilyHead.SarDiscountAmount;
                }

                model.TotalBasicPackage += basicPackageAmount;
                model.TotalEIADFAmount += eiadfAmount;
                model.TotalDiscount += discountAmount;

                float totalPackageAmount = basicPackageAmount + eiadfAmount - discountAmount;

                model.GrandTotalPackage += totalPackageAmount;

                float balanceBeforeTrasit = totalPackageAmount - totalPaymentReceivedSar;
                model.TotalBalanceBeforeTransitAmount += balanceBeforeTrasit;

                float balanceAfterTrasit = totalPackageAmount - (totalPaymentReceivedSar + paymentInTrasitSar);
                model.TotalFinalBalanceReceivable += balanceAfterTrasit;

                model.TotalReceivedAmount += totalPaymentReceivedSar;

                double percOfPaymentReceived = Math.Round((totalPaymentReceivedSar / totalPackageAmount) * 100, 2);

                var packageRooms = _dbContext.PackageCategoryFacilities
                    .Include(e => e.IndividualFacilityDescription)
                    .Where(e => e.PackageId == currentMember.FamilyHead.PackageId);

                var packageFlights = _dbContext.PackageFlights
                                        .Where(e => e.PackageId == currentMember.FamilyHead.PackageId)
                                        .ToList();

                var packageTrains = _dbContext.PackageTrains
                                        .Where(e => e.PackageId == currentMember.FamilyHead.PackageId)
                                        .ToList();

                var _ = new FamilyMemberModel
                {
                    BaseCurrency = currentMember.FamilyHead.Currencies.ToString(),
                    CompanyCode = company.CompanyCode,
                    PackageName = currentMember.FamilyHead.Package.Name,
                    Group = groupNumber,
                    EIADFAmount = eiadfAmount.ToString(),
                    DiscountAmount = discountAmount.ToString(),
                    EHajj = currentMember.FamilyHead.Ehajj is not null ? 1 : 0,
                    RowSpan = _FamilyPAX,
                    IsHead = currentMember.IsHead,
                    HeadName = currentMember.FamilyHead.FullName,
                    SPOName = currentMember.FamilyHead?.SPO?.Name,
                    POD = currentMember.FamilyHead.POD,
                    pax = familyPax.ToString(),
                    M = memberage >= 12 && currentMember.FamilyHead.Gender == Gender.Male ? "1" : "0",
                    F = memberage >= 12 && currentMember.FamilyHead.Gender == Gender.Female ? "1" : "0",
                    C = memberage >= 2 && memberage < 12 ? "1" : "0",
                    I = memberage <= 2 ? "1" : "0",
                    remarks = remarks == null ? "" : remarks.FullName,
                    ReceivedAmountTotal = totalPaymentReceivedSar.ToString(),
                    BalanceBeforeTransit = balanceBeforeTrasit.ToString(),
                    PaymentinTransit = paymentInTrasitSar.ToString(),
                    FinalBalanceReceivable = balanceAfterTrasit.ToString(),
                    ReceivedAmountPercent = percOfPaymentReceived.ToString() + "%",
                    //Days = daysDifference.ToString(),
                    //LastPaymentRecDate = DateTimeManager.GetDate(lastInvoice?.ReceiptDate),
                    //ReceiptNo = lastInvoice is not null ? $"{currentMember.Head.FcId} R#{lastInvoice?.InvoiceId}" : "",
                    Residency = currentMember.FamilyHead.Residency,
                    BioStatus = currentMember.FamilyHead.ENumberVerified == true ? "1" : "0",
                    //AgreementSign = agreement == null ? "0" : "1",
                    //DateFromSign = agreement == null ? "0" : (agreement.SignDataForm ? "1" : "0"),
                    Category = currentMember.FamilyHead.ApplicantType.ToString(),
                    //MegaId = megaId,
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
                };

                float? newTotal = 0f;
                float newBasic = 0f;
                float newFlightTicket = 0f;
                float newTrainTicket = 0f;
                float newRoomTotal = 0f;
                float newGeneralDiscount = 0f;


                float additionalTrasnportSar = headMemberTransports.Sum(e => e.Price);


                var extraPriceSar = headMembersEIadfs.Sum(e => e.Price);
                extraPriceSar -= headMembersEIadfs.Sum(e => e.Discount) ?? 0f;

                //return Ok(headMembersEIadfs);

                if (currentMember.FamilyHead.Currencies == Currencies.SAR)
                {
                    _.EIADFAmount = extraPriceSar.ToString();
                    _.AdditionalTransportAmount = additionalTrasnportSar.ToString();
                }
                else if (currentMember.FamilyHead.Currencies == Currencies.PKR)
                {
                    _.EIADFAmount = (extraPriceSar * currentMember.FamilyHead.Package.PKRROE).ToString();
                    _.AdditionalTransportAmount = (additionalTrasnportSar * currentMember.FamilyHead.Package.PKRROE).ToString();

                }
                else if (currentMember.FamilyHead.Currencies == Currencies.USD)
                {
                    _.EIADFAmount = (extraPriceSar * currentMember.FamilyHead.Package.USDROE).ToString();
                    _.AdditionalTransportAmount = (additionalTrasnportSar * currentMember.FamilyHead.Package.USDROE).ToString();
                }


                foreach (var m in headMembers)
                {
                    var currency = m.FamilyHead.Currencies ?? Currencies.SAR;
                    // Basic Package Amount Calculation
                    newBasic += CalculateBasicAmount(m.FamilyHead, currentMember.FamilyHead);

                    // Flight Ticket Calculation
                    if (m.FamilyHead.FlightAsPerPackage && m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                    {
                        newFlightTicket += CalculateTicketAmount(packageFlights.Sum(e => e.TicketAmount) ?? 0f, currency, currentMember.FamilyHead.Package);
                    }
                    else if (!m.FamilyHead.FlightAsPerPackage && m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                    {
                        //newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                        //    .Sum(e => e.Commission ?? 0f + e.FareAmount ?? 0f + e.Taxes ?? 0f);

                        if (currentMember.FamilyHead.Currencies == Currencies.SAR)
                        {
                            newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                                .Sum(e => e.Total ?? 0f);
                        }
                        else if (currentMember.FamilyHead.Currencies == Currencies.PKR)
                        {
                            newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                               .Sum(e => e.Total ?? 0f) * currentMember.FamilyHead.Package.PKRROE;
                        }
                        else if (currentMember.FamilyHead.Currencies == Currencies.USD)
                        {
                            newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                               .Sum(e => e.Total ?? 0f) * currentMember.FamilyHead.Package.USDROE;
                        }
                    }

                    // Train Ticket Calculation
                    if (m.FamilyHead.TrainAsPerPackage)
                    {
                        newTrainTicket += CalculateTicketAmount(packageTrains.Sum(e => e.TicketAmount) ?? 0f, currency, currentMember.FamilyHead.Package);
                    }
                    else if (!m.FamilyHead.TrainAsPerPackage)
                    {

                        if (currentMember.FamilyHead.Currencies == Currencies.SAR)
                        {
                            newTrainTicket += familyHeadTrains.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                                .Sum(e => e.Total ?? 0f);
                        }
                        else if (currentMember.FamilyHead.Currencies == Currencies.PKR)
                        {
                            newTrainTicket += familyHeadTrains.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                               .Sum(e => e.Total ?? 0f) * currentMember.FamilyHead.Package.PKRROE;
                        }
                        else if (currentMember.FamilyHead.Currencies == Currencies.USD)
                        {
                            newTrainTicket += familyHeadTrains.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                               .Sum(e => e.Total ?? 0f) * currentMember.FamilyHead.Package.USDROE;
                        }
                    }

                    if (m.FamilyHead.AccomodationAsPerPackage)
                    {
                        // Room Total Calculation
                        newRoomTotal += CalculateTicketAmount(packageRooms.Sum(e => e.IndividualFacilityDescription.Price), currency, currentMember.FamilyHead.Package);
                    }
                    else
                    {

                        if (currentMember.FamilyHead.Currencies == Currencies.SAR)
                        {
                            newRoomTotal += familyHeadAccommodations.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                                .Sum(e => e.Price);
                        }
                        else if (currentMember.FamilyHead.Currencies == Currencies.PKR)
                        {
                            newRoomTotal += familyHeadAccommodations.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                               .Sum(e => e.Price) * currentMember.FamilyHead.Package.PKRROE;
                        }
                        else if (currentMember.FamilyHead.Currencies == Currencies.USD)
                        {
                            newRoomTotal += familyHeadAccommodations.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                               .Sum(e => e.Price) * currentMember.FamilyHead.Package.USDROE;
                        }
                    }


                    // General Discount will be as per base currency
                    newGeneralDiscount += m.FamilyHead.GeneralDiscount ?? 0f;
                }

                _.BasicPackageAmount = newBasic.ToString();
                _.TicketAmount = newFlightTicket.ToString();
                _.TrainTicketAmount = newTrainTicket.ToString();
                _.DiscountAmount = newGeneralDiscount.ToString();

                newTotal += newBasic + newFlightTicket + newTrainTicket + newRoomTotal - newGeneralDiscount + extraPriceSar + additionalTrasnportSar;

                _.RoomsAmount = newRoomTotal.ToString();
                _.PackageAmount = newTotal.ToString();


                float convertedClearedAmount = 0f;

                foreach (NewInvoice item in invoicesCleared)
                {
                    if (currentMember.FamilyHead.Currencies == Currencies.SAR)
                    {
                        if (item.Currencies == Currencies.SAR)
                        {
                            convertedClearedAmount += item.AmountRecieved;
                        }
                        else if (item.Currencies == Currencies.PKR)
                        {
                            convertedClearedAmount += item.AmountRecieved / item.ROE;
                        }
                        else if (item.Currencies == Currencies.USD)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                        else if (item.Currencies == Currencies.AED)
                        {
                            convertedClearedAmount += item.AmountRecieved / item.ROE;
                        }
                        else if (item.Currencies == Currencies.GBP)
                        {
                            convertedClearedAmount += item.AmountRecieved / item.ROE;
                        }
                        else if (item.Currencies == Currencies.EURO)
                        {
                            convertedClearedAmount += item.AmountRecieved / item.ROE;
                        }
                    }
                    else if (currentMember.FamilyHead.Currencies == Currencies.PKR)
                    {
                        if (item.Currencies == Currencies.SAR)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                        else if (item.Currencies == Currencies.PKR)
                        {
                            convertedClearedAmount += item.AmountRecieved;
                        }
                        else if (item.Currencies == Currencies.USD)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                        else if (item.Currencies == Currencies.AED)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                        else if (item.Currencies == Currencies.GBP)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                        else if (item.Currencies == Currencies.EURO)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                    }
                    else if (currentMember.FamilyHead.Currencies == Currencies.USD)
                    {
                        if (item.Currencies == Currencies.SAR)
                        {
                            convertedClearedAmount += item.AmountRecieved / item.ROE;
                        }
                        else if (item.Currencies == Currencies.PKR)
                        {
                            convertedClearedAmount += item.AmountRecieved / item.ROE;
                        }
                        else if (item.Currencies == Currencies.USD)
                        {
                            convertedClearedAmount += item.AmountRecieved;
                        }
                        else if (item.Currencies == Currencies.AED)
                        {
                            convertedClearedAmount += item.AmountRecieved / item.ROE;
                        }
                        else if (item.Currencies == Currencies.GBP)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                        else if (item.Currencies == Currencies.EURO)
                        {
                            convertedClearedAmount += item.AmountRecieved * item.ROE;
                        }
                    }
                }

                _.ReceivedAmountTotal = convertedClearedAmount.ToString();

                _.ReceivedAmountPercent = ((convertedClearedAmount / newTotal) * 100).ToString() + "%";

                _.FinalBalanceReceivable = (newTotal - convertedClearedAmount).ToString();

                model.FamilyMembers.Add(_);
                model.TotalBasicPackage += newBasic;
                model.TotalDiscount += newGeneralDiscount;
                model.TotalTicketAmount += newFlightTicket;
                model.TotalTrainTicketAmount += newTrainTicket;
                model.TotalReceivedAmount = convertedClearedAmount;
            }


            foreach (var item in model.FamilyMembers)
            {
                model.totalmale += Convert.ToInt32(string.IsNullOrEmpty(item.M) ? "0" : item.M);
                model.totalfemale += Convert.ToInt32(string.IsNullOrEmpty(item.F) ? "0" : item.M);
                model.totalchild += Convert.ToInt32(string.IsNullOrEmpty(item.C) ? "0" : item.C);
                model.totalinfant += Convert.ToInt32(string.IsNullOrEmpty(item.I) ? "0" : item.I);
                model.totalregular += Convert.ToInt32(string.IsNullOrEmpty(item.Category) ? "0" : item.Category.ToLower() == "Regular".ToLower() ? "1" : "0");
                model.totalsponser += Convert.ToInt32(string.IsNullOrEmpty(item.Category) ? "0" : item.Category.ToLower() == "Sponser".ToLower() ? "1" : "0");
                model.totalbiostatus += Convert.ToInt32(string.IsNullOrEmpty(item.BioStatus) ? "0" : item.BioStatus);
                model.totalagreementsign += Convert.ToInt32(string.IsNullOrEmpty(item.AgreementSign) ? "0" : item.AgreementSign);
                model.totaldataformsign += Convert.ToInt32(string.IsNullOrEmpty(item.DateFromSign) ? "0" : item.DateFromSign);
                model.TotalEHajj += item.EHajj;
            }


            return View(model);
        }

        // Helper Methods
        float CalculateBasicAmount(FamilyHead member, FamilyHead head)
        {
            return head.Currencies switch
            {
                Currencies.PKR => member.IsAdult ? member.Package.AdultPkr :
                                 member.IsChild ? member.Package.ChildPkr :
                                 member.Package.InfantPkr,
                Currencies.SAR => member.IsAdult ? member.Package.AdultSar :
                                 member.IsChild ? member.Package.ChildSar :
                                 member.Package.InfantSar,
                Currencies.USD => member.IsAdult ? member.Package.AdultUsd :
                                 member.IsChild ? member.Package.ChildUsd :
                                 member.Package.InfantUsd,
                _ => 0f
            };
        }

        float CalculateTicketAmount(float ticketTotal, Currencies currency, Package package)
        {
            return currency switch
            {
                Currencies.PKR => ticketTotal * package.SARROE,
                Currencies.SAR => ticketTotal,
                Currencies.USD => ticketTotal * package.USDROE,
                _ => 0f
            };
        }

        public IActionResult Report(int id)
        {

            List<Package> package = _dbContext.Packages.AsNoTracking().Where(e => e.CompanyId == id).ToList();

            PackageReportModel model = new PackageReportModel
            {
                PackageId = id,
                FamilyMembers = new List<FamilyMemberModel>(),

                //PackageName = package.Name,
                //PackageCode = package.Code,
                //PackageDays = package.Days,
                //PackageDuration = package.HajjDuration,
                //MedinahArrival = package.MedinahArrival,
                //Quota = package.Quota,
            };

            var members = _dbContext.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Where(e => e.FamilyHead.Package.CompanyId == id && !e.FamilyHead.Trashed)
                .AsNoTracking()
                .OrderBy(e => e.HeadId)
                .ToList();
            model.Bookings = members.Count;

            var familiesGroups = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .Where(e => e.FamilyHead.Package.CompanyId == id && !e.FamilyHead.Trashed)
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

                var RoomsTypeAssignedToMembers = from af in _dbContext.AdditionalFacilities
                                                 join fhad in _dbContext.FamilyHeadAdditionalFacilities
                                                 on af.AdditionalFacilitiesId equals fhad.AdditionalFacilitiesId
                                                 join ifd in _dbContext.IndividualFacilityDescriptions
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

                var memberInGroup = _dbContext.GroupMembers
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
                var memberContact = _dbContext.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _dbContext.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var makkahAcc = _dbContext.PackageCategoryFacilities
                                    .Include(e => e.Package)
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.Package.CompanyId == id && e.IndividualFacilityDescription.PlaceId == Place.MAKKAH_ID);

                var medinaAcc = _dbContext.PackageCategoryFacilities
                    .Include(e => e.Package)
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.Package.CompanyId == id && e.IndividualFacilityDescription.PlaceId == Place.MEDINA_ID);

                var minaAcc = _dbContext.PackageCategoryFacilities
                    .Include(e => e.Package)
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.Package.CompanyId == id && e.IndividualFacilityDescription.PlaceId == Place.MINA_ID);

                var aziziaAcc = _dbContext.PackageCategoryFacilities
                    .Include(e => e.Package)
                                    .Include(e => e.IndividualFacilityDescription)
                                    .ThenInclude(e => e.Room)
                                    .FirstOrDefault(e => e.Package.CompanyId == id && e.IndividualFacilityDescription.PlaceId == Place.AZIZIA_ID);

                float totalAmount = PackageManager.GetTotalPackageAmountInSarForEntireFamily(_dbContext, currentMember.FamilyHead.PackageId, currentMember.FamilyHeadId);

                var invoices = _dbContext.InvoicesWithoutTrash.Select(e => new { e.InvoiceId, e.FamilyHeadId }).Where(e => e.FamilyHeadId == currentMember.HeadId);
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

            return View(model);
        }
    }
}
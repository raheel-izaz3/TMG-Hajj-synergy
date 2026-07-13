using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Transactions;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models;
using TMG.Models.AccountModels;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.FormModels.FamilyModels;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;
using static QRCoder.PayloadGenerator;

namespace TMG.Controllers.FamilyControllers;
public partial class FamilyHeadFormController : Controller
{
    private readonly TMGDbContext _context;
    private readonly IWebHostEnvironment _host;
    private readonly IHttpContextAccessor _http;

    public FamilyHeadFormController(TMGDbContext context, IWebHostEnvironment host, IHttpContextAccessor http)
    {
        _context = context;
        _host = host;
        _http = http;
    }

    public async Task<JsonResult> SavePassport(FamilyHeadPassportModel request, int id)
    {
        #region Mock Data
        /*request = new FamilyHeadPassportModel
        {
            SPOId = 1,
            FeedBy = "System",
            HonorificsId = 1,
            SurName = "Doe",
            GivenName = "John",
            MehramRelationEnum = MehramRelationEnum.Neither,
            FatherHusbandName = "Robert Doe",
            CNIC = "12345-6789012-3",
            DateOfBirth = new DateTime(1980, 5, 20),
            PlaceOfBirth = "Karachi",
            Nationality = "Pakistani",
            Gender = Gender.Male,
            BloodGroup = "B+",
            FiqahId = 2,
            IsMarried = true,
            IsEmployed = true,
            EmployedAt = "ABC Corporation",
            Designation = "Software Engineer",
            Residency = "House 123, Street 45, City XYZ",
            PassportType = "Ordinary",
            CountryCode = "PK",
            PassportNumber = "AB1234567",
            PassportDateOfIssue = new DateTime(2020, 1, 1),
            PassportDateOfExpiry = new DateTime(2030, 1, 1),
            IssuingAuthority = "NADRA",
            TrackingNumber = "TRK123456",
            BookletNumber = "BL123456",
            BiometricVerified = true,
            ContactPerson = false,
            BiometricCode = "BIO123",
            TokenRemarks = "Remarks about the token",
            FileNumber = "FN123456"
        };*/
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.SPOId = request.SPOId;
            familyHead.FeedBy = request.FeedBy;
            familyHead.HonorificsId = request.HonorificsId;
            familyHead.SurName = request.SurName;
            familyHead.GivenName = request.GivenName;
            familyHead.MehramRelationEnum = request.MehramRelationEnum;
            familyHead.FatherHusbandName = request.FatherHusbandName;
            familyHead.CNIC = request.CNIC;
            familyHead.DateOfBirth = request.DateOfBirth;
            familyHead.PlaceOfBirth = request.PlaceOfBirth;
            familyHead.Nationality = request.Nationality;
            familyHead.Gender = request.Gender;
            familyHead.BloodGroup = request.BloodGroup;
            familyHead.FiqahId = request.FiqahId;
            familyHead.IsMarried = request.IsMarried;
            familyHead.Residency = request.Residency;
            familyHead.PassportType = request.PassportType;
            familyHead.PassportTypeId = request.PassportTypeId;
            familyHead.EmployedAt = request.EmployedAt;
            familyHead.DesignationId = request.Designation;
            familyHead.CountryId = request.CountryId;
            familyHead.NationalityId = request.NationalityId;
            familyHead.CountryCodeId = request.CountryCodeId;
            familyHead.CountryCode = request.CountryCode;
            familyHead.PassportNumber = request.PassportNumber;
            familyHead.PassportDateOfIssue = request.PassportDateOfIssue;
            familyHead.PassportDateOfExpiry = request.PassportDateOfExpiry;
            familyHead.IssuingAuthority = request.IssuingAuthority;
            familyHead.TrackingNumber = request.TrackingNumber;
            familyHead.BookletNumber = request.BookletNumber;
            familyHead.BiometricVerified = request.BiometricVerified;
            familyHead.ContactPerson = request.ContactPerson;
            familyHead.BiometricCode = request.BiometricCode;
            familyHead.TokenRemarks = request.TokenRemarks;
            familyHead.FileNumber = request.FileNumber;
            familyHead.ApplicantType = request.ApplicantType;
            //familyHead.FFNumber = request.FFNumber;

            if (familyHead.IsHead == true && familyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
            {
                var AccAsset = _context.Account.Where(a => a.AccountsId == familyHead.ReceivableAccountsId).FirstOrDefault();
                var AccRevenue = _context.Account.Where(a => a.AccountsId == familyHead.RevenueAccountsId).FirstOrDefault();

                if (AccAsset != null)
                {
                    AccAsset.Name = $"{request.GivenName} {request.SurName} ({familyHead.FcId}) - Receivable ({familyHead.Currencies.ToString})";
                    AccAsset.Currencies = familyHead.Currencies;
                    AccAsset.IsHajjiAccount = true;
                    AccAsset.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                    _context.Entry(AccAsset).State = EntityState.Modified;
                }
                
                if (AccRevenue != null)
                {
                    AccRevenue.Name = $"{request.GivenName} {request.SurName} ({familyHead.FcId}) - {COAManager.ServiceRevenueString} ({familyHead.Currencies.ToString})";
                    AccRevenue.Currencies = familyHead.Currencies;
                    AccRevenue.IsHajjiAccount = true;
                    AccRevenue.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                    _context.Entry(AccRevenue).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Passport details saved!",
                headId = id,
                familyHead
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving passport details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveMehram(FamilyHeadMehramModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadMehramModel
        //{
        //    MehramFamilyHeadId = 4,
        //    RelationWithMehramId = 1,
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);
            FamilyHead mehramFamilyHead = await _context.FamilyHeads
                                                    .FirstOrDefaultAsync(e => e.CNIC == request.MehramCNIC);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            if (familyHead.Gender != Gender.Female)
            {
                return Json(new
                {
                    status = 201,
                    msg = "Member not female"
                });
            }

            if (mehramFamilyHead is null)
            {
                return Json(new
                {
                    status = 202,
                    msg = "No member with provided CNIC."
                });
            }

            familyHead.MehramFamilyHeadId = mehramFamilyHead?.FamilyHeadId;
            familyHead.RelationWithMehramId = request.RelationWithMehramId;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Mehram details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving mehram8 details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveDiscount(FamilyHeadDiscountModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadDiscountModel
        //{
        //    SarDiscountAmount = 300,
        //    UsdDiscountAmount = 500,
        //    PkrDiscountAmount = 0
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            if (request.SarDiscountAmount < 0 || request.PkrDiscountAmount < 0 || request.UsdDiscountAmount < 0)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Discount cannot be less than 0",
                    request = request
                });
            }

            familyHead.GeneralDiscount = request.GeneralDiscount;

            #region Ledger Entry

            if (familyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
            {

                int RecevableId = Convert.ToInt32(familyHead.ReceivableAccountsId);
                int RevenueId = Convert.ToInt32(familyHead.RevenueAccountsId);

                float Discount = request.GeneralDiscount ?? 0.00f;

                GeneralLedger generalLedger = new()
                {
                    Date = DateTime.Now,
                    Trashed = false,
                    CreatedAt = DateTime.Now,
                };
                _context.GeneralLedgers.Add(generalLedger);
                _context.SaveChanges();

                GeneralLedgerEntry debitEntry = new GeneralLedgerEntry()
                {
                    Description = $"{familyHead.GivenName} {familyHead.SurName} ({familyHead.FcId}) ({familyHead.Currencies.ToString()}) - Discount",
                    AccountNature = AccountNature.Credit,
                    GeneralLedgerId = generalLedger.GeneralLedgerId,
                    Amount =  request.GeneralDiscount ?? 0.00f,
                    AccountsId = RecevableId
                };

                GeneralLedgerEntry creditEntry = new GeneralLedgerEntry()
                {
                    Description = $"{familyHead.GivenName} {familyHead.SurName} ({familyHead.FcId}) ({familyHead.Currencies.ToString()}) - Discount",
                    AccountNature = AccountNature.Debit,
                    GeneralLedgerId = generalLedger.GeneralLedgerId,
                    Amount = request.GeneralDiscount ?? 0.00f,
                    AccountsId = RevenueId
                };

                _context.GeneralLedgerEntries.Add(debitEntry);
                _context.GeneralLedgerEntries.Add(creditEntry);
                await _context.SaveChangesAsync();

            }

            #endregion

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Discount details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving discount details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveVaccine(FamilyHeadVaccineModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadVaccineModel
        //{
        //    VaccineId1 = 1,                           // Mock VaccineId1
        //    DoseDate1 = new DateTime(2022, 1, 10),      // Mock DoseDate1

        //    VaccineId2 = 1,                           // Mock VaccineId2
        //    DoseDate2 = new DateTime(2022, 2, 15),      // Mock DoseDate2

        //    VaccineId3 = 1,                           // Mock VaccineId3
        //    DoseDate3 = new DateTime(2022, 3, 20),      // Mock DoseDate3

        //    VaccineId4 = 1,                           // Mock VaccineId4
        //    DoseDate4 = new DateTime(2022, 4, 25),      // Mock DoseDate4

        //    VaccineId5 = 1,                           // Mock VaccineId5
        //    DoseDate5 = new DateTime(2022, 5, 30)       // Mock DoseDate5
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.VaccineId1 = request.VaccineId1;
            familyHead.VaccineId2 = request.VaccineId2;
            familyHead.VaccineId3 = request.VaccineId3;
            familyHead.VaccineId4 = request.VaccineId4;
            familyHead.VaccineId5 = request.VaccineId5;

            familyHead.DoseDate1 = request.DoseDate1;
            familyHead.DoseDate2 = request.DoseDate2;
            familyHead.DoseDate3 = request.DoseDate3;
            familyHead.DoseDate4 = request.DoseDate4;
            familyHead.DoseDate5 = request.DoseDate5;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Vaccine details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving vaccine details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SavePackageDetails(FamilyHeadPackageModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadPackageModel
        //{
        //    PackageId = 3,                               // Mock PackageId
        //    POD = "JFK",                                   // Mock POD (Point of Departure)
        //    FlightTicketType = FlightTicketType.Included,   // Mock FlightTicketType (assuming FlightTicketType is an enum)
        //    FlightClass = "Business",                      // Mock FlightClass
        //    FlightTicketDifferencesPkr = 5000,            // Mock FlightTicketDifferencePkr
        //    FlightTicketDifferencesSar = 1200,            // Mock FlightTicketDifferenceSar
        //    FlightTicketDifferencesUsd = null
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.Include(f => f.Accounts).Where(f => f.FamilyHeadId == id).FirstOrDefaultAsync();

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.PackageId = request.PackageId;
            familyHead.POD = request.POD;
            familyHead.FlightClass = request.FlightClass;
            familyHead.FlightTicketType = request.FlightTicketType;
            familyHead.FlightTicketDifferencesPkr = request.FlightTicketDifferencesPkr;
            familyHead.FlightTicketDifferencesSar = request.FlightTicketDifferencesSar;
            familyHead.FlightTicketDifferencesUsd = request.FlightTicketDifferencesUsd;
            familyHead.Currencies = request.Currencies;
            familyHead.TrainNumber = request.TrainNumberForPackage;
            familyHead.SeatNo = request.SeatNo;
            familyHead.CoachNo = request.CoachNo;
            //familyHead.BaseAmount = request.BasePackageAmount;

            #region Accounts

            if (familyHead.IsHead == true && familyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
            {
                var AccReceivable = await _context.Account.Where(a => !a.Trashed && a.AccountsId == familyHead.ReceivableAccountsId).FirstOrDefaultAsync();

                var Pckg = await _context.Packages.Where(p => p.PackageId == request.PackageId).FirstOrDefaultAsync();

                if (AccReceivable != null)
                {
                    AccReceivable.Name = $"{familyHead.GivenName} {familyHead.SurName} ({familyHead.FcId}) - Receivable ({familyHead.Currencies.ToString()})";

                    if (Pckg != null)
                    {
                        AccReceivable.CompanyId = Pckg.CompanyId;
                    }

                    _context.Entry(AccReceivable).State = EntityState.Modified;
                }

                var AccRevenue = await _context.Account.Where(a => !a.Trashed && a.AccountsId == familyHead.RevenueAccountsId).FirstOrDefaultAsync();

                if (AccRevenue != null)
                {
                    AccRevenue.Name = $"{familyHead.GivenName} {familyHead.SurName} ({familyHead.FcId}) - {COAManager.ServiceRevenueString} ({familyHead.Currencies.ToString()})";

                    if (Pckg != null)
                    {
                        AccRevenue.CompanyId = Pckg.CompanyId;
                    }

                    _context.Entry(AccRevenue).State = EntityState.Modified;
                }
            }

            #endregion

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Package details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving package details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveHajjAppForm(FamilyHeadHajjAppFormModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadHajjAppFormModel
        //{
        //    FormNumber = "FH123456789",
        //    HajjAppSerialNumber = "HASN987654321",
        //    HajjApplicationNumber = "HAN001234567",
        //    GroupCoordAppNumber = "GC001234",
        //    SubstitutedAgainstAppNumber = "SA001234",
        //    ENumber = "E123456",
        //    EHajjId = "EHJ123456",
        //    EHajjSerial = "EHJ789012",
        //    MofaNumber = "MOFA123456",
        //    TicketNumber = "TICKET789012",
        //    VisaNumber = "VISA123456",
        //    PerformedHajjInLastFiveYears = true,
        //    WantToPerformHajjEBadal = false,
        //    WantToPerformAsGroupWorker = true,
        //    Eligible = true,
        //    DocumentRemarks = new List<string>() { "", "", "" },
        //    RequiredDocumentIds = new List<int>() { 1, 2, 3 },
        //    Recieved = new List<bool>() { true, false, true }
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.HajjApplicationNumber = request.HajjApplicationNumber;
            familyHead.HajjAppSerialNumber = request.HajjAppSerialNumber;
            familyHead.EHajjId = request.EHajjId;
            familyHead.EHajjSerial = request.EHajjSerial;
            familyHead.FormNumber = request.FormNumber;
            familyHead.GroupCoordAppNumber = request.GroupCoordAppNumber;
            familyHead.SubstitutedAgainstAppNumber = request.SubstitutedAgainstAppNumber;
            familyHead.ENumber = request.ENumber;
            familyHead.MofaNumber = request.MofaNumber;
            familyHead.TicketNumber = request.TicketNumber;
            familyHead.VisaNumber = request.VisaNumber;
            familyHead.PerformedHajjInLastFiveYears = request.PerformedHajjInLastFiveYears;
            familyHead.WantToPerformHajjEBadal = request.WantToPerformHajjEBadal;
            familyHead.WantToPerformAsGroupWorker = request.WantToPerformAsGroupWorker;
            familyHead.Eligible = request.Eligible;

            await _context.FamilyHeadDocuments.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.RequiredDocumentIds?.Count ?? 0); i++)
            {
                var familyHeadDocument = new FamilyHeadDocument
                {
                    FamilyHeadId = familyHead.FamilyHeadId,
                    RequiredDocumentId = request.RequiredDocumentIds[i],
                    Recieved = request.Recieved[i],
                    Remarks = request.DocumentRemarks[i]
                };

                //if (request.NomineeCnic is not null)
                //{
                //    string path = Path.Combine(_host.WebRootPath, "Images", "NomineeCnicImages", request.NomineeCnic.FileName);
                //    await using (FileStream fs = new FileStream(path, FileMode.Create))
                //    {
                //        await request.NomineeCnic.CopyToAsync(fs);
                //    }

                //    familyHeadDocument.NomineeCnic = @"Images\NomineeCnicImages\" + request.NomineeCnic.FileName;
                //    familyHeadDocument.Recieved = true;
                //}
                //else
                //{
                //    familyHeadDocument.Recieved = false;
                //}

                await _context.FamilyHeadDocuments.AddAsync(familyHeadDocument);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Hajj App Form details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving form details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveContactInfo(FamilyHeadContactModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadContactModel
        //{
        //    MemberAddressTypeIds = new List<int?> { 1 },
        //    MemberAddressCities = new List<string> { "" },
        //    MemberAddressCountries = new List<string> { "" },
        //    MemberAddresses = new List<string> { "" },
        //    MemberAddressPoBoxes = new List<string> { "" },
        //    MemberAddressPreferred = new List<bool> { true },
        //    MemberAddressTehsilCodes = new List<int?> { null },
        //    MemberAddressZipCodes = new List<string> { null },

        //    MemberContactTypeIds = new List<int?> { 1, 2 },
        //    MemberContactPreferred = new List<bool> { true, false },
        //    MemberContacts = new List<string> { "0330", "021" },


        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            await _context.FamilyHeadAddresses.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberAddressTypeIds?.Count ?? 0); i++)
            {
                await _context.FamilyHeadAddresses.AddAsync(
                    new FamilyHeadAddress
                    {
                        FamilyHeadId = familyHead.FamilyHeadId,
                        MemberAddressTypeId = request.MemberAddressTypeIds[i],
                        Address = request.MemberAddresses[i],
                        //Country = request.MemberAddressCountries[i],
                        //City = request.MemberAddressCities[i],
                        ZipCode = request.MemberAddressZipCodes[i],
                        PoBox = request.MemberAddressPoBoxes[i],
                        Preferred = request.MemberAddressPreferred[i],
                        TehsilId = request.MemberAddressTehsilCodes[i],
                        CountryId = request.CountryId[i],
                        CityId = request.CityId[i],
                    }
                    );
            }

            await _context.FamilyHeadContacts.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberContactTypeIds?.Count ?? 0); i++)
            {
                await _context.FamilyHeadContacts.AddAsync(
                    new FamilyHeadContact
                    {
                        FamilyHeadId = familyHead.FamilyHeadId,
                        MemberContactTypeId = request.MemberContactTypeIds[i],
                        Contact = request.MemberContacts[i],
                        Preferred = request.MemberContactPreferred[i]
                    }
                    );
            }

            await _context.FamilyHeadEmails.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberEmailTypeIds?.Count ?? 0); i++)
            {
                await _context.FamilyHeadEmails.AddAsync(
                    new FamilyHeadEmail
                    {
                        FamilyHeadId = familyHead.FamilyHeadId,
                        MemberEmailTypeId = request.MemberEmailTypeIds[i],
                        Email = request.MemberEmails[i],
                        Preferred = request.MemberEmailPreferred[i]
                    }
                    );
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Contact details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving contact details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveReference(FamilyHeadReferenceModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadReferenceModel
        //{
        //    FamilyHeadRelationId = 3,
        //    ReferenceId = null
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.ReferenceId = request.ReferenceId;
            familyHead.FamilyHeadRelationId = request.FamilyHeadRelationId;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Reference details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving reference details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveRemarks(FamilyHeadRemarksModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadRemarksModel
        //{
        //    Remarks = new List<string>() { 
        //        "ABC", "XYZ"
        //    }
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.VisaRemarks = request.VisaRemarks;
            await _context.SaveChangesAsync();

            await _context.FamilyHeadRemarks.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.Remarks?.Count ?? 0); i++)
            {
                await _context.FamilyHeadRemarks.AddAsync(
                    new FamilyHeadRemark
                    {
                        FamilyHeadId = familyHead.FamilyHeadId,
                        Remarks = request.Remarks[i],
                    });
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Remarks details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving remarks details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveNominees(FamilyHeadNomineeModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadContactModel
        //{
        //    MemberAddressTypeIds = new List<int?> { 1 },
        //    MemberAddressCities = new List<string> { "" },
        //    MemberAddressCountries = new List<string> { "" },
        //    MemberAddresses = new List<string> { "" },
        //    MemberAddressPoBoxes = new List<string> { "" },
        //    MemberAddressPreferred = new List<bool> { true },
        //    MemberAddressTehsilCodes = new List<int?> { null },
        //    MemberAddressZipCodes = new List<string> { null },

        //    MemberContactTypeIds = new List<int?> { 1, 2 },
        //    MemberContactPreferred = new List<bool> { true, false },
        //    MemberContacts = new List<string> { "0330", "021" },


        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            await _context.FamilyHeadNominees.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.NomineeNames?.Count ?? 0); i++)
            {
                var nominee = new FamilyHeadNominee
                {
                    FamilyHeadId = familyHead.FamilyHeadId,
                    Name = request.NomineeNames[i],
                    NomineeRelationId = request.NomineeRelations[i],
                    CNIC = request.NomineeCNICs[i],
                    Address = request.NomineeAddresses[i],
                    CountryId = request.NomineeCountriesId[i],
                    CityId = request.NomineeCitiesId[i],
                    ZipCode = request.NomineeZipCodes[i],
                    PoBox = request.NomineePoBoxes[i],
                    Contact = request.NomineeContacts[i],
                    Email = request.NomineeEmails[i],
                    Preferred = request.NomineePreferred[i]
                };

                //if (request.NomineeCnic != null && request.NomineeCnic.Count > i && request.NomineeCnic[i] != null)
                //{
                //    string fileName = request.NomineeCnic[i].FileName;
                //    string path = Path.Combine(_host.WebRootPath, "Images", "NomineeCnic", fileName);

                //    using (FileStream fs = new FileStream(path, FileMode.Create))
                //    {
                //        await request.NomineeCnic[i].CopyToAsync(fs);
                //    }
                //    nominee.NomineeCnicImage = ApplicationManager.NomineeCnicPath + fileName;
                //}

                if (request.NomineeCnic != null )
                {
                    string fileName = request.NomineeCnic.FileName;
                    string path = Path.Combine(_host.WebRootPath, "Images", "NomineeCnic", fileName);

                    if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        await request.NomineeCnic.CopyToAsync(fs);
                    }
                    nominee.NomineeCnicImage = ApplicationManager.NomineeCnicPath + fileName;
                }

                await _context.FamilyHeadNominees.AddAsync(nominee);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Nominee details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving nominee details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    #region Default
    //public async Task<JsonResult> SaveExtraAdfs(FamilyHeadExtraAdfModel request, int id)
    //{
    //    await using var transaction = await _context.Database.BeginTransactionAsync();

    //    try
    //    {
    //        FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

    //        if (familyHead == null)
    //        {
    //            return Json(new
    //            {
    //                status = 404,
    //                msg = "Family head not found."
    //            });
    //        }

    //        // Remove old records
    //        await _context.FamilyHeadExtras.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

    //        for (int i = 0; i < (request.ExtraFacilityIds?.Count ?? 0); i++)
    //        {
    //            if (request.ExtraFacilitiesSelected?[i] == false || request.Price?[i] <= 0) continue;

    //            // Add new extra facilities
    //            await _context.FamilyHeadExtras.AddAsync(new FamilyHeadExtra
    //            {
    //                FamilyHeadId = familyHead.FamilyHeadId,
    //                ExtraFacilityDescriptionId = request.ExtraFacilityIds[i],
    //                Price = request.Price[i],
    //                Discount = request.Discount?[i] ?? 0
    //            });

    //            #region Ledger

    //            #endregion

    //        }

    //        await _context.SaveChangesAsync();
    //        await transaction.CommitAsync();

    //        return Json(new
    //        {
    //            status = 200,
    //            msg = "Extra facility details saved!"
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        await transaction.RollbackAsync();

    //        return Json(new
    //        {
    //            status = 500,
    //            msg = "An error occurred while saving extra facility details.",
    //            details = ex.Message,
    //            innerException = ex.InnerException?.Message
    //        });
    //    }
    //} 
    #endregion

    public async Task<JsonResult> SaveExtraAdfs(FamilyHeadExtraAdfModel request, int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.Include(f => f.Accounts).Where(f => f.FamilyHeadId == id).FirstOrDefaultAsync();

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            await _context.FamilyHeadExtras.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.ExtraFacilityIds?.Count ?? 0); i++)
            {
                if (request.ExtraFacilitiesSelected?[i] == false || request.Price?[i] <= 0) continue;

                await _context.FamilyHeadExtras.AddAsync(new FamilyHeadExtra
                {
                    FamilyHeadId = familyHead.FamilyHeadId,
                    ExtraFacilityDescriptionId = request.ExtraFacilityIds[i],
                    Price = request.Price[i],
                    Discount = request.Discount?[i] ?? 0
                });

                #region Ledger

                if (request.ExtraFacilitiesSelected?[i] != false)
                {
                    int AddOnsId = Convert.ToInt32(request.ExtraFacilityIds[i]);

                    var AddOns = await _context.ExtraFacilityDescriptions.Where(e => e.ExtraFacilityDescriptionId == AddOnsId).FirstOrDefaultAsync();

                    if (familyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
                    {
                        int receivableId = Convert.ToInt32(familyHead.ReceivableAccountsId);
                        int revenueId = Convert.ToInt32(familyHead.RevenueAccountsId);

                        float price = request.Price[i] ?? 0;

                        GeneralLedger generalLedgerPrice = new()
                        {
                            Date = DateTime.Now,
                            Trashed = false,
                            CreatedAt = DateTime.Now,
                        };
                        _context.GeneralLedgers.Add(generalLedgerPrice);
                        await _context.SaveChangesAsync();

                        _context.GeneralLedgerEntries.Add(new GeneralLedgerEntry
                        {
                            Description = $"{familyHead.GivenName} {familyHead.SurName} Price for Add-on Facility: ({AddOns.Description})",
                            AccountNature = AccountNature.Debit,
                            GeneralLedgerId = generalLedgerPrice.GeneralLedgerId,
                            Amount = price,
                            AccountsId = receivableId
                        });

                        _context.GeneralLedgerEntries.Add(new GeneralLedgerEntry
                        {
                            Description = $"{familyHead.GivenName} {familyHead.SurName} Price for Add-on Facility ({AddOns.Description})",
                            AccountNature = AccountNature.Credit,
                            GeneralLedgerId = generalLedgerPrice.GeneralLedgerId,
                            Amount = price,
                            AccountsId = revenueId
                        });

                        // Discount Ledger Entries

                        float discount = request.Discount?[i] ?? 0;

                        if (discount > 0)
                        {
                            GeneralLedger generalLedgerDiscount = new()
                            {
                                Date = DateTime.Now,
                                Trashed = false,
                                CreatedAt = DateTime.Now,
                            };
                            _context.GeneralLedgers.Add(generalLedgerDiscount);
                            await _context.SaveChangesAsync();

                            _context.GeneralLedgerEntries.Add(new GeneralLedgerEntry
                            {
                                Description = $"{familyHead.GivenName} {familyHead.SurName} Discount for Add-on Facility ({AddOns.Description})",
                                AccountNature = AccountNature.Credit,
                                GeneralLedgerId = generalLedgerDiscount.GeneralLedgerId,
                                Amount = discount,
                                AccountsId = receivableId
                            });

                            _context.GeneralLedgerEntries.Add(new GeneralLedgerEntry
                            {
                                Description = $"{familyHead.GivenName} {familyHead.SurName} Discount for Add-on Facility ({AddOns.Description})",
                                AccountNature = AccountNature.Debit,
                                GeneralLedgerId = generalLedgerDiscount.GeneralLedgerId,
                                Amount = discount,
                                AccountsId = revenueId
                            });
                        }
                    }
                }

                #endregion

            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Extra facility details saved!"
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving extra facility details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveTransport(FamilyHeadTransportModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadContactModel
        //{
        //    MemberAddressTypeIds = new List<int?> { 1 },
        //    MemberAddressCities = new List<string> { "" },
        //    MemberAddressCountries = new List<string> { "" },
        //    MemberAddresses = new List<string> { "" },
        //    MemberAddressPoBoxes = new List<string> { "" },
        //    MemberAddressPreferred = new List<bool> { true },
        //    MemberAddressTehsilCodes = new List<int?> { null },
        //    MemberAddressZipCodes = new List<string> { null },

        //    MemberContactTypeIds = new List<int?> { 1, 2 },
        //    MemberContactPreferred = new List<bool> { true, false },
        //    MemberContacts = new List<string> { "0330", "021" },


        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }


            await _context.FamilyHeadTransports.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();

            familyHead.RouteAsPerPackage = request.RouteAsPerPackage;
            await _context.SaveChangesAsync();

            for (int i = 0; i < (request.RouteIds?.Count ?? 0); i++)
            {
                await _context.FamilyHeadTransports.AddAsync(
                    new FamilyHeadTransport
                    {
                        FamilyHeadId = familyHead.FamilyHeadId,
                        RouteId = request.RouteIds[i],
                        CheckIn = request.CheckIns[i],
                        //CheckOut = request.CheckOuts[i],
                        TransportType = request.TransportTypes[i],
                        VehicleId = request.VehicleIds[i],
                        Price = request.Prices[i],
                    });
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Transport details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving transport details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveDocuments(FamilyHeadDocumentsModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadContactModel
        //{
        //    MemberAddressTypeIds = new List<int?> { 1 },
        //    MemberAddressCities = new List<string> { "" },
        //    MemberAddressCountries = new List<string> { "" },
        //    MemberAddresses = new List<string> { "" },
        //    MemberAddressPoBoxes = new List<string> { "" },
        //    MemberAddressPreferred = new List<bool> { true },
        //    MemberAddressTehsilCodes = new List<int?> { null },
        //    MemberAddressZipCodes = new List<string> { null },

        //    MemberContactTypeIds = new List<int?> { 1, 2 },
        //    MemberContactPreferred = new List<bool> { true, false },
        //    MemberContacts = new List<string> { "0330", "021" },


        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            if (request.MemberImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "MemberImages", request.MemberImage.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.MemberImage.CopyToAsync(fs);
                }
                familyHead.MemberImage = @"Images\MemberImages\" + request.MemberImage.FileName;
            }

            if (request.PassportImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "PassportImages", request.PassportImage.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.PassportImage.CopyToAsync(fs);
                }
                familyHead.PassportImage = @"Images\PassportImages\" + request.PassportImage.FileName;
            }

            if (request.VisaImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "VisaImages", request.VisaImage.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.VisaImage.CopyToAsync(fs);
                }
                familyHead.VisaImage = @"Images\VisaImages\" + request.VisaImage.FileName;
            }

            if (request.MemberVaccinationImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "MemberVaccImages", request.MemberVaccinationImage.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.MemberVaccinationImage.CopyToAsync(fs);
                }
                familyHead.MemberVaccinationImage = @"Images\MemberVaccImages\" + request.MemberVaccinationImage.FileName;
            }

            if (request.InsuranceImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "InsuranceImages", request.InsuranceImage.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.InsuranceImage.CopyToAsync(fs);
                }
                familyHead.InsuranceImage = @"Images\InsuranceImages\" + request.InsuranceImage.FileName;
            }

            if (request.MISPdfPath is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Pdfs", "MIS", request.MISPdfPath.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.MISPdfPath.CopyToAsync(fs);
                }
                familyHead.MISPdfPath = @"Pdfs\MIS\" + request.MISPdfPath.FileName;
            }

            if (request.BloodProof is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "BloodProof", request.BloodProof.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.BloodProof.CopyToAsync(fs);
                }
                familyHead.BloodProof = @"Images\BloodProof\" + request.BloodProof.FileName;
            }

            if (request.CnicUpload is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.CnicUpload.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.CnicUpload.CopyToAsync(fs);
                }
                familyHead.CnicUpload = @"Images\CnicUpload\" + request.CnicUpload.FileName;
            }

            if (request.PolioUpload is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.PolioUpload.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.PolioUpload.CopyToAsync(fs);
                }
                familyHead.PolioUpload = @"Images\CnicUpload\" + request.PolioUpload.FileName;
            }

            if (request.CovidUpload is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.CovidUpload.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.CovidUpload.CopyToAsync(fs);
                }
                familyHead.CovidUpload = @"Images\CnicUpload\" + request.CovidUpload.FileName;
            }

            if (request.MangatitiseUpload is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.MangatitiseUpload.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.MangatitiseUpload.CopyToAsync(fs);
                }
                familyHead.MangatitiseUpload = @"Images\CnicUpload\" + request.MangatitiseUpload.FileName;
            }

            if (request.InfluenzaUpload is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "CnicUpload", request.InfluenzaUpload.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.InfluenzaUpload.CopyToAsync(fs);
                }
                familyHead.InfluenzaUpload = @"Images\CnicUpload\" + request.InfluenzaUpload.FileName;
            }

            if (request.MemberTrainImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "TrainImages", request.MemberTrainImage.FileName);
                if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.MemberTrainImage.CopyToAsync(fs);
                }
                familyHead.MemberTrainImage = @"Images\TrainImages\" + request.MemberTrainImage.FileName;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Transport details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving transport details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveFamilyMembers(FamilyHeadMembersModel request, int id)
    {
        #region Mock Data
        //request = new FamilyHeadMembersModel
        //{
        //    FamilyHeadIds = new List<int> { 101, 102, 103 },
        //    HonorificIds = new List<int> { 1, 2, 2 },
        //    GivenNames = new List<string> { "John", "Jane", "Alice" },
        //    SurNames = new List<string> { "Doe", "Smith", "Brown" },
        //    Genders = new List<Gender> { Gender.Male, Gender.Female, Gender.Female },
        //    DateOfBirths = new List<DateTime>
        //    {
        //        new DateTime(1990, 5, 20),
        //        new DateTime(1985, 10, 15),
        //        new DateTime(1992, 3, 25)
        //    },
        //            FiqahIds = new List<int> { 1, 1, 1 },
        //            PassportNumbers = new List<string> { "A1234567", "B2345678", "C3456789" },
        //            PassportsDateOfIssue = new List<DateTime>
        //    {
        //        new DateTime(2015, 5, 1),
        //        new DateTime(2016, 6, 2),
        //        new DateTime(2017, 7, 3)
        //    },
        //            PassportsDateOfExpiry = new List<DateTime>
        //    {
        //        new DateTime(2025, 5, 1),
        //        new DateTime(2026, 6, 2),
        //        new DateTime(2027, 7, 3)
        //    }
        //};
        #endregion

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                FamilyHead member = await _context.FamilyHeads.FindAsync(request.FamilyHeadIds[i]);

                member.HonorificsId = request.HonorificIds[i];
                member.GivenName = request.GivenNames[i];
                member.SurName = request.SurNames[i];
                member.Gender = request.Genders[i];
                member.DateOfBirth = request.DateOfBirths[i];
                member.FiqahId = request.FiqahIds[i];
                member.PassportNumber = request.PassportNumbers[i];
                member.PassportDateOfIssue = request.PassportsDateOfIssue[i];
                member.PassportDateOfExpiry = request.PassportsDateOfExpiry[i];

                await _context.SaveChangesAsync();
            }


            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Members detail saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving members detail.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveFlightDetails(FamilyHeadFlightModel request, int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.FlightAsPerPackage = request.FlightAsPerPackage;
            await _context.SaveChangesAsync();

            if (request.AirlineIds is null)
            {
                await _context.FamilyHeadFlight
                    .Where(e => e.FamilyHeadId == id && e.TicketStatus == TicketStatus.Active)
                    .ExecuteUpdateAsync(e => e.SetProperty(f => f.TicketStatus, TicketStatus.InActive));
            }

            for (int i = 0; i < request.AirlineIds?.Count; i++)
            {
                if (request.FamilyHeadFlightIds[i] == null)
                {
                    FamilyHeadFlight newFlight = new FamilyHeadFlight
                    {
                        FamilyHeadId = id,
                        AirlineId = request.AirlineIds[i],
                        DepartureDateTime = request.DepartureDateTimes[i],
                        ArrivalDateTime = request.ArrivalDateTimes[i],
                        //DepartureTime = request.DepartureTimes[i],
                        //ArrivalTime = request.ArrivalTimes[i],
                        //Commission = request.Commissions[i],
                        //CurrencyId = request.CurrencyIds[i],
                        Origin = request.Origins[i],
                        Destination = request.Destinations[i],
                        FareAmount = request.FareAmounts[i],
                        FlightNo = request.FlightNos[i],
                        FlightType = request.FlightTypes[i],
                        PNRNo = request.PNRNos[i],
                        //Taxes = request.Taxes[i],
                        TicketNo = request.TicketNos[i],
                        TicketStatus = TicketStatus.Active,
                        //Total = request.FareAmounts[i] ?? 0,
                        FlightClass = request.FlightClass[i],
                        //FFNumber = request.FFNumber[i],
                        //IssuingAgentId = request.IssuingAgentIds[i],
                    };

                    await _context.FamilyHeadFlight.AddAsync(newFlight);
                }
                else
                {
                    int? flightId = request.FamilyHeadFlightIds[i];
                    var existingFlight = await _context.FamilyHeadFlight.FindAsync(flightId);

                    if (request.Modified[i])
                    {

                        existingFlight.TicketStatus = TicketStatus.InActive;

                        await _context.FamilyHeadFlight.AddAsync(new FamilyHeadFlight
                        {
                            FamilyHeadId = id,
                            AirlineId = request.AirlineIds[i],
                            DepartureDateTime = request.DepartureDateTimes[i],
                            ArrivalDateTime = request.ArrivalDateTimes[i],
                            DepartureTime = request.DepartureTimes[i],
                            ArrivalTime = request.ArrivalTimes[i],
                            //Commission = request.Commissions[i],
                            //CurrencyId = request.CurrencyIds[i],
                            Origin = request.Origins[i],
                            Destination = request.Destinations[i],
                            FareAmount = request.FareAmounts[i],
                            FlightNo = request.FlightNos[i],
                            FlightType = request.FlightTypes[i],
                            PNRNo = request.PNRNos[i],
                            //Taxes = request.Taxes[i],
                            TicketNo = request.TicketNos[i],
                            TicketStatus = TicketStatus.Active,
                            Total = request.FareAmounts[i] ?? 0,
                            FlightClass = request.FlightClass[i],
                            FFNumber = request.FFNumber[i],
                            //IssuingAgentId = request.IssuingAgentIds[i],
                        });

                    }
                    else
                    {
                        existingFlight.TicketStatus = TicketStatus.Active;
                        existingFlight.AirlineId = request.AirlineIds[i];
                        existingFlight.DepartureDateTime = request.DepartureDateTimes[i];
                        existingFlight.ArrivalDateTime = request.ArrivalDateTimes[i];
                        existingFlight.DepartureTime = request.DepartureTimes[i];
                        existingFlight.ArrivalTime = request.ArrivalTimes[i];
                        //existingFlight.Commission = request.Commissions[i];
                        existingFlight.CurrencyId = request.CurrencyIds[i];
                        existingFlight.Origin = request.Origins[i];
                        existingFlight.Destination = request.Destinations[i];
                        existingFlight.FareAmount = request.FareAmounts[i];
                        existingFlight.FlightType = request.FlightTypes[i];
                        existingFlight.FlightNo = request.FlightNos[i];
                        existingFlight.PNRNo = request.PNRNos[i];
                        //existingFlight.Taxes = request.Taxes[i];
                        existingFlight.TicketNo = request.TicketNos[i];
                        existingFlight.FFNumber = request.FFNumber[i];
                        existingFlight.Total = (request.FareAmounts[i] ?? 0) + (request.Commissions[i] ?? 0) + (request.Taxes[i] ?? 0);
                        existingFlight.FlightClass = request.FlightClass[i];
                        //existingFlight.IssuingAgentId = request.IssuingAgentIds[i];
                    }
                }
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Flight details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving flight details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> DeleteFlightDetails(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            await _context.FamilyHeadFlight
                .Where(e => e.FamilyHeadFlightId == id)
                .ExecuteUpdateAsync(e => e.SetProperty(f => f.TicketStatus, TicketStatus.InActive));

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Flight details moved to history!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving flight details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveTrainDetails(FamilyHeadTrainModel request, int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.TrainAsPerPackage = request.TrainAsPerPackage;
            await _context.SaveChangesAsync();

            if (request.RailwayIds is null)
            {
                await _context.FamilyHeadTrain
                    .Where(e => e.FamilyHeadId == id && e.TicketStatus == TicketStatus.Active)
                    .ExecuteUpdateAsync(e => e.SetProperty(f => f.TicketStatus, TicketStatus.InActive));
            }

            for (int i = 0; i < request.RailwayIds?.Count; i++)
            {
                // Add new train
                if (request.FamilyHeadTrainIds[i] == null)
                {
                    FamilyHeadTrain newTrain = new FamilyHeadTrain
                    {
                        FamilyHeadId = id,
                        RailwayId = request.RailwayIds[i],
                        DepartureDateTime = request.DepartureDateTimes[i],
                        ArrivalDateTime = request.ArrivalDateTimes[i],
                        DepartureTime = request.DepartureTimes[i],
                        ArrivalTime = request.ArrivalTimes[i],
                        //Commission = request.Commissions[i],
                        //CurrencyId = request.CurrencyIds[i],
                        Origin = request.Origins[i],
                        Destination = request.Destinations[i],
                        FareAmount = request.FareAmounts[i],
                        TrainNo = request.TrainNos[i],
                        PNRNo = request.PNRNos[i],
                        //Taxes = request.Taxes[i],
                        TicketNo = request.TicketNos[i],
                        TicketStatus = TicketStatus.Active,
                        Total = request.FareAmounts[i] ?? 0,
                        TrainClass = request.TrainClass[i],
                        //IssuingAgentId = request.IssuingAgentIds[i],
                    };

                    await _context.FamilyHeadTrain.AddAsync(newTrain);
                }
                else
                {
                    int? trainId = request.FamilyHeadTrainIds[i];
                    var existingTrain = await _context.FamilyHeadTrain.FindAsync(trainId);

                    if (request.Modified[i])
                    {

                        existingTrain.TicketStatus = TicketStatus.InActive;

                        await _context.FamilyHeadTrain.AddAsync(new FamilyHeadTrain
                        {
                            FamilyHeadId = id,
                            RailwayId = request.RailwayIds[i],
                            DepartureDateTime = request.DepartureDateTimes[i],
                            ArrivalDateTime = request.ArrivalDateTimes[i],
                            DepartureTime = request.DepartureTimes[i],
                            ArrivalTime = request.ArrivalTimes[i],
                            Commission = request.Commissions[i],
                            CurrencyId = request.CurrencyIds[i],
                            Origin = request.Origins[i],
                            Destination = request.Destinations[i],
                            FareAmount = request.FareAmounts[i],
                            TrainNo = request.TrainNos[i],
                            PNRNo = request.PNRNos[i],
                            Taxes = request.Taxes[i],
                            TicketNo = request.TicketNos[i],
                            TicketStatus = TicketStatus.Active,
                            Total = (request.FareAmounts[i] ?? 0) + (request.Commissions[i] ?? 0) + (request.Taxes[i] ?? 0),
                            TrainClass = request.TrainClass[i],
                            IssuingAgentId = request.IssuingAgentIds[i],
                        });
                    }
                    else
                    {
                        existingTrain.TicketStatus = TicketStatus.Active;
                        existingTrain.RailwayId = request.RailwayIds[i];
                        existingTrain.DepartureDateTime = request.DepartureDateTimes[i];
                        existingTrain.ArrivalDateTime = request.ArrivalDateTimes[i];
                        existingTrain.DepartureTime = request.DepartureTimes[i];
                        existingTrain.ArrivalTime = request.ArrivalTimes[i];
                        existingTrain.Commission = request.Commissions[i];
                        existingTrain.CurrencyId = request.CurrencyIds[i];
                        existingTrain.Origin = request.Origins[i];
                        existingTrain.Destination = request.Destinations[i];
                        existingTrain.FareAmount = request.FareAmounts[i];
                        existingTrain.TrainNo = request.TrainNos[i];
                        existingTrain.PNRNo = request.PNRNos[i];
                        existingTrain.Taxes = request.Taxes[i];
                        existingTrain.TicketNo = request.TicketNos[i];
                        existingTrain.Total = (request.FareAmounts[i] ?? 0) + (request.Commissions[i] ?? 0) + (request.Taxes[i] ?? 0);
                        existingTrain.TrainClass = request.TrainClass[i];
                        existingTrain.IssuingAgentId = request.IssuingAgentIds[i];
                    }
                }
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Train details saved!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving train details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> DeleteTrainDetails(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            await _context.FamilyHeadTrain
                .Where(e => e.FamilyHeadTrainId == id)
                .ExecuteUpdateAsync(e => e.SetProperty(f => f.TicketStatus, TicketStatus.InActive));

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Train details moved to history!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving train details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveFlightImage(FamilyHeadFlightPdfModel request, int id)
    {
        FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);
        FamilyHeadFlight flight = _context.FamilyHeadFlight.Find(request.FamilyHeadFlightId);

        if (request.TicketPDF is not null)
        {
            string pdfDirectory = Path.Combine(_host.WebRootPath, "Pdfs", "FlightTicket");

            if (!Directory.Exists(pdfDirectory))
            {
                Directory.CreateDirectory(pdfDirectory);
            }

            string fileName = Path.GetFileName(request.TicketPDF.FileName);
            string path = Path.Combine(pdfDirectory, fileName);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            await using FileStream fs = new(path, FileMode.Create);
            await request.TicketPDF.CopyToAsync(fs);

            flight.TicketPDF = Path.Combine("Pdfs", "FlightTicket", fileName);

            await _context.SaveChangesAsync();

            return Json(new
            {
                status = 200,
                msg = "Flight PDF saved!",
            });
        }

        return Json(new
        {
            status = 400,
            msg = "Flight PDF not saved!",
        });
    }

    public async Task<JsonResult> SaveTrainImage(FamilyHeadTrainPdfModel request, int id)
    {
        FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);
        FamilyHeadTrain train = _context.FamilyHeadTrain.Find(request.FamilyHeadTrainId);

        if (request.TicketPDF is not null)
        {
            string pdfDirectory = Path.Combine(_host.WebRootPath, "Pdfs", "TrainTicket");

            if (!Directory.Exists(pdfDirectory))
            {
                Directory.CreateDirectory(pdfDirectory);
            }

            string fileName = Path.GetFileName(request.TicketPDF.FileName);
            string path = Path.Combine(pdfDirectory, fileName);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            await using FileStream fs = new(path, FileMode.Create);
            await request.TicketPDF.CopyToAsync(fs);

            train.TicketPDF = Path.Combine("Pdfs", "TrainTicket", fileName);

            await _context.SaveChangesAsync();

            return Json(new
            {
                status = 200,
                msg = "Train PDF saved!",
            });
        }

        return Json(new
        {
            status = 400,
            msg = "Train PDF not saved!",
        });
    }

    public async Task<JsonResult> SaveAccomodation(FamilyHeadAccomodationModel request, int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.AccomodationAsPerPackage = request.AccomodationAsPerPackage;
            await _context.SaveChangesAsync();

            await _context.FamilyHeadAccomodations
                .Where(e => e.FamilyHeadId == id)
                .ExecuteDeleteAsync();

            for (int i = 0; i < request.HotelIds.Count; i++)
            {
                FamilyHeadAccomodation accomodation = new FamilyHeadAccomodation
                {
                    FamilyHeadId = id,
                    HotelId = request.HotelIds[i],
                    IndividualFacilityDescriptionId = request.IndividualFacilityDescriptionIds[i],
                    CheckIn = request.CheckIns[i],
                    CheckOut = request.CheckOuts[i],
                    FloorNumber = request.FloorNumbers[i],
                    RoomNumber = request.RoomNumbers[i],
                    Day = request.Day[i],
                    Night = request.Night[i],
                    FoodPackageId = request.MealPlan[i],
                    //GenderSegregated = request.GenderSegregateds[i],
                };

                await _context.FamilyHeadAccomodations.AddAsync(accomodation);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Accomodation details saved successfully!",
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return Json(new
            {
                status = 500,
                msg = "An error occurred while saving accomodation details.",
                details = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    public async Task<JsonResult> SaveProformaDate(DateTime date, int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);
            if (familyHead == null)
            {
                return Json(new { status = 404, msg = "Family Head not found" });
            }

            familyHead.ProformaDate = date;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { status = 200, msg = "Date Saved Successfully" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { status = 500, msg = "Error saving date: " + ex.Message });
        }
    }
    
    public async Task<JsonResult> SaveNomineeCnic(int id, IFormFile NomineeCnic)
    {
        //FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);
        //FamilyHeadNominee nominee = await _context.FamilyHeadNominees.FindAsync(familyHead.FamilyHeadId);

        FamilyHeadNominee nominee = await _context.FamilyHeadNominees.Where(e => e.FamilyHeadId == id).FirstOrDefaultAsync();

        if (NomineeCnic != null)
        {
            string fileName = NomineeCnic.FileName;
            string path = Path.Combine(_host.WebRootPath, "Images", "NomineeCnic", fileName);

            if (!string.IsNullOrEmpty(path)) { string dir_dt = Path.GetDirectoryName(path); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                await NomineeCnic.CopyToAsync(fs);
            }

            nominee.NomineeCnicImage = ApplicationManager.NomineeCnicPath + fileName;
            await _context.SaveChangesAsync();
        }

        return Json(new { status = 200, msg = "Date Saved Successfully"});
    }

    public async Task<JsonResult> SaveFFNumber(FFNumber request, int id)
    {
        FamilyHead familyHead = await _context.FamilyHeads.FindAsync(id);

        FamilyHeadFFNumber FFNumber = await _context.FamilyHeadFFNumbers.FindAsync(familyHead.FamilyHeadId);


        if (familyHead == null)
        {
            return Json(new
            {
                status = 404,
                msg = "Family head not found."
            });
        }

        await _context.FamilyHeadFFNumbers.Where(e => e.FamilyHeadId == id).ExecuteDeleteAsync();


        for (int i = 0; i < (request.AirlineId.Count); i++)
        {
            await _context.FamilyHeadFFNumbers.AddAsync
                (new FamilyHeadFFNumber
                {
                    FamilyHeadId = familyHead.FamilyHeadId,
                    AirlineId = request.AirlineId[i],
                    FFNumber = request.FFNumbers[i]
                });
        }
        await _context.SaveChangesAsync();

        return Json(new { status = 200, msg = "Date Saved Successfully" });
    }
}

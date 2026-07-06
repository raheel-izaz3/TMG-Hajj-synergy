using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Transactions;
using TMG.Data;
using TMG.Models.AirlineModels;
using TMG.Models.FamilyModels;
using TMG.Models.FormModels.FamilyModels;
using TMG.Models.MiscModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static QRCoder.PayloadGenerator;
using static TMG.Controllers.FamilyControllers.FamilyHeadFormController;

namespace TMG.Controllers.FamilyControllers;
public partial class CustomerPortfolioFormController : Controller
{
    private readonly TMGDbContext _context;
    private readonly IWebHostEnvironment _host;
    private readonly IHttpContextAccessor _http;

    public CustomerPortfolioFormController(TMGDbContext context, IWebHostEnvironment host, IHttpContextAccessor http)
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
            CustomerPortfolio familyHead = await _context.CustomerPortfolios.FindAsync(id);

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
            familyHead.FatherHusbandName = request.FatherHusbandName;
            familyHead.CNIC = request.CNIC;
            familyHead.DateOfBirth = request.DateOfBirth;
            familyHead.PlaceOfBirth = request.PlaceOfBirth;
            familyHead.Nationality = request.Nationality;
            familyHead.NationalityId = request.NationalityId;
            familyHead.Gender = request.Gender;
            familyHead.BloodGroup = request.BloodGroup;
            familyHead.FiqahId = request.FiqahId;
            familyHead.IsMarried = request.IsMarried;
            familyHead.Residency = request.Residency;
            familyHead.CountryId = request.CountryId;
            familyHead.PassportType = request.PassportType;
            familyHead.PassportTypeId = request.PassportTypeId;
            familyHead.CountryCode = request.CountryCode;
            familyHead.CountryCodeId = request.CountryCodeId;
            familyHead.CityId = request.CityId;
            familyHead.HajjDepartmentId = request.HajjDepartmentId;
            familyHead.PassportNumber = request.PassportNumber;
            familyHead.PassportDateOfIssue = request.PassportDateOfIssue;
            familyHead.PassportDateOfExpiry = request.PassportDateOfExpiry;
            familyHead.TokenRemarks = request.TokenRemarks;
            familyHead.ApplicantType = request.ApplicantType;
            familyHead.DateOfEmail = request.DateOfEmail;

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
            CustomerPortfolio familyHead = await _context.CustomerPortfolios.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            await _context.CustomerPortfolioAddress.Where(e => e.CustomerPortfolioId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberAddressTypeIds?.Count ?? 0); i++)
            {
                await _context.CustomerPortfolioAddress.AddAsync(
                    new CustomerPortfolioAddress
                    {
                        CustomerPortfolioId = familyHead.CustomerPortfolioId,
                        MemberAddressTypeId = request.MemberAddressTypeIds[i],
                        Address = request.MemberAddresses[i],
                        Country = request.MemberAddressCountries[i],
                        City = request.MemberAddressCities[i],
                        ZipCode = request.MemberAddressZipCodes[i],
                        PoBox = request.MemberAddressPoBoxes[i],
                        Preferred = request.MemberAddressPreferred[i],
                        TehsilId = request.MemberAddressTehsilCodes[i],
                    }
                    );
            }

            await _context.CustomerPortfolioContacts.Where(e => e.CustomerPortfolioId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberContactTypeIds?.Count ?? 0); i++)
            {
                await _context.CustomerPortfolioContacts.AddAsync(
                    new CustomerPortfolioContact
                    {
                        CustomerPortfolioId = familyHead.CustomerPortfolioId,
                        MemberContactTypeId = request.MemberContactTypeIds[i],
                        Contact = request.MemberContacts[i],
                        Preferred = request.MemberContactPreferred[i]
                    }
                    );
            }

            await _context.CustomerPortfolioEmails.Where(e => e.CustomerPortfolioId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberEmailTypeIds?.Count ?? 0); i++)
            {
                await _context.CustomerPortfolioEmails.AddAsync(
                    new CustomerPortfolioEmail
                    {
                        CustomerPortfolioId = familyHead.CustomerPortfolioId,
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
            CustomerPortfolio familyHead = await _context.CustomerPortfolios.FindAsync(id);

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
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.MemberImage.CopyToAsync(fs);
                }
                familyHead.MemberImage = @"Images\MemberImages\" + request.MemberImage.FileName;
            }

            if (request.PassportImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "PassportImages", request.PassportImage.FileName);
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.PassportImage.CopyToAsync(fs);
                }
                familyHead.PassportImage = @"Images\PassportImages\" + request.PassportImage.FileName;
            }

            if (request.VisaImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "VisaImages", request.VisaImage.FileName);
                await using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await request.VisaImage.CopyToAsync(fs);
                }
                familyHead.VisaImage = @"Images\VisaImages\" + request.VisaImage.FileName;
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
            CustomerPortfolio familyHead = await _context.CustomerPortfolios.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            for (int i = 0; i < request.CustomerPortfolioIds.Count; i++)
            {
                CustomerPortfolio member = await _context.CustomerPortfolios.FindAsync(request.CustomerPortfolioIds[i]);

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

    public async Task<JsonResult> SaveRemarks(string remarks, int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            CustomerPortfolio familyHead = await _context.CustomerPortfolios.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            var oldRemarksLog = new CustomerRemarksLogs
            {
                CustomerPortfolioId = familyHead.CustomerPortfolioId,
                Remarks = familyHead.Remarks
                //CustomerRemarksId = familyHead.CustomerRemarksId
            };

            await _context.CustomerRemarksLogs.AddAsync(oldRemarksLog);

            familyHead.Remarks = remarks;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Remarks & Status saved!",
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

    public async Task<JsonResult> SaveStatus(CustomerProfileStatusModel request, int id)
    {

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            CustomerPortfolio familyHead = await _context.CustomerPortfolios.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.CustomerStatusId = request.Status;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Remarks & Status saved!",
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
            CustomerPortfolio familyHead = await _context.CustomerPortfolios.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            familyHead.ReferenceId = request.ReferenceId;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Refferel details saved!",
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

}

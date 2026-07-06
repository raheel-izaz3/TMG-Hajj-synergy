using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.ClientModels;
using TMG.Models.FormModels.FamilyModels;
using static TMG.Controllers.FamilyControllers.FamilyHeadFormController;

namespace TMG.Controllers.ClientControllers;
public partial class ClientDataFormController : Controller
{
    private readonly TMGDbContext _context;
    private readonly IWebHostEnvironment _host;
    private readonly IHttpContextAccessor _http;

    public ClientDataFormController(TMGDbContext context, IWebHostEnvironment host, IHttpContextAccessor http)
    {
        _context = context;
        _host = host;
        _http = http;
    }

    public async Task<JsonResult> SavePassport(FamilyHeadPassportModel request, int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            ClientData clientData = await _context.ClientDatas.FindAsync(id);

            if (clientData == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Client not found."
                });
            }

            clientData.SPOId = request.SPOId;
            clientData.FeedBy = request.FeedBy;
            clientData.HonorificsId = request.HonorificsId;
            clientData.SurName = request.SurName;
            clientData.GivenName = request.GivenName;
            clientData.FatherHusbandName = request.FatherHusbandName;
            clientData.CNIC = request.CNIC;
            clientData.DateOfBirth = request.DateOfBirth;
            clientData.PlaceOfBirth = request.PlaceOfBirth;
            clientData.Nationality = request.Nationality;
            clientData.NationalityId = request.NationalityId;
            clientData.Gender = request.Gender;
            clientData.BloodGroup = request.BloodGroup;
            clientData.FiqahId = request.FiqahId;
            clientData.IsMarried = request.IsMarried;
            clientData.Residency = request.Residency;
            clientData.CountryId = request.CountryId;
            clientData.PassportType = request.PassportType;
            clientData.PassportTypeId = request.PassportTypeId;
            clientData.CountryCode = request.CountryCode;
            clientData.CountryCodeId = request.CountryCodeId;
            clientData.CityId = request.CityId;
            clientData.HajjDepartmentId = request.HajjDepartmentId;
            clientData.PassportNumber = request.PassportNumber;
            clientData.PassportDateOfIssue = request.PassportDateOfIssue;
            clientData.PassportDateOfExpiry = request.PassportDateOfExpiry;
            clientData.TokenRemarks = request.TokenRemarks;
            clientData.ApplicantType = request.ApplicantType;
            clientData.DateOfEmail = request.DateOfEmail;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new
            {
                status = 200,
                msg = "Passport details saved!",
                headId = id,
                clientData
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
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            ClientData clientData = await _context.ClientDatas.FindAsync(id);

            if (clientData == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Client not found."
                });
            }

            await _context.ClientDataAddresses.Where(e => e.ClientDataId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberAddressTypeIds?.Count ?? 0); i++)
            {
                await _context.ClientDataAddresses.AddAsync(
                    new ClientDataAddress
                    {
                        ClientDataId = clientData.ClientDataId,
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

            await _context.ClientDataContacts.Where(e => e.ClientDataId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberContactTypeIds?.Count ?? 0); i++)
            {
                await _context.ClientDataContacts.AddAsync(
                    new ClientDataContact
                    {
                        ClientDataId = clientData.ClientDataId,
                        MemberContactTypeId = request.MemberContactTypeIds[i],
                        Contact = request.MemberContacts[i],
                        Preferred = request.MemberContactPreferred[i]
                    }
                );
            }

            await _context.ClientDataEmails.Where(e => e.ClientDataId == id).ExecuteDeleteAsync();

            for (int i = 0; i < (request.MemberEmailTypeIds?.Count ?? 0); i++)
            {
                await _context.ClientDataEmails.AddAsync(
                    new ClientDataEmail
                    {
                        ClientDataId = clientData.ClientDataId,
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
            ClientData familyHead = await _context.ClientDatas.FindAsync(id);

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
            ClientData familyHead = await _context.ClientDatas.FindAsync(id);

            if (familyHead == null)
            {
                return Json(new
                {
                    status = 404,
                    msg = "Family head not found."
                });
            }

            for (int i = 0; i < request.ClientDataIds.Count; i++)
            {
                ClientData member = await _context.ClientDatas.FindAsync(request.ClientDataIds[i]);

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
} 
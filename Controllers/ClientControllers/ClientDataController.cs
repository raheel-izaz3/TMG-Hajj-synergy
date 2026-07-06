using Microsoft.AspNetCore.Mvc;
using TMG.Data;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using TMG.Models.ClientModels;
using Microsoft.EntityFrameworkCore;
using TMG.Models.FormModels.FamilyModels;
using TMG.ManagerClasses;

namespace TMG.Controllers.ClientControllers
{
    public class ClientDataController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public ClientDataController(TMGDbContext context, IHttpContextAccessor http, IWebHostEnvironment host)
        {
            _context = context;
            _http = http;
            _host = host;
        }

        public IActionResult Index()
        {
            var data = _context.ClientDatas
                .Include(e => e.SPO)
                .Include(e => e.Reference)
                .Include(e => e.HajjDepartment)
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        && !e.Trashed && e.IsHead)
                .OrderByDescending(e => e.ClientDataId)
                .ToList();

            ViewBag.TrashedCount = _context.ClientDatas
                .Where(e => e.Trashed
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Count();

            return View(data);
        }

        public IActionResult ViewMember(int id)
        {
            ViewBag.EditAllowed = PrivilegeManager.CanEditFamily(_context, _http);
            ViewBag.TrashAllowed = PrivilegeManager.CanTrashFamily(_context, _http);
            ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreFamily(_context, _http);
            ViewBag.VerifyAllowed = PrivilegeManager.CanVerifyFamilyMember(_context, _http);
            ViewBag.CustomerPortfolioId = id;

            var customerPortfolio = _context
                .ClientDataMembers.Include(e => e.ClientData)
                .AsNoTracking()
                .Where(e => e.HeadId == id && !e.ClientData.Trashed);

            ViewBag.TrashCount = _context
                .ClientDataMembers.Where(e => e.ClientData.Trashed && e.HeadId == id)
                .AsNoTracking()
                .Count();

            ViewBag.HajjiBookingStatus = Enum.GetValues(typeof(HajjiBookingStatus))
                .Cast<HajjiBookingStatus>()
                .Select(c => new { Name = c.ToString(), Value = (int)c })
                .ToList();

            return View(customerPortfolio);
        }

        public IActionResult ViewTrash()
        {
            return View(
                _context.ClientDatas.Where(s =>
                    s.Trashed
                    && s.IsHead
                    && s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
            );
        }

        public IActionResult TrashFamily(int id)
        {
            var members = _context
                .ClientDataMembers.Include(e => e.ClientData)
                .Where(e => e.HeadId == id)
                .ToList();

            foreach (var m in members)
            {
                m.ClientData.Trashed = true;
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Trash(int id)
        {
            ClientData familyHead = _context.ClientDatas.Find(id);
            if (familyHead == null)
                return RedirectToAction(nameof(Index));

            familyHead.Trashed = true;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RestoreHead(int id)
        {
            var family = _context.ClientDataMembers.Where(e => e.HeadId == id);
            foreach (var item in family)
            {
                ClientData familyMember = _context.ClientDatas.Find(
                    item.ClientDataId
                );

                ClientDataMembers fm = _context.ClientDataMembers.FirstOrDefault(e =>
                    e.HeadId == item.ClientDataId
                );

                familyMember.Trashed = false;
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateTentative()
        {
            var applicantTypeNames = Enum.GetValues(typeof(ApplicantType))
                .Cast<ApplicantType>()
                .ToDictionary(e => (int)e, e => e.ToString());

            ViewBag.ApplicantTypeNames = _context.ApplicantTypes
                .Where(e => !e.Trashed)
                .ToList();

            ViewBag.Tehsil = _context.TehsilsWithoutTrash;

            ViewBag.City = _context.Cities.Where(e => !e.Trashed).ToList();

            ViewBag.Country = _context.Countries.Where(e => !e.Trashed).ToList();

            ViewBag.HajjDepartment = _context.HajjDepartments.Where(e => !e.Trashed).ToList();

            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypes;
            ViewBag.Remarks = _context.CustomerRemarks.Where(e => !e.Trashed).ToList();
            ViewBag.Status = _context.CustomerStatuses.Where(e => !e.Trashed).ToList();
            ViewBag.Referral = _context.ReferencesWithoutTrash;

            return View();
        }

        [HttpPost]
        public IActionResult CreateTentative(TentativeFamilyHead request)
        {
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Remarks = _context.CustomerRemarks.Where(e => !e.Trashed).ToList();
            ViewBag.Status = _context.CustomerStatuses.Where(e => !e.Trashed).ToList();
            ViewBag.Referral = _context.ReferencesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypes;

            //string cnic = request.CNIC;
            //var cnicFh = _context
            //    .FamilyHeadsWithoutTrash.Select(e => e.CNIC)
            //    .FirstOrDefault(e => e == cnic);

            //if (cnicFh is not null)
            //{
            //    ModelState.AddModelError("CNIC", "CNIC already exists");
            //}

            if (!ModelState.IsValid)
                return View(request);

            int Pax = request.adult + request.child + request.infant;

            var transaction = _context.Database.BeginTransaction();

            Fiqah fiqah = _context.FiqahsWithoutTrash.FirstOrDefault();
            Honorifics honorifics = _context.HonorificsWithoutTrash.FirstOrDefault();

            DateTime currentDate = DateTime.Today;
            DateTime dateTime = DateTime.Parse(currentDate.ToString());

            DateTime infantAge = DateTime.Today;
            DateTime childAge = DateTime.Today.AddYears(-3);
            DateTime adultAge = DateTime.Today.AddYears(-13);

            var settings = _context
                .ApplicationSettings.Where(s =>
                    s.HajjSeasonsId
                    == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                )
                .FirstOrDefault();

            ClientData fh = new ClientData
            {
                GivenName = request.GivenName,
                SurName = request.SurName,
                CNIC = request.CNIC,
                IsHead = true,
                Tentative = true,
                DateOfBirth = adultAge,
                FiqahId = fiqah.FiqahId,
                HonorificsId = honorifics.HonorificsId,
                Gender = Gender.Male,
                PassportDateOfIssue = DateTime.Now,
                PassportDateOfExpiry = DateTime.Now,
                SPOId = request.SPOID,
                FeedBy = _http.HttpContext.Session.GetString("Username"),
                DateOfQuery = DateTime.Now,
                ReferenceId = request.Refferel,
                ApplicantType = request.ApplicantType,
                CityId = request.CityId,
                CountryId = request.CountryId,
                HajjDepartmentId = request.HajjDepartmentId,
                HajjSeasonsId = Convert.ToInt32(
                    _http.HttpContext.Session.GetString("ActiveSeasonId")
                ),
                IsQuery = false,
                ApplicantTypesId = request.ApplicantTypesId
            };

            _context.ClientDatas.Add(fh);
            _context.SaveChanges();

            //settings.FamilyCount++;
            //settings.IndividualCount++;
            _context.SaveChanges();

            ClientDataContact contact = new ClientDataContact
            {
                Contact = request.Phone,
                MemberContactTypeId = request.ContactTypeId,
                ClientDataId = fh.ClientDataId,
                Preferred = true,
            };

            _context.ClientDataContacts.Add(contact);
            _context.SaveChanges();

            ClientDataAddress address = new ClientDataAddress
            {
                Address = request.Address,
                TehsilId = request.TehsilId,
                ZipCode = request.ZipCode,
                PoBox = request.ZipCode,
                CityId = request.CityId,
                CountryId = request.CountryId,
                ClientDataId = fh.ClientDataId
            };

            _context.ClientDataAddresses.Add(address);
            _context.SaveChanges();

            ClientDataEmail email = new ClientDataEmail
            {
                Email = request.Email,
                ClientDataId = fh.ClientDataId,
                Preferred = true,
            };

            _context.ClientDataEmails.Add(email);
            _context.SaveChanges();

            var reqDocs = _context.RequiredDocumentsWithoutTrash.ToList();

            for (int i = 0; i < reqDocs.Count; i++)
            {
                _context.ClientDataDocuments.Add(
                    new ClientDataDocuments
                    {
                        ClientDataId = fh.ClientDataId,
                        RequiredDocumentId = reqDocs.ElementAt(i).RequiredDocumentId,
                        Recieved = false,
                        Remarks = "",
                    }
                );
            }

            _context.SaveChanges();

            _context.ClientDataMembers.Add(
                new ClientDataMembers
                {
                    ClientDataId = fh.ClientDataId,
                    HeadId = fh.ClientDataId,
                    IsHead = true,
                }
            );

            _context.SaveChanges();

            for (int i = 1; i < request.adult; i++)
            {
                ClientData fhm = new ClientData
                {
                    GivenName = request.GivenName + "'s Member",
                    SurName = request.SurName + "'s Member",
                    CNIC = request.CNIC,
                    IsHead = false,
                    Tentative = true,
                    DateOfBirth = adultAge,
                    FiqahId = fiqah.FiqahId,
                    HonorificsId = honorifics.HonorificsId,
                    Gender = Gender.Male,
                    PassportDateOfIssue = DateTime.Now,
                    PassportDateOfExpiry = DateTime.Now,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    DateOfQuery = DateTime.Now,
                    ApplicantType = request.ApplicantType,
                    CityId = request.CityId,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    IsQuery = false,
                    ApplicantTypesId = request.ApplicantTypesId
                };

                _context.ClientDatas.Add(fhm);
                _context.SaveChanges();

                settings.IndividualCount++;
                _context.SaveChanges();

                for (int j = 0; j < reqDocs.Count; j++)
                {
                    _context.ClientDataDocuments.Add(
                        new ClientDataDocuments
                        {
                            ClientDataId = fhm.ClientDataId,
                            RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                _context.SaveChanges();

                ClientDataContact contactm = new ClientDataContact
                {
                    Contact = request.Phone,
                    MemberContactTypeId = request.ContactTypeId,
                    ClientDataId = fhm.ClientDataId,
                    Preferred = true,
                };

                _context.ClientDataContacts.Add(contactm);
                _context.SaveChanges();

                ClientDataAddress addressm = new ClientDataAddress
                {
                    Address = request.Address,
                    TehsilId = request.TehsilId,
                    ZipCode = request.ZipCode,
                    PoBox = request.ZipCode,
                    CityId = request.CityId,
                    CountryId = request.CountryId,
                    ClientDataId = fhm.ClientDataId
                };

                _context.ClientDataAddresses.Add(addressm);
                _context.SaveChanges();

                ClientDataEmail emailm = new ClientDataEmail
                {
                    Email = request.Email,
                    ClientDataId = fhm.ClientDataId,
                    Preferred = true,
                };

                _context.ClientDataEmails.Add(emailm);
                _context.SaveChanges();

                _context.ClientDataMembers.Add(
                    new ClientDataMembers
                    {
                        ClientDataId = fhm.ClientDataId,
                        HeadId = fh.ClientDataId,
                        IsHead = false,
                    }
                );

                _context.SaveChanges();
            }

            for (int i = 0; i < request.child; i++)
            {
                ClientData fhm = new ClientData
                {
                    GivenName = request.GivenName + "'s Member",
                    SurName = request.SurName + "'s Member",
                    CNIC = request.CNIC,
                    IsHead = false,
                    Tentative = true,
                    DateOfBirth = childAge,
                    FiqahId = fiqah.FiqahId,
                    HonorificsId = honorifics.HonorificsId,
                    Gender = Gender.Male,
                    PassportDateOfIssue = DateTime.Now,
                    PassportDateOfExpiry = DateTime.Now,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    DateOfQuery = DateTime.Now,
                    ApplicantType = request.ApplicantType,
                    CityId = request.CityId,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    IsQuery = false,
                    ApplicantTypesId = request.ApplicantTypesId
                };

                _context.ClientDatas.Add(fhm);
                _context.SaveChanges();

                settings.IndividualCount++;
                _context.SaveChanges();

                for (int j = 0; j < reqDocs.Count; j++)
                {
                    _context.ClientDataDocuments.Add(
                        new ClientDataDocuments
                        {
                            ClientDataId = fhm.ClientDataId,
                            RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                ClientDataContact contactm = new ClientDataContact
                {
                    Contact = request.Phone,
                    MemberContactTypeId = request.ContactTypeId,
                    ClientDataId = fhm.ClientDataId,
                    Preferred = true,
                };

                _context.ClientDataContacts.Add(contactm);
                _context.SaveChanges();

                ClientDataAddress addressm = new ClientDataAddress
                {
                    Address = request.Address,
                    TehsilId = request.TehsilId,
                    ZipCode = request.ZipCode,
                    PoBox = request.ZipCode,
                    CityId = request.CityId,
                    CountryId = request.CountryId,
                    ClientDataId = fhm.ClientDataId
                };

                _context.ClientDataAddresses.Add(addressm);
                _context.SaveChanges();

                ClientDataEmail emailm = new ClientDataEmail
                {
                    Email = request.Email,
                    ClientDataId = fhm.ClientDataId,
                    Preferred = true,
                };

                _context.ClientDataEmails.Add(emailm);
                _context.SaveChanges();

                _context.ClientDataMembers.Add(
                    new ClientDataMembers
                    {
                        ClientDataId = fhm.ClientDataId,
                        HeadId = fh.ClientDataId,
                        IsHead = false,
                    }
                );

                _context.SaveChanges();
            }

            for (int i = 0; i < request.infant; i++)
            {
                ClientData fhm = new ClientData
                {
                    GivenName = request.GivenName + "'s Member",
                    SurName = request.SurName + "'s Member",
                    CNIC = request.CNIC,
                    IsHead = false,
                    Tentative = true,
                    DateOfBirth = infantAge,
                    FiqahId = fiqah.FiqahId,
                    HonorificsId = honorifics.HonorificsId,
                    Gender = Gender.Male,
                    PassportDateOfIssue = DateTime.Now,
                    PassportDateOfExpiry = DateTime.Now,
                    SPOId = request.SPOID,
                    FeedBy = _http.HttpContext.Session.GetString("Username"),
                    DateOfQuery = DateTime.Now,
                    ApplicantType = request.ApplicantType,
                    CityId = request.CityId,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                    IsQuery = false,
                    ApplicantTypesId = request.ApplicantTypesId
                };

                _context.ClientDatas.Add(fhm);
                _context.SaveChanges();

                settings.IndividualCount++;
                _context.SaveChanges();

                for (int j = 0; j < reqDocs.Count; j++)
                {
                    _context.ClientDataDocuments.Add(
                        new ClientDataDocuments
                        {
                            ClientDataId = fhm.ClientDataId,
                            RequiredDocumentId = reqDocs.ElementAt(j).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                ClientDataContact contactm = new ClientDataContact
                {
                    Contact = request.Phone,
                    MemberContactTypeId = request.ContactTypeId,
                    ClientDataId = fhm.ClientDataId,
                    Preferred = true,
                };

                _context.ClientDataContacts.Add(contactm);
                _context.SaveChanges();

                ClientDataAddress addressm = new ClientDataAddress
                {
                    Address = request.Address,
                    TehsilId = request.TehsilId,
                    ZipCode = request.ZipCode,
                    PoBox = request.ZipCode,
                    CityId = request.CityId,
                    CountryId = request.CountryId,
                    ClientDataId = fhm.ClientDataId
                };

                _context.ClientDataAddresses.Add(addressm);
                _context.SaveChanges();

                ClientDataEmail emailm = new ClientDataEmail
                {
                    Email = request.Email,
                    ClientDataId = fhm.ClientDataId,
                    Preferred = true,
                };

                _context.ClientDataEmails.Add(emailm);
                _context.SaveChanges();

                _context.ClientDataMembers.Add(
                    new ClientDataMembers
                    {
                        ClientDataId = fhm.ClientDataId,
                        HeadId = fh.ClientDataId,
                        IsHead = false,
                    }
                );

                _context.SaveChanges();
            }

            _context.SaveChanges();

            transaction.Commit();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.ViewFamilyId = _context
                .ClientDataMembers.FirstOrDefault(e => e.ClientDataId == id)
                .HeadId;

            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.NomineeRels = _context.NomineeRelationsWithoutTrash;

            ViewBag.PassportType = _context.PassportTypes.Where(e => !e.Trashed).ToList();

            ViewBag.Nationality = _context.Nationalities.Where(e => !e.Trashed).ToList();

            ViewBag.Country = _context.Countries.Where(e => !e.Trashed).ToList();

            ViewBag.City = _context.Cities.Where(e => !e.Trashed).ToList();

            ViewBag.CountryCode = _context.CountryCodes.Where(e => !e.Trashed).ToList();

            ViewBag.HajjDepartment = _context.HajjDepartments.Where(e => !e.Trashed).ToList();

            ClientData customerPortfolio = _context
                .ClientDatas.Where(e => !e.Trashed)
                .FirstOrDefault(e => e.ClientDataId == id);

            ViewBag.FeedBy = customerPortfolio.FeedBy;

            var headid = _context
                .ClientDataMembers.FirstOrDefault(e =>
                    e.ClientDataId == customerPortfolio.ClientDataId
                )
                .HeadId;
            var headdata = _context.ClientDatas.FirstOrDefault(e =>
                e.ClientDataId == headid
            );

            ViewBag.FamilyHeadAddresses = _context.ClientDataAddresses.Where(e =>
                e.ClientDataId == id
            );
            ViewBag.FamilyHeadContacts = _context.ClientDataContacts.Where(e =>
                e.ClientDataId == id
            );
            ViewBag.FamilyHeadEmails = _context.ClientDataEmails.Where(e =>
                e.ClientDataId == id
            );
            ViewBag.FamilyHeadDocuments = _context
                .ClientDataDocuments.Include(e => e.RequiredDocument)
                .Where(e => e.ClientDataId == id);

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.References = _context.ReferencesWithoutTrash;
            ViewBag.FamilyMembers = _context
                .ClientDataMembers.Include(e => e.ClientData)
                .Where(e => e.HeadId == id && !e.ClientData.Trashed && !e.IsHead)
                .ToList();

            ViewBag.FamilyMembersWithHead = _context
                .ClientDataMembers.Include(e => e.ClientData)
                .Where(e => e.HeadId == id && !e.ClientData.Trashed)
                .ToList();

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            var AssignAddresshead = _context.ClientDataAddresses.Where(e =>
                e.ClientDataId == headid
            );
            var AssignAddress = _context.ClientDataAddresses.Where(e =>
                e.ClientDataId == id
            );
            if (AssignAddress.Count() == 0)
            {
                ViewBag.AssignedAddresses = AssignAddresshead;
            }
            else
            {
                ViewBag.AssignedAddresses = AssignAddress;
            }

            var AssignContactshead = _context.ClientDataContacts.Where(e =>
                e.ClientDataId == headid
            );
            var AssignContacts = _context.ClientDataContacts.Where(e =>
                e.ClientDataId == id
            );
            if (AssignContacts.Count() == 0)
            {
                ViewBag.AssignedContacts = AssignContactshead;
            }
            else
            {
                ViewBag.AssignedContacts = AssignContacts;
            }

            var AssignedEmailshead = _context.ClientDataEmails.Where(e =>
                e.ClientDataId == headid
            );
            var AssignedEmails = _context.ClientDataEmails.Where(e =>
                e.ClientDataId == id
            );
            if (AssignedEmails.Count() == 0)
            {
                ViewBag.AssignedEmails = AssignedEmailshead;
            }
            else
            {
                ViewBag.AssignedEmails = AssignedEmails;
            }

            ClientData mehram = null;

            FamilyHeadEditRequest request = new FamilyHeadEditRequest
            {
                ClientDataId = id,
                FamilyHeadRelationId = customerPortfolio.FamilyHeadRelationId,
                MehramCNIC = mehram?.CNIC,
                FamilyHeadId = id,
                SPOId = customerPortfolio.SPOId,
                FeedBy = customerPortfolio.FeedBy,
                HonorificsId = customerPortfolio.HonorificsId,
                SurName = customerPortfolio.SurName,
                GivenName = customerPortfolio.GivenName,
                FatherName = customerPortfolio.FatherHusbandName,
                CNIC = customerPortfolio.CNIC,
                DateOfBirth = customerPortfolio.DateOfBirth,
                PlaceOfBirth = customerPortfolio.PlaceOfBirth,
                Nationality = customerPortfolio.Nationality,
                NationalityId = customerPortfolio.NationalityId,
                Gender = customerPortfolio.Gender,
                BloodGroup = customerPortfolio.BloodGroup,
                FiqahId = customerPortfolio.FiqahId,
                IsEmployed = customerPortfolio.IsEmployed,
                IsMarried = customerPortfolio.IsMarried,
                EmployedAt = customerPortfolio.EmployedAt,
                Designation = customerPortfolio.Designation,
                Residency = customerPortfolio.Residency,
                CountryId = customerPortfolio.CountryId,
                PassportType = customerPortfolio.PassportType,
                PassportTypeId = customerPortfolio.PassportTypeId,
                CountryCode = customerPortfolio.CountryCode,
                CountryCodeId = customerPortfolio.CountryCodeId,
                CityId = customerPortfolio.CityId,
                HajjDepartmentId = customerPortfolio.HajjDepartmentId,
                PassportNumber = customerPortfolio.PassportNumber,
                PassportDateOfIssue = customerPortfolio?.PassportDateOfIssue,
                PassportDateOfExpiry = customerPortfolio?.PassportDateOfExpiry,
                IsHead = customerPortfolio.IsHead,
                Tentative = customerPortfolio.Tentative,
                CustomerRemarks = customerPortfolio.Remarks,
                Status = customerPortfolio.Status,
                ApplicantType = customerPortfolio.ApplicantType,
                ReferenceId = customerPortfolio.ReferenceId,
                DateOfEmail = customerPortfolio.DateOfEmail,
            };

            return View(request);
        }

        public IActionResult AddMember(int customerPortfolioId)
        {
            ClientData familyHead = _context.ClientDatas.FirstOrDefault(e =>
                e.IsHead && e.ClientDataId == customerPortfolioId
            );

            ViewBag.ClientDataId = customerPortfolioId;

            ViewBag.FeedBy = familyHead.FeedBy;
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.FamilyHeadAddresses = _context.ClientDataAddresses.Where(e =>
                e.ClientDataId == customerPortfolioId
            );
            ViewBag.FamilyHeadContacts = _context.ClientDataContacts.Where(e =>
                e.ClientDataId == customerPortfolioId
            );
            ViewBag.FamilyHeadEmails = _context.ClientDataEmails.Where(e =>
                e.ClientDataId == customerPortfolioId
            );

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            ViewBag.customerPortfolioId = customerPortfolioId;

            ViewBag.SPOId = familyHead.SPOId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMember(FamilyMemberRegisterRequest request)
        {
            ClientData _familyHead = _context.ClientDatas.FirstOrDefault(e =>
                e.IsHead && e.ClientDataId == request.ClientDataId
            );

            var reqDocs = _context.RequiredDocumentsWithoutTrash.ToList();
            ViewBag.ClientDataId = request.ClientDataId;
            ViewBag.SPOs = _context.SPOsWithoutTrash;
            ViewBag.FamilyHeadAddresses = _context.ClientDataAddresses.Where(e =>
                e.ClientDataId == request.ClientDataId
            );
            ViewBag.FamilyHeadContacts = _context.ClientDataContacts.Where(e =>
                e.ClientDataId == request.ClientDataId
            );
            ViewBag.FamilyHeadEmails = _context.ClientDataEmails.Where(e =>
                e.ClientDataId == request.ClientDataId
            );

            ViewBag.Honorifics = _context.HonorificsWithoutTrash;
            ViewBag.Fiqahs = _context.FiqahsWithoutTrash;
            ViewBag.Tehsils = _context.TehsilsWithoutTrash;

            ViewBag.AddressTypes = _context.MemberAddressTypesWithoutTrash;
            ViewBag.ContactTypes = _context.MemberContactTypesWithoutTrash;
            ViewBag.EmailTypes = _context.MemberEmailTypesWithoutTrash;

            ViewBag.RequiredDocuments = _context.RequiredDocumentsWithoutTrash;

            ViewBag.FamilyHeadRelations = _context.FamilyHeadRelationsWithoutTrash;

            var transaction = _context.Database.BeginTransaction();

            try
            {
                var settings = _context
                    .ApplicationSettings.Where(s =>
                        s.HajjSeasonsId
                        == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                    )
                    .FirstOrDefault();
                settings.IndividualCount++;
                _context.SaveChanges();

                ClientData familyHead = new()
                {
                    Tentative = request.Tentative,
                    ApplicantType = request.ApplicantType,
                    IsHead = false,
                    SPOId = request.SPOId,
                    FeedBy = request.FeedBy?.ToUpper(),
                    HonorificsId = request.HonorificsId,
                    SurName = request.SurName?.ToUpper(),
                    GivenName = request.GivenName?.ToUpper(),
                    FatherHusbandName = request.FatherHusbandName?.ToUpper(),
                    CNIC = request.CNIC,
                    DateOfBirth = request.DateOfBirth,
                    PlaceOfBirth = request.PlaceOfBirth,
                    Nationality = request.Nationality?.ToUpper(),
                    Gender = request.Gender,
                    BloodGroup = request.BloodGroup,
                    FiqahId = request.FiqahId,
                    IsMarried = request.IsMarried,
                    Residency = request.Residency?.ToUpper(),
                    PassportType = request.PassportType,
                    CountryCode = request.CountryCode?.ToUpper(),
                    PassportNumber = request.PassportNumber?.ToUpper(),
                    PassportDateOfIssue = request.PassportDateOfIssue,
                    PassportDateOfExpiry = request.PassportDateOfExpiry,
                    HajjSeasonsId = Convert.ToInt32(
                        _http.HttpContext.Session.GetString("ActiveSeasonId")
                    ),
                };

                _context.ClientDatas.Add(familyHead);
                _context.SaveChanges();

                _context.ClientDataMembers.Add(
                    new ClientDataMembers
                    {
                        ClientDataId = familyHead.ClientDataId,
                        HeadId = _familyHead.ClientDataId,
                        IsHead = false,
                    }
                );
                _context.SaveChanges();

                for (int i = 0; i < reqDocs.Count; i++)
                {
                    _context.ClientDataDocuments.Add(
                        new ClientDataDocuments
                        {
                            ClientDataId = familyHead.ClientDataId,
                            RequiredDocumentId = reqDocs.ElementAt(i).RequiredDocumentId,
                            Recieved = false,
                            Remarks = "",
                        }
                    );
                }

                _context.SaveChanges();

                transaction.Commit();

                return RedirectToAction(
                    nameof(ViewMember),
                    new { id = request.ClientDataId }
                );
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return Ok(new { ex.InnerException.Message });
            }
        }
    }
}

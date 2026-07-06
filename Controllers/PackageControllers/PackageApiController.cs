using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.PackageModels;
using static TMG.Controllers.ReportController;

namespace TMG.Controllers.PackageControllers
{
    public class PackageApiController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        
        public PackageApiController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }
        [HttpGet]
        public async Task<JsonResult> GetFeature()
        {
            var packages = await _dbContext.Packages
                .Select(e => new
                {
                    e.PackageId, // Include PackageId
                    e.Name,
                    e.Days,
                    // Conditional check for MedinahArrival
                    MedinahArrival = e.MedinahArrival == "Before" ? "Madina" :
                                     e.MedinahArrival == "After" ? "Makka" : e.MedinahArrival,
                    e.AdultSar,
                    e.Image
                })
                .ToListAsync();

            if (packages == null || !packages.Any())
            {
                return Json(new { success = false, message = "No packages found." });
            }

            var packageDetails = new List<object>();

            foreach (var package in packages)
            {
                var flightsSum = await _dbContext.PackageFlights
                    .Where(f => f.PackageId == package.PackageId)
                    .SumAsync(f => f.TicketAmount);

                var trainsSum = await _dbContext.PackageTrains
                    .Where(t => t.PackageId == package.PackageId)
                    .SumAsync(t => t.TicketAmount);

                var categoryFacilityPrices = await _dbContext.PackageCategoryFacilities
                    .Include(cf => cf.IndividualFacilityDescription)
                    .Where(cf => cf.PackageId == package.PackageId)
                    .SumAsync(cf => cf.IndividualFacilityDescription.Price);

                var totalAmount = package.AdultSar + (flightsSum ?? 0f) + (trainsSum ?? 0f) + (categoryFacilityPrices);

                var packageInfo = new
                {
                    Id = package.PackageId, 
                    Image = package.Image,
                    Name = package.Name, 
                    Title = $"{package.Days} Days, Arrival: {package.MedinahArrival}",
                    Price = totalAmount 
                };

                packageDetails.Add(packageInfo);
            }

            var result = new
            {
                success = true,
                packages = packageDetails
            };

            return Json(result);
        }
        //Dashborad counts
        public JsonResult Users()
        {
            int userCount = _dbContext.UsersWithoutTrash.Count();

            return Json(new
            {
                UserCount = userCount
            });
        }
        public JsonResult CompanyCount()
        {
            var CompanyCount = _dbContext.CompaniesWithoutTrash
               .Count();

            return Json(new
            {
                CompanyCount = CompanyCount
            });
        }
        public JsonResult PackagesCount()
        {
            int PackageCount = _dbContext.Packages.Where(e => !e.Trashed)
                 .Where(e => e.HajjSeasonsId == 2)
                .Count();

            return Json(new
            {
                PackagesCount = PackageCount
            });
        }
        public JsonResult FamilyCount()
        {
            int familyCount = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2 && e.IsHead)
                .Count();

            return Json(new
            {
                FamilyCount = familyCount
            });
        }
        public JsonResult MemberCount()
        {
            int memberCount = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2 &&
                    (e.HajjiBookingStatus == HajjiBookingStatus.Booked ||
                     e.HajjiBookingStatus == HajjiBookingStatus.Tentative))
                .Count();

            return Json(new
            {
                MemberCount = memberCount
            });
        }
        public JsonResult TentativeCount()
        {
            int Tentative = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2)
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Tentative);

            return Json(new
            {
                Tentative = Tentative
            });
        }
        public JsonResult BookRegularCount()
        {
            int BookRegular = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2)
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.ApplicantType == ApplicantType.Regular);

            return Json(new
            {
                BookRegular = BookRegular
            });
        }
        public JsonResult BookOverseasCount()
        {
            int BookOverseas = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2)
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.ApplicantType == ApplicantType.Overseas);

            return Json(new
            {
                BookOverseas = BookOverseas
            });
        }
        public JsonResult BookIntercityCount()
        {
            int BookIntercity = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2)
                .Count(e => e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.ApplicantType == ApplicantType.Intercity);

            return Json(new { BookIntercity = BookIntercity });
        }
        public JsonResult MaleCountTotal()
        {
            int MaleCount = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2)
                .Count(e => e.Gender == Gender.Male
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            return Json(new { MaleCount = MaleCount });
        }
        public JsonResult FemaleCountTotal()
        {
            int FemaleCount = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2)
                .Count(e => e.Gender == Gender.Female
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            return Json(new { FemaleCount = FemaleCount });
        }
        public JsonResult TotalAdultsCount()
        {
            int AdultCounts = _dbContext.FamilyHeadsWithoutTrash
               .Where(e => e.HajjSeasonsId == 2)
               .AsEnumerable()
               .Count(e => e.IsAdult
                       && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            return Json(new{AdultCounts = AdultCounts});
        }
        public JsonResult ChildCount()
        {
            int ChildCount = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == 2)
                .AsEnumerable()
                .Count(e => e.IsChild
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);
            return Json(new { ChildCount = ChildCount });
        }
        //public JsonResult InfantReportCount()
        //{
        //    int InfantCount = DbContext.FamilyHeadsWithoutTrash
        //        .Where(e => e.HajjSeasonsId == 2)
        //        .AsEnumerable()
        //        .Count(e => e.IsInfant
        //                && e.HajjiBookingStatus == HajjiBookingStatus.Booked);
        //    return Json(new { InfantCount = InfantCount });
        //}
        [HttpGet]
        public JsonResult GetPackageDetail(int id)
        {
            Package package = _dbContext.Packages
                .Include(e => e.TravelRoute)
                .Include(e => e.Company)
                .Include(e => e.Room)
                .Include(e => e.hajjSeasons)
                .FirstOrDefault(e => e.PackageId == id);

            if (package == null)
            {
                return Json(new { success = false, message = "Package not found." });
            }

            var routes = _dbContext.TravelRouteStops
                            .Include(e => e.Vehicle)
                            .Include(e => e.Route)
                            .Where(e => e.TravelRouteId == package.TravelRouteId)
                            .OrderBy(e => e.CheckIn)
                            .ToList();

            var categoryFacilities = _dbContext.PackageCategoryFacilities
                .Include(e => e.IndividualFacilityDescription)
                .ThenInclude(e => e.Place)
                .Where(e => e.PackageId == id)
                .ToList();

            var facilityDescriptions = categoryFacilities
                .Select(e => new
                {
                    Description = e.IndividualFacilityDescription.Description,
                    PlaceName = e.IndividualFacilityDescription.Place.PlaceName
                });

            var assignedAccommodations = _dbContext.PackageAccomodtions
                .Include(e => e.Place)
                .Include(e => e.Accomodation)
                .Include(e => e.AccomodationCategory)
                .Include(e => e.FoodPackage)
                .Include(e => e.Hotel)
                .Where(e => e.PackageId == id)
                .ToList();

            var flights = _dbContext.PackageFlights
                                .Include(e => e.Airline)
                                .OrderBy(e => e.DepartureDate)
                                .Where(e => e.PackageId == id)
                                .ToList();

            var trains = _dbContext.PackageTrains
                                .Include(e => e.Railway)
                                .OrderBy(e => e.DepartureDate)
                                .Where(e => e.PackageId == id)
                                .ToList();

            var terms = _dbContext.PackageTermsConditions
                                .Where(e => e.PackageId == id)
                                .ToList();

            var itineraryInfo = _dbContext.PackageItinerary
                                .Where(e => e.PackageId == id)
                                .ToList();

            var assignedGifts = _dbContext.PackageGifts
                .Include(e => e.Gift)
                .Where(e => e.PackageId == id)
                .ToList();

            var training = _dbContext.PackageTrainings
                .Include(e => e.Training)
                .Where(e => e.PackageId == id)
                .ToList();

            float amount = package.AdultSar
                + trains.Sum(e => e.TicketAmount) ?? 0
                + flights.Sum(e => e.TicketAmount) ?? 0
                + categoryFacilities.Sum(e => e.IndividualFacilityDescription.Price);

            var result = new
            {
                success = true,
                package,
                routes,
                facilityDescriptions,
                assignedAccommodations,
                flights,
                trains,
                terms,
                itineraryInfo,
                assignedGifts,
                training,
                packageAmount = amount.ToString("N0")
            };

            return Json(result);
        }
        [HttpGet]
        public JsonResult GetSpoWiseCounts()
        {
            int hajjSeasonId = 2;

            var spo = _dbContext.SPOsWithoutTrash.ToList();
            var familyheads = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjSeasonsId == hajjSeasonId)
                .ToList();

            List<SPOsData> model = new();

            foreach (var item in spo)
            {
                var tentativeCount = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId && e.HajjiBookingStatus == HajjiBookingStatus.Tentative);

                var bookedCount = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

                var count = (float)familyheads
                    .Count(e => e.SPOId == item.SPOId);

                if (tentativeCount > 0 || bookedCount > 0 || count > 0)
                {
                    model.Add(new SPOsData
                    {
                        SPOId = item.SPOId,
                        SPOName = item.Name,
                        SPOEmail = item.Email,
                        Tentative = tentativeCount,
                        Booked = bookedCount,
                        Count = count
                    });
                }
            }

            return Json(new { success = true, packages = model });
        }
        
        public JsonResult CompanyIndex()
        {
            var packages = _dbContext.PackagesWithoutTrash
                .Select(e => new
                {
                    e.PackageId,
                    e.CompanyId,
                    e.Code
                })
                .ToList();

            var familyHeads = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Select(e => new
                {
                    e.FamilyHeadId,
                    e.PackageId,
                    e.Package.CompanyId
                })
                .ToList();

            
            var companies = _dbContext
                .UserCompanies.Include(e => e.Company)
                .Where(e => !e.Company.Trashed)
                .AsEnumerable()  
                .GroupBy(e => e.CompanyId)
                .Select(g => g.First())
                .Select(e => new
                {
                    e.UserId,
                    e.CompanyId,
                    e.Company.CompanyName,
                    e.Company.Trashed,
                    e.Company.Quota,
                    e.Company.Website,
                    e.Company.CompanyCode,
                })
                .ToList();

            CompanyIndexModel model = new CompanyIndexModel();

            for (int i = 0; i < companies.Count; i++)
            {
                var currCompany = companies.ElementAt(i);

                var companyPackages = packages
                    .Where(e => e.CompanyId == currCompany.CompanyId)
                    .ToList();

                List<CompanyPackageListModel> packagesData = new();

                for (int j = 0; j < companyPackages.Count; j++)
                {
                    var currPackage = companyPackages.ElementAt(j);

                    packagesData.Add(new CompanyPackageListModel(
                        currPackage.PackageId,
                        currPackage.Code,
                        familyHeads.Count(e => e.PackageId == currPackage.PackageId)
                    ));
                }

                int companyPackagesQuota = familyHeads
                    .Where(e => e.CompanyId == currCompany.CompanyId)
                    .Count();

                int remainingQuota = currCompany.Quota - companyPackagesQuota;

                model.Entries.Add(new CompanyIndexModelEntry
                {
                    CompanyId = currCompany.CompanyId,
                    Name = currCompany.CompanyName,
                    ActualQuota = currCompany.Quota,
                    Website = currCompany.Website,
                    Code = currCompany.CompanyCode,
                    Packages = packagesData,
                    RemainingQuota = remainingQuota
                });
            }

            return Json(model);
        }
        public JsonResult FamilyReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => 
                e.HajjSeasonsId == 2 && e.IsHead);

            var result = query.Select(e => new PackageAPiSummary
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult MemberReport()
        {

            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.CompanyVisas)
                .Include(e => e.POTickets)
                .ThenInclude(e => e.FlightDetails)
                .Where(e =>
                    (e.HajjiBookingStatus == HajjiBookingStatus.Booked || e.HajjiBookingStatus == HajjiBookingStatus.Tentative)
                    && e.HajjSeasonsId == 2);

            /* var familyHeads = _dbContext.FamilyHeadsWithoutTrash
                        .Include(e => e.CompanyVisas)
                        .Where(e =>
                            (e.HajjiBookingStatus == HajjiBookingStatus.Booked || e.HajjiBookingStatus == HajjiBookingStatus.Tentative)
                            && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))
                        );*/

            var familyHeadsWithPOTickets = query
                .Where(fh => _dbContext.POTickets
                    .Any(pt => pt.FamilyMembersId.HasValue && !pt.Trashed &&
                               _dbContext.FamilyMembers
                                   .Where(fm => fm.HeadId == fh.FamilyHeadId)
                                   .Select(fm => fm.FamilyMembersId)
                                   .Contains(pt.FamilyMembersId.Value) &&
                               _dbContext.POFlightDetails
                                   .Any(fd => fd.POTicketId == pt.POTicketId && !fd.Trashed)) // Check if associated FlightDetails are not trashed
                )
                .ToList();

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                Gender = e.Gender.ToString(),
                PackageName = e.Package.Name, // <- FIXED
                PassportNo = e.PassportNumber,
                DOB = e.DateOfBirth,
                CompanyNames = e.CompanyVisas.CompanyName != null
                ? e.CompanyVisas.CompanyName : "N/A",
                Visa = e.VisaImage != null ? "✔️" : "❌",

                /*TicketImage = _dbContext.POFlightDetails
                    .Include(e => e.POTicket)
                    .Where(pt => pt.POTicket.FamilyMembersId == e.FamilyHeadId
                            && !pt.POTicket.Trashed)
                    .Select(e => e.TicketImages != null ? "✔️" : "❌")
                    .FirstOrDefault() ?? "❌",*/

                Makkah = _dbContext.ItineraryBlockReservationForMakkah
                    .Any(m => _dbContext.FamilyHeadsWithoutTrash
                        .Where(fm => fm.FamilyHeadId == e.FamilyHeadId)
                        .Select(fm => fm.FamilyHeadId)
                        .Contains(m.FamilyHeadId)) ? "✔️" : "❌",

                Madinah = _dbContext.ItineraryBlockReservationForMadinah
                    .Any(md => _dbContext.FamilyHeadsWithoutTrash
                        .Where(fm => fm.FamilyHeadId == e.FamilyHeadId)
                        .Select(fm => fm.FamilyHeadId)
                        .Contains(md.FamilyHeadId)) ? "✔️" : "❌",

                Azizias = _dbContext.ItineraryBlockReservationForAzizias
                    .Any(a => _dbContext.FamilyHeadsWithoutTrash
                        .Where(fm => fm.FamilyHeadId == e.FamilyHeadId)
                        .Select(fm => fm.FamilyHeadId)
                        .Contains(a.FamilyHeadId)) ? "✔️" : "❌",

                GroupName = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .Where(g => g.FamilyHeadId == e.FamilyHeadId
                            && !g.Group.Trashed)
                    .Select(e => e.Group.Name)
                    .FirstOrDefault(),

                GiveAway = e.DeliveryDate != null ? "✔️" : "❌",

                PNR = _dbContext.PNRMembers
                    .Include(e => e.PNR)
                    .Where(e => !e.PNR.Trashed)
                        .Where(p => p.FamilyHeadId == e.FamilyHeadId)
                    .Select(fm => fm.FamilyHeadId)
                    .Contains(e.FamilyHeadId) ? "✔️" : "❌",

            }).ToList();

            return Json(result);

        }
        public JsonResult TentativeReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == 2 && e.HajjiBookingStatus == HajjiBookingStatus.Tentative);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult ApplicantReport(ApplicantType applicantType)
        {
            ViewBag.Family = _dbContext.FamilyHeadsWithoutTrash
             .Count(e => e.ApplicantType == applicantType
                     && e.HajjSeasonsId == 2
                     && e.HajjiBookingStatus == HajjiBookingStatus.Booked && e.IsHead);

            ViewBag.MaleCount = _dbContext.FamilyHeadsWithoutTrash
                .Count(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == 2
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked && e.Gender == Gender.Male);

            ViewBag.FemaleCount = _dbContext.FamilyHeadsWithoutTrash
                .Count(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == 2
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked && e.Gender == Gender.Female);

            ViewBag.PAX = _dbContext.FamilyHeadsWithoutTrash
                .Count(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == 2
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Countries)
                .Include(e => e.FamilyHeadAddresses)
                .ThenInclude(e => e.Cities)
                .Where(e => e.ApplicantType == applicantType
                        && e.HajjSeasonsId == 2
                        && e.HajjiBookingStatus == HajjiBookingStatus.Booked && e.FamilyHeadAddresses.FirstOrDefault().Preferred);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PassportNo = e.PassportNumber,
                Gender = e.Gender.ToString(),
                CountryName = e.Countries.Name,
                CityName = e.FamilyHeadAddresses.FirstOrDefault().Cities.Name,
                Address = e.FamilyHeadAddresses.FirstOrDefault().Address,
                Number = e.FamilyHeadContacts.FirstOrDefault().Contact
            }).ToList();

            return Json(result);
        }
        public JsonResult NewReport(Gender gender)
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.Gender == gender && e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.HajjSeasonsId == 2);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                Gender = e.Gender.ToString(),
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult AdultReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .AsEnumerable()
                .Where(e => e.HajjSeasonsId == 2 && e.IsAdult && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult ChildReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .AsEnumerable()
                .Where(e => e.HajjSeasonsId == 2 && e.IsChild && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult InfantReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .AsEnumerable()
                .Where(e => e.HajjSeasonsId == 2 && e.IsInfant && e.HajjiBookingStatus == HajjiBookingStatus.Booked);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult GroupIndex()
        {
            ViewBag.TrashCount = _dbContext.Groups
            .Where(e => e.HajjSeasonsId == 2)
            .Where(e => e.Trashed).Count();
            var groups = _dbContext.Groups
                .Where(e => e.HajjSeasonsId == 2)
                .Include(e => e.Package)
                .Where(e => !e.Trashed);
            List<int> selected = new List<int>();
            foreach (var item in groups)
            {
                var groupMembers = _dbContext.GroupMembers.Where(e => e.GroupId == item.GroupId).ToList();
                selected.Add(groupMembers.Count);
            }

            ViewBag.Selected = selected;
            return Json(groups);
        }
        public JsonResult QueriesReport(ApplicantType applicantType)
        {
            var query = _dbContext.CustomerPortfolios
                .Where(e => e.ApplicantType == applicantType && !e.Trashed
                        && e.HajjSeasonsId == 2);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                PassportNo = e.PassportNumber,
                Gender = e.Gender.ToString(),
            }).ToList();

            return Json(result);
        }
        public JsonResult CancelReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.HajjiBookingStatus == HajjiBookingStatus.Cancelled
                        && e.HajjSeasonsId == 2);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                Gender = e.Gender.ToString(),
                PackageName = e.Package.Name,
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult ClientDataReport()
        {
            var query = _dbContext.ClientDatas
                .Where(e => !e.Trashed
                        && e.HajjSeasonsId == 2);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                Gender = e.Gender.ToString(),
                PassportNo = e.PassportNumber
            }).ToList();

            return Json(result);
        }
        public JsonResult OtherServiceReport()
        {
            var extraAdf = _dbContext.ExtraFacilityDescriptionsWithoutTrash;

            List<OtherServices> model = new();

            foreach (var item in extraAdf)
            {
                OtherServices view = new()
                {
                    EAdfId = item.ExtraFacilityDescriptionId,
                    Name = item.Description,
                    Count = _dbContext.FamilyHeadsWithoutTrash
                         .Join(_dbContext.FamilyHeadExtras,
                               fh => fh.FamilyHeadId,
                               fhe => fhe.FamilyHeadId,
                               (fh, fhe) => new { fh, fhe })
                         .Where(joined => joined.fhe.ExtraFacilityDescriptionId == item.ExtraFacilityDescriptionId
                                && joined.fh.HajjSeasonsId == 2)
                         .Count()
                };

                model.Add(view);
            }

            return Json(model);
        }
        public JsonResult GiveAway()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == 2 && e.DeliveryDate != null);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PackageName = e.Package.Name,
                Gender = e.Gender.ToString(),
                PassportNo = e.PassportNumber,
                DeliveryDate = e.DeliveryDate.Value.Date
            }).ToList();

            return Json(result);
        }
        public JsonResult VisaReport()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.HajjSeasonsId == 2 && e.VisaImage != null);

            var result = query.Select(e => new HajjSummaryData
            {
                HajjiName = e.FullName,
                HBNo = e.FcId,
                PassportNo = e.PassportNumber,
                PackageName = e.Package.Name,
                Gender = e.Gender.ToString()
            }).ToList();

            return Json(result);
        }

    }
    public record PackageAPiSummary
    {
       
        public string HajjiName { get; set; }
        public string HBNo { get; set; }
        public string PackageName { get; set; }
        public string PassportNo { get; set; }
        
    }
}


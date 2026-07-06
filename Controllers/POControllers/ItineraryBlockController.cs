using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Packaging;
using System.Text.RegularExpressions;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.AccomodationModels;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.PackageModels;
using TMG.Models.POModels;
using TMG.Models.ReservationModels;
using TMG.Models.TransportationBlock;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.POControllers
{

    public class ItineraryViewModel
    {
        public List<ItineraryBlock> ItineraryBlocks { get; set; }
        public List<int> SelectedMemberCounts { get; set; }
        public List<DateTime?> CheckIn { get; set; }
        public List<DateTime?> CheckOut { get; set; }
        public int TrashCount { get; set; }
    }


    public class ItineraryBlockController : Controller
    {
        public static int GroupId = 0;
        public static int HotelId = 0;
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public ItineraryBlockController(TMGDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _http = contextAccessor;
        }

        public IActionResult Index()
        {
            int activeSeasonId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

            int trashCount = _dbContext.ItineraryBlocks
                .Where(e => !e.Trashed && e.HajjSeasonsId == activeSeasonId)
                .Count();

            var itinerary = _dbContext.ItineraryBlocks
                .Where(e => e.HajjSeasonsId == activeSeasonId && !e.Trashed)
                .Include(e => e.Package)
                .Include(e => e.POHotels)
                    .ThenInclude(e => e.Hotel)
                .Include(e => e.POHotels)
                    .ThenInclude(e => e.Place)
                .ToList();

            List<int> selected = new List<int>();
            List<DateTime?> checkInDates = new List<DateTime?>();
            List<DateTime?> checkOutDates = new List<DateTime?>();

            foreach (var item in itinerary)
            {
                var groupMembers = _dbContext.ItineraryBlockGroupMmebers
                    .Include(g => g.FamilyHead)
                    .Where(e => e.ItineraryBlockId == item.ItineraryBlockId)
                    .ToList();

                selected.Add(groupMembers.Count);

                DateTime? checkIn = null;
                DateTime? checkOut = null;

                var firstMember = groupMembers.FirstOrDefault();
                if (firstMember != null)
                {
                    if (firstMember.FamilyHead.AccomodationAsPerPackage)
                    {
                        var accommodation = _dbContext.PackageAccomodtions
                            .Where(e => e.PackageId == firstMember.FamilyHead.PackageId && e.PlaceId == item.POHotels.PlaceId)
                            .Select(e => new { e.ActualCheckInTime, e.ActualCheckOutTime })
                            .FirstOrDefault();

                        checkIn = accommodation?.ActualCheckInTime;
                        checkOut = accommodation?.ActualCheckOutTime;
                    }
                    else
                    {
                        var accommodation = _dbContext.FamilyHeadAccomodations
                            .Where(e => e.FamilyHeadId == firstMember.FamilyHeadId && e.Hotel.PlaceId == item.POHotels.PlaceId)
                            .Select(e => new { e.CheckIn, e.CheckOut })
                            .FirstOrDefault();

                        checkIn = accommodation?.CheckIn;
                        checkOut = accommodation?.CheckOut;
                    }
                }

                checkInDates.Add(checkIn);
                checkOutDates.Add(checkOut);
            }

            var combinedList = itinerary
                .Select((block, index) => new
                {
                    Block = block,
                    CheckIn = checkInDates[index],
                    CheckOut = checkOutDates[index],
                    MemberCount = selected[index]
                })
                .OrderByDescending(x => x.CheckIn) 
                .ToList();

            var viewModel = new ItineraryViewModel
            {
                ItineraryBlocks = combinedList.Select(x => x.Block).ToList(),
                SelectedMemberCounts = combinedList.Select(x => x.MemberCount).ToList(),
                CheckIn = combinedList.Select(x => x.CheckIn).ToList(),
                CheckOut = combinedList.Select(x => x.CheckOut).ToList(),
                TrashCount = trashCount
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.POHotelCode = _dbContext.POHotelsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();


            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            return View();
        }

        [HttpPost]
        public IActionResult Create(Models.POModels.ItineraryBlock itineraryBlock)
        {
            ViewBag.POHotelCode = _dbContext.POHotelsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();


            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.ItineraryBlocks.Add(itineraryBlock);
                _dbContext.SaveChanges();

                itineraryBlock.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                for (int i = 0; i < (itineraryBlock.PackageIds?.Count ?? 0); i++)
                {
                    if (itineraryBlock.PackagesSelected[i] == false) continue;

                    _dbContext.ItineraryBlockGroups.Add(new ItineraryBlockGroup
                    {
                        PackageId = itineraryBlock.PackageIds[i],
                        ItineraryBlockId = itineraryBlock.ItineraryBlockId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(itineraryBlock);
            }

        }

        [HttpGet]
        public IActionResult CreateForMina()
        {
            ViewBag.POHotelCode = _dbContext.POHotelsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.GenderType = Enum.GetValues(typeof(Gender))
                          .Cast<Gender>()
                          .Select(e => new { Id = (int)e, Name = e.ToString() })
                          .ToList();


            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            return View();
        }


        [HttpPost]
        public IActionResult CreateForMina(Models.POModels.ItineraryBlock itineraryBlock)
        {
            ViewBag.POHotelCode = _dbContext.POHotelsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();


            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash
                    .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );

            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                _dbContext.ItineraryBlocks.Add(itineraryBlock);
                _dbContext.SaveChanges();

                itineraryBlock.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));

                for (int i = 0; i < (itineraryBlock.PackageIds?.Count ?? 0); i++)
                {
                    if (itineraryBlock.PackagesSelected[i] == false) continue;

                    _dbContext.ItineraryBlockGroups.Add(new ItineraryBlockGroup
                    {
                        PackageId = itineraryBlock.PackageIds[i],
                        ItineraryBlockId = itineraryBlock.ItineraryBlockId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(itineraryBlock);
            }

        }

        public IActionResult Edit(int id)
        {
            ViewBag.POHotelCode = _dbContext.POHotelsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.GroupPackages = _dbContext.ItineraryBlockGroups.Where(e => e.ItineraryBlockId == id);
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash.Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))),
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            Models.POModels.ItineraryBlock Group = _dbContext.ItineraryBlocks.Include(e => e.POHotels).FirstOrDefault(e => e.ItineraryBlockId == id);

            return View(Group);
        }

        [HttpPost]
        public IActionResult Edit(Models.POModels.ItineraryBlock group)
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            if (!ModelState.IsValid) return View(group);


            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                group.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                _dbContext.Entry(group).State = EntityState.Modified;

                _dbContext.SaveChanges();
                _dbContext.ItineraryBlockGroups.Where(e => e.ItineraryBlockId == group.ItineraryBlockId).ExecuteDelete();
                for (int i = 0; i < (group.PackageIds?.Count ?? 0); i++)
                {
                    if (group.PackagesSelected[i] == false) continue;

                    _dbContext.ItineraryBlockGroups.Add(new ItineraryBlockGroup
                    {
                        PackageId = group.PackageIds[i],
                        ItineraryBlockId = group.ItineraryBlockId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(group);
            }

            //_dbContext.Entry(group).State = EntityState.Modified;

            //_dbContext.SaveChanges();

            //return RedirectToAction(nameof(Index));
        }

        public IActionResult EditForMina(int id)
        {
            ViewBag.POHotelCode = _dbContext.POHotelsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            ViewBag.GenderType = Enum.GetValues(typeof(Gender))
                         .Cast<Gender>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

            ViewBag.GroupPackages = _dbContext.ItineraryBlockGroups.Where(e => e.ItineraryBlockId == id);
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            Models.POModels.ItineraryBlock Group = _dbContext.ItineraryBlocks.Include(e => e.POHotels).FirstOrDefault(e => e.ItineraryBlockId == id);

            return View(Group);
        }

        [HttpPost]
        public IActionResult EditForMina(Models.POModels.ItineraryBlock group)
        {
            ViewBag.Packages1 = _dbContext.UserCompanies
                .Where(uc => uc.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")))
                .Join(
                    _dbContext.CompaniesWithoutTrash,
                    uc => uc.CompanyId,
                    c => c.CompanyId,
                    (uc, c) => new { uc.UserId, c.CompanyId, c.CompanyName, c.CompanyCode }
                )
                .Join(
                    _dbContext.PackagesWithoutTrash,
                    uc => uc.CompanyId,
                    p => p.CompanyId,
                    (uc, p) => new
                    {
                        uc.UserId,
                        uc.CompanyId,
                        uc.CompanyName,
                        p.Name,
                        p.PackageId,
                        p.Quota,
                        uc.CompanyCode
                    }
                );
            ViewBag.Packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            if (!ModelState.IsValid) return View(group);


            var transaction = _dbContext.Database.BeginTransaction();
            try
            {

                group.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
                _dbContext.Entry(group).State = EntityState.Modified;

                _dbContext.SaveChanges();
                _dbContext.ItineraryBlockGroups.Where(e => e.ItineraryBlockId == group.ItineraryBlockId).ExecuteDelete();
                for (int i = 0; i < (group.PackageIds?.Count ?? 0); i++)
                {
                    if (group.PackagesSelected[i] == false) continue;

                    _dbContext.ItineraryBlockGroups.Add(new ItineraryBlockGroup
                    {
                        PackageId = group.PackageIds[i],
                        ItineraryBlockId = group.ItineraryBlockId
                    });
                }
                _dbContext.SaveChanges();

                transaction.Commit();


                TempData["created"] = true;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return View(group);
            }

        }

        public IActionResult AddMember(int groupId, int PlaceId)
        {
            Models.POModels.ItineraryBlock group = _dbContext.ItineraryBlocks.Include(e => e.POHotels).FirstOrDefault(e => e.ItineraryBlockId == groupId);

            ViewBag.GroupId = groupId;
            ViewBag.GroupCode = group.POHotels.Code;
            //ViewBag.GroupName = group.Name;
            ViewBag.GroupPAX = group.PAX;

            var groupMembers = _dbContext.ItineraryBlockGroupMmebers.Where(e => e.ItineraryBlockId == groupId && !e.ItineraryBlock.Trashed && !e.FamilyHead.Trashed).ToList(); // Fetching group members once

            ViewBag.GroupMembers = groupMembers;
            ViewBag.GroupMembersCount = groupMembers.Count();

            foreach (var currentMember in groupMembers)
            {
                bool? inSameGroup = null;
                var _m = _dbContext.ItineraryBlockGroupMmebers
                    .Where(a => a.FamilyHead.Trashed == false)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                if (_m is null)
                {
                    inSameGroup = null;
                }
                else if (_m.ItineraryBlockId == groupId)
                {
                    inSameGroup = true;
                }
                else if (_m.ItineraryBlockId != groupId)
                {
                    inSameGroup = false;
                }

                ViewBag.InSameGroup = inSameGroup;
            }

            var grouppackages = _dbContext.ItineraryBlockGroups.Where(e => e.ItineraryBlockId == group.ItineraryBlockId);

            List<FamilyMembers> model = new List<FamilyMembers>();
            foreach (var package in grouppackages)
            {
                var familymember = _dbContext.FamilyMembers
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Reference)
                    .Where(e => e.FamilyHead.PackageId == package.PackageId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
                    .OrderBy(e => e.HeadId)
                    .ThenBy(e => e.FamilyHeadId);

                model.AddRange(familymember);
            }


            ViewBag.selectedmemebers = _dbContext.ItineraryBlockGroupMmebers.Where(e => e.ItineraryBlockId != groupId && e.ItineraryBlock.POHotels.PlaceId == PlaceId && !e.ItineraryBlock.Trashed);
            //ViewBag.selectedmemebers = new List<ItineraryBlockGroupMmebers>();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddMembers(int groupId, int memberId, int PlaceId)
        {
            int paxLimit = _dbContext.ItineraryBlocks.FirstOrDefault(e => !e.Trashed && e.ItineraryBlockId == groupId).PAX;
            int selected = _dbContext.ItineraryBlockGroupMmebers.Where(e => e.ItineraryBlockId == groupId).Count();

            var exists = _dbContext.ItineraryBlockGroupMmebers
                .Where(e => e.FamilyHeadId == memberId)
                .Join(_dbContext.ItineraryBlocks, gm => gm.ItineraryBlockId, g => g.ItineraryBlockId, (gm, g) => new { gm, g })
                .FirstOrDefault(x => x.g.Trashed == false && x.g.POHotels.PlaceId == PlaceId);

            if (exists is null)
            {
                _dbContext.ItineraryBlockGroupMmebers.Add(
                    new ItineraryBlockGroupMmebers { FamilyHeadId = memberId, ItineraryBlockId = groupId }
                );

                _dbContext.ItineraryBlocks.FirstOrDefault(e => e.ItineraryBlockId == groupId).Selected = selected + 1;

                _dbContext.SaveChanges();

                return Ok(new { status = 200, message = $"Member Id: {memberId} added to group Id: {groupId}" });
            }

            return Ok(new { status = 200, message = $"Member Id: {memberId} already exists in a valid group" });
        }

        public IActionResult AddMemberForMina(int groupId, int PlaceId , int Gender)
        {
            Models.POModels.ItineraryBlock group = _dbContext.ItineraryBlocks.Include(e => e.POHotels).FirstOrDefault(e => e.ItineraryBlockId == groupId);

            ViewBag.GroupId = groupId;
            ViewBag.GroupCode = group.POHotels.Code;
            //ViewBag.GroupName = group.Name;
            ViewBag.GroupPAX = group.PAX;

            var groupMembers = _dbContext.ItineraryBlockGroupMmebers.Where(e => e.ItineraryBlockId == groupId && !e.ItineraryBlock.Trashed && !e.FamilyHead.Trashed).ToList(); // Fetching group members once

            ViewBag.GroupMembers = groupMembers;
            ViewBag.GroupMembersCount = groupMembers.Count();

            foreach (var currentMember in groupMembers)
            {
                bool? inSameGroup = null;
                var _m = _dbContext.ItineraryBlockGroupMmebers
                    .Where(a => a.FamilyHead.Trashed == false)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                if (_m is null)
                {
                    inSameGroup = null;
                }
                else if (_m.ItineraryBlockId == groupId)
                {
                    inSameGroup = true;
                }
                else if (_m.ItineraryBlockId != groupId)
                {
                    inSameGroup = false;
                }

                ViewBag.InSameGroup = inSameGroup;
            }

            var grouppackages = _dbContext.ItineraryBlockGroups.Where(e => e.ItineraryBlockId == group.ItineraryBlockId);

            List<FamilyMembers> model = new List<FamilyMembers>();
            foreach (var package in grouppackages)
            {
                var familymember = _dbContext.FamilyMembers
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                    .Include(e => e.FamilyHead)
                        .ThenInclude(e => e.Reference)
                    .Where(e => e.FamilyHead.PackageId == package.PackageId && (int)e.FamilyHead.Gender == Gender && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus == HajjiBookingStatus.Booked)
                    .OrderBy(e => e.HeadId)
                    .ThenBy(e => e.FamilyHeadId);

                model.AddRange(familymember);
            }


            ViewBag.selectedmemebers = _dbContext.ItineraryBlockGroupMmebers.Where(e => e.ItineraryBlockId != groupId && e.ItineraryBlock.POHotels.PlaceId == PlaceId && !e.ItineraryBlock.Trashed);
            //ViewBag.selectedmemebers = new List<ItineraryBlockGroupMmebers>();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddMemberForMinaSave(int groupId, int memberId, int PlaceId)
        {
            int paxLimit = _dbContext.ItineraryBlocks.FirstOrDefault(e => !e.Trashed && e.ItineraryBlockId == groupId).PAX;
            int selected = _dbContext.ItineraryBlockGroupMmebers.Where(e => e.ItineraryBlockId == groupId).Count();

            var exists = _dbContext.ItineraryBlockGroupMmebers
                .Where(e => e.FamilyHeadId == memberId)
                .Join(_dbContext.ItineraryBlocks, gm => gm.ItineraryBlockId, g => g.ItineraryBlockId, (gm, g) => new { gm, g })
                .FirstOrDefault(x => x.g.Trashed == false && x.g.POHotels.PlaceId == PlaceId);

            if (exists is null)
            {
                _dbContext.ItineraryBlockGroupMmebers.Add(
                    new ItineraryBlockGroupMmebers { FamilyHeadId = memberId, ItineraryBlockId = groupId }
                );

                _dbContext.ItineraryBlocks.FirstOrDefault(e => e.ItineraryBlockId == groupId).Selected = selected + 1;

                _dbContext.SaveChanges();

                return Ok(new { status = 200, message = $"Member Id: {memberId} added to group Id: {groupId}" });
            }

            return Ok(new { status = 200, message = $"Member Id: {memberId} already exists in a valid group" });
        }

        [HttpPost]
        public IActionResult RemoveMember(int groupId, int memberId)
        {
            int selected = _dbContext.ItineraryBlocks.Where(e => e.ItineraryBlockId == groupId).Count();
            /*var sgm = _dbContext.SubGroupMembers.FirstOrDefault(e => e.FamilyHeadId == memberId && e.SubGroup.GroupId == groupId);
            if (sgm != null)
            {
                _dbContext.SubGroupMembers.Remove(sgm);
            }*/

            var gm = _dbContext.ItineraryBlockGroupMmebers.FirstOrDefault(e => e.ItineraryBlockId == groupId && e.FamilyHeadId == memberId);
            if (gm != null)
            {
                _dbContext.ItineraryBlockGroupMmebers.Remove(gm);
                _dbContext.ItineraryBlocks.FirstOrDefault(e => e.ItineraryBlockId == groupId).Selected = selected - 1;
            }

            _dbContext.SaveChanges();

            return Ok(new { status = 200, message = $"Member Id: {memberId} removed from group Id: {groupId}" });
        }

        public IActionResult GetGroups(int id)
        {
            var groups = _dbContext.ItineraryBlockGroups
                .Include(e => e.ItineraryBlock)
                .Where(e => e.PackageId == id && e.ItineraryBlock.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Select(e => new
                {
                    GroupId = e.ItineraryBlock.ItineraryBlockId,
                    //Name = e.ItineraryBlock.Name
                })
                .ToList();

            return Ok(groups);
        }

        public IActionResult Report(int id, int Groupid, int PlaceId)
        {
            Package package = _dbContext.Packages.Find(id);

            GroupReportModel model = new GroupReportModel
            {
                PackageId = id,
                FamilyMembers = new List<FamilyMemberModel>(),
                PackageName = package.Name,
                PackageCode = package.Code,
                PackageDays = package.Days,
                PackageDuration = package.HajjDuration,
                MedinahArrival = package.MedinahArrival,
                //Quota = package.Quota
            };

            //var members = _context.FamilyMembers
            //    .Include(e => e.Head)
            //    .Include(e => e.FamilyHead)
            //    .ThenInclude(e => e.FamilyHeadRelation)
            //    .Include(e => e.FamilyHead)
            //    .ThenInclude(e => e.Package)
            //    .ThenInclude(e => e.Company)
            //    .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
            //    .OrderBy(e => e.HeadId)
            //    .ToList();

            var members = _dbContext.FamilyMembers
                .Include(e => e.Head)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.FamilyHeadRelation)
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .Join(
                    _dbContext.ItineraryBlockGroupMmebers,
                    f => f.FamilyHeadId,
                    gm => gm.FamilyHeadId,
                    (f, gm) => new { FamilyMember = f, ItineraryBlockGroupMmebers = gm }
                )
                .Where(joinResult => joinResult.FamilyMember.FamilyHead.Trashed == false
                                  && joinResult.ItineraryBlockGroupMmebers.ItineraryBlockId == Groupid)
                .OrderBy(joinResult => joinResult.FamilyMember.HeadId)
                .Select(joinResult => joinResult.FamilyMember)
                .ToList();

            var familiesGroups = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.FamilyHead.PackageId == id && !e.FamilyHead.Trashed)
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

                // Ensure the index x is within the range of familiesGroups
                int familyPax = (x < familiesGroups.Count) ? familiesGroups.ElementAt(x).Count() : 0;

                var memberInGroup = _dbContext.ItineraryBlockGroupMmebers
                    .Include(e => e.ItineraryBlock)
                    .ThenInclude(e => e.POHotels)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                string cId =
                    $"{currentMember.FamilyHead.Package.Company.CompanyCode}.{currentMember.FamilyHead.IndId}" +
                    $".{currentMember.Head.ExtractedFamilyNumber}({familyPax})" +
                    $".{currentMember.FamilyHead.Package.Name}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $"{memberInGroup.ItineraryBlockId}";
                    groupNumber = memberInGroup?.ItineraryBlock?.POHotels.Code ?? "-";
                    //model.PackageName = memberInGroup.ItineraryBlock.Package.Name;
                    model.PackageCode = memberInGroup.ItineraryBlock.POHotels.Code;
                }

                string megaId = currentMember.FamilyHead.ReferenceId == 0 ? "-" : currentMember.FamilyHead.ReferenceId?.ToString();

                // Main Contact & Address
                var memberContact = _dbContext.FamilyHeadContacts
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                var memberAddress = _dbContext.FamilyHeadAddresses
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId && e.Preferred);

                string roomName = "-";

                if (PlaceId == 1)
                {
                    var reservation = _dbContext.ItineraryBlockReservationForMakkah
                        .Include(e => e.RoomDetails)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                    roomName = reservation?.RoomDetails?.Room?.RoomName ?? "-";
                }
                else if (PlaceId == 2)
                {
                    var reservation = _dbContext.ItineraryBlockReservationForMadinah
                        .Include(e => e.RoomDetails)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                    roomName = reservation?.RoomDetails?.Room?.RoomName ?? "-";
                }
                else if (PlaceId == 4)
                {
                    var reservation = _dbContext.ItineraryBlockReservationForAzizias
                        .Include(e => e.FloorRoom)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);

                    roomName = reservation?.FloorRoom?.Room?.RoomName ?? "-";
                }

                /*var memberRoomsType = _dbContext.ItineraryBlockReservationForMadinah
                    .Include(e => e.RoomDetails)
                    .ThenInclude(e => e.Room)
                    .FirstOrDefault(e => e.FamilyHeadId == currentMember.FamilyHeadId);*/

                // Makkah Accommodation
                /*var makkahAccommodation = _dbContext.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 1);

                // Medina Accommodation
                var medinahAccommodation = _dbContext.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 2);

                // Mina Accommodation
                var minaAccommodation = _dbContext.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 3);

                // Azizia Accommodation
                var aziziaAccommodation = _dbContext.PackageAccomodtions
                    .Include(e => e.Room)
                    .FirstOrDefault(e => e.PackageId == id && e.PlaceId == 4);*/

                int? accountId = currentMember.Head.AccountId;

                PaymentVoucher voucher = _dbContext.PaymentVouchersWithoutTrash
                    .FirstOrDefault(e => e.AccountId == accountId);

                float receivableUsd = (voucher != null && voucher.USDROE != 0) ? voucher.Amount / voucher.USDROE : 0;

                var invoices = _dbContext.InvoicesWithoutTrash
                    .Where(e => e.FamilyHeadId == currentMember.HeadId)
                    .Select(e => new { e.InvoiceId, e.FamilyHeadId });

                string invoicesString = string.Join(", ", invoices.Select(e => "R#" + e.InvoiceId));

                if (!string.IsNullOrEmpty(groupNumber)/* && memberInGroup.GroupId == Groupid*/)
                {
                    model.FamilyMembers.Add(
                        new FamilyMemberModel
                        {
                            FamilyHeadId = currentMember.FamilyHeadId,
                            MegaId = megaId,
                            EHajjId = currentMember.FamilyHead.EHajjId,
                            MOFA = currentMember.FamilyHead.MofaNumber,
                            FID = currentMember.Head.FcId,
                            CID = cId,
                            GNumber = groupNumber,
                            FormNumber = currentMember.FamilyHead.FormNumber,
                            Image = currentMember.FamilyHead.MemberImage,
                            PassportImage = currentMember.FamilyHead.PassportImage,
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
                            Makkah = roomName,
                            //Medinah = medinahAccommodation?.Room?.RoomCode,
                            //Azizia = aziziaAccommodation?.Room?.RoomCode,
                            //Mina = minaAccommodation?.Room?.RoomCode,
                            FriendId = currentMember.FamilyHead?.FriendId?.ToString() ?? "-",
                            TotalAmount = "$" + receivableUsd.ToString("N0"),
                            Invoices = invoicesString
                        }
                    );
                }
            }

            int familyCount = 0;
            for (int i = 0; i < familiesGroups.Count; i++)
            {
                familyCount++;
            }

            model.PackageSummary = new GroupSummaryModel
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

            var facilities = _dbContext.AdditionalFacilities
                .Include(e => e.Room)
                .Where(e => e.PackageId == id)
                .ToList();

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = _dbContext.AdditionalFacilities
            .Join(_dbContext.Rooms, af => af.RoomId, r => r.RoomId, (af, r) => new { AdditionalFacility = af, Room = r })
            .Join(_dbContext.FamilyHeads, join1 => join1.AdditionalFacility.PackageId, fh => fh.PackageId, (join1, fh) => new { join1.AdditionalFacility, join1.Room, FamilyHead = fh })
            .Join(_dbContext.ItineraryBlockGroupMmebers, join2 => join2.FamilyHead.FamilyHeadId, gm => gm.FamilyHeadId, (join2, gm) => new { join2.AdditionalFacility, join2.Room, join2.FamilyHead, ItineraryBlockGroupMmeber = gm })
            .Where(join3 => join3.AdditionalFacility.PackageId == id && join3.ItineraryBlockGroupMmeber.ItineraryBlockId == GroupId)
            .GroupBy(join4 => new { join4.AdditionalFacility.RoomId, join4.AdditionalFacility.PlaceId, join4.Room.PAX, join4.Room.RoomCode, join4.Room.RoomName })
            .Select(grouped => new
            {
                RoomId = grouped.Key.RoomId,
                PlaceId = grouped.Key.PlaceId,
                PAX = grouped.Key.PAX,
                RoomCode = grouped.Key.RoomCode,
                RoomName = grouped.Key.RoomName
            })
            .ToList();


            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var famFacilities = _dbContext.FamilyHeadAdditionalFacilities
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

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Report(PackageReportSaveModel request)
        {
            //return Ok(request);

            var families = _dbContext.FamilyHeadsWithoutTrash
                //.Where(e => e.grou == request.PackageId)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var currentMember = families.FirstOrDefault(e => e.FamilyHeadId == request.FamilyHeadIds[i]);
                //currentMember.MofaNumber = request.MOFAs.ElementAt(i) ?? "";
                currentMember.MofaNumber = string.IsNullOrEmpty(request.MOFAs[i]) ? "" : request.MOFAs[i].ToString();
                currentMember.HajjApplicationNumber = request.HajjAppNumbers.ElementAt(i) ?? "";
                currentMember.HajjAppSerialNumber = request.HajjSerials.ElementAt(i) ?? "";
                currentMember.TicketNumber = request.TicketNumbers.ElementAt(i) ?? "";
                currentMember.VisaNumber = request.VisaNumbers.ElementAt(i) ?? "";
                currentMember.EHajjId = request.EHajjIds.ElementAt(i) ?? "";
            }

            _dbContext.SaveChanges();
            return RedirectToAction("Index", "Package");
        }

        public IActionResult AziziaRoomListingSource(int id, int PackageId, int POHotelsId, int PlaceId)
        {
            GroupId = id;


            // Step 1: Fetch distinct RoomDetailsId
            var roomDetailsIds = _dbContext.ItineraryBlockReservationForAzizias
                .Where(e => e.ItineraryBlockId == id)
                .Select(e => e.FloorRoomId)
                .Distinct()
                .ToList();

            var roomAssignments = roomDetailsIds.Select(roomDetailsId =>
            {
                var roomDetail = _dbContext.FloorRoom
                    .Include(e => e.Room)
                    .FirstOrDefault(r => r.FloorRoomId == roomDetailsId);

                if (roomDetail != null)
                {
                    var count = _dbContext.ItineraryBlockReservationForAzizias
                        .Count(e => e.FloorRoomId == roomDetailsId && e.ItineraryBlockId == id);

                    return new RoomAssignment
                    {
                        RoomName = roomDetail.Room.RoomName,
                        Quantity = count / roomDetail.Room.PAX
                    };
                }
                return null;
            }).Where(x => x != null).ToList();

            // Group the RoomAssignments by RoomName and sum the Quantity for duplicates
            var groupedRoomAssignments = roomAssignments
                .GroupBy(x => x.RoomName)
                .Select(g => new RoomAssignment
                {
                    RoomName = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                }).ToList();

            // Assign to ViewBag
            ViewBag.RoomAssignments = groupedRoomAssignments;

            //ViewBag.FloorRooms = _dbContext.FloorRoom.Include(e => e.Room).Where(e => !e.Trashed).ToList();

           /* var rooms = (from fr in _dbContext.FloorRoom
                         join f in _dbContext.Floors on fr.FloorId equals f.FloorId
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join rfm in _dbContext.ItineraryBlockReservationForAzizias on fr.FloorRoomId equals rfm.FloorRoomId into rfmGroup
                         from rfm in rfmGroup.DefaultIfEmpty()
                         where fr.POHotelRooms.POHotels.POHotelsId == POHotelsId
                         select new
                         {
                             fr.FloorRoomId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,  // Ensure RoomName is directly included here
                             r.PAX,
                             ReservationFamilyMemberExists = rfm != null
                         })
                 .AsEnumerable() // In-memory processing to apply custom logic
                 .GroupBy(x => new { x.FloorRoomId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX })
                 .Select(g => new
                 {
                     FloorRoomId = g.Key.FloorRoomId,
                     RoomName = g.Key.RoomName,
                     RoomCode = g.Key.RoomCode,// Make sure RoomName is separately available
                     RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode})", // Can still include as a formatted string
                     Count = g.Count(x => x.ReservationFamilyMemberExists),
                     PAX = g.Key.PAX,
                     Availability = g.Count() == g.Key.PAX ? "Maxed" : "Open"
                 })
                 .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
                 .ToList();

            ViewBag.FloorRooms = rooms;*/


            var rooms = (from fr in _dbContext.FloorRoom
                         join f in _dbContext.Floors on fr.FloorId equals f.FloorId
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join rfm in _dbContext.ItineraryBlockReservationForAzizias
                             on fr.FloorRoomId equals rfm.FloorRoomId into rfmGroup
                         where fr.POHotelRooms.POHotels.POHotelsId == POHotelsId
                         from rfm in rfmGroup.DefaultIfEmpty()
                         select new
                         {
                             fr.FloorRoomId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,
                             r.PAX,
                             ReservationFamilyMemberExists = rfm != null
                         })
                .AsEnumerable()
                .GroupBy(x => new { x.FloorRoomId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX })
                .Select(g => {
                    var assignedCount = g.Count(x => x.ReservationFamilyMemberExists);
                    return new
                    {
                        FloorRoomId = g.Key.FloorRoomId,
                        RoomName = g.Key.RoomName,
                        RoomCode = g.Key.RoomCode,
                        RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode})",
                        Count = assignedCount,
                        PAX = g.Key.PAX,
                        Availability = assignedCount == g.Key.PAX ? "Maxed" : "Open"
                    };
                })
                .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
                .ToList();

            ViewBag.FloorRooms = rooms;


            var group = _dbContext.ItineraryBlocks
                                .Include(e => e.POHotels)
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.ItineraryBlockId,
                                    e.POHotels.Code,
                                    //e.Name,
                                    GroupCode = e.POHotels.Code,
                                    //GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                })
                                .FirstOrDefault(e => e.ItineraryBlockId == id);


            ViewBag.SubGroupName = group?.Code;
            ViewBag.GroupCode = group?.GroupCode;
            //ViewBag.GroupName = group?.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _dbContext.ItineraryBlockGroupMmebers
                        join sg in _dbContext.ItineraryBlocks on gm.ItineraryBlockId equals sg.ItineraryBlockId
                        join po in _dbContext.POHotels on gm.ItineraryBlock.POHotelsId equals po.POHotelsId
                        join g in _dbContext.ItineraryBlocks on sg.ItineraryBlockId equals g.ItineraryBlockId
                        join fh in _dbContext.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where gm.ItineraryBlockId == id && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.ItineraryBlockId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Name,
                            fh.ReferenceId,
                            //GroupName = g.Name,
                            GroupCode = g.POHotels.Code,
                            fm.FamilyHead.PassportImage,
                            fm.FamilyHead.VisaImage,
                            fh.GivenName,
                            fm.FamilyHead.VisaNumber,
                            fm.FamilyHead.FamilyHeadEmails.FirstOrDefault().Email,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckInTime,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckOutTime,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId,
                            fh.CompanyVisas,
                            fh.AccomodationAsPerPackage

                        };

            var result = query.ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.PackageId == group.PackageId);
            var familyHeadAdditionalFacilities = _dbContext
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == group.PackageId);

            // var groupMemberRooms = _context.GroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                string roomType = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == 4 && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                var reservation = _dbContext.ItineraryBlockReservationForMakkah
                        .Include(e => e.RoomDetails)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && e.ItineraryBlockId == id);

                string roomName = reservation?.RoomDetails?.Room?.RoomName ?? "-";

                // string stayRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                // string stayFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                // string connectedRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                // string connectedFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;


                var roomNumberData = _dbContext.ItineraryBlockReservationForAzizias
                                        //.Include(e => e.Floor)
                                        .Include(e => e.FloorRoom)
                                        .ThenInclude(e => e.Room)
                                        .OrderBy(e => e.ItineraryBlockReservationForAziziaId)
                                        .LastOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && e.ItineraryBlockId == id);

                DateTime? CheckInDate = null;
                DateTime? CheckOutDate = null;


               
                if (_.AccomodationAsPerPackage)
                {
                    var accommodation = _dbContext.PackageAccomodtions
                        .Where(e => e.PackageId == _.PackageId && e.PlaceId == PlaceId)
                        .Select(e => new
                        {
                            e.ActualCheckInTime,
                            e.ActualCheckOutTime
                        })
                        .FirstOrDefault();

                    if (accommodation != null)
                    {
                        CheckInDate = accommodation.ActualCheckInTime;
                        CheckOutDate = accommodation.ActualCheckOutTime;
                    }
                }
                else
                {
                    var accommodation = _dbContext.FamilyHeadAccomodations
                        .Where(e => e.FamilyHeadId == _.FamilyHeadId && e.Hotel.PlaceId == PlaceId)
                        .Select(e => new
                        {
                            e.CheckIn,
                            e.CheckOut
                        })
                        .FirstOrDefault();

                    if (accommodation != null)
                    {
                        CheckInDate = accommodation.CheckIn;
                        CheckOutDate = accommodation.CheckOut;
                    }
                }

                string NextHotelName = string.Empty;
                if (_.AccomodationAsPerPackage)
                {
                    var nextAccommodation = _dbContext.PackageAccomodtions
                        .Include(e => e.Hotel)
                        .Where(e => e.PackageId == _.PackageId
                            //&& e.ActualCheckInTime != null
                            //&& CheckOutDate != null
                            //&& e.ActualCheckInTime.Value.Date == CheckOutDate.Value.Date
                            && e.PlaceId == 4)
                        .Select(e => new
                        {
                            e.Hotel.HotelName
                        })
                        .FirstOrDefault();

                    if (nextAccommodation != null)
                    {
                        NextHotelName = nextAccommodation?.HotelName;
                    }
                }
                else
                {
                    var nextAccommodation = _dbContext.FamilyHeadAccomodations
                        .Where(e => e.FamilyHeadId == _.FamilyHeadId
                            //&& e.CheckIn != null
                            //&& CheckOutDate != null
                            //&& e.CheckIn.Value.Date == CheckOutDate.Value.Date
                            && e.Hotel.PlaceId == 4)
                        .Select(e => new
                        {
                            e.Hotel.HotelName
                        })
                        .FirstOrDefault();

                    if (nextAccommodation != null)
                    {
                        NextHotelName = nextAccommodation?.HotelName;
                    }
                }

                model.Members.Add(new RoomListingMember
                {
                    IsHead = _.IsHead,
                    //FloorId = roomNumberData?.FloorId,
                    FloorRoomId = roomNumberData?.FloorRoomId,
                    FamilyHeadId = _.FamilyHeadId,
                    RoomId = roomId,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    RoomType = roomNumberData?.FloorRoom?.Room?.RoomName ?? "-",
                    NextHotelName = NextHotelName != null ? NextHotelName : "N/A",
                    PassportImage = string.IsNullOrEmpty(_.PassportImage) ? null : _.PassportImage.Replace("\\", "/"),
                    PassporFileExtension = string.IsNullOrEmpty(_.PassportImage) ? null : Path.GetExtension(_.PassportImage).ToLower(),
                    VisaImage = string.IsNullOrEmpty(_.VisaImage) ? null : _.VisaImage.Replace("\\", "/"),
                    FileExtension = string.IsNullOrEmpty(_.VisaImage) ? null : Path.GetExtension(_.VisaImage).ToLower(),
                    RoomPax = roomPax ?? 5,
                    VisaNumber = _.VisaNumber,
                    CheckIn = CheckInDate,
                    CheckOut = CheckOutDate,
                    Email = _.Email,
                    FcId = _.FcId,
                    CompanyNames = _.CompanyVisas?.CompanyName ?? "N/A",
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    StayWith = roomNumberData?.StayWith,
                    ConnectedFloorNumber = roomNumberData?.ConnectedFloorNumber,
                    ConnectedRoomNumber = roomNumberData?.ConnectedRoomNumber,
                    //FloorNumber = roomNumberData?.Floor?.FloorNumber.ToString(),
                    RoomNumberPax = roomNumberData?.FloorRoom?.Room?.PAX,
                    RoomNumberWithCode = $"{roomNumberData?.FloorRoom?.RoomNumber} ({roomNumberData?.FloorRoom?.Room?.RoomCode})",
                    PackageId = _.PackageId,
                    GroupName = _dbContext.GroupMembers
                        .Include(e => e.Group)
                        .Where(e => e.FamilyHeadId == _.FamilyHeadId && !e.Group.Trashed)
                        .Select(e => e.Group.Name)
                        .FirstOrDefault()
                });
            }

            // for (int i = 0; i < groupRooms.Count; i++)
            // {
            //     var __ = groupRooms.ElementAt(i);

            //     model.Rooms.Add(new RoomListingRoom
            //     {
            //         RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
            //         Quantity = __.Quantity
            //     });
            // }

            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = group.CompanyName;
            //model.Group = group.GroupName;
            //model.SubGroup = group.Name;

            //=====================================//
            if (model.Members.Count > 0)
            {
                //model.Members.RemoveAll(e=> e.FloorRoomId != null);

                var packageId = model.Members[0].PackageId;
                var placeId = 4; //azizia

                var hotelIds = _dbContext.PackageAccomodtions
                    .Where(pa => pa.PackageId == packageId && pa.PlaceId == placeId)
                    .Select(pa => pa.HotelId)
                    .ToList();

                int? firstHotelId = hotelIds.FirstOrDefault(); // Get the first hotel id if available, otherwise default to null or 0
                HotelId = Convert.ToInt32(firstHotelId);
                var floorNumbers = _dbContext.Floors
                    //.Where(fr => fr.HotelId == firstHotelId && !fr.Trashed)
                    .Where(fr => !fr.Trashed)
                    .Select(fr => fr.FloorNumber.ToString())
                    // .Distinct()
                    .ToList();

                ViewBag.floorNumber = floorNumbers;
                ViewBag.HotelId = firstHotelId;
            }
            else
            {
                ViewBag.floorNumber = null;
                ViewBag.HotelId = null;

            }

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = _dbContext.AdditionalFacilities
                .Join(_dbContext.Rooms, af => af.RoomId, r => r.RoomId, (af, r) => new { AdditionalFacility = af, Room = r })
                .Join(_dbContext.FamilyHeads, join1 => join1.AdditionalFacility.PackageId, fh => fh.PackageId, (join1, fh) => new { join1.AdditionalFacility, join1.Room, FamilyHead = fh })
                .Join(_dbContext.ItineraryBlockGroupMmebers, join2 => join2.FamilyHead.FamilyHeadId, gm => gm.FamilyHeadId, (join2, gm) => new { join2.AdditionalFacility, join2.Room, join2.FamilyHead, ItineraryBlockGroupMmeber = gm })
                .Where(join3 => join3.ItineraryBlockGroupMmeber.ItineraryBlockId == id)
                .GroupBy(join4 => new { join4.AdditionalFacility.RoomId, join4.AdditionalFacility.PlaceId, join4.Room.PAX, join4.Room.RoomCode, join4.Room.RoomName })
                .Select(grouped => new
                {
                    RoomId = grouped.Key.RoomId,
                    PlaceId = grouped.Key.PlaceId,
                    PAX = grouped.Key.PAX,
                    RoomCode = grouped.Key.RoomCode,
                    RoomName = grouped.Key.RoomName
                })
                .ToList();


            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var groupPackages = _dbContext.ItineraryBlockGroups
                                          .Where(e => e.ItineraryBlockId == id)
                                          .Select(e => e.PackageId)
                                          .ToList();

            var famFacilities = _dbContext.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .Join(_dbContext.GroupMembers, e => e.FamilyHeadId, e => e.FamilyHeadId, (fhaf, gm) => new { fhaf, gm })
                    .Where(e => groupPackages.Contains(e.fhaf.AdditionalFacilities.PackageId));


            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MAKKAH_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MEDINA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MASHAIR_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == AZIZIA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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

        public IActionResult MakkahRoomListingSource(int id, int PackageId ,int POHotelsId, int PlaceId)
        {
            GroupId = id;

            // Step 1: Fetch distinct RoomDetailsId
            var roomDetailsIds = _dbContext.ItineraryBlockReservationForMakkah
                .Where(e => e.ItineraryBlockId == id)
                .Select(e => e.RoomDetailsId)
                .Distinct()
                .ToList();

            var roomAssignments = roomDetailsIds.Select(roomDetailsId =>
            {
                var roomDetail = _dbContext.RoomDetails
                    .Include(e => e.Room)
                    .FirstOrDefault(r => r.RoomDetailsId == roomDetailsId);

                if (roomDetail != null)
                {
                    var count = _dbContext.ItineraryBlockReservationForMakkah
                        .Count(e => e.RoomDetailsId == roomDetailsId && e.ItineraryBlockId == id);

                    return new RoomAssignment
                    {
                        RoomName = roomDetail.Room.RoomName,
                        Quantity = count / roomDetail.Room.PAX
                    };
                }
                return null;
            }).Where(x => x != null).ToList();

            // Group the RoomAssignments by RoomName and sum the Quantity for duplicates
            var groupedRoomAssignments = roomAssignments
                .GroupBy(x => x.RoomName)
                .Select(g => new RoomAssignment
                {
                    RoomName = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                }).ToList();

            // Assign to ViewBag
            ViewBag.RoomAssignments = groupedRoomAssignments;

            //ViewBag.FloorRooms = _dbContext.FloorRoom.Include(e => e.Room).Where(e => !e.Trashed).ToList();

            /*var rooms = (from fr in _dbContext.RoomDetails
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join rfm in _dbContext.ItineraryBlockReservationForMakkah on fr.RoomDetailsId equals rfm.RoomDetailsId into rfmGroup
                         where fr.PlaceId == 1 && fr.POHotelsId == POHotelsId
                         from rfm in rfmGroup.DefaultIfEmpty()
                         select new
                         {
                             fr.RoomDetailsId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,
                             r.PAX,
                             ReservationFamilyMemberExists = rfm != null
                         })
                .AsEnumerable()
                .GroupBy(x => new { x.RoomDetailsId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX })
                .Select(g => new
                {
                    FloorRoomId = g.Key.RoomDetailsId,
                    RoomName = g.Key.RoomName,
                    RoomCode = g.Key.RoomCode,
                    RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode})",
                    Count = g.Count(x => x.ReservationFamilyMemberExists),
                    PAX = g.Key.PAX,
                    Availability = g.Count() == g.Key.PAX ? "Maxed" : "Open"
                })
                .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
                .ToList();

            ViewBag.FloorRooms = rooms;*/

            var rooms = (from fr in _dbContext.RoomDetails
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join rfm in _dbContext.ItineraryBlockReservationForMakkah
                             on fr.RoomDetailsId equals rfm.RoomDetailsId into rfmGroup
                         where fr.PlaceId == 1 && fr.POHotelsId == POHotelsId
                         from rfm in rfmGroup.DefaultIfEmpty()
                         select new
                         {
                             fr.RoomDetailsId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,
                             r.PAX,
                             Code = fr.POHotels != null ? fr.POHotels.Code : "",
                             ReservationFamilyMemberExists = rfm != null
                         })
                .AsEnumerable()
                .GroupBy(x => new { x.RoomDetailsId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX, x.Code })
                .Select(g => {
                    var assignedCount = g.Count(x => x.ReservationFamilyMemberExists);
                    return new
                    {
                        FloorRoomId = g.Key.RoomDetailsId,
                        RoomName = g.Key.RoomName,
                        RoomCode = g.Key.RoomCode,
                        RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode}) ({g.Key.Code})",
                        Count = assignedCount,
                        PAX = g.Key.PAX,
                        Availability = assignedCount == g.Key.PAX ? "Maxed" : "Open"
                    };
                })
                .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
                .ToList();

            ViewBag.FloorRooms = rooms;



            var group = _dbContext.ItineraryBlocks
                                .Include(e => e.POHotels)
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.ItineraryBlockId,
                                    e.POHotels.Code,
                                    //e.Name,
                                    GroupCode = e.POHotels.Code,
                                    //GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                })
                                .FirstOrDefault(e => e.ItineraryBlockId == id);


            ViewBag.SubGroupName = group?.Code;
            ViewBag.GroupCode = group?.GroupCode;
            //ViewBag.GroupName = group?.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _dbContext.ItineraryBlockGroupMmebers
                        join sg in _dbContext.ItineraryBlocks on gm.ItineraryBlockId equals sg.ItineraryBlockId
                        join po in _dbContext.POHotels on gm.ItineraryBlock.POHotelsId equals po.POHotelsId
                        join g in _dbContext.ItineraryBlocks on sg.ItineraryBlockId equals g.ItineraryBlockId
                        join fh in _dbContext.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where gm.ItineraryBlockId == id && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.ItineraryBlockId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Name,
                            fh.ReferenceId,
                            //GroupName = g.Name,
                            fm.FamilyHead.PassportImage,
                            fm.FamilyHead.VisaImage,
                            GroupCode = g.POHotels.Code,
                            fm.FamilyHead.VisaNumber,
                            fm.FamilyHead.FamilyHeadEmails.FirstOrDefault().Email,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckInTime,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckOutTime,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId,
                            fh.CompanyVisas
                        };

            var result = query.ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.PackageId == group.PackageId);
            var familyHeadAdditionalFacilities = _dbContext
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == group.PackageId);

            // var groupMemberRooms = _context.GroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                string roomType = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                // string stayRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                // string stayFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                // string connectedRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                // string connectedFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;



                var reservation = _dbContext.ItineraryBlockReservationForMakkah
                        .Include(e => e.RoomDetails)
                        .ThenInclude(e => e.Room)
                        .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && e.ItineraryBlockId == id);

                string roomName = reservation?.RoomDetails?.Room?.RoomName ?? "-";

                var roomNumberData = _dbContext.ItineraryBlockReservationForMakkah
                                        //.Include(e => e.Floor)
                                        .Include(e => e.RoomDetails)
                                        .ThenInclude(e => e.Room)
                                        .OrderBy(e => e.ItineraryBlockReservationForMakkahId)
                                        .LastOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && e.ItineraryBlockId == id);

                model.Members.Add(new RoomListingMember
                {
                    IsHead = _.IsHead,
                    //FloorId = roomNumberData?.FloorId,
                    FloorRoomId = roomNumberData?.RoomDetailsId,
                    FamilyHeadId = _.FamilyHeadId,
                    RoomId = roomId,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    RoomType = roomNumberData?.RoomDetails?.Room?.RoomName ?? "-",
                    RoomPax = roomPax ?? 5,
                    PassportImage = string.IsNullOrEmpty(_.PassportImage) ? null : _.PassportImage.Replace("\\", "/"),
                    PassporFileExtension = string.IsNullOrEmpty(_.PassportImage) ? null : Path.GetExtension(_.PassportImage).ToLower(),
                    VisaImage = string.IsNullOrEmpty(_.VisaImage) ? null : _.VisaImage.Replace("\\", "/"),
                    FileExtension = string.IsNullOrEmpty(_.VisaImage) ? null : Path.GetExtension(_.VisaImage).ToLower(),
                    VisaNumber = _.VisaNumber,
                    CheckIn = _.ActualCheckInTime,
                    CheckOut = _.ActualCheckOutTime,
                    Email = _.Email,
                    FcId = _.FcId,
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    CompanyNames = _.CompanyVisas?.CompanyName ?? "N/A",
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    StayWith = roomNumberData?.StayWith,
                    ConnectedFloorNumber = roomNumberData?.ConnectedFloorNumber,
                    ConnectedRoomNumber = roomNumberData?.ConnectedRoomNumber,
                    //FloorNumber = roomNumberData?.Floor?.FloorNumber.ToString(),
                    RoomNumberPax = roomNumberData?.RoomDetails?.Room?.PAX,
                    RoomNumberWithCode = $"{roomNumberData?.RoomDetails?.RoomNumber} ({roomNumberData?.RoomDetails?.Room?.RoomCode})",
                    PackageId = _.PackageId,
                    GroupName = _dbContext.GroupMembers
                        .Include(e => e.Group)
                        .Where(e => e.FamilyHeadId == _.FamilyHeadId && !e.Group.Trashed)
                        .Select(e => e.Group.Name)
                        .FirstOrDefault()
                });
            }

            // for (int i = 0; i < groupRooms.Count; i++)
            // {
            //     var __ = groupRooms.ElementAt(i);

            //     model.Rooms.Add(new RoomListingRoom
            //     {
            //         RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
            //         Quantity = __.Quantity
            //     });
            // }

            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = group.CompanyName;
            //model.Group = group.GroupName;
            //model.SubGroup = group.Name;

            //=====================================//
            if (model.Members.Count > 0)
            {
                //model.Members.RemoveAll(e=> e.FloorRoomId != null);

                var packageId = model.Members[0].PackageId;
                var placeId = 4; //azizia

                var hotelIds = _dbContext.PackageAccomodtions
                    .Where(pa => pa.PackageId == packageId && pa.PlaceId == placeId)
                    .Select(pa => pa.HotelId)
                    .ToList();

                int? firstHotelId = hotelIds.FirstOrDefault(); // Get the first hotel id if available, otherwise default to null or 0
                HotelId = Convert.ToInt32(firstHotelId);
                var floorNumbers = _dbContext.Floors
                    //.Where(fr => fr.HotelId == firstHotelId && !fr.Trashed)
                    .Where(fr => !fr.Trashed)
                    .Select(fr => fr.FloorNumber.ToString())
                    // .Distinct()
                    .ToList();

                ViewBag.floorNumber = floorNumbers;
                ViewBag.HotelId = firstHotelId;
            }
            else
            {
                ViewBag.floorNumber = null;
                ViewBag.HotelId = null;

            }

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = _dbContext.AdditionalFacilities
                .Join(_dbContext.Rooms, af => af.RoomId, r => r.RoomId, (af, r) => new { AdditionalFacility = af, Room = r })
                .Join(_dbContext.FamilyHeads, join1 => join1.AdditionalFacility.PackageId, fh => fh.PackageId, (join1, fh) => new { join1.AdditionalFacility, join1.Room, FamilyHead = fh })
                .Join(_dbContext.ItineraryBlockGroupMmebers, join2 => join2.FamilyHead.FamilyHeadId, gm => gm.FamilyHeadId, (join2, gm) => new { join2.AdditionalFacility, join2.Room, join2.FamilyHead, ItineraryBlockGroupMmeber = gm })
                .Where(join3 => join3.ItineraryBlockGroupMmeber.ItineraryBlockId == id)
                .GroupBy(join4 => new { join4.AdditionalFacility.RoomId, join4.AdditionalFacility.PlaceId, join4.Room.PAX, join4.Room.RoomCode, join4.Room.RoomName })
                .Select(grouped => new
                {
                    RoomId = grouped.Key.RoomId,
                    PlaceId = grouped.Key.PlaceId,
                    PAX = grouped.Key.PAX,
                    RoomCode = grouped.Key.RoomCode,
                    RoomName = grouped.Key.RoomName
                })
                .ToList();


            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var groupPackages = _dbContext.ItineraryBlockGroups
                                          .Where(e => e.ItineraryBlockId == id)
                                          .Select(e => e.PackageId)
                                          .ToList();

            var famFacilities = _dbContext.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .Join(_dbContext.GroupMembers, e => e.FamilyHeadId, e => e.FamilyHeadId, (fhaf, gm) => new { fhaf, gm })
                    .Where(e => groupPackages.Contains(e.fhaf.AdditionalFacilities.PackageId));


            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MAKKAH_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MEDINA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MASHAIR_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == AZIZIA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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

        public IActionResult MadinahRoomListingSource(int id, int PackageId, int POHotelsId, int PlaceId)
        {
            GroupId = id;

            // Step 1: Fetch distinct RoomDetailsId
            var roomDetailsIds = _dbContext.ItineraryBlockReservationForMadinah
                .Where(e => e.ItineraryBlockId == id)
                .Select(e => e.RoomDetailsId)
                .Distinct()
                .ToList();

            var roomAssignments = roomDetailsIds.Select(roomDetailsId =>
            {
                var roomDetail = _dbContext.RoomDetails
                    .Include(e => e.Room)
                    .FirstOrDefault(r => r.RoomDetailsId == roomDetailsId);

                if (roomDetail != null)
                {
                    var count = _dbContext.ItineraryBlockReservationForMadinah
                        .Count(e => e.RoomDetailsId == roomDetailsId && e.ItineraryBlockId == id);

                    return new RoomAssignment
                    {
                        RoomName = roomDetail.Room.RoomName,
                        Quantity = count / roomDetail.Room.PAX
                    };
                }
                return null;
            }).Where(x => x != null).ToList();

            // Group the RoomAssignments by RoomName and sum the Quantity for duplicates
            var groupedRoomAssignments = roomAssignments
                .GroupBy(x => x.RoomName)
                .Select(g => new RoomAssignment
                {
                    RoomName = g.Key,
                    Quantity = g.Sum(x => x.Quantity) 
                }).ToList();

            // Assign to ViewBag
            ViewBag.RoomAssignments = groupedRoomAssignments;

            //ViewBag.FloorRooms = _dbContext.FloorRoom.Include(e => e.Room).Where(e => !e.Trashed).ToList();

            /*var rooms = (from fr in _dbContext.RoomDetails
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join p in _dbContext.POHotels on fr.POHotelsId equals p.POHotelsId
                         join rfm in _dbContext.ItineraryBlockReservationForMakkah on fr.RoomDetailsId equals rfm.RoomDetailsId into rfmGroup
                         where fr.PlaceId == 2 && fr.POHotelsId == POHotelsId
                         from rfm in rfmGroup.DefaultIfEmpty()
                         select new
                         {
                             fr.RoomDetailsId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,
                             r.PAX,
                             ReservationFamilyMemberExists = rfm != null
                         })
            .AsEnumerable()
            .GroupBy(x => new { x.RoomDetailsId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX })
            .Select(g => new
            {
                FloorRoomId = g.Key.RoomDetailsId,
                RoomName = g.Key.RoomName,
                RoomCode = g.Key.RoomCode,
                RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode})",
                Count = g.Count(x => x.ReservationFamilyMemberExists), // <- fixed
                PAX = g.Key.PAX,
                Availability = g.Count(x => x.ReservationFamilyMemberExists) == g.Key.PAX ? "Maxed" : "Open" // <- fixed
            })
            .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
            .ToList();


            ViewBag.FloorRooms = rooms;*/

            var rooms = (from fr in _dbContext.RoomDetails
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join rfm in _dbContext.ItineraryBlockReservationForMadinah
                             on fr.RoomDetailsId equals rfm.RoomDetailsId into rfmGroup
                         where fr.PlaceId == 2 && fr.POHotelsId == POHotelsId
                         from rfm in rfmGroup.DefaultIfEmpty()
                         select new
                         {
                             fr.RoomDetailsId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,
                             r.PAX,
                             Code = fr.POHotels != null ? fr.POHotels.Code : "",
                             ReservationFamilyMemberExists = rfm != null
                         })
                .AsEnumerable()
                .GroupBy(x => new { x.RoomDetailsId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX, x.Code })
                .Select(g => {
                    var assignedCount = g.Count(x => x.ReservationFamilyMemberExists);
                    return new
                    {
                        FloorRoomId = g.Key.RoomDetailsId,
                        RoomName = g.Key.RoomName,
                        RoomCode = g.Key.RoomCode,
                        RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode}) ({g.Key.Code})",
                        Count = assignedCount,
                        PAX = g.Key.PAX,
                        Availability = assignedCount == g.Key.PAX ? "Maxed" : "Open"
                    };
                })
                .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
                .ToList();

            ViewBag.FloorRooms = rooms;


            var group = _dbContext.ItineraryBlocks
                                .Include(e => e.POHotels)
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.ItineraryBlockId,
                                    e.POHotels.Code,
                                    //e.Name,
                                    GroupCode = e.POHotels.Code,
                                    //GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                })
                                .FirstOrDefault(e => e.ItineraryBlockId == id);


            ViewBag.SubGroupName = group?.Code;
            ViewBag.GroupCode = group?.GroupCode;
            //ViewBag.GroupName = group?.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _dbContext.ItineraryBlockGroupMmebers
                        join sg in _dbContext.ItineraryBlocks on gm.ItineraryBlockId equals sg.ItineraryBlockId
                        join po in _dbContext.POHotels on gm.ItineraryBlock.POHotelsId equals po.POHotelsId
                        join g in _dbContext.ItineraryBlocks on sg.ItineraryBlockId equals g.ItineraryBlockId
                        join fh in _dbContext.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where gm.ItineraryBlockId == id && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.ItineraryBlockId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Name,
                            fh.ReferenceId,
                            fm.FamilyHead.PassportImage,
                            fm.FamilyHead.VisaImage,
                            fm.FamilyHead.VisaNumber,
                            fm.FamilyHead.FamilyHeadEmails.FirstOrDefault().Email,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckInTime,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckOutTime,
                            //GroupName = g.Name,
                            GroupCode = g.POHotels.Code,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId,
                            fh.CompanyVisas
                        };

            var result = query.ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.PackageId == group.PackageId);
            var familyHeadAdditionalFacilities = _dbContext
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == group.PackageId);

            // var groupMemberRooms = _context.GroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                string roomType = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                // string stayRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                // string stayFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                // string connectedRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                // string connectedFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;

                var roomNumberData = _dbContext.ItineraryBlockReservationForMadinah
                                        //.Include(e => e.Floor)
                                        .Include(e => e.RoomDetails)
                                        .ThenInclude(e => e.Room)
                                        .OrderBy(e => e.ItineraryBlockReservationForMadinahId)
                                        .LastOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && e.ItineraryBlockId == id);

                model.Members.Add(new RoomListingMember
                {
                    IsHead = _.IsHead,
                    //FloorId = roomNumberData?.FloorId,
                    FloorRoomId = roomNumberData?.RoomDetailsId,
                    FamilyHeadId = _.FamilyHeadId,
                    RoomId = roomId,
                    PassportImage = string.IsNullOrEmpty(_.PassportImage) ? null : _.PassportImage.Replace("\\", "/"),
                    PassporFileExtension = string.IsNullOrEmpty(_.PassportImage) ? null : Path.GetExtension(_.PassportImage).ToLower(),
                    VisaImage = string.IsNullOrEmpty(_.VisaImage) ? null : _.VisaImage.Replace("\\", "/"),
                    FileExtension = string.IsNullOrEmpty(_.VisaImage) ? null : Path.GetExtension(_.VisaImage).ToLower(),
                    VisaNumber = _.VisaNumber,
                    CheckIn = _.ActualCheckInTime,
                    CheckOut = _.ActualCheckOutTime,
                    Email = _.Email,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    RoomType = roomNumberData?.RoomDetails?.Room?.RoomName ?? "-",
                    RoomPax = roomPax ?? 5,
                    FcId = _.FcId,
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    CompanyNames = _.CompanyVisas?.CompanyName ?? "N/A",
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    StayWith = roomNumberData?.StayWith,
                    ConnectedFloorNumber = roomNumberData?.ConnectedFloorNumber,
                    ConnectedRoomNumber = roomNumberData?.ConnectedRoomNumber,
                    //FloorNumber = roomNumberData?.Floor?.FloorNumber.ToString(),
                    RoomNumberPax = roomNumberData?.RoomDetails?.Room?.PAX,
                    RoomNumberWithCode = $"{roomNumberData?.RoomDetails?.RoomNumber} ({roomNumberData?.RoomDetails?.Room?.RoomCode})",
                    PackageId = _.PackageId,
                    GroupName = _dbContext.GroupMembers
                        .Include(e => e.Group)
                        .Where(e => e.FamilyHeadId == _.FamilyHeadId && !e.Group.Trashed)
                        .Select(e => e.Group.Name)
                        .FirstOrDefault()
                });
            }

            // for (int i = 0; i < groupRooms.Count; i++)
            // {
            //     var __ = groupRooms.ElementAt(i);

            //     model.Rooms.Add(new RoomListingRoom
            //     {
            //         RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
            //         Quantity = __.Quantity
            //     });
            // }

            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = group.CompanyName;
            //model.Group = group.GroupName;
            //model.SubGroup = group.Name;

            //=====================================//
            if (model.Members.Count > 0)
            {
                //model.Members.RemoveAll(e=> e.FloorRoomId != null);

                var packageId = model.Members[0].PackageId;
                var placeId = 4; //azizia

                var hotelIds = _dbContext.PackageAccomodtions
                    .Where(pa => pa.PackageId == packageId && pa.PlaceId == placeId)
                    .Select(pa => pa.HotelId)
                    .ToList();

                int? firstHotelId = hotelIds.FirstOrDefault(); // Get the first hotel id if available, otherwise default to null or 0
                HotelId = Convert.ToInt32(firstHotelId);
                var floorNumbers = _dbContext.Floors
                    //.Where(fr => fr.HotelId == firstHotelId && !fr.Trashed)
                    .Where(fr => !fr.Trashed)
                    .Select(fr => fr.FloorNumber.ToString())
                    // .Distinct()
                    .ToList();

                ViewBag.floorNumber = floorNumbers;
                ViewBag.HotelId = firstHotelId;
            }
            else
            {
                ViewBag.floorNumber = null;
                ViewBag.HotelId = null;

            }

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = _dbContext.AdditionalFacilities
                .Join(_dbContext.Rooms, af => af.RoomId, r => r.RoomId, (af, r) => new { AdditionalFacility = af, Room = r })
                .Join(_dbContext.FamilyHeads, join1 => join1.AdditionalFacility.PackageId, fh => fh.PackageId, (join1, fh) => new { join1.AdditionalFacility, join1.Room, FamilyHead = fh })
                .Join(_dbContext.ItineraryBlockGroupMmebers, join2 => join2.FamilyHead.FamilyHeadId, gm => gm.FamilyHeadId, (join2, gm) => new { join2.AdditionalFacility, join2.Room, join2.FamilyHead, ItineraryBlockGroupMmeber = gm })
                .Where(join3 => join3.ItineraryBlockGroupMmeber.ItineraryBlockId == id)
                .GroupBy(join4 => new { join4.AdditionalFacility.RoomId, join4.AdditionalFacility.PlaceId, join4.Room.PAX, join4.Room.RoomCode, join4.Room.RoomName })
                .Select(grouped => new
                {
                    RoomId = grouped.Key.RoomId,
                    PlaceId = grouped.Key.PlaceId,
                    PAX = grouped.Key.PAX,
                    RoomCode = grouped.Key.RoomCode,
                    RoomName = grouped.Key.RoomName
                })
                .ToList();


            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var groupPackages = _dbContext.ItineraryBlockGroups
                                          .Where(e => e.ItineraryBlockId == id)
                                          .Select(e => e.PackageId)
                                          .ToList();

            var famFacilities = _dbContext.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .Join(_dbContext.GroupMembers, e => e.FamilyHeadId, e => e.FamilyHeadId, (fhaf, gm) => new { fhaf, gm })
                    .Where(e => groupPackages.Contains(e.fhaf.AdditionalFacilities.PackageId));


            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MAKKAH_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MEDINA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MASHAIR_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == AZIZIA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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

        public IActionResult MinaRoomListingSource(int id, int PackageId, int POHotelsId, int PlaceId)
        {
            GroupId = id;

            // Step 1: Fetch distinct RoomDetailsId
            var roomDetailsIds = _dbContext.ItineraryBlockReservationForMinah
                .Where(e => e.ItineraryBlockId == id)
                .Select(e => e.RoomDetailsId)
                .Distinct()
                .ToList();

            var roomAssignments = roomDetailsIds.Select(roomDetailsId =>
            {
                var roomDetail = _dbContext.RoomDetails
                    .Include(e => e.Room)
                    .FirstOrDefault(r => r.RoomDetailsId == roomDetailsId);

                if (roomDetail != null)
                {
                    var count = _dbContext.ItineraryBlockReservationForMinah
                        .Count(e => e.RoomDetailsId == roomDetailsId && e.ItineraryBlockId == id);

                    return new RoomAssignment
                    {
                        RoomName = roomDetail.Room.RoomName,
                        Quantity = count / roomDetail.Room.PAX
                    };
                }
                return null;
            }).Where(x => x != null).ToList();

            // Group the RoomAssignments by RoomName and sum the Quantity for duplicates
            var groupedRoomAssignments = roomAssignments
                .GroupBy(x => x.RoomName)
                .Select(g => new RoomAssignment
                {
                    RoomName = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                }).ToList();

            // Assign to ViewBag
            ViewBag.RoomAssignments = groupedRoomAssignments;

            //ViewBag.FloorRooms = _dbContext.FloorRoom.Include(e => e.Room).Where(e => !e.Trashed).ToList();

            /*var rooms = (from fr in _dbContext.RoomDetails
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join p in _dbContext.POHotels on fr.POHotelsId equals p.POHotelsId
                         join rfm in _dbContext.ItineraryBlockReservationForMakkah on fr.RoomDetailsId equals rfm.RoomDetailsId into rfmGroup
                         where fr.PlaceId == 2 && fr.POHotelsId == POHotelsId
                         from rfm in rfmGroup.DefaultIfEmpty()
                         select new
                         {
                             fr.RoomDetailsId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,
                             r.PAX,
                             ReservationFamilyMemberExists = rfm != null
                         })
            .AsEnumerable()
            .GroupBy(x => new { x.RoomDetailsId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX })
            .Select(g => new
            {
                FloorRoomId = g.Key.RoomDetailsId,
                RoomName = g.Key.RoomName,
                RoomCode = g.Key.RoomCode,
                RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode})",
                Count = g.Count(x => x.ReservationFamilyMemberExists), // <- fixed
                PAX = g.Key.PAX,
                Availability = g.Count(x => x.ReservationFamilyMemberExists) == g.Key.PAX ? "Maxed" : "Open" // <- fixed
            })
            .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
            .ToList();


            ViewBag.FloorRooms = rooms;*/

            var rooms = (from fr in _dbContext.RoomDetails
                         join r in _dbContext.Rooms on fr.RoomId equals r.RoomId
                         join rfm in _dbContext.ItineraryBlockReservationForMinah on fr.RoomDetailsId equals rfm.RoomDetailsId into rfmGroup
                         where fr.PlaceId == 2 && fr.POHotelsId == POHotelsId
                         from rfm in rfmGroup.DefaultIfEmpty()
                         select new
                         {
                             fr.RoomDetailsId,
                             fr.RoomNumber,
                             r.RoomCode,
                             r.RoomName,
                             r.PAX,
                             fr.POHotels.Code,
                             ReservationFamilyMemberExists = rfm != null
                         })
                .AsEnumerable()
                .GroupBy(x => new { x.RoomDetailsId, x.RoomNumber, x.RoomCode, x.RoomName, x.PAX, x.Code })
                .Select(g => new
                {
                    FloorRoomId = g.Key.RoomDetailsId,
                    RoomName = g.Key.RoomName,
                    RoomCode = g.Key.RoomCode,
                    RoomInfo = $"{g.Key.RoomName} ({g.Key.RoomCode}) ({g.Key.Code})",
                    Count = g.Count(x => x.ReservationFamilyMemberExists),
                    PAX = g.Key.PAX,
                    Availability = g.Count() == g.Key.PAX ? "Maxed" : "Open"
                })
                .Where(x => x.Availability == "Open" || x.Availability == "Maxed")
                .ToList();

            ViewBag.FloorRooms = rooms;

            var group = _dbContext.ItineraryBlocks
                                .Include(e => e.POHotels)
                                .Include(e => e.Package)
                                .ThenInclude(e => e.Company)
                                .Select(e => new
                                {
                                    e.ItineraryBlockId,
                                    e.POHotels.Code,
                                    //e.Name,
                                    GroupCode = e.POHotels.Code,
                                    //GroupName = e.Name,
                                    e.Package.Company.CompanyName,
                                    e.PackageId,
                                })
                                .FirstOrDefault(e => e.ItineraryBlockId == id);


            ViewBag.SubGroupName = group?.Code;
            ViewBag.GroupCode = group?.GroupCode;
            //ViewBag.GroupName = group?.GroupName;

            // return Ok(groupRooms);
            RoomListingModel model = new();
            model.GroupId = id;

            var query = from gm in _dbContext.ItineraryBlockGroupMmebers
                        join sg in _dbContext.ItineraryBlocks on gm.ItineraryBlockId equals sg.ItineraryBlockId
                        join po in _dbContext.POHotels on gm.ItineraryBlock.POHotelsId equals po.POHotelsId
                        join g in _dbContext.ItineraryBlocks on sg.ItineraryBlockId equals g.ItineraryBlockId
                        join fh in _dbContext.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where gm.ItineraryBlockId == id && !fh.Trashed
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            gm.ItineraryBlockId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            PackageName = p.Name,
                            fh.ReferenceId,
                            fm.FamilyHead.PassportImage,
                            fm.FamilyHead.VisaImage,
                            fm.FamilyHead.VisaNumber,
                            fm.FamilyHead.FamilyHeadEmails.FirstOrDefault().Email,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckInTime,
                            p.PackageAccomodations.Where(e => e.PlaceId == PlaceId).FirstOrDefault().ActualCheckOutTime,
                            //GroupName = g.Name,
                            GroupCode = g.POHotels.Code,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            fh.StayWithId,
                            fh.Age,
                            fm.Head.FcId,
                            p.PackageId
                        };

            var result = query.ToList();

            var families = _dbContext.FamilyHeadsWithoutTrash.Where(e => e.PackageId == group.PackageId);
            var familyHeadAdditionalFacilities = _dbContext
                                                .FamilyHeadAdditionalFacilities
                                                .Include(e => e.FamilyHead)
                                                .Select(e => new
                                                {
                                                    e.FamilyHeadId,
                                                    e.FamilyHead.PackageId,
                                                    e.AdditionalFacilities.PlaceId,
                                                    e.AdditionalFacilities.Place.PlaceName,
                                                    e.AdditionalFacilities.Room.RoomName,
                                                    e.AdditionalFacilities.Room.RoomCode,
                                                    e.AdditionalFacilities.Room.PAX,
                                                    e.AdditionalFacilities.RoomId

                                                })
                                                .Where(e => e.PackageId == group.PackageId);

            // var groupMemberRooms = _context.GroupMemberRooms.Where(e => e.GroupId == id);
            for (int i = 0; i < result.Count; i++)
            {
                var _ = result.ElementAt(i);
                var stayWith = families
                .Select(e => new
                {
                    e.GivenName,
                    e.SurName,
                    e.FamilyHeadId,
                    e.FcId
                })
                .FirstOrDefault(e => e.FamilyHeadId == _.StayWithId);

                string stayString = "-";
                if (stayWith is not null) stayString = $"{stayWith.GivenName} {stayWith.SurName} ({stayWith.FcId})";

                string roomType = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomName;

                int? roomPax = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.PAX;

                int? roomId = familyHeadAdditionalFacilities
                                .FirstOrDefault(e => e.PlaceId == PlaceId && e.FamilyHeadId == _.FamilyHeadId)?.RoomId;

                // string stayRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.RoomNumber;

                // string stayFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.FloorNumber;

                // string connectedRoomNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedRoomNumber;

                // string connectedFloorNumber = groupMemberRooms
                //     .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId)?.ConnectedFloorNumber;

                var roomNumberData = _dbContext.ItineraryBlockReservationForMinah
                                        //.Include(e => e.Floor)
                                        .Include(e => e.RoomDetails)
                                        .ThenInclude(e => e.Room)
                                        .OrderBy(e => e.ItineraryBlockReservationForMinahId)
                                        .LastOrDefault(e => e.FamilyHeadId == _.FamilyHeadId);

                model.Members.Add(new RoomListingMember
                {
                    IsHead = _.IsHead,
                    //FloorId = roomNumberData?.FloorId,
                    FloorRoomId = roomNumberData?.RoomDetailsId,
                    FamilyHeadId = _.FamilyHeadId,
                    RoomId = roomId,
                    PassportImage = string.IsNullOrEmpty(_.PassportImage) ? null : _.PassportImage.Replace("\\", "/"),
                    PassporFileExtension = string.IsNullOrEmpty(_.PassportImage) ? null : Path.GetExtension(_.PassportImage).ToLower(),
                    VisaImage = string.IsNullOrEmpty(_.VisaImage) ? null : _.VisaImage.Replace("\\", "/"),
                    FileExtension = string.IsNullOrEmpty(_.VisaImage) ? null : Path.GetExtension(_.VisaImage).ToLower(),
                    VisaNumber = _.VisaNumber,
                    CheckIn = _.ActualCheckInTime,
                    CheckOut = _.ActualCheckOutTime,
                    Email = _.Email,
                    SNo = i + 1,
                    Company = _.CompanyCode,
                    Package = _.PackageName,
                    Group = _.GroupCode,
                    RoomType = roomNumberData?.RoomDetails?.Room?.RoomName ?? "-",
                    RoomPax = roomPax ?? 5,
                    FcId = _.FcId,
                    Name = $"{_.GivenName} {_.SurName}",
                    Age = _.Age,
                    Gender = _.Gender.ToString(),
                    Passport = _.PassportNumber,
                    StayWith = roomNumberData?.StayWith,
                    ConnectedFloorNumber = roomNumberData?.ConnectedFloorNumber,
                    ConnectedRoomNumber = roomNumberData?.ConnectedRoomNumber,
                    //FloorNumber = roomNumberData?.Floor?.FloorNumber.ToString(),
                    RoomNumberPax = roomNumberData?.RoomDetails?.Room?.PAX,
                    RoomNumberWithCode = $"{roomNumberData?.RoomDetails?.RoomNumber} ({roomNumberData?.RoomDetails?.Room?.RoomCode})",
                    PackageId = _.PackageId,
                    GroupName = _dbContext.GroupMembers
                        .Include(e => e.Group)
                        .Where(e => e.FamilyHeadId == _.FamilyHeadId)
                        .Select(e => e.Group.Name)
                        .FirstOrDefault()
                });
            }

            // for (int i = 0; i < groupRooms.Count; i++)
            // {
            //     var __ = groupRooms.ElementAt(i);

            //     model.Rooms.Add(new RoomListingRoom
            //     {
            //         RoomCode = $"{__.Room.RoomName} ({__.Room.RoomCode})",
            //         Quantity = __.Quantity
            //     });
            // }

            model.MemberSummary.MaleCount = result.Where(e => e.Gender == Gender.Male).Count();
            model.MemberSummary.AdultMales = result.Where(e => e.Gender == Gender.Male && e.Age >= 12).Count();
            model.MemberSummary.ChildMales = result.Where(e => e.Gender == Gender.Male && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantMales = result.Where(e => e.Gender == Gender.Male && e.Age < 2).Count();

            model.MemberSummary.FemaleCount = result.Where(e => e.Gender == Gender.Female).Count();
            model.MemberSummary.AdultFemales = result.Where(e => e.Gender == Gender.Female && e.Age >= 12).Count();
            model.MemberSummary.ChildFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 12 && e.Age >= 2).Count();
            model.MemberSummary.InfantFemales = result.Where(e => e.Gender == Gender.Female && e.Age < 2).Count();

            model.MemberSummary.TotalMembers = model.MemberSummary.FemaleCount + model.MemberSummary.MaleCount;

            model.Company = group.CompanyName;
            //model.Group = group.GroupName;
            //model.SubGroup = group.Name;

            //=====================================//
            if (model.Members.Count > 0)
            {
                //model.Members.RemoveAll(e=> e.FloorRoomId != null);

                var packageId = model.Members[0].PackageId;
                var placeId = 4; //azizia

                var hotelIds = _dbContext.PackageAccomodtions
                    .Where(pa => pa.PackageId == packageId && pa.PlaceId == placeId)
                    .Select(pa => pa.HotelId)
                    .ToList();

                int? firstHotelId = hotelIds.FirstOrDefault(); // Get the first hotel id if available, otherwise default to null or 0
                HotelId = Convert.ToInt32(firstHotelId);
                var floorNumbers = _dbContext.Floors
                    //.Where(fr => fr.HotelId == firstHotelId && !fr.Trashed)
                    .Where(fr => !fr.Trashed)
                    .Select(fr => fr.FloorNumber.ToString())
                    // .Distinct()
                    .ToList();

                ViewBag.floorNumber = floorNumbers;
                ViewBag.HotelId = firstHotelId;
            }
            else
            {
                ViewBag.floorNumber = null;
                ViewBag.HotelId = null;

            }

            const int MAKKAH_ID = 1, MEDINA_ID = 2, AZIZIA_ID = 4, MASHAIR_ID = 3;
            List<AccommodationRooms> makkahRooms = new();
            List<AccommodationRooms> medinaRooms = new();
            List<AccommodationRooms> aziziaRooms = new();
            List<AccommodationRooms> mashairRooms = new();

            var facilitiesGroupByPlaceAndRoom = _dbContext.AdditionalFacilities
                .Join(_dbContext.Rooms, af => af.RoomId, r => r.RoomId, (af, r) => new { AdditionalFacility = af, Room = r })
                .Join(_dbContext.FamilyHeads, join1 => join1.AdditionalFacility.PackageId, fh => fh.PackageId, (join1, fh) => new { join1.AdditionalFacility, join1.Room, FamilyHead = fh })
                .Join(_dbContext.ItineraryBlockGroupMmebers, join2 => join2.FamilyHead.FamilyHeadId, gm => gm.FamilyHeadId, (join2, gm) => new { join2.AdditionalFacility, join2.Room, join2.FamilyHead, ItineraryBlockGroupMmeber = gm })
                .Where(join3 => join3.ItineraryBlockGroupMmeber.ItineraryBlockId == id)
                .GroupBy(join4 => new { join4.AdditionalFacility.RoomId, join4.AdditionalFacility.PlaceId, join4.Room.PAX, join4.Room.RoomCode, join4.Room.RoomName })
                .Select(grouped => new
                {
                    RoomId = grouped.Key.RoomId,
                    PlaceId = grouped.Key.PlaceId,
                    PAX = grouped.Key.PAX,
                    RoomCode = grouped.Key.RoomCode,
                    RoomName = grouped.Key.RoomName
                })
                .ToList();


            // return Ok(new {
            //     facilitiesGroupByPlaceAndRoom
            // });

            var groupPackages = _dbContext.ItineraryBlockGroups
                                          .Where(e => e.ItineraryBlockId == id)
                                          .Select(e => e.PackageId)
                                          .ToList();

            var famFacilities = _dbContext.FamilyHeadAdditionalFacilities
                    .Include(e => e.AdditionalFacilities)
                    .Join(_dbContext.GroupMembers, e => e.FamilyHeadId, e => e.FamilyHeadId, (fhaf, gm) => new { fhaf, gm })
                    .Where(e => groupPackages.Contains(e.fhaf.AdditionalFacilities.PackageId));


            for (int i = 0; i < facilitiesGroupByPlaceAndRoom.Count; i++)
            {
                var _currFac = facilitiesGroupByPlaceAndRoom.ElementAt(i);

                if (_currFac.PlaceId == MAKKAH_ID)
                {
                    int totalMembers = famFacilities
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MAKKAH_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MEDINA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == MASHAIR_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
                                        .Where(e => e.fhaf.AdditionalFacilities.PlaceId == AZIZIA_ID &&
                                                e.fhaf.AdditionalFacilities.RoomId == _currFac.RoomId)?
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
        public IActionResult Save(int id, RoomSaveModel request)
        {

            request.FamilyHeadIds ??= new List<int>();
            request.StayRoomNumbers ??= new List<int?>();
            request.StayFloorNumbers ??= new List<string>();
            request.ConnectedFloorNumbers ??= new List<string>();
            request.ConnectedRoomNumbers ??= new List<string>();


            // Step 1: Get all existing records for this ItineraryBlockId
            var existingRecords = _dbContext.ItineraryBlockReservationForAzizias
                .Where(x => x.ItineraryBlockId == id)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                //var familyHeadId = request.FamilyHeadIds[i];
                //var floorRoomid = (i < request.StayRoomNumbers.Count) ? request.StayRoomNumbers[i] : null;
                //var floorRoomid = request.StayRoomNumbers[i];
                //var floorRoomid = (request.StayRoomNumbers != null && i < request.StayRoomNumbers.Count && request.StayRoomNumbers[i] != null)
                //    ? request.StayRoomNumbers[i]
                //    : null;



                /*string StayWith = (request.StayFloorNumbers != null && i < request.StayFloorNumbers.Count && request.StayFloorNumbers[i] != null)
                    ? request.StayFloorNumbers[i].ToString()
                    : null;

                string ConnectedFloorNumber = (request.ConnectedFloorNumbers != null && i < request.ConnectedFloorNumbers.Count && request.ConnectedFloorNumbers[i] != null)
                    ? request.ConnectedFloorNumbers[i].ToString()
                    : null;

                string ConnectedRoomNumber = (request.ConnectedRoomNumbers != null && i < request.ConnectedRoomNumbers.Count && request.ConnectedRoomNumbers[i] != null)
                    ? request.ConnectedRoomNumbers[i].ToString()
                    : null;*/

                var familyHeadId = request.FamilyHeadIds[i];
                var floorRoomid = (i < request.StayRoomNumbers.Count) ? request.StayRoomNumbers[i] : null;
                var StayWith = (i < request.StayFloorNumbers.Count) ? request.StayFloorNumbers[i] : null;
                var ConnectedFloorNumber = (i < request.ConnectedFloorNumbers.Count) ? request.ConnectedFloorNumbers[i] : null;
                var ConnectedRoomNumber = (i < request.ConnectedRoomNumbers.Count) ? request.ConnectedRoomNumbers[i] : null;


                var existing = existingRecords.FirstOrDefault(x => x.FamilyHeadId == familyHeadId);

                if (!floorRoomid.HasValue)
                {
                    // If no room selected, and record exists, then delete it
                    if (existing != null)
                        _dbContext.ItineraryBlockReservationForAzizias.Remove(existing);

                    continue;
                }

                if (existing != null)
                {
                    // Update existing record
                    existing.FloorRoomId = floorRoomid.Value;
                    existing.StayWith = StayWith;
                    existing.ConnectedFloorNumber = ConnectedFloorNumber;
                    existing.ConnectedRoomNumber = ConnectedRoomNumber;
                }
                else
                {
                    // Insert new record
                    _dbContext.ItineraryBlockReservationForAzizias.Add(new ItineraryBlockReservationForAzizia
                    {
                        FamilyHeadId = familyHeadId,
                        FloorRoomId = floorRoomid.Value,
                        StayWith = StayWith,
                        ConnectedFloorNumber = ConnectedFloorNumber,
                        ConnectedRoomNumber = ConnectedRoomNumber,
                        ItineraryBlockId = id
                    });
                }
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SaveRoomAssignment(int id, int familyHeadId, int? floorRoomId, string stayWith, string connectedFloorNumber, string connectedRoomNumber)
        {
            var existing = _dbContext.ItineraryBlockReservationForAzizias
                .FirstOrDefault(x => x.ItineraryBlockId == id && x.FamilyHeadId == familyHeadId);

            if (floorRoomId == null)
            {
                if (existing != null)
                    _dbContext.ItineraryBlockReservationForAzizias.Remove(existing);

                _dbContext.SaveChanges();
                return Ok();
            }

            if (existing != null)
            {
                existing.FloorRoomId = floorRoomId.Value;
                existing.StayWith = stayWith;
                existing.ConnectedFloorNumber = connectedFloorNumber;
                existing.ConnectedRoomNumber = connectedRoomNumber;
            }
            else
            {
                _dbContext.ItineraryBlockReservationForAzizias.Add(new ItineraryBlockReservationForAzizia
                {
                    FamilyHeadId = familyHeadId,
                    FloorRoomId = floorRoomId.Value,
                    StayWith = stayWith,
                    ConnectedFloorNumber = connectedFloorNumber,
                    ConnectedRoomNumber = connectedRoomNumber,
                    ItineraryBlockId = id
                });
            }

            _dbContext.SaveChanges();
            return Ok();
        }


        [HttpPost]
        public IActionResult SaveForMakkah(int id, RoomSaveModel request)
        {
            // Step 1: Get all existing records for this ItineraryBlockId
            var existingRecords = _dbContext.ItineraryBlockReservationForMakkah
                .Where(x => x.ItineraryBlockId == id)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var familyHeadId = request.FamilyHeadIds[i];
                var floorRoomid = request.StayRoomNumbers[i];
                string StayWith = (request.StayFloorNumbers != null && i < request.StayFloorNumbers.Count && request.StayFloorNumbers[i] != null)
                    ? request.StayFloorNumbers[i].ToString()
                    : null;

                string ConnectedFloorNumber = (request.ConnectedFloorNumbers != null && i < request.ConnectedFloorNumbers.Count && request.ConnectedFloorNumbers[i] != null)
                    ? request.ConnectedFloorNumbers[i].ToString()
                    : null;

                string ConnectedRoomNumber = (request.ConnectedRoomNumbers != null && i < request.ConnectedRoomNumbers.Count && request.ConnectedRoomNumbers[i] != null)
                    ? request.ConnectedRoomNumbers[i].ToString()
                    : null;

                var existing = existingRecords.FirstOrDefault(x => x.FamilyHeadId == familyHeadId);

                if (!floorRoomid.HasValue)
                {
                    // If no room selected, and record exists, then delete it
                    if (existing != null)
                        _dbContext.ItineraryBlockReservationForMakkah.Remove(existing);

                    continue;
                }

                if (existing != null)
                {
                    // Update existing record
                    existing.RoomDetailsId = floorRoomid.Value;
                    existing.StayWith = StayWith;
                    existing.ConnectedFloorNumber = ConnectedFloorNumber;
                    existing.ConnectedRoomNumber = ConnectedRoomNumber;
                }
                else
                {
                    // Insert new record
                    _dbContext.ItineraryBlockReservationForMakkah.Add(new ItineraryBlockReservationForMakkah
                    {
                        FamilyHeadId = familyHeadId,
                        RoomDetailsId = floorRoomid.Value,
                        StayWith = StayWith,
                        ConnectedFloorNumber = ConnectedFloorNumber,
                        ConnectedRoomNumber = ConnectedRoomNumber,
                        ItineraryBlockId = id
                    });
                }
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SaveForMadinah(int id, RoomSaveModel request)
        {
            // Step 1: Get all existing records for this ItineraryBlockId
            var existingRecords = _dbContext.ItineraryBlockReservationForMadinah
                .Where(x => x.ItineraryBlockId == id)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var familyHeadId = request.FamilyHeadIds[i];
                var floorRoomid = request.StayRoomNumbers[i];
                string StayWith = (request.StayFloorNumbers != null && i < request.StayFloorNumbers.Count && request.StayFloorNumbers[i] != null)
                    ? request.StayFloorNumbers[i].ToString()
                    : null;

                string ConnectedFloorNumber = (request.ConnectedFloorNumbers != null && i < request.ConnectedFloorNumbers.Count && request.ConnectedFloorNumbers[i] != null)
                    ? request.ConnectedFloorNumbers[i].ToString()
                    : null;

                string ConnectedRoomNumber = (request.ConnectedRoomNumbers != null && i < request.ConnectedRoomNumbers.Count && request.ConnectedRoomNumbers[i] != null)
                    ? request.ConnectedRoomNumbers[i].ToString()
                    : null;

                var existing = existingRecords.FirstOrDefault(x => x.FamilyHeadId == familyHeadId);

                if (!floorRoomid.HasValue)
                {
                    // If no room selected, and record exists, then delete it
                    if (existing != null)
                        _dbContext.ItineraryBlockReservationForMadinah.Remove(existing);

                    continue;
                }

                if (existing != null)
                {
                    // Update existing record
                    existing.RoomDetailsId = floorRoomid.Value;
                    existing.StayWith = StayWith;
                    existing.ConnectedFloorNumber = ConnectedFloorNumber;
                    existing.ConnectedRoomNumber = ConnectedRoomNumber;
                }
                else
                {
                    // Insert new record
                    _dbContext.ItineraryBlockReservationForMadinah.Add(new ItineraryBlockReservationForMadinah
                    {
                        FamilyHeadId = familyHeadId,
                        RoomDetailsId = floorRoomid.Value,
                        StayWith = StayWith,
                        ConnectedFloorNumber = ConnectedFloorNumber,
                        ConnectedRoomNumber = ConnectedRoomNumber,
                        ItineraryBlockId = id
                    });
                }
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SaveForMinah(int id, RoomSaveModel request)
        {
            // Step 1: Get all existing records for this ItineraryBlockId
            var existingRecords = _dbContext.ItineraryBlockReservationForMinah
                .Where(x => x.ItineraryBlockId == id)
                .ToList();

            for (int i = 0; i < request.FamilyHeadIds.Count; i++)
            {
                var familyHeadId = request.FamilyHeadIds[i];
                var floorRoomid = request.StayRoomNumbers[i];
                string StayWith = (request.StayFloorNumbers != null && i < request.StayFloorNumbers.Count && request.StayFloorNumbers[i] != null)
                    ? request.StayFloorNumbers[i].ToString()
                    : null;

                string ConnectedFloorNumber = (request.ConnectedFloorNumbers != null && i < request.ConnectedFloorNumbers.Count && request.ConnectedFloorNumbers[i] != null)
                    ? request.ConnectedFloorNumbers[i].ToString()
                    : null;

                string ConnectedRoomNumber = (request.ConnectedRoomNumbers != null && i < request.ConnectedRoomNumbers.Count && request.ConnectedRoomNumbers[i] != null)
                    ? request.ConnectedRoomNumbers[i].ToString()
                    : null;

                var existing = existingRecords.FirstOrDefault(x => x.FamilyHeadId == familyHeadId);

                if (!floorRoomid.HasValue)
                {
                    // If no room selected, and record exists, then delete it
                    if (existing != null)
                        _dbContext.ItineraryBlockReservationForMinah.Remove(existing);

                    continue;
                }

                if (existing != null)
                {
                    // Update existing record
                    existing.RoomDetailsId = floorRoomid.Value;
                    existing.StayWith = StayWith;
                    existing.ConnectedFloorNumber = ConnectedFloorNumber;
                    existing.ConnectedRoomNumber = ConnectedRoomNumber;
                }
                else
                {
                    // Insert new record
                    _dbContext.ItineraryBlockReservationForMinah.Add(new ItineraryBlockReservationForMinah
                    {
                        FamilyHeadId = familyHeadId,
                        RoomDetailsId = floorRoomid.Value,
                        StayWith = StayWith,
                        ConnectedFloorNumber = ConnectedFloorNumber,
                        ConnectedRoomNumber = ConnectedRoomNumber,
                        ItineraryBlockId = id
                    });
                }
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult UnAllocatedMember(int id)
        {
            List<GroupUnAllocatedMembersModel> model = new();

            var members = _dbContext.ItineraryBlockGroupMmebers
                            .Select(e => new {
                                FHTrashed = e.FamilyHead.Trashed,
                                GroupTrashed = e.ItineraryBlock.Trashed,
                                e.FamilyHead,
                                e.FamilyHeadId,
                                e.ItineraryBlockId,
                                e.FamilyHead.FcId,
                                e.FamilyHead.FullName,
                                e.FamilyHead.Gender,
                                e.ItineraryBlock.POHotels.Hotel.HotelName,
                                e.FamilyHead.Package.Company.CompanyCode,
                                PackageName = e.FamilyHead.Package.Name,
                                e.FamilyHead.IndId,
                                e.FamilyHead.ExtractedFamilyNumber

                            })
                            .Where(e => !e.FHTrashed && !e.GroupTrashed && e.ItineraryBlockId == id
                                    && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .AsNoTracking()
                            .ToList();

            int i = 1;
            foreach (var m in members)
            {
                if (_dbContext.ItineraryBlockReservationForAzizias
                    .AsNoTracking()
                    .Select(e => new {
                        e.FamilyHeadId,
                        e.ItineraryBlockReservationForAziziaId,
                        e.FloorRoomId,
                        e.ItineraryBlockId
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id)?.FloorRoomId != null)
                {
                    continue;
                }

                GroupUnAllocatedMembersModel _ = new()
                {
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    SrNo = i++,
                    GroupName = m.HotelName,
                };

                int headId = _dbContext.FamilyMembers
                    .Select(e => new {
                        e.FamilyHeadId,
                        e.HeadId
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId).HeadId;
                int familyPax = _dbContext.FamilyMembers
                    .Select(e => new {
                        e.HeadId
                    })
                    .AsNoTracking()
                    .Where(e => e.HeadId == headId).Count();
                string cId =
                        $"{m.CompanyCode}.{m.IndId}" +
                        $".{m?.ExtractedFamilyNumber}({familyPax})" ;

                var memberInGroup = _dbContext.ItineraryBlockGroupMmebers
                    .Select(e => new {
                        e.FamilyHeadId,
                        //e.Group.Name,
                        e.ItineraryBlockId,
                        e.ItineraryBlock.Trashed
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id && !e.Trashed);
                if (memberInGroup != null)
                {
                    cId += $"{memberInGroup.ItineraryBlockId}";
                }


                _.CID = cId;
                /* _.RoomType = _dbContext.FamilyHeadAdditionalFacilities
                                 .Select(e => new {
                                     e.FamilyHeadId,
                                     e.AdditionalFacilitiesId,
                                     e.AdditionalFacilities.PlaceId,
                                     e.AdditionalFacilities.Room.RoomCode
                                 })
                                 .AsNoTracking()
                                 .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId
                                 && e.PlaceId == 4)?
                                 .RoomCode ?? "Sharing";*/

                var familyHead = _dbContext.FamilyHeads
                    .AsNoTracking()
                    .FirstOrDefault(fh => fh.FamilyHeadId == m.FamilyHeadId);

                if (familyHead?.AccomodationAsPerPackage == true)
                {
                    // If SameAsPerPackage is true, get RoomType from PackageAccomodations
                    _.RoomType = _dbContext.PackageAccomodtions
                        .Where(p => p.PackageId == familyHead.PackageId && p.PlaceId == 1)
                        .Select(p => p.Package.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }
                else
                {
                    // Else get RoomType from FamilyHeadAccomodations
                    _.RoomType = _dbContext.FamilyHeadAccomodations
                        .Where(fha => fha.FamilyHeadId == m.FamilyHeadId && fha.Hotel.PlaceId == 1)
                        .Select(fha => fha.IndividualFacilityDescription.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }

                model.Add(_);

            }


            // return Ok(model);
            return View(model);
        }

        public IActionResult UnAllocatedMemberForMakkah(int id)
        {
            List<GroupUnAllocatedMembersModel> model = new();

            var members = _dbContext.ItineraryBlockGroupMmebers
                            .Select(e => new {
                                FHTrashed = e.FamilyHead.Trashed,
                                GroupTrashed = e.ItineraryBlock.Trashed,
                                e.FamilyHead,
                                e.FamilyHeadId,
                                e.ItineraryBlockId,
                                e.FamilyHead.FcId,
                                e.FamilyHead.FullName,
                                e.FamilyHead.Gender,
                                e.ItineraryBlock.POHotels.Hotel.HotelName,
                                e.FamilyHead.Package.Company.CompanyCode,
                                PackageName = e.FamilyHead.Package.Name,
                                e.FamilyHead.IndId,
                                e.FamilyHead.ExtractedFamilyNumber

                            })
                            .Where(e => !e.FHTrashed && !e.GroupTrashed && e.ItineraryBlockId == id
                                    && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .AsNoTracking()
                            .ToList();

            int i = 1;
            foreach (var m in members)
            {
                if (_dbContext.ItineraryBlockReservationForMakkah
                    .AsNoTracking()
                    .Select(e => new {
                        e.FamilyHeadId,
                        e.ItineraryBlockReservationForMakkahId,
                        e.RoomDetailsId,
                        e.ItineraryBlockId
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id)?.RoomDetailsId != null)
                {
                    continue;
                }

                GroupUnAllocatedMembersModel _ = new()
                {
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    SrNo = i++,
                    GroupName = m.HotelName,
                };

                int headId = _dbContext.FamilyMembers
                    .Select(e => new {
                        e.FamilyHeadId,
                        e.HeadId
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId).HeadId;
                int familyPax = _dbContext.FamilyMembers
                    .Select(e => new {
                        e.HeadId
                    })
                    .AsNoTracking()
                    .Where(e => e.HeadId == headId).Count();
                string cId =
                        $"{m.CompanyCode}.{m.IndId}" +
                        $".{m?.ExtractedFamilyNumber}({familyPax})";

                var memberInGroup = _dbContext.ItineraryBlockGroupMmebers
                    .Select(e => new {
                        e.FamilyHeadId,
                        //e.Group.Name,
                        e.ItineraryBlockId,
                        e.ItineraryBlock.Trashed
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id && !e.Trashed);
                if (memberInGroup != null)
                {
                    cId += $"{memberInGroup.ItineraryBlockId}";
                }


                _.CID = cId;
                /* _.RoomType = _dbContext.FamilyHeadAccomodations
                                 .Select(e => new {
                                     e.FamilyHeadId,
                                     e.FamilyHeadAccomodationId,
                                     e.Hotel.PlaceId,
                                     e.IndividualFacilityDescription.Room.RoomCode
                                 })
                                 .AsNoTracking()
                                 .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId
                                 && e.PlaceId == 1)?
                                 .RoomCode ?? "Sharing";*/

                var familyHead = _dbContext.FamilyHeads
                    .AsNoTracking()
                    .FirstOrDefault(fh => fh.FamilyHeadId == m.FamilyHeadId);

                if (familyHead?.AccomodationAsPerPackage == true)
                {
                    // If SameAsPerPackage is true, get RoomType from PackageAccomodations
                    _.RoomType = _dbContext.PackageAccomodtions
                        .Where(p => p.PackageId == familyHead.PackageId && p.PlaceId == 1)
                        .Select(p => p.Package.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }
                else
                {
                    // Else get RoomType from FamilyHeadAccomodations
                    _.RoomType = _dbContext.FamilyHeadAccomodations
                        .Where(fha => fha.FamilyHeadId == m.FamilyHeadId && fha.Hotel.PlaceId == 1)
                        .Select(fha => fha.IndividualFacilityDescription.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }

                model.Add(_);

            }


            // return Ok(model);
            return View(model);
        }

        public IActionResult UnAllocatedMemberForMadinah(int id)
        {
            List<GroupUnAllocatedMembersModel> model = new();

            var members = _dbContext.ItineraryBlockGroupMmebers
                            .Select(e => new
                            {
                                FHTrashed = e.FamilyHead.Trashed,
                                GroupTrashed = e.ItineraryBlock.Trashed,
                                e.FamilyHead,
                                e.FamilyHeadId,
                                e.ItineraryBlockId,
                                e.FamilyHead.FcId,
                                e.FamilyHead.FullName,
                                e.FamilyHead.Gender,
                                e.ItineraryBlock.POHotels.Hotel.HotelName,
                                e.FamilyHead.Package.Company.CompanyCode,
                                PackageName = e.FamilyHead.Package.Name,
                                e.FamilyHead.IndId,
                                e.FamilyHead.ExtractedFamilyNumber

                            })
                            .Where(e => !e.FHTrashed && !e.GroupTrashed && e.ItineraryBlockId == id
                                    && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .AsNoTracking()
                            .ToList();

            int i = 1;
            foreach (var m in members)
            {
                if (_dbContext.ItineraryBlockReservationForMadinah
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.ItineraryBlockReservationForMadinahId,
                        e.RoomDetailsId,
                        e.ItineraryBlockId
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id)?.RoomDetailsId != null)
                {
                    continue;
                }

                GroupUnAllocatedMembersModel _ = new()
                {
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    SrNo = i++,
                    GroupName = m.HotelName,
                };

                int headId = _dbContext.FamilyMembers
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.HeadId
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId).HeadId;
                int familyPax = _dbContext.FamilyMembers
                    .Select(e => new
                    {
                        e.HeadId
                    })
                    .AsNoTracking()
                    .Where(e => e.HeadId == headId).Count();
                string cId =
                        $"{m.CompanyCode}.{m.IndId}" +
                        $".{m?.ExtractedFamilyNumber}({familyPax})";

                var memberInGroup = _dbContext.ItineraryBlockGroupMmebers
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        //e.Group.Name,
                        e.ItineraryBlockId,
                        e.ItineraryBlock.Trashed
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id && !e.Trashed);
                if (memberInGroup != null)
                {
                    cId += $"{memberInGroup.ItineraryBlockId}";
                }


                _.CID = cId;
                /*_.RoomType = _dbContext.FamilyHeadAdditionalFacilities
                                .Select(e => new
                                {
                                    e.FamilyHeadId,
                                    e.AdditionalFacilitiesId,
                                    e.AdditionalFacilities.PlaceId,
                                    e.AdditionalFacilities.Room.RoomCode
                                })
                                .AsNoTracking()
                                .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId
                                && e.PlaceId == 1)?
                                .RoomCode ?? "Sharing";*/

                // Get the FamilyHead record first
                var familyHead = _dbContext.FamilyHeads
                    .AsNoTracking()
                    .FirstOrDefault(fh => fh.FamilyHeadId == m.FamilyHeadId);

                if (familyHead?.AccomodationAsPerPackage == true)
                {
                    // If SameAsPerPackage is true, get RoomType from PackageAccomodations
                    _.RoomType = _dbContext.PackageAccomodtions
                        .Where(p => p.PackageId == familyHead.PackageId && p.PlaceId == 1)
                        .Select(p => p.Package.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }
                else
                {
                    // Else get RoomType from FamilyHeadAccomodations
                    _.RoomType = _dbContext.FamilyHeadAccomodations
                        .Where(fha => fha.FamilyHeadId == m.FamilyHeadId && fha.Hotel.PlaceId == 1)
                        .Select(fha => fha.IndividualFacilityDescription.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }


                model.Add(_);

            }


            // return Ok(model);
            return View(model);
        }

        public IActionResult UnAllocatedMemberForMinah(int id)
        {
            List<GroupUnAllocatedMembersModel> model = new();

            var members = _dbContext.ItineraryBlockGroupMmebers
                            .Select(e => new
                            {
                                FHTrashed = e.FamilyHead.Trashed,
                                GroupTrashed = e.ItineraryBlock.Trashed,
                                e.FamilyHead,
                                e.FamilyHeadId,
                                e.ItineraryBlockId,
                                e.FamilyHead.FcId,
                                e.FamilyHead.FullName,
                                e.FamilyHead.Gender,
                                e.ItineraryBlock.POHotels.Hotel.HotelName,
                                e.FamilyHead.Package.Company.CompanyCode,
                                PackageName = e.FamilyHead.Package.Name,
                                e.FamilyHead.IndId,
                                e.FamilyHead.ExtractedFamilyNumber

                            })
                            .Where(e => !e.FHTrashed && !e.GroupTrashed && e.ItineraryBlockId == id
                                    && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .AsNoTracking()
                            .ToList();

            int i = 1;
            foreach (var m in members)
            {
                if (_dbContext.ItineraryBlockReservationForMinah
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.ItineraryBlockReservationForMinahId,
                        e.RoomDetailsId,
                        e.ItineraryBlockId
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id)?.RoomDetailsId != null)
                {
                    continue;
                }

                GroupUnAllocatedMembersModel _ = new()
                {
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    SrNo = i++,
                    GroupName = m.HotelName,
                };

                int headId = _dbContext.FamilyMembers
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.HeadId
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId).HeadId;
                int familyPax = _dbContext.FamilyMembers
                    .Select(e => new
                    {
                        e.HeadId
                    })
                    .AsNoTracking()
                    .Where(e => e.HeadId == headId).Count();
                string cId =
                        $"{m.CompanyCode}.{m.IndId}" +
                        $".{m?.ExtractedFamilyNumber}({familyPax})";

                var memberInGroup = _dbContext.ItineraryBlockGroupMmebers
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        //e.Group.Name,
                        e.ItineraryBlockId,
                        e.ItineraryBlock.Trashed
                    })
                    .AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.ItineraryBlockId == id && !e.Trashed);
                if (memberInGroup != null)
                {
                    cId += $"{memberInGroup.ItineraryBlockId}";
                }


                _.CID = cId;
                /*_.RoomType = _dbContext.FamilyHeadAdditionalFacilities
                                .Select(e => new
                                {
                                    e.FamilyHeadId,
                                    e.AdditionalFacilitiesId,
                                    e.AdditionalFacilities.PlaceId,
                                    e.AdditionalFacilities.Room.RoomCode
                                })
                                .AsNoTracking()
                                .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId
                                && e.PlaceId == 1)?
                                .RoomCode ?? "Sharing";*/

                // Get the FamilyHead record first
                var familyHead = _dbContext.FamilyHeads
                    .AsNoTracking()
                    .FirstOrDefault(fh => fh.FamilyHeadId == m.FamilyHeadId);

                if (familyHead?.AccomodationAsPerPackage == true)
                {
                    // If SameAsPerPackage is true, get RoomType from PackageAccomodations
                    _.RoomType = _dbContext.PackageAccomodtions
                        .Where(p => p.PackageId == familyHead.PackageId && p.PlaceId == 1)
                        .Select(p => p.Package.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }
                else
                {
                    // Else get RoomType from FamilyHeadAccomodations
                    _.RoomType = _dbContext.FamilyHeadAccomodations
                        .Where(fha => fha.FamilyHeadId == m.FamilyHeadId && fha.Hotel.PlaceId == 1)
                        .Select(fha => fha.IndividualFacilityDescription.Room.RoomCode)
                        .FirstOrDefault() ?? "N/A";
                }


                model.Add(_);

            }


            // return Ok(model);
            return View(model);
        }

        public IActionResult Trash(int id)
        {
            ItineraryBlock fiqah = _dbContext.ItineraryBlocks.Find(id);
            if (fiqah == null) return RedirectToAction(nameof(Index));

            fiqah.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}

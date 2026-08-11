using TMG.Data;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.PackageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using TMG.Models.ReservationModels;
using TMG.Models;
using TMG.Models.FormModels;
using System.Linq;

namespace TMG.Controllers.GroupController
{


    public class SubGroupController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public SubGroupController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.SubGroups
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.Trashed && e.PlaceId == 2)
                .Count();

            return View(_dbContext.SubGroupsWithoutTrash
                .Include(e => e.Group)
                .Include(e => e.Place)
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.PlaceId == 2));
        }
        public IActionResult MakkahIndex()
        {
            ViewBag.TrashCount = _dbContext.SubGroups
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.Trashed && e.PlaceId == 1).Count();

            return View(_dbContext.SubGroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Include(e => e.Group).Include(e => e.Place).Where(e => e.PlaceId == 1));
        }
        public IActionResult MinaIndex()
        {
            ViewBag.TrashCount = _dbContext.SubGroups
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(e => e.Trashed && e.PlaceId == 3).Count();

            return View(_dbContext.SubGroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Include(e => e.Group).Include(e => e.Place).Where(e => e.PlaceId == 3));
        }
        public IActionResult Create()
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId") ?? "0");

            var existingRoomDetailsIds = _dbContext.SubGroupRooms
            .Select(e => e.RoomDetailsId)
            .ToList();

            ViewBag.RoomDetails = _dbContext.RoomDetails
                .Include(e => e.POHotels)
                .Include(e => e.Room)
                .Where(e => e.POHotels.HajjSeasonsId == activeSeasonId
                       && e.PlaceId == 2
                       && !existingRoomDetailsIds.Contains(e.RoomDetailsId)) 
                .ToList();

            ViewBag.BathType = Enum.GetValues(typeof(BathroomType))
                .Cast<BathroomType>()
                .Select(e => new BathTypeModel { Id = (int)e, Name = e.ToString() })
                .ToList();

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash
                //.Where(e => e.HajjSeasonsId == activeSeasonId)
                .ToList();

            ViewBag.RoomViews = _dbContext.RoomViewsWithoutTrash.ToList();



            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            ViewBag.SubGroups = _dbContext.SubGroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            /*ViewBag.Rooms = _dbContext.RoomsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));*/
            return View();
        }

        public IActionResult MakkahCreate()
        {
            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId") ?? "0");

            var existingRoomDetailsIds = _dbContext.SubGroupRooms
            .Select(e => e.RoomDetailsId)
            .ToList();

            ViewBag.RoomDetails = _dbContext.RoomDetails
                .Include(e => e.POHotels)
                .Include(e => e.Room)
                .Where(e => e.POHotels.HajjSeasonsId == activeSeasonId
                       && e.PlaceId == 1
                       && !existingRoomDetailsIds.Contains(e.RoomDetailsId))
                .ToList();

            ViewBag.BathType = Enum.GetValues(typeof(BathroomType))
                .Cast<BathroomType>()
                .Select(e => new BathTypeModel { Id = (int)e, Name = e.ToString() })
                .ToList();

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash
                //.Where(e => e.HajjSeasonsId == activeSeasonId)
                .ToList();

            ViewBag.RoomViews = _dbContext.RoomViewsWithoutTrash.ToList();



            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            ViewBag.SubGroups = _dbContext.SubGroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            /*ViewBag.Rooms = _dbContext.RoomsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));*/
            return View();
        }

        [HttpPost]
        /* public IActionResult Create(SubGroup subGroup)
         {
             ViewBag.Places = _dbContext.PlacesWithoutTrash;
             ViewBag.Groups = _dbContext.GroupsWithoutTrash
                 .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
             ViewBag.SubGroups = _dbContext.SubGroupsWithoutTrash
                 .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
             ViewBag.Rooms = _dbContext.RoomsWithoutTrash
                 .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

             if (!ModelState.IsValid) return View();

             subGroup.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
             _dbContext.SubGroups.Add(subGroup);
             _dbContext.SaveChanges();

             for (int i = 0; i < subGroup.RoomIds.Count; i++)
             {
                 _dbContext.SubGroupRooms.Add(new SubGroupRooms
                 {
                     SubGroupId = subGroup.SubGroupId,
                     RoomId = subGroup.RoomIds[i],
                     //Quantity = subGroup.Quantities[i],
                     BathroomType = subGroup.BathroomType[i] as BathroomType?,
                     RoomViewId = subGroup.RoomViewId[i],
                     POHotelsId = subGroup.POHotelsId[i],
                     POHotelRoomsId = subGroup.POHotelRoomsId[i],
                     PlaceId = subGroup.PLaceIds[i],
                     RoomNumber = subGroup.RoomNumber[i],
                     RoomDetailsId = subGroup.RoomDetailsId[i]
                 });
             }
             _dbContext.SaveChanges();

             return RedirectToAction(nameof(Index));
         }*/
        public IActionResult Create(SubGroup subGroup, List<int> SelectedRooms)
        {
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            ViewBag.SubGroups = _dbContext.SubGroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;

            if (!ModelState.IsValid) return View();

            subGroup.HajjSeasonsId = Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.SubGroups.Add(subGroup);
            _dbContext.SaveChanges();

            // protect against null collection properties coming from model binding
            var roomIds = subGroup.RoomIds ?? new List<int>();
            var roomDetails = subGroup.RoomDetailsId ?? new List<int>();

            // if SelectedRooms is null there is nothing to add
            if (SelectedRooms != null)
            {
                for (int i = 0; i < roomIds.Count; i++)
                {
                    // ensure corresponding indexed collections have values
                    if (i >= roomDetails.Count) continue;

                    if (SelectedRooms.Contains(roomDetails[i]))
                    {
                        _dbContext.SubGroupRooms.Add(new SubGroupRooms
                        {
                            SubGroupId = subGroup.SubGroupId,
                            RoomId = roomIds[i],
                            BathroomType = (subGroup.BathroomType != null && subGroup.BathroomType.Count > i) ? subGroup.BathroomType[i] : null,
                            RoomViewId = (subGroup.RoomViewId != null && subGroup.RoomViewId.Count > i) ? subGroup.RoomViewId[i] : (int?)null,
                            POHotelsId = (subGroup.POHotelsId != null && subGroup.POHotelsId.Count > i) ? subGroup.POHotelsId[i] : (int?)null,
                            POHotelRoomsId = (subGroup.POHotelRoomsId != null && subGroup.POHotelRoomsId.Count > i) ? subGroup.POHotelRoomsId[i] : (int?)null,
                            PlaceId = (subGroup.PLaceIds != null && subGroup.PLaceIds.Count > i) ? subGroup.PLaceIds[i] : (int?)null,
                            RoomNumber = (subGroup.RoomNumber != null && subGroup.RoomNumber.Count > i) ? subGroup.RoomNumber[i] : null,
                            RoomDetailsId = roomDetails[i]
                        });
                    }
                }
            }

            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id, int PlaceId)
        {

            var activeSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId") ?? "0");

            var selectedRoomIds = _dbContext.SubGroupRooms
                .Where(e => e.SubGroupId == id)
                .Select(e => e.RoomDetailsId)
                .ToList();

            ViewBag.RoomDetails = _dbContext.RoomDetails
                .Include(e => e.POHotels)
                .Include(e => e.Room)
                .Where(e => e.POHotels.HajjSeasonsId == activeSeasonId
                           && e.PlaceId == PlaceId
                           && (selectedRoomIds.Contains(e.RoomDetailsId) ||
                               !_dbContext.SubGroupRooms.Any(sr => sr.RoomDetailsId == e.RoomDetailsId)))
                .ToList();


            ViewBag.BathType = Enum.GetValues(typeof(BathroomType))
                         .Cast<BathroomType>()
                         .Select(e => new { Id = (int)e, Name = e.ToString() })
                         .ToList();

            ViewBag.Rooms = _dbContext.RoomsWithoutTrash
                //.Where(e => e.HajjSeasonsId == activeSeasonId)
                .ToList();

            ViewBag.RoomViews = _dbContext.RoomViewsWithoutTrash.ToList();

            ViewBag.SelectedRoomDetails = _dbContext.SubGroupRooms
                .Where(e => e.SubGroupId == id)
                .Select(e => e.RoomDetailsId)
                .ToList();


            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            ViewBag.SubGroups = _dbContext.SubGroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            /*ViewBag.Rooms = _dbContext.RoomsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));*/

            SubGroup SubGroup = _dbContext.SubGroups.Find(id);
            var rooms = _dbContext.SubGroupRooms.Where(e => e.SubGroupId == id).ToList();
            /*var roomTypes = rooms.Select(e => e.RoomId).ToList();
            var roomQuantities = rooms.Select(e => e.Quantity).ToList();*/

            SubGroupRoomsEditModel roomsEditModel = new SubGroupRoomsEditModel
            {
                SubGroupId = SubGroup.SubGroupId,
                Contract = SubGroup.Contract,
                Code = SubGroup.Code,
                Name = SubGroup.Name,
                PAX = SubGroup.PAX,
                GroupId = SubGroup.GroupId,
                SubGroupMergeId = SubGroup.SubGroupMergeId,
                PlaceIds = SubGroup.PlaceId,
                SubGroupRooms = rooms.Select(room => new SubGroupRoomsModel
                {
                    RoomId = room.RoomId,
                    RoomNumber = room.RoomNumber,
                    BathroomType = room.BathroomType,
                    RoomViewId = room.RoomViewId,
                    POHotelsId = room.POHotelsId,
                    POHotelRoomsId = room.POHotelRoomsId ?? 0,
                    PlaceId = room.PlaceId,
                    RoomDetailsId = room.RoomDetailsId
                }).ToList(),
            };

            /* SubGroup.RoomIds = roomTypes;
             SubGroup.Quantities = roomQuantities;*/

            return View(roomsEditModel);
        }


        [HttpPost]
        public IActionResult Edit(SubGroupRoomsEditModel subGroup)
        {
            ViewBag.Places = _dbContext.PlacesWithoutTrash;
            ViewBag.Groups = _dbContext.GroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            ViewBag.SubGroups = _dbContext.SubGroupsWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));
            ViewBag.Rooms = _dbContext.RoomsWithoutTrash;

            if (!ModelState.IsValid) return View();

            var subGroups = _dbContext.SubGroups.FirstOrDefault(e => e.SubGroupId == subGroup.SubGroupId);
            if (subGroups == null)
            {
                return NotFound();
            }

        
            subGroups.Contract = subGroup.Contract;
            subGroups.Code = subGroup.Code;
            subGroups.Name = subGroup.Name;
            subGroups.PAX = subGroup.PAX;
            subGroups.PlaceId = subGroup.PlaceIds;
            subGroups.GroupId = subGroup.GroupId;
            subGroups.SubGroupMergeId = subGroup.SubGroupMergeId;

            _dbContext.SaveChanges();

      
            var existingRooms = _dbContext.SubGroupRooms
                .Where(e => e.SubGroupId == subGroup.SubGroupId)
                .ToList();

         
            var selectedRoomIds = subGroup.SelectedRoomDetailsId ?? new List<int>();

            Console.WriteLine("Selected RoomDetailsId: " + string.Join(", ", selectedRoomIds));
            Console.WriteLine("Existing Rooms in DB: " + string.Join(", ", existingRooms.Select(r => r.RoomDetailsId)));

       
            var roomsToDelete = existingRooms
                .Where(r => !selectedRoomIds.Contains(r.RoomDetailsId.GetValueOrDefault()))
                .ToList();

            Console.WriteLine("Rooms to be Deleted: " + string.Join(", ", roomsToDelete.Select(r => r.RoomDetailsId)));

            _dbContext.SubGroupRooms.RemoveRange(roomsToDelete);
            _dbContext.SaveChanges(); 

          
            for (int i = 0; i < subGroup.RoomDetailsId.Count; i++)
            {
                var roomDetailsId = subGroup.RoomDetailsId[i];
                if (!roomDetailsId.HasValue) continue;

            
                if (!selectedRoomIds.Contains(roomDetailsId.Value))
                    continue;

            
                var existingRoom = existingRooms.FirstOrDefault(r => r.RoomDetailsId == roomDetailsId.Value);

                if (existingRoom == null) 
                {
                    _dbContext.SubGroupRooms.Add(new SubGroupRooms
                    {
                        SubGroupId = subGroup.SubGroupId,
                        RoomDetailsId = roomDetailsId.Value,
                        RoomId = subGroup.RoomId[i],
                        RoomNumber = subGroup.RoomNumber[i],
                        BathroomType = subGroup.BathroomType[i],
                        RoomViewId = subGroup.RoomViewId[i],
                        POHotelsId = subGroup.POHotelsId[i],
                        POHotelRoomsId = subGroup.POHotelRoomsId[i],
                        PlaceId = subGroup.PlaceId
                    });

                    Console.WriteLine($"Added RoomDetailsId: {roomDetailsId.Value}");
                }
                else // ✅ Agar pehle se hai toh UPDATE karein
                {
                    existingRoom.RoomId = subGroup.RoomId[i];
                    existingRoom.RoomNumber = subGroup.RoomNumber[i];
                    existingRoom.BathroomType = subGroup.BathroomType[i];
                    existingRoom.RoomViewId = subGroup.RoomViewId[i];
                    existingRoom.POHotelsId = subGroup.POHotelsId[i];
                    existingRoom.POHotelRoomsId = subGroup.POHotelRoomsId[i];
                    existingRoom.PlaceId = subGroup.PlaceId;

                    Console.WriteLine($"Updated RoomDetailsId: {roomDetailsId.Value}");
                }
            }

            _dbContext.SaveChanges(); // ✅ Final Save


            return RedirectToAction(nameof(Index));
        }



        public IActionResult ViewTrash()
        {
            return View(_dbContext.SubGroups
                .Include(e => e.Group)
                .Include(e => e.Place)
                .Where(e => e.HajjSeasonsId == Convert.ToInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            SubGroup subGroup = _dbContext.SubGroups.Find(id);
            if (subGroup == null) return RedirectToAction(nameof(Index));

            subGroup.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            SubGroup subGroup = _dbContext.SubGroups.Find(id);
            subGroup.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult AddMember(int subGroupId)
        {
            SubGroup subGroup = _dbContext.SubGroupsWithoutTrash
            .Include(e => e.Group)
            .Include(e => e.Place)
            .FirstOrDefault(e => e.SubGroupId == subGroupId);

            if (subGroup == null)
            {
                return NotFound();
            }

            var accommodation = _dbContext.PackageAccomodtions
                                    .Where(e => e.PackageId == subGroup.Group.PackageId && e.PlaceId == subGroup.PlaceId)
                                    .Select(e => new
                                    {
                                        e.PlaceId,
                                        e.CheckIn,
                                        e.CheckOut,
                                        e.PackageId
                                    })
                                    .FirstOrDefault();

            // return Ok(new {
            //     Acc = accommodation
            // });

            ViewBag.SubGroupId = subGroupId;
            ViewBag.SubGroupCode = subGroup.Code;
            ViewBag.SubGroupName = subGroup.Name;
            ViewBag.SubGroupPAX = subGroup.PAX;

            ViewBag.GroupId = subGroup.GroupId;
            ViewBag.GroupCode = subGroup.Group.Code;
            ViewBag.GroupName = subGroup.Group.Name;
            ViewBag.GroupPAX = subGroup.Group.PAX;

            ViewBag.Place = subGroup.Place?.PlaceName;
            ViewBag.CheckIn = accommodation?.CheckIn;
            ViewBag.CheckOut = accommodation?.CheckOut;

            var subGroupMembers = _dbContext.SubGroupMembers.Where(e => e.SubGroupId == subGroupId);

            ViewBag.SubGroupMembers = subGroupMembers;
            ViewBag.SubGroupMembersCount = subGroupMembers.Count();

            var query = from gm in _dbContext.GroupMembers
                        join g in _dbContext.Groups on gm.GroupId equals g.GroupId
                        join fh in _dbContext.FamilyHeads on gm.FamilyHeadId equals fh.FamilyHeadId
                        join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                        join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                        join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                        join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                        where g.GroupId == subGroup.GroupId && !fh.Trashed && pa.PlaceId == subGroup.PlaceId
                        orderby fm.HeadId, fm.FamilyHeadId
                        select new
                        {
                            fh.FcId,
                            fm.HeadId,
                            fm.FamilyHeadId,
                            fm.IsHead,
                            c.CompanyCode,
                            p.Name,
                            fh.ReferenceId,
                            GroupName = g.Name,
                            GroupCode = g.Code,
                            fh.GivenName,
                            fh.SurName,
                            fh.DateOfBirth,
                            fh.Gender,
                            fh.PassportNumber,
                            fh.FriendId,
                            g.GroupId,
                            fh.Age
                        };

            var result = query.ToList();


            return View(
                result
                );
        }

        [HttpPost]
        public IActionResult AddMember(int subGroupId, int memberId)
        {
            int paxLimit = _dbContext.SubGroupsWithoutTrash.FirstOrDefault(e => e.SubGroupId == subGroupId).PAX;

            var subGroup = _dbContext.SubGroupsWithoutTrash.FirstOrDefault(e => e.SubGroupId == subGroupId);
            if (subGroup == null)
            {
                return BadRequest("Subgroup not found");
            }

            FamilyMembers fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == memberId);
            if (fm == null)
            {
                return BadRequest("Family member not found");
            }

            
            _dbContext.SubGroupMembers.Add(new SubGroupMember
            {
                FamilyHeadId = memberId,
                SubGroupId = subGroupId,
                FamilyMembersId = fm.FamilyMembersId
            });

            _dbContext.SaveChanges(); 

       
            subGroup.Selected = _dbContext.SubGroupMembers.Count(e => e.SubGroupId == subGroupId);
            _dbContext.Entry(subGroup).State = EntityState.Modified;  
            _dbContext.SaveChanges(); 

            return Ok(new { status = 200, message = $"Member Id: {memberId} added to group Id: {subGroupId}" });
        }

        /*public IActionResult AddMember(int subGroupId, int memberId)
        {
            int paxLimit = _dbContext.SubGroupsWithoutTrash.FirstOrDefault(e => e.SubGroupId == subGroupId).PAX;
            int selected = _dbContext.SubGroupMembers.Where(e => e.SubGroupId == subGroupId).Count();

            // if (selected == paxLimit)
            // {
            //    return BadRequest("Limit Reached");
            //}
            //else
            //{
            FamilyMembers fm = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == memberId);

            _dbContext.SubGroupMembers.Add(
                new SubGroupMember
                {
                    FamilyHeadId = memberId,
                    SubGroupId = subGroupId,
                    FamilyMembersId = fm.FamilyMembersId
                }
                );

            _dbContext.SaveChanges();

            _dbContext.SubGroupsWithoutTrash.FirstOrDefault(e => e.SubGroupId == subGroupId).Selected = selected + 1;
            _dbContext.SaveChanges();
            // }


            return Ok(new { status = 200, message = $"Member Id: {memberId} added to group Id: {subGroupId}" });
        }*/


        [HttpPost]
        public IActionResult RemoveMember(int subGroupId, int memberId)
        {
            int selected = _dbContext.SubGroupMembers.Where(e => e.SubGroupId == subGroupId).Count();
            var sgm = _dbContext.SubGroupMembers.FirstOrDefault(e => e.SubGroupId == subGroupId && e.FamilyHeadId == memberId);
            if (sgm != null)
            {
                _dbContext.SubGroupMembers.Remove(sgm);
                _dbContext.SaveChanges();
                _dbContext.SubGroupsWithoutTrash.FirstOrDefault(e => e.SubGroupId == subGroupId).Selected = selected - 1;
                _dbContext.SaveChanges();
            }

            return Ok(new { status = 200, message = $"Member Id: {memberId} removed from sub group Id: {subGroupId}" });
        }

        public IActionResult GetSubGroups(int id)
        {
            var subGroups = _dbContext.SubGroupsWithoutTrash
                .Where(e => e.GroupId == id && e.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            return Ok(subGroups);
        }
    }
}

namespace TMG.Models.GroupModels
{
    public record SubGroupAddModel
    {
        public int HeadId { get; set; }
        public int FamilyHeadId { get; set; }
        public bool IsHead { get; set; }
        public string CompanyCode { get; set; }
        public string Name { get; set; }
        public int ReferenceId { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }
        public string GivenName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
        public int FriendId { get; set; }
        public int GroupId { get; set; }
        public bool Selected { get; set; }
    }
}
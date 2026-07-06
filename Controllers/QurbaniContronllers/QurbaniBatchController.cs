using Azure.Core;
using Humanizer;
using TMG.Data;
using TMG.ManagerClasses;
using TMG.Models.FamilyModels;
using TMG.Models.GroupModels;
using TMG.Models.MiscModels;
using TMG.Models.PackageModels;
using TMG.Models.QurbaniModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Drawing.Printing;

namespace TMG.Controllers.QurbaniContronllers
{
    public class QurbaniBatchController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;

        public QurbaniBatchController(TMGDbContext humsDbContext, IWebHostEnvironment host)
        {
            _dbContext = humsDbContext;
            _host = host;
        }



        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.QurbaniBatches
                .Where(e => e.Trashed).Count();

            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashQurbaniBatch(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateQurbaniBatch(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditQurbaniBatch(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreQurbaniBatch(_dbContext);

            List<QurbaniBatchIndexModel> model = new();

            foreach (var item in _dbContext.QurbaniBatches.Where(e => !e.Trashed).ToList())
            {
                QurbaniBatchIndexModel _ = new()
                {
                    BatchName = item.Name,
                    QurbaniBatchId = item.QurbaniBatchId,
                    Capacity = item.Capacity,
                    WhatsappGroupLink = item.WhatsappGroupLink,
                    WhatsappQrCodePath = item.WhatsappQrCodePath,
                    Selected = _dbContext.QurbaniQueue.Where(e => e.QurbaniBatchId == item.QurbaniBatchId).Count(),
                    Completed = item.Completed
                };

                model.Add(_);


            }

            return View(model);
        }

        public IActionResult Create()
        {
            List<QurbaniQueueViewModel> model = new();


            var familyMembers = from fm in _dbContext.FamilyMembers
                                join fh in _dbContext.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                                join qq in _dbContext.QurbaniQueue on fm.FamilyHeadId equals qq.FamilyHeadId
                                where qq.QurbaniBatchId == null
                                orderby fm.HeadId ascending
                                select new
                                {
                                    fm.IsHead,
                                    fm.FamilyHeadId,
                                    fm.HeadId,
                                    fh.FcId,
                                    fh.FullName,
                                    fh.Gender,
                                    fh.PassportNumber,
                                };


            int i = 1;
            foreach (var m in familyMembers)
            {
                model.Add(new QurbaniQueueViewModel
                {
                    IsHead = m.IsHead,
                    FamilyHeadId = m.FamilyHeadId,
                    HeadId = m.HeadId,
                    SrNo = i,
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    Passport = m.PassportNumber,
                });
                i++;
            }

            ViewBag.Members = model;

            return View();
        }

        [HttpPost]
        public IActionResult Create(QurbaniBatch batch)
        {

            List<QurbaniQueueViewModel> model = new();


            var familyMembers = from fm in _dbContext.FamilyMembers
                                join fh in _dbContext.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                                join qq in _dbContext.QurbaniQueue on fm.FamilyHeadId equals qq.FamilyHeadId
                                where qq.QurbaniBatchId == null
                                orderby fm.HeadId ascending
                                select new
                                {
                                    fm.IsHead,
                                    fm.FamilyHeadId,
                                    fm.HeadId,
                                    fh.FcId,
                                    fh.FullName,
                                    fh.Gender,
                                    fh.PassportNumber,
                                };


            int i = 1;
            foreach (var m in familyMembers)
            {
                model.Add(new QurbaniQueueViewModel
                {
                    IsHead = m.IsHead,
                    FamilyHeadId = m.FamilyHeadId,
                    HeadId = m.HeadId,
                    SrNo = i,
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    Passport = m.PassportNumber,
                });
                i++;
            }

            ViewBag.Members = model;

            if (!ModelState.IsValid) return View(batch);

            var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                string path = Path.Combine(_host.WebRootPath, "Images", "BatchQrCode", batch.WhatsappQrCodeFile.FileName);

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    batch.WhatsappQrCodeFile.CopyTo(fs);
                }

                QurbaniBatch _ = new QurbaniBatch()
                {
                    Name = batch.Name,
                    Capacity = batch.Capacity,
                    WhatsappQrCodePath = path,
                    WhatsappGroupLink = batch.WhatsappGroupLink
                };

                _dbContext.QurbaniBatches.Add(_);
                _dbContext.SaveChanges();

                var queue = _dbContext.QurbaniQueue.ToList();
                for (int z = 0; z < batch.FamilyHeadIds.Count; z++)
                {
                    if (batch.Selected.ElementAt(z) == false) continue;
                    var q = queue.FirstOrDefault(e => e.FamilyHeadId == batch.FamilyHeadIds.ElementAt(z));
                    q.QurbaniBatchId = _.QurbaniBatchId;
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return RedirectToAction(nameof(Index));

            }
            catch (Exception e)
            {
                transaction.Rollback();
                return Ok(new
                {
                    e
                });
                return View(batch);
            }


        }

        public IActionResult Edit(int id)
        {
            QurbaniBatch QurbaniBatch = _dbContext.QurbaniBatches.Find(id);

            return View(QurbaniBatch);
        }


        [HttpPost]
        public IActionResult Edit(QurbaniBatch list)
        {
            if (!ModelState.IsValid) return View();

            _dbContext.Entry(list).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.QurbaniBatches.Where(s => s.Trashed));
        }

        public IActionResult Trash(int id)
        {
            QurbaniBatch list = _dbContext.QurbaniBatches.Find(id);
            if (list == null) return RedirectToAction(nameof(Index));

            list.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            QurbaniBatch list = _dbContext.QurbaniBatches.Find(id);
            list.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RamiStartTimeEdit(int id)
        {
            ViewBag.QurbaniBatchId = id;

            List<QurbaniQueueViewModel> model = new();


            var familyMembers = from fm in _dbContext.FamilyMembers
                                join fh in _dbContext.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                                join qq in _dbContext.QurbaniQueue on fm.FamilyHeadId equals qq.FamilyHeadId
                                where qq.QurbaniBatchId == id
                                orderby fm.HeadId ascending
                                select new
                                {
                                    fm.IsHead,
                                    fm.FamilyHeadId,
                                    fm.HeadId,
                                    fh.FcId,
                                    fh.FullName,
                                    fh.Gender,
                                    fh.PassportNumber,
                                    qq.StartTime
                                };


            int i = 1;
            foreach (var m in familyMembers)
            {
                model.Add(new QurbaniQueueViewModel
                {
                    StartTime = m.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    IsHead = m.IsHead,
                    FamilyHeadId = m.FamilyHeadId,
                    HeadId = m.HeadId,
                    SrNo = i,
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    Passport = m.PassportNumber,
                });
                i++;
            }


            return View(model);
        }

        [HttpPost]
        public IActionResult RamiStartTimeEdit(int QurbaniBatchId, List<int> FamilyHeadIds, List<DateTime> RamiTime)
        {
            ViewBag.QurbaniBatchId = QurbaniBatchId;

            var members = _dbContext.QurbaniQueue
                            .Where(e => FamilyHeadIds.Contains(e.FamilyHeadId))
                            .ToList();

            for (int i = 0; i < members.Count; i++)
            {
                members.ElementAt(i).StartTime = RamiTime.ElementAt(i);
            }
            _dbContext.SaveChanges();

            return RedirectToAction("Index");

        }

        public JsonResult AllQurbaniMembers()
        {
            List<QurbaniQueueViewModel> model = new();


            var familyMembers = from fm in _dbContext.FamilyMembers
                                join fh in _dbContext.FamilyHeads on fm.FamilyHeadId equals fh.FamilyHeadId
                                join qq in _dbContext.QurbaniQueue on fm.FamilyHeadId equals qq.FamilyHeadId
                                where qq.QurbaniBatchId == null
                                orderby fm.HeadId ascending
                                select new
                                {
                                    fm.IsHead,
                                    fm.FamilyHeadId,
                                    fm.HeadId,
                                    fh.FcId,
                                    fh.FullName,
                                    fh.Gender,
                                    fh.PassportNumber,
                                };


            int i = 1;
            foreach (var m in familyMembers)
            {
                model.Add(new QurbaniQueueViewModel
                {
                    IsHead = m.IsHead,
                    FamilyHeadId = m.FamilyHeadId,
                    HeadId = m.HeadId,
                    SrNo = i,
                    FcId = m.FcId,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    Passport = m.PassportNumber,
                });
                i++;
            }

            return Json(new
            {
                model
            });
        }

        public IActionResult AllBatches()
        {
            ViewBag.TotalMembers = _dbContext.FamilyHeadExtras
                                    .Select(e => new {
                                        e.ExtraFacilityDescriptionId,
                                        e.FamilyHead.Trashed,
                                        e.FamilyHeadId
                                    })
                                    .Where(e => !e.Trashed && e.ExtraFacilityDescriptionId == 1)
                                    .Count();
            
            List<QurbaniAllBatchesModel> model = new();

            var allBatches = _dbContext.QurbaniBatches
                .AsNoTracking()
                .Where(e => !e.Trashed)
                .OrderByDescending(e => e.QurbaniBatchId)
                .ToList();

            foreach (var batch in allBatches)
            {
                QurbaniAllBatchesModel _ = new()
                {
                    QurbaniBatchId = batch.QurbaniBatchId,
                    Name = batch.Name,
                    Capacity = batch.Capacity,
                    QrCode = batch.WhatsappQrCodePath,
                    Link = batch.WhatsappGroupLink,
                };

                var batchMemberCount = _dbContext.QurbaniQueue
                                    .AsNoTracking()
                                    .Where(e => e.QurbaniBatchId == batch.QurbaniBatchId).Count();

                _.Stopped = batch.Stopped | batch.Capacity == batchMemberCount;
                model.Add(_);
            }

            return View(model);
        }


        [HttpPost]
        public IActionResult StopBatch(int id)
        {
            _dbContext.QurbaniBatches.Find(id).Stopped = true;
            _dbContext.SaveChanges();

            return Ok();
        }


        public IActionResult IncreaseDuration(int id)
        {
            var batch = _dbContext.QurbaniBatches.Find(id);

            return View(batch);
        }

        [HttpPost]
        public IActionResult IncreaseDuration(int id, int MinutesToAdd)
        {
            var batch = _dbContext.QurbaniBatches.Find(id);

            var members = _dbContext.QurbaniQueue
                            .Where(e => e.QurbaniBatchId == id)
                            .ExecuteUpdate(e => e.SetProperty(x => x.StartTime, x => x.StartTime.AddMinutes(MinutesToAdd)));

            return RedirectToAction("Index");
        }

        public IActionResult CompleteQurbani(int id)
        {
            var batch = _dbContext.QurbaniBatches.Find(id);

            return View(batch);
        }

        [HttpPost]
        public IActionResult CompleteQurbani(int id, int _)
        {

            _dbContext.QurbaniBatches
                        .Where(e => e.QurbaniBatchId == id)
                        .ExecuteUpdate(e => e.SetProperty(x => x.Completed, true));

            var members = _dbContext.QurbaniQueue
                            .Where(e => e.QurbaniBatchId == id)
                            .ExecuteUpdate(e => e.SetProperty(x => x.StartTime, x => x.StartTime.AddDays(-1)));


            return RedirectToAction("Index");
        }

        public IActionResult BatchMembers(int id)
        {
            ViewBag.BatchName = _dbContext.QurbaniBatches.Find(id).Name;

            var members = _dbContext.QurbaniQueue
                        .Select(e => new
                        {
                            e.QurbaniBatchId,
                            e.FamilyHead.FullName,
                            e.FamilyHead.FcId,
                            e.FamilyHead.Gender,
                            e.FamilyHead.KSAMobileNumber,
                            PackageName = e.FamilyHead.Package.Name,
                            e.FamilyHead.Package.Company.CompanyCode,
                            e.FamilyHead.IndId,
                            e.FamilyHead.FamilyHeadId,
                        })
                        .Where(e => e.QurbaniBatchId == id)
                        .ToList();

            List<BatchMemberModel> model = new();

            int i = 0;
            foreach (var m in members)
            {
                BatchMemberModel _ = new()
                {
                    SrNo = ++i,
                    FullName = m.FullName,
                    Gender = m.Gender.ToString(),
                    MobileNumber = _dbContext.FamilyHeadContacts.FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId)?.Contact,
                    KSANumber = m.KSAMobileNumber,
                    FcId = m.FcId
                };

                int headId = _dbContext.FamilyMembers.FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId).HeadId;
                int familyPax = _dbContext.FamilyMembers.Where(e => e.HeadId == headId).Count();

                string cId =
                      $"{m.CompanyCode}" +
                      $".{m.IndId}" +
                      $".{headId}({familyPax})" +
                      $".{m.PackageName}";

                var memberInGroup = _dbContext.GroupMembers
                                    .Include(e => e.Group)
                                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);

                if (memberInGroup != null)
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";

                _.CID = cId;

                model.Add(_);
            }

            return View(model);
        }

    }
}

namespace TMG.Models.QurbaniModels
{
    public record QurbaniBatchIndexModel
    {
        public int QurbaniBatchId { get; set; }
        public int Capacity { get; set; }
        public int Selected { get; set; }
        public string BatchName { get; set; }
        public string WhatsappGroupLink { get; set; }
        public string WhatsappQrCodePath { get; set; }
        public bool Completed { get; set; }
    }
    public record BatchMemberModel
    {
        public int SrNo { get; set; }
        public string FcId { get; set; }
        public string CID { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string KSANumber { get; set; }
        public string MobileNumber { get; set; }
    }

    public record QurbaniAllBatchesModel
    {
        public int QurbaniBatchId { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string QrCode { get; set; }
        public string Link { get; set; }
        public bool Stopped { get; set; }
    }


    public record QurbaniQueueViewModel
    {
        public string StartTime { get; set; }
        public bool IsHead { get; set; }
        public int FamilyHeadId { get; set; }
        public int HeadId { get; set; }
        public int SrNo { get; set; }
        public string FcId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Passport { get; set; }
    }


    public record BatchSlipsModel
    {
        public string TCsString { get; set; }
        public string NotesString { get; set; }
        public string QrCodePath { get; set; }
        public bool displayconfirm { get; set; }
        public List<BatchSlipEntryModel> Entries { get; set; } = new List<BatchSlipEntryModel>();
    }

    public record BatchSlipEntryModel
    {
        public bool QurbaniEnabled { get; set; }
        public string CID { get; set; }
        public string FCID { get; set; }
        public string CompanyName { get; set; }
        public string FHName { get; set; }
        public string TokenNumber { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string KSAMobileNumber { get; set; }
        public string GroupNumber { get; set; }
        public string Received { get; set; }
        public string USDPrice { get; set; }
        public int FamilyHeadId { get; set; }
        public bool saved { get; set; }
        public string WhatsappGroupLink { get; set; }
        public DateTime RamiDoneDateTime { get; set; }
        public DateTime EstimatedQurbaniDateTime { get; set; }
        public string BatchNumber { get; set; }
        public bool BatchCompleted { get; set; }
    }

    public record QurbaniListViewModel
    {
        public List<QurbaniBatchListModel> QurbaniBatchListModels { get; set; } = new List<QurbaniBatchListModel>();
        public string CompanyName { get; set; }
        public int BatchNumber { get; set; }
        public int Amount { get; set; }
        public List<int?> SubmittedIds { get; set; }
        public List<bool> SubmittedPaids { get; set; }
        public List<string> GivenNames { get; set; } = new List<string>();
        public List<string> QurbaniTypes { get; set; } = new List<string>();
        public List<int?> QurbaniIds { get; set; } = new List<int?>();
    }

    public record QurbaniBatchListModel
    {
        public int TokenNumber { get; set; }
        public int? FCId { get; set; }
    }

    public record QurbaniListPrintModel
    {
        public int SerialNumber { get; set; }
        public string FCId { get; set; }
        public string CompanyName { get; set; }
        public string BatchNumber { get; set; }
        public int Amount { get; set; }
    }
}


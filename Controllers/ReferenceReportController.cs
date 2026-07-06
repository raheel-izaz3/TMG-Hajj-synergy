using TMG.Data;
using Microsoft.AspNetCore.Mvc;
using TMG.Models.CompanyModels;
using Microsoft.EntityFrameworkCore;
using TMG.Models.PackageModels;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;

namespace TMG.Models.CompanyModels
{
    public record RequiredDocumentsReferenceReportModel
    {
        public string ReferenceName { get; set; }
        public List<string> RequiredDocHeaders { get; set; } = new();
        public int FamilyMembersCount { get; set; }
        public List<int> TotalPending { get; set; } = new();
        public List<int> TotalReceived { get; set; } = new();

        public List<ReferencePackageGroup> PackageGroups { get; set; } = new();
    }

    public record ReferencePackageGroup
    {
        public string PackageName { get; set; }
        public string PackageCode { get; set; }

        public List<ReferenceRequiredDocumentFamilyMemberGroupModel> Families { get; set; } = new();
    }

    public record ReferenceRequiredDocumentFamilyMemberGroupModel
    {
        public string FCId { get; set; }
        public List<ReferenceFamilyMemberRequiredDocuments> Documents { get; set; } = new List<ReferenceFamilyMemberRequiredDocuments>();
    }

    public record ReferenceFamilyMemberRequiredDocuments
    {
        public int FamilyHeadId { get; set; }
        public string Name { get; set; }
        public List<ReferenceDocumentEntry> Entries { get; set; } = new List<ReferenceDocumentEntry>();
    }

    public record ReferenceDocumentEntry
    {
        public RequiredDocument RequiredDocument { get; set; }
        public bool Checked { get; set; }
    }

}

namespace TMG.Controllers
{
    public class ReferenceReportController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public ReferenceReportController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult RequiredDocuments(int id)
        {
            var reference = _dbContext.References.Find(id);

            var documents = _dbContext.RequiredDocumentsWithoutTrash.ToList();

            var familyHeads = _dbContext.FamilyHeadsWithoutTrash
                            .Include(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .Select(e => new
                            {
                                e.Package.CompanyId,
                                e.PackageId,
                                e.FamilyHeadId,
                                e.IsHead,
                                e.FullName,
                                e.ReferenceId,
                                e.IndId,
                                e.ExtractedFamilyNumber,
                            })
                            .Where(e => e.ReferenceId == id && e.IsHead)
                            .ToList();

            var families = _dbContext.FamilyMembers
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                            .Select(e => new
                            {
                                e.FamilyHead.Package.CompanyId,
                                e.FamilyHead.PackageId,
                                e.FamilyHeadId,
                                e.FamilyHead.FullName,
                                e.FamilyHead.Trashed,
                                e.HeadId,
                                e.FamilyHead.ReferenceId,
                                e.FamilyHead.IndId,
                                e.FamilyHead.ExtractedFamilyNumber,
                            })
                            .Where(e => e.ReferenceId == id && !e.Trashed)
                            .ToList();

            var totalReceived = _dbContext.FamilyHeadDocuments
               .Join(_dbContext.FamilyHeads,
                   fhd => fhd.FamilyHeadId,
                   fh => fh.FamilyHeadId,
                   (fhd, fh) => new { fhd, fh })
               .Where(joined => joined.fh.ReferenceId == id && !joined.fh.Trashed)
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
                .Where(joined => joined.fh.ReferenceId == id && !joined.fh.Trashed)
                .GroupBy(joined => joined.fhd.RequiredDocumentId)
                .Select(group => new
                {
                    RequiredDocumentId = group.Key,
                    TotalPending = group.Count(item => !item.fhd.Recieved)
                })
                .ToList();

            RequiredDocumentsReferenceReportModel model = new()
            {
                ReferenceName = reference.ReferenceName,
                TotalPending = totalPending.Select(e => e.TotalPending).ToList(),
                TotalReceived = totalReceived.Select(e => e.TotalReceived).ToList(),
                RequiredDocHeaders = documents.Select(e => e.Documents).ToList(),
                FamilyMembersCount = families.Count
            };

            var packages = _dbContext.PackagesWithoutTrash
                .Where(e => e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .ToList();

            var memberDocuments = _dbContext.FamilyHeadDocuments
                .Include(e => e.RequiredDocument)
                .Where(e => e.FamilyHead.ReferenceId == id)
                .ToList();


            for (int i = 0; i < packages.Count; i++)
            {
                var currPackage = packages.ElementAt(i);
                var currPackageHeads = familyHeads
                        .Where(e => e.PackageId == currPackage.PackageId)
                        .ToList();

                ReferencePackageGroup packageGroup = new()
                {
                    PackageCode = currPackage.Code,
                    PackageName = currPackage.Name
                };

                for (int j = 0; j < currPackageHeads.Count; j++)
                {
                    var _currFam = currPackageHeads.ElementAt(j);
                    var _currFamMembers = families.Where(e => e.HeadId == _currFam.FamilyHeadId).ToList();

                    var famModel = new ReferenceRequiredDocumentFamilyMemberGroupModel()
                    {
                        FCId = $"HB-{_currFam.ExtractedFamilyNumber}"
                    };

                    for (int k = 0; k < _currFamMembers.Count; k++)
                    {
                        var _currFamMember = _currFamMembers.ElementAt(k);
                        var memberModel = new ReferenceFamilyMemberRequiredDocuments
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

                            memberModel.Entries.Add(new ReferenceDocumentEntry
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
                return RedirectToAction("Index", "Reference");
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




    }
}
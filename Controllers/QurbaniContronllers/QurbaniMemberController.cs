using TMG.Data;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.QurbaniModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace TMG.Controllers.QurbaniContronllers
{
    public class QurbaniMemberController : Controller
    {
        private readonly TMGDbContext _dbContext;
        public QurbaniMemberController(TMGDbContext humsDbContext)
        {
            _dbContext = humsDbContext;
        }

        public IActionResult Index()
        {
            QurbaniMemberModel model = new QurbaniMemberModel();

            model.MemberEntries = new List<MemberEntry>();

            var members = _dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .ThenInclude(e => e.Company)
                .OrderBy(e => e.HeadId)
                .ToList();

            FamilyMembers prevMember = null;
            bool sameFamily = true;
            for(int i = 0, j = 0, k = 1;  i < members.Count; i++)
            {
                FamilyMembers currentMember = members.ElementAt(i);

                if(i > 0)
                {
                    prevMember = members.ElementAt(i - 1);
                }
                if(prevMember != null)
                {
                    if(prevMember.HeadId != currentMember.HeadId)
                    {
                        sameFamily = false;
                        k++;
                    }
                }
                
                var memberInGroup = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .FirstOrDefault(e => e.FamilyHeadId ==  currentMember.FamilyHeadId);
                
                MemberEntry entry = new MemberEntry();

                entry.FamilyHeadId = currentMember.FamilyHeadId;

                string sNo = k.ToString();

                if (currentMember.IsHead)
                {
                    sNo += ".0";
                    j = 1;
                    entry.TextAlign = "left";
                }
                else
                {
                    sNo += "." + j++;
                    entry.TextAlign = "right";
                }
                entry.SNumber = sNo;

                entry.CompanyName = currentMember.FamilyHead.Package.Company.CompanyName;
                entry.PackageName = currentMember.FamilyHead.Package.Name;

                if(memberInGroup == null)
                {
                    entry.Group = "-";
                }
                else
                {
                    entry.Group = memberInGroup.Group.Name;
                }
                entry.Name = currentMember.FamilyHead.GivenName;
                entry.Passport = currentMember.FamilyHead.PassportNumber;

                var memberQurbani = _dbContext.Qurbani
                    .FirstOrDefault(e => e.FamilyHeadId ==  currentMember.FamilyHeadId && e.Active);

                entry.QurbaniId = memberQurbani?.QurbaniId;

                entry.Checked = memberQurbani != null;

                model.MemberEntries.Add(entry);
            }

            return View(model);
        }
    }
}


namespace TMG.Models.QurbaniModels
{
    public record QurbaniMemberModel
    {
        public List<MemberEntry> MemberEntries { get; set; }
    }

    public record MemberEntry
    {
        public int FamilyHeadId { get; set; }
        public string SNumber { get; set; }
        public string CompanyName { get; set; }
        public string PackageName { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public string Passport { get; set; }
        public int? QurbaniId { get; set; }
        public int? QurbaniTypeId { get; set; }
        public bool Checked { get; set; }
        public string TextAlign { get; set; }
    }


}
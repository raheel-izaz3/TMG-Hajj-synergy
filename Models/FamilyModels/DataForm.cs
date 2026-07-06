using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public enum DataStatus
    {
        Verified, Unverified
    }

    public class DataForm : BaseEntity
    {
        public int DataFormId { get; set; }
        public int SPOId { get; set; }
        public SPO SPO { get; set; }
        public string FeedBy { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string FatherHusbandName { get; set; }
        public string CNICNo { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }
        public string GroupNo { get; set; }
        public string PassportAttachment { get; set; }
        public string CNICBackAttachment { get; set; }
        public string CNICAttachment { get; set; }
        public string FormAttachment { get; set; }
        public string MemberImage { get; set; }
        public string Address { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DataStatus Status { get; set; }
        public bool IsHead { get; set; }
        public string Password { get; set; }
    }

    public class DataFormMember
    {
        public int DataFormMemberId { get; set; }

        public int DataFormId { get; set; }
        public DataForm DataForm { get; set; }

        [ForeignKey("DataFormId")]
        public int HeadId { get; set; }
        public DataForm Head { get; set; }

        public bool IsHead { get; set; }
    }
}

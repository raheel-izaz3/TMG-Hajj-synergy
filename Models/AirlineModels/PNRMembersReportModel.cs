namespace TMG.Models.AirlineModels
{
    public record PNRMembersReportModel
    {
        public int SNo { get; set; }
        public string Title { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
        public string PassportDOE { get; set; }
        public string DateOfBirth { get; set; }
    }
}
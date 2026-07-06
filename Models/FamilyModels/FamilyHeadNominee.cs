using TMG.Models.MiscModels;

namespace TMG.Models.FamilyModels
{
    public class FamilyHeadNominee
    {
        public int FamilyHeadNomineeId { get; set; }

        public int FamilyHeadId { get; set; }
        public FamilyHead FamilyHead { get; set; }

        public string Name { get; set; }
        public int? NomineeRelationId { get; set; }
        public NomineeRelation NomineeRelation { get; set; }
        public string CNIC { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public int? CountryId { get; set; }
        public Country Countries { get; set; }
        public string City { get; set; }
        public int? CityId { get; set; }
        public City Cities { get; set; }
        public string ZipCode { get; set; }
        public string PoBox { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public bool Preferred { get; set; }
        public string NomineeCnicImage { get; set; }
    }
}

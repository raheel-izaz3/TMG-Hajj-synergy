using System.ComponentModel.DataAnnotations;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;

namespace TMG.Models.AccountModels
{

    public class ExpenseContactPerson : BaseEntity
    {
        public int ExpenseContactPersonId { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PoBox { get; set; }

        [Required, Display(Name = "Status")]
        public ActiveStatus ActiveStatus { get; set; }

        // FK

        public int? CountryId { get; set; }
        public Country Country { get; set; }
        public int? CityId { get; set; }
        public City City { get; set; }
        public int ExpenseSubTypeId { get; set; }
        public ExpenseSubType ExpenseSubType { get; set; }
        public int? HajjSeasonsId { get; set; }
        public HajjSeasons HajjSeasons { get; set; }
        public int? DesignationId { get; set; }
        public Designation Designation { get; set; }
    }
}

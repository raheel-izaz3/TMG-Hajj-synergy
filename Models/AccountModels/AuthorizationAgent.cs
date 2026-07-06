using System.ComponentModel.DataAnnotations;

namespace TMG.Models.AccountModels
{
    public class AuthorizationAgent : BaseEntity
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
    }
}

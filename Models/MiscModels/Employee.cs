namespace TMG.Models.MiscModels
{
    public class Employee : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string CNIC { get; set; }
        public string NTN { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string AddressProof { get; set; }
        public string FamilyCNIC { get; set; }
        public string CV {  get; set; }
        public string EmergencyContact { get; set; }
        public string CNICCopy { get; set; }
        public string OtherDoc { get; set; }
        public float? BasicSalary {  get; set; }
    }
}

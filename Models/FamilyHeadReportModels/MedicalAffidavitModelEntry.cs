namespace TMG.Models.FamilyHeadReportModels
{
    public record MedicalAffidavitModelEntry
    {
        public int FamilyHeadId { get; set; }
        public bool ShowSignature { get; set; }
        public string SignatureBase64 { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public string FatherHusbandName { get; set; }
        public string CNIC { get; set; }
        public string PassportNumber { get; set; }
        public int Age { get; set; }
        public string BloodGroup { get; set; }

        public string Vaccine1 { get; set; }
        public string DoseDate1 { get; set; }

        public string Vaccine2 { get; set; }
        public string DoseDate2 { get; set; }

        public string Vaccine3 { get; set; }
        public string DoseDate3 { get; set; }

        public string Vaccine4 { get; set; }
        public string DoseDate4 { get; set; }

        public string Vaccine5 { get; set; }
        public string DoseDate5 { get; set; }
    }
}

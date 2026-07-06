namespace TMG.Models.FamilyHeadReportModels
{
    public record HajjPackageInformationModel
    {
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }

        public string SPOName { get; set; }
        public string FCId { get; set; }
        public string FamilyHeadName { get; set; }
        public string PackageName { get; set; }
        public string Occupancy { get; set; }
        public int TotalPax { get; set; }
        public int AdultPax { get; set; }
        public int ChildPax { get; set; }
        public int InfantPax { get; set; }

        public List<string> RoomTableHeaders { get; set; } = new();
        public int HeaderCountForColSpan { get; set; }

        public List<string> AdultPackageAmountPP { get; set; } = new();
        public List<string> TotalAdultPackagePP { get; set; } = new();
        public string TicketPricePP { get; set; }
        public string ExtraPricePP { get; set; }

        public string ChildTotalPP { get; set; }
        public string InfantTotalPP { get; set; }

        public List<string> IADF { get; set; } = new();
        public List<string> EIADF { get; set; } = new();
        public List<string> Remarks { get; set; } = new();

        public string TotalDiscount { get; set; }
        public string InvoiceGrandTotal { get; set; }
        public string NetTotal { get; set; }

        public string AdultTotalUsd { get; set; }
        public string ChildTotalUsd { get; set; }
        public string InfantTotalUsd { get; set; }

    }
}

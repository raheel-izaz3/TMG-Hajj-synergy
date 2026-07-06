using TMG.Models.FamilyModels;

namespace TMG.Models;

public class UserHajjAgreement
{
    public int UserHajjAgreementId { get; set; }
    public string ExtraFacilities { get; set; }
    public string ExtraFacilitiesPriceUsd { get; set; }
    public string IADFPriceWithPackage { get; set; }
    public string IADFString { get; set; }
    public string NetReceivable { get; set; }
    public string Category { get; set; }
    public string CompanyName { get; set; }
    public string ClusterName { get; set; }

    public int FamilyHeadId { get; set; }
    public FamilyHead FamilyHead { get; set; }

    public DateTime SignedOn { get; set; }
    public string SignatureBase64 { get; set; }

    public bool SignMedicalAffidavit { get; set; }
    public bool SignDataForm { get; set; }
    public bool SignMISDataForm { get; set; }
    public bool MedicallyAble { get; set; }

    public string SharedAgreement { get; set; }
    public string SpecificAgreement { get; set; }
    public string CompanySeal { get; set; }

    public string Title { get; set; }
    public string Name { get; set; }
    public string CID { get; set; }
    public string PassportNumber { get; set; }
    public string Address { get; set; }
    public string Contact { get; set; }
    public string Email { get; set; }
    public string ApplicantType { get; set; }
    public string HajjAppNumber { get; set; }
    public string HajjiImage { get; set; }

    public string PricingPerPersonPkr { get; set; }
    public string PricingPerPersonSar { get; set; }
    public string PricingPerPersonUsd { get; set; }

    public string IADFPricePkr { get; set; }
    public string IADFPriceSar { get; set; }
    public string IADFPriceUsd { get; set; }

    public string EIADFPricePkr { get; set; }
    public string EIADFPriceSar { get; set; }
    public string EIADFPriceUsd { get; set; }

    public string TicketString { get; set; }
    public string FlightTicketPricePkr { get; set; }
    public string FlightTicketPriceSar { get; set; }
    public string FlightTicketPriceUsd { get; set; }
    public string FlightTicketDifferencePriceUsd { get; set; }

    public string DiscountPkr { get; set; }
    public string DiscountSar { get; set; }
    public string DiscountUsd { get; set; }

    public string TotalPerPersonPkr { get; set; }
    public string TotalPerPersonSar { get; set; }
    public string TotalPerPersonUsd { get; set; }

    public string TotalWithFamilyPkr { get; set; }
    public string TotalWithFamilySar { get; set; }
    public string TotalWithFamilyUsd { get; set; }

    public string AmountReceivedPkr { get; set; }
    public string AmountReceivedSar { get; set; }
    public string AmountReceivedUsd { get; set; }

    public string BalanceDuePkr { get; set; }
    public string BalanceDueSar { get; set; }
    public string BalanceDueUsd { get; set; }

    public string Package { get; set; }
    public string MakkahFacilities { get; set; }
    public string MedinaFacilities { get; set; }
    public string AziziaFacilities { get; set; }
    public string MashairFacilities { get; set; }
    public string Airline { get; set; }
    public string MedinahArrival { get; set; }
}
namespace TMG.Models.POModels;

public class POVisaInstallment
{
    public int POVisaInstallmentId { get; set; }

    public int POVisaId { get; set; }
    public POVisa POVisa { get; set; }

    public DateTime DueDate { get; set; }
    public float Percentage { get; set; }
    public float Amount { get; set; }
}

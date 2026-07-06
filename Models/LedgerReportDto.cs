namespace TMG.Models;

public class LedgerReportDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

public class BalanceCFDto
{
    public decimal BalanceCF { get; set; }
}

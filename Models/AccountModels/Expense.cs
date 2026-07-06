using System.ComponentModel.DataAnnotations.Schema;
using TMG.Models.MiscModels;

namespace TMG.Models.AccountModels;

public class Expense: BaseEntity
{
    public int ExpenseId { get; set; }
    
    public int ExpenseTypeId { get; set; }
    public ExpenseType ExpenseType { get; set; }


    public int CompanyBankAccountId { get; set; }
    public CompanyBankAccount CompanyBankAccount { get; set; }

    public DateTime Date { get; set; }

    public string Description {  get; set; }

    public float Amount { get; set; } // Base amount w/o taxes

    public string Image { get; set; }

    public int HajjSeasonsId { get;set; }
    public HajjSeasons HajjSeasons {  get; set; }

    public float WithholdingTax { get; set; }
    public float AdvanceTax { get; set; }
    public float SalesTax { get; set; }
    public float MiscTax { get; set; }

    [NotMapped]
    public float TotalAmount => Amount + WithholdingTax + AdvanceTax + SalesTax + MiscTax;

    [NotMapped] public IFormFile ImageFile {  get; set; }

}

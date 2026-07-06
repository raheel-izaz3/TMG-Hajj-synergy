using TMG.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Models.AccountModels
{
    public record IncomeStatementModel
    {
        public List<VoucherEntry> ExpenseEntries { get; set; }
        public float TotalExpense { get; set; }
        public List<VoucherEntry> RevenueEntries { get; set; }
        public float TotalRevenue { get; set; }
        public float NetIncome { get; set; }


    }

    namespace TMG.Controllers.AccountControllers
    {
        public class IncomeStatementController : Controller
        {
            private readonly TMGDbContext _dbContext;

            public IncomeStatementController(TMGDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public IActionResult Index()
            {
                var entries = _dbContext.VoucherEntries
                    .Include(e => e.Account)
                    .ThenInclude(e => e.Category)
                    .ThenInclude(e => e.SubType)
                    .ThenInclude(e => e.Type)
                    .Where(e => e.Account.Category.SubType.TypeId == 2 || e.Account.Category.SubType.TypeId == 3);

                var expenseEntries = entries.Where(e => e.Account.Category.SubType.TypeId == 2).ToList();
                var revenueEntries = entries.Where(e => e.Account.Category.SubType.TypeId == 3).ToList();

                float totalExpense = expenseEntries.Sum(e => e.Debit);
                float totalRevenue = revenueEntries.Sum(e => e.Credit);

                float netIncome = totalRevenue - totalExpense;

                IncomeStatementModel model = new IncomeStatementModel
                {
                    ExpenseEntries = expenseEntries,
                    RevenueEntries = revenueEntries,
                    TotalExpense = totalExpense,
                    TotalRevenue = totalRevenue,
                    NetIncome = netIncome
                };

                return View(model);
            }
        }
    }
}

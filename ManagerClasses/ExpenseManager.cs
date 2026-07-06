using TMG.Data;
using TMG.Models.AccountModels;

namespace TMG.ManagerClasses;

public static class ExpenseManager
{
    public static float GetTotalExpense(TMGDbContext dbContext)
    {
        float totalExpenses = dbContext.Expenses
            .Sum(e => e.Amount);

        return totalExpenses;
    }

}

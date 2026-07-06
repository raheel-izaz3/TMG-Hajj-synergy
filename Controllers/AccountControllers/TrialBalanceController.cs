using TMG.Data;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Models.AccountModels
{
    public record TrialBalanceModel(Category Category, float Debit, float Credit);
}

namespace TMG.Controllers.AccountControllers
{
    public class TrialBalanceController : Controller
    {

        private readonly TMGDbContext _dbContext;

        public TrialBalanceController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var categories = _dbContext.Categories.Include(e => e.SubType) as IEnumerable<Category>;
            var vouchers = _dbContext.PaymentVouchersWithoutTrash;
            var voucherEntries = _dbContext.VoucherEntries.Include(e => e.Account) as IEnumerable<VoucherEntry>;

            //return Ok(vouchers);

            List<TrialBalanceModel> models = new List<TrialBalanceModel>();

            for (int i = 0; i < categories.Count(); i++)
            {
                var currenyCategory = categories.ElementAt(i);
                float totalDebit = 0, totalCredit = 0;

                totalDebit = voucherEntries.Where(e => e.Account.CategoryId == currenyCategory.CategoryId).Sum(e => e.Debit);
                totalCredit = voucherEntries.Where(e => e.Account.CategoryId == currenyCategory.CategoryId).Sum(e => e.Credit);

                models.Add(new TrialBalanceModel(currenyCategory, totalDebit, totalCredit));
            }

            return View(models);
        }
    }
}

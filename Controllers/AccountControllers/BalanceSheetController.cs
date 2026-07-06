using TMG.Data;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Models.AccountModels
{
    public record BalanceSheetModel
    {
        public List<VoucherEntry> AssetEntries { get; set; }
        public List<SubType> AssetSubTypes {  get; set; }
        public List<Category> AssetCategories {  get; set; }
        public List<float> AssetCategoryWiseAmounts {  get; set; }
        public float AssetTotal { get; set; }

        public List<VoucherEntry> LiabilityEntries { get; set; }
        public List<SubType> LiabilitySubTypes { get; set; }
        public List<Category> LiabilityCategories { get; set; }
        public List<float> LiabilityCategoryWiseAmounts { get; set; }
        public float LiabilityTotal { get; set; }


        public List<VoucherEntry> EquityEntries { get; set; }
        public List<SubType> EquitySubTypes { get; set; }
        public List<Category> EquityCategories { get; set; }
        public List<float> EquityCategoryWiseAmounts { get; set; }
        public float EquityTotal { get; set; }

        public float NetIncome { get; set; }
    }
}

namespace TMG.Controllers.AccountControllers
{
    public class BalanceSheetController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public BalanceSheetController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var entries = _dbContext.VoucherEntries
                    .Include(e => e.Account)
                    .ThenInclude(e => e.Category)
                    .ThenInclude(e => e.SubType)
                    .ThenInclude(e => e.Type);


            var assetEntries = entries.Where(e => e.Account.Category.SubType.TypeId == 1).ToList();
            float assetTotal = assetEntries.Sum(e => e.Debit);

            var assetSubTypes = _dbContext.SubTypes.Where(e => e.TypeId == 1).ToList();
            var assetCategories = _dbContext.Categories.Include(e => e.SubType).Where(e => e.SubType.TypeId == 1).ToList();

            var assetCategoryAmounts = new List<float>();

            for(int i = 0; i < assetCategories.Count; i++)
            {
                var currentCategory = assetCategories[i];

                var categoryEntries = assetEntries.Where(e => e.Account.CategoryId == currentCategory.CategoryId);
                List<float> categoryEntryAmounts = new List<float>();
                if(categoryEntries.Count() == 0)
                {
                    categoryEntryAmounts.Add(0);
                }
                for(int j = 0; j < categoryEntries.Count(); j++)
                {
                    var currentCategoryEntry = categoryEntries.ElementAt(j);
                    if (currentCategoryEntry.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Debit)
                    {
                        categoryEntryAmounts.Add(currentCategoryEntry.Debit);
                    }
                    else if(currentCategoryEntry.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Credit)
                    {
                        categoryEntryAmounts.Add(-currentCategoryEntry.Credit);
                    }
                }

                assetCategoryAmounts.Add(categoryEntryAmounts.Sum());

                
            }




            var liabilityEntries = entries.Where(e => e.Account.Category.SubType.TypeId == 4).ToList();
            float liabilityTotal = liabilityEntries.Sum(e => e.Debit);

            var liabilitySubTypes = _dbContext.SubTypes.Where(e => e.TypeId == 4).ToList();
            var liabilityCategories = _dbContext.Categories.Include(e => e.SubType).Where(e => e.SubType.TypeId == 4).ToList();

            var liabilityCategoryAmounts = new List<float>();

            for (int i = 0; i < liabilityCategories.Count; i++)
            {
                var currentCategory = liabilityCategories[i];

                var categoryEntries = liabilityEntries.Where(e => e.Account.CategoryId == currentCategory.CategoryId);
                List<float> categoryEntryAmounts = new List<float>();
                if (categoryEntries.Count() == 0)
                {
                    categoryEntryAmounts.Add(0);
                }
                for (int j = 0; j < categoryEntries.Count(); j++)
                {
                    var currentCategoryEntry = categoryEntries.ElementAt(j);
                    if (currentCategoryEntry.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Credit)
                    {
                        categoryEntryAmounts.Add(currentCategoryEntry.Credit);
                    }
                    else if (currentCategoryEntry.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Debit)
                    {
                        categoryEntryAmounts.Add(-currentCategoryEntry.Debit);
                    }
                }

                liabilityCategoryAmounts.Add(categoryEntryAmounts.Sum());


            }



            var equityEntries = entries.Where(e => e.Account.Category.SubType.TypeId == 5).ToList();
            float equityTotal = equityEntries.Sum(e => e.Debit);

            var equitySubTypes = _dbContext.SubTypes.Where(e => e.TypeId == 5).ToList();
            var equityCategories = _dbContext.Categories.Include(e => e.SubType).Where(e => e.SubType.TypeId == 5).ToList();

            var equityCategoryAmounts = new List<float>();

            for (int i = 0; i < equityCategories.Count; i++)
            {
                var currentCategory = equityCategories[i];

                var categoryEntries = equityEntries.Where(e => e.Account.CategoryId == currentCategory.CategoryId);
                List<float> categoryEntryAmounts = new List<float>();
                if (categoryEntries.Count() == 0)
                {
                    categoryEntryAmounts.Add(0);
                }
                for (int j = 0; j < categoryEntries.Count(); j++)
                {
                    var currentCategoryEntry = categoryEntries.ElementAt(j);
                    if (currentCategoryEntry.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Credit)
                    {
                        categoryEntryAmounts.Add(currentCategoryEntry.Credit);
                    }
                    else if (currentCategoryEntry.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Debit)
                    {
                        categoryEntryAmounts.Add(-currentCategoryEntry.Debit);
                    }
                }

                equityCategoryAmounts.Add(categoryEntryAmounts.Sum());


            }

            BalanceSheetModel model = new BalanceSheetModel
            {
                AssetEntries = assetEntries,
                AssetTotal = assetTotal,
                AssetSubTypes = assetSubTypes,
                AssetCategories = assetCategories,
                AssetCategoryWiseAmounts = assetCategoryAmounts,

                LiabilityEntries = liabilityEntries,
                LiabilityTotal = liabilityTotal,
                LiabilitySubTypes = liabilitySubTypes,
                LiabilityCategories = liabilityCategories,
                LiabilityCategoryWiseAmounts = liabilityCategoryAmounts,

                EquityEntries = equityEntries,
                EquityTotal = equityTotal,
                EquitySubTypes = equitySubTypes,
                EquityCategories = equityCategories,
                EquityCategoryWiseAmounts = equityCategoryAmounts,
                NetIncome = CalculateNetIncome()
            };

            return View(model);
        }

        public float CalculateNetIncome()
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
            return netIncome;
        }
    }
}

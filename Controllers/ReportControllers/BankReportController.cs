using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TMG.Controllers.CompanyControllers;
using TMG.Controllers.FamilyControllers;
using TMG.Data;
using TMG.Models.AccountModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace TMG.Controllers
{
    public partial class BankReportController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;

        public BankReportController(TMGDbContext dbContext, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _http = http;
        }

        public IActionResult LedgerReportSelection()
        {
            ViewBag.CompanyBankAccounts = _dbContext.CompanyBankAccountsWithoutTrash;

            return View();
        }

        [HttpPost]
        public IActionResult LedgerReport(int accountId, string startDate, string endDate)
        {
            //return Ok(new
            //{
            //    startDate, endDate
            //});

            string queryForBalanceCF = @"WITH combined AS (
    SELECT 
        AmountRecieved AS Amount, 
        CompanyBankAccountId,
        'NewInvoice' AS Source
    FROM NewInvoice
    WHERE CAST(date AS DATE) < @startDate 
        AND CompanyBankAccountId = @accountId

    UNION ALL

    SELECT 
        AmountValue AS Amount, 
        CompanyBankAccountId,
        'Expenses' AS Source
    FROM 
    (
        SELECT 
            Date,
            Description,
            Amount,
            AdvanceTax,
            MiscTax,
            SalesTax,
            WithholdingTax,
            CompanyBankAccountId
        FROM Expenses
    ) AS SourceTable
    UNPIVOT
    (
        AmountValue FOR AmountType IN 
        (Amount, AdvanceTax, MiscTax, SalesTax, WithholdingTax)
    ) AS UnpivotedTable
    WHERE AmountValue > 0
        AND CompanyBankAccountId = @accountId
        AND CAST(date AS DATE) < @startDate
)

SELECT 
    CAST(COALESCE(SUM(CASE WHEN Source = 'NewInvoice' THEN Amount ELSE 0 END), 0) AS DECIMAL(18, 2)) -
    CAST(COALESCE(SUM(CASE WHEN Source = 'Expenses' THEN Amount ELSE 0 END), 0) AS DECIMAL(18, 2)) AS BalanceCF
FROM combined;

	";

            string query = @"WITH CombinedResults AS (
    SELECT 
        AmountRecieved AS Amount, 
        CompanyBankAccountId,
        'NewInvoice' AS Source,
        Date
    FROM NewInvoice
    WHERE CAST(date AS DATE) < @startDate
        AND CompanyBankAccountId = @accountId

    UNION ALL

    SELECT 
        AmountValue AS Amount, 
        CompanyBankAccountId,
        'Expenses' AS Source,
        Date
    FROM 
    (
        SELECT 
            Date,
            Description,
            Amount,
            AdvanceTax,
            MiscTax,
            SalesTax,
            WithholdingTax,
            CompanyBankAccountId
        FROM Expenses
    ) AS SourceTable
    UNPIVOT
    (
        AmountValue FOR AmountType IN 
        (Amount, AdvanceTax, MiscTax, SalesTax, WithholdingTax)
    ) AS UnpivotedTable
    WHERE AmountValue > 0
        AND CompanyBankAccountId = @accountId 
        AND CAST(date AS DATE) < @startDate
),

PreviousBalance AS (
    SELECT 
        SUM(CASE WHEN Source = 'NewInvoice' THEN Amount ELSE 0 END) 
        - SUM(CASE WHEN Source = 'Expenses' THEN Amount ELSE 0 END) AS BalanceBefore
    FROM CombinedResults
    WHERE CAST(Date AS DATE) < @startDate
    AND CompanyBankAccountId = @accountId
)

SELECT 
    Date,
    Description,
    CAST(Debit AS DECIMAL(18, 2)) AS Debit, 
    CAST(Credit AS DECIMAL(18, 2)) AS Credit,
    CAST(
        COALESCE(
            (SUM(Credit - Debit) OVER (ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)),
            0
        ) + COALESCE((SELECT BalanceBefore FROM PreviousBalance), 0)
        AS DECIMAL(18, 2)
    ) AS Balance
FROM (
    SELECT 
        Date, 
        inv.Remarks AS Description, 
        0 AS Debit,  -- Set Debit to 0 for Credit entries
        AmountRecieved AS Credit,  
        CompanyBankAccountId,
        'Credit' AS Type   
    FROM NewInvoice AS inv 
    WHERE inv.CompanyBankAccountId = @accountId

    UNION ALL

    SELECT 
        ExpenseDate,
        CASE 
            WHEN AmountType = 'Amount' THEN Description
            WHEN AmountType = 'SalesTax' THEN 'Sales Tax'
            WHEN AmountType = 'AdvanceTax' THEN 'Advance Tax'
            WHEN AmountType = 'MiscTax' THEN 'Misc Tax'
            WHEN AmountType = 'WithholdingTax' THEN 'Withholding Tax'
            ELSE AmountType
        END AS Description,
        AmountValue AS Debit,  
        0 AS Credit,  
        CompanyBankAccountId,
        'Debit' AS Type   
    FROM 
    (
        SELECT 
            Date AS ExpenseDate,
            Description,
            Amount,
            AdvanceTax,
            MiscTax,
            SalesTax,
            WithholdingTax,
            CompanyBankAccountId
        FROM Expenses
    ) AS SourceTable
    UNPIVOT
    (
        AmountValue FOR AmountType IN 
        (Amount, AdvanceTax, MiscTax, SalesTax, WithholdingTax)
    ) AS UnpivotedTable
    WHERE AmountValue > 0
    AND CompanyBankAccountId = @accountId
) AS CombinedResults
WHERE cast(Date as date) >= @startDate
    AND cast(Date as date) <= @endDate
ORDER BY Date;
";

            var parameters = new[]
            {
                new SqlParameter("@accountId", accountId),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            };

            var parametersForBalanceCFQuery = new[]
            {
                new SqlParameter("@accountId", accountId),
                new SqlParameter("@startDate", startDate),
            };

            var result = _dbContext.LedgerReportDto
               .FromSqlRaw(query, parameters).ToList();

            var resultBalanceCf = _dbContext.BalanceCFDto
               .FromSqlRaw(queryForBalanceCF, parametersForBalanceCFQuery).ToList();


            CompanyBankAccount cba = _dbContext.CompanyBankAccounts
                .Include(e => e.Company)
                .FirstOrDefault(e => e.CompanyBankAccountId == accountId);

            ViewBag.Title = cba.Title;
            string formattedStartDate = DateTime.Parse(startDate).ToString("dd/MMM/yyyy");
            string formattedEndDate = DateTime.Parse(endDate).ToString("dd/MMM/yyyy");

            ViewBag.BalanceCF = resultBalanceCf.ElementAtOrDefault(0).BalanceCF;
            ViewBag.Period = $"{formattedStartDate} - {formattedEndDate}";
            ViewBag.CompanyLogo = cba.Company.Logo;
            ViewBag.CompanyName = cba.Company.CompanyName;


            return View(result);
        }

    }
}

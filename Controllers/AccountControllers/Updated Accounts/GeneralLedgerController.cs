using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models.AccountModels;

namespace TMG.Controllers.AccountControllers.Updated_Accounts;
public class GeneralLedgerController : Controller
{
    private readonly TMGDbContext _context;
    private readonly IWebHostEnvironment _host;
    private readonly IHttpContextAccessor _http;

    public GeneralLedgerController(TMGDbContext context, IWebHostEnvironment host, IHttpContextAccessor http)
    {
        _context = context;
        _host = host;
        _http = http;
    }

    public IActionResult Ledger(int id, bool isBank)
    {
        var query = from gl in _context.GeneralLedgers
                    join gle in _context.GeneralLedgerEntries on gl.GeneralLedgerId equals gle.GeneralLedgerId
                    join a in _context.Account on gle.AccountsId equals a.AccountsId
                    join ag in _context.AccountGroups on a.AccountGroupsId equals ag.AccountGroupId
                    join at in _context.AccountTypes on ag.AccountTypesId equals at.AccountTypeId
                    where gl.Trashed == false && a.AccountsId == id
                    orderby gle.GeneralLedgerEntryId
                    select new
                    {
                        gle.Date,
                        gle.GeneralLedgerEntryId,
                        gle.Description,
                        Debit = gle.AccountNature == AccountNature.Debit ? gle.Amount : 0,
                        Credit = gle.AccountNature == AccountNature.Credit ? gle.Amount : 0,
                        AccountNature = at.AccountNature,
                        a.OpeningBalance
                    };

        var result = query
            .AsEnumerable()
            .Select((entry, index) =>
            {
                float balance = 0;

                if (isBank)
                {
                    balance = query.FirstOrDefault().OpeningBalance ?? 0;
                }

                balance += query.Take(index + 1).Sum(e =>
                    e.AccountNature == 0
                        ? (e.Debit - e.Credit)
                        : (e.Credit - e.Debit)
                );

                return new
                {
                    Date = entry.Date.ToString("dd/MMM/yyyy"),
                    entry.GeneralLedgerEntryId,
                    entry.Description,
                    entry.Debit,
                    entry.Credit,
                    Balance = balance,
                    entry.OpeningBalance
                };
            })
            .ToList();

        if (isBank)
        {
            var OB = _context.Account.Where(a => a.AccountsId == id && a.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).FirstOrDefault();
            ViewBag.OpeningBalance = OB?.OpeningBalance ?? 0;
        }

        ViewBag.IsBank = isBank;

        return View(result);
    }

}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Diagnostics;
using System.Text;
using TMG.Data;
using TMG.Interface;
using TMG.Models.FamilyModels;
using TMG.Models.FormModels.FamilyModels;

public class DailyEmailJob : IJob
{
    private readonly IEmailService _emailService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DailyEmailJob> _logger;
    private readonly IHttpContextAccessor _http;

    public DailyEmailJob(IEmailService emailService, IServiceScopeFactory serviceScopeFactory, ILogger<DailyEmailJob> logger, IHttpContextAccessor http)
    {
        _emailService = emailService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _http = http;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await SendSPOReportEmailsAsync();
            await SendDayWiseFollowUpEmailsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the DailyEmailJob.");
        }
    }

    private async Task SendSPOReportEmailsAsync()
    {
        var spoData = await GetSPODataAsync();

        foreach (var entry in spoData)
        {
            try
            {
                string spoName = entry.Value?.FirstOrDefault()?.SPOName ?? "SPO";
                string currentDate = DateTime.Now.ToString("dd/MMM/yyyy");

                string emailBody = GenerateEmailBody(entry.Value);
                string subject = $"Data Report_{spoName}_{currentDate}";
                await _emailService.SendEmailAsync(entry.Key, subject, emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SPO report email to {entry.Key}");
            }
        }
    }

    private async Task SendDayWiseFollowUpEmailsAsync(DateTime? specificDate = null)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TMGDbContext>();

            var portfolioEmails = await dbContext.CustomerPortfolioEmails
                .Include(cp => cp.CustomerPortfolio)
                .Where(cp => !cp.CustomerPortfolio.Trashed 
                    && cp.CustomerPortfolio.DateOfEmail != null
                    && cp.Email != null)
                .ToListAsync();

            foreach (var portfolioEmail in portfolioEmails)
            {
                try
                {
                        var emailData = await dbContext.EmailDatas
                            .FirstOrDefaultAsync(ed => ed.SpecificDate.HasValue 
                                                        && ed.SpecificDate.Value.Date == DateTime.Today
                                                        && !ed.Trashed);

                        if (emailData != null)
                        {
                            await _emailService.SendEmailAsync(
                                portfolioEmail.Email,
                                emailData.Subject,
                                emailData.Email
                            );

                            _logger.LogInformation($"Sent email for specific date {specificDate.Value.ToString("dd/MMM/yyyy")} to {portfolioEmail.Email}");
                            continue;
                        }
                        else
                        {
                            _logger.LogWarning($"No email data found for specific date: {specificDate.Value.ToString("dd/MMM/yyyy")}");
                        }

                    var daysSinceQuery = (DateTime.Today - portfolioEmail.CustomerPortfolio.DateOfEmail)?.Days;

                    var fallbackEmailData = await dbContext.EmailDatas
                        .FirstOrDefaultAsync(ed => ed.Day == daysSinceQuery && !ed.Trashed);

                    if (fallbackEmailData != null)
                    {
                        await _emailService.SendEmailAsync(
                            portfolioEmail.Email,
                            fallbackEmailData.Subject,
                            fallbackEmailData.Email
                        );

                        _logger.LogInformation($"Sent follow-up email for day {daysSinceQuery} to {portfolioEmail.Email}");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending follow-up email to {portfolioEmail.Email}");
                }
            }
        }
    }

    public async Task<Dictionary<string, List<CustomerPortfolioViewModel>>> GetSPODataAsync()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TMGDbContext>();

            var data = await dbContext.CustomerPortfolios
                .Include(e => e.CustomerRemarks)
                .Include(e => e.CustomerStatus)
                .Include(cp => cp.SPO)
                .Include(cp => cp.Countries)
                .Include(e => e.Reference)
                .Include(cp => cp.City)
                .Join(dbContext.CustomerPortfolioContacts, cp => cp.CustomerPortfolioId, cpc => cpc.CustomerPortfolioId, (cp, cpc) => new { cp, cpc })
                .Join(dbContext.CustomerPortfolioEmails, combined => combined.cp.CustomerPortfolioId, cpe => cpe.CustomerPortfolioId, (combined, cpe) => new
                {
                    combined.cp.SPO.Email,
                    FullName = combined.cp.GivenName + " " + combined.cp.SurName,
                    DateOfQuery = combined.cp.DateOfQuery,
                    CustomerPortfolioId = combined.cp.CustomerPortfolioId,
                    CustomerStatus = combined.cp.CustomerStatus.Status,
                    CustomerRemarks = combined.cp.CustomerRemarks.Remarks,
                    SPOName = combined.cp.SPO.Name,
                    CountryName = combined.cp.Countries.Name,
                    CityName = combined.cp.City.Name,
                    ApplicantType = combined.cp.ApplicantType,
                    Contact = combined.cpc.Preferred == true ? combined.cpc.Contact : null,
                    EmailContact = cpe.Preferred == true ? cpe.Email : null,
                    HajjDepartment = combined.cp.HajjDepartment.Name,
                    Country= combined.cp.Countries.Name,
                    City= combined.cp.City.Name,
                    IsHead = combined.cp.IsHead,
                    Trashed = combined.cp.Trashed,
                    FeedBy = combined.cp.FeedBy,
                    Refferel = combined.cp.Reference.ReferenceName
                })
                .Where(x => !x.Email.Contains("null") && x.IsHead && !x.Trashed)
                .GroupBy(x => x.Email)
                .Select(g => new
                {
                    Email = g.Key,
                    AssignedData = g.Select(x => new CustomerPortfolioViewModel
                    {
                        CustomerPortfolioId = x.CustomerPortfolioId,
                        FullName = x.FullName,
                        DateOfQuery = x.DateOfQuery,
                        CustomerStatus = x.CustomerStatus,
                        CustomerRemarks = x.CustomerRemarks,
                        SPOName = x.SPOName,
                        CountryName = x.CountryName,
                        CityName = x.CityName,
                        ApplicantType = x.ApplicantType,
                        Contact = x.Contact,
                        Email = x.EmailContact,
                        HajjDepartment = x.HajjDepartment,
                        Feedby = x.FeedBy,
                        ReferenceName = x.Refferel,
                    }).ToList()
                })
                .ToDictionaryAsync(x => x.Email, x => x.AssignedData);

            foreach (var entry in data)
            {
                int serialNumber = 1;
                foreach (var item in entry.Value)
                {
                    item.SerialNumber = serialNumber++;
                }
            }

            return data;
        }
    }

    public string GenerateEmailBody(List<CustomerPortfolioViewModel> spoData)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("<strong>Dear Sales Officer</strong>");
        sb.AppendLine("<br/>");
        sb.AppendLine("<br/>");
        sb.AppendLine("Your Daily Performance & Inquiry Statement for the duration of the last 30 days is attached. Kindly update your status and remarks accordingly to review.");
        sb.AppendLine("<br/>");
        sb.AppendLine("<br/>");
        sb.AppendLine("<table border='1'>");
        sb.AppendLine("<tr><th>#</th><th>FeedBy</th><th>SPO</th><th>Refferel</th><th>Enquiry</th><th>DateOfQuery</th><th>Applicant Type</th><th>Country</th><th>City</th><th>Status</th><th>Remarks</th><th>Contact</th><th>Email</th><th>Hajj Department</th></tr>");
        foreach (var item in spoData)
        {
            sb.AppendLine($"<tr><td>{item.SerialNumber}</td><td>{item.Feedby ?? "N/A"}</td><td>{item.SPOName ?? "N/A"}</td><td>{item.ReferenceName ?? "N/A"}</td><td>{item.FullName ?? "N/A"}</td><td>{item.DateOfQuery.ToString("dd/MMM/yyyy") ?? "N/A"}</td><td>{item.ApplicantType.ToString()}</td><td>{item.CountryName}</td><td>{item.CityName}</td><td>{item.CustomerStatus ?? "N/A"}</td><td>{item.CustomerRemarks ?? "N/A"}</td><td>{item.Contact ?? "N/A"}</td><td>{item.Email ?? "N/A"}</td><td>{item.HajjDepartment ?? "N/A"}</td></tr>");
        }
        sb.AppendLine("</table>");

        sb.AppendLine("<br/>");
        sb.AppendLine("<br/>");
        sb.AppendLine("<img src='https://hajj.tmgpk.com/Promotion.jpg' style='width: 25%;'/>");
        sb.AppendLine("<ul>");
        sb.AppendLine("<li><a href='www.travelmate.com.pk'>www.travelmate.com.pk</a></li>");
        sb.AppendLine("<li><a href='www.youtube.com/travelmate1459'>www.youtube.com/travelmate1459</a></li>");
        sb.AppendLine("<li><a href='https://www.facebook.com/TravelMate'>https://www.facebook.com/TravelMate</a></li>");
        sb.AppendLine("<li><a href='www.insta.com/travelmategroup'>www.insta.com/travelmategroup</a></li>");
        sb.AppendLine("</ul>");

        sb.AppendLine("<p>Regards,<br/>Management TMG</p>");

        return sb.ToString();
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;
using TMG.Data;
using DinkToPdf.Contracts;
using DinkToPdf;
using System.Net.Mail;
using System.Net;
using TMG.ManagerClasses;
using Microsoft.EntityFrameworkCore;
using TMG.Models.FamilyModels;
using TMG.Models.PackageModels;
using iText.Commons.Utils;
using TMG.Models;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using TMG.Models.FamilyHeadReportModels;
using TMG.Models.AccountModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using Humanizer;
using TMG.Models.CompanyModels;
using System.Text.RegularExpressions;
using Grpc.Core;
using NuGet.DependencyResolver;
using jsreport.Local;
using jsreport.Types;
using TMG.Models.FormModels.FamilyModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Mime;
//using AspNetCore;

namespace TMG.Controllers
{
    public class EmailController : Controller
    {
        public record HajjiData
        {
            public int FamilyHeadId { get; set; }
            public string FullName { get; set; }
            public string HbNo { get; set; }
            public string Email { get; set; }
            public string GroupName { get; set; }
        }

        public class InvoiceRequest
        {
            public string PdfBase64 { get; set; }
            public string PdfPackageBase64 { get; set; }
            public int Id { get; set; }
        }

        private readonly IConverter _converter;
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;
        private readonly IConverter _pdfConverter;

        public EmailController(TMGDbContext dbContext, IConverter converter, IWebHostEnvironment host, IHttpContextAccessor http, IConverter pdfConverter)
        {
            _dbContext = dbContext;
            _converter = converter;
            _host = host;
            _http = http;
            _pdfConverter = pdfConverter;
        }

        [HttpPost]
        public IActionResult SendVendorCertificate(string pdfBase64, int id)
        {
            string email = _dbContext.VendorContactPersons
                .Where(e => e.VendorContactPersonId == id)
                .Select(e => e.Email)
                .FirstOrDefault();

            byte[] imageBytes = Convert.FromBase64String(pdfBase64.Substring(pdfBase64.IndexOf(",") + 1));

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG");
            var toAddress = new MailAddress(email);
            string subject = $"Certificate";
            const string body = @"Dear Patrons,

We appreciate your outstanding service and collaboration. Your dedication has been invaluable to Travel Mate Group (TMG) and its success.

We appreciate your hospitality & support to our needs. Your contributions have significantly achieved our operational efficiency.

Thank you again for your exceptional support.

Regards, 
Team TMG";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");
            var ccAddress2 = new MailAddress("noman.siddiqui@travelmate.com.pk", "CC Recipient");
            var ccAddress3 = new MailAddress("accounts@travelmate.com.pk", "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "mail.smtp2go.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("newslettertmg", "0JZ7F6rZg1dXWzWx")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            mail.CC.Add(ccAddress);
            mail.CC.Add(ccAddress2);
            mail.CC.Add(ccAddress3);

            var memoryStream = new MemoryStream(imageBytes);
            var attachment = new Attachment(memoryStream, $"Certificate.png", "image/png");
            mail.Attachments.Add(attachment);

            smtp.Send(mail);

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SendCertificate(string pdfBase64, int id)
        {
            string email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Email)
                .FirstOrDefault();

            var hajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            var hbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            var pkgName = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Package.Name)
                .FirstOrDefault();

            var spoEmail = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.SPO)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.SPO.Email)
                .FirstOrDefault();

            byte[] imageBytes = Convert.FromBase64String(pdfBase64.Substring(pdfBase64.IndexOf(",") + 1));

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG");
            var toAddress = new MailAddress(email);
            string subject = $"HajjCertificate_{hajjiName}_{hbNo}";
            const string body = @"Dear Pilgrim,

GREETINGS FROM TMG!

Hajj e Mabror

Assalam Alaikum,

I hope you are doing well. Please find the attached certificate of the TMG Hajj 2025.

May Allah Accept your Hajj

Regards";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");
            var ccAddress2 = new MailAddress("noman.siddiqui@travelmate.com.pk", "CC Recipient");
            var ccAddress3 = new MailAddress("accounts@travelmate.com.pk", "CC Recipient");
            var ccAddress4 = new MailAddress(spoEmail, "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            mail.CC.Add(ccAddress);
            mail.CC.Add(ccAddress2);
            mail.CC.Add(ccAddress3);
            mail.CC.Add(ccAddress4);

            var memoryStream = new MemoryStream(imageBytes);
            var attachment = new Attachment(memoryStream, $"HajjCertificate_{hajjiName}_{hbNo}_{pkgName}.png", "image/png");
            mail.Attachments.Add(attachment);

            smtp.Send(mail);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SendCard(IFormFile pdfFile, int id)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Invalid PDF file.");
            }

            string email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Email)
                .FirstOrDefault();

            var hajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            var hbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            var pkgName = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Package.Name)
                .FirstOrDefault();

            var spoEmail = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.SPO)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.SPO.Email)
                .FirstOrDefault();

            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                await pdfFile.CopyToAsync(ms);
                pdfBytes = ms.ToArray();
            }

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG - Subscription");
            var toAddress = new MailAddress(email);
            string subject = $"Documents_{hajjiName}_{hbNo}";

            const string body = @"Dear Passenger,

Welcome to TMG!!!
Your Ultimate travel Partner and Complete Travel House since over 2 decades and serving over Millions of pilgrims.

Kindly find enclosed the Performa Invoice for your record;

Should you have any query or discrepancy kindly contact to your assigned officer.
Hajj Terms & Condition available for your review;

For Further FAQs for Hajj 2025, please find below link
https://www.travelmate.com.pk/tmg-hajj-packages-faqs/

https://hajj.tmgpk.com/Promotion.jpg?width=25%

www.travelmate.com.pk
www.youtube.com/travelmate1459
https://www.facebook.com/TravelMate
www.insta.com/travelmategroup

Regards,
Team TMG";

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            var memoryStream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(memoryStream, $"Documents_{hajjiName}_{hbNo}_{pkgName}.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            smtp.Send(mail);

            return Json(new { success = true });
        }

        public IActionResult HajjiForm()
        {
            var query = _dbContext.DataForms
                .Where(e => e.IsHead && !e.Trashed);

            ViewBag.FamilyHeads = query.Select(fh => new HajjiData
            {
                FamilyHeadId = fh.DataFormId,
                FullName = $"{fh.GivenName + ' ' + fh.SurName }",
                Email = fh.Email
            }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendHajjEmail(string subject, string content, IFormFile file, int[] FamilyHeadIds, bool[] FamilyHeadsSelected)
        {
            var selectedIds = new List<int>();

            for (int i = 0; i < FamilyHeadIds.Length; i++)
            {
                if (FamilyHeadsSelected[i])
                {
                    selectedIds.Add(FamilyHeadIds[i]);
                }
            }

            if (selectedIds.Count == 0)
            {
                return Json(new { success = false, message = "No Hajji selected." });
            }

            var selectedFamilyHeads = _dbContext.DataForms
                .Where(f => selectedIds.Contains(f.DataFormId))
                .ToList();

            foreach (var familyHead in selectedFamilyHeads)
            {
                var preferredEmails = familyHead.Email
                   .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(e => e.Trim())
                   .ToList();

                foreach (var email in preferredEmails)
                {
                    await SendEmailAsync(email, subject, content, file);
                }
            }

            return Json(new { success = true, message = "Emails sent successfully." });
        }

        public IActionResult InquiryHajjiForm()
        {
            var query = _dbContext.CustomerPortfolios
                .Where(e => e.IsHead && !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            ViewBag.FamilyHeads = query.Select(fh => new HajjiData
            {
                FamilyHeadId = fh.CustomerPortfolioId,
                FullName = fh.FullName,
                Email = _dbContext.CustomerPortfolioEmails
                    .Where(e => e.CustomerPortfolioId == fh.CustomerPortfolioId
                            && e.Preferred)
                    .Select(e => e.Email)
                    .FirstOrDefault()
            }).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendInquiryHajjiEmail(string subject, string content, IFormFile file, int[] FamilyHeadIds, bool[] FamilyHeadsSelected)
        {
            var selectedIds = new List<int>();

            for (int i = 0; i < FamilyHeadIds.Length; i++)
            {
                if (FamilyHeadsSelected[i])
                {
                    selectedIds.Add(FamilyHeadIds[i]);
                }
            }

            if (selectedIds.Count == 0)
            {
                return Json(new { success = false, message = "No Hajji selected." });
            }

            var selectedFamilyHeads = _dbContext.CustomerPortfolios
                .Include(e => e.CustomerPortfolioEmails)
                .Where(f => selectedIds.Contains(f.CustomerPortfolioId))
                .ToList();

            foreach (var familyHead in selectedFamilyHeads)
            {
                var preferredEmails = familyHead.CustomerPortfolioEmails
                    .Where(e => e.Preferred)
                    .Select(e => e.Email)
                    .ToList();

                foreach (var email in preferredEmails)
                {
                    await SendEmailAsync(email, subject, content, file);
                }
            }

            return Json(new { success = true, message = "Emails sent successfully." });
        }

        public IActionResult BookedHajjiForm()
        {
            var query = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.IsHead && e.HajjiBookingStatus == HajjiBookingStatus.Booked
                        && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")));

            ViewBag.FamilyHeads = query.Select(fh => new HajjiData
            {
                FamilyHeadId = fh.FamilyHeadId,
                FullName = fh.FullName,
                HbNo = fh.FcId,
                Email = _dbContext.FamilyHeadEmails
                    .Where(e => e.FamilyHeadId == fh.FamilyHeadId
                            && e.Preferred)
                    .Select(e => e.Email)
                    .FirstOrDefault(),

                GroupName = _dbContext.GroupMembers
                    .Include(e => e.Group)
                    .Where(e => e.FamilyHeadId == fh.FamilyHeadId
                            && !e.Group.Trashed)
                    .Select(e => e.Group.Name)
                    .FirstOrDefault()
            }).ToList();
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendHajjiEmail(string subject, string content, IFormFile file, int[] FamilyHeadIds, bool[] FamilyHeadsSelected)
        {
            var selectedIds = new List<int>();

            for (int i = 0; i < FamilyHeadIds.Length; i++)
            {
                if (FamilyHeadsSelected[i])
                {
                    selectedIds.Add(FamilyHeadIds[i]);
                }
            }

            if (selectedIds.Count == 0)
            {
                return Json(new { success = false, message = "No Hajji selected." });
            }

            var selectedFamilyHeads = _dbContext.FamilyHeads
                .Include(e => e.FamilyHeadEmails)
                .Where(f => selectedIds.Contains(f.FamilyHeadId))
                .ToList();

            foreach (var familyHead in selectedFamilyHeads)
            {
                var preferredEmails = familyHead.FamilyHeadEmails
                    .Where(e => e.Preferred)
                    .Select(e => e.Email)
                    .ToList();

                foreach (var email in preferredEmails)
                {
                    await SendEmailAsync(email, subject, content, file);
                }
            }

            return Json(new { success = true, message = "Emails sent successfully." });
        }

        private async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, IFormFile file)
        {
            var fromEmail = "newslettertmg";
            var fromPassword = "0JZ7F6rZg1dXWzWx";

            var smtpClient = new SmtpClient("mail.smtp2go.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = false,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@tmgpk.com", "TMG"),
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            var base64Regex = new Regex("<img[^>]+src=[\"']data:image/(?<type>[^;]+);base64,(?<data>[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase);
            var matches = base64Regex.Matches(bodyHtml);

            List<LinkedResource> linkedResources = new List<LinkedResource>();
            int counter = 1;

            foreach (Match match in matches)
            {
                var base64Data = match.Groups["data"].Value;
                var imageType = match.Groups["type"].Value;
                var imageBytes = Convert.FromBase64String(base64Data);

                var stream = new MemoryStream(imageBytes);
                var contentId = $"image{counter}@cid";

                var imageResource = new LinkedResource(stream, $"image/{imageType}")
                {
                    ContentId = contentId,
                    TransferEncoding = TransferEncoding.Base64,
                    ContentType = new ContentType($"image/{imageType}"),
                    ContentLink = new Uri($"cid:{contentId}")
                };

                linkedResources.Add(imageResource);

                bodyHtml = bodyHtml.Replace(match.Value, $"<img src=\"cid:{contentId}\" />");

                counter++;
            }

            var htmlView = AlternateView.CreateAlternateViewFromString(bodyHtml, null, MediaTypeNames.Text.Html);
            foreach (var res in linkedResources)
            {
                htmlView.LinkedResources.Add(res);
            }
            mailMessage.AlternateViews.Add(htmlView);

            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var attachment = new Attachment(new MemoryStream(ms.ToArray()), file.FileName, file.ContentType);
                mailMessage.Attachments.Add(attachment);
            }

            await smtpClient.SendMailAsync(mailMessage);
        }

        [HttpPost]
        public async Task<IActionResult> SendIDCardList(IFormFile pdfFile, IFormFile excelFile, string email)
        {
            try
            {
                var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG");
                var toAddress = new MailAddress(email);
                const string subject = "Here is the ID Card List";
                const string body = "Please find the attached ID Card List (PDF) and ID Card List (Excel) documents.";

                using (var smtp = new SmtpClient("smtp.tmgpk.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k");
                    smtp.EnableSsl = false;

                    using (var mail = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        if (pdfFile != null && pdfFile.Length > 0)
                        {
                            using (var pdfStream = new MemoryStream())
                            {
                                await pdfFile.CopyToAsync(pdfStream);
                                pdfStream.Position = 0;
                                mail.Attachments.Add(new Attachment(pdfStream, pdfFile.FileName, "application/pdf"));
                            }
                        }

                        if (excelFile != null && excelFile.Length > 0)
                        {
                            using (var excelStream = new MemoryStream())
                            {
                                await excelFile.CopyToAsync(excelStream);
                                excelStream.Position = 0;
                                mail.Attachments.Add(new Attachment(excelStream, excelFile.FileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                            }
                        }

                        await smtp.SendMailAsync(mail);
                    }
                }

                return Json(new { success = true, message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.ToString() });
            }
        }

        [HttpPost]
        public IActionResult SendLetterHeadEmail(string pdfBase64, string email)
        {
            byte[] pdfBytes = Convert.FromBase64String(pdfBase64.Substring(pdfBase64.IndexOf(",") + 1));

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG");
            var toAddress = new MailAddress(email);
            const string subject = "Here is your Letter Head";
            const string body = "Please find the attached LetterHead document.";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            mail.CC.Add(ccAddress);

            var memoryStream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(memoryStream, $"LetterHead.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            smtp.Send(mail);

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SendBooking(int id)
        {
            string email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Email ?? "-")
                .FirstOrDefault();

            var userEmail = _http.HttpContext.Session.GetString("UserEmail");

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG - Subscription");
            var toAddress = new MailAddress(email);
            const string subject = "Booked in TMG Hajj";
            string body = "Dear Passenger,<br/><br/>" +
                         "Welcome to TMG!!!<br/><br/>" +
                         "You are successfully booked in Hajj 2025 with Travel Mate.<br/><br/>" +
                         "<img src='https://hajj.tmgpk.com/Promotion.jpg' style='width: 25%;'/><br/><br/>" +
                         "<a href='https://www.travelmate.com.pk'>www.travelmate.com.pk</a><br/>" +
                         "<a href='https://www.youtube.com/travelmate1459'>www.youtube.com/travelmate1459</a><br/>" +
                         "<a href='https://www.facebook.com/TravelMate'>https://www.facebook.com/TravelMate</a><br/>" +
                         "<a href='https://www.insta.com/travelmategroup'>www.insta.com/travelmategroup</a><br/><br/>" +
                         "Regards,<br/>" +
                         "Team TMG";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");
            var ccAddress2 = new MailAddress(userEmail, "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.CC.Add(ccAddress);
            mail.CC.Add(ccAddress2);

            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }

            return RedirectToAction("Index", "FamilyHeads");
        }

        [HttpPost]
        public IActionResult SendHajjSlip(string pdfBase64, int id)
        {
            string email = _dbContext.NewInvoicesWithoutTrash
                        .Where(ni => ni.NewInvoiceId == id)
                        .Join(
                            _dbContext.FamilyHeads.Where(fh => fh.IsHead),
                            ni => ni.FamilyHeadId,
                            fh => fh.FamilyHeadId,
                            (ni, fh) => new { ni, fh }
                        )
                        .Join(
                            _dbContext.FamilyHeadEmails.Where(fhe => fhe.Preferred),
                            combined => combined.fh.FamilyHeadId,
                            fhe => fhe.FamilyHeadId,
                            (combined, fhe) => new
                            {
                                Email = fhe.Email,
                                Invoice = combined.ni,
                                FamilyHead = combined.fh
                            }
                        )
                        .Select(e => e.Email)
                        .FirstOrDefault();

            var hajjiName = _dbContext.NewInvoicesWithoutTrash
                .Include(e => e.FamilyHead)
                .Where(e => e.NewInvoiceId == id)
                .Select(e => e.FamilyHead.FullName)
                .FirstOrDefault();

            var hbNo = _dbContext.NewInvoicesWithoutTrash
                .Include(e => e.FamilyHead)
                .Where(e => e.NewInvoiceId == id)
                .Select(e => e.FamilyHead.FcId)
                .FirstOrDefault();

            var pkgName = _dbContext.NewInvoicesWithoutTrash
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.Package)
                .Where(e => e.NewInvoiceId == id)
                .Select(e => e.FamilyHead.Package.Name)
                .FirstOrDefault();

            var spoEmail = _dbContext.NewInvoicesWithoutTrash
                .Include(e => e.FamilyHead)
                .ThenInclude(e => e.SPO)
                .Where(e => e.NewInvoiceId == id)
                .Select(e => e.FamilyHead.SPO.Email)
                .FirstOrDefault();

            var rvNo = _dbContext.NewInvoicesWithoutTrash
                .Where(e => e.NewInvoiceId == id)
                .Select(e => e.ReceiptNumber)
                .FirstOrDefault();

            byte[] pdfBytes = Convert.FromBase64String(pdfBase64.Substring(pdfBase64.IndexOf(",") + 1));

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG - Finance");
            var toAddress = new MailAddress(email);
            string subject = $"HajjSlip_{hajjiName}_{hbNo}_{rvNo}";
            const string body = @"Dear Passenger,

Welcome to TMG!!!
Your Ultimate travel Partner and Complete Travel House since over 2 decades and serving over Millions of pilgrims.

Kindly find enclosed the Performa Invoice for your record;

Should you have any query or discrepancy kindly contact to your assigned officer.
Hajj Terms & Condition available for your review;

For Further FAQs for Hajj 2025, please find below link
https://www.travelmate.com.pk/tmg-hajj-packages-faqs/

https://hajj.tmgpk.com/Promotion.jpg?width=25%

www.travelmate.com.pk
www.youtube.com/travelmate1459
https://www.facebook.com/TravelMate
www.insta.com/travelmategroup

Regards,
Team TMG";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");
            var ccAddress2 = new MailAddress("noman.siddiqui@travelmate.com.pk", "CC Recipient");
            var ccAddress3 = new MailAddress("accounts@travelmate.com.pk", "CC Recipient");
            var ccAddress4 = new MailAddress(spoEmail, "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            mail.CC.Add(ccAddress);
            mail.CC.Add(ccAddress2);
            mail.CC.Add(ccAddress3);
            mail.CC.Add(ccAddress4);

            var memoryStream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(memoryStream, $"HajjSlip_{hajjiName}_{hbNo}_{pkgName}_{rvNo}.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            smtp.Send(mail);

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SendMiniStatement(string pdfBase64, int id)
        {
            string email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Email)
                .FirstOrDefault();

            var hajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            var hbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            var pkgName = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Package.Name)
                .FirstOrDefault();

            var spoEmail = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.SPO)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.SPO.Email)
                .FirstOrDefault();

            byte[] pdfBytes = Convert.FromBase64String(pdfBase64.Substring(pdfBase64.IndexOf(",") + 1));

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG - Finance");
            var toAddress = new MailAddress(email);
            string subject = $"MiniStatement_{hajjiName}_{hbNo}";
            const string body = @"Dear Passenger,

Welcome to TMG!!!
Your Ultimate travel Partner and Complete Travel House since over 2 decades and serving over Millions of pilgrims.

Kindly find enclosed the Performa Invoice for your record;

Should you have any query or discrepancy kindly contact to your assigned officer.
Hajj Terms & Condition available for your review;

For Further FAQs for Hajj 2025, please find below link
https://www.travelmate.com.pk/tmg-hajj-packages-faqs/

https://hajj.tmgpk.com/Promotion.jpg?width=25%

www.travelmate.com.pk
www.youtube.com/travelmate1459
https://www.facebook.com/TravelMate
www.insta.com/travelmategroup

Regards,
Team TMG";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");
            var ccAddress2 = new MailAddress("noman.siddiqui@travelmate.com.pk", "CC Recipient");
            var ccAddress3 = new MailAddress("accounts@travelmate.com.pk", "CC Recipient");
            var ccAddress4 = new MailAddress(spoEmail, "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            mail.CC.Add(ccAddress);
            mail.CC.Add(ccAddress2);
            mail.CC.Add(ccAddress3);
            mail.CC.Add(ccAddress4);

            var memoryStream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(memoryStream, $"MiniStatement_{hajjiName}_{hbNo}_{pkgName}.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            smtp.Send(mail);

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SendInvoice([FromBody] InvoiceRequest request)
        {
            // Fetch the necessary data from the database
            string email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == request.Id)
                .Select(e => e.Email)
                .FirstOrDefault();

            var hajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == request.Id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            var hbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == request.Id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            var pkgName = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == request.Id)
                .Select(e => e.Package.Name)
                .FirstOrDefault();

            var spoEmail = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.SPO)
                .Where(e => e.FamilyHeadId == request.Id)
                .Select(e => e.SPO.Email)
                .FirstOrDefault();

            // Convert the base64 PDF data for invoice and package
            byte[] pdfBytes = Convert.FromBase64String(request.PdfBase64.Substring(request.PdfBase64.IndexOf(",") + 1));
            byte[] pdfPackageBytes = null;
            if (!string.IsNullOrEmpty(request.PdfPackageBase64))
            {
                pdfPackageBytes = Convert.FromBase64String(request.PdfPackageBase64.Substring(request.PdfPackageBase64.IndexOf(",") + 1));
            }

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG - Finance");
            var toAddress = new MailAddress(email);
            string subject = $"ProformaInvoice_{hajjiName}_{hbNo}";
            const string body = @"Dear Passenger,

Welcome to TMG!!!
Your Ultimate travel Partner and Complete Travel House since over 2 decades and serving over Millions of pilgrims.

Kindly find enclosed the Performa Invoice for your record;

Should you have any query or discrepancy kindly contact your assigned officer.
Hajj Terms & Condition available for your review;

For Further FAQs for Hajj 2025, please find below link
https://www.travelmate.com.pk/tmg-hajj-packages-faqs/

https://hajj.tmgpk.com/Promotion.jpg?width=25%

www.travelmate.com.pk
www.youtube.com/travelmate1459
https://www.facebook.com/TravelMate
www.insta.com/travelmategroup

Regards,
Team TMG";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");
            var ccAddress2 = new MailAddress("noman.siddiqui@travelmate.com.pk", "CC Recipient");
            var ccAddress3 = new MailAddress("accounts@travelmate.com.pk", "CC Recipient");
            var ccAddress4 = new MailAddress(spoEmail, "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            mail.CC.Add(ccAddress);
            mail.CC.Add(ccAddress2);
            mail.CC.Add(ccAddress3);
            mail.CC.Add(ccAddress4);

            // Attach Invoice PDF
            var memoryStream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(memoryStream, $"ProformaInvoice_{hajjiName}_{hbNo}_{pkgName}.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            // Attach Package PDF if available
            if (pdfPackageBytes != null)
            {
                var packageMemoryStream = new MemoryStream(pdfPackageBytes);
                var packageAttachment = new Attachment(packageMemoryStream, $"PackageDetails_{hajjiName}_{hbNo}_{pkgName}.pdf", "application/pdf");
                mail.Attachments.Add(packageAttachment);
            }

            // Send email
            smtp.Send(mail);

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SendAgreement(string email, string url)
        {
            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG");
            var toAddress = new MailAddress(email);
            const string subject = "Here is your Hajj Agreement";
            string body = $@"Dear Passenger,<br/><br/>
Welcome to TMG!!!<br/><br/>
Your Ultimate travel Partner and Complete Travel House since over 2 decades and serving over Millions of pilgrims.<br/><br/>
Kindly find enclosed the DATA form for your review & Update and get it signed.<br/><br/>
Should you have any query or discrepancy kindly contact to your assigned officer<br/>
Hajj Terms & Condition available for your review;<br/><br/>
<a href='{url}' target='_blank'>Click Here</a> to proceed to the agreement.<br/><br/>
For Further FAQs for Hajj 2025, please find below link<br/>
<a href='https://www.travelmate.com.pk/tmg-hajj-packages-faqs/'>https://www.travelmate.com.pk/tmg-hajj-packages-faqs/</a><br/><br/>
<img src='https://hajj.tmgpk.com/Promotion.jpg' style='width: 25%;'/><br/><br/>
<a href='www.travelmate.com.pk'>www.travelmate.com.pk</a><br/>
<a href='www.youtube.com/travelmate1459'>www.youtube.com/travelmate1459</a><br/>
<a href='https://www.facebook.com/TravelMate'>https://www.facebook.com/TravelMate</a><br/>
<a href='www.insta.com/travelmategroup'>www.insta.com/travelmategroup</a><br/><br/>
Regards,<br/>
Team TMG";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.CC.Add(ccAddress);

            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SendMedia(string email, string link)
        {
            var userEmail = _http.HttpContext.Session.GetString("UserEmail");

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG - Media Channel");
            var toAddress = new MailAddress(email);
            const string subject = "Media Link";
            string body = "Dear Passenger,<br/><br/>" +
                         "Welcome to TMG!!!<br/><br/>" +
                         "Your Ultimate travel Partner and Complete Travel House since over 2 decades and serving over Millions of pilgrims.<br/><br/>" +
                         "Kindly find below the Media Channel link for your review;<br/><br/>" +
                         $"<a href='https://hajj.tmgpk.com{link}' target='_blank'>https://hajj.tmgpk.com{link}</a><br/><br/>" +
                         "<img src='https://hajj.tmgpk.com/Promotion.jpg' style='width: 25%;'/><br/><br/>" +
                         "<a href='https://www.travelmate.com.pk'>www.travelmate.com.pk</a><br/>" +
                         "<a href='https://www.youtube.com/travelmate1459'>www.youtube.com/travelmate1459</a><br/>" +
                         "<a href='https://www.facebook.com/TravelMate'>https://www.facebook.com/TravelMate</a><br/>" +
                         "<a href='https://www.insta.com/travelmategroup'>www.insta.com/travelmategroup</a><br/><br/>" +
                         "Regards,<br/>" +
                         "Team TMG";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");
            var ccAddress2 = new MailAddress(userEmail, "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.CC.Add(ccAddress);
            mail.CC.Add(ccAddress2);

            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }

            return RedirectToAction("Index", "EVideoFiling");
        }

        [HttpPost]
        public IActionResult SendEmailWithPdf(string pdfBase64, int id)
        {
            string email = _dbContext.FamilyHeadEmails
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Email)
                .FirstOrDefault();

            var hajjiName = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FullName)
                .FirstOrDefault();

            var hbNo = _dbContext.FamilyHeadsWithoutTrash
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.FcId)
                .FirstOrDefault();

            var pkgName = _dbContext.FamilyHeadsWithoutTrash
                .Include(e => e.Package)
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Package.Name)
                .FirstOrDefault();

            byte[] pdfBytes = Convert.FromBase64String(pdfBase64.Substring(pdfBase64.IndexOf(",") + 1));

            var fromAddress = new MailAddress("noreply@tmgpk.com", "TMG");
            var toAddress = new MailAddress(email);
            const string subject = "Here is your Itinerary";
            const string body = "Please find the attached Itinerary document.";

            var ccAddress = new MailAddress("cc@travelmate.com.pk", "CC Recipient");

            var smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                Port = 587,
                EnableSsl = false,
                Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k")
            };

            var mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            mail.CC.Add(ccAddress);

            var memoryStream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(memoryStream, $"Itinerary_{hajjiName}_{hbNo}_{pkgName}.pdf", "application/pdf");
            mail.Attachments.Add(attachment);

            smtp.Send(mail);

            return Json(new { success = true });
        }

        public async Task<IActionResult> SendDocuments(int id)
        {
            FamilyHead familyHead = _dbContext.FamilyHeads.Find(id);

            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            if (email is null)
            {
                return BadRequest(new
                {
                    status = 400
                });
            }

            MailMessage mm = new MailMessage("noreply@tmgpk.com", email);
            mm.IsBodyHtml = true;
            mm.Subject = "Attached Hajj Visa, Form & Airline Ticket";
            mm.Body = "";



            if (familyHead.VisaImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, familyHead.VisaImage);
                Attachment attachment = new Attachment(path);
                mm.Attachments.Add(attachment);
            }
            if (familyHead.InsuranceImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, familyHead.InsuranceImage);
                Attachment attachment = new Attachment(path);
                mm.Attachments.Add(attachment);
            }
            if (familyHead.SignedMISPdfPath is not null)
            {
                string path = Path.Combine(_host.WebRootPath, familyHead.SignedMISPdfPath);
                Attachment attachment = new Attachment(path);
                mm.Attachments.Add(attachment);
            }
            if (familyHead.AirlineTicketImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, familyHead.AirlineTicketImage);
                Attachment attachment = new Attachment(path);
                mm.Attachments.Add(attachment);
            }
            if (familyHead.TrainTicketImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, familyHead.TrainTicketImage);
                Attachment attachment = new Attachment(path);
                mm.Attachments.Add(attachment);
            }

            mm.IsBodyHtml = true;

            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "noreply@tmgpk.com",
                Password = "U7H#sT8sLXA^2!8k"
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.tmgpk.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 465
            };
            smtp.Send(mm);

            return Ok(new
            {
                status = 200
            });
        }

        public async Task<IActionResult> SendHajjAgreementPdfEmail(int id)
        {

            var agreement = _dbContext
                    .UserHajjAgreements.AsNoTracking()
                    .FirstOrDefault(e => e.FamilyHeadId == id);

            var familyHead = _dbContext.FamilyHeads
                .AsNoTracking()
                .Include(e => e.Honorifics)
                .Include(e => e.Fiqah)
                .Include(e => e.RelationWithMehram)
                .Include(e => e.Package)
                .ThenInclude(e => e.Company)
                .FirstOrDefault(e => e.FamilyHeadId == id);

            FamilyHeadAddress headAddress = _dbContext
                                        .FamilyHeadAddresses
                                        .AsNoTracking()
                                        .Include(e => e.Tehsil)
                                        .FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred);

            FamilyHeadContact preferredContact = _dbContext
                .FamilyHeadContacts
                .AsNoTracking()
                .FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred);

            FamilyHeadContact otherContact = _dbContext
                .FamilyHeadContacts
                .AsNoTracking()
                .FirstOrDefault(e => e.FamilyHeadId == id && !e.Preferred);

            FamilyHeadNominee headNominee = _dbContext
                .FamilyHeadNominees
                .AsNoTracking()
                .Include(e => e.NomineeRelation)
                .FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred);

            string email = _dbContext.FamilyHeadEmails.AsNoTracking().FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            string presentPostalAddress = $"{headAddress?.Address}, {headAddress?.City}, {headAddress?.PoBox}, {headAddress?.Country}";

            var officialAddress = _dbContext.FamilyHeadAddresses.AsNoTracking().FirstOrDefault(e => e.FamilyHeadId == id && e.MemberAddressTypeId == 2);
            var officialContact = _dbContext.FamilyHeadContacts.AsNoTracking().FirstOrDefault(e => e.FamilyHeadId == id && e.MemberContactTypeId == 2);
            var officialEmail = _dbContext.FamilyHeadEmails.AsNoTracking().FirstOrDefault(e => e.FamilyHeadId == id && e.MemberEmailTypeId == 2);

            string officialAddressString = officialAddress is not null ?
                                            $"{officialAddress?.Address}, {officialAddress?.City}, {officialAddress?.PoBox}, {officialAddress?.Country}" : "";

            var fm = _dbContext.FamilyMembers.Include(e => e.Head).AsNoTracking().FirstOrDefault(e => e.FamilyHeadId == id);
            var famCount = _dbContext.FamilyMembers.AsNoTracking().Where(e => e.HeadId == fm.HeadId).Count();

            string cId =
                            $"{familyHead.Package.Company.CompanyCode}.{familyHead.IndId}" +
                            $".{fm.Head.ExtractedFamilyNumber}.({famCount})" +
                            $".{familyHead.Package.Name}";

            FamilyHead mehram = _dbContext.FamilyHeads.FirstOrDefault(e => e.MehramFamilyHeadId == familyHead.FamilyHeadId && !e.Trashed);

            if (familyHead.Gender == Gender.Male)
            {
                // mehram is not null ? "YES" : "NO";
            }
            else
            {

                // model.AreYouMehramOfALady = "-";
                // model.MehramAppNumber = mehram?.HajjApplicationNumber;
                // model.MehramRelationship = currMember?.FamilyHead.RelationWithMehram?.Relation;
                // model.MehramSignatureBase64 = mehram?.SignatureBase64;
                // model.MehramName = mehram?.FullName;
            }

            float packageAmount = 0f;
            var individualFacilities =
                        _dbContext.FamilyHeadAdditionalFacilities.AsNoTracking()
                        .Include(e => e.AdditionalFacilities)
                        .ThenInclude(e => e.IndividiualFacilityDescription)
                        .Where(e => e.FamilyHeadId == familyHead.FamilyHeadId)
                        .ToList();

            float? iadfAmountUsd = 0f;

            var extraFacilities = _dbContext
                       .FamilyHeadExtras.AsNoTracking()
                       .Where(e => e.FamilyHeadId == familyHead.FamilyHeadId)
                       .ToList();

            float totalExtraAdfUsd = 0;
            for (int k = 0; k < extraFacilities.Count; k++)
            {
                totalExtraAdfUsd += extraFacilities.ElementAt(k).PriceUsd ?? 0;
            }

            var trainings = _dbContext
                .PackageTrainings.AsNoTracking()
                .Include(e => e.Training)
                .Where(e => e.PackageId == familyHead.PackageId)
                .ToList();

            var gifts = _dbContext
                .PackageGifts.AsNoTracking()
                .Include(e => e.Gift)
                .Where(e => e.PackageId == familyHead.PackageId)
                .ToList();

            if (familyHead.Age < 2)
            {
                packageAmount = familyHead.Package.InfantUsd;
                iadfAmountUsd = _dbContext.FamilyHeadAdditionalFacilities.AsNoTracking()
                            .Where(e => e.FamilyHeadId == id)
                            .Sum(e => e.AdditionalFacilities.InfantUsd);
            }
            else if (familyHead.Age < 12)
            {
                packageAmount = familyHead.Package.ChildUsd;
                iadfAmountUsd = _dbContext.FamilyHeadAdditionalFacilities.AsNoTracking()
                            .Where(e => e.FamilyHeadId == id)
                            .Sum(e => e.AdditionalFacilities.ChildUsd);
            }
            else
            {
                packageAmount = familyHead.Package.AdultUsd;
                iadfAmountUsd = _dbContext.FamilyHeadAdditionalFacilities.AsNoTracking()
                            .Where(e => e.FamilyHeadId == id)
                            .Sum(e => e.AdditionalFacilities.AdultUsd);
            }

            float? additionalFacilitiesAmountUsd = 0;
            float? totalFlightUsd = 0;
            float? totalUsd = 0;
            float discountUsd = familyHead.UsdDiscountAmount;

            string ticketIncluded = familyHead.FlightTicketType.ToString();

            if (familyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
            {
                ticketIncluded = "Included With Differences";
                totalFlightUsd = familyHead.Package.FlightTicketPriceUsd + familyHead.FlightTicketDifferencesUsd;
            }
            else if (familyHead.FlightTicketType == FlightTicketType.Excluded)
            {

            }
            else
            {
                totalFlightUsd = familyHead.Package.FlightTicketPriceUsd;
            }

            totalUsd += totalFlightUsd;
            totalUsd += packageAmount;
            totalUsd += iadfAmountUsd;
            totalUsd += totalExtraAdfUsd;
            totalUsd -= discountUsd;

            PackageAccomodations aziziaAccomadation = _dbContext
                        .PackageAccomodtions.AsNoTracking()
                        .Include(e => e.Accomodation)
                        .Include(e => e.FoodPackage)
                        .Include(e => e.AccomodationCategory)
                        .FirstOrDefault(e => e.PackageId == familyHead.PackageId && e.PlaceId == 4);

            PackageAccomodations makkahAccomadation = _dbContext
                .PackageAccomodtions.AsNoTracking()
                .Include(e => e.Accomodation)
                .Include(e => e.FoodPackage)
                .Include(e => e.AccomodationCategory)
                .FirstOrDefault(e => e.PackageId == familyHead.PackageId && e.PlaceId == 1);

            PackageAccomodations medinaAccomadation = _dbContext
                .PackageAccomodtions.AsNoTracking()
                .Include(e => e.Accomodation)
                .Include(e => e.FoodPackage)
                .Include(e => e.AccomodationCategory)
                .FirstOrDefault(e => e.PackageId == familyHead.PackageId && e.PlaceId == 2);

            PackageAccomodations minaAccomadation = _dbContext
                .PackageAccomodtions.AsNoTracking()
                .Include(e => e.Accomodation)
                .Include(e => e.FoodPackage)
                .Include(e => e.AccomodationCategory)
                .FirstOrDefault(e => e.PackageId == familyHead.PackageId && e.PlaceId == 3);



            string hajjPdfString = $@"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>AGREEMENT- HAJJ (1445-H)</title>
    <style>
      * {{
        box-sizing: border-box;
        font-family: system-ui, -apple-system, ""Segoe UI"", Roboto,
          ""Helvetica Neue"", Arial, ""Noto Sans"", ""Liberation Sans"", sans-serif,
          ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"",
          ""Noto Color Emoji"";
      }}
      body {{
        margin: 15px;
      }}

      p {{
        font-size: 12px;
        margin: 0;
      }}
      span {{
        font-weight: bold;
      }}

     
      .company_name_box h1 {{
        margin: 0;
        padding: 10px 0;
      }}
      header {{
        display: flex;
        justify-content: space-between;
        align-items: center;
      }}
      
      .AgreementText{{
         background-color: rgb(68, 67, 67);
         display: inline-block;
         color: white;
         padding:  10px;
         border-radius: 10px;
      }}
      .DateText p{{
        font-size:10px ;
        margin-top: 5px;
        margin-left: 1px;
      }}
    
     

      .FirstPart{{
        margin: 10px 0;
      }}
      .company_Info{{
        font-size: 8px;
      }}
      .Second_Part{{
        float: left !important;
      }}
      .Second_Part p{{
        font-size: 8px
      }}

      .userInfoBoxs{{
        float: right !important;
         margin-bottom: 25px;
      }}
     
      h1 {{
        text-align: center !important;
      }}
      
      .profile_picture{{
        border: 1px solid black;
      }}
      
      .userInfoBox {{
        display: flex;
        justify-content: space-between;
        align-items: center;
      }}
      .table_container {{
        margin-top: 10px;
        float: left !important;
      }}
      

      .table_profile {{
        border-collapse: collapse;
        width: 100%;
      }}
      .table_profile tr {{
        display: flex;
        justify-content: space-between;
        font-size: 12px !important;
        border: 1px solid black;
        
      }}
      .table_profile th {{
        font-size: 8px !important;
       
      }}
      .table_profile td {{
        font-size: 8px !important;
        padding: 2px 0;
      }}
      .table_profile tr td:nth-child(2) {{
        text-align: right;
      }}
      .packagesection {{
        margin-top: 30px;
      }}
      .Our_Package {{
        border-collapse: collapse;
        width: 100%;
        border: 1px solid black;
      }}
      .Our_Package td {{
        font-size: 8px !important;
      }}
      .Our_Package th {{
        font-size: 8px !important;
        padding: 3px 0;
        text-align: left;
      }}
      .Our_Package tr {{
        border-bottom: 1px solid black;
      }}
      .Our_Package th:nth-child(2) {{
        padding-left: 100px;
      }}
      .Our_Package td:nth-child(2) {{
        padding-left: 100px;
      }}

      .termsConditions{{
        margin-top:15px;
      }}
      .termsConditions p{{
        text-align: justify;
      }}
      .termsConditions h5{{
        font-size: 10px;
        background-color: lightblue;
        color: black;
        display: inline-block;
        padding: 0 8px !important ;
      }}
      .termsConditions h5,p,li{{
        margin: 0;padding: 0;
      }}
      .termsConditions p{{
        font-size: 8px;
      }}
      .profile_image{{
        float: left !important;
       width: 100px;
       height: 130px;
       border: 1px solid black;
       margin-top: 15px;
       box-shadow: rgba(6, 24, 44, 0.4) 0px 0px 0px 2px, rgba(6, 24, 44, 0.65) 0px 4px 6px -1px, rgba(255, 255, 255, 0.08) 0px 1px 0px inset;
       margin: 11px 20px;
      }}
     
      .UnOrded_list li{{
        font-size: 8px;
      }}
      footer{{
        display: flex;
        justify-content: space-between;
        margin-top: 2px;
      }}
      .QR_Code_logo{{
        text-align: right;
      }}
      .footer-text {{
        display: flex;
        justify-content: center;
        align-items: center;
        text-align: center;
   
      }}
     
    </style>
</head>

<body>
    <header>
        <div class=""agreement_Container"">
            <div class=""AgreementText"">
                <h4 style=""margin: 0; padding: 0;"">AGREEMENT HAJJ 2024 - (1445-H)
                </h4>
            </div>
            <div class=""DateText"">
                <p> <span>DATE OF AGREEMENT: {DateTimeManager.GetDate(DateTime.Today)}</span></p>
            </div>
        </div>
    </header>
    <div class=""FirstPart"">
        <h6 style=""margin: 0; padding: 0;"">FIRST PART:</h6>
        <p class=""company_Info"">M/S {agreement.CompanyName}. A CLUSTER PART OF
            {agreement.ClusterName}. <br> A COLLABORATIVE CLUSTER-AFFILIATE HAJJ GROUP
            MANAGEMENT COMPANY.</p>
    </div>
    <div class=""SecondPart_Table_box"">
            <div class=""Second_Part"">
                <h6 style=""margin: 0; padding: 0;"">SECOND PART:</h6>
                <p style=""margin: 0; padding: 0; font-weight: bold;"">{agreement.Title}+{agreement.Name}</p>
                <p><span>Passport Number: </span>{agreement.PassportNumber}</p>
                <p><span>Address: </span> {agreement.Address}</p>
                <p><span>Contact: </span>{agreement.Contact}</p>
                <p><span>Email: </span>{agreement.Email}</p>
                <p><span>Category: </span>{agreement.Category}</p>
                <p><span>Hajj Application No: </span>{agreement.HajjAppNumber}</p>
            </div>
            <div class=""userInfoBoxs"">
                    <img src=""[URL LINK]/{agreement.HajjiImage}"" alt="""" class=""profile_image"">
                <div class=""table_container"" id=""div1"">
                    <table class=""top_service_table table_profile title_table"">
                        <tr style=""background-color: lightblue;"">
                            <th colspan=""1"" style=""
                          text-align: left;
                          font-size: 10px;
                          color: black;
                          background-color: lightblue;
                          font-weight: bolder;

                        "">
                                BASIC PACKAGE WITHOUT AIR TICKET, INCLUDING *I-ADF: ({agreement.Package} MEDINAH {agreement.MedinahArrival})
                            </th>


                        </tr>
                        <tr>
                            <td style=""font-weight: bold;""> DESCRIPTION</td>
                            <td style=""font-weight: bold;""> AMOUNT (USD)</td>
                        </tr>
                        <tr>
                            <td>{agreement.IADFString}</td>
                            <td>{agreement.IADFPriceWithPackage}</td>
                        </tr>
                        <tr>
                            <td>** EI-ADF (EXTRA INDIVIDUAL ADDITIONAL FACILITY) </td>
                            <td>{agreement.ExtraFacilitiesPriceUsd}</td>
                        </tr>
                        <tr>
                            <td>AIR TICKET </td>
                            <td>{agreement.FlightTicketPriceUsd}</td>
                        </tr>
                        <tr>
                            <td>AIR TICKET DIFFERENCE AMOUNT (IF APPLICABLE)</td>
                            <td>{agreement.FlightTicketDifferencePriceUsd}</td>
                        </tr>
                        <tr id=""AmountUSD_ROW"">
                            <td>TOTAL AMOUNT</td>
                            <td contenteditable=""true"" id=""editable-row"">{agreement.TotalPerPersonUsd}</td>
                        </tr>
                        <tr>
                            <td>DISCOUNT/REBATE/ROE ADJUSTMENT (IF APPLICABLE)</td>
                            <td>{agreement.DiscountUsd}</td>
                        </tr>
                        <tr>
                            <td>NET RECEIVABLE AMOUNT (USD)</td>
                            <td>{agreement.NetReceivable}</td>
                        </tr>
                        <tr style=""background-color: grey !important; color: white"">
                            <td>PAYMENT RECEIVED (USD) (REFER TO STATEMENT) </td>
                            <td></td>
                        </tr>

                    </table>
                </div>
             </div>
    </div>

    <div class=""packagesection"">
        <table class=""Our_Package"">
            <thead>
                <tr style=""background-image: linear-gradient(315deg, #278533 0%, #378b29 74%); color: white;"">
                    <th>PACKAGE DETAIL (TABLE B)</th>
                    <th>TRANSPORT ROUTE (TABLE C)</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>{agreement.Package}</td>
                    <td>
                        Upon Arrival in KSA, Airport to Hotel / Building by Airport Naqaba
                        Sharing Group Bus
                    </td>
                </tr>
                <tr>
                    <td>
                        * Medina: {agreement.MedinaFacilities}
                    </td>
                    <td>
                        Hotel to Train Station &amp; Train Station to Accommodation
                        (Intercity) by Group Bus
                    </td>
                </tr>
                <tr>
                    <td>
                        * Makkah: {agreement.MakkahFacilities}
                    </td>
                    <td>
                        Med to Mak / Mak to Med by Bus or by Train in Economy Class with
                        Hand Carry only
                    </td>
                </tr>

                <tr>
                    <td>* Azizia: {agreement.AziziaFacilities}</td>
                    <td>
                        Luggage will be Shuttled separately in a Truck with approximate
                        8hrs journey (in-case of a train)
                    </td>
                </tr>

                <tr>
                    <td>** EI-ADF: {agreement.ExtraFacilities}</td>
                    <td>
                        Mashair: Mina, Arafat, Muzdalfa &amp; Back to Azizia by Group Bus
                        Seat to Seat
                    </td>
                </tr>
                <tr>
                    <td>Airline: {agreement.Airline}</td>
                    <td>
                        Makkah/Medinah Accommodation to Airport by Group Sharing Bus
                    </td>
                </tr>
                <tr>
                    <td>Medina Arrival: {agreement.MedinahArrival}</td>
                    <td>
                        Ziarat in Makkah &amp; Medinah (as per package) by Group Sharing
                        Bus
                    </td>
                </tr>
            </tbody>
        </table>
    </div>

    <div class=""termsConditions"">
        {agreement.SharedAgreement}
        <hr>
        {agreement.SpecificAgreement}
    </div>

    <div class=""signature-bottom"" style=""margin-top: 30px;"">
        <div class=""footer-text"">
            <p><b> I HAVE CAREFULLY READ AND UNDERSTOOD THE TERMS AND CONDITIONS OF THIS AGREEMENT AND THESE ARE CORRECT
                    <br>
                    IN WITNESS WHEREOF </b> , the parties hereto have executed this Hajj Program Reservation Agreement
                as of the date first above written.</p>
        </div>

        <div class=""signature-part"" >

            <div style="" width: 200px; height: 200px;   text-align: center; float: left; padding: 20px;"">
                <div style=""margin-top: 155px;"">
                    <span>{DateTime.Now.ToString("dd/MM/yyyy")}</span>
                    <p style=""border-top: 2px solid black;"">Date</p>
                </div>

            </div>

            <div style=""float: left; padding: 20px; "">
                <div class=""col-md-12 mt-3"">
                <p class=""text-center border-bottom border-dark""><img src=""{agreement.SignatureBase64}"" width=""220"" height=""200"" /> </p>
                </div>

                <p style=""border-top: 2px solid black;"">Applicant's Signature & Thumb Impression</p>

            </div>

            <div style=""float: left; padding: 20px;"">
               <img src=""[URL LINK]/{agreement.CompanySeal}"" id=""signatureStamp"" width=""180"" height=""180"">
                <p style=""border-top: 2px solid black;"">Company Seal Representative Sign</p>
            </div>
        </div>
        
    </div>
</body></html>
";

            string dataFormPdfString = $@"<!DOCTYPE html ><html><head><link rel=""icon"" type=""image/x-icon"" href=""~/favicon.ico"" /> " +
                $@"<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" /> " +
                $@"<link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.0.1/css/bootstrap.min.css""" +
                $@"integrity=""sha512-Ez0cGzNzHR1tYAv56860NLspgUGuQw16GiOOp/I2LuTmpSK9xDXlgJz3XN4cnpXWDmkNBKXR/VDMTCnAaEooxA=="" " +
                $@" crossorigin=""anonymous"" referrerpolicy=""no-referrer"" /> <title>TMG Report</title> <style>" +
                $@"html {{ zoom: 70%; font-family: sans-serif; }}.page-break {{ page-break-after: always; }}" +
                $@"p {{ margin: 0; }} * {{margin-bottom }}" +
                $@" .user-img img, .monogram img {{height: 100%;width: 100%;object-fit: contain;}}.border-dark span {{border: none !important;}}" +
                $@".monogram {{height: 60px;}}.fields div {{border: none !important;}}.user-img {{width: 130px;height: 130px;}}.h5,.h6,.border-right p {{font-weight: 500 !important;}}" +
                $@"p.h6 {{margin-bottom: 0;}}* {{margin: 0;padding: 0;text-indent: 0;font-size: small;text-transform: uppercase;}}" +
                $@".page-break {{page-break-after: always;}}.border-dark {{border: 0;border-bottom-width: 1pt;border-style: dashed !important;}}" +
                $@".accommodation td {{border-bottom: none !important;border-top: none !important;}}.accommodation tr {{border-top-style: solid;width: 33% !important;}}" +
                $@"table,tbody {{overflow: visible;width: 99vw;}} " +
                $@"#noteBackground {{background-color: rgb(226, 226, 226) !important;}} * {{font-size: 8.6pt !important;}}" +
                $@".title{{font-size: 18px !important;margin: 0;}} table {{vertical-align: middle;overflow: visible;width: 100vw;}}" +
                $@".show-prices-fac {{display: none}}.hideInPrint {{display: none;}}p.totalPackage {{font-size: 16px !important;font-weight: bold !important;}}" +
                $@"span.totalPackage {{font-size: 16px !important;font-weight: bolder !important;}}p,h6 span {{font-weight: 100 !important;}}" +
                $@".table tr td {{border-top-style: solid;border-top-width: 1pt;border-left-style: solid;border-left-width: 1pt;border-bottom-style: solid;" +
                $@"border-bottom-width: 1pt;border-right-style: solid;border-right-width: 1pt;}}.additional-facility tr {{text-align: center;}}" +
                $@".border {{border-width: 1pt !important;}}.fillable {{font-weight: 400 !important;}}</style></head>" +
                $@"<body>" +
                $@"<div class=""form-container container-fluid""> <div class=""header-section  d-flex justify-content-end ""> <div class=""mid-div col-12 text-center""> " +
                $@"<h1 class=""fw-bold text-center title"" style=""font-size: 18px;"">HAJJ FORM 2024 (1445 AH)</h1> </div> <div class=""additional-fields d-flex justify-content-end col-4""> " +
                $@"<table class=""table""> <tr> <td class=""fields text-right"" style=""text-wrap: nowrap;""> <div class=""border d-flex-column""> " +
                $@"<p class=""d-block border border-dark p-1 mb-2"" style=""text-align: center;"">APPLICANT TYPE: </p> " +
                $@"<p class=""d-block border border-dark p-1"" style=""text-align: center;"">Hajj App #</p> </div> </td> <td class=""fields text-right"" style=""text-wrap: nowrap;"">" +
                $@"<p></p> <div class=""border ""> <p class=""d-block border border-dark p-1  mb-2"" style=""text-align: center;""> <span>{familyHead.ApplicantType}</span> </p>" +
                $@" <p class=""d-block border border-dark p-1 "" style=""text-align: center;""> <span>{familyHead.HajjApplicationNumber}</span> </p> </div> </td> </tr> </table> </div>" +
                $@"</div><table class=""table m-0 p-0""> <tr> <td class=""border-right user-img"" colspan=""1"" rowspan=""3""> " +
                $@"<img loading=""lazy"" src=""[URL LINK]/{agreement.HajjiImage}"" alt="""" class="" ""> </td> <td class=""border-right"" colspan=""2""> <p class=""d-inline"">CID</p> " +
                $@"<span class=""fillable"">{cId}</span> </td> <td class=""border-right"" colspan=""2""> <p class=""d-inline"">PACKAGE: </p> " +
                $@"<span class=""fillable"">{familyHead.Package.Name} ({familyHead.Package.Days})</span> </td> <td class=""border-right"" colspan=""4""> <p class=""d-inline"">HGO NAME: </p> " +
                $@"<span class=""fillable"">{familyHead.Package.Company.CompanyName}</span> </td> </tr> <tr> <td class=""border-right""> " +
                $@"<p class=""d-inline"">ENROLLMENT NO.</p> <span class=""fillable"">{familyHead.ENumber}</span> </td> <td class=""border-right""> " +
                $@"<p class=""d-inline"">STAY DURATION: <span class=""fillable"">{familyHead.Package.HajjDuration}</span></p></td><td> <p class=""h6 d-inline"">MARRIED? </p> " +
                $@"<p class=""fillable d-inline"">{(familyHead.IsMarried ? "YES" : "NO")}</p></td><td> <p class=""h6 d-inline"">BLOOD GROUP:</p> " +
                $@"<p class=""fillable d-inline"">{familyHead.BloodGroup}</p> </td>" +
                $@" <td> <p class=""h6 d-inline"">FIQAH: <span class=""fillable"">{familyHead.Fiqah.Name}</span></p> </td> <td> " +
                $@"<p class=""h6 d-inline"">GENDER: <span class=""fillable"">{familyHead.Gender}</span></p> </td> <td colspan=""2""> <p class=""text-nowrap h6 d-inline"">CNIC No.</p>" +
                $@" <p class=""fillable"">{familyHead.CNIC}</p> </td> </tr> <tr> <td> <p class=""h6 mt-1"">GIVEN NAME: </p> <span class=""fillable"">{familyHead.GivenName}</span>" +
                $@"</td><td> <p class=""h6 mt-1""> SURNAME: </p> <span class=""fillable"">{familyHead.SurName}</span> </td> <td> <p class=""h6""> D.O.B: </p> " +
                $@"<p class=""fillable"">{DateTimeManager.GetDate(familyHead.DateOfBirth)}</p> </td><td colspan=""2""> <p class=""h6"">FATHER'S / HUSBAND'S NAME </p> <p class=""fillable"">{familyHead.FatherHusbandName}</p></td><td>" +
                $@" <p class=""text-nowrap h6"">PASSPORT NO. </p> <p class=""fillable"">{familyHead.PassportNumber} </p></td><td> <p class=""text-nowrap h6"">DATE OF EXPIRY </p>" +
                $@"<p class=""fillable"">{familyHead.PassportDateOfExpiry}</p></td></tr> <tr style=""""><td class=""border-right"" style=""vertical-align: top;"" colspan=""4""> " +
                $@"<p> PRESENT POSTAL ADDRESS: <span class=""fillable d-inline"">{presentPostalAddress}</span></p></td><td class=""border-right"" colspan=""1"">" +
                $@"<p>TEHSIL CODE: <span class=""fillable d-inline"">{headAddress?.Tehsil?.Name}</span></p></td><td class=""border-right"" colspan=""1"">" +
                $@"<p>MOBILE: <span class=""fillable d-inline"">{preferredContact?.Contact}</span></p></td><td class=""border-right border-left"" colspan=""2"">" +
                $@"<p>EMAIL: <span class=""fillable d-inline"">{email}</span></p></td></tr><tr><td colspan=""2""><p class=""h6"">Employer / Business Name: " +
                $@"<span class=""fillable"">{familyHead.EmployedAt}</span></p></td><td><p class=""h6"">Designation: <span class=""fillable"">{familyHead.Designation}</span></p></td>" +
                $@"<td colspan=""2""><p class=""h6"">Official Address: <span class=""fillable"">{officialAddressString}</span></p></td><td><p class=""h6"">Official Contact: " +
                $@"<span class=""fillable"">{officialContact?.Contact}</span></p></td><td colspan=""2""><p class=""h6"">Official Email: <span class=""fillable"">{officialEmail?.Email}</span></p>" +
                $@"</td></tr><tr style=""height:15pt"" class=""text-nowrap""><td class=""border-right"" colspan=""2""><p>PERFORMED HAJJ IN LAST FIVE YEARS?" +
                $@"<span class=""fillable px-3"">{(familyHead.PerformedHajjInLastFiveYears ? "YES" : "NO")}</span></p>{(familyHead.PerformedHajjInLastFiveYears ? "Years: " + familyHead.YearsPerformedHajj : "")}" +
                $@"</td><td class=""border-right"" colspan=""2""><p>WANT TO PERFORM HAJJ-E-BADAL?<span class=""fillable px-3"">{(familyHead.WantToPerformHajjEBadal ? "YES" : "NO")}</span></p></td>" +
                $@"<td class=""border-right"" colspan=""2""><p>WANT TO PERFORM HAJJ AS GROUP WORKER?<span class=""fillable px-3"">{(familyHead.WantToPerformAsGroupWorker ? "YES" : "NO")}</span>" +
                $@"</p></td><td class=""border-right"" colspan=""3""><p>ARE YOU MEHRAM OF A LADY?<span class=""fillable px-3"">{(mehram is not null && familyHead.Gender == Gender.Male ? "YES" : "NO")}</span></p></td></tr>" +
                $@"<tr style=""height: 70pt""><td class=""border-right"" colspan=""4""><p><u>NOMINEE IN CASE OF DEATH (MUST BE ADULT/RELATIVE)</u></p><p>NAME: " +
                $@"<span class=""fillable"">{headNominee?.FamilyHead?.FullName}</span></p><p>RELATIONSHIP: <span class=""fillable"">{headNominee?.NomineeRelation}</span></p><p>CNIC NO. " +
                $@"<span class=""fillable"">{headNominee?.CNIC}</span></p><p>MOBILE NO. <span class=""fillable"">{headNominee?.Contact}</span></p><p>TELEPHONE NO. " +
                $@"<span class=""fillable""></span></p></td><td class=""border-right p-1"" colspan=""5""><p><u>MEHRAM'S DETAILS " +
                $@"<small>(FOR LADIES ONLY)</small></u> &nbsp;APPLICATION NO. CODE: <span>{(familyHead.Gender == Gender.Female ? mehram?.HajjApplicationNumber : "")}</span> <SMALL style=""COLOR: GRAY"">(FOR OFFICE USE)</SMALL>" +
                $@"</p><p>MEHRAM'S NAME: {(familyHead.Gender == Gender.Female ? mehram?.FullName : "")} RELATIONSHIP: {(familyHead.Gender == Gender.Female ? familyHead.RelationWithMehram?.Relation : "")}</p><p style=""min-height: 80px;"" class=""border"">" +
                $@"<img loading=""lazy"" src=""{(familyHead.Gender == Gender.Female ? mehram?.SignatureBase64 : "")}"" height=""80"" alt=""""></p> <p class=""text-center""> SIGNATURE &amp; THUMB IMPRESSION</p></td> </tr> " +
                $@"<tr style=""""><td class=""border-right"" colspan=""9"" id=""noteBackground""> <p class=""d-inline"">Note: </p>" +
                $@"<p class=""d-inline"" style=""font-weight: 400 !important; font-size: 0.9rem !important;"">Every Hajji is required to personally complete their " +
                $@"biometrics using the Saudi Bio App, if applicable (self-biometrics). The costs, charges, and results are the individual's responsibility. </p></td> </tr> " +
                $@"</table>" +
                $@"<table cellspacing=""0"" class=""table mb-0""> <tr> <td class=""border-right"" colspan=""14""> " +
                $@"<p class=""h5 text-center mb-0"">HAJJ PACKAGE DETAILS (MEDINA ARRIVAL: {familyHead.Package.MedinahArrival} HAJJ)</p> </td> </tr>" +
                $@"<tr style=""""> <td class=""border-right"" colspan=""6""> <p>HAJJ PACKAGE W/O TICKET: <span class=""fillable package-usd"" style=""display:inline"">" +
                $@"USD ${packageAmount.ToString("N0")}/-</span> </p> <p>* INDIVIDUAL ADDITIONAL FACILITIES (I-ADF): <span class=""fillable package-usd"" " +
                $@"style=""display:inline"">USD ${iadfAmountUsd}/-</span> </p> <p>** EXTRA INDIVIDUAL ADDITIONAL FACILITIES (EI-ADF): " +
                $@"<span class=""fillable package-usd"" style=""display:inline"">USD ${totalExtraAdfUsd}/-</span> </p> <p>Discount / Rebate: <span class=""fillable package-usd"" " +
                $@"style=""display:inline"">USD ${discountUsd.ToString("N0")}/-</span> </p>";
            if (!ticketIncluded.Equals("Excluded"))
            {
                dataFormPdfString += $@"<p>BASIC TICKET: <span class=""fillable package-usd"" " +
            $@"style=""display:inline"">USD {familyHead.Package.FlightTicketPriceUsd.ToString("N0")}/-</span>";
                if (ticketIncluded.Equals("Included With Differences"))
                {
                    dataFormPdfString += $@"<span class=""fillable package-usd"" style=""display:inline"">(DIFFERENCE AMOUNT USD ${familyHead.FlightTicketDifferencesUsd?.ToString("N0")}/-)</span>";
                }
                dataFormPdfString += $@"</p>";
            }
            dataFormPdfString += $@"<p class=""totalPackage""> TOTAL PACKAGE: <span class=""fillable package-usd  totalPackage"" style=""display:inline"">USD ${totalUsd?.ToString("N0")}/-</span> </p> </td> " +
            $@"<td class=""border-right"" colspan=""4"">";

            if (!ticketIncluded.Equals("Excluded"))
            {
                dataFormPdfString += $@"<p class=""h6""> FLIGHT TYPE: <span class=""fillable"">{familyHead.Package.FlightType}</span>" +
            $@" </p> <p class=""h6"">CLASS: <span class=""fillable"">{familyHead.Package.FlightClass}</p> <p class=""h6"">PLACE OF DEPARTURE: <span class=""fillable"">{familyHead.POD}</span></p>" +
            $@" <p class=""h6"">AIR TICKET: <span class=""fillable"">{ticketIncluded}</span></p><p class=""h6"">TICKET PERIOD: <span class=""fillable"">{familyHead.Package.FlightTicketPeriod}</span>" +
            $@"</p>";
            }
            else
            {
                dataFormPdfString += $@"<p class=""h6"">AIR TICKET: <span class=""fillable"">EXCLUDED</span></p>";
            }
            dataFormPdfString += $@"</td><td class=""border-right"" colspan=""5"">" +
            $@"<p class=""m-0"">TRAINING SESSION NO.</p>";
            int x = 0, y = 0, z = 0; ;
            foreach (var ts in trainings)
            {
                dataFormPdfString += $@" <span class=""fillable mb-1""><b>{x + 1}</b> - {ts.Training.TrainingSession}</span>";
                x++;
            }

            dataFormPdfString += $@"<p class=""m-0"">EXPECTED TIME:</p>";
            foreach (var ts in trainings)
            {
                dataFormPdfString += $@" <span class=""fillable mb-1""><b>{y + 1}</b> - {ts.Training.ExpectedTrainingTime}</span>";
                y++;

            }
            dataFormPdfString += $@"<p class=""m-0"">EXPECTED VENUE:</p>";
            foreach (var ts in trainings)
            {
                dataFormPdfString += $@" <span class=""fillable mb-1""><b>{z + 1}</b> - {ts.Training.ExpectedTrainingVenue}</span>";
                z++;
            }
            dataFormPdfString += $@"</td></tr>" +
            $@"";

            var _medinaIndId = _dbContext.FamilyHeadAdditionalFacilities.AsNoTracking()
                                                .Include(e => e.AdditionalFacilities)
                                                .FirstOrDefault(e => e.FamilyHeadId == familyHead.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 2);

            var _makkahIndId = _dbContext.FamilyHeadAdditionalFacilities.AsNoTracking()
                                                .Include(e => e.AdditionalFacilities)
                                                .FirstOrDefault(e => e.FamilyHeadId == familyHead.FamilyHeadId
                                                && e.AdditionalFacilities.PlaceId == 1);

            var _aziziaIndId = _dbContext.FamilyHeadAdditionalFacilities.AsNoTracking()
                                               .Include(e => e.AdditionalFacilities)
                                               .FirstOrDefault(e => e.FamilyHeadId == familyHead.FamilyHeadId
                                               && e.AdditionalFacilities.PlaceId == 4);

            var remarks = _dbContext.FamilyHeadRemarks
                .Where(e => e.FamilyHeadId == id)
                .Select(e => e.Remarks)
                .ToList();

            dataFormPdfString += $@"<tr><td class=""border-right"" colspan=""14""><p class=""h5 text-center mb-0"">TRANSPORT</p></td></tr><tr><td class="" border-right"" colspan=""4"">" +
                $@"<p>INTERCITY: <span class=""fillable"">{familyHead.Package.TransportIntercity}</span></p> </td> <td class=""border-right"" colspan=""4""> <p>WITHIN CITY: " +
                $@"<span class=""fillable"">{familyHead.Package.WithInCity}</span></p> </td><td class=""border-right"" colspan=""3"">" +
                $@"<p>WITHIN MASHAIR: <span class=""fillable"">{familyHead.Package.TransportWithMashair}</span></p>" +
                $@"</td><td class=""border-right"" colspan=""4""><p>AIRPORT BUILDING: <span class=""fillable"">{familyHead.Package.AirportTransport}</span> </p> </td> </tr>" +
                $@"<tr><td class=""border-right"" colspan=""14""><p class=""h5 text-center mb-0"">ACCOMMODATION DETAILS</p></td></tr>" +
                $@"<tr class=""accommodation-row"">";

            if (familyHead.Package.MedinahArrival.Equals("BEFORE"))
            {
                dataFormPdfString += $@"<td class=""border-right"" colspan=""5"" style=""width: 25%;""> <u class=""d-block text-center"">IN MEDINA</u> " +
                    $@"<p> ACCOMMODATION TYPE: <span class=""fillable""> {medinaAccomadation?.Accomodation.AccomodationName} </span> </p> <p>Name: <span class=""fillable""> " +
                    $@"{medinaAccomadation?.AccomodationName} </span></p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">{medinaAccomadation?.CheckIn}</span></p>" +
                    $@" &nbsp;&nbsp;<p class=""d-inline"">DAYS: <span class=""fillable"">{medinaAccomadation?.Days}</span></p> <p>DISTANCE (METERS): " +
                    $@"<span class=""fillable"">{medinaAccomadation?.DistanceInMeters}</span></p> <p>FOOD PACKAGE: <span class=""fillable"">{medinaAccomadation?.FoodPackage.Package}</span></p> " +
                    $@"<p>LOAD PER ROOM / WASHROOM: <span class=""fillable"">{medinaAccomadation/*?.LoadPerRoom*/} / {medinaAccomadation/*?.LoadPerWashroom*/} </span>" +
                    $@"<b> {(_medinaIndId is not null ? $"*{_medinaIndId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "")}</b> " +
                    $@"</p> <p>ROOM SHARING: <span class=""fillable"">{medinaAccomadation?.Sharing}</span></p> " +
                    $@"<p STYLE=""text-decoration: underline;"">If 'Family Room' is selected, please refer to the details provided in the 'Individual Facilities' column below.</p> </td>";

                dataFormPdfString += $@"<td class=""border-right"" colspan=""3"" style=""width: 25%;""> <u class=""d-block text-center"">IN MAKKAH</u> " +
                    $@"<p> ACCOMMODATION TYPE: <span class=""fillable""> {makkahAccomadation?.Accomodation.AccomodationName} </span> </p> <p>Name: <span class=""fillable""> " +
                    $@"{makkahAccomadation?.AccomodationName} </span></p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">{makkahAccomadation?.CheckIn}</span></p>" +
                    $@" &nbsp;&nbsp;<p class=""d-inline"">DAYS: <span class=""fillable"">{makkahAccomadation?.Days}</span></p> <p>DISTANCE (METERS): " +
                    $@"<span class=""fillable"">{makkahAccomadation?.DistanceInMeters}</span></p> <p>FOOD PACKAGE: <span class=""fillable"">{makkahAccomadation?.FoodPackage.Package}</span></p> " +
                    $@"<p>LOAD PER ROOM / WASHROOM: <span class=""fillable"">{makkahAccomadation/*?.LoadPerRoom*/} / {makkahAccomadation/*?.LoadPerWashroom*/} </span>" +
                    $@"<b> {(_makkahIndId is not null ? $@"*{_makkahIndId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "")}</b> " +
                    $@"</p> <p>ROOM SHARING: <span class=""fillable"">{makkahAccomadation?.Sharing}</span></p> " +
                    $@"<p STYLE=""text-decoration: underline;"">If 'Family Room' is selected, please refer to the details provided in the 'Individual Facilities' column below.</p> </td>";

                dataFormPdfString += $@"<td class=""border-right"" colspan=""5"" style=""width: 25%;""> <u class=""d-block text-center"">IN AZIZIA</u> ";
                if (aziziaAccomadation is not null)
                {
                    dataFormPdfString += $@"<p> ACCOMMODATION TYPE: <span class=""fillable""> {aziziaAccomadation.Accomodation.AccomodationName} </span>" +
                    $@"</p><p>Name: <span class=""fillable"">{aziziaAccomadation.AccomodationName} </span></p> <p class=""d-inline"">CATEGORY: <span class=""fillable"">" +
                    $@"{aziziaAccomadation.AccomodationCategory.Category}</span></p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">{aziziaAccomadation.CheckIn}</span></p> <br>" +
                    $@" <p class=""d-inline"">DAYS: <span class=""fillable"">{aziziaAccomadation.Days}</span></p> &nbsp; &nbsp;" +
                    $@"<p class=""d-inline"">DISTANCE (METER): <span class=""fillable"">{aziziaAccomadation.DistanceInMeters}</span></p> <p>FOOD PACKAGE: " +
                    $@"<span class=""fillable"">{aziziaAccomadation.FoodPackage.Package}</span></p> <p>LOAD PER ROOM / WASHROOM: " +
                    $@"<span class=""fillable"">{aziziaAccomadation/*.LoadPerRoom*/} / {aziziaAccomadation/*.LoadPerWashroom*/}</span> " +
                    $@"<b>{(_aziziaIndId is not null ? $@"*{_aziziaIndId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "")}</b></p> " +
                    $@"<p>HARAM SHUTTLE: <span class=""fillable"">{aziziaAccomadation.Shuttle}</span></p> <p>RELIGIOUS LECTURES: " +
                    $@"<span class=""fillable"">{aziziaAccomadation.ReligiousLectures}</span></p> ";
                }
                else
                {
                    dataFormPdfString += $@"<p class=""text-center"">N/A</p> ";
                }
                dataFormPdfString += $@"</td>";

                dataFormPdfString += $@"<td class=""border-right"" colspan=""3"" style=""width: 25%;""> <u class=""d-block text-center"">IN MASHAIR</u> <p> MAKTAB: <span class=""fillable""> " +
                    $@"{familyHead.Package.CategoryZone} </span> </p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">08ZH 1445-H</span></p> &nbsp;&nbsp;<p class=""d-inline"">" +
                    $@"DAYS: <span class=""fillable"">5 (MINA / ARAFAT / MUZDALIFAH)</span></p> <p>FOOD PACKAGE: <span class=""fillable"">{minaAccomadation?.FoodPackage.Package}</span></p>" +
                    $@" <p>SOFA MATTRESS / SITTING: <span class=""fillable"">{minaAccomadation?.Bedding}</span> <br>CAMP/TENT SHARING: " +
                    $@"<span class=""fillable"">{minaAccomadation?.Camp}</span> <br>ARAFAT: <span class=""fillable"">{minaAccomadation?.Arafat}</span> <br>" +
                    $@"RELIGIOUS LECTURES: <span class=""fillable"">{minaAccomadation?.ReligiousLectures}</span></p></td>";

            }
            else
            {
                dataFormPdfString += $@"<td class=""border-right"" colspan=""3"" style=""width: 25%;""> <u class=""d-block text-center"">IN MAKKAH</u> " +
                    $@"<p> ACCOMMODATION TYPE: <span class=""fillable""> {makkahAccomadation?.Accomodation.AccomodationName} </span> </p> <p>Name: <span class=""fillable""> " +
                    $@"{makkahAccomadation?.AccomodationName} </span></p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">{makkahAccomadation?.CheckIn}</span></p>" +
                    $@" &nbsp;&nbsp;<p class=""d-inline"">DAYS: <span class=""fillable"">{makkahAccomadation?.Days}</span></p> <p>DISTANCE (METERS): " +
                    $@"<span class=""fillable"">{makkahAccomadation?.DistanceInMeters}</span></p> <p>FOOD PACKAGE: <span class=""fillable"">{makkahAccomadation?.FoodPackage.Package}</span></p> " +
                    $@"<p>LOAD PER ROOM / WASHROOM: <span class=""fillable"">{makkahAccomadation/*?.LoadPerRoom*/} / {makkahAccomadation/*?.LoadPerWashroom*/} </span>" +
                    $@"<b> {(_makkahIndId is not null ? $@"*{_makkahIndId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "")}</b> " +
                    $@"</p> <p>ROOM SHARING: <span class=""fillable"">{makkahAccomadation?.Sharing}</span></p> " +
                    $@"<p STYLE=""text-decoration: underline;"">If 'Family Room' is selected, please refer to the details provided in the 'Individual Facilities' column below.</p> </td>";

                dataFormPdfString += $@"<td class=""border-right"" colspan=""5"" style=""width: 25%;""> <u class=""d-block text-center"">IN AZIZIA</u> ";
                if (aziziaAccomadation is not null)
                {
                    dataFormPdfString += $@"<p> ACCOMMODATION TYPE: <span class=""fillable""> {aziziaAccomadation.Accomodation.AccomodationName} </span>" +
                    $@"</p><p>Name: <span class=""fillable"">{aziziaAccomadation.AccomodationName} </span></p> <p class=""d-inline"">CATEGORY: <span class=""fillable"">" +
                    $@"{aziziaAccomadation.AccomodationCategory.Category}</span></p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">{aziziaAccomadation.CheckIn}</span></p> <br>" +
                    $@" <p class=""d-inline"">DAYS: <span class=""fillable"">{aziziaAccomadation.Days}</span></p> &nbsp; &nbsp;" +
                    $@"<p class=""d-inline"">DISTANCE (METER): <span class=""fillable"">{aziziaAccomadation.DistanceInMeters}</span></p> <p>FOOD PACKAGE: " +
                    $@"<span class=""fillable"">{aziziaAccomadation.FoodPackage.Package}</span></p> <p>LOAD PER ROOM / WASHROOM: " +
                    $@"<span class=""fillable"">{aziziaAccomadation/*.LoadPerRoom*/} / {aziziaAccomadation/*.LoadPerWashroom*/}</span> " +
                    $@"<b> {(_aziziaIndId is not null ? $@"*{_aziziaIndId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "")}</b></p> " +
                    $@"<p>HARAM SHUTTLE: <span class=""fillable"">{aziziaAccomadation.Shuttle}</span></p> <p>RELIGIOUS LECTURES: " +
                    $@"<span class=""fillable"">{aziziaAccomadation.ReligiousLectures}</span></p> ";
                }
                else
                {
                    dataFormPdfString += $@"<p class=""text-center"">N/A</p> ";
                }
                dataFormPdfString += $@"</td>";

                dataFormPdfString += $@"<td class=""border-right"" colspan=""3"" style=""width: 25%;""> <u class=""d-block text-center"">IN MASHAIR</u> <p> MAKTAB: <span class=""fillable""> " +
                    $@"{familyHead.Package.CategoryZone} </span> </p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">08ZH 1445-H</span></p> &nbsp;&nbsp;<p class=""d-inline"">" +
                    $@"DAYS: <span class=""fillable"">5 (MINA / ARAFAT / MUZDALIFAH)</span></p> <p>FOOD PACKAGE: <span class=""fillable"">{minaAccomadation?.FoodPackage.Package}</span></p>" +
                    $@" <p>SOFA MATTRESS / SITTING: <span class=""fillable"">{minaAccomadation?.Bedding}</span> <br>CAMP/TENT SHARING: " +
                    $@"<span class=""fillable"">{minaAccomadation?.Camp}</span> <br>ARAFAT: <span class=""fillable"">{minaAccomadation?.Arafat}</span> <br>" +
                    $@"RELIGIOUS LECTURES: <span class=""fillable"">{minaAccomadation?.ReligiousLectures}</span></p></td>";

                dataFormPdfString += $@"<td class=""border-right"" colspan=""5"" style=""width: 25%;""> <u class=""d-block text-center"">IN MEDINA</u> " +
                    $@"<p> ACCOMMODATION TYPE: <span class=""fillable""> {makkahAccomadation?.Accomodation.AccomodationName} </span> </p> <p>Name: <span class=""fillable""> " +
                    $@"{makkahAccomadation?.AccomodationName} </span></p> <p class=""d-inline"">CHECK IN: <span class=""fillable"">{makkahAccomadation?.CheckIn}</span></p>" +
                    $@" &nbsp;&nbsp;<p class=""d-inline"">DAYS: <span class=""fillable"">{medinaAccomadation.Days}</span></p> <p>DISTANCE (METERS): " +
                    $@"<span class=""fillable"">{medinaAccomadation.DistanceInMeters}</span></p> <p>FOOD PACKAGE: <span class=""fillable"">{medinaAccomadation.FoodPackage.Package}</span></p> " +
                    $@"<p>LOAD PER ROOM / WASHROOM: <span class=""fillable"">{medinaAccomadation/*.LoadPerRoom*/} / {medinaAccomadation/*.LoadPerWashroom*/} </span>" +
                    $@"<b> {(_medinaIndId is not null ? $@"*{_medinaIndId.AdditionalFacilities.IndividiualFacilityDescriptionId}" : "")}</b> " +
                    $@"</p> <p>ROOM SHARING: <span class=""fillable"">{medinaAccomadation.Sharing}</span></p> " +
                    $@"<p STYLE=""text-decoration: underline;"">If 'Family Room' is selected, please refer to the details provided in the 'Individual Facilities' column below.</p> </td>";

            }

            dataFormPdfString += $@"</tr>" +
                $@"</table>";

            dataFormPdfString += $@"<table class=""table accommodation""> <tr class=""border""> <td class=""border-right"" colspan=""10""> " +
            $@"<p class=""h5 text-center mb-0"">INDIVIDUAL FACILITIES AND GIVEAWAYS</p> </td> </tr><tr> <td class=""border-right col-4""> " +
            $@"<h6>* Individual ADDITIONAL Facilities (I-ADF) <span class=""fillable show-prices-fac""> </span></h6>";

            if (familyHead.Age < 2)
            {
                for (int k = 0; k < individualFacilities.Count; k++)
                {
                    var _if = individualFacilities.ElementAt(k);
                    dataFormPdfString += $@"<span> <b>* {_if.AdditionalFacilities.IndividiualFacilityDescriptionId}</b> - {_if.AdditionalFacilities.IndividiualFacilityDescription.Description} " +
                        $@"<b class=""usd-fac-prices fac-prices"" style=""display: inline;"">(USD ${_if.AdditionalFacilities.InfantUsd}/-)</b></span><br />";

                }
            }
            else if (familyHead.Age < 12)
            {
                for (int k = 0; k < individualFacilities.Count; k++)
                {
                    var _if = individualFacilities.ElementAt(k);
                    dataFormPdfString += $@"<span> <b>* {_if.AdditionalFacilities.IndividiualFacilityDescriptionId}</b> - {_if.AdditionalFacilities.IndividiualFacilityDescription.Description} " +
                        $@"<b class=""usd-fac-prices fac-prices"" style=""display: inline;"">(USD ${_if.AdditionalFacilities.ChildUsd}/-)</b></span><br />";

                }
            }
            else
            {
                for (int k = 0; k < individualFacilities.Count; k++)
                {
                    var _if = individualFacilities.ElementAt(k);
                    dataFormPdfString += $@"<span> <b>* {_if.AdditionalFacilities.IndividiualFacilityDescriptionId}</b> - {_if.AdditionalFacilities.IndividiualFacilityDescription.Description} " +
                        $@"<b class=""usd-fac-prices fac-prices"" style=""display: inline;"">(USD ${_if.AdditionalFacilities.AdultUsd}/-)</b></span><br />";

                }
            }

            dataFormPdfString += $@"</td><td class=""border-right col-4""> <h6>** Extra INDIVIDUAL ADDITIONAL Facilities (EI-ADF) <span class=""fillable show-prices-extra hideInPrint"">" +
                $@"</span></h6>";

            int n = 0;
            foreach (var e in extraFacilities)
            {
                dataFormPdfString += $@"<span> <b>** {n + 1}</b> - {e.Description}" +
                    $@"<b class=""usd-extra-prices extra-prices"" style=""display: inline;"">(USD ${e.PriceUsd}/-)</b> </span> <br />";
                n++;
            }
            dataFormPdfString += $@"</td><td class=""border-right col-4""><h6>GIVEAWAYS</h6>";

            int l = 0;
            foreach (var g in gifts)
            {
                dataFormPdfString += $@"<span class="""" style=""padding-right: 10px""><b>{l + 1}</b> - {g.Gift.Giveaway}</span>";
                l++;
            }

            dataFormPdfString += $@"</td></tr><tr class=""border""> <td class=""border-right"" colspan=""10""> <p class=""d-inline fw-bold"">Remarks: </p>";

            if (remarks.Count == 0)
            {
                dataFormPdfString += $@"<p class=""d-inline fw-bold"">N/A</p>";
            }
            else
            {
                int r = 0;
                foreach (string s in remarks)
                {
                    dataFormPdfString += $@"<p class=""d-inline"">{r + 1}: {s}</p>";
                }
            }

            dataFormPdfString += $@"</td></tr><tr class=""border""><td class=""border-right"" colspan=""10""> <ul id=""l1""> <li data-list-text=""-"">I have read and accepted all " +
                $@"terms and conditions of the package, including the agreement and" +
                $@" details. I confirm my choices for Additional & Individual Additional Facilities. All provided information is accurate, and I commit to " +
                $@"following MORA/MOH KSA regulations and Taleemat.</li></ul></td></tr><tr class=""border""> <td class=""border-right"" colspan=""10""> " +
                $@"<div class=""d-flex justify-content-around align-items-stretch""> <div class=""col-4 d-flex-column px-3 "" style=""float: left;"">" +
                $@"<p class="" text-center"" style=""height: 90px; position: relative; top: 70px;"">{(agreement.SignDataForm ? DateTimeManager.GetDate(agreement.SignedOn) : "")}</p>" +
                $@"<p class=""border-top text-center text-nowrap "">Date</p></div><div class=""col-5 d-flex-column px-3 "" style=""float: right;"">";

            if (agreement.SignatureBase64 is not null && agreement.SignDataForm)
            {
                dataFormPdfString += $@"<p class=""text-center border-bottom border-dark""><img loading = ""lazy"" src=""{agreement.SignatureBase64}"" width=""180px"" /> </p>";
            }
            else
            {
                dataFormPdfString += $@"<p class=""text-center ""><div style=""height: 90px;"" class=""border-bottom border-dark""></div></p>";
            }
            dataFormPdfString += $@"<p class=""text-center text-nowrap"">SIGNATURE &amp; THUMB IMPRESSION OF APPLICANT</p></div></div></td></tr>";

            dataFormPdfString += $@"</table></div>" +
                    $@"</body></html>";


            var hajjDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = hajjPdfString,

                        }
                    }
            };

            var dataFormDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                        Margins = new MarginSettings() {
                            Top = 10,
                            Bottom = 10,
                            Left = 10,
                            Right = 10,
                        }
                    },
                Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = dataFormPdfString,
                            //WebSettings = new WebSettings() {
                            //    MinimumFontSize = 5
                            //}
                        }
                    }
            };

            var hajjDocBytes = _converter.Convert(hajjDoc);
            var dataFormBytes = _converter.Convert(dataFormDoc);
            MailMessage mm = new MailMessage("noreply@tmg.com.pk", agreement.Email);
            mm.IsBodyHtml = true;
            mm.Subject = "Confirmation: Receipt of Hajj 2024 Agreement & Internal Form";
            mm.Body = $"";

            mm.Attachments.Add(new Attachment(new MemoryStream(hajjDocBytes), "Hajj Agreement 2024.pdf"));
            mm.Attachments.Add(new Attachment(new MemoryStream(dataFormBytes), $"{familyHead.FullName} ({familyHead.FcId}) Data Form.pdf"));
            mm.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            NetworkCredential NetworkCred = new NetworkCredential();
            NetworkCred.UserName = "";
            NetworkCred.Password = "";
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = NetworkCred;
            smtp.Port = 587;
            smtp.Send(mm);

            // Return response
            return RedirectToAction("AlreadyExists", "HajjAgreement");

        }

        public async Task<IActionResult> SendPdfs(int id)
        {

            FamilyHead fh = _dbContext.FamilyHeads.Find(id);

            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            List<string> hajjPdfStringList = ConvertHajjAgreementToPdfString(id);
            string ItineraryString = ConvertItineraryToPdfString(id);

            List<string> hajjReceiptsList = ConvertHajjReceiptToPdfString(id);

            MailMessage mm = new MailMessage("noreply@tmg.com.pk", email);
            // MailMessage mm = new MailMessage("noreply@tmg.com.pk", "m.alikhaninbox@gmail.com");
            mm.IsBodyHtml = true;
            mm.Subject = " Attached Service/Itinerary Voucher, Hajj Agreement, Hajj Booklet & Others";
            mm.Body = $"";




            int h = 1;
            foreach (var s in hajjPdfStringList)
            {
                var pdfDoc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s,
                        }
                    }
                };

                var pdfDocBytes = _converter.Convert(pdfDoc);

                mm.Attachments.Add(new Attachment(new MemoryStream(pdfDocBytes), "Hajj Agreement " + h++ + ".pdf"));
            }


            var itPdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = ItineraryString,
                        }
                    }
            };

            var itPdfDocBytes = _converter.Convert(itPdfDoc);

            mm.Attachments.Add(new Attachment(new MemoryStream(itPdfDocBytes), "Itinerary.pdf"));

            mm.IsBodyHtml = true;

            int i = 1;
            foreach (var s in hajjReceiptsList)
            {
                var _pdfDoc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s,
                        }
                    }
                };

                var _pdfDocBytes = _converter.Convert(_pdfDoc);

                mm.Attachments.Add(new Attachment(new MemoryStream(_pdfDocBytes), "Hajj Receipt " + i++ + ".pdf"));
            }

            var visaAndPassports = _dbContext.FamilyMembers
                    .AsNoTracking()
                    .Where(e => e.HeadId == id)
                    .Select(e => new
                    {
                        e.FamilyHead.VisaImage,
                        e.FamilyHead.PassportImage
                    }).ToList();

            foreach (var vp in visaAndPassports)
            {
                if (vp.VisaImage is not null)
                {
                    string visaPath = Path.Combine(_host.WebRootPath, vp.VisaImage);
                    if (System.IO.File.Exists(visaPath))
                    {
                        mm.Attachments.Add(new Attachment(visaPath));
                    }

                }

                if (vp.PassportImage is not null)
                {
                    string passportPath = Path.Combine(_host.WebRootPath, vp.PassportImage);
                    if (System.IO.File.Exists(passportPath))
                    {
                        mm.Attachments.Add(new Attachment(passportPath));
                    }
                }
            }

            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "",
                Password = ""
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 587
            };
            smtp.Send(mm);

            return Ok(new
            {
                msg = "sent"
            });
        }

        public string ConvertItineraryToPdfString(int id)
        {

            var members = _dbContext.FamilyMembers
                            .Join(_dbContext.FamilyHeads,
                                fm => fm.FamilyHeadId,
                                fh => fh.FamilyHeadId,
                                (fm, fh) => new { fm, fh })
                            .Join(_dbContext.Packages,
                                fmh => fmh.fh.PackageId,
                                p => p.PackageId,
                                (fmh, p) => new { fmh.fm, fmh.fh, p })
                            .Join(_dbContext.Companies,
                                fmhp => fmhp.p.CompanyId,
                                c => c.CompanyId,
                                (fmhp, c) => new { fmhp.fm, fmhp.fh, fmhp.p, c })
                            .Where(result => result.fm.HeadId == id && !result.fh.Trashed)
                            .Select(result => new
                            {
                                result.fh.FcId,
                                CompanyCode = result.c.CompanyCode,
                                IndId = result.fh.IndId,
                                result.fm.HeadId,
                                result.fm.FamilyHeadId,
                                PackageName = result.p.Name,
                                result.fh.FullName,
                                PassportNumber = result.fh.PassportNumber,
                                TicketNumber = result.fh.TicketNumber,
                                PackageId = result.fh.PackageId,
                                Qurbani = _dbContext.FamilyHeadExtras
                                            .Any(fe => fe.ExtraFacilityDescriptionId == 1
                                            && fe.FamilyHeadId == result.fh.FamilyHeadId)
                                            ? "Yes"
                                            : "No"
                            })
                            .Distinct()
                            .AsNoTracking()
                            .ToList();

            var pnrMembers = _dbContext.PNRMembers
                                .Include(e => e.PNR)
                                .ThenInclude(e => e.Flight)
                                .ThenInclude(e => e.Airline)
                                .Where(
                                    e => members
                                        .Select(e => e.FamilyHeadId)
                                        .Contains(e.FamilyHeadId)
                                    )
                                .AsNoTracking()
                                .ToList();

            var accommodations = _dbContext.PackageAccomodtions
                                    .AsNoTracking()
                                    .Include(e => e.Hotel)
                                    .Include(e => e.FoodPackage)
                                    .Where(e => e.PackageId == (members.Count > 0 ? members.ElementAt(0).PackageId : 0))
                                    .OrderBy(e => e.ActualCheckInTime)
                                    .ToList();

            ItineraryReportModel model = new()
            {
                AirlineName = pnrMembers.ElementAtOrDefault(0)?.PNR?.Flight.Airline.Name,
                AirlineLogo = pnrMembers.ElementAtOrDefault(0)?.PNR?.Flight.Airline.AirlineLogo
            };

            int FamilyHeadId = members.Count > 0 ? members.ElementAt(0).FamilyHeadId : 0;
            var flightQuery = (from f in _dbContext.Flights
                               join fp in _dbContext.FlightPackages on f.FlightId equals fp.FlightId
                               join p in _dbContext.Packages on fp.PackageId equals p.PackageId
                               join pnr in _dbContext.PNRs on f.FlightId equals pnr.FlightId
                               join pnrm in _dbContext.PNRMembers on pnr.PNRId equals pnrm.PNRId
                               join ss in _dbContext.Sectors on f.FlightId equals ss.FlightId
                               join fh in _dbContext.FamilyHeads on pnrm.FamilyHeadId equals fh.FamilyHeadId
                               where fh.FamilyHeadId == FamilyHeadId && ss.DepartureDateTime != null && ss.ArrivalDateTime != null
                               group new { f, ss, pnr } by new
                               {
                                   pnr.Type,
                                   ss.FlightType,
                                   ss.FlightNo,
                                   f.OutboundFlightNumber,
                                   f.InboundFlightNumber,
                                   ss.Destination,
                                   ss.DepartureDateTime,
                                   ss.ArrivalDateTime
                               } into grp
                               orderby grp.Key.DepartureDateTime ascending, grp.Key.ArrivalDateTime ascending, grp.Key.FlightType ascending
                               select new
                               {
                                   PNRType = grp.Key.Type,
                                   FlightType = grp.Key.FlightType,
                                   OutboundFlightNumber = grp.Key.FlightNo,
                                   InboundFlightNumber = grp.Key.FlightNo,
                                   Destination = grp.Key.Destination,
                                   DepartureDate = grp.Key.DepartureDateTime,
                                   ArrivalDate = grp.Key.ArrivalDateTime
                               }).AsNoTracking().ToList();


            int familyPax = members.Count;
            foreach (var m in members)
            {
                var memberInGroup = _dbContext.GroupMembers
                    .AsNoTracking()
                    .Include(e => e.Group)
                    .Select(e => new
                    {
                        e.FamilyHeadId,
                        e.Group.Name,
                        e.GroupId,
                        e.Group.Code
                    })
                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);

                string cId =
                        $"{m.CompanyCode}" +
                        $".{m.IndId}" +
                        $".{m.HeadId}({familyPax})" +
                        $".{m.PackageName}";

                string groupNumber = "";
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Name}.{memberInGroup.GroupId}";
                    groupNumber = memberInGroup?.Code ?? "-";
                }

                ItineraryReportMemberModel _memberModel = new()
                {
                    Name = m.FullName,
                    Passport = m.PassportNumber,
                    TicketNumber = m.TicketNumber,
                    PnrNumber = pnrMembers.Select(e => new
                    {
                        e.FamilyHeadId,
                        e.PNR.Name
                    }).FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId)?.Name,
                    Qurbani = m.Qurbani,
                    CID = cId
                };

                model.Members.Add(_memberModel);
            }

            foreach (var sector in flightQuery)
            {
                ItineraryReportSectorModel _sectorModel = new()
                {
                    FlightNumber = sector.FlightType == 0 ?
                        sector.InboundFlightNumber : sector.OutboundFlightNumber,
                    Sector = sector.Destination,
                    DepartureDate = sector.DepartureDate?.ToString("dd MMM"),
                    DepartureTime = sector.DepartureDate?.ToString("HH:mm"),
                    ArrivalDate = sector.ArrivalDate?.ToString("dd MMM"),
                    ArrivalTime = sector.ArrivalDate?.ToString("HH:mm"),
                    Luggage = "",
                    Class = sector.PNRType == 0 ? "Economy" : "Business"
                };

                model.FlightSectors.Add(_sectorModel);
            }

            var rooms = _dbContext.FamilyHeadAdditionalFacilities
                        .Where(
                                e => members
                                    .Select(e => e.FamilyHeadId)
                                    .Contains(e.FamilyHeadId)
                                )
                        .Select(e => new
                        {
                            e.FamilyHeadId,
                            e.AdditionalFacilities.Room.RoomName,
                            e.AdditionalFacilities.PlaceId,
                            e.AdditionalFacilitiesId,
                            e.AdditionalFacilities.Room.PAX,
                        })
                        .GroupBy(e => new { e.AdditionalFacilitiesId, e.RoomName, e.PAX, e.PlaceId })
                        .Select(e => new
                        {
                            e.Key.RoomName,
                            Qty = e.Count() / e.Key.PAX,
                            e.Key.PlaceId
                        })
                        .AsNoTracking()
                        .ToList();


            foreach (var acc in accommodations)
            {
                string roomString = "", quantityString = "";

                roomString = string.Join(" / ", rooms.Where(e => e.PlaceId == acc.PlaceId).Select(e => e.RoomName));
                quantityString = string.Join(" / ", rooms.Where(e => e.PlaceId == acc.PlaceId).Select(e => e.Qty));

                ItineraryReportAccommodationModel _accModel = new()
                {
                    HotelName = acc.Hotel?.HotelName,
                    Nights = acc.Nights,
                    CheckIn = acc.ActualCheckInTime?.ToString("dd MMM"),
                    CheckOut = acc.ActualCheckOutTime?.ToString("dd MMM"),
                    MealPlan = acc.FoodPackage.Package,
                    RoomType = roomString,
                    Quantity = quantityString
                };

                model.Accommodations.Add(_accModel);
            }

            var travelRoute = _dbContext
                .Packages
                .AsNoTracking()
                .Select(e => new
                {
                    e.PackageId,
                    e.TravelRouteId,
                    e.TravelRoute
                })
                .FirstOrDefault(e => e.PackageId == members.ElementAt(0).PackageId).TravelRoute;

            var stops = _dbContext.TravelRouteStops
                .AsNoTracking()

                    .Include(e => e.Route)
                    .Where(e => e.TravelRouteId == travelRoute.TravelRouteId)
                    .Select(e => new
                    {
                        Route = $"{e.Route.StartPlace} to {e.Route.Destination}",
                        Date = e.CheckIn.ToString("dd MMM")
                    });

            var routeData = _dbContext.
                            FamilyHeadItineraryVehicles
                            .AsNoTracking()
                            .Select(e => new
                            {
                                e.FamilyHeadId,
                                e.Route.StartPlace,
                                e.Route.Destination,
                                e.Vehicle.VehicleType,
                                e.Quantity,
                                e.RouteId,
                                e.TravelRouteId
                            })
                            .Where(e => e.FamilyHeadId == id)
                            .ToList();


            foreach (var stop in routeData)
            {
                model.Routes.Add(new ItineraryReportRouteModel
                {
                    Route = $"{stop.StartPlace} to {stop.Destination}",
                    Vehicle = stop.VehicleType,
                    Quantity = stop.Quantity.ToString(),
                    Date = _dbContext.TravelRouteStops.Select(e => new
                    {
                        e.RouteId,
                        e.TravelRouteId,
                        e.CheckIn
                    }).FirstOrDefault(e => e.RouteId == stop.RouteId && e.TravelRouteId == stop.TravelRouteId).CheckIn.ToString("dd/MMM"),
                    Type = stop.VehicleType.ToString()
                });
            }
            model.Routes = model.Routes.OrderBy(m => m.Date).ToList();


            model.FamilyHeadId = id;

            // EXTRA ADDITIONAL FACILITY
            List<int> memberIds = _dbContext.FamilyMembers
                            .AsNoTracking()
                                .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                                .Select(e => e.FamilyHeadId)
                                .ToList();

            var extras = _dbContext.FamilyHeadExtras
                        .AsNoTracking()
                        .Select(e => new
                        {
                            e.FamilyHeadId,
                            e.ExtraFacilityDescriptionId,
                            e.ExtraFacilityDescription.Description,
                            e.FamilyHead.Package.Company.CompanyCode,
                            e.FamilyHead.IndId
                        })
                       .Where(e => memberIds.Contains(e.FamilyHeadId)
                       && e.ExtraFacilityDescriptionId != null);


            var extrasGrouped = extras
                        .GroupBy(e => e.ExtraFacilityDescriptionId);


            List<ADFDetails> eIADFDetails = new();

            int s = 1;
            foreach (var e in extrasGrouped)
            {
                string _extraFacMembers = "";
                ADFDetails _d = new();
                foreach (var obj in e)
                {
                    _d.Sr = s;
                    _d.DescriptionId = (int)obj.ExtraFacilityDescriptionId;
                    _d.Description = obj?.Description;
                    _extraFacMembers += _extraFacMembers.Equals("") ? $"{obj.CompanyCode}-{obj.IndId}"
                        : $", {obj.CompanyCode}-{obj.IndId}";
                    _d.Ids = _extraFacMembers;
                }
                s++;
                eIADFDetails.Add(_d);
            }

            var remarks = _dbContext.FamilyHeadRemarks
                           .Include(e => e.FamilyHead)
                           .ThenInclude(e => e.Package)
                           .ThenInclude(e => e.Company)
                           .Where(e => memberIds.Contains(e.FamilyHeadId))
                           .Select(e => new
                           {
                               e.Remarks,
                               e.FamilyHeadId,
                               e.FamilyHead.IndId,
                               e.FamilyHead.Package.Company.CompanyCode
                           })
                           .ToList();

            int rIter = 1;
            foreach (var r in remarks)
            {
                model.Remarks.Add(new MemberRemarks
                {
                    Sr = rIter,
                    Remarks = r.Remarks,
                    IndivdualId = $"{r.CompanyCode}-{r.IndId}"
                });
                rIter++;
            }


            model.EIADFDetails = eIADFDetails;


            string path = Path.Combine(_host.WebRootPath, "BookletTemplates", "Itinerary.html");
            string contents = "";

            using (StreamReader reader = new(path))
            {
                contents = reader.ReadToEnd();
            }

            contents = contents.Replace("{0}", model.AirlineLogo);
            contents = contents.Replace("{1}", model.AirlineName);

            string memberModel = "";

            foreach (var m in model.Members)
            {
                memberModel += $"""
                    <tr>
                        <td>{m.CID}</td>
                        <td>{m.Name}</td>
                        <td>{m.Passport}</td>
                        <td>{m.TicketNumber}</td>
                        <td>{m.PnrNumber}</td>
                        <td>{m.Qurbani}</td>
                    </tr>
                """;
            }

            contents = contents.Replace("{2}", memberModel);

            string flightModel = "";
            foreach (var m in model.FlightSectors)
            {
                flightModel += $"""
                    <tr>
                        <td>{m.FlightNumber}</td>
                        <td>{m.Sector}</td>
                        <td>{m.DepartureDate}</td>
                        <td>{m.DepartureTime}</td>
                        <td>{m.ArrivalDate}</td>
                        <td>{m.ArrivalTime}</td>
                        <td>As Per Airline Policy</td>
                        <td>{m.Class}</td>
                    </tr>
                """;
            }

            contents = contents.Replace("{3}", flightModel);

            string accommodationModel = "";
            foreach (var m in model.Accommodations)
            {
                accommodationModel += $"<tr>";

                if (m.CheckIn.ToString() == "14 Jun" && (m.HotelName == null || m.HotelName == ""))
                {
                    accommodationModel += "<td>Mashair (Mina / Arafat / Muzdalifah)</td>";
                }
                else
                {
                    accommodationModel += $"<td>{m.HotelName}</td>";
                }
                accommodationModel += $"""
                            <td>{m.CheckIn.Substring(0, 6)}<br></td>
                            <td>{m.CheckOut}</td>
                            <td>{m.Nights}</td>
                            <td>{m.RoomType}</td>
                            <td>{m.Quantity}</td>
                            <td>{m.MealPlan}</td>
                            <td>{m.View}</td>
                            <td>{m.ConfimationNumber}</td>
                        </tr>
                """;
            }

            contents = contents.Replace("{4}", accommodationModel);

            string routesModel = "";
            int counter = 0;
            string location = "";

            foreach (var m in model.Routes)
            {
                counter++;
                routesModel += $"""
                    <tr>
                        <td>{m.Route}</td>
                        <td>{m.Vehicle}</td>
                        <td>{m.Type}</td>
                        <td>{m.Quantity}</td>
                """;
                for (int i = 0; i < model.Accommodations.Count; i++)
                {

                    if (location == "")
                    {
                        location = m.Route.ToString().Contains("Jeddah Airport to") ? "After Hajj" : "Before Hajj";
                    }
                    if (location == "Before Hajj")
                    {
                        if (counter == 1 && i == 0)
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (counter == 2 && i == 1)
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (counter == 3 && i == 1 && (m.Route.ToString().Contains("Medinah Train Station") ||
                        m.Route.ToString().Contains("Medinah to Makkah")))
                        {

                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (counter == 3 && i == 2)
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }

                        else if (m.Route.ToString().Contains("to Azizia") && (model.Accommodations[i].HotelName != null ?
                        model.Accommodations[i].HotelName.Contains("Azizia") : false))
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (m.Route.ToString().Contains("to Jeddah Airport"))
                        {
                            int abc = 0;
                            string ab = "";
                            foreach (var item in model.FlightSectors)
                            {
                                if (item.DepartureDate.Contains("Jul"))
                                {
                                    abc = Convert.ToInt32(item.DepartureDate.Substring(0, 2));
                                    ab = item.DepartureDate;
                                    if ((item.Sector.Substring(0, 3).Contains("JED") || item.Sector.Substring(0, 3).Contains("MED")) && !item.Sector.Contains("JED-MED"))
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(item.DepartureDate.Substring(0, 2)) > abc)
                                    {
                                        abc = Convert.ToInt32(item.DepartureDate.Substring(0, 2));
                                        ab = item.DepartureDate;
                                        if ((item.Sector.Substring(0, 3).Contains("JED") || item.Sector.Substring(0, 3).Contains("MED")) && !item.Sector.Contains("JED-MED"))
                                        {
                                            break;
                                        }
                                    }
                                }

                            }
                            var count = model.FlightSectors.Count;
                            routesModel += $"<td> {ab}</td>";
                            break;
                        }
                        else if (m.Route.Contains("Azizia to Mashair"))
                        {
                            routesModel += $"<td> 14 Jun </td>";
                            break;
                        }
                        else if (counter == 4 && i == 1 && m.Route.ToString().Contains("Makkah Train Station"))
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (counter == 4 && i == 2 && m.Route.ToString().Contains("Azizia Building to Makkah Hotel"))
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckOut} </td>";
                            break;
                        }
                        else if (counter == 4 && i == 2)
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (counter == 5 && i == 2 && m.Route.ToString().Contains("Makkah Hotel to Mashair"))
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (counter == 5 && i == 3)
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (m.Route.ToString().Contains("Azizia to Makkah Hotel"))
                        {
                            foreach (var item in model.Accommodations)
                            {
                                if (item.HotelName == "Rafahya Hotel Azizia" || item.HotelName == "Azizia Building")
                                {
                                    routesModel += $"<td> {item.CheckOut} </td>";
                                }

                            }

                            break;
                        }
                        else if (counter == 6 && i == 3)
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckIn} </td>";
                            break;
                        }
                        else if (counter == 7 && i == 3)
                        {
                            routesModel += $"<td> {model.Accommodations[i].CheckOut} </td>";
                            break;
                        }
                    }
                    else
                    {
                        //before hajj
                        for (int j = 0; j < model.Accommodations.Count; j++)
                        {
                            if (counter == 1 && j == 0)
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            if (counter == 2 && m.Route.ToString().Contains("Makkah Hotel to Azizia"))
                            {
                                if (model.Accommodations[j].HotelName.Contains("Azizia"))
                                {
                                    routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                    break;
                                }
                                //routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                //break;
                            }
                            else if (counter == 2 && m.Route.ToString().Contains("to Mashair"))
                            {
                                routesModel += $"<td> 14 Jun </td>";
                                break;
                            }
                            else if (counter == 3 && j == 2)
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (counter == 3 && j == 1 && (m.Route.ToString().Contains("Makkah  to Medinah") || m.Route.ToString().Contains("Azizia  to Jeddah Airport") || m.Route.ToString().Contains("Azizia to Jeddah Airport") || m.Route.ToString().Contains("Makkah to Medinah") || m.Route.ToString().Contains("Station  to Medinah Hotel")))
                            {
                                if (model.Accommodations.Count == 2)
                                {
                                    routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                    break;
                                }
                                else if (model.Accommodations.Count == 3)
                                {
                                    routesModel += $"<td> {model.Accommodations[2].CheckIn} </td>";
                                    break;
                                }
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (counter == 3 && j == 1 && (m.Route.ToString().Contains("Azizia  to Jeddah Airport") || m.Route.ToString().Contains("Azizia to Jeddah Airport")))
                            {
                                int abc = 0;
                                string ab = "";
                                foreach (var item in model.FlightSectors)
                                {
                                    if (Convert.ToInt32(item.DepartureDate.Substring(0, 2)) > abc)
                                    {
                                        abc = Convert.ToInt32(item.DepartureDate.Substring(0, 2));
                                        ab = item.DepartureDate;
                                        if ((item.Sector.Substring(0, 3).Contains("JED") || item.Sector.Substring(0, 3).Contains("MED")) && !item.Sector.Contains("JED-MED"))
                                        {
                                            break;
                                        }
                                    }
                                }
                                var count = model.FlightSectors.Count;
                                routesModel += $"<td> {ab} </td>";
                                break;
                            }
                            else if (model.Accommodations.Count == 3 && counter == 4 && j == 2 && (m.Route.ToString().Contains("Makkah Train Station") ||
                            m.Route.ToString().Contains("Makkah  to Medinah") || m.Route.ToString().Contains("Makkah to Medinah") || m.Route.ToString().Contains("Station  to Medinah Hotel") ||
                            m.Route.ToString().Contains("Station to Medinah Hotel")))
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (model.Accommodations.Count == 3 && counter == 5 && j == 2 && (m.Route.ToString().Contains("Makkah Train Station") ||
                            m.Route.ToString().Contains("Makkah  to Medinah") || m.Route.ToString().Contains("Makkah to Medinah") || m.Route.ToString().Contains("Station to Medinah Hotel") ||
                            m.Route.ToString().Contains("Station  to Medinah Hotel")))
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (counter == 4 && j == 3 && (m.Route.ToString().Contains("Makkah Train Station") ||
                            m.Route.ToString().Contains("Makkah  to Medinah") || m.Route.ToString().Contains("Makkah to Medinah") || m.Route.ToString().Contains("Station  to Medinah Hotel") ||
                            m.Route.ToString().Contains("Station to Medinah Hotel")))
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (counter == 5 && j == 3 && (m.Route.ToString().Contains("Makkah Train Station") ||
                            m.Route.ToString().Contains("Makkah  to Medinah") || m.Route.ToString().Contains("Makkah to Medinah") || m.Route.ToString().Contains("Station  to Medinah Hotel") ||
                            m.Route.ToString().Contains("Station to Medinah Hotel")))
                            {
                                if (model.Accommodations.Count == 5)
                                {
                                    routesModel += $"<td> {model.Accommodations[j].CheckOut} </td>";
                                    break;
                                }
                                else
                                {
                                    routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                    break;
                                }
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }

                            else if (m.Route.ToString().Contains("to Medinah Airport") || m.Route.ToString().Contains("to Jeddah Airport"))
                            {
                                int abc = 0;
                                string ab = "";
                                foreach (var item in model.FlightSectors)
                                {
                                    if (Convert.ToInt32(item.DepartureDate.Substring(0, 2)) > abc)
                                    {
                                        abc = Convert.ToInt32(item.DepartureDate.Substring(0, 2));
                                        ab = item.DepartureDate;
                                        if ((item.Sector.Substring(0, 3).Contains("JED") || item.Sector.Substring(0, 3).Contains("MED")) && !item.Sector.Contains("JED-MED"))
                                        {
                                            break;
                                        }
                                    }
                                }
                                var count = model.FlightSectors.Count;
                                routesModel += $"<td> {ab}  </td>";
                                break;
                            }
                            else if (m.Route.Contains("Azizia to Mashair"))
                            {
                                routesModel += $"<td> 14 Jun </td>";
                                break;
                            }

                            else if (counter == 4 && j == 2 && m.Route.ToString().Contains("Azizia Building to Makkah Hotel"))
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckOut} </td>";
                                break;
                            }

                            else if (counter == 5 && j == 2 && m.Route.ToString().Contains("Makkah Hotel to Mashair"))
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (counter == 5 && j == 3)
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (m.Route.ToString().Contains("Azizia to Makkah Hotel"))
                            {
                                foreach (var item in model.Accommodations)
                                {
                                    if (item.HotelName == "Rafahya Hotel Azizia" || item.HotelName == "Azizia Building")
                                    {
                                        routesModel += $"<td> {item.CheckOut} </td>";
                                    }

                                }

                                break;
                            }
                            else if (counter == 6 && j == 3)
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckIn} </td>";
                                break;
                            }
                            else if (counter == 7 && j == 3)
                            {
                                routesModel += $"<td> {model.Accommodations[j].CheckOut} </td>";
                                break;
                            }
                        }

                        routesModel += "</tr>";

                    }



                }


            }

            contents = contents.Replace("{5}", routesModel);


            string extrasString = "";
            foreach (var e in eIADFDetails)
            {
                extrasString += $"<tr> <td style='text-align: left;'> {e.Description} </td><td style='text-align: left;'> {e.Ids} </td></tr>";
            }

            contents = contents.Replace("{6}", extrasString);

            string remarksString = "";

            foreach (var e in model.Remarks)
            {
                remarksString += $"<tr><td style='text-align: left;'>{e.Remarks}</td><td style='text-align: left;'>{e.IndivdualId}</td></tr>";
            }

            contents = contents.Replace("{7}", remarksString);


            return contents;
        }

        public List<string> ConvertHajjAgreementToPdfString(int id)
        {
            List<string> model = new();
            var members = _dbContext.FamilyMembers
                            .AsNoTracking()
                            .Where(e => e.HeadId == id && !e.FamilyHead.Trashed)
                            .Select(e => e.FamilyHeadId)
                            .ToList();

            foreach (var m in members)
            {
                var agreement = _dbContext
                                .UserHajjAgreements.AsNoTracking()
                                .FirstOrDefault(e => e.FamilyHeadId == m);

                string hajjPdfString = $@"<!DOCTYPE html>
            <html lang=""en"">

            <head>
            <meta charset=""UTF-8"" />
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
            <title>AGREEMENT- HAJJ (1445-H)</title>
            <style>
                * {{
                box-sizing: border-box;
                font-family: system-ui, -apple-system, ""Segoe UI"", Roboto,
                    ""Helvetica Neue"", Arial, ""Noto Sans"", ""Liberation Sans"", sans-serif,
                    ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"",
                    ""Noto Color Emoji"";
                }}
                body {{
                margin: 15px;
                }}

                p {{
                font-size: 12px;
                margin: 0;
                }}
                span {{
                font-weight: bold;
                }}

                
                .company_name_box h1 {{
                margin: 0;
                padding: 10px 0;
                }}
                header {{
                display: flex;
                justify-content: space-between;
                align-items: center;
                }}
                
                .AgreementText{{
                    background-color: rgb(68, 67, 67);
                    display: inline-block;
                    color: white;
                    padding:  10px;
                    border-radius: 10px;
                }}
                .DateText p{{
                font-size:10px ;
                margin-top: 5px;
                margin-left: 1px;
                }}

                

                .FirstPart{{
                margin: 10px 0;
                }}
                .company_Info{{
                font-size: 8px;
                }}
                .Second_Part{{
                float: left !important;
                }}
                .Second_Part p{{
                font-size: 8px
                }}

                .userInfoBoxs{{
                float: right !important;
                    margin-bottom: 25px;
                }}
                
                h1 {{
                text-align: center !important;
                }}
                
                .profile_picture{{
                border: 1px solid black;
                }}
                
                .userInfoBox {{
                display: flex;
                justify-content: space-between;
                align-items: center;
                }}
                .table_container {{
                margin-top: 10px;
                float: left !important;
                }}
                

                .table_profile {{
                border-collapse: collapse;
                width: 100%;
                }}
                .table_profile tr {{
                display: flex;
                justify-content: space-between;
                font-size: 12px !important;
                border: 1px solid black;
                
                }}
                .table_profile th {{
                font-size: 8px !important;
                
                }}
                .table_profile td {{
                font-size: 8px !important;
                padding: 2px 0;
                }}
                .table_profile tr td:nth-child(2) {{
                text-align: right;
                }}
                .packagesection {{
                margin-top: 30px;
                }}
                .Our_Package {{
                border-collapse: collapse;
                width: 100%;
                border: 1px solid black;
                }}
                .Our_Package td {{
                font-size: 8px !important;
                }}
                .Our_Package th {{
                font-size: 8px !important;
                padding: 3px 0;
                text-align: left;
                }}
                .Our_Package tr {{
                border-bottom: 1px solid black;
                }}
                .Our_Package th:nth-child(2) {{
                padding-left: 100px;
                }}
                .Our_Package td:nth-child(2) {{
                padding-left: 100px;
                }}

                .termsConditions{{
                margin-top:15px;
                }}
                .termsConditions p{{
                text-align: justify;
                }}
                .termsConditions h5{{
                font-size: 10px;
                background-color: lightblue;
                color: black;
                display: inline-block;
                padding: 0 8px !important ;
                }}
                .termsConditions h5,p,li{{
                margin: 0;padding: 0;
                }}
                .termsConditions p{{
                font-size: 8px;
                }}
                .profile_image{{
                float: left !important;
                width: 100px;
                height: 130px;
                border: 1px solid black;
                margin-top: 15px;
                box-shadow: rgba(6, 24, 44, 0.4) 0px 0px 0px 2px, rgba(6, 24, 44, 0.65) 0px 4px 6px -1px, rgba(255, 255, 255, 0.08) 0px 1px 0px inset;
                margin: 11px 20px;
                }}
                
                .UnOrded_list li{{
                font-size: 8px;
                }}
                footer{{
                display: flex;
                justify-content: space-between;
                margin-top: 2px;
                }}
                .QR_Code_logo{{
                text-align: right;
                }}
                .footer-text {{
                display: flex;
                justify-content: center;
                align-items: center;
                text-align: center;

                }}
                
            </style>
            </head>

            <body>
            <header>
                <div class=""agreement_Container"">
                    <div class=""AgreementText"">
                        <h4 style=""margin: 0; padding: 0;"">AGREEMENT HAJJ 2024 - (1445-H)
                        </h4>
                    </div>
                    <div class=""DateText"">
                        <p> <span>DATE OF AGREEMENT: {DateTimeManager.GetDate(DateTime.Today)}</span></p>
                    </div>
                </div>
            </header>
            <div class=""FirstPart"">
                <h6 style=""margin: 0; padding: 0;"">FIRST PART:</h6>
                <p class=""company_Info"">M/S {agreement.CompanyName}. A CLUSTER PART OF
                    {agreement.ClusterName}. <br> A COLLABORATIVE CLUSTER-AFFILIATE HAJJ GROUP
                    MANAGEMENT COMPANY.</p>
            </div>
            <div class=""SecondPart_Table_box"">
                    <div class=""Second_Part"">
                        <h6 style=""margin: 0; padding: 0;"">SECOND PART:</h6>
                        <p style=""margin: 0; padding: 0; font-weight: bold;"">{agreement.Title}+{agreement.Name}</p>
                        <p><span>Passport Number: </span>{agreement.PassportNumber}</p>
                        <p><span>Address: </span> {agreement.Address}</p>
                        <p><span>Contact: </span>{agreement.Contact}</p>
                        <p><span>Email: </span>{agreement.Email}</p>
                        <p><span>Category: </span>{agreement.Category}</p>
                        <p><span>Hajj Application No: </span>{agreement.HajjAppNumber}</p>
                    </div>
                    <div class=""userInfoBoxs"">
                            <img src=""[URL LINK]/{agreement.HajjiImage}"" alt="""" class=""profile_image"">
                        <div class=""table_container"" id=""div1"">
                            <table class=""top_service_table table_profile title_table"">
                                <tr style=""background-color: lightblue;"">
                                    <th colspan=""1"" style=""
                                    text-align: left;
                                    font-size: 10px;
                                    color: black;
                                    background-color: lightblue;
                                    font-weight: bolder;

                                "">
                                        BASIC PACKAGE WITHOUT AIR TICKET, INCLUDING *I-ADF: ({agreement.Package} MEDINAH {agreement.MedinahArrival})
                                    </th>


                                </tr>
                                <tr>
                                    <td style=""font-weight: bold;""> DESCRIPTION</td>
                                    <td style=""font-weight: bold;""> AMOUNT (USD)</td>
                                </tr>
                                <tr>
                                    <td>{agreement.IADFString}</td>
                                    <td>{agreement.IADFPriceWithPackage}</td>
                                </tr>
                                <tr>
                                    <td>** EI-ADF (EXTRA INDIVIDUAL ADDITIONAL FACILITY) </td>
                                    <td>{agreement.ExtraFacilitiesPriceUsd}</td>
                                </tr>
                                <tr>
                                    <td>AIR TICKET </td>
                                    <td>{agreement.FlightTicketPriceUsd}</td>
                                </tr>
                                <tr>
                                    <td>AIR TICKET DIFFERENCE AMOUNT (IF APPLICABLE)</td>
                                    <td>{agreement.FlightTicketDifferencePriceUsd}</td>
                                </tr>
                                <tr id=""AmountUSD_ROW"">
                                    <td>TOTAL AMOUNT</td>
                                    <td contenteditable=""true"" id=""editable-row"">{agreement.TotalPerPersonUsd}</td>
                                </tr>
                                <tr>
                                    <td>DISCOUNT/REBATE/ROE ADJUSTMENT (IF APPLICABLE)</td>
                                    <td>{agreement.DiscountUsd}</td>
                                </tr>
                                <tr>
                                    <td>NET RECEIVABLE AMOUNT (USD)</td>
                                    <td>{agreement.NetReceivable}</td>
                                </tr>
                                <tr style=""background-color: grey !important; color: white"">
                                    <td>PAYMENT RECEIVED (USD) (REFER TO STATEMENT) </td>
                                    <td></td>
                                </tr>

                            </table>
                        </div>
                        </div>
            </div>

            <div class=""packagesection"">
                <table class=""Our_Package"">
                    <thead>
                        <tr style=""background-image: linear-gradient(315deg, #278533 0%, #378b29 74%); color: white;"">
                            <th>PACKAGE DETAIL (TABLE B)</th>
                            <th>TRANSPORT ROUTE (TABLE C)</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{agreement.Package}</td>
                            <td>
                                Upon Arrival in KSA, Airport to Hotel / Building by Airport Naqaba
                                Sharing Group Bus
                            </td>
                        </tr>
                        <tr>
                            <td>
                                * Medina: {agreement.MedinaFacilities}
                            </td>
                            <td>
                                Hotel to Train Station &amp; Train Station to Accommodation
                                (Intercity) by Group Bus
                            </td>
                        </tr>
                        <tr>
                            <td>
                                * Makkah: {agreement.MakkahFacilities}
                            </td>
                            <td>
                                Med to Mak / Mak to Med by Bus or by Train in Economy Class with
                                Hand Carry only
                            </td>
                        </tr>

                        <tr>
                            <td>* Azizia: {agreement.AziziaFacilities}</td>
                            <td>
                                Luggage will be Shuttled separately in a Truck with approximate
                                8hrs journey (in-case of a train)
                            </td>
                        </tr>

                        <tr>
                            <td>** EI-ADF: {agreement.ExtraFacilities}</td>
                            <td>
                                Mashair: Mina, Arafat, Muzdalfa &amp; Back to Azizia by Group Bus
                                Seat to Seat
                            </td>
                        </tr>
                        <tr>
                            <td>Airline: {agreement.Airline}</td>
                            <td>
                                Makkah/Medinah Accommodation to Airport by Group Sharing Bus
                            </td>
                        </tr>
                        <tr>
                            <td>Medina Arrival: {agreement.MedinahArrival}</td>
                            <td>
                                Ziarat in Makkah &amp; Medinah (as per package) by Group Sharing
                                Bus
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class=""termsConditions"">
                {agreement.SharedAgreement}
                <hr>
                {agreement.SpecificAgreement}
            </div>

            <div class=""signature-bottom"" style=""margin-top: 30px;"">
                <div class=""footer-text"">
                    <p><b> I HAVE CAREFULLY READ AND UNDERSTOOD THE TERMS AND CONDITIONS OF THIS AGREEMENT AND THESE ARE CORRECT
                            <br>
                            IN WITNESS WHEREOF </b> , the parties hereto have executed this Hajj Program Reservation Agreement
                        as of the date first above written.</p>
                </div>

                <div class=""signature-part"" >

                    <div style="" width: 200px; height: 200px;   text-align: center; float: left; padding: 20px;"">
                        <div style=""margin-top: 155px;"">
                            <span>{DateTime.Now.ToString("dd/MM/yyyy")}</span>
                            <p style=""border-top: 2px solid black;"">Date</p>
                        </div>

                    </div>

                    <div style=""float: left; padding: 20px; "">
                        <div class=""col-md-12 mt-3"">
                        <p class=""text-center border-bottom border-dark""><img src=""{agreement.SignatureBase64}"" width=""220"" height=""200"" /> </p>
                        </div>

                        <p style=""border-top: 2px solid black;"">Applicant's Signature & Thumb Impression</p>

                    </div>

                    <div style=""float: left; padding: 20px;"">
                        <img src=""[URL LINK]/{agreement.CompanySeal}"" id=""signatureStamp"" width=""180"" height=""180"">
                        <p style=""border-top: 2px solid black;"">Company Seal Representative Sign</p>
                    </div>
                </div>
                
            </div>
            </body></html>
            ";

                model.Add(hajjPdfString);
            }
            return model;
        }

        public List<string> ConvertHajjReceiptToPdfString(int id)
        {

            List<string> _ = new();
            var invoices = _dbContext.InvoicesWithoutTrash
                            .Where(e => e.FamilyHeadId == id)
                            .AsNoTracking()
                            .ToList();

            int memberCount = _dbContext.FamilyMembers
                                .Where(e => e.HeadId == id)
                                .Count();

            int z = 1;
            foreach (var invoice in invoices)
            {
                var query = (from fh in _dbContext.FamilyHeads
                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                             join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                             join mc in _dbContext.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                             join m in _dbContext.Munazzims on mc.MunazzimId equals m.MunazzimId
                             join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                             join inv in _dbContext.Invoices on fh.FamilyHeadId equals inv.FamilyHeadId
                             join ca in _dbContext.CompanyAddresses on c.CompanyId equals ca.CompanyId
                             join cc in _dbContext.CompanyContacts on c.CompanyId equals cc.CompanyId
                             where inv.InvoiceId == invoice.InvoiceId && !fh.Trashed && !c.Trashed && !inv.Trashed
                             select new
                             {
                                 EnrollmentNO = cl.CompanyLicense,
                                 MunazimName = m.ClusterNumber.ToString() + "-" + m.Name,
                                 fh.HajjApplicationNumber,
                                 ApplicantName = fh.GivenName + " " + fh.SurName,
                                 scheme = fh.ApplicantType,
                                 PackageName = p.Name,
                                 PackageId = p.PackageId,
                                 fh.FamilyHeadId,
                                 inv.AmountRecived,
                                 inv.ModeOfPayment,
                                 ReferenceNo = inv.Instrument,
                                 inv.Date,
                                 inv.Remarks,
                                 MemberImage = fh.MemberImage,
                                 CompanyName = c.CompanyName,
                                 CompanyAddress = ca.Address,
                                 CompanyContact = cc.CompanyContact,
                                 inv.ImageBase64,
                                 inv.ImagePath,
                                 CompanyId = c.CompanyId,
                                 inv.ProformaNumber,
                                 fh.SignatureBase64

                             })
                             .AsNoTracking()
                             .ToList();

                int FamilyHeadId = 0;
                int PackageId = 0;
                foreach (var item in query)
                {
                    FamilyHeadId = item.FamilyHeadId;
                    PackageId = item.PackageId;
                    break;
                }
                var packageaccomodation = _dbContext.PackageAccomodtions.AsNoTracking().Where(e => e.PackageId == PackageId && e.PlaceId == 1);
                var headid = _dbContext.FamilyMembers.AsNoTracking().FirstOrDefault(e => e.FamilyHeadId == FamilyHeadId);
                var familyhead = _dbContext.FamilyMembers
                .AsNoTracking()
                    .Select(e => new
                    {
                        e.HeadId,
                        e.FamilyHead.Trashed,
                        e.FamilyHead.FlightTicketType,
                        e.FamilyHead.FlightTicketDifferencesUsd,
                        e.FamilyHead.Age,
                        e.FamilyHead.UsdDiscountAmount
                    })
                                    .Where(e => e.HeadId == (headid == null ? 0 : headid.HeadId) && !e.Trashed)
                            .ToList();

                var _p = _dbContext.Packages
                            .AsNoTracking()
                            .Select(e => new
                            {
                                e.PackageId,
                                e.FlightTicketPriceUsd,
                                e.InfantUsd,
                                e.ChildUsd,
                                e.AdultUsd
                            })
                            .FirstOrDefault(e => e.PackageId == PackageId);



                float ticketTotal = 0;
                float ticketDiffTotal = 0;
                float discount = 0;
                float packagePriceTotal = 0;

                float infantPackageUsd = 0, childPackageUsd = 0, adultPackageUsd = 0;

                foreach (var f in familyhead)
                {
                    if (f.FlightTicketType != FlightTicketType.Excluded)
                    {
                        ticketTotal += _p.FlightTicketPriceUsd;
                    }
                    if (f.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                    {
                        ticketDiffTotal += f.FlightTicketDifferencesUsd ?? 0;
                    }

                    discount += f.UsdDiscountAmount;

                    if (f.Age < 2)
                    {
                        packagePriceTotal += _p.InfantUsd;
                        infantPackageUsd = _p.InfantUsd;
                    }
                    else if (f.Age < 12)
                    {
                        packagePriceTotal += _p.ChildUsd;
                        childPackageUsd = _p.ChildUsd;

                    }
                    else
                    {
                        packagePriceTotal += _p.AdultUsd;
                        adultPackageUsd = _p.AdultUsd;
                    }
                }
                HajjReceiptModel model = new HajjReceiptModel();
                if (query.Count > 0)
                {
                    model.EnrollmentNo = query[0].EnrollmentNO ?? "";
                    model.MunazamCompany = query[0].MunazimName ?? "";
                    model.HajjApplicationNumber = query[0].HajjApplicationNumber ?? "";
                    model.ApplicantName = query[0].ApplicantName ?? "";
                    model.Scheme = query[0].scheme == null ? "" : query[0].scheme.ToString();
                    model.HajjPackage = query[0].PackageName ?? "";
                    model.MakkahAccommodation = "NO";
                    model.PackageAmount = "As Per Package";
                    model.AdditionFacilities = "NO";
                    model.AdditionalAmount = "0";
                    model.TotalAmount = "As Per Package";
                    model.AmountReceived = query[0].AmountRecived == null ? "0" : query[0].AmountRecived.ToString("N0");
                    model.PaymentMode = query[0].ModeOfPayment == null ? "" : query[0].ModeOfPayment.ToString();
                    model.ReferenceNo = query[0].ReferenceNo == null ? "" : query[0].ReferenceNo.ToString();
                    model.Amount = query[0].AmountRecived == null ? "0" : query[0].AmountRecived.ToString("N0");
                    model.Date = query[0].Date == null ? "" : query[0].Date.ToString("dd/MM/yyyy");
                    model.Remarks = query[0].Remarks == null ? "" : query[0].Remarks.ToString();
                    model.MemberImage = query[0].MemberImage == null ? "" : query[0].MemberImage.ToString();
                    model.CompanyName = query[0].CompanyName == null ? "" : query[0].CompanyName.ToString();
                    model.CompanyContact = query[0].CompanyContact == null ? "" : query[0].CompanyContact.ToString();
                    model.CompanyAddress = query[0].CompanyAddress == null ? "" : query[0].CompanyAddress;
                    model.MakkahAccommodation = packageaccomodation.Count() > 0 ? "YES" : "NO";
                    model.CompanyStamp = _dbContext.MisStamps.FirstOrDefault(e => e.CompanyId == query[0].CompanyId)?.FilePath;
                    model.ReceiptNumber = query[0].ProformaNumber;
                    model.AccountantSign = _dbContext.MisStamps.FirstOrDefault(e => e.CompanyId == query[0].CompanyId)?.AccountantSignatureFilePath;
                    model.ApplicantSignature = query[0].SignatureBase64;
                }

                string contents = "";
                string path = Path.Combine(_host.WebRootPath, "BookletTemplates", "HajjPrint.html");

                using (StreamReader reader = new(path))
                {
                    contents = reader.ReadToEnd();
                }

                contents = contents.Replace("{1}", model.CompanyName);
                contents = contents.Replace("{2}", model.CompanyAddress);
                contents = contents.Replace("{3}", model.CompanyContact);
                contents = contents.Replace("{4}", "02134860711");
                contents = contents.Replace("{5}", model.ReceiptNumber.ToString());
                contents = contents.Replace("{6}", DateTime.Now.ToString());
                contents = contents.Replace("{7}", model.EnrollmentNo);
                contents = contents.Replace("{8}", model.MunazamCompany);
                contents = contents.Replace("{9}", model.MemberImage);
                contents = contents.Replace("{10}", model.HajjApplicationNumber);
                contents = contents.Replace("{11}", model.ApplicantName);
                contents = contents.Replace("{12}", model.Scheme);
                contents = contents.Replace("{13}", model.HajjPackage);
                contents = contents.Replace("{14}", model.MakkahAccommodation);
                contents = contents.Replace("{15}", model.PackageAmount);
                contents = contents.Replace("{16}", "No");
                contents = contents.Replace("{17}", "0");
                contents = contents.Replace("{18}", model.TotalAmount);
                contents = contents.Replace("{19}", model.AmountReceived);
                contents = contents.Replace("{20}", model.PaymentMode);
                contents = contents.Replace("{21}", model.ReferenceNo);
                contents = contents.Replace("{22}", model.Amount);
                contents = contents.Replace("{23}", model.Date);
                contents = contents.Replace("{24}", model.Remarks);
                if (query[0].ImagePath is not null)
                {
                    contents = contents.Replace("{25}", "[URL LINK]/" + query[0].ImagePath);
                }
                else if (query[0].ImageBase64 is not null)
                {
                    contents = contents.Replace("{25}", query[0].ImageBase64);
                }
                contents = contents.Replace("{26}", model.HajjApplicationNumber);
                contents = contents.Replace("{27}", model.ReceiptNumber.ToString());
                contents = contents.Replace("{28}", (z++).ToString());
                contents = contents.Replace("{29}", invoices.Count.ToString());
                contents = contents.Replace("{30}", model.ApplicantSignature);
                contents = contents.Replace("{31}", model.AccountantSign);
                contents = contents.Replace("{32}", model.CompanyStamp);


                _.Add(contents);
            }

            return _;
        }

        public IActionResult ViewDocuments(int id)
        {
            FamilyHead familyHead = _dbContext.FamilyHeads.Find(id);

            return View(familyHead);
        }

        public List<string> ConvertGovermentIDCardToPdfString(int id)
        {

            List<string> _ = new();
            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);


            string[] suffixesToRemove = { "LIMITED", "SERVICES", "PRIVATE LIMITED", "(PRIVATE)", "& UMRAH", "FOR HAJJ" };

            model.Munazzim = RemoveSuffixes(munazzim?.Munazzim.Name ?? string.Empty, suffixesToRemove);
            model.Company = RemoveSuffixes(package.Company.CompanyName, suffixesToRemove);


            // Function to remove suffixes from a given string
            string RemoveSuffixes(string name, string[] suffixes)
            {
                foreach (string suffix in suffixes)
                {
                    name = Regex.Replace(name, $@"\s*{Regex.Escape(suffix)}\s*", "", RegexOptions.IgnoreCase).Trim();
                }
                return name;
            }

            model.Package = package.Package.Name;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;

            if (munazzim.Munazzim.Name == "AL BAYAN MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-407-M#20046";
            }
            else if (munazzim.Munazzim.Name == "AL MAIRAJ MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-408-M#20047";
            }
            else if (munazzim.Munazzim.Name == "AL HAMID MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-415-M#20048";
            }


            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;

                var hotelnamemakkah = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 1
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.MakkahAccomodation = hotelnamemakkah.FirstOrDefault();

                var hotellocationmakkah = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 1
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.MakkahLocation = hotellocationmakkah.FirstOrDefault();


                //madina
                var hotelnamemedina = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 2
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.MedinahAccomodation = hotelnamemedina.FirstOrDefault();

                var hotellocationmedina = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 2
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.MedinahLocation = hotellocationmedina.FirstOrDefault();







                //azizia
                var hotelnameazizia = from fh in _dbContext.FamilyHeads
                                      join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                      join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                      join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                      where !fh.Trashed
                                            && pa.PlaceId == 4
                                            && fh.FamilyHeadId == id
                                      select h.HotelName;


                _model.AziziaAccomodation = hotelnameazizia.FirstOrDefault();

                var hotellocationazizai = from fh in _dbContext.FamilyHeads
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join h in _dbContext.Hotels on pa.HotelId equals h.HotelId
                                          where !fh.Trashed
                                                && pa.PlaceId == 4
                                                && fh.FamilyHeadId == id
                                          select h.Location;


                _model.AziziaLocation = hotellocationazizai.FirstOrDefault();


                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";


                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);

                string contents = "";
                string path = Path.Combine(_host.WebRootPath, "Cards", "Cards.html");

                using (StreamReader reader = new(path))
                {
                    contents = reader.ReadToEnd();
                }

                contents = contents.Replace("{1}", model.Munazzim);
                contents = contents.Replace("{2}", model.MunazimNo);
                contents = contents.Replace("{3}", model.Company);
                contents = contents.Replace("{4}", model.CompanyLicenseNo);
                contents = contents.Replace("{5}", _model.CID);
                contents = contents.Replace("{6}", _model.HajjAppNo);
                contents = contents.Replace("{7}", _model.Passport);
                contents = contents.Replace("{8}", _model.HajiName);
                contents = contents.Replace("{9}", _model.HajiSurName);
                contents = contents.Replace("{10}", _model.FatherName);
                contents = contents.Replace("{11}", _model.MakkahAccomodation);
                contents = contents.Replace("{12}", _model.AziziaAccomodation);
                contents = contents.Replace("{13}", _model.AziziaLocation);
                contents = contents.Replace("{14}", _model.MedinahAccomodation);
                contents = contents.Replace("{15}", _model.minaAccomodation);

                if (_currMember.FamilyHead.MemberImage is not null)
                {
                    contents = contents.Replace("{25}", "[URL LINK]/" + _currMember.FamilyHead.MemberImage);
                }
                else if (_currMember.FamilyHead.MemberImage is not null)
                {
                    contents = contents.Replace("{25}", "[URL LINK]/" + _currMember.FamilyHead.MemberImage);
                }
                _.Add(contents);
            }




            return _;
        }

        public async Task<IActionResult> SendCardPdfs(int id)
        {

            FamilyHead fh = _dbContext.FamilyHeads.Find(id);

            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            List<string> govermentCardPdfStringList = ConvertGovermentIDCardToPdfString(id);
            //string ItineraryString = ConvertItineraryToPdfString(id);

            //List<string> hajjReceiptsList = ConvertHajjReceiptToPdfString(id);

            MailMessage mm = new MailMessage("noreply@tmg.com.pk", email);
            // MailMessage mm = new MailMessage("noreply@tmg.com.pk", "m.alikhaninbox@gmail.com");
            mm.IsBodyHtml = true;
            mm.Subject = " Attached Service/Itinerary Voucher, Hajj Agreement, Hajj Booklet & Others";
            mm.Body = "";


            int h = 1;
            foreach (var s in govermentCardPdfStringList)
            {
                var pdfDoc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s,
                        }
                    }
                };

                var pdfDocBytes = _converter.Convert(pdfDoc);

                mm.Attachments.Add(new Attachment(new MemoryStream(pdfDocBytes), "Goverment ID Card " + h++ + ".pdf"));
            }

            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "",
                Password = ""
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 587
            };
            smtp.Send(mm);

            return Ok(new
            {
                msg = "sent"
            });
        }

        public List<string> ConvertIDCardToPdfString(int id)
        {

            List<string> _ = new();
            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);


            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;

            if (munazzim.Munazzim.Name == "AL BAYAN MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-407-M#20046";
            }
            else if (munazzim.Munazzim.Name == "AL MAIRAJ MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-408-M#20047";
            }
            else if (munazzim.Munazzim.Name == "AL HAMID MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-415-M#20048";
            }


            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.CompanyImg = _currMember.FamilyHead.Package.Company.LetterHeadCompany;

                _model.MakkahAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MakkahLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                         .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.MedinahAccomodation = (from fh in _dbContext.FamilyHeads
                                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                              select new
                                              {
                                                  FamilyHead = fh,
                                                  FamilyMembers = fm,
                                                  Package = p,
                                                  PackageAccomodations = pa,
                                                  Hotel = h
                                              })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MedinahLocation = (from fh in _dbContext.FamilyHeads
                                          join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                          join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                          where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                          select new
                                          {
                                              FamilyHead = fh,
                                              FamilyMembers = fm,
                                              Package = p,
                                              PackageAccomodations = pa,
                                              Hotel = h
                                          })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.AziziaAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;


                _model.AziziaLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";


                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);

                string contents = "";
                string path = Path.Combine(_host.WebRootPath, "Cards", "IDCard.html");

                using (StreamReader reader = new(path))
                {
                    contents = reader.ReadToEnd();
                }

                contents = contents.Replace("{1}", _model.CompanyImg);
                contents = contents.Replace("{2}", _model.HajiName);
                contents = contents.Replace("{3}", _model.HajiSurName);
                contents = contents.Replace("{4}", _model.PackageName);
                contents = contents.Replace("{5}", _model.Passport);
                contents = contents.Replace("{6}", _model.CID);
                contents = contents.Replace("{7}", _model.minaAccomodation);
                contents = contents.Replace("{8}", model.Munazzim);
                _.Add(contents);
            }




            return _;
        }

        public async Task<IActionResult> SendIDCardPdfs(int id)
        {

            FamilyHead fh = _dbContext.FamilyHeads.Find(id);

            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            List<string> iDCardPdfStringList = ConvertIDCardToPdfString(id);


            MailMessage mm = new MailMessage("noreply@tmg.com.pk", email);
            mm.IsBodyHtml = true;
            mm.Subject = " Attached Service/Itinerary Voucher, Hajj Agreement, Hajj Booklet & Others";
            mm.Body = "";


            int h = 1;
            foreach (var s in iDCardPdfStringList)
            {
                var pdfDoc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s,
                        }
                    }
                };

                var pdfDocBytes = _converter.Convert(pdfDoc);

                mm.Attachments.Add(new Attachment(new MemoryStream(pdfDocBytes), "ID Card " + h++ + ".pdf"));
            }

            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "",
                Password = ""
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 587
            };
            smtp.Send(mm);

            return Ok(new
            {
                msg = "sent"
            });
        }

        public List<string> ConvertLugguageCardToPdfString(int id)
        {

            List<string> _ = new();
            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);


            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;

            if (munazzim.Munazzim.Name == "AL BAYAN MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-407-M#20046";
            }
            else if (munazzim.Munazzim.Name == "AL MAIRAJ MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-408-M#20047";
            }
            else if (munazzim.Munazzim.Name == "AL HAMID MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-415-M#20048";
            }


            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.CompanyImg = _currMember.FamilyHead.Package.Company.LetterHeadCompany;

                _model.MakkahAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MakkahLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                         .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.MedinahAccomodation = (from fh in _dbContext.FamilyHeads
                                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                              select new
                                              {
                                                  FamilyHead = fh,
                                                  FamilyMembers = fm,
                                                  Package = p,
                                                  PackageAccomodations = pa,
                                                  Hotel = h
                                              })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MedinahLocation = (from fh in _dbContext.FamilyHeads
                                          join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                          join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                          where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                          select new
                                          {
                                              FamilyHead = fh,
                                              FamilyMembers = fm,
                                              Package = p,
                                              PackageAccomodations = pa,
                                              Hotel = h
                                          })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.AziziaAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;


                _model.AziziaLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";


                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);

                string contents = "";
                string path = Path.Combine(_host.WebRootPath, "Cards", "LugguageCard.html");

                using (StreamReader reader = new(path))
                {
                    contents = reader.ReadToEnd();
                }

                contents = contents.Replace("{1}", _model.CompanyImg);
                contents = contents.Replace("{2}", _model.HajiName);
                contents = contents.Replace("{3}", _model.HajiSurName);
                contents = contents.Replace("{4}", _model.PackageName);
                contents = contents.Replace("{5}", _model.Passport);
                contents = contents.Replace("{6}", _model.CID);
                contents = contents.Replace("{7}", _model.minaAccomodation);
                contents = contents.Replace("{8}", model.Munazzim);

                _.Add(contents);
            }




            return _;
        }

        public async Task<IActionResult> SendLuggageCardPdfs(int id)
        {

            FamilyHead fh = _dbContext.FamilyHeads.Find(id);

            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            List<string> luggageCardPdfStringList = ConvertLugguageCardToPdfString(id);


            MailMessage mm = new MailMessage("noreply@tmg.com.pk", email);
            mm.IsBodyHtml = true;
            mm.Subject = "Hajj ID and Luggage Card Digital Copy & Instructions for Overseas Pilgrims";
            mm.Body = "";



            int h = 1;
            foreach (var s in luggageCardPdfStringList)
            {
                var pdfDoc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s,
                        }
                    }
                };

                var pdfDocBytes = _converter.Convert(pdfDoc);

                mm.Attachments.Add(new Attachment(new MemoryStream(pdfDocBytes), "Lugguage Card " + h++ + ".pdf"));
                string luggageCard = Path.Combine(_host.WebRootPath, "Images/Cards/LuggageCard.jpg");
                if (System.IO.File.Exists(luggageCard))
                {
                    mm.Attachments.Add(new Attachment(luggageCard));
                }



            }

            // IDCARD
            List<string> iDCardPdfStringList1 = ConvertIDCardToPdfString(id);
            int h1 = 1;
            foreach (var s1 in iDCardPdfStringList1)
            {
                var pdfDoc1 = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s1,
                        }
                    }
                };

                var pdfDocBytes1 = _converter.Convert(pdfDoc1);

                mm.Attachments.Add(new Attachment(new MemoryStream(pdfDocBytes1), "ID Card " + h1++ + ".pdf"));
            }

            //govenrment card
            List<string> govermentCardPdfStringList2 = ConvertGovermentIDCardToPdfString(id);

            int h2 = 1;
            foreach (var s2 in govermentCardPdfStringList2)
            {
                var pdfDoc2 = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s2,
                        }
                    }
                };

                var pdfDocBytes2 = _converter.Convert(pdfDoc2);

                mm.Attachments.Add(new Attachment(new MemoryStream(pdfDocBytes2), "Goverment ID Card " + h2++ + ".pdf"));
            }

            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "",
                Password = ""
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 587
            };
            smtp.Send(mm);

            return Ok(new
            {
                msg = "sent"
            });
        }

        public List<string> ConvertNusukCardToPdfString(int id)
        {

            List<string> _ = new();
            CardModel model = new();


            var package = (from fh in _dbContext.FamilyHeads
                           join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                           join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                           join c in _dbContext.Companies on p.CompanyId equals c.CompanyId
                           join cl in _dbContext.CompanyLicenses on c.CompanyId equals cl.CompanyId
                           where !p.Trashed && !c.Trashed && !fh.Trashed
                           select new
                           {
                               FamilyHead = fh,
                               FamilyMembers = fm,
                               Package = p,
                               Company = c,
                               CompanyLicense = cl
                           })
                                       .FirstOrDefault(a => a.FamilyMembers.HeadId == id);

            var munazzim = _dbContext.MunazzimCompanies
                .Include(e => e.Munazzim)
                .FirstOrDefault(e => e.CompanyId == package.Company.CompanyId);


            model.Package = package.Package.Name;
            model.Company = package.Company.CompanyName;
            model.CompanyLicenseNo = package.CompanyLicense.CompanyLicense;
            model.Munazzim = munazzim.Munazzim.Name;

            if (munazzim.Munazzim.Name == "AL BAYAN MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-407-M#20046";
            }
            else if (munazzim.Munazzim.Name == "AL MAIRAJ MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-408-M#20047";
            }
            else if (munazzim.Munazzim.Name == "AL HAMID MUNAZZAM (PRIVATE) LIMITED")
            {
                model.MunazimNo = "HP# K-415-M#20048";
            }


            var members = _dbContext.FamilyMembers
                            .Include(e => e.Head)
                            .Include(e => e.FamilyHead)
                            .ThenInclude(e => e.Package)
                            .ThenInclude(e => e.Company)
                            .Where(e => e.HeadId == id
                            && !e.FamilyHead.Trashed)
                            .OrderBy(e => e.HeadId)
                            .ThenBy(e => e.FamilyHeadId)
                            .ToList();


            for (int i = 0; i < members.Count; i++)
            {
                var _currMember = members.ElementAt(i);
                CardMemberModel _model = new();
                _model.Gender = _currMember.FamilyHead.Gender.ToString();
                _model.HajiName = _currMember.FamilyHead.GivenName + " " + _currMember.FamilyHead.SurName;
                _model.FatherName = _currMember.FamilyHead.FatherHusbandName;
                _model.PackageName = _currMember.FamilyHead.Package?.Name;
                _model.Passport = _currMember.FamilyHead.PassportNumber;
                _model.HajjAppNo = _currMember.FamilyHead.HajjApplicationNumber;
                _model.HajiImg = _currMember.FamilyHead.MemberImage;
                _model.DOB = _currMember.FamilyHead.DateOfBirth.ToString("dd/MM/yyyy");
                _model.NusukId = _currMember.FamilyHead.NusukId;
                _model.VisaNo = _currMember.FamilyHead.VisaNumber;

                _model.MakkahAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MakkahLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                         .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 1 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.MedinahAccomodation = (from fh in _dbContext.FamilyHeads
                                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                              select new
                                              {
                                                  FamilyHead = fh,
                                                  FamilyMembers = fm,
                                                  Package = p,
                                                  PackageAccomodations = pa,
                                                  Hotel = h
                                              })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;

                _model.MedinahLocation = (from fh in _dbContext.FamilyHeads
                                          join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                          join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                          join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                          join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                          join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                          where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                          select new
                                          {
                                              FamilyHead = fh,
                                              FamilyMembers = fm,
                                              Package = p,
                                              PackageAccomodations = pa,
                                              Hotel = h
                                          })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 2 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                _model.AziziaAccomodation = (from fh in _dbContext.FamilyHeads
                                             join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                             join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                             join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                             join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                             join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                             where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                             select new
                                             {
                                                 FamilyHead = fh,
                                                 FamilyMembers = fm,
                                                 Package = p,
                                                 PackageAccomodations = pa,
                                                 Hotel = h
                                             })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.HotelName;


                _model.AziziaLocation = (from fh in _dbContext.FamilyHeads
                                         join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                                         join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                                         join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                                         join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                                         join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                                         where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                                         select new
                                         {
                                             FamilyHead = fh,
                                             FamilyMembers = fm,
                                             Package = p,
                                             PackageAccomodations = pa,
                                             Hotel = h
                                         })
                                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 4 && e.FamilyMembers.HeadId == id)?.Hotel?.Location;


                var result = (from fh in _dbContext.FamilyHeads
                              join fm in _dbContext.FamilyMembers on fh.FamilyHeadId equals fm.FamilyHeadId
                              join p in _dbContext.Packages on fh.PackageId equals p.PackageId
                              join pa in _dbContext.PackageAccomodtions on p.PackageId equals pa.PackageId
                              join pl in _dbContext.Places on pa.PlaceId equals pl.PlaceId
                              join h in _dbContext.Hotels on pl.PlaceId equals h.PlaceId
                              where !p.Trashed && !fh.Trashed && !p.Trashed && !h.Trashed
                              select new
                              {
                                  FamilyHead = fh,
                                  FamilyMembers = fm,
                                  Package = p,
                                  PackageAccomodations = pa,
                                  Hotel = h,
                              })
                              .FirstOrDefault(e => e.PackageAccomodations.PlaceId == 3 && e.FamilyMembers.HeadId == id)?.PackageAccomodations?.AccomodationName;

                _model.minaAccomodation = result?.EndsWith("B") == true ? "3" : "1";



                int pax = _dbContext.FamilyMembers.Count(e => e.HeadId == _currMember.HeadId);

                string cId =
                            $"{_currMember.FamilyHead.Package.Company.CompanyCode}.{_currMember.FamilyHead.IndId}" +
                            $".{_currMember.Head.ExtractedFamilyNumber}.({pax})" +
                            $".{_currMember.FamilyHead.Package.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _currMember.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                _model.CID = cId;

                model.Members.Add(_model);

                string contents = "";
                string path = Path.Combine(_host.WebRootPath, "Cards", "NusukCard.html");

                using (StreamReader reader = new(path))
                {
                    contents = reader.ReadToEnd();
                }

                contents = contents.Replace("{1}", _model.HajiName);
                contents = contents.Replace("{2}", _model.HajiSurName);
                contents = contents.Replace("{3}", _model.DOB);
                contents = contents.Replace("{4}", _model.VisaNo);
                contents = contents.Replace("{5}", _model.NusukId);

                if (_currMember.FamilyHead.MemberImage is not null)
                {
                    contents = contents.Replace("{25}", "[URL LINK]/" + _currMember.FamilyHead.MemberImage);
                }
                else if (_currMember.FamilyHead.MemberImage is not null)
                {
                    contents = contents.Replace("{25}", _currMember.FamilyHead.MemberImage);
                }
                _.Add(contents);
            }




            return _;
        }

        public async Task<IActionResult> SendNusukCardPdfs(int id)
        {

            FamilyHead fh = _dbContext.FamilyHeads.Find(id);

            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            List<string> nusukCardPdfStringList = ConvertNusukCardToPdfString(id);


            MailMessage mm = new MailMessage("noreply@tmg.com.pk", email);
            mm.IsBodyHtml = true;
            mm.Subject = " Attached Service/Itinerary Voucher, Hajj Agreement, Hajj Booklet & Others";
            mm.Body = $"";



            int h = 1;
            foreach (var s in nusukCardPdfStringList)
            {
                var pdfDoc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s,
                        }
                    }
                };

                var pdfDocBytes = _converter.Convert(pdfDoc);

                mm.Attachments.Add(new Attachment(new MemoryStream(pdfDocBytes), "Nusuk Card " + h++ + ".pdf"));
            }

            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "",
                Password = ""
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 587
            };
            smtp.Send(mm);

            return Ok(new
            {
                msg = "sent"
            });
        }

        public IActionResult EmailQurbaniLinks(int id)
        {

            var member = _dbContext.FamilyHeadsWithoutTrash
                            .FirstOrDefault(e => e.FamilyHeadId == id && e.IsHead);

            var email = _dbContext.FamilyHeadEmails
                            .FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            var otherEmail = _dbContext.FamilyHeadEmails
                            .FirstOrDefault(e => e.FamilyHeadId == id)?.Email;

            MailMessage mm = new MailMessage("noreply@tmg.com.pk", email ?? otherEmail);
            mm.IsBodyHtml = true;
            mm.Subject = "Hajj Qurbani Email Step by Step";
            mm.Body = $"";
            mm.IsBodyHtml = true;

            List<string> tokens = ConvertQurbaniToken(id);

            int k = 1;
            foreach (var s in tokens)
            {
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            HtmlContent = s,
                        }
                    }
                };

                var docBytes = _converter.Convert(doc);
                mm.Attachments.Add(new Attachment(new MemoryStream(docBytes), "Token " + k++ + ".pdf"));

            }



            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "",
                Password = ""
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 587
            };
            smtp.Send(mm);


            return Ok(member.FullName);
        }

        public async Task<IActionResult> SendTrainTicket(int id)
        {
            FamilyHead familyHead = _dbContext.FamilyHeads.Find(id);

            string email = _dbContext.FamilyHeadEmails.FirstOrDefault(e => e.FamilyHeadId == id && e.Preferred)?.Email;

            if (email is null)
            {
                return BadRequest(new
                {
                    status = 400
                });
            }

            MailMessage mm = new MailMessage("noreply@tmg.com.pk", email);
            mm.IsBodyHtml = true;
            mm.Subject = "Attached Nusuk Crad";
            mm.Body = "";


            //mm.Subject = "Attached Hajj Visa, Form & Airline Ticket";
            //mm.Body = $"Dear Pilgrim,\r\n\r\nAlhamdulillah.\r\n\r\nYour Hajj Visa 2024 has been issued and attached, along with the following documents. Kindly confirm receipt of these documents and also confirm in your WhatsApp group.\r\n\r\n1. Visa Copy\r\n2. Insurance Copy\r\n3. Hajj Form (signed & stamped)\r\n4. Airline Ticket Copy\r\n\r\nNote: Print at least 2 copies—one for your purse/pouch/hand carry and one for your luggage.\r\n\r\nHajj Mubarak & safe journey!\r\n\r\nTeam #HAJJwithTMG\r\n\r\nNote for Email 2: Other documents, i.e., service itinerary voucher, Hajj agreement, receipts, and Hajj booklet will be received in the next 3 days.";

            if (familyHead.TrainTicketImage is not null)
            {
                string path = Path.Combine(_host.WebRootPath, familyHead.TrainTicketImage);
                Attachment attachment = new Attachment(path);
                mm.Attachments.Add(attachment);
            }

            mm.IsBodyHtml = true;

            NetworkCredential NetworkCred = new NetworkCredential
            {
                UserName = "",
                Password = ""
            };

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = NetworkCred,
                Port = 587
            };
            smtp.Send(mm);

            return Ok(new
            {
                status = 200
            });
        }

        public List<string> ConvertQurbaniToken(int id)
        {
            List<string> model = new();

            var members = _dbContext.FamilyMembers
                        .AsNoTracking()
                        .Select(e => new
                        {
                            e.HeadId,
                            e.FamilyHead.Trashed,
                            e.FamilyHeadId
                        })
                        .Where(e => e.HeadId == id && !e.Trashed)
                        .ToList();

            int pax = members.Count;

            string path = Path.Combine(_host.WebRootPath, "ReceiptTemplates", "IndividualReceipt.html");

            foreach (var m in members)
            {
                string contents = "";

                using (StreamReader reader = new(path))
                {
                    contents = reader.ReadToEnd();
                }

                var _ = _dbContext.FamilyHeads
                        .AsNoTracking()
                        .Select(e => new
                        {
                            e.FamilyHeadId,
                            e.Package.Name,
                            e.Package.Company.CompanyCode,
                            e.IndId,
                            e.ExtractedFamilyNumber,
                            e.FullName,
                            e.FcId,
                            e.KSAMobileNumber,
                            e.QurbaniToken
                        })
                        .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId);

                var _mobNumber = _dbContext.FamilyHeadContacts
                                    .AsNoTracking()
                                    .FirstOrDefault(e => e.FamilyHeadId == m.FamilyHeadId && e.Preferred)?.Contact;

                string cId =
                            $"{_?.CompanyCode}.{_?.IndId}" +
                            $".{_?.ExtractedFamilyNumber}.({pax})" +
                            $".{_?.Name}";

                var memberInGroup = _dbContext.GroupMembers
                .AsNoTracking()
                .Include(e => e.Group)
                .FirstOrDefault(e => e.FamilyHeadId == _.FamilyHeadId && !e.Group.Trashed);
                if (memberInGroup != null)
                {
                    cId += $".{memberInGroup.Group.Name}.{memberInGroup.GroupId}";
                }

                contents = contents.Replace("{0}", _?.FullName);
                contents = contents.Replace("{1}", _?.FcId);
                contents = contents.Replace("{2}", cId);
                contents = contents.Replace("{3}", _mobNumber);
                contents = contents.Replace("{4}", _?.KSAMobileNumber);
                contents = contents.Replace("{5}", "Q" + _.QurbaniToken?.ToString());
                contents = contents.Replace("{6}", m.HeadId.ToString());

                model.Add(contents);
            }

            return model;
        }

        public record ChutiyaCardModel
        {
            public string Munazzim { get; set; }
            public string MunazimNo { get; set; }
            public string Company { get; set; }
            public string CompanyImg { get; set; }
            public string CompanyLicenseNo { get; set; }
            public string Package { get; set; }
            public string Maktab { get; set; }
            public string MakkahAccomodation { get; set; }
            public string MakkahLocation { get; set; }
            public string MedinahAccomodation { get; set; }
            public string MedinahLocation { get; set; }
            public string AziziaAccomodation { get; set; }
            public string AziziaLocation { get; set; }
            public List<ChutiyeModel> Members { get; set; } = new();
        }

        public record ChutiyeModel
        {
            public string HajiName { get; set; }
            public string HajiImg { get; set; }
            public string HajiSurName { get; set; }
            public string FatherName { get; set; }
            public string CID { get; set; }
            public string Passport { get; set; }
            public string Gender { get; set; }
            public string KSANo { get; set; }
            public string HajjAppNo { get; set; }
            public string VisaNo { get; set; }
            public string NusukId { get; set; }
            public string DOB { get; set; }
            public string minaAccomodation { get; set; }
            public string MakkahAccomodation { get; set; }
            public string MakkahLocation { get; set; }
            public string MedinahAccomodation { get; set; }
            public string MedinahLocation { get; set; }
            public string AziziaAccomodation { get; set; }
            public string AziziaLocation { get; set; }
            public string PackageName { get; set; }
            public string CompanyImg { get; set; }
            public string Maktab { get; set; }
            public string MaktabNumber { get; set; }




        }

    }

}

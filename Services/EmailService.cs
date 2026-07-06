using System.Net.Mail;
using System.Net;
using TMG.Interface;

namespace TMG.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("mail.smtp2go.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("newslettertmg", "0JZ7F6rZg1dXWzWx"),
                EnableSsl = false,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@tmgpk.com", "TMG"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);
            mailMessage.CC.Add("muhammadammar7016@gmail.com");
            mailMessage.CC.Add("noman.siddiqui@travelmate.com.pk");
            mailMessage.CC.Add("reservations1@travelmate.com.pk");
            mailMessage.CC.Add("cc@travelmate.com.pk");

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
